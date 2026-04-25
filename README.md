# unity-shader-graph-mcp

Shader Graph focused MCP for Unity.

Current stable release: `1.2.0`

Next milestone target: post-`1.2` triage

`1.2.0` closes the focused post-`1.1` backlog while keeping the
package-backed editing loop explicit and verification-driven:

- `create_graph` for blank graphs
- `read_graph_summary`
- `add_property` for the current verified scalar, vector, texture, gradient, sampler, and boolean property types
- property-bound `PropertyNode` workflows and graph contract replay metadata
- `add_node` for the current verified graph-addable catalog subset
- `connect_ports` for the currently verified scalar, vector-builder, logic, texture, normal, color-routing, and property-node paths
- Boolean-bound `PropertyNode.Out -> BranchNode.Predicate`
- `SampleGradient` graph-addable promotion
- graph and subgraph export/import contract path parity
- `save_graph`

The Unity-side package-backed engine and Unity batchmode MCP bridge are now stable enough for the focused `1.2.0` cut.
The server now supports a live stdio MCP transport and an optional Unity batchmode bridge for real external tool calls.
The `1.2.0` milestone is now closed; remaining engine expansion and authoring-surface work should be tracked as post-`1.2` follow-up scope.

This repository is intentionally split into independent work areas so multiple sub-agents can work in parallel with minimal merge risk.

## Workspace Layout

- `packages/unity-shader-graph-mcp/`: Unity package and Editor bridge
- `server/`: Python MCP server
- `contracts/`: shared JSON examples and protocol notes
- `docs/`: architecture, ADRs, and parallel work instructions

## Parallel Work Rule

Each sub-agent owns a write scope. Do not edit files outside your assigned scope unless the owner explicitly hands them off.

See `docs/parallel-work-split.md` for the current task split.

## Release Notes

- Changelog: [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md)
- Release checklist: [release-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/release-checklist.md)
- GitHub release draft: [github-release-1.2.0.md](/Users/song/Projects/unity-shader-graph-mcp/docs/github-release-1.2.0.md)
- Final 1.0 checklist: [1.0.0-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.0.0-checklist.md)
- Final 1.0 work split: [1.0.0-work-split.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.0.0-work-split.md)
- Completed 1.1 plan: [1.1.0-plan.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.1.0-plan.md)
- Compatibility matrix: [compatibility-matrix.md](/Users/song/Projects/unity-shader-graph-mcp/docs/compatibility-matrix.md)
- Current implementation boundary: [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)
