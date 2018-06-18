using System;
using System.StubHelpers;

namespace Internal.Runtime.InteropServices.WindowsRuntime
{
    public static class EventArgsMarshalerSupport
    {
        public static IntPtr CreateNativeNCCEventArgsInstance(int action, object newItems, object oldItems, int newIndex, int oldIndex)
        {
            return EventArgsMarshaler.CreateNativeNCCEventArgsInstance(action, newItems, oldItems, newIndex, oldIndex);
        }

        public static IntPtr CreateNativePCEventArgsInstance(string name)
        {
            return EventArgsMarshaler.CreateNativePCEventArgsInstance(name);
        }
    }
}