// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;

namespace System
{
    internal static partial class AppContextDefaultValues
    {
        // For parsing a target Framework moniker, from the FrameworkName class
        private const char c_componentSeparator = ',';
        private const char c_keyValueSeparator = '=';
        private const char c_versionValuePrefix = 'v';
        private const String c_versionKey = "Version";
        private const String c_profileKey = "Profile";

        public static void PopulateDefaultValues()
        {
            string platformIdentifier, profile;
            int version;

            ParseTargetFrameworkName(out platformIdentifier, out profile, out version);

            // Call into each library to populate their default switches
            PopulateDefaultValuesPartial(platformIdentifier, profile, version);
        }

        /// <summary>
        /// We have this separate method for getting the parsed elements out of the TargetFrameworkName so we can
        /// more easily support this on other platforms.
        /// </summary>
        private static void ParseTargetFrameworkName(out string identifier, out string profile, out int version)
        {
            string targetFrameworkMoniker = AppDomain.CurrentDomain.SetupInformation.TargetFrameworkName;

            if (!TryParseFrameworkName(targetFrameworkMoniker, out identifier, out version, out profile))
            {
                // If we can't parse the TFM or we don't have a TFM, default to latest behavior for all 
                // switches (ie. all of them false).
                // If we want to use the latest behavior it is enough to set the value of the switch to string.Empty.
                // When the get to the caller of this method (PopulateDefaultValuesPartial) we are going to use the 
                // identifier we just set to decide which switches to turn on. By having an empty string as the 
                // identifier we are simply saying -- don't turn on any switches, and we are going to get the latest
                // behavior for all the switches
                identifier = string.Empty;
            }
        }

        // This code was a constructor copied from the FrameworkName class, which is located in System.dll.
        // Parses strings in the following format: "<identifier>, Version=[v|V]<version>, Profile=<profile>"
        //  - The identifier and version is required, profile is optional
        //  - Only three components are allowed.
        //  - The version string must be in the System.Version format; an optional "v" or "V" prefix is allowed
        private static bool TryParseFrameworkName(String frameworkName, out String identifier, out int version, out String profile)
        {
            identifier = profile = string.Empty;
            version = 0;

            if (frameworkName == null || frameworkName.Length == 0)
            {
                return false;
            }
            
            version = 0;

            // Identifer and Version are required, Profile is optional.
            int firstSeparatorIndex = frameworkName.IndexOf(c_componentSeparator);
            if (firstSeparatorIndex == -1) // No commas
            {
                return false;
            }
            
            int lastSeparatorIndex = frameworkName.LastIndexOf(c_componentSeparator);
            bool twoComponents = firstSeparatorIndex == lastSeparatorIndex;
            
            if (!twoComponents)
            {
                // Find the next comma after the first one
                int middleSeparatorIndex = frameworkName.IndexOf(c_componentSeparator, firstSeparatorIndex + 1);
                if (middleSeparatorIndex != lastSeparatorIndex) // More than 4 components
                {
                    return false;
                }
            }

            //
            // 1) Parse the "Identifier", which must come first. Trim any whitespace
            //
            identifier = frameworkName.Substring(0, firstSeparatorIndex).Trim();

            if (identifier.Length == 0)
            {
                return false;
            }
            
            string secondComponent;
            int secondComponentIndex = firstSeparatorIndex + 1;
            
            if (twoComponents) // Second string must be the version
            {
                secondComponent = frameworkName.Substring(secondComponentIndex);
                return TryParseVersion(secondComponent, out version);
            }
            
            // Version and profile were provided (or 2 versions)
            secondComponent = frameworkName.Substring(secondComponentIndex, lastSeparatorIndex - secondComponentIndex);
            string thirdComponent = frameworkName.Substring(lastSeparatorIndex + 1);
            
            if (TryParseVersion(secondComponent, out version))
            {
                // Third component has to be a profile or another version
                if (TryParseProfile(thirdComponent, out profile))
                {
                    return true;
                }
                return TryParseVersion(thirdComponent, out version);
            }
            
            // Second component has to be a profile
            if (!TryParseProfile(secondComponent, out profile))
            {
                return false;
            }
            
            // and the third has to be a version
            return TryParseVersion(thirdComponent, out version);
        }
        
        private static bool TryParseVersion(string input, out int version)
        {
            // Get the key/value pair separated by '='
            int separatorIndex = input.IndexOf(c_keyValueSeparator);
            if (separatorIndex != input.LastIndexOf(c_keyValueSeparator))
            {
                version = 0;
                return false;
            }
            
            // Get the key and value, trimming any whitespace
            string key = input.Substring(0, separatorIndex).Trim();
            string value = input.Substring(separatorIndex + 1).Trim();

            if (!key.Equals(c_versionKey, StringComparison.OrdinalIgnoreCase))
            {
                version = 0;
                return false;
            }
            
            // Allow the version to include a 'v' or 'V' prefix...
            if (value.Length > 0 && (value[0] == c_versionValuePrefix || value[0] == 'V'))
            {
                value = value.Substring(1);
            }
            
            Version realVersion = new Version(value);
            // The version class will represent some unset values as -1 internally (instead of 0).
            version = realVersion.Major * 10000;
            if (realVersion.Minor > 0)
                version += realVersion.Minor * 100;
            if (realVersion.Build > 0)
                version += realVersion.Build;
            
            return true;
        }
        
        private static bool TryParseProfile(string input, out string profile)
        {
            // Get the key/value pair separated by '='
            int separatorIndex = input.IndexOf(c_keyValueSeparator);
            if (separatorIndex != input.LastIndexOf(c_keyValueSeparator))
            {
                profile = string.Empty;
                return false;
            }
            
            // Get the key and value, trimming any whitespace
            string key = input.Substring(0, separatorIndex).Trim();
            string value = input.Substring(separatorIndex + 1).Trim();
            
            bool validProfile = key.Equals(c_profileKey, StringComparison.OrdinalIgnoreCase);
            profile = validProfile ? value : string.Empty;
            return validProfile;
        }

        // This is a partial method. Platforms (such as Desktop) can provide an implementation of it that will read override value
        // from whatever mechanism is available on that platform. If no implementation is provided, the compiler is going to remove the calls
        // to it from the code
        static partial void TryGetSwitchOverridePartial(string switchName, ref bool overrideFound, ref bool overrideValue);

        /// This is a partial method. This method is responsible for populating the default values based on a TFM.
        /// It is partial because each library should define this method in their code to contain their defaults.
        static partial void PopulateDefaultValuesPartial(string platformIdentifier, string profile, int version);
    }
}
