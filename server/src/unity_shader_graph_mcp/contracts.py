"""Shared request and response shapes for the Shader Graph MCP server."""

from __future__ import annotations

from dataclasses import dataclass, field
from typing import Any, Literal, Mapping

ShaderGraphAction = Literal[
    "create_graph",
    "read_graph_summary",
    "find_node",
    "list_supported_nodes",
    "update_property",
    "move_node",
    "add_property",
    "add_node",
    "connect_ports",
    "save_graph",
]

ShaderGraphTemplate = Literal["urp_lit", "urp_unlit", "blank"]


class ShaderGraphRequestError(ValueError):
    """Raised when a Shader Graph request cannot be normalized."""


@dataclass(frozen=True, slots=True)
class ShaderGraphAssetRequest:
    """Transport-neutral request model for milestone 1."""

    action: ShaderGraphAction
    name: str | None = None
    path: str | None = None
    template: ShaderGraphTemplate | None = None
    payload: dict[str, Any] = field(default_factory=dict)


@dataclass(frozen=True, slots=True)
class ShaderGraphAssetResponse:
    """Transport-neutral response model for milestone 1."""

    success: bool
    message: str
    data: dict[str, Any] = field(default_factory=dict)


def optional_text(value: Any) -> str | None:
    """Return a trimmed string or None for empty values."""

    text = None if value is None else str(value).strip()
    return text or None


def require_text(value: Any, field_name: str) -> str:
    """Require a non-empty string and normalize whitespace."""

    text = optional_text(value)
    if text is None:
        raise ShaderGraphRequestError(f"Missing required field '{field_name}'.")
    return text


def coerce_mapping(value: Mapping[str, Any] | None) -> dict[str, Any]:
    """Return a defensive copy of a mapping or an empty dict."""

    return {} if value is None else dict(value)


def as_response(
    success: bool,
    message: str,
    data: dict[str, Any] | None = None,
) -> dict[str, Any]:
    """Return a plain dict so the server can stay transport-agnostic."""

    response = ShaderGraphAssetResponse(
        success=success,
        message=message,
        data={} if data is None else data,
    )
    return {
        "success": response.success,
        "message": response.message,
        "data": response.data,
    }
