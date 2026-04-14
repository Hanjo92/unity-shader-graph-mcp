using System;
using System.Collections.Generic;
using System.IO;
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
        public void BlankCreateSubGraph_ThenReadSummary_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankCreateSubGraphReadSummary", out ShaderGraphResponse createResponse);

            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "template"), Is.EqualTo("blank"));
            Assert.That(ShaderGraphTestAssets.GetString(createResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(createResponse.Data["isSubGraph"], Is.EqualTo(true));

            var createdSubGraph = ShaderGraphTestAssets.RequireDictionary(createResponse.Data, "createdSubGraph");
            Assert.That(ShaderGraphTestAssets.GetString(createdSubGraph, "resolvedTemplate"), Is.EqualTo("blank"));
            Assert.That(ShaderGraphTestAssets.GetString(createdSubGraph, "graphPathLabel"), Is.EqualTo("Sub Graphs"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.GreaterThanOrEqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(0));
        }

        [Test]
        public void BlankSubGraph_ReadSubGraphSummary_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphReadSummary", out ShaderGraphResponse createResponse);

            ShaderGraphTestAssets.RequirePackageReady(createResponse);
            Assert.That(createResponse.Data["isSubGraph"], Is.EqualTo(true));

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleReadSubGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("read_subgraph_summary"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(response.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "categoryCount"), Is.GreaterThanOrEqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "nodeCount"), Is.GreaterThanOrEqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "connectionCount"), Is.GreaterThanOrEqualTo(0));
        }

        [Test]
        public void BlankSubGraph_RenameSubGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphRenameSubGraph", out _);
            string previousAssetName = Path.GetFileNameWithoutExtension(assetPath);

            ShaderGraphResponse renameResponse = ShaderGraphAssetTool.HandleRenameSubGraph(
                assetPath,
                "Renamed Blank Sub Graph");
            ShaderGraphTestAssets.RequirePackageReady(renameResponse);

            Assert.That(ShaderGraphTestAssets.GetString(renameResponse.Data, "operation"), Is.EqualTo("rename_subgraph"));
            Assert.That(ShaderGraphTestAssets.GetString(renameResponse.Data, "previousAssetPath"), Is.EqualTo(assetPath));
            Assert.That(renameResponse.Data["isSubGraph"], Is.EqualTo(true));

            string renamedAssetPath = ShaderGraphTestAssets.GetString(renameResponse.Data, "assetPath");
            Assert.That(renamedAssetPath, Does.EndWith("Renamed Blank Sub Graph.shadersubgraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(renamedAssetPath), Is.Not.Null);

            var renamedSubGraph = ShaderGraphTestAssets.RequireDictionary(renameResponse.Data, "renamedSubGraph");
            Assert.That(ShaderGraphTestAssets.GetString(renamedSubGraph, "assetPath"), Is.EqualTo(renamedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(renamedSubGraph, "assetName"), Is.EqualTo("Renamed Blank Sub Graph"));
            Assert.That(ShaderGraphTestAssets.GetString(renamedSubGraph, "previousAssetName"), Is.EqualTo(previousAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(renamedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(renamedAssetPath));
        }

        [Test]
        public void BlankSubGraph_DuplicateSubGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphDuplicateSubGraph", out _);
            string sourceAssetName = Path.GetFileNameWithoutExtension(assetPath);

            ShaderGraphResponse duplicateResponse = ShaderGraphAssetTool.HandleDuplicateSubGraph(
                assetPath,
                "Copied Blank Sub Graph");
            ShaderGraphTestAssets.RequirePackageReady(duplicateResponse);

            Assert.That(ShaderGraphTestAssets.GetString(duplicateResponse.Data, "operation"), Is.EqualTo("duplicate_subgraph"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicateResponse.Data, "sourceAssetPath"), Is.EqualTo(assetPath));
            Assert.That(duplicateResponse.Data["isSubGraph"], Is.EqualTo(true));

            string duplicatedAssetPath = ShaderGraphTestAssets.GetString(duplicateResponse.Data, "assetPath");
            Assert.That(duplicatedAssetPath, Does.EndWith("Copied Blank Sub Graph.shadersubgraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(duplicatedAssetPath), Is.Not.Null);

            var duplicatedSubGraph = ShaderGraphTestAssets.RequireDictionary(duplicateResponse.Data, "duplicatedSubGraph");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedSubGraph, "assetPath"), Is.EqualTo(duplicatedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedSubGraph, "assetName"), Is.EqualTo("Copied Blank Sub Graph"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedSubGraph, "sourceAssetName"), Is.EqualTo(sourceAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(duplicatedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(duplicatedAssetPath));
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
        public void BlankGraph_RenameGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRenameGraph", out _);
            string previousAssetName = Path.GetFileNameWithoutExtension(assetPath);

            ShaderGraphResponse renameResponse = ShaderGraphAssetTool.HandleRenameGraph(
                assetPath,
                "Renamed Blank Graph");
            ShaderGraphTestAssets.RequirePackageReady(renameResponse);

            Assert.That(ShaderGraphTestAssets.GetString(renameResponse.Data, "operation"), Is.EqualTo("rename_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(renameResponse.Data, "previousAssetPath"), Is.EqualTo(assetPath));

            string renamedAssetPath = ShaderGraphTestAssets.GetString(renameResponse.Data, "assetPath");
            Assert.That(renamedAssetPath, Does.EndWith("Renamed Blank Graph.shadergraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(renamedAssetPath), Is.Not.Null);

            var renamedGraph = ShaderGraphTestAssets.RequireDictionary(renameResponse.Data, "renamedGraph");
            Assert.That(ShaderGraphTestAssets.GetString(renamedGraph, "assetPath"), Is.EqualTo(renamedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(renamedGraph, "assetName"), Is.EqualTo("Renamed Blank Graph"));
            Assert.That(ShaderGraphTestAssets.GetString(renamedGraph, "previousAssetName"), Is.EqualTo(previousAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(renamedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(renamedAssetPath));
        }

        [Test]
        public void BlankGraph_DuplicateGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDuplicateGraph", out _);
            string sourceAssetName = Path.GetFileNameWithoutExtension(assetPath);

            ShaderGraphResponse duplicateResponse = ShaderGraphAssetTool.HandleDuplicateGraph(
                assetPath,
                "Copied Blank Graph");
            ShaderGraphTestAssets.RequirePackageReady(duplicateResponse);

            Assert.That(ShaderGraphTestAssets.GetString(duplicateResponse.Data, "operation"), Is.EqualTo("duplicate_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicateResponse.Data, "sourceAssetPath"), Is.EqualTo(assetPath));

            string duplicatedAssetPath = ShaderGraphTestAssets.GetString(duplicateResponse.Data, "assetPath");
            Assert.That(duplicatedAssetPath, Does.EndWith("Copied Blank Graph.shadergraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(duplicatedAssetPath), Is.Not.Null);

            var duplicatedGraph = ShaderGraphTestAssets.RequireDictionary(duplicateResponse.Data, "duplicatedGraph");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedGraph, "assetPath"), Is.EqualTo(duplicatedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedGraph, "assetName"), Is.EqualTo("Copied Blank Graph"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedGraph, "sourceAssetName"), Is.EqualTo(sourceAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(duplicatedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(duplicatedAssetPath));
        }

        [Test]
        public void BlankGraph_DeleteGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDeleteGraph", out _);

            ShaderGraphResponse deleteResponse = ShaderGraphAssetTool.HandleDeleteGraph(assetPath);
            Assert.That(deleteResponse.Success, Is.True, deleteResponse.Message);
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "operation"), Is.EqualTo("delete_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(deleteResponse.Data.ContainsKey("exists"), Is.True);
            Assert.That(deleteResponse.Data["exists"], Is.EqualTo(false));

            var deletedGraph = ShaderGraphTestAssets.RequireDictionary(deleteResponse.Data, "deletedGraph");
            Assert.That(ShaderGraphTestAssets.GetString(deletedGraph, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            Assert.That(summaryResponse.Success, Is.False);
            Assert.That(summaryResponse.Message, Does.Contain("not found"));
        }

        [Test]
        public void BlankSubGraph_DeleteSubGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphDeleteSubGraph", out _);

            ShaderGraphResponse deleteResponse = ShaderGraphAssetTool.HandleDeleteSubGraph(assetPath);
            Assert.That(deleteResponse.Success, Is.True, deleteResponse.Message);
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "operation"), Is.EqualTo("delete_subgraph"));
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(deleteResponse.Data, "executionBackendKind"), Is.EqualTo("PackageBacked"));
            Assert.That(deleteResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(deleteResponse.Data.ContainsKey("exists"), Is.True);
            Assert.That(deleteResponse.Data["exists"], Is.EqualTo(false));

            var deletedSubGraph = ShaderGraphTestAssets.RequireDictionary(deleteResponse.Data, "deletedSubGraph");
            Assert.That(ShaderGraphTestAssets.GetString(deletedSubGraph, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(assetPath);
            Assert.That(summaryResponse.Success, Is.False);
            Assert.That(summaryResponse.Message, Does.Contain("not found"));
        }

        [Test]
        public void BlankGraph_MoveGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMoveGraph", out _);
            string previousAssetName = Path.GetFileNameWithoutExtension(assetPath);
            string movedAssetPath = Path.Combine(
                temporaryFolder.AssetPath,
                "Moved",
                Path.GetFileName(assetPath)).Replace('\\', '/');

            ShaderGraphResponse moveResponse = ShaderGraphAssetTool.HandleMoveGraph(assetPath, movedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(moveResponse);

            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "operation"), Is.EqualTo("move_graph"));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "previousAssetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "assetPath"), Is.EqualTo(movedAssetPath));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(movedAssetPath), Is.Not.Null);

            var movedGraph = ShaderGraphTestAssets.RequireDictionary(moveResponse.Data, "movedGraph");
            Assert.That(ShaderGraphTestAssets.GetString(movedGraph, "assetPath"), Is.EqualTo(movedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(movedGraph, "assetName"), Is.EqualTo(previousAssetName));
            Assert.That(ShaderGraphTestAssets.GetString(movedGraph, "previousAssetName"), Is.EqualTo(previousAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(movedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(movedAssetPath));
        }

        [Test]
        public void BlankSubGraph_MoveSubGraph_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphMoveSubGraph", out _);
            string previousAssetName = Path.GetFileNameWithoutExtension(assetPath);
            string movedAssetPath = Path.Combine(
                temporaryFolder.AssetPath,
                "MovedSubGraphs",
                Path.GetFileName(assetPath)).Replace('\\', '/');

            ShaderGraphResponse moveResponse = ShaderGraphAssetTool.HandleMoveSubGraph(assetPath, movedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(moveResponse);

            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "operation"), Is.EqualTo("move_subgraph"));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "previousAssetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "assetPath"), Is.EqualTo(movedAssetPath));
            Assert.That(moveResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Null);
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(movedAssetPath), Is.Not.Null);

            var movedSubGraph = ShaderGraphTestAssets.RequireDictionary(moveResponse.Data, "movedSubGraph");
            Assert.That(ShaderGraphTestAssets.GetString(movedSubGraph, "assetPath"), Is.EqualTo(movedAssetPath));
            Assert.That(ShaderGraphTestAssets.GetString(movedSubGraph, "assetName"), Is.EqualTo(previousAssetName));
            Assert.That(ShaderGraphTestAssets.GetString(movedSubGraph, "previousAssetName"), Is.EqualTo(previousAssetName));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(movedAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "assetPath"), Is.EqualTo(movedAssetPath));
        }

        [Test]
        public void BlankSubGraph_Vector4ToOutput_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphVector4ToOutput", out _);

            ShaderGraphResponse addVector4Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", "Sub Graph Vector4");
            ShaderGraphTestAssets.RequirePackageReady(addVector4Response);

            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(addVector4Response);
            Assert.That(vector4NodeId, Is.Not.Empty);

            ShaderGraphResponse findOutputNodeResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                null,
                null,
                "SubGraphOutput");
            ShaderGraphTestAssets.RequirePackageReady(findOutputNodeResponse);

            var foundOutputNode = ShaderGraphTestAssets.RequireDictionary(findOutputNodeResponse.Data, "foundNode");
            string outputNodeId = ShaderGraphTestAssets.GetString(foundOutputNode, "objectId");
            Assert.That(outputNodeId, Is.Not.Empty);
            Assert.That(ShaderGraphTestAssets.GetString(foundOutputNode, "nodeType"), Is.EqualTo("SubGraphOutput"));

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                vector4NodeId,
                "Out",
                outputNodeId,
                "Out");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            Assert.That(ShaderGraphTestAssets.GetString(connectResponse.Data, "operation"), Is.EqualTo("connect_ports"));
            var resolvedConnection = ShaderGraphTestAssets.RequireDictionary(connectResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector4Node"));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SubGraphOutputNode"));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedConnection, "inputPort"), Is.EqualTo("Out"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.GreaterThanOrEqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
        }

        [Test]
        public void BlankSubGraph_ExportGraphContract_StaysPackageBacked()
        {
            string assetPath = CreateBlankSubGraph("BlankSubGraphExportGraphContract", out _);

            ShaderGraphResponse addVector4Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", "Sub Graph Contract Source");
            ShaderGraphTestAssets.RequirePackageReady(addVector4Response);
            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(addVector4Response);

            ShaderGraphResponse findOutputNodeResponse = ShaderGraphAssetTool.HandleFindNode(assetPath, null, null, "SubGraphOutput");
            ShaderGraphTestAssets.RequirePackageReady(findOutputNodeResponse);
            string outputNodeId = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(findOutputNodeResponse.Data, "foundNode"),
                "objectId");

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                vector4NodeId,
                "Out",
                outputNodeId,
                "Out");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            ShaderGraphResponse exportResponse = ShaderGraphAssetTool.HandleExportGraphContract(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(exportResponse);

            Assert.That(ShaderGraphTestAssets.GetString(exportResponse.Data, "operation"), Is.EqualTo("export_graph_contract"));
            Assert.That(exportResponse.Data["isSubGraph"], Is.EqualTo(true));

            var exportedGraphContract = ShaderGraphTestAssets.RequireDictionary(exportResponse.Data, "exportedGraphContract");
            Assert.That(exportedGraphContract["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "propertyCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "connectionCount"), Is.EqualTo(1));
        }

        [Test]
        public void BlankSubGraph_ImportGraphContract_StaysPackageBacked()
        {
            string sourceAssetPath = CreateBlankSubGraph("BlankSubGraphImportGraphContractSource", out _);

            ShaderGraphResponse addVector4Response = ShaderGraphAssetTool.HandleAddNode(sourceAssetPath, "Vector4", "Imported Sub Graph Source");
            ShaderGraphTestAssets.RequirePackageReady(addVector4Response);
            string vector4NodeId = ShaderGraphTestAssets.GetAddedNodeId(addVector4Response);

            ShaderGraphResponse findOutputNodeResponse = ShaderGraphAssetTool.HandleFindNode(sourceAssetPath, null, null, "SubGraphOutput");
            ShaderGraphTestAssets.RequirePackageReady(findOutputNodeResponse);
            string outputNodeId = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(findOutputNodeResponse.Data, "foundNode"),
                "objectId");

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                sourceAssetPath,
                vector4NodeId,
                "Out",
                outputNodeId,
                "Out");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            ShaderGraphResponse exportResponse = ShaderGraphAssetTool.HandleExportGraphContract(sourceAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(exportResponse);
            IReadOnlyDictionary<string, object> exportedGraphContract = ShaderGraphTestAssets.RequireDictionary(exportResponse.Data, "exportedGraphContract");

            string targetAssetPath = CreateBlankSubGraph("BlankSubGraphImportGraphContractTarget", out _);
            ShaderGraphResponse importResponse = ShaderGraphAssetTool.HandleImportGraphContract(
                targetAssetPath,
                ShaderGraphTestAssets.SerializeToJson(exportedGraphContract));
            ShaderGraphTestAssets.RequirePackageReady(importResponse);

            Assert.That(ShaderGraphTestAssets.GetString(importResponse.Data, "operation"), Is.EqualTo("import_graph_contract"));
            Assert.That(importResponse.Data["isSubGraph"], Is.EqualTo(true));

            var importedCounts = ShaderGraphTestAssets.RequireDictionary(importResponse.Data, "importedCounts");
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "categoryCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "propertyCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "connectionCount"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadSubGraphSummary(targetAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(summaryResponse.Data["isSubGraph"], Is.EqualTo(true));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
        }

        [Test]
        public void BlankGraph_SetGraphMetadata_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphSetGraphMetadata", out _);

            ShaderGraphResponse setMetadataResponse = ShaderGraphAssetTool.HandleSetGraphMetadata(
                assetPath,
                "Material Inputs",
                "Half");
            ShaderGraphTestAssets.RequirePackageReady(setMetadataResponse);

            Assert.That(ShaderGraphTestAssets.GetString(setMetadataResponse.Data, "operation"), Is.EqualTo("set_graph_metadata"));

            var updatedMetadata = ShaderGraphTestAssets.RequireDictionary(setMetadataResponse.Data, "updatedMetadata");
            Assert.That(ShaderGraphTestAssets.GetString(updatedMetadata, "graphPathLabel"), Is.EqualTo("Material Inputs"));
            Assert.That(ShaderGraphTestAssets.GetString(updatedMetadata, "graphDefaultPrecision"), Is.EqualTo("Half"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "graphPathLabel"), Is.EqualTo("Material Inputs"));
            Assert.That(ShaderGraphTestAssets.GetString(summaryResponse.Data, "graphDefaultPrecision"), Is.EqualTo("Half"));
        }

        [Test]
        public void BlankGraph_ExportGraphContract_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphExportGraphContract", out _);

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse movePropertyResponse = ShaderGraphAssetTool.HandleMovePropertyToCategory(
                assetPath,
                "Tint",
                string.Empty,
                "Surface Inputs",
                0);
            ShaderGraphTestAssets.RequirePackageReady(movePropertyResponse);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Contract Source");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            ShaderGraphResponse exportResponse = ShaderGraphAssetTool.HandleExportGraphContract(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(exportResponse);

            Assert.That(ShaderGraphTestAssets.GetString(exportResponse.Data, "operation"), Is.EqualTo("export_graph_contract"));
            Assert.That(ShaderGraphTestAssets.GetString(exportResponse.Data, "contractVersion"), Is.EqualTo("unity-shader-graph-mcp/export-graph-contract-v1"));

            var exportedGraphContract = ShaderGraphTestAssets.RequireDictionary(exportResponse.Data, "exportedGraphContract");
            Assert.That(ShaderGraphTestAssets.GetString(exportedGraphContract, "assetPath"), Is.EqualTo(assetPath));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "categoryCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(exportedGraphContract, "nodeCount"), Is.EqualTo(1));
            Assert.That(exportedGraphContract.ContainsKey("categories"), Is.True);
            Assert.That(exportedGraphContract.ContainsKey("properties"), Is.True);
            Assert.That(exportedGraphContract.ContainsKey("nodes"), Is.True);
            Assert.That(exportedGraphContract.ContainsKey("connections"), Is.True);
        }

        [Test]
        public void BlankGraph_ImportGraphContract_StaysPackageBacked()
        {
            string sourceAssetPath = CreateBlankGraph("BlankGraphImportGraphContractSource", out _);

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(sourceAssetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                sourceAssetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse movePropertyResponse = ShaderGraphAssetTool.HandleMovePropertyToCategory(
                sourceAssetPath,
                "Tint",
                string.Empty,
                "Surface Inputs",
                0);
            ShaderGraphTestAssets.RequirePackageReady(movePropertyResponse);

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(sourceAssetPath, "Vector1", "Import Source");
            ShaderGraphTestAssets.RequirePackageReady(addSourceNodeResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceNodeResponse);

            ShaderGraphResponse addTargetNodeResponse = ShaderGraphAssetTool.HandleAddNode(sourceAssetPath, "Vector1", "Import Target");
            ShaderGraphTestAssets.RequirePackageReady(addTargetNodeResponse);
            string targetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTargetNodeResponse);

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                sourceAssetPath,
                sourceNodeId,
                "Out",
                targetNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            ShaderGraphResponse exportResponse = ShaderGraphAssetTool.HandleExportGraphContract(sourceAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(exportResponse);
            IReadOnlyDictionary<string, object> exportedGraphContract = ShaderGraphTestAssets.RequireDictionary(exportResponse.Data, "exportedGraphContract");

            string targetAssetPath = CreateBlankGraph("BlankGraphImportGraphContractTarget", out _);
            ShaderGraphResponse importResponse = ShaderGraphAssetTool.HandleImportGraphContract(
                targetAssetPath,
                ShaderGraphTestAssets.SerializeToJson(exportedGraphContract));
            ShaderGraphTestAssets.RequirePackageReady(importResponse);

            Assert.That(ShaderGraphTestAssets.GetString(importResponse.Data, "operation"), Is.EqualTo("import_graph_contract"));

            var importedCounts = ShaderGraphTestAssets.RequireDictionary(importResponse.Data, "importedCounts");
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "categoryCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(importedCounts, "connectionCount"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(targetAssetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "categoryCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "properties"), Has.Some.Contains("Tint"));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Sample Split");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphResponse uvToSampleResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                uvNodeId,
                "Out",
                sampleNodeId,
                "UV");
            ShaderGraphTestAssets.RequirePackageReady(uvToSampleResponse);

            ShaderGraphResponse textureToSampleResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                textureNodeId,
                "Out",
                sampleNodeId,
                "Texture");
            ShaderGraphTestAssets.RequirePackageReady(textureToSampleResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", splitNodeId, "In"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            var uvResolved = ShaderGraphTestAssets.RequireDictionary(uvToSampleResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(uvResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.UVNode"));
            Assert.That(ShaderGraphTestAssets.GetString(uvResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(uvResolved, "inputSlotId"), Is.EqualTo(2));

            var textureResolved = ShaderGraphTestAssets.RequireDictionary(textureToSampleResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(textureResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Texture2DAssetNode"));
            Assert.That(ShaderGraphTestAssets.GetString(textureResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(textureResolved, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(4));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DColorWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DColorWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Sample Tint");
            ShaderGraphTestAssets.RequirePackageReady(addColorResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(addColorResponse);

            ShaderGraphResponse addMultiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Sample Multiply");
            ShaderGraphTestAssets.RequirePackageReady(addMultiplyResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(addMultiplyResponse);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Sample Split");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", multiplyNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", multiplyNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", splitNodeId, "In"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DChannelWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DChannelWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "R", sinkNodeId, "X"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(4));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DChannelAppendWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DChannelAppendWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addAppendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Sample Append");
            ShaderGraphTestAssets.RequirePackageReady(addAppendResponse);
            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(addAppendResponse);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Sample Split");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));

            ShaderGraphResponse appendAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "R",
                appendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendAConnection);

            ShaderGraphResponse appendBConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "G",
                appendNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(appendBConnection);

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
                "G",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(splitToSinkConnection);

            var appendAResolved = ShaderGraphTestAssets.RequireDictionary(appendAConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendAResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendAResolved, "outputSlotId"), Is.EqualTo(4));
            Assert.That(ShaderGraphTestAssets.GetString(appendAResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendAResolved, "inputSlotId"), Is.EqualTo(0));

            var appendBResolved = ShaderGraphTestAssets.RequireDictionary(appendBConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendBResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendBResolved, "outputSlotId"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetString(appendBResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendBResolved, "inputSlotId"), Is.EqualTo(1));

            var appendResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplitConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(appendResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(appendResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DNormalStrengthWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DNormalStrengthWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addStrengthResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Strength");
            ShaderGraphTestAssets.RequirePackageReady(addStrengthResponse);
            string strengthNodeId = ShaderGraphTestAssets.GetAddedNodeId(addStrengthResponse);

            ShaderGraphResponse addNormalStrengthResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalStrength", "Sample NormalStrength");
            ShaderGraphTestAssets.RequirePackageReady(addNormalStrengthResponse);
            string normalStrengthNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalStrengthResponse);

            ShaderGraphResponse addSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Sample Split");
            ShaderGraphTestAssets.RequirePackageReady(addSplitResponse);
            string splitNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSplitResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", normalStrengthNodeId, "In"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, strengthNodeId, "Out", normalStrengthNodeId, "Strength"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, normalStrengthNodeId, "Out", splitNodeId, "In"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "B", sinkNodeId, "X"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DNormalUnpackWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DNormalUnpackWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addNormalUnpackResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalUnpack", "Sample NormalUnpack");
            ShaderGraphTestAssets.RequirePackageReady(addNormalUnpackResponse);
            string normalUnpackNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalUnpackResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", normalUnpackNodeId, "In"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(4));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DNormalBlendWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DNormalBlendWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D A");
            ShaderGraphTestAssets.RequirePackageReady(addSampleAResponse);
            string sampleANodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleAResponse);

            ShaderGraphResponse addSampleBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D B");
            ShaderGraphTestAssets.RequirePackageReady(addSampleBResponse);
            string sampleBNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleBResponse);

            ShaderGraphResponse addNormalBlendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalBlend", "Sample NormalBlend");
            ShaderGraphTestAssets.RequirePackageReady(addNormalBlendResponse);
            string normalBlendNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalBlendResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleANodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleBNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleANodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleBNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleANodeId, "RGBA", normalBlendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleBNodeId, "RGBA", normalBlendNodeId, "B"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(7));
        }

        [Test]
        public void BlankGraph_ColorNormalBlendWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorNormalBlendWorkflow", out _);

            ShaderGraphResponse addColorAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Normal Blend A");
            ShaderGraphTestAssets.RequirePackageReady(addColorAResponse);
            string colorANodeId = ShaderGraphTestAssets.GetAddedNodeId(addColorAResponse);

            ShaderGraphResponse addColorBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Normal Blend B");
            ShaderGraphTestAssets.RequirePackageReady(addColorBResponse);
            string colorBNodeId = ShaderGraphTestAssets.GetAddedNodeId(addColorBResponse);

            ShaderGraphResponse addNormalBlendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalBlend", "Color Normal Blend");
            ShaderGraphTestAssets.RequirePackageReady(addNormalBlendResponse);
            string normalBlendNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalBlendResponse);

            ShaderGraphResponse colorAToNormalBlendResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorANodeId,
                "Out",
                normalBlendNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorAToNormalBlendResponse);

            ShaderGraphResponse colorBToNormalBlendResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorBNodeId,
                "Out",
                normalBlendNodeId,
                "B");
            ShaderGraphTestAssets.RequirePackageReady(colorBToNormalBlendResponse);

            var resolvedA = ShaderGraphTestAssets.RequireDictionary(colorAToNormalBlendResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedA, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedA, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedA, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.NormalBlendNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedA, "inputSlotId"), Is.EqualTo(0));

            var resolvedB = ShaderGraphTestAssets.RequireDictionary(colorBToNormalBlendResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(resolvedB, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedB, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(resolvedB, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.NormalBlendNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(resolvedB, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(2));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DNormalReconstructZWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DNormalReconstructZWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse addVector2Response = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector2", "Sample Vector2");
            ShaderGraphTestAssets.RequirePackageReady(addVector2Response);
            string vector2NodeId = ShaderGraphTestAssets.GetAddedNodeId(addVector2Response);

            ShaderGraphResponse addNormalReconstructZResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalReconstructZ", "Sample NormalReconstructZ");
            ShaderGraphTestAssets.RequirePackageReady(addNormalReconstructZResponse);
            string normalReconstructZNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalReconstructZResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "R", vector2NodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "G", vector2NodeId, "Y"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector2NodeId, "Out", normalReconstructZNodeId, "In"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
        }

        [Test]
        public void BlankGraph_UvNormalFromTextureWorkflow_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvNormalFromTextureWorkflow", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTilingAndOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", "Sample TilingOffset");
            ShaderGraphTestAssets.RequirePackageReady(addTilingAndOffsetResponse);
            string tilingAndOffsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTilingAndOffsetResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addOffsetResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Offset");
            ShaderGraphTestAssets.RequirePackageReady(addOffsetResponse);
            string offsetNodeId = ShaderGraphTestAssets.GetAddedNodeId(addOffsetResponse);

            ShaderGraphResponse addStrengthResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Sample Strength");
            ShaderGraphTestAssets.RequirePackageReady(addStrengthResponse);
            string strengthNodeId = ShaderGraphTestAssets.GetAddedNodeId(addStrengthResponse);

            ShaderGraphResponse addNormalFromTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalFromTexture", "Sample NormalFromTexture");
            ShaderGraphTestAssets.RequirePackageReady(addNormalFromTextureResponse);
            string normalFromTextureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNormalFromTextureResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", normalFromTextureNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", normalFromTextureNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, offsetNodeId, "Out", normalFromTextureNodeId, "Offset"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, strengthNodeId, "Out", normalFromTextureNodeId, "Strength"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(6));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(5));
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
        public void BlankGraph_RenameProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRenameProperty", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse renamePropertyResponse = ShaderGraphAssetTool.HandleRenameProperty(
                assetPath,
                "Tint",
                "Base Tint",
                null);
            ShaderGraphTestAssets.RequirePackageReady(renamePropertyResponse);

            Assert.That(ShaderGraphTestAssets.GetString(renamePropertyResponse.Data, "operation"), Is.EqualTo("rename_property"));
            Assert.That(ShaderGraphTestAssets.GetInt(renamePropertyResponse.Data, "matchCount"), Is.EqualTo(1));

            var renamedProperty = ShaderGraphTestAssets.RequireDictionary(renamePropertyResponse.Data, "renamedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(renamedProperty, "displayName"), Is.EqualTo("Base Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(renamedProperty, "previousDisplayName"), Is.EqualTo("Tint"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(1));
            var properties = ShaderGraphTestAssets.GetStringList(summaryResponse.Data, "properties");
            Assert.That(properties, Has.Some.Contains("Base Tint"));
        }

        [Test]
        public void BlankGraph_FindProperty_ByDisplayNameAndReferenceName_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphFindProperty", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse renamePropertyResponse = ShaderGraphAssetTool.HandleRenameProperty(
                assetPath,
                "Tint",
                "Base Tint",
                "_BaseTint");
            ShaderGraphTestAssets.RequirePackageReady(renamePropertyResponse);

            ShaderGraphResponse findByDisplayNameResponse = ShaderGraphAssetTool.HandleFindProperty(
                assetPath,
                null,
                "Base Tint",
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findByDisplayNameResponse);

            var foundByDisplayName = ShaderGraphTestAssets.RequireDictionary(findByDisplayNameResponse.Data, "foundProperty");
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "displayName"), Is.EqualTo("Base Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "referenceName"), Is.EqualTo("_BaseTint"));

            ShaderGraphResponse findByReferenceResponse = ShaderGraphAssetTool.HandleFindProperty(
                assetPath,
                null,
                null,
                "_BaseTint",
                "Color");
            ShaderGraphTestAssets.RequirePackageReady(findByReferenceResponse);

            var foundByReference = ShaderGraphTestAssets.RequireDictionary(findByReferenceResponse.Data, "foundProperty");
            Assert.That(ShaderGraphTestAssets.GetString(foundByReference, "displayName"), Is.EqualTo("Base Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(foundByReference, "resolvedPropertyType"), Is.EqualTo("Color"));
        }

        [Test]
        public void BlankGraph_DuplicateProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDuplicateProperty", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse duplicatePropertyResponse = ShaderGraphAssetTool.HandleDuplicateProperty(
                assetPath,
                "Tint",
                "Copied Tint",
                "_CopiedTint");
            ShaderGraphTestAssets.RequirePackageReady(duplicatePropertyResponse);

            Assert.That(ShaderGraphTestAssets.GetString(duplicatePropertyResponse.Data, "operation"), Is.EqualTo("duplicate_property"));
            Assert.That(ShaderGraphTestAssets.GetInt(duplicatePropertyResponse.Data, "matchCount"), Is.EqualTo(1));

            var duplicatedFrom = ShaderGraphTestAssets.RequireDictionary(duplicatePropertyResponse.Data, "duplicatedFrom");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedFrom, "displayName"), Is.EqualTo("Tint"));

            var duplicatedProperty = ShaderGraphTestAssets.RequireDictionary(duplicatePropertyResponse.Data, "duplicatedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedProperty, "displayName"), Is.EqualTo("Copied Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedProperty, "referenceName"), Is.EqualTo("_CopiedTint"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedProperty, "sourceDisplayName"), Is.EqualTo("Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedProperty, "resolvedPropertyType"), Is.EqualTo("Color"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(2));

            ShaderGraphResponse findByReferenceResponse = ShaderGraphAssetTool.HandleFindProperty(
                assetPath,
                null,
                null,
                "_CopiedTint",
                "Color");
            ShaderGraphTestAssets.RequirePackageReady(findByReferenceResponse);

            var foundByReference = ShaderGraphTestAssets.RequireDictionary(findByReferenceResponse.Data, "foundProperty");
            Assert.That(ShaderGraphTestAssets.GetString(foundByReference, "displayName"), Is.EqualTo("Copied Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(foundByReference, "referenceName"), Is.EqualTo("_CopiedTint"));
        }

        [Test]
        public void BlankGraph_ReorderProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphReorderProperty", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Exposure", "Float/Vector1", "0"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));

            ShaderGraphResponse reorderToFrontResponse = ShaderGraphAssetTool.HandleReorderProperty(
                assetPath,
                "Tint",
                0);
            ShaderGraphTestAssets.RequirePackageReady(reorderToFrontResponse);

            Assert.That(ShaderGraphTestAssets.GetString(reorderToFrontResponse.Data, "operation"), Is.EqualTo("reorder_property"));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToFrontResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToFrontResponse.Data, "previousIndex"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToFrontResponse.Data, "newIndex"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(reorderToFrontResponse.Data, "categoryGuid"), Is.Not.Empty);

            var categoryOrder = ShaderGraphTestAssets.GetStringList(reorderToFrontResponse.Data, "categoryPropertyOrder");
            Assert.That(categoryOrder.Count, Is.EqualTo(2));
            Assert.That(categoryOrder[0], Does.Contain("Tint"));

            var reorderedProperty = ShaderGraphTestAssets.RequireDictionary(reorderToFrontResponse.Data, "reorderedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(reorderedProperty, "displayName"), Is.EqualTo("Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(reorderedProperty, "resolvedPropertyType"), Is.EqualTo("Color"));

            ShaderGraphResponse reorderToBackResponse = ShaderGraphAssetTool.HandleReorderProperty(
                assetPath,
                "Tint",
                1);
            ShaderGraphTestAssets.RequirePackageReady(reorderToBackResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(reorderToBackResponse.Data, "previousIndex"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToBackResponse.Data, "newIndex"), Is.EqualTo(1));

            var reorderedBackOrder = ShaderGraphTestAssets.GetStringList(reorderToBackResponse.Data, "categoryPropertyOrder");
            Assert.That(reorderedBackOrder.Count, Is.EqualTo(2));
            Assert.That(reorderedBackOrder[1], Does.Contain("Tint"));
        }

        [Test]
        public void BlankGraph_CreateCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphCreateCategory", out _);

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(createCategoryResponse.Data, "operation"), Is.EqualTo("create_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(createCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(createCategoryResponse.Data, "categoryCount"), Is.GreaterThanOrEqualTo(2));

            var createdCategory = ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory");
            Assert.That(ShaderGraphTestAssets.GetString(createdCategory, "displayName"), Is.EqualTo("Surface Inputs"));
            Assert.That(ShaderGraphTestAssets.GetString(createdCategory, "categoryGuid"), Is.Not.Empty);
            Assert.That(ShaderGraphTestAssets.GetInt(createdCategory, "childCount"), Is.EqualTo(0));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(createCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Contain("Surface Inputs"));
        }

        [Test]
        public void BlankGraph_RenameCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRenameCategory", out _);

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string categoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphResponse renameCategoryResponse = ShaderGraphAssetTool.HandleRenameCategory(
                assetPath,
                categoryGuid,
                null,
                "Material Inputs");
            ShaderGraphTestAssets.RequirePackageReady(renameCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(renameCategoryResponse.Data, "operation"), Is.EqualTo("rename_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(renameCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(renameCategoryResponse.Data, "categoryCount"), Is.GreaterThanOrEqualTo(2));

            var renamedCategory = ShaderGraphTestAssets.RequireDictionary(renameCategoryResponse.Data, "renamedCategory");
            Assert.That(ShaderGraphTestAssets.GetString(renamedCategory, "categoryGuid"), Is.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(renamedCategory, "displayName"), Is.EqualTo("Material Inputs"));
            Assert.That(ShaderGraphTestAssets.GetString(renamedCategory, "previousDisplayName"), Is.EqualTo("Surface Inputs"));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(renameCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Contain("Material Inputs"));
        }

        [Test]
        public void BlankGraph_FindCategory_ByGuidAndDisplayName_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphFindCategory", out _);

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string categoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphResponse findByGuidResponse = ShaderGraphAssetTool.HandleFindCategory(
                assetPath,
                categoryGuid,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findByGuidResponse);

            Assert.That(ShaderGraphTestAssets.GetString(findByGuidResponse.Data, "operation"), Is.EqualTo("find_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(findByGuidResponse.Data, "matchCount"), Is.EqualTo(1));

            var foundByGuid = ShaderGraphTestAssets.RequireDictionary(findByGuidResponse.Data, "foundCategory");
            Assert.That(ShaderGraphTestAssets.GetString(foundByGuid, "categoryGuid"), Is.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(foundByGuid, "displayName"), Is.EqualTo("Surface Inputs"));

            ShaderGraphResponse findByNameResponse = ShaderGraphAssetTool.HandleFindCategory(
                assetPath,
                null,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(findByNameResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(findByNameResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetString(findByNameResponse.Data, "matchStrategy"), Is.EqualTo("categoryName/displayName"));

            var foundByName = ShaderGraphTestAssets.RequireDictionary(findByNameResponse.Data, "foundCategory");
            Assert.That(ShaderGraphTestAssets.GetString(foundByName, "categoryGuid"), Is.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(foundByName, "displayName"), Is.EqualTo("Surface Inputs"));
        }

        [Test]
        public void BlankGraph_DeleteCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDeleteCategory", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string categoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(
                    assetPath,
                    "Tint",
                    categoryGuid,
                    null,
                    0));

            ShaderGraphResponse deleteCategoryResponse = ShaderGraphAssetTool.HandleDeleteCategory(
                assetPath,
                categoryGuid,
                null);
            ShaderGraphTestAssets.RequirePackageReady(deleteCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(deleteCategoryResponse.Data, "operation"), Is.EqualTo("delete_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(deleteCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(deleteCategoryResponse.Data, "movedPropertyCount"), Is.EqualTo(1));

            var deletedCategory = ShaderGraphTestAssets.RequireDictionary(deleteCategoryResponse.Data, "deletedCategory");
            Assert.That(ShaderGraphTestAssets.GetString(deletedCategory, "categoryGuid"), Is.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(deletedCategory, "displayName"), Is.EqualTo("Surface Inputs"));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(deleteCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Not.Contain("Surface Inputs"));

            var defaultCategoryPropertyOrder = ShaderGraphTestAssets.GetStringList(deleteCategoryResponse.Data, "defaultCategoryPropertyOrder");
            Assert.That(defaultCategoryPropertyOrder.Any(entry => entry.Contains("Tint")), Is.True);
        }

        [Test]
        public void BlankGraph_ReorderCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphReorderCategory", out _);

            ShaderGraphResponse createSurfaceCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createSurfaceCategoryResponse);

            ShaderGraphResponse createMathCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(
                assetPath,
                "Math Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createMathCategoryResponse);

            string mathCategoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createMathCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphResponse reorderToMiddleResponse = ShaderGraphAssetTool.HandleReorderCategory(
                assetPath,
                mathCategoryGuid,
                null,
                1);
            ShaderGraphTestAssets.RequirePackageReady(reorderToMiddleResponse);

            Assert.That(ShaderGraphTestAssets.GetString(reorderToMiddleResponse.Data, "operation"), Is.EqualTo("reorder_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToMiddleResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToMiddleResponse.Data, "previousIndex"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToMiddleResponse.Data, "newIndex"), Is.EqualTo(1));

            var reorderedCategory = ShaderGraphTestAssets.RequireDictionary(reorderToMiddleResponse.Data, "reorderedCategory");
            Assert.That(ShaderGraphTestAssets.GetString(reorderedCategory, "categoryGuid"), Is.EqualTo(mathCategoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(reorderedCategory, "displayName"), Is.EqualTo("Math Inputs"));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(reorderToMiddleResponse.Data, "categoryOrder");
            Assert.That(categoryOrder.Count, Is.EqualTo(3));
            Assert.That(categoryOrder[1], Is.EqualTo("Math Inputs"));

            ShaderGraphResponse reorderToBackResponse = ShaderGraphAssetTool.HandleReorderCategory(
                assetPath,
                mathCategoryGuid,
                null,
                2);
            ShaderGraphTestAssets.RequirePackageReady(reorderToBackResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(reorderToBackResponse.Data, "previousIndex"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(reorderToBackResponse.Data, "newIndex"), Is.EqualTo(2));

            var reorderedBackOrder = ShaderGraphTestAssets.GetStringList(reorderToBackResponse.Data, "categoryOrder");
            Assert.That(reorderedBackOrder.Count, Is.EqualTo(3));
            Assert.That(reorderedBackOrder[2], Is.EqualTo("Math Inputs"));
        }

        [Test]
        public void BlankGraph_ListCategories_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphListCategories", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Math Inputs"));

            ShaderGraphResponse response = ShaderGraphAssetTool.HandleListCategories(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("list_categories"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "categoryCount"), Is.EqualTo(3));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(response.Data, "categoryOrder");
            Assert.That(categoryOrder.Count, Is.EqualTo(3));
            Assert.That(categoryOrder[0], Is.EqualTo("(Default Category)"));
            Assert.That(categoryOrder, Does.Contain("Surface Inputs"));
            Assert.That(categoryOrder, Does.Contain("Math Inputs"));

            Assert.That(response.Data.TryGetValue("categories", out object rawCategories), Is.True);
            var categories = rawCategories as object[];
            Assert.That(categories, Is.Not.Null);
            Assert.That(categories.Length, Is.EqualTo(3));
        }

        [Test]
        public void BlankGraph_MergeCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMergeCategory", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Exposure", "Float/Vector1", "0"));

            ShaderGraphResponse createSourceCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createSourceCategoryResponse);

            ShaderGraphResponse createTargetCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Material Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createTargetCategoryResponse);

            string sourceCategoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createSourceCategoryResponse.Data, "createdCategory"),
                "categoryGuid");
            string targetCategoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createTargetCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(
                    assetPath,
                    "Tint",
                    sourceCategoryGuid,
                    null,
                    0));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(
                    assetPath,
                    "Exposure",
                    sourceCategoryGuid,
                    null,
                    1));

            ShaderGraphResponse mergeCategoryResponse = ShaderGraphAssetTool.HandleMergeCategory(
                assetPath,
                sourceCategoryGuid,
                null,
                targetCategoryGuid,
                null);
            ShaderGraphTestAssets.RequirePackageReady(mergeCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(mergeCategoryResponse.Data, "operation"), Is.EqualTo("merge_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(mergeCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(mergeCategoryResponse.Data, "sourceMatchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(mergeCategoryResponse.Data, "targetMatchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(mergeCategoryResponse.Data, "movedPropertyCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(mergeCategoryResponse.Data, "targetCategoryGuid"), Is.EqualTo(targetCategoryGuid));

            var mergedFromCategory = ShaderGraphTestAssets.RequireDictionary(mergeCategoryResponse.Data, "mergedFromCategory");
            Assert.That(ShaderGraphTestAssets.GetString(mergedFromCategory, "categoryGuid"), Is.EqualTo(sourceCategoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(mergedFromCategory, "displayName"), Is.EqualTo("Surface Inputs"));
            Assert.That(ShaderGraphTestAssets.GetInt(mergedFromCategory, "previousChildCount"), Is.EqualTo(2));

            var mergedIntoCategory = ShaderGraphTestAssets.RequireDictionary(mergeCategoryResponse.Data, "mergedIntoCategory");
            Assert.That(ShaderGraphTestAssets.GetString(mergedIntoCategory, "categoryGuid"), Is.EqualTo(targetCategoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(mergedIntoCategory, "displayName"), Is.EqualTo("Material Inputs"));
            Assert.That(ShaderGraphTestAssets.GetInt(mergedIntoCategory, "childCount"), Is.EqualTo(2));

            var categoryOrder = ShaderGraphTestAssets.GetStringList(mergeCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Contain("Material Inputs"));
            Assert.That(categoryOrder, Does.Not.Contain("Surface Inputs"));

            var targetCategoryPropertyOrder = ShaderGraphTestAssets.GetStringList(mergeCategoryResponse.Data, "targetCategoryPropertyOrder");
            Assert.That(targetCategoryPropertyOrder.Count, Is.EqualTo(2));
            Assert.That(targetCategoryPropertyOrder[0], Does.Contain("Tint"));
            Assert.That(targetCategoryPropertyOrder[1], Does.Contain("Exposure"));
        }

        [Test]
        public void BlankGraph_DuplicateCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDuplicateCategory", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Exposure", "Float/Vector1", "0"));

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string sourceCategoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(
                    assetPath,
                    "Tint",
                    sourceCategoryGuid,
                    null,
                    0));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(
                    assetPath,
                    "Exposure",
                    sourceCategoryGuid,
                    null,
                    1));

            ShaderGraphResponse duplicateCategoryResponse = ShaderGraphAssetTool.HandleDuplicateCategory(
                assetPath,
                sourceCategoryGuid,
                null,
                "Surface Inputs Copy");
            ShaderGraphTestAssets.RequirePackageReady(duplicateCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(duplicateCategoryResponse.Data, "operation"), Is.EqualTo("duplicate_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(duplicateCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(duplicateCategoryResponse.Data, "duplicatedPropertyCount"), Is.EqualTo(2));

            var duplicatedFromCategory = ShaderGraphTestAssets.RequireDictionary(duplicateCategoryResponse.Data, "duplicatedFromCategory");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedFromCategory, "categoryGuid"), Is.EqualTo(sourceCategoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedFromCategory, "displayName"), Is.EqualTo("Surface Inputs"));

            var duplicatedCategory = ShaderGraphTestAssets.RequireDictionary(duplicateCategoryResponse.Data, "duplicatedCategory");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedCategory, "displayName"), Is.EqualTo("Surface Inputs Copy"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedCategory, "categoryGuid"), Is.Not.Empty);

            var categoryOrder = ShaderGraphTestAssets.GetStringList(duplicateCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Contain("Surface Inputs"));
            Assert.That(categoryOrder, Does.Contain("Surface Inputs Copy"));

            var categoryPropertyOrder = ShaderGraphTestAssets.GetStringList(duplicateCategoryResponse.Data, "categoryPropertyOrder");
            Assert.That(categoryPropertyOrder.Count, Is.EqualTo(2));
            Assert.That(categoryPropertyOrder.Any(entry => entry.Contains("Tint Copy")), Is.True);
            Assert.That(categoryPropertyOrder.Any(entry => entry.Contains("Exposure Copy")), Is.True);

            Assert.That(duplicateCategoryResponse.Data.TryGetValue("duplicatedProperties", out object rawDuplicatedProperties), Is.True);
            var duplicatedProperties = rawDuplicatedProperties as object[];
            Assert.That(duplicatedProperties, Is.Not.Null);
            Assert.That(duplicatedProperties.Length, Is.EqualTo(2));
        }

        [Test]
        public void BlankGraph_SplitCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphSplitCategory", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Exposure", "Float/Vector1", "0"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Metallic", "Float/Vector1", "0"));

            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string sourceCategoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(assetPath, "Tint", sourceCategoryGuid, null, 0));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(assetPath, "Exposure", sourceCategoryGuid, null, 1));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleMovePropertyToCategory(assetPath, "Metallic", sourceCategoryGuid, null, 2));

            ShaderGraphResponse splitCategoryResponse = ShaderGraphAssetTool.HandleSplitCategory(
                assetPath,
                sourceCategoryGuid,
                null,
                "Surface Inputs Primary",
                "Tint",
                "Exposure");
            ShaderGraphTestAssets.RequirePackageReady(splitCategoryResponse);

            Assert.That(ShaderGraphTestAssets.GetString(splitCategoryResponse.Data, "operation"), Is.EqualTo("split_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(splitCategoryResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetInt(splitCategoryResponse.Data, "movedPropertyCount"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(splitCategoryResponse.Data, "sourceCategoryPreviousChildCount"), Is.EqualTo(3));

            var splitFromCategory = ShaderGraphTestAssets.RequireDictionary(splitCategoryResponse.Data, "splitFromCategory");
            Assert.That(ShaderGraphTestAssets.GetString(splitFromCategory, "categoryGuid"), Is.EqualTo(sourceCategoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(splitFromCategory, "displayName"), Is.EqualTo("Surface Inputs"));

            var splitIntoCategory = ShaderGraphTestAssets.RequireDictionary(splitCategoryResponse.Data, "splitIntoCategory");
            Assert.That(ShaderGraphTestAssets.GetString(splitIntoCategory, "displayName"), Is.EqualTo("Surface Inputs Primary"));
            Assert.That(ShaderGraphTestAssets.GetString(splitIntoCategory, "categoryGuid"), Is.Not.Empty);

            var categoryOrder = ShaderGraphTestAssets.GetStringList(splitCategoryResponse.Data, "categoryOrder");
            Assert.That(categoryOrder, Does.Contain("Surface Inputs"));
            Assert.That(categoryOrder, Does.Contain("Surface Inputs Primary"));

            var sourceCategoryPropertyOrder = ShaderGraphTestAssets.GetStringList(splitCategoryResponse.Data, "sourceCategoryPropertyOrder");
            Assert.That(sourceCategoryPropertyOrder.Count, Is.EqualTo(1));
            Assert.That(sourceCategoryPropertyOrder[0], Does.Contain("Metallic"));

            var targetCategoryPropertyOrder = ShaderGraphTestAssets.GetStringList(splitCategoryResponse.Data, "targetCategoryPropertyOrder");
            Assert.That(targetCategoryPropertyOrder.Count, Is.EqualTo(2));
            Assert.That(targetCategoryPropertyOrder[0], Does.Contain("Tint"));
            Assert.That(targetCategoryPropertyOrder[1], Does.Contain("Exposure"));
        }

        [Test]
        public void BlankGraph_MovePropertyToCategory_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMovePropertyToCategory", out _);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Exposure", "Float/Vector1", "0"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleAddProperty(assetPath, "Tint", "Color", "#FFFFFFFF"));
            ShaderGraphResponse createCategoryResponse = ShaderGraphAssetTool.HandleCreateCategory(assetPath, "Surface Inputs");
            ShaderGraphTestAssets.RequirePackageReady(createCategoryResponse);

            string categoryGuid = ShaderGraphTestAssets.GetString(
                ShaderGraphTestAssets.RequireDictionary(createCategoryResponse.Data, "createdCategory"),
                "categoryGuid");

            ShaderGraphResponse moveResponse = ShaderGraphAssetTool.HandleMovePropertyToCategory(
                assetPath,
                "Tint",
                categoryGuid,
                null,
                0);
            ShaderGraphTestAssets.RequirePackageReady(moveResponse);

            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "operation"), Is.EqualTo("move_property_to_category"));
            Assert.That(ShaderGraphTestAssets.GetInt(moveResponse.Data, "matchCount"), Is.EqualTo(1));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "previousCategoryGuid"), Is.Not.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetString(moveResponse.Data, "categoryGuid"), Is.EqualTo(categoryGuid));
            Assert.That(ShaderGraphTestAssets.GetInt(moveResponse.Data, "newIndex"), Is.EqualTo(0));

            var movedProperty = ShaderGraphTestAssets.RequireDictionary(moveResponse.Data, "movedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(movedProperty, "displayName"), Is.EqualTo("Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(movedProperty, "categoryGuid"), Is.EqualTo(categoryGuid));

            var categoryPropertyOrder = ShaderGraphTestAssets.GetStringList(moveResponse.Data, "categoryPropertyOrder");
            Assert.That(categoryPropertyOrder.Count, Is.EqualTo(1));
            Assert.That(categoryPropertyOrder[0], Does.Contain("Tint"));
        }

        [Test]
        public void BlankGraph_RenameNode_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRenameNode", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Add",
                "Original Add");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            string nodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(nodeId, Is.Not.Empty);

            ShaderGraphResponse findBeforeRenameResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                nodeId,
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findBeforeRenameResponse);

            var foundBeforeRename = ShaderGraphTestAssets.RequireDictionary(findBeforeRenameResponse.Data, "foundNode");
            string persistedDisplayNameBeforeRename = ShaderGraphTestAssets.GetString(foundBeforeRename, "displayName");

            ShaderGraphResponse renameNodeResponse = ShaderGraphAssetTool.HandleRenameNode(
                assetPath,
                nodeId,
                "Renamed Add");
            ShaderGraphTestAssets.RequirePackageReady(renameNodeResponse);

            Assert.That(ShaderGraphTestAssets.GetString(renameNodeResponse.Data, "operation"), Is.EqualTo("rename_node"));
            Assert.That(ShaderGraphTestAssets.GetInt(renameNodeResponse.Data, "matchCount"), Is.EqualTo(1));

            var renamedNode = ShaderGraphTestAssets.RequireDictionary(renameNodeResponse.Data, "renamedNode");
            Assert.That(ShaderGraphTestAssets.GetString(renamedNode, "objectId"), Is.EqualTo(nodeId));
            Assert.That(ShaderGraphTestAssets.GetString(renamedNode, "displayName"), Is.EqualTo("Renamed Add"));
            Assert.That(ShaderGraphTestAssets.GetString(renamedNode, "previousDisplayName"), Is.EqualTo(persistedDisplayNameBeforeRename));

            ShaderGraphResponse findByIdResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                nodeId,
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findByIdResponse);

            var foundById = ShaderGraphTestAssets.RequireDictionary(findByIdResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundById, "displayName"), Is.EqualTo("Renamed Add"));

            ShaderGraphResponse findByDisplayNameResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                null,
                "Renamed Add",
                "Add");
            ShaderGraphTestAssets.RequirePackageReady(findByDisplayNameResponse);

            var foundByDisplayName = ShaderGraphTestAssets.RequireDictionary(findByDisplayNameResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "objectId"), Is.EqualTo(nodeId));
        }

        [Test]
        public void BlankGraph_DuplicateNode_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphDuplicateNode", out _);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Add",
                "Source Add");
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNodeResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse duplicateNodeResponse = ShaderGraphAssetTool.HandleDuplicateNode(
                assetPath,
                sourceNodeId,
                "Copied Add");
            ShaderGraphTestAssets.RequirePackageReady(duplicateNodeResponse);

            Assert.That(ShaderGraphTestAssets.GetString(duplicateNodeResponse.Data, "operation"), Is.EqualTo("duplicate_node"));
            Assert.That(ShaderGraphTestAssets.GetInt(duplicateNodeResponse.Data, "matchCount"), Is.EqualTo(1));

            var duplicatedFrom = ShaderGraphTestAssets.RequireDictionary(duplicateNodeResponse.Data, "duplicatedFrom");
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedFrom, "objectId"), Is.EqualTo(sourceNodeId));

            var duplicatedNode = ShaderGraphTestAssets.RequireDictionary(duplicateNodeResponse.Data, "duplicatedNode");
            string duplicatedNodeId = ShaderGraphTestAssets.GetString(duplicatedNode, "objectId");
            Assert.That(duplicatedNodeId, Is.Not.Empty);
            Assert.That(duplicatedNodeId, Is.Not.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedNode, "displayName"), Is.EqualTo("Copied Add"));
            Assert.That(ShaderGraphTestAssets.GetString(duplicatedNode, "sourceNodeId"), Is.EqualTo(sourceNodeId));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(2));

            ShaderGraphResponse findByIdResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                duplicatedNodeId,
                null,
                null);
            ShaderGraphTestAssets.RequirePackageReady(findByIdResponse);

            var foundById = ShaderGraphTestAssets.RequireDictionary(findByIdResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundById, "displayName"), Is.EqualTo("Copied Add"));

            ShaderGraphResponse findByDisplayNameResponse = ShaderGraphAssetTool.HandleFindNode(
                assetPath,
                null,
                "Copied Add",
                "Add");
            ShaderGraphTestAssets.RequirePackageReady(findByDisplayNameResponse);

            var foundByDisplayName = ShaderGraphTestAssets.RequireDictionary(findByDisplayNameResponse.Data, "foundNode");
            Assert.That(ShaderGraphTestAssets.GetString(foundByDisplayName, "objectId"), Is.EqualTo(duplicatedNodeId));
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
        public void BlankGraph_AddNodeRelativeToAnchor_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAddNodeRelativeToAnchor", out _);

            ShaderGraphResponse anchorResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Anchor Source");
            ShaderGraphTestAssets.RequirePackageReady(anchorResponse);

            string anchorNodeId = ShaderGraphTestAssets.GetAddedNodeId(anchorResponse);
            Assert.That(anchorNodeId, Is.Not.Empty);

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.Handle(
                new AddNodeRequest(
                    assetPath,
                    "Vector1",
                    "Relative Sink",
                    null,
                    null,
                    anchorNodeId,
                    null,
                    null,
                    "right",
                    320f,
                    null));
            ShaderGraphTestAssets.RequirePackageReady(addNodeResponse);

            var placement = ShaderGraphTestAssets.RequireDictionary(addNodeResponse.Data, "placement");
            Assert.That(ShaderGraphTestAssets.GetString(placement, "mode"), Is.EqualTo("relative"));
            Assert.That(ShaderGraphTestAssets.GetString(placement, "direction"), Is.EqualTo("right"));

            var resolvedPosition = ShaderGraphTestAssets.RequireDictionary(placement, "resolvedPosition");
            Assert.That(resolvedPosition["x"], Is.EqualTo(-300f));
            Assert.That(resolvedPosition["y"], Is.EqualTo(140f));

            var addedNode = ShaderGraphTestAssets.RequireDictionary(addNodeResponse.Data, "addedNode");
            var position = ShaderGraphTestAssets.RequireDictionary(addedNode, "position");
            Assert.That(position["x"], Is.EqualTo(-300f));
            Assert.That(position["y"], Is.EqualTo(140f));
        }

        [Test]
        public void BlankGraph_MoveNodeRelativeToAnchor_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphMoveNodeRelativeToAnchor", out _);

            ShaderGraphResponse anchorResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Anchor Source");
            ShaderGraphTestAssets.RequirePackageReady(anchorResponse);

            string anchorNodeId = ShaderGraphTestAssets.GetAddedNodeId(anchorResponse);
            Assert.That(anchorNodeId, Is.Not.Empty);

            ShaderGraphResponse movingResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Relative Move Source");
            ShaderGraphTestAssets.RequirePackageReady(movingResponse);

            string movingNodeId = ShaderGraphTestAssets.GetAddedNodeId(movingResponse);
            Assert.That(movingNodeId, Is.Not.Empty);

            ShaderGraphResponse moveNodeResponse = ShaderGraphAssetTool.Handle(
                new MoveNodeRequest(
                    assetPath,
                    movingNodeId,
                    null,
                    null,
                    anchorNodeId,
                    null,
                    null,
                    "down",
                    200f,
                    null));
            ShaderGraphTestAssets.RequirePackageReady(moveNodeResponse);

            var placement = ShaderGraphTestAssets.RequireDictionary(moveNodeResponse.Data, "placement");
            Assert.That(ShaderGraphTestAssets.GetString(placement, "mode"), Is.EqualTo("relative"));
            Assert.That(ShaderGraphTestAssets.GetString(placement, "direction"), Is.EqualTo("down"));

            var resolvedPosition = ShaderGraphTestAssets.RequireDictionary(placement, "resolvedPosition");
            Assert.That(resolvedPosition["x"], Is.EqualTo(-620f));
            Assert.That(resolvedPosition["y"], Is.EqualTo(340f));

            var movedNode = ShaderGraphTestAssets.RequireDictionary(moveNodeResponse.Data, "movedNode");
            var position = ShaderGraphTestAssets.RequireDictionary(movedNode, "position");
            Assert.That(position["x"], Is.EqualTo(-620f));
            Assert.That(position["y"], Is.EqualTo(340f));
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
        public void BlankGraph_RemoveExistingProperty_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRemoveProperty", out _);

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                "Tint",
                "Color",
                "#FFFFFFFF");
            ShaderGraphTestAssets.RequirePackageReady(addPropertyResponse);

            ShaderGraphResponse removePropertyResponse = ShaderGraphAssetTool.HandleRemoveProperty(
                assetPath,
                "Tint");
            ShaderGraphTestAssets.RequirePackageReady(removePropertyResponse);

            Assert.That(ShaderGraphTestAssets.GetString(removePropertyResponse.Data, "operation"), Is.EqualTo("remove_property"));
            Assert.That(ShaderGraphTestAssets.GetInt(removePropertyResponse.Data, "matchCount"), Is.EqualTo(1));

            var deletedProperty = ShaderGraphTestAssets.RequireDictionary(removePropertyResponse.Data, "deletedProperty");
            Assert.That(ShaderGraphTestAssets.GetString(deletedProperty, "displayName"), Is.EqualTo("Tint"));
            Assert.That(ShaderGraphTestAssets.GetString(deletedProperty, "resolvedPropertyType"), Is.EqualTo("Color"));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "propertyCount"), Is.EqualTo(0));
        }

        [Test]
        public void BlankGraph_RemoveConnection_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphRemoveConnection", out _);

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Remove Source");
            ShaderGraphTestAssets.RequirePackageReady(addSourceNodeResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceNodeResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse addSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Remove Sink");
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

            ShaderGraphResponse removeConnectionResponse = ShaderGraphAssetTool.HandleRemoveConnection(
                assetPath,
                sourceNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(removeConnectionResponse);

            Assert.That(ShaderGraphTestAssets.GetString(removeConnectionResponse.Data, "operation"), Is.EqualTo("remove_connection"));
            Assert.That(ShaderGraphTestAssets.GetInt(removeConnectionResponse.Data, "matchCount"), Is.EqualTo(1));

            var deletedConnection = ShaderGraphTestAssets.RequireDictionary(removeConnectionResponse.Data, "deletedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(deletedConnection, "outputNodeId"), Is.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(deletedConnection, "inputNodeId"), Is.EqualTo(sinkNodeId));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(0));
        }

        [Test]
        public void BlankGraph_FindConnection_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphFindConnection", out _);

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Find Source");
            ShaderGraphTestAssets.RequirePackageReady(addSourceNodeResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceNodeResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse addSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Find Sink");
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

            ShaderGraphResponse findConnectionResponse = ShaderGraphAssetTool.HandleFindConnection(
                assetPath,
                sourceNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(findConnectionResponse);

            Assert.That(ShaderGraphTestAssets.GetString(findConnectionResponse.Data, "operation"), Is.EqualTo("find_connection"));
            Assert.That(ShaderGraphTestAssets.GetInt(findConnectionResponse.Data, "matchCount"), Is.EqualTo(1));

            var query = ShaderGraphTestAssets.RequireDictionary(findConnectionResponse.Data, "query");
            Assert.That(ShaderGraphTestAssets.GetString(query, "outputNodeId"), Is.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(query, "inputNodeId"), Is.EqualTo(sinkNodeId));

            var foundConnection = ShaderGraphTestAssets.RequireDictionary(findConnectionResponse.Data, "foundConnection");
            Assert.That(ShaderGraphTestAssets.GetString(foundConnection, "outputNodeId"), Is.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(foundConnection, "inputNodeId"), Is.EqualTo(sinkNodeId));
            Assert.That(ShaderGraphTestAssets.GetInt(foundConnection, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetInt(foundConnection, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
        }

        [Test]
        public void BlankGraph_ReconnectConnection_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphReconnectConnection", out _);

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Reconnect Source");
            ShaderGraphTestAssets.RequirePackageReady(addSourceNodeResponse);
            string sourceNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSourceNodeResponse);
            Assert.That(sourceNodeId, Is.Not.Empty);

            ShaderGraphResponse addOldSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Reconnect Old Sink");
            ShaderGraphTestAssets.RequirePackageReady(addOldSinkNodeResponse);
            string oldSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addOldSinkNodeResponse);
            Assert.That(oldSinkNodeId, Is.Not.Empty);

            ShaderGraphResponse addNewSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                "Reconnect New Sink");
            ShaderGraphTestAssets.RequirePackageReady(addNewSinkNodeResponse);
            string newSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addNewSinkNodeResponse);
            Assert.That(newSinkNodeId, Is.Not.Empty);

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceNodeId,
                "Out",
                oldSinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(connectResponse);

            ShaderGraphResponse reconnectResponse = ShaderGraphAssetTool.HandleReconnectConnection(
                assetPath,
                sourceNodeId,
                "Out",
                oldSinkNodeId,
                "X",
                sourceNodeId,
                "Out",
                newSinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(reconnectResponse);

            Assert.That(ShaderGraphTestAssets.GetString(reconnectResponse.Data, "operation"), Is.EqualTo("reconnect_connection"));
            Assert.That(ShaderGraphTestAssets.GetInt(reconnectResponse.Data, "matchCount"), Is.EqualTo(1));

            var previousConnection = ShaderGraphTestAssets.RequireDictionary(reconnectResponse.Data, "previousConnection");
            Assert.That(ShaderGraphTestAssets.GetString(previousConnection, "outputNodeId"), Is.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(previousConnection, "inputNodeId"), Is.EqualTo(oldSinkNodeId));

            var requestedConnection = ShaderGraphTestAssets.RequireDictionary(reconnectResponse.Data, "requestedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(requestedConnection, "outputNodeId"), Is.EqualTo(sourceNodeId));
            Assert.That(ShaderGraphTestAssets.GetString(requestedConnection, "inputNodeId"), Is.EqualTo(newSinkNodeId));

            var removedConnection = ShaderGraphTestAssets.RequireDictionary(reconnectResponse.Data, "removedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(removedConnection, "inputNodeId"), Is.EqualTo(oldSinkNodeId));

            var connectedConnection = ShaderGraphTestAssets.RequireDictionary(reconnectResponse.Data, "connectedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(connectedConnection, "inputNodeId"), Is.EqualTo(newSinkNodeId));

            ShaderGraphResponse oldConnectionResponse = ShaderGraphAssetTool.HandleFindConnection(
                assetPath,
                sourceNodeId,
                "Out",
                oldSinkNodeId,
                "X");
            Assert.That(oldConnectionResponse.Success, Is.False);
            Assert.That(ShaderGraphTestAssets.GetString(oldConnectionResponse.Data, "backendKind"), Is.EqualTo("PackageReady"));
            Assert.That(ShaderGraphTestAssets.GetInt(oldConnectionResponse.Data, "matchCount"), Is.EqualTo(0));

            ShaderGraphResponse newConnectionResponse = ShaderGraphAssetTool.HandleFindConnection(
                assetPath,
                sourceNodeId,
                "Out",
                newSinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(newConnectionResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(newConnectionResponse.Data, "matchCount"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(1));
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
        public void ListSupportedProperties_ReturnsPackageBackedCatalog()
        {
            ShaderGraphResponse response = ShaderGraphAssetTool.HandleListSupportedProperties();
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("list_supported_properties"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "supportedPropertyCount"), Is.GreaterThan(0));

            var supportedPropertyTypes = ShaderGraphTestAssets.GetStringList(response.Data, "supportedPropertyTypes");
            Assert.That(supportedPropertyTypes, Has.Some.EqualTo("Float/Vector1"));
            Assert.That(supportedPropertyTypes, Has.Some.EqualTo("Color"));
        }

        [Test]
        public void ListSupportedConnections_ReturnsPackageBackedCatalog()
        {
            ShaderGraphResponse response = ShaderGraphAssetTool.HandleListSupportedConnections();
            ShaderGraphTestAssets.RequirePackageReady(response);

            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "operation"), Is.EqualTo("list_supported_connections"));
            Assert.That(ShaderGraphTestAssets.GetString(response.Data, "connectionCatalogSemantics"), Is.EqualTo("supportedConnectionRules=enforced-runtime-rules"));
            Assert.That(ShaderGraphTestAssets.GetInt(response.Data, "supportedConnectionRuleCount"), Is.GreaterThan(0));

            var supportedConnectionRules = ShaderGraphTestAssets.GetStringList(response.Data, "supportedConnectionRules");
            Assert.That(supportedConnectionRules, Does.Contain("Self-connections are rejected."));
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
        public void BlankGraph_UvSampleTexture2DChannelsToComparisonBranchChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DComparisonBranchChain", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);

            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);

            ShaderGraphResponse trueResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch True");
            ShaderGraphTestAssets.RequirePackageReady(trueResponse);
            string trueNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueResponse);

            ShaderGraphResponse falseResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch False");
            ShaderGraphTestAssets.RequirePackageReady(falseResponse);
            string falseNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseResponse);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));

            ShaderGraphResponse comparisonAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "R",
                comparisonNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(comparisonAConnection);

            ShaderGraphResponse comparisonBConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "G",
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

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueNodeId, "Out", branchNodeId, "True"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseNodeId, "Out", branchNodeId, "False"));

            ShaderGraphResponse sinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(sinkConnection);

            var comparisonAResolved = ShaderGraphTestAssets.RequireDictionary(comparisonAConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(comparisonAResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(comparisonAResolved, "outputSlotId"), Is.EqualTo(4));
            Assert.That(ShaderGraphTestAssets.GetString(comparisonAResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(comparisonAResolved, "inputSlotId"), Is.EqualTo(0));

            var comparisonBResolved = ShaderGraphTestAssets.RequireDictionary(comparisonBConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(comparisonBResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(comparisonBResolved, "outputSlotId"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetString(comparisonBResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(comparisonBResolved, "inputSlotId"), Is.EqualTo(1));

            var predicateResolved = ShaderGraphTestAssets.RequireDictionary(predicateConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(predicateResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(ShaderGraphTestAssets.GetString(predicateResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateResolved, "inputSlotId"), Is.EqualTo(0));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(sinkConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(8));
        }

        [Test]
        public void BlankGraph_UvSampleTexture2DChannelsToBranchChain_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphUvSampleTexture2DBranchChain", out _);

            ShaderGraphResponse addUvResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", "Sample UV");
            ShaderGraphTestAssets.RequirePackageReady(addUvResponse);
            string uvNodeId = ShaderGraphTestAssets.GetAddedNodeId(addUvResponse);

            ShaderGraphResponse addTextureResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", "Sample Texture");
            ShaderGraphTestAssets.RequirePackageReady(addTextureResponse);
            string textureNodeId = ShaderGraphTestAssets.GetAddedNodeId(addTextureResponse);

            ShaderGraphResponse addSampleResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", "Sample Texture2D");
            ShaderGraphTestAssets.RequirePackageReady(addSampleResponse);
            string sampleNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSampleResponse);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Branch Comparison");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);

            ShaderGraphResponse compareAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphTestAssets.RequirePackageReady(compareAResponse);
            string compareANodeId = ShaderGraphTestAssets.GetAddedNodeId(compareAResponse);

            ShaderGraphResponse compareBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphTestAssets.RequirePackageReady(compareBResponse);
            string compareBNodeId = ShaderGraphTestAssets.GetAddedNodeId(compareBResponse);

            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", sampleNodeId, "UV"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));

            ShaderGraphResponse trueConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "R",
                branchNodeId,
                "True");
            ShaderGraphTestAssets.RequirePackageReady(trueConnection);

            ShaderGraphResponse falseConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sampleNodeId,
                "G",
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

            var trueResolved = ShaderGraphTestAssets.RequireDictionary(trueConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(trueResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(trueResolved, "outputSlotId"), Is.EqualTo(4));
            Assert.That(ShaderGraphTestAssets.GetString(trueResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(trueResolved, "inputSlotId"), Is.EqualTo(1));

            var falseResolved = ShaderGraphTestAssets.RequireDictionary(falseConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(falseResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SampleTexture2DNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(falseResolved, "outputSlotId"), Is.EqualTo(5));
            Assert.That(ShaderGraphTestAssets.GetString(falseResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(falseResolved, "inputSlotId"), Is.EqualTo(2));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(sinkConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "inputSlotId"), Is.EqualTo(1));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(8));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(8));
        }

        [Test]
        public void BlankGraph_ComparisonToMultipleBranchPredicates_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphComparisonMultipleBranchPredicates", out _);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);

            ShaderGraphResponse branchAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node A");
            ShaderGraphTestAssets.RequirePackageReady(branchAResponse);
            string branchANodeId = ShaderGraphTestAssets.GetAddedNodeId(branchAResponse);

            ShaderGraphResponse branchBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node B");
            ShaderGraphTestAssets.RequirePackageReady(branchBResponse);
            string branchBNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchBResponse);

            ShaderGraphResponse sourceAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphTestAssets.RequirePackageReady(sourceAResponse);
            string sourceANodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceAResponse);

            ShaderGraphResponse sourceBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphTestAssets.RequirePackageReady(sourceBResponse);
            string sourceBNodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceBResponse);

            ShaderGraphResponse trueAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch A True");
            ShaderGraphTestAssets.RequirePackageReady(trueAResponse);
            string trueANodeId = ShaderGraphTestAssets.GetAddedNodeId(trueAResponse);

            ShaderGraphResponse falseAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch A False");
            ShaderGraphTestAssets.RequirePackageReady(falseAResponse);
            string falseANodeId = ShaderGraphTestAssets.GetAddedNodeId(falseAResponse);

            ShaderGraphResponse trueBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch B True");
            ShaderGraphTestAssets.RequirePackageReady(trueBResponse);
            string trueBNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueBResponse);

            ShaderGraphResponse falseBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch B False");
            ShaderGraphTestAssets.RequirePackageReady(falseBResponse);
            string falseBNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseBResponse);

            ShaderGraphResponse sinkAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch A Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkAResponse);
            string sinkANodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkAResponse);

            ShaderGraphResponse sinkBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch B Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkBResponse);
            string sinkBNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkBResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", comparisonNodeId, "B"));

            ShaderGraphResponse predicateAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                comparisonNodeId,
                "Out",
                branchANodeId,
                "Predicate");
            ShaderGraphTestAssets.RequirePackageReady(predicateAConnection);

            ShaderGraphResponse predicateBConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                comparisonNodeId,
                "Out",
                branchBNodeId,
                "Predicate");
            ShaderGraphTestAssets.RequirePackageReady(predicateBConnection);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueANodeId, "Out", branchANodeId, "True"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseANodeId, "Out", branchANodeId, "False"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueBNodeId, "Out", branchBNodeId, "True"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseBNodeId, "Out", branchBNodeId, "False"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchANodeId, "Out", sinkANodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchBNodeId, "Out", sinkBNodeId, "X"));

            var predicateAResolved = ShaderGraphTestAssets.RequireDictionary(predicateAConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(predicateAResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(ShaderGraphTestAssets.GetString(predicateAResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateAResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateAResolved, "inputSlotId"), Is.EqualTo(0));

            var predicateBResolved = ShaderGraphTestAssets.RequireDictionary(predicateBConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(predicateBResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ComparisonNode"));
            Assert.That(ShaderGraphTestAssets.GetString(predicateBResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateBResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetInt(predicateBResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(11));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(10));
        }

        [Test]
        public void BlankGraph_BranchOutToMultipleScalarConsumers_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphBranchOutMultipleScalarConsumers", out _);

            ShaderGraphResponse comparisonResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", "Comparison Node");
            ShaderGraphTestAssets.RequirePackageReady(comparisonResponse);
            string comparisonNodeId = ShaderGraphTestAssets.GetAddedNodeId(comparisonResponse);

            ShaderGraphResponse branchResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", "Branch Node");
            ShaderGraphTestAssets.RequirePackageReady(branchResponse);
            string branchNodeId = ShaderGraphTestAssets.GetAddedNodeId(branchResponse);

            ShaderGraphResponse sourceAResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison A");
            ShaderGraphTestAssets.RequirePackageReady(sourceAResponse);
            string sourceANodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceAResponse);

            ShaderGraphResponse sourceBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Comparison B");
            ShaderGraphTestAssets.RequirePackageReady(sourceBResponse);
            string sourceBNodeId = ShaderGraphTestAssets.GetAddedNodeId(sourceBResponse);

            ShaderGraphResponse trueResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch True");
            ShaderGraphTestAssets.RequirePackageReady(trueResponse);
            string trueNodeId = ShaderGraphTestAssets.GetAddedNodeId(trueResponse);

            ShaderGraphResponse falseResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch False");
            ShaderGraphTestAssets.RequirePackageReady(falseResponse);
            string falseNodeId = ShaderGraphTestAssets.GetAddedNodeId(falseResponse);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Branch Sink");
            ShaderGraphTestAssets.RequirePackageReady(sinkResponse);
            string sinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(sinkResponse);

            ShaderGraphResponse addResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Add", "Add Node");
            ShaderGraphTestAssets.RequirePackageReady(addResponse);
            string addNodeId = ShaderGraphTestAssets.GetAddedNodeId(addResponse);

            ShaderGraphResponse addBResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Add B");
            ShaderGraphTestAssets.RequirePackageReady(addBResponse);
            string addBNodeId = ShaderGraphTestAssets.GetAddedNodeId(addBResponse);

            ShaderGraphResponse addSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Add Sink");
            ShaderGraphTestAssets.RequirePackageReady(addSinkResponse);
            string addSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(addSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", comparisonNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", comparisonNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueNodeId, "Out", branchNodeId, "True"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseNodeId, "Out", branchNodeId, "False"));

            ShaderGraphResponse sinkConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                sinkNodeId,
                "X");
            ShaderGraphTestAssets.RequirePackageReady(sinkConnection);

            ShaderGraphResponse addAConnection = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                branchNodeId,
                "Out",
                addNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(addAConnection);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, addBNodeId, "Out", addNodeId, "B"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, addNodeId, "Out", addSinkNodeId, "X"));

            var sinkResolved = ShaderGraphTestAssets.RequireDictionary(sinkConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetString(sinkResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.Vector1Node"));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetInt(sinkResolved, "inputSlotId"), Is.EqualTo(1));

            var addAResolved = ShaderGraphTestAssets.RequireDictionary(addAConnection.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(addAResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.BranchNode"));
            Assert.That(ShaderGraphTestAssets.GetString(addAResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AddNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(addAResolved, "outputSlotId"), Is.EqualTo(3));
            Assert.That(ShaderGraphTestAssets.GetInt(addAResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(10));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(9));
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
        public void BlankGraph_ColorOutToSplitAndMultiply_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphColorOutToSplitAndMultiply", out _);

            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Color Source");
            ShaderGraphResponse directSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Direct Split");
            ShaderGraphResponse multiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Consumer");
            ShaderGraphResponse multipliedSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Multiplied Split");
            ShaderGraphResponse multiplyColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Multiply Color");
            ShaderGraphResponse directSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Direct Sink");
            ShaderGraphResponse multipliedSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Multiplied Sink");

            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(directSplitResponse);
            ShaderGraphTestAssets.RequirePackageReady(multiplyResponse);
            ShaderGraphTestAssets.RequirePackageReady(multipliedSplitResponse);
            ShaderGraphTestAssets.RequirePackageReady(multiplyColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(directSinkResponse);
            ShaderGraphTestAssets.RequirePackageReady(multipliedSinkResponse);

            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string directSplitNodeId = ShaderGraphTestAssets.GetAddedNodeId(directSplitResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyResponse);
            string multipliedSplitNodeId = ShaderGraphTestAssets.GetAddedNodeId(multipliedSplitResponse);
            string multiplyColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyColorResponse);
            string directSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(directSinkResponse);
            string multipliedSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(multipliedSinkResponse);

            ShaderGraphResponse colorToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorNodeId,
                "Out",
                directSplitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(colorToSplitResponse);

            ShaderGraphResponse colorToMultiplyResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                colorNodeId,
                "Out",
                multiplyNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(colorToMultiplyResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyColorNodeId, "Out", multiplyNodeId, "B"));

            ShaderGraphResponse multiplyToSplitResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                multipliedSplitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(multiplyToSplitResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, directSplitNodeId, "R", directSinkNodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multipliedSplitNodeId, "G", multipliedSinkNodeId, "X"));

            var splitResolved = ShaderGraphTestAssets.RequireDictionary(colorToSplitResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(splitResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(splitResolved, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(splitResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(splitResolved, "inputSlotId"), Is.EqualTo(0));

            var multiplyResolved = ShaderGraphTestAssets.RequireDictionary(colorToMultiplyResponse.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.ColorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "outputSlotId"), Is.EqualTo(0));
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(7));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(6));
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
        public void BlankGraph_AppendOutToSplitAndMultiply_StaysPackageBacked()
        {
            string assetPath = CreateBlankGraph("BlankGraphAppendOutToSplitAndMultiply", out _);

            ShaderGraphResponse appendResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", "Append Source");
            ShaderGraphResponse directSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Direct Split");
            ShaderGraphResponse multiplyResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", "Multiply Consumer");
            ShaderGraphResponse multipliedSplitResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", "Multiplied Split");
            ShaderGraphResponse colorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Append Color");
            ShaderGraphResponse scalarResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Append Scalar");
            ShaderGraphResponse multiplyColorResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", "Multiply Color");
            ShaderGraphResponse directSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Direct Sink");
            ShaderGraphResponse multipliedSinkResponse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", "Multiplied Sink");

            ShaderGraphTestAssets.RequirePackageReady(appendResponse);
            ShaderGraphTestAssets.RequirePackageReady(directSplitResponse);
            ShaderGraphTestAssets.RequirePackageReady(multiplyResponse);
            ShaderGraphTestAssets.RequirePackageReady(multipliedSplitResponse);
            ShaderGraphTestAssets.RequirePackageReady(colorResponse);
            ShaderGraphTestAssets.RequirePackageReady(scalarResponse);
            ShaderGraphTestAssets.RequirePackageReady(multiplyColorResponse);
            ShaderGraphTestAssets.RequirePackageReady(directSinkResponse);
            ShaderGraphTestAssets.RequirePackageReady(multipliedSinkResponse);

            string appendNodeId = ShaderGraphTestAssets.GetAddedNodeId(appendResponse);
            string directSplitNodeId = ShaderGraphTestAssets.GetAddedNodeId(directSplitResponse);
            string multiplyNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyResponse);
            string multipliedSplitNodeId = ShaderGraphTestAssets.GetAddedNodeId(multipliedSplitResponse);
            string colorNodeId = ShaderGraphTestAssets.GetAddedNodeId(colorResponse);
            string scalarNodeId = ShaderGraphTestAssets.GetAddedNodeId(scalarResponse);
            string multiplyColorNodeId = ShaderGraphTestAssets.GetAddedNodeId(multiplyColorResponse);
            string directSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(directSinkResponse);
            string multipliedSinkNodeId = ShaderGraphTestAssets.GetAddedNodeId(multipliedSinkResponse);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));

            ShaderGraphResponse appendToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                directSplitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(appendToSplit);

            ShaderGraphResponse appendToMultiply = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                appendNodeId,
                "Out",
                multiplyNodeId,
                "A");
            ShaderGraphTestAssets.RequirePackageReady(appendToMultiply);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyColorNodeId, "Out", multiplyNodeId, "B"));

            ShaderGraphResponse multiplyToSplit = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                multiplyNodeId,
                "Out",
                multipliedSplitNodeId,
                "In");
            ShaderGraphTestAssets.RequirePackageReady(multiplyToSplit);

            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, directSplitNodeId, "R", directSinkNodeId, "X"));
            ShaderGraphTestAssets.RequirePackageReady(
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multipliedSplitNodeId, "G", multipliedSinkNodeId, "X"));

            var splitResolved = ShaderGraphTestAssets.RequireDictionary(appendToSplit.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(splitResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(splitResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(splitResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.SplitNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(splitResolved, "inputSlotId"), Is.EqualTo(0));

            var multiplyResolved = ShaderGraphTestAssets.RequireDictionary(appendToMultiply.Data, "resolvedConnection");
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "outputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.AppendVectorNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "outputSlotId"), Is.EqualTo(2));
            Assert.That(ShaderGraphTestAssets.GetString(multiplyResolved, "inputNodeType"), Is.EqualTo("UnityEditor.ShaderGraph.MultiplyNode"));
            Assert.That(ShaderGraphTestAssets.GetInt(multiplyResolved, "inputSlotId"), Is.EqualTo(0));

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            ShaderGraphTestAssets.RequirePackageReady(summaryResponse);

            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "nodeCount"), Is.EqualTo(9));
            Assert.That(ShaderGraphTestAssets.GetInt(summaryResponse.Data, "connectionCount"), Is.EqualTo(8));
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

        private string CreateBlankSubGraph(string graphNamePrefix, out ShaderGraphResponse createResponse)
        {
            string graphName = $"{graphNamePrefix}_{Guid.NewGuid():N}";
            createResponse = ShaderGraphAssetTool.HandleCreateSubGraph(
                graphName,
                temporaryFolder.AssetPath,
                "blank");
            ShaderGraphTestAssets.RequirePackageReady(createResponse);

            string assetPath = ShaderGraphTestAssets.GetString(createResponse.Data, "assetPath");
            Assert.That(assetPath, Does.EndWith(".shadersubgraph"));
            Assert.That(AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath), Is.Not.Null);
            return assetPath;
        }

    }
}
