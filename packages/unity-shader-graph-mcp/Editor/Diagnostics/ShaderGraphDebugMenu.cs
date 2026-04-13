using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShaderGraphMcp.Editor.Adapters;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Diagnostics
{
    internal static class ShaderGraphDebugMenu
    {
        private const string DefaultAssetPath = "Assets/ShaderGraphs/ConnectSmoke.shadergraph";
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
            "Append",
            "Vector2",
            "Vector3",
            "Vector4",
            "Normalize",
            "Time",
            "TilingAndOffset",
            "SampleTexture2D",
        };
        private static readonly string[] PreferredArithmeticConnectNodeTypes =
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
        };

        [MenuItem("Tools/Shader Graph MCP/Debug/Read Graph Summary")]
        public static void ReadGraphSummary()
        {
            string assetPath = ResolveTargetAssetPath();
            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Vector1 Node")]
        public static void AddVector1Node()
        {
            string assetPath = ResolveTargetAssetPath();
            string displayName = $"Vector1 {DateTime.Now:HHmmss}";

            LogResponse(
                "add_node",
                assetPath,
                ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", displayName)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Node")]
        public static void AddColorNode()
        {
            string assetPath = ResolveTargetAssetPath();
            string displayName = $"Color {DateTime.Now:HHmmss}";

            LogResponse(
                "add_node",
                assetPath,
                ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", displayName)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Split Node")]
        public static void AddSplitNode()
        {
            string assetPath = ResolveTargetAssetPath();
            string displayName = $"Split {DateTime.Now:HHmmss}";

            LogResponse(
                "add_node",
                assetPath,
                ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", displayName)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Common Graph-Addable Sample Nodes")]
        public static void AddCommonGraphAddableSampleNodes()
        {
            string assetPath = ResolveTargetAssetPath();
            IReadOnlyList<string> selectedNodeTypes = SelectPreferredSupportedNodeTypes(6);
            if (selectedNodeTypes.Count == 0)
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] No preferred common graph-addable node types were available in the current supported catalog."
                );
                return;
            }

            Debug.Log(
                $"[ShaderGraphMcp] Adding common graph-addable sample nodes to '{assetPath}': {string.Join(", ", selectedNodeTypes)}");

            for (int index = 0; index < selectedNodeTypes.Count; index += 1)
            {
                string nodeType = selectedNodeTypes[index];
                string displayName = $"{nodeType} {DateTime.Now:HHmmss}";
                LogResponse(
                    "add_node",
                    assetPath,
                    ShaderGraphAssetTool.HandleAddNode(assetPath, nodeType, displayName)
                );
            }

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Vector1 Arithmetic Connection Samples")]
        public static void AddVector1ArithmeticConnectionSamples()
        {
            string assetPath = ResolveTargetAssetPath();
            IReadOnlyList<string> selectedNodeTypes = SelectPreferredSupportedArithmeticNodeTypes();
            if (selectedNodeTypes.Count == 0)
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] No preferred arithmetic node types were available in the current supported catalog."
                );
                return;
            }

            foreach (string nodeType in selectedNodeTypes)
            {
                string[] inputPorts = GetArithmeticInputPorts(nodeType);
                if (inputPorts.Length == 0)
                {
                    continue;
                }

                ShaderGraphResponse addArithmeticNode = ShaderGraphAssetTool.HandleAddNode(
                    assetPath,
                    nodeType,
                    $"{nodeType} {DateTime.Now:HHmmss}");
                LogResponse("add_node", assetPath, addArithmeticNode);

                ShaderGraphResponse addSinkNode = ShaderGraphAssetTool.HandleAddNode(
                    assetPath,
                    "Vector1",
                    $"{nodeType} Sink {DateTime.Now:HHmmss}");
                LogResponse("add_node", assetPath, addSinkNode);

                if (!TryExtractAddedNodeId(addArithmeticNode, out string arithmeticNodeId) ||
                    !TryExtractAddedNodeId(addSinkNode, out string sinkNodeId))
                {
                    Debug.LogError(
                        $"[ShaderGraphMcp] Could not extract node ids for the '{nodeType}' arithmetic connection sample."
                    );
                    continue;
                }

                for (int index = 0; index < inputPorts.Length; index += 1)
                {
                    string inputPort = inputPorts[index];
                    ShaderGraphResponse addSourceNode = ShaderGraphAssetTool.HandleAddNode(
                        assetPath,
                        "Vector1",
                        $"{nodeType} {inputPort} {DateTime.Now:HHmmss}");
                    LogResponse("add_node", assetPath, addSourceNode);

                    if (!TryExtractAddedNodeId(addSourceNode, out string sourceNodeId))
                    {
                        Debug.LogError(
                            $"[ShaderGraphMcp] Could not extract source node id for '{nodeType}.{inputPort}'."
                        );
                        continue;
                    }

                    LogResponse(
                        "connect_ports",
                        assetPath,
                        ShaderGraphAssetTool.HandleConnectPorts(
                            assetPath,
                            sourceNodeId,
                            "Out",
                            arithmeticNodeId,
                            inputPort
                        )
                    );
                }

                LogResponse(
                    "connect_ports",
                    assetPath,
                    ShaderGraphAssetTool.HandleConnectPorts(
                        assetPath,
                        arithmeticNodeId,
                        "Out",
                        sinkNodeId,
                        "X"
                    )
                );
            }

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Arithmetic Chain Sample")]
        public static void AddArithmeticChainSample()
        {
            string assetPath = ResolveTargetAssetPath();

            if (!IsArithmeticNodeSupported("Add") || !IsArithmeticNodeSupported("Multiply"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Arithmetic chain sample requires Add and Multiply in the current supported catalog."
                );
                return;
            }

            ShaderGraphResponse addNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Add",
                $"Add {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addNodeResponse);

            ShaderGraphResponse multiplyNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Multiply",
                $"Multiply {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, multiplyNodeResponse);

            ShaderGraphResponse sourceAResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Add A {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, sourceAResponse);

            ShaderGraphResponse sourceBResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Add B {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, sourceBResponse);

            ShaderGraphResponse multiplyBResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Multiply B {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, multiplyBResponse);

            ShaderGraphResponse sinkResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Chain Sink {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, sinkResponse);

            if (!TryExtractAddedNodeId(addNodeResponse, out string addNodeId) ||
                !TryExtractAddedNodeId(multiplyNodeResponse, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(sourceAResponse, out string sourceANodeId) ||
                !TryExtractAddedNodeId(sourceBResponse, out string sourceBNodeId) ||
                !TryExtractAddedNodeId(multiplyBResponse, out string multiplyBNodeId) ||
                !TryExtractAddedNodeId(sinkResponse, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract one or more node ids for the arithmetic chain sample.");
                return;
            }

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", addNodeId, "A")
            );
            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", addNodeId, "B")
            );
            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, addNodeId, "Out", multiplyNodeId, "A")
            );
            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyBNodeId, "Out", multiplyNodeId, "B")
            );
            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", sinkNodeId, "X")
            );

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Comparison Branch Sample")]
        public static void AddComparisonBranchSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Comparison") || !supportedNodeTypes.Contains("Branch"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Comparison and Branch must both be graph-addable before the logic sample can run."
                );
                return;
            }

            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Comparison {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addComparison);

            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addBranch);

            ShaderGraphResponse addSourceA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Comparison A {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSourceA);

            ShaderGraphResponse addSourceB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Comparison B {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSourceB);

            ShaderGraphResponse addTrue = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch True {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addTrue);

            ShaderGraphResponse addFalse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch False {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addFalse);

            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Sink {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addSourceA, out string sourceANodeId) ||
                !TryExtractAddedNodeId(addSourceB, out string sourceBNodeId) ||
                !TryExtractAddedNodeId(addTrue, out string trueNodeId) ||
                !TryExtractAddedNodeId(addFalse, out string falseNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError(
                    "[ShaderGraphMcp] Could not extract node ids for the Comparison -> Branch sample."
                );
                return;
            }

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", comparisonNodeId, "A")
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", comparisonNodeId, "B")
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate")
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueNodeId, "Out", branchNodeId, "True")
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseNodeId, "Out", branchNodeId, "False")
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", sinkNodeId, "X")
            );

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Combine Split Sample")]
        public static void AddCombineSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Combine"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Combine must be graph-addable before the Combine -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addCombine = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", $"Combine {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addR = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine R {DateTime.Now:HHmmss}");
            ShaderGraphResponse addG = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine G {DateTime.Now:HHmmss}");
            ShaderGraphResponse addB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addCombine);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addR);
            LogResponse("add_node", assetPath, addG);
            LogResponse("add_node", assetPath, addB);
            LogResponse("add_node", assetPath, addA);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addCombine, out string combineNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addR, out string rNodeId) ||
                !TryExtractAddedNodeId(addG, out string gNodeId) ||
                !TryExtractAddedNodeId(addB, out string bNodeId) ||
                !TryExtractAddedNodeId(addA, out string aNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Combine -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, combineNodeId, "RGBA", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Vector4 Split Sample")]
        public static void AddVector4SplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Vector4"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Vector4 must be graph-addable before the Vector4 -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addVector4 = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", $"Vector4 {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addX = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 X {DateTime.Now:HHmmss}");
            ShaderGraphResponse addY = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Y {DateTime.Now:HHmmss}");
            ShaderGraphResponse addZ = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Z {DateTime.Now:HHmmss}");
            ShaderGraphResponse addW = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 W {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addVector4);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addX);
            LogResponse("add_node", assetPath, addY);
            LogResponse("add_node", assetPath, addZ);
            LogResponse("add_node", assetPath, addW);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addVector4, out string vector4NodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addX, out string xNodeId) ||
                !TryExtractAddedNodeId(addY, out string yNodeId) ||
                !TryExtractAddedNodeId(addZ, out string zNodeId) ||
                !TryExtractAddedNodeId(addW, out string wNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Vector4 -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector4NodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Combine Append Split Sample")]
        public static void AddCombineAppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Combine") || !supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Combine and Append must both be graph-addable before the Combine -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addCombine = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", $"Combine Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Combine {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Combine Append Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addR = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine R {DateTime.Now:HHmmss}");
            ShaderGraphResponse addG = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine G {DateTime.Now:HHmmss}");
            ShaderGraphResponse addB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addCombine);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addR);
            LogResponse("add_node", assetPath, addG);
            LogResponse("add_node", assetPath, addB);
            LogResponse("add_node", assetPath, addA);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addCombine, out string combineNodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addR, out string rNodeId) ||
                !TryExtractAddedNodeId(addG, out string gNodeId) ||
                !TryExtractAddedNodeId(addB, out string bNodeId) ||
                !TryExtractAddedNodeId(addA, out string aNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Combine -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, combineNodeId, "RGBA", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Vector4 Append Split Sample")]
        public static void AddVector4AppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Vector4") || !supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Vector4 and Append must both be graph-addable before the Vector4 -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addVector4 = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", $"Vector4 Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Vector4 {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Vector4 Append Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addX = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 X {DateTime.Now:HHmmss}");
            ShaderGraphResponse addY = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Y {DateTime.Now:HHmmss}");
            ShaderGraphResponse addZ = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Z {DateTime.Now:HHmmss}");
            ShaderGraphResponse addW = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 W {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addVector4);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addX);
            LogResponse("add_node", assetPath, addY);
            LogResponse("add_node", assetPath, addZ);
            LogResponse("add_node", assetPath, addW);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addVector4, out string vector4NodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addX, out string xNodeId) ||
                !TryExtractAddedNodeId(addY, out string yNodeId) ||
                !TryExtractAddedNodeId(addZ, out string zNodeId) ||
                !TryExtractAddedNodeId(addW, out string wNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Vector4 -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector4NodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Combine Append Lerp Split Sample")]
        public static void AddCombineAppendLerpSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Combine") ||
                !supportedNodeTypes.Contains("Append") ||
                !supportedNodeTypes.Contains("Lerp"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Combine, Append, and Lerp must all be graph-addable before the Combine -> Append -> Lerp -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addCombine = ShaderGraphAssetTool.HandleAddNode(assetPath, "Combine", $"Combine Lerp {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Lerp {DateTime.Now:HHmmss}");
            ShaderGraphResponse addLerp = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", $"Lerp Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Lerp Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addR = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine R {DateTime.Now:HHmmss}");
            ShaderGraphResponse addG = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine G {DateTime.Now:HHmmss}");
            ShaderGraphResponse addB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Combine A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppendScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp Color B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addT = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp T {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addCombine);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addLerp);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addR);
            LogResponse("add_node", assetPath, addG);
            LogResponse("add_node", assetPath, addB);
            LogResponse("add_node", assetPath, addA);
            LogResponse("add_node", assetPath, addAppendScalar);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addT);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addCombine, out string combineNodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addLerp, out string lerpNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addR, out string rNodeId) ||
                !TryExtractAddedNodeId(addG, out string gNodeId) ||
                !TryExtractAddedNodeId(addB, out string bNodeId) ||
                !TryExtractAddedNodeId(addA, out string aNodeId) ||
                !TryExtractAddedNodeId(addAppendScalar, out string appendScalarNodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addT, out string tNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Combine -> Append -> Lerp -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, rNodeId, "Out", combineNodeId, "R"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, gNodeId, "Out", combineNodeId, "G"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, bNodeId, "Out", combineNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, aNodeId, "Out", combineNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, combineNodeId, "RGBA", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendScalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", lerpNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, lerpNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Vector4 Append Branch Split Sample")]
        public static void AddVector4AppendBranchSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Vector4") ||
                !supportedNodeTypes.Contains("Append") ||
                !supportedNodeTypes.Contains("Comparison") ||
                !supportedNodeTypes.Contains("Branch"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Vector4, Append, Comparison, and Branch must all be graph-addable before the Vector4 -> Append -> Branch -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addVector4 = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector4", $"Vector4 Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Compare Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Branch Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addX = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 X {DateTime.Now:HHmmss}");
            ShaderGraphResponse addY = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Y {DateTime.Now:HHmmss}");
            ShaderGraphResponse addZ = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 Z {DateTime.Now:HHmmss}");
            ShaderGraphResponse addW = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector4 W {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppendScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch False {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Compare A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Compare B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addVector4);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranch);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addX);
            LogResponse("add_node", assetPath, addY);
            LogResponse("add_node", assetPath, addZ);
            LogResponse("add_node", assetPath, addW);
            LogResponse("add_node", assetPath, addAppendScalar);
            LogResponse("add_node", assetPath, addFalseColor);
            LogResponse("add_node", assetPath, addCompareA);
            LogResponse("add_node", assetPath, addCompareB);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addVector4, out string vector4NodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addX, out string xNodeId) ||
                !TryExtractAddedNodeId(addY, out string yNodeId) ||
                !TryExtractAddedNodeId(addZ, out string zNodeId) ||
                !TryExtractAddedNodeId(addW, out string wNodeId) ||
                !TryExtractAddedNodeId(addAppendScalar, out string appendScalarNodeId) ||
                !TryExtractAddedNodeId(addFalseColor, out string falseColorNodeId) ||
                !TryExtractAddedNodeId(addCompareA, out string compareANodeId) ||
                !TryExtractAddedNodeId(addCompareB, out string compareBNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Vector4 -> Append -> Branch -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, xNodeId, "Out", vector4NodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, yNodeId, "Out", vector4NodeId, "Y"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, zNodeId, "Out", vector4NodeId, "Z"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, wNodeId, "Out", vector4NodeId, "W"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector4NodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendScalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", branchNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Multiply Split Sample")]
        public static void AddColorMultiplySplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Multiply"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Multiply must be graph-addable before the Color -> Multiply -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addMultiply = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", $"Multiply {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Color A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Color B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Color Multiply Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addMultiply);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColorA);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addMultiply, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColorA, out string colorANodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Color -> Multiply -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", multiplyNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", multiplyNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D Split Sample")]
        public static void AddUvSampleTexture2DSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, and Texture2DAsset must all be graph-addable before the UV -> SampleTexture2D -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSample = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"Sample {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Texture Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Texture Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSample);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSample, out string sampleNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV -> SampleTexture2D -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D Multiply Split Sample")]
        public static void AddUvSampleTexture2DMultiplySplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, and Texture2DAsset must all be graph-addable before the UV SampleTexture2D color workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSample = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"Sample {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Tint {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultiply = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", $"Texture Multiply {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Texture Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Texture Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSample);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addMultiply);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSample, out string sampleNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addMultiply, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV SampleTexture2D color workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", multiplyNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", multiplyNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D NormalStrength Split Sample")]
        public static void AddUvSampleTexture2DNormalStrengthSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset") ||
                !supportedNodeTypes.Contains("NormalStrength"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, Texture2DAsset, and NormalStrength must all be graph-addable before the UV SampleTexture2D normal workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSample = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"Sample {DateTime.Now:HHmmss}");
            ShaderGraphResponse addStrength = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Normal Strength {DateTime.Now:HHmmss}");
            ShaderGraphResponse addNormalStrength = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalStrength", $"NormalStrength {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Normal Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Normal Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSample);
            LogResponse("add_node", assetPath, addStrength);
            LogResponse("add_node", assetPath, addNormalStrength);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSample, out string sampleNodeId) ||
                !TryExtractAddedNodeId(addStrength, out string strengthNodeId) ||
                !TryExtractAddedNodeId(addNormalStrength, out string normalStrengthNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV SampleTexture2D NormalStrength workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", normalStrengthNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, strengthNodeId, "Out", normalStrengthNodeId, "Strength"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, normalStrengthNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "B", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D NormalUnpack Sample")]
        public static void AddUvSampleTexture2DNormalUnpackSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset") ||
                !supportedNodeTypes.Contains("NormalUnpack"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, Texture2DAsset, and NormalUnpack must all be graph-addable before the UV SampleTexture2D normal unpack workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSample = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"Sample {DateTime.Now:HHmmss}");
            ShaderGraphResponse addNormalUnpack = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalUnpack", $"NormalUnpack {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSample);
            LogResponse("add_node", assetPath, addNormalUnpack);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSample, out string sampleNodeId) ||
                !TryExtractAddedNodeId(addNormalUnpack, out string normalUnpackNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV SampleTexture2D NormalUnpack workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "RGBA", normalUnpackNodeId, "In"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D NormalBlend Sample")]
        public static void AddUvSampleTexture2DNormalBlendSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset") ||
                !supportedNodeTypes.Contains("NormalBlend"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, Texture2DAsset, and NormalBlend must all be graph-addable before the UV SampleTexture2D normal blend workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSampleA = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"SampleA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSampleB = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"SampleB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addNormalBlend = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalBlend", $"NormalBlend {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSampleA);
            LogResponse("add_node", assetPath, addSampleB);
            LogResponse("add_node", assetPath, addNormalBlend);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSampleA, out string sampleANodeId) ||
                !TryExtractAddedNodeId(addSampleB, out string sampleBNodeId) ||
                !TryExtractAddedNodeId(addNormalBlend, out string normalBlendNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV SampleTexture2D NormalBlend workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleANodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleBNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleANodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleBNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleANodeId, "RGBA", normalBlendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleBNodeId, "RGBA", normalBlendNodeId, "B"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV SampleTexture2D NormalReconstructZ Sample")]
        public static void AddUvSampleTexture2DNormalReconstructZSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("SampleTexture2D") ||
                !supportedNodeTypes.Contains("Texture2DAsset") ||
                !supportedNodeTypes.Contains("Vector2") ||
                !supportedNodeTypes.Contains("NormalReconstructZ"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, SampleTexture2D, Texture2DAsset, Vector2, and NormalReconstructZ must all be graph-addable before the UV SampleTexture2D normal reconstruct workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSample = ShaderGraphAssetTool.HandleAddNode(assetPath, "SampleTexture2D", $"Sample {DateTime.Now:HHmmss}");
            ShaderGraphResponse addVector2 = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector2", $"Vector2 {DateTime.Now:HHmmss}");
            ShaderGraphResponse addNormalReconstructZ = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalReconstructZ", $"NormalReconstructZ {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addSample);
            LogResponse("add_node", assetPath, addVector2);
            LogResponse("add_node", assetPath, addNormalReconstructZ);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addSample, out string sampleNodeId) ||
                !TryExtractAddedNodeId(addVector2, out string vector2NodeId) ||
                !TryExtractAddedNodeId(addNormalReconstructZ, out string normalReconstructZNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV SampleTexture2D NormalReconstructZ workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", sampleNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", sampleNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "R", vector2NodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sampleNodeId, "G", vector2NodeId, "Y"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, vector2NodeId, "Out", normalReconstructZNodeId, "In"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add UV NormalFromTexture Sample")]
        public static void AddUvNormalFromTextureSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("UV") ||
                !supportedNodeTypes.Contains("TilingAndOffset") ||
                !supportedNodeTypes.Contains("Texture2DAsset") ||
                !supportedNodeTypes.Contains("Vector1") ||
                !supportedNodeTypes.Contains("NormalFromTexture"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] UV, TilingAndOffset, Texture2DAsset, Vector1, and NormalFromTexture must all be graph-addable before the UV NormalFromTexture workflow sample can run."
                );
                return;
            }

            ShaderGraphResponse addUv = ShaderGraphAssetTool.HandleAddNode(assetPath, "UV", $"UV {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTilingAndOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "TilingAndOffset", $"TilingOffset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "Texture2DAsset", $"Texture {DateTime.Now:HHmmss}");
            ShaderGraphResponse addOffset = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Offset {DateTime.Now:HHmmss}");
            ShaderGraphResponse addStrength = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Strength {DateTime.Now:HHmmss}");
            ShaderGraphResponse addNormalFromTexture = ShaderGraphAssetTool.HandleAddNode(assetPath, "NormalFromTexture", $"NormalFromTexture {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addUv);
            LogResponse("add_node", assetPath, addTilingAndOffset);
            LogResponse("add_node", assetPath, addTexture);
            LogResponse("add_node", assetPath, addOffset);
            LogResponse("add_node", assetPath, addStrength);
            LogResponse("add_node", assetPath, addNormalFromTexture);

            if (!TryExtractAddedNodeId(addUv, out string uvNodeId) ||
                !TryExtractAddedNodeId(addTilingAndOffset, out string tilingAndOffsetNodeId) ||
                !TryExtractAddedNodeId(addTexture, out string textureNodeId) ||
                !TryExtractAddedNodeId(addOffset, out string offsetNodeId) ||
                !TryExtractAddedNodeId(addStrength, out string strengthNodeId) ||
                !TryExtractAddedNodeId(addNormalFromTexture, out string normalFromTextureNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the UV NormalFromTexture workflow sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, uvNodeId, "Out", tilingAndOffsetNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tilingAndOffsetNodeId, "Out", normalFromTextureNodeId, "UV"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, textureNodeId, "Out", normalFromTextureNodeId, "Texture"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, offsetNodeId, "Out", normalFromTextureNodeId, "Offset"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, strengthNodeId, "Out", normalFromTextureNodeId, "Strength"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Comparison Predicate FanOut Sample")]
        public static void AddComparisonPredicateFanOutSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Comparison") ||
                !supportedNodeTypes.Contains("Branch") ||
                !supportedNodeTypes.Contains("Vector1"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Comparison, Branch, and Vector1 must all be graph-addable before the comparison predicate fan-out sample can run."
                );
                return;
            }

            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Comparison {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranchA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"BranchA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranchB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"BranchB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSourceA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"ComparisonA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSourceB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"ComparisonB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTrueA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"TrueA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"FalseA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTrueB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"TrueB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"FalseB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSinkA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"SinkA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSinkB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"SinkB {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranchA);
            LogResponse("add_node", assetPath, addBranchB);
            LogResponse("add_node", assetPath, addSourceA);
            LogResponse("add_node", assetPath, addSourceB);
            LogResponse("add_node", assetPath, addTrueA);
            LogResponse("add_node", assetPath, addFalseA);
            LogResponse("add_node", assetPath, addTrueB);
            LogResponse("add_node", assetPath, addFalseB);
            LogResponse("add_node", assetPath, addSinkA);
            LogResponse("add_node", assetPath, addSinkB);

            if (!TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranchA, out string branchANodeId) ||
                !TryExtractAddedNodeId(addBranchB, out string branchBNodeId) ||
                !TryExtractAddedNodeId(addSourceA, out string sourceANodeId) ||
                !TryExtractAddedNodeId(addSourceB, out string sourceBNodeId) ||
                !TryExtractAddedNodeId(addTrueA, out string trueANodeId) ||
                !TryExtractAddedNodeId(addFalseA, out string falseANodeId) ||
                !TryExtractAddedNodeId(addTrueB, out string trueBNodeId) ||
                !TryExtractAddedNodeId(addFalseB, out string falseBNodeId) ||
                !TryExtractAddedNodeId(addSinkA, out string sinkANodeId) ||
                !TryExtractAddedNodeId(addSinkB, out string sinkBNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the comparison predicate fan-out sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchANodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchBNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueANodeId, "Out", branchANodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseANodeId, "Out", branchANodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueBNodeId, "Out", branchBNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseBNodeId, "Out", branchBNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchANodeId, "Out", sinkANodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchBNodeId, "Out", sinkBNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Branch Output FanOut Sample")]
        public static void AddBranchOutputFanOutSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Comparison") ||
                !supportedNodeTypes.Contains("Branch") ||
                !supportedNodeTypes.Contains("Vector1") ||
                !supportedNodeTypes.Contains("Add"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Comparison, Branch, Vector1, and Add must all be graph-addable before the branch output fan-out sample can run."
                );
                return;
            }

            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Comparison {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSourceA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"ComparisonA {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSourceB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"ComparisonB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTrue = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"True {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalse = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"False {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Sink {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAdd = ShaderGraphAssetTool.HandleAddNode(assetPath, "Add", $"Add {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAddB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"AddB {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAddSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"AddSink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranch);
            LogResponse("add_node", assetPath, addSourceA);
            LogResponse("add_node", assetPath, addSourceB);
            LogResponse("add_node", assetPath, addTrue);
            LogResponse("add_node", assetPath, addFalse);
            LogResponse("add_node", assetPath, addSink);
            LogResponse("add_node", assetPath, addAdd);
            LogResponse("add_node", assetPath, addAddB);
            LogResponse("add_node", assetPath, addAddSink);

            if (!TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addSourceA, out string sourceANodeId) ||
                !TryExtractAddedNodeId(addSourceB, out string sourceBNodeId) ||
                !TryExtractAddedNodeId(addTrue, out string trueNodeId) ||
                !TryExtractAddedNodeId(addFalse, out string falseNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId) ||
                !TryExtractAddedNodeId(addAdd, out string addNodeId) ||
                !TryExtractAddedNodeId(addAddB, out string addBNodeId) ||
                !TryExtractAddedNodeId(addAddSink, out string addSinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the branch output fan-out sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, sourceBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueNodeId, "Out", branchNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseNodeId, "Out", branchNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", sinkNodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", addNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, addBNodeId, "Out", addNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, addNodeId, "Out", addSinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Branch Split Sample")]
        public static void AddColorBranchSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Comparison") || !supportedNodeTypes.Contains("Branch"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Comparison and Branch must both be graph-addable before the Color -> Branch -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Comparison {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTrueColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch True Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch False Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Comparison A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Comparison B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Color Branch Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranch);
            LogResponse("add_node", assetPath, addTrueColor);
            LogResponse("add_node", assetPath, addFalseColor);
            LogResponse("add_node", assetPath, addCompareA);
            LogResponse("add_node", assetPath, addCompareB);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addTrueColor, out string trueColorNodeId) ||
                !TryExtractAddedNodeId(addFalseColor, out string falseColorNodeId) ||
                !TryExtractAddedNodeId(addCompareA, out string compareANodeId) ||
                !TryExtractAddedNodeId(addCompareB, out string compareBNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Color -> Branch -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueColorNodeId, "Out", branchNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "B", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Lerp Split Sample")]
        public static void AddColorLerpSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Lerp"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Lerp must be graph-addable before the Color -> Lerp -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addLerp = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", $"Lerp {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp Color A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp Color B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addT = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp T {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Color Lerp Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addLerp);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColorA);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addT);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addLerp, out string lerpNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColorA, out string colorANodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addT, out string tNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Color -> Lerp -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", lerpNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, lerpNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Append Split Sample")]
        public static void AddColorAppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append must be graph-addable before the Color -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Append Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Color -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Append Chain Sample")]
        public static void AddAppendChainSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append must be graph-addable before the Append -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppendA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppendB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Chain Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Chain Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalarA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Chain Scalar A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalarB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Chain Scalar B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Chain Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppendA);
            LogResponse("add_node", assetPath, addAppendB);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalarA);
            LogResponse("add_node", assetPath, addScalarB);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addAppendA, out string appendANodeId) ||
                !TryExtractAddedNodeId(addAppendB, out string appendBNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalarA, out string scalarANodeId) ||
                !TryExtractAddedNodeId(addScalarB, out string scalarBNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Append -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendANodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarANodeId, "Out", appendANodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendANodeId, "Out", appendBNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarBNodeId, "Out", appendBNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendBNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Append Multiply Split Sample")]
        public static void AddAppendMultiplySplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append") || !supportedNodeTypes.Contains("Multiply"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append and Multiply must both be graph-addable before the Append -> Multiply -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultiply = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", $"Multiply Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Mix Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Mix Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Mix Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Mix Color B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Mix Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addMultiply);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addMultiply, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Append -> Multiply -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", multiplyNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", multiplyNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Append Output FanOut Sample")]
        public static void AddAppendOutputFanOutSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append") || !supportedNodeTypes.Contains("Multiply") || !supportedNodeTypes.Contains("Split"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append, Multiply, and Split must all be graph-addable before the Append output fan-out sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append FanOut {DateTime.Now:HHmmss}");
            ShaderGraphResponse addDirectSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"FanOut Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultiply = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", $"FanOut Multiply {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultipliedSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"FanOut Result Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"FanOut Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"FanOut Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultiplyColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"FanOut Multiply Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addDirectSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"FanOut Direct Sink {DateTime.Now:HHmmss}");
            ShaderGraphResponse addMultipliedSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"FanOut Result Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addDirectSplit);
            LogResponse("add_node", assetPath, addMultiply);
            LogResponse("add_node", assetPath, addMultipliedSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addMultiplyColor);
            LogResponse("add_node", assetPath, addDirectSink);
            LogResponse("add_node", assetPath, addMultipliedSink);

            if (!TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addDirectSplit, out string directSplitNodeId) ||
                !TryExtractAddedNodeId(addMultiply, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(addMultipliedSplit, out string multipliedSplitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addMultiplyColor, out string multiplyColorNodeId) ||
                !TryExtractAddedNodeId(addDirectSink, out string directSinkNodeId) ||
                !TryExtractAddedNodeId(addMultipliedSink, out string multipliedSinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Append output fan-out sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", directSplitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", multiplyNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyColorNodeId, "Out", multiplyNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, directSplitNodeId, "R", directSinkNodeId, "X"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", multipliedSplitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multipliedSplitNodeId, "G", multipliedSinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Append Lerp Split Sample")]
        public static void AddAppendLerpSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append") || !supportedNodeTypes.Contains("Lerp"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append and Lerp must both be graph-addable before the Append -> Lerp -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Lerp {DateTime.Now:HHmmss}");
            ShaderGraphResponse addLerp = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", $"Lerp Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Lerp Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp Color A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp Color B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addT = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp T {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addLerp);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addT);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addLerp, out string lerpNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addT, out string tNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Append -> Lerp -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", lerpNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, lerpNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Append Branch Split Sample")]
        public static void AddAppendBranchSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Append") ||
                !supportedNodeTypes.Contains("Comparison") ||
                !supportedNodeTypes.Contains("Branch"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Append, Comparison, and Branch must all be graph-addable before the Append -> Branch -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Compare Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Branch Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch Color {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch False {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Compare A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Compare B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranch);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColor);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addFalseColor);
            LogResponse("add_node", assetPath, addCompareA);
            LogResponse("add_node", assetPath, addCompareB);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addFalseColor, out string falseColorNodeId) ||
                !TryExtractAddedNodeId(addCompareA, out string compareANodeId) ||
                !TryExtractAddedNodeId(addCompareB, out string compareBNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Append -> Branch -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", branchNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "B", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Multiply Append Split Sample")]
        public static void AddMultiplyAppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Multiply") || !supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Multiply and Append must both be graph-addable before the Multiply -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addMultiply = ShaderGraphAssetTool.HandleAddNode(assetPath, "Multiply", $"Multiply Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Mix {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Multiply Append Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Multiply A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Multiply B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addMultiply);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColorA);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addMultiply, out string multiplyNodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColorA, out string colorANodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Multiply -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", multiplyNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", multiplyNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, multiplyNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "G", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Lerp Append Split Sample")]
        public static void AddLerpAppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Lerp") || !supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Lerp and Append must both be graph-addable before the Lerp -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addLerp = ShaderGraphAssetTool.HandleAddNode(assetPath, "Lerp", $"Lerp Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Lerp {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Lerp Append Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addColorB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Lerp B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addT = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp T {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Lerp Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addLerp);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addColorA);
            LogResponse("add_node", assetPath, addColorB);
            LogResponse("add_node", assetPath, addT);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addLerp, out string lerpNodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addColorA, out string colorANodeId) ||
                !TryExtractAddedNodeId(addColorB, out string colorBNodeId) ||
                !TryExtractAddedNodeId(addT, out string tNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Lerp -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorANodeId, "Out", lerpNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, colorBNodeId, "Out", lerpNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, tNodeId, "Out", lerpNodeId, "T"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, lerpNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "R", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Branch Append Split Sample")]
        public static void AddBranchAppendSplitSample()
        {
            string assetPath = ResolveTargetAssetPath();

            IReadOnlyList<string> supportedNodeTypes = SelectPreferredSupportedNodeTypes(128);
            if (!supportedNodeTypes.Contains("Comparison") ||
                !supportedNodeTypes.Contains("Branch") ||
                !supportedNodeTypes.Contains("Append"))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] Comparison, Branch, and Append must all be graph-addable before the Branch -> Append -> Split sample can run."
                );
                return;
            }

            ShaderGraphResponse addComparison = ShaderGraphAssetTool.HandleAddNode(assetPath, "Comparison", $"Compare Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addBranch = ShaderGraphAssetTool.HandleAddNode(assetPath, "Branch", $"Branch Append {DateTime.Now:HHmmss}");
            ShaderGraphResponse addAppend = ShaderGraphAssetTool.HandleAddNode(assetPath, "Append", $"Append Branch {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Branch Append Split {DateTime.Now:HHmmss}");
            ShaderGraphResponse addTrueColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch True {DateTime.Now:HHmmss}");
            ShaderGraphResponse addFalseColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Branch False {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Compare A {DateTime.Now:HHmmss}");
            ShaderGraphResponse addCompareB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Compare B {DateTime.Now:HHmmss}");
            ShaderGraphResponse addScalar = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Append Scalar {DateTime.Now:HHmmss}");
            ShaderGraphResponse addSink = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Branch Append Sink {DateTime.Now:HHmmss}");

            LogResponse("add_node", assetPath, addComparison);
            LogResponse("add_node", assetPath, addBranch);
            LogResponse("add_node", assetPath, addAppend);
            LogResponse("add_node", assetPath, addSplit);
            LogResponse("add_node", assetPath, addTrueColor);
            LogResponse("add_node", assetPath, addFalseColor);
            LogResponse("add_node", assetPath, addCompareA);
            LogResponse("add_node", assetPath, addCompareB);
            LogResponse("add_node", assetPath, addScalar);
            LogResponse("add_node", assetPath, addSink);

            if (!TryExtractAddedNodeId(addComparison, out string comparisonNodeId) ||
                !TryExtractAddedNodeId(addBranch, out string branchNodeId) ||
                !TryExtractAddedNodeId(addAppend, out string appendNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addTrueColor, out string trueColorNodeId) ||
                !TryExtractAddedNodeId(addFalseColor, out string falseColorNodeId) ||
                !TryExtractAddedNodeId(addCompareA, out string compareANodeId) ||
                !TryExtractAddedNodeId(addCompareB, out string compareBNodeId) ||
                !TryExtractAddedNodeId(addScalar, out string scalarNodeId) ||
                !TryExtractAddedNodeId(addSink, out string sinkNodeId))
            {
                Debug.LogError("[ShaderGraphMcp] Could not extract node ids for the Branch -> Append -> Split sample.");
                return;
            }

            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareANodeId, "Out", comparisonNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, compareBNodeId, "Out", comparisonNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, comparisonNodeId, "Out", branchNodeId, "Predicate"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, trueColorNodeId, "Out", branchNodeId, "True"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, falseColorNodeId, "Out", branchNodeId, "False"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, branchNodeId, "Out", appendNodeId, "A"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, scalarNodeId, "Out", appendNodeId, "B"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, appendNodeId, "Out", splitNodeId, "In"));
            LogResponse("connect_ports", assetPath, ShaderGraphAssetTool.HandleConnectPorts(assetPath, splitNodeId, "B", sinkNodeId, "X"));

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Create Blank Graph Here")]
        public static void CreateBlankGraphHere()
        {
            string targetDirectory = ResolveTargetDirectory();
            string graphName = $"DebugShaderGraph {DateTime.Now:HHmmss}";

            ShaderGraphResponse createResponse = ShaderGraphAssetTool.HandleCreateGraph(
                graphName,
                targetDirectory,
                "blank"
            );
            string assetPath = $"{targetDirectory.TrimEnd('/')}/{graphName}.shadergraph";
            LogResponse("create_graph", assetPath, createResponse);

            if (!createResponse.Success)
            {
                return;
            }

            UnityEngine.Object createdAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (createdAsset != null)
            {
                Selection.activeObject = createdAsset;
                EditorGUIUtility.PingObject(createdAsset);
            }

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Run Blank Graph Happy Path")]
        public static void RunBlankGraphHappyPathMenu()
        {
            string targetDirectory = ResolveTargetDirectory();
            string graphName = $"ReleaseSmokeGraph {DateTime.Now:HHmmss}";

            ShaderGraphResponse response = RunBlankGraphHappyPath(targetDirectory, graphName, true);
            string assetPath = $"{targetDirectory.TrimEnd('/')}/{graphName}.shadergraph";
            LogResponse("release_happy_path", assetPath, response);
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Float Vector1 Property")]
        public static void AddFloatVector1Property()
        {
            AddPropertyAndReadSummary(
                "Float/Vector1",
                "0",
                "Float Vector1"
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Property")]
        public static void AddColorProperty()
        {
            AddPropertyAndReadSummary(
                "Color",
                "#FFFFFFFF",
                "Color"
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Save Graph")]
        public static void SaveGraph()
        {
            string assetPath = ResolveTargetAssetPath();
            LogResponse(
                "save_graph",
                assetPath,
                ShaderGraphAssetTool.HandleSaveGraph(assetPath)
            );

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Write Node Catalog Report")]
        public static void WriteNodeCatalogReport()
        {
            const string diagnosticsFolderAssetPath = "Assets/ShaderGraphMcpDiagnostics";

            string diagnosticsFolderAbsolutePath = Path.Combine(Application.dataPath, "ShaderGraphMcpDiagnostics");
            Directory.CreateDirectory(diagnosticsFolderAbsolutePath);

            string fileName = $"shadergraph-node-catalog-report-{System.DateTime.Now:yyyyMMdd-HHmmss}.txt";
            string assetPath = $"{diagnosticsFolderAssetPath}/{fileName}";
            string absolutePath = Path.Combine(diagnosticsFolderAbsolutePath, fileName);

            int discoveredCount = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogCount();
            int supportedCount = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogCount();
            int probeRejectedCount = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogCount();
            int excludedBucketCount = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogBucketCount();
            int probeRejectedBucketCount = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogBucketCount();
            IReadOnlyList<string> discoveredLines = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogReportLines();
            IReadOnlyList<string> supportedLines = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogReportLines();
            IReadOnlyList<string> excludedLines = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogReportLines();
            IReadOnlyList<string> excludedBucketLines = ShaderGraphPackageGraphInspector.GetExcludedNodeCatalogBucketReportLines();
            IReadOnlyList<string> rejectedLines = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogReportLines();
            IReadOnlyList<string> rejectedBucketLines = ShaderGraphPackageGraphInspector.GetProbeRejectedNodeCatalogBucketReportLines();
            var builder = new StringBuilder();
            builder.AppendLine("Shader Graph Node Catalog Report");
            builder.AppendLine($"generatedAtUtc: {DateTime.UtcNow:O}");
            builder.AppendLine($"discoveredCount: {discoveredCount}");
            builder.AppendLine($"graphAddableCount: {supportedCount}");
            builder.AppendLine($"excludedCount: {excludedLines.Count}");
            builder.AppendLine($"probeRejectedCount: {probeRejectedCount}");
            builder.AppendLine($"excludedBucketCount: {excludedBucketCount}");
            builder.AppendLine($"probeRejectedBucketCount: {probeRejectedBucketCount}");
            builder.AppendLine();
            builder.AppendLine("## discoverable");
            builder.AppendLine();

            for (int index = 0; index < discoveredLines.Count; index += 1)
            {
                builder.AppendLine(discoveredLines[index]);
            }

            builder.AppendLine();
            builder.AppendLine("## graph-addable");
            builder.AppendLine();

            for (int index = 0; index < supportedLines.Count; index += 1)
            {
                builder.AppendLine(supportedLines[index]);
            }

            if (excludedLines.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## excluded-buckets");
                builder.AppendLine();

                for (int index = 0; index < excludedBucketLines.Count; index += 1)
                {
                    builder.AppendLine(excludedBucketLines[index]);
                }

                builder.AppendLine();
                builder.AppendLine("## excluded");
                builder.AppendLine();

                for (int index = 0; index < excludedLines.Count; index += 1)
                {
                    builder.AppendLine(excludedLines[index]);
                }
            }

            if (rejectedLines.Count > 0)
            {
                builder.AppendLine();
                builder.AppendLine("## probe-rejected-buckets");
                builder.AppendLine();

                for (int index = 0; index < rejectedBucketLines.Count; index += 1)
                {
                    builder.AppendLine(rejectedBucketLines[index]);
                }

                builder.AppendLine();
                builder.AppendLine("## probe-rejected");
                builder.AppendLine();

                for (int index = 0; index < rejectedLines.Count; index += 1)
                {
                    builder.AppendLine(rejectedLines[index]);
                }
            }

            File.WriteAllText(absolutePath, builder.ToString(), Encoding.UTF8);
            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            UnityEngine.Object reportAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (reportAsset != null)
            {
                Selection.activeObject = reportAsset;
                EditorGUIUtility.PingObject(reportAsset);
            }

            Debug.Log(
                $"[ShaderGraphMcp] Wrote node catalog report to '{assetPath}' " +
                $"with discovered={discoveredCount}, graphAddable={supportedCount}, " +
                $"excluded={excludedLines.Count} ({excludedBucketCount} buckets), " +
                $"probeRejected={probeRejectedCount} ({probeRejectedBucketCount} buckets).");
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Connect Latest Vector1 Nodes")]
        public static void ConnectLatestVector1Nodes()
        {
            string assetPath = ResolveTargetAssetPath();
            ShaderGraphResponse summary = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            LogResponse("read_graph_summary", assetPath, summary);

            if (!summary.Success)
            {
                return;
            }

            List<string> vector1NodeIds = ExtractVector1NodeIds(summary);
            if (vector1NodeIds.Count < 2)
            {
                Debug.LogError(
                    $"[ShaderGraphMcp] Need at least 2 Vector1 nodes before connect_ports. " +
                    $"Target='{assetPath}', found={vector1NodeIds.Count}."
                );
                return;
            }

            string outputNodeId = vector1NodeIds[vector1NodeIds.Count - 2];
            string inputNodeId = vector1NodeIds[vector1NodeIds.Count - 1];

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    outputNodeId,
                    "Out",
                    inputNodeId,
                    "X"
                )
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add 2 Vector1 Nodes And Connect")]
        public static void AddTwoVector1NodesAndConnect()
        {
            string assetPath = ResolveTargetAssetPath();

            ShaderGraphResponse addA = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector1 A {DateTime.Now:HHmmss}");
            LogResponse(
                "add_node",
                assetPath,
                addA
            );
            ShaderGraphResponse addB = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector1 B {DateTime.Now:HHmmss}");
            LogResponse(
                "add_node",
                assetPath,
                addB
            );

            if (!TryExtractAddedNodeId(addA, out string outputNodeId) ||
                !TryExtractAddedNodeId(addB, out string inputNodeId))
            {
                Debug.LogWarning(
                    "[ShaderGraphMcp] add_node responses did not expose stable objectId values. " +
                    "Falling back to read_graph_summary parsing."
                );
                ConnectLatestVector1Nodes();
                return;
            }

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    outputNodeId,
                    "Out",
                    inputNodeId,
                    "X"
                )
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color Node And Split Node And Connect")]
        public static void AddColorNodeAndSplitNodeAndConnect()
        {
            string assetPath = ResolveTargetAssetPath();

            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Color {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addColor);

            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSplit);

            if (!TryExtractAddedNodeId(addColor, out string outputNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string inputNodeId))
            {
                Debug.LogError(
                    "[ShaderGraphMcp] Could not extract node ids from add_node responses for the Color -> Split connect smoke test."
                );
                return;
            }

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    outputNodeId,
                    "Out",
                    inputNodeId,
                    "In"
                )
            );

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        [MenuItem("Tools/Shader Graph MCP/Debug/Add Color, Split, Vector1 Chain")]
        public static void AddColorSplitVector1Chain()
        {
            string assetPath = ResolveTargetAssetPath();

            ShaderGraphResponse addColor = ShaderGraphAssetTool.HandleAddNode(assetPath, "Color", $"Color {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addColor);

            ShaderGraphResponse addSplit = ShaderGraphAssetTool.HandleAddNode(assetPath, "Split", $"Split {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSplit);

            ShaderGraphResponse addVector1 = ShaderGraphAssetTool.HandleAddNode(assetPath, "Vector1", $"Vector1 {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addVector1);

            if (!TryExtractAddedNodeId(addColor, out string colorNodeId) ||
                !TryExtractAddedNodeId(addSplit, out string splitNodeId) ||
                !TryExtractAddedNodeId(addVector1, out string vector1NodeId))
            {
                Debug.LogError(
                    "[ShaderGraphMcp] Could not extract node ids from add_node responses for the Color -> Split -> Vector1 connect smoke test."
                );
                return;
            }

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    colorNodeId,
                    "Out",
                    splitNodeId,
                    "In"
                )
            );

            LogResponse(
                "connect_ports",
                assetPath,
                ShaderGraphAssetTool.HandleConnectPorts(
                    assetPath,
                    splitNodeId,
                    "R",
                    vector1NodeId,
                    "X"
                )
            );

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        private static string ResolveTargetAssetPath()
        {
            string selectedAssetPath = Selection.activeObject == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(Selection.activeObject);

            if (!string.IsNullOrWhiteSpace(selectedAssetPath) &&
                selectedAssetPath.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase))
            {
                return selectedAssetPath;
            }

            return DefaultAssetPath;
        }

        private static string ResolveTargetDirectory()
        {
            string selectedAssetPath = Selection.activeObject == null
                ? string.Empty
                : AssetDatabase.GetAssetPath(Selection.activeObject);

            if (string.IsNullOrWhiteSpace(selectedAssetPath))
            {
                return "Assets/ShaderGraphs";
            }

            if (AssetDatabase.IsValidFolder(selectedAssetPath))
            {
                return selectedAssetPath;
            }

            if (selectedAssetPath.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase))
            {
                string directory = Path.GetDirectoryName(selectedAssetPath)?.Replace('\\', '/');
                return string.IsNullOrWhiteSpace(directory) ? "Assets/ShaderGraphs" : directory;
            }

            string fallbackDirectory = Path.GetDirectoryName(selectedAssetPath)?.Replace('\\', '/');
            return string.IsNullOrWhiteSpace(fallbackDirectory) ? "Assets/ShaderGraphs" : fallbackDirectory;
        }

        private static IReadOnlyList<string> SelectPreferredSupportedNodeTypes(int maxCount)
        {
            var supported = new HashSet<string>(
                ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames(),
                StringComparer.OrdinalIgnoreCase);

            return PreferredCommonNodeTypes
                .Where(nodeType => supported.Contains(nodeType))
                .Take(Math.Max(0, maxCount))
                .ToArray();
        }

        private static IReadOnlyList<string> SelectPreferredSupportedArithmeticNodeTypes()
        {
            var supported = new HashSet<string>(
                ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames(),
                StringComparer.OrdinalIgnoreCase);

            return PreferredArithmeticConnectNodeTypes
                .Where(nodeType => supported.Contains(nodeType))
                .ToArray();
        }

        private static bool IsArithmeticNodeSupported(string nodeType)
        {
            return SelectPreferredSupportedArithmeticNodeTypes()
                .Any(candidate => string.Equals(candidate, nodeType, StringComparison.OrdinalIgnoreCase));
        }

        private static string[] GetArithmeticInputPorts(string nodeType)
        {
            return nodeType switch
            {
                "Add" => new[] { "A", "B" },
                "Subtract" => new[] { "A", "B" },
                "Multiply" => new[] { "A", "B" },
                "Divide" => new[] { "A", "B" },
                "Power" => new[] { "A", "B" },
                "Minimum" => new[] { "A", "B" },
                "Maximum" => new[] { "A", "B" },
                "Modulo" => new[] { "A", "B" },
                "Lerp" => new[] { "A", "B", "T" },
                "Smoothstep" => new[] { "Edge1", "Edge2", "In" },
                "Clamp" => new[] { "In", "Min", "Max" },
                "Step" => new[] { "Edge", "In" },
                "Absolute" => new[] { "In" },
                "Floor" => new[] { "In" },
                "Ceiling" => new[] { "In" },
                "Round" => new[] { "In" },
                "Sign" => new[] { "In" },
                "Sine" => new[] { "In" },
                "Cosine" => new[] { "In" },
                "Tangent" => new[] { "In" },
                "Negate" => new[] { "In" },
                "Reciprocal" => new[] { "In" },
                "SquareRoot" => new[] { "In" },
                "Fraction" => new[] { "In" },
                "Truncate" => new[] { "In" },
                "Saturate" => new[] { "In" },
                "OneMinus" => new[] { "In" },
                _ => Array.Empty<string>(),
            };
        }

        private static List<string> ExtractVector1NodeIds(ShaderGraphResponse response)
        {
            var result = new List<string>();
            if (response?.Data == null || !response.Data.TryGetValue("nodes", out object rawNodes))
            {
                return result;
            }

            if (rawNodes is not IEnumerable nodes)
            {
                return result;
            }

            foreach (object item in nodes)
            {
                string nodeDescription = item?.ToString() ?? string.Empty;
                if (!nodeDescription.Contains("Vector1Node", StringComparison.Ordinal))
                {
                    continue;
                }

                int start = nodeDescription.LastIndexOf('(');
                int end = nodeDescription.LastIndexOf(')');
                if (start < 0 || end <= start + 1)
                {
                    continue;
                }

                string objectId = nodeDescription.Substring(start + 1, end - start - 1).Trim();
                if (!string.IsNullOrWhiteSpace(objectId))
                {
                    result.Add(objectId);
                }
            }

            return result;
        }

        internal static bool TryExtractAddedNodeId(ShaderGraphResponse response, out string objectId)
        {
            objectId = string.Empty;

            if (response?.Data == null || !response.Data.TryGetValue("addedNode", out object rawAddedNode))
            {
                return false;
            }

            if (rawAddedNode is IReadOnlyDictionary<string, object> readOnlyAddedNode &&
                readOnlyAddedNode.TryGetValue("objectId", out object readOnlyObjectId))
            {
                objectId = readOnlyObjectId?.ToString()?.Trim() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(objectId);
            }

            if (rawAddedNode is IDictionary dictionary && dictionary.Contains("objectId"))
            {
                objectId = dictionary["objectId"]?.ToString()?.Trim() ?? string.Empty;
                return !string.IsNullOrWhiteSpace(objectId);
            }

            return false;
        }

        private static void AddPropertyAndReadSummary(string propertyType, string defaultValue, string propertyLabel)
        {
            string assetPath = ResolveTargetAssetPath();
            string propertyName = $"{propertyLabel} {DateTime.Now:HHmmss}";

            ShaderGraphResponse addResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                propertyName,
                propertyType,
                defaultValue
            );
            LogResponse("add_property", assetPath, addResponse);

            if (!addResponse.Success)
            {
                return;
            }

            LogResponse(
                "read_graph_summary",
                assetPath,
                ShaderGraphAssetTool.HandleReadGraphSummary(assetPath)
            );
        }

        internal static ShaderGraphResponse RunBlankGraphHappyPath(string targetDirectory, string graphName, bool selectCreatedAsset)
        {
            string normalizedDirectory = string.IsNullOrWhiteSpace(targetDirectory)
                ? "Assets/ShaderGraphs"
                : targetDirectory.TrimEnd('/');
            string assetPath = $"{normalizedDirectory}/{graphName}.shadergraph";

            ShaderGraphResponse createResponse = ShaderGraphAssetTool.HandleCreateGraph(
                graphName,
                normalizedDirectory,
                "blank"
            );
            LogResponse("create_graph", assetPath, createResponse);
            if (!createResponse.Success)
            {
                return createResponse;
            }

            if (selectCreatedAsset)
            {
                UnityEngine.Object createdAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (createdAsset != null)
                {
                    Selection.activeObject = createdAsset;
                    EditorGUIUtility.PingObject(createdAsset);
                }
            }

            ShaderGraphResponse addPropertyResponse = ShaderGraphAssetTool.HandleAddProperty(
                assetPath,
                $"Exposure {DateTime.Now:HHmmss}",
                "Float/Vector1",
                "0");
            LogResponse("add_property", assetPath, addPropertyResponse);
            if (!addPropertyResponse.Success)
            {
                return addPropertyResponse;
            }

            ShaderGraphResponse addSourceNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Happy Path Source {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSourceNodeResponse);
            if (!TryExtractAddedNodeId(addSourceNodeResponse, out string sourceNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Could not extract the source node id for the blank graph happy path."
                );
            }

            ShaderGraphResponse addSinkNodeResponse = ShaderGraphAssetTool.HandleAddNode(
                assetPath,
                "Vector1",
                $"Happy Path Sink {DateTime.Now:HHmmss}");
            LogResponse("add_node", assetPath, addSinkNodeResponse);
            if (!TryExtractAddedNodeId(addSinkNodeResponse, out string sinkNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Could not extract the sink node id for the blank graph happy path."
                );
            }

            ShaderGraphResponse connectResponse = ShaderGraphAssetTool.HandleConnectPorts(
                assetPath,
                sourceNodeId,
                "Out",
                sinkNodeId,
                "X");
            LogResponse("connect_ports", assetPath, connectResponse);
            if (!connectResponse.Success)
            {
                return connectResponse;
            }

            ShaderGraphResponse saveResponse = ShaderGraphAssetTool.HandleSaveGraph(assetPath);
            LogResponse("save_graph", assetPath, saveResponse);
            if (!saveResponse.Success)
            {
                return saveResponse;
            }

            ShaderGraphResponse summaryResponse = ShaderGraphAssetTool.HandleReadGraphSummary(assetPath);
            LogResponse("read_graph_summary", assetPath, summaryResponse);
            return summaryResponse;
        }

        private static void LogResponse(string action, string assetPath, ShaderGraphResponse response)
        {
            if (response == null)
            {
                Debug.LogError($"[ShaderGraphMcp] {action} returned a null response for '{assetPath}'.");
                return;
            }

            var builder = new StringBuilder();
            builder.AppendLine($"[ShaderGraphMcp] action={action}");
            builder.AppendLine($"success={response.Success}");
            builder.AppendLine($"assetPath={assetPath}");
            builder.AppendLine($"message={response.Message}");

            if (response.Data != null && response.Data.Count > 0)
            {
                builder.AppendLine("data:");
                AppendDictionary(builder, response.Data, "  ");
            }

            if (response.Success)
            {
                Debug.Log(builder.ToString());
            }
            else
            {
                Debug.LogError(builder.ToString());
            }
        }

        private static void AppendDictionary(
            StringBuilder builder,
            IReadOnlyDictionary<string, object> dictionary,
            string indent)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (pair.Value is IReadOnlyDictionary<string, object> nestedReadOnly)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    AppendDictionary(builder, nestedReadOnly, indent + "  ");
                    continue;
                }

                if (pair.Value is IDictionary nestedDictionary)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    AppendDictionary(builder, ToReadOnlyDictionary(nestedDictionary), indent + "  ");
                    continue;
                }

                if (pair.Value is IEnumerable enumerable && pair.Value is not string)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    AppendEnumerable(builder, enumerable, indent + "  ");
                    continue;
                }

                builder.AppendLine($"{indent}{pair.Key}: {pair.Value ?? "<null>"}");
            }
        }

        private static void AppendEnumerable(StringBuilder builder, IEnumerable values, string indent)
        {
            foreach (object value in values)
            {
                if (value is IReadOnlyDictionary<string, object> nestedReadOnly)
                {
                    builder.AppendLine($"{indent}-");
                    AppendDictionary(builder, nestedReadOnly, indent + "  ");
                    continue;
                }

                if (value is IDictionary nestedDictionary)
                {
                    builder.AppendLine($"{indent}-");
                    AppendDictionary(builder, ToReadOnlyDictionary(nestedDictionary), indent + "  ");
                    continue;
                }

                builder.AppendLine($"{indent}- {value ?? "<null>"}");
            }
        }

        private static IReadOnlyDictionary<string, object> ToReadOnlyDictionary(IDictionary dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach (DictionaryEntry entry in dictionary)
            {
                string key = entry.Key?.ToString() ?? string.Empty;
                result[key] = entry.Value;
            }

            return result;
        }
    }
}
