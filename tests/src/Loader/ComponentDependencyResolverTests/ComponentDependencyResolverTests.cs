using System;
using System.IO;
using System.Reflection;
using Xunit;

namespace ComponentDependencyResolverTests
{
    class ComponentDependencyResolverTests : TestBase
    {
        string _componentDirectory;
        string _componentAssemblyPath;

        protected override void Initialize()
        {
            HostPolicyMock.Initialize(TestBasePath, CoreRoot);
            _componentDirectory = Path.Combine(TestBasePath, $"TestComponent_{Guid.NewGuid().ToString().Substring(0, 8)}");
            Directory.CreateDirectory(_componentDirectory);
            _componentAssemblyPath = CreateMockAssembly("TestComponent.dll");
        }

        protected override void Cleanup()
        {
            if (Directory.Exists(_componentDirectory))
            {
                Directory.Delete(_componentDirectory, recursive: true);
            }
        }

        public void TestComponentLoadFailure()
        {
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                134,
                "",
                "",
                ""))
            {
                string message = Assert.Throws<InvalidOperationException>(() =>
                {
                    ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                        Path.Combine(TestBasePath, _componentAssemblyPath));
                }).Message;

                Assert.Contains("134", message);
            }
        }

        public void TestAssembly()
        {
            string assemblyDependencyPath = CreateMockAssembly("AssemblyDependency.dll");
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                assemblyDependencyPath,
                "",
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    assemblyDependencyPath,
                    resolver.ResolveAssemblyPath(new AssemblyName("AssemblyDependency")));
            }
        }

        public void TestAssemblyWithNoRecord()
        {
            // If the reqest is for assembly which is not listed in .deps.json
            // the resolver should return null.
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                "",
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Null(resolver.ResolveAssemblyPath(new AssemblyName("AssemblyWithNoRecord")));
            }
        }

        public void TestAssemblyWithMissingFile()
        {
            // Even if the .deps.json can resolve the request, if the file is not present
            // the resolution should still return null.
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                Path.Combine(_componentDirectory, "NonExistingAssembly.dll"),
                "",
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Null(resolver.ResolveAssemblyPath(new AssemblyName("NonExistingAssembly")));
            }
        }

        public void TestSingleResource()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                "",
                _componentDirectory))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyPath(new AssemblyName("TestComponent.resources, Culture=en")));
            }
        }

        public void TestMutipleResourcesWithSameBasePath()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            string csResourcePath = CreateMockAssembly($"cs{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                "",
                _componentDirectory))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyPath(new AssemblyName("TestComponent.resources, Culture=en")));
                Assert.Equal(
                    csResourcePath,
                    resolver.ResolveAssemblyPath(new AssemblyName("TestComponent.resources, Culture=cs")));
            }
        }

        public void TestMutipleResourcesWithDifferentBasePath()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            string frResourcePath = CreateMockAssembly($"SubComponent{Path.DirectorySeparatorChar}fr{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                "",
                $"{_componentDirectory}{Path.PathSeparator}{Path.GetDirectoryName(Path.GetDirectoryName(frResourcePath))}"))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyPath(new AssemblyName("TestComponent.resources, Culture=en")));
                Assert.Equal(
                    frResourcePath,
                    resolver.ResolveAssemblyPath(new AssemblyName("TestComponent.resources, Culture=fr")));
            }
        }

        public void TestAssemblyWithNeutralCulture()
        {
            string neutralAssemblyPath = CreateMockAssembly("NeutralAssembly.dll");
            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                neutralAssemblyPath,
                "",
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    neutralAssemblyPath,
                    resolver.ResolveAssemblyPath(new AssemblyName("NeutralAssembly, Culture=neutral")));
            }
        }

        public void TestSingleNativeDependency()
        {
            string nativeLibraryPath = CreateMockStandardNativeLibrary("native", "Single");

            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                Path.GetDirectoryName(nativeLibraryPath),
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    nativeLibraryPath,
                    resolver.ResolveUnmanagedDllPath("Single"));
            }
        }

        public void TestMultipleNativeDependencies()
        {
            string oneNativeLibraryPath = CreateMockStandardNativeLibrary($"native{Path.DirectorySeparatorChar}one", "One");
            string twoNativeLibraryPath = CreateMockStandardNativeLibrary($"native{Path.DirectorySeparatorChar}two", "Two");

            using (HostPolicyMock.Mock_corehost_resolve_componet_dependencies(
                0,
                "",
                $"{Path.GetDirectoryName(oneNativeLibraryPath)}{Path.PathSeparator}{Path.GetDirectoryName(twoNativeLibraryPath)}",
                ""))
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    oneNativeLibraryPath,
                    resolver.ResolveUnmanagedDllPath("One"));
                Assert.Equal(
                    twoNativeLibraryPath,
                    resolver.ResolveUnmanagedDllPath("Two"));
            }
        }

        private string CreateMockAssembly(string relativePath)
        {
            string fullPath = Path.Combine(_componentDirectory, relativePath);
            if (!File.Exists(fullPath))
            {
                string directory = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                File.WriteAllText(fullPath, "Mock assembly");
            }

            return fullPath;
        }

        private string CreateMockStandardNativeLibrary(string relativePath, string simpleName)
        {
            return CreateMockAssembly(
                relativePath + Path.DirectorySeparatorChar + XPlatformUtils.GetStandardNativeLibraryFileName(simpleName));
        }

        public static int Main()
        {
            return TestBase.RunTests(
                // It's important that the invalid hosting test runs first as it relies on the ability
                // to delete (if it's there) the hostpolicy.dll. All other tests will end up loading the dll
                // and thus locking it.
                typeof(InvalidHostingTest),
                typeof(ComponentDependencyResolverTests));
        }
    }
}
