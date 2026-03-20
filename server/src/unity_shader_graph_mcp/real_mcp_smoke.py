"""One-shot real MCP smoke runner for the Unity batchmode bridge.

This helper starts the local ``--mcp`` server, performs:

1. initialize
2. tools/list
3. tools/call(shadergraph_asset, read_graph_summary)

and then prints a compact summary plus the parsed tool payload.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from pathlib import Path
from typing import Any

from .mcp_smoke import McpStdioSmokeClient
from .unity_bridge import UNITY_EXE_ENV, UNITY_PROJECT_ENV


def main(argv: list[str] | None = None) -> int:
    parser = argparse.ArgumentParser(
        prog="python -m unity_shader_graph_mcp.real_mcp_smoke",
        description="Run a one-shot live MCP smoke against a real Unity batchmode bridge.",
    )
    parser.add_argument(
        "--asset-path",
        required=True,
        help="Shader Graph asset path inside the Unity project, for example Assets/Scripts/Shader/New Shader Graph.shadergraph.",
    )
    parser.add_argument(
        "--unity-exe",
        help=f"Unity executable path. Defaults to ${UNITY_EXE_ENV}.",
    )
    parser.add_argument(
        "--project-path",
        help=f"Unity project path. Defaults to ${UNITY_PROJECT_ENV}.",
    )
    parser.add_argument(
        "--timeout-seconds",
        type=float,
        default=120.0,
        help="How long to wait for each MCP response. Real Unity batchmode startup often needs more than the default smoke timeout.",
    )
    args = parser.parse_args(argv)

    unity_exe = _first_non_blank(args.unity_exe, os.environ.get(UNITY_EXE_ENV))
    project_path = _first_non_blank(args.project_path, os.environ.get(UNITY_PROJECT_ENV))

    if not unity_exe:
        print(
            f"Missing Unity executable. Pass --unity-exe or set {UNITY_EXE_ENV}.",
            file=sys.stderr,
        )
        return 2

    if not project_path:
        print(
            f"Missing Unity project path. Pass --project-path or set {UNITY_PROJECT_ENV}.",
            file=sys.stderr,
        )
        return 2

    env = os.environ.copy()
    env[UNITY_EXE_ENV] = unity_exe
    env[UNITY_PROJECT_ENV] = project_path
    env["PYTHONUNBUFFERED"] = "1"

    repo_root = Path(__file__).resolve().parents[3]
    server_src = Path(__file__).resolve().parents[1]
    command = [sys.executable, "-u", "-m", "unity_shader_graph_mcp", "--mcp"]

    with McpStdioSmokeClient.spawn(command, cwd=server_src, env=env) as client:
        client.timeout_seconds = args.timeout_seconds
        initialize = client.initialize()
        tools = client.list_tools()
        call = client.call_tool(
            "shadergraph_asset",
            {
                "action": "read_graph_summary",
                "path": args.asset_path,
            },
        )

    if "result" not in call:
        print("tools/call did not return a result envelope.", file=sys.stderr)
        print(json.dumps(call, indent=2, sort_keys=True))
        return 1

    raw_payload = call["result"]["content"][0]["text"]
    payload = json.loads(raw_payload)

    print(f"repo: {repo_root}")
    print(f"unity_exe: {unity_exe}")
    print(f"project_path: {project_path}")
    print(f"asset_path: {args.asset_path}")
    print("")
    print(f"initialize_ok: {'result' in initialize}")
    print(f"tools_list_ok: {'result' in tools}")
    print(f"tool_count: {len(tools.get('result', {}).get('tools', []))}")
    print(f"is_error: {call['result'].get('isError')}")
    print(f"success: {payload.get('success')}")
    print(f"message: {payload.get('message')}")
    print(f"status: {payload.get('data', {}).get('status')}")
    print(f"executionBackendKind: {payload.get('data', {}).get('executionBackendKind')}")
    print("")
    print(json.dumps(payload, indent=2, sort_keys=True))

    if call["result"].get("isError"):
        return 1
    if not payload.get("success"):
        return 1
    if payload.get("data", {}).get("status") == "scaffold":
        return 1
    return 0


def _first_non_blank(*values: str | None) -> str | None:
    for value in values:
        if value is not None:
            stripped = value.strip()
            if stripped:
                return stripped
    return None


if __name__ == "__main__":
    raise SystemExit(main())
