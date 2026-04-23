# SamplerState Defaults Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Expand package-backed `SamplerState` property defaults from the empty-only path to a small real sampler configuration contract.

**Architecture:** Keep the existing `SamplerStateShaderProperty` type resolution intact and extend only the parser plus property-default formatting helpers. Lock the new behavior with focused EditMode smoke tests and response-metadata assertions before widening the backend implementation.

**Tech Stack:** C#, Unity Shader Graph package reflection, NUnit EditMode tests.

---

### Task 1: Lock the SamplerState default contract with tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that creates a `SamplerState` property with an explicit default such as `Point, Clamp, x4` and verifies the canonical default string.
- [ ] Add a smoke test that updates an existing `SamplerState` property and verifies the new default string after `find_property`.
- [ ] Update metadata tests so package-backed `SamplerState` add and update envelopes preserve canonical default strings.

### Task 2: Implement package-backed SamplerState default parsing

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Add helper parsing for `SamplerState` filter, wrap, and anisotropic tokens.
- [ ] Canonicalize empty input to `Linear, Repeat, None`.
- [ ] Assign the parsed values onto the reflective `TextureSamplerState` object carried by `SamplerStateShaderProperty`.
- [ ] Report canonical default strings back through add, update, find, summary, and export helpers.
- [ ] Keep invalid-token failures explicit and list the supported token set.

### Task 3: Align docs with the widened boundary

**Files:**
- Modify: `docs/milestone-boundary.md`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/README.md`

- [ ] Note that `SamplerState` now supports explicit `Filter, Wrap, Anisotropic` defaults in the current package-backed boundary.
