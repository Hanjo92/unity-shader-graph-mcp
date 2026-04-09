using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using ShaderGraphMcp.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Tools
{
    public static class ShaderGraphBatchmodeBridge
    {
        private static readonly string[] RequestFileArgumentNames =
        {
            "-shaderGraphMcpRequestPath",
            "--shaderGraphMcpRequestPath",
            "--shadergraph-mcp-request-file",
            "--shadergraph-mcp-request",
            "--request-file",
            "--request",
        };

        private static readonly string[] ResponseFileArgumentNames =
        {
            "-shaderGraphMcpResponsePath",
            "--shaderGraphMcpResponsePath",
            "--shadergraph-mcp-response-file",
            "--shadergraph-mcp-response",
            "--response-file",
            "--response",
        };

        public static void Execute()
        {
            Run(Environment.GetCommandLineArgs(), Application.isBatchMode);
        }

        internal static BatchmodeRunResult Run(string[] args, bool exitOnCompletion)
        {
            var result = new BatchmodeRunResult();

            if (!TryParseInvocation(args, out ShaderGraphBatchmodeInvocation invocation, out string invocationError))
            {
                result.Response = ShaderGraphResponse.Fail(invocationError);
                result.ExitCode = 1;
                result.ErrorMessage = invocationError;

                TryWriteResponseFile(invocation.ResponseFilePath, result.Response, out string writeError);
                if (!string.IsNullOrWhiteSpace(writeError))
                {
                    result.ErrorMessage = AppendMessage(result.ErrorMessage, writeError);
                }

                ExitIfNeeded(exitOnCompletion, result.ExitCode);
                return result;
            }

            result.RequestFilePath = invocation.RequestFilePath;
            result.ResponseFilePath = invocation.ResponseFilePath;

            if (!TryReadAllText(invocation.RequestFilePath, out string requestJson, out string requestError))
            {
                result.Response = ShaderGraphResponse.Fail(requestError);
                result.ExitCode = 1;
                result.ErrorMessage = requestError;

                TryWriteResponseFile(invocation.ResponseFilePath, result.Response, out string writeError);
                if (!string.IsNullOrWhiteSpace(writeError))
                {
                    result.ErrorMessage = AppendMessage(result.ErrorMessage, writeError);
                }

                ExitIfNeeded(exitOnCompletion, result.ExitCode);
                return result;
            }

            if (!TryParseRequest(requestJson, out ShaderGraphRequest request, out string requestParseError))
            {
                result.Response = ShaderGraphResponse.Fail(requestParseError);
                result.ExitCode = 1;
                result.ErrorMessage = requestParseError;

                TryWriteResponseFile(invocation.ResponseFilePath, result.Response, out string writeError);
                if (!string.IsNullOrWhiteSpace(writeError))
                {
                    result.ErrorMessage = AppendMessage(result.ErrorMessage, writeError);
                }

                ExitIfNeeded(exitOnCompletion, result.ExitCode);
                return result;
            }

            result.Request = request;
            result.Response = ShaderGraphAssetTool.Handle(request);
            result.ExitCode = result.Response.Success ? 0 : 1;

            if (!TryWriteResponseFile(invocation.ResponseFilePath, result.Response, out string responseWriteError))
            {
                result.ErrorMessage = responseWriteError;
                result.ExitCode = 1;
            }

            ExitIfNeeded(exitOnCompletion, result.ExitCode);
            return result;
        }

        internal static bool TryParseInvocation(string[] args, out ShaderGraphBatchmodeInvocation invocation, out string errorMessage)
        {
            string requestFilePath = TryGetArgumentValue(args, RequestFileArgumentNames);
            string responseFilePath = TryGetArgumentValue(args, ResponseFileArgumentNames);
            var missingParts = new List<string>();

            if (string.IsNullOrWhiteSpace(requestFilePath))
            {
                missingParts.Add("request file");
            }

            if (string.IsNullOrWhiteSpace(responseFilePath))
            {
                missingParts.Add("response file");
            }

            invocation = new ShaderGraphBatchmodeInvocation(requestFilePath, responseFilePath);

            if (missingParts.Count > 0)
            {
                errorMessage = $"Missing batchmode argument(s): {string.Join(", ", missingParts)}.";
                return false;
            }

            errorMessage = null;
            return true;
        }

        internal static bool TryParseRequest(string json, out ShaderGraphRequest request, out string errorMessage)
        {
            request = null;

            if (string.IsNullOrWhiteSpace(json))
            {
                errorMessage = "Request JSON is required.";
                return false;
            }

            ShaderGraphBatchmodeRequestPayload payload;
            try
            {
                payload = JsonUtility.FromJson<ShaderGraphBatchmodeRequestPayload>(json);
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to parse request JSON: {exception.Message}";
                return false;
            }

            if (payload == null)
            {
                errorMessage = "Request JSON did not produce a payload.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(payload.tool) &&
                !string.Equals(payload.tool, "shadergraph_asset", StringComparison.OrdinalIgnoreCase))
            {
                errorMessage = $"Unsupported tool '{payload.tool}'. Expected 'shadergraph_asset'.";
                return false;
            }

            return TryCreateRequest(payload, out request, out errorMessage);
        }

        internal static string SerializeResponse(ShaderGraphResponse response)
        {
            if (response == null)
            {
                response = ShaderGraphResponse.Fail("Response is required.");
            }

            var builder = new StringBuilder();
            builder.Append('{');
            AppendProperty(builder, "success", response.Success);
            builder.Append(',');
            AppendProperty(builder, "message", response.Message ?? string.Empty);
            builder.Append(',');
            builder.Append("\"data\":");
            IReadOnlyDictionary<string, object> responseData = response.Data ?? new Dictionary<string, object>();
            AppendDictionary(builder, responseData);
            builder.Append('}');
            return builder.ToString();
        }

        internal static string SerializeValueToJson(object value)
        {
            var builder = new StringBuilder();
            AppendValue(builder, value);
            return builder.ToString();
        }

        internal static ShaderGraphResponse HandleRequestJson(string json, out ShaderGraphRequest request, out string errorMessage)
        {
            if (!TryParseRequest(json, out request, out errorMessage))
            {
                return ShaderGraphResponse.Fail(errorMessage);
            }

            return ShaderGraphAssetTool.Handle(request);
        }

        private static bool TryCreateRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            request = null;

            bool success;
            switch (NormalizeAction(payload.action))
            {
                case ShaderGraphAction.CreateGraph:
                    success = TryCreateGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.CreateSubGraph:
                    success = TryCreateSubGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RenameGraph:
                    success = TryCreateRenameGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RenameSubGraph:
                    success = TryCreateRenameSubGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DuplicateGraph:
                    success = TryCreateDuplicateGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DuplicateSubGraph:
                    success = TryCreateDuplicateSubGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DeleteGraph:
                    success = TryCreateDeleteGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DeleteSubGraph:
                    success = TryCreateDeleteSubGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.MoveGraph:
                    success = TryCreateMoveGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.MoveSubGraph:
                    success = TryCreateMoveSubGraphRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.SetGraphMetadata:
                    success = TryCreateSetGraphMetadataRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.CreateCategory:
                    success = TryCreateCreateCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RenameCategory:
                    success = TryCreateRenameCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.FindCategory:
                    success = TryCreateFindCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DeleteCategory:
                    success = TryCreateDeleteCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ReorderCategory:
                    success = TryCreateReorderCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.MergeCategory:
                    success = TryCreateMergeCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DuplicateCategory:
                    success = TryCreateDuplicateCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.SplitCategory:
                    success = TryCreateSplitCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ListCategories:
                    success = TryCreateListCategoriesRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ReadGraphSummary:
                    success = TryCreateReadGraphSummaryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ReadSubGraphSummary:
                    success = TryCreateReadSubGraphSummaryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ExportGraphContract:
                    success = TryCreateExportGraphContractRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ImportGraphContract:
                    success = TryCreateImportGraphContractRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.FindNode:
                    success = TryCreateFindNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.FindProperty:
                    success = TryCreateFindPropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ListSupportedNodes:
                    request = new ListSupportedNodesRequest();
                    errorMessage = null;
                    success = true;
                    break;
                case ShaderGraphAction.ListSupportedProperties:
                    request = new ListSupportedPropertiesRequest();
                    errorMessage = null;
                    success = true;
                    break;
                case ShaderGraphAction.ListSupportedConnections:
                    request = new ListSupportedConnectionsRequest();
                    errorMessage = null;
                    success = true;
                    break;
                case ShaderGraphAction.UpdateProperty:
                    success = TryCreateUpdatePropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RenameProperty:
                    success = TryCreateRenamePropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DuplicateProperty:
                    success = TryCreateDuplicatePropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ReorderProperty:
                    success = TryCreateReorderPropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.MovePropertyToCategory:
                    success = TryCreateMovePropertyToCategoryRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RenameNode:
                    success = TryCreateRenameNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DuplicateNode:
                    success = TryCreateDuplicateNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.MoveNode:
                    success = TryCreateMoveNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.DeleteNode:
                    success = TryCreateDeleteNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RemoveProperty:
                    success = TryCreateRemovePropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.AddProperty:
                    success = TryCreateAddPropertyRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.AddNode:
                    success = TryCreateAddNodeRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ConnectPorts:
                    success = TryCreateConnectPortsRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.FindConnection:
                    success = TryCreateFindConnectionRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.RemoveConnection:
                    success = TryCreateRemoveConnectionRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.ReconnectConnection:
                    success = TryCreateReconnectConnectionRequest(payload, out request, out errorMessage);
                    break;
                case ShaderGraphAction.SaveGraph:
                    success = TryCreateSaveGraphRequest(payload, out request, out errorMessage);
                    break;
                default:
                    errorMessage = $"Unsupported or missing action '{payload.action}'.";
                    return false;
            }

            if (!success)
            {
                return false;
            }

            if (!TryValidateRequestAssetPathKind(request, out errorMessage))
            {
                request = null;
                return false;
            }

            return true;
        }

        private static bool TryCreateGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string name = FirstNonBlank(payload.name, GetNameFromKnownAssetPath(payload.assetPath), GetNameFromKnownAssetPath(payload.path));
            string directoryPath = FirstNonBlank(payload.path, payload.assetPath);

            if (!string.IsNullOrWhiteSpace(directoryPath) && LooksLikeKnownShaderAssetPath(directoryPath))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileNameWithoutExtension(directoryPath);
                }

                directoryPath = Path.GetDirectoryName(directoryPath)?.Replace('\\', '/');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "UntitledShaderGraph";
            }

            request = new CreateGraphRequest(name, directoryPath, payload.template);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateSubGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string name = FirstNonBlank(payload.name, GetNameFromKnownAssetPath(payload.assetPath), GetNameFromKnownAssetPath(payload.path));
            string directoryPath = FirstNonBlank(payload.path, payload.assetPath);

            if (!string.IsNullOrWhiteSpace(directoryPath) && LooksLikeKnownShaderAssetPath(directoryPath))
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    name = Path.GetFileNameWithoutExtension(directoryPath);
                }

                directoryPath = Path.GetDirectoryName(directoryPath)?.Replace('\\', '/');
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                name = "UntitledShaderSubGraph";
            }

            request = new CreateSubGraphRequest(name, directoryPath, payload.template);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateReadGraphSummaryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Read graph summary requires an asset path.";
                return false;
            }

            request = new ReadGraphSummaryRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateReadSubGraphSummaryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Read sub graph summary requires an asset path.";
                return false;
            }

            request = new ReadSubGraphSummaryRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateExportGraphContractRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Export graph contract requires an asset path.";
                return false;
            }

            request = new ExportGraphContractRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateImportGraphContractRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Import graph contract requires an asset path.";
                return false;
            }

            string graphContractJson = FirstNonBlank(payload.graphContractJson, payload.contractJson);
            if (string.IsNullOrWhiteSpace(graphContractJson))
            {
                request = null;
                errorMessage = "Import graph contract requires graphContractJson/contractJson.";
                return false;
            }

            request = new ImportGraphContractRequest(assetPath, graphContractJson);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRenameGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Rename graph requires an asset path.";
                return false;
            }

            string name = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(name))
            {
                request = null;
                errorMessage = "Rename graph requires newDisplayName/displayName/name.";
                return false;
            }

            request = new RenameGraphRequest(assetPath, name);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRenameSubGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Rename sub graph requires an asset path.";
                return false;
            }

            string name = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(name))
            {
                request = null;
                errorMessage = "Rename sub graph requires newDisplayName/displayName/name.";
                return false;
            }

            request = new RenameSubGraphRequest(assetPath, name);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDuplicateGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Duplicate graph requires an asset path.";
                return false;
            }

            string name = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(name))
            {
                request = null;
                errorMessage = "Duplicate graph requires newDisplayName/displayName/name.";
                return false;
            }

            request = new DuplicateGraphRequest(assetPath, name);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDuplicateSubGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Duplicate sub graph requires an asset path.";
                return false;
            }

            string name = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(name))
            {
                request = null;
                errorMessage = "Duplicate sub graph requires newDisplayName/displayName/name.";
                return false;
            }

            request = new DuplicateSubGraphRequest(assetPath, name);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDeleteGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Delete graph requires an asset path.";
                return false;
            }

            request = new DeleteGraphRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDeleteSubGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Delete sub graph requires an asset path.";
                return false;
            }

            request = new DeleteSubGraphRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateMoveGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Move graph requires an asset path.";
                return false;
            }

            string targetAssetPath = ResolveMoveGraphTargetAssetPath(payload, assetPath);
            if (string.IsNullOrWhiteSpace(targetAssetPath))
            {
                request = null;
                errorMessage = "Move graph requires targetAssetPath/targetPath/newAssetPath/newPath/destinationPath.";
                return false;
            }

            request = new MoveGraphRequest(assetPath, targetAssetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateMoveSubGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Move sub graph requires an asset path.";
                return false;
            }

            string targetAssetPath = ResolveMoveSubGraphTargetAssetPath(payload, assetPath);
            if (string.IsNullOrWhiteSpace(targetAssetPath))
            {
                request = null;
                errorMessage = "Move sub graph requires targetAssetPath/targetPath/newAssetPath/newPath/destinationPath.";
                return false;
            }

            request = new MoveSubGraphRequest(assetPath, targetAssetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateSetGraphMetadataRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Set graph metadata requires an asset path.";
                return false;
            }

            string graphPathLabel = FirstNonBlank(payload.graphPathLabel, payload.pathLabel);
            string graphDefaultPrecision = FirstNonBlank(payload.graphDefaultPrecision, payload.defaultPrecision, payload.precision);
            if (string.IsNullOrWhiteSpace(graphPathLabel) && string.IsNullOrWhiteSpace(graphDefaultPrecision))
            {
                request = null;
                errorMessage = "Set graph metadata requires graphPathLabel/pathLabel and/or graphDefaultPrecision/defaultPrecision/precision.";
                return false;
            }

            request = new SetGraphMetadataRequest(assetPath, graphPathLabel, graphDefaultPrecision);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateCreateCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Create category requires an asset path.";
                return false;
            }

            string categoryName = FirstNonBlank(payload.categoryName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Create category requires categoryName/displayName/name.";
                return false;
            }

            request = new CreateCategoryRequest(assetPath, categoryName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRenameCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Rename category requires an asset path.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid);
            string categoryName = FirstNonBlank(payload.categoryName);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Rename category requires categoryGuid or categoryName.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                request = null;
                errorMessage = "Rename category requires newDisplayName/displayName/name.";
                return false;
            }

            request = new RenameCategoryRequest(assetPath, categoryGuid, categoryName, displayName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateFindCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Find category requires an asset path.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid);
            string categoryName = FirstNonBlank(payload.categoryName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Find category requires categoryGuid or categoryName/displayName/name.";
                return false;
            }

            request = new FindCategoryRequest(assetPath, categoryGuid, categoryName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDeleteCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Delete category requires an asset path.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid);
            string categoryName = FirstNonBlank(payload.categoryName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Delete category requires categoryGuid or categoryName/displayName/name.";
                return false;
            }

            request = new DeleteCategoryRequest(assetPath, categoryGuid, categoryName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateReorderCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Reorder category requires an asset path.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid);
            string categoryName = FirstNonBlank(payload.categoryName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Reorder category requires categoryGuid or categoryName/displayName/name.";
                return false;
            }

            string indexText = FirstNonBlank(payload.newIndex, payload.targetIndex, payload.index);
            if (!int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
            {
                request = null;
                errorMessage = "Reorder category requires index/newIndex/targetIndex.";
                return false;
            }

            request = new ReorderCategoryRequest(assetPath, categoryGuid, categoryName, index);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateListCategoriesRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "List categories requires an asset path.";
                return false;
            }

            request = new ListCategoriesRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateMergeCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Merge category requires an asset path.";
                return false;
            }

            string sourceCategoryGuid = FirstNonBlank(payload.sourceCategoryGuid);
            string sourceCategoryName = FirstNonBlank(payload.sourceCategoryName, payload.sourceDisplayName, payload.sourceName);
            if (string.IsNullOrWhiteSpace(sourceCategoryGuid) && string.IsNullOrWhiteSpace(sourceCategoryName))
            {
                request = null;
                errorMessage = "Merge category requires sourceCategoryGuid or sourceCategoryName/sourceDisplayName/sourceName.";
                return false;
            }

            string targetCategoryGuid = FirstNonBlank(payload.targetCategoryGuid);
            string targetCategoryName = FirstNonBlank(payload.targetCategoryName, payload.targetDisplayName, payload.targetName);
            if (string.IsNullOrWhiteSpace(targetCategoryGuid) && string.IsNullOrWhiteSpace(targetCategoryName))
            {
                request = null;
                errorMessage = "Merge category requires targetCategoryGuid or targetCategoryName/targetDisplayName/targetName.";
                return false;
            }

            request = new MergeCategoryRequest(
                assetPath,
                sourceCategoryGuid,
                sourceCategoryName,
                targetCategoryGuid,
                targetCategoryName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDuplicateCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Duplicate category requires an asset path.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid, payload.sourceCategoryGuid);
            string categoryName = FirstNonBlank(
                payload.categoryName,
                payload.sourceCategoryName,
                payload.sourceDisplayName,
                payload.sourceName);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Duplicate category requires categoryGuid or categoryName/sourceCategoryName/sourceDisplayName/sourceName.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.targetCategoryName, payload.targetDisplayName, payload.targetName, payload.name);
            request = new DuplicateCategoryRequest(assetPath, categoryGuid, categoryName, displayName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateSplitCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Split category requires an asset path.";
                return false;
            }

            string sourceCategoryGuid = FirstNonBlank(payload.categoryGuid, payload.sourceCategoryGuid);
            string sourceCategoryName = FirstNonBlank(
                payload.categoryName,
                payload.sourceCategoryName,
                payload.sourceDisplayName,
                payload.sourceName);
            if (string.IsNullOrWhiteSpace(sourceCategoryGuid) && string.IsNullOrWhiteSpace(sourceCategoryName))
            {
                request = null;
                errorMessage = "Split category requires categoryGuid/sourceCategoryGuid or categoryName/sourceCategoryName/sourceDisplayName/sourceName.";
                return false;
            }

            string displayName = FirstNonBlank(
                payload.newDisplayName,
                payload.targetCategoryName,
                payload.targetDisplayName,
                payload.targetName,
                payload.displayName,
                payload.name);
            string[] propertyNames = CollectTextValues(
                payload.propertyName,
                payload.propertyNames,
                payload.properties);
            if (propertyNames.Length == 0)
            {
                request = null;
                errorMessage = "Split category requires propertyName or propertyNames/properties.";
                return false;
            }

            request = new SplitCategoryRequest(assetPath, sourceCategoryGuid, sourceCategoryName, displayName, propertyNames);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateFindNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Find node requires an asset path.";
                return false;
            }

            string nodeId = FirstNonBlank(payload.nodeId, payload.objectId);
            string displayName = FirstNonBlank(payload.displayName);
            string nodeType = FirstNonBlank(payload.nodeType);
            if (string.IsNullOrWhiteSpace(nodeId) &&
                string.IsNullOrWhiteSpace(displayName) &&
                string.IsNullOrWhiteSpace(nodeType))
            {
                request = null;
                errorMessage = "Find node requires at least one of: nodeId/objectId, displayName, nodeType.";
                return false;
            }

            request = new FindNodeRequest(assetPath, nodeId, displayName, nodeType);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateFindPropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Find property requires an asset path.";
                return false;
            }

            string propertyName = FirstNonBlank(payload.propertyName);
            string displayName = FirstNonBlank(payload.displayName);
            string referenceName = FirstNonBlank(payload.referenceName);
            string propertyType = FirstNonBlank(payload.propertyType);
            if (string.IsNullOrWhiteSpace(propertyName) &&
                string.IsNullOrWhiteSpace(displayName) &&
                string.IsNullOrWhiteSpace(referenceName) &&
                string.IsNullOrWhiteSpace(propertyType))
            {
                request = null;
                errorMessage = "Find property requires at least one of: propertyName, displayName, referenceName, propertyType.";
                return false;
            }

            request = new FindPropertyRequest(assetPath, propertyName, displayName, referenceName, propertyType);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateAddPropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Add property requires an asset path.";
                return false;
            }

            request = new AddPropertyRequest(assetPath, payload.propertyName, payload.propertyType, payload.defaultValue);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateUpdatePropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Update property requires an asset path.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.propertyName))
            {
                request = null;
                errorMessage = "Update property requires a property name.";
                return false;
            }

            request = new UpdatePropertyRequest(assetPath, payload.propertyName, payload.propertyType, payload.defaultValue);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRenamePropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Rename property requires an asset path.";
                return false;
            }

            string propertyName = FirstNonBlank(payload.propertyName);
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                request = null;
                errorMessage = "Rename property requires a property name.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                request = null;
                errorMessage = "Rename property requires displayName/newDisplayName/name.";
                return false;
            }

            string referenceName = FirstNonBlank(payload.newReferenceName, payload.referenceName);
            request = new RenamePropertyRequest(assetPath, propertyName, displayName, referenceName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDuplicatePropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Duplicate property requires an asset path.";
                return false;
            }

            string propertyName = FirstNonBlank(payload.propertyName);
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                request = null;
                errorMessage = "Duplicate property requires a property name.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            string referenceName = FirstNonBlank(payload.newReferenceName, payload.referenceName);
            request = new DuplicatePropertyRequest(assetPath, propertyName, displayName, referenceName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateReorderPropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Reorder property requires an asset path.";
                return false;
            }

            string propertyName = FirstNonBlank(payload.propertyName);
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                request = null;
                errorMessage = "Reorder property requires a property name.";
                return false;
            }

            string indexText = FirstNonBlank(payload.newIndex, payload.targetIndex, payload.index);
            if (string.IsNullOrWhiteSpace(indexText))
            {
                request = null;
                errorMessage = "Reorder property requires index/newIndex/targetIndex.";
                return false;
            }

            if (!int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
            {
                request = null;
                errorMessage = "Reorder property requires an integer index.";
                return false;
            }

            request = new ReorderPropertyRequest(assetPath, propertyName, index);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateMovePropertyToCategoryRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Move property to category requires an asset path.";
                return false;
            }

            string propertyName = FirstNonBlank(payload.propertyName);
            if (string.IsNullOrWhiteSpace(propertyName))
            {
                request = null;
                errorMessage = "Move property to category requires propertyName.";
                return false;
            }

            string categoryGuid = FirstNonBlank(payload.categoryGuid);
            string categoryName = FirstNonBlank(payload.categoryName, payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(categoryGuid) && string.IsNullOrWhiteSpace(categoryName))
            {
                request = null;
                errorMessage = "Move property to category requires categoryGuid or categoryName/displayName/name.";
                return false;
            }

            string indexText = FirstNonBlank(payload.newIndex, payload.targetIndex, payload.index);
            int? index = null;
            if (!string.IsNullOrWhiteSpace(indexText))
            {
                if (!int.TryParse(indexText, NumberStyles.Integer, CultureInfo.InvariantCulture, out int parsedIndex))
                {
                    request = null;
                    errorMessage = "Move property to category requires an integer index when provided.";
                    return false;
                }

                index = parsedIndex;
            }

            request = new MovePropertyToCategoryRequest(assetPath, propertyName, categoryGuid, categoryName, index);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRenameNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Rename node requires an asset path.";
                return false;
            }

            string nodeId = FirstNonBlank(payload.nodeId, payload.objectId);
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                request = null;
                errorMessage = "Rename node requires nodeId/objectId.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            if (string.IsNullOrWhiteSpace(displayName))
            {
                request = null;
                errorMessage = "Rename node requires displayName/newDisplayName/name.";
                return false;
            }

            request = new RenameNodeRequest(assetPath, nodeId, displayName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDuplicateNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Duplicate node requires an asset path.";
                return false;
            }

            string nodeId = FirstNonBlank(payload.nodeId, payload.objectId);
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                request = null;
                errorMessage = "Duplicate node requires nodeId/objectId.";
                return false;
            }

            string displayName = FirstNonBlank(payload.newDisplayName, payload.displayName, payload.name);
            request = new DuplicateNodeRequest(assetPath, nodeId, displayName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateMoveNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Move node requires an asset path.";
                return false;
            }

            string nodeId = FirstNonBlank(payload.nodeId, payload.objectId);
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                request = null;
                errorMessage = "Move node requires nodeId/objectId.";
                return false;
            }

            bool hasX = TryParseOptionalSingle(payload.x, out float x);
            bool hasY = TryParseOptionalSingle(payload.y, out float y);
            if (hasX != hasY)
            {
                request = null;
                errorMessage = "Move node requires both x and y when using exact coordinates.";
                return false;
            }

            string anchorNodeId = FirstNonBlank(
                payload.anchorNodeId,
                payload.anchorObjectId,
                payload.relativeToNodeId,
                payload.relativeToObjectId);
            string anchorDisplayName = FirstNonBlank(payload.anchorDisplayName, payload.relativeToDisplayName);
            string anchorNodeType = FirstNonBlank(payload.anchorNodeType, payload.relativeToNodeType);
            string direction = FirstNonBlank(payload.direction, payload.relativeDirection);
            string layoutPreset = FirstNonBlank(payload.layoutPreset, payload.preset);
            bool hasAnchorQuery =
                !string.IsNullOrWhiteSpace(anchorNodeId) ||
                !string.IsNullOrWhiteSpace(anchorDisplayName) ||
                !string.IsNullOrWhiteSpace(anchorNodeType);

            float? spacing = null;
            if (!string.IsNullOrWhiteSpace(payload.spacing))
            {
                if (!TryParseSingle(payload.spacing, out float parsedSpacing))
                {
                    request = null;
                    errorMessage = "Move node requires spacing to be a valid number when provided.";
                    return false;
                }

                spacing = parsedSpacing;
            }

            bool hasRelativeHints =
                hasAnchorQuery ||
                !string.IsNullOrWhiteSpace(direction) ||
                !string.IsNullOrWhiteSpace(layoutPreset) ||
                spacing.HasValue;

            if (!hasX && !hasRelativeHints)
            {
                request = null;
                errorMessage = "Move node requires either x/y or anchorNodeId/anchorDisplayName/anchorNodeType with direction/layoutPreset.";
                return false;
            }

            if (!hasX && hasRelativeHints && !hasAnchorQuery)
            {
                request = null;
                errorMessage = "Move node relative placement requires anchorNodeId/anchorDisplayName/anchorNodeType.";
                return false;
            }

            if (!hasX && hasRelativeHints && string.IsNullOrWhiteSpace(direction) && string.IsNullOrWhiteSpace(layoutPreset))
            {
                request = null;
                errorMessage = "Move node relative placement requires direction/relativeDirection or layoutPreset/preset.";
                return false;
            }

            request = new MoveNodeRequest(
                assetPath,
                nodeId,
                hasX ? x : (float?)null,
                hasY ? y : (float?)null,
                anchorNodeId,
                anchorDisplayName,
                anchorNodeType,
                direction,
                spacing,
                layoutPreset);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateDeleteNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Delete node requires an asset path.";
                return false;
            }

            string nodeId = FirstNonBlank(payload.nodeId, payload.objectId);
            if (string.IsNullOrWhiteSpace(nodeId))
            {
                request = null;
                errorMessage = "Delete node requires nodeId/objectId.";
                return false;
            }

            request = new DeleteNodeRequest(assetPath, nodeId);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRemovePropertyRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Remove property requires an asset path.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(payload.propertyName))
            {
                request = null;
                errorMessage = "Remove property requires a property name.";
                return false;
            }

            request = new RemovePropertyRequest(assetPath, payload.propertyName);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateAddNodeRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Add node requires an asset path.";
                return false;
            }

            bool hasX = TryParseOptionalSingle(payload.x, out float x);
            bool hasY = TryParseOptionalSingle(payload.y, out float y);
            if (hasX != hasY)
            {
                request = null;
                errorMessage = "Add node requires both x and y when using exact coordinates.";
                return false;
            }

            string anchorNodeId = FirstNonBlank(
                payload.anchorNodeId,
                payload.anchorObjectId,
                payload.relativeToNodeId,
                payload.relativeToObjectId);
            string anchorDisplayName = FirstNonBlank(payload.anchorDisplayName, payload.relativeToDisplayName);
            string anchorNodeType = FirstNonBlank(payload.anchorNodeType, payload.relativeToNodeType);
            string direction = FirstNonBlank(payload.direction, payload.relativeDirection);
            string layoutPreset = FirstNonBlank(payload.layoutPreset, payload.preset);
            bool hasAnchorQuery =
                !string.IsNullOrWhiteSpace(anchorNodeId) ||
                !string.IsNullOrWhiteSpace(anchorDisplayName) ||
                !string.IsNullOrWhiteSpace(anchorNodeType);

            float? spacing = null;
            if (!string.IsNullOrWhiteSpace(payload.spacing))
            {
                if (!TryParseSingle(payload.spacing, out float parsedSpacing))
                {
                    request = null;
                    errorMessage = "Add node requires spacing to be a valid number when provided.";
                    return false;
                }

                spacing = parsedSpacing;
            }

            bool hasRelativeHints =
                hasAnchorQuery ||
                !string.IsNullOrWhiteSpace(direction) ||
                !string.IsNullOrWhiteSpace(layoutPreset) ||
                spacing.HasValue;

            if (!hasX && hasRelativeHints && !hasAnchorQuery)
            {
                request = null;
                errorMessage = "Add node relative placement requires anchorNodeId/anchorDisplayName/anchorNodeType.";
                return false;
            }

            if (!hasX && hasRelativeHints && string.IsNullOrWhiteSpace(direction) && string.IsNullOrWhiteSpace(layoutPreset))
            {
                request = null;
                errorMessage = "Add node relative placement requires direction/relativeDirection or layoutPreset/preset.";
                return false;
            }

            request = new AddNodeRequest(
                assetPath,
                payload.nodeType,
                payload.displayName,
                hasX ? x : (float?)null,
                hasY ? y : (float?)null,
                anchorNodeId,
                anchorDisplayName,
                anchorNodeType,
                direction,
                spacing,
                layoutPreset);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateConnectPortsRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Connect ports requires an asset path.";
                return false;
            }

            string outputNodeId = FirstNonBlank(payload.outputNodeId, payload.sourceNodeId);
            string outputPort = FirstNonBlank(payload.outputPort, payload.sourcePort);
            string inputNodeId = FirstNonBlank(payload.inputNodeId, payload.targetNodeId);
            string inputPort = FirstNonBlank(payload.inputPort, payload.targetPort);

            request = new ConnectPortsRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateRemoveConnectionRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Remove connection requires an asset path.";
                return false;
            }

            string outputNodeId = FirstNonBlank(payload.outputNodeId, payload.sourceNodeId);
            string outputPort = FirstNonBlank(payload.outputPort, payload.sourcePort);
            string inputNodeId = FirstNonBlank(payload.inputNodeId, payload.targetNodeId);
            string inputPort = FirstNonBlank(payload.inputPort, payload.targetPort);

            request = new RemoveConnectionRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateFindConnectionRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Find connection requires an asset path.";
                return false;
            }

            string outputNodeId = FirstNonBlank(payload.outputNodeId, payload.sourceNodeId);
            string outputPort = FirstNonBlank(payload.outputPort, payload.sourcePort);
            string inputNodeId = FirstNonBlank(payload.inputNodeId, payload.targetNodeId);
            string inputPort = FirstNonBlank(payload.inputPort, payload.targetPort);

            request = new FindConnectionRequest(assetPath, outputNodeId, outputPort, inputNodeId, inputPort);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateReconnectConnectionRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Reconnect connection requires an asset path.";
                return false;
            }

            string oldOutputNodeId = FirstNonBlank(payload.oldOutputNodeId, payload.oldSourceNodeId);
            string oldOutputPort = FirstNonBlank(payload.oldOutputPort, payload.oldSourcePort);
            string oldInputNodeId = FirstNonBlank(payload.oldInputNodeId, payload.oldTargetNodeId);
            string oldInputPort = FirstNonBlank(payload.oldInputPort, payload.oldTargetPort);
            string outputNodeId = FirstNonBlank(payload.outputNodeId, payload.sourceNodeId, payload.newOutputNodeId, payload.newSourceNodeId);
            string outputPort = FirstNonBlank(payload.outputPort, payload.sourcePort, payload.newOutputPort, payload.newSourcePort);
            string inputNodeId = FirstNonBlank(payload.inputNodeId, payload.targetNodeId, payload.newInputNodeId, payload.newTargetNodeId);
            string inputPort = FirstNonBlank(payload.inputPort, payload.targetPort, payload.newInputPort, payload.newTargetPort);

            request = new ReconnectConnectionRequest(
                assetPath,
                oldOutputNodeId,
                oldOutputPort,
                oldInputNodeId,
                oldInputPort,
                outputNodeId,
                outputPort,
                inputNodeId,
                inputPort);
            errorMessage = null;
            return true;
        }

        private static bool TryCreateSaveGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string assetPath = ResolveAssetPath(payload);
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                request = null;
                errorMessage = "Save graph requires an asset path.";
                return false;
            }

            request = new SaveGraphRequest(assetPath);
            errorMessage = null;
            return true;
        }

        private static ShaderGraphAction NormalizeAction(string action)
        {
            if (string.IsNullOrWhiteSpace(action))
            {
                return (ShaderGraphAction)(-1);
            }

            string normalized = action.Trim();
            if (Enum.TryParse(action, true, out ShaderGraphAction parsedAction))
            {
                return parsedAction;
            }

            switch (normalized.Replace("-", "_").ToLowerInvariant())
            {
                case "create_graph":
                    return ShaderGraphAction.CreateGraph;
                case "create_subgraph":
                    return ShaderGraphAction.CreateSubGraph;
                case "rename_graph":
                    return ShaderGraphAction.RenameGraph;
                case "rename_subgraph":
                    return ShaderGraphAction.RenameSubGraph;
                case "duplicate_graph":
                    return ShaderGraphAction.DuplicateGraph;
                case "duplicate_subgraph":
                    return ShaderGraphAction.DuplicateSubGraph;
                case "delete_graph":
                    return ShaderGraphAction.DeleteGraph;
                case "delete_subgraph":
                    return ShaderGraphAction.DeleteSubGraph;
                case "move_graph":
                    return ShaderGraphAction.MoveGraph;
                case "move_subgraph":
                    return ShaderGraphAction.MoveSubGraph;
                case "set_graph_metadata":
                    return ShaderGraphAction.SetGraphMetadata;
                case "create_category":
                    return ShaderGraphAction.CreateCategory;
                case "rename_category":
                    return ShaderGraphAction.RenameCategory;
                case "find_category":
                    return ShaderGraphAction.FindCategory;
                case "delete_category":
                return ShaderGraphAction.DeleteCategory;
            case "reorder_category":
                return ShaderGraphAction.ReorderCategory;
            case "merge_category":
                return ShaderGraphAction.MergeCategory;
            case "duplicate_category":
                return ShaderGraphAction.DuplicateCategory;
            case "split_category":
                return ShaderGraphAction.SplitCategory;
                case "list_categories":
                    return ShaderGraphAction.ListCategories;
                case "read_graph_summary":
                    return ShaderGraphAction.ReadGraphSummary;
                case "read_subgraph_summary":
                    return ShaderGraphAction.ReadSubGraphSummary;
                case "export_graph_contract":
                    return ShaderGraphAction.ExportGraphContract;
                case "import_graph_contract":
                    return ShaderGraphAction.ImportGraphContract;
                case "find_node":
                    return ShaderGraphAction.FindNode;
                case "find_property":
                    return ShaderGraphAction.FindProperty;
                case "list_supported_nodes":
                    return ShaderGraphAction.ListSupportedNodes;
                case "list_supported_properties":
                    return ShaderGraphAction.ListSupportedProperties;
                case "list_supported_connections":
                    return ShaderGraphAction.ListSupportedConnections;
                case "update_property":
                    return ShaderGraphAction.UpdateProperty;
                case "rename_property":
                    return ShaderGraphAction.RenameProperty;
                case "duplicate_property":
                    return ShaderGraphAction.DuplicateProperty;
                case "reorder_property":
                    return ShaderGraphAction.ReorderProperty;
                case "move_property_to_category":
                    return ShaderGraphAction.MovePropertyToCategory;
                case "rename_node":
                    return ShaderGraphAction.RenameNode;
                case "duplicate_node":
                    return ShaderGraphAction.DuplicateNode;
                case "move_node":
                    return ShaderGraphAction.MoveNode;
                case "delete_node":
                    return ShaderGraphAction.DeleteNode;
                case "remove_property":
                    return ShaderGraphAction.RemoveProperty;
                case "add_property":
                    return ShaderGraphAction.AddProperty;
                case "add_node":
                    return ShaderGraphAction.AddNode;
                case "connect_ports":
                    return ShaderGraphAction.ConnectPorts;
                case "find_connection":
                    return ShaderGraphAction.FindConnection;
                case "remove_connection":
                    return ShaderGraphAction.RemoveConnection;
                case "reconnect_connection":
                    return ShaderGraphAction.ReconnectConnection;
                case "save_graph":
                    return ShaderGraphAction.SaveGraph;
            }

            return (ShaderGraphAction)(-1);
        }

        private static string ResolveAssetPath(ShaderGraphBatchmodeRequestPayload payload)
        {
            return FirstNonBlank(payload.assetPath, payload.path);
        }

        private static bool TryValidateRequestAssetPathKind(ShaderGraphRequest request, out string errorMessage)
        {
            errorMessage = null;
            if (request == null || string.IsNullOrWhiteSpace(request.AssetPath))
            {
                return true;
            }

            string assetPath = request.AssetPath.Trim().Replace('\\', '/');
            bool isShaderGraph = LooksLikeShaderGraphAssetPath(assetPath);
            bool isShaderSubGraph = LooksLikeShaderSubGraphAssetPath(assetPath);

            switch (request.Action)
            {
                case ShaderGraphAction.CreateSubGraph:
                case ShaderGraphAction.RenameSubGraph:
                case ShaderGraphAction.DuplicateSubGraph:
                case ShaderGraphAction.DeleteSubGraph:
                case ShaderGraphAction.MoveSubGraph:
                case ShaderGraphAction.ReadSubGraphSummary:
                    if (isShaderGraph)
                    {
                        errorMessage = $"{request.Action} requires a .shadersubgraph asset path, got '{request.AssetPath}'.";
                        return false;
                    }
                    break;
                case ShaderGraphAction.CreateGraph:
                case ShaderGraphAction.RenameGraph:
                case ShaderGraphAction.DuplicateGraph:
                case ShaderGraphAction.DeleteGraph:
                case ShaderGraphAction.MoveGraph:
                case ShaderGraphAction.SetGraphMetadata:
                case ShaderGraphAction.CreateCategory:
                case ShaderGraphAction.RenameCategory:
                case ShaderGraphAction.FindCategory:
                case ShaderGraphAction.DeleteCategory:
                case ShaderGraphAction.ReorderCategory:
                case ShaderGraphAction.MergeCategory:
                case ShaderGraphAction.DuplicateCategory:
                case ShaderGraphAction.SplitCategory:
                case ShaderGraphAction.ListCategories:
                case ShaderGraphAction.ReadGraphSummary:
                case ShaderGraphAction.ExportGraphContract:
                case ShaderGraphAction.ImportGraphContract:
                case ShaderGraphAction.FindNode:
                case ShaderGraphAction.FindProperty:
                case ShaderGraphAction.ListSupportedNodes:
                case ShaderGraphAction.ListSupportedProperties:
                case ShaderGraphAction.ListSupportedConnections:
                case ShaderGraphAction.UpdateProperty:
                case ShaderGraphAction.RenameProperty:
                case ShaderGraphAction.DuplicateProperty:
                case ShaderGraphAction.ReorderProperty:
                case ShaderGraphAction.MovePropertyToCategory:
                case ShaderGraphAction.RenameNode:
                case ShaderGraphAction.DuplicateNode:
                case ShaderGraphAction.MoveNode:
                case ShaderGraphAction.DeleteNode:
                case ShaderGraphAction.RemoveProperty:
                case ShaderGraphAction.AddProperty:
                case ShaderGraphAction.AddNode:
                case ShaderGraphAction.ConnectPorts:
                case ShaderGraphAction.FindConnection:
                case ShaderGraphAction.RemoveConnection:
                case ShaderGraphAction.ReconnectConnection:
                case ShaderGraphAction.SaveGraph:
                    if (isShaderSubGraph)
                    {
                        errorMessage = $"{request.Action} requires a .shadergraph asset path, got '{request.AssetPath}'.";
                        return false;
                    }
                    break;
            }

            return true;
        }

        private static string ResolveMoveGraphTargetAssetPath(ShaderGraphBatchmodeRequestPayload payload, string assetPath)
        {
            string rawTargetPath = FirstNonBlank(
                payload.targetAssetPath,
                payload.newAssetPath,
                payload.targetPath,
                payload.newPath,
                payload.destinationPath);
            if (string.IsNullOrWhiteSpace(rawTargetPath))
            {
                return null;
            }

            string normalizedTargetPath = rawTargetPath.Trim().Replace('\\', '/');
            if (LooksLikeShaderGraphAssetPath(normalizedTargetPath))
            {
                return normalizedTargetPath;
            }

            string sourceFileName = Path.GetFileName((assetPath ?? string.Empty).Trim().Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                return normalizedTargetPath;
            }

            return Path.Combine(normalizedTargetPath.TrimEnd('/'), sourceFileName).Replace('\\', '/');
        }

        private static string ResolveMoveSubGraphTargetAssetPath(ShaderGraphBatchmodeRequestPayload payload, string assetPath)
        {
            string rawTargetPath = FirstNonBlank(
                payload.targetAssetPath,
                payload.newAssetPath,
                payload.targetPath,
                payload.newPath,
                payload.destinationPath);
            if (string.IsNullOrWhiteSpace(rawTargetPath))
            {
                return null;
            }

            string normalizedTargetPath = rawTargetPath.Trim().Replace('\\', '/');
            if (normalizedTargetPath.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
            {
                return normalizedTargetPath;
            }

            string sourceFileName = Path.GetFileName((assetPath ?? string.Empty).Trim().Replace('\\', '/'));
            if (string.IsNullOrWhiteSpace(sourceFileName))
            {
                return normalizedTargetPath;
            }

            return Path.Combine(normalizedTargetPath.TrimEnd('/'), sourceFileName).Replace('\\', '/');
        }

        private static string FirstNonBlank(params string[] values)
        {
            if (values == null)
            {
                return null;
            }

            foreach (string value in values)
            {
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value.Trim();
                }
            }

            return null;
        }

        private static bool TryParseSingle(string value, out float result)
        {
            result = default;
            if (string.IsNullOrWhiteSpace(value))
            {
                return false;
            }

            return float.TryParse(
                value.Trim(),
                NumberStyles.Float | NumberStyles.AllowLeadingSign,
                CultureInfo.InvariantCulture,
                out result);
        }

        private static bool TryParseOptionalSingle(string value, out float result)
        {
            result = default;
            return !string.IsNullOrWhiteSpace(value) && TryParseSingle(value, out result);
        }

        private static string GetNameFromKnownAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            string normalized = assetPath.Trim().Replace('\\', '/');
            if (!normalized.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase) &&
                !normalized.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            return Path.GetFileNameWithoutExtension(normalized);
        }

        private static bool LooksLikeShaderGraphAssetPath(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().Replace('\\', '/').EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeShaderSubGraphAssetPath(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   value.Trim().Replace('\\', '/').EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase);
        }

        private static bool LooksLikeKnownShaderAssetPath(string value)
        {
            return !string.IsNullOrWhiteSpace(value) &&
                   (value.Trim().Replace('\\', '/').EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase) ||
                    value.Trim().Replace('\\', '/').EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase));
        }

        private static string TryGetArgumentValue(IEnumerable<string> args, IReadOnlyList<string> optionNames)
        {
            if (args == null)
            {
                return null;
            }

            string[] argArray = args as string[] ?? args.ToArray();
            for (int index = 0; index < argArray.Length; index += 1)
            {
                string currentArg = argArray[index];
                if (string.IsNullOrWhiteSpace(currentArg))
                {
                    continue;
                }

                for (int optionIndex = 0; optionIndex < optionNames.Count; optionIndex += 1)
                {
                    string optionName = optionNames[optionIndex];
                    if (string.Equals(currentArg, optionName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (index + 1 < argArray.Length && !string.IsNullOrWhiteSpace(argArray[index + 1]) && !argArray[index + 1].StartsWith("-", StringComparison.Ordinal))
                        {
                            return argArray[index + 1];
                        }
                    }

                    string prefix = $"{optionName}=";
                    if (currentArg.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    {
                        return currentArg.Substring(prefix.Length);
                    }
                }
            }

            return null;
        }

        private static string[] CollectTextValues(string singleValue, params string[][] valueSets)
        {
            var values = new List<string>();
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            void AddValue(string value)
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }

                string trimmed = value.Trim();
                if (seen.Add(trimmed))
                {
                    values.Add(trimmed);
                }
            }

            AddValue(singleValue);

            if (valueSets != null)
            {
                foreach (string[] valueSet in valueSets)
                {
                    if (valueSet == null)
                    {
                        continue;
                    }

                    foreach (string value in valueSet)
                    {
                        AddValue(value);
                    }
                }
            }

            return values.ToArray();
        }

        private static bool TryReadAllText(string filePath, out string content, out string errorMessage)
        {
            content = null;

            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "Request file path is required.";
                return false;
            }

            if (!File.Exists(filePath))
            {
                errorMessage = $"Request file does not exist: {filePath}";
                return false;
            }

            try
            {
                content = File.ReadAllText(filePath);
                errorMessage = null;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to read request file '{filePath}': {exception.Message}";
                return false;
            }
        }

        private static bool TryWriteResponseFile(string filePath, ShaderGraphResponse response, out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(filePath))
            {
                errorMessage = "Response file path is required.";
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrWhiteSpace(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(filePath, SerializeResponse(response), new UTF8Encoding(false));
                errorMessage = null;
                return true;
            }
            catch (Exception exception)
            {
                errorMessage = $"Failed to write response file '{filePath}': {exception.Message}";
                return false;
            }
        }

        private static string AppendMessage(string currentMessage, string additionalMessage)
        {
            if (string.IsNullOrWhiteSpace(currentMessage))
            {
                return additionalMessage;
            }

            if (string.IsNullOrWhiteSpace(additionalMessage))
            {
                return currentMessage;
            }

            return $"{currentMessage} {additionalMessage}";
        }

        private static void ExitIfNeeded(bool exitOnCompletion, int exitCode)
        {
            if (exitOnCompletion && Application.isBatchMode)
            {
                EditorApplication.Exit(exitCode);
            }
        }

        private static void AppendProperty(StringBuilder builder, string name, bool value)
        {
            builder.Append('"');
            builder.Append(name);
            builder.Append("\":");
            builder.Append(value ? "true" : "false");
        }

        private static void AppendProperty(StringBuilder builder, string name, string value)
        {
            builder.Append('"');
            builder.Append(name);
            builder.Append("\":");
            AppendString(builder, value);
        }

        private static void AppendValue(StringBuilder builder, object value)
        {
            if (value == null)
            {
                builder.Append("null");
                return;
            }

            if (value is string stringValue)
            {
                AppendString(builder, stringValue);
                return;
            }

            if (value is char character)
            {
                AppendString(builder, character.ToString());
                return;
            }

            if (value is bool booleanValue)
            {
                builder.Append(booleanValue ? "true" : "false");
                return;
            }

            if (value is Enum enumValue)
            {
                AppendString(builder, enumValue.ToString());
                return;
            }

            if (IsIntegerLike(value))
            {
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            if (value is float or double or decimal)
            {
                builder.Append(Convert.ToString(value, CultureInfo.InvariantCulture));
                return;
            }

            if (value is IDictionary dictionary)
            {
                AppendDictionary(builder, dictionary);
                return;
            }

            if (value is IReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                AppendDictionary(builder, readOnlyDictionary);
                return;
            }

            if (value is IEnumerable enumerable)
            {
                AppendEnumerable(builder, enumerable);
                return;
            }

            AppendString(builder, value.ToString());
        }

        private static void AppendDictionary(StringBuilder builder, IDictionary dictionary)
        {
            var entries = new List<KeyValuePair<string, object>>();
            foreach (DictionaryEntry entry in dictionary)
            {
                string key = Convert.ToString(entry.Key, CultureInfo.InvariantCulture) ?? string.Empty;
                entries.Add(new KeyValuePair<string, object>(key, entry.Value));
            }

            entries.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));

            builder.Append('{');
            for (int index = 0; index < entries.Count; index += 1)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                AppendString(builder, entries[index].Key);
                builder.Append(':');
                AppendValue(builder, entries[index].Value);
            }

            builder.Append('}');
        }

        private static void AppendDictionary(StringBuilder builder, IReadOnlyDictionary<string, object> dictionary)
        {
            var entries = new List<KeyValuePair<string, object>>();
            foreach (KeyValuePair<string, object> entry in dictionary)
            {
                entries.Add(new KeyValuePair<string, object>(entry.Key, entry.Value));
            }

            entries.Sort((left, right) => string.CompareOrdinal(left.Key, right.Key));

            builder.Append('{');
            for (int index = 0; index < entries.Count; index += 1)
            {
                if (index > 0)
                {
                    builder.Append(',');
                }

                AppendString(builder, entries[index].Key);
                builder.Append(':');
                AppendValue(builder, entries[index].Value);
            }

            builder.Append('}');
        }

        private static void AppendEnumerable(StringBuilder builder, IEnumerable enumerable)
        {
            builder.Append('[');
            bool isFirst = true;
            foreach (object item in enumerable)
            {
                if (!isFirst)
                {
                    builder.Append(',');
                }

                AppendValue(builder, item);
                isFirst = false;
            }

            builder.Append(']');
        }

        private static void AppendString(StringBuilder builder, string value)
        {
            builder.Append('"');
            if (!string.IsNullOrEmpty(value))
            {
                for (int index = 0; index < value.Length; index += 1)
                {
                    char character = value[index];
                    switch (character)
                    {
                        case '"':
                            builder.Append("\\\"");
                            break;
                        case '\\':
                            builder.Append("\\\\");
                            break;
                        case '\b':
                            builder.Append("\\b");
                            break;
                        case '\f':
                            builder.Append("\\f");
                            break;
                        case '\n':
                            builder.Append("\\n");
                            break;
                        case '\r':
                            builder.Append("\\r");
                            break;
                        case '\t':
                            builder.Append("\\t");
                            break;
                        default:
                            if (character < ' ')
                            {
                                builder.Append("\\u");
                                builder.Append(((int)character).ToString("x4", CultureInfo.InvariantCulture));
                            }
                            else
                            {
                                builder.Append(character);
                            }

                            break;
                    }
                }
            }

            builder.Append('"');
        }

        private static bool IsIntegerLike(object value)
        {
            return value is byte or sbyte or short or ushort or int or uint or long or ulong;
        }

        [Serializable]
        internal sealed class ShaderGraphBatchmodeInvocation
        {
            public string RequestFilePath { get; }
            public string ResponseFilePath { get; }

            public ShaderGraphBatchmodeInvocation(string requestFilePath, string responseFilePath)
            {
                RequestFilePath = requestFilePath;
                ResponseFilePath = responseFilePath;
            }
        }

        [Serializable]
        internal sealed class ShaderGraphBatchmodeRequestPayload
        {
            public string tool;
            public string action;
            public string assetPath;
            public string path;
            public string targetAssetPath;
            public string newAssetPath;
            public string targetPath;
            public string newPath;
            public string destinationPath;
            public string name;
            public string categoryName;
            public string sourceCategoryName;
            public string sourceDisplayName;
            public string sourceName;
            public string targetCategoryName;
            public string targetDisplayName;
            public string targetName;
            public string template;
            public string propertyName;
            public string[] propertyNames;
            public string[] properties;
            public string propertyType;
            public string defaultValue;
            public string graphPathLabel;
            public string pathLabel;
            public string graphDefaultPrecision;
            public string defaultPrecision;
            public string precision;
            public string graphContractJson;
            public string contractJson;
            public string categoryGuid;
            public string sourceCategoryGuid;
            public string targetCategoryGuid;
            public string referenceName;
            public string newReferenceName;
            public string index;
            public string newIndex;
            public string targetIndex;
            public string nodeId;
            public string objectId;
            public string anchorNodeId;
            public string anchorObjectId;
            public string anchorDisplayName;
            public string anchorNodeType;
            public string relativeToNodeId;
            public string relativeToObjectId;
            public string relativeToDisplayName;
            public string relativeToNodeType;
            public string direction;
            public string relativeDirection;
            public string spacing;
            public string layoutPreset;
            public string preset;
            public string x;
            public string y;
            public string nodeType;
            public string displayName;
            public string newDisplayName;
            public string outputNodeId;
            public string outputPort;
            public string inputNodeId;
            public string inputPort;
            public string sourceNodeId;
            public string sourcePort;
            public string targetNodeId;
            public string targetPort;
            public string oldOutputNodeId;
            public string oldOutputPort;
            public string oldInputNodeId;
            public string oldInputPort;
            public string oldSourceNodeId;
            public string oldSourcePort;
            public string oldTargetNodeId;
            public string oldTargetPort;
            public string newOutputNodeId;
            public string newOutputPort;
            public string newInputNodeId;
            public string newInputPort;
            public string newSourceNodeId;
            public string newSourcePort;
            public string newTargetNodeId;
            public string newTargetPort;
        }

        internal sealed class BatchmodeRunResult
        {
            public int ExitCode { get; set; }
            public string ErrorMessage { get; set; }
            public string RequestFilePath { get; set; }
            public string ResponseFilePath { get; set; }
            public ShaderGraphRequest Request { get; set; }
            public ShaderGraphResponse Response { get; set; }
        }
    }
}

namespace ShaderGraphMcp.Editor.Batchmode
{
    public static class ShaderGraphBatchEntryPoint
    {
        public static void Run()
        {
            ShaderGraphMcp.Editor.Tools.ShaderGraphBatchmodeBridge.Execute();
        }
    }
}
