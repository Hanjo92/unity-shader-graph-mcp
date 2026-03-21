using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ShaderGraphMcp.Editor.Adapters;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Helpers
{
    internal static class ShaderGraphScaffoldStore
    {
        private const string ScaffoldSchema = "unity-shader-graph-mcp/scaffold-v1";
        private const string DefaultTemplate = "blank";
        private const string DefaultFolder = "Assets/ShaderGraphs";
        private const string ManifestSuffix = ".mcp.json";
        private static readonly Lazy<ShaderGraphCompatibilitySnapshot> CompatibilitySnapshot =
            new Lazy<ShaderGraphCompatibilitySnapshot>(ShaderGraphPackageCompatibility.Capture);

        public static ShaderGraphResponse CreateGraph(
            CreateGraphRequest request,
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

            string absoluteAssetPath = ToAbsolutePath(assetPath);
            string absoluteManifestPath = ToAbsolutePath(GetManifestPath(assetPath));

            try
            {
                EnsureParentDirectory(absoluteAssetPath);
            }
            catch (Exception ex)
            {
                return ShaderGraphResponse.Fail(
                    $"Unable to prepare folder for '{assetPath}': {ex.Message}"
                );
            }

            if (File.Exists(absoluteAssetPath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset already exists at '{assetPath}'."
                );
            }

            var now = UtcNow();
            var manifest = new ShaderGraphScaffoldManifest
            {
                schema = ScaffoldSchema,
                assetPath = assetPath,
                assetName = Path.GetFileNameWithoutExtension(assetPath),
                template = string.IsNullOrWhiteSpace(request.Template) ? DefaultTemplate : request.Template,
                createdUtc = now,
                updatedUtc = now,
            };

            try
            {
                File.WriteAllText(
                    absoluteAssetPath,
                    BuildPlaceholderPayload(manifest),
                    new UTF8Encoding(false)
                );
                WriteManifest(absoluteManifestPath, manifest);
                ImportAsset(assetPath);
            }
            catch (Exception ex)
            {
                return ShaderGraphResponse.Fail(
                    $"Failed to create scaffold Shader Graph at '{assetPath}': {ex.Message}"
                );
            }

            return ShaderGraphResponse.Ok(
                $"Scaffold Shader Graph created at '{assetPath}'.",
                BuildSummaryData(
                    manifest,
                    assetPath,
                    absoluteAssetPath,
                    true,
                    true,
                    executionKind,
                    "create_graph",
                    new[] { "Created scaffold placeholder asset and sidecar manifest." },
                    null
                )
            );
        }

        public static ShaderGraphResponse ReadGraphSummary(
            ReadGraphSummaryRequest request,
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

            string absoluteAssetPath = ToAbsolutePath(assetPath);
            if (!File.Exists(absoluteAssetPath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            string absoluteManifestPath = ToAbsolutePath(GetManifestPath(assetPath));
            ShaderGraphScaffoldManifest manifest;
            bool hasManifest = TryLoadManifest(absoluteManifestPath, out manifest);
            string[] previewLines = ReadPreviewLines(absoluteAssetPath, 16);

            if (hasManifest)
            {
                return ShaderGraphResponse.Ok(
                    $"Scaffold summary loaded for '{assetPath}'.",
                    BuildSummaryData(
                        manifest,
                        assetPath,
                        absoluteAssetPath,
                        true,
                        true,
                        executionKind,
                        "read_graph_summary",
                        new[] { "Scaffold manifest loaded successfully." },
                        previewLines
                    )
                );
            }

            return ShaderGraphResponse.Ok(
                $"Raw Shader Graph file summary loaded for '{assetPath}'.",
                BuildRawSummaryData(assetPath, absoluteAssetPath, previewLines, executionKind)
            );
        }

        public static ShaderGraphResponse FindNode(
            FindNodeRequest request,
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

            string absoluteAssetPath = ToAbsolutePath(assetPath);
            string absoluteManifestPath = ToAbsolutePath(GetManifestPath(assetPath));
            if (!File.Exists(absoluteAssetPath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            if (!TryLoadManifest(absoluteManifestPath, out ShaderGraphScaffoldManifest manifest))
            {
                return ShaderGraphResponse.Fail(
                    "'find_node' only supports graphs created by this scaffold. Use create_graph first so the manifest exists."
                );
            }

            string queryNodeId = string.IsNullOrWhiteSpace(request.NodeId) ? string.Empty : request.NodeId.Trim();
            string queryDisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? string.Empty : request.DisplayName.Trim();
            string queryNodeType = string.IsNullOrWhiteSpace(request.NodeType) ? string.Empty : request.NodeType.Trim();

            ShaderGraphScaffoldNode[] matches = manifest.nodes
                .Where(node => NodeMatchesQuery(node, queryNodeId, queryDisplayName, queryNodeType))
                .ToArray();

            var data = BuildSummaryData(
                manifest,
                assetPath,
                absoluteAssetPath,
                true,
                true,
                executionKind,
                "find_node",
                new[] { "Scaffold manifest queried for node lookup." },
                null
            );
            data["query"] = BuildNodeQuery(queryNodeId, queryDisplayName, queryNodeType);
            data["matchCount"] = matches.Length;
            data["matchStrategy"] = BuildFindNodeMatchStrategy(queryNodeId, queryDisplayName, queryNodeType);

            if (matches.Length == 1)
            {
                data["foundNode"] = BuildScaffoldNodeData(matches[0]);
                return ShaderGraphResponse.Ok(
                    $"Found scaffold node '{matches[0].displayName}' in '{assetPath}'.",
                    data
                );
            }

            data["candidateNodes"] = matches.Select(BuildScaffoldNodeData).Cast<object>().ToArray();

            if (matches.Length == 0)
            {
                return ShaderGraphResponse.Fail(
                    $"Could not find a scaffold node matching the provided query in '{assetPath}'.",
                    data
                );
            }

            return ShaderGraphResponse.Fail(
                $"Node query matched multiple scaffold nodes in '{assetPath}'. Narrow the query with nodeId/objectId, displayName, or nodeType.",
                data
            );
        }

        public static ShaderGraphResponse ListSupportedNodes(
            ListSupportedNodesRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            ShaderGraphCompatibilitySnapshot compatibility = CompatibilitySnapshot.Value;
            string[] supportedNodeTypes = ShaderGraphPackageGraphInspector.GetSupportedNodeCatalogLabels();
            string[] supportedNodeCanonicalNames = ShaderGraphPackageGraphInspector.GetSupportedNodeCanonicalNames().ToArray();
            string[] discoveredNodeTypes = ShaderGraphPackageGraphInspector.GetDiscoveredNodeCatalogLabels();

            var data = new Dictionary<string, object>
            {
                ["operation"] = "list_supported_nodes",
                ["executionBackendKind"] = executionKind.ToString(),
                ["backendKind"] = compatibility.BackendKind.ToString(),
                ["packageDetected"] = compatibility.HasShaderGraphPackage,
                ["compatibility"] = compatibility.ToDictionary(),
                ["supportedNodeTypes"] = supportedNodeTypes,
                ["supportedNodeCanonicalNames"] = supportedNodeCanonicalNames,
                ["supportedNodeCount"] = supportedNodeTypes.Length,
                ["discoveredNodeTypes"] = discoveredNodeTypes,
                ["discoveredNodeCount"] = discoveredNodeTypes.Length,
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

        public static ShaderGraphResponse AddProperty(
            AddPropertyRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Add property request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "add_property",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.PropertyName))
                    {
                        return "Property name is required.";
                    }

                    if (string.IsNullOrWhiteSpace(request.PropertyType))
                    {
                        return "Property type is required.";
                    }

                    string propertyName = request.PropertyName.Trim();
                    string propertyType = request.PropertyType.Trim();

                    var existing = manifest.properties.FirstOrDefault(
                        property => string.Equals(property.name, propertyName, StringComparison.Ordinal)
                    );

                    if (existing == null)
                    {
                        existing = new ShaderGraphScaffoldProperty();
                        manifest.properties.Add(existing);
                    }

                    existing.name = propertyName;
                    existing.type = propertyType;
                    existing.defaultValue = request.DefaultValue;
                    existing.updatedUtc = UtcNow();

                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    return BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "add_property",
                        new[] { $"Property '{request.PropertyName}' recorded in scaffold manifest." },
                        null
                    );
                }
            );
        }

        public static ShaderGraphResponse UpdateProperty(
            UpdatePropertyRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Update property request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "update_property",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.PropertyName))
                    {
                        return "Property name is required.";
                    }

                    string propertyName = request.PropertyName.Trim();
                    var existing = manifest.properties.FirstOrDefault(
                        property => string.Equals(property.name, propertyName, StringComparison.Ordinal));

                    if (existing == null)
                    {
                        return $"Property '{propertyName}' does not exist in the scaffold manifest.";
                    }

                    if (!string.IsNullOrWhiteSpace(request.PropertyType) &&
                        !string.Equals(existing.type ?? string.Empty, request.PropertyType.Trim(), StringComparison.OrdinalIgnoreCase))
                    {
                        return $"Property '{propertyName}' exists, but its scaffold type is '{existing.type}'. Requested type '{request.PropertyType.Trim()}' does not match.";
                    }

                    existing.defaultValue = request.DefaultValue;
                    existing.updatedUtc = UtcNow();
                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    var data = BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "update_property",
                        new[] { $"Property '{request.PropertyName}' updated in scaffold manifest." },
                        null
                    );

                    string propertyName = request.PropertyName.Trim();
                    ShaderGraphScaffoldProperty existing = manifest.properties.First(
                        property => string.Equals(property.name, propertyName, StringComparison.Ordinal));

                    data["updatedProperty"] = new Dictionary<string, object>
                    {
                        ["displayName"] = existing.name ?? string.Empty,
                        ["referenceName"] = existing.name ?? string.Empty,
                        ["resolvedPropertyType"] = existing.type ?? string.Empty,
                        ["defaultValue"] = existing.defaultValue ?? string.Empty,
                    };
                    return data;
                }
            );
        }

        public static ShaderGraphResponse MoveNode(
            MoveNodeRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Move node request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "move_node",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.NodeId))
                    {
                        return "Node id is required.";
                    }

                    string nodeId = request.NodeId.Trim();
                    ShaderGraphScaffoldNode existing = manifest.nodes.FirstOrDefault(
                        node => string.Equals(node.id, nodeId, StringComparison.Ordinal));
                    if (existing == null)
                    {
                        return $"Node '{nodeId}' does not exist in the scaffold manifest.";
                    }

                    existing.x = request.X;
                    existing.y = request.Y;
                    existing.updatedUtc = UtcNow();
                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    var data = BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "move_node",
                        new[] { $"Node '{request.NodeId}' moved in scaffold manifest." },
                        null
                    );

                    ShaderGraphScaffoldNode existing = manifest.nodes.First(
                        node => string.Equals(node.id, request.NodeId, StringComparison.Ordinal));
                    data["query"] = BuildNodeQuery(request.NodeId, null, null);
                    data["matchCount"] = 1;
                    data["movedNode"] = BuildScaffoldNodeData(existing);
                    return data;
                }
            );
        }

        public static ShaderGraphResponse DeleteNode(
            DeleteNodeRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Delete node request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "delete_node",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.NodeId))
                    {
                        return "Node id is required.";
                    }

                    string nodeId = request.NodeId.Trim();
                    ShaderGraphScaffoldNode existing = manifest.nodes.FirstOrDefault(
                        node => string.Equals(node.id, nodeId, StringComparison.Ordinal));
                    if (existing == null)
                    {
                        return $"Node '{nodeId}' does not exist in the scaffold manifest.";
                    }

                    manifest.nodes.Remove(existing);
                    manifest.connections.RemoveAll(
                        connection =>
                            string.Equals(connection.outputNodeId, nodeId, StringComparison.Ordinal) ||
                            string.Equals(connection.inputNodeId, nodeId, StringComparison.Ordinal));
                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    var data = BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "delete_node",
                        new[] { $"Node '{request.NodeId}' deleted from scaffold manifest." },
                        null
                    );
                    data["query"] = BuildNodeQuery(request.NodeId, null, null);
                    data["matchCount"] = 1;
                    data["deletedNodeId"] = request.NodeId.Trim();
                    return data;
                }
            );
        }

        public static ShaderGraphResponse RemoveProperty(
            RemovePropertyRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Remove property request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "remove_property",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.PropertyName))
                    {
                        return "Property name is required.";
                    }

                    string propertyName = request.PropertyName.Trim();
                    ShaderGraphScaffoldProperty existing = manifest.properties.FirstOrDefault(
                        property => string.Equals(property.name, propertyName, StringComparison.Ordinal));
                    if (existing == null)
                    {
                        return $"Property '{propertyName}' does not exist in the scaffold manifest.";
                    }

                    manifest.properties.Remove(existing);
                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    var data = BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "remove_property",
                        new[] { $"Property '{request.PropertyName}' removed from scaffold manifest." },
                        null
                    );
                    data["query"] = new Dictionary<string, object>
                    {
                        ["propertyName"] = request.PropertyName.Trim(),
                    };
                    data["matchCount"] = 1;
                    data["deletedPropertyName"] = request.PropertyName.Trim();
                    return data;
                }
            );
        }

        public static ShaderGraphResponse AddNode(
            AddNodeRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Add node request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "add_node",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.NodeType))
                    {
                        return "Node type is required.";
                    }

                    if (string.IsNullOrWhiteSpace(request.DisplayName))
                    {
                        return "Display name is required.";
                    }

                    string nodeType = request.NodeType.Trim();
                    string displayName = request.DisplayName.Trim();

                    string nodeId = GenerateNodeId(manifest);
                    manifest.nodes.Add(new ShaderGraphScaffoldNode
                    {
                        id = nodeId,
                        nodeType = nodeType,
                        displayName = displayName,
                        x = 0f,
                        y = 0f,
                        updatedUtc = UtcNow(),
                    });

                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    return BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "add_node",
                        new[] { $"Node '{request.DisplayName}' recorded in scaffold manifest." },
                        null
                    );
                }
            );
        }

        public static ShaderGraphResponse ConnectPorts(
            ConnectPortsRequest request,
            ShaderGraphExecutionKind executionKind)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Connect ports request is required.");
            }

            return MutateScaffold(
                request.AssetPath,
                "connect_ports",
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    if (string.IsNullOrWhiteSpace(request.OutputNodeId) || string.IsNullOrWhiteSpace(request.InputNodeId))
                    {
                        return "Both output and input node ids are required.";
                    }

                    if (string.IsNullOrWhiteSpace(request.OutputPort) || string.IsNullOrWhiteSpace(request.InputPort))
                    {
                        return "Both output and input port names are required.";
                    }

                    string outputNodeId = request.OutputNodeId.Trim();
                    string outputPort = request.OutputPort.Trim();
                    string inputNodeId = request.InputNodeId.Trim();
                    string inputPort = request.InputPort.Trim();

                    if (!HasNode(manifest, outputNodeId))
                    {
                        return $"Output node '{outputNodeId}' does not exist in the scaffold manifest.";
                    }

                    if (!HasNode(manifest, inputNodeId))
                    {
                        return $"Input node '{inputNodeId}' does not exist in the scaffold manifest.";
                    }

                    bool alreadyConnected = manifest.connections.Any(
                        connection =>
                            string.Equals(connection.outputNodeId, outputNodeId, StringComparison.Ordinal) &&
                            string.Equals(connection.outputPort, outputPort, StringComparison.Ordinal) &&
                            string.Equals(connection.inputNodeId, inputNodeId, StringComparison.Ordinal) &&
                            string.Equals(connection.inputPort, inputPort, StringComparison.Ordinal)
                    );

                    if (!alreadyConnected)
                    {
                        manifest.connections.Add(new ShaderGraphScaffoldConnection
                        {
                            outputNodeId = outputNodeId,
                            outputPort = outputPort,
                            inputNodeId = inputNodeId,
                            inputPort = inputPort,
                            updatedUtc = UtcNow(),
                        });
                    }

                    return null;
                },
                delegate(ShaderGraphScaffoldManifest manifest)
                {
                    return BuildSummaryData(
                        manifest,
                        NormalizeAssetPath(request.AssetPath),
                        ToAbsolutePath(NormalizeAssetPath(request.AssetPath)),
                        true,
                        true,
                        executionKind,
                        "connect_ports",
                        new[] { "Port connection recorded in scaffold manifest." },
                        null
                    );
                }
            );
        }

        public static ShaderGraphResponse SaveGraph(
            SaveGraphRequest request,
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

            string absoluteAssetPath = ToAbsolutePath(assetPath);
            string absoluteManifestPath = ToAbsolutePath(GetManifestPath(assetPath));

            if (!File.Exists(absoluteAssetPath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            ShaderGraphScaffoldManifest manifest;
            bool hasManifest = TryLoadManifest(absoluteManifestPath, out manifest);

            try
            {
                if (hasManifest)
                {
                    manifest.updatedUtc = UtcNow();
                    WriteManifest(absoluteManifestPath, manifest);
                }

                AssetDatabase.SaveAssets();
                ImportAsset(assetPath);
            }
            catch (Exception ex)
            {
                return ShaderGraphResponse.Fail(
                    $"Failed to save Shader Graph at '{assetPath}': {ex.Message}"
                );
            }

            if (hasManifest)
            {
                return ShaderGraphResponse.Ok(
                    $"Scaffold Shader Graph saved at '{assetPath}'.",
                    BuildSummaryData(
                        manifest,
                        assetPath,
                        absoluteAssetPath,
                        true,
                        true,
                        executionKind,
                        "save_graph",
                        new[] { "Scaffold manifest timestamp updated and Unity import refreshed." },
                        null
                    )
                );
            }

            return ShaderGraphResponse.Ok(
                $"Shader Graph import refreshed at '{assetPath}'.",
                BuildRawSummaryData(
                    assetPath,
                    absoluteAssetPath,
                    ReadPreviewLines(absoluteAssetPath, 8),
                    executionKind
                )
            );
        }

        private static ShaderGraphResponse MutateScaffold(
            string assetPathInput,
            string actionName,
            Func<ShaderGraphScaffoldManifest, string> validateAndMutate,
            Func<ShaderGraphScaffoldManifest, Dictionary<string, object>> buildData
        )
        {
            string assetPath = NormalizeAssetPath(assetPathInput);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return ShaderGraphResponse.Fail("A valid Shader Graph asset path is required.");
            }

            string absoluteAssetPath = ToAbsolutePath(assetPath);
            string absoluteManifestPath = ToAbsolutePath(GetManifestPath(assetPath));

            if (!File.Exists(absoluteAssetPath))
            {
                return ShaderGraphResponse.Fail(
                    $"Shader Graph asset not found at '{assetPath}'."
                );
            }

            ShaderGraphScaffoldManifest manifest;
            if (!TryLoadManifest(absoluteManifestPath, out manifest))
            {
                return ShaderGraphResponse.Fail(
                    $"'{actionName}' only supports graphs created by this scaffold. " +
                    "Use create_graph first so the manifest exists."
                );
            }

            string validationMessage = validateAndMutate(manifest);
            if (!string.IsNullOrWhiteSpace(validationMessage))
            {
                return ShaderGraphResponse.Fail(validationMessage);
            }

            manifest.updatedUtc = UtcNow();

            try
            {
                WriteManifest(absoluteManifestPath, manifest);
                AssetDatabase.SaveAssets();
                ImportAsset(assetPath);
            }
            catch (Exception ex)
            {
                return ShaderGraphResponse.Fail(
                    $"Failed to update scaffold manifest at '{assetPath}': {ex.Message}"
                );
            }

            return ShaderGraphResponse.Ok(
                $"Scaffold manifest updated for '{assetPath}'.",
                buildData(manifest)
            );
        }

        private static Dictionary<string, object> BuildSummaryData(
            ShaderGraphScaffoldManifest manifest,
            string assetPath,
            string absoluteAssetPath,
            bool exists,
            bool hasManifest,
            ShaderGraphExecutionKind executionKind,
            string operation,
            IEnumerable<string> notes,
            IEnumerable<string> previewLines
        )
        {
            var snapshot = new ShaderGraphAssetSnapshot(
                operation,
                assetPath,
                GetManifestPath(assetPath),
                absoluteAssetPath,
                exists,
                hasManifest,
                manifest.schema,
                manifest.assetName,
                manifest.template,
                manifest.createdUtc,
                manifest.updatedUtc,
                manifest.properties.Count,
                manifest.nodes.Count,
                manifest.connections.Count,
                executionKind,
                manifest.properties.Select(property => property.name).ToArray(),
                manifest.nodes.Select(node => node.displayName).ToArray(),
                manifest.connections.Select(connection =>
                    string.Format(
                        "{0}:{1} -> {2}:{3}",
                        connection.outputNodeId,
                        connection.outputPort,
                        connection.inputNodeId,
                        connection.inputPort
                    )).ToArray(),
                notes == null ? Array.Empty<string>() : notes.ToArray(),
                previewLines == null ? Array.Empty<string>() : previewLines.ToArray(),
                CompatibilitySnapshot.Value
            );

            return new Dictionary<string, object>(snapshot.ToDictionary());
        }

        private static Dictionary<string, object> BuildRawSummaryData(
            string assetPath,
            string absoluteAssetPath,
            IEnumerable<string> previewLines,
            ShaderGraphExecutionKind executionKind
        )
        {
            var fileInfo = new FileInfo(absoluteAssetPath);
            var snapshot = new ShaderGraphAssetSnapshot(
                "read_graph_summary",
                assetPath,
                GetManifestPath(assetPath),
                absoluteAssetPath,
                true,
                false,
                "raw-file",
                Path.GetFileNameWithoutExtension(assetPath),
                null,
                null,
                fileInfo.LastWriteTimeUtc.ToString("O"),
                0,
                0,
                0,
                executionKind,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                new[] { "Raw file summary only; no scaffold manifest detected." },
                previewLines == null ? Array.Empty<string>() : previewLines.ToArray(),
                CompatibilitySnapshot.Value
            );

            var data = new Dictionary<string, object>(snapshot.ToDictionary());
            data["fileSizeBytes"] = fileInfo.Exists ? fileInfo.Length : 0L;
            return data;
        }

        private static string BuildPlaceholderPayload(ShaderGraphScaffoldManifest manifest)
        {
            var builder = new StringBuilder();
            builder.AppendLine("# ShaderGraphMcp scaffold placeholder");
            builder.AppendLine("# This file is not a serialized Shader Graph yet.");
            builder.AppendLine("# It exists so milestone 1 can create, inspect, and save a .shadergraph path safely.");
            builder.AppendLine("# Sidecar manifest: " + GetManifestName(manifest.assetPath));
            builder.AppendLine("# schema: " + manifest.schema);
            builder.AppendLine("# assetPath: " + manifest.assetPath);
            builder.AppendLine("# assetName: " + manifest.assetName);
            builder.AppendLine("# template: " + manifest.template);
            builder.AppendLine("# createdUtc: " + manifest.createdUtc);
            builder.AppendLine("# updatedUtc: " + manifest.updatedUtc);
            builder.AppendLine("# backendKind: " + CompatibilitySnapshot.Value.BackendKind);
            builder.AppendLine("# packageDetected: " + CompatibilitySnapshot.Value.HasShaderGraphPackage);
            builder.AppendLine("# graphType: " + CompatibilitySnapshot.Value.GraphSurface.GraphTypeName);
            builder.AppendLine("# graphBaseType: " + CompatibilitySnapshot.Value.GraphSurface.BaseTypeName);
            builder.AppendLine("# executionBackendKind: " + ShaderGraphExecutionKind.Scaffold);
            builder.AppendLine("# resolvedMethodSignatures: " + string.Join(", ", CompatibilitySnapshot.Value.GraphSurface.ResolvedMethodSignatures));
            builder.AppendLine("# missingMethodNames: " + string.Join(", ", CompatibilitySnapshot.Value.GraphSurface.MissingMethodNames));
            builder.AppendLine("# candidateTypeNames: " + string.Join(", ", CompatibilitySnapshot.Value.CandidateTypeNames));
            builder.AppendLine("# discoveredTypeNames: " + string.Join(", ", CompatibilitySnapshot.Value.DiscoveredTypeNames));
            builder.AppendLine();
            builder.AppendLine("This is a milestone-1 scaffold placeholder.");
            builder.AppendLine("Use the Unity Shader Graph editor or a later API integration to replace it with a real graph.");
            return builder.ToString();
        }

        private static bool NodeMatchesQuery(
            ShaderGraphScaffoldNode node,
            string queryNodeId,
            string queryDisplayName,
            string queryNodeType)
        {
            if (node == null)
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryNodeId) &&
                !string.Equals(node.id ?? string.Empty, queryNodeId, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName) &&
                !string.Equals(node.displayName ?? string.Empty, queryDisplayName, StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!string.IsNullOrWhiteSpace(queryNodeType) &&
                !string.Equals(
                    NormalizeNodeToken(node.nodeType),
                    NormalizeNodeToken(queryNodeType),
                    StringComparison.Ordinal))
            {
                return false;
            }

            return true;
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
                strategy.Add("Exact objectId/nodeId match.");
            }

            if (!string.IsNullOrWhiteSpace(queryDisplayName))
            {
                strategy.Add("Case-insensitive displayName match.");
            }

            if (!string.IsNullOrWhiteSpace(queryNodeType))
            {
                strategy.Add("Normalized nodeType token match.");
            }

            return strategy.ToArray();
        }

        private static Dictionary<string, object> BuildScaffoldNodeData(ShaderGraphScaffoldNode node)
        {
            string positionDescription = FormatScaffoldNodePosition(node);
            return new Dictionary<string, object>
            {
                ["objectId"] = node?.id ?? string.Empty,
                ["nodeId"] = node?.id ?? string.Empty,
                ["displayName"] = node?.displayName ?? string.Empty,
                ["nodeType"] = node?.nodeType ?? string.Empty,
                ["position"] = new Dictionary<string, object>
                {
                    ["x"] = node?.x ?? 0f,
                    ["y"] = node?.y ?? 0f,
                },
                ["summary"] = $"{node?.displayName ?? string.Empty} ({node?.id ?? string.Empty}) [{node?.nodeType ?? string.Empty}] @ {positionDescription}",
            };
        }

        private static string FormatScaffoldNodePosition(ShaderGraphScaffoldNode node)
        {
            return string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                "({0:0}, {1:0})",
                node?.x ?? 0f,
                node?.y ?? 0f);
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

        private static bool TryLoadManifest(string absoluteManifestPath, out ShaderGraphScaffoldManifest manifest)
        {
            manifest = null;

            if (!File.Exists(absoluteManifestPath))
            {
                return false;
            }

            string json = File.ReadAllText(absoluteManifestPath);
            manifest = JsonUtility.FromJson<ShaderGraphScaffoldManifest>(json);
            return manifest != null && string.Equals(manifest.schema, ScaffoldSchema, StringComparison.Ordinal);
        }

        private static void WriteManifest(string absoluteManifestPath, ShaderGraphScaffoldManifest manifest)
        {
            File.WriteAllText(
                absoluteManifestPath,
                JsonUtility.ToJson(manifest, true),
                new UTF8Encoding(false)
            );
        }

        private static string[] ReadPreviewLines(string absoluteAssetPath, int maxLines)
        {
            var previewLines = new List<string>();
            using (var reader = new StreamReader(absoluteAssetPath))
            {
                while (!reader.EndOfStream && previewLines.Count < maxLines)
                {
                    previewLines.Add(reader.ReadLine());
                }
            }

            return previewLines.ToArray();
        }

        private static void EnsureParentDirectory(string absoluteFilePath)
        {
            string directory = Path.GetDirectoryName(absoluteFilePath);
            if (string.IsNullOrWhiteSpace(directory))
            {
                return;
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            }
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return string.Empty;
            }

            string normalized = assetPath.Replace('\\', '/').Trim();
            normalized = normalized.TrimStart('/');

            if (string.Equals(normalized, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (!normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                normalized = string.Format("{0}/{1}", DefaultFolder, normalized.TrimStart('/'));
            }

            return normalized;
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
                return Application.dataPath;
            }

            string relative = normalized.Substring("Assets/".Length).Replace('/', Path.DirectorySeparatorChar);
            return Path.Combine(Application.dataPath, relative);
        }

        private static string GetManifestPath(string assetPath)
        {
            return NormalizeAssetPath(assetPath) + ManifestSuffix;
        }

        private static string GetManifestName(string assetPath)
        {
            return Path.GetFileName(GetManifestPath(assetPath));
        }

        private static void ImportAsset(string assetPath)
        {
            AssetDatabase.ImportAsset(NormalizeAssetPath(assetPath), ImportAssetOptions.ForceSynchronousImport);
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
        }

        private static bool HasNode(ShaderGraphScaffoldManifest manifest, string nodeId)
        {
            return manifest.nodes.Any(node => string.Equals(node.id, nodeId, StringComparison.Ordinal));
        }

        private static string GenerateNodeId(ShaderGraphScaffoldManifest manifest)
        {
            int index = manifest.nodes.Count + 1;
            string candidate = "node-" + index;
            while (manifest.nodes.Any(node => string.Equals(node.id, candidate, StringComparison.Ordinal)))
            {
                index += 1;
                candidate = "node-" + index;
            }

            return candidate;
        }

        private static string UtcNow()
        {
            return DateTime.UtcNow.ToString("O");
        }
    }

    [Serializable]
    public sealed class ShaderGraphScaffoldManifest
    {
        public string schema = "unity-shader-graph-mcp/scaffold-v1";
        public string assetPath;
        public string assetName;
        public string template;
        public string createdUtc;
        public string updatedUtc;
        public List<ShaderGraphScaffoldProperty> properties = new List<ShaderGraphScaffoldProperty>();
        public List<ShaderGraphScaffoldNode> nodes = new List<ShaderGraphScaffoldNode>();
        public List<ShaderGraphScaffoldConnection> connections = new List<ShaderGraphScaffoldConnection>();
    }

    [Serializable]
    public sealed class ShaderGraphScaffoldProperty
    {
        public string name;
        public string type;
        public string defaultValue;
        public string updatedUtc;
    }

    [Serializable]
    public sealed class ShaderGraphScaffoldNode
    {
        public string id;
        public string nodeType;
        public string displayName;
        public float x;
        public float y;
        public string updatedUtc;
    }

    [Serializable]
    public sealed class ShaderGraphScaffoldConnection
    {
        public string outputNodeId;
        public string outputPort;
        public string inputNodeId;
        public string inputPort;
        public string updatedUtc;
    }
}
