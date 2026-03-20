using NUnit.Framework;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphRequestTests
    {
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

    }
}
