"""Transport adapter placeholders for future MCP integration.

The current server stays stdlib-only and uses a JSON CLI entrypoint.
This module defines the seam we can replace with an MCP transport later
without changing the request/response contract used by the CLI.
"""

from __future__ import annotations

from dataclasses import dataclass
from typing import TYPE_CHECKING, Any, Mapping, Protocol

if TYPE_CHECKING:
    from .server import ServerDefinition


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


def build_in_process_transport(server: "ServerDefinition" | None = None) -> InProcessTransportAdapter:
    """Build the default in-process transport around the current server registry."""

    from .server import build_server

    return InProcessTransportAdapter(server or build_server())


__all__ = [
    "InProcessTransportAdapter",
    "TransportAdapter",
    "build_in_process_transport",
]
