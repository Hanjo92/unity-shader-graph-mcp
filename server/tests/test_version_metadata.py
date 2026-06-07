from __future__ import annotations

import json
import sys
import tomllib
import unittest
from pathlib import Path

SERVER_ROOT = Path(__file__).resolve().parents[1]
REPO_ROOT = SERVER_ROOT.parent
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.transport import MCP_SERVER_VERSION


class VersionMetadataTests(unittest.TestCase):
    def test_unity_python_and_mcp_versions_match(self) -> None:
        unity_package = json.loads(
            (REPO_ROOT / "packages/unity-shader-graph-mcp/package.json").read_text(encoding="utf-8")
        )
        python_project = tomllib.loads(
            (SERVER_ROOT / "pyproject.toml").read_text(encoding="utf-8")
        )

        self.assertEqual(unity_package["version"], python_project["project"]["version"])
        self.assertEqual(unity_package["version"], MCP_SERVER_VERSION)


if __name__ == "__main__":
    unittest.main()
