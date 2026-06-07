# unity-shader-graph-mcp 1.4.0

Draft release notes. Publish only after the release commit is verified, pushed,
tagged, and the GitHub release is created from that tag.

## Summary

`1.4.0` promotes explicit configurable and asset-bound Shader Graph node support
without claiming universal all-node or all-port coverage.

This release keeps four contracts separate:

- `supportedNodeTypes` is the graph-addable allowlist.
- `discoveredNodeTypes` is diagnostic assembly discovery.
- `nodeCatalogClassification` explains supported, filtered, configurable,
  asset-bound, and deferred families.
- `supportedConnectionRules` is the verified port compatibility matrix.

## Highlights

- Adds metadata-backed `Dropdown` node support for static entries.
- Adds metadata-backed `Keyword` node support for boolean and enum modes.
- Adds string-body `CustomFunction` support with declared scalar ports.
- Adds explicit `.shadersubgraph` asset binding for `SubGraphNode`.
- Replays `Dropdown`, `Keyword`, `CustomFunction`, and `SubGraphNode`
  configuration through `export_graph_contract -> import_graph_contract`.
- Adds minimal scalar routes for promoted configurable node `Out` ports through
  the existing scalar connection matrix.

## Known Limits

- No universal Shader Graph node support is implied.
- No universal port compatibility, implicit coercion, or arbitrary fan-out is
  implied.
- File-mode `CustomFunction` remains unsupported.
- Arbitrary subgraph composition remains unsupported outside explicit
  `.shadersubgraph` asset binding.
- Explicit-asset `SubGraphNode` add/replay support does not imply asset-specific
  subgraph port routing unless the route is listed by `supportedConnectionRules`.
- Package-context UI, sprite, VFX, deformation, and custom-interpolator families
  remain diagnostic follow-up scope unless promoted by explicit fixtures.

## Verification

Fill in before publishing:

- Unity EditMode full suite: pending
- Unity targeted checks: pending
- Python server tests: local preflight passed with 70 tests before final Unity verification
- MCP subprocess smoke: local preflight passed with 2 tests before final Unity verification
- Real Unity bridge smoke: pending
- `git diff --check`: local preflight passed before final Unity verification

Targeted Unity checks for this release:

- `SupportedNodeCanonicalNames_ClassifyConfigurableNodes`
- `SupportedNodeCatalogReportLines_RecordInitializerBackedPromotions`
- `ListSupportedNodes_ReturnsPackageBackedCatalog`
- `BlankGraph_CustomFunctionStringBodyWithExplicitMetadata_StaysPackageBacked`
- `BlankGraph_CustomFunctionFileMode_IsRejectedBeforeMutation`
- `BlankGraph_CustomFunctionStringBodyMissingBody_IsRejectedBeforeMutation`
- `BlankGraph_CustomFunctionStringBodyMissingFunctionName_IsRejectedBeforeMutation`
- `BlankGraph_CustomFunctionStringBodyMissingOutputs_IsRejectedBeforeMutation`
- `BlankGraph_CustomFunctionStringBodyUnsupportedPortType_IsRejectedBeforeMutation`
- `BlankGraph_SubGraphNodeWithExplicitAssetBinding_StaysPackageBacked`
- `BlankGraph_SubGraphNodeMissingNodeConfig_IsRejectedBeforeMutation`
- `BlankGraph_SubGraphNodeMissingAssetPath_IsRejectedBeforeMutation`
- `BlankGraph_SubGraphNodeWithShaderGraphAssetPath_IsRejectedBeforeMutation`
- `BlankGraph_ImportGraphContract_ReplaysSubGraphAssetBinding_StaysPackageBacked`
- `BlankGraph_ImportGraphContract_MissingConfigurableNodeConfig_IsRejectedBeforeMutation`
- `BlankGraph_ImportGraphContract_InvalidSubGraphAssetBinding_IsRejectedBeforeMutation`
- `BlankSubGraph_ImportGraphContract_ReplaysDropdownMetadata_StaysPackageBacked`
- `BlankGraph_ImportGraphContract_ReplaysKeywordMetadata_StaysPackageBacked`
- `BlankGraph_ImportGraphContract_ReplaysCustomFunctionMetadata_StaysPackageBacked`
- `BlankGraph_CustomFunctionScalarToVector1_StaysPackageBacked`
- `BlankGraph_CustomFunctionScalarToSplit_IsRejectedWithConnectionRules`
- `BlankGraph_KeywordScalarToVector1_StaysPackageBacked`
- `BlankSubGraph_DropdownScalarToVector1_StaysPackageBacked`

## Issues

- #27: Design configurable node metadata contract
- #28: Promote Dropdown and Keyword nodes
- #29: Promote CustomFunction node support
- #30: Promote SubGraphNode with explicit asset binding
- #31: Triage remaining asset-bound and package-context nodes
- #32: Expand node initializer registry for promoted families
- #33: Contract replay for configurable and asset-bound nodes
- #34: Add supportedConnectionRules for newly promoted nodes
- #35: Document and package explicit full-node support boundary
