using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Models
{
    public enum ShaderGraphAction
    {
        CreateGraph,
        CreateSubGraph,
        RenameGraph,
        RenameSubGraph,
        DuplicateGraph,
        DuplicateSubGraph,
        DeleteGraph,
        MoveGraph,
        SetGraphMetadata,
        CreateCategory,
        RenameCategory,
        FindCategory,
        DeleteCategory,
        ReorderCategory,
        MergeCategory,
        DuplicateCategory,
        SplitCategory,
        ListCategories,
        ReadGraphSummary,
        ReadSubGraphSummary,
        ExportGraphContract,
        ImportGraphContract,
        FindNode,
        FindProperty,
        ListSupportedNodes,
        ListSupportedProperties,
        ListSupportedConnections,
        UpdateProperty,
        RenameProperty,
        DuplicateProperty,
        ReorderProperty,
        MovePropertyToCategory,
        RenameNode,
        DuplicateNode,
        MoveNode,
        DeleteNode,
        RemoveProperty,
        AddProperty,
        AddNode,
        ConnectPorts,
        FindConnection,
        RemoveConnection,
        ReconnectConnection,
        SaveGraph,
    }

    public abstract class ShaderGraphRequest
    {
        public ShaderGraphAction Action { get; }
        public string AssetPath { get; }

        protected ShaderGraphRequest(ShaderGraphAction action, string assetPath)
        {
            Action = action;
            AssetPath = assetPath;
        }
    }

    public sealed class CreateGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string Path { get; }
        public string Template { get; }

        public CreateGraphRequest(string name, string path, string template)
            : base(ShaderGraphAction.CreateGraph, CombinePath(path, name))
        {
            Name = NormalizeName(name);
            Path = NormalizeDirectory(path);
            Template = string.IsNullOrWhiteSpace(template) ? "blank" : template.Trim();
        }

        private static string CombinePath(string path, string name)
        {
            string directory = NormalizeDirectory(path);
            string fileName = EnsureShaderGraphExtension(NormalizeName(name));

            return NormalizeAssetPath(System.IO.Path.Combine(directory, fileName));
        }

        private static string NormalizeName(string name)
        {
            string normalized = string.IsNullOrWhiteSpace(name)
                ? "UntitledShaderGraph"
                : System.IO.Path.GetFileNameWithoutExtension(name.Trim());

            return string.IsNullOrWhiteSpace(normalized) ? "UntitledShaderGraph" : normalized;
        }

        private static string NormalizeDirectory(string path)
        {
            string normalized = string.IsNullOrWhiteSpace(path)
                ? "Assets/ShaderGraphs"
                : path.Replace('\\', '/').Trim().TrimEnd('/');

            if (string.Equals(normalized, "Assets", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (!normalized.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = $"Assets/{normalized.TrimStart('/')}";
            }

            return normalized;
        }

        private static string EnsureShaderGraphExtension(string name)
        {
            return name.EndsWith(".shadergraph", System.StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadergraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/');
        }
    }

    public sealed class CreateSubGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string Path { get; }
        public string Template { get; }

        public CreateSubGraphRequest(string name, string path, string template)
            : base(ShaderGraphAction.CreateSubGraph, CombinePath(path, name))
        {
            Name = NormalizeName(name);
            Path = NormalizeDirectory(path);
            Template = string.IsNullOrWhiteSpace(template) ? "blank" : template.Trim();
        }

        private static string CombinePath(string path, string name)
        {
            string directory = NormalizeDirectory(path);
            string fileName = EnsureShaderSubGraphExtension(NormalizeName(name));

            return NormalizeAssetPath(System.IO.Path.Combine(directory, fileName));
        }

        private static string NormalizeName(string name)
        {
            string normalized = string.IsNullOrWhiteSpace(name)
                ? "UntitledShaderSubGraph"
                : System.IO.Path.GetFileNameWithoutExtension(name.Trim());

            return string.IsNullOrWhiteSpace(normalized) ? "UntitledShaderSubGraph" : normalized;
        }

        private static string NormalizeDirectory(string path)
        {
            string normalized = string.IsNullOrWhiteSpace(path)
                ? "Assets/ShaderSubGraphs"
                : path.Replace('\\', '/').Trim().TrimEnd('/');

            if (string.Equals(normalized, "Assets", System.StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (!normalized.StartsWith("Assets/", System.StringComparison.OrdinalIgnoreCase))
            {
                normalized = $"Assets/{normalized.TrimStart('/')}";
            }

            return normalized;
        }

        private static string EnsureShaderSubGraphExtension(string name)
        {
            return name.EndsWith(".shadersubgraph", System.StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadersubgraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return assetPath.Replace('\\', '/');
        }
    }

    public sealed class ReadGraphSummaryRequest : ShaderGraphRequest
    {
        public ReadGraphSummaryRequest(string assetPath)
            : base(ShaderGraphAction.ReadGraphSummary, assetPath)
        {
        }
    }

    public sealed class ReadSubGraphSummaryRequest : ShaderGraphRequest
    {
        public ReadSubGraphSummaryRequest(string assetPath)
            : base(ShaderGraphAction.ReadSubGraphSummary, assetPath)
        {
        }
    }

    public sealed class ExportGraphContractRequest : ShaderGraphRequest
    {
        public ExportGraphContractRequest(string assetPath)
            : base(ShaderGraphAction.ExportGraphContract, assetPath)
        {
        }
    }

    public sealed class ImportGraphContractRequest : ShaderGraphRequest
    {
        public string GraphContractJson { get; }

        public ImportGraphContractRequest(string assetPath, string graphContractJson)
            : base(ShaderGraphAction.ImportGraphContract, assetPath)
        {
            GraphContractJson = string.IsNullOrWhiteSpace(graphContractJson)
                ? string.Empty
                : graphContractJson.Trim();
        }
    }

    public sealed class RenameGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string TargetAssetPath => BuildTargetAssetPath(AssetPath, Name);

        public RenameGraphRequest(string assetPath, string name)
            : base(ShaderGraphAction.RenameGraph, assetPath)
        {
            Name = NormalizeName(name);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string normalized = System.IO.Path.GetFileNameWithoutExtension(name.Trim());
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static string BuildTargetAssetPath(string assetPath, string name)
        {
            string normalizedAssetPath = NormalizeAssetPath(assetPath);
            string normalizedName = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalizedAssetPath) || string.IsNullOrWhiteSpace(normalizedName))
            {
                return normalizedAssetPath;
            }

            string directory = System.IO.Path.GetDirectoryName(normalizedAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = "Assets";
            }

            return NormalizeAssetPath(System.IO.Path.Combine(directory, EnsureShaderGraphExtension(normalizedName)));
        }

        private static string EnsureShaderGraphExtension(string name)
        {
            return name.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadergraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }

    public sealed class RenameSubGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string TargetAssetPath => BuildTargetAssetPath(AssetPath, Name);

        public RenameSubGraphRequest(string assetPath, string name)
            : base(ShaderGraphAction.RenameSubGraph, assetPath)
        {
            Name = NormalizeName(name);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string normalized = System.IO.Path.GetFileNameWithoutExtension(name.Trim());
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static string BuildTargetAssetPath(string assetPath, string name)
        {
            string normalizedAssetPath = NormalizeAssetPath(assetPath);
            string normalizedName = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalizedAssetPath) || string.IsNullOrWhiteSpace(normalizedName))
            {
                return normalizedAssetPath;
            }

            string directory = System.IO.Path.GetDirectoryName(normalizedAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = "Assets";
            }

            return NormalizeAssetPath(System.IO.Path.Combine(directory, EnsureShaderSubGraphExtension(normalizedName)));
        }

        private static string EnsureShaderSubGraphExtension(string name)
        {
            return name.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadersubgraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }

    public sealed class DuplicateGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string TargetAssetPath => BuildTargetAssetPath(AssetPath, Name);

        public DuplicateGraphRequest(string assetPath, string name)
            : base(ShaderGraphAction.DuplicateGraph, assetPath)
        {
            Name = NormalizeName(name);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string normalized = System.IO.Path.GetFileNameWithoutExtension(name.Trim());
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static string BuildTargetAssetPath(string assetPath, string name)
        {
            string normalizedAssetPath = NormalizeAssetPath(assetPath);
            string normalizedName = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalizedAssetPath) || string.IsNullOrWhiteSpace(normalizedName))
            {
                return normalizedAssetPath;
            }

            string directory = System.IO.Path.GetDirectoryName(normalizedAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = "Assets";
            }

            return NormalizeAssetPath(System.IO.Path.Combine(directory, EnsureShaderGraphExtension(normalizedName)));
        }

        private static string EnsureShaderGraphExtension(string name)
        {
            return name.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadergraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }

    public sealed class DuplicateSubGraphRequest : ShaderGraphRequest
    {
        public string Name { get; }
        public string TargetAssetPath => BuildTargetAssetPath(AssetPath, Name);

        public DuplicateSubGraphRequest(string assetPath, string name)
            : base(ShaderGraphAction.DuplicateSubGraph, assetPath)
        {
            Name = NormalizeName(name);
        }

        private static string NormalizeName(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return string.Empty;
            }

            string normalized = System.IO.Path.GetFileNameWithoutExtension(name.Trim());
            return string.IsNullOrWhiteSpace(normalized) ? string.Empty : normalized;
        }

        private static string BuildTargetAssetPath(string assetPath, string name)
        {
            string normalizedAssetPath = NormalizeAssetPath(assetPath);
            string normalizedName = NormalizeName(name);
            if (string.IsNullOrWhiteSpace(normalizedAssetPath) || string.IsNullOrWhiteSpace(normalizedName))
            {
                return normalizedAssetPath;
            }

            string directory = System.IO.Path.GetDirectoryName(normalizedAssetPath)?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(directory))
            {
                directory = "Assets";
            }

            return NormalizeAssetPath(System.IO.Path.Combine(directory, EnsureShaderSubGraphExtension(normalizedName)));
        }

        private static string EnsureShaderSubGraphExtension(string name)
        {
            return name.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase)
                ? name
                : $"{name}.shadersubgraph";
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }

    public sealed class DeleteGraphRequest : ShaderGraphRequest
    {
        public DeleteGraphRequest(string assetPath)
            : base(ShaderGraphAction.DeleteGraph, assetPath)
        {
        }
    }

    public sealed class MoveGraphRequest : ShaderGraphRequest
    {
        public string TargetPath { get; }
        public string TargetAssetPath => BuildTargetAssetPath(AssetPath, TargetPath);

        public MoveGraphRequest(string assetPath, string targetPath)
            : base(ShaderGraphAction.MoveGraph, assetPath)
        {
            TargetPath = NormalizeAssetPath(targetPath);
        }

        private static string BuildTargetAssetPath(string assetPath, string targetPath)
        {
            string normalizedAssetPath = NormalizeAssetPath(assetPath);
            string normalizedTargetPath = NormalizeAssetPath(targetPath);
            if (string.IsNullOrWhiteSpace(normalizedTargetPath))
            {
                return string.Empty;
            }

            if (normalizedTargetPath.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedTargetPath;
            }

            string fileName = Path.GetFileName(normalizedAssetPath);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                return normalizedTargetPath;
            }

            return NormalizeAssetPath(Path.Combine(normalizedTargetPath.TrimEnd('/'), fileName));
        }

        private static string NormalizeAssetPath(string assetPath)
        {
            return string.IsNullOrWhiteSpace(assetPath)
                ? string.Empty
                : assetPath.Replace('\\', '/').Trim();
        }
    }

    public sealed class SetGraphMetadataRequest : ShaderGraphRequest
    {
        public string GraphPathLabel { get; }
        public string GraphDefaultPrecision { get; }

        public SetGraphMetadataRequest(string assetPath, string graphPathLabel, string graphDefaultPrecision)
            : base(ShaderGraphAction.SetGraphMetadata, assetPath)
        {
            GraphPathLabel = string.IsNullOrWhiteSpace(graphPathLabel) ? string.Empty : graphPathLabel.Trim();
            GraphDefaultPrecision = string.IsNullOrWhiteSpace(graphDefaultPrecision) ? string.Empty : graphDefaultPrecision.Trim();
        }
    }

    public sealed class CreateCategoryRequest : ShaderGraphRequest
    {
        public string Name { get; }

        public CreateCategoryRequest(string assetPath, string name)
            : base(ShaderGraphAction.CreateCategory, assetPath)
        {
            Name = string.IsNullOrWhiteSpace(name) ? string.Empty : name.Trim();
        }
    }

    public sealed class RenameCategoryRequest : ShaderGraphRequest
    {
        public string CategoryGuid { get; }
        public string CategoryName { get; }
        public string DisplayName { get; }

        public RenameCategoryRequest(string assetPath, string categoryGuid, string categoryName, string displayName)
            : base(ShaderGraphAction.RenameCategory, assetPath)
        {
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
        }
    }

    public sealed class FindCategoryRequest : ShaderGraphRequest
    {
        public string CategoryGuid { get; }
        public string CategoryName { get; }

        public FindCategoryRequest(string assetPath, string categoryGuid, string categoryName)
            : base(ShaderGraphAction.FindCategory, assetPath)
        {
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
        }
    }

    public sealed class DeleteCategoryRequest : ShaderGraphRequest
    {
        public string CategoryGuid { get; }
        public string CategoryName { get; }

        public DeleteCategoryRequest(string assetPath, string categoryGuid, string categoryName)
            : base(ShaderGraphAction.DeleteCategory, assetPath)
        {
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
        }
    }

    public sealed class ReorderCategoryRequest : ShaderGraphRequest
    {
        public string CategoryGuid { get; }
        public string CategoryName { get; }
        public int Index { get; }

        public ReorderCategoryRequest(string assetPath, string categoryGuid, string categoryName, int index)
            : base(ShaderGraphAction.ReorderCategory, assetPath)
        {
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
            Index = index;
        }
    }

    public sealed class MergeCategoryRequest : ShaderGraphRequest
    {
        public string SourceCategoryGuid { get; }
        public string SourceCategoryName { get; }
        public string TargetCategoryGuid { get; }
        public string TargetCategoryName { get; }

        public MergeCategoryRequest(
            string assetPath,
            string sourceCategoryGuid,
            string sourceCategoryName,
            string targetCategoryGuid,
            string targetCategoryName)
            : base(ShaderGraphAction.MergeCategory, assetPath)
        {
            SourceCategoryGuid = string.IsNullOrWhiteSpace(sourceCategoryGuid) ? string.Empty : sourceCategoryGuid.Trim();
            SourceCategoryName = string.IsNullOrWhiteSpace(sourceCategoryName) ? string.Empty : sourceCategoryName.Trim();
            TargetCategoryGuid = string.IsNullOrWhiteSpace(targetCategoryGuid) ? string.Empty : targetCategoryGuid.Trim();
            TargetCategoryName = string.IsNullOrWhiteSpace(targetCategoryName) ? string.Empty : targetCategoryName.Trim();
        }
    }

    public sealed class DuplicateCategoryRequest : ShaderGraphRequest
    {
        public string CategoryGuid { get; }
        public string CategoryName { get; }
        public string DisplayName { get; }

        public DuplicateCategoryRequest(string assetPath, string categoryGuid, string categoryName, string displayName)
            : base(ShaderGraphAction.DuplicateCategory, assetPath)
        {
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
        }
    }

    public sealed class SplitCategoryRequest : ShaderGraphRequest
    {
        public string SourceCategoryGuid { get; }
        public string SourceCategoryName { get; }
        public string DisplayName { get; }
        public IReadOnlyList<string> PropertyNames { get; }

        public SplitCategoryRequest(
            string assetPath,
            string sourceCategoryGuid,
            string sourceCategoryName,
            string displayName,
            IEnumerable<string> propertyNames)
            : base(ShaderGraphAction.SplitCategory, assetPath)
        {
            SourceCategoryGuid = string.IsNullOrWhiteSpace(sourceCategoryGuid) ? string.Empty : sourceCategoryGuid.Trim();
            SourceCategoryName = string.IsNullOrWhiteSpace(sourceCategoryName) ? string.Empty : sourceCategoryName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            PropertyNames = NormalizePropertyNames(propertyNames);
        }

        private static IReadOnlyList<string> NormalizePropertyNames(IEnumerable<string> propertyNames)
        {
            if (propertyNames == null)
            {
                return Array.Empty<string>();
            }

            var normalized = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (string propertyName in propertyNames)
            {
                string trimmed = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || !seen.Add(trimmed))
                {
                    continue;
                }

                normalized.Add(trimmed);
            }

            return normalized;
        }
    }

    public sealed class ListCategoriesRequest : ShaderGraphRequest
    {
        public ListCategoriesRequest(string assetPath)
            : base(ShaderGraphAction.ListCategories, assetPath)
        {
        }
    }

    public sealed class FindNodeRequest : ShaderGraphRequest
    {
        public string NodeId { get; }
        public string DisplayName { get; }
        public string NodeType { get; }

        public FindNodeRequest(string assetPath, string nodeId, string displayName, string nodeType)
            : base(ShaderGraphAction.FindNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            NodeType = string.IsNullOrWhiteSpace(nodeType) ? string.Empty : nodeType.Trim();
        }
    }

    public sealed class FindPropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string DisplayName { get; }
        public string ReferenceName { get; }
        public string PropertyType { get; }

        public FindPropertyRequest(string assetPath, string propertyName, string displayName, string referenceName, string propertyType)
            : base(ShaderGraphAction.FindProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            ReferenceName = string.IsNullOrWhiteSpace(referenceName) ? string.Empty : referenceName.Trim();
            PropertyType = string.IsNullOrWhiteSpace(propertyType) ? string.Empty : propertyType.Trim();
        }
    }

    public sealed class ListSupportedNodesRequest : ShaderGraphRequest
    {
        public ListSupportedNodesRequest()
            : base(ShaderGraphAction.ListSupportedNodes, null)
        {
        }
    }

    public sealed class ListSupportedPropertiesRequest : ShaderGraphRequest
    {
        public ListSupportedPropertiesRequest()
            : base(ShaderGraphAction.ListSupportedProperties, null)
        {
        }
    }

    public sealed class ListSupportedConnectionsRequest : ShaderGraphRequest
    {
        public ListSupportedConnectionsRequest()
            : base(ShaderGraphAction.ListSupportedConnections, null)
        {
        }
    }

    public sealed class UpdatePropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string PropertyType { get; }
        public string DefaultValue { get; }

        public UpdatePropertyRequest(string assetPath, string propertyName, string propertyType, string defaultValue)
            : base(ShaderGraphAction.UpdateProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            PropertyType = string.IsNullOrWhiteSpace(propertyType) ? string.Empty : propertyType.Trim();
            DefaultValue = defaultValue;
        }
    }

    public sealed class RenamePropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string DisplayName { get; }
        public string ReferenceName { get; }

        public RenamePropertyRequest(string assetPath, string propertyName, string displayName, string referenceName)
            : base(ShaderGraphAction.RenameProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            ReferenceName = string.IsNullOrWhiteSpace(referenceName) ? string.Empty : referenceName.Trim();
        }
    }

    public sealed class DuplicatePropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string DisplayName { get; }
        public string ReferenceName { get; }

        public DuplicatePropertyRequest(string assetPath, string propertyName, string displayName, string referenceName)
            : base(ShaderGraphAction.DuplicateProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            ReferenceName = string.IsNullOrWhiteSpace(referenceName) ? string.Empty : referenceName.Trim();
        }
    }

    public sealed class ReorderPropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public int Index { get; }

        public ReorderPropertyRequest(string assetPath, string propertyName, int index)
            : base(ShaderGraphAction.ReorderProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            Index = index;
        }
    }

    public sealed class MovePropertyToCategoryRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string CategoryGuid { get; }
        public string CategoryName { get; }
        public int? Index { get; }

        public MovePropertyToCategoryRequest(string assetPath, string propertyName, string categoryGuid, string categoryName, int? index)
            : base(ShaderGraphAction.MovePropertyToCategory, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
            CategoryGuid = string.IsNullOrWhiteSpace(categoryGuid) ? string.Empty : categoryGuid.Trim();
            CategoryName = string.IsNullOrWhiteSpace(categoryName) ? string.Empty : categoryName.Trim();
            Index = index;
        }
    }

    public sealed class RenameNodeRequest : ShaderGraphRequest
    {
        public string NodeId { get; }
        public string DisplayName { get; }

        public RenameNodeRequest(string assetPath, string nodeId, string displayName)
            : base(ShaderGraphAction.RenameNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
        }
    }

    public sealed class DuplicateNodeRequest : ShaderGraphRequest
    {
        public string NodeId { get; }
        public string DisplayName { get; }

        public DuplicateNodeRequest(string assetPath, string nodeId, string displayName)
            : base(ShaderGraphAction.DuplicateNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
        }
    }

    public sealed class MoveNodeRequest : ShaderGraphRequest
    {
        public string NodeId { get; }
        public bool HasExactPosition { get; }
        public float X { get; }
        public float Y { get; }
        public string AnchorNodeId { get; }
        public string AnchorDisplayName { get; }
        public string AnchorNodeType { get; }
        public string Direction { get; }
        public float? Spacing { get; }
        public string LayoutPreset { get; }

        public MoveNodeRequest(string assetPath, string nodeId, float x, float y)
            : this(assetPath, nodeId, x, y, null, null, null, null, null, null)
        {
        }

        public MoveNodeRequest(
            string assetPath,
            string nodeId,
            float? x,
            float? y,
            string anchorNodeId,
            string anchorDisplayName,
            string anchorNodeType,
            string direction,
            float? spacing,
            string layoutPreset)
            : base(ShaderGraphAction.MoveNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            HasExactPosition = x.HasValue && y.HasValue;
            X = x ?? 0f;
            Y = y ?? 0f;
            AnchorNodeId = string.IsNullOrWhiteSpace(anchorNodeId) ? string.Empty : anchorNodeId.Trim();
            AnchorDisplayName = string.IsNullOrWhiteSpace(anchorDisplayName) ? string.Empty : anchorDisplayName.Trim();
            AnchorNodeType = string.IsNullOrWhiteSpace(anchorNodeType) ? string.Empty : anchorNodeType.Trim();
            Direction = string.IsNullOrWhiteSpace(direction) ? string.Empty : direction.Trim();
            Spacing = spacing;
            LayoutPreset = string.IsNullOrWhiteSpace(layoutPreset) ? string.Empty : layoutPreset.Trim();
        }
    }

    public sealed class DeleteNodeRequest : ShaderGraphRequest
    {
        public string NodeId { get; }

        public DeleteNodeRequest(string assetPath, string nodeId)
            : base(ShaderGraphAction.DeleteNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
        }
    }

    public sealed class RemovePropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }

        public RemovePropertyRequest(string assetPath, string propertyName)
            : base(ShaderGraphAction.RemoveProperty, assetPath)
        {
            PropertyName = string.IsNullOrWhiteSpace(propertyName) ? string.Empty : propertyName.Trim();
        }
    }

    public sealed class AddPropertyRequest : ShaderGraphRequest
    {
        public string PropertyName { get; }
        public string PropertyType { get; }
        public string DefaultValue { get; }

        public AddPropertyRequest(string assetPath, string propertyName, string propertyType, string defaultValue)
            : base(ShaderGraphAction.AddProperty, assetPath)
        {
            PropertyName = propertyName;
            PropertyType = propertyType;
            DefaultValue = defaultValue;
        }
    }

    public sealed class AddNodeRequest : ShaderGraphRequest
    {
        public string NodeType { get; }
        public string DisplayName { get; }
        public bool HasExactPosition { get; }
        public float X { get; }
        public float Y { get; }
        public string AnchorNodeId { get; }
        public string AnchorDisplayName { get; }
        public string AnchorNodeType { get; }
        public string Direction { get; }
        public float? Spacing { get; }
        public string LayoutPreset { get; }

        public AddNodeRequest(string assetPath, string nodeType, string displayName)
            : this(assetPath, nodeType, displayName, null, null, null, null, null, null, null, null)
        {
        }

        public AddNodeRequest(
            string assetPath,
            string nodeType,
            string displayName,
            float? x,
            float? y,
            string anchorNodeId,
            string anchorDisplayName,
            string anchorNodeType,
            string direction,
            float? spacing,
            string layoutPreset)
            : base(ShaderGraphAction.AddNode, assetPath)
        {
            NodeType = string.IsNullOrWhiteSpace(nodeType) ? string.Empty : nodeType.Trim();
            DisplayName = string.IsNullOrWhiteSpace(displayName) ? string.Empty : displayName.Trim();
            HasExactPosition = x.HasValue && y.HasValue;
            X = x ?? 0f;
            Y = y ?? 0f;
            AnchorNodeId = string.IsNullOrWhiteSpace(anchorNodeId) ? string.Empty : anchorNodeId.Trim();
            AnchorDisplayName = string.IsNullOrWhiteSpace(anchorDisplayName) ? string.Empty : anchorDisplayName.Trim();
            AnchorNodeType = string.IsNullOrWhiteSpace(anchorNodeType) ? string.Empty : anchorNodeType.Trim();
            Direction = string.IsNullOrWhiteSpace(direction) ? string.Empty : direction.Trim();
            Spacing = spacing;
            LayoutPreset = string.IsNullOrWhiteSpace(layoutPreset) ? string.Empty : layoutPreset.Trim();
        }
    }

    public sealed class ConnectPortsRequest : ShaderGraphRequest
    {
        public string OutputNodeId { get; }
        public string OutputPort { get; }
        public string InputNodeId { get; }
        public string InputPort { get; }

        public ConnectPortsRequest(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort
        ) : base(ShaderGraphAction.ConnectPorts, assetPath)
        {
            OutputNodeId = outputNodeId;
            OutputPort = outputPort;
            InputNodeId = inputNodeId;
            InputPort = inputPort;
        }
    }

    public sealed class RemoveConnectionRequest : ShaderGraphRequest
    {
        public string OutputNodeId { get; }
        public string OutputPort { get; }
        public string InputNodeId { get; }
        public string InputPort { get; }

        public RemoveConnectionRequest(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort
        ) : base(ShaderGraphAction.RemoveConnection, assetPath)
        {
            OutputNodeId = outputNodeId;
            OutputPort = outputPort;
            InputNodeId = inputNodeId;
            InputPort = inputPort;
        }
    }

    public sealed class FindConnectionRequest : ShaderGraphRequest
    {
        public string OutputNodeId { get; }
        public string OutputPort { get; }
        public string InputNodeId { get; }
        public string InputPort { get; }

        public FindConnectionRequest(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort
        ) : base(ShaderGraphAction.FindConnection, assetPath)
        {
            OutputNodeId = outputNodeId;
            OutputPort = outputPort;
            InputNodeId = inputNodeId;
            InputPort = inputPort;
        }
    }

    public sealed class ReconnectConnectionRequest : ShaderGraphRequest
    {
        public string OldOutputNodeId { get; }
        public string OldOutputPort { get; }
        public string OldInputNodeId { get; }
        public string OldInputPort { get; }
        public string OutputNodeId { get; }
        public string OutputPort { get; }
        public string InputNodeId { get; }
        public string InputPort { get; }

        public ReconnectConnectionRequest(
            string assetPath,
            string oldOutputNodeId,
            string oldOutputPort,
            string oldInputNodeId,
            string oldInputPort,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort
        ) : base(ShaderGraphAction.ReconnectConnection, assetPath)
        {
            OldOutputNodeId = string.IsNullOrWhiteSpace(oldOutputNodeId) ? string.Empty : oldOutputNodeId.Trim();
            OldOutputPort = string.IsNullOrWhiteSpace(oldOutputPort) ? string.Empty : oldOutputPort.Trim();
            OldInputNodeId = string.IsNullOrWhiteSpace(oldInputNodeId) ? string.Empty : oldInputNodeId.Trim();
            OldInputPort = string.IsNullOrWhiteSpace(oldInputPort) ? string.Empty : oldInputPort.Trim();
            OutputNodeId = string.IsNullOrWhiteSpace(outputNodeId) ? string.Empty : outputNodeId.Trim();
            OutputPort = string.IsNullOrWhiteSpace(outputPort) ? string.Empty : outputPort.Trim();
            InputNodeId = string.IsNullOrWhiteSpace(inputNodeId) ? string.Empty : inputNodeId.Trim();
            InputPort = string.IsNullOrWhiteSpace(inputPort) ? string.Empty : inputPort.Trim();
        }
    }

    public sealed class SaveGraphRequest : ShaderGraphRequest
    {
        public SaveGraphRequest(string assetPath)
            : base(ShaderGraphAction.SaveGraph, assetPath)
        {
        }
    }

    [Serializable]
    public sealed class ImportedGraphContract
    {
        public string contractVersion;
        public string assetPath;
        public string assetName;
        public string template;
        public string graphPathLabel;
        public string graphDefaultPrecision;
        public ImportedGraphContractCategory[] categories = Array.Empty<ImportedGraphContractCategory>();
        public ImportedGraphContractProperty[] properties = Array.Empty<ImportedGraphContractProperty>();
        public ImportedGraphContractNode[] nodes = Array.Empty<ImportedGraphContractNode>();
        public ImportedGraphContractConnection[] connections = Array.Empty<ImportedGraphContractConnection>();
    }

    [Serializable]
    public sealed class ImportedGraphContractCategory
    {
        public string categoryGuid;
        public string displayName;
        public string name;
        public string[] propertyOrder = Array.Empty<string>();
    }

    [Serializable]
    public sealed class ImportedGraphContractProperty
    {
        public string objectId;
        public string displayName;
        public string referenceName;
        public string resolvedPropertyType;
        public string resolvedShaderInputType;
        public string defaultValue;
        public string categoryGuid;
        public string categoryDisplayName;
    }

    [Serializable]
    public sealed class ImportedGraphContractNode
    {
        public string objectId;
        public string nodeId;
        public string displayName;
        public string nodeType;
        public ImportedGraphContractPosition position;
    }

    [Serializable]
    public sealed class ImportedGraphContractPosition
    {
        public float x;
        public float y;
    }

    [Serializable]
    public sealed class ImportedGraphContractConnection
    {
        public string outputNodeId;
        public string outputPort;
        public string inputNodeId;
        public string inputPort;
    }

    public static class ImportedGraphContractJsonUtility
    {
        public static bool TryParse(
            string graphContractJson,
            out ImportedGraphContract contract,
            out string failureReason)
        {
            contract = null;

            if (string.IsNullOrWhiteSpace(graphContractJson))
            {
                failureReason = "Graph contract JSON is required.";
                return false;
            }

            try
            {
                contract = JsonUtility.FromJson<ImportedGraphContract>(graphContractJson);
            }
            catch (Exception exception)
            {
                failureReason = $"Failed to parse graph contract JSON: {exception.Message}";
                return false;
            }

            if (contract == null)
            {
                failureReason = "Graph contract JSON did not produce a contract payload.";
                return false;
            }

            contract.categories ??= Array.Empty<ImportedGraphContractCategory>();
            contract.properties ??= Array.Empty<ImportedGraphContractProperty>();
            contract.nodes ??= Array.Empty<ImportedGraphContractNode>();
            contract.connections ??= Array.Empty<ImportedGraphContractConnection>();
            foreach (ImportedGraphContractNode node in contract.nodes)
            {
                if (node == null)
                {
                    continue;
                }

                node.position ??= new ImportedGraphContractPosition();
            }

            failureReason = null;
            return true;
        }
    }
}
