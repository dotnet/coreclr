// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    internal static partial class Number
    {
        [MethodImpl(MethodImplOptions.InternalCall)]
        public static extern double NumberToDouble(ref NumberBuffer number);
    }
}
