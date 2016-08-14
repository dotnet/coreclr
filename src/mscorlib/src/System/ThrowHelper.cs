// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


namespace System {
    // This file defines an internal class used to throw exceptions in BCL code.
    // The main purpose is to reduce code size. 
    // 
    // The old way to throw an exception generates quite a lot IL code and assembly code.
    // Following is an example:
    //     C# source
    //          throw new ArgumentNullException("key", Environment.GetResourceString("ArgumentNull_Key"));
    //     IL code:
    //          IL_0003:  ldstr      "key"
    //          IL_0008:  ldstr      "ArgumentNull_Key"
    //          IL_000d:  call       string System.Environment::GetResourceString(string)
    //          IL_0012:  newobj     instance void System.ArgumentNullException::.ctor(string,string)
    //          IL_0017:  throw
    //    which is 21bytes in IL.
    // 
    // So we want to get rid of the ldstr and call to Environment.GetResource in IL.
    // In order to do that, I created two enums: ExceptionResource, ExceptionArgument to represent the
    // argument name and resource name in a small integer. The source code will be changed to 
    //    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key, ExceptionResource.ArgumentNull_Key);
    //
    // The IL code will be 7 bytes.
    //    IL_0008:  ldc.i4.4
    //    IL_0009:  ldc.i4.4
    //    IL_000a:  call       void System.ThrowHelper::ThrowArgumentNullException(valuetype System.ExceptionArgument)
    //    IL_000f:  ldarg.0
    //
    // This will also reduce the Jitted code size a lot. 
    //
    // It is very important we do this for generic classes because we can easily generate the same code 
    // multiple times for different instantiation. 
    // 

    using System.Runtime.CompilerServices;
    using System.Runtime.Serialization;
    using System.Diagnostics.Contracts;
    using Collections.Generic;

    [Pure]
    internal static class ThrowHelper
    {
        internal static void ThrowIndexArgumentOutOfRange_IndexException()
        {
            throw new ArgumentOutOfRangeException(
                        GetArgumentName(ExceptionArgument.index),
                        Environment.GetResourceString(GetResourceName(ExceptionResource.ArgumentOutOfRange_Index)));
        }

        internal static void ThrowIndexArgumentOutOfRange_NeedNonNegNumException()
        {
            throw new ArgumentOutOfRangeException(
                        GetArgumentName(ExceptionArgument.index),
                        Environment.GetResourceString(GetResourceName(ExceptionResource.ArgumentOutOfRange_NeedNonNegNum)));
        }

        internal static void ThrowWrongKeyTypeArgumentException(object key, Type targetType) {
            throw new ArgumentException(Environment.GetResourceString(GetResourceName(ExceptionResource.Arg_WrongType), key, targetType), GetArgumentName(ExceptionArgument.key));
        }

        internal static void ThrowWrongValueTypeArgumentException(object value, Type targetType) {
            throw new ArgumentException(Environment.GetResourceString(GetResourceName(ExceptionResource.Arg_WrongType), value, targetType), GetArgumentName(ExceptionArgument.value));
        }

#if FEATURE_CORECLR
        internal static void ThrowAddingDuplicateWithKeyArgumentException(object key) {
            throw new ArgumentException(Environment.GetResourceString(GetResourceName(ExceptionResource.Argument_AddingDuplicateWithKey), key));
        }
#endif

        internal static void ThrowKeyNotFoundException() {
            throw new System.Collections.Generic.KeyNotFoundException();
        }
        
        internal static void ThrowArgumentException(ExceptionResource resource) {
            throw new ArgumentException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowArgumentException(ExceptionResource resource, ExceptionArgument argument) {
            throw new ArgumentException(Environment.GetResourceString(GetResourceName(resource)), GetArgumentName(argument));
        }

        internal static void ThrowArgumentNullException(ExceptionArgument argument) {
            throw new ArgumentNullException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentNullException(ExceptionResource resource)
        {
            throw new ArgumentNullException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowFormatException(ExceptionResource resource)
        {
            throw new FormatException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument) {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource) {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument),
                                                    Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument) + "[" + paramNumber.ToString()+ "]",
                                                    Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw new InvalidOperationException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource, string value)
        {
            throw new InvalidOperationException(Environment.GetResourceString(GetResourceName(resource), value));
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e)
        {
            throw new InvalidOperationException(Environment.GetResourceString(GetResourceName(resource)), e);
        }

        internal static void ThrowSerializationException(ExceptionResource resource) {
            throw new SerializationException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowSerializationException(ExceptionResource resource, string memberName)
        {
            throw new SerializationException(Environment.GetResourceString(GetResourceName(resource), memberName));
        }

        internal static void ThrowRankException(ExceptionResource resource)
        {
            throw new RankException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void  ThrowSecurityException(ExceptionResource resource) {
            throw new System.Security.SecurityException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowNotSupportedException()
        {
            throw new NotSupportedException();
        }

        internal static void ThrowNotSupportedException(ExceptionResource resource) {
            throw new NotSupportedException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowUnauthorizedAccessException(ExceptionResource resource) {
            throw new UnauthorizedAccessException(Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowObjectDisposedException(string objectName, ExceptionResource resource) {
            throw new ObjectDisposedException(objectName, Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowObjectDisposedException(ExceptionResource resource)
        {
            throw new ObjectDisposedException(null, Environment.GetResourceString(GetResourceName(resource)));
        }

        internal static void ThrowTypeLoadException(ExceptionResource resource, ExceptionArgument argument)
        {
            throw new TypeLoadException(Environment.GetResourceString(GetResourceName(resource), GetArgumentName(argument)));
        }

        internal static void ThrowNotImplementedException()
        {
            throw new NotImplementedException();
        }

        internal static void ThrowAggregateException(IEnumerable<Exception> innerExceptions)
        {
            throw new AggregateException(innerExceptions);
        }



        // Allow nulls for reference types and Nullable<U>, but not for value types.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argument) {
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>. 
            if (!(default(T) == null) && value == null)
                ThrowHelper.ThrowArgumentNullException(argument);
        }

        //
        // This function will convert an ExceptionArgument enum value to the argument name string.
        //
        internal static string GetArgumentName(ExceptionArgument argument)
        {
            Contract.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument),
                "The enum value is not defined, please check the ExceptionArgument Enum.");

            return argument.ToString();
        }

        //
        // This function will convert an ExceptionResource enum value to the resource string.
        //
        internal static string GetResourceName(ExceptionResource resource)
        {
            Contract.Assert(Enum.IsDefined(typeof(ExceptionResource), resource),
                "The enum value is not defined, please check the ExceptionResource Enum.");

            return resource.ToString();
        }
    }

    //
    // The convention for this enum is using the argument name as the enum name
    // 
    internal enum ExceptionArgument {
        obj,
        dictionary,
        dictionaryCreationThreshold,
        array,
        info,
        key,
        collection,
        list,
        match,
        converter,
        queue,
        stack,
        capacity,
        index,
        startIndex,
        value,
        count,
        arrayIndex,
        name,
        mode,
        item,
        options,
        view,
        sourceBytesToCopy,
        action,
        comparison,
        keys,
        elementType,
        newSize,
        len,
        length,
        length1,
        length2,
        length3,
        lengths,
        sourceIndex,
        destinationIndex,
        index1,
        index2,
        index3,
        indices,
        offset,
        lowerBounds,
        sourceArray,
        destinationArray,
        comparer,
        endIndex,
        other,
        function,
        scheduler,
        continuationAction,
        creationOptions,
        observer,
        continuationFunction,
        valueFactory,
        handler,
        typeName,
        culture,
        addValueFactory,
        updateValueFactory,
        concurrencyLevel,
        items,
        source,
        partitionerOptions,
        toExclusive,
        rangeSize,
        partitionCount,
        SymWriter,
        path,
        buffer,
        asyncResult,
        bufferSize,
        access,
        share,
        endMethod,
        beginMethod,
        endFunction,
        tasks,
        exception,
        timeout,
        millisecondsTimeout,
        cancellationToken,
        continuationOptions,
        delay,
        millisecondsDelay,
        exceptions,
    }

    //
    // The convention for this enum is using the resource name as the enum name
    // 
    internal enum ExceptionResource {
        Argument_ImplementIComparable,
        Argument_InvalidType,     
        Argument_InvalidArgumentForComparison,
        Argument_InvalidRegistryKeyPermissionCheck,        
        ArgumentOutOfRange_NeedNonNegNum,
        
        Arg_ArrayPlusOffTooSmall,
        Arg_NonZeroLowerBound,        
        Arg_RankMultiDimNotSupported,        
        Arg_RegKeyDelHive,
        Arg_RegKeyStrLenBug,  
        Arg_RegSetStrArrNull,
        Arg_RegSetMismatchedKind,
        Arg_RegSubKeyAbsent,        
        Arg_RegSubKeyValueAbsent,
        
        Argument_AddingDuplicate,
        Serialization_InvalidOnDeser,
        Serialization_MissingKeys,
        Serialization_NullKey,
        Argument_InvalidArrayType,
        NotSupported_KeyCollectionSet,
        NotSupported_ValueCollectionSet,
        ArgumentOutOfRange_SmallCapacity,
        ArgumentOutOfRange_Index,
        Argument_InvalidOffLen,
        Argument_ItemNotExist,
        ArgumentOutOfRange_Count,
        ArgumentOutOfRange_InvalidThreshold,
        ArgumentOutOfRange_ListInsert,
        NotSupported_ReadOnlyCollection,
        InvalidOperation_CannotRemoveFromStackOrQueue,
        InvalidOperation_EmptyQueue,
        InvalidOperation_EnumOpCantHappen,
        InvalidOperation_EnumFailedVersion,
        InvalidOperation_EmptyStack,
        ArgumentOutOfRange_BiggerThanCollection,
        InvalidOperation_EnumNotStarted,
        InvalidOperation_EnumEnded,
        NotSupported_SortedListNestedWrite,
        InvalidOperation_NoValue,
        InvalidOperation_RegRemoveSubKey,
        Security_RegistryPermission,
        UnauthorizedAccess_RegistryNoWrite,
        ObjectDisposed_RegKeyClosed,
        NotSupported_InComparableType,
        Argument_InvalidRegistryOptionsCheck,
        Argument_InvalidRegistryViewCheck,

        InvalidOperation_IComparerFailed,
        ArgumentOutOfRange_HugeArrayNotSupported,
        NotSupported_FixedSizeCollection,
        Arg_MustBeType,
        InvalidOperation_NullArray,
        Arg_NeedAtLeast1Rank,
        Arg_RanksAndBounds,
        Arg_RankIndices,
        Arg_Need1DArray,
        Arg_Need2DArray,
        Arg_Need3DArray,
        ArgumentException_OtherNotArrayOfCorrectLength,
        Rank_MultiDimNotSupported,
        ArgumentOutOfRange_EndIndexStartIndex,
        Arg_LowerBoundsMustMatch,
        Arg_BogusIComparer,

        Arg_WrongType,
        Argument_AddingDuplicateWithKey,

        TaskT_ctor_SelfReplicating,

        Lazy_ctor_ModeInvalid,
        Lazy_Value_RecursiveCallsToValue,

        ConcurrentDictionary_SourceContainsDuplicateKeys,
        ConcurrentDictionary_ArrayNotLargeEnough,
        ConcurrentDictionary_KeyAlreadyExisted,
        ConcurrentDictionary_TypeOfKeyIncorrect,
        ConcurrentDictionary_TypeOfValueIncorrect,
        ConcurrentDictionary_ArrayIncorrectType,
        ConcurrentCollection_SyncRoot_NotSupported,
        ConcurrentDictionary_ConcurrencyLevelMustBePositive,
        ConcurrentDictionary_CapacityMustNotBeNegative,
        ConcurrentDictionary_IndexIsNegative,
        ConcurrentDictionary_ItemKeyIsNull,

        ConcurrentStack_PushPopRange_InvalidCount,
        ConcurrentStack_PushPopRange_CountOutOfRange,
        ConcurrentStack_PushPopRange_StartOutOfRange,

        Partitioner_DynamicPartitionsNotSupported,
        OrderablePartitioner_GetPartitions_WrongNumberOfPartitions,
        PartitionerStatic_CanNotCallGetEnumeratorAfterSourceHasBeenDisposed,
        PartitionerStatic_CurrentCalledBeforeMoveNext,

        NotSupported_Constructor,
        Format_NeedSingleChar,
        NotSupported_Method,
        Argument_UnclosedExceptionBlock,
        InvalidOperation_AssemblyHasBeenSaved,
        MissingType,
        Serialization_InsufficientState,
        Serialization_NullSignature,
        Serialization_BadParameterInfo,
        Serialization_NoParameterInfo,
        Serialization_UnknownMember,

        InvalidOperation_WithoutARM,

        ArgumentNull_Path,
        Argument_EmptyPath,
        ArgumentNull_Buffer,
        ArgumentOutOfRange_Enum,
        ArgumentOutOfRange_NeedPosNum,

        Task_MultiTaskContinuation_EmptyTaskList,
        InvalidOperation_WrongAsyncResultOrEndCalledMultiple,
        Task_MultiTaskContinuation_NullTask,
        Task_Start_TaskCompleted,
        Task_Start_Promise,
        Task_Start_ContinuationTask,
        Task_Start_AlreadyStarted,
        Task_Dispose_NotCompleted,
        Task_RunSynchronously_TaskCompleted,
        Task_RunSynchronously_Continuation,
        Task_RunSynchronously_Promise,
        Task_RunSynchronously_AlreadyStarted,
        Task_ctor_LRandSR,
        Task_ThrowIfDisposed,
        Task_WaitMulti_NullTask,
        Task_ContinueWith_NotOnAnything,
        Task_ContinueWith_ESandLR,
        Task_Delay_InvalidDelay,
        Task_Delay_InvalidMillisecondsDelay,
        TaskT_TransitionToFinal_AlreadyCompleted,
        TaskCompletionSourceT_TrySetException_NullException,
        TaskCompletionSourceT_TrySetException_NoExceptions

    }
}

