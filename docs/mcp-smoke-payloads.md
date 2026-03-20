# MCP Smoke Payloads

Use this when you want to verify the live stdio MCP transport from an external MCP client.

Important:

- Start the server with `--mcp`.
- Without Unity bridge environment variables, these payloads verify the transport and scaffold fallback path.
- With Unity bridge environment variables, the same payloads drive the Unity Editor batchmode bridge.

## Start The Server

Transport-only smoke:

```bash
python3 server/src/unity_shader_graph_mcp/__main__.py --mcp
```

Real Unity bridge smoke:

```bash
export UNITY_SHADER_GRAPH_MCP_UNITY_EXE="/Applications/Unity/Hub/Editor/2022.3.xf1/Unity.app/Contents/MacOS/Unity"
export UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT="/absolute/path/to/YourUnityProject"
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
- transport-only fallback returns `"status": "scaffold"`
- Unity bridge mode returns the Unity-side response envelope such as `"executionBackendKind": "PackageBacked"`

## Current Limitation

The bridge path depends on a real Unity Editor executable and project path. If either environment variable is missing, the server intentionally falls back to the scaffold response path instead of pretending the Unity side ran.
