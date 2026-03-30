from __future__ import annotations

import io
import inspect
import json
import sys
import unittest
from contextlib import redirect_stdout
from pathlib import Path
from unittest.mock import patch

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp import server as server_module
from unity_shader_graph_mcp.contracts import ShaderGraphRequestError
from unity_shader_graph_mcp.server import build_server, main
from unity_shader_graph_mcp.transport import build_in_process_transport
from unity_shader_graph_mcp.tools import (
    SUPPORTED_SHADERGRAPH_ASSET_ACTIONS,
    handle_shadergraph_asset,
    normalize_shadergraph_asset_request,
)


ACTION_FIXTURES: dict[str, dict[str, object]] = {
    "create_graph": {
        "action": "create_graph",
        "name": "ExampleLitGraph",
        "path": "Assets/ShaderGraphs",
        "template": "urp_lit",
    },
    "rename_graph": {
        "action": "rename_graph",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "newDisplayName": "RenamedLitGraph",
    },
    "set_graph_metadata": {
        "action": "set_graph_metadata",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "graphPathLabel": "Material Inputs",
        "graphDefaultPrecision": "Half",
    },
    "create_category": {
        "action": "create_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "categoryName": "Surface Inputs",
    },
    "rename_category": {
        "action": "rename_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "categoryName": "Surface Inputs",
        "newDisplayName": "Material Inputs",
    },
    "find_category": {
        "action": "find_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "categoryName": "Surface Inputs",
    },
    "delete_category": {
        "action": "delete_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "categoryName": "Surface Inputs",
    },
    "reorder_category": {
        "action": "reorder_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "categoryName": "Surface Inputs",
        "newIndex": 1,
    },
    "merge_category": {
        "action": "merge_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "sourceCategoryName": "Surface Inputs",
        "targetCategoryName": "Material Inputs",
    },
    "duplicate_category": {
        "action": "duplicate_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "sourceCategoryName": "Surface Inputs",
        "newDisplayName": "Surface Inputs Copy",
    },
    "split_category": {
        "action": "split_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "sourceCategoryName": "Surface Inputs",
        "newDisplayName": "Surface Inputs Primary",
        "propertyNames": ["Tint", "Exposure"],
    },
    "list_categories": {
        "action": "list_categories",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
    },
    "read_graph_summary": {
        "action": "read_graph_summary",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
    },
    "find_node": {
        "action": "find_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "displayName": "Base Color",
    },
    "find_property": {
        "action": "find_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
    },
    "list_supported_nodes": {
        "action": "list_supported_nodes",
    },
    "list_supported_properties": {
        "action": "list_supported_properties",
    },
    "list_supported_connections": {
        "action": "list_supported_connections",
    },
    "update_property": {
        "action": "update_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "defaultValue": "#FFFFFFFF",
    },
    "rename_property": {
        "action": "rename_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "displayName": "Base Tint",
    },
    "duplicate_property": {
        "action": "duplicate_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "displayName": "Copied Tint",
    },
    "reorder_property": {
        "action": "reorder_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "newIndex": 0,
    },
    "move_property_to_category": {
        "action": "move_property_to_category",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "categoryName": "Surface Inputs",
        "targetIndex": 0,
    },
    "rename_node": {
        "action": "rename_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "objectId": "node-17",
        "displayName": "Renamed Source",
    },
    "duplicate_node": {
        "action": "duplicate_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "objectId": "node-17",
        "displayName": "Copied Source",
    },
    "move_node": {
        "action": "move_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "objectId": "node-17",
        "x": -420,
        "y": 180,
    },
    "delete_node": {
        "action": "delete_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "objectId": "node-17",
    },
    "remove_property": {
        "action": "remove_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
    },
    "add_property": {
        "action": "add_property",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "propertyName": "ExampleColor",
        "propertyType": "Color",
        "defaultValue": "[1,1,1,1]",
    },
    "add_node": {
        "action": "add_node",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "nodeType": "UnityEditor.ShaderGraph.ColorNode",
        "displayName": "Color",
    },
    "connect_ports": {
        "action": "connect_ports",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "outputNodeId": "node-1",
        "outputPort": "Out",
        "inputNodeId": "node-2",
        "inputPort": "In",
    },
    "find_connection": {
        "action": "find_connection",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "sourceNodeId": "node-1",
        "sourcePort": "Out",
        "targetNodeId": "node-2",
        "targetPort": "In",
    },
    "remove_connection": {
        "action": "remove_connection",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "sourceNodeId": "node-1",
        "sourcePort": "Out",
        "targetNodeId": "node-2",
        "targetPort": "In",
    },
    "reconnect_connection": {
        "action": "reconnect_connection",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
        "oldSourceNodeId": "node-1",
        "oldSourcePort": "Out",
        "oldTargetNodeId": "node-2",
        "oldTargetPort": "In",
        "sourceNodeId": "node-1",
        "sourcePort": "Out",
        "targetNodeId": "node-3",
        "targetPort": "In",
    },
    "save_graph": {
        "action": "save_graph",
        "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
    },
}


class ShaderGraphServerContractTests(unittest.TestCase):
    def test_every_supported_action_has_a_contract_fixture(self) -> None:
        self.assertEqual(set(ACTION_FIXTURES), set(SUPPORTED_SHADERGRAPH_ASSET_ACTIONS))

    def test_each_action_normalizes_and_dispatches(self) -> None:
        for action, payload in ACTION_FIXTURES.items():
            with self.subTest(action=action):
                request = normalize_shadergraph_asset_request(payload)
                with patch(
                    "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
                    return_value=None,
                ):
                    response = handle_shadergraph_asset(payload)

                self.assertEqual(request.action, action)
                self.assertTrue(response["success"])
                self.assertEqual(response["data"]["request"]["action"], action)
                self.assertIn(response["data"]["status"], {"scaffold", "validated_scaffold"})
                self.assertIn(response["data"]["validationState"], {"validated"})

    def test_normalization_trims_text_fields_and_keeps_payload_copy(self) -> None:
        payload = {
            "action": "  create_graph  ",
            "name": "  ExampleLitGraph  ",
            "path": "  Assets/ShaderGraphs  ",
            "template": "  urp_lit  ",
        }

        request = normalize_shadergraph_asset_request(payload)

        self.assertEqual(request.action, "create_graph")
        self.assertEqual(request.name, "ExampleLitGraph")
        self.assertEqual(request.path, "Assets/ShaderGraphs")
        self.assertEqual(request.template, "urp_lit")
        self.assertEqual(request.payload, payload)
        self.assertIsNot(request.payload, payload)

    def test_unknown_action_reports_supported_actions_in_error_message(self) -> None:
        with self.assertRaises(ShaderGraphRequestError) as ctx:
            normalize_shadergraph_asset_request({"action": "not_real"})

        message = str(ctx.exception)
        self.assertIn("Unsupported action 'not_real'", message)
        self.assertIn("Supported actions:", message)
        for action in SUPPORTED_SHADERGRAPH_ASSET_ACTIONS:
            self.assertIn(action, message)

    def test_missing_required_fields_raise_contract_error(self) -> None:
        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "create_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "rename_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "rename_graph", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "set_graph_metadata", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "find_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "merge_category", "path": "Assets/X.shadergraph", "sourceCategoryName": "A"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "duplicate_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "split_category", "path": "Assets/X.shadergraph", "sourceCategoryName": "A"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "delete_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "reorder_category", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "add_property", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "find_node", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "find_property", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request({"action": "duplicate_property", "path": "Assets/X.shadergraph"})

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "reorder_property", "path": "Assets/X.shadergraph", "propertyName": "Tint"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "move_property_to_category", "path": "Assets/X.shadergraph", "propertyName": "Tint"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "move_node", "path": "Assets/X.shadergraph", "objectId": "node-1"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "rename_node", "path": "Assets/X.shadergraph", "objectId": "node-1"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "rename_property", "path": "Assets/X.shadergraph", "propertyName": "Tint"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "duplicate_node", "path": "Assets/X.shadergraph"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "delete_node", "path": "Assets/X.shadergraph"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "remove_property", "path": "Assets/X.shadergraph"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "remove_connection", "path": "Assets/X.shadergraph", "sourceNodeId": "node-1"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "find_connection", "path": "Assets/X.shadergraph", "sourceNodeId": "node-1"}
            )

        with self.assertRaises(ShaderGraphRequestError):
            normalize_shadergraph_asset_request(
                {"action": "reconnect_connection", "path": "Assets/X.shadergraph", "oldSourceNodeId": "node-1"}
            )

        with patch(
            "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
            return_value=None,
        ):
            response = handle_shadergraph_asset({"action": "add_node", "path": "Assets/X.shadergraph"})
        self.assertFalse(response["success"])
        self.assertIn("Missing required field", response["message"])
        self.assertEqual(response["data"]["supportedActions"], list(SUPPORTED_SHADERGRAPH_ASSET_ACTIONS))

    def test_server_registry_exposes_shadergraph_asset_tool(self) -> None:
        server = build_server()

        self.assertIn("shadergraph_asset", server.tools)
        self.assertEqual(
            server.tools["shadergraph_asset"].description,
            "Focused Shader Graph asset operations for milestone 1.",
        )

    def test_in_process_transport_lists_tools(self) -> None:
        transport = build_in_process_transport()

        response = transport.list_tools()

        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["server"], "unity-shader-graph-mcp")
        self.assertEqual(response["data"]["toolCount"], 1)
        self.assertEqual(response["data"]["tools"][0]["name"], "shadergraph_asset")

    def test_in_process_transport_invokes_request(self) -> None:
        transport = build_in_process_transport()

        with patch(
            "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
            return_value=None,
        ):
            response = transport.invoke(
                {
                    "tool": "shadergraph_asset",
                    "action": "read_graph_summary",
                    "path": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                }
            )

        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["request"]["action"], "read_graph_summary")
        self.assertIn(response["data"]["status"], {"scaffold", "validated_scaffold"})

    def test_unknown_tool_raises_stable_key_error_shape(self) -> None:
        server = build_server()

        with self.assertRaises(KeyError) as ctx:
            server.invoke("missing_tool", {})

        self.assertEqual(str(ctx.exception), "\"Unknown tool 'missing_tool'.\"")

    def test_main_builds_server_and_prints_status(self) -> None:
        buffer = io.StringIO()
        request = {
            "tool": "shadergraph_asset",
            "action": "create_graph",
            "name": "ExampleLitGraph",
            "path": "Assets/ShaderGraphs",
            "template": "urp_lit",
        }

        with patch.object(server_module, "build_server", wraps=server_module.build_server) as build_server_mock:
            with patch(
                "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
                return_value=None,
            ):
                with redirect_stdout(buffer):
                    exit_code = self._call_main_with_explicit_argv(["--request", json.dumps(request)])

        self.assertEqual(exit_code, 0)
        self.assertEqual(build_server_mock.call_count, 1)
        response = json.loads(buffer.getvalue())
        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["request"]["action"], "create_graph")
        self.assertIn(response["data"]["status"], {"scaffold", "validated_scaffold"})
        self.assertEqual(response["data"]["validationState"], "validated")

    def _call_main_with_explicit_argv(self, argv: list[str]) -> int:
        signature = inspect.signature(main)
        if signature.parameters:
            return main(argv)
        return main()


if __name__ == "__main__":
    unittest.main()
