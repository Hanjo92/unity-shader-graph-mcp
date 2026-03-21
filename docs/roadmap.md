# Roadmap

## Milestone 1

Deliver a narrow but real Shader Graph editing path:

- create graph with a blank-only package-backed path
- read summary
- add property
- add node
- connect ports for the verified package-backed paths
- save through the package-backed validate + write + refresh flow
- keep contracts/examples ahead of implementation changes

Candidate next connection area:

- broader Color routing beyond the current `Multiply` / `Branch` / `Lerp` / `Append` vector paths, richer boolean fan-out beyond `Comparison -> Branch.Predicate`, and a wider node/port matrix beyond the current verified scalar, vector-builder, logic, and early Color-routing paths

## Milestone 2 / 1.1.0

Improve graph ergonomics:

- node lookup by type and title
- supported node catalog exposure for external callers
- deterministic node identity lookup helpers
- subgraph support
- property updates
- node positioning helpers

See [1.1.0-plan.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.1.0-plan.md) for the current target cut.

## Milestone 3

Production hardening:

- better validation
- richer error messages
- compatibility matrix by Unity version
- test fixtures and sample graphs
