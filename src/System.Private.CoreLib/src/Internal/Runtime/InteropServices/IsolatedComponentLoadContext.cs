// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Reflection;
using System.Runtime.Loader;

using Console = Internal.Console;

namespace Internal.Runtime.InteropServices
{
    /// <summary>
    /// An <see cref="IsolatedComponentLoadContext" /> is an AssemblyLoadContext that can be used to isolate components such as COM components
    /// or IJW components loaded from native. It provides a load context that uses an <see cref="AssemblyDependencyResolver" /> to resolve the component's
    /// dependencies within the ALC and not pollute the default ALC.
    ///</summary>
    internal sealed class IsolatedComponentLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver? _resolver;
        private readonly string _path;
        private readonly AssemblyName _assemblyName;

        public IsolatedComponentLoadContext(string componentAssemblyPath) : base($"IsolatedComponentLoadContext({componentAssemblyPath})")
        {
            Console.WriteLine("IsolatedComponentLoadContext");

            _path = componentAssemblyPath;
            _assemblyName = GetAssemblyName(componentAssemblyPath);

            Console.WriteLine(_assemblyName.ToString());
            try
            {
                _resolver = new AssemblyDependencyResolver(componentAssemblyPath);
            }
            catch
            {
                LoadFromAssemblyPath(componentAssemblyPath);
            }
        }

        protected override Assembly? Load(AssemblyName assemblyName)
        {
            Console.WriteLine("IsolatedComponentLoadContext.Load");
            Console.WriteLine(assemblyName.ToString());
            string? assemblyPath = _resolver?.ResolveAssemblyToPath(assemblyName);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            if (_assemblyName.Name == assemblyName.Name)
            {
                return LoadFromAssemblyPath(_path);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _resolver?.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
