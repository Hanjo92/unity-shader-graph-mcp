using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    internal interface IShaderGraphBackend
    {
        ShaderGraphExecutionKind ExecutionKind { get; }
        ShaderGraphResponse CreateGraph(CreateGraphRequest request);
        ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request);
        ShaderGraphResponse FindNode(FindNodeRequest request);
        ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request);
        ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request);
        ShaderGraphResponse RenameNode(RenameNodeRequest request);
        ShaderGraphResponse MoveNode(MoveNodeRequest request);
        ShaderGraphResponse DeleteNode(DeleteNodeRequest request);
        ShaderGraphResponse RemoveProperty(RemovePropertyRequest request);
        ShaderGraphResponse AddProperty(AddPropertyRequest request);
        ShaderGraphResponse AddNode(AddNodeRequest request);
        ShaderGraphResponse ConnectPorts(ConnectPortsRequest request);
        ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request);
        ShaderGraphResponse SaveGraph(SaveGraphRequest request);
    }
}
