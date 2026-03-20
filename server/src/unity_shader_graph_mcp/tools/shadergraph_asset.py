"""Shader Graph asset tool scaffold.

This module intentionally avoids MCP transport imports so the server can be
grown from a clean contract first. The real Unity-side implementation can land
behind this shape later.
"""

from __future__ import annotations

from dataclasses import asdict
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

SUPPORTED_SHADERGRAPH_ASSET_ACTIONS: tuple[str, ...] = (
    "create_graph",
    "read_graph_summary",
    "add_property",
    "add_node",
    "connect_ports",
    "save_graph",
)

ACTION_DEFAULT_PATHS: dict[str, str] = {
    "create_graph": "Assets/ShaderGraphs",
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

    request = ShaderGraphAssetRequest(
        action=action,  # type: ignore[arg-type]
        name=optional_text(_pick_value(request_payload, "name", "graphName")),
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


def _validate_shadergraph_asset_request(request: ShaderGraphAssetRequest) -> None:
    """Enforce per-action requirements with clear error messages."""

    if request.action == "create_graph" and request.name is None:
        raise ShaderGraphRequestError("Missing required field 'name'.")

    if request.action == "read_graph_summary" and request.path is None:
        raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "add_property":
        _require_payload_text(request.payload, "propertyName", "property_name")
        _require_payload_text(request.payload, "propertyType", "property_type")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "add_node":
        _require_payload_text(request.payload, "nodeType", "node_type")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.action == "connect_ports":
        _require_payload_text(request.payload, "outputNodeId", "output_node_id")
        _require_payload_text(request.payload, "outputPort", "output_port")
        _require_payload_text(request.payload, "inputNodeId", "input_node_id")
        _require_payload_text(request.payload, "inputPort", "input_port")
        if request.path is None:
            raise ShaderGraphRequestError("Missing required field 'path' or 'assetPath'.")

    if request.template is not None and request.template not in {"urp_lit", "urp_unlit", "blank"}:
        raise ShaderGraphRequestError(
            "Invalid template. Expected one of: urp_lit, urp_unlit, blank."
        )

    if request.action == "create_graph" and request.path is None:
        default_path = ACTION_DEFAULT_PATHS["create_graph"]
        request.payload.setdefault("path", default_path)

    if request.action == "save_graph" and request.path is None:
        # Saving the active graph is still useful in the scaffold phase.
        request.payload.setdefault("path", "Assets/ShaderGraphs/ActiveGraph.shadergraph")


def handle_shadergraph_asset(payload: Mapping[str, Any]) -> dict[str, Any]:
    """Dispatch the focused Shader Graph asset tool.

    Milestone 1 is a scaffold, so the tool reports the intended operation and
    returns a deterministic placeholder response instead of touching Unity.
    """
    try:
        request = normalize_shadergraph_asset_request(payload)
        summary = _request_summary(request)
        return as_response(
            success=True,
            message=f"Shader Graph asset request validated for '{request.action}'.",
            data={
                "request": summary,
                "status": "scaffold",
                "validationState": "validated",
                "next_step": "Connect this contract to the Unity Editor implementation.",
            },
        )
    except ShaderGraphRequestError as exc:
        return as_response(
            success=False,
            message=str(exc),
            data={
                "supportedActions": list(SUPPORTED_SHADERGRAPH_ASSET_ACTIONS),
                "errorCode": "validation_error",
            },
        )


def _request_summary(request: ShaderGraphAssetRequest) -> dict[str, Any]:
    """Build a JSON-friendly view of the normalized request."""

    summary = asdict(request)
    summary["path"] = _effective_path(request)
    summary["assetPath"] = _derive_asset_path(request)
    summary.pop("payload", None)
    return summary


def _derive_asset_path(request: ShaderGraphAssetRequest) -> str | None:
    """Compute a likely asset path for create requests or normalize direct paths."""

    effective_path = _effective_path(request)
    if request.action == "create_graph":
        name = request.name
        if name is None:
            return None
        directory = effective_path or ACTION_DEFAULT_PATHS["create_graph"]
        return str(PurePosixPath(directory) / f"{name}.shadergraph")
    return effective_path


def _effective_path(request: ShaderGraphAssetRequest) -> str | None:
    """Return the normalized path from the request or embedded defaults."""

    return request.path or optional_text(request.payload.get("path"))


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
