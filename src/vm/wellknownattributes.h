// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef __WELLKNOWNATTRIBUTES_H_
#define __WELLKNOWNATTRIBUTES_H_

enum class WellKnownAttribute : DWORD
{
    DisablePrivateReflectionType,
    ImportedFromTypeLib,
    PrimaryInteropAssembly,
    CoClass,
    ComEventInterface,
    IDispatchImpl,
    NativeCallable,
    DefaultMember,
    UnmanagedFunctionPointer,
    ManagedToNativeComInteropStub,
    ThreadStatic,
    FixedAddressValueType,
    UnsafeValueType,
    IsByRefLike,
    Intrinsic,
    WinRTMarshalingBehaviorAttribute,
    DefaultDllImportSearchPaths,
    LCIDConversion,
    ComVisible,
    ComCompatibleVersion,
    ClassInterface,
    ComDefaultInterface,
    ComSourceInterfaces,
    TypeIdentifier,
    ParamArray,
    Guid,

    CountOfWellKnownAttributes
};

const char *GetWellKnownAttributeName(WellKnownAttribute attribute)
{
    switch (attribute)
    {
        case WellKnownAttribute::DisablePrivateReflectionType:
            return "System.Runtime.CompilerServices.DisablePrivateReflectionAttribute";
        case WellKnownAttribute::ImportedFromTypeLib:
            return "System.Runtime.InteropServices.ImportedFromTypeLibAttribute";
        case WellKnownAttribute::PrimaryInteropAssembly:
            return "System.Runtime.InteropServices.PrimaryInteropAssemblyAttribute";
        case WellKnownAttribute::CoClass:
            return "System.Runtime.InteropServices.CoClassAttribute";
        case WellKnownAttribute::ComEventInterface:
            return "System.Runtime.InteropServices.ComEventInterfaceAttribute";
        case WellKnownAttribute::IDispatchImpl:
            return "System.Runtime.InteropServices.IDispatchImplAttribute";
        case WellKnownAttribute::NativeCallable:
            return "System.Runtime.InteropServices.NativeCallableAttribute";
        case WellKnownAttribute::DefaultMember:
            return "System.Reflection.DefaultMemberAttribute";
        case WellKnownAttribute::UnmanagedFunctionPointer:
            return "System.Runtime.InteropServices.UnmanagedFunctionPointerAttribute";
        case WellKnownAttribute::ManagedToNativeComInteropStub:
            return "System.Runtime.InteropServices.ManagedToNativeComInteropStubAttribute";
        case WellKnownAttribute::ThreadStatic:
            return "System.ThreadStaticAttribute";
        case WellKnownAttribute::FixedAddressValueType:
            return "System.Runtime.CompilerServices.FixedAddressValueTypeAttribute";
        case WellKnownAttribute::UnsafeValueType:
            return "System.Runtime.CompilerServices.UnsafeValueTypeAttribute";
        case WellKnownAttribute::IsByRefLike:
            return "System.Runtime.CompilerServices.IsByRefLikeAttribute";
        case WellKnownAttribute::Intrinsic:
            return "System.Runtime.CompilerServices.IntrinsicAttribute";
        case WellKnownAttribute::WinRTMarshalingBehaviorAttribute:
            return "Windows.Foundation.Metadata.MarshalingBehaviorAttribute";
        case WellKnownAttribute::DefaultDllImportSearchPaths:
            return "System.Runtime.InteropServices.DefaultDllImportSearchPathsAttribute";
        case WellKnownAttribute::LCIDConversion:
            return "System.Runtime.InteropServices.LCIDConversionAttribute";
        case WellKnownAttribute::ComVisible:
            return "System.Runtime.InteropServices.ComVisibleAttribute";
        case WellKnownAttribute::ComCompatibleVersion:
            return "System.Runtime.InteropServices.ComCompatibleVersionAttribute";
        case WellKnownAttribute::BestFitMapping:
            return "System.Runtime.InteropServices.BestFitMappingAttribute";
        case WellKnownAttribute::ClassInterface:
            return "System.Runtime.InteropServices.ClassInterfaceAttribute";
        case WellKnownAttribute::ComDefaultInterface:
            return "System.Runtime.InteropServices.ComDefaultInterfaceAttribute";
        case WellKnownAttribute::ComSourceInterfaces:
            return "System.Runtime.InteropServices.ComSourceInterfacesAttribute";
        case WellKnownAttribute::TypeIdentifier:
            return "System.Runtime.InteropServices.TypeIdentifierAttribute";
        case WellKnownAttribute::ParamArray:
            return "System.ParamArrayAttribute";
        case WellKnownAttribute::Guid:
            return "System.Runtime.InteropServices.GuidAttribute";
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
        case WellKnownAttribute::
            return ;
    }
    _ASSERTE(false); // Should not be possible
    return nullptr;
}

#endif // __WELLKNOWNATTRIBUTES_H_
