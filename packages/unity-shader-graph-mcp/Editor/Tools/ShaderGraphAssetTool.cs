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
                request.Action != ShaderGraphAction.CreateSubGraph &&
                request.Action != ShaderGraphAction.ListSupportedNodes &&
                request.Action != ShaderGraphAction.ListSupportedProperties &&
                request.Action != ShaderGraphAction.ListSupportedConnections)
            {
                return ShaderGraphResponse.Fail("Asset path is required.");
            }

            switch (request)
            {
                case CreateGraphRequest createGraphRequest:
                    return Adapter.CreateGraph(createGraphRequest);
                case CreateSubGraphRequest createSubGraphRequest:
                    return Adapter.CreateSubGraph(createSubGraphRequest);
                case RenameGraphRequest renameGraphRequest:
                    return Adapter.RenameGraph(renameGraphRequest);
                case RenameSubGraphRequest renameSubGraphRequest:
                    return Adapter.RenameSubGraph(renameSubGraphRequest);
                case DuplicateGraphRequest duplicateGraphRequest:
                    return Adapter.DuplicateGraph(duplicateGraphRequest);
                case DuplicateSubGraphRequest duplicateSubGraphRequest:
                    return Adapter.DuplicateSubGraph(duplicateSubGraphRequest);
                case DeleteGraphRequest deleteGraphRequest:
                    return Adapter.DeleteGraph(deleteGraphRequest);
                case DeleteSubGraphRequest deleteSubGraphRequest:
                    return Adapter.DeleteSubGraph(deleteSubGraphRequest);
                case MoveGraphRequest moveGraphRequest:
                    return Adapter.MoveGraph(moveGraphRequest);
                case SetGraphMetadataRequest setGraphMetadataRequest:
                    return Adapter.SetGraphMetadata(setGraphMetadataRequest);
                case CreateCategoryRequest createCategoryRequest:
                    return Adapter.CreateCategory(createCategoryRequest);
                case RenameCategoryRequest renameCategoryRequest:
                    return Adapter.RenameCategory(renameCategoryRequest);
                case FindCategoryRequest findCategoryRequest:
                    return Adapter.FindCategory(findCategoryRequest);
                case DeleteCategoryRequest deleteCategoryRequest:
                    return Adapter.DeleteCategory(deleteCategoryRequest);
                case ReorderCategoryRequest reorderCategoryRequest:
                    return Adapter.ReorderCategory(reorderCategoryRequest);
                case MergeCategoryRequest mergeCategoryRequest:
                    return Adapter.MergeCategory(mergeCategoryRequest);
                case DuplicateCategoryRequest duplicateCategoryRequest:
                    return Adapter.DuplicateCategory(duplicateCategoryRequest);
                case SplitCategoryRequest splitCategoryRequest:
                    return Adapter.SplitCategory(splitCategoryRequest);
                case ListCategoriesRequest listCategoriesRequest:
                    return Adapter.ListCategories(listCategoriesRequest);
                case ReadGraphSummaryRequest readGraphSummaryRequest:
                    return Adapter.ReadGraphSummary(readGraphSummaryRequest);
                case ReadSubGraphSummaryRequest readSubGraphSummaryRequest:
                    return Adapter.ReadSubGraphSummary(readSubGraphSummaryRequest);
                case ExportGraphContractRequest exportGraphContractRequest:
                    return Adapter.ExportGraphContract(exportGraphContractRequest);
                case ImportGraphContractRequest importGraphContractRequest:
                    return Adapter.ImportGraphContract(importGraphContractRequest);
                case FindNodeRequest findNodeRequest:
                    return Adapter.FindNode(findNodeRequest);
                case FindPropertyRequest findPropertyRequest:
                    return Adapter.FindProperty(findPropertyRequest);
                case ListSupportedNodesRequest listSupportedNodesRequest:
                    return Adapter.ListSupportedNodes(listSupportedNodesRequest);
                case ListSupportedPropertiesRequest listSupportedPropertiesRequest:
                    return Adapter.ListSupportedProperties(listSupportedPropertiesRequest);
                case ListSupportedConnectionsRequest listSupportedConnectionsRequest:
                    return Adapter.ListSupportedConnections(listSupportedConnectionsRequest);
                case UpdatePropertyRequest updatePropertyRequest:
                    return Adapter.UpdateProperty(updatePropertyRequest);
                case RenamePropertyRequest renamePropertyRequest:
                    return Adapter.RenameProperty(renamePropertyRequest);
                case DuplicatePropertyRequest duplicatePropertyRequest:
                    return Adapter.DuplicateProperty(duplicatePropertyRequest);
                case ReorderPropertyRequest reorderPropertyRequest:
                    return Adapter.ReorderProperty(reorderPropertyRequest);
                case MovePropertyToCategoryRequest movePropertyToCategoryRequest:
                    return Adapter.MovePropertyToCategory(movePropertyToCategoryRequest);
                case RenameNodeRequest renameNodeRequest:
                    return Adapter.RenameNode(renameNodeRequest);
                case DuplicateNodeRequest duplicateNodeRequest:
                    return Adapter.DuplicateNode(duplicateNodeRequest);
                case MoveNodeRequest moveNodeRequest:
                    return Adapter.MoveNode(moveNodeRequest);
                case DeleteNodeRequest deleteNodeRequest:
                    return Adapter.DeleteNode(deleteNodeRequest);
                case RemovePropertyRequest removePropertyRequest:
                    return Adapter.RemoveProperty(removePropertyRequest);
                case AddPropertyRequest addPropertyRequest:
                    return Adapter.AddProperty(addPropertyRequest);
                case AddNodeRequest addNodeRequest:
                    return Adapter.AddNode(addNodeRequest);
                case ConnectPortsRequest connectPortsRequest:
                    return Adapter.ConnectPorts(connectPortsRequest);
                case FindConnectionRequest findConnectionRequest:
                    return Adapter.FindConnection(findConnectionRequest);
                case RemoveConnectionRequest removeConnectionRequest:
                    return Adapter.RemoveConnection(removeConnectionRequest);
                case ReconnectConnectionRequest reconnectConnectionRequest:
                    return Adapter.ReconnectConnection(reconnectConnectionRequest);
                case SaveGraphRequest saveGraphRequest:
                    return Adapter.SaveGraph(saveGraphRequest);
                default:
                    return ShaderGraphResponse.Fail(
                        $"Unsupported Shader Graph action: {request.Action}. Supported actions: create_graph, create_subgraph, rename_graph, rename_subgraph, duplicate_graph, duplicate_subgraph, delete_graph, delete_subgraph, move_graph, set_graph_metadata, create_category, rename_category, find_category, delete_category, reorder_category, merge_category, duplicate_category, split_category, list_categories, read_graph_summary, read_subgraph_summary, export_graph_contract, import_graph_contract, find_node, find_property, list_supported_nodes, list_supported_properties, list_supported_connections, update_property, rename_property, duplicate_property, reorder_property, move_property_to_category, rename_node, duplicate_node, move_node, delete_node, remove_property, add_property, add_node, connect_ports, find_connection, remove_connection, reconnect_connection, save_graph."
                    );
            }
        }

        public static ShaderGraphResponse HandleCreateGraph(string name, string path, string template)
        {
            return Handle(new CreateGraphRequest(name, path, template));
        }

        public static ShaderGraphResponse HandleCreateSubGraph(string name, string path, string template)
        {
            return Handle(new CreateSubGraphRequest(name, path, template));
        }

        public static ShaderGraphResponse HandleRenameGraph(string assetPath, string name)
        {
            return Handle(new RenameGraphRequest(assetPath, name));
        }

        public static ShaderGraphResponse HandleRenameSubGraph(string assetPath, string name)
        {
            return Handle(new RenameSubGraphRequest(assetPath, name));
        }

        public static ShaderGraphResponse HandleDuplicateGraph(string assetPath, string name)
        {
            return Handle(new DuplicateGraphRequest(assetPath, name));
        }

        public static ShaderGraphResponse HandleDuplicateSubGraph(string assetPath, string name)
        {
            return Handle(new DuplicateSubGraphRequest(assetPath, name));
        }

        public static ShaderGraphResponse HandleDeleteGraph(string assetPath)
        {
            return Handle(new DeleteGraphRequest(assetPath));
        }

        public static ShaderGraphResponse HandleDeleteSubGraph(string assetPath)
        {
            return Handle(new DeleteSubGraphRequest(assetPath));
        }

        public static ShaderGraphResponse HandleMoveGraph(string assetPath, string targetPath)
        {
            return Handle(new MoveGraphRequest(assetPath, targetPath));
        }

        public static ShaderGraphResponse HandleSetGraphMetadata(
            string assetPath,
            string graphPathLabel,
            string graphDefaultPrecision)
        {
            return Handle(new SetGraphMetadataRequest(assetPath, graphPathLabel, graphDefaultPrecision));
        }

        public static ShaderGraphResponse HandleCreateCategory(string assetPath, string name)
        {
            return Handle(new CreateCategoryRequest(assetPath, name));
        }

        public static ShaderGraphResponse HandleRenameCategory(
            string assetPath,
            string categoryGuid,
            string categoryName,
            string displayName)
        {
            return Handle(new RenameCategoryRequest(assetPath, categoryGuid, categoryName, displayName));
        }

        public static ShaderGraphResponse HandleFindCategory(
            string assetPath,
            string categoryGuid,
            string categoryName)
        {
            return Handle(new FindCategoryRequest(assetPath, categoryGuid, categoryName));
        }

        public static ShaderGraphResponse HandleDeleteCategory(
            string assetPath,
            string categoryGuid,
            string categoryName)
        {
            return Handle(new DeleteCategoryRequest(assetPath, categoryGuid, categoryName));
        }

        public static ShaderGraphResponse HandleReorderCategory(
            string assetPath,
            string categoryGuid,
            string categoryName,
            int index)
        {
            return Handle(new ReorderCategoryRequest(assetPath, categoryGuid, categoryName, index));
        }

        public static ShaderGraphResponse HandleMergeCategory(
            string assetPath,
            string sourceCategoryGuid,
            string sourceCategoryName,
            string targetCategoryGuid,
            string targetCategoryName)
        {
            return Handle(new MergeCategoryRequest(
                assetPath,
                sourceCategoryGuid,
                sourceCategoryName,
                targetCategoryGuid,
                targetCategoryName));
        }

        public static ShaderGraphResponse HandleDuplicateCategory(
            string assetPath,
            string categoryGuid,
            string categoryName,
            string displayName)
        {
            return Handle(new DuplicateCategoryRequest(assetPath, categoryGuid, categoryName, displayName));
        }

        public static ShaderGraphResponse HandleSplitCategory(
            string assetPath,
            string sourceCategoryGuid,
            string sourceCategoryName,
            string displayName,
            params string[] propertyNames)
        {
            return Handle(new SplitCategoryRequest(assetPath, sourceCategoryGuid, sourceCategoryName, displayName, propertyNames));
        }

        public static ShaderGraphResponse HandleListCategories(string assetPath)
        {
            return Handle(new ListCategoriesRequest(assetPath));
        }

        public static ShaderGraphResponse HandleReadGraphSummary(string assetPath)
        {
            return Handle(new ReadGraphSummaryRequest(assetPath));
        }

        public static ShaderGraphResponse HandleReadSubGraphSummary(string assetPath)
        {
            return Handle(new ReadSubGraphSummaryRequest(assetPath));
        }

        public static ShaderGraphResponse HandleExportGraphContract(string assetPath)
        {
            return Handle(new ExportGraphContractRequest(assetPath));
        }

        public static ShaderGraphResponse HandleImportGraphContract(string assetPath, string graphContractJson)
        {
            return Handle(new ImportGraphContractRequest(assetPath, graphContractJson));
        }

        public static ShaderGraphResponse HandleFindNode(
            string assetPath,
            string nodeId,
            string displayName,
            string nodeType)
        {
            return Handle(new FindNodeRequest(assetPath, nodeId, displayName, nodeType));
        }

        public static ShaderGraphResponse HandleFindProperty(
            string assetPath,
            string propertyName,
            string displayName,
            string referenceName,
            string propertyType)
        {
            return Handle(new FindPropertyRequest(assetPath, propertyName, displayName, referenceName, propertyType));
        }

        public static ShaderGraphResponse HandleListSupportedNodes()
        {
            return Handle(new ListSupportedNodesRequest());
        }

        public static ShaderGraphResponse HandleListSupportedProperties()
        {
            return Handle(new ListSupportedPropertiesRequest());
        }

        public static ShaderGraphResponse HandleListSupportedConnections()
        {
            return Handle(new ListSupportedConnectionsRequest());
        }

        public static ShaderGraphResponse HandleUpdateProperty(
            string assetPath,
            string propertyName,
            string propertyType,
            string defaultValue)
        {
            return Handle(new UpdatePropertyRequest(assetPath, propertyName, propertyType, defaultValue));
        }

        public static ShaderGraphResponse HandleRenameProperty(
            string assetPath,
            string propertyName,
            string displayName,
            string referenceName)
        {
            return Handle(new RenamePropertyRequest(assetPath, propertyName, displayName, referenceName));
        }

        public static ShaderGraphResponse HandleDuplicateProperty(
            string assetPath,
            string propertyName,
            string displayName,
            string referenceName)
        {
            return Handle(new DuplicatePropertyRequest(assetPath, propertyName, displayName, referenceName));
        }

        public static ShaderGraphResponse HandleReorderProperty(
            string assetPath,
            string propertyName,
            int index)
        {
            return Handle(new ReorderPropertyRequest(assetPath, propertyName, index));
        }

        public static ShaderGraphResponse HandleMovePropertyToCategory(
            string assetPath,
            string propertyName,
            string categoryGuid,
            string categoryName,
            int? index)
        {
            return Handle(new MovePropertyToCategoryRequest(assetPath, propertyName, categoryGuid, categoryName, index));
        }

        public static ShaderGraphResponse HandleRenameNode(
            string assetPath,
            string nodeId,
            string displayName)
        {
            return Handle(new RenameNodeRequest(assetPath, nodeId, displayName));
        }

        public static ShaderGraphResponse HandleDuplicateNode(
            string assetPath,
            string nodeId,
            string displayName)
        {
            return Handle(new DuplicateNodeRequest(assetPath, nodeId, displayName));
        }

        public static ShaderGraphResponse HandleMoveNode(
            string assetPath,
            string nodeId,
            float x,
            float y)
        {
            return Handle(new MoveNodeRequest(assetPath, nodeId, x, y));
        }

        public static ShaderGraphResponse HandleDeleteNode(
            string assetPath,
            string nodeId)
        {
            return Handle(new DeleteNodeRequest(assetPath, nodeId));
        }

        public static ShaderGraphResponse HandleRemoveProperty(
            string assetPath,
            string propertyName)
        {
            return Handle(new RemovePropertyRequest(assetPath, propertyName));
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

        public static ShaderGraphResponse HandleFindConnection(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort)
        {
            return Handle(new FindConnectionRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort));
        }

        public static ShaderGraphResponse HandleRemoveConnection(
            string assetPath,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort)
        {
            return Handle(new RemoveConnectionRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort));
        }

        public static ShaderGraphResponse HandleReconnectConnection(
            string assetPath,
            string oldOutputNodeId,
            string oldOutputPort,
            string oldInputNodeId,
            string oldInputPort,
            string outputNodeId,
            string outputPort,
            string inputNodeId,
            string inputPort)
        {
            return Handle(new ReconnectConnectionRequest(
                assetPath,
                oldOutputNodeId,
                oldOutputPort,
                oldInputNodeId,
                oldInputPort,
                outputNodeId,
                outputPort,
                inputNodeId,
                inputPort));
        }

        public static ShaderGraphResponse HandleSaveGraph(string assetPath)
        {
            return Handle(new SaveGraphRequest(assetPath));
        }
    }
}
