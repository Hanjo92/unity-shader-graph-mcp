# Parallel Work Split

This repository is prepared for multiple sub-agents working at the same time.

## Shared Rules

- Do not move or rename top-level directories without coordination.
- Do not edit another agent's write scope.
- If an API shape changes, update `contracts/examples/` first.
- Keep the first milestone small: one Shader Graph tool family only.

## Agent 1: Unity Bridge

Write scope:

- `packages/unity-shader-graph-mcp/`

Owns:

- Unity package manifest
- Editor-only code
- Shader Graph adapters
- Unity request handlers
- C# tests under the package

Must not edit:

- `server/`
- `docs/`

## Agent 2: MCP Server

Write scope:

- `server/`

Owns:

- Python package
- MCP tool registration
- schema validation
- CLI or local dev entrypoints
- Python tests

Must not edit:

- `packages/unity-shader-graph-mcp/`
- `docs/`

## Agent 3: Contracts and Docs

Write scope:

- `contracts/`
- `docs/adr/`
- `docs/roadmap.md`

Owns:

- request and response examples
- ADRs
- milestone definitions
- glossary

Must not edit:

- `packages/unity-shader-graph-mcp/`
- `server/`

## Main Agent

Owns:

- repository root files
- cross-cutting review
- integration checks
- conflict resolution

## Milestone 1

Target:

- create a Shader Graph asset
- inspect a compact graph summary
- add a blackboard property
- add one node
- connect compatible ports
- save and refresh

## 1.0.0 Release Split

The repo is now past the narrow MVP cut. For the final `1.0.0` push, use the release split instead of the original milestone split.

- Server transport stream: `server/src/` and `server/tests/`
- Unity hardening stream: `packages/unity-shader-graph-mcp/Editor/` and `packages/unity-shader-graph-mcp/Tests/Editor/`
- Contracts/docs stream: `contracts/` and `docs/`

See [1.0.0-work-split.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.0.0-work-split.md) for the detailed ownership map.
