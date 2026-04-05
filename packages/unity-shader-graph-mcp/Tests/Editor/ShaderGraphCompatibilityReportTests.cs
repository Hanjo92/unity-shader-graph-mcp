using NUnit.Framework;
using ShaderGraphMcp.Editor.Diagnostics;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphCompatibilityReportTests
    {
        [Test]
        public void BuildReportText_IncludesKeySections()
        {
            var surface = new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                "System.Object",
                true,
                true,
                true,
                true,
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                new[] { "System.Void AddNode(UnityEditor.ShaderGraph.AbstractMaterialNode node)" },
                new[] { "Connect" });

            var snapshot = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.PackageDetectedButIncomplete,
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "Unity.ShaderGraph.Editor" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphObject" },
                surface,
                new[] { "graph surface resolved" });

            string report = ShaderGraphCompatibilityReport.BuildReportText(snapshot);

            Assert.That(report, Does.Contain("# Shader Graph Compatibility Report"));
            Assert.That(report, Does.Contain("unityVersion:"));
            Assert.That(report, Does.Contain("backendKind: PackageDetectedButIncomplete"));
            Assert.That(report, Does.Contain("graphTypeName: UnityEditor.ShaderGraph.GraphData"));
            Assert.That(report, Does.Contain("hasAddGraphInput: True"));
            Assert.That(report, Does.Contain("## Compatibility Snapshot"));
            Assert.That(report, Does.Contain("## Fallback Behavior"));
            Assert.That(report, Does.Contain("## How To Capture"));
            Assert.That(report, Does.Contain("## resolvedMethodSignatures"));
            Assert.That(report, Does.Contain("## Next Spike"));
        }
    }
}
