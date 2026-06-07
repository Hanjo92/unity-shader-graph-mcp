# Release Checklist

## Target

- Current stable release: `1.3.0`
- Candidate release: `1.4.0`
- Final `1.4.0` release commit must align:
  - Unity package version
  - Python package metadata version
  - MCP server handshake version

## Before Tagging

- Run Unity `EditMode` tests and confirm full pass.
- For the `1.4.0` candidate, run the targeted configurable and asset-bound
  checks before the full suite:
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
- Run `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`.
- Run the newest fan-in / chaining debug smoke menus only if you changed matrix coverage.
- Run `python3.12 -m unittest discover -s server/tests -p 'test_*.py'` to confirm the server suite passes.
- Run `python3.12 -m unittest discover -s server/tests -p 'test_mcp_transport_subprocess.py'` to confirm live `--mcp` subprocess smoke passes.
- For final `1.4.0`, run one real Unity batchmode bridge smoke with `UNITY_SHADER_GRAPH_MCP_UNITY_EXE` and `UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT` set when a Unity project is available.
- Confirm [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md) still matches runtime behavior.
- Confirm [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md) describes the actual release scope.
- Confirm package versions:
  - [package.json](/Users/song/Projects/unity-shader-graph-mcp/packages/unity-shader-graph-mcp/package.json)
  - [pyproject.toml](/Users/song/Projects/unity-shader-graph-mcp/server/pyproject.toml)
- For the final `1.4.0` release commit only, bump the Unity package version,
  Python package metadata version, and MCP server handshake version together.
- Confirm `server/tests/test_version_metadata.py` passes after the final version
  bump so the Unity package, Python package, and MCP server handshake versions
  remain aligned.

## Release Payload

- Unity package under `packages/unity-shader-graph-mcp`
- Python server under `server`
- contracts examples under `contracts`
- implementation boundary docs under `docs`

## Release Message

- Describe `1.4.0` as the explicit configurable and asset-bound node promotion release.
- Call out metadata-backed `Dropdown`, `Keyword`, string-body `CustomFunction`,
  explicit `.shadersubgraph` `SubGraphNode` binding, and contract replay.
- Keep the universal node/port support boundary explicit.
- Call out whether the release was verified with scaffold fallback only or with the real Unity batchmode bridge enabled.

## After Tagging

- Open a fresh Unity project smoke check with the package imported from the release cut.
- Verify one blank graph can complete the full happy path through `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`.
- Verify one external MCP client can complete `initialize -> tools/list -> tools/call` against `python3.12 server/src/unity_shader_graph_mcp/__main__.py --mcp`.
- Verify that same external MCP flow once more with the Unity bridge environment variables enabled.
- Use post-release feedback to choose the next slice across package-context UI, sprite, VFX, deformation, custom-interpolator, deeper subgraph composition, and remaining verified connection routes.
