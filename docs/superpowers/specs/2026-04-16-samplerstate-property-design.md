# SamplerState Property Design

**Goal:** Extend the package-backed Shader Graph property surface by one narrow utility-input step so external callers can add a `SamplerState` blackboard property through the existing `add_property` contract.

## Scope

- Add package-backed `SamplerState` property support.
- Expose `SamplerState` through `list_supported_properties`.
- Preserve the supported-property catalog and add-property response metadata.

## Design

The current package-backed property flow already supports scalar, vector, boolean, texture-family, and gradient inputs. `SamplerState` is represented by `UnityEditor.ShaderGraph.SamplerStateShaderProperty`, which carries a `TextureSamplerState` value with constructor defaults for filter, wrap, and anisotropic mode. The backend can widen the existing reflective property flow to this type without changing the external request contract.

To keep the slice narrow and low-risk, `SamplerState` defaults will accept only an empty value for now. That means the backend preserves the constructor-provided default sampler state and reports an empty default string through the MCP contract rather than introducing filter/wrap/anisotropic parsing in this step.

## Acceptance Criteria

- `list_supported_properties` includes `SamplerState`.
- `add_property` succeeds for a blank graph with an empty `SamplerState` default.
- `find_property` resolves the added property back to canonical type `SamplerState`.
- Response metadata preserves `SamplerState` in supported catalogs and add-property envelopes.
