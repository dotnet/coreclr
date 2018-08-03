// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <stdlib.h>
#include <stdio.h>
#include <windows.h>
#include <string.h>
#include <mbstring.h>
#include <oleauto.h>
#include <xplatform.h>


extern "C" bool DLL_EXPORT __cdecl Char_In(char c)
{
    printf ("Char_In ");
    printf ("%c",c);
    printf ("\n");

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_InByRef(char* c)
{
    printf ("Char_InByRef ");
    printf ("%c",*c);
    printf ("\n");

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_InOutByRef(char* c)
{
    printf ("Char_InOutByRef ");
    printf ("%c",*c);
    printf ("\n");

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_In_String(char* c)
{
    printf ("native %s \n", c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_InByRef_String(char** c)
{
    printf ("native %s \n", *c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_InOutByRef_String(char** c)
{
    printf ("native %s \n", *c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_In_StringBuilder(char* c)
{
    printf ("native %s \n", c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_InByRef_StringBuilder(char** c)
{
    printf ("native %s \n", *c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl CharBuffer_InOutByRef_StringBuilder(char** c)
{
    printf ("native %s \n", *c);

    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_In_ArrayWithOffset (char* pArrayWithOffset)
{
    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_InOut_ArrayWithOffset (char* pArrayWithOffset)
{
    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_InByRef_ArrayWithOffset (char** ppArrayWithOffset)
{
    return TRUE;
}

extern "C" bool DLL_EXPORT __cdecl Char_InOutByRef_ArrayWithOffset (char** ppArrayWithOffset)
{
    return TRUE;
}
