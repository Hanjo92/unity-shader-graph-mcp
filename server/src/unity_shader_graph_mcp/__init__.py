"""Unity Shader Graph MCP server package."""

from .server import ServerDefinition, ToolDefinition, build_server, build_transport, main
from .transport import (
    InProcessTransportAdapter,
    MCPStdioTransportAdapter,
    TransportAdapter,
    build_in_process_transport,
    build_mcp_stdio_transport,
    serve_mcp_stdio,
)

__all__ = [
    "ServerDefinition",
    "InProcessTransportAdapter",
    "MCPStdioTransportAdapter",
    "ToolDefinition",
    "TransportAdapter",
    "build_server",
    "build_in_process_transport",
    "build_mcp_stdio_transport",
    "build_transport",
    "main",
    "serve_mcp_stdio",
]
