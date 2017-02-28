// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if ENABLE_WINRT
using Internal.Runtime.Augments;
#endif

namespace System.Globalization
{
    public partial class CultureInfo : IFormatProvider
    {
        /// <summary>
        /// Gets the default user culture from WinRT, if available.
        /// </summary>
        /// <remarks>
        /// This method may return null, if there is no default user culture or if WinRT isn't available.
        /// </remarks>
        private static CultureInfo GetUserDefaultCultureCacheOverride()
        {
#if ENABLE_WINRT
            WinRTInteropCallbacks callbacks = WinRTInterop.UnsafeCallbacks;
            if (callbacks != null && callbacks.IsAppxModel())
            {
                return (CultureInfo)callbacks.GetUserDefaultCulture();
            }
#endif

            return null;
        }

        internal static CultureInfo GetUserDefaultCulture()
        {
            const uint LOCALE_SNAME = 0x0000005c;
            const string LOCALE_NAME_USER_DEFAULT = null;
            const string LOCALE_NAME_SYSTEM_DEFAULT = "!x-sys-default-locale";

            string strDefault = CultureData.GetLocaleInfoEx(LOCALE_NAME_USER_DEFAULT, LOCALE_SNAME);
            if (strDefault == null)
            {
                strDefault = CultureData.GetLocaleInfoEx(LOCALE_NAME_SYSTEM_DEFAULT, LOCALE_SNAME);

                if (strDefault == null)
                {
                    // If system default doesn't work, use invariant
                    return CultureInfo.InvariantCulture;
                }
            }

            CultureInfo temp = GetCultureByName(strDefault, true);

            temp._isReadOnly = true;

            return temp;
        }

        private static CultureInfo GetUserDefaultUILanguage()
        {
            const uint MUI_LANGUAGE_NAME = 0x8;    // Use ISO language (culture) name convention
            uint langCount = 0;
            uint bufLen = 0;

            if (Interop.Kernel32.GetUserPreferredUILanguages(MUI_LANGUAGE_NAME, out langCount, null, ref bufLen))
            {
                char [] languages = new char[bufLen];
                if (Interop.Kernel32.GetUserPreferredUILanguages(MUI_LANGUAGE_NAME, out langCount, languages, ref bufLen))
                {
                    int index = 0;
                    while (languages[index] != (char) 0 && index<languages.Length)
                    {
                        index++;
                    }

                    CultureInfo temp = GetCultureByName(new String(languages, 0, index), true);
                    temp._isReadOnly = true;
                    return temp;
                }
            }

            return GetUserDefaultCulture();
        }
    }
}
