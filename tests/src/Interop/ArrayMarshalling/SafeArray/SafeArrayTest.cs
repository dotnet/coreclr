// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*	SafeArrayMarshaler.cs
	Coverage test for UnmanagedType.SafeArray 
	YAlvi, 10/3/01
	Addition tests written: 5/11/03
*/

using System;
using System.Threading;
using System.Runtime.InteropServices;

public class Tester
{
    [DllImport("SafeArrayNative.dll")]
    public static extern bool SafeArray_In([In][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] int[] arr);

    [DllImport("SafeArrayNative.dll")]
    public static extern bool SafeArray_InOut([In, Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] int[] arr);

    [DllImport("SafeArrayNative.dll")]
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    public static extern int[] SafeArray_Ret();

    [DllImport("SafeArrayNative.dll")]
    public static extern bool SafeArray_InByRef([In][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] ref int[] arr);

    [DllImport("SafeArrayNative.dll")]
    public static extern bool SafeArray_InOutByRef([In, Out][MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] ref int[] arr);

    [DllImport("SafeArrayNative.dll", EntryPoint = "SafeArrayWithOutAttribute")]
    public static extern bool SafeArrayWithOutAttribute([Out, MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)] int[] arr);

    [DllImport("SafeArrayNative.dll")]
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    public static extern int[] SafeArray_Ret_MismatchRank();

    [DllImport("SafeArrayNative.dll")]
    [return: MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    public static extern int[] SafeArray_Ret_InvalidLBound();

    public static int Main()
    {
        int retVal = 100;
        int size = 256;
        int[] arr = NewIntArr(size);

        Console.WriteLine("Testing SafeArray Marshaling...\n");

        //testing SafeArray_In
        Console.WriteLine("Calling SafeArray_In...");
        if (!SafeArray_In(arr))
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_In call failed!");
        }
        else
            Console.WriteLine("\tPassed.");

        //testing SafeArray_InOut
        Console.WriteLine("Calling SafeArray_InOut...");
        if (!SafeArray_InOut(arr))
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_InOut did not receive param as expected!");
        }
        else
        {
            if (!IsIntArrReversed(arr)) //data in array should have been reversed
            {
                retVal = 101;
                Console.WriteLine("\tSafeArray_InOut did not return param as expected!");
            }
            else
                Console.WriteLine("\tPassed.");
        }

        //testing SafeArray_Ret
        Console.WriteLine("Calling SafeArray_Ret...");
        int[] arrRet = SafeArray_Ret();
        if (arrRet.Length != 1024)
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_Ret: returned array's size not as expected!");
        }
        else if (!IsIntArrOfAllOnes(arrRet))//every bin should contain -1
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_Ret's returned array not as expected!");
        }
        else
            Console.WriteLine("\tPassed.");

        //reset arr an put elements in order again
        arr = NewIntArr(size);

        //testing SafeArray_InByRef
        Console.WriteLine("Calling SafeArray_InByRef...");
        if (!SafeArray_InByRef(ref arr))
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_InByRef did not receive param as expected!");
        }
        else
            Console.WriteLine("\tPassed.");

        //testing SafeArray_InOutByRef
        Console.WriteLine("Calling SafeArray_InOutByRef...");
        if (!SafeArray_InOutByRef(ref arr))
        {
            retVal = 101;
            Console.WriteLine("\tSafeArray_InOutByRef did not receive param as expected!");
        }
        else
        {
            if (!IsIntArrReversed(arr)) //data in array should have been reversed
            {
                retVal = 101;
                Console.WriteLine("\tSafeArray_InOutByRef did not return param as expected!");
            }
            else
                Console.WriteLine("\tPassed.");
        }

        //reset arr an put elements in order again
        arr = NewIntArr(size);

        Console.WriteLine("Calling SafeArrayWithOutAttribute...");
        if (!SafeArrayWithOutAttribute(arr))
        {
            retVal = 101;
            Console.WriteLine("\tSafeArrayWithOutAttribute call failed!");
        }
        else if (!IsIntArrOfAllOnes(arr))//every bin should contain -1
        {
            retVal = 101;
            Console.WriteLine("\tSafeArrayWithOutAttribute returned array not as expected!");
        }
        else
            Console.WriteLine("\tPassed.");

        //testing SafeArray_Ret_MismatchRank
        Console.WriteLine("Calling SafeArray_Ret_MismatchRank...");
        try
        {
            arrRet = SafeArray_Ret_MismatchRank();
        }
        catch (SafeArrayRankMismatchException sae)
        {
            if (Thread.CurrentThread.CurrentCulture.Name == "en-US")
            {
                if (sae.Message != "SafeArray of rank 2 has been passed to a method expecting an array of rank 1.")
                {
                    retVal = 101;
                    Console.WriteLine("Exception message not as expected! FAILED! Message is: " + sae.Message);
                }
                else
                    Console.WriteLine("\tPassed.");
            }
            else
                Console.WriteLine("\tPassed.");
        }
        catch (Exception e)
        {
            retVal = 101;
            Console.WriteLine("Unexpected exception: " + e.ToString());
        }

        //testing SafeArray_Ret_InvalidLBound
        Console.WriteLine("Calling SafeArray_Ret_InvalidLBound..");
        try
        {
            arrRet = SafeArray_Ret_InvalidLBound();
        }
        catch (SafeArrayRankMismatchException sae)
        {
            Console.WriteLine("\tPassed.");
        }
        catch (Exception e)
        {
            retVal = 101;
            Console.WriteLine("Unexpected exception: " + e.ToString());
        }

        return retVal;
    }

    internal static int[] NewIntArr(int size)
    {
        int[] arr = new int[size];
        //initializing arr
        for (int i = 0; i < arr.Length; i++)
            arr[i] = i;
        return arr;
    }

    internal static bool IsIntArrReversed(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != arr.Length - i - 1) //data in array should have been reversed
                return false;
        }
        return true;
    }

    internal static bool IsIntArrOfAllOnes(int[] arr)
    {
        for (int i = 0; i < arr.Length; i++)
        {
            if (arr[i] != -1) //every bin should contain -1
                return false;
        }
        return true;
    }
}
