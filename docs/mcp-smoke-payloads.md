# MCP Smoke Payloads

Use this when you want to verify the live stdio MCP transport from an external MCP client.

Important:

- Start the server with `--mcp`.
- These payloads verify the transport and tool dispatch path.
- The current Python tool handler still validates and echoes the Shader Graph contract rather than driving the Unity Editor directly.

## Start The Server

```bash
python3 server/src/unity_shader_graph_mcp/__main__.py --mcp
```

## Initialize

```json
{
  "jsonrpc": "2.0",
  "id": 1,
  "method": "initialize",
  "params": {
    "protocolVersion": "2024-11-05",
    "capabilities": {},
    "clientInfo": {
      "name": "manual-smoke-client",
      "version": "0.1.0"
    }
  }
}
```

## Initialized Notification

```json
{
  "jsonrpc": "2.0",
  "method": "notifications/initialized"
}
```

## List Tools

```json
{
  "jsonrpc": "2.0",
  "id": 2,
  "method": "tools/list",
  "params": {}
}
```

Expected outcome:

- one tool named `shadergraph_asset`

## Call Tool: Read Graph Summary

```json
{
  "jsonrpc": "2.0",
  "id": 3,
  "method": "tools/call",
  "params": {
    "name": "shadergraph_asset",
    "arguments": {
      "action": "read_graph_summary",
      "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph"
    }
  }
}
```

## Call Tool: Create Blank Graph

```json
{
  "jsonrpc": "2.0",
  "id": 4,
  "method": "tools/call",
  "params": {
    "name": "shadergraph_asset",
    "arguments": {
      "action": "create_graph",
      "name": "ExampleLitGraph",
      "path": "Assets/ShaderGraphs",
      "template": "blank"
    }
  }
}
```

## What Success Looks Like

- `initialize` returns server info and `tools` capability
- `tools/list` returns `shadergraph_asset`
- `tools/call` returns `isError: false`
- the first content item is JSON text containing the server response envelope

## Current Limitation

At the moment, a successful `tools/call` proves the live MCP transport and request normalization path, not the full Unity Editor execution path. Use Unity EditMode tests and the package debug menus for the real package-backed graph mutation confirmation.
