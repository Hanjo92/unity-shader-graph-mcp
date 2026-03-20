# Install And Happy Path

This is the shortest path from a fresh checkout to the current supported Shader Graph editing flow.

## Prerequisites

- Unity 2022.3 with Shader Graph installed.
- Python 3 available on the command line.

## Unity Package Import

Import the package under test from this repository:

1. Open your Unity project.
2. Add the local package from `packages/unity-shader-graph-mcp/package.json`.
3. Let Unity recompile the Editor assembly.
4. Open `Window > General > Test Runner` and confirm the package EditMode tests are visible.

If you prefer a manifest entry, add the local package path as a file dependency in the target project's `Packages/manifest.json`.

## Server Startup

The current server entrypoint accepts JSON requests directly. For a quick smoke test, run:

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

## Recommended Happy Path

Use the current verified package-backed flow in this order:

1. `create_graph` with `template: blank`
2. `read_graph_summary`
3. `add_property` for `Color` or `Float/Vector1`
4. `add_node` for a supported graph-addable node
5. `connect_ports` using the verified package-backed matrix
6. `save_graph`

The supported boundary is intentionally narrow. Template-backed graph creation, universal node coverage, and universal port coverage are not part of the 1.0.0 release contract.
