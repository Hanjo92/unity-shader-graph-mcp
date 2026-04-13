# Architecture

## Goal

Build a focused MCP integration for Unity Shader Graph, not a general-purpose Unity MCP.

## Bounded Areas

### 1. Unity Package

Owns all Unity Editor integration:

- Shader Graph asset creation and loading
- graph inspection
- graph mutation
- graph save and refresh
- Unity-side request handlers

Path:

- `packages/unity-shader-graph-mcp/`

### 2. MCP Server

Owns MCP tool definitions and transport-facing API:

- tool schema
- request validation
- response normalization
- server entrypoints

Path:

- `server/`

### 3. Shared Contracts

Owns stable payload examples and protocol notes shared by Unity and server:

- request examples
- response examples
- tool naming conventions

Path:

- `contracts/`

### 4. Docs and Planning

Owns decisions, roadmaps, and agent coordination:

- ADRs
- task split
- roadmap

Path:

- `docs/`

## First Tool Slice

Start with one focused tool family:

- `shadergraph_asset`

Initial actions:

- `create_graph` with a blank-only package-backed path
- `read_graph_summary`
- `add_property`
- `add_node`
- `node catalog` split into two meanings:
  `discoverable`: reflection-found candidates for diagnostics
  `supported`: the `graph-addable` subset that passed the current runtime probe and can be returned through `supportedNodeTypes`
  `probe-rejected`: discoverable candidates that passed reflection filters but still failed the in-memory `AddNode` / `ValidateGraph` probe, so they remain diagnostics-only until the failure reason is addressed
- `connect_ports` with the current package-backed paths: `Vector1Node.Out -> Vector1Node.X`, `ColorNode.Out -> SplitNode.In`, `SplitNode.R/G/B/A -> Vector1Node.X`, scalar component routing into `CombineNode` and `Vector2/Vector3/Vector4` inputs, `CombineNode.RGBA -> SplitNode.In`, `Vector4Node.Out -> SplitNode.In`, the scalar arithmetic `Vector1Node.Out -> Add/Subtract/Multiply/Divide/Power/Minimum/Maximum/Modulo/Lerp/Smoothstep/Clamp/Step/Absolute/Floor/Ceiling/Round/Sign/Sine/Cosine/Tangent/Negate/Reciprocal/SquareRoot/Fraction/Truncate/Saturate/OneMinus` input ports, arithmetic `Out -> Vector1Node.X`, arithmetic `Out -> arithmetic` inputs, `Vector1/arithmetic -> Comparison.A/B`, `Comparison.Out -> one or more Branch.Predicate`, `Vector1/arithmetic -> Branch.True/False`, `Branch.Out -> one or more Vector1/arithmetic inputs`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Multiply.A/B`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Branch.True/False`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Lerp.A/B/T`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append/SampleTexture2D.RGBA -> NormalBlend.A/B`, `Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append plus Vector1/Split/arithmetic -> Append.A/B`, `Multiply.Out -> SplitNode.In`, `Branch.Out -> SplitNode.In`, `Lerp.Out -> SplitNode.In`, `AppendVectorNode.Out -> SplitNode.In`, the currently verified `ColorNode.Out -> SplitNode.In + MultiplyNode.A/B` and `AppendVectorNode.Out -> SplitNode.In + MultiplyNode.A/B` fan-out paths, and the currently verified fan-in continuation chains `Combine -> Append -> Lerp -> Split` plus `Vector4 -> Append -> Branch -> Split`, using exact node `objectId` values from `add_node` or `read_graph_summary`
- `save_graph` as the package-backed validate + write + refresh step that keeps the edited graph asset and import state aligned
- next connection expansion candidate: broader Color routing beyond the current `Multiply` / `Branch` / `Lerp` / `Append` / `NormalBlend` vector paths, richer boolean fan-out beyond `Comparison -> Branch.Predicate`, and a wider node/port matrix beyond the current verified paths

## Dependency Direction

- `server` may depend on `contracts`
- `packages/unity-shader-graph-mcp` may depend on `contracts` examples as reference only
- `contracts` depends on nothing
- `docs` depends on nothing

## Coordination Rule

If a change needs both server and Unity edits, first update the shared contract example in `contracts/`, then let each owner implement their side independently.
