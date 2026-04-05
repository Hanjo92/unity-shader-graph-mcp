using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;
using ShaderGraphMcp.Editor.Tools;
using UnityEditor;

namespace ShaderGraphMcp.Editor.Tests
{
    internal static class ShaderGraphTestAssets
    {
        public const string DefaultRootFolder = "Assets/ShaderGraphMcpSmokeTests";

        public static void RequirePackageReady()
        {
            ShaderGraphCompatibilitySnapshot snapshot = ShaderGraphPackageCompatibility.Capture();
            if (snapshot.BackendKind != ShaderGraphBackendKind.PackageReady)
            {
                Assert.Ignore(
                    $"Package-backed Shader Graph APIs are not ready in this editor. backendKind={snapshot.BackendKind}.");
            }
        }

        public static TemporaryFolderScope CreateTemporaryFolder(string testName)
        {
            string folderName = SanitizeName(testName);
            string relativeFolderPath = EnsureFolderHierarchy(DefaultRootFolder, folderName);
            return new TemporaryFolderScope(relativeFolderPath);
        }

        public static string CreateTemporaryGraphPath(string testName, string assetName, string extension = ".shadergraph")
        {
            string folderPath = NormalizeAssetPath($"{DefaultRootFolder}/{SanitizeName(testName)}");
            return CombineAssetPath(folderPath, assetName, extension);
        }

        public static void RequirePackageReady(ShaderGraphResponse response)
        {
            Assert.That(response, Is.Not.Null, "Shader Graph response is required.");
            Assert.That(response.Success, Is.True, response?.Message);

            string backendKind = GetString(response.Data, "backendKind");
            Assert.That(backendKind, Is.EqualTo(ShaderGraphBackendKind.PackageReady.ToString()));
        }

        public static IReadOnlyDictionary<string, object> RequireDictionary(
            IReadOnlyDictionary<string, object> dictionary,
            string key)
        {
            object value = RequireValue(dictionary, key);
            if (value is IReadOnlyDictionary<string, object> nestedReadOnly)
            {
                return nestedReadOnly;
            }

            if (value is IDictionary nestedDictionary)
            {
                return ToDictionary(nestedDictionary);
            }

            Assert.Fail($"Expected '{key}' to be a nested dictionary.");
            return null;
        }

        public static string GetString(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            object value = RequireValue(dictionary, key);
            return value?.ToString() ?? string.Empty;
        }

        public static int GetInt(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            object value = RequireValue(dictionary, key);
            if (value is int intValue)
            {
                return intValue;
            }

            if (value is long longValue)
            {
                return unchecked((int)longValue);
            }

            if (value is float floatValue)
            {
                return MathfRoundToInt(floatValue);
            }

            if (value is double doubleValue)
            {
                return MathfRoundToInt((float)doubleValue);
            }

            Assert.Fail($"Expected '{key}' to be an integer value.");
            return -1;
        }

        public static IReadOnlyList<string> GetStringList(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            object value = RequireValue(dictionary, key);

            if (value is string[] stringArray)
            {
                return stringArray;
            }

            if (value is IEnumerable enumerable && value is not string)
            {
                var result = new List<string>();
                foreach (object item in enumerable)
                {
                    result.Add(item?.ToString() ?? string.Empty);
                }

                return result;
            }

            Assert.Fail($"Expected '{key}' to be a list of strings.");
            return Array.Empty<string>();
        }

        public static void DeleteAssetIfExists(string assetPath)
        {
            if (string.IsNullOrWhiteSpace(assetPath))
            {
                return;
            }

            if (AssetDatabase.DeleteAsset(assetPath))
            {
                AssetDatabase.Refresh();
            }
        }

        public static string GetAddedNodeId(ShaderGraphResponse response)
        {
            if (response?.Data == null || !response.Data.TryGetValue("addedNode", out object rawAddedNode))
            {
                return string.Empty;
            }

            if (rawAddedNode is IReadOnlyDictionary<string, object> readOnlyAddedNode &&
                readOnlyAddedNode.TryGetValue("objectId", out object readOnlyObjectId))
            {
                return readOnlyObjectId?.ToString()?.Trim() ?? string.Empty;
            }

            if (rawAddedNode is IDictionary dictionary && dictionary.Contains("objectId"))
            {
                return dictionary["objectId"]?.ToString()?.Trim() ?? string.Empty;
            }

            return string.Empty;
        }

        public static string SerializeToJson(object value)
        {
            return ShaderGraphBatchmodeBridge.SerializeValueToJson(value);
        }

        public static string CombineAssetPath(string folderPath, string assetName, string extension)
        {
            string normalizedFolder = NormalizeAssetPath(folderPath);
            string normalizedExtension = string.IsNullOrWhiteSpace(extension) ? ".shadergraph" : extension.Trim();
            if (!normalizedExtension.StartsWith(".", StringComparison.Ordinal))
            {
                normalizedExtension = "." + normalizedExtension;
            }

            return NormalizeAssetPath($"{normalizedFolder}/{SanitizeName(assetName)}{normalizedExtension}");
        }

        private static object RequireValue(IReadOnlyDictionary<string, object> dictionary, string key)
        {
            if (dictionary == null)
            {
                Assert.Fail("Dictionary is required.");
                return null;
            }

            if (string.IsNullOrWhiteSpace(key))
            {
                Assert.Fail("Key is required.");
                return null;
            }

            if (!dictionary.TryGetValue(key, out object value))
            {
                Assert.Fail($"Dictionary is missing key '{key}'.");
                return null;
            }

            return value;
        }

        private static string EnsureFolderHierarchy(string rootFolder, string childFolder)
        {
            string normalizedRoot = NormalizeAssetPath(rootFolder);
            if (!AssetDatabase.IsValidFolder(normalizedRoot))
            {
                EnsureFolderChain("Assets", normalizedRoot.Substring("Assets/".Length));
            }

            string currentFolder = normalizedRoot;
            if (!string.IsNullOrWhiteSpace(childFolder))
            {
                string[] segments = childFolder.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string segment in segments)
                {
                    currentFolder = EnsureChildFolder(currentFolder, segment);
                }
            }

            return currentFolder;
        }

        private static string EnsureFolderChain(string parentFolder, string relativePath)
        {
            string currentFolder = NormalizeAssetPath(parentFolder);
            string[] segments = relativePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string segment in segments)
            {
                currentFolder = EnsureChildFolder(currentFolder, segment);
            }

            return currentFolder;
        }

        private static string EnsureChildFolder(string parentFolder, string childName)
        {
            string normalizedParent = NormalizeAssetPath(parentFolder);
            string normalizedChildName = SanitizeName(childName);
            string candidate = NormalizeAssetPath($"{normalizedParent}/{normalizedChildName}");
            if (!AssetDatabase.IsValidFolder(candidate))
            {
                AssetDatabase.CreateFolder(normalizedParent, normalizedChildName);
            }

            return candidate;
        }

        private static string SanitizeName(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "Unnamed";
            }

            var builder = new System.Text.StringBuilder(value.Length);
            foreach (char character in value)
            {
                if (char.IsLetterOrDigit(character) || character == '-' || character == '_' || character == ' ')
                {
                    builder.Append(character);
                }
            }

            string sanitized = builder.ToString().Trim();
            return string.IsNullOrWhiteSpace(sanitized) ? "Unnamed" : sanitized;
        }

        private static string NormalizeAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            return path.Replace('\\', '/').TrimEnd('/');
        }

        private static int MathfRoundToInt(float value)
        {
            return (int)Math.Round(value, MidpointRounding.AwayFromZero);
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

        public sealed class TemporaryFolderScope : IDisposable
        {
            public string AssetPath { get; }

            public TemporaryFolderScope(string assetPath)
            {
                AssetPath = NormalizeAssetPath(assetPath);
            }

            public void Dispose()
            {
                if (!string.IsNullOrWhiteSpace(AssetPath) && AssetDatabase.IsValidFolder(AssetPath))
                {
                    string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { AssetPath });
                    Array.Sort(guids, StringComparer.OrdinalIgnoreCase);
                    Array.Reverse(guids);
                    foreach (string guid in guids)
                    {
                        string childAssetPath = AssetDatabase.GUIDToAssetPath(guid);
                        if (!string.IsNullOrWhiteSpace(childAssetPath))
                        {
                            AssetDatabase.DeleteAsset(childAssetPath);
                        }
                    }

                    AssetDatabase.DeleteAsset(AssetPath);
                }

                if (AssetDatabase.IsValidFolder(DefaultRootFolder))
                {
                    string[] rootGuids = AssetDatabase.FindAssets(string.Empty, new[] { DefaultRootFolder });
                    if (rootGuids == null || rootGuids.Length == 0)
                    {
                        AssetDatabase.DeleteAsset(DefaultRootFolder);
                    }
                }

                AssetDatabase.Refresh();
            }
        }
    }
}
