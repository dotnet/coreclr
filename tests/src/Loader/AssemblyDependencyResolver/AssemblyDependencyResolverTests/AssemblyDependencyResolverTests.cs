// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
using System;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using TestLibrary;
using Xunit;

using Assert = Xunit.Assert;

namespace AssemblyDependencyResolverTests
{
    class AssemblyDependencyResolverTests : TestBase
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
            const string errorMessageFirstLine = "First line: failure";
            const string errorMessageSecondLine = "Second line: value";

            using (HostPolicyMock.MockValues_corehost_set_error_writer errorWriterMock = 
                HostPolicyMock.Mock_corehost_set_error_writer())
            {
                using (HostPolicyMock.MockValues_corehost_resolve_component_dependencies resolverMock = 
                    HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                        134,
                        "",
                        "",
                        ""))
                {
                    // When the resolver is called, emulate error behavior
                    // which is to write to the error writer some error message.
                    resolverMock.Callback = (string componentAssemblyPath) =>
                    {
                        Assert.NotNull(errorWriterMock.LastSetErrorWriter);
                        errorWriterMock.LastSetErrorWriter(errorMessageFirstLine);
                        errorWriterMock.LastSetErrorWriter(errorMessageSecondLine);
                    };

                    string message = Assert.Throws<InvalidOperationException>(() =>
                    {
                        AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                            Path.Combine(TestBasePath, _componentAssemblyPath));
                    }).Message;

                    Assert.Contains("134", message);
                    Assert.Contains(
                        errorMessageFirstLine + Environment.NewLine + errorMessageSecondLine,
                        message);

                    // After everything is done, the error writer should be reset.
                    Assert.Null(errorWriterMock.LastSetErrorWriter);
                }
            }
        }

        public void TestComponentLoadFailureWithPreviousErrorWriter()
        {
            IntPtr previousWriter = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(
                (HostPolicyMock.ErrorWriterDelegate)((string _) => { Assert.True(false, "Should never get here"); }));

            using (HostPolicyMock.MockValues_corehost_set_error_writer errorWriterMock =
                HostPolicyMock.Mock_corehost_set_error_writer(previousWriter))
            {
                using (HostPolicyMock.MockValues_corehost_resolve_component_dependencies resolverMock =
                    HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                        134,
                        "",
                        "",
                        ""))
                {
                    Assert.Throws<InvalidOperationException>(() =>
                    {
                        AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                            Path.Combine(TestBasePath, _componentAssemblyPath));
                    });

                    // After everything is done, the error writer should be reset to the original value.
                    Assert.Equal(previousWriter, errorWriterMock.LastSetErrorWriterPtr);
                }
            }
        }

        public void TestAssembly()
        {
            string assemblyDependencyPath = CreateMockAssembly("AssemblyDependency.dll");

            IntPtr previousWriter = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(
                (HostPolicyMock.ErrorWriterDelegate)((string _) => { Assert.True(false, "Should never get here"); }));

            using (HostPolicyMock.MockValues_corehost_set_error_writer errorWriterMock =
                HostPolicyMock.Mock_corehost_set_error_writer(previousWriter))
            {
                using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                    0,
                    assemblyDependencyPath,
                    "",
                    ""))
                {
                    AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                        Path.Combine(TestBasePath, _componentAssemblyPath));

                    Assert.Equal(
                        assemblyDependencyPath,
                        resolver.ResolveAssemblyToPath(new AssemblyName("AssemblyDependency")));

                    // After everything is done, the error writer should be reset to the original value.
                    Assert.Equal(previousWriter, errorWriterMock.LastSetErrorWriterPtr);
                }
            }
        }

        public void TestAssemblyWithCaseDifferent()
        {
            // Testing case sensitive file name resolution
            // Host policy returns 2 file paths with the casing changed,
            // AssemblyDependencyResolver should not throw since the first path exists in the file system
            string assemblyDependencyPath = CreateMockAssembly("TestAssemblyWithCaseDifferent.dll");
            string nameWOExtension = Path.GetFileNameWithoutExtension(assemblyDependencyPath);
            string nameWOExtensionCaseChanged = (Char.IsUpper(nameWOExtension[0]) ? nameWOExtension[0].ToString().ToLower() : nameWOExtension[0].ToString().ToUpper()) + nameWOExtension.Substring(1);
            string changeFile = Path.Combine(Path.GetDirectoryName(assemblyDependencyPath), (nameWOExtensionCaseChanged + Path.GetExtension(assemblyDependencyPath)));

            IntPtr previousWriter = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(
                (HostPolicyMock.ErrorWriterDelegate)((string _) => { Assert.True(false, "Should never get here"); }));

            using (HostPolicyMock.MockValues_corehost_set_error_writer errorWriterMock =
                HostPolicyMock.Mock_corehost_set_error_writer(previousWriter))
            {
                using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                    0,
                    $"{assemblyDependencyPath}{Path.PathSeparator}{changeFile}",
                    "",
                    ""))
                {
                    AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(changeFile);

                    string asmResolveName = resolver.ResolveAssemblyToPath(new AssemblyName(nameWOExtensionCaseChanged));

                    Assert.Equal(
                        changeFile, asmResolveName, StringComparer.InvariantCultureIgnoreCase
                        );

                    // After everything is done, the error writer should be reset to the original value.
                    Assert.Equal(previousWriter, errorWriterMock.LastSetErrorWriterPtr);
                }
            }
        }

        public void TestAssemblyWithCaseReversed()
        {            
            // Testing case sensitive file name resolution
            // Host policy returns 2 file paths with the casing changed and names swapped.
            // AssemblyDependencyResolver should not throw but has different returned values,
            // Based on case sensitive nature of the file system since AssemblyDependencyResolver checks if file exists 
            // Case insensitive file systems: a valid path is returned
            // Case sensitive file systems: null (since the first path does not exist in the system)
            string assemblyDependencyPath = CreateMockAssembly("TestAssemblyWithCaseReversed.dll");
            string nameWOExtension = Path.GetFileNameWithoutExtension(assemblyDependencyPath);
            string nameWOExtensionCaseChanged = (Char.IsUpper(nameWOExtension[0]) ? nameWOExtension[0].ToString().ToLower() : nameWOExtension[0].ToString().ToUpper()) + nameWOExtension.Substring(1);
            string changeFile = Path.Combine(Path.GetDirectoryName(assemblyDependencyPath), (nameWOExtensionCaseChanged + Path.GetExtension(assemblyDependencyPath)));


            IntPtr previousWriter = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(
                (HostPolicyMock.ErrorWriterDelegate)((string _) => { Assert.True(false, "Should never get here"); }));

            using (HostPolicyMock.MockValues_corehost_set_error_writer errorWriterMock =
                HostPolicyMock.Mock_corehost_set_error_writer(previousWriter))
            {
                using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                    0,
                    $"{changeFile}{Path.PathSeparator}{assemblyDependencyPath}",
                    "",
                    ""))
                {
                    AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(changeFile);

                    string asmResolveName = resolver.ResolveAssemblyToPath(new AssemblyName(nameWOExtensionCaseChanged));

                    // Case sensitive systems return null (see notes above)
                    // We don't check the OS or the file system here since AssemblyDependencyResolver itself stays away from OS specific checks
                    // In path resolutions
                    if(asmResolveName != null)
                    {
                        Assert.Equal(
                            assemblyDependencyPath, asmResolveName, StringComparer.InvariantCultureIgnoreCase
                            );
                    }

                    // After everything is done, the error writer should be reset to the original value.
                    Assert.Equal(previousWriter, errorWriterMock.LastSetErrorWriterPtr);
                }
            }
        }

        public void TestAssemblyWithNoRecord()
        {
            // If the reqest is for assembly which is not listed in .deps.json
            // the resolver should return null.
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                "",
                ""))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Null(resolver.ResolveAssemblyToPath(new AssemblyName("AssemblyWithNoRecord")));
            }
        }

        public void TestAssemblyWithMissingFile()
        {
            // Even if the .deps.json can resolve the request, if the file is not present
            // the resolution should still return null.
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                Path.Combine(_componentDirectory, "NonExistingAssembly.dll"),
                "",
                ""))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Null(resolver.ResolveAssemblyToPath(new AssemblyName("NonExistingAssembly")));
            }
        }

        public void TestSingleResource()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                "",
                _componentDirectory))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("TestComponent.resources, Culture=en")));
            }
        }

        public void TestMutipleResourcesWithSameBasePath()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            string csResourcePath = CreateMockAssembly($"cs{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                "",
                _componentDirectory))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("TestComponent.resources, Culture=en")));
                Assert.Equal(
                    csResourcePath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("TestComponent.resources, Culture=cs")));
            }
        }

        public void TestMutipleResourcesWithDifferentBasePath()
        {
            string enResourcePath = CreateMockAssembly($"en{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            string frResourcePath = CreateMockAssembly($"SubComponent{Path.DirectorySeparatorChar}fr{Path.DirectorySeparatorChar}TestComponent.resources.dll");
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                "",
                $"{_componentDirectory}{Path.PathSeparator}{Path.GetDirectoryName(Path.GetDirectoryName(frResourcePath))}"))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    enResourcePath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("TestComponent.resources, Culture=en")));
                Assert.Equal(
                    frResourcePath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("TestComponent.resources, Culture=fr")));
            }
        }

        public void TestAssemblyWithNeutralCulture()
        {
            string neutralAssemblyPath = CreateMockAssembly("NeutralAssembly.dll");
            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                neutralAssemblyPath,
                "",
                ""))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    neutralAssemblyPath,
                    resolver.ResolveAssemblyToPath(new AssemblyName("NeutralAssembly, Culture=neutral")));
            }
        }

        public void TestSingleNativeDependency()
        {
            string nativeLibraryPath = CreateMockStandardNativeLibrary("native", "Single");

            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                Path.GetDirectoryName(nativeLibraryPath),
                ""))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    nativeLibraryPath,
                    resolver.ResolveUnmanagedDllToPath("Single"));
            }
        }

        public void TestMultipleNativeDependencies()
        {
            string oneNativeLibraryPath = CreateMockStandardNativeLibrary($"native{Path.DirectorySeparatorChar}one", "One");
            string twoNativeLibraryPath = CreateMockStandardNativeLibrary($"native{Path.DirectorySeparatorChar}two", "Two");

            using (HostPolicyMock.Mock_corehost_resolve_component_dependencies(
                0,
                "",
                $"{Path.GetDirectoryName(oneNativeLibraryPath)}{Path.PathSeparator}{Path.GetDirectoryName(twoNativeLibraryPath)}",
                ""))
            {
                AssemblyDependencyResolver resolver = new AssemblyDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));

                Assert.Equal(
                    oneNativeLibraryPath,
                    resolver.ResolveUnmanagedDllToPath("One"));
                Assert.Equal(
                    twoNativeLibraryPath,
                    resolver.ResolveUnmanagedDllToPath("Two"));
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
                typeof(AssemblyDependencyResolverTests),
                typeof(NativeDependencyTests));
        }
    }
}
