// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

class CustomLoadContext : AssemblyLoadContext
{
    private string unmanagedDirectory;

    internal CustomLoadContext(string unmanagedDirectory)
    {
        this.unmanagedDirectory = unmanagedDirectory;
    }

    protected override Assembly Load(AssemblyName assemblyName)
    {
        string assemblyPath = Path.Combine(".", assemblyName.Name) + ".dll";
        if (File.Exists(assemblyPath))
        {
            return LoadFromAssemblyPath(assemblyPath);
        }

        return Default.LoadFromAssemblyName(assemblyName);
    }

    protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
    {
        string unmanagedDllPath = Directory.EnumerateFiles(
            unmanagedDirectory,
            $"{unmanagedDllName}.*").Concat(
                Directory.EnumerateFiles(
                    unmanagedDirectory,
                    $"lib{unmanagedDllName}.*"))
            .FirstOrDefault();

        if (unmanagedDllPath != null)
        {
            return this.LoadUnmanagedDllFromPath(unmanagedDllPath);
        }

        return base.LoadUnmanagedDll(unmanagedDllName);
    }
}
