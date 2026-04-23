# Cubemap Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one narrow texture-family step so external callers can add a `Cubemap` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Cubemap` property support.
- Expose `Cubemap` through `list_supported_properties`.
- Preserve the new type in response metadata for add and update operations.
- Lock the behavior with focused EditMode smoke and metadata tests.

## Approach

The current property path is already stable for `Color`, `Float/Vector1`, `Integer`, `Vector2`, `Vector3`, `Vector4`, `Boolean`, and `Texture2D`, so this slice reuses the same backend entrypoints and only widens the supported-type table. On the installed Shader Graph package, cubemap blackboard properties are represented by `UnityEditor.ShaderGraph.Internal.CubemapShaderProperty`, which stores a serialized cubemap reference.

To keep the slice narrow, `Cubemap` defaults will not accept asset paths yet. The contract will accept only an empty default value, which means "no cubemap assigned." Broader cubemap expansion such as asset-path assignment, import-time validation, or cubemap-node workflow promotion remains out of scope for this slice.

## Validation

- `list_supported_properties` should include `Cubemap`.
- `add_property` should succeed for a blank graph with an empty `Cubemap` default.
- `find_property` should resolve the added property back to canonical type `Cubemap`.
- Response metadata should preserve `Cubemap` in supported catalogs and property envelopes.

## Non-Goals

- Assigning a real cubemap asset path as the default value in this slice.
- Expanding multiple texture-family property kinds in one change.
- Reworking the request contract or adding new tool shapes.
