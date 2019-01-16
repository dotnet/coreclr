// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
**
** Implementation details of CLR Contracts.
**
===========================================================*/
#define DEBUG // The behavior of this contract library should be consistent regardless of build type.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace System.Diagnostics.Contracts
{
    public static partial class Contract
    {
        #region Private Methods

        /// <summary>
        /// This method is used internally to trigger a failure indicating to the "programmer" that he is using the interface incorrectly.
        /// It is NEVER used to indicate failure of actual contracts at runtime.
        /// </summary>
        static void AssertMustUseRewriter(ContractFailureKind kind, string contractKind)
        {
            // For better diagnostics, report which assembly is at fault.  Walk up stack and
            // find the first non-mscorlib assembly.
            Assembly thisAssembly = typeof(Contract).Assembly;  // In case we refactor mscorlib, use Contract class instead of Object.
            StackTrace stack = new StackTrace();
            Assembly probablyNotRewritten = null;
            for (int i = 0; i < stack.FrameCount; i++)
            {
                Assembly caller = stack.GetFrame(i).GetMethod().DeclaringType.Assembly;
                if (caller != thisAssembly)
                {
                    probablyNotRewritten = caller;
                    break;
                }
            }

            if (probablyNotRewritten == null)
                probablyNotRewritten = thisAssembly;
            string simpleName = probablyNotRewritten.GetName().Name;
            System.Runtime.CompilerServices.ContractHelper.TriggerFailure(kind, SR.Format(SR.MustUseCCRewrite, contractKind, simpleName), null, null, null);
        }

        #endregion Private Methods

        #region Failure Behavior

        /// <summary>
        /// Without contract rewriting, failing Assert/Assumes end up calling this method.
        /// Code going through the contract rewriter never calls this method. Instead, the rewriter produced failures call
        /// System.Runtime.CompilerServices.ContractHelper.RaiseContractFailedEvent, followed by 
        /// System.Runtime.CompilerServices.ContractHelper.TriggerFailure.
        /// </summary>
        [System.Diagnostics.DebuggerNonUserCode]
        static void ReportFailure(ContractFailureKind failureKind, string userMessage, string conditionText, Exception innerException)
        {
            if (failureKind < ContractFailureKind.Precondition || failureKind > ContractFailureKind.Assume)
                throw new ArgumentException(SR.Format(SR.Arg_EnumIllegalVal, failureKind), nameof(failureKind));

            // displayMessage == null means: yes we handled it. Otherwise it is the localized failure message
            var displayMessage = System.Runtime.CompilerServices.ContractHelper.RaiseContractFailedEvent(failureKind, userMessage, conditionText, innerException);

            if (displayMessage == null) return;

            System.Runtime.CompilerServices.ContractHelper.TriggerFailure(failureKind, displayMessage, userMessage, conditionText, innerException);
        }

        /// <summary>
        /// Allows a managed application environment such as an interactive interpreter (IronPython)
        /// to be notified of contract failures and 
        /// potentially "handle" them, either by throwing a particular exception type, etc.  If any of the
        /// event handlers sets the Cancel flag in the ContractFailedEventArgs, then the Contract class will
        /// not pop up an assert dialog box or trigger escalation policy.  Hooking this event requires 
        /// full trust, because it will inform you of bugs in the appdomain and because the event handler
        /// could allow you to continue execution.
        /// </summary>
        public static event EventHandler<ContractFailedEventArgs> ContractFailed
        {
            add
            {
                System.Runtime.CompilerServices.ContractHelper.InternalContractFailed += value;
            }
            remove
            {
                System.Runtime.CompilerServices.ContractHelper.InternalContractFailed -= value;
            }
        }
        #endregion FailureBehavior
    }
}  // namespace System.Diagnostics.Contracts

