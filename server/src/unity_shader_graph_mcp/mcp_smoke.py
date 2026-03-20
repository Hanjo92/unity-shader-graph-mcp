"""Small stdlib-only helper for smoking the live MCP stdio transport.

This module is intentionally tiny so tests and manual checks can drive the
real ``--mcp`` entrypoint without pulling in an external MCP dependency.
"""

from __future__ import annotations

import json
import os
import select
import subprocess
import time
from dataclasses import dataclass
from pathlib import Path
from typing import Any, Mapping, Sequence

from .transport import MCP_PROTOCOL_VERSION


@dataclass(slots=True)
class McpStdioSmokeClient:
    """Minimal framed-stdio client for the live MCP transport."""

    process: subprocess.Popen[bytes]
    timeout_seconds: float = 10.0

    @classmethod
    def spawn(
        cls,
        command: Sequence[str],
        *,
        cwd: str | Path | None = None,
        env: Mapping[str, str] | None = None,
        timeout_seconds: float = 10.0,
    ) -> "McpStdioSmokeClient":
        process = subprocess.Popen(
            list(command),
            cwd=None if cwd is None else str(cwd),
            env=None if env is None else dict(env),
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
        )
        return cls(process=process, timeout_seconds=timeout_seconds)

    def __enter__(self) -> "McpStdioSmokeClient":
        return self

    def __exit__(self, exc_type, exc, tb) -> None:
        self.close()

    def close(self) -> None:
        if self.process.stdin and not self.process.stdin.closed:
            self.process.stdin.close()

        try:
            self.process.wait(timeout=self.timeout_seconds)
        except subprocess.TimeoutExpired:
            self.process.kill()
            self.process.wait(timeout=self.timeout_seconds)
        finally:
            if self.process.stdout and not self.process.stdout.closed:
                self.process.stdout.close()
            if self.process.stderr and not self.process.stderr.closed:
                self.process.stderr.close()

    def initialize(self, protocol_version: str = MCP_PROTOCOL_VERSION) -> dict[str, Any]:
        return self.request(
            {
                "jsonrpc": "2.0",
                "id": 1,
                "method": "initialize",
                "params": {
                    "protocolVersion": protocol_version,
                    "capabilities": {},
                    "clientInfo": {
                        "name": "unity-shader-graph-mcp-smoke",
                        "version": "0.1.0",
                    },
                },
            }
        )

    def list_tools(self, request_id: int = 2) -> dict[str, Any]:
        return self.request(
            {
                "jsonrpc": "2.0",
                "id": request_id,
                "method": "tools/list",
                "params": {},
            }
        )

    def call_tool(
        self,
        tool_name: str,
        arguments: Mapping[str, Any],
        request_id: int = 3,
    ) -> dict[str, Any]:
        return self.request(
            {
                "jsonrpc": "2.0",
                "id": request_id,
                "method": "tools/call",
                "params": {
                    "name": tool_name,
                    "arguments": dict(arguments),
                },
            }
        )

    def request(self, message: Mapping[str, Any]) -> dict[str, Any]:
        self._write_frame(message)
        return self._read_frame()

    def _write_frame(self, message: Mapping[str, Any]) -> None:
        if self.process.stdin is None:
            raise RuntimeError("Smoke client stdin is not available.")

        payload = json.dumps(message, separators=(",", ":"), sort_keys=True).encode("utf-8")
        header = f"Content-Length: {len(payload)}\r\n\r\n".encode("ascii")
        self.process.stdin.write(header)
        self.process.stdin.write(payload)
        self.process.stdin.flush()

    def _read_frame(self) -> dict[str, Any]:
        if self.process.stdout is None:
            raise RuntimeError("Smoke client stdout is not available.")

        header_bytes = self._read_until(b"\r\n\r\n")
        header_text = header_bytes.decode("utf-8")
        content_length = None
        for line in header_text.split("\r\n"):
            key, _, value = line.partition(":")
            if key.lower() == "content-length":
                content_length = int(value.strip())
                break
        if content_length is None:
            raise RuntimeError("MCP response missing Content-Length header.")

        body = self._read_exact(content_length)
        return json.loads(body.decode("utf-8"))

    def _read_until(self, marker: bytes) -> bytes:
        if self.process.stdout is None:
            raise RuntimeError("Smoke client stdout is not available.")

        buffer = bytearray()
        while not buffer.endswith(marker):
            chunk = self._read_available(1)
            if not chunk:
                raise EOFError(self._read_process_error("Unexpected EOF while reading MCP frame headers."))
            buffer.extend(chunk)
        return bytes(buffer[:-len(marker)])

    def _read_exact(self, size: int) -> bytes:
        chunks: list[bytes] = []
        remaining = size
        while remaining > 0:
            chunk = self._read_available(remaining)
            if not chunk:
                raise EOFError(self._read_process_error("Unexpected EOF while reading MCP frame body."))
            chunks.append(chunk)
            remaining -= len(chunk)
        return b"".join(chunks)

    def _read_available(self, size: int) -> bytes:
        if self.process.stdout is None:
            raise RuntimeError("Smoke client stdout is not available.")

        deadline = time.monotonic() + self.timeout_seconds
        fd = self.process.stdout.fileno()
        while True:
            remaining = deadline - time.monotonic()
            if remaining <= 0:
                raise TimeoutError(self._read_process_error("Timed out waiting for MCP response."))
            ready, _, _ = select.select([fd], [], [], remaining)
            if not ready:
                continue
            chunk = os.read(fd, size)
            if chunk:
                return chunk
            if self.process.poll() is not None:
                return b""

    def _read_process_error(self, message: str) -> str:
        stderr_text = self._read_stderr()
        if stderr_text:
            return f"{message} Process stderr: {stderr_text}"
        return message

    def _read_stderr(self) -> str:
        if self.process.stderr is None:
            return ""
        if self.process.stderr.closed:
            return ""
        try:
            if self.process.poll() is None:
                return ""
            data = self.process.stderr.read()
            if not data:
                return ""
            return data.decode("utf-8", errors="replace").strip()
        except Exception:
            return ""


__all__ = [
    "McpStdioSmokeClient",
]
