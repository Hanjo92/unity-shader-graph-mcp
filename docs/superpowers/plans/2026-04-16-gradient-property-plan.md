# Gradient Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-backed `Gradient` Shader Graph property support with matching catalog and metadata coverage.

**Architecture:** Extend the existing property-type resolution and default-value parsing inside the package-backed backend, then lock the wider surface with smoke and response-metadata tests. Keep the request contract unchanged and treat `Gradient` defaults as a narrow empty-default path on `GradientShaderProperty`.

**Tech Stack:** C#, Unity Shader Graph package reflection, NUnit EditMode tests.

---

### Task 1: Lock the Gradient contract with tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that adds a `Gradient` property to a blank graph and verifies the resolved property type.
- [ ] Extend the supported-property catalog assertions so `Gradient` is present.
- [ ] Add a response-metadata test that preserves the `Gradient` add-property envelope.

### Task 2: Implement package-backed Gradient property support

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Extend the supported property type list to include `Gradient`.
- [ ] Resolve the request type to `UnityEditor.ShaderGraph.GradientShaderProperty`.
- [ ] Teach property-instance inspection to map gradient property instances back to canonical type `Gradient`.
- [ ] Add a narrow default parser that only accepts an empty `Gradient` default and preserves a blank contract default string.
- [ ] Surface `Gradient` defaults through property lookup metadata as an empty default string for this slice.

### Task 3: Align milestone docs

**Files:**
- Modify: `docs/1.1.0-plan.md`
- Modify: `docs/milestone-boundary.md`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/README.md`

- [ ] Update the documented stable property boundary so it mentions `Gradient` alongside the already verified property types.
