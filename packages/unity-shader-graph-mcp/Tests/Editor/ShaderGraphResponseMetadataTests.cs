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
            "Vector4Node output slot 0 / Out is supported when the input node is SubGraphOutputNode input slot 0 / Out.",
            "UVNode output slot Out / UV is supported when the input node is TilingAndOffsetNode input slot UV, SampleTexture2DNode input slot UV, or NormalFromTextureNode input slot UV.",
            "TilingAndOffsetNode output slot Out is supported when the input node is SampleTexture2DNode input slot UV or NormalFromTextureNode input slot UV.",
            "Texture2DAssetNode output slot Out / Texture is supported when the input node is SampleTexture2DNode input slot Texture or NormalFromTextureNode input slot Texture.",
            "SampleTexture2DNode output slot RGBA is supported when the input node is SplitNode input slot 0 / In.",
            "SampleTexture2DNode output slot RGBA is supported when the input node is NormalStrengthNode input slot In.",
            "SampleTexture2DNode output slot RGBA is supported when the input node is NormalUnpackNode input slot In.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is NormalBlendNode input slot 0 / A or 1 / B.",
            "Vector2Node output slot Out is supported when the input node is NormalReconstructZNode input slot In.",
            "SampleTexture2DNode output slots R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X, CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots, ComparisonNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or AppendVectorNode input slot 0 / A or 1 / B.",
            "Vector1Node output slot 0 / Out and scalar arithmetic output slot Out are supported when the input node is NormalStrengthNode input slot Strength or NormalFromTextureNode input slot Offset or Strength.",
            "SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X.",
            "NormalStrengthNode output slot Out is supported when the input node is SplitNode input slot 0 / In.",
            "Vector1Node, SplitNode channel outputs, and scalar arithmetic output slot Out are supported when the input node is CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is SplitNode input slot 0 / In.",
            "Vector1Node output slot 0 / Out is also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are supported when the input node is a different Vector1Node input slot 1 / X.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "Vector1Node, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are supported when the input node is ComparisonNode input slot 0 / A or 1 / B.",
            "ComparisonNode output slot 2 / Out is supported when the input node is BranchNode input slot 0 / Predicate, including fan-out into multiple Branch predicates.",
            "Vector1Node, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are supported when the input node is BranchNode input slot 1 / True or 2 / False.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B.",
            "ColorNode output slot 0 / Out is also supported when the same source fans out into SplitNode input slot 0 / In and MultiplyNode input slot 0 / A or 1 / B within one graph.",
            "AppendVectorNode output slot Out is also supported when the same source fans out into SplitNode input slot 0 / In and MultiplyNode input slot 0 / A or 1 / B within one graph.",
            "BranchNode output slot 3 / Out is supported when the input node is one or more Vector1Node input slot 1 / X or scalar arithmetic input ports.",
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
        public void Ok_PreservesSupportedConnectionCatalogEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "supported connection catalog ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["supportedConnectionRuleCount"] = CurrentSupportedConnectionRules.Length,
                    ["connectionCatalogSemantics"] = "supportedConnectionRules=enforced-runtime-rules",
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["supportedConnectionRuleCount"], Is.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(response.Data["connectionCatalogSemantics"], Is.EqualTo("supportedConnectionRules=enforced-runtime-rules"));

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Is.EquivalentTo(CurrentSupportedConnectionRules));
        }

        [Test]
        public void Ok_PreservesCreatedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "create category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs" },
                    ["categoryCreateSemantics"] = new[]
                    {
                        "Category names are compared case-insensitively for duplicate prevention.",
                        "The created category is appended to GraphData.categories.",
                    },
                    ["createdCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-guid-1",
                        ["displayName"] = "Surface Inputs",
                        ["name"] = "Surface Inputs",
                        ["childCount"] = 0,
                        ["propertyOrder"] = new string[0],
                        ["isDefaultCategory"] = false,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(2));

            var categoryOrder = (string[])response.Data["categoryOrder"];
            Assert.That(categoryOrder, Is.EquivalentTo(new[] { "(Default Category)", "Surface Inputs" }));

            var createdCategory = (IReadOnlyDictionary<string, object>)response.Data["createdCategory"];
            Assert.That(createdCategory["categoryGuid"], Is.EqualTo("category-guid-1"));
            Assert.That(createdCategory["displayName"], Is.EqualTo("Surface Inputs"));
            Assert.That(createdCategory["childCount"], Is.EqualTo(0));
            Assert.That(createdCategory["isDefaultCategory"], Is.EqualTo(false));
        }

        [Test]
        public void Ok_PreservesRenamedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "rename category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Material Inputs" },
                    ["query"] = new Dictionary<string, object>
                    {
                        ["categoryName"] = "Surface Inputs",
                        ["displayName"] = "Material Inputs",
                    },
                    ["renamedCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-guid-1",
                        ["displayName"] = "Material Inputs",
                        ["name"] = "Material Inputs",
                        ["previousDisplayName"] = "Surface Inputs",
                        ["childCount"] = 1,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(2));

            var renamedCategory = (IReadOnlyDictionary<string, object>)response.Data["renamedCategory"];
            Assert.That(renamedCategory["categoryGuid"], Is.EqualTo("category-guid-1"));
            Assert.That(renamedCategory["displayName"], Is.EqualTo("Material Inputs"));
            Assert.That(renamedCategory["previousDisplayName"], Is.EqualTo("Surface Inputs"));
        }

        [Test]
        public void Ok_PreservesFoundCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "find category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["matchStrategy"] = "categoryName/displayName",
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs" },
                    ["query"] = new Dictionary<string, object>
                    {
                        ["categoryName"] = "Surface Inputs",
                    },
                    ["foundCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-guid-1",
                        ["displayName"] = "Surface Inputs",
                        ["name"] = "Surface Inputs",
                        ["childCount"] = 1,
                        ["isDefaultCategory"] = false,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["matchStrategy"], Is.EqualTo("categoryName/displayName"));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(2));

            var foundCategory = (IReadOnlyDictionary<string, object>)response.Data["foundCategory"];
            Assert.That(foundCategory["categoryGuid"], Is.EqualTo("category-guid-1"));
            Assert.That(foundCategory["displayName"], Is.EqualTo("Surface Inputs"));
            Assert.That(foundCategory["isDefaultCategory"], Is.EqualTo(false));
        }

        [Test]
        public void Ok_PreservesDeletedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "delete category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["movedPropertyCount"] = 1,
                    ["categoryCount"] = 1,
                    ["categoryOrder"] = new[] { "(Default Category)" },
                    ["defaultCategoryGuid"] = "category-default",
                    ["defaultCategoryPropertyOrder"] = new[] { "Tint [Color]" },
                    ["deletedCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface",
                        ["displayName"] = "Surface Inputs",
                        ["previousChildCount"] = 1,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["movedPropertyCount"], Is.EqualTo(1));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(1));

            var deletedCategory = (IReadOnlyDictionary<string, object>)response.Data["deletedCategory"];
            Assert.That(deletedCategory["categoryGuid"], Is.EqualTo("category-surface"));
            Assert.That(deletedCategory["displayName"], Is.EqualTo("Surface Inputs"));
            Assert.That(deletedCategory["previousChildCount"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesReorderedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "reorder category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["previousIndex"] = 2,
                    ["newIndex"] = 1,
                    ["categoryCount"] = 3,
                    ["categoryOrder"] = new[] { "(Default Category)", "Math Inputs", "Surface Inputs" },
                    ["reorderedCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-math",
                        ["displayName"] = "Math Inputs",
                        ["childCount"] = 0,
                        ["isDefaultCategory"] = false,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["previousIndex"], Is.EqualTo(2));
            Assert.That(response.Data["newIndex"], Is.EqualTo(1));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(3));

            var reorderedCategory = (IReadOnlyDictionary<string, object>)response.Data["reorderedCategory"];
            Assert.That(reorderedCategory["categoryGuid"], Is.EqualTo("category-math"));
            Assert.That(reorderedCategory["displayName"], Is.EqualTo("Math Inputs"));
            Assert.That(reorderedCategory["isDefaultCategory"], Is.EqualTo(false));
        }

        [Test]
        public void Ok_PreservesCategoryListEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "list categories ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs" },
                    ["categoryListSemantics"] = new[]
                    {
                        "categories contains every GraphData blackboard category, including the default category.",
                        "categoryOrder reflects the current GraphData.categories list order.",
                    },
                    ["categories"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["categoryGuid"] = "category-default",
                            ["displayName"] = "(Default Category)",
                            ["childCount"] = 1,
                            ["isDefaultCategory"] = true,
                        },
                        new Dictionary<string, object>
                        {
                            ["categoryGuid"] = "category-surface",
                            ["displayName"] = "Surface Inputs",
                            ["childCount"] = 0,
                            ["isDefaultCategory"] = false,
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["categoryCount"], Is.EqualTo(2));
            var categories = (object[])response.Data["categories"];
            Assert.That(categories.Length, Is.EqualTo(2));

            var firstCategory = (IReadOnlyDictionary<string, object>)categories[0];
            Assert.That(firstCategory["displayName"], Is.EqualTo("(Default Category)"));
            Assert.That(firstCategory["isDefaultCategory"], Is.EqualTo(true));
        }

        [Test]
        public void Ok_PreservesMergedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "merge category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["sourceMatchCount"] = 1,
                    ["targetMatchCount"] = 1,
                    ["movedPropertyCount"] = 2,
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Material Inputs" },
                    ["targetCategoryGuid"] = "category-material",
                    ["targetCategoryPropertyOrder"] = new[] { "Tint [Color]", "Exposure [Float]" },
                    ["mergedFromCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface",
                        ["displayName"] = "Surface Inputs",
                        ["previousChildCount"] = 2,
                        ["propertyOrder"] = new[] { "Tint [Color]", "Exposure [Float]" },
                    },
                    ["mergedIntoCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-material",
                        ["displayName"] = "Material Inputs",
                        ["childCount"] = 2,
                        ["previousChildCount"] = 0,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["sourceMatchCount"], Is.EqualTo(1));
            Assert.That(response.Data["targetMatchCount"], Is.EqualTo(1));
            Assert.That(response.Data["movedPropertyCount"], Is.EqualTo(2));
            Assert.That(response.Data["targetCategoryGuid"], Is.EqualTo("category-material"));

            var mergedFromCategory = (IReadOnlyDictionary<string, object>)response.Data["mergedFromCategory"];
            Assert.That(mergedFromCategory["categoryGuid"], Is.EqualTo("category-surface"));
            Assert.That(mergedFromCategory["displayName"], Is.EqualTo("Surface Inputs"));
            Assert.That(mergedFromCategory["previousChildCount"], Is.EqualTo(2));

            var mergedIntoCategory = (IReadOnlyDictionary<string, object>)response.Data["mergedIntoCategory"];
            Assert.That(mergedIntoCategory["categoryGuid"], Is.EqualTo("category-material"));
            Assert.That(mergedIntoCategory["displayName"], Is.EqualTo("Material Inputs"));
            Assert.That(mergedIntoCategory["childCount"], Is.EqualTo(2));
        }

        [Test]
        public void Ok_PreservesDuplicatedCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "duplicate category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["categoryCount"] = 3,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs", "Surface Inputs Copy" },
                    ["duplicatedPropertyCount"] = 2,
                    ["categoryPropertyOrder"] = new[] { "Tint Copy [Color]", "Exposure Copy [Float/Vector1]" },
                    ["duplicationStrategy"] = new[]
                    {
                        "Creates a new category with the requested displayName or appends 'Copy' to the source category displayName.",
                        "Duplicates each source category property via GraphData.AddCopyOfShaderInput(source, -1).",
                        "Duplicated category properties receive 'Copy' display names and are appended to the duplicated category order.",
                    },
                    ["duplicatedFromCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface",
                        ["displayName"] = "Surface Inputs",
                        ["childCount"] = 2,
                    },
                    ["duplicatedCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface-copy",
                        ["displayName"] = "Surface Inputs Copy",
                        ["childCount"] = 2,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(3));
            Assert.That(response.Data["duplicatedPropertyCount"], Is.EqualTo(2));

            var duplicatedFromCategory = (IReadOnlyDictionary<string, object>)response.Data["duplicatedFromCategory"];
            Assert.That(duplicatedFromCategory["categoryGuid"], Is.EqualTo("category-surface"));
            Assert.That(duplicatedFromCategory["displayName"], Is.EqualTo("Surface Inputs"));

            var duplicatedCategory = (IReadOnlyDictionary<string, object>)response.Data["duplicatedCategory"];
            Assert.That(duplicatedCategory["categoryGuid"], Is.EqualTo("category-surface-copy"));
            Assert.That(duplicatedCategory["displayName"], Is.EqualTo("Surface Inputs Copy"));

            var categoryPropertyOrder = (string[])response.Data["categoryPropertyOrder"];
            Assert.That(categoryPropertyOrder[0], Does.Contain("Tint Copy"));
            Assert.That(categoryPropertyOrder[1], Does.Contain("Exposure Copy"));
        }

        [Test]
        public void Ok_PreservesSplitCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "split category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["movedPropertyCount"] = 2,
                    ["categoryCount"] = 3,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs", "Surface Inputs Primary" },
                    ["sourceCategoryPreviousChildCount"] = 3,
                    ["sourceCategoryPropertyOrder"] = new[] { "Metallic [Float/Vector1]" },
                    ["targetCategoryPropertyOrder"] = new[] { "Tint [Color]", "Exposure [Float/Vector1]" },
                    ["splitFromCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface",
                        ["displayName"] = "Surface Inputs",
                        ["childCount"] = 1,
                    },
                    ["splitIntoCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface-primary",
                        ["displayName"] = "Surface Inputs Primary",
                        ["childCount"] = 2,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["movedPropertyCount"], Is.EqualTo(2));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(3));
            Assert.That(response.Data["sourceCategoryPreviousChildCount"], Is.EqualTo(3));

            var splitFromCategory = (IReadOnlyDictionary<string, object>)response.Data["splitFromCategory"];
            Assert.That(splitFromCategory["categoryGuid"], Is.EqualTo("category-surface"));
            Assert.That(splitFromCategory["displayName"], Is.EqualTo("Surface Inputs"));

            var splitIntoCategory = (IReadOnlyDictionary<string, object>)response.Data["splitIntoCategory"];
            Assert.That(splitIntoCategory["categoryGuid"], Is.EqualTo("category-surface-primary"));
            Assert.That(splitIntoCategory["displayName"], Is.EqualTo("Surface Inputs Primary"));

            var sourceCategoryPropertyOrder = (string[])response.Data["sourceCategoryPropertyOrder"];
            Assert.That(sourceCategoryPropertyOrder[0], Does.Contain("Metallic"));

            var targetCategoryPropertyOrder = (string[])response.Data["targetCategoryPropertyOrder"];
            Assert.That(targetCategoryPropertyOrder[0], Does.Contain("Tint"));
            Assert.That(targetCategoryPropertyOrder[1], Does.Contain("Exposure"));
        }

        [Test]
        public void Ok_PreservesMovedPropertyToCategoryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "move property to category ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["previousCategoryGuid"] = "category-default",
                    ["categoryGuid"] = "category-surface",
                    ["previousIndex"] = 1,
                    ["newIndex"] = 0,
                    ["previousGraphInputIndex"] = 1,
                    ["graphInputIndex"] = 1,
                    ["categoryCount"] = 2,
                    ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs" },
                    ["categoryPropertyOrder"] = new[] { "Tint [Color]" },
                    ["targetCategory"] = new Dictionary<string, object>
                    {
                        ["categoryGuid"] = "category-surface",
                        ["displayName"] = "Surface Inputs",
                        ["childCount"] = 1,
                    },
                    ["movedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Tint",
                        ["referenceName"] = "_Tint",
                        ["categoryGuid"] = "category-surface",
                        ["resolvedPropertyType"] = "Color",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["previousCategoryGuid"], Is.EqualTo("category-default"));
            Assert.That(response.Data["categoryGuid"], Is.EqualTo("category-surface"));
            Assert.That(response.Data["newIndex"], Is.EqualTo(0));

            var movedProperty = (IReadOnlyDictionary<string, object>)response.Data["movedProperty"];
            Assert.That(movedProperty["displayName"], Is.EqualTo("Tint"));
            Assert.That(movedProperty["categoryGuid"], Is.EqualTo("category-surface"));
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
                    ["placement"] = new Dictionary<string, object>
                    {
                        ["mode"] = "suggested",
                        ["resolvedPosition"] = new Dictionary<string, object>
                        {
                            ["x"] = -620f,
                            ["y"] = 140f,
                        },
                    },
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

            var placement = (IReadOnlyDictionary<string, object>)response.Data["placement"];
            Assert.That(placement["mode"], Is.EqualTo("suggested"));

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
        public void Ok_PreservesSupportedPropertyCatalogEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "supported property catalog ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedPropertyTypes"] = new[] { "Color", "Float/Vector1" },
                    ["supportedPropertyCount"] = 2,
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["supportedPropertyCount"], Is.EqualTo(2));
            Assert.That((string[])response.Data["supportedPropertyTypes"], Is.EquivalentTo(new[] { "Color", "Float/Vector1" }));
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
        public void Ok_PreservesReorderedPropertyEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "reorder property ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["propertyName"] = "Tint",
                        ["index"] = 0,
                    },
                    ["previousIndex"] = 1,
                    ["newIndex"] = 0,
                    ["previousGraphInputIndex"] = 1,
                    ["graphInputIndex"] = 1,
                    ["categoryGuid"] = "category-default",
                    ["categoryPropertyOrder"] = new[]
                    {
                        "Tint [Color] = _BaseTint",
                        "Exposure [Float/Vector1] = _Exposure",
                    },
                    ["reorderSemantics"] = new[]
                    {
                        "Index is 0-based within the resolved blackboard category.",
                        "graphInputIndex may differ from category-local order.",
                    },
                    ["reorderedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = "Tint",
                        ["referenceName"] = "_BaseTint",
                        ["resolvedPropertyType"] = "Color",
                        ["resolvedShaderInputType"] = "UnityEditor.ShaderGraph.Internal.ColorShaderProperty",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));
            Assert.That(response.Data["previousIndex"], Is.EqualTo(1));
            Assert.That(response.Data["newIndex"], Is.EqualTo(0));
            Assert.That(response.Data["categoryGuid"], Is.EqualTo("category-default"));

            var categoryPropertyOrder = (string[])response.Data["categoryPropertyOrder"];
            Assert.That(categoryPropertyOrder[0], Does.Contain("Tint"));

            var reorderedProperty = (IReadOnlyDictionary<string, object>)response.Data["reorderedProperty"];
            Assert.That(reorderedProperty["displayName"], Is.EqualTo("Tint"));
            Assert.That(reorderedProperty["resolvedPropertyType"], Is.EqualTo("Color"));
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
                    ["placement"] = new Dictionary<string, object>
                    {
                        ["mode"] = "relative",
                        ["direction"] = "down",
                        ["spacing"] = 200f,
                        ["resolvedPosition"] = new Dictionary<string, object>
                        {
                            ["x"] = -420f,
                            ["y"] = 180f,
                        },
                        ["anchorQuery"] = new Dictionary<string, object>
                        {
                            ["displayName"] = "Anchor Source",
                        },
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

            var placement = (IReadOnlyDictionary<string, object>)response.Data["placement"];
            Assert.That(placement["mode"], Is.EqualTo("relative"));
            Assert.That(placement["direction"], Is.EqualTo("down"));

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
        public void Ok_PreservesFindConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "find connection ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["matchCount"] = 1,
                    ["query"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-node",
                        ["inputPort"] = "X",
                    },
                    ["foundConnection"] = new Dictionary<string, object>
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

            var foundConnection = (IReadOnlyDictionary<string, object>)response.Data["foundConnection"];
            Assert.That(foundConnection["outputNodeId"], Is.EqualTo("source-node"));
            Assert.That(foundConnection["inputNodeId"], Is.EqualTo("target-node"));
            Assert.That(foundConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(foundConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesReconnectConnectionEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "reconnect connection ready",
                new Dictionary<string, object>
                {
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["supportedConnectionRules"] = CurrentSupportedConnectionRules,
                    ["matchCount"] = 1,
                    ["previousConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-a",
                        ["inputPort"] = "X",
                    },
                    ["requestedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-b",
                        ["inputPort"] = "X",
                    },
                    ["removedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-a",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                    },
                    ["connectedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "source-node",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "target-b",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(response.Data["matchCount"], Is.EqualTo(1));

            var previousConnection = (IReadOnlyDictionary<string, object>)response.Data["previousConnection"];
            Assert.That(previousConnection["inputNodeId"], Is.EqualTo("target-a"));

            var requestedConnection = (IReadOnlyDictionary<string, object>)response.Data["requestedConnection"];
            Assert.That(requestedConnection["inputNodeId"], Is.EqualTo("target-b"));

            var connectedConnection = (IReadOnlyDictionary<string, object>)response.Data["connectedConnection"];
            Assert.That(connectedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(connectedConnection["inputSlotId"], Is.EqualTo(1));
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
                1,
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
        public void Ok_PreservesPackageBackedCreateSubGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Created blank package-backed Shader Sub Graph at 'Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "create_subgraph",
                    ["assetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["template"] = "blank",
                    ["isSubGraph"] = true,
                    ["supportedCreateTemplates"] = new[] { "blank" },
                    ["createdSubGraph"] = new Dictionary<string, object>
                    {
                        ["name"] = "BlankSubGraph",
                        ["requestedTemplate"] = "blank",
                        ["resolvedTemplate"] = "blank",
                        ["graphPathLabel"] = "Sub Graphs",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("create_subgraph"));
            Assert.That(response.Data["template"], Is.EqualTo("blank"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var createdSubGraph = (IReadOnlyDictionary<string, object>)response.Data["createdSubGraph"];
            Assert.That(createdSubGraph["name"], Is.EqualTo("BlankSubGraph"));
            Assert.That(createdSubGraph["requestedTemplate"], Is.EqualTo("blank"));
            Assert.That(createdSubGraph["resolvedTemplate"], Is.EqualTo("blank"));
            Assert.That(createdSubGraph["graphPathLabel"], Is.EqualTo("Sub Graphs"));
        }

        [Test]
        public void Ok_PreservesPackageBackedRenameGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Renamed Shader Graph 'Blank' to 'Renamed Blank' at 'Assets/ShaderGraphs/Renamed Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "rename_graph",
                    ["assetPath"] = "Assets/ShaderGraphs/Renamed Blank.shadergraph",
                    ["previousAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Renamed Blank.shadergraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Renamed Blank",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.RenameAsset(...) invoked successfully." },
                    ["preview"] = new[] { "renamed graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["renameGraphSemantics"] = new[]
                    {
                        "rename_graph renames the current .shadergraph asset in-place within its existing folder.",
                        "The response assetPath always points at the renamed asset path.",
                        "Package-backed graph rename is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["renamedGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderGraphs/Renamed Blank.shadergraph",
                        ["assetName"] = "Renamed Blank",
                        ["displayName"] = "Renamed Blank",
                        ["name"] = "Renamed Blank",
                        ["requestedName"] = "Renamed Blank",
                        ["previousAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                        ["previousAssetName"] = "Blank",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("rename_graph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Renamed Blank.shadergraph"));
            Assert.That(response.Data["previousAssetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));

            var renamedGraph = (IReadOnlyDictionary<string, object>)response.Data["renamedGraph"];
            Assert.That(renamedGraph["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Renamed Blank.shadergraph"));
            Assert.That(renamedGraph["assetName"], Is.EqualTo("Renamed Blank"));
            Assert.That(renamedGraph["requestedName"], Is.EqualTo("Renamed Blank"));
            Assert.That(renamedGraph["previousAssetName"], Is.EqualTo("Blank"));
        }

        [Test]
        public void Ok_PreservesPackageBackedRenameSubGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Renamed Shader Sub Graph 'BlankSubGraph' to 'Renamed Blank Sub Graph' at 'Assets/ShaderSubGraphs/Renamed Blank Sub Graph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "rename_subgraph",
                    ["assetPath"] = "Assets/ShaderSubGraphs/Renamed Blank Sub Graph.shadersubgraph",
                    ["previousAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["isSubGraph"] = true,
                    ["renameSubGraphSemantics"] = new[]
                    {
                        "rename_subgraph renames the current .shadersubgraph asset in-place within its existing folder.",
                        "The response assetPath always points at the renamed asset path.",
                        "Package-backed sub graph rename is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["renamedSubGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderSubGraphs/Renamed Blank Sub Graph.shadersubgraph",
                        ["assetName"] = "Renamed Blank Sub Graph",
                        ["displayName"] = "Renamed Blank Sub Graph",
                        ["name"] = "Renamed Blank Sub Graph",
                        ["requestedName"] = "Renamed Blank Sub Graph",
                        ["previousAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                        ["previousAssetName"] = "BlankSubGraph",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("rename_subgraph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Renamed Blank Sub Graph.shadersubgraph"));
            Assert.That(response.Data["previousAssetPath"], Is.EqualTo("Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var renamedSubGraph = (IReadOnlyDictionary<string, object>)response.Data["renamedSubGraph"];
            Assert.That(renamedSubGraph["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Renamed Blank Sub Graph.shadersubgraph"));
            Assert.That(renamedSubGraph["assetName"], Is.EqualTo("Renamed Blank Sub Graph"));
            Assert.That(renamedSubGraph["requestedName"], Is.EqualTo("Renamed Blank Sub Graph"));
            Assert.That(renamedSubGraph["previousAssetName"], Is.EqualTo("BlankSubGraph"));
        }

        [Test]
        public void Ok_PreservesPackageBackedDuplicateGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Duplicated Shader Graph 'Blank' to 'Copied Blank' at 'Assets/ShaderGraphs/Copied Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "duplicate_graph",
                    ["assetPath"] = "Assets/ShaderGraphs/Copied Blank.shadergraph",
                    ["sourceAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Copied Blank.shadergraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Copied Blank",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.CopyAsset(...) invoked successfully." },
                    ["preview"] = new[] { "duplicated graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["duplicateGraphSemantics"] = new[]
                    {
                        "duplicate_graph copies the current .shadergraph asset into a new asset within its existing folder.",
                        "The response assetPath always points at the duplicated asset path while sourceAssetPath keeps the original.",
                        "Package-backed graph duplicate is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["duplicatedGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderGraphs/Copied Blank.shadergraph",
                        ["assetName"] = "Copied Blank",
                        ["displayName"] = "Copied Blank",
                        ["name"] = "Copied Blank",
                        ["requestedName"] = "Copied Blank",
                        ["sourceAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                        ["sourceAssetName"] = "Blank",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("duplicate_graph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Copied Blank.shadergraph"));
            Assert.That(response.Data["sourceAssetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));

            var duplicatedGraph = (IReadOnlyDictionary<string, object>)response.Data["duplicatedGraph"];
            Assert.That(duplicatedGraph["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Copied Blank.shadergraph"));
            Assert.That(duplicatedGraph["assetName"], Is.EqualTo("Copied Blank"));
            Assert.That(duplicatedGraph["requestedName"], Is.EqualTo("Copied Blank"));
            Assert.That(duplicatedGraph["sourceAssetName"], Is.EqualTo("Blank"));
        }

        [Test]
        public void Ok_PreservesPackageBackedDuplicateSubGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Duplicated Shader Sub Graph 'BlankSubGraph' to 'Copied Blank Sub Graph' at 'Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "duplicate_subgraph",
                    ["assetPath"] = "Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph",
                    ["sourceAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Copied Blank Sub Graph",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 1,
                    ["connectionCount"] = 0,
                    ["categoryCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new[] { "Output (node-1) [SubGraphOutputNode] @ (0, 0)" },
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.CopyAsset(...) invoked successfully." },
                    ["preview"] = new[] { "duplicated sub graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["isSubGraph"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["duplicateSubGraphSemantics"] = new[]
                    {
                        "duplicate_subgraph copies the current .shadersubgraph asset into a new asset within its existing folder.",
                        "The response assetPath always points at the duplicated asset path while sourceAssetPath keeps the original.",
                        "Package-backed sub graph duplicate is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["duplicatedSubGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph",
                        ["assetName"] = "Copied Blank Sub Graph",
                        ["displayName"] = "Copied Blank Sub Graph",
                        ["name"] = "Copied Blank Sub Graph",
                        ["requestedName"] = "Copied Blank Sub Graph",
                        ["sourceAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                        ["sourceAssetName"] = "BlankSubGraph",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("duplicate_subgraph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph"));
            Assert.That(response.Data["sourceAssetPath"], Is.EqualTo("Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var duplicatedSubGraph = (IReadOnlyDictionary<string, object>)response.Data["duplicatedSubGraph"];
            Assert.That(duplicatedSubGraph["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Copied Blank Sub Graph.shadersubgraph"));
            Assert.That(duplicatedSubGraph["assetName"], Is.EqualTo("Copied Blank Sub Graph"));
            Assert.That(duplicatedSubGraph["requestedName"], Is.EqualTo("Copied Blank Sub Graph"));
            Assert.That(duplicatedSubGraph["sourceAssetName"], Is.EqualTo("BlankSubGraph"));
        }

        [Test]
        public void Ok_PreservesPackageBackedDeleteGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Deleted Shader Graph 'Blank' at 'Assets/ShaderGraphs/Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "delete_graph",
                    ["assetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Blank.shadergraph",
                    ["exists"] = false,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Blank",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.DeleteAsset(...) invoked successfully." },
                    ["preview"] = new[] { "deleted graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["deleteGraphSemantics"] = new[]
                    {
                        "delete_graph removes the current .shadergraph asset at its existing path.",
                        "The response assetPath continues to point at the deleted asset path and exists is false.",
                        "Package-backed graph delete is followed by synchronous refresh so Unity no longer resolves the deleted asset.",
                    },
                    ["deletedGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                        ["assetName"] = "Blank",
                        ["displayName"] = "Blank",
                        ["name"] = "Blank",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("delete_graph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(response.Data["exists"], Is.False);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));

            var deletedGraph = (IReadOnlyDictionary<string, object>)response.Data["deletedGraph"];
            Assert.That(deletedGraph["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(deletedGraph["assetName"], Is.EqualTo("Blank"));
        }

        [Test]
        public void Ok_PreservesPackageBackedDeleteSubGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Deleted Shader Sub Graph 'BlankSubGraph' at 'Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "delete_subgraph",
                    ["assetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["exists"] = false,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "BlankSubGraph",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 1,
                    ["connectionCount"] = 0,
                    ["categoryCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new[] { "Output (node-1) [SubGraphOutputNode] @ (0, 0)" },
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.DeleteAsset(...) invoked successfully." },
                    ["preview"] = new[] { "deleted sub graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["isSubGraph"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["deleteSubGraphSemantics"] = new[]
                    {
                        "delete_subgraph removes the current .shadersubgraph asset at its existing path.",
                        "The response assetPath continues to point at the deleted asset path and exists is false.",
                        "Package-backed sub graph delete is followed by synchronous refresh so Unity no longer resolves the deleted asset.",
                    },
                    ["deletedSubGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                        ["assetName"] = "BlankSubGraph",
                        ["displayName"] = "BlankSubGraph",
                        ["name"] = "BlankSubGraph",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("delete_subgraph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph"));
            Assert.That(response.Data["exists"], Is.False);
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var deletedSubGraph = (IReadOnlyDictionary<string, object>)response.Data["deletedSubGraph"];
            Assert.That(deletedSubGraph["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph"));
            Assert.That(deletedSubGraph["assetName"], Is.EqualTo("BlankSubGraph"));
        }

        [Test]
        public void Ok_PreservesPackageBackedMoveGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Moved Shader Graph 'Blank' to 'Assets/ShaderGraphs/Moved/Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "move_graph",
                    ["assetPath"] = "Assets/ShaderGraphs/Moved/Blank.shadergraph",
                    ["previousAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Moved/Blank.shadergraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Blank",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.MoveAsset(...) invoked successfully." },
                    ["preview"] = new[] { "moved graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["moveGraphSemantics"] = new[]
                    {
                        "move_graph moves the current .shadergraph asset to the exact target asset path, including folder changes.",
                        "The response assetPath always points at the moved asset path.",
                        "Package-backed graph move is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["movedGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderGraphs/Moved/Blank.shadergraph",
                        ["assetName"] = "Blank",
                        ["displayName"] = "Blank",
                        ["name"] = "Blank",
                        ["previousAssetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                        ["previousAssetName"] = "Blank",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("move_graph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Moved/Blank.shadergraph"));
            Assert.That(response.Data["previousAssetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));

            var movedGraph = (IReadOnlyDictionary<string, object>)response.Data["movedGraph"];
            Assert.That(movedGraph["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Moved/Blank.shadergraph"));
            Assert.That(movedGraph["assetName"], Is.EqualTo("Blank"));
            Assert.That(movedGraph["previousAssetName"], Is.EqualTo("Blank"));
        }

        [Test]
        public void Ok_PreservesPackageBackedMoveSubGraphEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Moved Shader Sub Graph 'BlankSubGraph' to 'Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "move_subgraph",
                    ["assetPath"] = "Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph",
                    ["previousAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "BlankSubGraph",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 1,
                    ["connectionCount"] = 0,
                    ["categoryCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new[] { "Output (node-1) [SubGraphOutputNode] @ (0, 0)" },
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "AssetDatabase.MoveAsset(...) invoked successfully." },
                    ["preview"] = new[] { "moved sub graph preview" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["isSubGraph"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["moveSubGraphSemantics"] = new[]
                    {
                        "move_subgraph moves the current .shadersubgraph asset to the exact target asset path, including folder changes.",
                        "The response assetPath always points at the moved asset path.",
                        "Package-backed sub graph move is followed by synchronous import and refresh before the summary is rebuilt.",
                    },
                    ["movedSubGraph"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph",
                        ["assetName"] = "BlankSubGraph",
                        ["displayName"] = "BlankSubGraph",
                        ["name"] = "BlankSubGraph",
                        ["previousAssetPath"] = "Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph",
                        ["previousAssetName"] = "BlankSubGraph",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("move_subgraph"));
            Assert.That(response.Data["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph"));
            Assert.That(response.Data["previousAssetPath"], Is.EqualTo("Assets/ShaderSubGraphs/BlankSubGraph.shadersubgraph"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var movedSubGraph = (IReadOnlyDictionary<string, object>)response.Data["movedSubGraph"];
            Assert.That(movedSubGraph["assetPath"], Is.EqualTo("Assets/ShaderSubGraphs/Moved/BlankSubGraph.shadersubgraph"));
            Assert.That(movedSubGraph["assetName"], Is.EqualTo("BlankSubGraph"));
            Assert.That(movedSubGraph["previousAssetName"], Is.EqualTo("BlankSubGraph"));
        }

        [Test]
        public void Ok_PreservesPackageBackedSetGraphMetadataEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Updated Shader Graph metadata for 'Assets/ShaderGraphs/Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "set_graph_metadata",
                    ["assetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["manifestPath"] = string.Empty,
                    ["absolutePath"] = "/Users/song/Projects/unity-shader-graph-mcp/Assets/ShaderGraphs/Blank.shadergraph",
                    ["exists"] = true,
                    ["hasManifest"] = false,
                    ["schema"] = "unity-shader-graph-mcp/package-backed-v1",
                    ["assetName"] = "Blank",
                    ["template"] = "blank",
                    ["createdUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["updatedUtc"] = "2026-03-19T00:00:00.0000000Z",
                    ["graphPathLabel"] = "Material Inputs",
                    ["graphDefaultPrecision"] = "Half",
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 0,
                    ["connectionCount"] = 0,
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["properties"] = new string[0],
                    ["nodes"] = new string[0],
                    ["connections"] = new string[0],
                    ["notes"] = new[] { "GraphData.SetGraphDefaultPrecision('Half') invoked successfully." },
                    ["preview"] = new[] { "graphDefaultPrecision=Half" },
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["packageDetected"] = true,
                    ["compatibility"] = new Dictionary<string, object>
                    {
                        ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                        ["packageDetected"] = true,
                        ["notes"] = new[] { "package present" },
                    },
                    ["updatedMetadata"] = new Dictionary<string, object>
                    {
                        ["graphPathLabel"] = "Material Inputs",
                        ["graphDefaultPrecision"] = "Half",
                        ["previousGraphPathLabel"] = "Shader Graphs",
                        ["previousGraphDefaultPrecision"] = "Single",
                    },
                    ["metadataSemantics"] = new[]
                    {
                        "set_graph_metadata updates GraphData.path and graphDefaultPrecision when provided.",
                        "Shader graphs accept Single or Half precision; Graph/Switchable precision is valid only for sub graphs.",
                        "The response includes the rebuilt graph summary after synchronous save and import.",
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("set_graph_metadata"));
            Assert.That(response.Data["graphPathLabel"], Is.EqualTo("Material Inputs"));
            Assert.That(response.Data["graphDefaultPrecision"], Is.EqualTo("Half"));

            var updatedMetadata = (IReadOnlyDictionary<string, object>)response.Data["updatedMetadata"];
            Assert.That(updatedMetadata["graphPathLabel"], Is.EqualTo("Material Inputs"));
            Assert.That(updatedMetadata["graphDefaultPrecision"], Is.EqualTo("Half"));
            Assert.That(updatedMetadata["previousGraphPathLabel"], Is.EqualTo("Shader Graphs"));
            Assert.That(updatedMetadata["previousGraphDefaultPrecision"], Is.EqualTo("Single"));
        }

        [Test]
        public void Ok_PreservesPackageBackedExportGraphContractEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Exported package-backed Shader Graph contract from 'Assets/ShaderGraphs/Blank.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "export_graph_contract",
                    ["assetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["contractVersion"] = "unity-shader-graph-mcp/export-graph-contract-v1",
                    ["exportGraphContractSemantics"] = new[]
                    {
                        "exportedGraphContract is read-only structured output for external tooling.",
                    },
                    ["exportedGraphContract"] = new Dictionary<string, object>
                    {
                        ["contractVersion"] = "unity-shader-graph-mcp/export-graph-contract-v1",
                        ["assetPath"] = "Assets/ShaderGraphs/Blank.shadergraph",
                        ["categoryCount"] = 2,
                        ["propertyCount"] = 1,
                        ["nodeCount"] = 1,
                        ["connectionCount"] = 0,
                        ["categoryOrder"] = new[] { "(Default Category)", "Surface Inputs" },
                        ["categories"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["categoryGuid"] = "category-default",
                                ["displayName"] = "(Default Category)",
                            },
                            new Dictionary<string, object>
                            {
                                ["categoryGuid"] = "category-surface",
                                ["displayName"] = "Surface Inputs",
                            },
                        },
                        ["properties"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["displayName"] = "Tint",
                                ["referenceName"] = "_Tint",
                                ["categoryGuid"] = "category-surface",
                            },
                        },
                        ["nodes"] = new object[]
                        {
                            new Dictionary<string, object>
                            {
                                ["objectId"] = "node-1",
                                ["displayName"] = "Contract Source",
                            },
                        },
                        ["connections"] = new object[0],
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("export_graph_contract"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["contractVersion"], Is.EqualTo("unity-shader-graph-mcp/export-graph-contract-v1"));

            var exportedGraphContract = (IReadOnlyDictionary<string, object>)response.Data["exportedGraphContract"];
            Assert.That(exportedGraphContract["assetPath"], Is.EqualTo("Assets/ShaderGraphs/Blank.shadergraph"));
            Assert.That(exportedGraphContract["categoryCount"], Is.EqualTo(2));
            Assert.That(exportedGraphContract["propertyCount"], Is.EqualTo(1));
            Assert.That(exportedGraphContract["nodeCount"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesPackageBackedImportGraphContractEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Imported Shader Graph contract into 'Assets/ShaderGraphs/Imported.shadergraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "import_graph_contract",
                    ["assetPath"] = "Assets/ShaderGraphs/Imported.shadergraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["contractVersion"] = "unity-shader-graph-mcp/export-graph-contract-v1",
                    ["importGraphContractSemantics"] = new[]
                    {
                        "import_graph_contract replays an exportedGraphContract payload into the target graph asset.",
                    },
                    ["importedCounts"] = new Dictionary<string, object>
                    {
                        ["categoryCount"] = 2,
                        ["propertyCount"] = 1,
                        ["nodeCount"] = 2,
                        ["connectionCount"] = 1,
                    },
                    ["nodeIdMap"] = new object[]
                    {
                        new Dictionary<string, object>
                        {
                            ["sourceNodeId"] = "node-1",
                            ["importedNodeId"] = "node-99",
                        },
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("import_graph_contract"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["contractVersion"], Is.EqualTo("unity-shader-graph-mcp/export-graph-contract-v1"));

            var importedCounts = (IReadOnlyDictionary<string, object>)response.Data["importedCounts"];
            Assert.That(importedCounts["categoryCount"], Is.EqualTo(2));
            Assert.That(importedCounts["propertyCount"], Is.EqualTo(1));
            Assert.That(importedCounts["nodeCount"], Is.EqualTo(2));
            Assert.That(importedCounts["connectionCount"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesPackageBackedExportSubGraphContractEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Exported package-backed Shader Sub Graph contract from 'Assets/ShaderSubGraphs/Example.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "export_graph_contract",
                    ["assetPath"] = "Assets/ShaderSubGraphs/Example.shadersubgraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["contractVersion"] = "unity-shader-graph-mcp/export-graph-contract-v1",
                    ["isSubGraph"] = true,
                    ["exportedGraphContract"] = new Dictionary<string, object>
                    {
                        ["assetPath"] = "Assets/ShaderSubGraphs/Example.shadersubgraph",
                        ["isSubGraph"] = true,
                        ["nodeCount"] = 2,
                        ["connectionCount"] = 1,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("export_graph_contract"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var exportedGraphContract = (IReadOnlyDictionary<string, object>)response.Data["exportedGraphContract"];
            Assert.That(exportedGraphContract["isSubGraph"], Is.EqualTo(true));
            Assert.That(exportedGraphContract["nodeCount"], Is.EqualTo(2));
            Assert.That(exportedGraphContract["connectionCount"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesPackageBackedImportSubGraphContractEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Imported Shader Sub Graph contract into 'Assets/ShaderSubGraphs/Imported.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "import_graph_contract",
                    ["assetPath"] = "Assets/ShaderSubGraphs/Imported.shadersubgraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["contractVersion"] = "unity-shader-graph-mcp/export-graph-contract-v1",
                    ["isSubGraph"] = true,
                    ["importedCounts"] = new Dictionary<string, object>
                    {
                        ["categoryCount"] = 0,
                        ["propertyCount"] = 0,
                        ["nodeCount"] = 2,
                        ["connectionCount"] = 1,
                    },
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("import_graph_contract"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));

            var importedCounts = (IReadOnlyDictionary<string, object>)response.Data["importedCounts"];
            Assert.That(importedCounts["categoryCount"], Is.EqualTo(0));
            Assert.That(importedCounts["propertyCount"], Is.EqualTo(0));
            Assert.That(importedCounts["nodeCount"], Is.EqualTo(2));
            Assert.That(importedCounts["connectionCount"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesPackageBackedReadSubGraphSummaryEnvelope()
        {
            var response = ShaderGraphResponse.Ok(
                "Loaded package-backed Shader Sub Graph summary from 'Assets/ShaderGraphs/ExampleSubGraph.shadersubgraph'.",
                new Dictionary<string, object>
                {
                    ["operation"] = "read_subgraph_summary",
                    ["assetPath"] = "Assets/ShaderGraphs/ExampleSubGraph.shadersubgraph",
                    ["executionBackendKind"] = ShaderGraphExecutionKind.PackageBacked.ToString(),
                    ["backendKind"] = ShaderGraphBackendKind.PackageReady.ToString(),
                    ["categoryCount"] = 1,
                    ["propertyCount"] = 0,
                    ["nodeCount"] = 1,
                    ["connectionCount"] = 0,
                    ["isSubGraph"] = true,
                });

            Assert.That(response.Success, Is.True);
            Assert.That(response.Data["operation"], Is.EqualTo("read_subgraph_summary"));
            Assert.That(response.Data["executionBackendKind"], Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["categoryCount"], Is.EqualTo(1));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));
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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is SplitNode input slot 0 / In."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesColorFanOutToSplitAndMultiplyConnectionEnvelope()
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
                        ["outputNodeId"] = "color-24",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-25",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-24",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-25",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out is also supported when the same source fans out into SplitNode input slot 0 / In and MultiplyNode input slot 0 / A or 1 / B within one graph."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesColorToNormalBlendConnectionEnvelope()
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
                        ["outputNodeId"] = "color-32",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-blend-33",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "color-32",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.ColorNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-blend-33",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalBlendNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is NormalBlendNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalBlendNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToNormalBlendConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-33",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-blend-34",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-33",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-blend-34",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalBlendNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is NormalBlendNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalBlendNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToNormalStrengthConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-34",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-strength-35",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-34",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-strength-35",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalStrengthNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slot RGBA is supported when the input node is NormalStrengthNode input slot In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalStrengthNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToNormalUnpackConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-36",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-unpack-37",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-36",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "normal-unpack-37",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalUnpackNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slot RGBA is supported when the input node is NormalUnpackNode input slot In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalUnpackNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesVector1ToNormalStrengthStrengthConnectionEnvelope()
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
                        ["outputNodeId"] = "vector1-38",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-strength-39",
                        ["inputPort"] = "Strength",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector1-38",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-strength-39",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalStrengthNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "Strength",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector1Node output slot 0 / Out and scalar arithmetic output slot Out are supported when the input node is NormalStrengthNode input slot Strength or NormalFromTextureNode input slot Offset or Strength."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalStrengthNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesNormalStrengthToSplitConnectionEnvelope()
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
                        ["outputNodeId"] = "normal-strength-40",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-41",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "normal-strength-40",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.NormalStrengthNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "split-41",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("NormalStrengthNode output slot Out is supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalStrengthNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesUvToNormalFromTextureUvConnectionEnvelope()
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
                        ["outputNodeId"] = "uv-42",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-43",
                        ["inputPort"] = "UV",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "uv-42",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.UVNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-43",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalFromTextureNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "UV",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("UVNode output slot Out / UV is supported when the input node is TilingAndOffsetNode input slot UV, SampleTexture2DNode input slot UV, or NormalFromTextureNode input slot UV."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.UVNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalFromTextureNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesTilingAndOffsetToNormalFromTextureUvConnectionEnvelope()
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
                        ["outputNodeId"] = "tiling-44",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-45",
                        ["inputPort"] = "UV",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "tiling-44",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.TilingAndOffsetNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-45",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalFromTextureNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "UV",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("TilingAndOffsetNode output slot Out is supported when the input node is SampleTexture2DNode input slot UV or NormalFromTextureNode input slot UV."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.TilingAndOffsetNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalFromTextureNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesTextureToNormalFromTextureTextureConnectionEnvelope()
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
                        ["outputNodeId"] = "texture-46",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-47",
                        ["inputPort"] = "Texture",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "texture-46",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Texture2DAssetNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-47",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalFromTextureNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "Texture",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Texture2DAssetNode output slot Out / Texture is supported when the input node is SampleTexture2DNode input slot Texture or NormalFromTextureNode input slot Texture."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Texture2DAssetNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalFromTextureNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesVector1ToNormalFromTextureOffsetConnectionEnvelope()
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
                        ["outputNodeId"] = "vector1-48",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-49",
                        ["inputPort"] = "Offset",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector1-48",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-49",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalFromTextureNode",
                        ["inputSlotId"] = 2,
                        ["inputPort"] = "Offset",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector1Node output slot 0 / Out and scalar arithmetic output slot Out are supported when the input node is NormalStrengthNode input slot Strength or NormalFromTextureNode input slot Offset or Strength."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalFromTextureNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(2));
        }

        [Test]
        public void Ok_PreservesVector1ToNormalFromTextureStrengthConnectionEnvelope()
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
                        ["outputNodeId"] = "vector1-50",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-51",
                        ["inputPort"] = "Strength",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector1-50",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-from-texture-51",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalFromTextureNode",
                        ["inputSlotId"] = 3,
                        ["inputPort"] = "Strength",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector1Node output slot 0 / Out and scalar arithmetic output slot Out are supported when the input node is NormalStrengthNode input slot Strength or NormalFromTextureNode input slot Offset or Strength."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalFromTextureNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(3));
        }

        [Test]
        public void Ok_PreservesVector2ToNormalReconstructZConnectionEnvelope()
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
                        ["outputNodeId"] = "vector2-52",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-reconstruct-z-53",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "vector2-52",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Vector2Node",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "normal-reconstruct-z-53",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.NormalReconstructZNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector2Node output slot Out is supported when the input node is NormalReconstructZNode input slot In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector2Node"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.NormalReconstructZNode"));
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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

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
        public void Ok_PreservesUvToSampleTexture2DUvConnectionEnvelope()
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
                        ["outputNodeId"] = "uv-20",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-21",
                        ["inputPort"] = "UV",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "uv-20",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.UVNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-21",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["inputSlotId"] = 2,
                        ["inputPort"] = "UV",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("UVNode output slot Out / UV is supported when the input node is TilingAndOffsetNode input slot UV, SampleTexture2DNode input slot UV, or NormalFromTextureNode input slot UV."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.UVNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(2));
        }

        [Test]
        public void Ok_PreservesTilingAndOffsetToSampleTexture2DUvConnectionEnvelope()
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
                        ["outputNodeId"] = "tiling-22",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-23",
                        ["inputPort"] = "UV",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "tiling-22",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.TilingAndOffsetNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-23",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["inputSlotId"] = 2,
                        ["inputPort"] = "UV",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("TilingAndOffsetNode output slot Out is supported when the input node is SampleTexture2DNode input slot UV or NormalFromTextureNode input slot UV."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.TilingAndOffsetNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(2));
        }

        [Test]
        public void Ok_PreservesTextureToSampleTexture2DTextureConnectionEnvelope()
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
                        ["outputNodeId"] = "texture-24",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-25",
                        ["inputPort"] = "Texture",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "texture-24",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.Texture2DAssetNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "sample-25",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "Texture",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Texture2DAssetNode output slot Out / Texture is supported when the input node is SampleTexture2DNode input slot Texture or NormalFromTextureNode input slot Texture."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Texture2DAssetNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToVector1ConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-26",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector1-27",
                        ["inputPort"] = "X",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-26",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector1-27",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector1Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slots R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X, CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots, ComparisonNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToCombineConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-28",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "combine-29",
                        ["inputPort"] = "R",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-28",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "combine-29",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.CombineNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "R",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slots R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X, CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots, ComparisonNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.CombineNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToVector2ConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-30",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector2-31",
                        ["inputPort"] = "X",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-30",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "vector2-31",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.Vector2Node",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "X",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slots R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X, CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots, ComparisonNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.Vector2Node"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToMultiplyConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-28",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "multiply-29",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-28",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "multiply-29",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToSplitConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-26",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "split-27",
                        ["inputPort"] = "In",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-26",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "split-27",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.SplitNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "In",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("SampleTexture2DNode output slot RGBA is supported when the input node is SplitNode input slot 0 / In."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToComparisonConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-22",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "comparison-23",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-22",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "comparison-23",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.ComparisonNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector1Node, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are supported when the input node is ComparisonNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToBranchConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-24",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "branch-25",
                        ["inputPort"] = "True",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-24",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "branch-25",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "True",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("Vector1Node, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are supported when the input node is BranchNode input slot 1 / True or 2 / False."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DChannelToAppendConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-26",
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "append-27",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-26",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 4,
                        ["outputPort"] = "R",
                        ["inputNodeId"] = "append-27",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(4));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToLerpConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-28",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "lerp-29",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-28",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "lerp-29",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.LerpNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesSampleTexture2DColorToBranchConnectionEnvelope()
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
                        ["outputNodeId"] = "sample-30",
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "branch-31",
                        ["inputPort"] = "True",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "sample-30",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.SampleTexture2DNode",
                        ["outputSlotId"] = 0,
                        ["outputPort"] = "RGBA",
                        ["inputNodeId"] = "branch-31",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["inputSlotId"] = 1,
                        ["inputPort"] = "True",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(0));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
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
            Assert.That(supportedConnectionRules, Does.Contain("ComparisonNode output slot 2 / Out is supported when the input node is BranchNode input slot 0 / Predicate, including fan-out into multiple Branch predicates."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is SplitNode input slot 0 / In."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is SplitNode input slot 0 / In."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are supported when the input node is SplitNode input slot 0 / In."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, and SampleTexture2DNode output slot RGBA are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(1));
        }

        [Test]
        public void Ok_PreservesAppendFanOutToSplitAndMultiplyConnectionEnvelope()
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
                        ["outputNodeId"] = "append-57",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-58",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "append-57",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.AppendVectorNode",
                        ["outputSlotId"] = 2,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "multiply-58",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.MultiplyNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("AppendVectorNode output slot Out is also supported when the same source fans out into SplitNode input slot 0 / In and MultiplyNode input slot 0 / A or 1 / B within one graph."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(2));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

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
            Assert.That(supportedConnectionRules, Does.Contain("ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, SampleTexture2DNode output slot RGBA, Vector1Node output slot 0 / Out, SplitNode channel outputs, scalar arithmetic output slot Out, and SampleTexture2DNode output slots R/G/B/A are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(resolvedConnection["inputSlotId"], Is.EqualTo(0));
        }

        [Test]
        public void Ok_PreservesBranchToAddConnectionEnvelope()
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
                        ["outputNodeId"] = "branch-70",
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "add-71",
                        ["inputPort"] = "A",
                    },
                    ["resolvedConnection"] = new Dictionary<string, object>
                    {
                        ["outputNodeId"] = "branch-70",
                        ["outputNodeType"] = "UnityEditor.ShaderGraph.BranchNode",
                        ["outputSlotId"] = 3,
                        ["outputPort"] = "Out",
                        ["inputNodeId"] = "add-71",
                        ["inputNodeType"] = "UnityEditor.ShaderGraph.AddNode",
                        ["inputSlotId"] = 0,
                        ["inputPort"] = "A",
                        ["connectedEdgeType"] = "UnityEditor.ShaderGraph.Edge",
                    },
                });

            var supportedConnectionRules = (string[])response.Data["supportedConnectionRules"];
            Assert.That(supportedConnectionRules, Has.Length.EqualTo(CurrentSupportedConnectionRules.Length));
            Assert.That(supportedConnectionRules, Does.Contain("BranchNode output slot 3 / Out is supported when the input node is one or more Vector1Node input slot 1 / X or scalar arithmetic input ports."));

            var resolvedConnection = (IReadOnlyDictionary<string, object>)response.Data["resolvedConnection"];
            Assert.That(resolvedConnection["outputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(resolvedConnection["outputSlotId"], Is.EqualTo(3));
            Assert.That(resolvedConnection["inputNodeType"], Is.EqualTo("UnityEditor.ShaderGraph.AddNode"));
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
