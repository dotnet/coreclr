using System;
using System.Security;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using TestLibrary;

namespace PInvokeTests
{
    [SecuritySafeCritical]
    [StructLayout(LayoutKind.Sequential)]
    public class SeqClass
    {
        public int a;
        public bool b;
        public string str;

        public SeqClass(int _a, bool _b, string _str)
        {
            a = _a;
            b = _b;
            str = String.Concat(_str, "");
        }
    }

    [SecuritySafeCritical]
    [StructLayout(LayoutKind.Explicit)]
    public class ExpClass
    {
        [FieldOffset(0)]
        public DialogResult type;

        [FieldOffset(8)]
        public int i;

        [FieldOffset(8)]
        public bool b;

        [FieldOffset(8)]
        public double c;

        public ExpClass(DialogResult t, int num)
        {
            type = t;
            b = false;
            c = num;
            i = num;
        }
        public ExpClass(DialogResult t, double dnum)
        {
            type = t;
            b = false;
            i = 0;
            c = dnum;
        }
        public ExpClass(DialogResult t, bool bnum)
        {
            type = t;
            i = 0;
            c = 0;
            b = bnum;
        }
     }

    public enum DialogResult
    {
        None = 0,
        OK = 1,
        Cancel = 2
    }

    public struct NestedLayout
    {
        public SeqClass value;
    }

    class StructureTests
    {
        //Simple struct - sequential layout by ref
        [DllImport("LayoutClassNative")]
        private static extern bool SimpleSeqLayoutClassByRef(SeqClass p);

        [DllImport("LayoutClassNative")]
        private static extern bool SimpleExpLayoutClassByRef(ExpClass p);

        [DllImport("LayoutClassNative")]
        private static extern bool SimpleNestedLayoutClassByValue(NestedLayout p);

        public static bool SequentialClass()
        {
            string s = "before";
            string changedValue = "after";
            bool retval = true;
            SeqClass p = new SeqClass(0, false, s);

            TestFramework.BeginScenario("Test #1 (Roundtrip of a sequential layout class. Verify that values updated on unmanaged side reflect on managed side)");

            try
            {
                retval = SimpleSeqLayoutClassByRef(p);

                if (retval == false)
                {
                    TestFramework.LogError("01", "PInvokeTests->SequentialClass : Unexpected error occured on unmanaged side");
                    return false;
                }
            }
            catch (Exception e)
            {
                TestFramework.LogError("04", "Unexpected exception: " + e.ToString());
                retval = false;
            }

            return retval;
        }

        public static bool ExplicitClass()
        {
            ExpClass p;
            bool retval = false;

            TestFramework.BeginScenario("Test #3 (Roundtrip of a explicit layout class by reference. Verify that values updated on unmanaged side reflect on managed side)");
            //direct pinvoke

            //cdecl
            try
            {
                p = new ExpClass(DialogResult.None, 10);
                retval = SimpleExpLayoutClassByRef(p);

                if (retval == false)
                {
                    TestFramework.LogError("01", "PInvokeTests->ExplicitClass : Unexpected error occured on unmanaged side");
                    return false;
                }

            }
            catch (Exception e)
            {
                TestFramework.LogError("03", "Unexpected exception: " + e.ToString());
                retval = false;
            }

            return retval;
        }

        
        public static bool NestedLayoutClass()
        {
            string s = "before";
            string changedValue = "after";
            bool retval = true;
            SeqClass p = new SeqClass(0, false, s);
            NestedLayout target = new NestedLayout
            {
                value = p
            };

            TestFramework.BeginScenario("Test #3 (Roundtrip of a nested sequential layout class in a structure. Verify that values updated on unmanaged side reflect on managed side)");

            try
            {
                retval = SimpleNestedLayoutClassByValue(target);

                if (retval == false)
                {
                    TestFramework.LogError("01", "PInvokeTests->NestedLayoutClass : Unexpected error occured on unmanaged side");
                    return false;
                }
            }
            catch (Exception e)
            {
                TestFramework.LogError("04", "Unexpected exception: " + e.ToString());
                retval = false;
            }

            return retval;
        }

        public static int Main(string[] argv)
        {
            bool retVal = true;

            retVal = retVal && SequentialClass();
            retVal = retVal && ExplicitClass();
            retVal = retVal && NestedLayoutClass();

            return (retVal ? 100 : 101);
        }


    }
}
