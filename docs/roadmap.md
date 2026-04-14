# Roadmap

## Milestone 1 / 1.0.0

Deliver a narrow but real Shader Graph editing path:

- create graph with a blank-only package-backed path
- read summary
- add property
- add node
- connect ports for the verified package-backed paths
- save through the package-backed validate + write + refresh flow
- keep contracts/examples ahead of implementation changes

This milestone is shipped and closed. The remaining expansion work now rolls
forward into `1.1.0`.

Candidate next connection area:

- broader Color routing beyond the current `Multiply` / `Branch` / `Lerp` / `Append` vector paths, richer boolean fan-out beyond `Comparison -> Branch.Predicate`, and a wider node/port matrix beyond the current verified scalar, vector-builder, logic, and early Color-routing paths

## Milestone 2 / 1.1.0

Expand the supported surface on top of the shipped `1.0.0` base:

- broaden the remaining package-backed connection matrix
- promote broader property and graph-addable node coverage carefully
- harden the supported subgraph composition paths
- keep diagnostics, compatibility notes, and metadata aligned with the real runtime boundary

See [1.1.0-plan.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.1.0-plan.md) for the current target cut.

## Milestone 3 / Post-1.1

Production hardening:

- broad authoring ergonomics beyond the current verified matrix approach
- compatibility matrix by Unity version and Shader Graph package version; see [compatibility-matrix.md](/Users/song/Projects/unity-shader-graph-mcp/docs/compatibility-matrix.md)
- still-deferred universal node/port ambitions
