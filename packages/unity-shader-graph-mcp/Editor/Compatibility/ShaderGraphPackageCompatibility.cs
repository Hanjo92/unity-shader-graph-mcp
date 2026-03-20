using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Compatibility
{
    internal static class ShaderGraphPackageCompatibility
    {
        private static readonly string[] CandidateTypeNames =
        {
            "UnityEditor.ShaderGraph.GraphData",
            "UnityEditor.ShaderGraph.GraphObject",
            "UnityEditor.ShaderGraph.AbstractMaterialNode",
            "UnityEditor.ShaderGraph.AbstractShaderProperty",
            "UnityEditor.ShaderGraph.BooleanShaderProperty",
            "UnityEditor.ShaderGraph.ColorShaderProperty",
            "UnityEditor.ShaderGraph.Vector1ShaderProperty",
            "UnityEditor.ShaderGraph.Vector2ShaderProperty",
            "UnityEditor.ShaderGraph.Vector3ShaderProperty",
            "UnityEditor.ShaderGraph.Vector4ShaderProperty",
            "UnityEditor.ShaderGraph.TextureShaderProperty",
            "UnityEditor.ShaderGraph.IntShaderProperty",
            "UnityEditor.ShaderGraph.PropertyCollector",
            "UnityEditor.ShaderGraph.MaterialSlot",
            "UnityEditor.ShaderGraph.IShaderProperty",
            "UnityEditor.ShaderGraph.Serialization.JsonObject",
        };

        private static readonly string[] CandidateMethodNames =
        {
            "AddGraphInput",
            "Connect",
            "AddNode",
            "ValidateGraph",
            "ReplaceWith",
            "GetNodes",
            "GetNodeFromGuid",
            "ContainsNodeGuid",
            "RemoveNode",
            "RemoveShaderProperty",
        };

        private static readonly string[] CandidateAssemblyHints =
        {
            "ShaderGraph",
            "shadergraph",
        };

        public static ShaderGraphCompatibilitySnapshot Capture()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            string[] shaderGraphAssemblies = assemblies
                .Where(IsShaderGraphAssembly)
                .Select(assembly => assembly.GetName().Name ?? assembly.FullName)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            var discoveredTypes = assemblies
                .SelectMany(SafeGetTypes)
                .Where(IsInterestingShaderGraphType)
                .Select(type => type.FullName ?? type.Name)
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name, StringComparer.OrdinalIgnoreCase)
                .Take(100)
                .ToArray();

            var resolvedTypes = new List<string>();
            var missingTypes = new List<string>();

            foreach (string candidate in CandidateTypeNames)
            {
                Type resolved = ResolveType(candidate, assemblies);
                if (resolved != null)
                {
                    resolvedTypes.Add(resolved.FullName ?? candidate);
                }
                else
                {
                    missingTypes.Add(candidate);
                }
            }

            Type graphType = ResolveType("UnityEditor.ShaderGraph.GraphData", assemblies)
                ?? ResolveType("UnityEditor.ShaderGraph.GraphObject", assemblies);

            ShaderGraphApiSurface surface = DescribeGraphSurface(graphType);
            ShaderGraphBackendKind backendKind = DetermineBackendKind(shaderGraphAssemblies, surface);

            var notes = new List<string>();
            if (shaderGraphAssemblies.Length == 0)
            {
                notes.Add("Shader Graph package assembly not detected in the current editor AppDomain.");
            }
            else
            {
                notes.Add("Shader Graph package assembly detected.");
            }

            if (surface.IsResolved)
            {
                notes.Add("GraphData surface resolved through reflection.");
            }
            else
            {
                notes.Add("Concrete Shader Graph graph API surface is not yet resolved.");
            }

            if (discoveredTypes.Length > 0)
            {
                notes.Add($"Observed {discoveredTypes.Length} Shader Graph-related type names in loaded assemblies.");
            }

            return new ShaderGraphCompatibilitySnapshot(
                backendKind,
                CandidateTypeNames,
                discoveredTypes,
                shaderGraphAssemblies,
                resolvedTypes,
                missingTypes,
                surface,
                notes
            );
        }

        private static ShaderGraphBackendKind DetermineBackendKind(
            IReadOnlyList<string> shaderGraphAssemblies,
            ShaderGraphApiSurface surface)
        {
            if (shaderGraphAssemblies.Count == 0)
            {
                return ShaderGraphBackendKind.Scaffold;
            }

            return surface.HasCoreMutationSurface
                ? ShaderGraphBackendKind.PackageReady
                : ShaderGraphBackendKind.PackageDetectedButIncomplete;
        }

        private static ShaderGraphApiSurface DescribeGraphSurface(Type graphType)
        {
            if (graphType == null)
            {
                return new ShaderGraphApiSurface(
                    "UnityEditor.ShaderGraph.GraphData",
                    string.Empty,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    false,
                    Array.Empty<string>(),
                    CandidateMethodNames.ToArray());
            }

            List<string> resolvedMethods;
            List<string> missingMethods;
            CaptureMethodSignatures(graphType, CandidateMethodNames, out resolvedMethods, out missingMethods);

            return new ShaderGraphApiSurface(
                graphType.FullName ?? graphType.Name,
                graphType.BaseType?.FullName ?? string.Empty,
                true,
                HasMethod(graphType, "AddGraphInput", 1) || HasMethod(graphType, "AddGraphInput", 2),
                HasMethod(graphType, "Connect", 2),
                HasMethod(graphType, "AddNode", 1),
                HasMethod(graphType, "ValidateGraph", 0),
                HasMethod(graphType, "ReplaceWith", 1),
                HasMethod(graphType, "GetNodes", 0),
                HasMethod(graphType, "GetNodeFromGuid", 1),
                HasMethod(graphType, "ContainsNodeGuid", 1),
                HasMethod(graphType, "RemoveNode", 1),
                HasMethod(graphType, "RemoveShaderProperty", 1),
                resolvedMethods,
                missingMethods);
        }

        private static void CaptureMethodSignatures(
            Type type,
            IReadOnlyList<string> candidateMethodNames,
            out List<string> resolvedSignatures,
            out List<string> missingMethodNames)
        {
            resolvedSignatures = new List<string>();
            missingMethodNames = new List<string>();

            foreach (string methodName in candidateMethodNames)
            {
                var matches = type
                    .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(method => string.Equals(method.Name, methodName, StringComparison.Ordinal))
                    .Select(FormatMethodSignature)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray();

                if (matches.Length == 0)
                {
                    missingMethodNames.Add(methodName);
                    continue;
                }

                resolvedSignatures.AddRange(matches);
            }
        }

        private static string FormatMethodSignature(MethodInfo method)
        {
            string parameters = string.Join(
                ", ",
                method.GetParameters().Select(parameter =>
                    $"{FormatTypeName(parameter.ParameterType)} {parameter.Name}"));
            return $"{FormatTypeName(method.ReturnType)} {method.Name}({parameters})";
        }

        private static string FormatTypeName(Type type)
        {
            if (type == null)
            {
                return "unknown";
            }

            if (!type.IsGenericType)
            {
                return type.FullName ?? type.Name;
            }

            string genericName = type.Name;
            int backtick = genericName.IndexOf('`');
            if (backtick >= 0)
            {
                genericName = genericName.Substring(0, backtick);
            }

            string args = string.Join(", ", type.GetGenericArguments().Select(FormatTypeName));
            return $"{(type.Namespace != null ? $"{type.Namespace}." : string.Empty)}{genericName}<{args}>";
        }

        private static bool IsShaderGraphAssembly(Assembly assembly)
        {
            string assemblyName = assembly.GetName().Name ?? string.Empty;
            if (CandidateAssemblyHints.Any(
                hint => assemblyName.IndexOf(hint, StringComparison.OrdinalIgnoreCase) >= 0))
            {
                return true;
            }

            return SafeGetTypes(assembly)
                .Any(type => type.Namespace != null && type.Namespace.StartsWith("UnityEditor.ShaderGraph", StringComparison.Ordinal));
        }

        private static bool IsInterestingShaderGraphType(Type type)
        {
            if (type == null || type.Namespace == null)
            {
                return false;
            }

            if (!type.Namespace.StartsWith("UnityEditor.ShaderGraph", StringComparison.Ordinal))
            {
                return false;
            }

            string name = type.Name;
            return name.IndexOf("Graph", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Node", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Property", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Slot", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Port", StringComparison.OrdinalIgnoreCase) >= 0 ||
                   name.IndexOf("Edge", StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static IEnumerable<Type> SafeGetTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null);
            }
            catch
            {
                return Array.Empty<Type>();
            }
        }

        private static Type ResolveType(string typeName, IEnumerable<Assembly> assemblies)
        {
            foreach (Assembly assembly in assemblies)
            {
                Type resolved = assembly.GetType(typeName, false);
                if (resolved != null)
                {
                    return resolved;
                }
            }

            return Type.GetType(typeName, false);
        }

        private static bool HasMethod(Type type, string methodName, int parameterCount)
        {
            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(method => string.Equals(method.Name, methodName, StringComparison.Ordinal) &&
                               method.GetParameters().Length == parameterCount);
        }
    }
}
