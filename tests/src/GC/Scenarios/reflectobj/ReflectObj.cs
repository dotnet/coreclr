// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/**************************************************************/
/* TEST: ReflectObj
/* Purpose: test if GC can handle objects create by reflect
/* Coverage:    Class.CreateInstance()
/*              Class.GetField()
/*              Class.GetConstructor()
/*              ConstructorInfo.Invoke()
/*              FieldInfo.SetValue()
/*              FieldInfo.IsStatic()
/*              FieldInfo.Ispublic()
/**************************************************************/

namespace App
{
    using System;
    using System.Reflection;
    using System.Collections;

    internal class ReflectObj
    {
        private Object _obj;
        public static int icCreat = 0;
        public static int icFinal = 0;
        public static ArrayList al = new ArrayList();
        public ReflectObj()
        {
            _obj = new long[1000];
            icCreat++;
        }

        public ReflectObj(int l)
        {
            _obj = new long[l];
            icCreat++;
        }

        public Object GetObj()
        {
            return _obj;
        }

        ~ReflectObj()
        {
            al.Add(GetObj());
            icFinal++;
        }

        public static int Main(String[] str)
        {
            Console.WriteLine("Test should return with ExitCode 100 ...");
            CreateObj temp = new CreateObj();
            if (temp.RunTest())
            {
                Console.WriteLine("Test Passed");
                return 100;
            }
            Console.WriteLine("Test Failed");
            return 1;
        }

        private class CreateObj
        {
            private Object[] _v;
            private Type _myClass;
            private Type[] _rtype;
            private ConstructorInfo _CInfo;

            public CreateObj()
            {
                _myClass = Type.GetType("App.ReflectObj");
                _v = new Object[1];
                for (int i = 0; i < 2000; i++)
                {
                    _v[0] = i;
                    Activator.CreateInstance(_myClass, _v);
                }
            }

            public bool RunTest()
            {
                bool retVal = false;
                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                Console.WriteLine("Created Objects: {0} Finalized objects: {1}", icCreat, icFinal);
                if (icFinal != icCreat)
                {
                    return false;
                }

                FieldInfo fInfo = _myClass.GetField("icCreat", BindingFlags.IgnoreCase);
                fInfo = _myClass.GetField("icFinal", BindingFlags.IgnoreCase);

                Console.WriteLine("Fieldinfo done"); //debug;

                CreateMoreObj();

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();

                retVal = (icFinal == icCreat);

                Console.WriteLine("Living objects: " + ReflectObj.al.Count);
                ReflectObj.al = null;

                return retVal;
            }

            public void CreateMoreObj()
            {
                _rtype = new Type[0];
                _CInfo = _myClass.GetConstructor(_rtype);

                for (int i = 0; i < 2000; i++)
                {
                    _CInfo.Invoke((Object[])null);
                }
            }
        }
    }
}
