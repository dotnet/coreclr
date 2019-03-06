// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


// This file defines an internal class used to throw exceptions in BCL code.
// The main purpose is to reduce code size.
//
// The old way to throw an exception generates quite a lot IL code and assembly code.
// Following is an example:
//     C# source
//          throw new ArgumentNullException(nameof(key), SR.ArgumentNull_Key);
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

using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace System
{
    [StackTraceHidden]
    internal static class ThrowHelper
    {
        internal static void ThrowArrayTypeMismatchException()
        {
            throw new ArrayTypeMismatchException();
        }

        internal static void ThrowInvalidTypeWithPointersNotSupported(Type targetType)
        {
            throw new ArgumentException(SR.Format(SR.Argument_InvalidTypeWithPointersNotSupported, targetType));
        }

        internal static void ThrowIndexOutOfRangeException()
        {
            throw new IndexOutOfRangeException();
        }

        internal static void ThrowArgumentOutOfRangeException()
        {
            throw new ArgumentOutOfRangeException();
        }

        internal static void ThrowArgumentException_DestinationTooShort()
        {
            throw new ArgumentException(SR.Argument_DestinationTooShort);
        }

        internal static void ThrowArgumentException_OverlapAlignmentMismatch()
        {
            throw new ArgumentException(SR.Argument_OverlapAlignmentMismatch);
        }

        internal static void ThrowArgumentException_CannotExtractScalar(ExceptionArgument argument)
        {
            throw GetArgumentException(ExceptionResource.Argument_CannotExtractScalar, argument);
        }

        internal static void ThrowArgumentOutOfRange_IndexException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.index,
                                                    ExceptionResource.ArgumentOutOfRange_Index);
        }

        internal static void ThrowIndexArgumentOutOfRange_NeedNonNegNumException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.index,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        internal static void ThrowValueArgumentOutOfRange_NeedNonNegNumException()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.value,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        internal static void ThrowLengthArgumentOutOfRange_ArgumentOutOfRange_NeedNonNegNum()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.length,
                                                    ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
        }

        internal static void ThrowStartIndexArgumentOutOfRange_ArgumentOutOfRange_Index()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.startIndex,
                                                    ExceptionResource.ArgumentOutOfRange_Index);
        }

        internal static void ThrowCountArgumentOutOfRange_ArgumentOutOfRange_Count()
        {
            throw GetArgumentOutOfRangeException(ExceptionArgument.count,
                                                    ExceptionResource.ArgumentOutOfRange_Count);
        }

        internal static void ThrowWrongKeyTypeArgumentException<T>(T key, Type targetType)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetWrongKeyTypeArgumentException((object)key, targetType);
        }

        internal static void ThrowWrongValueTypeArgumentException<T>(T value, Type targetType)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetWrongValueTypeArgumentException((object)value, targetType);
        }

        private static ArgumentException GetAddingDuplicateWithKeyArgumentException(object key)
        {
            return new ArgumentException(SR.Format(SR.Argument_AddingDuplicateWithKey, key));
        }

        internal static void ThrowAddingDuplicateWithKeyArgumentException<T>(T key)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetAddingDuplicateWithKeyArgumentException((object)key);
        }

        internal static void ThrowKeyNotFoundException<T>(T key)
        {
            // Generic key to move the boxing to the right hand side of throw
            throw GetKeyNotFoundException((object)key);
        }

        internal static void ThrowArgumentException(ExceptionResource resource)
        {
            throw GetArgumentException(resource);
        }

        internal static void ThrowArgumentException(ExceptionResource resource, ExceptionArgument argument)
        {
            throw GetArgumentException(resource, argument);
        }

        private static ArgumentNullException GetArgumentNullException(ExceptionArgument argument)
        {
            return new ArgumentNullException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentNullException(ExceptionArgument argument)
        {
            throw GetArgumentNullException(argument);
        }

        internal static void ThrowArgumentNullException(ExceptionResource resource)
        {
            throw new ArgumentNullException(GetResourceString(resource));
        }

        internal static void ThrowArgumentNullException(ExceptionArgument argument, ExceptionResource resource)
        {
            throw new ArgumentNullException(GetArgumentName(argument), GetResourceString(resource));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument)
        {
            throw new ArgumentOutOfRangeException(GetArgumentName(argument));
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, resource);
        }

        internal static void ThrowArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource)
        {
            throw GetArgumentOutOfRangeException(argument, paramNumber, resource);
        }

        internal static void ThrowInvalidOperationException()
        {
            throw new InvalidOperationException();
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource)
        {
            throw GetInvalidOperationException(resource);
        }

        internal static void ThrowInvalidOperationException_OutstandingReferences()
        {
            throw new InvalidOperationException(SR.Memory_OutstandingReferences);
        }

        internal static void ThrowInvalidOperationException(ExceptionResource resource, Exception e)
        {
            throw new InvalidOperationException(GetResourceString(resource), e);
        }

        internal static void ThrowSerializationException(ExceptionResource resource)
        {
            throw new SerializationException(GetResourceString(resource));
        }

        internal static void ThrowSecurityException(ExceptionResource resource)
        {
            throw new System.Security.SecurityException(GetResourceString(resource));
        }

        internal static void ThrowRankException(ExceptionResource resource)
        {
            throw new RankException(GetResourceString(resource));
        }

        internal static void ThrowNotSupportedException(ExceptionResource resource)
        {
            throw new NotSupportedException(GetResourceString(resource));
        }

        internal static void ThrowUnauthorizedAccessException(ExceptionResource resource)
        {
            throw new UnauthorizedAccessException(GetResourceString(resource));
        }

        internal static void ThrowObjectDisposedException(string objectName, ExceptionResource resource)
        {
            throw new ObjectDisposedException(objectName, GetResourceString(resource));
        }

        internal static void ThrowObjectDisposedException(ExceptionResource resource)
        {
            throw new ObjectDisposedException(null, GetResourceString(resource));
        }

        internal static void ThrowNotSupportedException()
        {
            throw new NotSupportedException();
        }

        internal static void ThrowAggregateException(List<Exception> exceptions)
        {
            throw new AggregateException(exceptions);
        }

        internal static void ThrowOutOfMemoryException()
        {
            throw new OutOfMemoryException();
        }

        internal static void ThrowArgumentException_Argument_InvalidArrayType()
        {
            throw new ArgumentException(SR.Argument_InvalidArrayType);
        }

        internal static void ThrowInvalidOperationException_InvalidOperation_EnumNotStarted()
        {
            throw new InvalidOperationException(SR.InvalidOperation_EnumNotStarted);
        }

        internal static void ThrowInvalidOperationException_InvalidOperation_EnumEnded()
        {
            throw new InvalidOperationException(SR.InvalidOperation_EnumEnded);
        }

        internal static void ThrowInvalidOperationException_EnumCurrent(int index)
        {
            throw GetInvalidOperationException_EnumCurrent(index);
        }

        internal static void ThrowInvalidOperationException_InvalidOperation_EnumFailedVersion()
        {
            throw new InvalidOperationException(SR.InvalidOperation_EnumFailedVersion);
        }

        internal static void ThrowInvalidOperationException_InvalidOperation_EnumOpCantHappen()
        {
            throw new InvalidOperationException(SR.InvalidOperation_EnumOpCantHappen);
        }

        internal static void ThrowInvalidOperationException_InvalidOperation_NoValue()
        {
            throw new InvalidOperationException(SR.InvalidOperation_NoValue);
        }

        internal static void ThrowInvalidOperationException_ConcurrentOperationsNotSupported()
        {
            throw new InvalidOperationException(SR.InvalidOperation_ConcurrentOperationsNotSupported);
        }

        internal static void ThrowInvalidOperationException_HandleIsNotInitialized()
        {
            throw new InvalidOperationException(SR.InvalidOperation_HandleIsNotInitialized);
        }

        internal static void ThrowInvalidOperationException_HandleIsNotPinned()
        {
            throw new InvalidOperationException(SR.InvalidOperation_HandleIsNotPinned);
        }

        internal static void ThrowArraySegmentCtorValidationFailedExceptions(Array array, int offset, int count)
        {
            throw GetArraySegmentCtorValidationFailedException(array, offset, count);
        }

        internal static void ThrowFormatException_BadFormatSpecifier()
        {
            throw new FormatException(SR.Argument_BadFormatSpecifier);
        }

        internal static void ThrowArgumentOutOfRangeException_PrecisionTooLarge()
        {
            throw new ArgumentOutOfRangeException("precision", SR.Format(SR.Argument_PrecisionTooLarge, StandardFormat.MaxPrecision));
        }

        internal static void ThrowArgumentOutOfRangeException_SymbolDoesNotFit()
        {
            throw new ArgumentOutOfRangeException("symbol", SR.Argument_BadFormatSpecifier);
        }

        private static Exception GetArraySegmentCtorValidationFailedException(Array array, int offset, int count)
        {
            if (array == null)
                return new ArgumentNullException(nameof(array));
            if (offset < 0)
                return new ArgumentOutOfRangeException(nameof(offset), SR.ArgumentOutOfRange_NeedNonNegNum);
            if (count < 0)
                return new ArgumentOutOfRangeException(nameof(count), SR.ArgumentOutOfRange_NeedNonNegNum);

            Debug.Assert(array.Length - offset < count);
            return new ArgumentException(SR.Argument_InvalidOffLen);
        }

        private static ArgumentException GetArgumentException(ExceptionResource resource)
        {
            return new ArgumentException(GetResourceString(resource));
        }

        private static InvalidOperationException GetInvalidOperationException(ExceptionResource resource)
        {
            return new InvalidOperationException(GetResourceString(resource));
        }

        private static ArgumentException GetWrongKeyTypeArgumentException(object key, Type targetType)
        {
            return new ArgumentException(SR.Format(SR.Arg_WrongType, key, targetType), nameof(key));
        }

        private static ArgumentException GetWrongValueTypeArgumentException(object value, Type targetType)
        {
            return new ArgumentException(SR.Format(SR.Arg_WrongType, value, targetType), nameof(value));
        }

        private static KeyNotFoundException GetKeyNotFoundException(object key)
        {
            return new KeyNotFoundException(SR.Format(SR.Arg_KeyNotFoundWithKey, key));
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, ExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument), GetResourceString(resource));
        }

        private static ArgumentException GetArgumentException(ExceptionResource resource, ExceptionArgument argument)
        {
            return new ArgumentException(GetResourceString(resource), GetArgumentName(argument));
        }

        private static ArgumentOutOfRangeException GetArgumentOutOfRangeException(ExceptionArgument argument, int paramNumber, ExceptionResource resource)
        {
            return new ArgumentOutOfRangeException(GetArgumentName(argument) + "[" + paramNumber.ToString() + "]", GetResourceString(resource));
        }

        private static InvalidOperationException GetInvalidOperationException_EnumCurrent(int index)
        {
            return new InvalidOperationException(
                index < 0 ?
                SR.InvalidOperation_EnumNotStarted :
                SR.InvalidOperation_EnumEnded);
        }

        // Allow nulls for reference types and Nullable<U>, but not for value types.
        // Aggressively inline so the jit evaluates the if in place and either drops the call altogether
        // Or just leaves null test and call to the Non-returning ThrowHelper.ThrowArgumentNullException
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void IfNullAndNullsAreIllegalThenThrow<T>(object value, ExceptionArgument argName)
        {
            // Note that default(T) is not equal to null for value types except when T is Nullable<U>.
            if (!(default(T) == null) && value == null)
                ThrowHelper.ThrowArgumentNullException(argName);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal static void ThrowForUnsupportedVectorBaseType<T>() where T : struct
        {
            if (typeof(T) != typeof(byte) && typeof(T) != typeof(sbyte) &&
                typeof(T) != typeof(short) && typeof(T) != typeof(ushort) &&
                typeof(T) != typeof(int) && typeof(T) != typeof(uint) &&
                typeof(T) != typeof(long) && typeof(T) != typeof(ulong) &&
                typeof(T) != typeof(float) && typeof(T) != typeof(double))
            {
                ThrowNotSupportedException(ExceptionResource.Arg_TypeNotSupported);
            }
        }

#if false // Reflection-based implementation does not work for CoreRT/ProjectN
        // This function will convert an ExceptionArgument enum value to the argument name string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetArgumentName(ExceptionArgument argument)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionArgument), argument),
                "The enum value is not defined, please check the ExceptionArgument Enum.");

            return argument.ToString();
        }

        // This function will convert an ExceptionResource enum value to the resource string.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static string GetResourceString(ExceptionResource resource)
        {
            Debug.Assert(Enum.IsDefined(typeof(ExceptionResource), resource),
                "The enum value is not defined, please check the ExceptionResource Enum.");

            return SR.GetResourceString(resource.ToString());
        }
#endif
    }
}
