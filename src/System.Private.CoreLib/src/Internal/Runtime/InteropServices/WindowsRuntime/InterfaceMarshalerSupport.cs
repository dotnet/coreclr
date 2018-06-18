using System;
using System.StubHelpers;

namespace Internal.Runtime.InteropServices.WindowsRuntime
{
    public static class InterfaceMarshalerSupport
    {
        public static object ConvertToManagedWithoutUnboxing(IntPtr pNative)
        {
            return InterfaceMarshaler.ConvertToManagedWithoutUnboxing(pNative);
        }
    }
}