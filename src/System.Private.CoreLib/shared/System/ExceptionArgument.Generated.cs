// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// 

/* --- */
// !! THIS FILE IS AUTO-GENERATED !!
// Any changes you make directly to this file will be overwritten by build tooling.
// Instead, make changes to ExceptionArgument.cs (see comment at the top of that file)."
/* --- */

using System.Diagnostics;

namespace System
{
    internal enum ExceptionArgument
    {
        action,
        array,
        arrayIndex,
        asyncResult,
        beginMethod,
        callBack,
        cancellationToken,
        capacity,
        ch,
        collection,
        comparable,
        comparer,
        comparison,
        comparisonType,
        continuationAction,
        continuationFunction,
        continuationOptions,
        converter,
        count,
        creationOptions,
        culture,
        delay,
        destinationArray,
        destinationIndex,
        dictionary,
        elementType,
        endFunction,
        endIndex,
        endMethod,
        exception,
        exceptions,
        format,
        function,
        index,
        index1,
        index2,
        index3,
        indices,
        info,
        input,
        item,
        key,
        keys,
        len,
        length,
        length1,
        length2,
        length3,
        lengths,
        list,
        lowerBounds,
        manager,
        match,
        millisecondsDelay,
        millisecondsTimeout,
        newSize,
        obj,
        other,
        ownedMemory,
        pHandle,
        pointer,
        s,
        scheduler,
        source,
        sourceArray,
        sourceBytesToCopy,
        sourceIndex,
        start,
        startIndex,
        state,
        stateMachine,
        task,
        tasks,
        text,
        timeout,
        type,
        value,
        values,
    }

    internal static partial class ThrowHelper
    {
        private static string GetArgumentName(ExceptionArgument argument)
        {
            switch (argument)
            {
                case ExceptionArgument.action:
                    return "action";
                case ExceptionArgument.array:
                    return "array";
                case ExceptionArgument.arrayIndex:
                    return "arrayIndex";
                case ExceptionArgument.asyncResult:
                    return "asyncResult";
                case ExceptionArgument.beginMethod:
                    return "beginMethod";
                case ExceptionArgument.callBack:
                    return "callBack";
                case ExceptionArgument.cancellationToken:
                    return "cancellationToken";
                case ExceptionArgument.capacity:
                    return "capacity";
                case ExceptionArgument.ch:
                    return "ch";
                case ExceptionArgument.collection:
                    return "collection";
                case ExceptionArgument.comparable:
                    return "comparable";
                case ExceptionArgument.comparer:
                    return "comparer";
                case ExceptionArgument.comparison:
                    return "comparison";
                case ExceptionArgument.comparisonType:
                    return "comparisonType";
                case ExceptionArgument.continuationAction:
                    return "continuationAction";
                case ExceptionArgument.continuationFunction:
                    return "continuationFunction";
                case ExceptionArgument.continuationOptions:
                    return "continuationOptions";
                case ExceptionArgument.converter:
                    return "converter";
                case ExceptionArgument.count:
                    return "count";
                case ExceptionArgument.creationOptions:
                    return "creationOptions";
                case ExceptionArgument.culture:
                    return "culture";
                case ExceptionArgument.delay:
                    return "delay";
                case ExceptionArgument.destinationArray:
                    return "destinationArray";
                case ExceptionArgument.destinationIndex:
                    return "destinationIndex";
                case ExceptionArgument.dictionary:
                    return "dictionary";
                case ExceptionArgument.elementType:
                    return "elementType";
                case ExceptionArgument.endFunction:
                    return "endFunction";
                case ExceptionArgument.endIndex:
                    return "endIndex";
                case ExceptionArgument.endMethod:
                    return "endMethod";
                case ExceptionArgument.exception:
                    return "exception";
                case ExceptionArgument.exceptions:
                    return "exceptions";
                case ExceptionArgument.format:
                    return "format";
                case ExceptionArgument.function:
                    return "function";
                case ExceptionArgument.index:
                    return "index";
                case ExceptionArgument.index1:
                    return "index1";
                case ExceptionArgument.index2:
                    return "index2";
                case ExceptionArgument.index3:
                    return "index3";
                case ExceptionArgument.indices:
                    return "indices";
                case ExceptionArgument.info:
                    return "info";
                case ExceptionArgument.input:
                    return "input";
                case ExceptionArgument.item:
                    return "item";
                case ExceptionArgument.key:
                    return "key";
                case ExceptionArgument.keys:
                    return "keys";
                case ExceptionArgument.len:
                    return "len";
                case ExceptionArgument.length:
                    return "length";
                case ExceptionArgument.length1:
                    return "length1";
                case ExceptionArgument.length2:
                    return "length2";
                case ExceptionArgument.length3:
                    return "length3";
                case ExceptionArgument.lengths:
                    return "lengths";
                case ExceptionArgument.list:
                    return "list";
                case ExceptionArgument.lowerBounds:
                    return "lowerBounds";
                case ExceptionArgument.manager:
                    return "manager";
                case ExceptionArgument.match:
                    return "match";
                case ExceptionArgument.millisecondsDelay:
                    return "millisecondsDelay";
                case ExceptionArgument.millisecondsTimeout:
                    return "millisecondsTimeout";
                case ExceptionArgument.newSize:
                    return "newSize";
                case ExceptionArgument.obj:
                    return "obj";
                case ExceptionArgument.other:
                    return "other";
                case ExceptionArgument.ownedMemory:
                    return "ownedMemory";
                case ExceptionArgument.pHandle:
                    return "pHandle";
                case ExceptionArgument.pointer:
                    return "pointer";
                case ExceptionArgument.s:
                    return "s";
                case ExceptionArgument.scheduler:
                    return "scheduler";
                case ExceptionArgument.source:
                    return "source";
                case ExceptionArgument.sourceArray:
                    return "sourceArray";
                case ExceptionArgument.sourceBytesToCopy:
                    return "sourceBytesToCopy";
                case ExceptionArgument.sourceIndex:
                    return "sourceIndex";
                case ExceptionArgument.start:
                    return "start";
                case ExceptionArgument.startIndex:
                    return "startIndex";
                case ExceptionArgument.state:
                    return "state";
                case ExceptionArgument.stateMachine:
                    return "stateMachine";
                case ExceptionArgument.task:
                    return "task";
                case ExceptionArgument.tasks:
                    return "tasks";
                case ExceptionArgument.text:
                    return "text";
                case ExceptionArgument.timeout:
                    return "timeout";
                case ExceptionArgument.type:
                    return "type";
                case ExceptionArgument.value:
                    return "value";
                case ExceptionArgument.values:
                    return "values";
                default:
                    Debug.Fail("The enum value is not defined, please check the ExceptionArgument enum in ExceptionArgument.cs.");
                    return "";
            }
        }
    }
}
/* */

// 