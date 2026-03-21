using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Adapters;
using ShaderGraphMcp.Editor.Diagnostics;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Tests
{
    public sealed class ShaderGraphPackageBackedSmokeTests
    {
        private static readonly string[] PreferredCommonNodeTypes =
        {
            "Add",
            "Subtract",
            "Multiply",
            "Divide",
            "Power",
            "Minimum",
            "Maximum",
            "Modulo",
            "Lerp",
            "Smoothstep",
            "Clamp",
            "Step",
            "Absolute",
            "Floor",
            "Ceiling",
            "Round",
            "Sign",
            "Sine",
            "Cosine",
            "Tangent",
            "Negate",
            "Reciprocal",
            "SquareRoot",
            "Fraction",
            "Truncate",
            "Saturate",
            "OneMinus",
            "Comparison",
            "Branch",
            "Combine",
            "Vector2",
            "Vector3",
            "Vector4",
            "Normalize",
            "Time",
            "TilingAndOffset",
            "SampleTexture2D",
        };

        private static IEnumerable<TestCaseData> ArithmeticVector1ChainCases()
        {
            yield return new TestCaseData("Add", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Subtract", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Multiply", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Divide", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Power", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Minimum", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Maximum", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Modulo", new[] { "A", "B" }, 2);
            yield return new TestCaseData("Lerp", new[] { "A", "B", "T" }, 3);
            yield return new TestCaseData("Smoothstep", new[] { "Edge1", "Edge2", "In" }, 3);
            yield return new TestCaseData("Clamp", new[] { "In", "Min", "Max" }, 3);
            yield return new TestCaseData("Step", new[] { "Edge", "In" }, 2);
            yield return new TestCaseData("Absolute", new[] { "In" }, 1);
            yield return new TestCaseData("Floor", new[] { "In" }, 1);
            yield return new TestCaseData("Ceiling", new[] { "In" }, 1);
            yield return new TestCaseData("Round", new[] { "In" }, 1);
            yield return new TestCaseData("Sign", new[] { "In" }, 1);
            yield return new TestCaseData("Sine", new[] { "In" }, 1);
            yield return new TestCaseData("Cosine", new[] { "In" }, 1);
            yield return new TestCaseData("Tangent", new[] { "In" }, 1);
            yield return new TestCaseData("Negate", new[] { "In" }, 1);
            yield return new TestCaseData("Reciprocal", new[] { "In" }, 1);
            yield return new TestCaseData("SquareRoot", new[] { "In" }, 1);
            yield return new TestCaseData("Fraction", new[] { "In" }, 1);
            yield return new TestCaseData("Truncate", new[] { "In" }, 1);
            yield return new TestCaseData("Saturate", new[] { "In" }, 1);
            yield return new TestCaseData("OneMinus", new[] { "In" }, 1);
        }

        private ShaderGraphTestAssets.TemporaryFolderScope temporaryFolder;

        [SetUp]
        public void SetUp()
        {
            ShaderGraphTestAssets.RequirePackageReady();
            temporaryFolder = ShaderGraphTestAssets.CreateTemporaryFolder(TestContext.CurrentContext.Test.Name);
        }

        [TearDown]
        public void TearDown()
        {
            temporaryFolder?.Dispose();
            temporaryFolder = null;
        }

        [Test]
        public void BlankCreateGraph_ThenReadSummary_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankCreateGraphReadSummary", out ShaderGraphResponse createResponse);

            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "template"), Is.EqualTo("blank"));
            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));

            var createdGraph = ShaderGraphTestAssets.RequireDictionary(createResponse.Data, "createdGraph");
            Assert.That(ShaderGraphTestAssets.GetString(createdGraph, "resolvedTemplate"), Is.EqualTo("blank"));
            Assert.That(ShaderGraphTestAssets.GetString(createdGraph, "graphPathLabel"), Is.EqualTo("Shader Graphs"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(0));
        }

        [Test]
        public void BlankCreateGraph_EndToEndHappyPath_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankCreateGraphHappyPath", out ShaderGraphResponse createResponse);

            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "template"), Is.EqualTo("blank"));
            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Exposure",
                "Float/Vector1",
                "0");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Happy Path Source");
            ShaderGraphTestAssets.RequirePackageReady(addSourceNodeResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceNodeResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse addSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Happy Path Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkNodeResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkNodeResponse);
            Assert.That(sinkNodeId, Is.Not.Empty);

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            ShaderGraphResponse saveResponse = ShaderGraphAssetTool.HandleSaveGraph(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(saveResponse);

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetString(saveResponse.Data, "operation"), Is.EqualTo("save_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(saveResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
        }

        [Test]
        public void BlankGraph_RunBlankGraphHappyPathHelper_StaysPackageBacked()
        {
            string graphName = "BlankGraphReleaseSmoke";
            ShaderGraphResponse response = ShaderGraphDebugMenu.RunBlankGraphHappyPath(
                temporaryFolder.AssetPath,
                graphName,
                false);

            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("read_graph_summary"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "connectionCount"), Is.EqualTo(1));

            var createdGraphPath = ShaderGraphTestAssets.GetString(response.Data, "assetPath");
            Assert.That(createdGraphPath, Does.Contain(graphName));
        }

        [Test]
        public void BlankGraph_AddFloatAndColorProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAddProperties", out _);

            ShaderGraphResponse addFloatResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Exposure",
                "Float/Vector1",
                "0");
            ShaderGraphTestAssets.RequirePackageReady(addFloatResponse);

            ShaderGraphResponse addColorResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addColorResponse);

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(2));

            var properties = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "properties");
            Assert.That(properties, Has.Some.Contains("Exposure"));
            Assert.That(properties, Has.Some.Contains("Tint"));
        }

        [Test]
        public void BlankGraph_UpdateExistingProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUpdateProperty", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#000000");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse updatePropertyResponse = ShaderGraphAssetTool.HandleUpdateProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(updatePropertyResponse);

            Assert.That(ShaderGraphTestAssets.GetString(updatePropertyResponse.Data, "operation"), Is.EqualTo("update_property"));

            var updatedProperty = ShaderGraphTestAssets.RequireDictionary(updatePropertyResponse.Data, "updatedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(updatedProperty, "displayName"), Is.EqualTo("Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(updatedProperty, "resolvedPropertyType"), Is.EqualTo("Color"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
        }

        [Test]
        public void BlankGraph_MoveNode_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMoveNode", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Move Source");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            string nodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(nodeId, Is.Not.Empty);

            ShaderGraphResponse moveNodeResponse = ShaderGraphAssetTool.HandleMoveNode(
                assetPath,
                nodeId,
                -420f,
                180f);
            ShaderGraphTestAssets.RequirePackageReady(moveNodeResponse);

            Assert.That(ShaderGraphTestAssets.GetString(moveNodeResponse.Data, "operation"), Is.EqualTo("move_node"));
            Assert.That(ShaderGraphTestAssets.GetInt(moveNodeResponse.Data, "matchCount"), Is.EqualTo(1));

            var movedNode = ShaderGraphTestAssets.RequireDictionary(moveNodeResponse.Data, "movedNode");
            Assert.That(ShaderGraphTestAssets.GetString(movedNode, "objectId"), Is.EqualTo(nodeId));
            var position = ShaderGraphTestAssets.RequireDictionary(movedNode, "position");
            Assert.That(position["x"], Is.EqualTo(-420f));
            Assert.That(position["y"], Is.EqualTo(180f));

            ShaderGraphResponse findNodeResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                nodeId,
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findNodeResponse);

            var foundNode = ShaderGraphTestAssets.RequireDictionary(findNodeResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundNode, "objectId"), Is.EqualTo(nodeId));
            Assert.That(ShaderGraphTestAssets.GetString(foundNode, "summary"), Does.Contain("(-420, 180)"));
        }

        [Test]
        public void BlankGraph_DeleteNode_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDeleteNode", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Delete Source");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            string nodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(nodeId, Is.Not.Empty);

            ShaderGraphResponse deleteNodeResponse = ShaderGraphAssetTool.HandleDeleteNode(
                assetPath,
                nodeId);
            ShaderGraphTestAssets.RequirePackageReady(deleteNodeResponse);

            Assert.That(ShaderGraphTestAssets.GetString(deleteNodeResponse.Data, "operation"), Is.EqualTo("delete_node"));
            Assert.That(ShaderGraphTestAssets.GetInt(deleteNodeResponse.Data, "matchCount"), Is.EqualTo(1));

            var deletedNode = ShaderGraphTestAssets.RequireDictionary(deleteNodeResponse.Data, "deletedNode");
            Assert.That(ShaderGraphTestAssets.GetString(deletedNode, "objectId"), Is.EqualTo(nodeId));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(0));

            ShaderGraphResponse findNodeResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                nodeId,
                null,
                null);

            Assert.That(findNodeResponse.Success, Is.False);
            Assert.That(findNodeResponse.Message, Does.Contain("Could not find a graph node"));
        }

        [Test]
        public void BlankGraph_FindNode_ByIdAndDisplayName_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphFindNode", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Lookup Source");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            string nodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(nodeId, Is.Not.Empty);

            ShaderGraphResponse findByIdResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                nodeId,
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findByIdResponse);

            var foundById = ShaderGraphTestAssets.RequireDictionary(findByIdResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundById, "objectId"), Is.EqualTo(nodeId));
            Assert.That(ShaderGraphTestAssets.GetString(findByIdResponse.Data, "operation"), Is.EqualTo("find_node"));
            Assert.That(ShaderGraphTestAssets.GetInt(findByIdResponse.Data, "matchCount"), Is.EqualTo(1));

            string persistedDisplayName = ShaderGraphTestAssets.GetString(foundById, "displayName");
            string persistedNodeType = ShaderGraphTestAssets.GetString(foundById, "nodeType");
            Assert.That(persistedDisplayName, Is.Not.Empty);
            Assert.That(persistedNodeType, Is.Not.Empty);

            ShaderGraphResponse findByDisplayNameResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                null,
                persistedDisplayName,
                persistedNodeType);
            ShaderGraphTestAssets.RequirePackageReady(findByDisplayNameResponse);

            var foundByDisplayName = ShaderGraphTestAssets.RequireDictionary(findByDisplayNameResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "objectId"), Is.EqualTo(nodeId));
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "nodeType"), Is.EqualTo(persistedNodeType));
        }

        [Test]
        public void ListSupportedNodes_ReturnsPackageBackedCatalog()
        {
            ShaderGraphResponse response = ShaderGraphAssetTool.HandleListSupportedNodes();
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("list_supported_nodes"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "nodeCatalogSemantics"), Is.EqualTo("supported=graph-addable"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "supportedNodeCount"), Is.GreaterThan(0));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "discoveredNodeCount"), Is.GreaterThan(0));

            var supportedCanonicalNames = ShaderGraphTestAssets.GetStringList(response.Data, "supportedNodeCanonicalNames");
            Assert.That(supportedCanonicalNames, Has.Some.EqualTo("Float/Vector1"));
            Assert.That(supportedCanonicalNames, Has.Some.EqualTo("Color"));
        }

        [Test]
        public void BlankGraph_AddUnsupportedNodeType_ReturnsSupportedNodeHint()
        {
            string assetPath = CreateBlankGraph("BlankGraphUnsupportedNode", out _);

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleAddNode(assetPath, "DefinitelyNotARealNode", "Unsupported Node");

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Does.Contain("Unsupported Shader Graph node type"));
            Assert.That(response.Message, Does.Contain("Supported node types"));
            Assert.That(response.Message, Does.Contain("Float/Vector1"));
        }

        [Test]
        public void BlankGraph_ConnectPortsUnsupportedInputPort_ReturnsCanonicalPortHint()
        {
            string assetPath = CreateBlankGraph("BlankGraphUnsupportedPort", out _);

            ShaderGraphResponse sourceResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Source");
            ShaderGraphTestAssets.RequirePackageReady(sourceResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);
            Assert.That(sinkNodeId, Is.Not.Empty);

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceNodeId,
                "Out",
                sinkNodeId,
                "Bogus");

            Assert.That(response.Success, Is.False);
            Assert.That(response.Message, Does.Contain("Unsupported input port"));
            Assert.That(response.Message, Does.Contain("Supported input ports: 1, X."));
        }

        [Test]
        public void BlankGraph_AddTwoVector1Nodes_ThenConnectOutToX_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphVector1Connect", out _);

            ShaderGraphResponse addAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector1 A");
            ShaderGraphTestAssets.RequirePackageReady(addAResponse);
            string outputNodeId = ShaderGraphTestAssets.GetAddedNodeId(addAResponse);
            Assert.That(outputNodeId, Is.Not.Empty);

            ShaderGraphResponse addBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector1 B");
            ShaderGraphTestAssets.RequirePackageReady(addBResponse);
            string inputNodeId = ShaderGraphTestAssets.GetAddedNodeId(addBResponse);
            Assert.That(inputNodeId, Is.Not.Empty);

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                outputNodeId,
                "Out",
                inputNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            var resolvedConnection = ShaderGraphTestAssets.RequireDictionary(connectResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));

            var nodes = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "nodes");
            Assert.That(nodes, Has.Some.Contains("Vector1Node"));

            var connections = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "connections");
            Assert.That(connections.Count, Is.EqualTo(1));
        }

        [Test]
        public void BlankGraph_AddColorAndSplitNodes_ThenConnectOutToIn_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorSplitConnect", out _);

            ShaderGraphResponse addColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Color A");
            ShaderGraphTestAssets.RequirePackageReady(addColorResponse);
            string outputNodeId = ShaderGraphTestAssets.GetAddedNodeId(addColorResponse);
            Assert.That(outputNodeId, Is.Not.Empty);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split A");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string inputNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);
            Assert.That(inputNodeId, Is.Not.Empty);

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                outputNodeId,
                "Out",
                inputNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            var resolvedConnection = ShaderGraphTestAssets.RequireDictionary(connectResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));

            var nodes = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "nodes");
            Assert.That(nodes, Has.Some.Contains("ColorNode"));
            Assert.That(nodes, Has.Some.Contains("SplitNode"));

            var connections = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "connections");
            Assert.That(connections.Count, Is.EqualTo(1));
        }

        [Test]
        public void BlankGraph_AddColorSplitAndVector1Nodes_ThenConnectOutToInAndRToX_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorSplitVector1Connect", out _);

            ShaderGraphResponse addColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Color A");
            ShaderGraphTestAssets.RequirePackageReady(addColorResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(addColorResponse);
            Assert.That(colorNodeId, Is.Not.Empty);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split A");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse addVector1Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector1 A");
            ShaderGraphTestAssets.RequirePackageReady(addVector1Response);
            string vector1NodeId = ShaderGraphTestAssets.GetAddedNodeId(addVector1Response);
            Assert.That(vector1NodeId, Is.Not.Empty);

            ShaderGraphResponse colorToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(colorToSplitResponse);

            ShaderGraphResponse splitToVector1Response = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                vector1NodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToVector1Response);

            var resolvedConnection = ShaderGraphTestAssets.RequireDictionary(splitToVector1Response.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "outputSlotId"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedConnection, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(2));

            var nodes = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "nodes");
            Assert.That(nodes, Has.Some.Contains("ColorNode"));
            Assert.That(nodes, Has.Some.Contains("SplitNode"));
            Assert.That(nodes, Has.Some.Contains("Vector1Node"));

            var connections = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "connections");
            Assert.That(connections.Count, Is.EqualTo(2));
        }

        [Test]
        public void BlankGraph_AddPropertyAndNode_ThenSaveGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphSaveGraph", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Exposure",
                "Float/Vector1",
                "0");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector1 Save");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            ShaderGraphResponse saveResponse = ShaderGraphAssetTool.HandleSaveGraph(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(saveResponse);

            Assert.That(ShaderGraphTestAssets.GetString(saveResponse.Data, "operation"), Is.EqualTo("save_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(saveResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));

            var saveGraphStrategy = ShaderGraphTestAssets.GetStringList(saveResponse.Data, "saveGraphStrategy");
            Assert.That(saveGraphStrategy, Has.Member("GraphData.ValidateGraph()"));
            Assert.That(saveGraphStrategy, Has.Member("AssetDatabase.Refresh(ForceSynchronousImport)"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(0));
        }

        [Test]
        public void BlankGraph_AddPreferredCommonGraphAddableNodes_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphCommonNodeBatch", out _);
            var supportedCanonicalNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames();
            string[] selectedNodeTypes = PreferredCommonNodeTypes
                .Where(nodeType => supportedCanonicalNames.Contains(nodeType))
                .Take(4)
                .ToArray();

            Assert.That(
                selectedNodeTypes.Length,
                Is.GreaterThanOrEqualTo(3),
                "Expected at least three preferred common graph-addable nodes in the supported catalog.");

            for (int index = 0; index < selectedNodeTypes.Length; index += 1)
            {
                string nodeType = selectedNodeTypes[index];
                ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                    assetPath,
                    nodeType,
                    $"{nodeType} Sample");
                ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

                string addedNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
                Assert.That(addedNodeId, Is.Not.Empty, $"Expected objectId for node type '{nodeType}'.");

                var addedNode = ShaderGraphTestAssets.RequireDictionary(addNodeResponse.Data, "addedNode");
                Assert.That(
                    ShaderGraphTestAssets.GetString(addedNode, "resolvedNodeType"),
                    Is.EqualTo(nodeType));
            }

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(
                ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"),
                Is.EqualTo(selectedNodeTypes.Length));
        }

        [TestCaseSource(nameof(ArithmeticVector1ChainCases))]
        public void BlankGraph_Vector1ToArithmeticNodeThenBackToVector1_StaysPackageBacked(
            string arithmeticNodeType,
            string[] inputPorts,
            int expectedOutputSlotId)
        {
            string assetPath = CreateBlankGraph($"BlankGraph{arithmeticNodeType}Chain", out _);

            ShaderGraphResponse addArithmeticNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                arithmeticNodeType,
                $"{arithmeticNodeType} Node");
            ShaderGraphTestAssets.RequirePackageReady(addArithmeticNodeResponse);
            string arithmeticNodeId = ShaderGraphTestAssets.GetAddedNodeId(addArithmeticNodeResponse);
            Assert.That(arithmeticNodeId, Is.Not.Empty);

            ShaderGraphResponse addSinkVectorResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"{arithmeticNodeType} Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkVectorResponse);
            string sinkVectorNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkVectorResponse);
            Assert.That(sinkVectorNodeId, Is.Not.Empty);

            for (int index = 0; index < inputPorts.Length; index += 1)
            {
                string inputPort = inputPorts[index];

                ShaderGraphResponse addSourceVectorResponse = ShaderGraphAssetTool.HandleAddNode(
                    assetPath,
                    "Vector1",
                    $"{arithmeticNodeType} Source {inputPort}");
                ShaderGraphTestAssets.RequirePackageReady(addSourceVectorResponse);
                string sourceVectorNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceVectorResponse);
                Assert.That(sourceVectorNodeId, Is.Not.Empty);

                ShaderGraphResponse connectInputResponse = ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    sourceVectorNodeId,
                    "Out",
                    arithmeticNodeId,
                    inputPort);
                ShaderGraphTestAssets.RequirePackageReady(connectInputResponse);

                var resolvedInputConnection = ShaderGraphTestAssets.RequireDictionary(connectInputResponse.Data, "resolvedConnection");
                Assert.That(
                    ShaderGraphTestAssets.GetString(resolvedInputConnection, "outputNodeType"),
                    Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
                Assert.That(
                    ShaderGraphTestAssets.GetString(resolvedInputConnection, "inputNodeType"),
                    Is.EqualTo($"UnityEditor.ShaderGraph.{arithmeticNodeType}Node"));
            }

            ShaderGraphResponse connectOutputResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                arithmeticNodeId,
                "Out",
                sinkVectorNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(connectOutputResponse);

            var resolvedOutputConnection = ShaderGraphTestAssets.RequireDictionary(connectOutputResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(resolvedOutputConnection, "outputNodeType"),
                Is.EqualTo($"UnityEditor.ShaderGraph.{arithmeticNodeType}Node"));
            Assert.That(
                ShaderGraphTestAssets.GetString(resolvedOutputConnection, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(resolvedOutputConnection, "outputSlotId"),
                Is.EqualTo(expectedOutputSlotId));
            Assert.That(
                ShaderGraphTestAssets.GetInt(resolvedOutputConnection, "inputSlotId"),
                Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(
                ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"),
                Is.EqualTo(inputPorts.Length + 2));
            Assert.That(
                ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"),
                Is.EqualTo(inputPorts.Length + 1));
        }

        [Test]
        public void BlankGraph_AddToMultiplyArithmeticChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAddMultiplyChain", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Add", "Add Node");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);
            string addNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(addNodeId, Is.Not.Empty);

            ShaderGraphResponse multiplyNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Node");
            ShaderGraphTestAssets.RequirePackageReady(multiplyNodeResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyNodeResponse);
            Assert.That(multiplyNodeId, Is.Not.Empty);

            ShaderGraphResponse sourceAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Add A");
            ShaderGraphTestAssets.RequirePackageReady(sourceAResponse);
            string sourceANodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceAResponse);
            Assert.That(sourceANodeId, Is.Not.Empty);

            ShaderGraphResponse sourceBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Add B");
            ShaderGraphTestAssets.RequirePackageReady(sourceBResponse);
            string sourceBNodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceBResponse);
            Assert.That(sourceBNodeId, Is.Not.Empty);

            ShaderGraphResponse multiplyBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Multiply B");
            ShaderGraphTestAssets.RequirePackageReady(multiplyBResponse);
            string multiplyBNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyBResponse);
            Assert.That(multiplyBNodeId, Is.Not.Empty);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Chain Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);
            Assert.That(sinkNodeId, Is.Not.Empty);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", addNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", addNodeId, "B"));

            ShaderGraphResponse arithmeticChainResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                addNodeId,
                "Out",
                multiplyNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(arithmeticChainResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyBNodeId, "Out", multiplyNodeId, "B"));

            ShaderGraphResponse sinkConnectionResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(sinkConnectionResponse);

            var arithmeticResolved = ShaderGraphTestAssets.RequireDictionary(arithmeticChainResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(arithmeticResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AddNode"));
            Assert.That(
                ShaderGraphTestAssets.GetString(arithmeticResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(arithmeticResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetInt(arithmeticResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(sinkConnectionResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(5));
        }

        [Test]
        public void BlankGraph_ComparisonToBranchChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphComparisonBranchChain", out _);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);
            Assert.That(comparisonNodeId, Is.Not.Empty);

            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);
            Assert.That(branchNodeId, Is.Not.Empty);

            ShaderGraphResponse sourceAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphTestAssets.RequirePackageReady(sourceAResponse);
            string sourceANodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceAResponse);
            Assert.That(sourceANodeId, Is.Not.Empty);

            ShaderGraphResponse sourceBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphTestAssets.RequirePackageReady(sourceBResponse);
            string sourceBNodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceBResponse);
            Assert.That(sourceBNodeId, Is.Not.Empty);

            ShaderGraphResponse trueResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch True");
            ShaderGraphTestAssets.RequirePackageReady(trueResponse);
            string trueNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueResponse);
            Assert.That(trueNodeId, Is.Not.Empty);

            ShaderGraphResponse falseResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch False");
            ShaderGraphTestAssets.RequirePackageReady(falseResponse);
            string falseNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseResponse);
            Assert.That(falseNodeId, Is.Not.Empty);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);
            Assert.That(sinkNodeId, Is.Not.Empty);

            ShaderGraphResponse comparisonAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceANodeId,
                "Out",
                comparisonNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(comparisonAConnection);

            ShaderGraphResponse comparisonBConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceBNodeId,
                "Out",
                comparisonNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(comparisonBConnection);

            ShaderGraphResponse predicateConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                comparisonNodeId,
                "Out",
                branchNodeId,
                "Predicate");
            ShaderGraphTestAssets.RequirePackageReady(predicateConnection);

            ShaderGraphResponse trueConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                trueNodeId,
                "Out",
                branchNodeId,
                "True");
            ShaderGraphTestAssets.RequirePackageReady(trueConnection);

            ShaderGraphResponse falseConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                falseNodeId,
                "Out",
                branchNodeId,
                "False");
            ShaderGraphTestAssets.RequirePackageReady(falseConnection);

            ShaderGraphResponse sinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(sinkConnection);

            var predicateResolved = ShaderGraphTestAssets.RequireDictionary(predicateConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(predicateResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(
                ShaderGraphTestAssets.GetString(predicateResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(predicateResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetInt(predicateResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(sinkConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(3));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "inputSlotId"),
                Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_CombineToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphCombineSplitChain", out _);

            ShaderGraphResponse combineResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", "Combine Node");
            ShaderGraphTestAssets.RequirePackageReady(combineResponse);
            string combineNodeId = ShaderGraphTestAssets.GetAddedNodeId(combineResponse);
            Assert.That(combineNodeId, Is.Not.Empty);

            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse rResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine R");
            ShaderGraphResponse gResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine G");
            ShaderGraphResponse bResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine B");
            ShaderGraphResponse aResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine A");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine Sink");

            ShaderGraphTestAssets.RequirePackageReady(rResponse);
            ShaderGraphTestAssets.RequirePackageReady(gResponse);
            ShaderGraphTestAssets.RequirePackageReady(bResponse);
            ShaderGraphTestAssets.RequirePackageReady(aResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string rNodeId = ShaderGraphTestAssets.GetAddedNodeId(rResponse);
            string gNodeId = ShaderGraphTestAssets.GetAddedNodeId(gResponse);
            string bNodeId = ShaderGraphTestAssets.GetAddedNodeId(bResponse);
            string aNodeId = ShaderGraphTestAssets.GetAddedNodeId(aResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));

            ShaderGraphResponse combineToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                combineNodeId,
                "RGBA",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(combineToSplitResponse);

            ShaderGraphResponse splitToSinkResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkResponse);

            var combineResolved = ShaderGraphTestAssets.RequireDictionary(combineToSplitResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(combineResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.CombineNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(combineResolved, "outputSlotId"),
                Is.EqualTo(4));
            Assert.That(
                ShaderGraphTestAssets.GetString(combineResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(combineResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(1));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_Vector4ToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphVector4SplitChain", out _);

            ShaderGraphResponse vector4Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", "Vector4 Node");
            ShaderGraphTestAssets.RequirePackageReady(vector4Response);
            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(vector4Response);
            Assert.That(vector4NodeId, Is.Not.Empty);

            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse xResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 X");
            ShaderGraphResponse yResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Y");
            ShaderGraphResponse zResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Z");
            ShaderGraphResponse wResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 W");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Sink");

            ShaderGraphTestAssets.RequirePackageReady(xResponse);
            ShaderGraphTestAssets.RequirePackageReady(yResponse);
            ShaderGraphTestAssets.RequirePackageReady(zResponse);
            ShaderGraphTestAssets.RequirePackageReady(wResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string xNodeId = ShaderGraphTestAssets.GetAddedNodeId(xResponse);
            string yNodeId = ShaderGraphTestAssets.GetAddedNodeId(yResponse);
            string zNodeId = ShaderGraphTestAssets.GetAddedNodeId(zResponse);
            string wNodeId = ShaderGraphTestAssets.GetAddedNodeId(wResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));

            ShaderGraphResponse vector4ToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                vector4NodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(vector4ToSplitResponse);

            ShaderGraphResponse splitToSinkResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkResponse);

            var vector4Resolved = ShaderGraphTestAssets.RequireDictionary(vector4ToSplitResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(vector4Resolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector4Node"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(vector4Resolved, "outputSlotId"),
                Is.EqualTo(0));
            Assert.That(
                ShaderGraphTestAssets.GetString(vector4Resolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(vector4Resolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_CombineAppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphCombineAppendSplitChain", out _);

            ShaderGraphResponse combineResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", "Combine Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse rResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine R");
            ShaderGraphResponse gResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine G");
            ShaderGraphResponse bResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine B");
            ShaderGraphResponse aResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine A");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(combineResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(rResponse);
            ShaderGraphTestAssets.RequirePackageReady(gResponse);
            ShaderGraphTestAssets.RequirePackageReady(bResponse);
            ShaderGraphTestAssets.RequirePackageReady(aResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string combineNodeId = ShaderGraphTestAssets.GetAddedNodeId(combineResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string rNodeId = ShaderGraphTestAssets.GetAddedNodeId(rResponse);
            string gNodeId = ShaderGraphTestAssets.GetAddedNodeId(gResponse);
            string bNodeId = ShaderGraphTestAssets.GetAddedNodeId(bResponse);
            string aNodeId = ShaderGraphTestAssets.GetAddedNodeId(aResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));

            ShaderGraphResponse combineToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                combineNodeId,
                "RGBA",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(combineToAppend);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var combineResolved = ShaderGraphTestAssets.RequireDictionary(combineToAppend.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(combineResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.CombineNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(combineResolved, "outputSlotId"), Is.EqualTo(4));
            Assert.That(ShaderGraphTestAssets.GetString(combineResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(combineResolved, "inputSlotId"), Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(9));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(8));
        }

        [Test]
        public void BlankGraph_Vector4AppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphVector4AppendSplitChain", out _);

            ShaderGraphResponse vector4Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", "Vector4 Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse xResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 X");
            ShaderGraphResponse yResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Y");
            ShaderGraphResponse zResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Z");
            ShaderGraphResponse wResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 W");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(vector4Response);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(xResponse);
            ShaderGraphTestAssets.RequirePackageReady(yResponse);
            ShaderGraphTestAssets.RequirePackageReady(zResponse);
            ShaderGraphTestAssets.RequirePackageReady(wResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(vector4Response);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string xNodeId = ShaderGraphTestAssets.GetAddedNodeId(xResponse);
            string yNodeId = ShaderGraphTestAssets.GetAddedNodeId(yResponse);
            string zNodeId = ShaderGraphTestAssets.GetAddedNodeId(zResponse);
            string wNodeId = ShaderGraphTestAssets.GetAddedNodeId(wResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));

            ShaderGraphResponse vector4ToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                vector4NodeId,
                "Out",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(vector4ToAppend);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var vector4Resolved = ShaderGraphTestAssets.RequireDictionary(vector4ToAppend.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(vector4Resolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector4Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(vector4Resolved, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(vector4Resolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(vector4Resolved, "inputSlotId"), Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(9));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(8));
        }

        [Test]
        public void BlankGraph_CombineAppendLerpToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphCombineAppendLerpSplitChain", out _);

            ShaderGraphResponse combineResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", "Combine Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse lerpResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", "Lerp Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse rResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine R");
            ShaderGraphResponse gResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine G");
            ShaderGraphResponse bResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine B");
            ShaderGraphResponse aResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Combine A");
            ShaderGraphResponse appendScalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp Color B");
            ShaderGraphResponse tResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Lerp T");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Lerp Sink");

            ShaderGraphTestAssets.RequirePackageReady(combineResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(lerpResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(rResponse);
            ShaderGraphTestAssets.RequirePackageReady(gResponse);
            ShaderGraphTestAssets.RequirePackageReady(bResponse);
            ShaderGraphTestAssets.RequirePackageReady(aResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendScalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(tResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string combineNodeId = ShaderGraphTestAssets.GetAddedNodeId(combineResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string lerpNodeId = ShaderGraphTestAssets.GetAddedNodeId(lerpResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string rNodeId = ShaderGraphTestAssets.GetAddedNodeId(rResponse);
            string gNodeId = ShaderGraphTestAssets.GetAddedNodeId(gResponse);
            string bNodeId = ShaderGraphTestAssets.GetAddedNodeId(bResponse);
            string aNodeId = ShaderGraphTestAssets.GetAddedNodeId(aResponse);
            string appendScalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendScalarResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string tNodeId = ShaderGraphTestAssets.GetAddedNodeId(tResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, combineNodeId, "RGBA", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendScalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToLerp = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                lerpNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendToLerp);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));

            ShaderGraphResponse lerpToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                lerpNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(lerpToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToLerp.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            var lerpResolved = ShaderGraphTestAssets.RequireDictionary(lerpToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(12));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(11));
        }

        [Test]
        public void BlankGraph_Vector4AppendBranchToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphVector4AppendBranchSplitChain", out _);

            ShaderGraphResponse vector4Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", "Vector4 Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse xResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 X");
            ShaderGraphResponse yResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Y");
            ShaderGraphResponse zResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 Z");
            ShaderGraphResponse wResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Vector4 W");
            ShaderGraphResponse appendScalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse falseColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch False");
            ShaderGraphResponse compareAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Compare A");
            ShaderGraphResponse compareBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Compare B");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch Sink");

            ShaderGraphTestAssets.RequirePackageReady(vector4Response);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(xResponse);
            ShaderGraphTestAssets.RequirePackageReady(yResponse);
            ShaderGraphTestAssets.RequirePackageReady(zResponse);
            ShaderGraphTestAssets.RequirePackageReady(wResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendScalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(falseColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareAResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareBResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(vector4Response);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string xNodeId = ShaderGraphTestAssets.GetAddedNodeId(xResponse);
            string yNodeId = ShaderGraphTestAssets.GetAddedNodeId(yResponse);
            string zNodeId = ShaderGraphTestAssets.GetAddedNodeId(zResponse);
            string wNodeId = ShaderGraphTestAssets.GetAddedNodeId(wResponse);
            string appendScalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendScalarResponse);
            string falseColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseColorResponse);
            string compareANodeId = ShaderGraphTestAssets.GetAddedNodeId(compareAResponse);
            string compareBNodeId = ShaderGraphTestAssets.GetAddedNodeId(compareBResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            ShaderGraphTestAssets.RequirePackageReady(ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector4NodeId, "Out", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendScalarNodeId, "Out", appendNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));

            ShaderGraphResponse appendToBranch = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                branchNodeId,
                "True");
            ShaderGraphTestAssets.RequirePackageReady(appendToBranch);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));

            ShaderGraphResponse branchToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(branchToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToBranch.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(1));

            var branchResolved = ShaderGraphTestAssets.RequireDictionary(branchToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(14));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(13));
        }

        [Test]
        public void BlankGraph_ColorMultiplyToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorMultiplySplitChain", out _);

            ShaderGraphResponse multiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Node");
            ShaderGraphTestAssets.RequirePackageReady(multiplyResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyResponse);
            Assert.That(multiplyNodeId, Is.Not.Empty);

            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse colorAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Color A");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Color B");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Color Multiply Sink");

            ShaderGraphTestAssets.RequirePackageReady(colorAResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string colorANodeId = ShaderGraphTestAssets.GetAddedNodeId(colorAResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphResponse colorAToMultiplyResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorANodeId,
                "Out",
                multiplyNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorAToMultiplyResponse);

            ShaderGraphResponse colorBToMultiplyResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorBNodeId,
                "Out",
                multiplyNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(colorBToMultiplyResponse);

            ShaderGraphResponse multiplyToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(multiplyToSplitResponse);

            ShaderGraphResponse splitToSinkResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkResponse);

            var colorResolved = ShaderGraphTestAssets.RequireDictionary(colorAToMultiplyResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "outputSlotId"),
                Is.EqualTo(0));
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "inputSlotId"),
                Is.EqualTo(0));

            var multiplyResolved = ShaderGraphTestAssets.RequireDictionary(multiplyToSplitResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(multiplyResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(multiplyResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(multiplyResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(multiplyResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkResponse.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(4));
        }

        [Test]
        public void BlankGraph_ColorBranchToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorBranchSplitChain", out _);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);
            Assert.That(comparisonNodeId, Is.Not.Empty);

            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);
            Assert.That(branchNodeId, Is.Not.Empty);

            ShaderGraphResponse trueColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch True Color");
            ShaderGraphResponse falseColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch False Color");
            ShaderGraphResponse compareAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphResponse compareBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Color Branch Sink");

            ShaderGraphTestAssets.RequirePackageReady(trueColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(falseColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareAResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareBResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string trueColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueColorResponse);
            string falseColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseColorResponse);
            string compareANodeId = ShaderGraphTestAssets.GetAddedNodeId(compareAResponse);
            string compareBNodeId = ShaderGraphTestAssets.GetAddedNodeId(compareBResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));

            ShaderGraphResponse predicateConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                comparisonNodeId,
                "Out",
                branchNodeId,
                "Predicate");
            ShaderGraphTestAssets.RequirePackageReady(predicateConnection);

            ShaderGraphResponse trueColorConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                trueColorNodeId,
                "Out",
                branchNodeId,
                "True");
            ShaderGraphTestAssets.RequirePackageReady(trueColorConnection);

            ShaderGraphResponse falseColorConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                falseColorNodeId,
                "Out",
                branchNodeId,
                "False");
            ShaderGraphTestAssets.RequirePackageReady(falseColorConnection);

            ShaderGraphResponse branchToSplitConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(branchToSplitConnection);

            ShaderGraphResponse splitToSinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "B",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkConnection);

            var trueResolved = ShaderGraphTestAssets.RequireDictionary(trueColorConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(trueResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(trueResolved, "outputSlotId"),
                Is.EqualTo(0));
            Assert.That(
                ShaderGraphTestAssets.GetString(trueResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(trueResolved, "inputSlotId"),
                Is.EqualTo(1));

            var branchResolved = ShaderGraphTestAssets.RequireDictionary(branchToSplitConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(branchResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(branchResolved, "outputSlotId"),
                Is.EqualTo(3));
            Assert.That(
                ShaderGraphTestAssets.GetString(branchResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(branchResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(3));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_ColorLerpToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorLerpSplitChain", out _);

            ShaderGraphResponse lerpResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", "Lerp Node");
            ShaderGraphTestAssets.RequirePackageReady(lerpResponse);
            string lerpNodeId = ShaderGraphTestAssets.GetAddedNodeId(lerpResponse);
            Assert.That(lerpNodeId, Is.Not.Empty);

            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse colorAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp Color A");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp Color B");
            ShaderGraphResponse tResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Lerp T");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Color Lerp Sink");

            ShaderGraphTestAssets.RequirePackageReady(colorAResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(tResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string colorANodeId = ShaderGraphTestAssets.GetAddedNodeId(colorAResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string tNodeId = ShaderGraphTestAssets.GetAddedNodeId(tResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphResponse colorAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorANodeId,
                "Out",
                lerpNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorAConnection);

            ShaderGraphResponse colorBConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorBNodeId,
                "Out",
                lerpNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(colorBConnection);

            ShaderGraphResponse tConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                tNodeId,
                "Out",
                lerpNodeId,
                "T");
            ShaderGraphTestAssets.RequirePackageReady(tConnection);

            ShaderGraphResponse lerpToSplitConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                lerpNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(lerpToSplitConnection);

            ShaderGraphResponse splitToSinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkConnection);

            var colorResolved = ShaderGraphTestAssets.RequireDictionary(colorAConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "outputSlotId"),
                Is.EqualTo(0));
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "inputSlotId"),
                Is.EqualTo(0));

            var lerpResolved = ShaderGraphTestAssets.RequireDictionary(lerpToSplitConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(lerpResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(lerpResolved, "outputSlotId"),
                Is.EqualTo(3));
            Assert.That(
                ShaderGraphTestAssets.GetString(lerpResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(lerpResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(1));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(5));
        }

        [Test]
        public void BlankGraph_ColorAppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorAppendSplitChain", out _);

            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            Assert.That(appendNodeId, Is.Not.Empty);

            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            Assert.That(splitNodeId, Is.Not.Empty);

            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Color");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphResponse colorConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorNodeId,
                "Out",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorConnection);

            ShaderGraphResponse scalarConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                scalarNodeId,
                "Out",
                appendNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(scalarConnection);

            ShaderGraphResponse appendToSplitConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplitConnection);

            ShaderGraphResponse splitToSinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkConnection);

            var colorResolved = ShaderGraphTestAssets.RequireDictionary(colorConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "outputSlotId"),
                Is.EqualTo(0));
            Assert.That(
                ShaderGraphTestAssets.GetString(colorResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(colorResolved, "inputSlotId"),
                Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplitConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"),
                Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(splitToSinkConnection.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"),
                Is.EqualTo(1));
            Assert.That(
                ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(4));
        }

        [Test]
        public void BlankGraph_AppendChainToSplit_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAppendChainSplit", out _);

            ShaderGraphResponse appendAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append A");
            ShaderGraphResponse appendBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append B");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Chain Color");
            ShaderGraphResponse scalarAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Chain Scalar A");
            ShaderGraphResponse scalarBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Chain Scalar B");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Chain Sink");

            ShaderGraphTestAssets.RequirePackageReady(appendAResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendBResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarAResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarBResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string appendANodeId = ShaderGraphTestAssets.GetAddedNodeId(appendAResponse);
            string appendBNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendBResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarANodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarAResponse);
            string scalarBNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarBResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphResponse colorToAppendA = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorNodeId,
                "Out",
                appendANodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorToAppendA);

            ShaderGraphResponse scalarToAppendA = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                scalarANodeId,
                "Out",
                appendANodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(scalarToAppendA);

            ShaderGraphResponse appendToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendANodeId,
                "Out",
                appendBNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendToAppend);

            ShaderGraphResponse scalarToAppendB = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                scalarBNodeId,
                "Out",
                appendBNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(scalarToAppendB);

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendBNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendChainResolved = ShaderGraphTestAssets.RequireDictionary(appendToAppend.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(appendChainResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendChainResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(appendChainResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendChainResolved, "inputSlotId"),
                Is.EqualTo(0));

            var appendToSplitResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(
                ShaderGraphTestAssets.GetString(appendToSplitResolved, "outputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendToSplitResolved, "outputSlotId"),
                Is.EqualTo(2));
            Assert.That(
                ShaderGraphTestAssets.GetString(appendToSplitResolved, "inputNodeType"),
                Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(
                ShaderGraphTestAssets.GetInt(appendToSplitResolved, "inputSlotId"),
                Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_AppendMultiplyToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAppendMultiplySplitChain", out _);

            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse multiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Mix Color");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Mix Scalar");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Multiply Color");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Multiply Sink");

            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(multiplyResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToMultiply = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                multiplyNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendToMultiply);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", multiplyNodeId, "B"));

            ShaderGraphResponse multiplyToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(multiplyToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToMultiply.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            var multiplyResolved = ShaderGraphTestAssets.RequireDictionary(multiplyToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_AppendLerpToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAppendLerpSplitChain", out _);

            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse lerpResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", "Lerp Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Lerp Color");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Lerp Scalar");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp Color B");
            ShaderGraphResponse tResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Lerp T");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Lerp Sink");

            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(lerpResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(tResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string lerpNodeId = ShaderGraphTestAssets.GetAddedNodeId(lerpResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string tNodeId = ShaderGraphTestAssets.GetAddedNodeId(tResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToLerp = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                lerpNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendToLerp);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));

            ShaderGraphResponse lerpToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                lerpNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(lerpToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToLerp.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            var lerpResolved = ShaderGraphTestAssets.RequireDictionary(lerpToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_AppendBranchToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAppendBranchSplitChain", out _);

            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Branch Color");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Branch Scalar");
            ShaderGraphResponse falseColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch False Color");
            ShaderGraphResponse compareAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphResponse compareBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Branch Sink");

            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(falseColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareAResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareBResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string falseColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseColorResponse);
            string compareANodeId = ShaderGraphTestAssets.GetAddedNodeId(compareAResponse);
            string compareBNodeId = ShaderGraphTestAssets.GetAddedNodeId(compareBResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));

            ShaderGraphResponse appendToBranch = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                branchNodeId,
                "True");
            ShaderGraphTestAssets.RequirePackageReady(appendToBranch);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));

            ShaderGraphResponse branchToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(branchToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "B",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToBranch.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(1));

            var branchResolved = ShaderGraphTestAssets.RequireDictionary(branchToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(10));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(9));
        }

        [Test]
        public void BlankGraph_MultiplyAppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMultiplyAppendSplitChain", out _);

            ShaderGraphResponse multiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Multiply A");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Multiply B");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(multiplyResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorAResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorANodeId = ShaderGraphTestAssets.GetAddedNodeId(colorAResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", multiplyNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", multiplyNodeId, "B"));

            ShaderGraphResponse multiplyToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(multiplyToAppend);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var multiplyResolved = ShaderGraphTestAssets.RequireDictionary(multiplyToAppend.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "inputSlotId"), Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_LerpAppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphLerpAppendSplitChain", out _);

            ShaderGraphResponse lerpResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", "Lerp Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse colorAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp A");
            ShaderGraphResponse colorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Lerp B");
            ShaderGraphResponse tResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Lerp T");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(lerpResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorAResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorBResponse);
            ShaderGraphTestAssets.RequirePackageReady(tResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string lerpNodeId = ShaderGraphTestAssets.GetAddedNodeId(lerpResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string colorANodeId = ShaderGraphTestAssets.GetAddedNodeId(colorAResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorBResponse);
            string tNodeId = ShaderGraphTestAssets.GetAddedNodeId(tResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", lerpNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));

            ShaderGraphResponse lerpToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                lerpNodeId,
                "Out",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(lerpToAppend);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "R",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var lerpResolved = ShaderGraphTestAssets.RequireDictionary(lerpToAppend.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.LerpNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(lerpResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(lerpResolved, "inputSlotId"), Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_BranchAppendToSplitChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphBranchAppendSplitChain", out _);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Node");
            ShaderGraphResponse splitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Split Node");
            ShaderGraphResponse trueColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch True");
            ShaderGraphResponse falseColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Branch False");
            ShaderGraphResponse compareAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphResponse compareBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Sink");

            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(splitResponse);
            ShaderGraphTestAssets.RequirePackageReady(trueColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(falseColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareAResponse);
            ShaderGraphTestAssets.RequirePackageReady(compareBResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);

            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(splitResponse);
            string trueColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueColorResponse);
            string falseColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseColorResponse);
            string compareANodeId = ShaderGraphTestAssets.GetAddedNodeId(compareAResponse);
            string compareBNodeId = ShaderGraphTestAssets.GetAddedNodeId(compareBResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueColorNodeId, "Out", branchNodeId, "True"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));

            ShaderGraphResponse branchToAppend = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(branchToAppend);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                splitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse splitToSink = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                splitNodeId,
                "B",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSink);

            var branchResolved = ShaderGraphTestAssets.RequireDictionary(branchToAppend.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(branchResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(branchResolved, "inputSlotId"), Is.EqualTo(0));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(10));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(9));
        }

        private string CreateBlankGraph(string graphNamePrefix, out ShaderGraphResponse createResponse)
        {
            string graphName = $"{graphNamePrefix}_{Guid.NewGuid():N}";
            createResponse = ShaderGraphAssetTool.HandleCreateGraph(
                graphName,
                temporaryFolder.AssetPath,
                "blank");
            ShaderGraphTestAssets.RequirePackageReady(createResponse);

            string assetPath = ShaderGraphTestAssets.GetString(createResponse.Data, "assetPath");
            Assert.That(assetPath, Does.EndWith(".shadergraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);
            return assetPath;
        }
    }
}
