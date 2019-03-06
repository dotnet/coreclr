// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// <#@ include file="ThrowHelper.ttinclude" #>


/* ---<# /*
 * 
 * 
 * 
 * 
 * !! READ ME !!
 * 
 * 
 * This file - ExceptionResource.cs - is a T4 template which contains the definition of the
 * ExceptionResource enum type and coordinates generation of the code-behind file. Other
 * infrastructure code exists in ThrowHelper.ttinclude.
 * 
 * To add a value to the enum, make a change to the type as defined in this file (it starts just
 * after the T4 goop below). Save this file, and build tooling will regenerate the
 * ExceptionResource.Generated.cs file.
 * 
 * 
 * 
 * 
 */ // #> */
// <# WriteAutogenWarning(nameof(ExceptionResource)); #>
/* --- <#= "*" #><#= "/" #>

using System.Diagnostics;

namespace System
{
<# WriteEnumType(nameof(ExceptionResource), typeof(ExceptionResource)); #>

<# WriteThrowHelperGetResourceString(typeof(ExceptionResource)); #>
}
<#= "/" #><#= "*" #> */

// <#+ /*
namespace System
{
    // */ #><#+

    //
    // The convention for this enum is using the resource name as the enum name
    //
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
    // #><#+ /*
}
// */ #>
