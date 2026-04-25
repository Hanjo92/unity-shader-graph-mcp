"""Shader Graph asset tool with an optional Unity batchmode bridge.

The server keeps the JSON contract stable whether requests are handled by the
real Unity batchmode bridge or by the scaffold fallback path.
"""

from __future__ import annotations

from dataclasses import asdict
import json
from pathlib import PurePosixPath
from typing import Any, Mapping

from ..contracts import (
    ShaderGraphAssetRequest,
    ShaderGraphRequestError,
    as_response,
    coerce_mapping,
    optional_text,
    require_text,
)
from ..unity_bridge import (
    UNITY_EXE_ENV,
    UNITY_PROJECT_ENV,
    UnityBridgeError,
    build_unity_batchmode_bridge,
)

SUPPORTED_SHADERGRAPH_ASSET_ACTIONS: tuple[str, ...] = (
    "create_graph",
    "create_subgraph",
    "rename_graph",
    "rename_subgraph",
    "duplicate_graph",
    "duplicate_subgraph",
    "delete_graph",
    "delete_subgraph",
    "move_graph",
    "move_subgraph",
    "set_graph_metadata",
    "create_category",
    "rename_category",
    "find_category",
    "delete_category",
    "reorder_category",
    "merge_category",
    "duplicate_category",
    "split_category",
    "list_categories",
    "read_graph_summary",
    "read_subgraph_summary",
    "export_graph_contract",
    "import_graph_contract",
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
)

ACTION_DEFAULT_PATHS: dict[str, str] = {
    "create_graph": "Assets/ShaderGraphs",
    "create_subgraph": "Assets/ShaderSubGraphs",
}

GRAPH_PATH_ACTIONS: frozenset[str] = frozenset(
    {
        "create_graph",
        "rename_graph",
        "duplicate_graph",
        "delete_graph",
        "move_graph",
        "set_graph_metadata",
        "create_category",
        "rename_category",
        "find_category",
        "delete_category",
        "reorder_category",
        "merge_category",
        "duplicate_category",
        "split_category",
        "list_categories",
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
    }
)

SUBGRAPH_PATH_ACTIONS: frozenset[str] = frozenset(
    {
        "create_subgraph",
        "rename_subgraph",
        "duplicate_subgraph",
        "delete_subgraph",
        "move_subgraph",
        "read_subgraph_summary",
    }
)

_PATH_ACTION_MISMATCH_HINTS: dict[str, str] = {
    "create_graph": "create_subgraph",
    "create_subgraph": "create_graph",
    "rename_graph": "rename_subgraph",
    "rename_subgraph": "rename_graph",
    "duplicate_graph": "duplicate_subgraph",
    "duplicate_subgraph": "duplicate_graph",
    "delete_graph": "delete_subgraph",
    "delete_subgraph": "delete_graph",
    "move_graph": "move_subgraph",
    "move_subgraph": "move_graph",
    "read_graph_summary": "read_subgraph_summary",
    "read_subgraph_summary": "read_graph_summary",
}


def normalize_shadergraph_asset_request(
    payload: Mapping[str, Any],
) -> ShaderGraphAssetRequest:
    """Coerce an inbound payload into the milestone 1 request model."""

    normalized_payload = coerce_mapping(payload)
    request_payload = _unwrap_request_payload(normalized_payload)

    action = require_text(request_payload.get("action"), "action")
    if action not in SUPPORTED_SHADERGRAPH_ASSET_ACTIONS:
        supported = ", ".join(SUPPORTED_SHADERGRAPH_ASSET_ACTIONS)
        raise ShaderGraphRequestError(
            f"Unsupported action '{action}'. Supported actions: {supported}."
        )

    request_name = optional_text(_pick_value(request_payload, "name", "graphName"))
    if action == "rename_graph":
        request_name = optional_text(
            _pick_value(request_payload, "newDisplayName", "new_display_name", "displayName", "display_name", "name", "graphName")
        )
    if action == "rename_subgraph":
        request_name = optional_text(
            _pick_value(request_payload, "newDisplayName", "new_display_name", "displayName", "display_name", "name", "graphName")
        )
    if action == "duplicate_graph":
        request_name = optional_text(
            _pick_value(request_payload, "newDisplayName", "new_display_name", "displayName", "display_name", "name", "graphName")
        )
    if action == "duplicate_subgraph":
        request_name = optional_text(
            _pick_value(request_payload, "newDisplayName", "new_display_name", "displayName", "display_name", "name", "graphName")
        )
    if action == "create_category":
        request_name = optional_text(
            _pick_value(request_payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
    if action == "find_category":
        request_name = optional_text(
            _pick_value(request_payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
    if action == "delete_category":
        request_name = optional_text(
            _pick_value(request_payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
    if action == "reorder_category":
        request_name = optional_text(
            _pick_value(request_payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
    if action == "merge_category":
        request_name = optional_text(
            _pick_value(
                request_payload,
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )
    if action == "duplicate_category":
        request_name = optional_text(
            _pick_value(
                request_payload,
                "categoryName",
                "category_name",
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )
    if action == "split_category":
        request_name = optional_text(
            _pick_value(
                request_payload,
                "categoryName",
                "category_name",
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )

    request = ShaderGraphAssetRequest(
        action=action,  # type: ignore[arg-type]
        name=request_name,
        path=optional_text(_pick_value(request_payload, "path", "assetPath", "asset_path")),
        template=optional_text(_pick_value(request_payload, "template")),
        payload=dict(request_payload),
    )

    _validate_shadergraph_asset_request(request)
    return request


def _unwrap_request_payload(payload: Mapping[str, Any]) -> dict[str, Any]:
    """Flatten common request envelope wrappers into a single mapping."""

    for key in ("request", "params", "payload"):
        nested = payload.get(key)
        if isinstance(nested, Mapping):
            merged = dict(payload)
            merged.pop(key, None)
            merged.update(nested)
            return merged
    return dict(payload)


def _pick_value(payload: Mapping[str, Any], *keys: str) -> Any:
    """Return the first non-empty value found under the provided keys."""

    for key in keys:
        if key in payload and payload[key] is not None:
            value = payload[key]
            if isinstance(value, str):
                if value.strip():
                    return value
            else:
                return value
    return None


def _pick_text_sequence(payload: Mapping[str, Any], *keys: str) -> list[str]:
    values: list[str] = []
    seen: set[str] = set()
    for key in keys:
        if key not in payload or payload[key] is None:
            continue

        candidate = payload[key]
        if isinstance(candidate, str):
            text = optional_text(candidate)
            if text is not None and text.lower() not in seen:
                seen.add(text.lower())
                values.append(text)
            continue

        if isinstance(candidate, (list, tuple)):
            for item in candidate:
                text = optional_text(item)
                if text is not None and text.lower() not in seen:
                    seen.add(text.lower())
                    values.append(text)

    return values


def _validate_shadergraph_asset_request(request: ShaderGraphAssetRequest) -> None:
    """Enforce per-action requirements with clear error messages."""

    if request.action in {"create_graph", "create_subgraph"} and request.name is None:
        raise ShaderGraphRequestError("Missing required field 'name'.")

    if request.action == "rename_graph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        display_name = optional_text(
            _pick_value(
                request.payload,
                "newDisplayName",
                "new_display_name",
                "displayName",
                "display_name",
                "name",
                "graphName",
            )
        )
        if display_name is None:
            raise ShaderGraphRequestError("Missing required field 'newDisplayName', 'displayName', 'name', or 'graphName'.")
        request.payload.setdefault("displayName", display_name)

    if request.action == "rename_subgraph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        display_name = optional_text(
            _pick_value(
                request.payload,
                "newDisplayName",
                "new_display_name",
                "displayName",
                "display_name",
                "name",
                "graphName",
            )
        )
        if display_name is None:
            raise ShaderGraphRequestError("Missing required field 'newDisplayName', 'displayName', 'name', or 'graphName'.")
        request.payload.setdefault("displayName", display_name)

    if request.action == "duplicate_graph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        display_name = optional_text(
            _pick_value(
                request.payload,
                "newDisplayName",
                "new_display_name",
                "displayName",
                "display_name",
                "name",
                "graphName",
            )
        )
        if display_name is None:
            raise ShaderGraphRequestError("Missing required field 'newDisplayName', 'displayName', 'name', or 'graphName'.")
        request.payload.setdefault("displayName", display_name)

    if request.action == "duplicate_subgraph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        display_name = optional_text(
            _pick_value(
                request.payload,
                "newDisplayName",
                "new_display_name",
                "displayName",
                "display_name",
                "name",
                "graphName",
            )
        )
        if display_name is None:
            raise ShaderGraphRequestError("Missing required field 'newDisplayName', 'displayName', 'name', or 'graphName'.")
        request.payload.setdefault("displayName", display_name)

    if request.action == "delete_graph" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "delete_subgraph" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "move_graph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

        target_asset_path = _normalize_move_graph_target_asset_path(
            request.path,
            _pick_value(
                request.payload,
                "targetAssetPath",
                "target_asset_path",
                "newAssetPath",
                "new_asset_path",
                "targetPath",
                "target_path",
                "newPath",
                "new_path",
                "destinationPath",
                "destination_path",
            ),
        )
        if target_asset_path is None:
            raise ShaderGraphRequestError(
                "Missing required field 'targetAssetPath', 'newAssetPath', 'targetPath', 'newPath', or 'destinationPath'."
            )
        request.payload.setdefault("targetAssetPath", target_asset_path)

    if request.action == "move_subgraph":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

        target_asset_path = _normalize_move_subgraph_target_asset_path(
            request.path,
            _pick_value(
                request.payload,
                "targetAssetPath",
                "target_asset_path",
                "newAssetPath",
                "new_asset_path",
                "targetPath",
                "target_path",
                "newPath",
                "new_path",
                "destinationPath",
                "destination_path",
            ),
        )
        if target_asset_path is None:
            raise ShaderGraphRequestError(
                "Missing required field 'targetAssetPath', 'newAssetPath', 'targetPath', 'newPath', or 'destinationPath'."
            )
        request.payload.setdefault("targetAssetPath", target_asset_path)

    if request.action == "set_graph_metadata":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        graph_path_label = optional_text(
            _pick_value(request.payload, "graphPathLabel", "graph_path_label", "pathLabel", "path_label")
        )
        graph_default_precision = optional_text(
            _pick_value(
                request.payload,
                "graphDefaultPrecision",
                "graph_default_precision",
                "defaultPrecision",
                "default_precision",
                "precision",
            )
        )
        if graph_path_label is None and graph_default_precision is None:
            raise ShaderGraphRequestError(
                "set_graph_metadata requires graphPathLabel/pathLabel and/or graphDefaultPrecision/defaultPrecision/precision."
            )
        if graph_path_label is not None:
            request.payload["graphPathLabel"] = graph_path_label
        if graph_default_precision is not None:
            request.payload["graphDefaultPrecision"] = graph_default_precision

    if request.action == "create_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        category_name = optional_text(
            _pick_value(request.payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
        if category_name is None:
            raise ShaderGraphRequestError("Missing required field 'categoryName', 'displayName', or 'name'.")
        request.payload.setdefault("categoryName", category_name)

    if request.action == "rename_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        category_guid = optional_text(_pick_value(request.payload, "categoryGuid", "category_guid"))
        category_name = optional_text(_pick_value(request.payload, "categoryName", "category_name"))
        if category_guid is None and category_name is None:
            raise ShaderGraphRequestError("Missing required field 'categoryGuid' or 'categoryName'.")
        display_name = optional_text(
            _pick_value(request.payload, "newDisplayName", "new_display_name", "displayName", "display_name", "name")
        )
        if display_name is None:
            raise ShaderGraphRequestError("Missing required field 'newDisplayName', 'displayName', or 'name'.")
        request.payload.setdefault("displayName", display_name)

    if request.action == "find_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "categoryGuid",
                "category_guid",
                "categoryName",
                "category_name",
                "displayName",
                "display_name",
                "name",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "find_category requires at least one of: categoryGuid, categoryName, displayName, name."
            )

    if request.action == "delete_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "categoryGuid",
                "category_guid",
                "categoryName",
                "category_name",
                "displayName",
                "display_name",
                "name",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "delete_category requires at least one of: categoryGuid, categoryName, displayName, name."
            )

    if request.action == "reorder_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "categoryGuid",
                "category_guid",
                "categoryName",
                "category_name",
                "displayName",
                "display_name",
                "name",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "reorder_category requires at least one of: categoryGuid, categoryName, displayName, name."
            )
        request.payload["index"] = _require_payload_int_text(
            request.payload,
            "newIndex",
            "new_index",
            "targetIndex",
            "target_index",
            "index",
        )

    if request.action == "merge_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_source_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "sourceCategoryGuid",
                "source_category_guid",
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )
        if not has_source_lookup:
            raise ShaderGraphRequestError(
                "merge_category requires at least one source category lookup: sourceCategoryGuid, sourceCategoryName, sourceDisplayName, sourceName."
            )
        has_target_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "targetCategoryGuid",
                "target_category_guid",
                "targetCategoryName",
                "target_category_name",
                "targetDisplayName",
                "target_display_name",
                "targetName",
                "target_name",
            )
        )
        if not has_target_lookup:
            raise ShaderGraphRequestError(
                "merge_category requires at least one target category lookup: targetCategoryGuid, targetCategoryName, targetDisplayName, targetName."
            )

    if request.action == "duplicate_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "categoryGuid",
                "category_guid",
                "categoryName",
                "category_name",
                "sourceCategoryGuid",
                "source_category_guid",
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "duplicate_category requires at least one source category lookup: categoryGuid, categoryName, sourceCategoryGuid, sourceCategoryName, sourceDisplayName, sourceName."
            )

    if request.action == "split_category":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "categoryGuid",
                "category_guid",
                "categoryName",
                "category_name",
                "sourceCategoryGuid",
                "source_category_guid",
                "sourceCategoryName",
                "source_category_name",
                "sourceDisplayName",
                "source_display_name",
                "sourceName",
                "source_name",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "split_category requires at least one source category lookup: categoryGuid, categoryName, sourceCategoryGuid, sourceCategoryName, sourceDisplayName, sourceName."
            )

        property_names = _pick_text_sequence(
            request.payload,
            "propertyName",
            "property_name",
            "propertyNames",
            "property_names",
            "properties",
        )
        if not property_names:
            raise ShaderGraphRequestError(
                "split_category requires propertyName or propertyNames/properties."
            )

    if request.action == "list_categories" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "read_graph_summary" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "read_subgraph_summary" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    _validate_shadergraph_asset_path_kind(request)

    if request.action == "export_graph_contract" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "import_graph_contract":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        graph_contract_json = _normalize_graph_contract_json(request.payload)
        if graph_contract_json is None:
            raise ShaderGraphRequestError(
                "import_graph_contract requires graphContract/exportedGraphContract/contract or graphContractJson/contractJson."
            )
        request.payload["graphContractJson"] = graph_contract_json

    if request.action == "find_node":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in ("nodeId", "node_id", "objectId", "displayName", "display_name", "nodeType", "node_type")
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "find_node requires at least one of: nodeId/objectId, displayName, nodeType."
            )

    if request.action == "find_property":
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        has_lookup = any(
            optional_text(_pick_value(request.payload, key)) is not None
            for key in (
                "propertyName",
                "property_name",
                "displayName",
                "display_name",
                "referenceName",
                "reference_name",
                "propertyType",
                "property_type",
            )
        )
        if not has_lookup:
            raise ShaderGraphRequestError(
                "find_property requires at least one of: propertyName, displayName, referenceName, propertyType."
            )

    if request.action == "add_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        _require_payload_text(request.payload, "propertyType", "property_type")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "update_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "rename_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        _require_payload_text(request.payload, "displayName", "display_name", "newDisplayName", "new_display_name", "name")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "duplicate_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "reorder_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        request.payload["index"] = _require_payload_int_text(
            request.payload,
            "newIndex",
            "new_index",
            "targetIndex",
            "target_index",
            "index",
        )
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "move_property_to_category":
        _require_payload_text(request.payload, "propertyName", "property_name")
        category_guid = optional_text(_pick_value(request.payload, "categoryGuid", "category_guid"))
        category_name = optional_text(
            _pick_value(request.payload, "categoryName", "category_name", "displayName", "display_name", "name")
        )
        if category_guid is None and category_name is None:
            raise ShaderGraphRequestError(
                "move_property_to_category requires categoryGuid or categoryName/displayName/name."
            )
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")
        index_value = _pick_value(
            request.payload,
            "newIndex",
            "new_index",
            "targetIndex",
            "target_index",
            "index",
        )
        if optional_text(index_value) is not None:
            request.payload["index"] = _require_payload_int_text(
                request.payload,
                "newIndex",
                "new_index",
                "targetIndex",
                "target_index",
                "index",
            )

    if request.action == "rename_node":
        _require_payload_text(request.payload, "nodeId", "node_id", "objectId", "object_id")
        _require_payload_text(request.payload, "displayName", "display_name", "newDisplayName", "new_display_name", "name")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "duplicate_node":
        _require_payload_text(request.payload, "nodeId", "node_id", "objectId", "object_id")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "move_node":
        _require_payload_text(request.payload, "nodeId", "node_id", "objectId", "object_id")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

        raw_x = _pick_value(request.payload, "x")
        raw_y = _pick_value(request.payload, "y")
        has_x = optional_text(raw_x) is not None
        has_y = optional_text(raw_y) is not None
        if has_x != has_y:
            raise ShaderGraphRequestError("move_node requires both x and y when using exact coordinates.")

        if has_x:
            request.payload["x"] = _require_payload_number_text(request.payload, "x")
            request.payload["y"] = _require_payload_number_text(request.payload, "y")

        anchor_node_id = optional_text(
            _pick_value(
                request.payload,
                "anchorNodeId",
                "anchor_node_id",
                "anchorObjectId",
                "anchor_object_id",
                "relativeToNodeId",
                "relative_to_node_id",
                "relativeToObjectId",
                "relative_to_object_id",
            )
        )
        anchor_display_name = optional_text(
            _pick_value(
                request.payload,
                "anchorDisplayName",
                "anchor_display_name",
                "relativeToDisplayName",
                "relative_to_display_name",
            )
        )
        anchor_node_type = optional_text(
            _pick_value(
                request.payload,
                "anchorNodeType",
                "anchor_node_type",
                "relativeToNodeType",
                "relative_to_node_type",
            )
        )
        direction = optional_text(_pick_value(request.payload, "direction", "relativeDirection", "relative_direction"))
        layout_preset = optional_text(_pick_value(request.payload, "layoutPreset", "layout_preset", "preset"))
        spacing_value = _pick_value(request.payload, "spacing")
        if optional_text(spacing_value) is not None:
            request.payload["spacing"] = _require_payload_number_text(request.payload, "spacing")

        has_anchor_query = any(value is not None for value in (anchor_node_id, anchor_display_name, anchor_node_type))
        has_relative_hints = has_anchor_query or direction is not None or layout_preset is not None or optional_text(spacing_value) is not None
        if not has_x and not has_relative_hints:
            raise ShaderGraphRequestError(
                "move_node requires either x/y or anchorNodeId/anchorDisplayName/anchorNodeType with direction/layoutPreset."
            )

        if not has_x and has_relative_hints and not has_anchor_query:
            raise ShaderGraphRequestError(
                "move_node relative placement requires anchorNodeId/anchorDisplayName/anchorNodeType."
            )

        if not has_x and has_relative_hints and direction is None and layout_preset is None:
            raise ShaderGraphRequestError(
                "move_node relative placement requires direction/relativeDirection or layoutPreset/preset."
            )

    if request.action == "delete_node":
        _require_payload_text(request.payload, "nodeId", "node_id", "objectId", "object_id")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "remove_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "add_node":
        _require_payload_text(request.payload, "nodeType", "node_type")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

        property_name = optional_text(_pick_value(request.payload, "propertyName", "property_name"))
        property_display_name = optional_text(
            _pick_value(request.payload, "propertyDisplayName", "property_display_name")
        )
        reference_name = optional_text(
            _pick_value(
                request.payload,
                "referenceName",
                "reference_name",
                "propertyReferenceName",
                "property_reference_name",
            )
        )
        property_type = optional_text(_pick_value(request.payload, "propertyType", "property_type"))

        if property_name is not None:
            request.payload["propertyName"] = property_name
        if property_display_name is not None:
            request.payload["propertyDisplayName"] = property_display_name
        if reference_name is not None:
            request.payload["referenceName"] = reference_name
        if property_type is not None:
            request.payload["propertyType"] = property_type

        raw_x = _pick_value(request.payload, "x")
        raw_y = _pick_value(request.payload, "y")
        has_x = optional_text(raw_x) is not None
        has_y = optional_text(raw_y) is not None
        if has_x != has_y:
            raise ShaderGraphRequestError("add_node requires both x and y when using exact coordinates.")

        if has_x:
            request.payload["x"] = _require_payload_number_text(request.payload, "x")
            request.payload["y"] = _require_payload_number_text(request.payload, "y")

        anchor_node_id = optional_text(
            _pick_value(
                request.payload,
                "anchorNodeId",
                "anchor_node_id",
                "anchorObjectId",
                "anchor_object_id",
                "relativeToNodeId",
                "relative_to_node_id",
                "relativeToObjectId",
                "relative_to_object_id",
            )
        )
        anchor_display_name = optional_text(
            _pick_value(
                request.payload,
                "anchorDisplayName",
                "anchor_display_name",
                "relativeToDisplayName",
                "relative_to_display_name",
            )
        )
        anchor_node_type = optional_text(
            _pick_value(
                request.payload,
                "anchorNodeType",
                "anchor_node_type",
                "relativeToNodeType",
                "relative_to_node_type",
            )
        )
        direction = optional_text(_pick_value(request.payload, "direction", "relativeDirection", "relative_direction"))
        layout_preset = optional_text(_pick_value(request.payload, "layoutPreset", "layout_preset", "preset"))
        spacing_value = _pick_value(request.payload, "spacing")
        if optional_text(spacing_value) is not None:
            request.payload["spacing"] = _require_payload_number_text(request.payload, "spacing")

        has_anchor_query = any(value is not None for value in (anchor_node_id, anchor_display_name, anchor_node_type))
        has_relative_hints = has_anchor_query or direction is not None or layout_preset is not None or optional_text(spacing_value) is not None
        if not has_x and has_relative_hints and not has_anchor_query:
            raise ShaderGraphRequestError(
                "add_node relative placement requires anchorNodeId/anchorDisplayName/anchorNodeType."
            )

        if not has_x and has_relative_hints and direction is None and layout_preset is None:
            raise ShaderGraphRequestError(
                "add_node relative placement requires direction/relativeDirection or layoutPreset/preset."
            )

        if _is_property_node_type(_pick_value(request.payload, "nodeType", "node_type")) and all(
            value is None for value in (property_name, property_display_name, reference_name, property_type)
        ):
            raise ShaderGraphRequestError(
                "add_node with nodeType=Property requires propertyName/propertyDisplayName/referenceName/propertyType."
            )

    if request.action == "connect_ports":
        _require_payload_text(request.payload, "outputNodeId", "output_node_id")
        _require_payload_text(request.payload, "outputPort", "output_port")
        _require_payload_text(request.payload, "inputNodeId", "input_node_id")
        _require_payload_text(request.payload, "inputPort", "input_port")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "find_connection":
        _require_payload_text(request.payload, "outputNodeId", "output_node_id", "sourceNodeId", "source_node_id")
        _require_payload_text(request.payload, "outputPort", "output_port", "sourcePort", "source_port")
        _require_payload_text(request.payload, "inputNodeId", "input_node_id", "targetNodeId", "target_node_id")
        _require_payload_text(request.payload, "inputPort", "input_port", "targetPort", "target_port")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "remove_connection":
        _require_payload_text(request.payload, "outputNodeId", "output_node_id", "sourceNodeId", "source_node_id")
        _require_payload_text(request.payload, "outputPort", "output_port", "sourcePort", "source_port")
        _require_payload_text(request.payload, "inputNodeId", "input_node_id", "targetNodeId", "target_node_id")
        _require_payload_text(request.payload, "inputPort", "input_port", "targetPort", "target_port")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "reconnect_connection":
        _require_payload_text(request.payload, "oldOutputNodeId", "old_output_node_id", "oldSourceNodeId", "old_source_node_id")
        _require_payload_text(request.payload, "oldOutputPort", "old_output_port", "oldSourcePort", "old_source_port")
        _require_payload_text(request.payload, "oldInputNodeId", "old_input_node_id", "oldTargetNodeId", "old_target_node_id")
        _require_payload_text(request.payload, "oldInputPort", "old_input_port", "oldTargetPort", "old_target_port")
        _require_payload_text(
            request.payload,
            "outputNodeId",
            "output_node_id",
            "sourceNodeId",
            "source_node_id",
            "newOutputNodeId",
            "new_output_node_id",
            "newSourceNodeId",
            "new_source_node_id",
        )
        _require_payload_text(
            request.payload,
            "outputPort",
            "output_port",
            "sourcePort",
            "source_port",
            "newOutputPort",
            "new_output_port",
            "newSourcePort",
            "new_source_port",
        )
        _require_payload_text(
            request.payload,
            "inputNodeId",
            "input_node_id",
            "targetNodeId",
            "target_node_id",
            "newInputNodeId",
            "new_input_node_id",
            "newTargetNodeId",
            "new_target_node_id",
        )
        _require_payload_text(
            request.payload,
            "inputPort",
            "input_port",
            "targetPort",
            "target_port",
            "newInputPort",
            "new_input_port",
            "newTargetPort",
            "new_target_port",
        )
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.template is not None and request.template not in {"urp_lit", "urp_unlit", "blank"}:
        raise ShaderGraphRequestError(
            "Invalid template. Expected one of: urp_lit, urp_unlit, blank."
        )

    if request.action in {"create_graph", "create_subgraph"} and request.path is None:
        default_path = ACTION_DEFAULT_PATHS[request.action]
        request.payload.setdefault("path", default_path)

    if request.action == "save_graph" and request.path is None:
        # Saving the active graph is still useful in the fallback path.
        request.payload.setdefault("path", "Assets/ShaderGraphs/ActiveGraph.shadergraph")


def handle_shadergraph_asset(
    payload: Mapping[str, Any],
    bridge: Any | None = None,
) -> dict[str, Any]:
    """Dispatch the focused Shader Graph asset tool.

    The server falls back to the scaffold response when the Unity bridge is not
    configured. When bridge configuration is present, the normalized request is
    handed to Unity batchmode and the response file is parsed back into the same
    response envelope.
    """
    try:
        request = normalize_shadergraph_asset_request(payload)
        summary = _request_summary(request)
        bridge_request = _build_bridge_request(request)
        bridge_instance = bridge if bridge is not None else build_unity_batchmode_bridge()
        if bridge_instance is None:
            return as_response(
                success=True,
                message=(
                    f"Shader Graph asset request validated for '{request.action}'. "
                    f"Set {UNITY_EXE_ENV} and {UNITY_PROJECT_ENV} to enable package-backed execution."
                ),
                data={
                    "request": summary,
                    "status": "scaffold",
                    "validationState": "validated",
                    "bridgeEnvironment": [UNITY_EXE_ENV, UNITY_PROJECT_ENV],
                    "next_step": "Connect this contract to the Unity Editor implementation.",
                },
            )

        response = bridge_instance.invoke(bridge_request)
        if not isinstance(response, Mapping):
            raise UnityBridgeError("Unity bridge returned a non-mapping response.")
        if "success" not in response or "message" not in response:
            raise UnityBridgeError("Unity bridge response did not match the server envelope.")
        return _bridge_response(response)
    except ShaderGraphRequestError as exc:
        return as_response(
            success=False,
            message=str(exc),
            data={
                "supportedActions": list(SUPPORTED_SHADERGRAPH_ASSET_ACTIONS),
                "errorCode": "validation_error",
            },
        )
    except UnityBridgeError as exc:
        return as_response(
            success=False,
            message=str(exc),
            data={
                "request": summary if "summary" in locals() else None,
                "status": "unity_bridge_error",
                "errorCode": "unity_bridge_error",
                "validationState": "validated",
            },
        )


def _bridge_response(response: Mapping[str, Any]) -> dict[str, Any]:
    data = response.get("data")
    if isinstance(data, Mapping):
        normalized_data = dict(data)
    elif data is None:
        normalized_data = {}
    else:
        normalized_data = {"value": data}

    return as_response(
        success=bool(response["success"]),
        message=str(response["message"]),
        data=normalized_data,
    )


def _request_summary(request: ShaderGraphAssetRequest) -> dict[str, Any]:
    """Build a JSON-friendly view of the normalized request."""

    summary = asdict(request)
    summary["path"] = _effective_path(request)
    summary["assetPath"] = _derive_asset_path(request)
    summary.pop("payload", None)
    for key_group in (
        ("categoryName", "category_name"),
        ("categoryGuid", "category_guid"),
        ("graphPathLabel", "graph_path_label", "pathLabel", "path_label"),
        ("graphDefaultPrecision", "graph_default_precision", "defaultPrecision", "default_precision", "precision"),
        ("propertyName", "property_name"),
        ("propertyDisplayName", "property_display_name"),
        ("propertyType", "property_type"),
        ("referenceName", "reference_name", "newReferenceName", "new_reference_name"),
        ("index", "newIndex", "new_index", "targetIndex", "target_index"),
        ("nodeType", "node_type"),
        ("displayName", "display_name", "newDisplayName", "new_display_name"),
        ("targetAssetPath", "target_asset_path", "newAssetPath", "new_asset_path", "targetPath", "target_path", "newPath", "new_path", "destinationPath", "destination_path"),
        ("nodeId", "node_id"),
        ("objectId", "object_id"),
        ("outputNodeId", "output_node_id", "sourceNodeId", "source_node_id"),
        ("outputPort", "output_port", "sourcePort", "source_port"),
        ("inputNodeId", "input_node_id", "targetNodeId", "target_node_id"),
        ("inputPort", "input_port", "targetPort", "target_port"),
        ("oldOutputNodeId", "old_output_node_id", "oldSourceNodeId", "old_source_node_id"),
        ("oldOutputPort", "old_output_port", "oldSourcePort", "old_source_port"),
        ("oldInputNodeId", "old_input_node_id", "oldTargetNodeId", "old_target_node_id"),
        ("oldInputPort", "old_input_port", "oldTargetPort", "old_target_port"),
        ("newOutputNodeId", "new_output_node_id", "newSourceNodeId", "new_source_node_id"),
        ("newOutputPort", "new_output_port", "newSourcePort", "new_source_port"),
        ("newInputNodeId", "new_input_node_id", "newTargetNodeId", "new_target_node_id"),
        ("newInputPort", "new_input_port", "newTargetPort", "new_target_port"),
        ("x",),
        ("y",),
    ):
        value = _pick_value(request.payload, *key_group)
        if value is not None:
            summary[key_group[0]] = value
    graph_contract_json = _normalize_graph_contract_json(request.payload)
    if graph_contract_json is not None:
        summary["graphContractJsonLength"] = len(graph_contract_json)
    return summary


def _build_bridge_request(request: ShaderGraphAssetRequest) -> dict[str, Any]:
    payload = dict(request.payload)
    payload["action"] = request.action

    if request.name is not None:
        payload.setdefault("name", request.name)
    if request.path is not None:
        payload.setdefault("path", request.path)
        payload.setdefault("assetPath", _derive_asset_path(request))
    if request.template is not None:
        payload.setdefault("template", request.template)

    graph_contract_json = _normalize_graph_contract_json(payload)
    if graph_contract_json is not None:
        payload["graphContractJson"] = graph_contract_json

    payload["request"] = asdict(request)
    return payload


def _derive_asset_path(request: ShaderGraphAssetRequest) -> str | None:
    """Compute a likely asset path for create requests or normalize direct paths."""

    effective_path = _effective_path(request)
    if request.action == "create_graph":
        name = request.name
        if name is None:
            return None
        directory = effective_path or ACTION_DEFAULT_PATHS["create_graph"]
        return str(PurePosixPath(directory) / f"{name}.shadergraph")
    if request.action == "create_subgraph":
        name = request.name
        if name is None:
            return None
        directory = effective_path or ACTION_DEFAULT_PATHS["create_subgraph"]
        return str(PurePosixPath(directory) / f"{name}.shadersubgraph")
    return effective_path


def _effective_path(request: ShaderGraphAssetRequest) -> str | None:
    """Return the normalized path from the request or embedded defaults."""

    return request.path or optional_text(request.payload.get("path"))


def _normalize_node_token(value: Any) -> str:
    text = optional_text(value) or ""
    return "".join(character.upper() for character in text if character.isalnum())


def _is_property_node_type(value: Any) -> bool:
    token = _normalize_node_token(value)
    return token in {"PROPERTY", "PROPERTYNODE", "UNITYEDITORSHADERGRAPHPROPERTYNODE"}


def _normalize_move_graph_target_asset_path(source_asset_path: str | None, raw_target_path: Any) -> str | None:
    target_path = optional_text(raw_target_path)
    if target_path is None:
        return None

    normalized_target_path = target_path.replace("\\", "/")
    if normalized_target_path.lower().endswith(".shadergraph"):
        return normalized_target_path

    source_path = optional_text(source_asset_path)
    if source_path is None:
        return normalized_target_path

    source_file_name = PurePosixPath(source_path).name
    if not source_file_name:
        return normalized_target_path

    return str(PurePosixPath(normalized_target_path) / source_file_name)


def _normalize_move_subgraph_target_asset_path(source_asset_path: str | None, raw_target_path: Any) -> str | None:
    target_path = optional_text(raw_target_path)
    if target_path is None:
        return None

    normalized_target_path = target_path.replace("\\", "/")
    if normalized_target_path.lower().endswith(".shadersubgraph"):
        return normalized_target_path

    source_path = optional_text(source_asset_path)
    if source_path is None:
        return normalized_target_path

    source_file_name = PurePosixPath(source_path).name
    if not source_file_name:
        return normalized_target_path

    return str(PurePosixPath(normalized_target_path) / source_file_name)


def _normalize_graph_contract_json(payload: Mapping[str, Any]) -> str | None:
    contract_json = optional_text(
        _pick_value(
            payload,
            "graphContractJson",
            "graph_contract_json",
            "contractJson",
            "contract_json",
        )
    )
    if contract_json is not None:
        return contract_json

    for key in ("graphContract", "graph_contract", "exportedGraphContract", "exported_graph_contract", "contract"):
        if key not in payload or payload[key] is None:
            continue
        candidate = payload[key]
        if isinstance(candidate, Mapping):
            return json.dumps(dict(candidate), sort_keys=True, separators=(",", ":"))
        if isinstance(candidate, (list, tuple)):
            return json.dumps(list(candidate), sort_keys=True, separators=(",", ":"))
        text = optional_text(candidate)
        if text is not None:
            return text

    return None


def _validate_shadergraph_asset_path_kind(request: ShaderGraphAssetRequest) -> None:
    path = optional_text(request.path)
    if path is None:
        return

    lowered_path = path.lower()
    action = request.action
    suggested_action = _PATH_ACTION_MISMATCH_HINTS.get(action)

    if action in SUBGRAPH_PATH_ACTIONS:
        if lowered_path.endswith(".shadergraph"):
            if suggested_action is not None:
                raise ShaderGraphRequestError(
                    f"{action} requires a .shadersubgraph asset path, got '{path}'. "
                    f"Did you mean '{suggested_action}'?"
                )
            raise ShaderGraphRequestError(
                f"{action} requires a .shadersubgraph asset path, got '{path}'."
            )
        return

    if action in GRAPH_PATH_ACTIONS and lowered_path.endswith(".shadersubgraph"):
        if suggested_action is not None:
            raise ShaderGraphRequestError(
                f"{action} requires a .shadergraph asset path, got '{path}'. "
                f"Did you mean '{suggested_action}'?"
            )
        raise ShaderGraphRequestError(
            f"{action} requires a .shadergraph asset path, got '{path}'."
        )


def _require_payload_text(payload: Mapping[str, Any], *keys: str) -> str:
    """Require a textual payload field by trying aliases in order."""

    value = _pick_value(payload, *keys)
    if value is None:
        labels = ", ".join(repr(key) for key in keys)
        raise ShaderGraphRequestError(f"Missing required field(s): {labels}.")
    text = optional_text(value)
    if text is None:
        labels = ", ".join(repr(key) for key in keys)
        raise ShaderGraphRequestError(f"Missing required field(s): {labels}.")
    return text


def _require_payload_number_text(payload: Mapping[str, Any], *keys: str) -> str:
    """Require a numeric payload field and normalize it to trimmed text."""

    value = _pick_value(payload, *keys)
    labels = ", ".join(repr(key) for key in keys)
    if value is None or isinstance(value, bool):
        raise ShaderGraphRequestError(f"Missing required field(s): {labels}.")

    text = str(value).strip()
    try:
        float(text)
    except (TypeError, ValueError) as exc:
        raise ShaderGraphRequestError(f"Field {labels} must be a valid number.") from exc
    return text


def _require_payload_int_text(payload: Mapping[str, Any], *keys: str) -> str:
    """Require an integer payload field and normalize it to trimmed text."""

    value = _pick_value(payload, *keys)
    labels = ", ".join(repr(key) for key in keys)
    if value is None or isinstance(value, bool):
        raise ShaderGraphRequestError(f"Missing required field(s): {labels}.")

    text = str(value).strip()
    try:
        int(text)
    except (TypeError, ValueError) as exc:
        raise ShaderGraphRequestError(f"Field {labels} must be a valid integer.") from exc
    return text
