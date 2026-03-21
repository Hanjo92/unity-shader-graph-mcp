from __future__ import annotations

import json
import subprocess
import sys
import unittest
from pathlib import Path
from unittest.mock import patch

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.tools.shadergraph_asset import handle_shadergraph_asset
from unity_shader_graph_mcp.unity_bridge import (
    DEFAULT_REQUEST_ARGUMENT,
    DEFAULT_RESPONSE_ARGUMENT,
    DEFAULT_EXECUTE_METHOD,
    UNITY_EXE_ENV,
    UNITY_PROJECT_ENV,
    UnityBatchmodeBridgeConfig,
    UnityBridgeError,
    build_unity_batchmode_bridge,
)


class UnityBridgeTests(unittest.TestCase):
    def test_bridge_is_disabled_without_required_environment(self) -> None:
        bridge = build_unity_batchmode_bridge({})
        self.assertIsNone(bridge)

    def test_bridge_config_uses_default_optional_values(self) -> None:
        config = UnityBatchmodeBridgeConfig.from_environment(
            {
                UNITY_EXE_ENV: "/Applications/Unity/Hub/Editor/Unity",
                UNITY_PROJECT_ENV: "/Users/song/Projects/ExampleProject",
            }
        )

        self.assertIsNotNone(config)
        assert config is not None
        self.assertEqual(config.execute_method, DEFAULT_EXECUTE_METHOD)
        self.assertEqual(config.request_argument, DEFAULT_REQUEST_ARGUMENT)
        self.assertEqual(config.response_argument, DEFAULT_RESPONSE_ARGUMENT)

    def test_bridge_invokes_unity_and_parses_response_file(self) -> None:
        recorded: dict[str, list[str]] = {}

        def fake_run(command: list[str], **kwargs: object) -> subprocess.CompletedProcess[str]:
            recorded["command"] = command
            response_path = Path(command[command.index(f"-{DEFAULT_RESPONSE_ARGUMENT}") + 1])
            response_path.write_text(
                json.dumps(
                    {
                        "success": True,
                        "message": "Loaded package-backed Shader Graph summary.",
                        "data": {
                            "status": "package-backed",
                            "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                        },
                    }
                ),
                encoding="utf-8",
            )
            return subprocess.CompletedProcess(command, 0, "", "")

        bridge = build_unity_batchmode_bridge(
            {
                UNITY_EXE_ENV: "/Applications/Unity/Hub/Editor/Unity",
                UNITY_PROJECT_ENV: "/Users/song/Projects/ExampleProject",
            },
            runner=fake_run,
        )
        assert bridge is not None

        response = bridge.invoke(
            {
                "action": "read_graph_summary",
                "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "template": None,
                "name": None,
                "payload": {
                    "action": "read_graph_summary",
                    "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                },
            }
        )

        command = recorded["command"]
        self.assertEqual(command[0], "/Applications/Unity/Hub/Editor/Unity")
        self.assertEqual(command[1:5], ["-batchmode", "-nographics", "-quit", "-projectPath"])
        self.assertIn("-executeMethod", command)
        self.assertIn(f"-{DEFAULT_REQUEST_ARGUMENT}", command)
        self.assertIn(f"-{DEFAULT_RESPONSE_ARGUMENT}", command)
        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["status"], "package-backed")
        self.assertEqual(
            response["data"]["assetPath"],
            "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        )

    def test_bridge_raises_when_unity_does_not_write_response_file(self) -> None:
        def fake_run(command: list[str], **kwargs: object) -> subprocess.CompletedProcess[str]:
            return subprocess.CompletedProcess(command, 0, "", "")

        bridge = build_unity_batchmode_bridge(
            {
                UNITY_EXE_ENV: "/Applications/Unity/Hub/Editor/Unity",
                UNITY_PROJECT_ENV: "/Users/song/Projects/ExampleProject",
            },
            runner=fake_run,
        )
        assert bridge is not None

        with self.assertRaises(UnityBridgeError):
            bridge.invoke(
                {
                    "action": "save_graph",
                    "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                    "template": None,
                    "name": None,
                    "payload": {
                        "action": "save_graph",
                        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                    },
                }
            )

    def test_handler_uses_bridge_when_available(self) -> None:
        class FakeBridge:
            def __init__(self) -> None:
                self.calls: list[dict[str, object]] = []

            def invoke(self, request: dict[str, object]) -> dict[str, object]:
                self.calls.append(request)
                return {
                    "success": True,
                    "message": "Loaded package-backed shader graph.",
                    "data": {
                        "status": "package-backed",
                        "request": request,
                    },
                }

        bridge = FakeBridge()
        response = handle_shadergraph_asset(
            {
                "action": "add_property",
                "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "ExampleColor",
                "propertyType": "Color",
                "defaultValue": "[1,1,1,1]",
            },
            bridge=bridge,
        )

        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["status"], "package-backed")
        self.assertEqual(bridge.calls[0]["propertyName"], "ExampleColor")
        self.assertEqual(bridge.calls[0]["propertyType"], "Color")
        self.assertIn("request", bridge.calls[0])

    def test_handler_falls_back_to_scaffold_when_bridge_is_unavailable(self) -> None:
        with patch(
            "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
            return_value=None,
        ):
            response = handle_shadergraph_asset(
                {
                    "action": "save_graph",
                    "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                }
            )

        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["status"], "scaffold")


if __name__ == "__main__":
    unittest.main()
