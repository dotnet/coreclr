// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.InteropServices;
using System;
using System.Reflection;
using System.Text;
using TestLibrary;

#pragma warning disable CS0612, CS0618

class Test
{
    [DllImport("VBByRefStrNative", CharSet = CharSet.Ansi)]
    private static extern bool Marshal_Ansi(string expected, [MarshalAs(UnmanagedType.VBByRefStr)] ref string actual, string newValue);
    [DllImport("VBByRefStrNative", CharSet = CharSet.Unicode)]
    private static extern bool Marshal_Unicode(string expected, [MarshalAs(UnmanagedType.VBByRefStr)] ref string actual, string newValue);

    [DllImport("VBByRefStrNative", EntryPoint = "Marshal_Invalid")]
    private static extern bool Marshal_StringBuilder([MarshalAs(UnmanagedType.VBByRefStr)]ref  StringBuilder builder);

    [DllImport("VBByRefStrNative", EntryPoint = "Marshal_Invalid")]
    private static extern bool Marshal_ByVal([MarshalAs(UnmanagedType.VBByRefStr)]string str);

    public static int Main(string[] args)
    {
        try
        {
            string expected = "abcdefgh";
            string actual;
            string newValue = "zyxwvut\0";

            actual = expected;
            Assert.IsTrue(Marshal_Ansi(expected, ref actual, newValue));
            Assert.AreEqual(newValue, actual);

            actual = expected;
            Assert.IsTrue(Marshal_Unicode(expected, ref actual, newValue));
            Assert.AreEqual(newValue, actual);

            StringBuilder builder = new StringBuilder();

            Assert.Throws<MarshalDirectiveException>(() => Marshal_StringBuilder(ref builder));
            Assert.Throws<MarshalDirectiveException>(() => Marshal_ByVal(string.Empty));
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return 101;
        }
        return 100;
    }
}
