// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Globalization;

namespace Internal.Globalization
{
    public static class CultureDataSupport
    {
        /// <summary>
        /// Check whether CultureData exists for specified language
        /// This API is used for WindowsRuntimeResourceManager in System.Runtime.WindowsRuntime
        /// </summary>
        public static bool IsCultureDataExists(string language)
        {
            return CultureData.GetCultureData(language, /* useUserOverride */ true) != null;
        }
    }
}
