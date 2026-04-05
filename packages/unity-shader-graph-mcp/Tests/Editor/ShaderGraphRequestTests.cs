using NUnit.Framework;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphRequestTests
    {
        private sealed class UnsupportedShaderGraphRequest : ShaderGraphRequest
        {
            public UnsupportedShaderGraphRequest()
                : base((ShaderGraphAction)999, "Assets/ShaderGraphs/Unsupported.shadergraph")
            {
            }
        }

        [Test]
        public void CreateGraphRequest_AppendsShaderGraphExtension()
        {
            var request = new CreateGraphRequest("ExampleLitGraph", "Assets/ShaderGraphs", "urp_lit");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Name, Is.EqualTo("ExampleLitGraph"));
            Assert.That(request.Path, Is.EqualTo("Assets/ShaderGraphs"));
            Assert.That(request.Template, Is.EqualTo("urp_lit"));
        }

        [Test]
        public void CreateGraphRequest_DefaultsToShaderGraphsFolder()
        {
            var request = new CreateGraphRequest("MyGraph", null, null);

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/MyGraph.shadergraph"));
            Assert.That(request.Path, Is.EqualTo("Assets/ShaderGraphs"));
            Assert.That(request.Template, Is.EqualTo("blank"));
        }

        [Test]
        public void CreateSubGraphRequest_AppendsShaderSubGraphExtension()
        {
            var request = new CreateSubGraphRequest("ExampleSubGraph", "Assets/ShaderSubGraphs", "blank");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderSubGraphs/ExampleSubGraph.shadersubgraph"));
            Assert.That(request.Name, Is.EqualTo("ExampleSubGraph"));
            Assert.That(request.Path, Is.EqualTo("Assets/ShaderSubGraphs"));
            Assert.That(request.Template, Is.EqualTo("blank"));
        }

        [Test]
        public void CreateSubGraphRequest_DefaultsToShaderSubGraphsFolder()
        {
            var request = new CreateSubGraphRequest("MySubGraph", null, null);

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderSubGraphs/MySubGraph.shadersubgraph"));
            Assert.That(request.Path, Is.EqualTo("Assets/ShaderSubGraphs"));
            Assert.That(request.Template, Is.EqualTo("blank"));
        }

        [Test]
        public void Handle_NullRequest_ReturnsFailure()
        {
            ShaderGraphResponse response = ShaderGraphAssetTool.Handle(null);

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Does.Contain("Request is required"));
        }

        [Test]
        public void Handle_UnsupportedAction_ReturnsSupportedActionList()
        {
            ShaderGraphResponse response = ShaderGraphAssetTool.Handle(new UnsupportedShaderGraphRequest());

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Does.Contain("Unsupported Shader Graph action"));
            Assert.That(response.Message, Does.Contain("Supported actions: create_graph, create_subgraph, rename_graph, rename_subgraph, duplicate_graph, delete_graph, move_graph, set_graph_metadata, create_category, rename_category, find_category, delete_category, reorder_category, merge_category, duplicate_category, split_category, list_categories, read_graph_summary, read_subgraph_summary, export_graph_contract, import_graph_contract, find_node, find_property, list_supported_nodes, list_supported_properties, list_supported_connections, update_property, rename_property, duplicate_property, reorder_property, move_property_to_category, rename_node, duplicate_node, move_node, delete_node, remove_property, add_property, add_node, connect_ports, find_connection, remove_connection, reconnect_connection, save_graph."));
        }

        [Test]
        public void ExportGraphContractRequest_PreservesAssetPath()
        {
            var request = new ExportGraphContractRequest("Assets/ShaderGraphs/ExampleLitGraph.shadergraph");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.ExportGraphContract));
        }

        [Test]
        public void ReadSubGraphSummaryRequest_PreservesAssetPath()
        {
            var request = new ReadSubGraphSummaryRequest("Assets/ShaderGraphs/ExampleSubGraph.shadersubgraph");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleSubGraph.shadersubgraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.ReadSubGraphSummary));
        }

        [Test]
        public void RenameSubGraphRequest_ResolvesTargetAssetPath()
        {
            var request = new RenameSubGraphRequest(
                "Assets/ShaderSubGraphs/ExampleSubGraph.shadersubgraph",
                "RenamedSubGraph");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderSubGraphs/ExampleSubGraph.shadersubgraph"));
            Assert.That(request.Name, Is.EqualTo("RenamedSubGraph"));
            Assert.That(request.TargetAssetPath, Is.EqualTo("Assets/ShaderSubGraphs/RenamedSubGraph.shadersubgraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.RenameSubGraph));
        }

        [Test]
        public void ImportGraphContractRequest_PreservesAssetPathAndContractJson()
        {
            const string contractJson = "{\"contractVersion\":\"unity-shader-graph-mcp/export-graph-contract-v1\",\"categories\":[],\"properties\":[],\"nodes\":[],\"connections\":[]}";
            var request = new ImportGraphContractRequest("Assets/ShaderGraphs/ExampleLitGraph.shadergraph", contractJson);

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.ImportGraphContract));
            Assert.That(request.GraphContractJson, Is.EqualTo(contractJson));
        }

        [Test]
        public void DuplicateGraphRequest_ResolvesTargetAssetPath()
        {
            var request = new DuplicateGraphRequest(
                "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "CopiedLitGraph");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Name, Is.EqualTo("CopiedLitGraph"));
            Assert.That(request.TargetAssetPath, Is.EqualTo("Assets/ShaderGraphs/CopiedLitGraph.shadergraph"));
        }

        [Test]
        public void DeleteGraphRequest_PreservesAssetPath()
        {
            var request = new DeleteGraphRequest("Assets/ShaderGraphs/ExampleLitGraph.shadergraph");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.DeleteGraph));
        }

        [Test]
        public void MoveGraphRequest_ResolvesTargetAssetPath()
        {
            var request = new MoveGraphRequest(
                "Assets/ShaderGraphs/ExampleLitGraph.shadergraph",
                "Assets/ShaderGraphs/Moved");

            Assert.That(request.AssetPath, Is.EqualTo("Assets/ShaderGraphs/ExampleLitGraph.shadergraph"));
            Assert.That(request.Action, Is.EqualTo(ShaderGraphAction.MoveGraph));
            Assert.That(request.TargetAssetPath, Is.EqualTo("Assets/ShaderGraphs/Moved/ExampleLitGraph.shadergraph"));
        }
    }
}
