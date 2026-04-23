# Vector3 Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one low-risk step so external callers can add a `Vector3` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Vector3` property support.
- Expose `Vector3` through `list_supported_properties`.
- Preserve the new type in response metadata for add and update operations.
- Lock the behavior with focused EditMode smoke and metadata tests.

## Approach

The current property path is already stable for `Color`, `Float/Vector1`, and `Vector2`, so this slice reuses the same backend entrypoints and only widens the supported-type table. The backend will resolve `UnityEditor.ShaderGraph.Internal.Vector3ShaderProperty`, create the shader input through the existing reflection path, and parse a simple invariant-culture default value format.

To keep the slice narrow, `Vector3` defaults will accept only three numeric components, either comma-separated or whitespace-separated. Empty defaults will fall back to `(0, 0, 0)`. Broader property expansion such as `Vector4`, `Boolean`, and textures remains out of scope for this slice.

## Validation

- `list_supported_properties` should include `Vector3`.
- `add_property` should succeed for a blank graph with a `Vector3` default.
- Response metadata should preserve `Vector3` in supported catalogs and property envelopes.

## Non-Goals

- Expanding multiple new property kinds in one change.
- Reworking the request contract or adding new property-default syntaxes.
- Broadening graph-addable node coverage in the same slice.
