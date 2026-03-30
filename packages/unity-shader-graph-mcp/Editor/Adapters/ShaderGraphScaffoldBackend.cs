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

        public ShaderGraphResponse RenameGraph(RenameGraphRequest request)
        {
            return ShaderGraphScaffoldStore.RenameGraph(request, ExecutionKind);
        }

        public ShaderGraphResponse CreateCategory(CreateCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.CreateCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse RenameCategory(RenameCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.RenameCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse FindCategory(FindCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.FindCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse DeleteCategory(DeleteCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.DeleteCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse ReorderCategory(ReorderCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.ReorderCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse MergeCategory(MergeCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.MergeCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse DuplicateCategory(DuplicateCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.DuplicateCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse SplitCategory(SplitCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.SplitCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse ListCategories(ListCategoriesRequest request)
        {
            return ShaderGraphScaffoldStore.ListCategories(request, ExecutionKind);
        }

        public ShaderGraphResponse ReadGraphSummary(ReadGraphSummaryRequest request)
        {
            return ShaderGraphScaffoldStore.ReadGraphSummary(request, ExecutionKind);
        }

        public ShaderGraphResponse FindNode(FindNodeRequest request)
        {
            return ShaderGraphScaffoldStore.FindNode(request, ExecutionKind);
        }

        public ShaderGraphResponse FindProperty(FindPropertyRequest request)
        {
            return ShaderGraphScaffoldStore.FindProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse ListSupportedNodes(ListSupportedNodesRequest request)
        {
            return ShaderGraphScaffoldStore.ListSupportedNodes(request, ExecutionKind);
        }

        public ShaderGraphResponse ListSupportedProperties(ListSupportedPropertiesRequest request)
        {
            return ShaderGraphScaffoldStore.ListSupportedProperties(request, ExecutionKind);
        }

        public ShaderGraphResponse ListSupportedConnections(ListSupportedConnectionsRequest request)
        {
            return ShaderGraphScaffoldStore.ListSupportedConnections(request, ExecutionKind);
        }

        public ShaderGraphResponse UpdateProperty(UpdatePropertyRequest request)
        {
            return ShaderGraphScaffoldStore.UpdateProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse RenameProperty(RenamePropertyRequest request)
        {
            return ShaderGraphScaffoldStore.RenameProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse DuplicateProperty(DuplicatePropertyRequest request)
        {
            return ShaderGraphScaffoldStore.DuplicateProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse ReorderProperty(ReorderPropertyRequest request)
        {
            return ShaderGraphScaffoldStore.ReorderProperty(request, ExecutionKind);
        }

        public ShaderGraphResponse MovePropertyToCategory(MovePropertyToCategoryRequest request)
        {
            return ShaderGraphScaffoldStore.MovePropertyToCategory(request, ExecutionKind);
        }

        public ShaderGraphResponse RenameNode(RenameNodeRequest request)
        {
            return ShaderGraphScaffoldStore.RenameNode(request, ExecutionKind);
        }

        public ShaderGraphResponse DuplicateNode(DuplicateNodeRequest request)
        {
            return ShaderGraphScaffoldStore.DuplicateNode(request, ExecutionKind);
        }

        public ShaderGraphResponse MoveNode(MoveNodeRequest request)
        {
            return ShaderGraphScaffoldStore.MoveNode(request, ExecutionKind);
        }

        public ShaderGraphResponse DeleteNode(DeleteNodeRequest request)
        {
            return ShaderGraphScaffoldStore.DeleteNode(request, ExecutionKind);
        }

        public ShaderGraphResponse RemoveProperty(RemovePropertyRequest request)
        {
            return ShaderGraphScaffoldStore.RemoveProperty(request, ExecutionKind);
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

        public ShaderGraphResponse FindConnection(FindConnectionRequest request)
        {
            return ShaderGraphScaffoldStore.FindConnection(request, ExecutionKind);
        }

        public ShaderGraphResponse RemoveConnection(RemoveConnectionRequest request)
        {
            return ShaderGraphScaffoldStore.RemoveConnection(request, ExecutionKind);
        }

        public ShaderGraphResponse ReconnectConnection(ReconnectConnectionRequest request)
        {
            return ShaderGraphScaffoldStore.ReconnectConnection(request, ExecutionKind);
        }

        public ShaderGraphResponse SaveGraph(SaveGraphRequest request)
        {
            return ShaderGraphScaffoldStore.SaveGraph(request, ExecutionKind);
        }
    }
}
