using NUnit.Framework;
using ShaderGraphMcp.Editor.Diagnostics;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphMcpPanelTests
    {
        [Test]
        public void BuildStatus_ReturnsPackageCatalogSummary()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            ShaderGraphMcpPanelStatus status = ShaderGraphMcpPanel.BuildStatus();

            Assert.That(status.BackendKind, Is.EqualTo(ShaderGraphBackendKind.PackageReady.ToString()));
            Assert.That(status.PackageDetected, Is.True);
            Assert.That(status.SupportedNodeCount, Is.GreaterThan(0));
            Assert.That(status.DiscoveredNodeCount, Is.GreaterThanOrEqualTo(status.SupportedNodeCount));
            Assert.That(status.NodeCatalogSemantics, Is.EqualTo("supported=graph-addable"));
            Assert.That(status.Summary, Does.Contain("PackageReady"));
        }

        [TestCase(null, "Assets/ShaderGraphs")]
        [TestCase("", "Assets/ShaderGraphs")]
        [TestCase("Assets/Scripts/Shader/New Shader Graph.shadergraph", "Assets/Scripts/Shader")]
        [TestCase("Assets/ShaderGraphs", "Assets/ShaderGraphs")]
        public void ResolveTargetDirectory_ReturnsStableFolder(string assetPath, string expectedDirectory)
        {
            Assert.That(ShaderGraphMcpPanel.ResolveTargetDirectory(assetPath), Is.EqualTo(expectedDirectory));
        }

        [Test]
        public void BuildCodexMcpCommand_ReturnsServerEntrypointCommand()
        {
            string command = ShaderGraphMcpPanel.BuildCodexMcpCommand();

            Assert.That(command, Does.Contain("python3.12"));
            Assert.That(command, Does.Contain("server/src/unity_shader_graph_mcp/__main__.py"));
            Assert.That(command, Does.Contain("--mcp"));
        }
    }
}
