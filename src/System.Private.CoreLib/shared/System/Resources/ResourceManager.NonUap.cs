// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;

namespace System.Resources
{
    public partial class ResourceManager
    {
        private bool UseUapResourceManagement { get => false; }

        private string GetStringFromPRI(string stringName, CultureInfo culture, string neutralResourcesCulture) { return null; }

        private void SetAppXConfiguration() { }
    }
}
