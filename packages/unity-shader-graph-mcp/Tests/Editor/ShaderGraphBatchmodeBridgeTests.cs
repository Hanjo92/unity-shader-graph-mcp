using NUnit.Framework;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphBatchmodeBridgeTests
    {
        [Test]
        public void TryParseInvocation_AcceptsSeparateAndEqualsArguments()
        {
            string[] args =
            {
                "/Applications/Unity/Unity.app",
                "-batchmode",
                "-executeMethod",
                "ShaderGraphMcp.Editor.Batchmode.ShaderGraphBatchEntryPoint.Run",
                "-shaderGraphMcpRequestPath",
                "/tmp/shadergraph-request.json",
                "-shaderGraphMcpResponsePath=/tmp/shadergraph-response.json",
            };

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseInvocation(args, out var invocation, out string errorMessage),
                Is.True,
                errorMessage);
            Assert.That(invocation.RequestFilePath, Is.EqualTo("/tmp/shadergraph-request.json"));
            Assert.That(invocation.ResponseFilePath, Is.EqualTo("/tmp/shadergraph-response.json"));
        }

        [Test]
        public void TryParseInvocation_AcceptsLegacyBatchmodeArgumentNames()
        {
            string[] args =
            {
                "/Applications/Unity/Unity.app",
                "--shadergraph-mcp-request-file=/tmp/legacy-request.json",
                "--shadergraph-mcp-response-file",
                "/tmp/legacy-response.json",
            };

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseInvocation(args, out var invocation, out string errorMessage),
                Is.True,
                errorMessage);
            Assert.That(invocation.RequestFilePath, Is.EqualTo("/tmp/legacy-request.json"));
            Assert.That(invocation.ResponseFilePath, Is.EqualTo("/tmp/legacy-response.json"));
        }

        [Test]
        public void TryParseRequest_ReturnsCreateGraphRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""create_graph"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""template"": ""blank""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var createGraphRequest = request as CreateGraphRequest;
            Assert.That(createGraphRequest, Is.Not.Null);
            Assert.That(createGraphRequest.Name, Is.EqualTo("ExampleLitGraph"));
            Assert.That(createGraphRequest.Path, Is.EqualTo("Assets/ShaderGraphs"));
            Assert.That(createGraphRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(createGraphRequest.Template, Is.EqualTo("blank"));
        }

        [Test]
        public void TryParseRequest_ReturnsRenameGraphRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""rename_graph"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""newDisplayName"": ""RenamedLitGraph""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var renameGraphRequest = request as RenameGraphRequest;
            Assert.That(renameGraphRequest, Is.Not.Null);
            Assert.That(renameGraphRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(renameGraphRequest.Name, Is.EqualTo("RenamedLitGraph"));
            Assert.That(renameGraphRequest.TargetAssetPath, Is.EqualTo("Assets/ShaderGraphs/RenamedLitGraph.shadergraph"));
        }

        [Test]
        public void TryParseRequest_ReturnsCreateCategoryRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""create_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""categoryName"": ""Surface Inputs""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var createCategoryRequest = request as CreateCategoryRequest;
            Assert.That(createCategoryRequest, Is.Not.Null);
            Assert.That(createCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(createCategoryRequest.Name, Is.EqualTo("Surface Inputs"));
        }

        [Test]
        public void TryParseRequest_ReturnsRenameCategoryRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""rename_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""categoryName"": ""Surface Inputs"",
                ""newDisplayName"": ""Material Inputs""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var renameCategoryRequest = request as RenameCategoryRequest;
            Assert.That(renameCategoryRequest, Is.Not.Null);
            Assert.That(renameCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(renameCategoryRequest.CategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(renameCategoryRequest.DisplayName, Is.EqualTo("Material Inputs"));
        }

        [Test]
        public void TryParseRequest_ReturnsFindCategoryRequest_FromDisplayNameAlias()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""find_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""displayName"": ""(Default Category)""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var findCategoryRequest = request as FindCategoryRequest;
            Assert.That(findCategoryRequest, Is.Not.Null);
            Assert.That(findCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(findCategoryRequest.CategoryGuid, Is.Empty);
            Assert.That(findCategoryRequest.CategoryName, Is.EqualTo("(Default Category)"));
        }

        [Test]
        public void TryParseRequest_ReturnsDeleteCategoryRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""delete_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""categoryName"": ""Surface Inputs""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var deleteCategoryRequest = request as DeleteCategoryRequest;
            Assert.That(deleteCategoryRequest, Is.Not.Null);
            Assert.That(deleteCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(deleteCategoryRequest.CategoryGuid, Is.Empty);
            Assert.That(deleteCategoryRequest.CategoryName, Is.EqualTo("Surface Inputs"));
        }

        [Test]
        public void TryParseRequest_ReturnsReorderCategoryRequest_WithIndexAliases()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""reorder_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""displayName"": ""Surface Inputs"",
                ""targetIndex"": ""1""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var reorderCategoryRequest = request as ReorderCategoryRequest;
            Assert.That(reorderCategoryRequest, Is.Not.Null);
            Assert.That(reorderCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(reorderCategoryRequest.CategoryGuid, Is.Empty);
            Assert.That(reorderCategoryRequest.CategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(reorderCategoryRequest.Index, Is.EqualTo(1));
        }

        [Test]
        public void TryParseRequest_ReturnsListCategoriesRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""list_categories"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var listCategoriesRequest = request as ListCategoriesRequest;
            Assert.That(listCategoriesRequest, Is.Not.Null);
            Assert.That(listCategoriesRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
        }

        [Test]
        public void TryParseRequest_ReturnsMergeCategoryRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""merge_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceDisplayName"": ""Surface Inputs"",
                ""targetName"": ""Material Inputs""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var mergeCategoryRequest = request as MergeCategoryRequest;
            Assert.That(mergeCategoryRequest, Is.Not.Null);
            Assert.That(mergeCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(mergeCategoryRequest.SourceCategoryGuid, Is.Empty);
            Assert.That(mergeCategoryRequest.SourceCategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(mergeCategoryRequest.TargetCategoryGuid, Is.Empty);
            Assert.That(mergeCategoryRequest.TargetCategoryName, Is.EqualTo("Material Inputs"));
        }

        [Test]
        public void TryParseRequest_ReturnsDuplicateCategoryRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""duplicate_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceDisplayName"": ""Surface Inputs"",
                ""newDisplayName"": ""Surface Inputs Copy""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var duplicateCategoryRequest = request as DuplicateCategoryRequest;
            Assert.That(duplicateCategoryRequest, Is.Not.Null);
            Assert.That(duplicateCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(duplicateCategoryRequest.CategoryGuid, Is.Empty);
            Assert.That(duplicateCategoryRequest.CategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(duplicateCategoryRequest.DisplayName, Is.EqualTo("Surface Inputs Copy"));
        }

        [Test]
        public void TryParseRequest_ReturnsSplitCategoryRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""split_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceDisplayName"": ""Surface Inputs"",
                ""newDisplayName"": ""Surface Inputs Primary"",
                ""propertyNames"": [""Tint"", ""Exposure""]
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var splitCategoryRequest = request as SplitCategoryRequest;
            Assert.That(splitCategoryRequest, Is.Not.Null);
            Assert.That(splitCategoryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(splitCategoryRequest.SourceCategoryGuid, Is.Empty);
            Assert.That(splitCategoryRequest.SourceCategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(splitCategoryRequest.DisplayName, Is.EqualTo("Surface Inputs Primary"));
            Assert.That(splitCategoryRequest.PropertyNames, Is.EquivalentTo(new[] { "Tint", "Exposure" }));
        }

        [Test]
        public void TryParseRequest_ReturnsMovePropertyToCategoryRequest_WithAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""move_property_to_category"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint"",
                ""categoryName"": ""Surface Inputs"",
                ""targetIndex"": ""1""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var moveRequest = request as MovePropertyToCategoryRequest;
            Assert.That(moveRequest, Is.Not.Null);
            Assert.That(moveRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(moveRequest.PropertyName, Is.EqualTo("Tint"));
            Assert.That(moveRequest.CategoryName, Is.EqualTo("Surface Inputs"));
            Assert.That(moveRequest.Index, Is.EqualTo(1));
        }

        [Test]
        public void TryParseRequest_ReturnsReadGraphSummaryRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""read_graph_summary"",
                ""path"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var summaryRequest = request as ReadGraphSummaryRequest;
            Assert.That(summaryRequest, Is.Not.Null);
            Assert.That(summaryRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
        }

        [Test]
        public void TryParseRequest_ReturnsFindNodeRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""find_node"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""objectId"": ""node-17"",
                ""nodeType"": ""Vector1"",
                ""displayName"": ""Source""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var findNodeRequest = request as FindNodeRequest;
            Assert.That(findNodeRequest, Is.Not.Null);
            Assert.That(findNodeRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(findNodeRequest.NodeId, Is.EqualTo("node-17"));
            Assert.That(findNodeRequest.NodeType, Is.EqualTo("Vector1"));
            Assert.That(findNodeRequest.DisplayName, Is.EqualTo("Source"));
        }

        [Test]
        public void TryParseRequest_ReturnsFindPropertyRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""find_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""referenceName"": ""_BaseColor"",
                ""propertyType"": ""Color""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var findPropertyRequest = request as FindPropertyRequest;
            Assert.That(findPropertyRequest, Is.Not.Null);
            Assert.That(findPropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(findPropertyRequest.ReferenceName, Is.EqualTo("_BaseColor"));
            Assert.That(findPropertyRequest.PropertyType, Is.EqualTo("Color"));
        }

        [Test]
        public void TryParseRequest_ReturnsListSupportedNodesRequest_WithoutAssetPath()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""list_supported_nodes""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var listSupportedNodesRequest = request as ListSupportedNodesRequest;
            Assert.That(listSupportedNodesRequest, Is.Not.Null);
            Assert.That(listSupportedNodesRequest.AssetPath, Is.Null);
        }

        [Test]
        public void TryParseRequest_ReturnsListSupportedPropertiesRequest_WithoutAssetPath()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""list_supported_properties""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var listSupportedPropertiesRequest = request as ListSupportedPropertiesRequest;
            Assert.That(listSupportedPropertiesRequest, Is.Not.Null);
            Assert.That(listSupportedPropertiesRequest.AssetPath, Is.Null);
        }

        [Test]
        public void TryParseRequest_ReturnsListSupportedConnectionsRequest_WithoutAssetPath()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""list_supported_connections""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var listSupportedConnectionsRequest = request as ListSupportedConnectionsRequest;
            Assert.That(listSupportedConnectionsRequest, Is.Not.Null);
            Assert.That(listSupportedConnectionsRequest.AssetPath, Is.Null);
        }

        [Test]
        public void TryParseRequest_ReturnsUpdatePropertyRequest_WithOptionalType()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""update_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint"",
                ""propertyType"": ""Color"",
                ""defaultValue"": ""#FFFFFFFF""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var updatePropertyRequest = request as UpdatePropertyRequest;
            Assert.That(updatePropertyRequest, Is.Not.Null);
            Assert.That(updatePropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(updatePropertyRequest.PropertyName, Is.EqualTo("Tint"));
            Assert.That(updatePropertyRequest.PropertyType, Is.EqualTo("Color"));
            Assert.That(updatePropertyRequest.DefaultValue, Is.EqualTo("#FFFFFFFF"));
        }

        [Test]
        public void TryParseRequest_ReturnsRenamePropertyRequest_WithOptionalReferenceName()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""rename_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint"",
                ""newDisplayName"": ""Base Tint"",
                ""newReferenceName"": ""_BaseTint""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var renamePropertyRequest = request as RenamePropertyRequest;
            Assert.That(renamePropertyRequest, Is.Not.Null);
            Assert.That(renamePropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(renamePropertyRequest.PropertyName, Is.EqualTo("Tint"));
            Assert.That(renamePropertyRequest.DisplayName, Is.EqualTo("Base Tint"));
            Assert.That(renamePropertyRequest.ReferenceName, Is.EqualTo("_BaseTint"));
        }

        [Test]
        public void TryParseRequest_ReturnsDuplicatePropertyRequest_WithOptionalReferenceName()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""duplicate_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint"",
                ""newDisplayName"": ""Copied Tint"",
                ""newReferenceName"": ""_CopiedTint""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var duplicatePropertyRequest = request as DuplicatePropertyRequest;
            Assert.That(duplicatePropertyRequest, Is.Not.Null);
            Assert.That(duplicatePropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(duplicatePropertyRequest.PropertyName, Is.EqualTo("Tint"));
            Assert.That(duplicatePropertyRequest.DisplayName, Is.EqualTo("Copied Tint"));
            Assert.That(duplicatePropertyRequest.ReferenceName, Is.EqualTo("_CopiedTint"));
        }

        [Test]
        public void TryParseRequest_ReturnsReorderPropertyRequest_WithIndexAliases()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""reorder_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint"",
                ""newIndex"": ""0""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var reorderPropertyRequest = request as ReorderPropertyRequest;
            Assert.That(reorderPropertyRequest, Is.Not.Null);
            Assert.That(reorderPropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(reorderPropertyRequest.PropertyName, Is.EqualTo("Tint"));
            Assert.That(reorderPropertyRequest.Index, Is.EqualTo(0));
        }

        [Test]
        public void TryParseRequest_ReturnsRenameNodeRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""rename_node"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""objectId"": ""node-17"",
                ""newDisplayName"": ""Renamed Source""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var renameNodeRequest = request as RenameNodeRequest;
            Assert.That(renameNodeRequest, Is.Not.Null);
            Assert.That(renameNodeRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(renameNodeRequest.NodeId, Is.EqualTo("node-17"));
            Assert.That(renameNodeRequest.DisplayName, Is.EqualTo("Renamed Source"));
        }

        [Test]
        public void TryParseRequest_ReturnsDuplicateNodeRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""duplicate_node"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""objectId"": ""node-17"",
                ""newDisplayName"": ""Copied Source""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var duplicateNodeRequest = request as DuplicateNodeRequest;
            Assert.That(duplicateNodeRequest, Is.Not.Null);
            Assert.That(duplicateNodeRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(duplicateNodeRequest.NodeId, Is.EqualTo("node-17"));
            Assert.That(duplicateNodeRequest.DisplayName, Is.EqualTo("Copied Source"));
        }

        [Test]
        public void TryParseRequest_ReturnsMoveNodeRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""move_node"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""objectId"": ""node-17"",
                ""x"": ""-420"",
                ""y"": ""180.5""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var moveNodeRequest = request as MoveNodeRequest;
            Assert.That(moveNodeRequest, Is.Not.Null);
            Assert.That(moveNodeRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(moveNodeRequest.NodeId, Is.EqualTo("node-17"));
            Assert.That(moveNodeRequest.X, Is.EqualTo(-420f));
            Assert.That(moveNodeRequest.Y, Is.EqualTo(180.5f));
        }

        [Test]
        public void TryParseRequest_ReturnsDeleteNodeRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""delete_node"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""objectId"": ""node-17""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var deleteNodeRequest = request as DeleteNodeRequest;
            Assert.That(deleteNodeRequest, Is.Not.Null);
            Assert.That(deleteNodeRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(deleteNodeRequest.NodeId, Is.EqualTo("node-17"));
        }

        [Test]
        public void TryParseRequest_ReturnsRemovePropertyRequest_FromAssetPathPayload()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""remove_property"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""propertyName"": ""Tint""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var removePropertyRequest = request as RemovePropertyRequest;
            Assert.That(removePropertyRequest, Is.Not.Null);
            Assert.That(removePropertyRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(removePropertyRequest.PropertyName, Is.EqualTo("Tint"));
        }

        [Test]
        public void TryParseRequest_ReturnsConnectPortsRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""connect_ports"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceNodeId"": ""source-node"",
                ""sourcePort"": ""Out"",
                ""targetNodeId"": ""target-node"",
                ""targetPort"": ""X""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var connectPortsRequest = request as ConnectPortsRequest;
            Assert.That(connectPortsRequest, Is.Not.Null);
            Assert.That(connectPortsRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(connectPortsRequest.OutputNodeId, Is.EqualTo("source-node"));
            Assert.That(connectPortsRequest.OutputPort, Is.EqualTo("Out"));
            Assert.That(connectPortsRequest.InputNodeId, Is.EqualTo("target-node"));
            Assert.That(connectPortsRequest.InputPort, Is.EqualTo("X"));
        }

        [Test]
        public void TryParseRequest_ReturnsRemoveConnectionRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""remove_connection"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceNodeId"": ""source-node"",
                ""sourcePort"": ""Out"",
                ""targetNodeId"": ""target-node"",
                ""targetPort"": ""X""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var removeConnectionRequest = request as RemoveConnectionRequest;
            Assert.That(removeConnectionRequest, Is.Not.Null);
            Assert.That(removeConnectionRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(removeConnectionRequest.OutputNodeId, Is.EqualTo("source-node"));
            Assert.That(removeConnectionRequest.OutputPort, Is.EqualTo("Out"));
            Assert.That(removeConnectionRequest.InputNodeId, Is.EqualTo("target-node"));
            Assert.That(removeConnectionRequest.InputPort, Is.EqualTo("X"));
        }

        [Test]
        public void TryParseRequest_ReturnsFindConnectionRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""find_connection"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""sourceNodeId"": ""source-node"",
                ""sourcePort"": ""Out"",
                ""targetNodeId"": ""target-node"",
                ""targetPort"": ""X""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var findConnectionRequest = request as FindConnectionRequest;
            Assert.That(findConnectionRequest, Is.Not.Null);
            Assert.That(findConnectionRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(findConnectionRequest.OutputNodeId, Is.EqualTo("source-node"));
            Assert.That(findConnectionRequest.OutputPort, Is.EqualTo("Out"));
            Assert.That(findConnectionRequest.InputNodeId, Is.EqualTo("target-node"));
            Assert.That(findConnectionRequest.InputPort, Is.EqualTo("X"));
        }

        [Test]
        public void TryParseRequest_ReturnsReconnectConnectionRequest_FromAliasFields()
        {
            string json = @"{
                ""tool"": ""shadergraph_asset"",
                ""action"": ""reconnect_connection"",
                ""assetPath"": ""Assets/ShaderGraphs/ExampleLitGraph.shadergraph"",
                ""oldSourceNodeId"": ""source-node"",
                ""oldSourcePort"": ""Out"",
                ""oldTargetNodeId"": ""target-a"",
                ""oldTargetPort"": ""X"",
                ""sourceNodeId"": ""source-node"",
                ""sourcePort"": ""Out"",
                ""targetNodeId"": ""target-b"",
                ""targetPort"": ""X""
            }";

            Assert.That(
                ShaderGraphBatchmodeBridge.TryParseRequest(json, out ShaderGraphRequest request, out string errorMessage),
                Is.True,
                errorMessage);

            var reconnectConnectionRequest = request as ReconnectConnectionRequest;
            Assert.That(reconnectConnectionRequest, Is.Not.Null);
            Assert.That(reconnectConnectionRequest.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(reconnectConnectionRequest.OldOutputNodeId, Is.EqualTo("source-node"));
            Assert.That(reconnectConnectionRequest.OldOutputPort, Is.EqualTo("Out"));
            Assert.That(reconnectConnectionRequest.OldInputNodeId, Is.EqualTo("target-a"));
            Assert.That(reconnectConnectionRequest.OldInputPort, Is.EqualTo("X"));
            Assert.That(reconnectConnectionRequest.OutputNodeId, Is.EqualTo("source-node"));
            Assert.That(reconnectConnectionRequest.OutputPort, Is.EqualTo("Out"));
            Assert.That(reconnectConnectionRequest.InputNodeId, Is.EqualTo("target-b"));
            Assert.That(reconnectConnectionRequest.InputPort, Is.EqualTo("X"));
        }

        [Test]
        public void SerializeResponse_WritesStableOrderedJson()
        {
            var response = ShaderGraphResponse.Ok(
                "done",
                new System.Collections.Generic.Dictionary<string, object>
                {
                    ["z"] = 3,
                    ["a"] = new System.Collections.Generic.Dictionary<string, object>
                    {
                        ["b"] = 2,
                        ["a"] = "x",
                    },
                    ["list"] = new object[] { "one", 2, true },
                });

            string json = ShaderGraphBatchmodeBridge.SerializeResponse(response);

            Assert.That(
                json,
                Is.EqualTo("{\"success\":true,\"message\":\"done\",\"data\":{\"a\":{\"a\":\"x\",\"b\":2},\"list\":[\"one\",2,true],\"z\":3}}"));
        }
    }
}
