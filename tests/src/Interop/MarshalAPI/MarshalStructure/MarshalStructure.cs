using System;
using System.Runtime.InteropServices;

class Program
{
    static int Main(string[] args)
    {
        MarshalLongerByValArrayInAStruct();
        return 100;
    }

    static void VerifyMarshalLongerByValArrayInAStruct()
    {
        var structure1 = new StructWithByValArray()
        {
            array = new StructWithIntField[]
            {
                new StructWithIntField { value = 1 },
                new StructWithIntField { value = 2 },
                new StructWithIntField { value = 3 },
                new StructWithIntField { value = 4 },
                new StructWithIntField { value = 5 }
            }
        };
        int size = Marshal.SizeOf(structure1);
        IntPtr memory = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure1, memory, false);
        }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }

        // underflow
        var structure2 = new StructWithByValArray()
        {
            array = new StructWithIntField[]
         {
                new StructWithIntField { value = 1 },
                new StructWithIntField { value = 2 },
                new StructWithIntField { value = 3 },
                new StructWithIntField { value = 4 }
         }
        };

        size = Marshal.SizeOf(structure2);
        memory = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure2, memory, false);
        }
        catch (ArgumentException){ }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }

        // overflow
        var structure3 = new StructWithByValArray()
        {
            array = new StructWithIntField[]
         {
                new StructWithIntField { value = 1 },
                new StructWithIntField { value = 2 },
                new StructWithIntField { value = 3 },
                new StructWithIntField { value = 4 },
                new StructWithIntField { value = 5 },
                new StructWithIntField { value = 6 }
         }
        };

        size = Marshal.SizeOf(structure3);
        memory = Marshal.AllocHGlobal(size);
        try
        {
            Marshal.StructureToPtr(structure3, memory, false);
        }
        catch (ArgumentException) { }
        finally
        {
            Marshal.FreeHGlobal(memory);
        }
    }

    public struct StructWithIntField
    {
        public int value;
    }

    public struct StructWithByValArray
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        public StructWithIntField[] array;
    }
}