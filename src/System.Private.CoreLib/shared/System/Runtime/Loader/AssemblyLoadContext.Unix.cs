// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#nullable enable
using System.Buffers;
using System.IO;
using System.Reflection;

namespace System.Runtime.Loader
{
    public partial class AssemblyLoadContext
    {
        // Find satellite path using case insensitive culture name
        internal static unsafe string? FindCaseInsensitiveSatellitePath(string parentDirectory, string cultureName, string assembly)
        {
            int bufferSize = Interop.Sys.GetReadDirRBufferSize();
            byte[]? dirBuffer = null;
            try
            {
                dirBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);

                fixed (byte* dirBufferPtr = dirBuffer)
                {
                    IntPtr dirHandle = Interop.Sys.OpenDir(parentDirectory);
                    if (dirHandle != IntPtr.Zero)
                    {
                        try
                        {
                            Interop.Sys.DirectoryEntry dirent;
                            while (Interop.Sys.ReadDirR(dirHandle, dirBufferPtr, bufferSize, out dirent) == 0)
                            {
                                if (dirent.InodeType != Interop.Sys.NodeType.DT_DIR)
                                    continue;

                                Span<char> nameBuffer = stackalloc char[Interop.Sys.DirectoryEntry.NameBufferSize];
                                ReadOnlySpan<char> entryName = dirent.GetName(nameBuffer);

                                if (cultureName.Length != entryName.Length)
                                    continue;

                                string entryNameString = entryName.ToString();

                                if (!cultureName.Equals(entryNameString, StringComparison.InvariantCultureIgnoreCase))
                                    continue;

                                string assemblyPath = $"{parentDirectory}/{entryNameString}/{assembly}.dll";

                                if (Internal.IO.File.InternalExists(assemblyPath))
                                    return assemblyPath;
                            }
                        }
                        finally
                        {
                            if (dirHandle != IntPtr.Zero)
                                Interop.Sys.CloseDir(dirHandle);
                        }
                    }
                }
            }
            finally
            {
                if (dirBuffer != null)
                    ArrayPool<byte>.Shared.Return(dirBuffer);
            }
            return null;
        }

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

            string assemblyPath = $"{parentDirectory}/{cultureName}/{assemblyName.Name}.dll";
            if (Internal.IO.File.InternalExists(assemblyPath))
            {
                Assembly satelliteAssembly = parentAlc.LoadFromAssemblyPath(assemblyPath);

                if (satelliteAssembly != null)
                    return satelliteAssembly;
            }
            else if (Path.IsCaseSensitive)
            {
                string? caseInsensitiveAssemblyPath = FindCaseInsensitiveSatellitePath(parentDirectory, cultureName, assemblyName.Name);

                if (caseInsensitiveAssemblyPath != null)
                {
                    Assembly satelliteAssembly = parentAlc.LoadFromAssemblyPath(caseInsensitiveAssemblyPath);

                    return satelliteAssembly;
                }
            }
            return null;
        }
    }
}
