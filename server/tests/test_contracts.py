from __future__ import annotations

import sys
import unittest
from pathlib import Path

SERVER_ROOT = Path(__file__).resolve().parents[1]
SRC_ROOT = SERVER_ROOT / "src"
if str(SRC_ROOT) not in sys.path:
    sys.path.insert(0, str(SRC_ROOT))

from unity_shader_graph_mcp.contracts import (
    ShaderGraphRequestError,
    optional_text,
    require_text,
    coerce_mapping,
    as_response,
)


class ContractsTests(unittest.TestCase):
    def test_optional_text_returns_none_for_none(self) -> None:
        self.assertIsNone(optional_text(None))

    def test_optional_text_returns_none_for_empty_string(self) -> None:
        self.assertIsNone(optional_text(""))

    def test_optional_text_returns_none_for_whitespace_string(self) -> None:
        self.assertIsNone(optional_text("   "))

    def test_optional_text_trims_valid_string(self) -> None:
        self.assertEqual(optional_text("  hello  "), "hello")

    def test_require_text_returns_trimmed_string(self) -> None:
        self.assertEqual(require_text("  hello  ", "field"), "hello")

    def test_require_text_raises_error_for_none(self) -> None:
        with self.assertRaises(ShaderGraphRequestError) as ctx:
            require_text(None, "my_field")
        self.assertEqual(str(ctx.exception), "Missing required field 'my_field'.")

    def test_require_text_raises_error_for_empty_string(self) -> None:
        with self.assertRaises(ShaderGraphRequestError) as ctx:
            require_text("", "my_field")
        self.assertEqual(str(ctx.exception), "Missing required field 'my_field'.")

    def test_require_text_raises_error_for_whitespace_string(self) -> None:
        with self.assertRaises(ShaderGraphRequestError) as ctx:
            require_text("   ", "my_field")
        self.assertEqual(str(ctx.exception), "Missing required field 'my_field'.")

    def test_coerce_mapping_returns_empty_dict_for_none(self) -> None:
        self.assertEqual(coerce_mapping(None), {})

    def test_coerce_mapping_returns_copy_of_mapping(self) -> None:
        input_map = {"a": 1}
        result = coerce_mapping(input_map)
        self.assertEqual(result, input_map)
        self.assertIsNot(result, input_map)

    def test_as_response_returns_expected_dict(self) -> None:
        result = as_response(True, "Success", {"key": "value"})
        self.assertEqual(
            result,
            {
                "success": True,
                "message": "Success",
                "data": {"key": "value"},
            },
        )

    def test_as_response_defaults_data_to_empty_dict(self) -> None:
        result = as_response(False, "Error")
        self.assertEqual(
            result,
            {
                "success": False,
                "message": "Error",
                "data": {},
            },
        )


if __name__ == "__main__":
    unittest.main()
