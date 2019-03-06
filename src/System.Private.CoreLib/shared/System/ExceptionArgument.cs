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
 * This file - ExceptionArgument.cs - is a T4 template which contains the definition of the
 * ExceptionArgument enum type and coordinates generation of the code-behind file. Other
 * infrastructure code exists in ThrowHelper.ttinclude.
 * 
 * To add a value to the enum, make a change to the type as defined in this file (it starts just
 * after the T4 goop below). Save this file, and build tooling will regenerate the
 * ExceptionArgument.Generated.cs file.
 * 
 * 
 * 
 * 
 */ // #> */
// <# WriteAutogenWarning(nameof(ExceptionArgument)); #>
/* --- <#= "*" #><#= "/" #>

using System.Diagnostics;

namespace System
{
<# WriteEnumType(nameof(ExceptionArgument), typeof(ExceptionArgument)); #>

<# WriteThrowHelperGetArgumentName(typeof(ExceptionArgument)); #>
}
<#= "/" #><#= "*" #> */

// <#+ /*
namespace System
{
    // */ #><#+

    //
    // The convention for this enum is using the argument name as the enum name
    //
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
    // #><#+ /*
}
// */ #>
