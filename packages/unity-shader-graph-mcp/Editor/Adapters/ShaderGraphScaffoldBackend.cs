using ShaderGraphMcp.Editor.Helpers;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    internal sealed class ShaderGraphScaffoldBackend : IShaderGraphBackend
    {
        public ShaderGraphExecutionKind ExecutionKind => ShaderGraphExecutionKind.Scaffold;

        public ShaderGraphResponse CreateGraph(CreateGraphRequest request)
        {
            return ShaderGraphScaffoldStore.CreateGraph(request, ExecutionKind);
        }

        public ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request)
        {
            return ShaderGraphScaffoldStore.ReadGraphSummary(request, ExecutionKind);
        }

        public ShaderGraphResponse FindNode(FindNodeRequest request)
        {
            return ShaderGraphScaffoldStore.FindNode(request, ExecutionKind);
        }

        public ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request)
        {
            return ShaderGraphScaffoldStore.ListSupportedNodes(request, ExecutionKind);
        }

        public ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request)
        {
            return ShaderGraphScaffoldStore.UpdateProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse MoveNode(MoveNodeRequest request)
        {
            return ShaderGraphScaffoldStore.MoveNode(request, ExecutionKind);
        }

        public ShaderGraphResponse DeleteNode(DeleteNodeRequest request)
        {
            return ShaderGraphScaffoldStore.DeleteNode(request, ExecutionKind);
        }

        public ShaderGraphResponse AddProperty(AddPropertyRequest request)
        {
            return ShaderGraphScaffoldStore.AddProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse AddNode(AddNodeRequest request)
        {
            return ShaderGraphScaffoldStore.AddNode(request, ExecutionKind);
        }

        public ShaderGraphResponse ConnectPorts(ConnectPortsRequest request)
        {
            return ShaderGraphScaffoldStore.ConnectPorts(request, ExecutionKind);
        }

        public ShaderGraphResponse SaveGraph(SaveGraphRequest request)
        {
            return ShaderGraphScaffoldStore.SaveGraph(request, ExecutionKind);
        }
    }
}
