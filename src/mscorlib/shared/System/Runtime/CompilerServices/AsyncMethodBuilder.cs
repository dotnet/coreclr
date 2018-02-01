// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Diagnostics;
using System.Threading;

namespace System.Runtime.CompilerServices
{
    internal static partial class AsyncMethodBuilder
    {
        /// <summary>Initiates the builder's execution with the associated state machine.</summary>
        /// <typeparam name="TStateMachine">Specifies the type of the state machine.</typeparam>
        /// <param name="stateMachine">The state machine instance, passed by reference.</param>
        [DebuggerStepThrough]
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Start<TStateMachine>(ref TStateMachine stateMachine) where TStateMachine : IAsyncStateMachine
        {
            if (stateMachine == null) // TStateMachines are generally non-nullable value types, so this check will be elided
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.stateMachine);
            }

            Thread currentThread = Thread.CurrentThread;
            ExecutionContext previousExecutionCtx = currentThread.ExecutionContext;
            SynchronizationContext previousSyncCtx = currentThread.SynchronizationContext;

            // Async state machines are required not to throw, so no need for try/finally here.
            stateMachine.MoveNext();

            // The common case is that these have not changed, so avoid the cost of a write barrier if not needed.
            if (previousSyncCtx != currentThread.SynchronizationContext)
            {
                // Restore changed SynchronizationContext back to previous
                currentThread.SynchronizationContext = previousSyncCtx;
            }

            ExecutionContext currentExecutionCtx = currentThread.ExecutionContext;
            if (previousExecutionCtx != currentExecutionCtx)
            {
                // Restore changed ExecutionContext back to previous
                currentThread.ExecutionContext = previousExecutionCtx;
                if ((currentExecutionCtx != null && currentExecutionCtx.HasChangeNotifications) ||
                    (previousExecutionCtx != null && previousExecutionCtx.HasChangeNotifications))
                {
                    // There are change notifications; trigger any affected
                    ExecutionContext.OnValuesChanged(currentExecutionCtx, previousExecutionCtx);
                }
            }
        }
    }
}
