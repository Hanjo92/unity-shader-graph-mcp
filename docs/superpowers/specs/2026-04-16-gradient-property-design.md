# Gradient Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one narrow non-texture step so external callers can add a `Gradient` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `Gradient` property support.
- Expose `Gradient` through `list_supported_properties`.
- Preserve the supported-property catalog and add-property response metadata.

## Design

The current package-backed property flow already supports scalar, vector, boolean, and several texture-family inputs. `Gradient` is represented by `UnityEditor.ShaderGraph.GradientShaderProperty`, which is graph-addable through the Shader Graph editor menu and uses a standard shader-input instance that can be created reflectively through the same backend flow as the existing property slices.

To keep the slice narrow and low-risk, `Gradient` defaults will accept only an empty value for now. That means the backend will preserve the constructor-provided default `Gradient` instance and will report an empty default string through the MCP contract rather than attempting to serialize gradient keys and modes in this step.

## Acceptance Criteria

- `list_supported_properties` includes `Gradient`.
- `add_property` succeeds for a blank graph with an empty `Gradient` default.
- `find_property` resolves the added property back to canonical type `Gradient`.
- Response metadata preserves `Gradient` in supported catalogs and add-property envelopes.
