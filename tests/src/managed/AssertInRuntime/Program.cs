using System.Runtime.InteropServices;

namespace AssertInRuntime
{
    class Program
    {

        unsafe struct FixedBufferClassificationTestBlittable
        {
            public fixed bool buffer[3];
            public float f;
        }
        
        [DllImport("MarshalStructAsParam")]
        static extern bool MarshalStructAsParam_AsSeqByValFixedBufferClassificationTest(FixedBufferClassificationTestBlittable str, float f);

        static void Main(string[] args)
        {
            FixedBufferClassificationTestBlittable buffer;
            buffer.f = 1.0f;
            MarshalStructAsParam_AsSeqByValFixedBufferClassificationTest(buffer, buffer.f);
        }
    }
}
