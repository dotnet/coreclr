// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.InteropServices;
using System.Text;

class LPTStrTestNative
{
    [DllImport(nameof(LPTStrTestNative), CharSet = CharSet.Unicode)]
    public static extern bool Verify_NullTerminators_PastEnd(StringBuilder builder, int length);

    [DllImport(nameof(LPTStrTestNative), EntryPoint = "Verify_NullTerminators_PastEnd", CharSet = CharSet.Unicode)]
    public static extern bool Verify_NullTerminators_PastEnd_Out([Out] StringBuilder builder, int length);
}
