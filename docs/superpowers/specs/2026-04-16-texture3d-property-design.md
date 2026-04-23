# Texture3D Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one narrow texture-family step so external callers can add a `Texture3D` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Texture3D` property support.
- Expose `Texture3D` through `list_supported_properties`.
- Preserve the supported-property catalog and add-property response metadata.

## Design

The existing property flow already supports scalar, vector, boolean, `Texture2D`, and `Cubemap` inputs through the package-backed `GraphData.AddGraphInput(...)` path. `Texture3D` uses `UnityEditor.ShaderGraph.Internal.Texture3DShaderProperty`, which mirrors the same serialized-texture pattern as the current texture-family properties, so this slice can widen the existing property-type resolution and default parsing without changing the external request shape.

To keep the slice narrow and low-risk, `Texture3D` defaults will accept only an empty value for now, meaning "no texture assigned." Asset-path assignment, import-time validation, and broader texture-family expansion remain out of scope for this step.

## Acceptance Criteria

- `list_supported_properties` includes `Texture3D`.
- `add_property` succeeds for a blank graph with an empty `Texture3D` default.
- `find_property` resolves the added property back to canonical type `Texture3D`.
- Response metadata preserves `Texture3D` in supported catalogs and add-property envelopes.
