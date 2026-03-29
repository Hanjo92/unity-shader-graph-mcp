using System.IO;

namespace ShaderGraphMcp.Editor.Models
{
    public enum ShaderGraphAction
    {
        CreateGraph,
        CreateCategory,
        RenameCategory,
        FindCategory,
        DeleteCategory,
        ReadGraphSummary,
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

    public sealed class ReadGraphSummaryRequest : ShaderGraphRequest
    {
        public ReadGraphSummaryRequest(string assetPath)
            : base(ShaderGraphAction.ReadGraphSummary, assetPath)
        {
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
        public float X { get; }
        public float Y { get; }

        public MoveNodeRequest(string assetPath, string nodeId, float x, float y)
            : base(ShaderGraphAction.MoveNode, assetPath)
        {
            NodeId = string.IsNullOrWhiteSpace(nodeId) ? string.Empty : nodeId.Trim();
            X = x;
            Y = y;
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

        public AddNodeRequest(string assetPath, string nodeType, string displayName)
            : base(ShaderGraphAction.AddNode, assetPath)
        {
            NodeType = nodeType;
            DisplayName = displayName;
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
}
