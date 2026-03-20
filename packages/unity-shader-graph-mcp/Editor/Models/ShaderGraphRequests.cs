using System.IO;

namespace ShaderGraphMcp.Editor.Models
{
    public enum ShaderGraphAction
    {
        CreateGraph,
        ReadGraphSummary,
        AddProperty,
        AddNode,
        ConnectPorts,
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

    public sealed class SaveGraphRequest : ShaderGraphRequest
    {
        public SaveGraphRequest(string assetPath)
            : base(ShaderGraphAction.SaveGraph, assetPath)
        {
        }
    }
}
