# Milestone Boundary

This repository is currently in the "narrow package-backed MVP" milestone.

The goal is to keep the contract, CLI, and Unity-side file flow real enough to exercise end-to-end, while only landing the narrowest Shader Graph mutations that have been confirmed against the installed package surface.

## Real Today

- The server accepts JSON requests on stdin or `--request` and returns a JSON response envelope.
- The shared contract examples are concrete and validated.
- The Unity package can create a `.shadergraph` placeholder asset path and maintain a sidecar scaffold manifest.
- `create_graph` now has a real package-backed path for blank graphs only.
- `read_graph_summary` now has a real package-backed path that loads `UnityEditor.ShaderGraph.GraphData` via reflection and reports graph metadata from the installed Shader Graph package.
- `add_property` now has a real package-backed path for `Color` and `Float`/`Vector1` blackboard inputs through `GraphData.AddGraphInput(...)`.
- `add_node` now resolves node types through a catalog-driven package-backed path. The runtime contract treats `supportedNodeTypes` as the current `graph-addable` subset, while wider `discoverable` node candidates can be reported separately for editor diagnostics.
- `connect_ports` now has real package-backed paths for three narrow rules: `Vector1Node` output slot `0` / `Out` into a different `Vector1Node` input slot `1` / `X`, `ColorNode` output slot `0` / `Out` into `SplitNode` input slot `0` / `In`, and `SplitNode` output slots `1-4` / `R,G,B,A` into a different `Vector1Node` input slot `1` / `X`, using exact `GraphData` `objectId` values from `add_node` or `read_graph_summary`.
- `connect_ports` now also supports scalar component routing into vector-builder nodes: `Vector1Node`, `SplitNode.R/G/B/A`, and scalar arithmetic `Out` can feed `CombineNode.R/G/B/A` plus `Vector2Node`, `Vector3Node`, and `Vector4Node` scalar inputs.
- `connect_ports` now also supports wider vector-to-split routing: `ColorNode.Out`, `CombineNode.RGBA`, and `Vector4Node.Out` can feed `SplitNode.In`.
- `connect_ports` now also has a package-backed scalar arithmetic path for `Vector1Node.Out -> Add/Subtract/Multiply/Divide/Power/Minimum/Maximum/Modulo/Lerp/Smoothstep/Clamp/Step/Absolute/Floor/Ceiling/Round/Sign/Sine/Cosine/Tangent/Negate/Reciprocal/SquareRoot/Fraction/Truncate/Saturate/OneMinus` input ports and those node `Out` ports back into a different `Vector1Node.X`.
- `connect_ports` now also supports scalar arithmetic chaining between `Add/Subtract/Multiply/Divide/Power/Minimum/Maximum/Modulo/Lerp/Smoothstep/Clamp/Step/Absolute/Floor/Ceiling/Round/Sign/Sine/Cosine/Tangent/Negate/Reciprocal/SquareRoot/Fraction/Truncate/Saturate/OneMinus` outputs and arithmetic input ports.
- `connect_ports` now also supports the first package-backed logic path: `Vector1` or scalar arithmetic outputs into `ComparisonNode.A/B`, `ComparisonNode.Out -> one or more BranchNode.Predicate` inputs, `Vector1` or scalar arithmetic outputs into `BranchNode.True/False`, and `BranchNode.Out -> one or more Vector1Node.X or scalar arithmetic inputs.
- `connect_ports` now also supports the first package-backed Color routing paths: `ColorNode` / `CombineNode.RGBA` / `Vector4Node.Out -> MultiplyNode.A/B`, `ColorNode` / `CombineNode.RGBA` / `Vector4Node.Out -> BranchNode.True/False`, `ColorNode` / `CombineNode.RGBA` / `Vector4Node.Out -> LerpNode.A/B/T`, `ColorNode` / `CombineNode.RGBA` / `Vector4Node.Out -> AppendVectorNode.A/B`, `AppendVectorNode.Out -> AppendVectorNode.A/B`, plus `MultiplyNode.Out -> SplitNode.In`, `BranchNode.Out -> SplitNode.In`, `LerpNode.Out -> SplitNode.In`, and `AppendVectorNode.Out -> SplitNode.In`.
- `connect_ports` now also supports the first package-backed NormalBlend vector input path: `ColorNode.Out`, `CombineNode.RGBA`, `Vector4Node.Out`, `MultiplyNode.Out`, `BranchNode.Out`, `LerpNode.Out`, `AppendVectorNode.Out`, and `SampleTexture2DNode.RGBA -> NormalBlendNode.A/B`.
- `connect_ports` now also supports the first package-backed `NormalFromTexture` input workflow: `Texture2DAssetNode.Out -> NormalFromTextureNode.Texture`, `UVNode.Out` or `TilingAndOffsetNode.Out -> NormalFromTextureNode.UV`, and `Vector1` or scalar arithmetic outputs into `NormalFromTextureNode.Offset/Strength`.
- The current EditMode smoke coverage now also locks color output fan-out: `ColorNode.Out` can feed both `SplitNode.In` and `MultiplyNode.A/B` inside the same graph, followed by `MultiplyNode.Out -> SplitNode.In`.
- The current EditMode smoke coverage now also locks append output fan-out: `AppendVectorNode.Out` can feed both `SplitNode.In` and `MultiplyNode.A/B` inside the same graph, followed by `MultiplyNode.Out -> SplitNode.In`.
- The current EditMode smoke coverage now also locks direct `ColorNode.Out -> NormalBlendNode.A/B` routing on a blank graph.
- The current EditMode smoke coverage also locks mixed Append chains into downstream vector consumers: `AppendVectorNode.Out -> MultiplyNode.A/B`, `AppendVectorNode.Out -> LerpNode.A/B/T`, and `AppendVectorNode.Out -> BranchNode.True/False`, followed by the existing `Multiply` / `Lerp` / `Branch -> SplitNode.In` paths.
- The current EditMode smoke coverage also locks the reverse mixed chains `MultiplyNode.Out -> AppendVectorNode.A/B`, `LerpNode.Out -> AppendVectorNode.A/B`, and `BranchNode.Out -> AppendVectorNode.A/B`, followed by `AppendVectorNode.Out -> SplitNode.In`.
- The current EditMode smoke coverage also locks vector fan-in into `Append`: `CombineNode.RGBA -> AppendVectorNode.A`, `Vector4Node.Out -> AppendVectorNode.A`, then `AppendVectorNode.Out -> SplitNode.In`.
- The current EditMode smoke coverage also locks vector fan-in continuation through downstream color/vector consumers: `CombineNode.RGBA -> AppendVectorNode.A -> LerpNode.A -> SplitNode.In` and `Vector4Node.Out -> AppendVectorNode.A -> BranchNode.True -> SplitNode.In`.
- The first package-backed graph creation and mutation loop has been manually verified in a real Unity project: `create_graph` for `blank`, `read_graph_summary`, `add_property`, `add_node`, and `connect_ports` succeed for the current verified paths.
- `save_graph` now has a real package-backed validate + write + refresh path that keeps the current graph asset and import state aligned.
- The install and happy-path smoke guide is documented in [install-and-happy-path.md](/Users/song/Projects/unity-shader-graph-mcp/docs/install-and-happy-path.md).
- The compatibility report captures `unityVersion`, backend kind, and the resolved `GraphData` surface, and the current matrix is documented in [compatibility-matrix.md](/Users/song/Projects/unity-shader-graph-mcp/docs/compatibility-matrix.md).
- The shared test metadata keeps `supportedConnectionRules` envelope-only so connection metadata can evolve without changing the response shape.
- The compatibility probe confirms `UnityEditor.ShaderGraph.GraphData` and the `AddGraphInput`, `AddNode`, `Connect`, and `ValidateGraph` reflection surface in Unity 2022.3 with Shader Graph 17.3.0.

## Scaffold-Only Today

- `create_graph` is blank-only when package-backed. Template-backed graph authoring remains unsupported.
- `connect_ports` currently supports the verified narrow rules plus the current scalar, vector-builder, logic, early Color-routing, and UV/texture-derived normal input package-backed paths: `Vector1Node.Out -> Vector1Node.X`, `ColorNode.Out -> SplitNode.In`, `SplitNode.R/G/B/A -> Vector1Node.X`, `Vector1/Split/arithmetic -> CombineNode.R/G/B/A`, `Vector1/Split/arithmetic -> Vector2/Vector3/Vector4` scalar inputs, `CombineNode.RGBA -> SplitNode.In`, `Vector4Node.Out -> SplitNode.In`, `Vector1Node.Out -> Add/Subtract/Multiply/Divide/Power/Minimum/Maximum/Modulo/Lerp/Smoothstep/Clamp/Step/Absolute/Floor/Ceiling/Round/Sign/Sine/Cosine/Tangent/Negate/Reciprocal/SquareRoot/Fraction/Truncate/Saturate/OneMinus` inputs, arithmetic `Out -> Vector1Node.X`, arithmetic `Out -> arithmetic` inputs, `Vector1/arithmetic -> Comparison.A/B`, `Comparison.Out -> one or more Branch.Predicate` inputs, `Vector1/arithmetic -> Branch.True/False`, `Branch.Out -> one or more Vector1.X/arithmetic inputs`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Multiply.A/B`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Branch.True/False`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Lerp.A/B/T`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append/SampleTexture2D.RGBA -> NormalBlend.A/B`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append plus Vector1/Split/arithmetic -> Append.A/B`, `Texture2DAssetNode.Out -> NormalFromTextureNode.Texture`, `UVNode/TilingAndOffsetNode.Out -> NormalFromTextureNode.UV`, `Vector1/arithmetic -> NormalFromTextureNode.Offset/Strength`, `Multiply.Out -> SplitNode.In`, `Branch.Out -> SplitNode.In`, `Lerp.Out -> SplitNode.In`, and `AppendVectorNode.Out -> SplitNode.In`.
- Connection conversion, general fan-out beyond the current `Comparison.Out -> multiple Branch.Predicate`, `Branch.Out -> multiple Vector1/arithmetic inputs`, `ColorNode.Out -> SplitNode.In + MultiplyNode.A/B`, and `AppendVectorNode.Out -> SplitNode.In + MultiplyNode.A/B` paths, cross-type coercion, direct boolean-to-scalar routing outside `Branch.Predicate`, Color node inputs, and broader Color connections beyond the currently verified `Multiply`, `Branch`, `Lerp`, `Append`, and `NormalBlend` vector paths stay unsupported.
- The discoverable node catalog is intentionally broader than the runtime-supported `add_node` contract. Internal, legacy, output-only, and probe-rejected node types remain visible in diagnostics but are excluded from `supportedNodeTypes`.
- Probe-rejected node types stay grouped by stable failure-reason buckets in diagnostics so the next `graph-addable` expansion target can be chosen from concrete graph-creation, instantiation, `AddNode`, layout, or `ValidateGraph` failures.
- The CLI is transport-agnostic and does not yet speak MCP directly.
- Tool registration is still an internal server registry, not a live MCP transport binding.

## Blocked On Unity Shader Graph APIs

- Creating a real Shader Graph asset from the package API surface beyond a blank-only package-backed path.
- Expanding blackboard property coverage beyond `Color` and `Float`/`Vector1`.
- Expanding the `graph-addable` node subset beyond the current verified smoke nodes and probe-passed catalog entries.
- Reducing repeated probe-rejected buckets by converting common failure reasons into explicit filters, allowlist promotions, or dedicated node-specific initialization paths.
- Expanding port and edge coverage beyond the current verified narrow paths and the scalar arithmetic `Vector1 <-> arithmetic node` routes.
- Expanding create_graph beyond the blank-only package-backed path.
- Hardening the save path across a wider Unity and Shader Graph version matrix.
- Capturing and maintaining a broader Unity / Shader Graph compatibility matrix while keeping the package-backed fallback behavior explicit.

## End-To-End CLI Example

The current server CLI can be exercised directly with JSON.

```bash
python3 server/src/unity_shader_graph_mcp/__main__.py --request '{
  "tool": "shadergraph_asset",
  "action": "create_graph",
  "name": "ExampleLitGraph",
  "path": "Assets/ShaderGraphs",
  "template": "blank"
}'
```

Expected shape:

```json
{
  "success": true,
  "message": "Created blank package-backed Shader Graph at 'Assets/ShaderGraphs/ExampleLitGraph.shadergraph'.",
  "data": {
    "operation": "create_graph",
    "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
    "executionBackendKind": "PackageBacked",
    "backendKind": "PackageReady",
    "template": "blank",
    "supportedCreateTemplates": ["blank"],
    "createdGraph": {
      "name": "ExampleLitGraph",
      "requestedTemplate": "blank",
      "resolvedTemplate": "blank",
      "graphPathLabel": "Shader Graphs"
    }
  }
}
```

## Manual Unity Spike

To run the next Unity editor spike by hand:

1. Open the repository in Unity 2022.3 with Shader Graph installed.
2. Import `packages/unity-shader-graph-mcp` as the package under test.
3. Run the Editor tests in `packages/unity-shader-graph-mcp/Tests/Editor`.
4. Run `Tools > Shader Graph MCP > Write Compatibility Report`.
5. Inspect the generated file under `Assets/ShaderGraphMcpDiagnostics/`.
6. Verify `candidateTypeNames`, `discoveredTypeNames`, `resolvedMethodSignatures`, and the `GraphData` surface before wiring any real graph mutation calls.
7. Only replace the scaffold backend after the compatibility snapshot reports `PackageReady`.
8. After that, treat `save_graph` as the package-backed validate + write + refresh step that closes the edit loop.

## Hand-Off Rule

When real Shader Graph API calls are ready, replace the scaffold manifest path first and keep the JSON contract stable so the server and docs do not need a shape change at the same time.
