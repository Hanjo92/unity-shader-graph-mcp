using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    public interface IShaderGraphAdapter
    {
        ShaderGraphResponse CreateGraph(CreateGraphRequest request);
        ShaderGraphResponse CreateSubGraph(CreateSubGraphRequest request);
        ShaderGraphResponse RenameGraph(RenameGraphRequest request);
        ShaderGraphResponse RenameSubGraph(RenameSubGraphRequest request);
        ShaderGraphResponse DuplicateGraph(DuplicateGraphRequest request);
        ShaderGraphResponse DeleteGraph(DeleteGraphRequest request);
        ShaderGraphResponse MoveGraph(MoveGraphRequest request);
        ShaderGraphResponse SetGraphMetadata(SetGraphMetadataRequest request);
        ShaderGraphResponse CreateCategory(CreateCategoryRequest request);
        ShaderGraphResponse RenameCategory(RenameCategoryRequest request);
        ShaderGraphResponse FindCategory(FindCategoryRequest request);
        ShaderGraphResponse DeleteCategory(DeleteCategoryRequest request);
        ShaderGraphResponse ReorderCategory(ReorderCategoryRequest request);
        ShaderGraphResponse MergeCategory(MergeCategoryRequest request);
        ShaderGraphResponse DuplicateCategory(DuplicateCategoryRequest request);
        ShaderGraphResponse SplitCategory(SplitCategoryRequest request);
        ShaderGraphResponse ListCategories(ListCategoriesRequest request);
        ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request);
        ShaderGraphResponse ReadSubGraphSummary(ReadSubGraphSummaryRequest request);
        ShaderGraphResponse ExportGraphContract(ExportGraphContractRequest request);
        ShaderGraphResponse ImportGraphContract(ImportGraphContractRequest request);
        ShaderGraphResponse FindNode(FindNodeRequest request);
        ShaderGraphResponse FindProperty(FindPropertyRequest request);
        ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request);
        ShaderGraphResponse ListSupportedProperties(ListSupportedPropertiesRequest request);
        ShaderGraphResponse ListSupportedConnections(ListSupportedConnectionsRequest request);
        ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request);
        ShaderGraphResponse RenameProperty(RenamePropertyRequest request);
        ShaderGraphResponse DuplicateProperty(DuplicatePropertyRequest request);
        ShaderGraphResponse ReorderProperty(ReorderPropertyRequest request);
        ShaderGraphResponse MovePropertyToCategory(MovePropertyToCategoryRequest request);
        ShaderGraphResponse RenameNode(RenameNodeRequest request);
        ShaderGraphResponse DuplicateNode(DuplicateNodeRequest request);
        ShaderGraphResponse MoveNode(MoveNodeRequest request);
        ShaderGraphResponse DeleteNode(DeleteNodeRequest request);
        ShaderGraphResponse RemoveProperty(RemovePropertyRequest request);
        ShaderGraphResponse AddProperty(AddPropertyRequest request);
        ShaderGraphResponse AddNode(AddNodeRequest request);
        ShaderGraphResponse ConnectPorts(ConnectPortsRequest request);
        ShaderGraphResponse FindConnection(FindConnectionRequest request);
        ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request);
        ShaderGraphResponse ReconnectConnection(ReconnectConnectionRequest request);
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

        public ShaderGraphResponse CreateSubGraph(CreateSubGraphRequest request)
        {
            return backend.CreateSubGraph(request);
        }

        public ShaderGraphResponse RenameGraph(RenameGraphRequest request)
        {
            return backend.RenameGraph(request);
        }

        public ShaderGraphResponse RenameSubGraph(RenameSubGraphRequest request)
        {
            return backend.RenameSubGraph(request);
        }

        public ShaderGraphResponse DuplicateGraph(DuplicateGraphRequest request)
        {
            return backend.DuplicateGraph(request);
        }

        public ShaderGraphResponse DeleteGraph(DeleteGraphRequest request)
        {
            return backend.DeleteGraph(request);
        }

        public ShaderGraphResponse MoveGraph(MoveGraphRequest request)
        {
            return backend.MoveGraph(request);
        }

        public ShaderGraphResponse SetGraphMetadata(SetGraphMetadataRequest request)
        {
            return backend.SetGraphMetadata(request);
        }

        public ShaderGraphResponse CreateCategory(CreateCategoryRequest request)
        {
            return backend.CreateCategory(request);
        }

        public ShaderGraphResponse RenameCategory(RenameCategoryRequest request)
        {
            return backend.RenameCategory(request);
        }

        public ShaderGraphResponse FindCategory(FindCategoryRequest request)
        {
            return backend.FindCategory(request);
        }

        public ShaderGraphResponse DeleteCategory(DeleteCategoryRequest request)
        {
            return backend.DeleteCategory(request);
        }

        public ShaderGraphResponse ReorderCategory(ReorderCategoryRequest request)
        {
            return backend.ReorderCategory(request);
        }

        public ShaderGraphResponse MergeCategory(MergeCategoryRequest request)
        {
            return backend.MergeCategory(request);
        }

        public ShaderGraphResponse DuplicateCategory(DuplicateCategoryRequest request)
        {
            return backend.DuplicateCategory(request);
        }

        public ShaderGraphResponse SplitCategory(SplitCategoryRequest request)
        {
            return backend.SplitCategory(request);
        }

        public ShaderGraphResponse ListCategories(ListCategoriesRequest request)
        {
            return backend.ListCategories(request);
        }

        public ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request)
        {
            return backend.ReadGraphSummary(request);
        }

        public ShaderGraphResponse ReadSubGraphSummary(ReadSubGraphSummaryRequest request)
        {
            return backend.ReadSubGraphSummary(request);
        }

        public ShaderGraphResponse ExportGraphContract(ExportGraphContractRequest request)
        {
            return backend.ExportGraphContract(request);
        }

        public ShaderGraphResponse ImportGraphContract(ImportGraphContractRequest request)
        {
            return backend.ImportGraphContract(request);
        }

        public ShaderGraphResponse FindNode(FindNodeRequest request)
        {
            return backend.FindNode(request);
        }

        public ShaderGraphResponse FindProperty(FindPropertyRequest request)
        {
            return backend.FindProperty(request);
        }

        public ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request)
        {
            return backend.ListSupportedNodes(request);
        }

        public ShaderGraphResponse ListSupportedProperties(ListSupportedPropertiesRequest request)
        {
            return backend.ListSupportedProperties(request);
        }

        public ShaderGraphResponse ListSupportedConnections(ListSupportedConnectionsRequest request)
        {
            return backend.ListSupportedConnections(request);
        }

        public ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request)
        {
            return backend.UpdateProperty(request);
        }

        public ShaderGraphResponse RenameProperty(RenamePropertyRequest request)
        {
            return backend.RenameProperty(request);
        }

        public ShaderGraphResponse DuplicateProperty(DuplicatePropertyRequest request)
        {
            return backend.DuplicateProperty(request);
        }

        public ShaderGraphResponse ReorderProperty(ReorderPropertyRequest request)
        {
            return backend.ReorderProperty(request);
        }

        public ShaderGraphResponse MovePropertyToCategory(MovePropertyToCategoryRequest request)
        {
            return backend.MovePropertyToCategory(request);
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

        public ShaderGraphResponse FindConnection(FindConnectionRequest request)
        {
            return backend.FindConnection(request);
        }

        public ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request)
        {
            return backend.RemoveConnection(request);
        }

        public ShaderGraphResponse ReconnectConnection(ReconnectConnectionRequest request)
        {
            return backend.ReconnectConnection(request);
        }

        public ShaderGraphResponse SaveGraph(SaveGraphRequest request)
        {
            return backend.SaveGraph(request);
        }
    }
}
