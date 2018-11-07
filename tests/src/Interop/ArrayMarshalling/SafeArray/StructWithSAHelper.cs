// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;

#pragma warning disable CS0612, CS0618

[StructLayout(LayoutKind.Sequential)]
public struct StructWithSA
{
    public int i32;
    [MarshalAs(UnmanagedType.SafeArray, SafeArraySubType = VarEnum.VT_I4)]
    public int[] ArrOfInt32s;
}

class Helper
{
	public static int NumArrElements = 256;

	public static bool CheckStructWithSA(StructWithSA s)
	{
		if( s.i32 != 1 ) {
			Console.WriteLine("\t\tError!  s.i32 = " + s.i32);
			return false;
		}
		
		if( s.ArrOfInt32s.Length != NumArrElements ) 
		{
			Console.WriteLine("\t\tError!  s.ArrOfInt32s.Length != NumArrElements; Length = " + s.ArrOfInt32s.Length);	 
			return false;
		}
		for(int i = 0; i < s.ArrOfInt32s.Length; i++)
        {
			if( s.ArrOfInt32s[i] != 7 )
			{
				Console.WriteLine("\t\tError!  s.ArrOfInt32s[i] != 7; i = " + i);	 
				return false;
			}
        }
		
		return true;
	}
	
	public static bool CheckChangedStructWithSA(StructWithSA s)
	{
		if( s.i32 != 77 ) {
			Console.WriteLine("\t\tError!  s.i32 = " + s.i32);
			return false;
		}
		
		if( s.ArrOfInt32s.Length != NumArrElements ) 
		{
			Console.WriteLine("\t\tError!  s.ArrOfInt32s.Length != NumArrElements; Length = " + s.ArrOfInt32s.Length);	 
			return false;
		}
		for(int i = 0; i < s.ArrOfInt32s.Length; i++)
        {
			if( s.ArrOfInt32s[i] != 77 )
			{
				Console.WriteLine("\t\tError!  s.ArrOfInt32s[i] != 77; i = " + i);	 
				return false;
			}
        }
		
		return true;
	}
	
	public static StructWithSA NewStructWithSA()
	{
		StructWithSA s = new StructWithSA();
		s.i32 = 1; 
		s.ArrOfInt32s = new Int32[NumArrElements];
		for(int i = 0; i < s.ArrOfInt32s.Length; i++)
        {
			s.ArrOfInt32s[i] = 7;
        }
			
		return s;
	}
		
}
