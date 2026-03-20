# Changelog

All notable changes to this project will be documented in this file.

## 1.0.0 - 2026-03-21

First stable release of the package-backed Unity Shader Graph MCP stack.

### Added

- Live stdio MCP transport backed by the Python server registry.
- Optional Unity batchmode bridge for real external tool calls.
- Unity batchmode request/response bridge entrypoint and EditMode tests.
- Real MCP smoke helper for one-shot external bridge verification.
- End-to-end smoke coverage for:
  - live MCP stdio transport
  - fake Unity bridge subprocess verification
  - real Unity bridge manual verification guidance

### Changed

- Promoted the Unity package version to `1.0.0`.
- Promoted the Python package metadata to `1.0.0`.
- Promoted the MCP server version handshake to `1.0.0`.
- Updated install, smoke, and release docs to describe the real Unity batchmode bridge path.

### Verified

- Unity EditMode tests passed for the package-backed engine and batchmode bridge contract.
- Python server tests passed for transport, bridge fallback, and subprocess smoke coverage.
- Real MCP client flow succeeded through `initialize -> tools/list -> tools/call(read_graph_summary)` against a real Unity project with the Unity batchmode bridge enabled.

## 1.0.0-rc.1 - 2026-03-20

Release candidate for the first package-backed Shader Graph editing engine.

### Added

- Real package-backed `create_graph` for blank graphs.
- Real package-backed `read_graph_summary`.
- Real package-backed `add_property` for `Color` and `Float/Vector1`.
- Catalog-driven package-backed `add_node`.
- Real package-backed `save_graph`.
- Package-backed `connect_ports` coverage for:
  - `Vector1 -> Vector1`
  - `Color -> Split`
  - `Split -> Vector1`
  - scalar routing into `Combine` and `Vector2/Vector3/Vector4`
  - `Combine/Vector4 -> Split`
  - scalar arithmetic nodes and arithmetic chaining
  - `Comparison -> Branch` logic flow
  - early color-routing through `Multiply`, `Lerp`, `Branch`, and `Append`
  - mixed Append chaining
  - reverse mixed chains into `Append`
  - vector fan-in `Combine/Vector4 -> Append -> Split`
  - fan-in continuation chains `Combine -> Append -> Lerp -> Split` and `Vector4 -> Append -> Branch -> Split`
- Unity Editor debug menus for manual graph mutation smoke tests.
- EditMode smoke tests for the current verified package-backed mutation matrix.
- Compatibility probe and node catalog diagnostics for Unity 2022.3 + Shader Graph 17.3.0.

### Changed

- Promoted the current repo cut from `0.1.0` to `1.0.0-rc.1` for the Unity package.
- Promoted the Python package metadata to `1.0.0rc1` for PEP 440 compatibility.
- Expanded docs to describe current verified runtime boundaries and release scope.

### Known Limits

- Template-backed graph creation is still unsupported. Only blank graph creation is package-backed.
- Blackboard property coverage is still limited to `Color` and `Float/Vector1`.
- Live MCP transport binding is not finalized yet. The server remains a transport-agnostic CLI/in-process tool layer.
- Connection coverage is intentionally matrix-based and verified-path-only, not “all nodes / all ports”.
