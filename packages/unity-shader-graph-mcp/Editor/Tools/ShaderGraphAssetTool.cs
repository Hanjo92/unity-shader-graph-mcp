using ShaderGraphMcp.Editor.Adapters;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Tools
{
    public static class ShaderGraphAssetTool
    {
        private static readonly IShaderGraphAdapter Adapter = new ShaderGraphAdapter();

        public static ShaderGraphResponse Handle(ShaderGraphRequest request)
        {
            if (request == null)
            {
                return ShaderGraphResponse.Fail("Request is required.");
            }

            if (string.IsNullOrWhiteSpace(request.AssetPath) &&
                request.Action != ShaderGraphAction.CreateGraph &&
                request.Action != ShaderGraphAction.ListSupportedNodes)
            {
                return ShaderGraphResponse.Fail("Asset path is required.");
            }

            switch (request)
            {
                case CreateGraphRequest createGraphRequest:
                    return Adapter.CreateGraph(createGraphRequest);
                case ReadGraphSummaryRequest readGraphSummaryRequest:
                    return Adapter.ReadGraphSummary(readGraphSummaryRequest);
                case FindNodeRequest findNodeRequest:
                    return Adapter.FindNode(findNodeRequest);
                case ListSupportedNodesRequest listSupportedNodesRequest:
                    return Adapter.ListSupportedNodes(listSupportedNodesRequest);
                case UpdatePropertyRequest updatePropertyRequest:
                    return Adapter.UpdateProperty(updatePropertyRequest);
                case MoveNodeRequest moveNodeRequest:
                    return Adapter.MoveNode(moveNodeRequest);
                case AddPropertyRequest addPropertyRequest:
                    return Adapter.AddProperty(addPropertyRequest);
                case AddNodeRequest addNodeRequest:
                    return Adapter.AddNode(addNodeRequest);
                case ConnectPortsRequest connectPortsRequest:
                    return Adapter.ConnectPorts(connectPortsRequest);
                case SaveGraphRequest saveGraphRequest:
                    return Adapter.SaveGraph(saveGraphRequest);
                default:
                    return ShaderGraphResponse.Fail(
                        $"Unsupported Shader Graph action: {request.Action}. Supported actions: create_graph, read_graph_summary, find_node, list_supported_nodes, update_property, move_node, add_property, add_node, connect_ports, save_graph."
                    );
            }
        }

        public static ShaderGraphResponse HandleCreateGraph(string name, string path, string template)
        {
            return Handle(new CreateGraphRequest(name, path, template));
        }

        public static ShaderGraphResponse HandleReadGraphSummary(string assetPath)
        {
            return Handle(new ReadGraphSummaryRequest(assetPath));
        }

        public static ShaderGraphResponse HandleFindNode(
            string assetPath,
            string nodeId,
            string displayName,
            string nodeType)
        {
            return Handle(new FindNodeRequest(assetPath, nodeId, displayName, nodeType));
        }

        public static ShaderGraphResponse HandleListSupportedNodes()
        {
            return Handle(new ListSupportedNodesRequest());
        }

        public static ShaderGraphResponse HandleUpdateProperty(
            string assetPath,
            string propertyName,
            string propertyType,
            string defaultValue)
        {
            return Handle(new UpdatePropertyRequest(assetPath, propertyName, propertyType, defaultValue));
        }

        public static ShaderGraphResponse HandleMoveNode(
            string assetPath,
            string nodeId,
            float x,
            float y)
        {
            return Handle(new MoveNodeRequest(assetPath, nodeId, x, y));
        }

        public static ShaderGraphResponse HandleAddProperty(string assetPath, string propertyName, string propertyType, string defaultValue)
        {
            return Handle(new AddPropertyRequest(assetPath, propertyName, propertyType, defaultValue));
        }

        public static ShaderGraphResponse HandleAddNode(string assetPath, string nodeType, string displayName)
        {
            return Handle(new AddNodeRequest(assetPath, nodeType, displayName));
        }

        public static ShaderGraphResponse HandleConnectPorts(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort
        )
        {
            return Handle(new ConnectPortsRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort));
        }

        public static ShaderGraphResponse HandleSaveGraph(string assetPath)
        {
            return Handle(new SaveGraphRequest(assetPath));
        }
    }
}
