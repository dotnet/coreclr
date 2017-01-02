using System;
using System.Runtime.InteropServices;

namespace PInvokeTests
{
    class SymVerTests
    {
        #region PInvoke declarations

        [DllImport("SymVerNative", EntryPoint="foo")]
        private static extern int foo_10();

        [DllImport("SymVerNative", EntryPoint="foo@VERS_1.1")]
        private static extern int foo_11();

        [DllImport("SymVerNative", EntryPoint="foo@VERS_1.2")]
        private static extern int foo_12();

        #endregion

        #region test methods

        public static void FooTest()
        {
            try
            {
                Assert(foo_10() == 10, "Call to foo_12 should return 10");
                Assert(foo_11() == 11, "Call to foo_12 should return 11");
                Assert(foo_12() == 12, "Call to foo_12 should return 12");
            }
            catch (Exception e)
            {
                Assert(false, e.ToString());
            }
        }

        #endregion
        
        private static bool passed = true;
        
        public static void Assert(bool value, string message)
        {
            if (!value)
            {
                Console.WriteLine("FAIL! " + message);
                passed = false;
            }
        }

        public static int Main(string[] argv)
        {
            FooTest();

            if (!passed)
                Console.WriteLine("FAIL");
            else
                Console.WriteLine("PASS");
            return (passed ? 100 : 101);
        }

        
    }
}
