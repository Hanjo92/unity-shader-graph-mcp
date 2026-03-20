from __future__ import annotations

import os
import sys
import unittest
from pathlib import Path

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.mcp_smoke import McpStdioSmokeClient


class McpTransportSubprocessTests(unittest.TestCase):
    def test_real_mcp_entrypoint_handles_initialize_list_and_call(self) -> None:
        env = os.environ.copy()
        env["PYTHONUNBUFFERED"] = "1"

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


if __name__ == "__main__":
    unittest.main()
