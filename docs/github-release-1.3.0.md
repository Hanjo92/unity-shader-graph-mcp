# GitHub Release Body - 1.3.0

## Summary

`1.3.0` is a focused full-node-support foundation release. It does not claim
universal Shader Graph node support; instead, it promotes verified node families
and makes the remaining unsupported surface visible through stable diagnostics.

This cut keeps the runtime contract explicit:

- `supportedNodeTypes` is the graph-addable allowlist.
- `discoveredNodeTypes` is diagnostic assembly discovery.
- `nodeCatalogClassification` explains supported, filtered, deferred, and
  probe-failed buckets.
- `supportedConnectionRules` remains the separate connection contract.

## Highlights

- Added `Tools > Shader Graph MCP > Open Panel` for package status, reports,
  install docs, test runner access, and happy-path smoke actions.
- Added node catalog classification for discovered Shader Graph node candidates,
  including stable buckets for filtered, probe-failed, initializer-backed,
  metadata-required, externally asset-bound, package-specific,
  Unity-version-sensitive, and render-pipeline-sensitive families.
- Promoted verified graph-addable batches for math/value/vector,
  texture/sample, coordinate/utility, normal/lighting/rendering, and portable
  default specialized nodes.
- Added a node initializer registry, with `PropertyNode` as the first
  initializer-backed graph-addable node.
- Added contract replay coverage for promoted node families through
  `export_graph_contract -> import_graph_contract`.
- Updated README, install, compatibility, release, and boundary docs so users do
  not confuse discovered nodes with supported runtime nodes or addable nodes
  with universal port compatibility.

## Verification

- Unity EditMode test suite passed for the full-node-support foundation slice.
- Python server tests passed with 67 tests.
- `git diff --check` passed before the release packaging commit.

## Known Limits

- Universal Shader Graph node support remains out of scope for this cut.
- `CustomFunction`, `Dropdown`, `Keyword`, arbitrary `SubGraphNode`, and
  package-specific UI/sprite/VFX/deformation/custom-interpolator families remain
  diagnostic-only unless future slices add explicit configuration serialization,
  asset binding, or package-context fixtures.
- Universal port compatibility, implicit type coercion, and arbitrary fan-out
  remain out of scope unless represented by `supportedConnectionRules`.
- Template-backed graph creation remains outside the release contract.

## Closed Backlog

- #14: Track full Shader Graph node support
- #15: Classify discovered Shader Graph nodes for full support
- #16: Add Shader Graph MCP setup panel
- #17: Promote pure math, value, and vector Shader Graph nodes
- #18: Add node-specific initializer registry for blocked candidates
- #19: Promote texture and sample asset-bound Shader Graph nodes
- #20: Promote coordinate, scene, camera, and utility Shader Graph nodes
- #21: Promote normal, lighting, and rendering Shader Graph nodes
- #22: Promote subgraph, custom function, keyword, and dropdown nodes
- #23: Promote UI, text, sprite, and specialized Shader Graph nodes
- #24: Add contract replay coverage for promoted node families
- #25: Document full-node-support boundary and connection non-goals

## Relevant Docs

- [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md)
- [release-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/release-checklist.md)
- [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)
- [compatibility-matrix.md](/Users/song/Projects/unity-shader-graph-mcp/docs/compatibility-matrix.md)
