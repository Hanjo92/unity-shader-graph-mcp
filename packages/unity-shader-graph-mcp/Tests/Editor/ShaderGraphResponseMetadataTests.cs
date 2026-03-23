using System.Collections.Generic;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Diagnostics;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphResponseMetadataTests
    {
        private static readonly string[] CurrentSupportedConnectionRules =
        {
            "Node ids must be exact GraphData objectId values reported by add_node or read_graph_summary.",
            "This first path only supports Vector1Node output slot 0 / Out into a different Vector1Node input slot 1 / X.",
            "ColorNode output slot 0 / Out is supported only when the input node is SplitNode input slot 0 / In.",
            "SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X.",
            "Vector1Node, SplitNode channel outputs, and scalar arithmetic output slot Out are supported when the input node is CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In.",
            "Vector1Node output slot 0 / Out is also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are supported when the input node is a different Vector1Node input slot 1 / X.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "Vector1Node and scalar arithmetic output slot Out are supported when the input node is ComparisonNode input slot 0 / A or 1 / B.",
            "ComparisonNode output slot 2 / Out is supported only when the input node is BranchNode input slot 0 / Predicate.",
            "Vector1Node and scalar arithmetic output slot Out are supported when the input node is BranchNode input slot 1 / True or 2 / False.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B.",
            "BranchNode output slot 3 / Out is supported when the input node is a different Vector1Node input slot 1 / X or scalar arithmetic input ports.",
            "Self-connections are rejected.",
        };

        [Test]
        public void Ok_PreservesMetadataEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "package-backed mutation ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["supportedPropertyTypes"] = new[] { "Color", "Float/Vector1" },
                    ["supportedNodeTypes"] = new[] { "Color", "Split", "Float/Vector1" },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Is.EqualTo("package-backed mutation ready"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["packageDetected"], Is.True);
            Assert.That(
                (string[])response.Data["supportedNodeTypes"],
                Is.EquivalentTo(new[] { "Color", "Split", "Float/Vector1" }));
        }

        [Test]
        public void Ok_PreservesConnectionMetadataEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Is.EqualTo("connect ports ready"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Is.EquivalentTo(CurrentSupportedConnectionRules));
        }

        [Test]
        public void Ok_PreservesAddedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["nodeCatalogSemantics"] = "supported=graph-addable",
                    ["supportedNodeTypes"] = new[]
                    {
                        "Color (UnityEditor.ShaderGraph.ColorNode)",
                        "Split (UnityEditor.ShaderGraph.SplitNode)",
                        "Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)",
                    },
                    ["supportedNodeCount"] = 3,
                    ["discoveredNodeTypes"] = new[]
                    {
                        "Add (UnityEditor.ShaderGraph.AddNode)",
                        "Color (UnityEditor.ShaderGraph.ColorNode)",
                        "Split (UnityEditor.ShaderGraph.SplitNode)",
                        "Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)",
                    },
                    ["discoveredNodeCount"] = 4,
                    ["addedNode"] = new Dictionary<string, object>
                    {
                        ["requestedNodeType"] = "Vector1",
                        ["resolvedNodeType"] = "Float/Vector1",
                        ["resolvedNodeClass"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["displayName"] = "Vector1 A",
                        ["objectId"] = "node-17",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -620f,
                            ["y"] = 140f,
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["nodeCatalogSemantics"], Is.EqualTo("supported=graph-addable"));
            Assert.That(response.Data["supportedNodeCount"], Is.EqualTo(3));
            Assert.That(response.Data["discoveredNodeCount"], Is.EqualTo(4));

            var addedNode = (IReadOnlyDictionary<string, object>)response.Data["addedNode"];
            Assert.That(addedNode["requestedNodeType"], Is.EqualTo("Vector1"));
            Assert.That(addedNode["resolvedNodeType"], Is.EqualTo("Float/Vector1"));
            Assert.That(addedNode["objectId"], Is.EqualTo("node-17"));

            Assert.That(
                ShaderGraphDebugMenu.TryExtractAddedNodeId(response, out string objectId),
                Is.True);
            Assert.That(objectId, Is.EqualTo("node-17"));

            var position = (IReadOnlyDictionary<string, object>)addedNode["position"];
            Assert.That(position["x"], Is.EqualTo(-620f));
            Assert.That(position["y"], Is.EqualTo(140f));
        }

        [Test]
        public void Ok_PreservesFindNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "find node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Lookup Source",
                        ["nodeType"] = "Vector1",
                    },
                    ["foundNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-lookup-1",
                        ["nodeId"] = "node-lookup-1",
                        ["displayName"] = "Lookup Source",
                        ["nodeType"] = "Float/Vector1",
                        ["fullTypeName"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["summary"] = "Lookup Source (node-lookup-1) [Vector1Node] @ (-620, 120)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var query = (IReadOnlyDictionary<string, object>)response.Data["query"];
            Assert.That(query["displayName"], Is.EqualTo("Lookup Source"));
            Assert.That(query["nodeType"], Is.EqualTo("Vector1"));

            var foundNode = (IReadOnlyDictionary<string, object>)response.Data["foundNode"];
            Assert.That(foundNode["objectId"], Is.EqualTo("node-lookup-1"));
            Assert.That(foundNode["nodeType"], Is.EqualTo("Float/Vector1"));
        }

        [Test]
        public void Ok_PreservesSupportedNodeCatalogEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "supported node catalog ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["nodeCatalogSemantics"] = "supported=graph-addable",
                    ["supportedNodeTypes"] = new[]
                    {
                        "Color (UnityEditor.ShaderGraph.ColorNode)",
                        "Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)",
                    },
                    ["supportedNodeCanonicalNames"] = new[] { "Color", "Float/Vector1" },
                    ["supportedNodeCount"] = 2,
                    ["discoveredNodeTypes"] = new[]
                    {
                        "Add (UnityEditor.ShaderGraph.AddNode)",
                        "Color (UnityEditor.ShaderGraph.ColorNode)",
                        "Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)",
                    },
                    ["discoveredNodeCount"] = 3,
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["nodeCatalogSemantics"], Is.EqualTo("supported=graph-addable"));
            Assert.That(response.Data["supportedNodeCount"], Is.EqualTo(2));
            Assert.That(response.Data["discoveredNodeCount"], Is.EqualTo(3));

            Assert.That((string[])response.Data["supportedNodeCanonicalNames"], Is.EquivalentTo(new[] { "Color", "Float/Vector1" }));
        }

        [Test]
        public void Ok_PreservesUpdatedPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "update property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedPropertyTypes"] = new[] { "Color", "Float/Vector1" },
                    ["updatedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Tint",
                        ["referenceName"] = "Tint",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                        ["defaultValue"] = "RGBA(1.000, 1.000, 1.000, 1.000)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));

            var updatedProperty = (IReadOnlyDictionary<string, object>)response.Data["updatedProperty"];
            Assert.That(updatedProperty["displayName"], Is.EqualTo("Tint"));
            Assert.That(updatedProperty["resolvedPropertyType"], Is.EqualTo("Color"));
        }

        [Test]
        public void Ok_PreservesRenamedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "rename node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["nodeId"] = "node-17",
                        ["objectId"] = "node-17",
                    },
                    ["renamedNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-17",
                        ["nodeId"] = "node-17",
                        ["displayName"] = "Renamed Source",
                        ["previousDisplayName"] = "Original Source",
                        ["nodeType"] = "Float/Vector1",
                        ["summary"] = "Renamed Source (node-17) [Vector1Node] @ (-620, 140)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var renamedNode = (IReadOnlyDictionary<string, object>)response.Data["renamedNode"];
            Assert.That(renamedNode["objectId"], Is.EqualTo("node-17"));
            Assert.That(renamedNode["displayName"], Is.EqualTo("Renamed Source"));
            Assert.That(renamedNode["previousDisplayName"], Is.EqualTo("Original Source"));
        }

        [Test]
        public void Ok_PreservesRenamedPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "rename property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["propertyName"] = "Tint",
                    },
                    ["renamedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Base Tint",
                        ["referenceName"] = "_BaseTint",
                        ["previousDisplayName"] = "Tint",
                        ["previousReferenceName"] = "_Tint",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var renamedProperty = (IReadOnlyDictionary<string, object>)response.Data["renamedProperty"];
            Assert.That(renamedProperty["displayName"], Is.EqualTo("Base Tint"));
            Assert.That(renamedProperty["referenceName"], Is.EqualTo("_BaseTint"));
            Assert.That(renamedProperty["previousDisplayName"], Is.EqualTo("Tint"));
        }

        [Test]
        public void Ok_PreservesFoundPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "find property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["referenceName"] = "_BaseColor",
                        ["propertyType"] = "Color",
                    },
                    ["foundProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Base Color",
                        ["referenceName"] = "_BaseColor",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var foundProperty = (IReadOnlyDictionary<string, object>)response.Data["foundProperty"];
            Assert.That(foundProperty["displayName"], Is.EqualTo("Base Color"));
            Assert.That(foundProperty["referenceName"], Is.EqualTo("_BaseColor"));
            Assert.That(foundProperty["resolvedPropertyType"], Is.EqualTo("Color"));
        }

        [Test]
        public void Ok_PreservesDuplicatedPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "duplicate property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["propertyName"] = "Tint",
                    },
                    ["duplicatedFrom"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Tint",
                        ["referenceName"] = "_Tint",
                        ["resolvedPropertyType"] = "Color",
                    },
                    ["duplicatedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Copied Tint",
                        ["referenceName"] = "_CopiedTint",
                        ["sourceDisplayName"] = "Tint",
                        ["sourceReferenceName"] = "_Tint",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var duplicatedProperty = (IReadOnlyDictionary<string, object>)response.Data["duplicatedProperty"];
            Assert.That(duplicatedProperty["displayName"], Is.EqualTo("Copied Tint"));
            Assert.That(duplicatedProperty["referenceName"], Is.EqualTo("_CopiedTint"));
            Assert.That(duplicatedProperty["sourceDisplayName"], Is.EqualTo("Tint"));
            Assert.That(duplicatedProperty["resolvedPropertyType"], Is.EqualTo("Color"));
        }

        [Test]
        public void Ok_PreservesDuplicatedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "duplicate node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["nodeId"] = "node-17",
                        ["objectId"] = "node-17",
                    },
                    ["duplicatedFrom"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-17",
                        ["nodeId"] = "node-17",
                        ["displayName"] = "Source Add",
                        ["nodeType"] = "Add",
                    },
                    ["duplicatedNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-18",
                        ["nodeId"] = "node-18",
                        ["displayName"] = "Copied Add",
                        ["sourceNodeId"] = "node-17",
                        ["sourceDisplayName"] = "Source Add",
                        ["nodeType"] = "Add",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -400f,
                            ["y"] = 200f,
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var duplicatedFrom = (IReadOnlyDictionary<string, object>)response.Data["duplicatedFrom"];
            Assert.That(duplicatedFrom["objectId"], Is.EqualTo("node-17"));

            var duplicatedNode = (IReadOnlyDictionary<string, object>)response.Data["duplicatedNode"];
            Assert.That(duplicatedNode["objectId"], Is.EqualTo("node-18"));
            Assert.That(duplicatedNode["displayName"], Is.EqualTo("Copied Add"));
            Assert.That(duplicatedNode["sourceNodeId"], Is.EqualTo("node-17"));
        }

        [Test]
        public void Ok_PreservesMovedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "move node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["nodeId"] = "node-17",
                        ["objectId"] = "node-17",
                    },
                    ["movedNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-17",
                        ["nodeId"] = "node-17",
                        ["displayName"] = "Move Source",
                        ["nodeType"] = "Float/Vector1",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -420f,
                            ["y"] = 180f,
                        },
                        ["previousPosition"] = new Dictionary<string, object>
                        {
                            ["x"] = -620f,
                            ["y"] = 140f,
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var movedNode = (IReadOnlyDictionary<string, object>)response.Data["movedNode"];
            Assert.That(movedNode["objectId"], Is.EqualTo("node-17"));
            Assert.That(movedNode["displayName"], Is.EqualTo("Move Source"));

            var position = (IReadOnlyDictionary<string, object>)movedNode["position"];
            Assert.That(position["x"], Is.EqualTo(-420f));
            Assert.That(position["y"], Is.EqualTo(180f));
        }

        [Test]
        public void Ok_PreservesDeletedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "delete node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["nodeId"] = "node-17",
                        ["objectId"] = "node-17",
                    },
                    ["deletedNode"] = new Dictionary<string, object>
                    {
                        ["objectId"] = "node-17",
                        ["nodeId"] = "node-17",
                        ["displayName"] = "Delete Source",
                        ["nodeType"] = "Float/Vector1",
                        ["summary"] = "Delete Source (node-17) [Vector1Node] @ (-620, 140)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var deletedNode = (IReadOnlyDictionary<string, object>)response.Data["deletedNode"];
            Assert.That(deletedNode["objectId"], Is.EqualTo("node-17"));
            Assert.That(deletedNode["displayName"], Is.EqualTo("Delete Source"));
        }

        [Test]
        public void Ok_PreservesRemovedPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "remove property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedPropertyTypes"] = new[] { "Color", "Float/Vector1" },
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["propertyName"] = "Tint",
                    },
                    ["deletedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Tint",
                        ["referenceName"] = "Tint",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                        ["summary"] = "Tint [Color]",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var query = (IReadOnlyDictionary<string, object>)response.Data["query"];
            Assert.That(query["propertyName"], Is.EqualTo("Tint"));

            var deletedProperty = (IReadOnlyDictionary<string, object>)response.Data["deletedProperty"];
            Assert.That(deletedProperty["displayName"], Is.EqualTo("Tint"));
            Assert.That(deletedProperty["resolvedPropertyType"], Is.EqualTo("Color"));
        }

        [Test]
        public void Ok_PreservesRemovedConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "remove connection ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["matchCount"] = 1,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-node",
                        ["inputPort"] = "X",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-node",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                    },
                    ["deletedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-node",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["fullTypeName"] = "UnityEditor.ShaderGraph.Edge",
                        ["summary"] = "Source (source-node) [Vector1Node] @ (-620, 140):Out(0) -> Target (target-node) [Vector1Node] @ (-260, 140):X(1)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var deletedConnection = (IReadOnlyDictionary<string, object>)response.Data["deletedConnection"];
            Assert.That(deletedConnection["outputNodeId"], Is.EqualTo("source-node"));
            Assert.That(deletedConnection["inputNodeId"], Is.EqualTo("target-node"));
            Assert.That(deletedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(deletedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesColorAndSplitAddedNodeEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "add node ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedNodeTypes"] = new[]
                    {
                        "Color (UnityEditor.ShaderGraph.ColorNode)",
                        "Split (UnityEditor.ShaderGraph.SplitNode)",
                        "Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)",
                    },
                    ["addedNode"] = new Dictionary<string, object>
                    {
                        ["requestedNodeType"] = "Split",
                        ["resolvedNodeType"] = "Split",
                        ["resolvedNodeClass"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["displayName"] = "Split A",
                        ["objectId"] = "node-18",
                        ["position"] = new Dictionary<string, object>
                        {
                            ["x"] = -260f,
                            ["y"] = -120f,
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));

            var addedNode = (IReadOnlyDictionary<string, object>)response.Data["addedNode"];
            Assert.That(addedNode["requestedNodeType"], Is.EqualTo("Split"));
            Assert.That(addedNode["resolvedNodeClass"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(addedNode["objectId"], Is.EqualTo("node-18"));

            var position = (IReadOnlyDictionary<string, object>)addedNode["position"];
            Assert.That(position["x"], Is.EqualTo(-260f));
            Assert.That(position["y"], Is.EqualTo(-120f));
        }

        [Test]
        public void Ok_PreservesPackageBackedBlankCreateGraphEnvelope()
        {
            var surface = new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                "UnityEditor.ShaderGraph.JsonObject",
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                new[] { "System.Void ValidateGraph()" },
                System.Array.Empty<string>());

            var compatibility = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.PackageReady,
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "Unity.ShaderGraph.Editor" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                System.Array.Empty<string>(),
                surface,
                new[] { "package-backed create_graph ready" });

            var snapshot = new ShaderGraphAssetSnapshot(
                "create_graph",
                "Assets/ShaderGraphs/Blank.shadergraph",
                string.Empty,
                "/tmp/Blank.shadergraph",
                true,
                false,
                "unity-shader-graph-mcp/package-backed-v1",
                "Blank",
                "blank",
                "2026-03-19T00:00:00.0000000Z",
                "2026-03-19T00:00:00.0000000Z",
                0,
                0,
                0,
                ShaderGraphExecutionKind.PackageBacked,
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                new[] { "GraphData.ValidateGraph() invoked successfully after create_graph." },
                new[] { "blank graph preview" },
                compatibility);

            var response = ShaderGraphResponse.Ok(
                "Created blank package-backed Shader Graph at 'Assets/ShaderGraphs/Blank.shadergraph'.",
                new Dictionary<string, object>(snapshot.ToDictionary())
                {
                    ["supportedCreateTemplates"] = new[] { "blank" },
                    ["createdGraph"] = new Dictionary<string, object>
                    {
                        ["name"] = "Blank",
                        ["requestedTemplate"] = "blank",
                        ["resolvedTemplate"] = "blank",
                        ["graphPathLabel"] = "Shader Graphs",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Does.StartWith("Created blank package-backed Shader Graph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["packageDetected"], Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("create_graph"));
            Assert.That(response.Data["template"], Is.EqualTo("blank"));
            Assert.That(response.Data["schema"], Is.EqualTo("unity-shader-graph-mcp/package-backed-v1"));

            var supportedCreateTemplates = (string[])response.Data["supportedCreateTemplates"];
            Assert.That(supportedCreateTemplates, Is.EquivalentTo(new[] { "blank" }));

            var createdGraph = (IReadOnlyDictionary<string, object>)response.Data["createdGraph"];
            Assert.That(createdGraph["name"], Is.EqualTo("Blank"));
            Assert.That(createdGraph["requestedTemplate"], Is.EqualTo("blank"));
            Assert.That(createdGraph["resolvedTemplate"], Is.EqualTo("blank"));
            Assert.That(createdGraph["graphPathLabel"], Is.EqualTo("Shader Graphs"));

            var compatibilityData = (IReadOnlyDictionary<string, object>)response.Data["compatibility"];
            Assert.That(compatibilityData["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(compatibilityData["packageDetected"], Is.True);
        }

        [Test]
        public void Ok_PreservesPackageBackedAddPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Added Float/Vector1 property 'Exposure' to 'Assets/ShaderGraphs/Test.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "add_property",
                    ["assetPath"] = "Assets/ShaderGraphs/Test.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Test.shadergraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "shadergraph",
                    ["assetName"] = "Test",
                    ["template"] = string.Empty,
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 1,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new[] { "Float/Vector1: Exposure" },
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "GraphData.ValidateGraph() invoked successfully." },
                    ["preview"] = new string[0],
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["supportedPropertyTypes"] = new[] { "Color", "Float/Vector1" },
                    ["addedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Exposure",
                        ["referenceName"] = "_Exposure",
                        ["requestedPropertyType"] = "Vector1",
                        ["resolvedPropertyType"] = "Float/Vector1",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty",
                        ["defaultValue"] = "0",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Does.StartWith("Added Float/Vector1 property 'Exposure'"));
            Assert.That(response.Data["operation"], Is.EqualTo("add_property"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Test.shadergraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["packageDetected"], Is.True);
            Assert.That((string[])response.Data["supportedPropertyTypes"], Is.EquivalentTo(new[] { "Color", "Float/Vector1" }));

            var compatibility = (IReadOnlyDictionary<string, object>)response.Data["compatibility"];
            Assert.That(compatibility["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(compatibility["packageDetected"], Is.True);

            var addedProperty = (IReadOnlyDictionary<string, object>)response.Data["addedProperty"];
            Assert.That(addedProperty["displayName"], Is.EqualTo("Exposure"));
            Assert.That(addedProperty["requestedPropertyType"], Is.EqualTo("Vector1"));
            Assert.That(addedProperty["resolvedPropertyType"], Is.EqualTo("Float/Vector1"));
            Assert.That(addedProperty["resolvedShaderInputType"], Is.EqualTo("UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty"));
            Assert.That(addedProperty["defaultValue"], Is.EqualTo("0"));
        }

        [Test]
        public void Ok_PreservesPackageBackedSaveGraphEnvelope()
        {
            var surface = new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                "UnityEditor.ShaderGraph.JsonObject",
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                true,
                new[] { "System.Void ValidateGraph()" },
                System.Array.Empty<string>());

            var compatibility = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.PackageReady,
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "Unity.ShaderGraph.Editor" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                System.Array.Empty<string>(),
                surface,
                new[] { "package-backed save_graph ready" });

            var snapshot = new ShaderGraphAssetSnapshot(
                "save_graph",
                "Assets/ShaderGraphs/Test.shadergraph",
                string.Empty,
                "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Test.shadergraph",
                true,
                false,
                "unity-shader-graph-mcp/package-backed-v1",
                "Test",
                string.Empty,
                "2026-03-19T00:00:00.0000000Z",
                "2026-03-19T00:00:00.0000000Z",
                1,
                2,
                1,
                ShaderGraphExecutionKind.PackageBacked,
                new[] { "Color: Tint" },
                new[] { "Color A [ColorNode] @ (-620, -120)", "Split A [SplitNode] @ (-260, -120)" },
                new[] { "ColorNode.Out -> SplitNode.In" },
                new[] { "GraphData.ValidateGraph() invoked successfully." },
                new[] { "save graph preview" },
                compatibility);

            var response = ShaderGraphResponse.Ok(
                "Saved package-backed Shader Graph at 'Assets/ShaderGraphs/Test.shadergraph'.",
                new Dictionary<string, object>(snapshot.ToDictionary())
                {
                    ["saveGraphStrategy"] = new[]
                    {
                        "GraphData.ValidateGraph()",
                        "FileUtilities.WriteShaderGraphToDisk(string, GraphData)",
                        "AssetDatabase.SaveAssets()",
                        "AssetDatabase.ImportAsset(..., ForceSynchronousImport | ForceUpdate)",
                        "AssetDatabase.Refresh(ForceSynchronousImport)",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Message, Does.StartWith("Saved package-backed Shader Graph"));
            Assert.That(response.Data["operation"], Is.EqualTo("save_graph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Test.shadergraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["packageDetected"], Is.True);
            Assert.That(response.Data["schema"], Is.EqualTo("unity-shader-graph-mcp/package-backed-v1"));

            var compatibilityData = (IReadOnlyDictionary<string, object>)response.Data["compatibility"];
            Assert.That(compatibilityData["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(compatibilityData["packageDetected"], Is.True);

            var saveGraphStrategy = (string[])response.Data["saveGraphStrategy"];
            Assert.That(saveGraphStrategy, Has.Length.EqualTo(5));
            Assert.That(saveGraphStrategy[0], Is.EqualTo("GraphData.ValidateGraph()"));
            Assert.That(saveGraphStrategy[4], Is.EqualTo("AssetDatabase.Refresh(ForceSynchronousImport)"));
        }

        [Test]
        public void Ok_PreservesFirstConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "node-16",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "node-17",
                        ["inputPort"] = "X",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "node-16",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "node-17",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out is supported only when the input node is SplitNode input slot 0 / In."));
            Assert.That(supportedConnectionRules, Does.Contain("SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X."));

            var requestedConnection = (IReadOnlyDictionary<string, object>)response.Data["requestedConnection"];
            Assert.That(requestedConnection["outputNodeId"], Is.EqualTo("node-16"));
            Assert.That(requestedConnection["inputPort"], Is.EqualTo("X"));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
            Assert.That(resolvedConnection["connectedEdgeType"], Is.EqualTo("UnityEditor.ShaderGraph.Edge"));
        }

        [Test]
        public void Ok_PreservesColorToSplitConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-16",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-17",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-16",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-17",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out is supported only when the input node is SplitNode input slot 0 / In."));
            Assert.That(supportedConnectionRules, Does.Contain("SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X."));

            var requestedConnection = (IReadOnlyDictionary<string, object>)response.Data["requestedConnection"];
            Assert.That(requestedConnection["outputNodeId"], Is.EqualTo("color-16"));
            Assert.That(requestedConnection["inputPort"], Is.EqualTo("In"));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["connectedEdgeType"], Is.EqualTo("UnityEditor.ShaderGraph.Edge"));
        }

        [Test]
        public void Ok_PreservesCombineToSplitConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "combine-18",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "split-19",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "combine-18",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.CombineNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "split-19",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.CombineNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesColorToMultiplyConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-22",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-23",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-22",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-23",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesColorToLerpConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-26",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "lerp-27",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-26",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "lerp-27",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.LerpNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesColorToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-30",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-31",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-30",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-31",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesCombineToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "combine-34",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "append-35",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "combine-34",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.CombineNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "append-35",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.CombineNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesVector4ToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector4-36",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-37",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector4-36",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector4Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-37",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector4Node"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSplitToVector1ConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "split-18",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector1-19",
                        ["inputPort"] = "X",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "split-18",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["outputSlotId"] = 1,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector1-19",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X."));

            var requestedConnection = (IReadOnlyDictionary<string, object>)response.Data["requestedConnection"];
            Assert.That(requestedConnection["outputNodeId"], Is.EqualTo("split-18"));
            Assert.That(requestedConnection["outputPort"], Is.EqualTo("R"));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(1));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
            Assert.That(resolvedConnection["connectedEdgeType"], Is.EqualTo("UnityEditor.ShaderGraph.Edge"));
        }

        [Test]
        public void Ok_PreservesComparisonToBranchConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "comparison-20",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "branch-21",
                        ["inputPort"] = "Predicate",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "comparison-20",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ComparisonNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "branch-21",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "Predicate",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ComparisonNode output slot 2 / Out is supported only when the input node is BranchNode input slot 0 / Predicate."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesBranchToSplitConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "branch-24",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-25",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "branch-24",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["outputSlotId"] = 3,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-25",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesLerpToSplitConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "lerp-28",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-29",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "lerp-28",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.LerpNode",
                        ["outputSlotId"] = 3,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-29",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesAppendToSplitConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-32",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-33",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-32",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-33",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesAppendToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-40",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-41",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-40",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-41",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesAppendToMultiplyConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-50",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-51",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-50",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-51",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesAppendToLerpConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-52",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "lerp-53",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-52",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "lerp-53",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.LerpNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesAppendToBranchConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-54",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "branch-55",
                        ["inputPort"] = "True",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-54",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "branch-55",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "True",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesMultiplyToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "multiply-60",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-61",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "multiply-60",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-61",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesLerpToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "lerp-62",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-63",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "lerp-62",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.LerpNode",
                        ["outputSlotId"] = 3,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-63",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesBranchToAppendConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "connect ports ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "branch-64",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-65",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "branch-64",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["outputSlotId"] = 3,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "append-65",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Fail_WithMetadata_PreservesEnvelopeShape()
        {
            var response = ShaderGraphResponse.Fail(
                "not ready yet",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.Scaffold.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageDetectedButIncomplete.ToString(),
                    ["notes"] = new[] { "placeholder" },
                });

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Is.EqualTo("not ready yet"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("Scaffold"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageDetectedButIncomplete"));
            Assert.That(response.Data["notes"], Is.Not.Null);
        }
    }
}
