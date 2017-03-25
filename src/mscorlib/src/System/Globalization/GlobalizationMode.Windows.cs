// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Globalization
{
    internal sealed partial class GlobalizationMode
    {
        private static bool GetGlobalizationInvariantMode()
        {
            return CLRConfig.GetBoolValue(c_InvariantModeConfigSwitch);
        }
    }
}
