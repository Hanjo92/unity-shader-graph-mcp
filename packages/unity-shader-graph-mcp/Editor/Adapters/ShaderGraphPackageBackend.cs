using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Adapters
{
    internal sealed class ShaderGraphPackageBackend : IShaderGraphBackend
    {
        private readonly ShaderGraphCompatibilitySnapshot compatibility;

        public ShaderGraphPackageBackend(ShaderGraphCompatibilitySnapshot compatibility)
        {
            this.compatibility = compatibility ?? ShaderGraphPackageCompatibility.Capture();
        }

        public ShaderGraphExecutionKind ExecutionKind => ShaderGraphExecutionKind.PackageBacked;

        public ShaderGraphResponse CreateGraph(CreateGraphRequest request)
        {
            return ShaderGraphPackageGraphInspector.CreateGraph(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request)
        {
            return ShaderGraphPackageGraphInspector.ReadGraphSummary(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse FindNode(FindNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.FindNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse FindProperty(FindPropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.FindProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request)
        {
            return ShaderGraphPackageGraphInspector.ListSupportedNodes(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ListSupportedProperties(ListSupportedPropertiesRequest request)
        {
            return ShaderGraphPackageGraphInspector.ListSupportedProperties(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ListSupportedConnections(ListSupportedConnectionsRequest request)
        {
            return ShaderGraphPackageGraphInspector.ListSupportedConnections(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.UpdateProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse RenameProperty(RenamePropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.RenameProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse DuplicateProperty(DuplicatePropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.DuplicateProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse RenameNode(RenameNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.RenameNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse DuplicateNode(DuplicateNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.DuplicateNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse MoveNode(MoveNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.MoveNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse DeleteNode(DeleteNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.DeleteNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse RemoveProperty(RemovePropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.RemoveProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse AddProperty(AddPropertyRequest request)
        {
            return ShaderGraphPackageGraphInspector.AddProperty(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse AddNode(AddNodeRequest request)
        {
            return ShaderGraphPackageGraphInspector.AddNode(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ConnectPorts(ConnectPortsRequest request)
        {
            return ShaderGraphPackageGraphInspector.ConnectPorts(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse FindConnection(FindConnectionRequest request)
        {
            return ShaderGraphPackageGraphInspector.FindConnection(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request)
        {
            return ShaderGraphPackageGraphInspector.RemoveConnection(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse ReconnectConnection(ReconnectConnectionRequest request)
        {
            return ShaderGraphPackageGraphInspector.ReconnectConnection(
                request,
                compatibility,
                ExecutionKind
            );
        }

        public ShaderGraphResponse SaveGraph(SaveGraphRequest request)
        {
            return ShaderGraphPackageGraphInspector.SaveGraph(
                request,
                compatibility,
                ExecutionKind
            );
        }

        private ShaderGraphResponse Fail(string action, string assetPath, string operationTitle, string note = null)
        {
            var data = new Dictionary<string, object>
            {
                ["action"] = action,
                ["assetPath"] = assetPath,
                ["executionBackendKind"] = ExecutionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["requiredNextSpike"] = new[]
                {
                    "Replace the package-backed placeholder with real Shader Graph API calls.",
                    "Use the compatibility snapshot to confirm the exact graph, node, property, and slot types in the target Unity version.",
                },
                ["notes"] = new[]
                {
                    note ?? $"{operationTitle} is still a placeholder.",
                },
            };

            return ShaderGraphResponse.Fail(
                $"Shader Graph package backend placeholder cannot yet execute '{action}' for '{assetPath}'.",
                data
            );
        }
    }

    internal static class ShaderGraphPackageGraphInspector
    {
        private const string GraphDataTypeName = "UnityEditor.ShaderGraph.GraphData";
        private const string CategoryDataTypeName = "UnityEditor.ShaderGraph.CategoryData";
        private const string FileUtilitiesTypeName = "UnityEditor.ShaderGraph.FileUtilities";
        private const string MultiJsonTypeName = "UnityEditor.ShaderGraph.Serialization.MultiJson";
        private const string MessageManagerTypeName = "UnityEditor.Graphing.Util.MessageManager";
        private const string DrawStateTypeName = "UnityEditor.Graphing.DrawState";
        private const string PackageSchema = "unity-shader-graph-mcp/package-backed-v1";
        private const string DefaultGraphPathLabel = "Shader Graphs";
        private const float DuplicateNodeOffsetX = 220f;
        private const float DuplicateNodeOffsetY = 60f;

        private static readonly BindingFlags InstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly BindingFlags StaticFlags =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

        private sealed class ShaderGraphNodeDescriptor
        {
            public ShaderGraphNodeDescriptor(
                string canonicalName,
                Type nodeClassType,
                IReadOnlyList<string> aliases,
                string catalogStatus,
                string catalogNote)
            {
                CanonicalName = canonicalName ?? string.Empty;
                NodeClassType = nodeClassType;
                FullTypeName = nodeClassType?.FullName ?? nodeClassType?.Name ?? string.Empty;
                Label = string.IsNullOrWhiteSpace(FullTypeName)
                    ? CanonicalName
                    : $"{CanonicalName} ({FullTypeName})";
                Aliases = aliases ?? Array.Empty<string>();
                CatalogStatus = string.IsNullOrWhiteSpace(catalogStatus)
                    ? "discoverable"
                    : catalogStatus;
                CatalogNote = catalogNote ?? string.Empty;
            }

            public string CanonicalName { get; }

            public Type NodeClassType { get; }

            public string FullTypeName { get; }

            public string Label { get; }

            public IReadOnlyList<string> Aliases { get; }

            public string CatalogStatus { get; }

            public string CatalogNote { get; }

            public bool IsGraphAddable =>
                string.Equals(CatalogStatus, "graph-addable", StringComparison.Ordinal);
        }

        private static readonly Lazy<IReadOnlyList<ShaderGraphNodeDescriptor>> DiscoveredNodeCatalog =
            new Lazy<IReadOnlyList<ShaderGraphNodeDescriptor>>(DiscoverSupportedNodeCatalog);

        private static readonly Lazy<IReadOnlyList<ShaderGraphNodeDescriptor>> SupportedNodeCatalog =
            new Lazy<IReadOnlyList<ShaderGraphNodeDescriptor>>(FilterSupportedNodeCatalog);

        public static ShaderGraphResponse CreateGraph(
            CreateGraphRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Create graph request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset already exists at '{assetPath}'.",
                    BuildUnsupportedCreateGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        Array.Empty<string>(),
                        request.Template
                    )
                );
            }

            string requestedTemplate = string.IsNullOrWhiteSpace(request.Template)
                ? "blank"
                : request.Template.Trim();
            if (!string.Equals(requestedTemplate, "blank", StringComparison.OrdinalIgnoreCase))
            {
                return ShaderGraphResponse.Fail(
                    $"Unsupported create_graph template '{requestedTemplate}'. Only 'blank' is currently package-backed.",
                    BuildUnsupportedCreateGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        Array.Empty<string>(),
                        requestedTemplate
                    )
                );
            }

            string parentDirectory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrWhiteSpace(parentDirectory))
            {
                Directory.CreateDirectory(parentDirectory);
            }

            var creationNotes = new List<string>();
            if (!TryCreateBlankGraphData(assetPath, out object graphData, creationNotes, out string creationFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to create blank Shader Graph data for '{assetPath}': {creationFailure}",
                    BuildUnsupportedCreateGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        creationNotes,
                        requestedTemplate
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to write blank Shader Graph to '{assetPath}': {writeFailure}",
                    BuildUnsupportedCreateGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        creationNotes,
                        requestedTemplate
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate);

            object loadedGraphData = graphData;
            if (TryLoadGraphData(assetPath, absolutePath, out object reloadedGraphData, creationNotes, out string reloadFailure))
            {
                loadedGraphData = reloadedGraphData;

                if (TryInvokeInstanceMethod(loadedGraphData, "OnEnable", out string onEnableFailure))
                {
                    creationNotes.Add("GraphData.OnEnable() invoked successfully after create_graph.");
                }
                else
                {
                    creationNotes.Add($"GraphData.OnEnable() could not be invoked after create_graph: {onEnableFailure}");
                }

                if (TryInvokeInstanceMethod(loadedGraphData, "ValidateGraph", out string validateFailure))
                {
                    creationNotes.Add("GraphData.ValidateGraph() invoked successfully after create_graph.");
                }
                else
                {
                    creationNotes.Add($"GraphData.ValidateGraph() could not be invoked after create_graph: {validateFailure}");
                }
            }
            else
            {
                creationNotes.Add($"Reload after create_graph fell back to the in-memory graph: {reloadFailure}");
            }

            var snapshot = BuildSnapshot(
                loadedGraphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                creationNotes,
                "create_graph"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["template"] = "blank",
                ["supportedCreateTemplates"] = SupportedCreateTemplates.ToArray(),
                ["createdGraph"] = new Dictionary<string, object>
                {
                    ["name"] = request.Name,
                    ["requestedTemplate"] = requestedTemplate,
                    ["resolvedTemplate"] = "blank",
                    ["graphPathLabel"] = DefaultGraphPathLabel,
                },
            };

            return ShaderGraphResponse.Ok(
                $"Created blank package-backed Shader Graph at '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse ReadGraphSummary(
            ReadGraphSummaryRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Read graph summary request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.ValidateGraph() could not be invoked: {validateFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "read_graph_summary"
            );
            return ShaderGraphResponse.Ok(
                $"Loaded package-backed Shader Graph summary from '{assetPath}'.",
                new Dictionary<string, object>(snapshot.ToDictionary())
            );
        }

        public static ShaderGraphResponse FindNode(
            FindNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Find node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.ValidateGraph() could not be invoked: {validateFailure}");
            }

            string queryNodeId = string.IsNullOrWhiteSpace(request.NodeId) ? string.Empty : request.NodeId.Trim();
            string queryDisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? string.Empty : request.DisplayName.Trim();
            string queryNodeType = string.IsNullOrWhiteSpace(request.NodeType) ? string.Empty : request.NodeType.Trim();

            object[] matches = EnumerateMember(graphData, "nodes")
                .Where(node => NodeMatchesQuery(node, queryNodeId, queryDisplayName, queryNodeType))
                .ToArray();

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "find_node"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(queryNodeId, queryDisplayName, queryNodeType),
                ["matchCount"] = matches.Length,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(queryNodeId, queryDisplayName, queryNodeType),
            };

            if (matches.Length == 1)
            {
                data["foundNode"] = BuildNodeLookupData(matches[0]);
                return ShaderGraphResponse.Ok(
                    $"Found Shader Graph node in '{assetPath}'.",
                    data
                );
            }

            data["candidateNodes"] = matches.Select(BuildNodeLookupData).Cast<object>().ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a graph node matching the provided query in '{assetPath}'.",
                    data
                );
            }

            return ShaderGraphResponse.Fail(
                $"Node query matched multiple graph nodes in '{assetPath}'. Narrow the query with nodeId/objectId, displayName, or nodeType.",
                data
            );
        }

        public static ShaderGraphResponse FindProperty(
            FindPropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Find property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.ValidateGraph() could not be invoked: {validateFailure}");
            }

            string queryPropertyName = string.IsNullOrWhiteSpace(request.PropertyName) ? string.Empty : request.PropertyName.Trim();
            string queryDisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? string.Empty : request.DisplayName.Trim();
            string queryReferenceName = string.IsNullOrWhiteSpace(request.ReferenceName) ? string.Empty : request.ReferenceName.Trim();
            string queryPropertyType = string.IsNullOrWhiteSpace(request.PropertyType) ? string.Empty : request.PropertyType.Trim();

            object[] matches = EnumerateMember(graphData, "properties")
                .Where(property => PropertyMatchesQuery(property, queryPropertyName, queryDisplayName, queryReferenceName, queryPropertyType))
                .ToArray();

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "find_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildPropertyQuery(queryPropertyName, queryDisplayName, queryReferenceName, queryPropertyType),
                ["matchCount"] = matches.Length,
                ["matchStrategy"] = BuildFindPropertyMatchStrategy(queryPropertyName, queryDisplayName, queryReferenceName, queryPropertyType),
            };

            if (matches.Length == 1)
            {
                object shaderInput = matches[0];
                var foundProperty = BuildPropertyLookupData(shaderInput);
                if (TryResolvePropertyTypeFromInstance(shaderInput, out string canonicalPropertyType, out Type shaderInputType, out _))
                {
                    foundProperty["resolvedPropertyType"] = canonicalPropertyType;
                    foundProperty["resolvedShaderInputType"] = shaderInputType.FullName ?? shaderInputType.Name;
                }

                data["foundProperty"] = foundProperty;
                return ShaderGraphResponse.Ok(
                    $"Found Shader Graph property in '{assetPath}'.",
                    data
                );
            }

            data["candidateProperties"] = matches.Select(BuildPropertyLookupData).Cast<object>().ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph property matching the provided query in '{assetPath}'.",
                    data
                );
            }

            return ShaderGraphResponse.Fail(
                $"Property query matched multiple Shader Graph properties in '{assetPath}'. Narrow the query with propertyName, displayName, referenceName, or propertyType.",
                data
            );
        }

        public static ShaderGraphResponse ListSupportedNodes(
            ListSupportedNodesRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            string[] supportedNodeTypes = GetSupportedNodeTypeLabels();
            string[] supportedNodeCanonicalNames = GetSupportedNodeCanonicalNames().ToArray();
            string[] discoveredNodeTypes = GetDiscoveredNodeTypeLabels();

            var data = new Dictionary<string, object>
            {
                ["operation"] = "list_supported_nodes",
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedNodeTypes"] = supportedNodeTypes,
                ["supportedNodeCanonicalNames"] = supportedNodeCanonicalNames,
                ["supportedNodeCount"] = GetGraphAddableNodeCatalog().Count,
                ["discoveredNodeTypes"] = discoveredNodeTypes,
                ["discoveredNodeCount"] = GetDiscoveredNodeCatalog().Count,
                ["nodeCatalogSemantics"] = "supported=graph-addable",
                ["notes"] = new[]
                {
                    "Node catalog query served without requiring a graph asset path.",
                    "supportedNodeTypes tracks the current graph-addable subset; discoveredNodeTypes remains broader for diagnostics.",
                },
            };

            return ShaderGraphResponse.Ok(
                "Loaded supported Shader Graph node catalog.",
                data
            );
        }

        public static ShaderGraphResponse ListSupportedProperties(
            ListSupportedPropertiesRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            string[] supportedPropertyTypes = GetSupportedPropertyTypes();

            var data = new Dictionary<string, object>
            {
                ["operation"] = "list_supported_properties",
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedPropertyTypes"] = supportedPropertyTypes,
                ["supportedPropertyCount"] = supportedPropertyTypes.Length,
                ["notes"] = new[]
                {
                    "Property catalog query served without requiring a graph asset path.",
                    "supportedPropertyTypes reflects the currently implemented package-backed property operations.",
                },
            };

            return ShaderGraphResponse.Ok(
                "Loaded supported Shader Graph property catalog.",
                data
            );
        }

        public static ShaderGraphResponse ListSupportedConnections(
            ListSupportedConnectionsRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            string[] supportedConnectionRules = GetSupportedConnectionRules();

            var data = new Dictionary<string, object>
            {
                ["operation"] = "list_supported_connections",
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedConnectionRules"] = supportedConnectionRules,
                ["supportedConnectionRuleCount"] = supportedConnectionRules.Length,
                ["connectionCatalogSemantics"] = "supportedConnectionRules=enforced-runtime-rules",
                ["notes"] = new[]
                {
                    "Connection rule catalog query served without requiring a graph asset path.",
                    "supportedConnectionRules reflects the currently enforced package-backed connection matrix.",
                },
            };

            return ShaderGraphResponse.Ok(
                "Loaded supported Shader Graph connection catalog.",
                data
            );
        }

        public static ShaderGraphResponse AddProperty(
            AddPropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Add property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (string.IsNullOrWhiteSpace(request.PropertyName))
            {
                return ShaderGraphResponse.Fail(
                    "Property name is required.",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        request.PropertyType,
                        "update_property"
                    )
                );
            }

            string propertyTypeInput = request.PropertyType?.Trim();
            if (!TryResolveSupportedPropertyType(propertyTypeInput, out string canonicalPropertyType, out Type shaderInputType, out string propertyTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    propertyTypeFailure,
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        propertyTypeInput
                    )
                );
            }

            if (!TryCreateShaderInput(
                    shaderInputType,
                    request.PropertyName.Trim(),
                    request.DefaultValue,
                    out object shaderInput,
                    out object parsedDefaultValue,
                    out string parseNote,
                    out string creationFailure))
            {
                return ShaderGraphResponse.Fail(
                    creationFailure,
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        propertyTypeInput
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(parseNote))
            {
                loadNotes.Add(parseNote);
            }

            if (!TryInvokeGraphAddInput(graphData, shaderInput, out string addInputFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to add Shader Graph property '{request.PropertyName.Trim()}' to '{assetPath}': {addInputFailure}",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        propertyTypeInput
                    )
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after adding property '{request.PropertyName.Trim()}': {validateFailure}",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        propertyTypeInput
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after adding property '{request.PropertyName.Trim()}': {writeFailure}",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        propertyTypeInput
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "add_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["supportedPropertyTypes"] = SupportedPropertyTypes.ToArray(),
                ["addedProperty"] = new Dictionary<string, object>
                {
                    ["displayName"] = GetStringProperty(shaderInput, "displayName"),
                    ["referenceName"] = GetStringProperty(shaderInput, "referenceName"),
                    ["requestedPropertyType"] = propertyTypeInput,
                    ["resolvedPropertyType"] = canonicalPropertyType,
                    ["resolvedShaderInputType"] = shaderInputType.FullName ?? shaderInputType.Name,
                    ["defaultValue"] = parsedDefaultValue?.ToString() ?? string.Empty,
                },
            };

            return ShaderGraphResponse.Ok(
                $"Added {canonicalPropertyType} property '{request.PropertyName.Trim()}' to '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse UpdateProperty(
            UpdatePropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Update property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (string.IsNullOrWhiteSpace(request.PropertyName))
            {
                return ShaderGraphResponse.Fail(
                    "Property name is required.",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        request.PropertyType,
                        "update_property"
                    )
                );
            }

            string propertyName = request.PropertyName.Trim();
            object[] matches = EnumerateMember(graphData, "properties")
                .Where(property => PropertyMatchesName(property, propertyName))
                .ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph property named '{propertyName}' in '{assetPath}'.",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        request.PropertyType
                    )
                );
            }

            if (matches.Length > 1)
            {
                var duplicateData = BuildUnsupportedPropertyData(
                    assetPath,
                    compatibility,
                    executionKind,
                    loadNotes,
                    request.PropertyType,
                    "update_property"
                );
                duplicateData["candidateProperties"] = matches.Select(BuildPropertyLookupData).Cast<object>().ToArray();
                duplicateData["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = propertyName,
                };
                return ShaderGraphResponse.Fail(
                    $"Property query for '{propertyName}' matched multiple Shader Graph properties in '{assetPath}'.",
                    duplicateData
                );
            }

            object shaderInput = matches[0];
            if (!TryResolvePropertyTypeFromInstance(shaderInput, out string canonicalPropertyType, out Type shaderInputType, out string propertyTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    propertyTypeFailure,
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        request.PropertyType,
                        "update_property"
                    )
                );
            }

            string requestedPropertyType = request.PropertyType?.Trim();
            if (!string.IsNullOrWhiteSpace(requestedPropertyType) &&
                !string.Equals(canonicalPropertyType, NormalizeRequestedPropertyType(requestedPropertyType), StringComparison.Ordinal))
            {
                return ShaderGraphResponse.Fail(
                    $"Property '{propertyName}' exists, but its type is '{canonicalPropertyType}'. Requested type '{requestedPropertyType}' does not match.",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        requestedPropertyType,
                        "update_property"
                    )
                );
            }

            if (!TryAssignShaderInputDefaultValue(
                    shaderInput,
                    shaderInputType,
                    request.DefaultValue,
                    out object parsedDefaultValue,
                    out string parseNote,
                    out string updateFailure))
            {
                return ShaderGraphResponse.Fail(
                    updateFailure,
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        canonicalPropertyType,
                        "update_property"
                    )
                );
            }

            if (!string.IsNullOrWhiteSpace(parseNote))
            {
                loadNotes.Add(parseNote);
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after updating property '{propertyName}': {validateFailure}",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        canonicalPropertyType,
                        "update_property"
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after updating property '{propertyName}': {writeFailure}",
                    BuildUnsupportedPropertyData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        canonicalPropertyType,
                        "update_property"
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "update_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["supportedPropertyTypes"] = SupportedPropertyTypes.ToArray(),
                ["updatedProperty"] = new Dictionary<string, object>
                {
                    ["displayName"] = GetStringProperty(shaderInput, "displayName", "name"),
                    ["referenceName"] = GetStringProperty(shaderInput, "referenceName"),
                    ["resolvedPropertyType"] = canonicalPropertyType,
                    ["resolvedShaderInputType"] = shaderInputType.FullName ?? shaderInputType.Name,
                    ["defaultValue"] = parsedDefaultValue?.ToString() ?? string.Empty,
                },
            };

            return ShaderGraphResponse.Ok(
                $"Updated {canonicalPropertyType} property '{propertyName}' in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse RenameProperty(
            RenamePropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Rename property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "rename_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = request.PropertyName?.Trim() ?? string.Empty,
                },
            };

            if (string.IsNullOrWhiteSpace(request.PropertyName))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Property name is required.", data);
            }

            string displayName = request.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Display name is required.", data);
            }

            string propertyName = request.PropertyName.Trim();
            object[] matches = EnumerateMember(graphData, "properties")
                .Where(property => PropertyMatchesName(property, propertyName))
                .ToArray();

            data["matchCount"] = matches.Length;

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph property named '{propertyName}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateProperties"] = matches.Select(BuildPropertyLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Property query for '{propertyName}' matched multiple Shader Graph properties in '{assetPath}'.",
                    data
                );
            }

            object shaderInput = matches[0];
            if (!TryResolvePropertyTypeFromInstance(shaderInput, out string canonicalPropertyType, out Type shaderInputType, out string propertyTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    propertyTypeFailure,
                    data
                );
            }

            string previousDisplayName = GetStringProperty(shaderInput, "displayName", "name");
            string previousReferenceName = GetStringProperty(shaderInput, "referenceName");

            if (TryInvokeInstanceMethod(
                    shaderInput,
                    "SetDisplayNameAndSanitizeForGraph",
                    new object[] { graphData, displayName },
                    out string renameDisplayFailure))
            {
                loadNotes.Add($"Property display name set to '{displayName}' via SetDisplayNameAndSanitizeForGraph().");
            }
            else
            {
                SetMemberValue(shaderInput, "displayName", displayName);
                SetMemberValue(shaderInput, "name", displayName);
                loadNotes.Add(
                    $"Property display name set to '{displayName}' via raw member update because SetDisplayNameAndSanitizeForGraph() was unavailable: {renameDisplayFailure}");
            }

            string referenceName = request.ReferenceName?.Trim();
            if (!string.IsNullOrWhiteSpace(referenceName))
            {
                if (TryInvokeInstanceMethod(
                        shaderInput,
                        "SetReferenceNameAndSanitizeForGraph",
                        new object[] { graphData, referenceName },
                        out string renameReferenceFailure))
                {
                    loadNotes.Add($"Property reference name set to '{referenceName}' via SetReferenceNameAndSanitizeForGraph().");
                }
                else
                {
                    SetMemberValue(shaderInput, "overrideReferenceName", referenceName);
                    loadNotes.Add(
                        $"Property reference name set to '{referenceName}' via raw overrideReferenceName update because SetReferenceNameAndSanitizeForGraph() was unavailable: {renameReferenceFailure}");
                }
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after renaming property '{propertyName}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after renaming property '{propertyName}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "rename_property"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = propertyName,
                },
                ["matchCount"] = 1,
                ["renamedProperty"] = BuildRenamedPropertyData(
                    shaderInput,
                    previousDisplayName,
                    previousReferenceName,
                    canonicalPropertyType,
                    shaderInputType),
            };

            return ShaderGraphResponse.Ok(
                $"Renamed Shader Graph property '{propertyName}' to '{GetStringProperty(shaderInput, "displayName", "name", "referenceName")}' in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse DuplicateProperty(
            DuplicatePropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Duplicate property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "duplicate_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = request.PropertyName?.Trim() ?? string.Empty,
                },
            };

            if (string.IsNullOrWhiteSpace(request.PropertyName))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Property name is required.", data);
            }

            string propertyName = request.PropertyName.Trim();
            object[] matches = EnumerateMember(graphData, "properties")
                .Where(property => PropertyMatchesName(property, propertyName))
                .ToArray();

            data["matchCount"] = matches.Length;

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph property named '{propertyName}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateProperties"] = matches.Select(BuildPropertyLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Property query for '{propertyName}' matched multiple Shader Graph properties in '{assetPath}'.",
                    data
                );
            }

            object sourceProperty = matches[0];
            if (!TryResolvePropertyTypeFromInstance(sourceProperty, out string canonicalPropertyType, out Type shaderInputType, out string propertyTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    propertyTypeFailure,
                    data
                );
            }

            if (!TryInvokeGraphAddCopyOfShaderInput(graphData, sourceProperty, out object duplicatedProperty, out string duplicateFailure))
            {
                data["duplicatedFrom"] = BuildPropertyLookupData(sourceProperty);
                return ShaderGraphResponse.Fail(
                    $"Unable to duplicate Shader Graph property '{propertyName}' in '{assetPath}': {duplicateFailure}",
                    data
                );
            }

            string sourceDisplayName = GetStringProperty(sourceProperty, "displayName", "name");
            string duplicatedDisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? BuildDuplicateDisplayName(sourceDisplayName, canonicalPropertyType)
                : request.DisplayName.Trim();

            if (!string.IsNullOrWhiteSpace(duplicatedDisplayName))
            {
                if (TryInvokeInstanceMethod(
                        duplicatedProperty,
                        "SetDisplayNameAndSanitizeForGraph",
                        new object[] { graphData, duplicatedDisplayName },
                        out string renameDisplayFailure))
                {
                    loadNotes.Add($"Duplicated property display name set to '{duplicatedDisplayName}' via SetDisplayNameAndSanitizeForGraph().");
                }
                else
                {
                    SetMemberValue(duplicatedProperty, "displayName", duplicatedDisplayName);
                    SetMemberValue(duplicatedProperty, "name", duplicatedDisplayName);
                    loadNotes.Add(
                        $"Duplicated property display name set to '{duplicatedDisplayName}' via raw member update because SetDisplayNameAndSanitizeForGraph() was unavailable: {renameDisplayFailure}");
                }
            }

            string referenceName = request.ReferenceName?.Trim();
            if (!string.IsNullOrWhiteSpace(referenceName))
            {
                if (TryInvokeInstanceMethod(
                        duplicatedProperty,
                        "SetReferenceNameAndSanitizeForGraph",
                        new object[] { graphData, referenceName },
                        out string renameReferenceFailure))
                {
                    loadNotes.Add($"Duplicated property reference name set to '{referenceName}' via SetReferenceNameAndSanitizeForGraph().");
                }
                else
                {
                    SetMemberValue(duplicatedProperty, "overrideReferenceName", referenceName);
                    loadNotes.Add(
                        $"Duplicated property reference name set to '{referenceName}' via raw overrideReferenceName update because SetReferenceNameAndSanitizeForGraph() was unavailable: {renameReferenceFailure}");
                }
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after duplicating property '{propertyName}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after duplicating property '{propertyName}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "duplicate_property"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = propertyName,
                },
                ["matchCount"] = 1,
                ["duplicationStrategy"] = new[]
                {
                    "Creates a new Shader Graph property via GraphData.AddCopyOfShaderInput(source, -1).",
                    "Uses requested displayName or appends 'Copy' to the source displayName.",
                    "Uses requested referenceName when provided; otherwise Unity keeps the duplicated property's sanitized default or copied override.",
                },
                ["duplicatedFrom"] = BuildPropertyLookupData(sourceProperty),
                ["duplicatedProperty"] = BuildDuplicatedPropertyData(
                    duplicatedProperty,
                    sourceProperty,
                    canonicalPropertyType,
                    shaderInputType),
            };

            return ShaderGraphResponse.Ok(
                $"Duplicated Shader Graph property '{sourceDisplayName}' as '{GetStringProperty(duplicatedProperty, "displayName", "name", "referenceName")}' in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse RemoveProperty(
            RemovePropertyRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Remove property request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "remove_property"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = request.PropertyName?.Trim() ?? string.Empty,
                },
            };

            if (string.IsNullOrWhiteSpace(request.PropertyName))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Property name is required.", data);
            }

            string propertyName = request.PropertyName.Trim();
            object[] matches = EnumerateMember(graphData, "properties")
                .Where(property => PropertyMatchesName(property, propertyName))
                .ToArray();

            data["matchCount"] = matches.Length;

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph property named '{propertyName}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateProperties"] = matches.Select(BuildPropertyLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Property query for '{propertyName}' matched multiple Shader Graph properties in '{assetPath}'.",
                    data
                );
            }

            object shaderInput = matches[0];
            if (!TryResolvePropertyTypeFromInstance(shaderInput, out string canonicalPropertyType, out Type shaderInputType, out string propertyTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    propertyTypeFailure,
                    data
                );
            }

            Dictionary<string, object> deletedProperty = BuildPropertyLookupData(shaderInput);
            deletedProperty["resolvedShaderInputType"] = shaderInputType.FullName ?? shaderInputType.Name;

            if (!TryInvokeGraphRemoveProperty(graphData, shaderInput, out string removePropertyFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to remove Shader Graph property '{propertyName}' from '{assetPath}': {removePropertyFailure}",
                    data
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after removing property '{propertyName}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after removing property '{propertyName}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "remove_property"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["supportedPropertyTypes"] = SupportedPropertyTypes.ToArray(),
                ["query"] = new Dictionary<string, object>
                {
                    ["propertyName"] = propertyName,
                },
                ["matchCount"] = 1,
                ["deletedProperty"] = deletedProperty,
            };

            return ShaderGraphResponse.Ok(
                $"Removed {canonicalPropertyType} property '{propertyName}' from '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse RenameNode(
            RenameNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Rename node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            string nodeId = request.NodeId?.Trim();
            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "rename_node"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
            };

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Node id is required.", data);
            }

            string displayName = request.DisplayName?.Trim();
            if (string.IsNullOrWhiteSpace(displayName))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Display name is required.", data);
            }

            object[] matches = EnumerateMember(graphData, "nodes")
                .Where(node => NodeMatchesQuery(node, nodeId, null, null))
                .ToArray();

            data["matchCount"] = matches.Length;
            data["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null);

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a graph node with objectId '{nodeId}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateNodes"] = matches.Select(BuildNodeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Node query for '{nodeId}' matched multiple graph nodes in '{assetPath}'.",
                    data
                );
            }

            object node = matches[0];
            string previousDisplayName = GetStringProperty(node, "displayName", "name");

            SetMemberValue(node, "name", displayName);
            SetMemberValue(node, "displayName", displayName);
            loadNotes.Add($"Node display name set to '{displayName}'.");

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after renaming node '{nodeId}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after renaming node '{nodeId}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "rename_node"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
                ["matchCount"] = 1,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null),
                ["renamedNode"] = BuildRenamedNodeData(node, previousDisplayName),
            };

            return ShaderGraphResponse.Ok(
                $"Renamed Shader Graph node '{previousDisplayName}' to '{GetStringProperty(node, "displayName", "name")}' in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse DuplicateNode(
            DuplicateNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Duplicate node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            string nodeId = request.NodeId?.Trim();
            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "duplicate_node"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
            };

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Node id is required.", data);
            }

            object[] matches = EnumerateMember(graphData, "nodes")
                .Where(node => NodeMatchesQuery(node, nodeId, null, null))
                .ToArray();

            data["matchCount"] = matches.Length;
            data["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null);

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a graph node with objectId '{nodeId}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateNodes"] = matches.Select(BuildNodeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Node query for '{nodeId}' matched multiple graph nodes in '{assetPath}'.",
                    data
                );
            }

            object sourceNode = matches[0];
            Type sourceNodeType = sourceNode.GetType();
            string canonicalNodeType = BuildCanonicalNodeName(sourceNodeType);
            string sourceDisplayName = GetStringProperty(sourceNode, "displayName", "name");
            string duplicatedDisplayName = string.IsNullOrWhiteSpace(request.DisplayName)
                ? BuildDuplicateDisplayName(sourceDisplayName, canonicalNodeType)
                : request.DisplayName.Trim();

            if (!TryCreateShaderNode(
                    sourceNodeType,
                    duplicatedDisplayName,
                    out object duplicatedNode,
                    out string nodeCreationFailure))
            {
                data["duplicatedFrom"] = BuildNodeLookupData(sourceNode);
                return ShaderGraphResponse.Fail(
                    $"Unable to duplicate Shader Graph node '{nodeId}' in '{assetPath}': {nodeCreationFailure}",
                    data
                );
            }

            Rect? sourcePosition = TryGetNodePositionRect(sourceNode, out Rect sourcePositionRect)
                ? sourcePositionRect
                : (Rect?)null;
            if (!TryAssignDuplicatedNodeLayout(
                    graphData,
                    sourceNode,
                    duplicatedNode,
                    canonicalNodeType,
                    out Rect duplicatedPosition,
                    out string layoutFailure))
            {
                loadNotes.Add($"Duplicated node draw position could not be assigned: {layoutFailure}");
            }
            else
            {
                loadNotes.Add(
                    $"Assigned duplicated node draw position to ({duplicatedPosition.x:0}, {duplicatedPosition.y:0})."
                );
            }

            if (!TryInvokeGraphAddNode(graphData, duplicatedNode, out string addNodeFailure))
            {
                data["duplicatedFrom"] = BuildDuplicatedFromData(sourceNode, sourcePosition);
                return ShaderGraphResponse.Fail(
                    $"Unable to duplicate Shader Graph node '{nodeId}' in '{assetPath}': {addNodeFailure}",
                    data
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after duplicating node '{nodeId}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after duplicating node '{nodeId}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "duplicate_node"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
                ["matchCount"] = 1,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null),
                ["duplicationStrategy"] = new[]
                {
                    "Creates a new node instance of the same CLR type as the source node.",
                    "Uses requested displayName or appends 'Copy' to the source displayName.",
                    "Offsets the duplicated node position by (+220, +60) when the source position is available.",
                    "Does not duplicate connections or node-specific serialized settings in this first path.",
                },
                ["duplicatedFrom"] = BuildDuplicatedFromData(sourceNode, sourcePosition),
                ["duplicatedNode"] = BuildDuplicatedNodeData(duplicatedNode, sourceNode, duplicatedPosition, sourcePosition),
            };

            return ShaderGraphResponse.Ok(
                $"Duplicated Shader Graph node '{sourceDisplayName}' as '{GetStringProperty(duplicatedNode, "displayName", "name")}' in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse MoveNode(
            MoveNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Move node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (string.IsNullOrWhiteSpace(request.NodeId))
            {
                var missingNodeData = new Dictionary<string, object>(BuildSnapshot(
                    graphData,
                    assetPath,
                    absolutePath,
                    executionKind,
                    compatibility,
                    loadNotes,
                    "move_node"
                ).ToDictionary())
                {
                    ["query"] = BuildNodeQuery(string.Empty, null, null),
                    ["matchCount"] = 0,
                };
                return ShaderGraphResponse.Fail("Node id is required.", missingNodeData);
            }

            string nodeId = request.NodeId.Trim();
            object[] matches = EnumerateMember(graphData, "nodes")
                .Where(node => NodeMatchesQuery(node, nodeId, null, null))
                .ToArray();

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "move_node"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
                ["matchCount"] = matches.Length,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null),
            };

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a graph node with objectId '{nodeId}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateNodes"] = matches.Select(BuildNodeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Node query for '{nodeId}' matched multiple graph nodes in '{assetPath}'.",
                    data
                );
            }

            object node = matches[0];
            Rect previousPosition = default;
            bool hadPreviousPosition = TryGetNodePositionRect(node, out previousPosition);

            if (!TryAssignExactNodePosition(node, request.X, request.Y, out Rect movedPosition, out string moveFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to move Shader Graph node '{nodeId}' in '{assetPath}': {moveFailure}",
                    data
                );
            }

            loadNotes.Add(
                $"Assigned node draw position to ({movedPosition.x:0}, {movedPosition.y:0})."
            );

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after moving node '{nodeId}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after moving node '{nodeId}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "move_node"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
                ["matchCount"] = 1,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null),
                ["movedNode"] = BuildMovedNodeData(node, movedPosition, hadPreviousPosition ? previousPosition : (Rect?)null),
            };

            return ShaderGraphResponse.Ok(
                $"Moved Shader Graph node '{GetStringProperty(node, "displayName", "name")}' to ({movedPosition.x:0}, {movedPosition.y:0}) in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse DeleteNode(
            DeleteNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Delete node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            string nodeId = request.NodeId?.Trim();
            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "delete_node"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
            };

            if (string.IsNullOrWhiteSpace(nodeId))
            {
                data["matchCount"] = 0;
                return ShaderGraphResponse.Fail("Node id is required.", data);
            }

            object[] matches = EnumerateMember(graphData, "nodes")
                .Where(node => NodeMatchesQuery(node, nodeId, null, null))
                .ToArray();

            data["matchCount"] = matches.Length;
            data["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null);

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a graph node with objectId '{nodeId}' in '{assetPath}'.",
                    data
                );
            }

            if (matches.Length > 1)
            {
                data["candidateNodes"] = matches.Select(BuildNodeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Node query for '{nodeId}' matched multiple graph nodes in '{assetPath}'.",
                    data
                );
            }

            object node = matches[0];
            Dictionary<string, object> deletedNode = BuildNodeLookupData(node);

            if (!TryInvokeGraphRemoveNode(graphData, node, out string removeNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to delete Shader Graph node '{nodeId}' from '{assetPath}': {removeNodeFailure}",
                    data
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after deleting node '{nodeId}': {validateFailure}",
                    data
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after deleting node '{nodeId}': {writeFailure}",
                    data
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "delete_node"
            );

            data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = BuildNodeQuery(nodeId, null, null),
                ["matchCount"] = 1,
                ["matchStrategy"] = BuildFindNodeMatchStrategy(nodeId, null, null),
                ["deletedNode"] = deletedNode,
            };

            return ShaderGraphResponse.Ok(
                $"Deleted Shader Graph node '{GetStringProperty(node, "displayName", "name")}' from '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse AddNode(
            AddNodeRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Add node request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            string nodeTypeInput = request.NodeType?.Trim();
            if (!TryResolveSupportedNodeType(
                    nodeTypeInput,
                    out string canonicalNodeType,
                    out Type nodeType,
                    out string nodeTypeFailure))
            {
                return ShaderGraphResponse.Fail(
                    nodeTypeFailure,
                    BuildUnsupportedNodeData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        nodeTypeInput,
                        request.DisplayName
                    )
                );
            }

            if (!TryCreateShaderNode(
                    nodeType,
                    request.DisplayName,
                    out object shaderNode,
                    out string nodeCreationFailure))
            {
                return ShaderGraphResponse.Fail(
                    nodeCreationFailure,
                    BuildUnsupportedNodeData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        nodeTypeInput,
                        request.DisplayName
                    )
                );
            }

            string displayName = request.DisplayName?.Trim();
            if (!string.IsNullOrWhiteSpace(displayName))
            {
                SetMemberValue(shaderNode, "name", displayName);
                loadNotes.Add($"Node display name set to '{displayName}'.");
            }

            Rect assignedNodePosition = default;
            if (TryAssignVisibleNodeLayout(
                    graphData,
                    shaderNode,
                    canonicalNodeType,
                    out assignedNodePosition,
                    out string layoutFailure))
            {
                loadNotes.Add(
                    $"Assigned node draw position to ({assignedNodePosition.x:0}, {assignedNodePosition.y:0})."
                );
            }
            else
            {
                loadNotes.Add($"Node draw position could not be assigned: {layoutFailure}");
            }

            if (!TryInvokeGraphAddNode(graphData, shaderNode, out string addNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to add Shader Graph node '{nodeTypeInput}' to '{assetPath}': {addNodeFailure}",
                    BuildUnsupportedNodeData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        nodeTypeInput,
                        request.DisplayName
                    )
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after adding node '{nodeTypeInput}': {validateFailure}",
                    BuildUnsupportedNodeData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        nodeTypeInput,
                        request.DisplayName
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after adding node '{nodeTypeInput}': {writeFailure}",
                    BuildUnsupportedNodeData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes,
                        nodeTypeInput,
                        request.DisplayName
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "add_node"
            );

            string addedNodeDisplayName = GetStringProperty(shaderNode, "displayName", "name");
            string addedNodeObjectId = GetStringProperty(shaderNode, "objectId");
            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["supportedNodeTypes"] = GetSupportedNodeTypeLabels(),
                ["supportedNodeCount"] = GetGraphAddableNodeCatalog().Count,
                ["discoveredNodeTypes"] = GetDiscoveredNodeTypeLabels(),
                ["discoveredNodeCount"] = GetDiscoveredNodeCatalog().Count,
                ["nodeCatalogSemantics"] = "supported=graph-addable",
                ["addedNode"] = new Dictionary<string, object>
                {
                    ["requestedNodeType"] = nodeTypeInput,
                    ["resolvedNodeType"] = canonicalNodeType,
                    ["resolvedNodeClass"] = nodeType.FullName ?? nodeType.Name,
                    ["displayName"] = addedNodeDisplayName,
                    ["objectId"] = addedNodeObjectId,
                    ["position"] = new Dictionary<string, object>
                    {
                        ["x"] = assignedNodePosition.x,
                        ["y"] = assignedNodePosition.y,
                    },
                },
            };

            return ShaderGraphResponse.Ok(
                $"Added {canonicalNodeType} node '{addedNodeDisplayName}' to '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse ConnectPorts(
            ConnectPortsRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Connect ports request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "connect_ports"
            );

            string requestedOutputNodeId = request.OutputNodeId?.Trim();
            string requestedOutputPort = request.OutputPort?.Trim();
            string requestedInputNodeId = request.InputNodeId?.Trim();
            string requestedInputPort = request.InputPort?.Trim();

            if (string.IsNullOrWhiteSpace(requestedOutputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOutputPort) ||
                string.IsNullOrWhiteSpace(requestedInputNodeId) ||
                string.IsNullOrWhiteSpace(requestedInputPort))
            {
                return ShaderGraphResponse.Fail(
                    "Connect ports request requires output node id, output port, input node id, and input port.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOutputNodeId,
                    out object outputNode,
                    out string outputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputNodeFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedInputNodeId,
                    out object inputNode,
                    out string inputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputNodeFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            string outputNodeId = GetStringProperty(outputNode, "objectId");
            string inputNodeId = GetStringProperty(inputNode, "objectId");
            if (string.IsNullOrWhiteSpace(outputNodeId) || string.IsNullOrWhiteSpace(inputNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Resolved graph nodes do not expose stable object ids. Use read_graph_summary node ids instead of display names.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (string.Equals(outputNodeId, inputNodeId, StringComparison.Ordinal))
            {
                return ShaderGraphResponse.Fail(
                    "Connect ports requires two distinct nodes. Self-connections are not supported in the first package-backed path.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    outputNode,
                    requestedOutputPort,
                    true,
                    out int outputSlotId,
                    out string canonicalOutputPort,
                    out string outputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputPortFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    inputNode,
                    requestedInputPort,
                    false,
                    out int inputSlotId,
                    out string canonicalInputPort,
                    out string inputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputPortFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryValidateSupportedConnectionPair(
                    outputNode,
                    inputNode,
                    canonicalOutputPort,
                    canonicalInputPort,
                    out string pairFailure))
            {
                return ShaderGraphResponse.Fail(
                    pairFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryCreateSlotReference(outputNode, outputSlotId, out object outputSlotRef, out string outputSlotFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputSlotFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryCreateSlotReference(inputNode, inputSlotId, out object inputSlotRef, out string inputSlotFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputSlotFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryInvokeGraphConnect(graphData, outputSlotRef, inputSlotRef, out object connectedEdge, out string connectFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to connect the resolved Shader Graph slots: {connectFailure}",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after connecting ports: {writeFailure}",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var refreshedSnapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "connect_ports"
            );

            var data = new Dictionary<string, object>(refreshedSnapshot.ToDictionary())
            {
                ["supportedConnectionRules"] = SupportedConnectionRules.ToArray(),
                ["requestedConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = requestedOutputNodeId,
                    ["outputPort"] = requestedOutputPort,
                    ["inputNodeId"] = requestedInputNodeId,
                    ["inputPort"] = requestedInputPort,
                },
                ["resolvedConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = outputNodeId,
                    ["outputNodeType"] = GetTypeName(outputNode),
                    ["outputSlotId"] = outputSlotId,
                    ["outputPort"] = canonicalOutputPort,
                    ["inputNodeId"] = inputNodeId,
                    ["inputNodeType"] = GetTypeName(inputNode),
                    ["inputSlotId"] = inputSlotId,
                    ["inputPort"] = canonicalInputPort,
                    ["connectedEdgeType"] = connectedEdge?.GetType().FullName ?? string.Empty,
                },
            };

            return ShaderGraphResponse.Ok(
                $"Connected {GetTypeName(outputNode)}.{canonicalOutputPort} -> {GetTypeName(inputNode)}.{canonicalInputPort} in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse RemoveConnection(
            RemoveConnectionRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Remove connection request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "remove_connection"
            );

            string requestedOutputNodeId = request.OutputNodeId?.Trim();
            string requestedOutputPort = request.OutputPort?.Trim();
            string requestedInputNodeId = request.InputNodeId?.Trim();
            string requestedInputPort = request.InputPort?.Trim();

            if (string.IsNullOrWhiteSpace(requestedOutputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOutputPort) ||
                string.IsNullOrWhiteSpace(requestedInputNodeId) ||
                string.IsNullOrWhiteSpace(requestedInputPort))
            {
                return ShaderGraphResponse.Fail(
                    "Remove connection request requires output node id, output port, input node id, and input port.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOutputNodeId,
                    out object outputNode,
                    out string outputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputNodeFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedInputNodeId,
                    out object inputNode,
                    out string inputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputNodeFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            string outputNodeId = GetStringProperty(outputNode, "objectId");
            string inputNodeId = GetStringProperty(inputNode, "objectId");
            if (string.IsNullOrWhiteSpace(outputNodeId) || string.IsNullOrWhiteSpace(inputNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Resolved graph nodes do not expose stable object ids. Use read_graph_summary node ids instead of display names.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (string.Equals(outputNodeId, inputNodeId, StringComparison.Ordinal))
            {
                return ShaderGraphResponse.Fail(
                    "Remove connection requires two distinct nodes. Self-connections are not supported in the first package-backed path.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    outputNode,
                    requestedOutputPort,
                    true,
                    out int outputSlotId,
                    out string canonicalOutputPort,
                    out string outputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputPortFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    inputNode,
                    requestedInputPort,
                    false,
                    out int inputSlotId,
                    out string canonicalInputPort,
                    out string inputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputPortFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryValidateSupportedConnectionPair(
                    outputNode,
                    inputNode,
                    canonicalOutputPort,
                    canonicalInputPort,
                    out string pairFailure))
            {
                return ShaderGraphResponse.Fail(
                    pairFailure,
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            object[] matches = EnumerateMember(graphData, "edges")
                .Where(edge => EdgeMatchesResolvedConnection(edge, outputNodeId, outputSlotId, inputNodeId, inputSlotId))
                .ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph connection matching '{outputNodeId}:{canonicalOutputPort} -> {inputNodeId}:{canonicalInputPort}' in '{assetPath}'.",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (matches.Length > 1)
            {
                var duplicateData = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "remove_connection",
                    "RemoveConnection"
                );
                duplicateData["matchCount"] = matches.Length;
                duplicateData["candidateConnections"] = matches.Select(BuildEdgeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Connection query for '{outputNodeId}:{canonicalOutputPort} -> {inputNodeId}:{canonicalInputPort}' matched multiple edges in '{assetPath}'.",
                    duplicateData
                );
            }

            object edge = matches[0];
            Dictionary<string, object> deletedConnection = BuildEdgeLookupData(edge);
            deletedConnection["outputNodeId"] = outputNodeId;
            deletedConnection["outputNodeType"] = GetTypeName(outputNode);
            deletedConnection["outputSlotId"] = outputSlotId;
            deletedConnection["outputPort"] = canonicalOutputPort;
            deletedConnection["inputNodeId"] = inputNodeId;
            deletedConnection["inputNodeType"] = GetTypeName(inputNode);
            deletedConnection["inputSlotId"] = inputSlotId;
            deletedConnection["inputPort"] = canonicalInputPort;

            if (!TryInvokeGraphRemoveEdge(graphData, edge, out string removeEdgeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to remove the resolved Shader Graph connection: {removeEdgeFailure}",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after removing the connection: {validateFailure}",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after removing the connection: {writeFailure}",
                    BuildUnsupportedConnectionData(
                        snapshot,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort,
                        "remove_connection",
                        "RemoveConnection"
                    )
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var refreshedSnapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "remove_connection"
            );

            var data = new Dictionary<string, object>(refreshedSnapshot.ToDictionary())
            {
                ["supportedConnectionRules"] = SupportedConnectionRules.ToArray(),
                ["matchCount"] = 1,
                ["requestedConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = requestedOutputNodeId,
                    ["outputPort"] = requestedOutputPort,
                    ["inputNodeId"] = requestedInputNodeId,
                    ["inputPort"] = requestedInputPort,
                },
                ["resolvedConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = outputNodeId,
                    ["outputNodeType"] = GetTypeName(outputNode),
                    ["outputSlotId"] = outputSlotId,
                    ["outputPort"] = canonicalOutputPort,
                    ["inputNodeId"] = inputNodeId,
                    ["inputNodeType"] = GetTypeName(inputNode),
                    ["inputSlotId"] = inputSlotId,
                    ["inputPort"] = canonicalInputPort,
                },
                ["deletedConnection"] = deletedConnection,
            };

            return ShaderGraphResponse.Ok(
                $"Removed connection {GetTypeName(outputNode)}.{canonicalOutputPort} -> {GetTypeName(inputNode)}.{canonicalInputPort} in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse ReconnectConnection(
            ReconnectConnectionRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Reconnect connection request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "reconnect_connection"
            );

            string requestedOldOutputNodeId = request.OldOutputNodeId?.Trim();
            string requestedOldOutputPort = request.OldOutputPort?.Trim();
            string requestedOldInputNodeId = request.OldInputNodeId?.Trim();
            string requestedOldInputPort = request.OldInputPort?.Trim();
            string requestedOutputNodeId = request.OutputNodeId?.Trim();
            string requestedOutputPort = request.OutputPort?.Trim();
            string requestedInputNodeId = request.InputNodeId?.Trim();
            string requestedInputPort = request.InputPort?.Trim();

            if (string.IsNullOrWhiteSpace(requestedOldOutputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOldOutputPort) ||
                string.IsNullOrWhiteSpace(requestedOldInputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOldInputPort))
            {
                return ShaderGraphResponse.Fail(
                    "Reconnect connection request requires old output node id, old output port, old input node id, and old input port.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (string.IsNullOrWhiteSpace(requestedOutputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOutputPort) ||
                string.IsNullOrWhiteSpace(requestedInputNodeId) ||
                string.IsNullOrWhiteSpace(requestedInputPort))
            {
                return ShaderGraphResponse.Fail(
                    "Reconnect connection request requires new output node id, output port, input node id, and input port.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOldOutputNodeId,
                    out object oldOutputNode,
                    out string oldOutputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    oldOutputNodeFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOldInputNodeId,
                    out object oldInputNode,
                    out string oldInputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    oldInputNodeFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            string oldOutputNodeId = GetStringProperty(oldOutputNode, "objectId");
            string oldInputNodeId = GetStringProperty(oldInputNode, "objectId");
            if (string.IsNullOrWhiteSpace(oldOutputNodeId) || string.IsNullOrWhiteSpace(oldInputNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Resolved previous graph nodes do not expose stable object ids. Use read_graph_summary node ids instead of display names.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (string.Equals(oldOutputNodeId, oldInputNodeId, StringComparison.Ordinal))
            {
                return ShaderGraphResponse.Fail(
                    "Reconnect connection requires two distinct previous endpoint nodes. Self-connections are not supported in the first package-backed path.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    oldOutputNode,
                    requestedOldOutputPort,
                    true,
                    out int oldOutputSlotId,
                    out string canonicalOldOutputPort,
                    out string oldOutputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    oldOutputPortFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    oldInputNode,
                    requestedOldInputPort,
                    false,
                    out int oldInputSlotId,
                    out string canonicalOldInputPort,
                    out string oldInputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    oldInputPortFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryValidateSupportedConnectionPair(
                    oldOutputNode,
                    oldInputNode,
                    canonicalOldOutputPort,
                    canonicalOldInputPort,
                    out string oldPairFailure))
            {
                return ShaderGraphResponse.Fail(
                    oldPairFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            object[] previousMatches = EnumerateMember(graphData, "edges")
                .Where(edge => EdgeMatchesResolvedConnection(edge, oldOutputNodeId, oldOutputSlotId, oldInputNodeId, oldInputSlotId))
                .ToArray();

            if (previousMatches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph connection matching '{oldOutputNodeId}:{canonicalOldOutputPort} -> {oldInputNodeId}:{canonicalOldInputPort}' in '{assetPath}'.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (previousMatches.Length > 1)
            {
                var duplicateData = BuildUnsupportedReconnectConnectionData(
                    snapshot,
                    requestedOldOutputNodeId,
                    requestedOldOutputPort,
                    requestedOldInputNodeId,
                    requestedOldInputPort,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort);
                duplicateData["matchCount"] = previousMatches.Length;
                duplicateData["candidateConnections"] = previousMatches.Select(BuildEdgeLookupData).Cast<object>().ToArray();
                return ShaderGraphResponse.Fail(
                    $"Previous connection query for '{oldOutputNodeId}:{canonicalOldOutputPort} -> {oldInputNodeId}:{canonicalOldInputPort}' matched multiple edges in '{assetPath}'.",
                    duplicateData
                );
            }

            object previousEdge = previousMatches[0];
            Dictionary<string, object> removedConnection = BuildEdgeLookupData(previousEdge);
            removedConnection["outputNodeId"] = oldOutputNodeId;
            removedConnection["outputNodeType"] = GetTypeName(oldOutputNode);
            removedConnection["outputSlotId"] = oldOutputSlotId;
            removedConnection["outputPort"] = canonicalOldOutputPort;
            removedConnection["inputNodeId"] = oldInputNodeId;
            removedConnection["inputNodeType"] = GetTypeName(oldInputNode);
            removedConnection["inputSlotId"] = oldInputSlotId;
            removedConnection["inputPort"] = canonicalOldInputPort;

            if (!TryInvokeGraphRemoveEdge(graphData, previousEdge, out string removeEdgeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to remove the resolved previous Shader Graph connection: {removeEdgeFailure}",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOutputNodeId,
                    out object outputNode,
                    out string outputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputNodeFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedInputNodeId,
                    out object inputNode,
                    out string inputNodeFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputNodeFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            string outputNodeId = GetStringProperty(outputNode, "objectId");
            string inputNodeId = GetStringProperty(inputNode, "objectId");
            if (string.IsNullOrWhiteSpace(outputNodeId) || string.IsNullOrWhiteSpace(inputNodeId))
            {
                return ShaderGraphResponse.Fail(
                    "Resolved graph nodes do not expose stable object ids. Use read_graph_summary node ids instead of display names.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (string.Equals(outputNodeId, inputNodeId, StringComparison.Ordinal))
            {
                return ShaderGraphResponse.Fail(
                    "Reconnect connection requires two distinct new endpoint nodes. Self-connections are not supported in the first package-backed path.",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    outputNode,
                    requestedOutputPort,
                    true,
                    out int outputSlotId,
                    out string canonicalOutputPort,
                    out string outputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputPortFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    inputNode,
                    requestedInputPort,
                    false,
                    out int inputSlotId,
                    out string canonicalInputPort,
                    out string inputPortFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputPortFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryValidateSupportedConnectionPair(
                    outputNode,
                    inputNode,
                    canonicalOutputPort,
                    canonicalInputPort,
                    out string pairFailure))
            {
                return ShaderGraphResponse.Fail(
                    pairFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryCreateSlotReference(outputNode, outputSlotId, out object outputSlotRef, out string outputSlotFailure))
            {
                return ShaderGraphResponse.Fail(
                    outputSlotFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryCreateSlotReference(inputNode, inputSlotId, out object inputSlotRef, out string inputSlotFailure))
            {
                return ShaderGraphResponse.Fail(
                    inputSlotFailure,
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryInvokeGraphConnect(graphData, outputSlotRef, inputSlotRef, out object connectedEdge, out string connectFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to connect the resolved Shader Graph slots: {connectFailure}",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed after reconnecting the connection: {validateFailure}",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph after reconnecting the connection: {writeFailure}",
                    BuildUnsupportedReconnectConnectionData(
                        snapshot,
                        requestedOldOutputNodeId,
                        requestedOldOutputPort,
                        requestedOldInputNodeId,
                        requestedOldInputPort,
                        requestedOutputNodeId,
                        requestedOutputPort,
                        requestedInputNodeId,
                        requestedInputPort)
                );
            }

            AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

            var refreshedSnapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "reconnect_connection"
            );

            var connectedConnection = BuildEdgeLookupData(connectedEdge);
            connectedConnection["outputNodeId"] = outputNodeId;
            connectedConnection["outputNodeType"] = GetTypeName(outputNode);
            connectedConnection["outputSlotId"] = outputSlotId;
            connectedConnection["outputPort"] = canonicalOutputPort;
            connectedConnection["inputNodeId"] = inputNodeId;
            connectedConnection["inputNodeType"] = GetTypeName(inputNode);
            connectedConnection["inputSlotId"] = inputSlotId;
            connectedConnection["inputPort"] = canonicalInputPort;
            connectedConnection["connectedEdgeType"] = connectedEdge?.GetType().FullName ?? string.Empty;

            var data = new Dictionary<string, object>(refreshedSnapshot.ToDictionary())
            {
                ["supportedConnectionRules"] = SupportedConnectionRules.ToArray(),
                ["matchCount"] = 1,
                ["previousConnection"] = BuildConnectionQuery(
                    requestedOldOutputNodeId,
                    requestedOldOutputPort,
                    requestedOldInputNodeId,
                    requestedOldInputPort),
                ["requestedConnection"] = BuildConnectionQuery(
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort),
                ["resolvedPreviousConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = oldOutputNodeId,
                    ["outputNodeType"] = GetTypeName(oldOutputNode),
                    ["outputSlotId"] = oldOutputSlotId,
                    ["outputPort"] = canonicalOldOutputPort,
                    ["inputNodeId"] = oldInputNodeId,
                    ["inputNodeType"] = GetTypeName(oldInputNode),
                    ["inputSlotId"] = oldInputSlotId,
                    ["inputPort"] = canonicalOldInputPort,
                },
                ["removedConnection"] = removedConnection,
                ["resolvedConnection"] = new Dictionary<string, object>
                {
                    ["outputNodeId"] = outputNodeId,
                    ["outputNodeType"] = GetTypeName(outputNode),
                    ["outputSlotId"] = outputSlotId,
                    ["outputPort"] = canonicalOutputPort,
                    ["inputNodeId"] = inputNodeId,
                    ["inputNodeType"] = GetTypeName(inputNode),
                    ["inputSlotId"] = inputSlotId,
                    ["inputPort"] = canonicalInputPort,
                    ["connectedEdgeType"] = connectedEdge?.GetType().FullName ?? string.Empty,
                },
                ["connectedConnection"] = connectedConnection,
            };

            return ShaderGraphResponse.Ok(
                $"Reconnected {GetTypeName(oldOutputNode)}.{canonicalOldOutputPort} -> {GetTypeName(oldInputNode)}.{canonicalOldInputPort} to {GetTypeName(outputNode)}.{canonicalOutputPort} -> {GetTypeName(inputNode)}.{canonicalInputPort} in '{assetPath}'.",
                data
            );
        }

        public static ShaderGraphResponse FindConnection(
            FindConnectionRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Find connection request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.ValidateGraph() could not be invoked: {validateFailure}");
            }

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "find_connection"
            );

            string requestedOutputNodeId = request.OutputNodeId?.Trim();
            string requestedOutputPort = request.OutputPort?.Trim();
            string requestedInputNodeId = request.InputNodeId?.Trim();
            string requestedInputPort = request.InputPort?.Trim();
            Dictionary<string, object> query = BuildConnectionQuery(
                requestedOutputNodeId,
                requestedOutputPort,
                requestedInputNodeId,
                requestedInputPort);
            string[] matchStrategy = BuildFindConnectionMatchStrategy(
                requestedOutputNodeId,
                requestedOutputPort,
                requestedInputNodeId,
                requestedInputPort);

            if (string.IsNullOrWhiteSpace(requestedOutputNodeId) ||
                string.IsNullOrWhiteSpace(requestedOutputPort) ||
                string.IsNullOrWhiteSpace(requestedInputNodeId) ||
                string.IsNullOrWhiteSpace(requestedInputPort))
            {
                var invalidData = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                invalidData["query"] = query;
                invalidData["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(
                    "Find connection request requires output node id, output port, input node id, and input port.",
                    invalidData
                );
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedOutputNodeId,
                    out object outputNode,
                    out string outputNodeFailure))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(outputNodeFailure, data);
            }

            if (!TryResolveGraphNodeByObjectId(
                    graphData,
                    requestedInputNodeId,
                    out object inputNode,
                    out string inputNodeFailure))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(inputNodeFailure, data);
            }

            string outputNodeId = GetStringProperty(outputNode, "objectId");
            string inputNodeId = GetStringProperty(inputNode, "objectId");
            if (string.IsNullOrWhiteSpace(outputNodeId) || string.IsNullOrWhiteSpace(inputNodeId))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(
                    "Resolved graph nodes do not expose stable object ids. Use read_graph_summary node ids instead of display names.",
                    data
                );
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    outputNode,
                    requestedOutputPort,
                    true,
                    out int outputSlotId,
                    out string canonicalOutputPort,
                    out string outputPortFailure))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(outputPortFailure, data);
            }

            if (!TryResolveSupportedConnectionEndpoint(
                    inputNode,
                    requestedInputPort,
                    false,
                    out int inputSlotId,
                    out string canonicalInputPort,
                    out string inputPortFailure))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(inputPortFailure, data);
            }

            if (!TryValidateSupportedConnectionPair(
                    outputNode,
                    inputNode,
                    canonicalOutputPort,
                    canonicalInputPort,
                    out string pairFailure))
            {
                var data = BuildUnsupportedConnectionData(
                    snapshot,
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort,
                    "find_connection",
                    "FindConnection");
                data["query"] = query;
                data["matchStrategy"] = matchStrategy;
                return ShaderGraphResponse.Fail(pairFailure, data);
            }

            object[] matches = EnumerateMember(graphData, "edges")
                .Where(edge => EdgeMatchesResolvedConnection(edge, outputNodeId, outputSlotId, inputNodeId, inputSlotId))
                .ToArray();

            var responseData = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["query"] = query,
                ["requestedConnection"] = BuildConnectionQuery(
                    requestedOutputNodeId,
                    requestedOutputPort,
                    requestedInputNodeId,
                    requestedInputPort),
                ["matchCount"] = matches.Length,
                ["matchStrategy"] = matchStrategy,
                ["supportedConnectionRules"] = SupportedConnectionRules.ToArray(),
            };

            if (matches.Length == 1)
            {
                Dictionary<string, object> foundConnection = BuildEdgeLookupData(matches[0]);
                foundConnection["outputNodeId"] = outputNodeId;
                foundConnection["outputNodeType"] = GetTypeName(outputNode);
                foundConnection["outputSlotId"] = outputSlotId;
                foundConnection["outputPort"] = canonicalOutputPort;
                foundConnection["inputNodeId"] = inputNodeId;
                foundConnection["inputNodeType"] = GetTypeName(inputNode);
                foundConnection["inputSlotId"] = inputSlotId;
                foundConnection["inputPort"] = canonicalInputPort;

                responseData["resolvedConnection"] = new Dictionary<string, object>(foundConnection);
                responseData["foundConnection"] = foundConnection;

                return ShaderGraphResponse.Ok(
                    $"Found connection {GetTypeName(outputNode)}.{canonicalOutputPort} -> {GetTypeName(inputNode)}.{canonicalInputPort} in '{assetPath}'.",
                    responseData
                );
            }

            responseData["candidateConnections"] = matches.Select(BuildEdgeLookupData).Cast<object>().ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a Shader Graph connection matching '{outputNodeId}:{canonicalOutputPort} -> {inputNodeId}:{canonicalInputPort}' in '{assetPath}'.",
                    responseData
                );
            }

            return ShaderGraphResponse.Fail(
                $"Connection query for '{outputNodeId}:{canonicalOutputPort} -> {inputNodeId}:{canonicalInputPort}' matched multiple edges in '{assetPath}'.",
                responseData
            );
        }

        public static ShaderGraphResponse SaveGraph(
            SaveGraphRequest request,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Save graph request is required.");
            }

            string assetPath = NormalizeAssetPath(request.AssetPath);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absolutePath = ToAbsolutePath(assetPath);
            if (!File.Exists(absolutePath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            object graphData;
            var loadNotes = new List<string>();
            string failureReason;
            if (!TryLoadGraphData(assetPath, absolutePath, out graphData, loadNotes, out failureReason))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to load Shader Graph GraphData from '{assetPath}': {failureReason}"
                );
            }

            if (TryInvokeInstanceMethod(graphData, "OnEnable", out string onEnableFailure))
            {
                loadNotes.Add("GraphData.OnEnable() invoked successfully.");
            }
            else
            {
                loadNotes.Add($"GraphData.OnEnable() could not be invoked: {onEnableFailure}");
            }

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                loadNotes.Add("GraphData.ValidateGraph() invoked successfully.");
            }
            else
            {
                return ShaderGraphResponse.Fail(
                    $"Graph validation failed while saving '{assetPath}': {validateFailure}",
                    BuildUnsupportedSaveGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes
                    )
                );
            }

            if (!TryWriteGraphDataToDisk(assetPath, graphData, out string writeFailure))
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to save Shader Graph at '{assetPath}': {writeFailure}",
                    BuildUnsupportedSaveGraphData(
                        assetPath,
                        compatibility,
                        executionKind,
                        loadNotes
                    )
                );
            }

            loadNotes.Add("FileUtilities.WriteShaderGraphToDisk(...) invoked successfully.");

            AssetDatabase.SaveAssets();
            loadNotes.Add("AssetDatabase.SaveAssets() invoked successfully.");

            AssetDatabase.ImportAsset(
                assetPath,
                ImportAssetOptions.ForceSynchronousImport | ImportAssetOptions.ForceUpdate
            );
            loadNotes.Add("AssetDatabase.ImportAsset(..., ForceSynchronousImport | ForceUpdate) invoked successfully.");

            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            loadNotes.Add("AssetDatabase.Refresh(ForceSynchronousImport) invoked successfully.");

            var snapshot = BuildSnapshot(
                graphData,
                assetPath,
                absolutePath,
                executionKind,
                compatibility,
                loadNotes,
                "save_graph"
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary())
            {
                ["saveGraphStrategy"] = new[]
                {
                    "GraphData.ValidateGraph()",
                    "FileUtilities.WriteShaderGraphToDisk(string, GraphData)",
                    "AssetDatabase.SaveAssets()",
                    "AssetDatabase.ImportAsset(..., ForceSynchronousImport | ForceUpdate)",
                    "AssetDatabase.Refresh(ForceSynchronousImport)",
                },
            };

            return ShaderGraphResponse.Ok(
                $"Saved package-backed Shader Graph at '{assetPath}'.",
                data
            );
        }

        private static ShaderGraphAssetSnapshot BuildSnapshot(
            object graphData,
            string assetPath,
            string absolutePath,
            ShaderGraphExecutionKind executionKind,
            ShaderGraphCompatibilitySnapshot compatibility,
            IReadOnlyList<string> loadNotes,
            string operation)
        {
            var fileInfo = new FileInfo(absolutePath);
            Type graphType = graphData.GetType();

            IReadOnlyList<string> properties = DescribeProperties(graphData);
            IReadOnlyList<string> nodes = DescribeNodes(graphData);
            IReadOnlyList<string> connections = DescribeConnections(graphData);
            IReadOnlyList<string> preview = BuildPreview(graphData, properties, nodes, connections);

            string assetName = Path.GetFileNameWithoutExtension(assetPath);
            string graphPath = GetStringProperty(graphData, "path");
            string graphGuid = GetStringProperty(graphData, "assetGuid");
            bool isSubGraph = GetBoolProperty(graphData, "isSubGraph");
            int categoryCount = CountEnumerableProperty(graphData, "categories");

            var notes = new List<string>();
            if (loadNotes != null)
            {
                notes.AddRange(loadNotes);
            }

            if (!string.IsNullOrWhiteSpace(graphPath))
            {
                notes.Add($"GraphData.path = '{graphPath}'.");
            }

            if (!string.IsNullOrWhiteSpace(graphGuid))
            {
                notes.Add($"GraphData.assetGuid = '{graphGuid}'.");
            }

            notes.Add(isSubGraph
                ? "GraphData represents a sub graph."
                : "GraphData represents a shader graph.");

            notes.Add($"GraphData categories observed: {categoryCount}.");

            if (graphType != null)
            {
                notes.Add($"Resolved graph type: {graphType.FullName ?? graphType.Name}.");
            }

            return new ShaderGraphAssetSnapshot(
                operation,
                assetPath,
                string.Empty,
                absolutePath,
                true,
                false,
                PackageSchema,
                assetName,
                string.Empty,
                fileInfo.CreationTimeUtc.ToString("O"),
                fileInfo.LastWriteTimeUtc.ToString("O"),
                properties.Count,
                nodes.Count,
                connections.Count,
                executionKind,
                properties,
                nodes,
                connections,
                notes,
                preview,
                compatibility
            );
        }

        private static bool NodeMatchesQuery(
            object node,
            string queryNodeId,
            string queryDisplayName,
            string queryNodeType)
        {
            if (node == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryNodeId))
            {
                string objectId = GetStringProperty(node, "objectId");
                if (!string.Equals(objectId, queryNodeId, StringComparison.Ordinal))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                string displayName = GetStringProperty(node, "displayName", "name");
                if (!string.Equals(displayName, queryDisplayName, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            if (!string.IsNullOrWhiteSpace(queryNodeType) &&
                !NodeTypeMatchesQuery(node.GetType(), queryNodeType))
            {
                return false;
            }

            return true;
        }

        private static bool NodeTypeMatchesQuery(Type nodeClassType, string queryNodeType)
        {
            if (nodeClassType == null)
            {
                return false;
            }

            string normalizedQuery = NormalizeNodeToken(queryNodeType);
            if (string.IsNullOrWhiteSpace(normalizedQuery))
            {
                return false;
            }

            string fullTypeName = nodeClassType.FullName ?? nodeClassType.Name ?? string.Empty;
            if (string.Equals(NormalizeNodeToken(fullTypeName), normalizedQuery, StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(NormalizeNodeToken(nodeClassType.Name), normalizedQuery, StringComparison.Ordinal))
            {
                return true;
            }

            string canonicalName = BuildCanonicalNodeName(nodeClassType);
            if (string.Equals(NormalizeNodeToken(canonicalName), normalizedQuery, StringComparison.Ordinal))
            {
                return true;
            }

            ShaderGraphNodeDescriptor descriptor = GetDiscoveredNodeCatalog()
                .FirstOrDefault(item => item.NodeClassType == nodeClassType);
            if (descriptor == null)
            {
                return false;
            }

            return descriptor.Aliases.Any(
                alias => string.Equals(
                    NormalizeNodeToken(alias),
                    normalizedQuery,
                    StringComparison.Ordinal));
        }

        private static Dictionary<string, object> BuildNodeQuery(
            string queryNodeId,
            string queryDisplayName,
            string queryNodeType)
        {
            var query = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(queryNodeId))
            {
                query["nodeId"] = queryNodeId;
                query["objectId"] = queryNodeId;
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                query["displayName"] = queryDisplayName;
            }

            if (!string.IsNullOrWhiteSpace(queryNodeType))
            {
                query["nodeType"] = queryNodeType;
            }

            return query;
        }

        private static string[] BuildFindNodeMatchStrategy(
            string queryNodeId,
            string queryDisplayName,
            string queryNodeType)
        {
            var strategy = new List<string>();
            if (!string.IsNullOrWhiteSpace(queryNodeId))
            {
                strategy.Add("Exact GraphData objectId match.");
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                strategy.Add("Case-insensitive displayName match.");
            }

            if (!string.IsNullOrWhiteSpace(queryNodeType))
            {
                strategy.Add("Normalized nodeType alias, canonical name, or CLR type match.");
            }

            return strategy.ToArray();
        }

        private static Dictionary<string, object> BuildNodeLookupData(object node)
        {
            Type nodeClassType = node?.GetType();
            string canonicalNodeType = BuildCanonicalNodeName(nodeClassType);
            string fullTypeName = nodeClassType?.FullName ?? nodeClassType?.Name ?? string.Empty;
            string displayName = GetStringProperty(node, "displayName", "name");
            string objectId = GetStringProperty(node, "objectId");

            var data = new Dictionary<string, object>
            {
                ["objectId"] = objectId,
                ["nodeId"] = objectId,
                ["displayName"] = displayName,
                ["nodeType"] = canonicalNodeType,
                ["fullTypeName"] = fullTypeName,
                ["summary"] = DescribeNode(node),
            };

            if (TryDescribeNodePosition(node, out string positionDescription))
            {
                data["position"] = positionDescription;
            }

            return data;
        }

        private static Dictionary<string, object> BuildMovedNodeData(
            object node,
            Rect movedPosition,
            Rect? previousPosition)
        {
            var data = BuildNodeLookupData(node);
            data["position"] = BuildPositionData(movedPosition);
            if (previousPosition.HasValue)
            {
                data["previousPosition"] = BuildPositionData(previousPosition.Value);
            }

            return data;
        }

        private static Dictionary<string, object> BuildRenamedNodeData(object node, string previousDisplayName)
        {
            var data = BuildNodeLookupData(node);
            data["previousDisplayName"] = previousDisplayName ?? string.Empty;
            return data;
        }

        private static Dictionary<string, object> BuildDuplicatedFromData(object sourceNode, Rect? sourcePosition)
        {
            var data = BuildNodeLookupData(sourceNode);
            if (sourcePosition.HasValue)
            {
                data["position"] = BuildPositionData(sourcePosition.Value);
            }

            return data;
        }

        private static Dictionary<string, object> BuildDuplicatedNodeData(
            object duplicatedNode,
            object sourceNode,
            Rect duplicatedPosition,
            Rect? sourcePosition)
        {
            var data = BuildNodeLookupData(duplicatedNode);
            data["position"] = BuildPositionData(duplicatedPosition);
            data["sourceNodeId"] = GetStringProperty(sourceNode, "objectId");
            data["sourceDisplayName"] = GetStringProperty(sourceNode, "displayName", "name");
            if (sourcePosition.HasValue)
            {
                data["sourcePosition"] = BuildPositionData(sourcePosition.Value);
            }

            return data;
        }

        private static Dictionary<string, object> BuildPositionData(Rect rect)
        {
            return new Dictionary<string, object>
            {
                ["x"] = rect.x,
                ["y"] = rect.y,
            };
        }

        private static bool PropertyMatchesName(object property, string propertyName)
        {
            if (property == null || string.IsNullOrWhiteSpace(propertyName))
            {
                return false;
            }

            return string.Equals(GetStringProperty(property, "displayName"), propertyName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(GetStringProperty(property, "referenceName"), propertyName, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(GetStringProperty(property, "name"), propertyName, StringComparison.OrdinalIgnoreCase);
        }

        private static bool PropertyMatchesQuery(
            object property,
            string queryPropertyName,
            string queryDisplayName,
            string queryReferenceName,
            string queryPropertyType)
        {
            if (property == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryPropertyName) &&
                !PropertyMatchesName(property, queryPropertyName))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName) &&
                !string.Equals(GetStringProperty(property, "displayName", "name"), queryDisplayName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryReferenceName) &&
                !string.Equals(GetStringProperty(property, "referenceName"), queryReferenceName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryPropertyType))
            {
                if (!TryResolvePropertyTypeFromInstance(property, out string canonicalPropertyType, out Type shaderInputType, out _))
                {
                    return false;
                }

                string normalizedQuery = queryPropertyType.Trim();
                string fullTypeName = shaderInputType?.FullName ?? shaderInputType?.Name ?? string.Empty;
                string shortTypeName = shaderInputType?.Name ?? string.Empty;
                if (!string.Equals(canonicalPropertyType, normalizedQuery, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(fullTypeName, normalizedQuery, StringComparison.OrdinalIgnoreCase) &&
                    !string.Equals(shortTypeName, normalizedQuery, StringComparison.OrdinalIgnoreCase))
                {
                    return false;
                }
            }

            return true;
        }

        private static Dictionary<string, object> BuildPropertyLookupData(object property)
        {
            string displayName = GetStringProperty(property, "displayName", "name");
            string referenceName = GetStringProperty(property, "referenceName");
            string fullTypeName = property?.GetType().FullName ?? property?.GetType().Name ?? string.Empty;

            var data = new Dictionary<string, object>
            {
                ["displayName"] = displayName,
                ["referenceName"] = referenceName,
                ["fullTypeName"] = fullTypeName,
                ["summary"] = DescribeProperty(property),
            };

            if (TryResolvePropertyTypeFromInstance(property, out string canonicalPropertyType, out _, out _))
            {
                data["resolvedPropertyType"] = canonicalPropertyType;
            }

            return data;
        }

        private static Dictionary<string, object> BuildPropertyQuery(
            string queryPropertyName,
            string queryDisplayName,
            string queryReferenceName,
            string queryPropertyType)
        {
            var query = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(queryPropertyName))
            {
                query["propertyName"] = queryPropertyName;
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                query["displayName"] = queryDisplayName;
            }

            if (!string.IsNullOrWhiteSpace(queryReferenceName))
            {
                query["referenceName"] = queryReferenceName;
            }

            if (!string.IsNullOrWhiteSpace(queryPropertyType))
            {
                query["propertyType"] = queryPropertyType;
            }

            return query;
        }

        private static Dictionary<string, object> BuildConnectionQuery(
            string queryOutputNodeId,
            string queryOutputPort,
            string queryInputNodeId,
            string queryInputPort)
        {
            var query = new Dictionary<string, object>();

            if (!string.IsNullOrWhiteSpace(queryOutputNodeId))
            {
                query["outputNodeId"] = queryOutputNodeId;
                query["sourceNodeId"] = queryOutputNodeId;
            }

            if (!string.IsNullOrWhiteSpace(queryOutputPort))
            {
                query["outputPort"] = queryOutputPort;
                query["sourcePort"] = queryOutputPort;
            }

            if (!string.IsNullOrWhiteSpace(queryInputNodeId))
            {
                query["inputNodeId"] = queryInputNodeId;
                query["targetNodeId"] = queryInputNodeId;
            }

            if (!string.IsNullOrWhiteSpace(queryInputPort))
            {
                query["inputPort"] = queryInputPort;
                query["targetPort"] = queryInputPort;
            }

            return query;
        }

        private static string[] BuildFindPropertyMatchStrategy(
            string queryPropertyName,
            string queryDisplayName,
            string queryReferenceName,
            string queryPropertyType)
        {
            var strategy = new List<string>();

            if (!string.IsNullOrWhiteSpace(queryPropertyName))
            {
                strategy.Add("Case-insensitive property name, displayName, or referenceName match.");
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                strategy.Add("Case-insensitive displayName match.");
            }

            if (!string.IsNullOrWhiteSpace(queryReferenceName))
            {
                strategy.Add("Case-insensitive referenceName match.");
            }

            if (!string.IsNullOrWhiteSpace(queryPropertyType))
            {
                strategy.Add("Property type canonical name or CLR type match.");
            }

            return strategy.ToArray();
        }

        private static string[] BuildFindConnectionMatchStrategy(
            string queryOutputNodeId,
            string queryOutputPort,
            string queryInputNodeId,
            string queryInputPort)
        {
            var strategy = new List<string>();

            if (!string.IsNullOrWhiteSpace(queryOutputNodeId))
            {
                strategy.Add("Exact GraphData output objectId/sourceNodeId match.");
            }

            if (!string.IsNullOrWhiteSpace(queryOutputPort))
            {
                strategy.Add("Supported output port alias resolves to the canonical slot id.");
            }

            if (!string.IsNullOrWhiteSpace(queryInputNodeId))
            {
                strategy.Add("Exact GraphData input objectId/targetNodeId match.");
            }

            if (!string.IsNullOrWhiteSpace(queryInputPort))
            {
                strategy.Add("Supported input port alias resolves to the canonical slot id.");
            }

            return strategy.ToArray();
        }

        private static Dictionary<string, object> BuildRenamedPropertyData(
            object property,
            string previousDisplayName,
            string previousReferenceName,
            string canonicalPropertyType,
            Type shaderInputType)
        {
            var data = BuildPropertyLookupData(property);
            data["previousDisplayName"] = previousDisplayName ?? string.Empty;
            data["previousReferenceName"] = previousReferenceName ?? string.Empty;
            data["resolvedPropertyType"] = canonicalPropertyType ?? string.Empty;
            data["resolvedShaderInputType"] = shaderInputType?.FullName ?? shaderInputType?.Name ?? string.Empty;
            return data;
        }

        private static Dictionary<string, object> BuildDuplicatedPropertyData(
            object duplicatedProperty,
            object sourceProperty,
            string canonicalPropertyType,
            Type shaderInputType)
        {
            var data = BuildPropertyLookupData(duplicatedProperty);
            data["sourceDisplayName"] = GetStringProperty(sourceProperty, "displayName", "name");
            data["sourceReferenceName"] = GetStringProperty(sourceProperty, "referenceName");
            data["resolvedPropertyType"] = canonicalPropertyType ?? string.Empty;
            data["resolvedShaderInputType"] = shaderInputType?.FullName ?? shaderInputType?.Name ?? string.Empty;
            return data;
        }

        private static readonly string[] SupportedPropertyTypes =
        {
            "Color",
            "Float/Vector1",
        };

        private static readonly string[] SupportedCreateTemplates =
        {
            "blank",
        };

        private static readonly string[] SupportedConnectionRules =
        {
            "Node ids must be exact GraphData objectId values reported by add_node or read_graph_summary.",
            "This first path only supports Vector1Node output slot 0 / Out into a different Vector1Node input slot 1 / X.",
            "ColorNode output slot 0 / Out is supported only when the input node is SplitNode input slot 0 / In.",
            "SplitNode output slots 1-4 / R,G,B,A are supported when the input node is a different Vector1Node input slot 1 / X.",
            "Vector1Node, SplitNode channel outputs, and scalar arithmetic output slot Out are supported when the input node is CombineNode input slots R/G/B/A or Vector2Node/Vector3Node/Vector4Node scalar input slots.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are supported when the input node is SplitNode input slot 0 / In.",
            "Vector1Node output slot 0 / Out is also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are supported when the input node is a different Vector1Node input slot 1 / X.",
            "AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, and OneMinusNode output slot Out are also supported when the input node is AddNode, SubtractNode, MultiplyNode, DivideNode, PowerNode, MinimumNode, MaximumNode, ModuloNode, LerpNode, SmoothstepNode, ClampNode, StepNode, AbsoluteNode, FloorNode, CeilingNode, RoundNode, SignNode, SineNode, CosineNode, TangentNode, NegateNode, ReciprocalNode, SquareRootNode, FractionNode, TruncateNode, SaturateNode, or OneMinusNode on their current scalar ports.",
            "Vector1Node and scalar arithmetic output slot Out are supported when the input node is ComparisonNode input slot 0 / A or 1 / B.",
            "ComparisonNode output slot 2 / Out is supported only when the input node is BranchNode input slot 0 / Predicate.",
            "Vector1Node and scalar arithmetic output slot Out are supported when the input node is BranchNode input slot 1 / True or 2 / False.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, and AppendVectorNode output slot Out are also supported when the input node is MultiplyNode input slot 0 / A or 1 / B, BranchNode input slot 1 / True or 2 / False, or LerpNode input slot 0 / A, 1 / B, or 2 / T.",
            "ColorNode output slot 0 / Out, CombineNode output slot 4 / RGBA, Vector4Node output slot 0 / Out, MultiplyNode output slot Out, BranchNode output slot Out, LerpNode output slot Out, AppendVectorNode output slot Out, Vector1Node output slot 0 / Out, SplitNode channel outputs, and scalar arithmetic output slot Out are also supported when the input node is AppendVectorNode input slot 0 / A or 1 / B.",
            "BranchNode output slot 3 / Out is supported when the input node is a different Vector1Node input slot 1 / X or scalar arithmetic input ports.",
            "Self-connections are rejected.",
        };

        internal static string[] GetSupportedConnectionRules()
        {
            return SupportedConnectionRules.ToArray();
        }

        private static Dictionary<string, object> BuildUnsupportedPropertyData(
            string assetPath,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind,
            IReadOnlyList<string> loadNotes,
            string requestedPropertyType,
            string actionName = "add_property")
        {
            var data = new Dictionary<string, object>
            {
                ["action"] = actionName,
                ["assetPath"] = assetPath,
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedPropertyTypes"] = SupportedPropertyTypes.ToArray(),
                ["requestedPropertyType"] = requestedPropertyType ?? string.Empty,
                ["notes"] = loadNotes == null ? Array.Empty<string>() : loadNotes.ToArray(),
            };

            return data;
        }

        private static Dictionary<string, object> BuildUnsupportedCreateGraphData(
            string assetPath,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind,
            IReadOnlyList<string> loadNotes,
            string requestedTemplate)
        {
            var data = new Dictionary<string, object>
            {
                ["action"] = "create_graph",
                ["assetPath"] = assetPath,
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedCreateTemplates"] = SupportedCreateTemplates.ToArray(),
                ["requestedTemplate"] = requestedTemplate ?? string.Empty,
                ["notes"] = loadNotes == null ? Array.Empty<string>() : loadNotes.ToArray(),
            };

            return data;
        }

        private static Dictionary<string, object> BuildUnsupportedNodeData(
            string assetPath,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind,
            IReadOnlyList<string> loadNotes,
            string requestedNodeType,
            string requestedDisplayName)
        {
            var data = new Dictionary<string, object>
            {
                ["action"] = "add_node",
                ["assetPath"] = assetPath,
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedNodeTypes"] = GetSupportedNodeTypeLabels(),
                ["requestedNodeType"] = requestedNodeType ?? string.Empty,
                ["requestedDisplayName"] = requestedDisplayName ?? string.Empty,
                ["supportedNodeCount"] = GetGraphAddableNodeCatalog().Count,
                ["discoveredNodeTypes"] = GetDiscoveredNodeTypeLabels(),
                ["discoveredNodeCount"] = GetDiscoveredNodeCatalog().Count,
                ["nodeCatalogSemantics"] = "supported=graph-addable",
                ["notes"] = loadNotes == null ? Array.Empty<string>() : loadNotes.ToArray(),
            };

            return data;
        }

        private static Dictionary<string, object> BuildUnsupportedConnectionData(
            ShaderGraphAssetSnapshot snapshot,
            string requestedOutputNodeId,
            string requestedOutputPort,
            string requestedInputNodeId,
            string requestedInputPort,
            string actionName = "connect_ports",
            string actionLabel = "ConnectPorts")
        {
            var data = snapshot == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(snapshot.ToDictionary());

            data["action"] = actionName;
            data["supportedConnectionRules"] = SupportedConnectionRules.ToArray();
            data["requestedConnection"] = new Dictionary<string, object>
            {
                ["outputNodeId"] = requestedOutputNodeId ?? string.Empty,
                ["outputPort"] = requestedOutputPort ?? string.Empty,
                ["inputNodeId"] = requestedInputNodeId ?? string.Empty,
                ["inputPort"] = requestedInputPort ?? string.Empty,
            };
            data["nodeIdentifierContract"] = $"{actionLabel} expects exact GraphData objectId values returned by addedNode.objectId or read_graph_summary nodes; display names are not supported.";

            return data;
        }

        private static Dictionary<string, object> BuildUnsupportedReconnectConnectionData(
            ShaderGraphAssetSnapshot snapshot,
            string requestedOldOutputNodeId,
            string requestedOldOutputPort,
            string requestedOldInputNodeId,
            string requestedOldInputPort,
            string requestedOutputNodeId,
            string requestedOutputPort,
            string requestedInputNodeId,
            string requestedInputPort)
        {
            var data = snapshot == null
                ? new Dictionary<string, object>()
                : new Dictionary<string, object>(snapshot.ToDictionary());

            data["action"] = "reconnect_connection";
            data["supportedConnectionRules"] = SupportedConnectionRules.ToArray();
            data["previousConnection"] = BuildConnectionQuery(
                requestedOldOutputNodeId,
                requestedOldOutputPort,
                requestedOldInputNodeId,
                requestedOldInputPort);
            data["requestedConnection"] = BuildConnectionQuery(
                requestedOutputNodeId,
                requestedOutputPort,
                requestedInputNodeId,
                requestedInputPort);
            data["nodeIdentifierContract"] = "ReconnectConnection expects exact GraphData objectId values returned by addedNode.objectId or read_graph_summary nodes; display names are not supported.";

            return data;
        }

        private static Dictionary<string, object> BuildUnsupportedSaveGraphData(
            string assetPath,
            ShaderGraphCompatibilitySnapshot compatibility,
            ShaderGraphExecutionKind executionKind,
            IReadOnlyList<string> loadNotes)
        {
            var data = new Dictionary<string, object>
            {
                ["action"] = "save_graph",
                ["assetPath"] = assetPath,
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["saveGraphStrategy"] = new[]
                {
                    "GraphData.ValidateGraph()",
                    "FileUtilities.WriteShaderGraphToDisk(string, GraphData)",
                    "AssetDatabase.SaveAssets()",
                    "AssetDatabase.ImportAsset(..., ForceSynchronousImport | ForceUpdate)",
                    "AssetDatabase.Refresh(ForceSynchronousImport)",
                },
                ["notes"] = loadNotes == null ? Array.Empty<string>() : loadNotes.ToArray(),
            };

            return data;
        }

        private static bool TryResolveSupportedNodeType(
            string nodeType,
            out string canonicalNodeType,
            out Type nodeClassType,
            out string failureReason)
        {
            canonicalNodeType = string.Empty;
            nodeClassType = null;
            failureReason = null;

            string normalized = (nodeType ?? string.Empty).Trim();
            string[] supportedNodeTypes = GetSupportedNodeTypeLabels();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                failureReason =
                    $"Node type is required. Supported node types: {string.Join(", ", supportedNodeTypes)}.";
                return false;
            }

            string normalizedToken = NormalizeNodeToken(normalized);
            foreach (ShaderGraphNodeDescriptor descriptor in GetGraphAddableNodeCatalog())
            {
                for (int i = 0; i < descriptor.Aliases.Count; i += 1)
                {
                    if (!string.Equals(NormalizeNodeToken(descriptor.Aliases[i]), normalizedToken, StringComparison.Ordinal))
                    {
                        continue;
                    }

                    canonicalNodeType = descriptor.CanonicalName;
                    nodeClassType = descriptor.NodeClassType;
                    return true;
                }
            }

            failureReason =
                $"Unsupported Shader Graph node type '{nodeType}'. Supported node types: {string.Join(", ", supportedNodeTypes)}.";
            return false;
        }

        private static bool TryCreateShaderNode(
            Type nodeClassType,
            string displayName,
            out object node,
            out string failureReason)
        {
            node = null;
            failureReason = null;

            if (nodeClassType == null)
            {
                failureReason = "Node class type is required.";
                return false;
            }

            try
            {
                node = Activator.CreateInstance(nodeClassType, true);
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to instantiate {nodeClassType.FullName}: {GetRootMessage(ex)}";
                return false;
            }

            string normalizedDisplayName = displayName?.Trim();
            if (!string.IsNullOrWhiteSpace(normalizedDisplayName))
            {
                SetMemberValue(node, "name", normalizedDisplayName);
            }

            return true;
        }

        private static bool TryAssignVisibleNodeLayout(
            object graphData,
            object node,
            string canonicalNodeType,
            out Rect assignedPosition,
            out string failureReason)
        {
            assignedPosition = default;
            failureReason = null;

            if (node == null)
            {
                failureReason = "Node instance is null.";
                return false;
            }

            Type drawStateType = ResolveType(DrawStateTypeName);
            if (drawStateType == null)
            {
                failureReason = $"Could not resolve {DrawStateTypeName}.";
                return false;
            }

            Vector2 origin = BuildSuggestedNodeOrigin(graphData, canonicalNodeType);
            assignedPosition = new Rect(origin, Vector2.zero);

            try
            {
                object drawState = Activator.CreateInstance(drawStateType, true);
                SetMemberValue(drawState, "expanded", true);
                SetMemberValue(drawState, "position", assignedPosition);
                SetMemberValue(node, "drawState", drawState);
                return true;
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to assign node draw state: {GetRootMessage(ex)}";
                return false;
            }
        }

        private static bool TryAssignExactNodePosition(
            object node,
            float x,
            float y,
            out Rect assignedPosition,
            out string failureReason)
        {
            assignedPosition = default;
            failureReason = null;

            if (node == null)
            {
                failureReason = "Node instance is null.";
                return false;
            }

            Type drawStateType = ResolveType(DrawStateTypeName);
            if (drawStateType == null)
            {
                failureReason = $"Could not resolve {DrawStateTypeName}.";
                return false;
            }

            try
            {
                object drawState = GetMemberValue(node, "drawState") ?? Activator.CreateInstance(drawStateType, true);
                Rect existingRect = TryGetNodePositionRect(node, out Rect currentRect)
                    ? currentRect
                    : new Rect(0f, 0f, 0f, 0f);

                assignedPosition = new Rect(x, y, existingRect.width, existingRect.height);
                SetMemberValue(drawState, "expanded", true);
                SetMemberValue(drawState, "position", assignedPosition);
                SetMemberValue(node, "drawState", drawState);
                return true;
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to assign node draw state: {GetRootMessage(ex)}";
                return false;
            }
        }

        private static bool TryAssignDuplicatedNodeLayout(
            object graphData,
            object sourceNode,
            object duplicatedNode,
            string canonicalNodeType,
            out Rect assignedPosition,
            out string failureReason)
        {
            assignedPosition = default;
            failureReason = null;

            if (sourceNode != null && TryGetNodePositionRect(sourceNode, out Rect sourcePosition))
            {
                return TryAssignExactNodePosition(
                    duplicatedNode,
                    sourcePosition.x + DuplicateNodeOffsetX,
                    sourcePosition.y + DuplicateNodeOffsetY,
                    out assignedPosition,
                    out failureReason);
            }

            return TryAssignVisibleNodeLayout(
                graphData,
                duplicatedNode,
                canonicalNodeType,
                out assignedPosition,
                out failureReason);
        }

        private static IReadOnlyList<ShaderGraphNodeDescriptor> GetSupportedNodeCatalog()
        {
            return SupportedNodeCatalog.Value;
        }

        private static string BuildDuplicateDisplayName(string displayName, string canonicalNodeType)
        {
            string label = string.IsNullOrWhiteSpace(displayName)
                ? (string.IsNullOrWhiteSpace(canonicalNodeType) ? "Node" : canonicalNodeType)
                : displayName.Trim();
            return label + " Copy";
        }

        private static IReadOnlyList<ShaderGraphNodeDescriptor> GetDiscoveredNodeCatalog()
        {
            return DiscoveredNodeCatalog.Value;
        }

        private static IReadOnlyList<ShaderGraphNodeDescriptor> GetGraphAddableNodeCatalog()
        {
            return SupportedNodeCatalog.Value;
        }

        private static string[] GetSupportedNodeTypeLabels()
        {
            return GetGraphAddableNodeCatalog()
                .Select(descriptor => descriptor.Label)
                .OrderBy(label => label, StringComparer.Ordinal)
                .ToArray();
        }

        internal static IReadOnlyList<string> GetSupportedNodeCanonicalNames()
        {
            return GetGraphAddableNodeCatalog()
                .Select(descriptor => descriptor.CanonicalName)
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        internal static string[] GetSupportedPropertyTypes()
        {
            return SupportedPropertyTypes
                .OrderBy(name => name, StringComparer.Ordinal)
                .ToArray();
        }

        internal static string[] GetSupportedNodeCatalogLabels()
        {
            return GetSupportedNodeTypeLabels();
        }

        internal static string[] GetDiscoveredNodeCatalogLabels()
        {
            return GetDiscoveredNodeTypeLabels();
        }

        private static string[] GetDiscoveredNodeTypeLabels()
        {
            return GetDiscoveredNodeCatalog()
                .Select(descriptor => descriptor.Label)
                .OrderBy(label => label, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<ShaderGraphNodeDescriptor> DiscoverSupportedNodeCatalog()
        {
            Type abstractMaterialNodeType = ResolveType("UnityEditor.ShaderGraph.AbstractMaterialNode");
            if (abstractMaterialNodeType == null)
            {
                return Array.Empty<ShaderGraphNodeDescriptor>();
            }

            var descriptors = new List<ShaderGraphNodeDescriptor>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (Type type in GetLoadableTypes(assembly))
                {
                    if (type == null ||
                        type.IsAbstract ||
                        type.ContainsGenericParameters ||
                        !abstractMaterialNodeType.IsAssignableFrom(type) ||
                        !IsSupportedNodeNamespace(type) ||
                        !HasParameterlessConstructor(type))
                    {
                        continue;
                    }

                    string canonicalName = BuildCanonicalNodeName(type);
                    IReadOnlyList<string> aliases = BuildNodeAliases(type, canonicalName);
                    if (TryGetNodeCatalogExclusionReason(type, out string exclusionReason))
                    {
                        descriptors.Add(new ShaderGraphNodeDescriptor(
                            canonicalName,
                            type,
                            aliases,
                            "filtered",
                            exclusionReason));
                        continue;
                    }

                    if (TryProbeGraphAddableNode(type, canonicalName, out string probeNote))
                    {
                        descriptors.Add(new ShaderGraphNodeDescriptor(
                            canonicalName,
                            type,
                            aliases,
                            "graph-addable",
                            probeNote));
                        continue;
                    }

                    descriptors.Add(new ShaderGraphNodeDescriptor(
                        canonicalName,
                        type,
                        aliases,
                        "probe-failed",
                        probeNote));
                }
            }

            return descriptors
                .OrderBy(descriptor => descriptor.CanonicalName, StringComparer.Ordinal)
                .ThenBy(descriptor => descriptor.FullTypeName, StringComparer.Ordinal)
                .ToArray();
        }

        private static IReadOnlyList<ShaderGraphNodeDescriptor> FilterSupportedNodeCatalog()
        {
            return GetDiscoveredNodeCatalog()
                .Where(descriptor => descriptor.IsGraphAddable)
                .OrderBy(descriptor => descriptor.CanonicalName, StringComparer.Ordinal)
                .ThenBy(descriptor => descriptor.FullTypeName, StringComparer.Ordinal)
                .ToArray();
        }

        private static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            if (assembly == null)
            {
                return Array.Empty<Type>();
            }

            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static bool IsSupportedNodeNamespace(Type type)
        {
            string namespaceName = type?.Namespace ?? string.Empty;
            return namespaceName.StartsWith("UnityEditor.ShaderGraph", StringComparison.Ordinal);
        }

        private static bool HasParameterlessConstructor(Type type)
        {
            if (type == null)
            {
                return false;
            }

            return type.GetConstructor(
                InstanceFlags,
                null,
                Type.EmptyTypes,
                null) != null;
        }

        private static string BuildCanonicalNodeName(Type type)
        {
            if (type == null)
            {
                return string.Empty;
            }

            string fullTypeName = type.FullName ?? type.Name;
            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                return "Float/Vector1";
            }

            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.ColorNode", StringComparison.Ordinal))
            {
                return "Color";
            }

            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal))
            {
                return "Split";
            }

            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal))
            {
                return "Append";
            }

            string shortName = type.Name ?? string.Empty;
            if (shortName.EndsWith("Node", StringComparison.Ordinal) && shortName.Length > "Node".Length)
            {
                return shortName.Substring(0, shortName.Length - "Node".Length);
            }

            return shortName;
        }

        private static IReadOnlyList<string> BuildNodeAliases(Type type, string canonicalName)
        {
            var aliases = new List<string>();

            void AddAlias(string alias)
            {
                if (string.IsNullOrWhiteSpace(alias))
                {
                    return;
                }

                if (aliases.Any(existing => string.Equals(existing, alias, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                aliases.Add(alias);
            }

            AddAlias(canonicalName);
            AddAlias(type?.Name);
            AddAlias(type?.FullName);

            string shortName = type?.Name ?? string.Empty;
            if (shortName.EndsWith("Node", StringComparison.Ordinal) && shortName.Length > "Node".Length)
            {
                AddAlias(shortName.Substring(0, shortName.Length - "Node".Length));
            }

            string fullTypeName = type?.FullName ?? string.Empty;
            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                AddAlias("Float");
                AddAlias("Vector1");
                AddAlias("Float/Vector1");
            }
            else if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.ColorNode", StringComparison.Ordinal))
            {
                AddAlias("Color");
            }
            else if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal))
            {
                AddAlias("Split");
            }
            else if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal))
            {
                AddAlias("Append");
            }

            return aliases;
        }

        private static string NormalizeNodeToken(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character))
                {
                    builder.Append(char.ToUpperInvariant(character));
                }
            }

            return builder.ToString();
        }

        private static bool TryGetNodeCatalogExclusionReason(Type nodeClassType, out string exclusionReason)
        {
            exclusionReason = null;
            if (nodeClassType == null)
            {
                exclusionReason = "Node type is null.";
                return true;
            }

            string fullTypeName = nodeClassType.FullName ?? nodeClassType.Name ?? string.Empty;
            string shortTypeName = nodeClassType.Name ?? string.Empty;

            if (nodeClassType.IsNested)
            {
                exclusionReason = "Nested internal node types are excluded from the initial graph-addable catalog.";
                return true;
            }

            if (fullTypeName.Contains(".Legacy.", StringComparison.Ordinal) ||
                shortTypeName.EndsWith("MasterNode1", StringComparison.Ordinal))
            {
                exclusionReason = "Legacy master node types are excluded from the safe addable catalog.";
                return true;
            }

            if (!shortTypeName.EndsWith("Node", StringComparison.Ordinal))
            {
                exclusionReason = "Types that do not follow the public *Node shape stay discoverable-only until explicitly validated.";
                return true;
            }

            if (string.Equals(shortTypeName, "PreviewNode", StringComparison.Ordinal) ||
                string.Equals(shortTypeName, "BlockNode", StringComparison.Ordinal) ||
                string.Equals(shortTypeName, "SubGraphOutputNode", StringComparison.Ordinal))
            {
                exclusionReason = "Preview, block-only, and output-only node types are excluded from the safe addable catalog.";
                return true;
            }

            if (string.Equals(shortTypeName, "RedirectNodeData", StringComparison.Ordinal) ||
                string.Equals(shortTypeName, "UnknownNodeType", StringComparison.Ordinal) ||
                fullTypeName.Contains("MultiJsonInternal+", StringComparison.Ordinal))
            {
                exclusionReason = "Serialization and redirect placeholder node types are excluded from the safe addable catalog.";
                return true;
            }

            return false;
        }

        private static bool TryProbeGraphAddableNode(
            Type nodeClassType,
            string canonicalNodeType,
            out string probeNote)
        {
            probeNote = null;

            if (nodeClassType == null)
            {
                probeNote = "Node type is null.";
                return false;
            }

            if (!TryCreateBlankGraphData(
                    "Assets/ShaderGraphMcpProbe/NodeCatalogProbe.shadergraph",
                    out object graphData,
                    null,
                    out string graphFailure))
            {
                probeNote = $"Probe graph creation failed: {graphFailure}";
                return false;
            }

            if (!TryCreateShaderNode(
                    nodeClassType,
                    canonicalNodeType,
                    out object shaderNode,
                    out string nodeFailure))
            {
                probeNote = $"Node instantiation failed: {nodeFailure}";
                return false;
            }

            if (!TryAssignVisibleNodeLayout(
                    graphData,
                    shaderNode,
                    canonicalNodeType,
                    out _,
                    out string layoutFailure))
            {
                probeNote = $"Node layout assignment failed: {layoutFailure}";
                return false;
            }

            if (!TryInvokeGraphAddNode(graphData, shaderNode, out string addNodeFailure))
            {
                probeNote = $"GraphData.AddNode(...) failed: {addNodeFailure}";
                return false;
            }

            if (!TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                probeNote = $"GraphData.ValidateGraph() failed: {validateFailure}";
                return false;
            }

            probeNote = "Activator -> AddNode -> ValidateGraph succeeded.";
            return true;
        }

        internal static IReadOnlyList<string> GetSupportedNodeCatalogReportLines()
        {
            return GetSupportedNodeCatalog()
                .Select(descriptor =>
                    $"{descriptor.Label} | status: {descriptor.CatalogStatus} | aliases: {string.Join(", ", descriptor.Aliases)} | note: {descriptor.CatalogNote}")
                .ToArray();
        }

        internal static int GetDiscoveredNodeCatalogCount()
        {
            return GetDiscoveredNodeCatalog().Count;
        }

        internal static int GetSupportedNodeCatalogCount()
        {
            return GetSupportedNodeCatalog().Count;
        }

        internal static int GetProbeRejectedNodeCatalogCount()
        {
            return GetDiscoveredNodeCatalog().Count(descriptor =>
                string.Equals(descriptor.CatalogStatus, "probe-failed", StringComparison.Ordinal));
        }

        internal static int GetExcludedNodeCatalogBucketCount()
        {
            return GetExcludedNodeCatalogBucketReportLines().Count;
        }

        internal static int GetProbeRejectedNodeCatalogBucketCount()
        {
            return GetProbeRejectedNodeCatalogBucketReportLines().Count;
        }

        internal static IReadOnlyList<string> GetDiscoveredNodeCatalogReportLines()
        {
            return GetDiscoveredNodeCatalog()
                .Select(descriptor =>
                    $"{descriptor.Label} | status: {descriptor.CatalogStatus} | aliases: {string.Join(", ", descriptor.Aliases)} | note: {descriptor.CatalogNote}")
                .ToArray();
        }

        internal static IReadOnlyList<string> GetExcludedNodeCatalogReportLines()
        {
            return GetDiscoveredNodeCatalog()
                .Where(descriptor => string.Equals(descriptor.CatalogStatus, "filtered", StringComparison.Ordinal))
                .Select(descriptor => $"{descriptor.Label} | excluded: {descriptor.CatalogNote}")
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToArray();
        }

        internal static IReadOnlyList<string> GetExcludedNodeCatalogBucketReportLines()
        {
            return GetNodeCatalogBucketReportLines("filtered");
        }

        internal static IReadOnlyList<string> GetProbeRejectedNodeCatalogReportLines()
        {
            return GetDiscoveredNodeCatalog()
                .Where(descriptor => string.Equals(descriptor.CatalogStatus, "probe-failed", StringComparison.Ordinal))
                .Select(descriptor => $"{descriptor.Label} | rejected: {descriptor.CatalogNote}")
                .OrderBy(line => line, StringComparer.Ordinal)
                .ToArray();
        }

        internal static IReadOnlyList<string> GetProbeRejectedNodeCatalogBucketReportLines()
        {
            return GetNodeCatalogBucketReportLines("probe-failed");
        }

        internal static string ClassifyNodeCatalogDiagnosticBucket(string catalogStatus, string catalogNote)
        {
            string status = catalogStatus?.Trim() ?? string.Empty;
            string note = catalogNote?.Trim() ?? string.Empty;
            if (string.Equals(status, "filtered", StringComparison.Ordinal))
            {
                if (note.StartsWith("Nested internal node types", StringComparison.Ordinal))
                {
                    return "filtered:nested-internal";
                }

                if (note.StartsWith("Legacy master node types", StringComparison.Ordinal))
                {
                    return "filtered:legacy-master";
                }

                if (note.StartsWith("Types that do not follow the public *Node shape", StringComparison.Ordinal))
                {
                    return "filtered:non-public-node-shape";
                }

                if (note.StartsWith("Preview, block-only, and output-only node types", StringComparison.Ordinal))
                {
                    return "filtered:preview-block-output";
                }

                if (note.StartsWith("Serialization and redirect placeholder node types", StringComparison.Ordinal))
                {
                    return "filtered:serialization-placeholder";
                }

                return "filtered:other";
            }

            if (string.Equals(status, "probe-failed", StringComparison.Ordinal))
            {
                if (note.StartsWith("Probe graph creation failed:", StringComparison.Ordinal))
                {
                    return "probe:graph-create";
                }

                if (note.StartsWith("Node instantiation failed:", StringComparison.Ordinal))
                {
                    return "probe:instantiation";
                }

                if (note.StartsWith("Node layout assignment failed:", StringComparison.Ordinal))
                {
                    return "probe:layout";
                }

                if (note.StartsWith("GraphData.AddNode(...) failed:", StringComparison.Ordinal))
                {
                    return "probe:add-node";
                }

                if (note.StartsWith("GraphData.ValidateGraph() failed:", StringComparison.Ordinal))
                {
                    return "probe:validate-graph";
                }

                if (note.StartsWith("Node type is null.", StringComparison.Ordinal))
                {
                    return "probe:null-type";
                }

                return "probe:other";
            }

            if (string.Equals(status, "graph-addable", StringComparison.Ordinal))
            {
                return "supported:graph-addable";
            }

            return "status:unknown";
        }

        private static IReadOnlyList<string> GetNodeCatalogBucketReportLines(string catalogStatus)
        {
            return GetDiscoveredNodeCatalog()
                .Where(descriptor => string.Equals(descriptor.CatalogStatus, catalogStatus, StringComparison.Ordinal))
                .GroupBy(descriptor => ClassifyNodeCatalogDiagnosticBucket(descriptor.CatalogStatus, descriptor.CatalogNote))
                .OrderByDescending(group => group.Count())
                .ThenBy(group => group.Key, StringComparer.Ordinal)
                .Select(group => $"{group.Key} | count: {group.Count()}")
                .ToArray();
        }

        private static Vector2 BuildSuggestedNodeOrigin(object graphData, string canonicalNodeType)
        {
            IReadOnlyList<object> nodes = EnumerateMember(graphData, "nodes");
            string canonicalType = canonicalNodeType?.Trim() ?? string.Empty;

            float baseX;
            float baseY;
            float stepX = 280f;
            float stepY = 180f;

            if (string.Equals(canonicalType, "Color", StringComparison.OrdinalIgnoreCase))
            {
                int typeIndex = CountNodesOfType(nodes, "UnityEditor.ShaderGraph.ColorNode");
                baseX = -620f;
                baseY = -120f;
                return new Vector2(baseX + (typeIndex * stepX), baseY);
            }

            if (string.Equals(canonicalType, "Split", StringComparison.OrdinalIgnoreCase))
            {
                int typeIndex = CountNodesOfType(nodes, "UnityEditor.ShaderGraph.SplitNode");
                baseX = -260f;
                baseY = -120f;
                return new Vector2(baseX + (typeIndex * stepX), baseY);
            }

            if (string.Equals(canonicalType, "Float/Vector1", StringComparison.OrdinalIgnoreCase))
            {
                int typeIndex = CountNodesOfType(nodes, "UnityEditor.ShaderGraph.Vector1Node");
                baseX = -620f;
                baseY = 140f;
                return new Vector2(baseX + (typeIndex * stepX), baseY);
            }

            int totalIndex = nodes.Count;
            return new Vector2(-620f + ((totalIndex % 3) * stepX), -120f + ((totalIndex / 3) * stepY));
        }

        private static int CountNodesOfType(IReadOnlyList<object> nodes, string typeName)
        {
            if (nodes == null || string.IsNullOrWhiteSpace(typeName))
            {
                return 0;
            }

            int count = 0;
            for (int i = 0; i < nodes.Count; i += 1)
            {
                object node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                string currentTypeName = node.GetType().FullName ?? node.GetType().Name;
                if (string.Equals(currentTypeName, typeName, StringComparison.Ordinal))
                {
                    count += 1;
                }
            }

            return count;
        }

        private static bool TryResolveGraphNodeByObjectId(
            object graphData,
            string requestedNodeId,
            out object resolvedNode,
            out string failureReason)
        {
            resolvedNode = null;
            failureReason = null;

            string normalizedNodeId = (requestedNodeId ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedNodeId))
            {
                failureReason =
                    "Node id is required. ConnectPorts expects exact GraphData objectId values from add_node or read_graph_summary.";
                return false;
            }

            IReadOnlyList<object> nodes = EnumerateMember(graphData, "nodes");
            foreach (object node in nodes)
            {
                string nodeId = GetStringProperty(node, "objectId");
                if (string.Equals(nodeId, normalizedNodeId, StringComparison.Ordinal))
                {
                    resolvedNode = node;
                    return true;
                }
            }

            failureReason =
                $"Could not find a graph node with objectId '{normalizedNodeId}'. ConnectPorts requires exact node ids from add_node or read_graph_summary, not display names.";
            return false;
        }

        private static bool TryResolveSupportedConnectionEndpoint(
            object node,
            string requestedPort,
            bool isOutput,
            out int slotId,
            out string canonicalPort,
            out string failureReason)
        {
            slotId = -1;
            canonicalPort = string.Empty;
            failureReason = null;

            if (node == null)
            {
                failureReason = "Resolved graph node is null.";
                return false;
            }

            string nodeTypeName = node.GetType().FullName ?? node.GetType().Name;
            string normalizedPort = (requestedPort ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalizedPort))
            {
                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Port value is required."
                );
                return false;
            }

            if (isOutput)
            {
                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "0", "Out"))
                    {
                        slotId = 0;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 0, Out on Vector1Node."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ColorNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "0", "Out"))
                    {
                        slotId = 0;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 0, Out on ColorNode."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector2Node", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "0", "Out"))
                    {
                        slotId = 0;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 0, Out on Vector2Node."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector3Node", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "0", "Out"))
                    {
                        slotId = 0;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 0, Out on Vector3Node."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector4Node", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "0", "Out"))
                    {
                        slotId = 0;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 0, Out on Vector4Node."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "1", "R"))
                    {
                        slotId = 1;
                        canonicalPort = "R";
                        return true;
                    }

                    if (IsPortAlias(normalizedPort, "2", "G"))
                    {
                        slotId = 2;
                        canonicalPort = "G";
                        return true;
                    }

                    if (IsPortAlias(normalizedPort, "3", "B"))
                    {
                        slotId = 3;
                        canonicalPort = "B";
                        return true;
                    }

                    if (IsPortAlias(normalizedPort, "4", "A"))
                    {
                        slotId = 4;
                        canonicalPort = "A";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 1/R, 2/G, 3/B, 4/A on SplitNode."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CombineNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "4", "RGBA"))
                    {
                        slotId = 4;
                        canonicalPort = "RGBA";
                        return true;
                    }

                    if (IsPortAlias(normalizedPort, "5", "RGB"))
                    {
                        slotId = 5;
                        canonicalPort = "RGB";
                        return true;
                    }

                    if (IsPortAlias(normalizedPort, "6", "RG"))
                    {
                        slotId = 6;
                        canonicalPort = "RG";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 4/RGBA, 5/RGB, 6/RG on CombineNode."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "2", "Out"))
                    {
                        slotId = 2;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 2, Out on AppendVectorNode."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ComparisonNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "2", "Out"))
                    {
                        slotId = 2;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 2, Out on ComparisonNode."
                    );
                    return false;
                }

                if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal))
                {
                    if (IsPortAlias(normalizedPort, "3", "Out"))
                    {
                        slotId = 3;
                        canonicalPort = "Out";
                        return true;
                    }

                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        "Supported output ports: 3, Out on BranchNode."
                    );
                    return false;
                }

                if (TryResolveArithmeticConnectionPort(
                        nodeTypeName,
                        normalizedPort,
                        isOutput,
                        out slotId,
                        out canonicalPort,
                        out string arithmeticSupportMessage))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(arithmeticSupportMessage))
                {
                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        arithmeticSupportMessage
                    );
                    return false;
                }

                if (TryResolveLogicConnectionPort(
                        nodeTypeName,
                        normalizedPort,
                        isOutput,
                        out slotId,
                        out canonicalPort,
                        out string logicOutputSupportMessage))
                {
                    return true;
                }

                if (!string.IsNullOrWhiteSpace(logicOutputSupportMessage))
                {
                    failureReason = BuildConnectionPortFailure(
                        node,
                        isOutput,
                        normalizedPort,
                        logicOutputSupportMessage
                    );
                    return false;
                }

                failureReason = BuildConnectionNodeFailure(
                    node,
                    isOutput,
                    "UnityEditor.ShaderGraph.Vector1Node",
                    "UnityEditor.ShaderGraph.ColorNode",
                    "UnityEditor.ShaderGraph.Vector2Node",
                    "UnityEditor.ShaderGraph.Vector3Node",
                    "UnityEditor.ShaderGraph.Vector4Node",
                    "UnityEditor.ShaderGraph.SplitNode",
                    "UnityEditor.ShaderGraph.CombineNode",
                    "UnityEditor.ShaderGraph.AppendVectorNode",
                    "UnityEditor.ShaderGraph.AddNode",
                    "UnityEditor.ShaderGraph.SubtractNode",
                    "UnityEditor.ShaderGraph.MultiplyNode",
                    "UnityEditor.ShaderGraph.DivideNode",
                    "UnityEditor.ShaderGraph.PowerNode",
                    "UnityEditor.ShaderGraph.MinimumNode",
                    "UnityEditor.ShaderGraph.MaximumNode",
                    "UnityEditor.ShaderGraph.ModuloNode",
                    "UnityEditor.ShaderGraph.LerpNode",
                    "UnityEditor.ShaderGraph.SmoothstepNode",
                    "UnityEditor.ShaderGraph.ClampNode",
                    "UnityEditor.ShaderGraph.StepNode",
                    "UnityEditor.ShaderGraph.AbsoluteNode",
                    "UnityEditor.ShaderGraph.FloorNode",
                    "UnityEditor.ShaderGraph.CeilingNode",
                    "UnityEditor.ShaderGraph.RoundNode",
                    "UnityEditor.ShaderGraph.SignNode",
                    "UnityEditor.ShaderGraph.SineNode",
                    "UnityEditor.ShaderGraph.CosineNode",
                    "UnityEditor.ShaderGraph.TangentNode",
                    "UnityEditor.ShaderGraph.NegateNode",
                    "UnityEditor.ShaderGraph.ReciprocalNode",
                    "UnityEditor.ShaderGraph.SquareRootNode",
                    "UnityEditor.ShaderGraph.FractionNode",
                    "UnityEditor.ShaderGraph.TruncateNode",
                    "UnityEditor.ShaderGraph.SaturateNode",
                    "UnityEditor.ShaderGraph.OneMinusNode",
                    "UnityEditor.ShaderGraph.ComparisonNode",
                    "UnityEditor.ShaderGraph.BranchNode"
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ComparisonNode", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "0", "A"))
                {
                    slotId = 0;
                    canonicalPort = "A";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "1", "B"))
                {
                    slotId = 1;
                    canonicalPort = "B";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 0/A, 1/B on ComparisonNode."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "0", "Predicate"))
                {
                    slotId = 0;
                    canonicalPort = "Predicate";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "1", "True"))
                {
                    slotId = 1;
                    canonicalPort = "True";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "2", "False"))
                {
                    slotId = 2;
                    canonicalPort = "False";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 0/Predicate, 1/True, 2/False on BranchNode."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "1", "X"))
                {
                    slotId = 1;
                    canonicalPort = "X";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 1, X."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector2Node", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "1", "X"))
                {
                    slotId = 1;
                    canonicalPort = "X";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "2", "Y"))
                {
                    slotId = 2;
                    canonicalPort = "Y";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 1/X, 2/Y on Vector2Node."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector3Node", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "1", "X"))
                {
                    slotId = 1;
                    canonicalPort = "X";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "2", "Y"))
                {
                    slotId = 2;
                    canonicalPort = "Y";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "3", "Z"))
                {
                    slotId = 3;
                    canonicalPort = "Z";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 1/X, 2/Y, 3/Z on Vector3Node."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector4Node", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "1", "X"))
                {
                    slotId = 1;
                    canonicalPort = "X";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "2", "Y"))
                {
                    slotId = 2;
                    canonicalPort = "Y";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "3", "Z"))
                {
                    slotId = 3;
                    canonicalPort = "Z";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "4", "W"))
                {
                    slotId = 4;
                    canonicalPort = "W";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 1/X, 2/Y, 3/Z, 4/W on Vector4Node."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "0", "In"))
                {
                    slotId = 0;
                    canonicalPort = "In";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 0, In on SplitNode."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CombineNode", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "0", "R"))
                {
                    slotId = 0;
                    canonicalPort = "R";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "1", "G"))
                {
                    slotId = 1;
                    canonicalPort = "G";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "2", "B"))
                {
                    slotId = 2;
                    canonicalPort = "B";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "3", "A"))
                {
                    slotId = 3;
                    canonicalPort = "A";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 0/R, 1/G, 2/B, 3/A on CombineNode."
                );
                return false;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal))
            {
                if (IsPortAlias(normalizedPort, "0", "A"))
                {
                    slotId = 0;
                    canonicalPort = "A";
                    return true;
                }

                if (IsPortAlias(normalizedPort, "1", "B"))
                {
                    slotId = 1;
                    canonicalPort = "B";
                    return true;
                }

                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    "Supported input ports: 0/A, 1/B on AppendVectorNode."
                );
                return false;
            }

            if (TryResolveArithmeticConnectionPort(
                    nodeTypeName,
                    normalizedPort,
                    isOutput,
                    out slotId,
                    out canonicalPort,
                    out string arithmeticInputSupportMessage))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(arithmeticInputSupportMessage))
            {
                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    arithmeticInputSupportMessage
                );
                return false;
            }

            if (TryResolveLogicConnectionPort(
                    nodeTypeName,
                    normalizedPort,
                    isOutput,
                    out slotId,
                    out canonicalPort,
                    out string logicInputSupportMessage))
            {
                return true;
            }

            if (!string.IsNullOrWhiteSpace(logicInputSupportMessage))
            {
                failureReason = BuildConnectionPortFailure(
                    node,
                    isOutput,
                    normalizedPort,
                    logicInputSupportMessage
                );
                return false;
            }

            failureReason = BuildConnectionNodeFailure(
                node,
                isOutput,
                "UnityEditor.ShaderGraph.Vector1Node",
                "UnityEditor.ShaderGraph.Vector2Node",
                "UnityEditor.ShaderGraph.Vector3Node",
                "UnityEditor.ShaderGraph.Vector4Node",
                "UnityEditor.ShaderGraph.SplitNode",
                "UnityEditor.ShaderGraph.CombineNode",
                "UnityEditor.ShaderGraph.AppendVectorNode",
                "UnityEditor.ShaderGraph.AddNode",
                "UnityEditor.ShaderGraph.SubtractNode",
                "UnityEditor.ShaderGraph.MultiplyNode",
                "UnityEditor.ShaderGraph.DivideNode",
                "UnityEditor.ShaderGraph.PowerNode",
                "UnityEditor.ShaderGraph.MinimumNode",
                "UnityEditor.ShaderGraph.MaximumNode",
                "UnityEditor.ShaderGraph.ModuloNode",
                "UnityEditor.ShaderGraph.LerpNode",
                "UnityEditor.ShaderGraph.SmoothstepNode",
                "UnityEditor.ShaderGraph.ClampNode",
                "UnityEditor.ShaderGraph.StepNode",
                "UnityEditor.ShaderGraph.AbsoluteNode",
                "UnityEditor.ShaderGraph.FloorNode",
                "UnityEditor.ShaderGraph.CeilingNode",
                "UnityEditor.ShaderGraph.RoundNode",
                "UnityEditor.ShaderGraph.SignNode",
                "UnityEditor.ShaderGraph.SineNode",
                "UnityEditor.ShaderGraph.CosineNode",
                "UnityEditor.ShaderGraph.TangentNode",
                "UnityEditor.ShaderGraph.NegateNode",
                "UnityEditor.ShaderGraph.ReciprocalNode",
                "UnityEditor.ShaderGraph.SquareRootNode",
                "UnityEditor.ShaderGraph.FractionNode",
                "UnityEditor.ShaderGraph.TruncateNode",
                "UnityEditor.ShaderGraph.SaturateNode",
                "UnityEditor.ShaderGraph.OneMinusNode",
                "UnityEditor.ShaderGraph.ComparisonNode",
                "UnityEditor.ShaderGraph.BranchNode"
            );
            return false;
        }

        private static bool TryResolveArithmeticConnectionPort(
            string nodeTypeName,
            string requestedPort,
            bool isOutput,
            out int slotId,
            out string canonicalPort,
            out string supportMessage)
        {
            slotId = -1;
            canonicalPort = string.Empty;
            supportMessage = null;

            switch (nodeTypeName)
            {
                case "UnityEditor.ShaderGraph.AddNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on AddNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on AddNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.SubtractNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on SubtractNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on SubtractNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.MultiplyNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on MultiplyNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on MultiplyNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.DivideNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on DivideNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on DivideNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.PowerNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on PowerNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on PowerNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.MinimumNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on MinimumNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on MinimumNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.MaximumNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on MaximumNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on MaximumNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.ModuloNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on ModuloNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on ModuloNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.LerpNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 3, Out on LerpNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (3, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B, 2/T on LerpNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"), (2, "T"));

                case "UnityEditor.ShaderGraph.SmoothstepNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 3, Out on SmoothstepNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (3, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/Edge1, 1/Edge2, 2/In on SmoothstepNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "Edge1"), (1, "Edge2"), (2, "In"));

                case "UnityEditor.ShaderGraph.ClampNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 3, Out on ClampNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (3, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In, 1/Min, 2/Max on ClampNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"), (1, "Min"), (2, "Max"));

                case "UnityEditor.ShaderGraph.StepNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on StepNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/Edge, 1/In on StepNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "Edge"), (1, "In"));

                case "UnityEditor.ShaderGraph.AbsoluteNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on AbsoluteNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on AbsoluteNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.FloorNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on FloorNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on FloorNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.CeilingNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on CeilingNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on CeilingNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.RoundNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on RoundNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on RoundNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.SignNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on SignNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on SignNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.SineNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on SineNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on SineNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.CosineNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on CosineNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on CosineNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.TangentNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on TangentNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on TangentNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.NegateNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on NegateNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on NegateNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.ReciprocalNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on ReciprocalNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on ReciprocalNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.SquareRootNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on SquareRootNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on SquareRootNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.FractionNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on FractionNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on FractionNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.TruncateNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on TruncateNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on TruncateNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.SaturateNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on SaturateNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on SaturateNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                case "UnityEditor.ShaderGraph.OneMinusNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 1, Out on OneMinusNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (1, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/In on OneMinusNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "In"));

                default:
                    supportMessage = null;
                    return false;
            }
        }

        private static bool TryResolveLogicConnectionPort(
            string nodeTypeName,
            string requestedPort,
            bool isOutput,
            out int slotId,
            out string canonicalPort,
            out string supportMessage)
        {
            slotId = -1;
            canonicalPort = string.Empty;
            supportMessage = null;

            switch (nodeTypeName)
            {
                case "UnityEditor.ShaderGraph.ComparisonNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 2, Out on ComparisonNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (2, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/A, 1/B on ComparisonNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "A"), (1, "B"));

                case "UnityEditor.ShaderGraph.BranchNode":
                    if (isOutput)
                    {
                        supportMessage = "Supported output ports: 3, Out on BranchNode.";
                        return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (3, "Out"));
                    }

                    supportMessage = "Supported input ports: 0/Predicate, 1/True, 2/False on BranchNode.";
                    return TryResolvePortAlias(requestedPort, out slotId, out canonicalPort, (0, "Predicate"), (1, "True"), (2, "False"));

                default:
                    supportMessage = null;
                    return false;
            }
        }

        private static bool TryResolvePortAlias(
            string requestedPort,
            out int slotId,
            out string canonicalPort,
            params (int SlotId, string DisplayPort)[] aliases)
        {
            slotId = -1;
            canonicalPort = string.Empty;

            if (aliases == null)
            {
                return false;
            }

            for (int index = 0; index < aliases.Length; index += 1)
            {
                (int aliasSlotId, string aliasDisplayPort) = aliases[index];
                if (!IsPortAlias(requestedPort, aliasSlotId.ToString(CultureInfo.InvariantCulture), aliasDisplayPort))
                {
                    continue;
                }

                slotId = aliasSlotId;
                canonicalPort = aliasDisplayPort;
                return true;
            }

            return false;
        }

        private static bool TryValidateSupportedConnectionPair(
            object outputNode,
            object inputNode,
            string canonicalOutputPort,
            string canonicalInputPort,
            out string failureReason)
        {
            failureReason = null;

            string outputNodeTypeName = GetTypeName(outputNode);
            string inputNodeTypeName = GetTypeName(inputNode);

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.ColorNode", StringComparison.Ordinal) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedSplitVectorSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal) &&
                string.Equals(canonicalInputPort, "In", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedColorValueSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal) &&
                string.Equals(canonicalInputPort, "In", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedScalarBuilderSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                IsSupportedVectorBuilderInputPort(inputNodeTypeName, canonicalInputPort))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal) &&
                IsSupportedArithmeticConnectionNodeType(inputNodeTypeName))
            {
                return true;
            }

            if (IsSupportedArithmeticConnectionNodeType(outputNodeTypeName) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedArithmeticConnectionNodeType(outputNodeTypeName) &&
                IsSupportedArithmeticConnectionNodeType(inputNodeTypeName))
            {
                return true;
            }

            if (IsSupportedScalarValueSourceNodeType(outputNodeTypeName) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.ComparisonNode", StringComparison.Ordinal) &&
                (string.Equals(canonicalInputPort, "A", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "B", StringComparison.Ordinal)))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.ComparisonNode", StringComparison.Ordinal) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal) &&
                string.Equals(canonicalInputPort, "Predicate", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedScalarValueSourceNodeType(outputNodeTypeName) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                (string.Equals(canonicalInputPort, "True", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "False", StringComparison.Ordinal)))
            {
                return true;
            }

            if (IsSupportedColorValueSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                (string.Equals(canonicalInputPort, "True", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "False", StringComparison.Ordinal)))
            {
                return true;
            }

            if (IsSupportedColorValueSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.MultiplyNode", StringComparison.Ordinal) &&
                (string.Equals(canonicalInputPort, "A", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "B", StringComparison.Ordinal)))
            {
                return true;
            }

            if (IsSupportedColorValueSourceOutput(outputNodeTypeName, canonicalOutputPort) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.LerpNode", StringComparison.Ordinal) &&
                (string.Equals(canonicalInputPort, "A", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "B", StringComparison.Ordinal) ||
                 string.Equals(canonicalInputPort, "T", StringComparison.Ordinal)))
            {
                return true;
            }

            if (string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal) &&
                IsSupportedAppendInputPort(canonicalInputPort) &&
                (IsSupportedColorValueSourceOutput(outputNodeTypeName, canonicalOutputPort) ||
                 IsSupportedScalarBuilderSourceOutput(outputNodeTypeName, canonicalOutputPort)))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal) &&
                string.Equals(inputNodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(outputNodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal) &&
                IsSupportedArithmeticConnectionNodeType(inputNodeTypeName))
            {
                return true;
            }

            failureReason =
                $"Unsupported connection pair '{outputNodeTypeName}.{canonicalOutputPort}' -> '{inputNodeTypeName}.{canonicalInputPort}'. " +
                "Current package-backed path supports Vector1 -> Vector1, Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Split, Split -> Vector1, scalar component outputs -> Combine or Vector2/Vector3/Vector4 inputs, Vector1 -> arithmetic inputs, arithmetic outputs -> Vector1, arithmetic outputs -> arithmetic inputs, scalar outputs -> Comparison A/B, Comparison Out -> Branch Predicate, scalar outputs -> Branch True/False, Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append -> Multiply A/B, Branch True/False, or Lerp A/B/T, Color/Combine RGBA/Vector4/Multiply/Branch/Lerp/Append plus Vector1/Split/scalar arithmetic -> Append A/B, and Branch Out -> Vector1 or arithmetic inputs.";
            return false;
        }

        private static bool IsSupportedArithmeticConnectionNodeType(string nodeTypeName)
        {
            return string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.AddNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SubtractNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.MultiplyNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.DivideNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.PowerNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.MinimumNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.MaximumNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ModuloNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.LerpNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SmoothstepNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ClampNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.StepNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.AbsoluteNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.FloorNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CeilingNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.RoundNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SignNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SineNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CosineNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.TangentNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.NegateNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ReciprocalNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SquareRootNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.FractionNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.TruncateNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SaturateNode", StringComparison.Ordinal) ||
                   string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.OneMinusNode", StringComparison.Ordinal);
        }

        private static bool IsSupportedScalarValueSourceNodeType(string nodeTypeName)
        {
            return string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal) ||
                   IsSupportedArithmeticConnectionNodeType(nodeTypeName);
        }

        private static bool IsSupportedScalarBuilderSourceOutput(string nodeTypeName, string canonicalOutputPort)
        {
            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector1Node", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            if (IsSupportedArithmeticConnectionNodeType(nodeTypeName) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            return string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.SplitNode", StringComparison.Ordinal) &&
                   (string.Equals(canonicalOutputPort, "R", StringComparison.Ordinal) ||
                    string.Equals(canonicalOutputPort, "G", StringComparison.Ordinal) ||
                    string.Equals(canonicalOutputPort, "B", StringComparison.Ordinal) ||
                    string.Equals(canonicalOutputPort, "A", StringComparison.Ordinal));
        }

        private static bool IsSupportedVectorBuilderInputPort(string nodeTypeName, string canonicalInputPort)
        {
            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CombineNode", StringComparison.Ordinal))
            {
                return string.Equals(canonicalInputPort, "R", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "G", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "B", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "A", StringComparison.Ordinal);
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector2Node", StringComparison.Ordinal))
            {
                return string.Equals(canonicalInputPort, "X", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "Y", StringComparison.Ordinal);
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector3Node", StringComparison.Ordinal))
            {
                return string.Equals(canonicalInputPort, "X", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "Y", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "Z", StringComparison.Ordinal);
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector4Node", StringComparison.Ordinal))
            {
                return string.Equals(canonicalInputPort, "X", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "Y", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "Z", StringComparison.Ordinal) ||
                       string.Equals(canonicalInputPort, "W", StringComparison.Ordinal);
            }

            return false;
        }

        private static bool IsSupportedAppendInputPort(string canonicalInputPort)
        {
            return string.Equals(canonicalInputPort, "A", StringComparison.Ordinal) ||
                   string.Equals(canonicalInputPort, "B", StringComparison.Ordinal);
        }

        private static bool IsSupportedSplitVectorSourceOutput(string nodeTypeName, string canonicalOutputPort)
        {
            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.ColorNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.Vector4Node", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            return string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.CombineNode", StringComparison.Ordinal) &&
                   string.Equals(canonicalOutputPort, "RGBA", StringComparison.Ordinal);
        }

        private static bool IsSupportedColorValueSourceOutput(string nodeTypeName, string canonicalOutputPort)
        {
            if (IsSupportedSplitVectorSourceOutput(nodeTypeName, canonicalOutputPort))
            {
                return true;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.MultiplyNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.BranchNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            if (string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.LerpNode", StringComparison.Ordinal) &&
                string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal))
            {
                return true;
            }

            return string.Equals(nodeTypeName, "UnityEditor.ShaderGraph.AppendVectorNode", StringComparison.Ordinal) &&
                   string.Equals(canonicalOutputPort, "Out", StringComparison.Ordinal);
        }

        private static bool TryCreateSlotReference(
            object node,
            int slotId,
            out object slotReference,
            out string failureReason)
        {
            slotReference = null;
            failureReason = null;

            if (node == null)
            {
                failureReason = "Node is null.";
                return false;
            }

            Type slotReferenceType = ResolveType("UnityEditor.Graphing.SlotReference");
            if (slotReferenceType == null)
            {
                failureReason = "Could not resolve UnityEditor.Graphing.SlotReference.";
                return false;
            }

            try
            {
                slotReference = Activator.CreateInstance(
                    slotReferenceType,
                    InstanceFlags,
                    null,
                    new object[] { node, slotId },
                    CultureInfo.InvariantCulture);
                return true;
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to create SlotReference for slot {slotId}: {GetRootMessage(ex)}";
                return false;
            }
        }

        private static bool TryInvokeGraphConnect(
            object graphData,
            object outputSlotRef,
            object inputSlotRef,
            out object connectedEdge,
            out string failureReason)
        {
            connectedEdge = null;
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (outputSlotRef == null || inputSlotRef == null)
            {
                failureReason = "Both slot references are required.";
                return false;
            }

            MethodInfo connectMethod = FindMethod(graphData.GetType(), "Connect", 2);
            if (connectMethod == null)
            {
                failureReason = $"Method 'Connect' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                connectedEdge = connectMethod.Invoke(graphData, new[] { outputSlotRef, inputSlotRef });
                if (connectedEdge == null)
                {
                    failureReason =
                        "GraphData.Connect returned null. The selected nodes and ports may be incompatible or the connection would create a cycle.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryCreateBlankGraphData(
            string assetPath,
            out object graphData,
            IList<string> notes,
            out string failureReason)
        {
            graphData = null;
            failureReason = null;

            Type graphType = ResolveType(GraphDataTypeName);
            if (graphType == null)
            {
                failureReason = $"Could not resolve {GraphDataTypeName}.";
                return false;
            }

            try
            {
                graphData = Activator.CreateInstance(graphType, true);
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to create {GraphDataTypeName}: {GetRootMessage(ex)}";
                return false;
            }

            SetMemberValue(graphData, "messageManager", CreateMessageManagerInstance());
            SetMemberValue(graphData, "path", DefaultGraphPathLabel);
            notes?.Add($"GraphData.path set to '{DefaultGraphPathLabel}'.");

            if (!TryInvokeInstanceMethod(graphData, "AddContexts", out string addContextsFailure))
            {
                failureReason = $"Unable to invoke GraphData.AddContexts(): {addContextsFailure}";
                return false;
            }

            notes?.Add("GraphData.AddContexts() invoked successfully.");

            if (!TryInvokeGraphInitializeOutputs(graphData, out string initializeOutputsFailure))
            {
                failureReason = $"Unable to invoke GraphData.InitializeOutputs(null, null): {initializeOutputsFailure}";
                return false;
            }

            notes?.Add("GraphData.InitializeOutputs(null, null) invoked successfully.");

            if (!TryInvokeGraphAddDefaultCategory(graphData, out string addCategoryFailure))
            {
                failureReason = $"Unable to add the default Shader Graph category: {addCategoryFailure}";
                return false;
            }

            notes?.Add("GraphData.AddCategory(CategoryData.DefaultCategory()) invoked successfully.");

            if (TryInvokeInstanceMethod(graphData, "ValidateGraph", out string validateFailure))
            {
                notes?.Add("GraphData.ValidateGraph() invoked successfully during create_graph.");
            }
            else
            {
                notes?.Add($"GraphData.ValidateGraph() could not be invoked during create_graph: {validateFailure}");
            }

            return true;
        }

        private static bool TryInvokeGraphInitializeOutputs(object graphData, out string failureReason)
        {
            failureReason = null;
            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            MethodInfo initializeOutputsMethod = FindMethod(graphData.GetType(), "InitializeOutputs", 2);
            if (initializeOutputsMethod == null)
            {
                failureReason = $"Method 'InitializeOutputs' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                initializeOutputsMethod.Invoke(graphData, new object[] { null, null });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryInvokeGraphAddDefaultCategory(object graphData, out string failureReason)
        {
            failureReason = null;
            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            Type categoryDataType = ResolveType(CategoryDataTypeName);
            if (categoryDataType == null)
            {
                failureReason = $"Could not resolve {CategoryDataTypeName}.";
                return false;
            }

            MethodInfo defaultCategoryMethod = FindMethod(categoryDataType, "DefaultCategory", 1)
                ?? FindMethod(categoryDataType, "DefaultCategory", 0);
            if (defaultCategoryMethod == null)
            {
                failureReason = $"{CategoryDataTypeName}.DefaultCategory(...) was not found.";
                return false;
            }

            MethodInfo addCategoryMethod = FindMethod(graphData.GetType(), "AddCategory", 1);
            if (addCategoryMethod == null)
            {
                failureReason = $"Method 'AddCategory' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                object defaultCategory = defaultCategoryMethod.GetParameters().Length == 0
                    ? defaultCategoryMethod.Invoke(null, Array.Empty<object>())
                    : defaultCategoryMethod.Invoke(null, new object[] { null });
                addCategoryMethod.Invoke(graphData, new[] { defaultCategory });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool IsPortAlias(string requestedPort, string numericPort, string displayPort)
        {
            return string.Equals(requestedPort, numericPort, StringComparison.Ordinal) ||
                   string.Equals(requestedPort, displayPort, StringComparison.OrdinalIgnoreCase);
        }

        private static string BuildConnectionNodeFailure(
            object node,
            bool isOutput,
            params string[] supportedNodeTypes)
        {
            string role = isOutput ? "output" : "input";
            string nodeTypeName = GetTypeName(node);
            return $"Unsupported {role} node type '{nodeTypeName}'. Supported {role} node types in the current release matrix: {string.Join(", ", supportedNodeTypes ?? Array.Empty<string>())}. Use exact GraphData objectIds from add_node or read_graph_summary.";
        }

        private static string BuildConnectionPortFailure(
            object node,
            bool isOutput,
            string requestedPort,
            string supportMessage)
        {
            string role = isOutput ? "output" : "input";
            return $"Unsupported {role} port '{requestedPort}' on node '{GetTypeName(node)}'. {supportMessage} Ports are matched using the canonical aliases reported by read_graph_summary.";
        }

        private static bool TryInvokeGraphAddNode(object graphData, object node, out string failureReason)
        {
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (node == null)
            {
                failureReason = "Node instance is null.";
                return false;
            }

            MethodInfo addNodeMethod = FindMethod(graphData.GetType(), "AddNode", 1);
            if (addNodeMethod == null)
            {
                failureReason = $"Method 'AddNode' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                addNodeMethod.Invoke(graphData, new[] { node });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryInvokeGraphRemoveNode(object graphData, object node, out string failureReason)
        {
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (node == null)
            {
                failureReason = "Node instance is null.";
                return false;
            }

            MethodInfo removeNodeMethod = FindMethod(graphData.GetType(), "RemoveNode", 1);
            if (removeNodeMethod == null)
            {
                failureReason = $"Method 'RemoveNode' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                removeNodeMethod.Invoke(graphData, new[] { node });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryInvokeGraphRemoveEdge(object graphData, object edge, out string failureReason)
        {
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (edge == null)
            {
                failureReason = "Edge instance is null.";
                return false;
            }

            MethodInfo removeEdgeMethod = FindMethod(graphData.GetType(), "RemoveEdge", 1);
            if (removeEdgeMethod == null)
            {
                failureReason = $"Method 'RemoveEdge' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                removeEdgeMethod.Invoke(graphData, new[] { edge });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryInvokeGraphRemoveProperty(object graphData, object shaderInput, out string failureReason)
        {
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (shaderInput == null)
            {
                failureReason = "Shader input instance is null.";
                return false;
            }

            MethodInfo removeGraphInputMethod = FindMethod(graphData.GetType(), "RemoveGraphInput", 1);
            if (removeGraphInputMethod == null)
            {
                failureReason = $"Method 'RemoveGraphInput' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                removeGraphInputMethod.Invoke(graphData, new[] { shaderInput });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryResolveSupportedPropertyType(
            string propertyType,
            out string canonicalPropertyType,
            out Type shaderInputType,
            out string failureReason)
        {
            canonicalPropertyType = string.Empty;
            shaderInputType = null;
            failureReason = null;

            string normalized = (propertyType ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(normalized))
            {
                failureReason =
                    "Property type is required. Supported types: Color, Float/Vector1.";
                return false;
            }

            if (string.Equals(normalized, "Color", StringComparison.OrdinalIgnoreCase))
            {
                canonicalPropertyType = "Color";
                shaderInputType = ResolveType("UnityEditor.ShaderGraph.Internal.ColorShaderProperty");
                if (shaderInputType == null)
                {
                    failureReason =
                        "Could not resolve UnityEditor.ShaderGraph.Internal.ColorShaderProperty.";
                    return false;
                }

                return true;
            }

            if (string.Equals(normalized, "Float", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Vector1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(normalized, "Float/Vector1", StringComparison.OrdinalIgnoreCase))
            {
                canonicalPropertyType = "Float/Vector1";
                shaderInputType = ResolveType("UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty");
                if (shaderInputType == null)
                {
                    failureReason =
                        "Could not resolve UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty.";
                    return false;
                }

                return true;
            }

            failureReason =
                $"Unsupported Shader Graph property type '{propertyType}'. Supported types: Color, Float/Vector1.";
            return false;
        }

        private static bool TryCreateShaderInput(
            Type shaderInputType,
            string displayName,
            string defaultValue,
            out object shaderInput,
            out object parsedDefaultValue,
            out string parseNote,
            out string failureReason)
        {
            shaderInput = null;
            parsedDefaultValue = null;
            parseNote = null;
            failureReason = null;

            if (shaderInputType == null)
            {
                failureReason = "Shader input type is required.";
                return false;
            }

            try
            {
                shaderInput = Activator.CreateInstance(shaderInputType, true);
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to instantiate {shaderInputType.FullName}: {GetRootMessage(ex)}";
                return false;
            }

            SetMemberValue(shaderInput, "displayName", displayName);

            return TryAssignShaderInputDefaultValue(
                shaderInput,
                shaderInputType,
                defaultValue,
                out parsedDefaultValue,
                out parseNote,
                out failureReason);
        }

        private static bool TryResolvePropertyTypeFromInstance(
            object shaderInput,
            out string canonicalPropertyType,
            out Type shaderInputType,
            out string failureReason)
        {
            canonicalPropertyType = string.Empty;
            shaderInputType = shaderInput?.GetType();
            failureReason = null;

            if (shaderInputType == null)
            {
                failureReason = "Shader property instance is required.";
                return false;
            }

            string fullTypeName = shaderInputType.FullName ?? shaderInputType.Name;
            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.Internal.ColorShaderProperty", StringComparison.Ordinal))
            {
                canonicalPropertyType = "Color";
                return true;
            }

            if (string.Equals(fullTypeName, "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty", StringComparison.Ordinal))
            {
                canonicalPropertyType = "Float/Vector1";
                return true;
            }

            failureReason = $"Unsupported Shader Graph property instance type '{fullTypeName}'.";
            return false;
        }

        private static string NormalizeRequestedPropertyType(string propertyType)
        {
            if (string.IsNullOrWhiteSpace(propertyType))
            {
                return string.Empty;
            }

            if (string.Equals(propertyType.Trim(), "Color", StringComparison.OrdinalIgnoreCase))
            {
                return "Color";
            }

            if (string.Equals(propertyType.Trim(), "Float", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(propertyType.Trim(), "Vector1", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(propertyType.Trim(), "Float/Vector1", StringComparison.OrdinalIgnoreCase))
            {
                return "Float/Vector1";
            }

            return propertyType.Trim();
        }

        private static bool TryAssignShaderInputDefaultValue(
            object shaderInput,
            Type shaderInputType,
            string defaultValue,
            out object parsedDefaultValue,
            out string parseNote,
            out string failureReason)
        {
            parsedDefaultValue = null;
            parseNote = null;
            failureReason = null;

            if (shaderInput == null || shaderInputType == null)
            {
                failureReason = "Shader input instance and type are required.";
                return false;
            }

            if (string.Equals(shaderInputType.FullName, "UnityEditor.ShaderGraph.Internal.ColorShaderProperty", StringComparison.Ordinal))
            {
                if (!TryParseColorDefault(defaultValue, out Color parsedColor, out string colorNote, out string colorFailure))
                {
                    failureReason = colorFailure;
                    return false;
                }

                SetMemberValue(shaderInput, "value", parsedColor);
                parsedDefaultValue = parsedColor;
                parseNote = colorNote;
                return true;
            }

            if (string.Equals(shaderInputType.FullName, "UnityEditor.ShaderGraph.Internal.Vector1ShaderProperty", StringComparison.Ordinal))
            {
                if (!TryParseFloatDefault(defaultValue, out float parsedFloat, out string floatNote, out string floatFailure))
                {
                    failureReason = floatFailure;
                    return false;
                }

                SetMemberValue(shaderInput, "value", parsedFloat);
                parsedDefaultValue = parsedFloat;
                parseNote = floatNote;
                return true;
            }

            failureReason = $"Unsupported shader input type '{shaderInputType.FullName ?? shaderInputType.Name}'.";
            return false;
        }

        private static bool TryParseFloatDefault(
            string defaultValue,
            out float parsedFloat,
            out string note,
            out string failureReason)
        {
            parsedFloat = 0f;
            note = null;
            failureReason = null;

            if (string.IsNullOrWhiteSpace(defaultValue))
            {
                note = "No default value was provided; used 0.";
                return true;
            }

            if (float.TryParse(defaultValue.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out parsedFloat))
            {
                return true;
            }

            failureReason =
                $"Unable to parse float default value '{defaultValue}'. Expected an invariant-culture float such as '0' or '0.5'.";
            return false;
        }

        private static bool TryParseColorDefault(
            string defaultValue,
            out Color parsedColor,
            out string note,
            out string failureReason)
        {
            parsedColor = Color.black;
            note = null;
            failureReason = null;

            if (string.IsNullOrWhiteSpace(defaultValue))
            {
                note = "No default value was provided; used Color.black.";
                return true;
            }

            string trimmed = defaultValue.Trim();
            if (TryParseColorCsv(trimmed, out parsedColor))
            {
                return true;
            }

            if (ColorUtility.TryParseHtmlString(trimmed, out parsedColor))
            {
                return true;
            }

            failureReason =
                $"Unable to parse color default value '{defaultValue}'. Expected an HTML color string like '#RRGGBB' or comma-separated floats like '1, 0, 0, 1'.";
            return false;
        }

        private static bool TryParseColorCsv(string text, out Color parsedColor)
        {
            parsedColor = Color.black;
            string[] parts = text.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3 || parts.Length > 4)
            {
                return false;
            }

            if (!TryParseFloatInvariant(parts[0], out float r) ||
                !TryParseFloatInvariant(parts[1], out float g) ||
                !TryParseFloatInvariant(parts[2], out float b))
            {
                return false;
            }

            float a = 1f;
            if (parts.Length == 4 && !TryParseFloatInvariant(parts[3], out a))
            {
                return false;
            }

            parsedColor = new Color(r, g, b, a);
            return true;
        }

        private static bool TryParseFloatInvariant(string text, out float value)
        {
            return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value);
        }

        private static bool TryInvokeGraphAddInput(object graphData, object shaderInput, out string failureReason)
        {
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (shaderInput == null)
            {
                failureReason = "Shader input instance is null.";
                return false;
            }

            MethodInfo addInputMethod = FindMethod(graphData.GetType(), "AddGraphInput", 2);
            if (addInputMethod == null)
            {
                failureReason = $"Method 'AddGraphInput' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                addInputMethod.Invoke(graphData, new[] { shaderInput, -1 });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryInvokeGraphAddCopyOfShaderInput(
            object graphData,
            object shaderInput,
            out object duplicatedShaderInput,
            out string failureReason)
        {
            duplicatedShaderInput = null;
            failureReason = null;

            if (graphData == null)
            {
                failureReason = "GraphData instance is null.";
                return false;
            }

            if (shaderInput == null)
            {
                failureReason = "Shader input instance is null.";
                return false;
            }

            MethodInfo addCopyMethod = FindMethod(graphData.GetType(), "AddCopyOfShaderInput", 2);
            if (addCopyMethod == null)
            {
                failureReason = $"Method 'AddCopyOfShaderInput' was not found on {graphData.GetType().FullName}.";
                return false;
            }

            try
            {
                duplicatedShaderInput = addCopyMethod.Invoke(graphData, new object[] { shaderInput, -1 });
                if (duplicatedShaderInput == null)
                {
                    failureReason = "AddCopyOfShaderInput returned null.";
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static bool TryWriteGraphDataToDisk(
            string assetPath,
            object graphData,
            out string failureReason)
        {
            failureReason = null;

            Type fileUtilitiesType = ResolveType(FileUtilitiesTypeName);
            if (fileUtilitiesType == null)
            {
                failureReason = $"Could not resolve {FileUtilitiesTypeName}.";
                return false;
            }

            MethodInfo writeMethod = FindMethod(fileUtilitiesType, "WriteShaderGraphToDisk", 2);
            if (writeMethod == null)
            {
                failureReason = $"{FileUtilitiesTypeName}.WriteShaderGraphToDisk(string, GraphData) was not found.";
                return false;
            }

            try
            {
                object writtenText = writeMethod.Invoke(null, new[] { (object)assetPath, graphData });
                if (writtenText is string text && !string.IsNullOrWhiteSpace(text))
                {
                    return true;
                }

                failureReason = $"FileUtilities.WriteShaderGraphToDisk returned null for '{assetPath}'.";
                return false;
            }
            catch (Exception ex)
            {
                failureReason = $"FileUtilities.WriteShaderGraphToDisk threw: {GetRootMessage(ex)}";
                return false;
            }
        }

        private static bool TryLoadGraphData(
            string assetPath,
            string absolutePath,
            out object graphData,
            IList<string> notes,
            out string failureReason)
        {
            graphData = null;
            failureReason = null;

            if (TryReadGraphDataWithFileUtilities(assetPath, out graphData, out failureReason))
            {
                notes?.Add("Loaded GraphData via FileUtilities.TryReadGraphDataFromDisk().");
                return true;
            }

            if (TryReadGraphDataWithMultiJson(assetPath, absolutePath, out graphData, out failureReason))
            {
                notes?.Add("Loaded GraphData via MultiJson.Deserialize() fallback.");
                return true;
            }

            return false;
        }

        private static bool TryReadGraphDataWithFileUtilities(
            string assetPath,
            out object graphData,
            out string failureReason)
        {
            graphData = null;
            failureReason = null;

            Type fileUtilitiesType = ResolveType(FileUtilitiesTypeName);
            if (fileUtilitiesType == null)
            {
                failureReason = $"Could not resolve {FileUtilitiesTypeName}.";
                return false;
            }

            MethodInfo readMethod = FindMethod(fileUtilitiesType, "TryReadGraphDataFromDisk", 2);
            if (readMethod == null)
            {
                failureReason = $"{FileUtilitiesTypeName}.TryReadGraphDataFromDisk(string, out GraphData) was not found.";
                return false;
            }

            var args = new object[] { assetPath, null };
            try
            {
                object result = readMethod.Invoke(null, args);
                if (result is bool success && success && args.Length > 1 && args[1] != null)
                {
                    graphData = args[1];
                    return true;
                }

                failureReason = $"FileUtilities.TryReadGraphDataFromDisk returned false for '{assetPath}'.";
                return false;
            }
            catch (Exception ex)
            {
                failureReason = $"FileUtilities.TryReadGraphDataFromDisk threw: {GetRootMessage(ex)}";
                return false;
            }
        }

        private static bool TryReadGraphDataWithMultiJson(
            string assetPath,
            string absolutePath,
            out object graphData,
            out string failureReason)
        {
            graphData = null;
            failureReason = null;

            Type graphType = ResolveType(GraphDataTypeName);
            if (graphType == null)
            {
                failureReason = $"Could not resolve {GraphDataTypeName}.";
                return false;
            }

            Type multiJsonType = ResolveType(MultiJsonTypeName);
            if (multiJsonType == null)
            {
                failureReason = $"Could not resolve {MultiJsonTypeName}.";
                return false;
            }

            string textGraph;
            try
            {
                textGraph = File.ReadAllText(absolutePath, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to read '{absolutePath}': {GetRootMessage(ex)}";
                return false;
            }

            try
            {
                graphData = Activator.CreateInstance(graphType, true);
            }
            catch (Exception ex)
            {
                failureReason = $"Unable to create {GraphDataTypeName}: {GetRootMessage(ex)}";
                return false;
            }

            SetMemberValue(graphData, "messageManager", CreateMessageManagerInstance());
            SetMemberValue(graphData, "assetGuid", AssetDatabase.AssetPathToGUID(assetPath));

            MethodInfo deserializeMethod = FindGenericMethod(multiJsonType, "Deserialize", 4);
            if (deserializeMethod == null)
            {
                failureReason = $"{MultiJsonTypeName}.Deserialize<T>(T, string, JsonObject, bool) was not found.";
                graphData = null;
                return false;
            }

            try
            {
                MethodInfo closedDeserialize = deserializeMethod.MakeGenericMethod(graphType);
                closedDeserialize.Invoke(null, new object[] { graphData, textGraph, null, false });
                return true;
            }
            catch (Exception ex)
            {
                failureReason = $"MultiJson.Deserialize failed: {GetRootMessage(ex)}";
                graphData = null;
                return false;
            }
        }

        private static IReadOnlyList<string> DescribeProperties(object graphData)
        {
            return EnumerateMember(graphData, "properties")
                .Select(DescribeProperty)
                .ToArray();
        }

        private static IReadOnlyList<string> DescribeNodes(object graphData)
        {
            return EnumerateMember(graphData, "nodes")
                .Select(DescribeNode)
                .ToArray();
        }

        private static IReadOnlyList<string> DescribeConnections(object graphData)
        {
            return EnumerateMember(graphData, "edges")
                .Select(DescribeEdge)
                .ToArray();
        }

        private static IReadOnlyList<string> BuildPreview(
            object graphData,
            IReadOnlyList<string> properties,
            IReadOnlyList<string> nodes,
            IReadOnlyList<string> connections)
        {
            var preview = new List<string>
            {
                $"graphType={graphData.GetType().FullName ?? graphData.GetType().Name}",
                $"graphBaseType={graphData.GetType().BaseType?.FullName ?? string.Empty}",
                $"graphPath={GetStringProperty(graphData, "path")}",
                $"assetGuid={GetStringProperty(graphData, "assetGuid")}",
                $"isSubGraph={GetBoolProperty(graphData, "isSubGraph")}",
                $"propertyCount={properties.Count}",
                $"nodeCount={nodes.Count}",
                $"connectionCount={connections.Count}",
                $"categoryCount={CountEnumerableProperty(graphData, "categories")}"
            };

            string outputNode = DescribeNode(GetMemberValue(graphData, "outputNode"));
            if (!string.IsNullOrWhiteSpace(outputNode))
            {
                preview.Add($"outputNode={outputNode}");
            }

            return preview;
        }

        private static string DescribeProperty(object property)
        {
            if (property == null)
            {
                return string.Empty;
            }

            string displayName = GetStringProperty(property, "displayName", "name", "referenceName");
            string referenceName = GetStringProperty(property, "referenceName");
            string typeName = property.GetType().Name;

            string label = string.IsNullOrWhiteSpace(displayName) ? typeName : displayName;
            if (!string.IsNullOrWhiteSpace(referenceName) &&
                !string.Equals(referenceName, label, StringComparison.Ordinal))
            {
                label = $"{label} ({referenceName})";
            }

            return $"{label} [{typeName}]";
        }

        private static string DescribeNode(object node)
        {
            if (node == null)
            {
                return string.Empty;
            }

            string displayName = GetStringProperty(node, "displayName", "name");
            string objectId = GetStringProperty(node, "objectId");
            string typeName = node.GetType().Name;
            string label = string.IsNullOrWhiteSpace(displayName) ? typeName : displayName;
            string positionSuffix = TryDescribeNodePosition(node, out string positionDescription)
                ? $" @ {positionDescription}"
                : string.Empty;

            if (!string.IsNullOrWhiteSpace(objectId))
            {
                return $"{label} ({objectId}) [{typeName}]{positionSuffix}";
            }

            return $"{label} [{typeName}]{positionSuffix}";
        }

        private static bool TryDescribeNodePosition(object node, out string positionDescription)
        {
            positionDescription = string.Empty;
            if (!TryGetNodePositionRect(node, out Rect rect))
            {
                return false;
            }

            positionDescription = $"({Mathf.RoundToInt(rect.x)}, {Mathf.RoundToInt(rect.y)})";
            return true;
        }

        private static bool TryGetNodePositionRect(object node, out Rect rect)
        {
            rect = default;
            if (node == null)
            {
                return false;
            }

            object drawState = GetMemberValue(node, "drawState");
            if (drawState == null)
            {
                return false;
            }

            object position = GetMemberValue(drawState, "position");
            if (position is not Rect foundRect)
            {
                return false;
            }

            rect = foundRect;
            return true;
        }

        private static string DescribeEdge(object edge)
        {
            if (edge == null)
            {
                return string.Empty;
            }

            object outputSlotRef = GetMemberValue(edge, "outputSlot");
            object inputSlotRef = GetMemberValue(edge, "inputSlot");
            if (outputSlotRef == null || inputSlotRef == null)
            {
                return edge.GetType().Name;
            }

            return $"{DescribeSlotReference(outputSlotRef)} -> {DescribeSlotReference(inputSlotRef)}";
        }

        private static Dictionary<string, object> BuildEdgeLookupData(object edge)
        {
            var data = new Dictionary<string, object>
            {
                ["summary"] = DescribeEdge(edge),
                ["fullTypeName"] = edge?.GetType().FullName ?? edge?.GetType().Name ?? string.Empty,
            };

            if (TryGetSlotReferenceEndpointData(GetMemberValue(edge, "outputSlot"), out string outputNodeId, out int outputSlotId))
            {
                data["outputNodeId"] = outputNodeId;
                data["outputSlotId"] = outputSlotId;
            }

            if (TryGetSlotReferenceEndpointData(GetMemberValue(edge, "inputSlot"), out string inputNodeId, out int inputSlotId))
            {
                data["inputNodeId"] = inputNodeId;
                data["inputSlotId"] = inputSlotId;
            }

            return data;
        }

        private static bool EdgeMatchesResolvedConnection(
            object edge,
            string outputNodeId,
            int outputSlotId,
            string inputNodeId,
            int inputSlotId)
        {
            if (edge == null)
            {
                return false;
            }

            return TryGetSlotReferenceEndpointData(GetMemberValue(edge, "outputSlot"), out string edgeOutputNodeId, out int edgeOutputSlotId) &&
                   TryGetSlotReferenceEndpointData(GetMemberValue(edge, "inputSlot"), out string edgeInputNodeId, out int edgeInputSlotId) &&
                   string.Equals(edgeOutputNodeId, outputNodeId, StringComparison.Ordinal) &&
                   edgeOutputSlotId == outputSlotId &&
                   string.Equals(edgeInputNodeId, inputNodeId, StringComparison.Ordinal) &&
                   edgeInputSlotId == inputSlotId;
        }

        private static bool TryGetSlotReferenceEndpointData(object slotReference, out string nodeId, out int slotId)
        {
            nodeId = string.Empty;
            slotId = -1;

            if (slotReference == null)
            {
                return false;
            }

            object node = GetMemberValue(slotReference, "node");
            nodeId = GetStringProperty(node, "objectId");
            slotId = GetIntProperty(slotReference, "slotId");

            return !string.IsNullOrWhiteSpace(nodeId) && slotId >= 0;
        }

        private static string DescribeSlotReference(object slotReference)
        {
            if (slotReference == null)
            {
                return string.Empty;
            }

            object node = GetMemberValue(slotReference, "node");
            object slot = GetMemberValue(slotReference, "slot");
            int slotId = GetIntProperty(slotReference, "slotId");

            string nodeLabel = DescribeNode(node);
            if (string.IsNullOrWhiteSpace(nodeLabel))
            {
                nodeLabel = node?.GetType().Name ?? "unknown-node";
            }

            string slotLabel = GetStringProperty(slot, "displayName", "name");
            if (string.IsNullOrWhiteSpace(slotLabel))
            {
                slotLabel = slotId >= 0 ? $"slot-{slotId}" : "slot";
            }

            return $"{nodeLabel}:{slotLabel}";
        }

        private static string GetTypeName(object target)
        {
            if (target == null)
            {
                return string.Empty;
            }

            Type type = target.GetType();
            return type.FullName ?? type.Name;
        }

        private static int CountEnumerableProperty(object target, string memberName)
        {
            return EnumerateMember(target, memberName).Count;
        }

        private static IReadOnlyList<object> EnumerateMember(object target, string memberName)
        {
            object value = GetMemberValue(target, memberName);
            if (value is IEnumerable enumerable)
            {
                return enumerable.Cast<object>().Where(item => item != null).ToArray();
            }

            if (TryInvokeEnumerableFallback(target, memberName, out IReadOnlyList<object> fallbackItems))
            {
                return fallbackItems;
            }

            return Array.Empty<object>();
        }

        private static bool TryInvokeEnumerableFallback(
            object target,
            string memberName,
            out IReadOnlyList<object> items)
        {
            items = Array.Empty<object>();
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return false;
            }

            string fallbackMethodName = memberName switch
            {
                "nodes" => "GetNodes",
                _ => string.Empty,
            };

            if (string.IsNullOrWhiteSpace(fallbackMethodName))
            {
                return false;
            }

            MethodInfo method = FindMethod(target.GetType(), fallbackMethodName, 0);
            if (method == null)
            {
                return false;
            }

            try
            {
                if (method.IsGenericMethodDefinition)
                {
                    Type genericArgument = memberName switch
                    {
                        "nodes" => ResolveType("UnityEditor.ShaderGraph.AbstractMaterialNode"),
                        _ => null,
                    };

                    if (genericArgument == null)
                    {
                        return false;
                    }

                    method = method.MakeGenericMethod(genericArgument);
                }

                object value = method.Invoke(target, Array.Empty<object>());
                if (value is IEnumerable enumerable)
                {
                    items = enumerable.Cast<object>().Where(item => item != null).ToArray();
                    return true;
                }
            }
            catch
            {
                return false;
            }

            return false;
        }

        private static object GetMemberValue(object target, string memberName)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return null;
            }

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(memberName, InstanceFlags);
            if (property != null)
            {
                try
                {
                    return property.GetValue(target, null);
                }
                catch
                {
                    return null;
                }
            }

            FieldInfo field = type.GetField(memberName, InstanceFlags);
            if (field != null)
            {
                try
                {
                    return field.GetValue(target);
                }
                catch
                {
                    return null;
                }
            }

            return null;
        }

        private static bool GetBoolProperty(object target, string memberName)
        {
            object value = GetMemberValue(target, memberName);
            return value is bool flag && flag;
        }

        private static int GetIntProperty(object target, string memberName)
        {
            object value = GetMemberValue(target, memberName);
            if (value is int intValue)
            {
                return intValue;
            }

            return -1;
        }

        private static string GetStringProperty(object target, params string[] memberNames)
        {
            if (target == null || memberNames == null)
            {
                return string.Empty;
            }

            foreach (string memberName in memberNames)
            {
                object value = GetMemberValue(target, memberName);
                if (value == null)
                {
                    continue;
                }

                string text = value as string ?? value.ToString();
                if (!string.IsNullOrWhiteSpace(text))
                {
                    return text;
                }
            }

            return string.Empty;
        }

        private static void SetMemberValue(object target, string memberName, object value)
        {
            if (target == null || string.IsNullOrWhiteSpace(memberName))
            {
                return;
            }

            Type type = target.GetType();
            PropertyInfo property = type.GetProperty(memberName, InstanceFlags);
            if (property != null && property.CanWrite)
            {
                property.SetValue(target, value, null);
                return;
            }

            FieldInfo field = type.GetField(memberName, InstanceFlags);
            if (field != null)
            {
                field.SetValue(target, value);
            }
        }

        private static object CreateMessageManagerInstance()
        {
            Type messageManagerType = ResolveType(MessageManagerTypeName);
            if (messageManagerType == null)
            {
                return null;
            }

            try
            {
                return Activator.CreateInstance(messageManagerType, true);
            }
            catch
            {
                return null;
            }
        }

        private static bool TryInvokeInstanceMethod(object target, string methodName, out string failureReason)
        {
            return TryInvokeInstanceMethod(target, methodName, Array.Empty<object>(), out failureReason);
        }

        private static bool TryInvokeInstanceMethod(object target, string methodName, object[] args, out string failureReason)
        {
            failureReason = null;

            if (target == null)
            {
                failureReason = "Target object is null.";
                return false;
            }

            object[] invocationArgs = args ?? Array.Empty<object>();
            MethodInfo method = FindMethod(target.GetType(), methodName, invocationArgs.Length);
            if (method == null)
            {
                failureReason = $"Method '{methodName}' was not found on {target.GetType().FullName}.";
                return false;
            }

            try
            {
                method.Invoke(target, invocationArgs);
                return true;
            }
            catch (Exception ex)
            {
                failureReason = GetRootMessage(ex);
                return false;
            }
        }

        private static MethodInfo FindMethod(Type type, string methodName, int parameterCount)
        {
            if (type == null || string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            return type
                .GetMethods(InstanceFlags | StaticFlags)
                .FirstOrDefault(method =>
                    string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                    method.GetParameters().Length == parameterCount);
        }

        private static MethodInfo FindGenericMethod(Type type, string methodName, int parameterCount)
        {
            if (type == null || string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            return type
                .GetMethods(StaticFlags)
                .FirstOrDefault(method =>
                    string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                    method.IsGenericMethodDefinition &&
                    method.GetParameters().Length == parameterCount);
        }

        private static Type ResolveType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
            {
                return null;
            }

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type resolved = assembly.GetType(typeName, false);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            return Type.GetType(typeName, false);
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            string normalized = assetPath.Replace('\\', '/').Trim();
            return normalized.TrimStart('/');
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string normalized = NormalizeAssetPath(assetPath);
            if (string.IsNullOrWhiteSpace(normalized))
            {
                return string.Empty;
            }

            if (!normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(normalized);
            }

            string relative = normalized.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static string GetRootMessage(Exception exception)
        {
            if (exception == null)
            {
                return string.Empty;
            }

            Exception root = exception;
            while (root.GetBaseException() != null && root.GetBaseException() != root)
            {
                root = root.GetBaseException();
            }

            return root.Message;
        }
    }
}
