from __future__ import annotations

import os
import stat
import sys
import tempfile
import textwrap
import unittest
from pathlib import Path

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.mcp_smoke import McpStdioSmokeClient
from unity_shader_graph_mcp.unity_bridge import (
    UNITY_EXE_ENV,
    UNITY_PROJECT_ENV,
)


class McpTransportSubprocessTests(unittest.TestCase):
    def test_real_mcp_entrypoint_handles_initialize_list_and_call(self) -> None:
        env = os.environ.copy()
        env["PYTHONUNBUFFERED"] = "1"
        env.pop(UNITY_EXE_ENV, None)
        env.pop(UNITY_PROJECT_ENV, None)

        command = [
            sys.executable,
            "-u",
            "-m",
            "unity_shader_graph_mcp",
            "--mcp",
        ]

        with McpStdioSmokeClient.spawn(command, cwd=SRC_ROOT, env=env) as client:
            initialize = client.initialize()
            self.assertEqual(initialize["jsonrpc"], "2.0")
            self.assertEqual(initialize["result"]["serverInfo"]["name"], "unity-shader-graph-mcp")
            self.assertEqual(initialize["result"]["protocolVersion"], "2024-11-05")

            tools_list = client.list_tools()
            self.assertEqual(tools_list["result"]["tools"][0]["name"], "shadergraph_asset")

            tool_call = client.call_tool(
                "shadergraph_asset",
                {
                    "action": "save_graph",
                    "path": "Assets/ShaderGraphs/Example.shadergraph",
                },
            )
            self.assertFalse(tool_call["result"]["isError"])

            payload = tool_call["result"]["content"][0]["text"]
            self.assertIn('"success": true', payload)
            self.assertIn('"action": "save_graph"', payload)

    def test_real_mcp_entrypoint_uses_unity_bridge_when_environment_is_configured(self) -> None:
        env = os.environ.copy()
        env["PYTHONUNBUFFERED"] = "1"

        with tempfile.TemporaryDirectory(prefix="unity-shader-graph-mcp-bridge-test-") as tempdir:
            tempdir_path = Path(tempdir)
            fake_project_path = tempdir_path / "ExampleUnityProject"
            fake_project_path.mkdir(parents=True, exist_ok=True)

            fake_unity_executable = tempdir_path / "fake-unity.py"
            fake_unity_executable.write_text(
                textwrap.dedent(
                    """\
                    #!/usr/bin/env python3
                    import json
                    import sys
                    from pathlib import Path

                    def read_argument(name: str) -> str:
                        for index, value in enumerate(sys.argv):
                            if value == name and index + 1 < len(sys.argv):
                                return sys.argv[index + 1]
                            if value.startswith(name + "="):
                                return value.split("=", 1)[1]
                        raise RuntimeError(f"Missing required argument: {name}")

                    request_path = Path(read_argument("-shaderGraphMcpRequestPath"))
                    response_path = Path(read_argument("-shaderGraphMcpResponsePath"))
                    request = json.loads(request_path.read_text(encoding="utf-8"))

                    response = {
                        "success": True,
                        "message": f"Fake Unity handled {request.get('action')}.",
                        "data": {
                            "status": "package-backed",
                            "executionBackendKind": "PackageBacked",
                            "echo": request,
                        },
                    }
                    response_path.write_text(json.dumps(response), encoding="utf-8")
                    """
                ),
                encoding="utf-8",
            )
            fake_unity_executable.chmod(fake_unity_executable.stat().st_mode | stat.S_IXUSR)

            env[UNITY_EXE_ENV] = str(fake_unity_executable)
            env[UNITY_PROJECT_ENV] = str(fake_project_path)

            command = [
                sys.executable,
                "-u",
                "-m",
                "unity_shader_graph_mcp",
                "--mcp",
            ]

            with McpStdioSmokeClient.spawn(command, cwd=SRC_ROOT, env=env) as client:
                client.initialize()
                client.list_tools()
                tool_call = client.call_tool(
                    "shadergraph_asset",
                    {
                        "action": "read_graph_summary",
                        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                    },
                )

            self.assertFalse(tool_call["result"]["isError"])

            payload = tool_call["result"]["content"][0]["text"]
            self.assertIn('"status": "package-backed"', payload)
            self.assertIn('"executionBackendKind": "PackageBacked"', payload)
            self.assertIn('"action": "read_graph_summary"', payload)
            self.assertNotIn('"status": "scaffold"', payload)


if __name__ == "__main__":
    unittest.main()
