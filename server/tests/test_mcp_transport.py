from __future__ import annotations

import io
import json
import sys
import unittest
from pathlib import Path
from unittest import mock

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.server import build_server, main
from unity_shader_graph_mcp.transport import build_mcp_stdio_transport, serve_mcp_stdio


class McpTransportTests(unittest.TestCase):
    def test_live_stdio_transport_handles_initialize_list_and_call(self) -> None:
        server = build_server()
        input_stream = io.BytesIO(
            b"".join(
                _encode_frame(message)
                for message in (
                    {
                        "jsonrpc": "2.0",
                        "id": 1,
                        "method": "initialize",
                        "params": {
                            "protocolVersion": "2024-11-05",
                        },
                    },
                    {
                        "jsonrpc": "2.0",
                        "id": 2,
                        "method": "tools/list",
                        "params": {},
                    },
                    {
                        "jsonrpc": "2.0",
                        "id": 3,
                        "method": "tools/call",
                        "params": {
                            "name": "shadergraph_asset",
                            "arguments": {
                                "action": "save_graph",
                                "path": "Assets/ShaderGraphs/Example.shadergraph",
                            },
                        },
                    },
                )
            )
        )
        output_stream = io.BytesIO()

        serve_mcp_stdio(server, stdin=input_stream, stdout=output_stream)

        responses = _decode_frames(output_stream.getvalue())
        self.assertEqual([response["id"] for response in responses], [1, 2, 3])

        initialize = responses[0]["result"]
        self.assertEqual(initialize["serverInfo"]["name"], "unity-shader-graph-mcp")
        self.assertIn("tools", initialize["capabilities"])

        tools_list = responses[1]["result"]
        self.assertEqual(tools_list["tools"][0]["name"], "shadergraph_asset")
        self.assertIn("inputSchema", tools_list["tools"][0])

        tool_call = responses[2]["result"]
        self.assertFalse(tool_call["isError"])
        payload = json.loads(tool_call["content"][0]["text"])
        self.assertTrue(payload["success"])
        self.assertEqual(payload["data"]["request"]["action"], "save_graph")

    def test_main_mcp_mode_routes_to_stdio_transport(self) -> None:
        with mock.patch("unity_shader_graph_mcp.server.serve_mcp_stdio") as serve_mock:
            exit_code = main(["--mcp"])

        self.assertEqual(exit_code, 0)
        self.assertEqual(serve_mock.call_count, 1)

    def test_mcp_transport_descriptor_lists_registered_tools(self) -> None:
        transport = build_mcp_stdio_transport(build_server())

        response = transport.list_tools()

        self.assertEqual(response["serverInfo"]["name"], "unity-shader-graph-mcp")
        self.assertEqual(response["tools"][0]["name"], "shadergraph_asset")


def _encode_frame(message: dict[str, object]) -> bytes:
    payload = json.dumps(message, separators=(",", ":"), sort_keys=True).encode("utf-8")
    header = f"Content-Length: {len(payload)}\r\n\r\n".encode("ascii")
    return header + payload


def _decode_frames(payload: bytes) -> list[dict[str, object]]:
    stream = io.BytesIO(payload)
    responses: list[dict[str, object]] = []
    while True:
        header = _read_until(stream, b"\r\n\r\n")
        if header is None:
            break
        content_length = None
        for line in header.decode("utf-8").split("\r\n"):
            key, _, value = line.partition(":")
            if key.lower() == "content-length":
                content_length = int(value.strip())
                break
        assert content_length is not None
        body = stream.read(content_length)
        if len(body) != content_length:
            raise AssertionError("Unexpected EOF while reading framed response body.")
        responses.append(json.loads(body.decode("utf-8")))
    return responses


def _read_until(stream: io.BytesIO, marker: bytes) -> bytes | None:
    buffer = bytearray()
    while True:
        chunk = stream.read(1)
        if not chunk:
            return None if not buffer else bytes(buffer)
        buffer.extend(chunk)
        if buffer.endswith(marker):
            return bytes(buffer[:-len(marker)])


if __name__ == "__main__":
    unittest.main()
