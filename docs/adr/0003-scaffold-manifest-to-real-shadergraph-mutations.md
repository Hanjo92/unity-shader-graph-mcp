# ADR 0003: Scaffold Manifest First, Real Shader Graph Later

## Status

Accepted

## Context

The repository needs an end-to-end path before the exact Unity Shader Graph API surface is fully wired. That means the current implementation can create assets, persist a sidecar scaffold manifest, and validate contracts without pretending the real graph mutation API is already solved.

## Decision

We will treat the scaffold manifest as a temporary compatibility layer and keep the external JSON contract stable while the Unity-side implementation is replaced with real Shader Graph mutation calls later.

## Consequences

- The server CLI and contract examples can be exercised now.
- Unity-side implementation can be swapped in incrementally without changing tool names or request shapes.
- Real Shader Graph API work is isolated to the Unity package instead of leaking into docs and contract definitions.
- The scaffold manifest should be removed only after the real graph mutation path can create, mutate, and save graphs reliably in the target Unity version.

## Replacement Criteria

The scaffold layer can be retired when all of the following are true:

- Graph creation uses the actual Unity Shader Graph API.
- Properties, nodes, and ports are created through the real graph model.
- Save and refresh use the version-specific Unity serialization pipeline.
- Existing contract examples still pass unchanged.
