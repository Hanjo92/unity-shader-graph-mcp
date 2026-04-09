using NUnit.Framework;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;
using UnityEditor;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphFixtureSampleTests
    {
        [Test]
        public void RepoOwnedShaderGraphFixture_ReadSummary_StaysPackageBacked()
        {
            string assetPath = ShaderGraphTestAssets.GetFixtureAssetPath("ShaderGraphs/TMP_SDF-URP Lit.shadergraph");

            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("read_graph_summary"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "nodeCount"), Is.GreaterThan(0));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "connectionCount"), Is.GreaterThan(0));
        }

        [Test]
        public void RepoOwnedShaderSubGraphFixture_ReadSummary_StaysPackageBacked()
        {
            string assetPath = ShaderGraphTestAssets.GetFixtureAssetPath("ShaderSubGraphs/WorldSpaceUV.shadersubgraph");

            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleReadSubGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("read_subgraph_summary"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "categoryCount"), Is.GreaterThanOrEqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "nodeCount"), Is.GreaterThan(0));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "connectionCount"), Is.GreaterThan(0));
        }
    }
}
