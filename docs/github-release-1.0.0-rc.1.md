# GitHub Release Body - 1.0.0-rc.1

## Summary

`1.0.0-rc.1` is the first release candidate for the real package-backed Unity Shader Graph editing engine in this repository.

This cut proves that the Unity-side graph mutation loop is no longer scaffold-only. The current release candidate supports package-backed:

- `create_graph` for blank graphs
- `read_graph_summary`
- `add_property` for `Color` and `Float/Vector1`
- `add_node` for the current graph-addable catalog subset
- `connect_ports` for the currently verified scalar, vector-builder, logic, and early color-routing paths
- `save_graph`

## Highlights

- Real package-backed graph mutation engine for Unity Shader Graph.
- Catalog-driven node addition instead of a tiny fixed node list.
- Verified connection matrix for:
  - scalar arithmetic chains
  - `Comparison -> Branch` logic flow
  - `Color -> Split`
  - `Split -> Vector1`
  - `Combine/Vector4 -> Split`
  - early color routing through `Multiply`, `Lerp`, `Branch`, and `Append`
  - Append chaining and reverse mixed chains
  - vector fan-in continuation paths such as `Combine -> Append -> Lerp -> Split`
- Unity Editor debug menus for manual smoke checks.
- EditMode smoke tests covering the verified package-backed mutation matrix.

## Release Scope

This release candidate is intended to freeze the current package-backed editing engine so the next iteration can focus on transport and productization work with a stable core.

Relevant docs:

- [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md)
- [release-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/release-checklist.md)
- [1.0.0-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.0.0-checklist.md)
- [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)

## Known Limits

- Blank graph creation is package-backed, but template-backed graph creation is not finalized.
- Blackboard property coverage is still limited to `Color` and `Float/Vector1`.
- Connection support is intentionally verified-path-only, not universal across all nodes and ports.
- The Python server is still a transport-agnostic CLI/in-process layer rather than the final live MCP transport surface.

## Recommended Next Step

Treat `1.0.0-rc.1` as the stable engine cut, gather real usage feedback, and use the next milestone to finish transport binding and final 1.0 packaging.
