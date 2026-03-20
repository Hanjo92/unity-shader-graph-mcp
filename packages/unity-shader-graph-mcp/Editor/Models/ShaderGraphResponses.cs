using System;
using System.Collections.Generic;

namespace ShaderGraphMcp.Editor.Models
{
    public class ShaderGraphResponse
    {
        public bool Success { get; }
        public string Message { get; }
        public IReadOnlyDictionary<string, object> Data { get; }

        public ShaderGraphResponse(bool success, string message, IReadOnlyDictionary<string, object> data = null)
        {
            Success = success;
            Message = message;
            Data = data ?? new Dictionary<string, object>();
        }

        public static ShaderGraphResponse Ok(string message, IReadOnlyDictionary<string, object> data = null)
        {
            return new ShaderGraphResponse(true, message, data);
        }

        public static ShaderGraphResponse Fail(string message)
        {
            return new ShaderGraphResponse(false, message);
        }

        public static ShaderGraphResponse Fail(string message, IReadOnlyDictionary<string, object> data)
        {
            return new ShaderGraphResponse(false, message, data);
        }
    }

    public sealed class ShaderGraphSummary
    {
        public string AssetPath { get; }
        public string GraphName { get; }
        public IReadOnlyList<string> Properties { get; }
        public IReadOnlyList<string> Nodes { get; }
        public IReadOnlyList<string> Connections { get; }

        public ShaderGraphSummary(
            string assetPath,
            string graphName,
            IReadOnlyList<string> properties,
            IReadOnlyList<string> nodes,
            IReadOnlyList<string> connections
        )
        {
            AssetPath = assetPath;
            GraphName = graphName;
            Properties = properties ?? Array.Empty<string>();
            Nodes = nodes ?? Array.Empty<string>();
            Connections = connections ?? Array.Empty<string>();
        }
    }
}
