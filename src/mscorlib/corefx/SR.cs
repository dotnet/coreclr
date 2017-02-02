using System;
using System.Globalization;

// CoreFX creates SR in the System namespace. While putting the CoreCLR SR adapter in the root
// may be unconventional, it allows us to keep the shared code identical.

internal static class SR 
{

    public static string ArgumentOutOfRange_Enum
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_Enum"); }
    }

    public static string ArgumentOutOfRange_NeedNonNegNum
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_NeedNonNegNum"); }
    }

    public static string ArgumentOutOfRange_NeedPosNum
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_NeedPosNum"); }
    }

    public static string InvalidOperation_EnumEnded
    {
        get { return Environment.GetResourceString("InvalidOperation_EnumEnded"); }
    }

    public static string InvalidOperation_EnumNotStarted
    {
        get { return Environment.GetResourceString("InvalidOperation_EnumNotStarted"); }
    }

    public static string InvalidOperation_ReadOnly
    {
        get { return Environment.GetResourceString("InvalidOperation_ReadOnly"); }
    }

    public static string Arg_InvalidHandle
    {
        get { return Environment.GetResourceString("Arg_InvalidHandle"); }
    }

    public static string ObjectDisposed_FileClosed
    {
        get { return Environment.GetResourceString("ObjectDisposed_FileClosed"); }
    }

    public static string Arg_HandleNotAsync
    {
        get { return Environment.GetResourceString("Arg_HandleNotAsync"); }
    }

    public static string ArgumentNull_Path
    {
        get { return Environment.GetResourceString("ArgumentNull_Path"); }
    }

    public static string Argument_EmptyPath
    {
        get { return Environment.GetResourceString("Argument_EmptyPath"); }
    }

    public static string Argument_InvalidFileModeAndAccessCombo
    {
        get { return Environment.GetResourceString("Argument_InvalidFileMode&AccessCombo"); }
    }

    public static string Argument_InvalidAppendMode
    {
        get { return Environment.GetResourceString("Argument_InvalidAppendMode"); }
    }

    public static string ArgumentNull_Buffer
    {
        get { return Environment.GetResourceString("ArgumentNull_Buffer"); }
    }

    public static string Argument_InvalidOffLen
    {
        get { return Environment.GetResourceString("Argument_InvalidOffLen"); }
    }

    public static string IO_UnknownFileName
    {
        get { return Environment.GetResourceString("IO_UnknownFileName"); }
    }

    public static string IO_FileStreamHandlePosition
    {
        get { return Environment.GetResourceString("IO.IO_FileStreamHandlePosition"); }
    }

    public static string NotSupported_FileStreamOnNonFiles
    {
        get { return Environment.GetResourceString("NotSupported_FileStreamOnNonFiles"); }
    }

    public static string IO_BindHandleFailed
    {
        get { return Environment.GetResourceString("IO.IO_BindHandleFailed"); }
    }

    public static string Arg_HandleNotSync
    {
        get { return Environment.GetResourceString("Arg_HandleNotSync"); }
    }

    public static string IO_SetLengthAppendTruncate
    {
        get { return Environment.GetResourceString("IO.IO_SetLengthAppendTruncate"); }
    }

    public static string ArgumentOutOfRange_FileLengthTooBig
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_FileLengthTooBig"); }
    }

    public static string Argument_InvalidSeekOrigin
    {
        get { return Environment.GetResourceString("Argument_InvalidSeekOrigin"); }
    }

    public static string IO_SeekAppendOverwrite
    {
        get { return Environment.GetResourceString("IO.IO_SeekAppendOverwrite"); }
    }

    public static string IO_FileTooLongOrHandleNotSync
    {
        get { return Environment.GetResourceString("IO_FileTooLongOrHandleNotSync"); }
    }

    public static string IndexOutOfRange_IORaceCondition
    {
        get { return Environment.GetResourceString("IndexOutOfRange_IORaceCondition"); }
    }

    public static string IO_FileNotFound
    {
        get { return Environment.GetResourceString("IO.FileNotFound"); }
    }

    public static string IO_FileNotFound_FileName
    {
        get { return Environment.GetResourceString("IO.FileNotFound_FileName"); }
    }

    public static string IO_PathNotFound_NoPathName
    {
        get { return Environment.GetResourceString("IO.PathNotFound_NoPathName"); }
    }

    public static string IO_PathNotFound_Path
    {
        get { return Environment.GetResourceString("IO.PathNotFound_Path"); }
    }

    public static string UnauthorizedAccess_IODenied_NoPathName
    {
        get { return Environment.GetResourceString("UnauthorizedAccess_IODenied_NoPathName"); }
    }

    public static string UnauthorizedAccess_IODenied_Path
    {
        get { return Environment.GetResourceString("UnauthorizedAccess_IODenied_Path"); }
    }

    public static string IO_AlreadyExists_Name
    {
        get { return Environment.GetResourceString("IO.IO_AlreadyExists_Name"); }
    }

    public static string IO_PathTooLong
    {
        get { return Environment.GetResourceString("IO.PathTooLong"); }
    }

    public static string IO_SharingViolation_NoFileName
    {
        get { return Environment.GetResourceString("IO.IO_SharingViolation_NoFileName"); }
    }

    public static string IO_SharingViolation_File
    {
        get { return Environment.GetResourceString("IO.IO_SharingViolation_File"); }
    }

    public static string IO_FileExists_Name
    {
        get { return Environment.GetResourceString("IO.IO_FileExists_Name"); }
    }

    public static string NotSupported_UnwritableStream
    {
        get { return Environment.GetResourceString("NotSupported_UnwritableStream"); }
    }

    public static string NotSupported_UnreadableStream
    {
        get { return Environment.GetResourceString("NotSupported_UnreadableStream"); }
    }

    public static string NotSupported_UnseekableStream
    {
        get { return Environment.GetResourceString("NotSupported_UnseekableStream"); }
    }

    public static string IO_EOF_ReadBeyondEOF
    {
        get { return Environment.GetResourceString("IO.EOF_ReadBeyondEOF"); }
    }

    public static string Argument_InvalidHandle
    {
        get { return Environment.GetResourceString("Argument_InvalidHandle"); }
    }

    public static string Argument_AlreadyBoundOrSyncHandle
    {
        get { return Environment.GetResourceString("Argument_AlreadyBoundOrSyncHandle"); }
    }

    public static string Argument_PreAllocatedAlreadyAllocated
    {
        get { return Environment.GetResourceString("Argument_PreAllocatedAlreadyAllocated"); }
    }

    public static string Argument_NativeOverlappedAlreadyFree
    {
        get { return Environment.GetResourceString("Argument_NativeOverlappedAlreadyFree"); }
    }

    public static string Argument_NativeOverlappedWrongBoundHandle
    {
        get { return Environment.GetResourceString("Argument_NativeOverlappedWrongBoundHandle"); }
    }

    public static string InvalidOperation_NativeOverlappedReused
    {
        get { return Environment.GetResourceString("InvalidOperation_NativeOverlappedReused"); }
    }
        
    public static string ArgumentOutOfRange_Length
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_Length"); }
    }

    public static string ArgumentOutOfRange_IndexString 
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_IndexString"); }
    }

    public static string ArgumentOutOfRange_Capacity 
    {
        get { return Environment.GetResourceString("ArgumentOutOfRange_Capacity"); }
    }

    public static string Arg_CryptographyException 
    {
        get { return Environment.GetResourceString("Arg_CryptographyException"); }
    }

    public static string ArgumentException_BufferNotFromPool
    {
        get { return Environment.GetResourceString("ArgumentException_BufferNotFromPool"); }
    }

    public static string Argument_InvalidPathChars
    {
        get { return Environment.GetResourceString("Argument_InvalidPathChars"); }
    }

    public static string Argument_PathFormatNotSupported
    {
        get { return Environment.GetResourceString("Argument_PathFormatNotSupported"); }
    }

    public static string Arg_PathIllegal
    {
        get { return Environment.GetResourceString("Arg_PathIllegal"); }
    }

    public static string Arg_PathIllegalUNC
    {
        get { return Environment.GetResourceString("Arg_PathIllegalUNC"); }
    }

    public static string Format(string formatString, params object[] args)
    {
        return string.Format(CultureInfo.CurrentCulture, formatString, args);
    }

    internal static string ArgumentException_ValueTupleIncorrectType
    {
        get { return Environment.GetResourceString("ArgumentException_ValueTupleIncorrectType"); }
    }

    internal static string ArgumentException_ValueTupleLastArgumentNotATuple
    {
        get { return Environment.GetResourceString("ArgumentException_ValueTupleLastArgumentNotATuple"); }
    }

    internal static string SpinLock_TryEnter_ArgumentOutOfRange
    {
        get { return Environment.GetResourceString("SpinLock_TryEnter_ArgumentOutOfRange"); }
    }

    internal static string SpinLock_TryReliableEnter_ArgumentException
    {
        get { return Environment.GetResourceString("SpinLock_TryReliableEnter_ArgumentException"); }
    }

    internal static string SpinLock_TryEnter_LockRecursionException
    {
        get { return Environment.GetResourceString("SpinLock_TryEnter_LockRecursionException"); }
    }

    internal static string SpinLock_Exit_SynchronizationLockException
    {
        get { return Environment.GetResourceString("SpinLock_Exit_SynchronizationLockException"); }
    }

    internal static string SpinLock_IsHeldByCurrentThread
    {
        get { return Environment.GetResourceString("SpinLock_IsHeldByCurrentThread"); }
    }

    internal static string ObjectDisposed_StreamIsClosed
    {
        get { return Environment.GetResourceString("ObjectDisposed_StreamIsClosed"); }
    }

    internal static string Arg_SystemException
    {
        get { return Environment.GetResourceString("Arg_SystemException"); }
    }

    internal static string Arg_StackOverflowException
    {
        get { return Environment.GetResourceString("Arg_StackOverflowException"); }
    }

    internal static string Arg_DataMisalignedException
    {
        get { return Environment.GetResourceString("Arg_DataMisalignedException"); }
    }

    internal static string Arg_ExecutionEngineException
    {
        get { return Environment.GetResourceString("Arg_ExecutionEngineException"); }
    }

    internal static string Arg_AccessException
    {
        get { return Environment.GetResourceString("Arg_AccessException"); }
    }

    internal static string Arg_AccessViolationException
    {
        get { return Environment.GetResourceString("Arg_AccessViolationException"); }
    }

    internal static string Arg_ApplicationException
    {
        get { return Environment.GetResourceString("Arg_ApplicationException"); }
    }

    internal static string Arg_ArgumentException
    {
        get { return Environment.GetResourceString("Arg_ArgumentException"); }
    }

    internal static string Arg_ParamName_Name
    {
        get { return Environment.GetResourceString("Arg_ParamName_Name"); }
    }

    internal static string ArgumentNull_Generic
    {
        get { return Environment.GetResourceString("ArgumentNull_Generic"); }
    }

    internal static string Arg_ArithmeticException
    {
        get { return Environment.GetResourceString("Arg_ArithmeticException"); }
    }

    internal static string Arg_ArrayTypeMismatchException
    {
        get { return Environment.GetResourceString("Arg_ArrayTypeMismatchException"); }
    }

    internal static string Arg_DivideByZero
    {
        get { return Environment.GetResourceString("Arg_DivideByZero"); }
    }

    internal static string Arg_DuplicateWaitObjectException
    {
        get { return Environment.GetResourceString("Arg_DuplicateWaitObjectException"); }
    }

    internal static string Arg_EntryPointNotFoundException
    {
        get { return Environment.GetResourceString("Arg_EntryPointNotFoundException"); }
    }

    internal static string Arg_FieldAccessException
    {
        get { return Environment.GetResourceString("Arg_FieldAccessException"); }
    }

    internal static string Arg_FormatException
    {
        get { return Environment.GetResourceString("Arg_FormatException"); }
    }

    internal static string Arg_IndexOutOfRangeException
    {
        get { return Environment.GetResourceString("Arg_IndexOutOfRangeException"); }
    }

    internal static string Arg_InsufficientExecutionStackException
    {
        get { return Environment.GetResourceString("Arg_InsufficientExecutionStackException"); }
    }

    internal static string Arg_InvalidCastException
    {
        get { return Environment.GetResourceString("Arg_InvalidCastException"); }
    }

    internal static string Arg_InvalidOperationException
    {
        get { return Environment.GetResourceString("Arg_InvalidOperationException"); }
    }

    internal static string InvalidProgram_Default
    {
        get { return Environment.GetResourceString("InvalidProgram_Default"); }
    }

    internal static string Arg_MethodAccessException
    {
        get { return Environment.GetResourceString("Arg_MethodAccessException"); }
    }

    internal static string Arg_MulticastNotSupportedException
    {
        get { return Environment.GetResourceString("Arg_MulticastNotSupportedException"); }
    }

    internal static string Arg_NotFiniteNumberException
    {
        get { return Environment.GetResourceString("Arg_NotFiniteNumberException"); }
    }

    internal static string Arg_NotImplementedException
    {
        get { return Environment.GetResourceString("Arg_NotImplementedException"); }
    }

    internal static string Arg_NotSupportedException
    {
        get { return Environment.GetResourceString("Arg_NotSupportedException"); }
    }

    internal static string Arg_NullReferenceException
    {
        get { return Environment.GetResourceString("Arg_NullReferenceException"); }
    }

    internal static string ObjectDisposed_Generic
    {
        get { return Environment.GetResourceString("ObjectDisposed_Generic"); }
    }

    internal static string ObjectDisposed_ObjectName_Name
    {
        get { return Environment.GetResourceString("ObjectDisposed_ObjectName_Name"); }
    }

    internal static string Arg_OverflowException
    {
        get { return Environment.GetResourceString("Arg_OverflowException"); }
    }

    internal static string Arg_PlatformNotSupported
    {
        get { return Environment.GetResourceString("Arg_PlatformNotSupported"); }
    }

    internal static string Arg_RankException
    {
        get { return Environment.GetResourceString("Arg_RankException"); }
    }

    internal static string Arg_TimeoutException
    {
        get { return Environment.GetResourceString("Arg_TimeoutException"); }
    }

    internal static string Arg_TypeAccessException
    {
        get { return Environment.GetResourceString("Arg_TypeAccessException"); }
    }

    internal static string TypeInitialization_Default
    {
        get { return Environment.GetResourceString("TypeInitialization_Default"); }
    }

    internal static string TypeInitialization_Type
    {
        get { return Environment.GetResourceString("TypeInitialization_Type"); }
    }

    internal static string Arg_UnauthorizedAccessException
    {
        get { return Environment.GetResourceString("Arg_UnauthorizedAccessException"); }
    }
}
