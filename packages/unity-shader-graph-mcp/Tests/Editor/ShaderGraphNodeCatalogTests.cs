using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Adapters;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphNodeCatalogTests
    {
        private static readonly string[] PureMathValueVectorNodeTypes =
        {
            "Float/Vector1",
            "Vector2",
            "Vector3",
            "Vector4",
            "Combine",
            "Split",
            "Append",
            "Add",
            "Subtract",
            "Multiply",
            "Divide",
            "Lerp",
            "Clamp",
            "Sine",
            "Cosine",
        };

        private static readonly string[] TextureSampleNodeTypes =
        {
            "Texture2DAsset",
            "SampleTexture2D",
            "Texture2DArrayAsset",
            "SampleTexture2DArray",
            "Texture3DAsset",
            "SampleTexture3D",
            "CubemapAsset",
            "SampleCubemap",
            "SamplerState",
        };

        private static readonly string[] CoordinateUtilityNodeTypes =
        {
            "UV",
            "ScreenPosition",
            "Position",
            "NormalVector",
            "TangentVector",
            "BitangentVector",
            "ViewDirection",
            "Time",
            "Object",
            "Camera",
            "Transform",
            "TransformationMatrix",
        };

        private static readonly string[] NormalLightingRenderingNodeTypes =
        {
            "NormalBlend",
            "NormalFromHeight",
            "NormalFromTexture",
            "NormalReconstructZ",
            "NormalStrength",
            "NormalUnpack",
            "Ambient",
            "BakedGI",
            "Blackbody",
            "DielectricSpecular",
            "MainLightDirection",
            "MetalReflectance",
            "Reflection",
            "ReflectionProbe",
            "Fog",
            "RenderType",
            "RenderTypeBranch",
        };

        private static readonly string[] ConfigurableMetadataRequiredNodeTypes =
        {
            "CustomFunction",
        };

        private static readonly string[] SpecializedPortableDefaultNodeTypes =
        {
            "DefaultBitmapText",
            "DefaultGradient",
            "DefaultSDFText",
            "DefaultSolid",
            "DefaultTexture",
        };

        private static readonly string[] SpecializedPackageSpecificNodeTypes =
        {
            "ComputeDeform",
            "CustomInterpolator",
            "ElementTextureUV",
            "LinearBlendSkinning",
            "SampleElementTexture",
            "SpriteSkinning",
        };

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
        public void SupportedNodeCanonicalNames_IncludePureMathValueVectorBatch()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();

            foreach (string nodeType in PureMathValueVectorNodeTypes)
            {
                Assert.That(supportedNames, Has.Some.EqualTo(nodeType), nodeType);
            }

            Assert.That(classification["semantics"], Does.Contain("graph-addable"));
            Assert.That(classification["supportedCount"], Is.GreaterThanOrEqualTo(PureMathValueVectorNodeTypes.Length));
        }

        [Test]
        public void SupportedNodeCanonicalNames_IncludeTextureSampleBatch()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();
            var textureSampleClassification = ShaderGraphTestAssets.RequireDictionary(
                classification,
                "textureSampleNodeClassification");
            var assetFreeNodeTypes = ShaderGraphTestAssets.GetStringList(textureSampleClassification, "assetFreeNodeTypes");
            var fixtureBackedNodeTypes = ShaderGraphTestAssets.GetStringList(textureSampleClassification, "fixtureBackedNodeTypes");

            foreach (string nodeType in TextureSampleNodeTypes)
            {
                Assert.That(supportedNames, Has.Some.EqualTo(nodeType), nodeType);
            }

            Assert.That(assetFreeNodeTypes, Has.Some.EqualTo("Texture2DAsset"));
            Assert.That(assetFreeNodeTypes, Has.Some.EqualTo("SamplerState"));
            Assert.That(fixtureBackedNodeTypes, Has.Some.EqualTo("SampleTexture2D"));
            Assert.That(fixtureBackedNodeTypes, Has.Some.EqualTo("SampleCubemap"));
        }

        [Test]
        public void SupportedNodeCanonicalNames_IncludeCoordinateUtilityBatch()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();
            var coordinateUtilityClassification = ShaderGraphTestAssets.RequireDictionary(
                classification,
                "coordinateUtilityNodeClassification");
            var coordinateSpaceNodeTypes = ShaderGraphTestAssets.GetStringList(coordinateUtilityClassification, "coordinateSpaceNodeTypes");
            var sceneCameraNodeTypes = ShaderGraphTestAssets.GetStringList(coordinateUtilityClassification, "sceneCameraNodeTypes");
            var utilityNodeTypes = ShaderGraphTestAssets.GetStringList(coordinateUtilityClassification, "utilityNodeTypes");

            foreach (string nodeType in CoordinateUtilityNodeTypes)
            {
                Assert.That(supportedNames, Has.Some.EqualTo(nodeType), nodeType);
            }

            Assert.That(coordinateSpaceNodeTypes, Has.Some.EqualTo("UV"));
            Assert.That(coordinateSpaceNodeTypes, Has.Some.EqualTo("Position"));
            Assert.That(sceneCameraNodeTypes, Has.Some.EqualTo("Camera"));
            Assert.That(utilityNodeTypes, Has.Some.EqualTo("Time"));
            Assert.That(utilityNodeTypes, Has.Some.EqualTo("TransformationMatrix"));
        }

        [Test]
        public void SupportedNodeCanonicalNames_IncludeNormalLightingRenderingBatch()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();
            var normalLightingClassification = ShaderGraphTestAssets.RequireDictionary(
                classification,
                "normalLightingRenderingNodeClassification");
            var normalWorkflowNodeTypes = ShaderGraphTestAssets.GetStringList(normalLightingClassification, "normalWorkflowNodeTypes");
            var lightingReflectionNodeTypes = ShaderGraphTestAssets.GetStringList(normalLightingClassification, "lightingReflectionNodeTypes");
            var renderingMaterialNodeTypes = ShaderGraphTestAssets.GetStringList(normalLightingClassification, "renderingMaterialNodeTypes");

            foreach (string nodeType in NormalLightingRenderingNodeTypes)
            {
                Assert.That(supportedNames, Has.Some.EqualTo(nodeType), nodeType);
            }

            Assert.That(normalWorkflowNodeTypes, Has.Some.EqualTo("NormalFromTexture"));
            Assert.That(normalWorkflowNodeTypes, Has.Some.EqualTo("NormalStrength"));
            Assert.That(lightingReflectionNodeTypes, Has.Some.EqualTo("BakedGI"));
            Assert.That(lightingReflectionNodeTypes, Has.Some.EqualTo("MainLightDirection"));
            Assert.That(renderingMaterialNodeTypes, Has.Some.EqualTo("RenderType"));
            Assert.That(renderingMaterialNodeTypes, Has.Some.EqualTo("RenderTypeBranch"));
        }

        [Test]
        public void SupportedNodeCanonicalNames_ClassifyConfigurableNodes()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();
            var configurableClassification = ShaderGraphTestAssets.RequireDictionary(
                classification,
                "configurableNodeClassification");
            var propertyBackedNodeTypes = ShaderGraphTestAssets.GetStringList(configurableClassification, "propertyBackedNodeTypes");
            var metadataRequiredNodeTypes = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "metadataRequiredNodeTypes");
            var supportedModeLabels = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "metadataRequiredSupportedModeLabels");
            var metadataRequiredUnsupportedNodeTypes = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "metadataRequiredUnsupportedNodeTypes");
            var metadataRequiredUnsupportedDiagnostics = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "metadataRequiredUnsupportedDiagnostics");
            var externallyAssetBoundUnsupportedNodeTypes = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "externallyAssetBoundUnsupportedNodeTypes");
            var externallyAssetBoundUnsupportedDiagnostics = ShaderGraphTestAssets.GetStringList(
                configurableClassification,
                "externallyAssetBoundUnsupportedDiagnostics");

            foreach (string nodeType in ConfigurableMetadataRequiredNodeTypes)
            {
                Assert.That(supportedNames, Has.None.EqualTo(nodeType), nodeType);
                Assert.That(metadataRequiredUnsupportedNodeTypes, Has.Some.EqualTo(nodeType), nodeType);
            }

            Assert.That(supportedNames, Has.Some.EqualTo("Dropdown"));
            Assert.That(supportedNames, Has.Some.EqualTo("Keyword"));
            Assert.That(metadataRequiredNodeTypes, Has.Some.EqualTo("Dropdown"));
            Assert.That(metadataRequiredNodeTypes, Has.Some.EqualTo("Keyword"));
            Assert.That(supportedModeLabels, Has.Some.EqualTo("Dropdown:static-string-entries"));
            Assert.That(supportedModeLabels, Has.Some.EqualTo("Keyword:boolean"));
            Assert.That(supportedModeLabels, Has.Some.EqualTo("Keyword:enum"));
            Assert.That(propertyBackedNodeTypes, Has.Some.EqualTo("Property"));
            Assert.That(supportedNames, Has.None.EqualTo("SubGraph"));
            AssertUnsupportedDiagnosticIfDiscovered(
                metadataRequiredUnsupportedDiagnostics,
                "CustomFunction",
                "configuration serialization");
            Assert.That(externallyAssetBoundUnsupportedNodeTypes, Has.Some.EqualTo("SubGraph"));
            AssertUnsupportedDiagnosticIfDiscovered(
                externallyAssetBoundUnsupportedDiagnostics,
                "SubGraph",
                "Externally asset-bound");
        }

        [Test]
        public void SupportedNodeCanonicalNames_ClassifySpecializedNodes()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var supportedNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();
            var specializedClassification = ShaderGraphTestAssets.RequireDictionary(
                classification,
                "specializedNodeClassification");
            var portableDefaultNodeTypes = ShaderGraphTestAssets.GetStringList(
                specializedClassification,
                "portableDefaultNodeTypes");
            var packageSpecificUnsupportedNodeTypes = ShaderGraphTestAssets.GetStringList(
                specializedClassification,
                "packageSpecificUnsupportedNodeTypes");
            var packageSpecificUnsupportedDiagnostics = ShaderGraphTestAssets.GetStringList(
                specializedClassification,
                "packageSpecificUnsupportedDiagnostics");

            foreach (string nodeType in SpecializedPortableDefaultNodeTypes)
            {
                Assert.That(supportedNames, Has.Some.EqualTo(nodeType), nodeType);
                Assert.That(portableDefaultNodeTypes, Has.Some.EqualTo(nodeType), nodeType);
            }

            foreach (string nodeType in SpecializedPackageSpecificNodeTypes)
            {
                Assert.That(supportedNames, Has.None.EqualTo(nodeType), nodeType);
                Assert.That(packageSpecificUnsupportedNodeTypes, Has.Some.EqualTo(nodeType), nodeType);
            }

            AssertUnsupportedDiagnosticIfDiscovered(
                packageSpecificUnsupportedDiagnostics,
                "SampleElementTexture",
                "Package-specific specialized");
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

        [Test]
        public void SupportedNodeCatalogReportLines_RecordInitializerBackedPromotions()
        {
            ShaderGraphTestAssets.RequirePackageReady();

            var lines = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogReportLines();
            var classification = ShaderGraphPackageGraphInspector.BuildNodeCatalogClassificationData();

            Assert.That(
                lines,
                Has.Some.Contains("Property (UnityEditor.ShaderGraph.PropertyNode)")
                    .And.Contains("Node initializer 'PropertyNode' applied"));
            Assert.That(ShaderGraphTestAssets.GetInt(classification, "initializerBackedCount"), Is.GreaterThanOrEqualTo(1));

            var initializerBackedNodeTypes = ShaderGraphTestAssets.GetStringList(classification, "initializerBackedNodeTypes");
            Assert.That(initializerBackedNodeTypes, Has.Some.EqualTo("Property"));
        }

        [TestCase("filtered", "Nested internal node types are excluded from the initial graph-addable catalog.", "filtered:nested-internal")]
        [TestCase("filtered", "Legacy master node types are excluded from the safe addable catalog.", "filtered:legacy-master")]
        [TestCase("filtered", "Types that do not follow the public *Node shape stay discoverable-only until explicitly validated.", "filtered:non-public-node-shape")]
        [TestCase("filtered", "Preview, block-only, and output-only node types are excluded from the safe addable catalog.", "filtered:preview-block-output")]
        [TestCase("filtered", "Serialization and redirect placeholder node types are excluded from the safe addable catalog.", "filtered:serialization-placeholder")]
        [TestCase("filtered", "Package-specific specialized node types require explicit package-context serialization before safe graph-addable support.", "filtered:package-specific-specialized")]
        [TestCase("filtered", "Metadata-backed configurable node types require explicit configuration serialization before safe graph-addable support.", "filtered:metadata-required")]
        [TestCase("filtered", "Externally asset-bound configurable node types require explicit asset binding before safe graph-addable support.", "filtered:external-asset-bound")]
        [TestCase("graph-addable", "Node initializer 'PropertyNode' applied. Activator -> AddNode -> ValidateGraph succeeded.", "supported:initializer-backed")]
        [TestCase("probe-failed", "Probe graph creation failed: no graph", "probe:graph-create")]
        [TestCase("probe-failed", "Node instantiation failed: ctor exploded", "probe:instantiation")]
        [TestCase("probe-failed", "Node initializer 'PropertyNode' failed: binding unavailable", "probe:missing-initializer")]
        [TestCase("probe-failed", "Property probe setup failed: binding unavailable", "probe:missing-initializer")]
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

        private static void AssertUnsupportedDiagnosticIfDiscovered(
            IReadOnlyList<string> diagnostics,
            string nodeType,
            string expectedNote)
        {
            string diagnostic = diagnostics.FirstOrDefault(line => line.Contains(nodeType));
            Assert.That(diagnostic, Is.Not.Null, nodeType);

            if (!diagnostic.Contains("status: not-discovered"))
            {
                Assert.That(diagnostic, Does.Contain(expectedNote), nodeType);
            }
        }
    }
}
