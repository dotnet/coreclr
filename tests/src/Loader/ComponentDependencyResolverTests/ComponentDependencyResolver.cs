using System;
using System.Reflection;

namespace ComponentDependencyResolverTests
{
    /// <summary>
    /// Temporary until the actual public API gets propagated through CoreFX.
    /// </summary>
    public class ComponentDependencyResolver
    {
        private object _implementation;
        private Type _implementationType;
        private MethodInfo _resolveAssemblyPathInfo;
        private MethodInfo _resolveUnmanagedDllPathInfo;

        public ComponentDependencyResolver(string componentAssemblyPath)
        {
            _implementationType = typeof(object).Assembly.GetType("System.Runtime.Loader.ComponentDependencyResolver");
            _resolveAssemblyPathInfo = _implementationType.GetMethod("ResolveAssemblyPath");
            _resolveUnmanagedDllPathInfo = _implementationType.GetMethod("ResolveUnmanagedDllPath");

            try
            {
                _implementation = Activator.CreateInstance(_implementationType, componentAssemblyPath);
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public string ResolveAssemblyPath(AssemblyName assemblyName)
        {
            try
            {
                return (string)_resolveAssemblyPathInfo.Invoke(_implementation, new object[] { assemblyName });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }

        public string ResolveUnmanagedDllPath(string unmanagedDllName)
        {
            try
            {
                return (string)_resolveUnmanagedDllPathInfo.Invoke(_implementation, new object[] { unmanagedDllName });
            }
            catch (TargetInvocationException tie)
            {
                throw tie.InnerException;
            }
        }
    }
}
