// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 

/* --- */
// !! THIS FILE IS AUTO-GENERATED !!
// Any changes you make directly to this file will be overwritten by build tooling.
// Instead, make changes to ExceptionResource.cs (see comment at the top of that file)."
/* --- */

using System.Diagnostics;

namespace System
{
    internal enum ExceptionResource
    {
        Arg_ArrayPlusOffTooSmall,
        Arg_LowerBoundsMustMatch,
        Arg_MustBeType,
        Arg_Need1DArray,
        Arg_Need2DArray,
        Arg_Need3DArray,
        Arg_NeedAtLeast1Rank,
        Arg_NonZeroLowerBound,
        Arg_RankIndices,
        Arg_RankMultiDimNotSupported,
        Arg_RanksAndBounds,
        Arg_TypeNotSupported,
        Argument_AddingDuplicate,
        Argument_CannotExtractScalar,
        Argument_InvalidArgumentForComparison,
        Argument_InvalidOffLen,
        ArgumentException_OtherNotArrayOfCorrectLength,
        ArgumentNull_SafeHandle,
        ArgumentOutOfRange_BiggerThanCollection,
        ArgumentOutOfRange_Count,
        ArgumentOutOfRange_EndIndexStartIndex,
        ArgumentOutOfRange_Enum,
        ArgumentOutOfRange_HugeArrayNotSupported,
        ArgumentOutOfRange_Index,
        ArgumentOutOfRange_ListInsert,
        ArgumentOutOfRange_NeedNonNegNum,
        ArgumentOutOfRange_SmallCapacity,
        AsyncMethodBuilder_InstanceNotInitialized,
        ConcurrentCollection_SyncRoot_NotSupported,
        InvalidOperation_IComparerFailed,
        InvalidOperation_NullArray,
        InvalidOperation_WrongAsyncResultOrEndCalledMultiple,
        NotSupported_FixedSizeCollection,
        NotSupported_KeyCollectionSet,
        NotSupported_ReadOnlyCollection,
        NotSupported_StringComparison,
        NotSupported_ValueCollectionSet,
        Rank_MultiDimNotSupported,
        Serialization_MissingKeys,
        Serialization_NullKey,
        Task_ContinueWith_ESandLR,
        Task_ContinueWith_NotOnAnything,
        Task_Delay_InvalidDelay,
        Task_Delay_InvalidMillisecondsDelay,
        Task_Dispose_NotCompleted,
        Task_MultiTaskContinuation_EmptyTaskList,
        Task_MultiTaskContinuation_NullTask,
        Task_RunSynchronously_AlreadyStarted,
        Task_RunSynchronously_Continuation,
        Task_RunSynchronously_Promise,
        Task_RunSynchronously_TaskCompleted,
        Task_Start_AlreadyStarted,
        Task_Start_ContinuationTask,
        Task_Start_Promise,
        Task_Start_TaskCompleted,
        Task_ThrowIfDisposed,
        Task_WaitMulti_NullTask,
        TaskCompletionSourceT_TrySetException_NoExceptions,
        TaskCompletionSourceT_TrySetException_NullException,
        TaskT_TransitionToFinal_AlreadyCompleted,
    }

    internal static partial class ThrowHelper
    {
        private static string GetResourceString(ExceptionResource resource)
        {
            string resourceName;

            // nameof(...) is used in the switch statement below to guarantee there's a matching property on the SR class.

            switch (resource)
            {
                case ExceptionResource.Arg_ArrayPlusOffTooSmall:
                    resourceName = nameof(System.SR.Arg_ArrayPlusOffTooSmall);
                    break;
                case ExceptionResource.Arg_LowerBoundsMustMatch:
                    resourceName = nameof(System.SR.Arg_LowerBoundsMustMatch);
                    break;
                case ExceptionResource.Arg_MustBeType:
                    resourceName = nameof(System.SR.Arg_MustBeType);
                    break;
                case ExceptionResource.Arg_Need1DArray:
                    resourceName = nameof(System.SR.Arg_Need1DArray);
                    break;
                case ExceptionResource.Arg_Need2DArray:
                    resourceName = nameof(System.SR.Arg_Need2DArray);
                    break;
                case ExceptionResource.Arg_Need3DArray:
                    resourceName = nameof(System.SR.Arg_Need3DArray);
                    break;
                case ExceptionResource.Arg_NeedAtLeast1Rank:
                    resourceName = nameof(System.SR.Arg_NeedAtLeast1Rank);
                    break;
                case ExceptionResource.Arg_NonZeroLowerBound:
                    resourceName = nameof(System.SR.Arg_NonZeroLowerBound);
                    break;
                case ExceptionResource.Arg_RankIndices:
                    resourceName = nameof(System.SR.Arg_RankIndices);
                    break;
                case ExceptionResource.Arg_RankMultiDimNotSupported:
                    resourceName = nameof(System.SR.Arg_RankMultiDimNotSupported);
                    break;
                case ExceptionResource.Arg_RanksAndBounds:
                    resourceName = nameof(System.SR.Arg_RanksAndBounds);
                    break;
                case ExceptionResource.Arg_TypeNotSupported:
                    resourceName = nameof(System.SR.Arg_TypeNotSupported);
                    break;
                case ExceptionResource.Argument_AddingDuplicate:
                    resourceName = nameof(System.SR.Argument_AddingDuplicate);
                    break;
                case ExceptionResource.Argument_CannotExtractScalar:
                    resourceName = nameof(System.SR.Argument_CannotExtractScalar);
                    break;
                case ExceptionResource.Argument_InvalidArgumentForComparison:
                    resourceName = nameof(System.SR.Argument_InvalidArgumentForComparison);
                    break;
                case ExceptionResource.Argument_InvalidOffLen:
                    resourceName = nameof(System.SR.Argument_InvalidOffLen);
                    break;
                case ExceptionResource.ArgumentException_OtherNotArrayOfCorrectLength:
                    resourceName = nameof(System.SR.ArgumentException_OtherNotArrayOfCorrectLength);
                    break;
                case ExceptionResource.ArgumentNull_SafeHandle:
                    resourceName = nameof(System.SR.ArgumentNull_SafeHandle);
                    break;
                case ExceptionResource.ArgumentOutOfRange_BiggerThanCollection:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_BiggerThanCollection);
                    break;
                case ExceptionResource.ArgumentOutOfRange_Count:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_Count);
                    break;
                case ExceptionResource.ArgumentOutOfRange_EndIndexStartIndex:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_EndIndexStartIndex);
                    break;
                case ExceptionResource.ArgumentOutOfRange_Enum:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_Enum);
                    break;
                case ExceptionResource.ArgumentOutOfRange_HugeArrayNotSupported:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_HugeArrayNotSupported);
                    break;
                case ExceptionResource.ArgumentOutOfRange_Index:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_Index);
                    break;
                case ExceptionResource.ArgumentOutOfRange_ListInsert:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_ListInsert);
                    break;
                case ExceptionResource.ArgumentOutOfRange_NeedNonNegNum:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_NeedNonNegNum);
                    break;
                case ExceptionResource.ArgumentOutOfRange_SmallCapacity:
                    resourceName = nameof(System.SR.ArgumentOutOfRange_SmallCapacity);
                    break;
                case ExceptionResource.AsyncMethodBuilder_InstanceNotInitialized:
                    resourceName = nameof(System.SR.AsyncMethodBuilder_InstanceNotInitialized);
                    break;
                case ExceptionResource.ConcurrentCollection_SyncRoot_NotSupported:
                    resourceName = nameof(System.SR.ConcurrentCollection_SyncRoot_NotSupported);
                    break;
                case ExceptionResource.InvalidOperation_IComparerFailed:
                    resourceName = nameof(System.SR.InvalidOperation_IComparerFailed);
                    break;
                case ExceptionResource.InvalidOperation_NullArray:
                    resourceName = nameof(System.SR.InvalidOperation_NullArray);
                    break;
                case ExceptionResource.InvalidOperation_WrongAsyncResultOrEndCalledMultiple:
                    resourceName = nameof(System.SR.InvalidOperation_WrongAsyncResultOrEndCalledMultiple);
                    break;
                case ExceptionResource.NotSupported_FixedSizeCollection:
                    resourceName = nameof(System.SR.NotSupported_FixedSizeCollection);
                    break;
                case ExceptionResource.NotSupported_KeyCollectionSet:
                    resourceName = nameof(System.SR.NotSupported_KeyCollectionSet);
                    break;
                case ExceptionResource.NotSupported_ReadOnlyCollection:
                    resourceName = nameof(System.SR.NotSupported_ReadOnlyCollection);
                    break;
                case ExceptionResource.NotSupported_StringComparison:
                    resourceName = nameof(System.SR.NotSupported_StringComparison);
                    break;
                case ExceptionResource.NotSupported_ValueCollectionSet:
                    resourceName = nameof(System.SR.NotSupported_ValueCollectionSet);
                    break;
                case ExceptionResource.Rank_MultiDimNotSupported:
                    resourceName = nameof(System.SR.Rank_MultiDimNotSupported);
                    break;
                case ExceptionResource.Serialization_MissingKeys:
                    resourceName = nameof(System.SR.Serialization_MissingKeys);
                    break;
                case ExceptionResource.Serialization_NullKey:
                    resourceName = nameof(System.SR.Serialization_NullKey);
                    break;
                case ExceptionResource.Task_ContinueWith_ESandLR:
                    resourceName = nameof(System.SR.Task_ContinueWith_ESandLR);
                    break;
                case ExceptionResource.Task_ContinueWith_NotOnAnything:
                    resourceName = nameof(System.SR.Task_ContinueWith_NotOnAnything);
                    break;
                case ExceptionResource.Task_Delay_InvalidDelay:
                    resourceName = nameof(System.SR.Task_Delay_InvalidDelay);
                    break;
                case ExceptionResource.Task_Delay_InvalidMillisecondsDelay:
                    resourceName = nameof(System.SR.Task_Delay_InvalidMillisecondsDelay);
                    break;
                case ExceptionResource.Task_Dispose_NotCompleted:
                    resourceName = nameof(System.SR.Task_Dispose_NotCompleted);
                    break;
                case ExceptionResource.Task_MultiTaskContinuation_EmptyTaskList:
                    resourceName = nameof(System.SR.Task_MultiTaskContinuation_EmptyTaskList);
                    break;
                case ExceptionResource.Task_MultiTaskContinuation_NullTask:
                    resourceName = nameof(System.SR.Task_MultiTaskContinuation_NullTask);
                    break;
                case ExceptionResource.Task_RunSynchronously_AlreadyStarted:
                    resourceName = nameof(System.SR.Task_RunSynchronously_AlreadyStarted);
                    break;
                case ExceptionResource.Task_RunSynchronously_Continuation:
                    resourceName = nameof(System.SR.Task_RunSynchronously_Continuation);
                    break;
                case ExceptionResource.Task_RunSynchronously_Promise:
                    resourceName = nameof(System.SR.Task_RunSynchronously_Promise);
                    break;
                case ExceptionResource.Task_RunSynchronously_TaskCompleted:
                    resourceName = nameof(System.SR.Task_RunSynchronously_TaskCompleted);
                    break;
                case ExceptionResource.Task_Start_AlreadyStarted:
                    resourceName = nameof(System.SR.Task_Start_AlreadyStarted);
                    break;
                case ExceptionResource.Task_Start_ContinuationTask:
                    resourceName = nameof(System.SR.Task_Start_ContinuationTask);
                    break;
                case ExceptionResource.Task_Start_Promise:
                    resourceName = nameof(System.SR.Task_Start_Promise);
                    break;
                case ExceptionResource.Task_Start_TaskCompleted:
                    resourceName = nameof(System.SR.Task_Start_TaskCompleted);
                    break;
                case ExceptionResource.Task_ThrowIfDisposed:
                    resourceName = nameof(System.SR.Task_ThrowIfDisposed);
                    break;
                case ExceptionResource.Task_WaitMulti_NullTask:
                    resourceName = nameof(System.SR.Task_WaitMulti_NullTask);
                    break;
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NoExceptions:
                    resourceName = nameof(System.SR.TaskCompletionSourceT_TrySetException_NoExceptions);
                    break;
                case ExceptionResource.TaskCompletionSourceT_TrySetException_NullException:
                    resourceName = nameof(System.SR.TaskCompletionSourceT_TrySetException_NullException);
                    break;
                case ExceptionResource.TaskT_TransitionToFinal_AlreadyCompleted:
                    resourceName = nameof(System.SR.TaskT_TransitionToFinal_AlreadyCompleted);
                    break;
                default:
                    Debug.Fail("The enum value is not defined, please check the ExceptionResource enum in ExceptionResource.cs.");
                    return "";
            }

            return System.SR.GetResourceString(resourceName, null);
        }
    }
}
/* */

// 