// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public partial class AssemblyLoadContext
    {
        private Assembly? ResolveSatelliteAssembly(AssemblyName assemblyName)
        {
            string? cultureName = assemblyName.CultureName;

            if (cultureName == null || cultureName.Length == 0)
                return null;

            if (assemblyName.Name == null)
                return null;

            AssemblyName parentAssemblyName = new AssemblyName(assemblyName.Name);

            Assembly? parentAssembly = LoadFromAssemblyName(parentAssemblyName);

            if (parentAssembly == null)
                return null;

            AssemblyLoadContext? parentAlc = GetLoadContext(parentAssembly);

            if (parentAlc == null)
                return null;

            string parentDirectory = Path.GetDirectoryName(parentAssembly.Location)!;

            string assemblyPath = Path.Combine(parentDirectory, dir, $"{assemblyName.Name}.dll");
            if (Internal.IO.File.InternalExists(assemblyPath))
            {
                return parentAlc.LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }
    }
}
