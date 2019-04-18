// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

using System;
namespace RangeCheck
{
    internal class RC1
    {
        public static int Main(String[] args)
        {
            int[] testArr = {0, 0, 0, 10, 10, 10, 10, 10, 10, 10};

            if (testArr.Length > 5)
            {
                testArr[0] = 10;
            }

            if (testArr.Length > 10)
            {
                testArr[10] = 10;
            }

            if (testArr.Length > 0 && testArr[0] == 10)
            {
                testArr[1] = 10;
            }
            else
            {
                try 
                {
                    testArr[10] = 10;
                    return 101;
                }
                catch(Exception e)
                {
                    
                }
            }

            if (testArr.Length < 3)
            {
                return 101;
            }
            else
            {
                testArr[2] = 10;
            }

            int result = 0;
            foreach(int i in testArr)
            {
                result += i;
            }

            return result;
        }
    }
}

