# Vector3 Property Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add package-backed `Vector3` Shader Graph property support with matching catalog and metadata coverage.

**Architecture:** Extend the existing property-type resolution and default-value parsing inside the package-backed backend, then lock the widened surface with smoke and response-metadata tests. Keep the request contract unchanged and limit the new default syntax to a small invariant-culture vector format.

**Tech Stack:** Unity Editor C#, NUnit EditMode tests, reflective Shader Graph package access

---

### Task 1: Add failing tests for the new property type

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphPackageBackedSmokeTests.cs`
- Modify: `packages/unity-shader-graph-mcp/Tests/Editor/ShaderGraphResponseMetadataTests.cs`

- [ ] Add a smoke test that requires `list_supported_properties` to include `Vector3`.
- [ ] Add a smoke test that adds a `Vector3` property to a blank graph and checks the resolved property type.
- [ ] Update metadata tests so the supported-property catalog and package-backed property envelopes preserve `Vector3`.

### Task 2: Implement package-backed Vector3 property support

**Files:**
- Modify: `packages/unity-shader-graph-mcp/Editor/Adapters/ShaderGraphPackageBackend.cs`

- [ ] Extend the supported property type list to include `Vector3`.
- [ ] Resolve `UnityEditor.ShaderGraph.Internal.Vector3ShaderProperty` in the property-type resolver.
- [ ] Teach property-instance inspection to map `Vector3ShaderProperty` back to canonical type `Vector3`.
- [ ] Add a minimal invariant-culture `Vector3` default-value parser and wire it into shader-input creation and update paths.

### Task 3: Refresh boundary docs

**Files:**
- Modify: `docs/milestone-boundary.md`

- [ ] Update the documented property boundary so it mentions `Vector3` alongside the previously shipped property types.

### Task 4: Verify and hand off

**Files:**
- None

- [ ] Run the most relevant available checks for the touched tests.
- [ ] Record any verification gap if Unity EditMode execution is not available in this workspace.
- [ ] Prepare the slice for the next commit once the user confirms the EditMode pass.
