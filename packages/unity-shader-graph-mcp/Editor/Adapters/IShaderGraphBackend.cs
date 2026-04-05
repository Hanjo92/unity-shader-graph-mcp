using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    internal interface IShaderGraphBackend
    {
        ShaderGraphExecutionKind ExecutionKind { get; }
        ShaderGraphResponse CreateGraph(CreateGraphRequest request);
        ShaderGraphResponse RenameGraph(RenameGraphRequest request);
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
}
