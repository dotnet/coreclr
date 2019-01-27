// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System.StubHelpers
{
    internal static class MngdNativeArrayMarshaler
    {
        // Needs to match exactly with MngdNativeArrayMarshaler in ilmarshalers.h
        internal struct MarshalerState
        {
            IntPtr m_pElementMT;
            IntPtr m_Array;
            int m_NativeDataValid;
            int m_BestFitMap;
            int m_ThrowOnUnmappableChar;
            short m_vt;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void CreateMarshaler(IntPtr pMarshalState, IntPtr pMT, int dwFlags);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ConvertSpaceToNative(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ConvertContentsToNative(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ConvertSpaceToManaged(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome,
                                                          int cElements);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ConvertContentsToManaged(IntPtr pMarshalState, ref object pManagedHome, IntPtr pNativeHome);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ClearNative(IntPtr pMarshalState, IntPtr pNativeHome, int cElements);

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ClearNativeContents(IntPtr pMarshalState, IntPtr pNativeHome, int cElements);
    }
}