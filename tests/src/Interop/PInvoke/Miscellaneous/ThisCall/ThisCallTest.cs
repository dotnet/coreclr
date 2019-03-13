// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using TestLibrary;

class ThisCallNative
{
    public struct SizeF
    {
        public float width;
        public float height;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate SizeF GetSizeFn(IntPtr c);

    [DllImport(nameof(ThisCallNative))]
    public static extern IntPtr CreateInstanceOfC(float width, float height);
    [DllImport(nameof(ThisCallNative))]
    public static extern GetSizeFn GetSizeMemberFunction();
    [DllImport(nameof(ThisCallNative))]
    public static extern IntPtr FreeInstanceOfC(IntPtr instance);
}

class HandleRefTest
{
    public unsafe static int Main(string[] args)
    {
        try
        {
            float width = 1.0f;
            float height = 2.0f;
            IntPtr instance = ThisCallNative.CreateInstanceOfC(width, height);
            ThisCallNative.GetSizeFn callback = ThisCallNative.GetSizeMemberFunction();

            ThisCallNative.SizeF result = callback(instance);

            Assert.AreEqual(width, result.width);
            Assert.AreEqual(height, result.height);

            ThisCallNative.FreeInstanceOfC(instance);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            return 101;
        }
        return 100;
    }
}
