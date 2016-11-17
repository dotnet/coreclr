// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.IO
{
    /// <summary>Contains internal path helpers that are shared between many projects.</summary>
    internal static partial class PathInternal
    {
        private static bool? s_isCaseSensitive;

        /// <summary>Returns a comparison that can be used to compare file and directory names for equality.</summary>
        internal static StringComparison StringComparison
        {
            get
            {
                return IsCaseSensitive ?
                    StringComparison.Ordinal :
                    StringComparison.OrdinalIgnoreCase;
            }
        }

        /// <summary>Gets whether the system is case-sensitive.</summary>
        internal static bool IsCaseSensitive
        {
            get
            {
                // This must be lazily initialized as there are dependencies on PathInternal's static constructor
                // being fully initialized. (GetIsCaseSensitive() calls GetFullPath() which needs to use PathInternal)
                if (!s_isCaseSensitive.HasValue)
                    s_isCaseSensitive = GetIsCaseSensitive();

                return s_isCaseSensitive.Value;
            }
        }

        /// <summary>
        /// Determines whether the file system is case sensitive.
        /// </summary>
        /// <remarks>
        /// Ideally we'd use something like pathconf with _PC_CASE_SENSITIVE, but that is non-portable, 
        /// not supported on Windows or Linux, etc. For now, this function creates a tmp file with capital letters 
        /// and then tests for its existence with lower-case letters.  This could return invalid results in corner 
        /// cases where, for example, different file systems are mounted with differing sensitivities.
        /// </remarks>
        private static bool GetIsCaseSensitive()
        {
            try
            {
                string pathWithUpperCase = Path.Combine(Path.GetTempPath(), "CASESENSITIVETEST" + Guid.NewGuid().ToString("N"));
                using (new FileStream(pathWithUpperCase, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None, 0x1000, FileOptions.DeleteOnClose))
                {
                    string lowerCased = pathWithUpperCase.ToLowerInvariant();
                    return !File.Exists(lowerCased);
                }
            }
            catch (Exception exc)
            {
                // In case something goes terribly wrong, we don't want to fail just because
                // of a casing test, so we assume case-insensitive-but-preserving.
                Debug.Fail("Casing test failed: " + exc);
                return false;
            }
        }
    }
}
