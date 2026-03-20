"""Unity batchmode bridge for Shader Graph asset requests.

The bridge stays opt-in: if the required environment variables are missing,
the server continues to fall back to the scaffold response path.
"""

from __future__ import annotations

import json
import os
import subprocess
import tempfile
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Callable, Mapping

from .contracts import as_response

UNITY_EXE_ENV = "UNITY_SHADER_GRAPH_MCP_UNITY_EXE"
UNITY_PROJECT_ENV = "UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT"
UNITY_EXECUTE_METHOD_ENV = "UNITY_SHADER_GRAPH_MCP_UNITY_EXECUTE_METHOD"
UNITY_REQUEST_ARG_ENV = "UNITY_SHADER_GRAPH_MCP_UNITY_REQUEST_ARG"
UNITY_RESPONSE_ARG_ENV = "UNITY_SHADER_GRAPH_MCP_UNITY_RESPONSE_ARG"

DEFAULT_EXECUTE_METHOD = "ShaderGraphMcp.Editor.Batchmode.ShaderGraphBatchEntryPoint.Run"
DEFAULT_REQUEST_ARGUMENT = "shaderGraphMcpRequestPath"
DEFAULT_RESPONSE_ARGUMENT = "shaderGraphMcpResponsePath"


class UnityBridgeError(RuntimeError):
    """Raised when the Unity batchmode bridge cannot produce a valid response."""


@dataclass(frozen=True, slots=True)
class UnityBatchmodeBridgeConfig:
    """Configuration for launching Unity in batchmode."""

    unity_executable: str
    unity_project_path: str
    execute_method: str = DEFAULT_EXECUTE_METHOD
    request_argument: str = DEFAULT_REQUEST_ARGUMENT
    response_argument: str = DEFAULT_RESPONSE_ARGUMENT

    @classmethod
    def from_environment(
        cls,
        env: Mapping[str, str] | None = None,
    ) -> "UnityBatchmodeBridgeConfig | None":
        source = os.environ if env is None else env

        unity_executable = _clean_text(source.get(UNITY_EXE_ENV))
        unity_project_path = _clean_text(source.get(UNITY_PROJECT_ENV))
        if unity_executable is None or unity_project_path is None:
            return None

        execute_method = _clean_text(source.get(UNITY_EXECUTE_METHOD_ENV)) or DEFAULT_EXECUTE_METHOD
        request_argument = _clean_text(source.get(UNITY_REQUEST_ARG_ENV)) or DEFAULT_REQUEST_ARGUMENT
        response_argument = _clean_text(source.get(UNITY_RESPONSE_ARG_ENV)) or DEFAULT_RESPONSE_ARGUMENT

        return cls(
            unity_executable=unity_executable,
            unity_project_path=unity_project_path,
            execute_method=execute_method,
            request_argument=request_argument,
            response_argument=response_argument,
        )


@dataclass(slots=True)
class UnityBatchmodeBridge:
    """Invoke a Unity batchmode entrypoint and parse the response file."""

    config: UnityBatchmodeBridgeConfig
    runner: Callable[..., subprocess.CompletedProcess[str]] = subprocess.run

    def is_available(self) -> bool:
        return True

    def invoke(self, request: Mapping[str, Any]) -> dict[str, Any]:
        payload = dict(request)
        with tempfile.TemporaryDirectory(prefix="unity-shader-graph-mcp-") as tempdir:
            tempdir_path = Path(tempdir)
            request_path = tempdir_path / "request.json"
            response_path = tempdir_path / "response.json"
            log_path = tempdir_path / "unity.log"

            request_path.write_text(
                json.dumps(payload, indent=2, sort_keys=True),
                encoding="utf-8",
            )

            command = self._build_command(
                request_path=request_path,
                response_path=response_path,
                log_path=log_path,
            )

            completed = self.runner(
                command,
                check=False,
                text=True,
                capture_output=True,
            )

            if response_path.exists():
                return self._parse_response_file(response_path)

            raise UnityBridgeError(
                "Unity batchmode bridge did not produce a response file. "
                f"Exit code: {getattr(completed, 'returncode', 'unknown')}. "
                f"Stdout: {_clean_text(getattr(completed, 'stdout', None))!r}. "
                f"Stderr: {_clean_text(getattr(completed, 'stderr', None))!r}."
            )

    def _build_command(
        self,
        *,
        request_path: Path,
        response_path: Path,
        log_path: Path,
    ) -> list[str]:
        return [
            self.config.unity_executable,
            "-batchmode",
            "-nographics",
            "-quit",
            "-projectPath",
            self.config.unity_project_path,
            "-executeMethod",
            self.config.execute_method,
            f"-{self.config.request_argument}",
            str(request_path),
            f"-{self.config.response_argument}",
            str(response_path),
            "-logFile",
            str(log_path),
        ]

    def _parse_response_file(self, response_path: Path) -> dict[str, Any]:
        try:
            raw_text = response_path.read_text(encoding="utf-8").strip()
        except OSError as exc:  # pragma: no cover - defensive
            raise UnityBridgeError(f"Failed to read Unity response file: {exc}") from exc

        if not raw_text:
            raise UnityBridgeError("Unity response file was empty.")

        try:
            payload = json.loads(raw_text)
        except json.JSONDecodeError as exc:
            raise UnityBridgeError(
                f"Unity response file contained invalid JSON: {exc.msg}."
            ) from exc

        return _normalize_response_payload(payload)


def build_unity_batchmode_bridge(
    env: Mapping[str, str] | None = None,
    *,
    runner: Callable[..., subprocess.CompletedProcess[str]] = subprocess.run,
) -> UnityBatchmodeBridge | None:
    """Build a Unity batchmode bridge if the required environment is present."""

    config = UnityBatchmodeBridgeConfig.from_environment(env)
    if config is None:
        return None
    return UnityBatchmodeBridge(config=config, runner=runner)


def _normalize_response_payload(payload: Any) -> dict[str, Any]:
    if not isinstance(payload, Mapping):
        raise UnityBridgeError("Unity response file must contain a JSON object.")

    direct = _extract_response_envelope(payload)
    if direct is not None:
        return direct

    nested = payload.get("response")
    if nested is not None:
        nested_response = _normalize_response_payload(nested)
        if nested_response:
            return nested_response

    result = payload.get("result")
    if isinstance(result, Mapping):
        nested_response = _extract_response_envelope(result)
        if nested_response is not None:
            return nested_response

    error_text = _extract_error_text(payload)
    if error_text is not None:
        return as_response(
            success=False,
            message=error_text,
            data={"errorCode": "unity_bridge_error", "rawResponse": dict(payload)},
        )

    raise UnityBridgeError(
        "Unity response file did not contain a recognizable response envelope."
    )


def _extract_response_envelope(payload: Mapping[str, Any]) -> dict[str, Any] | None:
    if "success" not in payload or "message" not in payload:
        return None

    success = bool(payload["success"])
    message = str(payload["message"])
    data = payload.get("data")
    if data is None:
        normalized_data: dict[str, Any] = {}
    elif isinstance(data, Mapping):
        normalized_data = dict(data)
    else:
        normalized_data = {"value": data}

    return as_response(success=success, message=message, data=normalized_data)


def _extract_error_text(payload: Mapping[str, Any]) -> str | None:
    for key in ("error", "errorMessage", "message"):
        value = payload.get(key)
        if isinstance(value, str) and value.strip():
            return value.strip()
        if isinstance(value, Mapping):
            nested = value.get("message") or value.get("errorMessage") or value.get("detail")
            if isinstance(nested, str) and nested.strip():
                return nested.strip()
    return None


def _clean_text(value: Any) -> str | None:
    if value is None:
        return None
    text = str(value).strip()
    return text or None
