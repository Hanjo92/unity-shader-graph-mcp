# unity-shader-graph-mcp

Shader Graph focused MCP for Unity.

Current release target: `1.0.0-rc.1`

This release candidate locks the first real package-backed editing loop:

- `create_graph` for blank graphs
- `read_graph_summary`
- `add_property` for `Color` and `Float/Vector1`
- `add_node` for the current graph-addable catalog subset
- `connect_ports` for the currently verified scalar, vector-builder, logic, and early color-routing paths
- `save_graph`

The Unity-side package-backed engine is now strong enough to cut a release candidate.
The server is still transport-agnostic JSON-in/JSON-out CLI rather than a final live MCP transport binding.

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
- GitHub release draft: [github-release-1.0.0-rc.1.md](/Users/song/Projects/unity-shader-graph-mcp/docs/github-release-1.0.0-rc.1.md)
- Final 1.0 checklist: [1.0.0-checklist.md](/Users/song/Projects/unity-shader-graph-mcp/docs/1.0.0-checklist.md)
- Current implementation boundary: [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md)
