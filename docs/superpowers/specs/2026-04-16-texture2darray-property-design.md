# Texture2DArray Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one narrow texture-family step so external callers can add a `Texture2DArray` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Texture2DArray` property support.
- Expose `Texture2DArray` through `list_supported_properties`.
- Preserve the supported-property catalog and add-property response metadata.

## Design

The current package-backed property flow already supports scalar, vector, boolean, and several texture-family inputs, including `Texture2D`, `Cubemap`, and `Texture3D`. `Texture2DArray` uses `UnityEditor.ShaderGraph.Internal.Texture2DArrayShaderProperty`, which stores a serialized texture-array reference through the same style of reflective property plumbing already used by the existing texture-family slices, so this step can widen the supported-type table without changing the external request contract.

To keep the slice narrow and low-risk, `Texture2DArray` defaults will accept only an empty value for now, meaning "no texture array assigned." Asset-path assignment, import-time validation, and broader texture-array workflow expansion remain out of scope for this step.

## Acceptance Criteria

- `list_supported_properties` includes `Texture2DArray`.
- `add_property` succeeds for a blank graph with an empty `Texture2DArray` default.
- `find_property` resolves the added property back to canonical type `Texture2DArray`.
- Response metadata preserves `Texture2DArray` in supported catalogs and add-property envelopes.
