// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Globalization
{
    internal sealed partial class GlobalizationMode
    {
        private const string c_InvariantModeConfigSwitch = "System.Globalization.Invariant";
        // Linux doesn't support environment variable names including dots
        private const string c_InvariantModeEnvironmentVariable = "DOTNET_System_Globalization_Invariant";
        internal static bool Invariant { get; } = GetGlobalizationInvariantMode();

        // GetInvariantSwitchValue calls CLRConfig first to detect if the switch is defined in the config file.
        // if the switch is defined we just use the value of this switch. otherwise, we'll try to get the switch
        // value from the environment variable if it is defined.
        internal static bool GetInvariantSwitchValue()
        {
            bool exist;
            bool ret = CLRConfig.GetBoolValue(c_InvariantModeConfigSwitch, out exist);
            if (!exist)
            {
                string switchValue = Environment.GetEnvironmentVariable(c_InvariantModeEnvironmentVariable);
                if (switchValue != null)
                {
                    ret = switchValue.Equals("true", StringComparison.OrdinalIgnoreCase);
                }
            }

            return ret;
        }
    }
}
