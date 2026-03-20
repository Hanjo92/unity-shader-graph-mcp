# Release Checklist

## Target

- Repo release: `1.0.0-rc.1`
- Unity package: `1.0.0-rc.1`
- Python package metadata: `1.0.0rc1`

## Before Tagging

- Run Unity `EditMode` tests and confirm full pass.
- Run the current debug smoke menus for the newest fan-in / chaining paths.
- Confirm [milestone-boundary.md](/Users/song/Projects/unity-shader-graph-mcp/docs/milestone-boundary.md) still matches runtime behavior.
- Confirm [CHANGELOG.md](/Users/song/Projects/unity-shader-graph-mcp/CHANGELOG.md) describes the actual release scope.
- Confirm package versions:
  - [package.json](/Users/song/Projects/unity-shader-graph-mcp/packages/unity-shader-graph-mcp/package.json)
  - [pyproject.toml](/Users/song/Projects/unity-shader-graph-mcp/server/pyproject.toml)

## Release Payload

- Unity package under `packages/unity-shader-graph-mcp`
- Python server under `server`
- contracts examples under `contracts`
- implementation boundary docs under `docs`

## Release Message

- Describe this cut as the first package-backed Shader Graph editing release candidate.
- Call out that the core graph mutation engine is real and EditMode-tested.
- Call out that live MCP transport binding is still not the final 1.0 transport surface.

## After Tagging

- Open a fresh Unity project smoke check with the package imported from the release cut.
- Verify one blank graph can complete:
  - `create_graph`
  - `add_property`
  - `add_node`
  - `connect_ports`
  - `save_graph`
- Use post-release feedback to decide whether final `1.0.0` only needs transport/productization work or also needs more node/port matrix coverage.
