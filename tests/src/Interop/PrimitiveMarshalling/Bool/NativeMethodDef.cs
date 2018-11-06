// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

﻿using System;
using System.Runtime.InteropServices;
using System.Reflection;
using System.Text;

public class NativeMethods
{

    public const string NativeSharedBinaryName = "BoolNative";

    [DllImport(NativeSharedBinaryName)]
    public static extern bool Marshal_In([In]bool boolValue);

    [DllImport(NativeSharedBinaryName)]
    public static extern bool Marshal_InOut([In, Out]bool boolValue);

    [DllImport(NativeSharedBinaryName)]
    public static extern bool Marshal_Out([Out]bool boolValue);

    [DllImport(NativeSharedBinaryName)]
    public static extern bool MarshalPointer_In([In]ref bool pboolValue);

    [DllImport(NativeSharedBinaryName)]
    public static extern bool MarshalPointer_InOut(ref bool pboolValue);

    [DllImport(NativeSharedBinaryName)]
    public static extern bool MarshalPointer_Out(out bool pboolValue);

    [DllImport(NativeSharedBinaryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Marshal_As_In(
      [In, MarshalAs(UnmanagedType.U1)]bool boolValue);

    [DllImport(NativeSharedBinaryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Marshal_As_InOut(
      [In, Out, MarshalAs(UnmanagedType.U1)]bool boolValue);

    [DllImport(NativeSharedBinaryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Marshal_As_Out(
      [Out, MarshalAs(UnmanagedType.U1)]bool boolValue);

#pragma warning disable CS0612, CS0618
    public struct ContainsVariantBool
    {
        [MarshalAs(UnmanagedType.VariantBool)]
        public bool value;
    }

    [DllImport(NativeSharedBinaryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Marshal_ByValue_Variant(
        [MarshalAs(UnmanagedType.VariantBool)] bool value,
        [MarshalAs(UnmanagedType.U1)] bool expected);

    
    [DllImport(NativeSharedBinaryName)]
    [return: MarshalAs(UnmanagedType.U1)]
    public static extern bool Marshal_ByValue_Struct_Variant(
        ContainsVariantBool value,
        [MarshalAs(UnmanagedType.U1)] bool expected);

#pragma warning restore CS0612, CS0618

}
