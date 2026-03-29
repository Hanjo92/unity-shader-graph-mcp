from __future__ import annotations

import sys
import unittest
from pathlib import Path
from unittest.mock import patch

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
                "create_category",
                "rename_category",
                "find_category",
                "delete_category",
                "read_graph_summary",
                "find_node",
                "find_property",
                "list_supported_nodes",
                "list_supported_properties",
                "list_supported_connections",
                "update_property",
                "rename_property",
                "duplicate_property",
                "reorder_property",
                "move_property_to_category",
                "rename_node",
                "duplicate_node",
                "move_node",
                "delete_node",
                "remove_property",
                "add_property",
                "add_node",
                "connect_ports",
                "find_connection",
                "remove_connection",
                "reconnect_connection",
                "save_graph",
            ),
        )

    def test_handler_returns_scaffold_response(self) -> None:
        with patch(
            "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
            return_value=None,
        ):
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

    def test_request_normalization_accepts_find_node_with_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "find_node",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "displayName": "Base Color",
            }
        )

        self.assertEqual(request.action, "find_node")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["displayName"], "Base Color")

    def test_request_normalization_accepts_create_category_with_category_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "create_category",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "categoryName": "Surface Inputs",
            }
        )

        self.assertEqual(request.action, "create_category")
        self.assertEqual(request.name, "Surface Inputs")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["categoryName"], "Surface Inputs")

    def test_request_normalization_accepts_rename_category_with_alias_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "rename_category",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "categoryName": "Surface Inputs",
                "newDisplayName": "Material Inputs",
            }
        )

        self.assertEqual(request.action, "rename_category")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["categoryName"], "Surface Inputs")
        self.assertEqual(request.payload["newDisplayName"], "Material Inputs")

    def test_request_normalization_accepts_find_category_with_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "find_category",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "displayName": "(Default Category)",
            }
        )

        self.assertEqual(request.action, "find_category")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["displayName"], "(Default Category)")

    def test_request_normalization_accepts_delete_category_with_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "delete_category",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "displayName": "Surface Inputs",
            }
        )

        self.assertEqual(request.action, "delete_category")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["displayName"], "Surface Inputs")

    def test_request_normalization_accepts_find_property_with_reference_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "find_property",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "referenceName": "_BaseColor",
            }
        )

        self.assertEqual(request.action, "find_property")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["referenceName"], "_BaseColor")

    def test_request_normalization_accepts_move_node_with_numeric_coordinates(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "move_node",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "objectId": "node-17",
                "x": -420,
                "y": 180.5,
            }
        )

        self.assertEqual(request.action, "move_node")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["objectId"], "node-17")
        self.assertEqual(request.payload["x"], "-420")
        self.assertEqual(request.payload["y"], "180.5")

    def test_request_normalization_accepts_rename_node_with_alias_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "rename_node",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "objectId": "node-17",
                "newDisplayName": "Renamed Source",
            }
        )

        self.assertEqual(request.action, "rename_node")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["objectId"], "node-17")
        self.assertEqual(request.payload["newDisplayName"], "Renamed Source")

    def test_request_normalization_accepts_rename_property_with_optional_reference_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "rename_property",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "Tint",
                "newDisplayName": "Base Tint",
                "newReferenceName": "_BaseTint",
            }
        )

        self.assertEqual(request.action, "rename_property")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["propertyName"], "Tint")
        self.assertEqual(request.payload["newDisplayName"], "Base Tint")
        self.assertEqual(request.payload["newReferenceName"], "_BaseTint")

    def test_request_normalization_accepts_duplicate_property_with_optional_reference_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "duplicate_property",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "Tint",
                "newDisplayName": "Copied Tint",
                "newReferenceName": "_CopiedTint",
            }
        )

        self.assertEqual(request.action, "duplicate_property")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["propertyName"], "Tint")
        self.assertEqual(request.payload["newDisplayName"], "Copied Tint")
        self.assertEqual(request.payload["newReferenceName"], "_CopiedTint")

    def test_request_normalization_accepts_reorder_property_with_index_alias(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "reorder_property",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "Tint",
                "newIndex": 0,
            }
        )

        self.assertEqual(request.action, "reorder_property")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["propertyName"], "Tint")
        self.assertEqual(request.payload["index"], "0")

    def test_request_normalization_accepts_move_property_to_category_with_optional_index(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "move_property_to_category",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "Tint",
                "categoryName": "Surface Inputs",
                "targetIndex": 1,
            }
        )

        self.assertEqual(request.action, "move_property_to_category")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["propertyName"], "Tint")
        self.assertEqual(request.payload["categoryName"], "Surface Inputs")
        self.assertEqual(request.payload["index"], "1")

    def test_request_normalization_accepts_duplicate_node_with_optional_alias_display_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "duplicate_node",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "objectId": "node-17",
                "newDisplayName": "Copied Source",
            }
        )

        self.assertEqual(request.action, "duplicate_node")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["objectId"], "node-17")
        self.assertEqual(request.payload["newDisplayName"], "Copied Source")

    def test_request_normalization_accepts_delete_node_with_object_id(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "delete_node",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "objectId": "node-17",
            }
        )

        self.assertEqual(request.action, "delete_node")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["objectId"], "node-17")

    def test_request_normalization_accepts_remove_property_with_property_name(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "remove_property",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "propertyName": "Tint",
            }
        )

        self.assertEqual(request.action, "remove_property")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["propertyName"], "Tint")

    def test_request_normalization_accepts_remove_connection_with_alias_fields(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "remove_connection",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "sourceNodeId": "node-1",
                "sourcePort": "Out",
                "targetNodeId": "node-2",
                "targetPort": "In",
            }
        )

        self.assertEqual(request.action, "remove_connection")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["sourceNodeId"], "node-1")

    def test_request_normalization_accepts_find_connection_with_alias_fields(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "find_connection",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "sourceNodeId": "node-1",
                "sourcePort": "Out",
                "targetNodeId": "node-2",
                "targetPort": "In",
            }
        )

        self.assertEqual(request.action, "find_connection")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["sourceNodeId"], "node-1")

    def test_request_normalization_accepts_reconnect_connection_with_old_and_new_alias_fields(self) -> None:
        request = normalize_shadergraph_asset_request(
            {
                "action": "reconnect_connection",
                "assetPath": "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "oldSourceNodeId": "node-1",
                "oldSourcePort": "Out",
                "oldTargetNodeId": "node-2",
                "oldTargetPort": "X",
                "sourceNodeId": "node-1",
                "sourcePort": "Out",
                "targetNodeId": "node-3",
                "targetPort": "X",
            }
        )

        self.assertEqual(request.action, "reconnect_connection")
        self.assertEqual(request.path, "Assets/ShaderGraphs/ExampleLitGraph.shadergraph")
        self.assertEqual(request.payload["oldSourceNodeId"], "node-1")
        self.assertEqual(request.payload["targetNodeId"], "node-3")

    def test_server_registry_invokes_shadergraph_asset(self) -> None:
        server = build_server()
        with patch(
            "unity_shader_graph_mcp.tools.shadergraph_asset.build_unity_batchmode_bridge",
            return_value=None,
        ):
            response = server.invoke("shadergraph_asset", {"action": "save_graph"})
        self.assertTrue(response["success"])
        self.assertEqual(response["data"]["request"]["action"], "save_graph")


if __name__ == "__main__":
    unittest.main()
