# Integer Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one low-risk step so external callers can add an `Integer` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Integer` property support.
- Expose `Integer` through `list_supported_properties`.
- Preserve the new type in response metadata for add and update operations.
- Lock the behavior with focused EditMode smoke and metadata tests.

## Approach

The current property path is already stable for `Color`, `Float/Vector1`, `Vector2`, `Vector3`, `Vector4`, and `Boolean`, so this slice reuses the same backend entrypoints and only widens the supported-type table. On the installed Shader Graph package, integer blackboard properties are represented by `UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty` with `floatType=Integer`, not a distinct runtime shader-input class, so the backend will configure that existing property type in integer mode before assigning the default value.

To keep the slice narrow, `Integer` defaults will accept only invariant-culture whole numbers such as `0`, `1`, or `-3`. Empty defaults will fall back to `0`. Broader property expansion such as textures and enums remains out of scope for this slice.

## Validation

- `list_supported_properties` should include `Integer`.
- `add_property` should succeed for a blank graph with an `Integer` default.
- Response metadata should preserve `Integer` in supported catalogs and property envelopes.

## Non-Goals

- Expanding multiple new property kinds in one change.
- Reworking the request contract or adding non-integer default syntaxes.
- Broadening graph-addable node coverage in the same slice.
