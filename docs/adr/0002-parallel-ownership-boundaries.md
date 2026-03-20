# ADR 0002: Parallel Ownership Boundaries

## Status

Accepted

## Context

Multiple sub-agents will work in this repository at the same time. Without explicit ownership boundaries, parallel edits would overlap and create avoidable merge conflicts and unclear responsibility.

## Decision

We will split the repository into disjoint write scopes and require shared contracts to be updated before implementation details when a shape changes.

## Consequences

- Sub-agents can work independently with lower collision risk.
- Cross-cutting changes stay visible through `contracts/` instead of hidden inside implementation files.
- The main integration pass becomes simpler because ownership is already documented.
