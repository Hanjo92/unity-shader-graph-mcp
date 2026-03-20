# Contracts

These examples define the shared JSON shape for the Shader Graph MCP surface.

## Conventions

- Use `assetPath` for the full Unity asset path, such as `Assets/ShaderGraphs/ExampleLitGraph.shadergraph`.
- Use `name` for human-readable graph, node, or property names.
- Use `referenceName` for shader property reference strings such as `_BaseColor`.
- Use `id` for stable node, property, or edge identifiers when a response needs one.
- Keep responses in a common envelope:
  - `success`
  - `message`
  - `data`
- Put action-specific fields inside `data` so the outer response stays uniform.

## Milestone 1 Scope

The current contract set covers:

- `create_graph`
- `read_graph_summary`
- `add_property`
- `add_node`
- `connect_ports`
- `save_graph`
