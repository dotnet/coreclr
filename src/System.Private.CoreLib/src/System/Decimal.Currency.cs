// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Runtime.CompilerServices;

namespace System
{
    public partial struct Decimal
    {
        // Converts a Decimal to a Currency. Since a Currency
        // has fewer significant digits than a Decimal, this operation may
        // produce round-off errors.
        //
        internal static Currency ToCurrency(decimal d)
        {
            Currency result = new Currency();
            FCallToCurrency(ref result, d);
            return result;
        }

        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern void FCallToCurrency(ref Currency result, decimal d);
    }
}
