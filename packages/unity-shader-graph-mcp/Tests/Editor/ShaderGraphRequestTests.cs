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
            Assert.That(response.Message, Does.Contain("Supported actions: create_graph, create_category, rename_category, find_category, delete_category, read_graph_summary, find_node, find_property, list_supported_nodes, list_supported_properties, list_supported_connections, update_property, rename_property, duplicate_property, reorder_property, move_property_to_category, rename_node, duplicate_node, move_node, delete_node, remove_property, add_property, add_node, connect_ports, find_connection, remove_connection, reconnect_connection, save_graph."));
        }
    }
}
