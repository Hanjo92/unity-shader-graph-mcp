# Texture3D Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-backed `Texture3D` Shader Graph property support with matching catalog and metadata coverage.

**Architecture:** Extend the existing property-type resolution and default-value parsing inside the package-backed backend, then lock the wider surface with smoke and response-metadata tests. Keep the request contract unchanged and treat `Texture3D` defaults as a narrow empty-reference path on `Texture3DShaderProperty`.

**Tech Stack:** C#, Unity Shader Graph package reflection, NUnit EditMode tests.

---

### Task 1: Lock the Texture3D contract with tests

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that adds a `Texture3D` property to a blank graph and verifies the resolved property type.
- [ ] Extend the supported-property catalog assertions so `Texture3D` is present.
- [ ] Add a response-metadata test that preserves the `Texture3D` add-property envelope.

### Task 2: Implement package-backed Texture3D property support

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Extend the supported property type list to include `Texture3D`.
- [ ] Resolve the request type to `UnityEditor.ShaderGraph.Internal.Texture3DShaderProperty`.
- [ ] Teach property-instance inspection to map texture3D property instances back to canonical type `Texture3D`.
- [ ] Add a narrow default parser that only accepts an empty `Texture3D` reference and clears the serialized texture slot.
- [ ] Surface `Texture3D` defaults through property lookup metadata using the serialized texture guid when present.

### Task 3: Align milestone docs

**Files:**
- Modify: `docs/1.1.0-plan.md`
- Modify: `docs/milestone-boundary.md`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/README.md`

- [ ] Update the documented stable property boundary so it mentions `Texture3D` alongside the already verified property types.
