// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//

//
////////////////////////////////////////////////////////////////////////////////
// This module defines a Utility Class used by reflection
//
//  

////////////////////////////////////////////////////////////////////////////////


#include "common.h"
#include "invokeutil.h"
#include "corpriv.h"
#include "method.hpp"
#include "threads.h"
#include "excep.h"
#include "field.h"
#include "customattribute.h"
#include "eeconfig.h"
#include "generics.h"
#include "runtimehandles.h"
#include "argdestination.h"

#ifndef CROSSGEN_COMPILE

// The Attributes Table
//  20 bits for built in types and 12 bits for Properties
//  The properties are followed by the widening mask.  All types widen to them selves.
const DWORD InvokeUtil::PrimitiveAttributes[PRIMITIVE_TABLE_SIZE] = {
    0x00,                     // ELEMENT_TYPE_END
    0x00,                     // ELEMENT_TYPE_VOID
    PT_Primitive | 0x0004,    // ELEMENT_TYPE_BOOLEAN
    PT_Primitive | 0x3F88,    // ELEMENT_TYPE_CHAR (W = U2, CHAR, I4, U4, I8, U8, R4, R8) (U2 == Char)
    PT_Primitive | 0x3550,    // ELEMENT_TYPE_I1   (W = I1, I2, I4, I8, R4, R8) 
    PT_Primitive | 0x3FE8,    // ELEMENT_TYPE_U1   (W = CHAR, U1, I2, U2, I4, U4, I8, U8, R4, R8)
    PT_Primitive | 0x3540,    // ELEMENT_TYPE_I2   (W = I2, I4, I8, R4, R8)
    PT_Primitive | 0x3F88,    // ELEMENT_TYPE_U2   (W = U2, CHAR, I4, U4, I8, U8, R4, R8)
    PT_Primitive | 0x3500,    // ELEMENT_TYPE_I4   (W = I4, I8, R4, R8)
    PT_Primitive | 0x3E00,    // ELEMENT_TYPE_U4   (W = U4, I8, R4, R8)
    PT_Primitive | 0x3400,    // ELEMENT_TYPE_I8   (W = I8, R4, R8)
    PT_Primitive | 0x3800,    // ELEMENT_TYPE_U8   (W = U8, R4, R8)
    PT_Primitive | 0x3000,    // ELEMENT_TYPE_R4   (W = R4, R8)
    PT_Primitive | 0x2000,    // ELEMENT_TYPE_R8   (W = R8) 
};

BOOL InvokeUtil::IsVoidPtr(TypeHandle th)
{
    LIMITED_METHOD_CONTRACT;

    if (!th.IsPointer())
        return FALSE;

    return th.AsTypeDesc()->GetTypeParam() == MscorlibBinder::GetElementType(ELEMENT_TYPE_VOID);
}

OBJECTREF InvokeUtil::CreatePointer(TypeHandle th, void * p)
{
    CONTRACT(OBJECTREF) {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(!th.IsNull());
        POSTCONDITION(RETVAL != NULL);
    }
    CONTRACT_END;

    OBJECTREF refObj = NULL;
    GCPROTECT_BEGIN(refObj);    
    
    refObj = AllocateObject(MscorlibBinder::GetClass(CLASS__POINTER));

    ((ReflectionPointer *)OBJECTREFToObject(refObj))->_ptr = p;

    OBJECTREF refType = th.GetManagedClassObject();
    SetObjectReference(&(((ReflectionPointer *)OBJECTREFToObject(refObj))->_ptrType), refType, GetAppDomain());
    
    GCPROTECT_END();
    RETURN refObj;
}

TypeHandle InvokeUtil::GetPointerType(OBJECTREF pObj) {
    CONTRACT(TypeHandle) {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(pObj != NULL);
        POSTCONDITION(!RETVAL.IsNull());
    }
    CONTRACT_END;

    ReflectionPointer * pReflectionPointer = (ReflectionPointer *)OBJECTREFToObject(pObj);
    REFLECTCLASSBASEREF o = (REFLECTCLASSBASEREF)pReflectionPointer->_ptrType;
    TypeHandle typeHandle = o->GetType();
    RETURN typeHandle;
}

void* InvokeUtil::GetPointerValue(OBJECTREF pObj) {
    CONTRACT(void*) {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(pObj != NULL);
        POSTCONDITION(CheckPointer(RETVAL, NULL_OK));
    }
    CONTRACT_END;

    ReflectionPointer * pReflectionPointer = (ReflectionPointer *)OBJECTREFToObject(pObj);
    void *value = pReflectionPointer->_ptr;
    RETURN value;
}

void *InvokeUtil::GetIntPtrValue(OBJECTREF pObj) {
    CONTRACT(void*) {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(pObj != NULL);
        POSTCONDITION(CheckPointer(RETVAL, NULL_OK));
    }
    CONTRACT_END;

    RETURN *(void **)((pObj)->UnBox());
}

void InvokeUtil::CopyArg(TypeHandle th, OBJECTREF *pObjUNSAFE, ArgDestination *argDest) {
    CONTRACTL {
        THROWS;
        GC_NOTRIGGER; // Caller does not protect object references
        MODE_COOPERATIVE;
        PRECONDITION(!th.IsNull());
        PRECONDITION(CheckPointer(pObjUNSAFE));
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    void *pArgDst = argDest->GetDestinationAddress();

    OBJECTREF rObj = *pObjUNSAFE;
    MethodTable* pMT;
    CorElementType oType;
    CorElementType type;

    if (rObj != 0) {
        pMT = rObj->GetMethodTable();
        oType = pMT->GetInternalCorElementType();
    }
    else {
        pMT = 0;
        oType = ELEMENT_TYPE_OBJECT;
    }
    type = th.GetVerifierCorElementType();
    
    // This basically maps the Signature type our type and calls the CreatePrimitiveValue
    //  method.  We can omit this if we get alignment on these types.
    switch (type) {
    case ELEMENT_TYPE_BOOLEAN:
    case ELEMENT_TYPE_I1:
    case ELEMENT_TYPE_U1:
    case ELEMENT_TYPE_I2:
    case ELEMENT_TYPE_U2:
    case ELEMENT_TYPE_CHAR:
    case ELEMENT_TYPE_I4:
    case ELEMENT_TYPE_U4:
    case ELEMENT_TYPE_R4:
    IN_WIN32(case ELEMENT_TYPE_I:)
    IN_WIN32(case ELEMENT_TYPE_U:)
    {
        // If we got the univeral zero...Then assign it and exit.
        if (rObj == 0)
            *(PVOID *)pArgDst = 0;
        else
        {
            ARG_SLOT slot;
            CreatePrimitiveValue(type, oType, rObj, &slot);
            *(PVOID *)pArgDst = (PVOID)slot;
        }
        break;
    }


    case ELEMENT_TYPE_I8:
    case ELEMENT_TYPE_U8:
    case ELEMENT_TYPE_R8:
    IN_WIN64(case ELEMENT_TYPE_I:)
    IN_WIN64(case ELEMENT_TYPE_U:)
    {
        // If we got the univeral zero...Then assign it and exit.
        if (rObj == 0)
            *(INT64 *)pArgDst = 0;
        else
        {
            ARG_SLOT slot;
            CreatePrimitiveValue(type, oType, rObj, &slot);
            *(INT64 *)pArgDst = (INT64)slot;
        }
        break;
    }

    case ELEMENT_TYPE_VALUETYPE:
    {
        // If we got the universal zero...Then assign it and exit.
        if (rObj == 0) {
            InitValueClassArg(argDest, th.AsMethodTable());
         }
        else {
            if (!th.AsMethodTable()->UnBoxIntoArg(argDest, rObj))
                COMPlusThrow(kArgumentException, W("Arg_ObjObj"));
        }
        break;
    }

    case ELEMENT_TYPE_SZARRAY:          // Single Dim
    case ELEMENT_TYPE_ARRAY:            // General Array
    case ELEMENT_TYPE_CLASS:            // Class
    case ELEMENT_TYPE_OBJECT:
    case ELEMENT_TYPE_STRING:           // System.String
    case ELEMENT_TYPE_VAR:
    {
        if (rObj == 0) 
            *(PVOID *)pArgDst = 0;
        else
            *(PVOID *)pArgDst = OBJECTREFToObject(rObj);
        break;
    }

    case ELEMENT_TYPE_BYREF:
    {
       // 
       //     (obj is the parameter passed to MethodInfo.Invoke, by the caller)
       //     if argument is a primitive
       //     {
       //         if incoming argument, obj, is null
       //             Allocate a boxed object and place ref to it in 'obj'
       //         Unbox 'obj' and pass it to callee
       //     }
       //     if argument is a value class
       //     {
       //         if incoming argument, obj, is null
       //             Allocate an object of that valueclass, and place ref to it in 'obj'
       //         Unbox 'obj' and pass it to callee
       //     }
       //     if argument is an objectref
       //     {
       //         pass obj to callee
       //     }
       //
        TypeHandle thBaseType = th.AsTypeDesc()->GetTypeParam();

        // We should never get here for nullable types.  Instead invoke
        // heads these off and morphs the type handle to not be byref anymore
        _ASSERTE(!Nullable::IsNullableType(thBaseType));

        TypeHandle srcTH = TypeHandle();
        if (rObj == 0) 
            oType = thBaseType.GetSignatureCorElementType();
        else
            srcTH = rObj->GetTypeHandle();

        //CreateByRef only triggers GC in throw path, so it's OK to use the raw unsafe pointer
        *(PVOID *)pArgDst = CreateByRef(thBaseType, oType, srcTH, rObj, pObjUNSAFE);
        break;
    }

    case ELEMENT_TYPE_TYPEDBYREF:
    {        
        TypedByRef* ptr = (TypedByRef*) pArgDst;
        TypeHandle srcTH;
        BOOL bIsZero = FALSE;

        // If we got the univeral zero...Then assign it and exit.
        if (rObj== 0) {
            bIsZero = TRUE;
            ptr->data = 0;
            ptr->type = TypeHandle();                
        }
        else {
            bIsZero = FALSE;
            srcTH = rObj->GetTypeHandle();
            ptr->type = rObj->GetTypeHandle();                
        }

        if (!bIsZero)
        {
            //CreateByRef only triggers GC in throw path
            ptr->data = CreateByRef(srcTH, oType, srcTH, rObj, pObjUNSAFE);
        }
        
        break;
    }

    case ELEMENT_TYPE_PTR: 
    case ELEMENT_TYPE_FNPTR:
    {
        // If we got the univeral zero...Then assign it and exit.
        if (rObj == 0) {
            *(PVOID *)pArgDst = 0;
        }
        else {
            if (rObj->GetMethodTable() == MscorlibBinder::GetClassIfExist(CLASS__POINTER) && type == ELEMENT_TYPE_PTR) 
                *(PVOID *)pArgDst = GetPointerValue(rObj);
            else if (rObj->GetTypeHandle().AsMethodTable() == MscorlibBinder::GetElementType(ELEMENT_TYPE_I)) 
            {
                ARG_SLOT slot;
                CreatePrimitiveValue(oType, oType, rObj, &slot);
                *(PVOID *)pArgDst = (PVOID)slot;
            }
            else
                COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
        }
        break;
    }

    case ELEMENT_TYPE_VOID:
    default:
        _ASSERTE(!"Unknown Type");
        COMPlusThrow(kNotSupportedException);
    }
}

// CreatePrimitiveValue
// This routine will validate the object and then place the value into 
//  the destination
//  dstType -- The type of the destination
//  srcType -- The type of the source
//  srcObj -- The Object containing the primitive value.
//  pDst -- pointer to the destination
void InvokeUtil::CreatePrimitiveValue(CorElementType dstType, 
                                      CorElementType srcType,
                                      OBJECTREF srcObj,
                                      ARG_SLOT *pDst) {
    CONTRACTL {
        THROWS;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(srcObj != NULL);
        PRECONDITION(CheckPointer(pDst));
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;
    CreatePrimitiveValue(dstType, srcType, srcObj->UnBox(), srcObj->GetMethodTable(), pDst);
}

void InvokeUtil::CreatePrimitiveValue(CorElementType dstType,CorElementType srcType,
    void *pSrc, MethodTable *pSrcMT, ARG_SLOT* pDst)
{

    CONTRACTL {
        THROWS;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(pDst));
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    if (!IsPrimitiveType(srcType) || !CanPrimitiveWiden(dstType, srcType))
        COMPlusThrow(kArgumentException, W("Arg_PrimWiden"));

    ARG_SLOT data = 0;
 
    switch (srcType) {
    case ELEMENT_TYPE_I1:
        data = *(INT8*)pSrc;
        break;
    case ELEMENT_TYPE_I2:
        data = *(INT16*)pSrc;
        break;
    IN_WIN32(case ELEMENT_TYPE_I:)
    case ELEMENT_TYPE_I4:
        data = *(INT32 *)pSrc;
        break;
    IN_WIN64(case ELEMENT_TYPE_I:)
    case ELEMENT_TYPE_I8:
        data = *(INT64 *)pSrc;
        break;
    default:
        switch (pSrcMT->GetNumInstanceFieldBytes())
        {
        case 1:
            data = *(UINT8 *)pSrc;
            break;
        case 2:
            data = *(UINT16 *)pSrc;
            break;
        case 4:
            data = *(UINT32 *)pSrc;
            break;
        case 8:
            data = *(UINT64 *)pSrc;
            break;
        default:
            _ASSERTE(!"Unknown conversion");
            // this is really an impossible condition
            COMPlusThrow(kNotSupportedException);
            break;
        }
    }

    if (srcType == dstType) {
        // shortcut
        *pDst = data;
        return;
    }

    // Copy the data and return
    switch (dstType) {
    case ELEMENT_TYPE_BOOLEAN:
    case ELEMENT_TYPE_I1:
    case ELEMENT_TYPE_U1:
    case ELEMENT_TYPE_CHAR:
    case ELEMENT_TYPE_I2:
    case ELEMENT_TYPE_U2:
    case ELEMENT_TYPE_I4:
    case ELEMENT_TYPE_U4:
    case ELEMENT_TYPE_I:
    case ELEMENT_TYPE_U:
    case ELEMENT_TYPE_I8:
    case ELEMENT_TYPE_U8:
        switch (srcType) {
        case ELEMENT_TYPE_BOOLEAN:
        case ELEMENT_TYPE_I1:
        case ELEMENT_TYPE_U1:
        case ELEMENT_TYPE_CHAR:
        case ELEMENT_TYPE_I2:
        case ELEMENT_TYPE_U2:
        case ELEMENT_TYPE_I4:
        case ELEMENT_TYPE_U4:
        case ELEMENT_TYPE_I:
        case ELEMENT_TYPE_U:
        case ELEMENT_TYPE_I8:
        case ELEMENT_TYPE_U8:
            *pDst = data;
            break;
        case ELEMENT_TYPE_R4:
            *pDst = (I8)(*(R4*)pSrc);
            break;
        case ELEMENT_TYPE_R8:
            *pDst = (I8)(*(R8*)pSrc);
            break;
        default:
            _ASSERTE(!"Unknown conversion");
            // this is really an impossible condition
            COMPlusThrow(kNotSupportedException);
        }
        break;
    case ELEMENT_TYPE_R4:
    case ELEMENT_TYPE_R8:
        {
        R8 r8 = 0;
        switch (srcType) {
        case ELEMENT_TYPE_BOOLEAN:
        case ELEMENT_TYPE_I1:
        case ELEMENT_TYPE_I2:
        case ELEMENT_TYPE_I4:
        IN_WIN32(case ELEMENT_TYPE_I:)
            r8 = (R8)((INT32)data);
            break;
        case ELEMENT_TYPE_U1:
        case ELEMENT_TYPE_CHAR:
        case ELEMENT_TYPE_U2:
        case ELEMENT_TYPE_U4:
        IN_WIN32(case ELEMENT_TYPE_U:)
            r8 = (R8)((UINT32)data);
            break;
        case ELEMENT_TYPE_U8:
        IN_WIN64(case ELEMENT_TYPE_U:)
            r8 = (R8)((UINT64)data);
            break;
        case ELEMENT_TYPE_I8:
        IN_WIN64(case ELEMENT_TYPE_I:)
            r8 = (R8)((INT64)data);
            break;
        case ELEMENT_TYPE_R4:
            r8 = *(R4*)pSrc;
            break;
        case ELEMENT_TYPE_R8:
            r8 = *(R8*)pSrc;
            break;
        default:
            _ASSERTE(!"Unknown R4 or R8 conversion");
            // this is really an impossible condition
            COMPlusThrow(kNotSupportedException);
        }

        if (dstType == ELEMENT_TYPE_R4) {
            R4 r4 = (R4)r8;
            *pDst = (UINT32&)r4;
        }
        else {
            *pDst = (UINT64&)r8;
        }

        }
        break;
    default:
        _ASSERTE(!"Unknown conversion");
    }
}

void* InvokeUtil::CreateByRef(TypeHandle dstTh,
                              CorElementType srcType, 
                              TypeHandle srcTH,
                              OBJECTREF srcObj, 
                              OBJECTREF *pIncomingObj) {
    CONTRACTL {
        THROWS;
        GC_NOTRIGGER;
        MODE_COOPERATIVE;
        PRECONDITION(!dstTh.IsNull());
        PRECONDITION(CheckPointer(pIncomingObj));

        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    CorElementType dstType = dstTh.GetSignatureCorElementType();
    if (IsPrimitiveType(srcType) && IsPrimitiveType(dstType)) {
        if (dstType != srcType)
        {
            CONTRACT_VIOLATION (GCViolation);
            COMPlusThrow(kArgumentException,W("Arg_PrimWiden"));
        }

        return srcObj->UnBox();
    }

    if (srcTH.IsNull()) {
        return pIncomingObj;
    }

    _ASSERTE(srcObj != NULL);

    if (dstType == ELEMENT_TYPE_VALUETYPE) {
        return srcObj->UnBox();
    }
    else
        return pIncomingObj;
}

// GetBoxedObject
// Given an address of a primitve type, this will box that data...
// <TODO>@TODO: We need to handle all value classes?</TODO>
OBJECTREF InvokeUtil::GetBoxedObject(TypeHandle th, void* pData) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(!th.IsNull());
        PRECONDITION(CheckPointer(pData));
        
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    MethodTable *pMethTable = th.GetMethodTable();
    PREFIX_ASSUME(pMethTable != NULL);
    // Save off the data.  We are going to create and object
    //  which may cause GC to occur.
    int size = pMethTable->GetNumInstanceFieldBytes();
    void *p = _alloca(size);
    memcpy(p, pData, size);
    OBJECTREF retO = pMethTable->Box(p);
    return retO;
}

//ValidField
// This method checks that the object can be widened to the proper type
void InvokeUtil::ValidField(TypeHandle th, OBJECTREF* value)
{
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(!th.IsNull());
        PRECONDITION(CheckPointer(value));
        PRECONDITION(IsProtectedByGCFrame (value));
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    if ((*value) == 0)
        return;

    MethodTable* pMT;
    CorElementType oType;
    CorElementType type = th.GetSignatureCorElementType();
    pMT = (*value)->GetMethodTable();
    oType = TypeHandle(pMT).GetSignatureCorElementType();

    // handle pointers
    if (type == ELEMENT_TYPE_PTR || type == ELEMENT_TYPE_FNPTR) {
        if (MscorlibBinder::IsClass((*value)->GetMethodTable(), CLASS__POINTER) && type == ELEMENT_TYPE_PTR) {
            TypeHandle srcTH = GetPointerType(*value);

            if (!IsVoidPtr(th)) {
                if (!srcTH.CanCastTo(th))
                    COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
            }
            return;
        }
        else if (MscorlibBinder::IsClass((*value)->GetMethodTable(), CLASS__INTPTR)) {
            return;
        }

        COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
    }

   // Need to handle Object special
    if (type == ELEMENT_TYPE_CLASS  || type == ELEMENT_TYPE_VALUETYPE ||
            type == ELEMENT_TYPE_OBJECT || type == ELEMENT_TYPE_STRING ||
            type == ELEMENT_TYPE_ARRAY  || type == ELEMENT_TYPE_SZARRAY) 
    {

        if (th.GetMethodTable() == g_pObjectClass)
            return;
        if (IsPrimitiveType(oType)) {
            if (type != ELEMENT_TYPE_VALUETYPE)
                COMPlusThrow(kArgumentException,W("Arg_ObjObj"));

            // Legacy behavior: The following if disallows assigning primitives to enums.
            if (th.IsEnum())
                COMPlusThrow(kArgumentException,W("Arg_ObjObj"));

            type = th.GetVerifierCorElementType();
            if (IsPrimitiveType(type))
            {
                if (CanPrimitiveWiden(type, oType)) 
                    return;
                else 
                    COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
            }
        }

        if (!ObjIsInstanceOf(OBJECTREFToObject(*value), th)) {
            COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
        }
        return;
    }


    if (!IsPrimitiveType(oType))
        COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
    // Now make sure we can widen into the proper type -- CanWiden may run GC...
    if (!CanPrimitiveWiden(type,oType))
        COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
}

//
// CreateObjectAfterInvoke
// This routine will create the specified object from the value returned by the Invoke target. 
//
// This does not handle the ELEMENT_TYPE_VALUETYPE case. There is no way for this function to handle that case in a GC-safe way.
// The caller must preallocate the box object and copy the value type into it afterward.
//
OBJECTREF InvokeUtil::CreateObjectAfterInvoke(TypeHandle th, void * pValue) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(!th.IsNull());
        
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    CorElementType type = th.GetSignatureCorElementType();
    MethodTable *pMT = NULL;
    OBJECTREF obj = NULL;

    // WARNING: pValue can be an inner reference into a managed object and it is not protected from GC. You must do nothing that
    // triggers a GC until the all the data it points to has been captured in a GC-protected location.

    // Handle the non-table types
    switch (type) {
    case ELEMENT_TYPE_VOID:
        break;

    case ELEMENT_TYPE_PTR:
    {
        obj = CreatePointer(th, *(void **)pValue);
        break;
    }

    case ELEMENT_TYPE_FNPTR:
        pMT = MscorlibBinder::GetElementType(ELEMENT_TYPE_I);
        goto PrimitiveType;

    case ELEMENT_TYPE_VALUETYPE:
        _ASSERTE(!"You cannot use this function for arbitrary value types. You must preallocate a box object and copy the value in yourself.");
        break;

    case ELEMENT_TYPE_CLASS:        // Class
    case ELEMENT_TYPE_SZARRAY:      // Single Dim, Zero
    case ELEMENT_TYPE_ARRAY:        // General Array
    case ELEMENT_TYPE_STRING:
    case ELEMENT_TYPE_OBJECT:
    case ELEMENT_TYPE_VAR:
        obj = *(OBJECTREF *)pValue;
        break;
    
    case ELEMENT_TYPE_BOOLEAN:      // boolean
    case ELEMENT_TYPE_I1:           // byte
    case ELEMENT_TYPE_U1:
    case ELEMENT_TYPE_I2:           // short
    case ELEMENT_TYPE_U2:           
    case ELEMENT_TYPE_CHAR:         // char
    case ELEMENT_TYPE_I4:           // int
    case ELEMENT_TYPE_U4:
    case ELEMENT_TYPE_I8:           // long
    case ELEMENT_TYPE_U8:       
    case ELEMENT_TYPE_R4:           // float
    case ELEMENT_TYPE_R8:           // double
    case ELEMENT_TYPE_I:
    case ELEMENT_TYPE_U:
        _ASSERTE(!th.IsTypeDesc());
        pMT = th.AsMethodTable();
    PrimitiveType:
        {
            // Don't use MethodTable::Box here for perf reasons
            PREFIX_ASSUME(pMT != NULL);
            DWORD size = pMT->GetNumInstanceFieldBytes();

            UINT64 capturedValue;
            memcpyNoGCRefs(&capturedValue, pValue, size); // Must capture the primitive value before we allocate the boxed object which can trigger a GC.

            INDEBUG(pValue = (LPVOID)0xcccccccc); // We're about to allocate a GC object - can no longer trust pValue
            obj = AllocateObject(pMT);
            memcpyNoGCRefs(obj->UnBox(), &capturedValue, size);
        }
        break;
    
    case ELEMENT_TYPE_BYREF:
        COMPlusThrow(kNotSupportedException, W("NotSupported_ByRefReturn"));
    case ELEMENT_TYPE_END:
    default:
        _ASSERTE(!"Unknown Type");
        COMPlusThrow(kNotSupportedException);
    }
    
    return obj;    
}

// This is a special purpose Exception creation function.  It
//  creates the ReflectionTypeLoadException placing the passed
//  classes array and exception array into it.
OBJECTREF InvokeUtil::CreateClassLoadExcept(OBJECTREF* classes, OBJECTREF* except) {
    CONTRACT(OBJECTREF) {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(classes));
        PRECONDITION(CheckPointer(except));
        PRECONDITION(IsProtectedByGCFrame (classes));
        PRECONDITION(IsProtectedByGCFrame (except));
        
        POSTCONDITION(RETVAL != NULL);

        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACT_END;

    OBJECTREF oRet = 0;

    struct {
        OBJECTREF o;
        STRINGREF str;
    } gc;
    ZeroMemory(&gc, sizeof(gc));

    MethodTable *pVMClassLoadExcept = MscorlibBinder::GetException(kReflectionTypeLoadException);
    gc.o = AllocateObject(pVMClassLoadExcept);
    GCPROTECT_BEGIN(gc);
    ARG_SLOT args[4];

    // Retrieve the resource string.
    ResMgrGetString(W("ReflectionTypeLoad_LoadFailed"), &gc.str);

    MethodDesc* pMD = MemberLoader::FindMethod(gc.o->GetTrueMethodTable(),
                            COR_CTOR_METHOD_NAME, &gsig_IM_ArrType_ArrException_Str_RetVoid);

    if (!pMD)
    {
        MAKE_WIDEPTR_FROMUTF8(wzMethodName, COR_CTOR_METHOD_NAME);
        COMPlusThrowNonLocalized(kMissingMethodException, wzMethodName);
    }

    MethodDescCallSite ctor(pMD);

    // Call the constructor
    args[0]  = ObjToArgSlot(gc.o);
    args[1]  = ObjToArgSlot(*classes);
    args[2]  = ObjToArgSlot(*except);
    args[3]  = ObjToArgSlot((OBJECTREF)gc.str);

    ctor.Call(args);

    oRet = gc.o;

    GCPROTECT_END();
    RETURN oRet;
}

OBJECTREF InvokeUtil::CreateTargetExcept(OBJECTREF* except) {
    CONTRACT(OBJECTREF) {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(except));
        PRECONDITION(IsProtectedByGCFrame (except));
        
        POSTCONDITION(RETVAL != NULL);

        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACT_END;

    OBJECTREF o;
    OBJECTREF oRet = 0;

    MethodTable *pVMTargetExcept = MscorlibBinder::GetException(kTargetInvocationException);
    o = AllocateObject(pVMTargetExcept);
    GCPROTECT_BEGIN(o);
    ARG_SLOT args[2];

    MethodDesc* pMD = MemberLoader::FindMethod(o->GetTrueMethodTable(),
                            COR_CTOR_METHOD_NAME, &gsig_IM_Exception_RetVoid);
    
    if (!pMD)
    {
        MAKE_WIDEPTR_FROMUTF8(wzMethodName, COR_CTOR_METHOD_NAME);
        COMPlusThrowNonLocalized(kMissingMethodException, wzMethodName);
    }

    MethodDescCallSite ctor(pMD);

    // Call the constructor
    args[0]  = ObjToArgSlot(o);
    // for security, don't allow a non-exception object to be spoofed as an exception object. We cast later and
    // don't check and this could cause us grief.
    _ASSERTE(!except || IsException((*except)->GetMethodTable()));  // how do we get non-exceptions?
    if (except && IsException((*except)->GetMethodTable()))
    {
        args[1]  = ObjToArgSlot(*except);
    }
    else
    {
        args[1] = NULL;
    }

    ctor.Call(args);

    oRet = o;

    GCPROTECT_END();
    RETURN oRet;
}

// ChangeType
// This method will invoke the Binder change type method on the object
//  binder -- The Binder object
//  srcObj -- The source object to be changed
//  th -- The TypeHandel of the target type
//  locale -- The locale passed to the class.
OBJECTREF InvokeUtil::ChangeType(OBJECTREF binder, OBJECTREF srcObj, TypeHandle th, OBJECTREF locale) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(binder != NULL);
        PRECONDITION(srcObj != NULL);
        
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;
    
    OBJECTREF typeClass = NULL;
    OBJECTREF o;

    struct _gc {
        OBJECTREF binder;
        OBJECTREF srcObj;
        OBJECTREF locale;
        OBJECTREF typeClass;
    } gc; 

    gc.binder = binder;
    gc.srcObj = srcObj;
    gc.locale = locale;
    gc.typeClass = NULL;

    GCPROTECT_BEGIN(gc);

    MethodDescCallSite changeType(METHOD__BINDER__CHANGE_TYPE, &gc.binder);

    // Now call this method on this object.
    typeClass = th.GetManagedClassObject();

    ARG_SLOT pNewArgs[] = {
            ObjToArgSlot(gc.binder),
            ObjToArgSlot(gc.srcObj),
            ObjToArgSlot(gc.typeClass),
            ObjToArgSlot(gc.locale),
    };

    o = changeType.Call_RetOBJECTREF(pNewArgs);

    GCPROTECT_END();

    return o;
}

// Ensure that the field is declared on the type or subtype of the type to which the typed reference refers.
// Note that a typed reference is a reference to an object and is not a field on that object (as in C# ref).
// Ensure that if the field is an instance field that the typed reference is not null.
void InvokeUtil::ValidateObjectTarget(FieldDesc *pField, TypeHandle enclosingType, OBJECTREF *target) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(pField));
        PRECONDITION(!enclosingType.IsNull() || pField->IsStatic());
        PRECONDITION(CheckPointer(target));
    
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;
    
    if (pField->IsStatic() && (enclosingType.IsNull() || !*target))
        return;

    if (!pField->IsStatic() && !*target)
        COMPlusThrow(kTargetException,W("RFLCT.Targ_StatFldReqTarg"));
        
    // Verify that the object is of the proper type...
    TypeHandle ty = (*target)->GetTrueTypeHandle();
    while (!ty.IsNull() && ty != enclosingType)
        ty = ty.GetParent();

    // Give a second chance to thunking classes to do the 
    // correct cast
    if (ty.IsNull()) {
        {
            COMPlusThrow(kArgumentException,W("Arg_ObjObj"));
        }
    }
}

// SetValidField
// Given an target object, a value object and a field this method will set the field
//  on the target object.  The field must be validate before calling this.
void InvokeUtil::SetValidField(CorElementType fldType,
                               TypeHandle fldTH,
                               FieldDesc *pField,
                               OBJECTREF *target,
                               OBJECTREF *valueObj,
                               TypeHandle declaringType,
                               CLR_BOOL *pDomainInitialized) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(!fldTH.IsNull());
        PRECONDITION(CheckPointer(pField));
        PRECONDITION(CheckPointer(target));
        PRECONDITION(CheckPointer(valueObj));
        PRECONDITION(IsProtectedByGCFrame (target));
        PRECONDITION(IsProtectedByGCFrame (valueObj));        
        PRECONDITION(declaringType.IsNull () || !declaringType.IsTypeDesc());
    
        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    // We don't allow setting the field of nullable<T> (hasValue and value)
    // Because you can't independantly set them for this type.  
    if (!declaringType.IsNull() && Nullable::IsNullableType(declaringType.GetMethodTable())) 
        COMPlusThrow(kNotSupportedException);

    // call the <cinit> 
    OBJECTREF Throwable = NULL;

    MethodTable * pDeclMT = NULL;
    if (!declaringType.IsNull())
    {
        pDeclMT = declaringType.GetMethodTable();

        if (pDeclMT->IsSharedByGenericInstantiations())
            COMPlusThrow(kNotSupportedException, W("NotSupported_Type"));
    }

    if (*pDomainInitialized == FALSE)
    {
        EX_TRY
        {
        if (declaringType.IsNull())
        {
            pField->GetModule()->GetGlobalMethodTable()->EnsureInstanceActive();
            pField->GetModule()->GetGlobalMethodTable()->CheckRunClassInitThrowing();
        }
        else
        {
            pDeclMT->EnsureInstanceActive();
            pDeclMT->CheckRunClassInitThrowing();   

            if (declaringType.IsDomainNeutral() == FALSE)
                *pDomainInitialized = TRUE;
        }
        }
        EX_CATCH_THROWABLE(&Throwable);
    }
#ifdef _DEBUG
    else if (*pDomainInitialized == TRUE && !declaringType.IsNull())
       CONSISTENCY_CHECK(declaringType.GetMethodTable()->CheckActivated());
#endif

    if(Throwable != NULL)
    {
        GCPROTECT_BEGIN(Throwable);
        OBJECTREF except = CreateTargetExcept(&Throwable);
        COMPlusThrow(except);
        GCPROTECT_END();
    }

    // Set the field
    ARG_SLOT value;

    void* valueptr;
    switch (fldType) {
    case ELEMENT_TYPE_VOID:
        _ASSERTE(!"Void used as Field Type!");
        COMPlusThrow(kNotSupportedException);

    case ELEMENT_TYPE_BOOLEAN:  // boolean
    case ELEMENT_TYPE_I1:       // byte
    case ELEMENT_TYPE_U1:       // unsigned byte
        value = 0;
        if (*valueObj != 0) {
            MethodTable *p = (*valueObj)->GetMethodTable();
            CorElementType oType = p->GetInternalCorElementType();
            CreatePrimitiveValue(fldType, oType, *valueObj, &value);
        }

        if (pField->IsStatic())
            pField->SetStaticValue8((unsigned char)value);
        else 
            pField->SetValue8(*target,(unsigned char)value);
        break;

    case ELEMENT_TYPE_I2:       // short
    case ELEMENT_TYPE_U2:       // unsigned short
    case ELEMENT_TYPE_CHAR:     // char
        value = 0;
        if (*valueObj != 0) {
            MethodTable *p = (*valueObj)->GetMethodTable();
            CorElementType oType = p->GetInternalCorElementType();
            CreatePrimitiveValue(fldType, oType, *valueObj, &value);
        }

        if (pField->IsStatic())
            pField->SetStaticValue16((short)value);
        else 
            pField->SetValue16(*target, (short)value);
        break;

    case ELEMENT_TYPE_I:
        valueptr = *valueObj != 0 ? GetIntPtrValue(*valueObj) : NULL;
        if (pField->IsStatic()) 
            pField->SetStaticValuePtr(valueptr);
        else 
            pField->SetValuePtr(*target,valueptr);
        break;

    case ELEMENT_TYPE_U:
        valueptr = *valueObj != 0 ? GetIntPtrValue(*valueObj) : NULL;
        if (pField->IsStatic()) 
            pField->SetStaticValuePtr(valueptr);
        else 
            pField->SetValuePtr(*target,valueptr);
        break;
    
    case ELEMENT_TYPE_PTR:      // pointers
        if (*valueObj != 0 && MscorlibBinder::IsClass((*valueObj)->GetMethodTable(), CLASS__POINTER)) {
            valueptr = GetPointerValue(*valueObj);
            if (pField->IsStatic()) 
                pField->SetStaticValuePtr(valueptr);
            else 
                pField->SetValuePtr(*target,valueptr);
            break;
        }
        // drop through
    case ELEMENT_TYPE_FNPTR:
        valueptr = *valueObj != 0 ? GetIntPtrValue(*valueObj) : NULL;
        if (pField->IsStatic()) 
            pField->SetStaticValuePtr(valueptr);
        else 
            pField->SetValuePtr(*target,valueptr);
        break;

    case ELEMENT_TYPE_I4:       // int
    case ELEMENT_TYPE_U4:       // unsigned int
    case ELEMENT_TYPE_R4:       // float
        value = 0;
        if (*valueObj != 0) {
            MethodTable *p = (*valueObj)->GetMethodTable();
            CorElementType oType = p->GetInternalCorElementType();
            CreatePrimitiveValue(fldType, oType, *valueObj, &value);
        }

        if (pField->IsStatic()) 
            pField->SetStaticValue32((int)value);
        else 
            pField->SetValue32(*target, (int)value);
        break;

    case ELEMENT_TYPE_I8:       // long
    case ELEMENT_TYPE_U8:       // unsigned long
    case ELEMENT_TYPE_R8:       // double
        value = 0;
        if (*valueObj != 0) {
            MethodTable *p = (*valueObj)->GetMethodTable();
            CorElementType oType = p->GetInternalCorElementType();
            CreatePrimitiveValue(fldType, oType, *valueObj, &value);
        }

        if (pField->IsStatic())
            pField->SetStaticValue64(value);
        else 
            pField->SetValue64(*target,value);
        break;

    case ELEMENT_TYPE_SZARRAY:          // Single Dim, Zero
    case ELEMENT_TYPE_ARRAY:            // General Array
    case ELEMENT_TYPE_CLASS:
    case ELEMENT_TYPE_OBJECT:
    case ELEMENT_TYPE_VAR:
        if (pField->IsStatic())
            pField->SetStaticOBJECTREF(*valueObj);
        else
            pField->SetRefValue(*target, *valueObj);
        break;

    case ELEMENT_TYPE_VALUETYPE:
    {
        _ASSERTE(!fldTH.IsTypeDesc());
        MethodTable *pMT = fldTH.AsMethodTable();
        {
            void* pFieldData;
            if (pField->IsStatic()) 
                pFieldData = pField->GetCurrentStaticAddress();
            else 
                pFieldData = (*((BYTE**)target)) + pField->GetOffset() + sizeof(Object);

            if (*valueObj == NULL) 
                InitValueClass(pFieldData, pMT);
            else  
                pMT->UnBoxIntoUnchecked(pFieldData, *valueObj);
        }
    }
    break;

    default:
        _ASSERTE(!"Unknown Type");
        // this is really an impossible condition
        COMPlusThrow(kNotSupportedException);
    }
}

// GetFieldValue
// This method will return an ARG_SLOT containing the value of the field.
// GetFieldValue
// This method will return an ARG_SLOT containing the value of the field.
OBJECTREF InvokeUtil::GetFieldValue(FieldDesc* pField, TypeHandle fieldType, OBJECTREF* target, TypeHandle declaringType, CLR_BOOL *pDomainInitialized) {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(pField));
        PRECONDITION(!fieldType.IsNull());
        PRECONDITION(CheckPointer(target));
        PRECONDITION(declaringType.IsNull () || !declaringType.IsTypeDesc());

        INJECT_FAULT(COMPlusThrowOM()); 
    }
    CONTRACTL_END;

    OBJECTREF obj = NULL;

    // call the .cctor 
    OBJECTREF Throwable = NULL;

    MethodTable * pDeclMT = NULL;
    if (!declaringType.IsNull())
    {
        pDeclMT = declaringType.GetMethodTable();

        if (pDeclMT->IsSharedByGenericInstantiations())
            COMPlusThrow(kNotSupportedException, W("NotSupported_Type"));
    }

    if (*pDomainInitialized == FALSE)
    {
        EX_TRY
        {
        if (declaringType.IsNull())
        {
            pField->GetModule()->GetGlobalMethodTable()->EnsureInstanceActive();
            pField->GetModule()->GetGlobalMethodTable()->CheckRunClassInitThrowing();
        }
        else
        {
            pDeclMT->EnsureInstanceActive();
            pDeclMT->CheckRunClassInitThrowing();   

            if (!declaringType.IsDomainNeutral())
                *pDomainInitialized = TRUE;
        }
        }
        EX_CATCH_THROWABLE(&Throwable);
    }
#ifdef _DEBUG
    else if (*pDomainInitialized == TRUE && !declaringType.IsNull())
       CONSISTENCY_CHECK(declaringType.GetMethodTable()->CheckActivated());
#endif


    if(Throwable != NULL)
    {
        GCPROTECT_BEGIN(Throwable);
        OBJECTREF except = CreateTargetExcept(&Throwable);
        COMPlusThrow(except);
        GCPROTECT_END();
    }

    // We don't allow getting the field just so we don't have more specical
    // cases than we need to.  The we need at least the throw check to insure
    // we don't allow data corruption, but 
    if (!declaringType.IsNull() && Nullable::IsNullableType(pDeclMT)) 
        COMPlusThrow(kNotSupportedException);

    CorElementType fieldElementType = pField->GetFieldType();

    switch (fieldElementType) {
    
    case ELEMENT_TYPE_BOOLEAN:  // boolean
    case ELEMENT_TYPE_I1:       // byte
    case ELEMENT_TYPE_U1:       // unsigned byte
    case ELEMENT_TYPE_I2:       // short
    case ELEMENT_TYPE_U2:       // unsigned short
    case ELEMENT_TYPE_CHAR:     // char
    case ELEMENT_TYPE_I4:       // int
    case ELEMENT_TYPE_U4:       // unsigned int
    case ELEMENT_TYPE_R4:       // float
    case ELEMENT_TYPE_I8:       // long
    case ELEMENT_TYPE_U8:       // unsigned long
    case ELEMENT_TYPE_R8:       // double
    case ELEMENT_TYPE_I:
    case ELEMENT_TYPE_U:
    {
        // create the object and copy
        fieldType.AsMethodTable()->EnsureActive();
        obj = AllocateObject(fieldType.AsMethodTable());
        GCPROTECT_BEGIN(obj);
        if (pField->IsStatic()) 
            CopyValueClass(obj->UnBox(), 
                           pField->GetCurrentStaticAddress(), 
                           fieldType.AsMethodTable(), 
                           obj->GetAppDomain());
        else
            pField->GetInstanceField(*target, obj->UnBox());
        GCPROTECT_END();
        break;
    }

    case ELEMENT_TYPE_OBJECT:
    case ELEMENT_TYPE_CLASS:
    case ELEMENT_TYPE_SZARRAY:          // Single Dim, Zero
    case ELEMENT_TYPE_ARRAY:            // general array
    case ELEMENT_TYPE_VAR:
        if (pField->IsStatic()) 
            obj = pField->GetStaticOBJECTREF();
        else 
            obj = pField->GetRefValue(*target);
        break;

    case ELEMENT_TYPE_VALUETYPE:
    {
        // Value classes require createing a boxed version of the field and then
        //  copying from the source...
        // Allocate an object to return...
        _ASSERTE(!fieldType.IsTypeDesc());
        
        void *p = NULL;
        fieldType.AsMethodTable()->EnsureActive();
        obj = fieldType.AsMethodTable()->Allocate();
        GCPROTECT_BEGIN(obj);
        // calculate the offset to the field...
        if (pField->IsStatic())
            p = pField->GetCurrentStaticAddress();
        else {
                p = (*((BYTE**)target)) + pField->GetOffset() + sizeof(Object);
        }
        GCPROTECT_END();

        // copy the field to the unboxed object.
        // note: this will be done only for the non-remoting case
        if (p) {
            CopyValueClass(obj->GetData(), p, fieldType.AsMethodTable(), obj->GetAppDomain());
        }

            // If it is a Nullable<T>, box it using Nullable<T> conventions.
            // TODO: this double allocates on constructions which is wastefull
        obj = Nullable::NormalizeBox(obj);
        break;
    }

    case ELEMENT_TYPE_FNPTR:
    {
        void *value = NULL;
        if (pField->IsStatic()) 
            value = pField->GetStaticValuePtr();
        else 
            value = pField->GetValuePtr(*target);

        MethodTable *pIntPtrMT = MscorlibBinder::GetClass(CLASS__INTPTR);
        obj = AllocateObject(pIntPtrMT);
        CopyValueClass(obj->UnBox(), &value, pIntPtrMT, obj->GetAppDomain());
        break;
    }

    case ELEMENT_TYPE_PTR:
    {
        void *value = NULL;
        if (pField->IsStatic()) 
            value = pField->GetStaticValuePtr();
        else 
            value = pField->GetValuePtr(*target);
        obj = CreatePointer(fieldType, value);
        break;
    }

    default:
        _ASSERTE(!"Unknown Type");
        // this is really an impossible condition
        COMPlusThrow(kNotSupportedException);
    }

    return obj;
}

void RefSecContext::FindCaller()
{
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;
    
    if (!m_fCheckedCaller)
    {
        m_pCaller = SystemDomain::GetCallersMethod(NULL, &m_pCallerDomain);

        // If we didn't find a caller, we were called through interop. In this
        // case we know we're going to get full permissions.
        if (m_pCaller == NULL && !m_fCheckedPerm) {
            m_fCallerHasPerm = true;

            m_fCheckedPerm = true;
        }
        m_fCheckedCaller = true;
    }
}

MethodDesc *RefSecContext::GetCallerMethod() {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;
    
    FindCaller();
    return m_pCaller;
}

AppDomain *RefSecContext::GetCallerDomain() {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;
    
    FindCaller();
    return m_pCallerDomain;
}

MethodTable *RefSecContext::GetCallerMT() {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;
    
    MethodDesc *pCaller = GetCallerMethod();
    return pCaller ? pCaller->GetMethodTable() : NULL;
}

Assembly *RefSecContext::GetCallerAssembly() {
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;
    
    MethodTable *pMT = GetCallerMT();
    return pMT ? pMT->GetAssembly() : NULL;
}

bool RefSecContext::IsCalledFromInterop()
{
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    MethodDesc *pCaller = GetCallerMethod();
    return (pCaller == NULL);
}

void InvokeUtil::CanAccessClass(RefSecContext*  pCtx,
                                MethodTable*    pClass,
                                BOOL            checkAccessForImplicitValueTypeCtor /*= FALSE*/)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    InvokeUtil::CheckAccessClass(pCtx, pClass, checkAccessForImplicitValueTypeCtor);

}

#ifndef DACCESS_COMPILE
void InvokeUtil::CanAccessMethod(MethodDesc*    pMeth,
                                 MethodTable*   pParentMT,
                                 MethodTable*   pInstanceMT,
                                 RefSecContext* pSCtx,
                                 BOOL           fCriticalToFullDemand,
                                 BOOL           checkSkipVer /*= FALSE*/)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(pMeth));
        PRECONDITION(CheckPointer(pSCtx));
    }
    CONTRACTL_END;

    InvokeUtil::CheckAccessMethod(pSCtx,
                                  pParentMT,
                                  pInstanceMT,
                                  pMeth);
}
#endif // #ifndef DACCESS_COMPILE

void InvokeUtil::CanAccessField(RefSecContext*  pCtx,
                                MethodTable*    pTargetMT,
                                MethodTable*    pInstanceMT,
                                FieldDesc*      pTargetField)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
    }
    CONTRACTL_END;

    InvokeUtil::CheckAccessField(pCtx, pTargetMT, pInstanceMT, pTargetField);

}

//
// Ensure that a type is accessable, throwing a TypeLoadException if not
//
// Arguments:
//    pCtx                  - current reflection context
//    pTargetMT             - class to check access to
//    checkAccessForImplicitValueTypeCtor - ValueTypes always have an implicit constructor.
//        If a user tries to do "new ValueType()", we support it even if there is not
//        explicit constructor. However, we want to throw MethodAccessException in this case,
//        though the accessibility check is done against the type.
//
// Return Value:
//    Nothing - throws an exception if access is not allowed
//

// static
void InvokeUtil::CheckAccessClass(RefSecContext *pCtx,
                                  MethodTable *pClassMT,
                                  BOOL checkAccessForImplicitValueTypeCtor /* = FALSE */)
{
    CONTRACTL 
    {
        THROWS;
        GC_TRIGGERS;
        MODE_COOPERATIVE;
        PRECONDITION(CheckPointer(pCtx));
        PRECONDITION(CheckPointer(pClassMT));
    }
    CONTRACTL_END;

    // Walking the stack is expensive so don't try to get the caller here.
    // ClassLoader::CanAccessClass will retrieve the caller when necessary
    // and it will give an interop (NULL) caller a pass.

    AccessCheckOptions accessCheckOptions(pCtx->GetAccessCheckType(),
                                          NULL,
                                          !checkAccessForImplicitValueTypeCtor,
                                          pClassMT);

    BOOL canAccess;
    canAccess = ClassLoader::CanAccessClass(pCtx,
                                            pClassMT,
                                            pClassMT->GetAssembly(),
                                            accessCheckOptions);

    if (!canAccess)
    {
        _ASSERTE(checkAccessForImplicitValueTypeCtor);
        COMPlusThrow(kMethodAccessException, W("Arg_MethodAccessException"));
    }
}

//
// Ensure that a method is accessable, throwing a MethodAccessException if not
//
// Arguments:
//    pCtx                  - current reflection context
//    pTargetMT             - class containing the method being checked
//    pInstanceMT           - instance being accessed if the method is not static
//    pTargetMethod         - method to check access to
//
// Return Value:
//    Nothing - throws an exception if access is not allowed
//

// static
void InvokeUtil::CheckAccessMethod(RefSecContext       *pCtx,
                                   MethodTable         *pTargetMT,
                                   MethodTable         *pInstanceMT,
                                   MethodDesc          *pTargetMethod)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(CheckPointer(pTargetMT));
        PRECONDITION(CheckPointer(pTargetMethod));
    }
    CONTRACTL_END;

    AccessCheckOptions accessCheckOptions(pCtx->GetAccessCheckType(),
                                          NULL,
                                          TRUE,
                                          pTargetMethod);
    InvokeUtil::CheckAccess(pCtx,
                            pTargetMT,
                            pInstanceMT,
                            pTargetMethod,
                            NULL,
                            accessCheckOptions);
}

//
// Ensure that a field is accessable, throwing a FieldAccessException if not
//
// Arguments:
//    pCtx                  - current reflection context
//    pTargetMT             - class containing the field being checked
//    pInstanceMT           - instance being accessed if the field is not static
//    pTargetField          - field to check access to
//
// Return Value:
//    Nothing - throws an exception if access is not allowed
//

// static
void InvokeUtil::CheckAccessField(RefSecContext       *pCtx,
                                  MethodTable         *pTargetMT,
                                  MethodTable         *pInstanceMT,
                                  FieldDesc           *pTargetField)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(CheckPointer(pTargetMT));
        PRECONDITION(CheckPointer(pTargetField));
    }
    CONTRACTL_END;

    AccessCheckOptions accessCheckOptions(pCtx->GetAccessCheckType(),
                                          NULL,
                                          TRUE,
                                          pTargetField);

    InvokeUtil::CheckAccess(pCtx,
                            pTargetMT,
                            pInstanceMT,
                            NULL,
                            pTargetField,
                            accessCheckOptions);
    }


//
// Check accessability of a field or method.
//
// Arguments:
//    pCtx                  - current reflection context
//    pTargetMT             - class containing the target being checked
//    pInstanceMT           - instance being accessed, if the field or method is non-NULL
//    pTargetMethod         - if checking access to a method, its MethodDesc
//    pTargetField          - if checking access to a field, its FieldDesc
//    accessCheckOptions    - CanAccess flags indicating how security demands should be done
//
// Return Value:
//    Nothing - throws an exception if access is not allowed
//
// Notes:
//    accessCheckOptions is required to be setup to throw if the target is inaccessable

// static
void InvokeUtil::CheckAccess(RefSecContext               *pCtx,
                             MethodTable                 *pTargetMT,
                             MethodTable                 *pInstanceMT,
                             MethodDesc                  *pTargetMethod,
                             FieldDesc                   *pTargetField,
                             const AccessCheckOptions    &accessCheckOptions)
{
    CONTRACTL
    {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
        PRECONDITION(CheckPointer(pTargetMT));
        PRECONDITION((pTargetMethod != NULL) ^ (pTargetField != NULL));
    }
    CONTRACTL_END;

    DWORD dwAttr = pTargetMethod != NULL ? pTargetMethod->GetAttrs() : pTargetField->GetAttributes();

    // Walking the stack is expensive so don't try to get the caller here.
    // ClassLoader::CanAccess will retrieve the caller when necessary
    // and it will give an interop (NULL) caller a pass.

    BOOL canAccess;

    canAccess = ClassLoader::CanAccess(pCtx,
                                       pTargetMT,
                                       pTargetMT->GetAssembly(),
                                       dwAttr,
                                       pTargetMethod,
                                       pTargetField,
                                       accessCheckOptions);
    if (pInstanceMT && canAccess)
    {
        if (pTargetMethod != NULL ? IsMdFamily(dwAttr) : IsFdFamily(dwAttr))
        {
            MethodTable* pCallerMT = pCtx->GetCallerMT();
            
            if (pCallerMT != NULL &&
                !ClassLoader::CanAccessFamilyVerification(pCallerMT, pInstanceMT))
            {
                canAccess = accessCheckOptions.DemandMemberAccessOrFail(pCtx,
                                                                        pInstanceMT,
                                                                        TRUE /*visibilityCheck*/);
            }
        }
    }

    // If this assert fires, ensure that accessCheckOptions was setup to throw if the target was inaccessable.
    _ASSERTE(canAccess);
}

/*static*/
AccessCheckOptions::AccessCheckType InvokeUtil::GetInvocationAccessCheckType(BOOL targetRemoted /*= FALSE*/)
{
    LIMITED_METHOD_CONTRACT;

    if (targetRemoted)
        return AccessCheckOptions::kMemberAccess;

    // Ignore transparency so that reflection invocation is consistenct with LCG.
    // There is no security concern because we are in Full Trust.
    return AccessCheckOptions::kRestrictedMemberAccessNoTransparency;
}

#endif // CROSSGEN_COMPILE

struct DangerousAPIEntry
{
    BinderClassID   classID;
    const LPCSTR    *pszAPINames;
    DWORD           cAPINames;
};

#define DEFINE_DANGEROUS_API(classID, szAPINames) static const LPCSTR g__ ## classID ## __DangerousAPIs[] = { szAPINames };
#include "dangerousapis.h"
#undef DEFINE_DANGEROUS_API

#define DEFINE_DANGEROUS_API(classID, szAPINames) { CLASS__ ## classID, g__ ## classID ## __DangerousAPIs, NumItems(g__ ## classID ## __DangerousAPIs)},
static const DangerousAPIEntry DangerousAPIs[] = 
{
#include "dangerousapis.h"
};
#undef DEFINE_DANGEROUS_API

/*static*/
bool InvokeUtil::IsDangerousMethod(MethodDesc *pMD)
{
    CONTRACTL {
        THROWS;
        GC_TRIGGERS;
        MODE_ANY;
    }
    CONTRACTL_END;

    MethodTable *pMT = pMD->GetMethodTable();

    if (pMT->GetModule()->IsSystem())
    {
        // All methods on these types are considered dangerous
        static const BinderClassID dangerousTypes[] = {
            CLASS__TYPE_HANDLE,
            CLASS__METHOD_HANDLE,
            CLASS__FIELD_HANDLE,
            CLASS__ACTIVATOR,
            CLASS__DELEGATE,
            CLASS__MULTICAST_DELEGATE,
            CLASS__RUNTIME_HELPERS
        };


        static bool fInited = false;

        if (!VolatileLoad(&fInited))
        {
            // Make sure all types are loaded so that we can use faster GetExistingClass()
            for (unsigned i = 0; i < NumItems(dangerousTypes); i++)
            {
                MscorlibBinder::GetClass(dangerousTypes[i]);
            }

            for (unsigned i = 0; i < NumItems(DangerousAPIs); i++)
            {
                MscorlibBinder::GetClass(DangerousAPIs[i].classID);
            }

            VolatileStore(&fInited, true);
        }

        for (unsigned i = 0; i < NumItems(dangerousTypes); i++)
        {
            if (MscorlibBinder::GetExistingClass(dangerousTypes[i]) == pMT)
                return true;
        }

        for (unsigned i = 0; i < NumItems(DangerousAPIs); i++)
        {
            DangerousAPIEntry entry = DangerousAPIs[i];
            if (MscorlibBinder::GetExistingClass(entry.classID) == pMT)
            {
                LPCUTF8 szMethodName = pMD->GetName();
                for (unsigned j = 0; j < entry.cAPINames; j++)
                {
                    if (strcmp(szMethodName, entry.pszAPINames[j]) == 0)
                        return true;
                }

                break;
            }
        }
    }

    // For reduce compat risks we treat non-ctors on DynamicMethod as safe.
    if (pMT->IsDelegate() && pMD->IsCtor())
        return true;

    return false;
}
