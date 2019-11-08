using System;
using System.Runtime.CompilerServices;

namespace Sample
{
    public sealed class C1 : I1<string>
    {
    }

    class Program
    {
        static int Main(string[] args)
        {
            Test1();
            Test2();
            return 100;
        }

        private static void Test1()
        {
            if (((I1<string>)new C1()).GetItemType() != typeof(string))
                throw new Exception("Test1 failed");
        }

        private static void Test2()
        {
            I1<string> c1 = new C1();
            if (c1.GetItemType() != typeof(string))
                throw new Exception("Test1 failed");
        }
    }
}

public interface I1<T>
{
    [MethodImpl(MethodImplOptions.NoInlining)]
    sealed Type GetItemType() => typeof(T);
}