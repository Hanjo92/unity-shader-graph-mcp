using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Diagnostics
{
    internal sealed class ShaderGraphMcpPanelStatus
    {
        public ShaderGraphMcpPanelStatus(
            string unityVersion,
            string backendKind,
            bool packageDetected,
            int supportedNodeCount,
            int discoveredNodeCount,
            int unsupportedNodeCount,
            string nodeCatalogSemantics,
            string summary)
        {
            UnityVersion = unityVersion ?? string.Empty;
            BackendKind = backendKind ?? string.Empty;
            PackageDetected = packageDetected;
            SupportedNodeCount = supportedNodeCount;
            DiscoveredNodeCount = discoveredNodeCount;
            UnsupportedNodeCount = unsupportedNodeCount;
            NodeCatalogSemantics = nodeCatalogSemantics ?? string.Empty;
            Summary = summary ?? string.Empty;
        }

        public string UnityVersion { get; }

        public string BackendKind { get; }

        public bool PackageDetected { get; }

        public int SupportedNodeCount { get; }

        public int DiscoveredNodeCount { get; }

        public int UnsupportedNodeCount { get; }

        public string NodeCatalogSemantics { get; }

        public string Summary { get; }
    }

    internal sealed class ShaderGraphMcpPanel : EditorWindow
    {
        private const string DefaultGraphDirectory = "Assets/ShaderGraphs";
        private const string DefaultGraphPath = "Assets/ShaderGraphs/ConnectSmoke.shadergraph";

        private Vector2 scrollPosition;
        private UnityEngine.Object targetGraphAsset;
        private string targetGraphPath = DefaultGraphPath;
        private string logText = "Ready.";
        private ShaderGraphMcpPanelStatus cachedStatus;

        [MenuItem("Tools/Shader Graph MCP/Open Panel")]
        public static void OpenPanel()
        {
            var window = GetWindow<ShaderGraphMcpPanel>("Shader Graph MCP");
            window.minSize = new Vector2(460f, 520f);
            window.Show();
        }

        internal static ShaderGraphMcpPanelStatus BuildStatus()
        {
            ShaderGraphCompatibilitySnapshot compatibility = ShaderGraphPackageCompatibility.Capture();
            ShaderGraphResponse nodesResponse = ShaderGraphAssetTool.HandleListSupportedNodes();

            IReadOnlyDictionary<string, object> data = nodesResponse?.Data;
            string backendKind = GetString(data, "backendKind", compatibility.BackendKind.ToString());
            bool packageDetected = GetBool(data, "packageDetected", compatibility.HasShaderGraphPackage);
            int supportedNodeCount = GetInt(data, "supportedNodeCount", 0);
            int discoveredNodeCount = GetInt(data, "discoveredNodeCount", 0);
            string nodeCatalogSemantics = GetString(data, "nodeCatalogSemantics", string.Empty);

            int unsupportedNodeCount = 0;
            if (TryGetDictionary(data, "nodeCatalogClassification", out IReadOnlyDictionary<string, object> classification))
            {
                unsupportedNodeCount = GetInt(classification, "unsupportedCount", 0);
            }

            string summary =
                $"{backendKind} | packageDetected={packageDetected} | " +
                $"supportedNodes={supportedNodeCount} | discoveredNodes={discoveredNodeCount}";

            return new ShaderGraphMcpPanelStatus(
                Application.unityVersion,
                backendKind,
                packageDetected,
                supportedNodeCount,
                discoveredNodeCount,
                unsupportedNodeCount,
                nodeCatalogSemantics,
                summary);
        }

        internal static string ResolveTargetDirectory(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return DefaultGraphDirectory;
            }

            string normalizedPath = assetPath.Trim().Replace('\\', '/').TrimEnd('/');
            if (normalizedPath.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase) ||
                normalizedPath.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
            {
                string directory = Path.GetDirectoryName(normalizedPath)?.Replace('\\', '/');
                return string.IsNullOrWhiteSpace(directory) ? DefaultGraphDirectory : directory;
            }

            return normalizedPath;
        }

        internal static string BuildCodexMcpCommand()
        {
            return "python3.12 server/src/unity_shader_graph_mcp/__main__.py --mcp";
        }

        private void OnEnable()
        {
            RefreshSelectedGraphPath();
            RefreshStatus();
        }

        private void OnSelectionChange()
        {
            RefreshSelectedGraphPath();
            Repaint();
        }

        private void OnGUI()
        {
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

            DrawHeader();
            DrawStatusSection();
            DrawTargetGraphSection();
            DrawSetupActionsSection();
            DrawMcpClientSection();
            DrawLogSection();

            EditorGUILayout.EndScrollView();
        }

        private void DrawHeader()
        {
            EditorGUILayout.LabelField("Shader Graph MCP", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Use this panel as the setup hub for package-backed Shader Graph MCP workflows. " +
                "The existing debug menu remains available for deeper smoke cases.",
                MessageType.Info);
        }

        private void DrawStatusSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Status", EditorStyles.boldLabel);

            if (cachedStatus == null)
            {
                RefreshStatus();
            }

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                EditorGUILayout.LabelField("Unity", cachedStatus.UnityVersion);
                EditorGUILayout.LabelField("Backend", cachedStatus.BackendKind);
                EditorGUILayout.LabelField("Package Detected", cachedStatus.PackageDetected ? "Yes" : "No");
                EditorGUILayout.LabelField("Supported Nodes", cachedStatus.SupportedNodeCount.ToString());
                EditorGUILayout.LabelField("Discovered Nodes", cachedStatus.DiscoveredNodeCount.ToString());
                EditorGUILayout.LabelField("Diagnostic-Only Nodes", cachedStatus.UnsupportedNodeCount.ToString());
                EditorGUILayout.LabelField("Catalog Semantics", cachedStatus.NodeCatalogSemantics);
            }

            if (GUILayout.Button("Refresh Status"))
            {
                RefreshStatus();
                AppendLog("Status refreshed.", cachedStatus.Summary);
            }
        }

        private void DrawTargetGraphSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Target Graph", EditorStyles.boldLabel);

            EditorGUI.BeginChangeCheck();
            targetGraphAsset = EditorGUILayout.ObjectField("Selected Asset", targetGraphAsset, typeof(UnityEngine.Object), false);
            if (EditorGUI.EndChangeCheck())
            {
                string selectedPath = AssetDatabase.GetAssetPath(targetGraphAsset);
                if (!string.IsNullOrWhiteSpace(selectedPath))
                {
                    targetGraphPath = selectedPath;
                }
            }

            targetGraphPath = EditorGUILayout.TextField("Asset Path", targetGraphPath);

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Use Selection"))
                {
                    RefreshSelectedGraphPath();
                    AppendLog("Target graph updated from current selection.", targetGraphPath);
                }

                if (GUILayout.Button("Ping Target"))
                {
                    PingAsset(targetGraphPath);
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Read Summary"))
                {
                    RunResponseAction(
                        "read_graph_summary",
                        targetGraphPath,
                        ShaderGraphAssetTool.HandleReadGraphSummary(targetGraphPath));
                }

                if (GUILayout.Button("Save Graph"))
                {
                    RunResponseAction(
                        "save_graph",
                        targetGraphPath,
                        ShaderGraphAssetTool.HandleSaveGraph(targetGraphPath));
                }
            }
        }

        private void DrawSetupActionsSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Setup & Diagnostics", EditorStyles.boldLabel);

            if (GUILayout.Button("Run Blank Graph Happy Path"))
            {
                string targetDirectory = ResolveTargetDirectory(targetGraphPath);
                string graphName = $"PanelSmokeGraph {DateTime.Now:HHmmss}";
                ShaderGraphResponse response = ShaderGraphDebugMenu.RunBlankGraphHappyPath(targetDirectory, graphName, true);
                string assetPath = $"{targetDirectory.TrimEnd('/')}/{graphName}.shadergraph";
                targetGraphPath = assetPath;
                targetGraphAsset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                RunResponseAction("release_happy_path", assetPath, response);
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Write Compatibility Report"))
                {
                    try
                    {
                        string assetPath = ShaderGraphCompatibilityReport.CaptureAndWriteReport();
                        PingAsset(assetPath);
                        AppendLog("Compatibility report written.", assetPath);
                    }
                    catch (Exception ex)
                    {
                        AppendLog("Compatibility report failed.", ex.ToString());
                        Debug.LogError($"[ShaderGraphMcp] Compatibility report failed: {ex}");
                    }
                }

                if (GUILayout.Button("Write Node Catalog Report"))
                {
                    ShaderGraphDebugMenu.WriteNodeCatalogReport();
                    AppendLog("Node catalog report requested.", "See Assets/ShaderGraphMcpDiagnostics and the Console for details.");
                }
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Open Test Runner"))
                {
                    EditorApplication.ExecuteMenuItem("Window/General/Test Runner");
                }

                if (GUILayout.Button("Open Install Docs"))
                {
                    OpenProjectFile("docs/install-and-happy-path.md");
                }
            }
        }

        private void DrawMcpClientSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("MCP Client", EditorStyles.boldLabel);

            string command = BuildCodexMcpCommand();
            EditorGUILayout.SelectableLabel(command, EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight));

            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Copy Codex Command"))
                {
                    EditorGUIUtility.systemCopyBuffer = command;
                    AppendLog("Copied Codex MCP command.", command);
                }

                if (GUILayout.Button("List Supported Nodes"))
                {
                    RunResponseAction(
                        "list_supported_nodes",
                        string.Empty,
                        ShaderGraphAssetTool.HandleListSupportedNodes());
                    RefreshStatus();
                }
            }
        }

        private void DrawLogSection()
        {
            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Last Result", EditorStyles.boldLabel);
            EditorGUILayout.TextArea(logText, GUILayout.MinHeight(140f));
        }

        private void RefreshStatus()
        {
            cachedStatus = BuildStatus();
        }

        private void RefreshSelectedGraphPath()
        {
            UnityEngine.Object selected = Selection.activeObject;
            string selectedPath = selected == null ? string.Empty : AssetDatabase.GetAssetPath(selected);
            if (string.IsNullOrWhiteSpace(selectedPath))
            {
                return;
            }

            targetGraphAsset = selected;
            if (selectedPath.EndsWith(".shadergraph", StringComparison.OrdinalIgnoreCase) ||
                selectedPath.EndsWith(".shadersubgraph", StringComparison.OrdinalIgnoreCase))
            {
                targetGraphPath = selectedPath;
            }
            else if (AssetDatabase.IsValidFolder(selectedPath))
            {
                targetGraphPath = $"{selectedPath.TrimEnd('/')}/ConnectSmoke.shadergraph";
            }
        }

        private void RunResponseAction(string action, string assetPath, ShaderGraphResponse response)
        {
            string text = FormatResponse(action, assetPath, response);
            logText = text;

            if (response?.Success == true)
            {
                Debug.Log(text);
            }
            else
            {
                Debug.LogError(text);
            }
        }

        private static string FormatResponse(string action, string assetPath, ShaderGraphResponse response)
        {
            if (response == null)
            {
                return $"[ShaderGraphMcp] action={action}\nassetPath={assetPath}\nresponse=<null>";
            }

            var builder = new StringBuilder();
            builder.AppendLine($"[ShaderGraphMcp] action={action}");
            if (!string.IsNullOrWhiteSpace(assetPath))
            {
                builder.AppendLine($"assetPath={assetPath}");
            }

            builder.AppendLine($"success={response.Success}");
            builder.AppendLine($"message={response.Message}");

            if (response.Data != null && response.Data.Count > 0)
            {
                builder.AppendLine("data:");
                AppendDictionary(builder, response.Data, "  ");
            }

            return builder.ToString();
        }

        private void AppendLog(string title, string details)
        {
            logText = string.IsNullOrWhiteSpace(details)
                ? title
                : $"{title}\n{details}";
            Debug.Log($"[ShaderGraphMcp] {logText}");
        }

        private static void PingAsset(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
            if (asset == null)
            {
                Debug.LogWarning($"[ShaderGraphMcp] Could not find asset '{assetPath}'.");
                return;
            }

            Selection.activeObject = asset;
            EditorGUIUtility.PingObject(asset);
        }

        private static void OpenProjectFile(string relativePath)
        {
            string root = Directory.GetParent(Application.dataPath)?.FullName ?? Application.dataPath;
            string absolutePath = Path.Combine(root, relativePath.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(absolutePath))
            {
                EditorUtility.OpenWithDefaultApp(absolutePath);
            }
        }

        private static bool TryGetDictionary(
            IReadOnlyDictionary<string, object> dictionary,
            string key,
            out IReadOnlyDictionary<string, object> nested)
        {
            nested = null;
            if (dictionary == null || !dictionary.TryGetValue(key, out object value))
            {
                return false;
            }

            if (value is IReadOnlyDictionary<string, object> readOnlyDictionary)
            {
                nested = readOnlyDictionary;
                return true;
            }

            if (value is IDictionary rawDictionary)
            {
                var converted = new Dictionary<string, object>();
                foreach (DictionaryEntry entry in rawDictionary)
                {
                    converted[entry.Key?.ToString() ?? string.Empty] = entry.Value;
                }

                nested = converted;
                return true;
            }

            return false;
        }

        private static string GetString(IReadOnlyDictionary<string, object> dictionary, string key, string fallback)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out object value))
            {
                return fallback;
            }

            return value?.ToString() ?? fallback;
        }

        private static int GetInt(IReadOnlyDictionary<string, object> dictionary, string key, int fallback)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out object value))
            {
                return fallback;
            }

            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return unchecked((int)longValue);
            }

            return int.TryParse(value?.ToString(), out int parsed)
                ? parsed
                : fallback;
        }

        private static bool GetBool(IReadOnlyDictionary<string, object> dictionary, string key, bool fallback)
        {
            if (dictionary == null || !dictionary.TryGetValue(key, out object value))
            {
                return fallback;
            }

            if (value is bool boolValue)
            {
                return boolValue;
            }

            return bool.TryParse(value?.ToString(), out bool parsed)
                ? parsed
                : fallback;
        }

        private static void AppendDictionary(
            StringBuilder builder,
            IReadOnlyDictionary<string, object> dictionary,
            string indent)
        {
            foreach (KeyValuePair<string, object> pair in dictionary)
            {
                if (pair.Value is IReadOnlyDictionary<string, object> nestedReadOnly)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    AppendDictionary(builder, nestedReadOnly, indent + "  ");
                    continue;
                }

                if (pair.Value is IDictionary nestedDictionary)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    AppendDictionary(builder, ToDictionary(nestedDictionary), indent + "  ");
                    continue;
                }

                if (pair.Value is IEnumerable enumerable && pair.Value is not string)
                {
                    builder.AppendLine($"{indent}{pair.Key}:");
                    foreach (object item in enumerable)
                    {
                        builder.AppendLine($"{indent}  - {item}");
                    }
                    continue;
                }

                builder.AppendLine($"{indent}{pair.Key}: {pair.Value}");
            }
        }

        private static IReadOnlyDictionary<string, object> ToDictionary(IDictionary dictionary)
        {
            var result = new Dictionary<string, object>();
            foreach (DictionaryEntry entry in dictionary)
            {
                result[entry.Key?.ToString() ?? string.Empty] = entry.Value;
            }

            return result;
        }
    }
}
