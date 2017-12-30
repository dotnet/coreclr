// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

namespace System.IO
{
    /// <summary>Contains internal path helpers that are shared between many projects.</summary>
    internal static partial class PathInternal
    {
        internal const char DirectorySeparatorChar = '/';
        internal const char AltDirectorySeparatorChar = '/';
        internal const char VolumeSeparatorChar = '/';
        internal const char PathSeparator = ':';

        internal const string DirectorySeparatorCharAsString = "/";

        // There is only one invalid path character in Unix
        private const char InvalidPathChar = '\0';

        internal const string ParentDirectoryPrefix = @"../";

        internal static int GetRootLength(string path)
        {
            return GetRootLength(path.AsReadOnlySpan());
        }

        internal static int GetRootLength(ReadOnlySpan<char> path)
        {
            return path.Length > 0 && IsDirectorySeparator(path[0]) ? 1 : 0;
        }

        internal static bool IsDirectorySeparator(char c)
        {
            // The alternate directory separator char is the same as the directory separator,
            // so we only need to check one.
            Debug.Assert(DirectorySeparatorChar == AltDirectorySeparatorChar);
            return c == DirectorySeparatorChar;
        }

        /// <summary>
        /// Normalize separators in the given path. Compresses forward slash runs.
        /// </summary>
        internal unsafe static string NormalizeDirectorySeparatorsIfNecessary(ReadOnlySpan<char> path)
        {
            if (path.IsEmpty)
                return string.Empty;

            // Make a pass to see if we need to normalize so we can potentially skip allocating
            bool normalized = true;

            for (int i = 0; i < path.Length; i++)
            {
                if (IsDirectorySeparator(path[i])
                    && (i + 1 < path.Length && IsDirectorySeparator(path[i + 1])))
                {
                    normalized = false;
                    break;
                }
            }

            if (normalized)
                return null;

            fixed (char* f = &MemoryMarshal.GetReference(path))
            {
                return string.Create(path.Length, (Path: (IntPtr)f, PathLength: path.Length), (dst, state) =>
                {
                    int j = 0;
                    ReadOnlySpan<char> temp = new Span<char>((char*)state.Path, state.PathLength);

                    for (int i = 0; i < temp.Length; i++)
                    {
                        char current = temp[i];

                        // Skip if we have another separator following
                        if (IsDirectorySeparator(current)
                            && (i + 1 < temp.Length && IsDirectorySeparator(temp[i + 1])))
                            continue;

                        dst[j++] = current;
                    }
                });
            }
        }
        
        /// <summary>
        /// Returns true if the character is a directory or volume separator.
        /// </summary>
        /// <param name="ch">The character to test.</param>
        internal static bool IsDirectoryOrVolumeSeparator(char ch)
        {
            // The directory separator, volume separator, and the alternate directory
            // separator should be the same on Unix, so we only need to check one.
            Debug.Assert(DirectorySeparatorChar == AltDirectorySeparatorChar);
            Debug.Assert(DirectorySeparatorChar == VolumeSeparatorChar);
            return ch == DirectorySeparatorChar;
        }

        internal static bool IsPartiallyQualified(string path)
        {
            // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
            // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
            return !Path.IsPathRooted(path);
        }

        internal static bool IsPartiallyQualified(ReadOnlySpan<char> path)
        {
            // This is much simpler than Windows where paths can be rooted, but not fully qualified (such as Drive Relative)
            // As long as the path is rooted in Unix it doesn't use the current directory and therefore is fully qualified.
            return !Path.IsPathRooted(path);
        }

        internal static string TrimEndingDirectorySeparator(string path) =>
            path.Length > 1 && IsDirectorySeparator(path[path.Length - 1]) ? // exclude root "/"
            path.Substring(0, path.Length - 1) :
            path;

        /// <summary>
        /// Returns true if the path is effectively empty for the current OS.
        /// For unix, this is empty or null. For Windows, this is empty, null, or 
        /// just spaces ((char)32).
        /// </summary>
        internal static bool IsEffectivelyEmpty(string path)
        {
            return string.IsNullOrEmpty(path);
        }

        internal static bool IsEffectivelyEmpty(ReadOnlySpan<char> path)
        {
            return path.IsEmpty;
        }
    }
}
