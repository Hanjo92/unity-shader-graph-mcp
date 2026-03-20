# Server Area

This write scope is reserved for the MCP Server sub-agent.

## CLI Workflow

The server stays stdlib-only for now and runs as a JSON-in/JSON-out CLI.

Examples:

```bash
python -m unity_shader_graph_mcp
python -m unity_shader_graph_mcp --list-tools
python -m unity_shader_graph_mcp --request '{"tool":"shadergraph_asset","action":"save_graph","path":"Assets/ShaderGraphs/Example.shadergraph"}'
echo '{"action":"read_graph_summary","path":"Assets/ShaderGraphs/Example.shadergraph"}' | python -m unity_shader_graph_mcp
```

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

## Future Transport Seam

The server package now includes a small transport adapter placeholder so we can swap in a real MCP transport later without changing the tool contract.
