# SamplerState Defaults Design

**Goal:** Extend the package-backed Shader Graph `SamplerState` property path so external callers can set a real sampler default through the existing `add_property` and `update_property` contract.

## Scope

- Keep the supported property type surface unchanged at `SamplerState`.
- Expand `SamplerState` default parsing beyond the current empty-only path.
- Preserve a canonical contract string for add, update, find, summary, and exported property metadata.

## Design

The current package-backed backend already resolves `UnityEditor.ShaderGraph.SamplerStateShaderProperty`, but it only accepts an empty default and reports an empty default string back to callers. Unity's installed Shader Graph package stores the effective sampler configuration in `TextureSamplerState`, which exposes `filter`, `wrap`, and `anisotropic` enum properties. That means the backend can widen the current reflective parser without changing tool shape or request routing.

This slice keeps the external contract intentionally narrow:

- empty input resets or preserves the package default sampler state
- explicit input uses a canonical comma-separated form: `Filter, Wrap, Anisotropic`
- `Anisotropic` is optional on input and defaults to `None`
- output metadata always uses the full canonical form, for example `Linear, Repeat, None` or `Point, Clamp, x4`

The accepted value set stays small and package-aligned:

- `Filter`: `Linear`, `Point`, `Trilinear`
- `Wrap`: `Repeat`, `Clamp`, `Mirror`, `MirrorOnce`
- `Anisotropic`: `None`, `x2`, `x4`, `x8`, `x16`

Input matching should stay case-insensitive and whitespace-tolerant. Common punctuation variants for `MirrorOnce` such as `Mirror Once`, `Mirror_Once`, and `Mirror-Once` are acceptable, but the backend should always normalize them back to `MirrorOnce` in responses.

## Acceptance Criteria

- `add_property` succeeds for `SamplerState` with an explicit value such as `Point, Clamp, x4`.
- `update_property` can reset or replace an existing `SamplerState` default.
- `addedProperty.defaultValue`, `updatedProperty.defaultValue`, `find_property`, and graph-summary property metadata all use the canonical string form.
- empty input canonicalizes to `Linear, Repeat, None`.
- invalid filter, wrap, or anisotropic tokens fail with a clear supported-values message.
