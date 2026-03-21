using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    public interface IShaderGraphAdapter
    {
        ShaderGraphResponse CreateGraph(CreateGraphRequest request);
        ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request);
        ShaderGraphResponse FindNode(FindNodeRequest request);
        ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request);
        ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request);
        ShaderGraphResponse RenameNode(RenameNodeRequest request);
        ShaderGraphResponse DuplicateNode(DuplicateNodeRequest request);
        ShaderGraphResponse MoveNode(MoveNodeRequest request);
        ShaderGraphResponse DeleteNode(DeleteNodeRequest request);
        ShaderGraphResponse RemoveProperty(RemovePropertyRequest request);
        ShaderGraphResponse AddProperty(AddPropertyRequest request);
        ShaderGraphResponse AddNode(AddNodeRequest request);
        ShaderGraphResponse ConnectPorts(ConnectPortsRequest request);
        ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request);
        ShaderGraphResponse SaveGraph(SaveGraphRequest request);
    }

    public sealed class ShaderGraphAdapter : IShaderGraphAdapter
    {
        private readonly IShaderGraphBackend backend;

        public ShaderGraphAdapter()
            : this(ShaderGraphBackendFactory.CreateDefault())
        {
        }

        internal ShaderGraphAdapter(IShaderGraphBackend backend)
        {
            this.backend = backend;
        }

        public ShaderGraphResponse CreateGraph(CreateGraphRequest request)
        {
            return backend.CreateGraph(request);
        }

        public ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request)
        {
            return backend.ReadGraphSummary(request);
        }

        public ShaderGraphResponse FindNode(FindNodeRequest request)
        {
            return backend.FindNode(request);
        }

        public ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request)
        {
            return backend.ListSupportedNodes(request);
        }

        public ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request)
        {
            return backend.UpdateProperty(request);
        }

        public ShaderGraphResponse RenameNode(RenameNodeRequest request)
        {
            return backend.RenameNode(request);
        }

        public ShaderGraphResponse DuplicateNode(DuplicateNodeRequest request)
        {
            return backend.DuplicateNode(request);
        }

        public ShaderGraphResponse MoveNode(MoveNodeRequest request)
        {
            return backend.MoveNode(request);
        }

        public ShaderGraphResponse DeleteNode(DeleteNodeRequest request)
        {
            return backend.DeleteNode(request);
        }

        public ShaderGraphResponse RemoveProperty(RemovePropertyRequest request)
        {
            return backend.RemoveProperty(request);
        }

        public ShaderGraphResponse AddProperty(AddPropertyRequest request)
        {
            return backend.AddProperty(request);
        }

        public ShaderGraphResponse AddNode(AddNodeRequest request)
        {
            return backend.AddNode(request);
        }

        public ShaderGraphResponse ConnectPorts(ConnectPortsRequest request)
        {
            return backend.ConnectPorts(request);
        }

        public ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request)
        {
            return backend.RemoveConnection(request);
        }

        public ShaderGraphResponse SaveGraph(SaveGraphRequest request)
        {
            return backend.SaveGraph(request);
        }
    }
}
