// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using TestLibrary;

unsafe class ThisCallNative
{
    public struct C
    {
        public struct VtableLayout
        {
            public IntPtr getSize;
            public IntPtr getWidth;
            public IntPtr getHeightAsInt;
            public IntPtr getInt;
        }

        public VtableLayout* vtable;
        private int c;
        public readonly float width;
        public readonly float height;
    }

    public struct SizeF
    {
        public float width;
        public float height;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct Width
    {
        // Add a duplicate field so we can still test a 4-byte HFA return type being passed byref
        // without hitting our workaround for single-field struct returns on instance methods.
        [FieldOffset(0)]
        private float dummyField;
        [FieldOffset(0)]
        public float width;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct HeightAsInt
    {
        // Add a duplicate field so we can still test a 4-byte non HFA return type being passed byref
        // without hitting our workaround for single-field struct returns on instance methods.
        [FieldOffset(0)]
        private int dummyField;
        [FieldOffset(0)]
        public int i;
    }

    public struct StructWrappingSingleInt
    {
        public int i;
    }

    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate SizeF GetSizeFn(C* c);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate Width GetWidthFn(C* c);
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate HeightAsInt GetHeightAsIntFn(C* c);

    // We specifically don't want to match the return type here so that we can correctly test
    // the workaround.
    [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
    public delegate StructWrappingSingleInt GetIntFn(C* c, int value);

    [DllImport(nameof(ThisCallNative))]
    public static extern C* CreateInstanceOfC(float width, float height);
}

class ThisCallTest
{
    public unsafe static int Main(string[] args)
    {
        try
        {
            float width = 1.0f;
            float height = 2.0f;
            ThisCallNative.C* instance = ThisCallNative.CreateInstanceOfC(width, height);
            Test8ByteHFA(instance);
            Test4ByteHFA(instance);
            Test4ByteNonHFA(instance);
            TestSingleFieldStructReturnWorkaround(instance);
        }
        catch (System.Exception ex)
        {
            Console.WriteLine(ex);
            return 101;
        }
        return 100;
    }

    private static unsafe void Test8ByteHFA(ThisCallNative.C* instance)
    {
        ThisCallNative.GetSizeFn callback = Marshal.GetDelegateForFunctionPointer<ThisCallNative.GetSizeFn>(instance->vtable->getSize);

        ThisCallNative.SizeF result = callback(instance);

        Assert.AreEqual(instance->width, result.width);
        Assert.AreEqual(instance->height, result.height);
    }

    private static unsafe void Test4ByteHFA(ThisCallNative.C* instance)
    {
        ThisCallNative.GetWidthFn callback = Marshal.GetDelegateForFunctionPointer<ThisCallNative.GetWidthFn>(instance->vtable->getWidth);

        ThisCallNative.Width result = callback(instance);

        Assert.AreEqual(instance->width, result.width);
    }

    private static unsafe void Test4ByteNonHFA(ThisCallNative.C* instance)
    {
        ThisCallNative.GetHeightAsIntFn callback = Marshal.GetDelegateForFunctionPointer<ThisCallNative.GetHeightAsIntFn>(instance->vtable->getHeightAsInt);

        ThisCallNative.HeightAsInt result = callback(instance);

        Assert.AreEqual((int)instance->height, result.i);
    }

    private static unsafe void TestSingleFieldStructReturnWorkaround(ThisCallNative.C* instance)
    {
        ThisCallNative.GetIntFn callback = Marshal.GetDelegateForFunctionPointer<ThisCallNative.GetIntFn>(instance->vtable->getInt);

        int value = 42;
        ThisCallNative.StructWrappingSingleInt result = callback(instance, value);

        Assert.AreEqual(value, result.i);
    }
}
