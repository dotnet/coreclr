// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Win32.SafeHandles;
using System.Runtime.InteropServices;

namespace System.IO
{
    public partial class FileStream : Stream
    {
        private unsafe SafeFileHandle OpenHandle(FileMode mode, FileShare share, FileOptions options)
        {
            return CreateFile2OpenHandle(mode, share, options);
        }
    }
}
