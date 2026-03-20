using System;
using System.Collections.Generic;
using System.Linq;

namespace ShaderGraphMcp.Editor.Models
{
    public enum ShaderGraphExecutionKind
    {
        Scaffold,
        PackageBacked,
    }

    public enum ShaderGraphBackendKind
    {
        Scaffold,
        PackageDetectedButIncomplete,
        PackageReady,
    }

    public sealed class ShaderGraphApiSurface
    {
        public string GraphTypeName { get; }
        public string BaseTypeName { get; }
        public bool IsResolved { get; }
        public bool HasAddGraphInput { get; }
        public bool HasConnect { get; }
        public bool HasAddNode { get; }
        public bool HasValidateGraph { get; }
        public bool HasReplaceWith { get; }
        public bool HasGetNodes { get; }
        public bool HasGetNodeFromGuid { get; }
        public bool HasContainsNodeGuid { get; }
        public bool HasRemoveNode { get; }
        public bool HasRemoveShaderProperty { get; }
        public IReadOnlyList<string> ResolvedMethodSignatures { get; }
        public IReadOnlyList<string> MissingMethodNames { get; }

        public ShaderGraphApiSurface(
            string graphTypeName,
            string baseTypeName,
            bool isResolved,
            bool hasAddGraphInput,
            bool hasConnect,
            bool hasAddNode,
            bool hasValidateGraph,
            bool hasReplaceWith,
            bool hasGetNodes,
            bool hasGetNodeFromGuid,
            bool hasContainsNodeGuid,
            bool hasRemoveNode,
            bool hasRemoveShaderProperty,
            IReadOnlyList<string> resolvedMethodSignatures,
            IReadOnlyList<string> missingMethodNames)
        {
            GraphTypeName = graphTypeName;
            BaseTypeName = baseTypeName;
            IsResolved = isResolved;
            HasAddGraphInput = hasAddGraphInput;
            HasConnect = hasConnect;
            HasAddNode = hasAddNode;
            HasValidateGraph = hasValidateGraph;
            HasReplaceWith = hasReplaceWith;
            HasGetNodes = hasGetNodes;
            HasGetNodeFromGuid = hasGetNodeFromGuid;
            HasContainsNodeGuid = hasContainsNodeGuid;
            HasRemoveNode = hasRemoveNode;
            HasRemoveShaderProperty = hasRemoveShaderProperty;
            ResolvedMethodSignatures = resolvedMethodSignatures ?? Array.Empty<string>();
            MissingMethodNames = missingMethodNames ?? Array.Empty<string>();
        }

        public bool HasCoreMutationSurface =>
            IsResolved &&
            HasAddGraphInput &&
            HasConnect &&
            HasAddNode &&
            HasValidateGraph;

        public IReadOnlyDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["graphTypeName"] = GraphTypeName,
                ["baseTypeName"] = BaseTypeName,
                ["resolved"] = IsResolved,
                ["hasAddGraphInput"] = HasAddGraphInput,
                ["hasConnect"] = HasConnect,
                ["hasAddNode"] = HasAddNode,
                ["hasValidateGraph"] = HasValidateGraph,
                ["hasReplaceWith"] = HasReplaceWith,
                ["hasGetNodes"] = HasGetNodes,
                ["hasGetNodeFromGuid"] = HasGetNodeFromGuid,
                ["hasContainsNodeGuid"] = HasContainsNodeGuid,
                ["hasRemoveNode"] = HasRemoveNode,
                ["hasRemoveShaderProperty"] = HasRemoveShaderProperty,
                ["hasCoreMutationSurface"] = HasCoreMutationSurface,
                ["resolvedMethodSignatures"] = ResolvedMethodSignatures.ToArray(),
                ["missingMethodNames"] = MissingMethodNames.ToArray(),
            };
        }
    }

    public sealed class ShaderGraphCompatibilitySnapshot
    {
        public ShaderGraphBackendKind BackendKind { get; }
        public IReadOnlyList<string> CandidateTypeNames { get; }
        public IReadOnlyList<string> DiscoveredTypeNames { get; }
        public IReadOnlyList<string> DetectedAssemblies { get; }
        public IReadOnlyList<string> ResolvedTypes { get; }
        public IReadOnlyList<string> MissingTypes { get; }
        public ShaderGraphApiSurface GraphSurface { get; }
        public IReadOnlyList<string> Notes { get; }

        public ShaderGraphCompatibilitySnapshot(
            ShaderGraphBackendKind backendKind,
            IReadOnlyList<string> candidateTypeNames,
            IReadOnlyList<string> discoveredTypeNames,
            IReadOnlyList<string> detectedAssemblies,
            IReadOnlyList<string> resolvedTypes,
            IReadOnlyList<string> missingTypes,
            ShaderGraphApiSurface graphSurface,
            IReadOnlyList<string> notes)
        {
            BackendKind = backendKind;
            CandidateTypeNames = candidateTypeNames ?? Array.Empty<string>();
            DiscoveredTypeNames = discoveredTypeNames ?? Array.Empty<string>();
            DetectedAssemblies = detectedAssemblies ?? Array.Empty<string>();
            ResolvedTypes = resolvedTypes ?? Array.Empty<string>();
            MissingTypes = missingTypes ?? Array.Empty<string>();
            GraphSurface = graphSurface ?? new ShaderGraphApiSurface(
                "UnityEditor.ShaderGraph.GraphData",
                string.Empty,
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
                false,
                Array.Empty<string>(),
                Array.Empty<string>());
            Notes = notes ?? Array.Empty<string>();
        }

        public bool HasShaderGraphPackage => DetectedAssemblies.Count > 0;

        public IReadOnlyDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["backendKind"] = BackendKind.ToString(),
                ["candidateTypeNames"] = CandidateTypeNames.ToArray(),
                ["discoveredTypeNames"] = DiscoveredTypeNames.ToArray(),
                ["packageDetected"] = HasShaderGraphPackage,
                ["detectedAssemblies"] = DetectedAssemblies.ToArray(),
                ["resolvedTypes"] = ResolvedTypes.ToArray(),
                ["missingTypes"] = MissingTypes.ToArray(),
                ["graphSurface"] = GraphSurface.ToDictionary(),
                ["notes"] = Notes.ToArray(),
            };
        }
    }

    public sealed class ShaderGraphAssetSnapshot
    {
        public string Operation { get; }
        public string AssetPath { get; }
        public string ManifestPath { get; }
        public string AbsolutePath { get; }
        public bool Exists { get; }
        public bool HasManifest { get; }
        public string Schema { get; }
        public string AssetName { get; }
        public string Template { get; }
        public string CreatedUtc { get; }
        public string UpdatedUtc { get; }
        public int PropertyCount { get; }
        public int NodeCount { get; }
        public int ConnectionCount { get; }
        public ShaderGraphExecutionKind ExecutionBackendKind { get; }
        public IReadOnlyList<string> Properties { get; }
        public IReadOnlyList<string> Nodes { get; }
        public IReadOnlyList<string> Connections { get; }
        public IReadOnlyList<string> Notes { get; }
        public IReadOnlyList<string> Preview { get; }
        public ShaderGraphCompatibilitySnapshot Compatibility { get; }

        public ShaderGraphAssetSnapshot(
            string operation,
            string assetPath,
            string manifestPath,
            string absolutePath,
            bool exists,
            bool hasManifest,
            string schema,
            string assetName,
            string template,
            string createdUtc,
            string updatedUtc,
            int propertyCount,
            int nodeCount,
            int connectionCount,
            ShaderGraphExecutionKind executionBackendKind,
            IReadOnlyList<string> properties,
            IReadOnlyList<string> nodes,
            IReadOnlyList<string> connections,
            IReadOnlyList<string> notes,
            IReadOnlyList<string> preview,
            ShaderGraphCompatibilitySnapshot compatibility)
        {
            Operation = operation;
            AssetPath = assetPath;
            ManifestPath = manifestPath;
            AbsolutePath = absolutePath;
            Exists = exists;
            HasManifest = hasManifest;
            Schema = schema;
            AssetName = assetName;
            Template = template;
            CreatedUtc = createdUtc;
            UpdatedUtc = updatedUtc;
            PropertyCount = propertyCount;
            NodeCount = nodeCount;
            ConnectionCount = connectionCount;
            ExecutionBackendKind = executionBackendKind;
            Properties = properties ?? Array.Empty<string>();
            Nodes = nodes ?? Array.Empty<string>();
            Connections = connections ?? Array.Empty<string>();
            Notes = notes ?? Array.Empty<string>();
            Preview = preview ?? Array.Empty<string>();
            Compatibility = compatibility ?? new ShaderGraphCompatibilitySnapshot(
                ShaderGraphBackendKind.Scaffold,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                null,
                Array.Empty<string>());
        }

        public ShaderGraphBackendKind BackendKind => Compatibility.BackendKind;

        public IReadOnlyDictionary<string, object> ToDictionary()
        {
            return new Dictionary<string, object>
            {
                ["operation"] = Operation,
                ["assetPath"] = AssetPath,
                ["manifestPath"] = ManifestPath,
                ["absolutePath"] = AbsolutePath,
                ["exists"] = Exists,
                ["hasManifest"] = HasManifest,
                ["schema"] = Schema,
                ["assetName"] = AssetName,
                ["template"] = Template,
                ["createdUtc"] = CreatedUtc,
                ["updatedUtc"] = UpdatedUtc,
                ["propertyCount"] = PropertyCount,
                ["nodeCount"] = NodeCount,
                ["connectionCount"] = ConnectionCount,
                ["executionBackendKind"] = ExecutionBackendKind.ToString(),
                ["properties"] = Properties.ToArray(),
                ["nodes"] = Nodes.ToArray(),
                ["connections"] = Connections.ToArray(),
                ["notes"] = Notes.ToArray(),
                ["preview"] = Preview.ToArray(),
                ["backendKind"] = BackendKind.ToString(),
                ["packageDetected"] = Compatibility.HasShaderGraphPackage,
                ["compatibility"] = Compatibility.ToDictionary(),
            };
        }
    }
}
