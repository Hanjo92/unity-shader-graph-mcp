# Texture2DArray Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-backed `Texture2DArray` Shader Graph property support with matching catalog and metadata coverage.

**Architecture:** Extend the existing property-type resolution and default-value parsing inside the package-backed backend, then lock the wider surface with smoke and response-metadata tests. Keep the request contract unchanged and treat `Texture2DArray` defaults as a narrow empty-reference path on `Texture2DArrayShaderProperty`.

**Tech Stack:** C#, Unity Shader Graph package reflection, NUnit EditMode tests.

---

### Task 1: Lock the Texture2DArray contract with tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that adds a `Texture2DArray` property to a blank graph and verifies the resolved property type.
- [ ] Extend the supported-property catalog assertions so `Texture2DArray` is present.
- [ ] Add a response-metadata test that preserves the `Texture2DArray` add-property envelope.

### Task 2: Implement package-backed Texture2DArray property support

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Extend the supported property type list to include `Texture2DArray`.
- [ ] Resolve the request type to `UnityEditor.ShaderGraph.Internal.Texture2DArrayShaderProperty`.
- [ ] Teach property-instance inspection to map texture-array property instances back to canonical type `Texture2DArray`.
- [ ] Add a narrow default parser that only accepts an empty `Texture2DArray` reference and clears the serialized texture-array slot.
- [ ] Surface `Texture2DArray` defaults through property lookup metadata using the serialized texture-array guid when present.

### Task 3: Align milestone docs

**Files:**
- Modify: `docs/1.1.0-plan.md`
- Modify: `docs/milestone-boundary.md`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/README.md`

- [ ] Update the documented stable property boundary so it mentions `Texture2DArray` alongside the already verified property types.
