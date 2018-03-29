using System;
using System.Diagnostics;

class Program
{
    static void Main()
    {
        A();
    }

    static void A()
    {
        B();
    }

    static void B()
    {
        throw new ArgumentException("my ae", ex);
    }
}
