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
        internal struct OrdinalIgnoreCaseCultureComparer
        {
            public readonly string _cultureNameUpper;
            public OrdinalIgnoreCaseCultureComparer(string cultureName)
            {
                _cultureNameUpper = cultureName.ToUpperInvariant();
            }

            public bool Equals(ref ReadOnlySpan<char> entryName)
            {
                if (_cultureNameUpper.Length != entryName.Length)
                    return false;

                for (int i = 0; i < _cultureNameUpper.Length; ++i)
                {
                    if (_cultureNameUpper[i] != char.ToUpperInvariant(entryName[i]))
                        return false;
                }
                return true;
            }
        }

        // Find satellite path using case insensitive culture name
        internal static unsafe string? FindCaseInsensitiveCultureName(string parentDirectory, string cultureName, string assembly)
        {
            int bufferSize = Interop.Sys.GetReadDirRBufferSize();

            byte[] dirBuffer = ArrayPool<byte>.Shared.Rent(bufferSize);
            IntPtr dirHandle = Interop.Sys.OpenDir(parentDirectory);

            string? result = null;

            try
            {
                if (dirHandle != IntPtr.Zero)
                {
                    try
                    {
                        fixed (byte* dirBufferPtr = dirBuffer)
                        {
                            Span<char> nameBuffer = stackalloc char[Interop.Sys.DirectoryEntry.NameBufferSize];
                            OrdinalIgnoreCaseCultureComparer cultureNameComparer = new OrdinalIgnoreCaseCultureComparer(cultureName);

                            Interop.Sys.DirectoryEntry dirent;
                            while (Interop.Sys.ReadDirR(dirHandle, dirBufferPtr, bufferSize, out dirent) == 0)
                            {
                                if (dirent.InodeType != Interop.Sys.NodeType.DT_DIR)
                                    continue;

                                ReadOnlySpan<char> entryName = dirent.GetName(nameBuffer);

                                if (!cultureNameComparer.Equals(ref entryName))
                                    continue;

                                if (result == null)
                                {
                                    // Convert to string because we will overwrite the backing buffer next loop
                                    result = entryName.ToString();
                                    // We do not return here because we do not want to debug/allow cases where
                                    // there are multiple directories which match case insensitive cultureName
                                    //
                                    // Do an exhaustive search
                                }
                                else
                                {
                                    // We found more than one directory with the same case insensitive name
                                    // return null to make this case predicatably fail.
                                    return null;
                                }
                            }
                        }
                    }
                    finally
                    {
                        Interop.Sys.CloseDir(dirHandle);
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(dirBuffer);
            }

            return result;
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
                return parentAlc.LoadFromAssemblyPath(assemblyPath);
            }
            else if (Path.IsCaseSensitive)
            {
                string? caseInsensitiveCultureName = FindCaseInsensitiveCultureName(parentDirectory, cultureName, assemblyName.Name);

                if (caseInsensitiveCultureName != null)
                {
                    assemblyPath = $"{parentDirectory}/{caseInsensitiveCultureName}/{assemblyName.Name}.dll";
                    if (Internal.IO.File.InternalExists(assemblyPath))
                        return parentAlc.LoadFromAssemblyPath(assemblyPath);
                }
            }
            return null;
        }
    }
}
