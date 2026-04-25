using System.Linq;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Adapters;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphNodeCatalogTests
    {
        [Test]
        public void SupportedNodeCatalogReportLines_IncludeCurrentSmokeNodes()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var lines = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogReportLines();
            int discoveredCount = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogCount();
            int supportedCount = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogCount();

            Assert.That(lines.Count, Is.GreaterThanOrEqualTo(3));
            Assert.That(supportedCount, Is.EqualTo(lines.Count));
            Assert.That(supportedCount, Is.LessThanOrEqualTo(discoveredCount));
            Assert.That(lines, Has.Some.Contains("Color (UnityEditor.ShaderGraph.ColorNode)"));
            Assert.That(lines, Has.Some.Contains("Split (UnityEditor.ShaderGraph.SplitNode)"));
            Assert.That(lines, Has.Some.Contains("Float/Vector1 (UnityEditor.ShaderGraph.Vector1Node)"));
            Assert.That(lines, Has.Some.Contains("SampleGradient (UnityEditor.ShaderGraph.SampleGradient)"));
        }

        [Test]
        public void SupportedNodeCatalogReportLines_ExposeCurrentAliases()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var lines = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogReportLines();

            string vector1Line = lines.First(line => line.Contains("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(vector1Line, Does.Contain("Float"));
            Assert.That(vector1Line, Does.Contain("Vector1"));
            Assert.That(vector1Line, Does.Contain("Float/Vector1"));

            string splitLine = lines.First(line => line.Contains("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(splitLine, Does.Contain("Split"));
        }

        [Test]
        public void SupportedNodeCatalogReportLines_ExcludeKnownInternalAndLegacyPatterns()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var lines = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogReportLines();

            Assert.That(lines, Has.None.Contains("UnityEditor.ShaderGraph.Legacy."));
            Assert.That(lines, Has.None.Contains("PreviewNode"));
            Assert.That(lines, Has.None.Contains("BlockNode"));
            Assert.That(lines, Has.None.Contains("RedirectNodeData"));
            Assert.That(lines, Has.None.Contains("UnknownNodeType"));
            Assert.That(lines, Has.None.Contains("SubGraphOutputNode"));
        }

        [Test]
        public void DiscoveredNodeCatalogReportLines_RemainBroaderThanSupportedSubset()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var discoveredLines = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogReportLines();
            int discoveredCount = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogCount();
            int supportedCount = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogCount();
            int excludedCount = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogReportLines().Count;
            int probeRejectedCount = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogCount();

            Assert.That(discoveredLines.Count, Is.EqualTo(discoveredCount));
            Assert.That(discoveredCount, Is.GreaterThanOrEqualTo(supportedCount));
            Assert.That(excludedCount + probeRejectedCount, Is.LessThanOrEqualTo(discoveredCount));
            Assert.That(discoveredLines, Has.Some.Contains("status: graph-addable"));
        }

        [Test]
        public void ExcludedNodeCatalogBuckets_SumToExcludedCount()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var excludedLines = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogReportLines();
            var bucketLines = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogBucketReportLines();
            int bucketedCount = bucketLines.Sum(ParseBucketCount);

            Assert.That(bucketedCount, Is.EqualTo(excludedLines.Count));

            if (excludedLines.Count > 0)
            {
                Assert.That(bucketLines.Count, Is.GreaterThan(0));
            }
        }

        [Test]
        public void ProbeRejectedNodeCatalogBuckets_SumToProbeRejectedCount()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var discoveredLines = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogReportLines();
            var bucketLines = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogBucketReportLines();
            int probeRejectedCount = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogCount();
            int bucketedCount = bucketLines.Sum(ParseBucketCount);

            Assert.That(bucketedCount, Is.EqualTo(probeRejectedCount));

            if (probeRejectedCount > 0)
            {
                Assert.That(bucketLines.Count, Is.GreaterThan(0));
                Assert.That(discoveredLines, Has.Some.Contains("status: probe-failed"));
            }
        }

        [TestCase("filtered", "Nested internal node types are excluded from the initial graph-addable catalog.", "filtered:nested-internal")]
        [TestCase("filtered", "Legacy master node types are excluded from the safe addable catalog.", "filtered:legacy-master")]
        [TestCase("filtered", "Types that do not follow the public *Node shape stay discoverable-only until explicitly validated.", "filtered:non-public-node-shape")]
        [TestCase("filtered", "Preview, block-only, and output-only node types are excluded from the safe addable catalog.", "filtered:preview-block-output")]
        [TestCase("filtered", "Serialization and redirect placeholder node types are excluded from the safe addable catalog.", "filtered:serialization-placeholder")]
        [TestCase("probe-failed", "Probe graph creation failed: no graph", "probe:graph-create")]
        [TestCase("probe-failed", "Node instantiation failed: ctor exploded", "probe:instantiation")]
        [TestCase("probe-failed", "Node layout assignment failed: drawState missing", "probe:layout")]
        [TestCase("probe-failed", "GraphData.AddNode(...) failed: invalid cast", "probe:add-node")]
        [TestCase("probe-failed", "GraphData.ValidateGraph() failed: validation error", "probe:validate-graph")]
        [TestCase("probe-failed", "Node type is null.", "probe:null-type")]
        public void ClassifyNodeCatalogDiagnosticBucket_MapsStableReasons(
            string catalogStatus,
            string catalogNote,
            string expectedBucket)
        {
            Assert.That(
                ShaderGraphPackageGraphInspector.ClassifyNodeCatalogDiagnosticBucket(catalogStatus, catalogNote),
                Is.EqualTo(expectedBucket));
        }

        private static int ParseBucketCount(string line)
        {
            const string marker = "| count: ";
            int markerIndex = line.IndexOf(marker);
            Assert.That(markerIndex, Is.GreaterThanOrEqualTo(0), $"Bucket line missing count marker: {line}");

            string countText = line.Substring(markerIndex + marker.Length).Trim();
            Assert.That(int.TryParse(countText, out int parsed), Is.True, $"Bucket line has invalid count: {line}");
            return parsed;
        }
    }
}
