// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

class WrappedString
{
    public WrappedString(string str)
    {
        _str = str;
    }

    internal string _str;
}

class WrappedStringCustomMarshaler : ICustomMarshaler
{
    public void CleanUpManagedData(object ManagedObj) { }
    public void CleanUpNativeData(IntPtr pNativeData) { Marshal.ZeroFreeCoTaskMemAnsi(pNativeData); }

    public int GetNativeDataSize() => IntPtr.Size;

    public IntPtr MarshalManagedToNative(object ManagedObj) => Marshal.StringToCoTaskMemAnsi(((WrappedString)ManagedObj)._str);
    public object MarshalNativeToManaged(IntPtr pNativeData) => new WrappedString(Marshal.PtrToStringAnsi(pNativeData));

    public static ICustomMarshaler GetInstance(string cookie) => new WrappedStringCustomMarshaler();
}

#if CUSTOMMARSHALERS2
namespace CustomMarshalers2
#else
namespace CustomMarshalers
#endif
{
    public class CustomMarshalerTest
    {
        [DllImport("CustomMarshalerNative", CharSet = CharSet.Ansi)]
        private static extern int NativeParseInt([MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(WrappedStringCustomMarshaler))] WrappedString str);

        public int ParseInt(string str)
        {
            return NativeParseInt(new WrappedString(str));
        }
    }
}

