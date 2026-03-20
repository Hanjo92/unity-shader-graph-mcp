"""Entry point scaffold for the Shader Graph MCP server."""

from __future__ import annotations

import argparse
import json
import sys
from dataclasses import dataclass, field
from typing import Any, Callable, Mapping, Sequence

from .contracts import as_response
from .tools import handle_shadergraph_asset
from .transport import build_in_process_transport, serve_mcp_stdio


JSONType = dict[str, Any]


@dataclass(slots=True)
class ToolDefinition:
    """Transport-neutral description of a server tool."""

    name: str
    description: str
    handler: Callable[[dict[str, Any]], dict[str, Any]]

    def to_dict(self) -> JSONType:
        return {
            "name": self.name,
            "description": self.description,
        }


@dataclass(slots=True)
class ServerDefinition:
    """Minimal server registry used for early development and tests."""

    name: str = "unity-shader-graph-mcp"
    tools: dict[str, ToolDefinition] = field(default_factory=dict)

    def register_tool(self, tool: ToolDefinition) -> None:
        if tool.name in self.tools:
            raise ValueError(f"Tool '{tool.name}' is already registered.")
        self.tools[tool.name] = tool

    def list_tools(self) -> list[JSONType]:
        return [tool.to_dict() for tool in self.tools.values()]

    def list_tools_response(self) -> dict[str, Any]:
        return as_response(
            True,
            "Registered tools listed successfully.",
            {
                "server": self.name,
                "toolCount": len(self.tools),
                "tools": self.list_tools(),
            },
        )

    def get_tool(self, name: str) -> ToolDefinition:
        if name not in self.tools:
            raise KeyError(f"Unknown tool '{name}'.")
        return self.tools[name]

    def invoke(self, name: str, payload: dict[str, Any]) -> dict[str, Any]:
        return self.get_tool(name).handler(payload)

    def invoke_request(self, request: Mapping[str, Any]) -> dict[str, Any]:
        normalized = _normalize_inbound_request(request)
        tool_name = normalized.pop("tool", None)
        if tool_name is None:
            tool_name = self._resolve_default_tool_name(normalized)
        if tool_name is None:
            raise ValueError(
                "Request must include 'tool' when more than one tool is available."
            )
        return self.invoke(str(tool_name), normalized)

    def _resolve_default_tool_name(self, request: Mapping[str, Any]) -> str | None:
        if len(self.tools) == 1:
            return next(iter(self.tools))
        if "shadergraph_asset" in self.tools and _looks_like_shadergraph_request(request):
            return "shadergraph_asset"
        return None


def build_server() -> ServerDefinition:
    """Build the milestone 1 server registry."""

    server = ServerDefinition()
    server.register_tool(
        ToolDefinition(
            name="shadergraph_asset",
            description="Focused Shader Graph asset operations for milestone 1.",
            handler=handle_shadergraph_asset,
        )
    )
    return server


def main(argv: Sequence[str] | None = None) -> int:
    """Run the server as a JSON-in/JSON-out CLI."""

    if argv is None:
        _print_scaffold_status()
        return 0

    argv_list = list(argv)
    if not argv_list and sys.stdin.isatty():
        _print_scaffold_status()
        return 0

    parser = argparse.ArgumentParser(
        prog="unity-shader-graph-mcp",
        description="Shader Graph MCP server scaffold with JSON request execution.",
    )
    parser.add_argument(
        "--list-tools",
        action="store_true",
        help="Print the registered tool list as a JSON envelope and exit.",
    )
    parser.add_argument(
        "--request",
        help="JSON request payload. If omitted, read a single JSON object from stdin.",
    )
    parser.add_argument(
        "--mcp",
        action="store_true",
        help="Run the live MCP stdio transport instead of the JSON CLI.",
    )
    args = parser.parse_args(argv_list)

    if args.mcp:
        if args.request is not None or args.list_tools:
            print("--mcp cannot be combined with --request or --list-tools.", file=sys.stderr)
            return 2
        serve_mcp_stdio(build_server())
        return 0

    transport = build_transport()

    if args.list_tools:
        if args.request is not None:
            print("--list-tools cannot be combined with --request.", file=sys.stderr)
            return 2
        json.dump(transport.list_tools(), fp=sys.stdout, indent=2, sort_keys=True)
        sys.stdout.write("\n")
        return 0

    request_text = args.request
    if request_text is None:
        request_text = _read_stdin_request()
    if request_text is None:
        print(
            "No JSON request provided. Use --request or pipe JSON into stdin.",
            file=sys.stderr,
        )
        parser.print_help()
        return 0

    try:
        request = json.loads(request_text)
        if not isinstance(request, dict):
            raise ValueError("JSON request must be an object.")
        response = transport.invoke(request)
    except json.JSONDecodeError as exc:
        response = as_response(
            False,
            f"Invalid JSON request: {exc.msg}.",
            {"errorType": exc.__class__.__name__, "errorCode": "invalid_json"},
        )
    except (KeyError, ValueError) as exc:
        response = as_response(
            False,
            str(exc),
            {"errorType": exc.__class__.__name__, "errorCode": "request_error"},
        )
    except Exception as exc:
        response = as_response(
            False,
            str(exc),
            {"errorType": exc.__class__.__name__, "errorCode": "unexpected_error"},
        )

    json.dump(response, fp=sys.stdout, indent=2, sort_keys=True)
    sys.stdout.write("\n")
    return 0 if response.get("success") else 1


def _read_stdin_request() -> str | None:
    if sys.stdin.isatty():
        return None
    text = sys.stdin.read().strip()
    return text or None


def _print_scaffold_status() -> None:
    server = build_server()
    print("unity-shader-graph-mcp server scaffold is ready.")
    print(f"Registered tools: {len(server.tools)}")
    print("Use --list-tools to inspect the registry or pipe JSON into stdin.")
    print("TODO: wire this registry into the chosen MCP transport.")


def build_transport():
    """Build the default in-process transport used by the CLI and tests."""

    return build_in_process_transport(build_server())


def _normalize_inbound_request(request: Mapping[str, Any]) -> dict[str, Any]:
    normalized = dict(request)
    if "request" in normalized and isinstance(normalized["request"], Mapping):
        nested = dict(normalized.pop("request"))
        nested.update(normalized)
        normalized = nested
    elif "params" in normalized and isinstance(normalized["params"], Mapping):
        nested = dict(normalized.pop("params"))
        nested.update(normalized)
        normalized = nested
    elif "payload" in normalized and isinstance(normalized["payload"], Mapping):
        nested = dict(normalized.pop("payload"))
        nested.update(normalized)
        normalized = nested

    tool_name = normalized.get("tool")
    if tool_name is not None:
        normalized["tool"] = tool_name
    else:
        normalized.pop("tool", None)

    return normalized


def _looks_like_shadergraph_request(request: Mapping[str, Any]) -> bool:
    return "action" in request or "propertyName" in request or "nodeType" in request


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
