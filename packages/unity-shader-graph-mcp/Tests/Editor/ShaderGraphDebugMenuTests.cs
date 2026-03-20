using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Diagnostics;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphDebugMenuTests
    {
        [Test]
        public void TryExtractAddedNodeId_ReturnsTrue_ForReadOnlyDictionaryPayloadWithExtraFields()
        {
            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["addedNode"] = new Dictionary<string, object>
                    {
                        ["requestedNodeType"] = "Vector1",
                        ["resolvedNodeType"] = "Float/Vector1",
                        ["objectId"] = "node-123",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -620,
                            ["y"] = 140,
                        },
                    },
                });

            Assert.That(
                ShaderGraphDebugMenu.TryExtractAddedNodeId(response, out string objectId),
                Is.True);
            Assert.That(objectId, Is.EqualTo("node-123"));
        }

        [Test]
        public void TryExtractAddedNodeId_ReturnsTrue_ForDictionaryPayloadWithExtraFields()
        {
            var addedNode = new Hashtable
            {
                ["requestedNodeType"] = "Color",
                ["resolvedNodeType"] = "Color",
                ["objectId"] = "node-456",
                ["position"] = new Hashtable
                {
                    ["x"] = -620,
                    ["y"] = -120,
                },
            };

            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["addedNode"] = addedNode,
                });

            Assert.That(
                ShaderGraphDebugMenu.TryExtractAddedNodeId(response, out string objectId),
                Is.True);
            Assert.That(objectId, Is.EqualTo("node-456"));
        }

        [Test]
        public void TryExtractAddedNodeId_ReturnsFalse_WhenAddedNodeIsMissing()
        {
            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                });

            Assert.That(
                ShaderGraphDebugMenu.TryExtractAddedNodeId(response, out string objectId),
                Is.False);
            Assert.That(objectId, Is.Empty);
        }

        [Test]
        public void TryExtractAddedNodeId_ReturnsFalse_WhenObjectIdIsBlank()
        {
            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["addedNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "   ",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -620,
                            ["y"] = 140,
                        },
                    },
                });

            Assert.That(
                ShaderGraphDebugMenu.TryExtractAddedNodeId(response, out string objectId),
                Is.False);
            Assert.That(objectId, Is.Empty);
        }
    }
}
