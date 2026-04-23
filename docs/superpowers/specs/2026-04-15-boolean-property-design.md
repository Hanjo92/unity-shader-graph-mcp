# Boolean Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one low-risk step so external callers can add a `Boolean` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Boolean` property support.
- Expose `Boolean` through `list_supported_properties`.
- Preserve the new type in response metadata for add and update operations.
- Lock the behavior with focused EditMode smoke and metadata tests.

## Approach

The current property path is already stable for `Color`, `Float/Vector1`, `Vector2`, and `Vector3`, so this slice reuses the same backend entrypoints and only widens the supported-type table. The backend will resolve `UnityEditor.ShaderGraph.Internal.BooleanShaderProperty`, create the shader input through the existing reflection path, and parse a small boolean default-value surface.

To keep the slice narrow, `Boolean` defaults will accept `true` and `false`, plus `1` and `0` as explicit aliases. Empty defaults will fall back to `false`. Broader property expansion such as `Vector4`, textures, and enums remains out of scope for this slice.

## Validation

- `list_supported_properties` should include `Boolean`.
- `add_property` should succeed for a blank graph with a `Boolean` default.
- Response metadata should preserve `Boolean` in supported catalogs and property envelopes.

## Non-Goals

- Expanding multiple new property kinds in one change.
- Reworking the request contract or adding new property-default syntaxes beyond the small boolean aliases.
- Broadening graph-addable node coverage in the same slice.
