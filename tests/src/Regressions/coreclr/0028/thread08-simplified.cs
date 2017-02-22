using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp22
{
    class Test
    {
        public static int Main(string[] args)
        {
            for (int i = 0; i < 100; i++)
            {
                var arr = new Test[100000];
                for (int j = 0; j < arr.Length; j++)
                {
                    arr[j] = new Test();
                }

                Parallel.For(0, 512, (e) =>
                {
                    for (int j = 0; j < arr.Length; j++)
                    {
                        arr[j].DoubleCheckLockTest();
                    }
                });
            }

            return 100;
        }


        bool flag;
        string str;

        public void DoubleCheckLockTest()
        {
            if (!flag)
            {
                lock(this)
                {
                    if (!flag)
                    {
                        str = "hello";
                    }

                    flag = true;
                }
            }

            str.GetType();
        }
    }
}
