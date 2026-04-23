"""Transport adapters for local CLI and live MCP stdio transport."""

from __future__ import annotations

import json
import sys
from dataclasses import dataclass
from typing import TYPE_CHECKING, Any, BinaryIO, Mapping, Protocol, TextIO

if TYPE_CHECKING:
    from .server import ServerDefinition

MCP_PROTOCOL_VERSION = "2024-11-05"
MCP_SERVER_VERSION = "1.1.0"


class TransportAdapter(Protocol):
    """Minimal interface for a future request transport."""

    name: str

    def list_tools(self) -> dict[str, Any]:
        """Return the transport's registered tool listing."""

    def invoke(self, request: Mapping[str, Any]) -> dict[str, Any]:
        """Dispatch a request and return a JSON-serializable response."""


@dataclass(slots=True)
class InProcessTransportAdapter:
    """Stdlib-only transport that wraps the in-memory server registry.

    This is the safest seam for tests and local tooling until a real MCP
    transport is introduced.
    """

    server: "ServerDefinition"
    name: str = "in_process"

    def list_tools(self) -> dict[str, Any]:
        return self.server.list_tools_response()

    def invoke(self, request: Mapping[str, Any]) -> dict[str, Any]:
        return self.server.invoke_request(request)


@dataclass(slots=True)
class MCPStdioTransportAdapter:
    """Minimal stdio MCP transport built on the server registry.

    This adapter speaks framed JSON-RPC 2.0 over stdin/stdout so the
    existing server registry can be reached from a real MCP client without
    introducing a new dependency.
    """

    server: "ServerDefinition"
    name: str = "mcp_stdio"
    protocol_version: str = MCP_PROTOCOL_VERSION
    server_version: str = MCP_SERVER_VERSION

    def list_tools(self) -> dict[str, Any]:
        return {
            "protocolVersion": self.protocol_version,
            "serverInfo": {
                "name": self.server.name,
                "version": self.server_version,
            },
            "capabilities": {
                "tools": {
                    "listChanged": False,
                }
            },
            "tools": [self._tool_descriptor(tool) for tool in self.server.tools.values()],
        }

    def handle_message(self, message: Mapping[str, Any]) -> dict[str, Any] | None:
        if not isinstance(message, Mapping):
            raise TypeError("MCP messages must be mappings.")

        method = message.get("method")
        if not isinstance(method, str) or not method:
            raise ValueError("MCP messages must include a method.")

        message_id = message.get("id")
        is_notification = "id" not in message

        if method == "initialize":
            return None if is_notification else self._result(message_id, self._initialize_result(message))
        if method == "notifications/initialized":
            return None
        if method == "ping":
            return None if is_notification else self._result(message_id, {})
        if method == "tools/list":
            return None if is_notification else self._result(message_id, self.list_tools())
        if method == "tools/call":
            params = message.get("params")
            if not isinstance(params, Mapping):
                raise ValueError("tools/call requires an object params payload.")
            tool_name = params.get("name")
            if not isinstance(tool_name, str) or not tool_name:
                raise ValueError("tools/call requires a non-empty tool name.")
            arguments = params.get("arguments") or {}
            if not isinstance(arguments, Mapping):
                raise ValueError("tools/call arguments must be an object.")
            return None if is_notification else self._result(message_id, self._call_tool(tool_name, arguments))

        raise ValueError(f"Unsupported MCP method '{method}'.")

    def invoke(self, request: Mapping[str, Any]) -> dict[str, Any]:
        """Preserve the direct call shape for local tests and helpers."""

        return self.server.invoke_request(request)

    def serve_stdio(
        self,
        stdin: BinaryIO | None = None,
        stdout: BinaryIO | None = None,
        stderr: TextIO | None = None,
    ) -> None:
        """Process framed MCP messages until stdin closes."""

        input_stream = stdin or sys.stdin.buffer
        output_stream = stdout or sys.stdout.buffer
        error_stream = stderr or sys.stderr

        while True:
            raw_message = _read_mcp_frame(input_stream)
            if raw_message is None:
                return

            message_id: Any = None
            try:
                message = json.loads(raw_message.decode("utf-8"))
                if not isinstance(message, Mapping):
                    raise ValueError("MCP payload must be a JSON object.")
                message_id = message.get("id")
                response = self.handle_message(message)
            except json.JSONDecodeError as exc:
                response = _error_response(
                    message_id,
                    -32700,
                    f"Invalid JSON: {exc.msg}.",
                )
            except Exception as exc:
                response = _error_response(
                    message_id,
                    -32603,
                    str(exc),
                )

            if response is None:
                continue
            _write_mcp_frame(output_stream, response)

        # pragma: no cover - the loop exits through EOF.
        if error_stream is not None:
            error_stream.flush()

    def _initialize_result(self, message: Mapping[str, Any]) -> dict[str, Any]:
        params = message.get("params")
        protocol_version = self.protocol_version
        if isinstance(params, Mapping):
            requested_version = params.get("protocolVersion")
            if isinstance(requested_version, str) and requested_version.strip():
                protocol_version = requested_version.strip()
        return {
            "protocolVersion": protocol_version,
            "serverInfo": {
                "name": self.server.name,
                "version": self.server_version,
            },
            "capabilities": {
                "tools": {
                    "listChanged": False,
                }
            },
        }

    def _call_tool(self, tool_name: str, arguments: Mapping[str, Any]) -> dict[str, Any]:
        response = self.server.invoke(tool_name, dict(arguments))
        return {
            "content": [
                {
                    "type": "text",
                    "text": json.dumps(response, indent=2, sort_keys=True),
                }
            ],
            "isError": not bool(response.get("success")),
        }

    def _tool_descriptor(self, tool: Any) -> dict[str, Any]:
        return {
            "name": tool.name,
            "description": tool.description,
            "inputSchema": {
                "type": "object",
                "additionalProperties": True,
            },
        }

    def _result(self, message_id: Any, result: dict[str, Any]) -> dict[str, Any]:
        return {
            "jsonrpc": "2.0",
            "id": message_id,
            "result": result,
        }


def build_in_process_transport(server: "ServerDefinition" | None = None) -> InProcessTransportAdapter:
    """Build the default in-process transport around the current server registry."""

    from .server import build_server

    return InProcessTransportAdapter(server or build_server())


def build_mcp_stdio_transport(server: "ServerDefinition" | None = None) -> MCPStdioTransportAdapter:
    """Build the live stdio MCP transport around the current server registry."""

    from .server import build_server

    return MCPStdioTransportAdapter(server or build_server())


def serve_mcp_stdio(
    server: "ServerDefinition" | None = None,
    stdin: BinaryIO | None = None,
    stdout: BinaryIO | None = None,
    stderr: TextIO | None = None,
) -> None:
    """Run the live stdio MCP transport until stdin closes."""

    build_mcp_stdio_transport(server).serve_stdio(stdin=stdin, stdout=stdout, stderr=stderr)


def _read_mcp_frame(stream: BinaryIO) -> bytes | None:
    header_bytes = bytearray()
    while True:
        chunk = stream.read(1)
        if not chunk:
            return None if not header_bytes else _raise_unexpected_eof("headers")
        header_bytes.extend(chunk)
        if header_bytes.endswith(b"\r\n\r\n"):
            break

    header_text = header_bytes[:-4].decode("utf-8")
    content_length = None
    for line in header_text.split("\r\n"):
        key, _, value = line.partition(":")
        if key.lower() == "content-length":
            content_length = int(value.strip())
            break
    if content_length is None:
        raise ValueError("MCP frame missing Content-Length header.")

    body = _read_exact(stream, content_length)
    if body is None:
        return _raise_unexpected_eof("body")
    return body


def _read_exact(stream: BinaryIO, size: int) -> bytes | None:
    remaining = size
    chunks: list[bytes] = []
    while remaining > 0:
        chunk = stream.read(remaining)
        if not chunk:
            return None
        chunks.append(chunk)
        remaining -= len(chunk)
    return b"".join(chunks)


def _write_mcp_frame(stream: BinaryIO, message: Mapping[str, Any]) -> None:
    payload = json.dumps(message, separators=(",", ":"), sort_keys=True).encode("utf-8")
    header = f"Content-Length: {len(payload)}\r\n\r\n".encode("ascii")
    stream.write(header)
    stream.write(payload)
    if hasattr(stream, "flush"):
        stream.flush()


def _error_response(message_id: Any, code: int, message: str) -> dict[str, Any]:
    return {
        "jsonrpc": "2.0",
        "id": message_id,
        "error": {
            "code": code,
            "message": message,
        },
    }


def _raise_unexpected_eof(section: str) -> None:
    raise EOFError(f"Unexpected EOF while reading MCP {section}.")


__all__ = [
    "MCPStdioTransportAdapter",
    "MCP_PROTOCOL_VERSION",
    "MCP_SERVER_VERSION",
    "InProcessTransportAdapter",
    "TransportAdapter",
    "build_mcp_stdio_transport",
    "build_in_process_transport",
    "serve_mcp_stdio",
]
