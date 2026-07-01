# Install And Happy Path

This is the shortest path from a fresh checkout to the current supported Shader Graph editing flow.

## Prerequisites

- Unity 2022.3 with Shader Graph installed.
- Python 3.11+ available on the command line. The release checklist uses
  `python3.12` explicitly to avoid older macOS `python3` shims.

## Unity Package Import

Import the package under test from this repository:

1. Open your Unity project.
2. Add the local package from `packages/unity-shader-graph-mcp/package.json`.
3. Let Unity recompile the Editor assembly.
4. Open `Window > General > Test Runner` and confirm the package EditMode tests are visible.
5. Open `Tools > Shader Graph MCP > Open Panel` to check package status, run setup reports, and launch the current happy-path smoke from one place.

If you prefer a manifest entry, add the local package path as a file dependency in the target project's `Packages/manifest.json`.

## Server Startup

The server currently supports two startup styles:

- JSON request CLI
- live stdio MCP transport

For the existing JSON request CLI smoke, run:

```bash
python3.12 server/src/unity_shader_graph_mcp/__main__.py --request '{
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
python3.12 server/src/unity_shader_graph_mcp/__main__.py --mcp
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

- `Tools > Shader Graph MCP > Open Panel`, then click `Run Blank Graph Happy Path`
- `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`

## Inspect Support Before Mutating

The panel is the preferred discovery path when using the package from Unity:

1. Open `Tools > Shader Graph MCP > Open Panel`.
2. Click `List Supported Nodes` before choosing an `add_node` type.
3. Run `Write Node Catalog Report` when you need the full discovered/support/deferred breakdown under `Assets/ShaderGraphMcpDiagnostics/`.
4. Run `Write Compatibility Report` after changing Unity or Shader Graph versions.

External MCP clients should do the same discovery through tool calls:

```json
{
  "tool": "shadergraph_asset",
  "action": "list_supported_nodes"
}
```

Read `supportedNodeTypes` as the graph-addable subset. Treat `discoveredNodeTypes`
as diagnostics only, and use `nodeCatalogClassification` to understand filtered
or deferred nodes.

For metadata-heavy promoted nodes, pass an explicit `nodeConfigJson` payload:
`Dropdown` needs static entries and a default index, `Keyword` needs boolean or
enum metadata, and string-body `CustomFunction` needs a function name, body,
and declared ports. For asset-bound promoted nodes, pass the required asset path
inside `nodeConfigJson`; `SubGraphNode` currently requires an explicit
`.shadersubgraph` path.

Before connecting ports, call:

```json
{
  "tool": "shadergraph_asset",
  "action": "list_supported_connections"
}
```

Read `supportedConnectionRules` as the enforced runtime connection matrix.
Addable nodes do not imply universal port compatibility, implicit type coercion,
or arbitrary fan-out. In particular, explicit-asset `SubGraphNode` add/replay
support does not imply asset-specific subgraph port routing unless that route is
listed by `supportedConnectionRules`.

The supported boundary is intentionally narrow. Template-backed graph creation,
universal node coverage, and universal port coverage are not part of the current
release contract.
