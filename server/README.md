# Server Area

This write scope is reserved for the MCP Server sub-agent.

## CLI Workflow

The server stays stdlib-only and runs as either a JSON-in/JSON-out CLI or a
stdio MCP server. Use Python 3.12 to match the package metadata.

Examples:

```bash
python3.12 -m unity_shader_graph_mcp
python3.12 -m unity_shader_graph_mcp --list-tools
python3.12 -m unity_shader_graph_mcp --request '{"tool":"shadergraph_asset","action":"save_graph","path":"Assets/ShaderGraphs/Example.shadergraph"}'
echo '{"action":"read_graph_summary","path":"Assets/ShaderGraphs/Example.shadergraph"}' | python3.12 -m unity_shader_graph_mcp
```

## Live MCP Transport

Run the live stdio MCP transport with:

```bash
python3.12 -m unity_shader_graph_mcp --mcp
```

Without Unity bridge environment variables, the tool validates and normalizes
requests through the transport-only fallback. With
`UNITY_SHADER_GRAPH_MCP_UNITY_EXE` and `UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT`
set, tool calls route through the Unity batchmode bridge.

## In-Process Transport

The same server registry can also be exercised in-process from tests or other
local tooling through the transport seam:

```python
from unity_shader_graph_mcp.transport import build_in_process_transport

transport = build_in_process_transport()
tools = transport.list_tools()
response = transport.invoke({
    "tool": "shadergraph_asset",
    "action": "save_graph",
    "path": "Assets/ShaderGraphs/Example.shadergraph",
})
```

## Response Envelope

All CLI responses use the same JSON envelope:

- `success`
- `message`
- `data`

## Transport Seam

The CLI, live MCP transport, and tests share the same transport adapter and tool
registry so request validation stays aligned across entrypoints.
