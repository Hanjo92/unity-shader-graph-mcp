# Texture2D Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one practical step so external callers can add a `Texture2D` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Texture2D` property support.
- Expose `Texture2D` through `list_supported_properties`.
- Preserve the new type in response metadata for add and update operations.
- Lock the behavior with focused EditMode smoke and metadata tests.

## Approach

The current property path is already stable for `Color`, `Float/Vector1`, `Integer`, `Vector2`, `Vector3`, `Vector4`, and `Boolean`, so this slice reuses the same backend entrypoints and only widens the supported-type table. On the installed Shader Graph package, texture blackboard properties are represented by `UnityEditor.ShaderGraph.Internal.Texture2DShaderProperty`, which exposes a `defaultType` fallback mode and a serialized texture value.

To keep the slice narrow, `Texture2D` defaults will initially cover only the built-in fallback modes, not asset-path assignment. The contract will accept empty defaults plus a small canonical mode surface such as `White`, `Black`, `Grey`, `NormalMap`, `LinearGrey`, and `Red`, along with common lowercase aliases. Empty defaults will fall back to `White`. Broader property expansion such as asset-backed texture assignment, cubemaps, gradients, and virtual textures remains out of scope for this slice.

## Validation

- `list_supported_properties` should include `Texture2D`.
- `add_property` should succeed for a blank graph with a `Texture2D` default mode.
- `find_property` should resolve the added property back to canonical type `Texture2D`.
- Response metadata should preserve `Texture2D` in supported catalogs and property envelopes.

## Non-Goals

- Assigning a real texture asset path as the default value in this slice.
- Expanding multiple texture-family property kinds in one change.
- Reworking the request contract or adding new tool shapes.
