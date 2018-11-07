using System;
using System.IO;
using Xunit;

namespace ComponentDependencyResolverTests
{
    class InvalidHostingTest : TestBase
    {
        private string _componentDirectory;
        private string _componentAssemblyPath;
        private string _officialHostPolicyPath;
        private string _localHostPolicyPath;
        private string _renamedHostPolicyPath;

        protected override void Initialize()
        {
            // Make sure there's no hostpolicy available
            _officialHostPolicyPath = HostPolicyMock.DeleteExistingHostpolicy(CoreRoot);
            string hostPolicyFileName = XPlatformUtils.GetStandardNativeLibraryFileName("hostpolicy");
            _localHostPolicyPath = Path.Combine(TestBasePath, hostPolicyFileName);
            _renamedHostPolicyPath = Path.Combine(TestBasePath, hostPolicyFileName + "_renamed");
            File.Move(_localHostPolicyPath, _renamedHostPolicyPath);

            _componentDirectory = Path.Combine(TestBasePath, $"InvalidHostingComponent_{Guid.NewGuid().ToString().Substring(0, 8)}");
            Directory.CreateDirectory(_componentDirectory);
            _componentAssemblyPath = Path.Combine(_componentDirectory, "InvalidHostingComponent.dll");
            File.WriteAllText(_componentAssemblyPath, "Mock assembly");
        }

        protected override void Cleanup()
        {
            if (File.Exists(_renamedHostPolicyPath))
            {
                File.Move(_renamedHostPolicyPath, _localHostPolicyPath);
            }
        }

        public void TestMissingHostPolicy()
        {
            object innerException = Assert.Throws<InvalidOperationException>(() =>
            {
                ComponentDependencyResolver resolver = new ComponentDependencyResolver(
                    Path.Combine(TestBasePath, _componentAssemblyPath));
            }).InnerException;

            Assert.IsType<DllNotFoundException>(innerException);
        }

        // Note: No good way to test the missing entry point case where hostpolicy.dll
        // exists, but it doesn't have the right entry points.
        // Loading a "wrong" hostpolicy.dll into the process is non-revertable operation
        // so we would not be able to run other tests along side this one.
        // Having a standalone .exe just for that one test is not worth it.
    }
}
