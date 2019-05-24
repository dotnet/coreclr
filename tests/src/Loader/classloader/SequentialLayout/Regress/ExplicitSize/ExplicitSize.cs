// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// The struct has an objref and is of odd size.
// The GC requires that all valuetypes containing objrefs be sized to a multiple of sizeof(void*) )== 4).
// Since the size of this struct was 17 we were throwing a TypeLoadException.
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

unsafe class Program
{
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    struct GUID
    {
        private int align;
    }

    static int Main()
    {
        Guid initialGuid = Guid.NewGuid();
        GUID g = default;
        Test(initialGuid, &g);
        return Unsafe.As<GUID, Guid>(ref g) == initialGuid ? 100 : 101;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    static GUID GetGUID(ref Guid guid) => Unsafe.As<Guid, GUID>(ref guid);

    [MethodImpl(MethodImplOptions.NoInlining)]
    static unsafe void Test(Guid initialGuid, GUID* result)
    {
        Guid g = initialGuid;
        GUID guid = GetGUID(ref g);
        Unsafe.CopyBlock(result, &guid, (uint)sizeof(GUID));
    }
}
