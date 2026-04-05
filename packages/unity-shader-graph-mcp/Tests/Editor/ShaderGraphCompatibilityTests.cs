using NUnit.Framework;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphCompatibilityTests
    {
        [Test]
        public void PackageCompatibilityProbe_ReturnsSnapshot()
        {
            var snapshot = ShaderGraphPackageCompatibility.Capture();

            Assert.That(snapshot, Is.Not.Null);
            Assert.That(snapshot.GraphSurface, Is.Not.Null);
        }

        [Test]
        public void CompatibilitySnapshot_ToDictionaryIncludesGraphSurfaceAndBackendKind()
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
                new[] { "System.Void AddGraphInput(UnityEditor.ShaderGraph.ShaderInput input, System.Int32 index)" },
                System.Array.Empty<string>());

            var snapshot = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.PackageReady,
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "Unity.ShaderGraph.Editor" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphObject" },
                surface,
                new[] { "graph surface resolved" });

            var data = snapshot.ToDictionary();

            Assert.That(data["backendKind"], Is.EqualTo("PackageReady"));
            Assert.That(data["packageDetected"], Is.True);

            var graphSurface =
                (System.Collections.Generic.IReadOnlyDictionary<string, object>)data["graphSurface"];
            Assert.That(graphSurface["hasCoreMutationSurface"], Is.True);
            Assert.That(graphSurface["hasAddGraphInput"], Is.True);
        }

        [Test]
        public void IncompleteSurface_StaysMarkedAsNotReady()
        {
            var surface = new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                "UnityEditor.ShaderGraph.JsonObject",
                true,
                true,
                true,
                true,
                false,
                true,
                true,
                true,
                true,
                true,
                true,
                System.Array.Empty<string>(),
                new[] { "ValidateGraph" });

            var snapshot = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.PackageDetectedButIncomplete,
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "Unity.ShaderGraph.Editor" },
                new[] { "UnityEditor.ShaderGraph.GraphData" },
                new[] { "UnityEditor.ShaderGraph.GraphObject" },
                surface,
                new[] { "missing ValidateGraph" });

            var data = snapshot.ToDictionary();
            var graphSurface =
                (System.Collections.Generic.IReadOnlyDictionary<string, object>)data["graphSurface"];

            Assert.That(data["backendKind"], Is.EqualTo("PackageDetectedButIncomplete"));
            Assert.That(data["packageDetected"], Is.True);
            Assert.That(graphSurface["hasCoreMutationSurface"], Is.False);
        }

        [Test]
        public void AssetSnapshot_ToDictionaryCarriesCompatibilityData()
        {
            var surface = new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                "UnityEditor.ShaderGraph.JsonObject",
                true,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                false,
                System.Array.Empty<string>(),
                new[] { "AddGraphInput", "AddNode", "Connect", "ValidateGraph" });

            var compatibility = new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.Scaffold,
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                System.Array.Empty<string>(),
                surface,
                new[] { "scaffold mode" });

            var snapshot = new ShaderGraphAssetSnapshot(
                "create_graph",
                "Assets/ShaderGraphs/Example.shadergraph",
                "Assets/ShaderGraphs/Example.shadergraph.mcp.json",
                "/tmp/Example.shadergraph",
                true,
                true,
                "unity-shader-graph-mcp/scaffold-v1",
                "Example",
                "blank",
                "2026-03-19T00:00:00.0000000Z",
                "2026-03-19T00:00:00.0000000Z",
                1,
                1,
                2,
                3,
                ShaderGraphExecutionKind.Scaffold,
                new[] { "Color" },
                new[] { "Output" },
                new[] { "Output:Out -> Input:In" },
                new[] { "note" },
                new[] { "# preview" },
                compatibility);

            var data = snapshot.ToDictionary();

            Assert.That(data["backendKind"], Is.EqualTo("Scaffold"));
            Assert.That(data["compatibility"], Is.Not.Null);

            var nested =
                (System.Collections.Generic.IReadOnlyDictionary<string, object>)data["compatibility"];
            Assert.That(nested["backendKind"], Is.EqualTo("Scaffold"));
            Assert.That(nested["graphSurface"], Is.Not.Null);
        }
    }
}
