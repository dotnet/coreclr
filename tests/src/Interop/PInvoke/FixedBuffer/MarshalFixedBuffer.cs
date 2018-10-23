using System;
using System.Runtime.InteropServices;
using System.Security;
using TestLibrary;

public unsafe class Managed
{
    private const int BufferSize = 10;

    struct BlittableFixedBuffer
    {
        public fixed int elements[BufferSize];
    }

    struct NonBlittableFixedBuffer
    {
        public fixed bool elements[BufferSize];
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    struct ElementAfterNonBlittableFixedBuffer
    {
        public fixed char fixedString[BufferSize];
        public int testValue;
    }

    [DllImport("MarshalFixedBuffer")]
    private static extern int SumBlittableFixedBuffer(BlittableFixedBuffer buffer);

    [DllImport("MarshalFixedBuffer")]
    private static extern bool AggregateNonBlittableFixedBuffer(NonBlittableFixedBuffer buffer);

    [DllImport("MarshalFixedBuffer")]
    private static extern bool VerifyElementAfterNonBlittableFixedBuffer(ElementAfterNonBlittableFixedBuffer buffer);

    private static ElementAfterNonBlittableFixedBuffer InitElementAfterNonBlittableFixedBuffer()
    {
        return new ElementAfterNonBlittableFixedBuffer
        {
            testValue = 42
        };
    }

    private static void TestBlittableFixedBuffer()
    {
        Console.WriteLine("Start blittable fixed buffer test");

        var buffer = new BlittableFixedBuffer();

        for (int i = 0; i < BufferSize; i++)
        {
            buffer.elements[i] = i;
        }

        Assert.AreEqual(45, SumBlittableFixedBuffer(buffer));
    }

    private static void TestNonBlittableFixedBuffer()
    {
        Console.WriteLine("Start non-blittable fixed buffer test");

        var buffer = new NonBlittableFixedBuffer();

        for (int i = 0; i < BufferSize; i++)
        {
            buffer.elements[i] = true;
        }

        Assert.IsTrue(AggregateNonBlittableFixedBuffer(buffer));

        buffer.elements[BufferSize - 1] = false;

        Assert.IsFalse(AggregateNonBlittableFixedBuffer(buffer));
    }

    private static void TestElementAfterNonBlittableFixedBuffer()
    {
        Console.WriteLine("Start element after non-blittable fixed buffer test");

        var buffer = InitElementAfterNonBlittableFixedBuffer();

        Assert.IsTrue(VerifyElementAfterNonBlittableFixedBuffer(buffer));
    }

    public static int Main()
    {
        try
        {
            TestBlittableFixedBuffer();
            TestNonBlittableFixedBuffer();
            TestElementAfterNonBlittableFixedBuffer();
        }
        catch (System.Exception e)
        {
            Console.WriteLine(e.Message);
            return 101;
        }
        return 100;
    }
}
