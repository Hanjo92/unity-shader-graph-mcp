# GitHub Release Body - 1.2.0

## Summary

`1.2.0` is a focused post-`1.1` package-backed Shader Graph release. It closes
the tracked `1.2.0` backlog without widening the support promise beyond
verified runtime behavior.

This cut keeps the release matrix intentionally explicit: only paths covered by
Unity EditMode smoke tests and server contract tests are promoted.

## Highlights

- Added Boolean-bound `PropertyNode.Out -> BranchNode.Predicate` routing.
- Added an explicit guard for unsupported Boolean property-node routes outside
  predicate inputs.
- Promoted `SampleGradient` into the graph-addable catalog despite its
  non-`*Node` Shader Graph type shape.
- Accepted `.shadersubgraph` paths for `export_graph_contract` and
  `import_graph_contract` through both Python MCP validation and Unity
  batchmode parsing.
- Updated release docs, changelog, checklist, and implementation boundary docs
  for the focused `1.2.0` scope.

## Verification

- Unity EditMode test suite passed for the focused `1.2.0` backlog
  implementation.
- Python server tests passed with 67 tests.
- `git diff --check` passed before the release packaging commit.

## Known Limits

- Universal node and port support remains intentionally out of scope.
- Template-backed graph creation remains outside the release contract.
- Graph-addable catalog expansion remains promotion-based rather than exposing
  the entire discovered Shader Graph type list.
- Deeper arbitrary `SubGraphNode` composition and broader connection coercion
  remain post-`1.2` follow-up work.

## Closed Backlog

- #10: `1.2.0` Boolean and advanced property-node routing
- #11: `1.2.0` graph-addable catalog expansion
- #12: `1.2.0` subgraph composition expansion
- #13: `1.2.0` release packaging

## Relevant Docs

- [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md)
- [release-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/release-checklist.md)
- [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)
