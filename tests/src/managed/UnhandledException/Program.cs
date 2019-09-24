using System;

namespace UnhandledException
{
    class Program
    {
        static void Main(string[] args)
        {
            Throw();
        }

        static void Throw() => throw new Exception();
    }
}
