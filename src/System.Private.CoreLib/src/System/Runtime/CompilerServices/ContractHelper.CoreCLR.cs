// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics.Contracts;

namespace System.Runtime.CompilerServices
{
    public static partial class ContractHelper
    {
        /// <summary>
        /// Rewriter calls this method to get the default failure behavior.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        public static void TriggerFailure(ContractFailureKind kind, string displayMessage, string userMessage, string conditionText, Exception innerException)
        {
            if (string.IsNullOrEmpty(displayMessage))
            {
                displayMessage = GetDisplayMessage(kind, userMessage, conditionText);
            }

            System.Diagnostics.Debug.ContractFailure(false, displayMessage, string.Empty, GetResourceNameForFailure(kind));
        }
    }
}
