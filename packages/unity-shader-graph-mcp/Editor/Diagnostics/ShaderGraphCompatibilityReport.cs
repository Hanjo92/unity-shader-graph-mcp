using System;
using System.IO;
using System.Linq;
using System.Text;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;
using UnityEditor;
using UnityEngine;

namespace ShaderGraphMcp.Editor.Diagnostics
{
    internal static class ShaderGraphCompatibilityReport
    {
        private const string DiagnosticsFolder = "Assets/ShaderGraphMcpDiagnostics";

        public static string CaptureAndWriteReport()
        {
            var snapshot = ShaderGraphPackageCompatibility.Capture();
            string assetPath = BuildReportAssetPath();
            string absolutePath = ToAbsolutePath(assetPath);

            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? Application.dataPath);
            File.WriteAllText(absolutePath, BuildReportText(snapshot), new UTF8Encoding(false));
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);

            return assetPath;
        }

        internal static string BuildReportText(ShaderGraphCompatibilitySnapshot snapshot)
        {
            var builder = new StringBuilder();

            builder.AppendLine("# Shader Graph Compatibility Report");
            builder.AppendLine();
            builder.AppendLine($"generatedUtc: {DateTime.UtcNow:O}");
            builder.AppendLine($"backendKind: {snapshot.BackendKind}");
            builder.AppendLine($"packageDetected: {snapshot.HasShaderGraphPackage}");
            builder.AppendLine($"graphTypeName: {snapshot.GraphSurface.GraphTypeName}");
            builder.AppendLine($"baseTypeName: {snapshot.GraphSurface.BaseTypeName}");
            builder.AppendLine($"hasCoreMutationSurface: {snapshot.GraphSurface.HasCoreMutationSurface}");
            builder.AppendLine($"hasAddGraphInput: {snapshot.GraphSurface.HasAddGraphInput}");
            builder.AppendLine($"hasAddNode: {snapshot.GraphSurface.HasAddNode}");
            builder.AppendLine($"hasConnect: {snapshot.GraphSurface.HasConnect}");
            builder.AppendLine($"hasValidateGraph: {snapshot.GraphSurface.HasValidateGraph}");
            builder.AppendLine();

            AppendList(builder, "detectedAssemblies", snapshot.DetectedAssemblies);
            AppendList(builder, "candidateTypeNames", snapshot.CandidateTypeNames);
            AppendList(builder, "resolvedTypes", snapshot.ResolvedTypes);
            AppendList(builder, "missingTypes", snapshot.MissingTypes);
            AppendList(builder, "discoveredTypeNames", snapshot.DiscoveredTypeNames);
            AppendList(builder, "resolvedMethodSignatures", snapshot.GraphSurface.ResolvedMethodSignatures);
            AppendList(builder, "missingMethodNames", snapshot.GraphSurface.MissingMethodNames);
            AppendList(builder, "notes", snapshot.Notes);

            builder.AppendLine("## Next Spike");
            builder.AppendLine("- Verify the resolved graph type and resolved method signatures match Unity 2022.3 Shader Graph.");
            builder.AppendLine("- Only switch to ShaderGraphPackageBackend after AddGraphInput, AddNode, Connect, and ValidateGraph are confirmed.");
            builder.AppendLine("- If type names differ, update ShaderGraphPackageCompatibility candidate lists before attempting mutations.");

            return builder.ToString();
        }

        private static void AppendList(StringBuilder builder, string title, System.Collections.Generic.IReadOnlyList<string> items)
        {
            builder.AppendLine($"## {title}");
            if (items == null || items.Count == 0)
            {
                builder.AppendLine("- <none>");
                builder.AppendLine();
                return;
            }

            foreach (string item in items.Where(value => !string.IsNullOrWhiteSpace(value)))
            {
                builder.AppendLine($"- {item}");
            }

            builder.AppendLine();
        }

        private static string BuildReportAssetPath()
        {
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd-HHmmss");
            return $"{DiagnosticsFolder}/shadergraph-compatibility-{timestamp}.txt";
        }

        private static string ToAbsolutePath(string assetPath)
        {
            string relative = assetPath.StartsWith("Assets/", StringComparison.Ordinal)
                ? assetPath.Substring("Assets/".Length)
                : assetPath;
            return Path.Combine(Application.dataPath, relative.Replace('/', Path.DirectorySeparatorChar));
        }
    }

    internal static class ShaderGraphCompatibilityReportMenu
    {
        [MenuItem("Tools/Shader Graph MCP/Write Compatibility Report")]
        public static void WriteCompatibilityReport()
        {
            try
            {
                string assetPath = ShaderGraphCompatibilityReport.CaptureAndWriteReport();
                UnityEngine.Object asset = AssetDatabase.LoadAssetAtPath<UnityEngine.Object>(assetPath);
                if (asset != null)
                {
                    EditorGUIUtility.PingObject(asset);
                }

                Debug.Log(
                    $"[ShaderGraphMcp] Compatibility report written to '{assetPath}'. " +
                    "Inspect this report before attempting a real Shader Graph backend mutation."
                );
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ShaderGraphMcp] Failed to write compatibility report: {ex}");
            }
        }
    }
}
