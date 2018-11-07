using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ComponentDependencyResolverTests
{
    class HostPolicyMock
    {
#if WINDOWS
        private const CharSet HostpolicyCharSet = CharSet.Unicode;
#else
        private const CharSet HostpolicyCharSet = CharSet.Ansi;
#endif

        [DllImport("hostpolicy", CharSet = HostpolicyCharSet)]
        private static extern int Set_corehost_resolve_component_dependencies_Values(
            int returnValue,
            string assemblyPaths,
            string nativeSearchPaths,
            string resourceSearchPaths);

        public static string DeleteExistingHostpolicy(string coreRoot)
        {
            string hostPolicyFileName = XPlatformUtils.GetStandardNativeLibraryFileName("hostpolicy");
            string destinationPath = Path.Combine(coreRoot, hostPolicyFileName);
            if (File.Exists(destinationPath))
            {
                File.Delete(destinationPath);
            }

            return destinationPath;
        }

        public static void Initialize(string testBasePath, string coreRoot)
        {
            string hostPolicyFileName = XPlatformUtils.GetStandardNativeLibraryFileName("hostpolicy");
            string destinationPath = DeleteExistingHostpolicy(coreRoot);

            File.Copy(
                Path.Combine(testBasePath, hostPolicyFileName),
                destinationPath);
        }

        public static IDisposable Mock_corehost_resolve_componet_dependencies(
            int returnValue,
            string assemblyPaths,
            string nativeSearchPaths,
            string resourceSearchPaths)
        {
            Set_corehost_resolve_component_dependencies_Values(
                returnValue,
                assemblyPaths,
                nativeSearchPaths,
                resourceSearchPaths);

            return new ResetMockValues_corehost_resolve_componet_dependencies();
        }

        private class ResetMockValues_corehost_resolve_componet_dependencies : IDisposable
        {
            public void Dispose()
            {
                Set_corehost_resolve_component_dependencies_Values(
                    -1,
                    string.Empty,
                    string.Empty,
                    string.Empty);
            }
        }
    }
}
