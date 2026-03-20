using ShaderGraphMcp.Editor.Compatibility;
using ShaderGraphMcp.Editor.Models;

namespace ShaderGraphMcp.Editor.Adapters
{
    internal static class ShaderGraphBackendFactory
    {
        public static IShaderGraphBackend CreateDefault()
        {
            ShaderGraphCompatibilitySnapshot compatibilitySnapshot = ShaderGraphPackageCompatibility.Capture();

            return compatibilitySnapshot.BackendKind == ShaderGraphBackendKind.PackageReady
                ? new ShaderGraphPackageBackend(compatibilitySnapshot)
                : new ShaderGraphScaffoldBackend();
        }

        public static IShaderGraphBackend CreatePackageBackend(ShaderGraphCompatibilitySnapshot compatibilitySnapshot)
        {
            return new ShaderGraphPackageBackend(compatibilitySnapshot);
        }
    }
}
