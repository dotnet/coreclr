// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: diyfp.h
//

//

#ifndef _DIYFP_H
#define _DIYFP_H

#include <clrtypes.h>

// An exteneded floating-point data structure.
// It defines a 64-bit significand and a 32-bit exponent, 
// which is EXTENDED compare to IEEE double precision floating-point number. 
class DiyFp
{
public:
    DiyFp()
        : m_f(0), m_e()
    {
    }

    DiyFp(UINT64 f, int e)
        : m_f(f), m_e(e)
    {
    }

    DiyFp(const DiyFp& rhs)
        : m_f(rhs.m_f), m_e(rhs.m_e)
    {
    }

    DiyFp& operator=(const DiyFp& rhs)
    {
        m_f = rhs.m_f;
        m_e = rhs.m_e;

        return *this;
    }

    UINT64 f() const
    {
        return m_f;
    }

    int e() const
    {
        return m_e;
    }

    void SetSignificand(UINT64 f)
    {
        m_f = f;
    }

    void SetExponent(int e)
    {
        m_e = e;
    }

    void Minus(const DiyFp& rhs);
    static void Minus(const DiyFp& left, const DiyFp& right, DiyFp& result);

    void Multiply(const DiyFp& rhs);
    static void Multiply(const DiyFp& left, const DiyFp& right, DiyFp& result);

    static void GenerateNormalizedDiyFp(double value, DiyFp& result);

public:
    static const int SIGNIFICAND_LENGTH = 64;

private:
    UINT64 m_f;
    int m_e;
};

#endif