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

            switch (NormalizeAction(payload.action))
            {
                case ShaderGraphAction.CreateGraph:
                    return TryCreateGraphRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.ReadGraphSummary:
                    return TryCreateReadGraphSummaryRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.FindNode:
                    return TryCreateFindNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.FindProperty:
                    return TryCreateFindPropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.ListSupportedNodes:
                    request = new ListSupportedNodesRequest();
                    errorMessage = null;
                    return true;
                case ShaderGraphAction.ListSupportedProperties:
                    request = new ListSupportedPropertiesRequest();
                    errorMessage = null;
                    return true;
                case ShaderGraphAction.UpdateProperty:
                    return TryCreateUpdatePropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.RenameProperty:
                    return TryCreateRenamePropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.DuplicateProperty:
                    return TryCreateDuplicatePropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.RenameNode:
                    return TryCreateRenameNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.DuplicateNode:
                    return TryCreateDuplicateNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.MoveNode:
                    return TryCreateMoveNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.DeleteNode:
                    return TryCreateDeleteNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.RemoveProperty:
                    return TryCreateRemovePropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.AddProperty:
                    return TryCreateAddPropertyRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.AddNode:
                    return TryCreateAddNodeRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.ConnectPorts:
                    return TryCreateConnectPortsRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.RemoveConnection:
                    return TryCreateRemoveConnectionRequest(payload, out request, out errorMessage);
                case ShaderGraphAction.SaveGraph:
                    return TryCreateSaveGraphRequest(payload, out request, out errorMessage);
                default:
                    errorMessage = $"Unsupported or missing action '{payload.action}'.";
                    return false;
            }
        }

        private static bool TryCreateGraphRequest(ShaderGraphBatchmodeRequestPayload payload, out ShaderGraphRequest request, out string errorMessage)
        {
            string name = FirstNonBlank(payload.name, GetNameFromAssetPath(payload.assetPath), GetNameFromAssetPath(payload.path));
            string directoryPath = FirstNonBlank(payload.path, payload.assetPath);

            if (!string.IsNullOrWhiteSpace(directoryPath) && LooksLikeShaderGraphAssetPath(directoryPath))
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

            if (!TryParseSingle(payload.x, out float x))
            {
                request = null;
                errorMessage = "Move node requires a valid x coordinate.";
                return false;
            }

            if (!TryParseSingle(payload.y, out float y))
            {
                request = null;
                errorMessage = "Move node requires a valid y coordinate.";
                return false;
            }

            request = new MoveNodeRequest(assetPath, nodeId, x, y);
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

            request = new AddNodeRequest(assetPath, payload.nodeType, payload.displayName);
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
                case "read_graph_summary":
                    return ShaderGraphAction.ReadGraphSummary;
                case "find_node":
                    return ShaderGraphAction.FindNode;
                case "find_property":
                    return ShaderGraphAction.FindProperty;
                case "list_supported_nodes":
                    return ShaderGraphAction.ListSupportedNodes;
                case "list_supported_properties":
                    return ShaderGraphAction.ListSupportedProperties;
                case "update_property":
                    return ShaderGraphAction.UpdateProperty;
                case "rename_property":
                    return ShaderGraphAction.RenameProperty;
                case "duplicate_property":
                    return ShaderGraphAction.DuplicateProperty;
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
                case "remove_connection":
                    return ShaderGraphAction.RemoveConnection;
                case "save_graph":
                    return ShaderGraphAction.SaveGraph;
            }

            return (ShaderGraphAction)(-1);
        }

        private static string ResolveAssetPath(ShaderGraphBatchmodeRequestPayload payload)
        {
            return FirstNonBlank(payload.assetPath, payload.path);
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

        private static string GetNameFromAssetPath(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return null;
            }

            string normalized = assetPath.Trim().Replace('\\', '/');
            if (!normalized.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase))
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

                        return string.Empty;
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
            public string name;
            public string template;
            public string propertyName;
            public string propertyType;
            public string defaultValue;
            public string referenceName;
            public string newReferenceName;
            public string nodeId;
            public string objectId;
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
