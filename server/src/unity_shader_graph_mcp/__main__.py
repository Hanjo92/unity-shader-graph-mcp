"""Module entry point for ``python -m unity_shader_graph_mcp``."""

from __future__ import annotations

import sys
from pathlib import Path

if __package__ in {None, ""}:
    sys.path.insert(0, str(Path(__file__).resolve().parents[1]))
    from unity_shader_graph_mcp.server import main
else:
    from .server import main


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
