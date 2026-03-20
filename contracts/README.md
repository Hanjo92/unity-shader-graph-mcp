# Contracts

These examples define the shared JSON shape for the current package-backed Shader Graph MCP surface.

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

- `create_graph` with the current blank-only path
- `read_graph_summary`
- `add_property`
- `add_node`
- `connect_ports`
- `save_graph`

## Recommended Happy Path

The current supported flow is:

1. `create_graph` with `template: blank`
2. `read_graph_summary`
3. `add_property`
4. `add_node`
5. `connect_ports`
6. `save_graph`

Contract examples should stay aligned to that blank-graph release path and should include the package-backed envelope fields used by the current stable release:

- `executionBackendKind`
- `backendKind`
- `compatibility`
- `supportedCreateTemplates` where applicable
- `supportedNodeTypes` and `supportedConnectionRules` where applicable
