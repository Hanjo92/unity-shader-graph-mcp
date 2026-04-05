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
            builder.AppendLine($"unityVersion: {Application.unityVersion}");
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

            AppendSection(builder, "Compatibility Snapshot");
            AppendList(builder, "detectedAssemblies", snapshot.DetectedAssemblies);
            AppendList(builder, "candidateTypeNames", snapshot.CandidateTypeNames);
            AppendList(builder, "resolvedTypes", snapshot.ResolvedTypes);
            AppendList(builder, "missingTypes", snapshot.MissingTypes);
            AppendList(builder, "discoveredTypeNames", snapshot.DiscoveredTypeNames);
            AppendList(builder, "resolvedMethodSignatures", snapshot.GraphSurface.ResolvedMethodSignatures);
            AppendList(builder, "missingMethodNames", snapshot.GraphSurface.MissingMethodNames);
            AppendList(builder, "notes", snapshot.Notes);
            AppendList(builder, "Fallback Behavior", BuildFallbackBehavior(snapshot));

            AppendSection(builder, "How To Capture");
            builder.AppendLine("- Run `Tools > Shader Graph MCP > Write Compatibility Report` from the Unity editor.");
            builder.AppendLine("- Regenerate the report after changing Unity or Shader Graph versions.");
            builder.AppendLine("- Compare `unityVersion`, `backendKind`, and the resolved `GraphData` surface before trusting a new upgrade.");
            builder.AppendLine();

            builder.AppendLine("## Next Spike");
            builder.AppendLine("- Verify the resolved graph type and resolved method signatures still match the current Unity/Shader Graph baseline.");
            builder.AppendLine("- Keep the compatibility report aligned with the observed editor version whenever the package surface changes.");
            builder.AppendLine("- If type names differ, update ShaderGraphPackageCompatibility candidate lists before attempting mutations.");

            return builder.ToString();
        }

        private static void AppendSection(StringBuilder builder, string title)
        {
            builder.AppendLine($"## {title}");
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

        private static System.Collections.Generic.IReadOnlyList<string> BuildFallbackBehavior(ShaderGraphCompatibilitySnapshot snapshot)
        {
            if (!snapshot.HasShaderGraphPackage)
            {
                return new[]
                {
                    "No Shader Graph package assembly was detected, so the default backend remains scaffold-only.",
                };
            }

            if (snapshot.BackendKind == ShaderGraphBackendKind.PackageDetectedButIncomplete)
            {
                return new[]
                {
                    "Shader Graph package is detected, but the core mutation surface is incomplete; the default backend stays on scaffold fallback for unsupported actions.",
                };
            }

            return new[]
            {
                "The package-backed backend is available for the verified core mutation surface; unsupported actions still fall back to scaffold-safe behavior when no package-backed path exists.",
            };
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
