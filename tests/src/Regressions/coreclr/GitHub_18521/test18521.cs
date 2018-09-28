using System;
using System.Runtime.InteropServices;
using System.Text;

namespace MarshalTest
{
    [StructLayout(LayoutKind.Sequential)]
    unsafe struct MyBufferStruct
    {
        public short number;
        public fixed byte number2[2];
        public short number3;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    struct MyParentStruct
    {
        public MyBufferStruct myBufferStruct;
        public string string1; // commenting this makes it work
        public IntPtr intptr1;
    }

    class MarshalExample
    {
        static unsafe int Main()
        {
            var str = default(MyParentStruct);

            // Assign values to the bytes.
            byte* ptr = (byte*)&str.myBufferStruct;
            for (int i = 0; i < sizeof(MyBufferStruct); i++)
                ptr[i] = (byte)(0x11 * (i + 1));

            MyBufferStruct* original = (MyBufferStruct*)ptr;
            
            // Marshal the parent struct.
            var parentStructIntPtr = Marshal.AllocHGlobal(Marshal.SizeOf<MyParentStruct>());
            Marshal.StructureToPtr(str, parentStructIntPtr, false);
            try
            {
                MyBufferStruct* bufferStructPtr = (MyBufferStruct*)parentStructIntPtr.ToPointer();
                if (original->number2[1] != original->number2[1])
                    return 101;
            }
            finally
            {
                Marshal.DestroyStructure<MyParentStruct>(parentStructIntPtr);
                Marshal.FreeHGlobal(parentStructIntPtr);
            }
            return 100;
        }
    }
}
