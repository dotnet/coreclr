// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System.Reflection
{
    public sealed partial class AssemblyName : ICloneable, IDeserializationCallback, ISerializable
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal extern void nInit(out RuntimeAssembly assembly, bool raiseResolveEvent);

        internal void nInit()
        {
            nInit(out RuntimeAssembly dummy, false);
        }

        // This call opens and closes the file, but does not add the
        // assembly to the domain.
        [MethodImpl(MethodImplOptions.InternalCall)]
        internal static extern AssemblyName nGetFileInformation(string s);

        [MethodImpl(MethodImplOptions.InternalCall)]
        private extern byte[] nGetPublicKeyToken();
    }
}
