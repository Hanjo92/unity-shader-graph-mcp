# GitHub Release Body - 1.1.0

## Summary

`1.1.0` widens the package-backed Unity Shader Graph authoring surface on top
of the stable `1.0.0` engine and MCP bridge.

This cut keeps the same rule as the first stable release: only verified
package-backed behavior is promoted into runtime support. The main difference
is breadth. More property types, property-bound nodes, texture/UV/normal
workflows, graph/subgraph contract boundaries, and response metadata are now
covered by focused EditMode smoke tests.

## Highlights

- Expanded package-backed `add_property` support for:
  - `Integer`
  - `Vector2`
  - `Vector3`
  - `Vector4`
  - `Boolean`
  - `Texture2D`
  - `Cubemap`
  - `Texture3D`
  - `Texture2DArray`
  - `Gradient`
  - `SamplerState`
- Property-bound `PropertyNode` creation, lookup, export, import, and contract
  replay metadata for the verified property families.
- Wider verified `connect_ports` coverage across texture sampling, UV routing,
  normal workflows, append chains, branch/comparison chains, color/vector
  routes, and property-node routes.
- Hardened `import_graph_contract` behavior for graph-vs-subgraph mismatches
  and non-blank import targets.
- Updated `list_supported_properties`, `list_supported_connections`, response
  metadata, docs, and smoke coverage to match the real runtime boundary.

## Verification

- Unity EditMode test suite passed for the widened package-backed surface.
- Python server tests passed with 67 tests.
- `git diff --check` passed before the release packaging commit.

## Known Limits

- Universal node and port support is still intentionally out of scope.
- Template-backed graph creation remains outside the release contract.
- Direct Boolean-to-scalar routing outside verified predicate/property paths is
  deferred.
- Deeper arbitrary `SubGraphNode` composition and multi-slot subgraph output
  parity are deferred.
- Graph-addable catalog expansion remains promotion-based rather than exposing
  the entire discovered Shader Graph type list.

## Follow-Up Backlog

- #10: post-`1.1` Boolean and advanced property-node routing
- #11: post-`1.1` graph-addable catalog expansion
- #12: post-`1.1` subgraph composition expansion

## Relevant Docs

- [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md)
- [release-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/release-checklist.md)
- [1.1.0-plan.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.1.0-plan.md)
- [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)
