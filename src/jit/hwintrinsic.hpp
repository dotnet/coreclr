// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************
*
*  Convert the type returned from the VM to a var_type using one to one mapping
*  without coalescing unsigned integral types into signed int or long.
*/

inline var_types JITtype2varTypeExact(CorInfoType type)
{

    static const var_types varTypeExactMap[CORINFO_TYPE_COUNT] = {
        // see the definition of enum CorInfoType in file inc/corinfo.h
        TYP_UNDEF,  // CORINFO_TYPE_UNDEF           = 0x0,
        TYP_VOID,   // CORINFO_TYPE_VOID            = 0x1,
        TYP_BOOL,   // CORINFO_TYPE_BOOL            = 0x2,
        TYP_USHORT, // CORINFO_TYPE_CHAR            = 0x3,
        TYP_BYTE,   // CORINFO_TYPE_BYTE            = 0x4,
        TYP_UBYTE,  // CORINFO_TYPE_UBYTE           = 0x5,
        TYP_SHORT,  // CORINFO_TYPE_SHORT           = 0x6,
        TYP_USHORT, // CORINFO_TYPE_USHORT          = 0x7,
        TYP_INT,    // CORINFO_TYPE_INT             = 0x8,
        TYP_UINT,   // CORINFO_TYPE_UINT            = 0x9,
        TYP_LONG,   // CORINFO_TYPE_LONG            = 0xa,
        TYP_ULONG,  // CORINFO_TYPE_ULONG           = 0xb,
        TYP_I_IMPL, // CORINFO_TYPE_NATIVEINT       = 0xc,
        TYP_U_IMPL, // CORINFO_TYPE_NATIVEUINT      = 0xd,
        TYP_FLOAT,  // CORINFO_TYPE_FLOAT           = 0xe,
        TYP_DOUBLE, // CORINFO_TYPE_DOUBLE          = 0xf,
        TYP_REF,    // CORINFO_TYPE_STRING          = 0x10,
        TYP_I_IMPL, // CORINFO_TYPE_PTR             = 0x11,
        TYP_BYREF,  // CORINFO_TYPE_BYREF           = 0x12,
        TYP_STRUCT, // CORINFO_TYPE_VALUECLASS      = 0x13,
        TYP_REF,    // CORINFO_TYPE_CLASS           = 0x14,
        TYP_STRUCT, // CORINFO_TYPE_REFANY          = 0x15,
        TYP_REF,    // CORINFO_TYPE_VAR             = 0x16,
    };

    // spot check to make certain enumerations have not changed

    assert(varTypeExactMap[CORINFO_TYPE_CLASS] == TYP_REF);
    assert(varTypeExactMap[CORINFO_TYPE_BYREF] == TYP_BYREF);
    assert(varTypeExactMap[CORINFO_TYPE_PTR] == TYP_I_IMPL);
    assert(varTypeExactMap[CORINFO_TYPE_INT] == TYP_INT);
    assert(varTypeExactMap[CORINFO_TYPE_UINT] == TYP_UINT);
    assert(varTypeExactMap[CORINFO_TYPE_LONG] == TYP_LONG);
    assert(varTypeExactMap[CORINFO_TYPE_ULONG] == TYP_ULONG);
    assert(varTypeExactMap[CORINFO_TYPE_DOUBLE] == TYP_DOUBLE);
    assert(varTypeExactMap[CORINFO_TYPE_VOID] == TYP_VOID);
    assert(varTypeExactMap[CORINFO_TYPE_VALUECLASS] == TYP_STRUCT);
    assert(varTypeExactMap[CORINFO_TYPE_REFANY] == TYP_STRUCT);

    assert(type < CORINFO_TYPE_COUNT);
    assert(varTypeExactMap[type] != TYP_UNDEF);

    return varTypeExactMap[type];
};
