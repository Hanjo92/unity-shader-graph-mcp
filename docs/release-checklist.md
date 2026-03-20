# Release Checklist

## Target

- Repo release: `1.0.0-rc.1`
- Unity package: `1.0.0-rc.1`
- Python package metadata: `1.0.0rc1`

## Before Tagging

- Run Unity `EditMode` tests and confirm full pass.
- Run `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`.
- Run the newest fan-in / chaining debug smoke menus only if you changed matrix coverage.
- Run `python3 -m unittest server.tests.test_mcp_transport_subprocess` to confirm live `--mcp` subprocess smoke passes.
- For final `1.0.0`, run one real Unity batchmode bridge smoke with `UNITY_SHADER_GRAPH_MCP_UNITY_EXE` and `UNITY_SHADER_GRAPH_MCP_UNITY_PROJECT` set.
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
- Call out whether the release was verified with scaffold fallback only or with the real Unity batchmode bridge enabled.

## After Tagging

- Open a fresh Unity project smoke check with the package imported from the release cut.
- Verify one blank graph can complete the full happy path through `Tools > Shader Graph MCP > Debug > Run Blank Graph Happy Path`.
- Verify one external MCP client can complete `initialize -> tools/list -> tools/call` against `python3 server/src/unity_shader_graph_mcp/__main__.py --mcp`.
- Verify that same external MCP flow once more with the Unity bridge environment variables enabled.
- Use post-release feedback to decide whether final `1.0.0` only needs transport/productization work or also needs more node/port matrix coverage.
