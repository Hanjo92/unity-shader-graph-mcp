# SamplerState Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-backed `SamplerState` Shader Graph property support with matching catalog and metadata coverage.

**Architecture:** Extend the existing property-type resolution and default-value parsing inside the package-backed backend, then lock the wider surface with smoke and response-metadata tests. Keep the request contract unchanged and treat `SamplerState` defaults as a narrow empty-default path on `SamplerStateShaderProperty`.

**Tech Stack:** C#, Unity Shader Graph package reflection, NUnit EditMode tests.

---

### Task 1: Lock the SamplerState contract with tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that adds a `SamplerState` property to a blank graph and verifies the resolved property type.
- [ ] Extend the supported-property catalog assertions so `SamplerState` is present.
- [ ] Add a response-metadata test that preserves the `SamplerState` add-property envelope.

### Task 2: Implement package-backed SamplerState property support

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Extend the supported property type list to include `SamplerState`.
- [ ] Resolve the request type to `UnityEditor.ShaderGraph.SamplerStateShaderProperty`.
- [ ] Teach property-instance inspection to map sampler-state property instances back to canonical type `SamplerState`.
- [ ] Add a narrow default parser that only accepts an empty `SamplerState` default and preserves the constructor default sampler state.
- [ ] Surface `SamplerState` defaults through property lookup metadata as an empty default string for this slice.

### Task 3: Align milestone docs

**Files:**
- Modify: `docs/1.1.0-plan.md`
- Modify: `docs/milestone-boundary.md`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/README.md`

- [ ] Update the documented stable property boundary so it mentions `SamplerState` alongside the already verified property types.
