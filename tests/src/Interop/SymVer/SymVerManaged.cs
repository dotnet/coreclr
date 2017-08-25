using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace PInvokeTests
{
    class SymVerTests
    {
        #region PInvoke declarations

        [DllImport("SymVerNative", EntryPoint="foo")]
        private static extern int foo();

        [DllImport("SymVerNative", EntryPoint="foo@")]
        private static extern int foo_base();

        [DllImport("SymVerNative", EntryPoint="foo@VERS_1.1")]
        private static extern int foo_11();

        [DllImport("SymVerNative", EntryPoint="foo@VERS_1.2")]
        private static extern int foo_12();

        [DllImport("SymVerNative", EntryPoint="foo@VERS_1.2", ExactSpelling=true)]
        private static extern int foo_12_exact();

        #endregion

        #region test methods

        public static void TestExactSpelling()
        {
            bool exceptionThrown = false;
            try
            {
                foo_12_exact();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Call to foo_12_exact should throw an exception");
        }

        public static void TestSupported()
        {
            try
            {
                Assert(foo() == 10, "Call to foo should return 10");
                Assert(foo_11() == 11, "Call to foo_11 should return 11");
                Assert(foo_12() == 12, "Call to foo_12 should return 12");
                Assert(foo_base() == 10, "Call to foo_base should return 10");
            }
            catch (Exception e)
            {
                Assert(false, e.ToString());
            }
        }

        public static void TestUnsupported()
        {
            bool exceptionThrown = false;
            try
            {
                foo_11();
            }
            catch (Exception)
            {
                exceptionThrown = true;
            }
            Assert(exceptionThrown, "Call to foo_11 should throw an exception");
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
            if (IsLinux)
            {
                TestSupported();
                TestExactSpelling();
            }
            else
            {
                TestUnsupported();
            }

            if (!passed)
                Console.WriteLine("FAIL");
            else
                Console.WriteLine("PASS");
            return (passed ? 100 : 101);
        }

        public static bool IsWindows
        {
            get
            {
                return Path.DirectorySeparatorChar == '\\';
            }
        }

        public static bool IsLinux
        {
            get
            {
                return GetUname() == "Linux";
            }
        }

        private static string _uname;
        private static string GetUname()
        {
            if (_uname != null)
            {
                return _uname;
            }
            if (IsWindows)
            {
                _uname = "Windows";
            }
            else
            {
                ProcessStartInfo startInfo = new ProcessStartInfo
                {
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    FileName = "uname",
                    Arguments = "-s"
                };

                using (Process process = new Process())
                {
                    process.StartInfo = startInfo;

                    process.Start();
                    _uname = process.StandardOutput.ReadLine();

                    process.WaitForExit();
                }
            }
            return _uname;
        }

    }
}
