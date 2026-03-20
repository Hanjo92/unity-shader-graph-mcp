# Install And Happy Path

This is the shortest path from a fresh checkout to the current supported Shader Graph editing flow.

## Prerequisites

- Unity 2022.3 with Shader Graph installed.
- Python 3.11+ available on the command line.

## Unity Package Import

Import the package under test from this repository:

1. Open your Unity project.
2. Add the local package from `packages/unity-shader-graph-mcp/package.json`.
3. Let Unity recompile the Editor assembly.
4. Open `Window > General > Test Runner` and confirm the package EditMode tests are visible.

If you prefer a manifest entry, add the local package path as a file dependency in the target project's `Packages/manifest.json`.

## Server Startup

The server currently supports two startup styles:

- JSON request CLI
- live stdio MCP transport

For the existing JSON request CLI smoke, run:

```bash
python3 server/src/unity_shader_graph_mcp/__main__.py --request '{
  "tool": "shadergraph_asset",
  "action": "create_graph",
  "name": "ExampleLitGraph",
  "path": "Assets/ShaderGraphs",
  "template": "blank"
}'
```

You can also pipe JSON to stdin if you prefer to automate the call flow.

For the live MCP transport smoke, run:

```bash
python3 server/src/unity_shader_graph_mcp/__main__.py --mcp
```

That is enough to verify transport-only behavior. To route live MCP calls into
the real Unity Editor batchmode bridge, set these environment variables first:

```bash
export UNITY_SHADER_GRAPH_MCP_UNITY_EXE="/Applications/Unity/Hub/Editor/2022.3.xf1/Unity.app/Contents/MacOS/Unity"
export UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT="/absolute/path/to/YourUnityProject"
```

Optional overrides are also supported:

- `UNITY_SHADER_GRAPH_MCP_UNITY_EXECUTE_METHOD`
- `UNITY_SHADER_GRAPH_MCP_UNITY_REQUEST_ARG`
- `UNITY_SHADER_GRAPH_MCP_UNITY_RESPONSE_ARG`

Then use the payloads in [mcp-smoke-payloads.md](/Users/song/Projects/unity-shader-graph-mcp/docs/mcp-smoke-payloads.md).

## Recommended Happy Path

Use the current verified package-backed flow in this order:

1. `create_graph` with `template: blank`
2. `read_graph_summary`
3. `add_property` for `Color` or `Float/Vector1`
4. `add_node` for a supported graph-addable node
5. `connect_ports` using the verified package-backed matrix
6. `save_graph`

For the shortest Unity-side release smoke, run:

- `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`

The supported boundary is intentionally narrow. Template-backed graph creation, universal node coverage, and universal port coverage are not part of the 1.0.0 release contract.
