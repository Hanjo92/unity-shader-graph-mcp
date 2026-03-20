from __future__ import annotations

import sys
import unittest
from pathlib import Path

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.contracts import ShaderGraphRequestError
from unity_shader_graph_mcp.server import build_server
from unity_shader_graph_mcp.tools import (
    SUPPORTED_SHADERGRAPH_ASSET_ACTIONS,
    handle_shadergraph_asset,
    normalize_shadergraph_asset_request,
)


class ShaderGraphAssetToolTests(unittest.TestCase):
    def test_supported_actions_match_milestone_one_scope(self) -> None:
        self.assertEqual(
            SUPPORTED_SHADERGRAPH_ASSET_ACTIONS,
            (
                "create_graph",
                "read_graph_summary",
                "add_property",
                "add_node",
                "connect_ports",
                "save_graph",
            ),
        )

    def test_handler_returns_scaffold_response(self) -> None:
        response = handle_shadergraph_asset(
            {
                "action": "create_graph",
                "name": "ExampleLitGraph",
                "path": "Assets/ShaderGraphs",
                "template": "urp_lit",
            }
        )
        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["request"]["action"], "create_graph")
        self.assertIn(response["data"]["status"], {"scaffold", "validated_scaffold"})
        self.assertEqual(response["data"]["validationState"], "validated")

    def test_request_normalization_rejects_unknown_action(self) -> None:
        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "not_real"})

    def test_server_registry_invokes_shadergraph_asset(self) -> None:
        server = build_server()
        response = server.invoke("shadergraph_asset", {"action": "save_graph"})
        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["request"]["action"], "save_graph")


if __name__ == "__main__":
    unittest.main()
