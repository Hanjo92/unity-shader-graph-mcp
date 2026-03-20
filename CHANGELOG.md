# Changelog

All notable changes to this project will be documented in this file.

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
