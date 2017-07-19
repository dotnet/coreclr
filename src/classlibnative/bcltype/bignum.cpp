// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: bignum.cpp
//

//

#include "bignum.h"
#include <cassert>

UINT32 BigNum::m_power10UInt32Table[UINT32POWER10NUM] =
{
    1,          // 10^0
    10,         // 10^1
    100,        // 10^2
    1000,       // 10^3
    10000,      // 10^4
    100000,     // 10^5
    1000000,    // 10^6
    10000000,   // 10^7
};

BigNum BigNum::m_power10BigNumTable[BIGPOWER10NUM];
BigNum::StaticInitializer BigNum::m_initializer;

BigNum::BigNum()
    :m_len(0)
{
}

BigNum::BigNum(UINT32 value)
{
    SetUInt32(value);
}

BigNum::BigNum(UINT64 value)
{
    SetUInt64(value);
}

BigNum::~BigNum()
{
}

BigNum& BigNum::operator=(const BigNum &rhs)
{
    UINT8 length = rhs.m_len;
    UINT32* pCurrent = m_blocks;
    const UINT32* pRhsCurrent = rhs.m_blocks;
    const UINT32* pRhsEnd = pRhsCurrent + length;

    while (pRhsCurrent != pRhsEnd)
    {
        *pCurrent = *pRhsCurrent;

        ++pCurrent;
        ++pRhsCurrent;
    }

    m_len = length;

    return *this;
}

int BigNum::Compare(const BigNum& lhs, UINT32 value)
{
    if (lhs.m_len == 0)
    {
        return value == 0 ? 0 : -1;
    }

    UINT32 lhsValue = lhs.m_blocks[0];

    if (lhsValue > value || lhs.m_len > 1)
    {
        return 1;
    }

    if (lhsValue < value)
    {
        return -1;
    }

    return 0;
}

int BigNum::Compare(const BigNum& lhs, const BigNum& rhs)
{
    int lenDiff = lhs.m_len - rhs.m_len;
    if (lenDiff != 0)
    {
        return lenDiff;
    }

    for (int i = lhs.m_len - 1; i >= 0; --i)
    {
        if (lhs.m_blocks[i] == rhs.m_blocks[i])
        {
            continue;
        }

        if (lhs.m_blocks[i] > rhs.m_blocks[i])
        {
            return 1;
        }
        else if (lhs.m_blocks[i] < rhs.m_blocks[i])
        {
            return -1;
        }
    }

    return 0;
}

void BigNum::ShiftLeft(UINT64 input, int shift, BigNum& output)
{
    int shiftBlocks = shift / 32;
    int remaningToShiftBits = shift % 32;

    for (int i = 0; i < shiftBlocks; ++i)
    {
        // If blocks shifted, we should fill the corresponding blocks with zero.
        output.ExtendBlock(0);
    }

    if (remaningToShiftBits == 0)
    {
        // We shift 32 * n (n >= 1) bits. No remaining bits.
        output.ExtendBlock((UINT32)(input & 0xFFFFFFFF));

        UINT32 highBits = (UINT32)(input >> 32);
        if (highBits != 0)
        {
            output.ExtendBlock(highBits);
        }
    }
    else
    {
        // Extract the high position bits which would be shifted out of range.
        UINT32 highPositionBits = (UINT32)input >> (32 + 32 - remaningToShiftBits);

        // Shift the input. The result should be stored to current block.
        UINT64 shiftedInput = input << remaningToShiftBits;
        output.ExtendBlock(shiftedInput & 0xFFFFFFFF);

        UINT32 highBits = (UINT32)(input >> 32);
        if (highBits != 0)
        {
            output.ExtendBlock(highBits);
        }

        if (highPositionBits != 0)
        {
            // If the high position bits is not 0, we should store them to next block.
            output.ExtendBlock(highPositionBits);
        }
    }
}

void BigNum::ShiftLeft(BigNum* pResult, UINT32 shift)
{
    UINT32 shiftBlocks = shift / 32;
    UINT32 shiftBits = shift % 32;

    // process blocks high to low so that we can safely process in place
    const UINT32* pInBlocks = pResult->m_blocks;
    int inLength = pResult->m_len;

    // check if the shift is block aligned
    if (shiftBits == 0)
    {
        // copy blocks from high to low
        for (UINT32 * pInCur = pResult->m_blocks + inLength, *pOutCur = pInCur + shiftBlocks;
            pInCur >= pInBlocks;
            --pInCur, --pOutCur)
        {
            *pOutCur = *pInCur;
        }

        // zero the remaining low blocks
        for (UINT32 i = 0; i < shiftBlocks; ++i)
        {
            pResult->m_blocks[i] = 0;
        }

        pResult->m_len += shiftBlocks;
    }
    // else we need to shift partial blocks
    else
    {
        int inBlockIdx = inLength - 1;
        UINT32 outBlockIdx = inLength + shiftBlocks;

        // set the length to hold the shifted blocks
        pResult->m_len = outBlockIdx + 1;

        // output the initial blocks
        const UINT32 lowBitsShift = (32 - shiftBits);
        UINT32 highBits = 0;
        UINT32 block = pResult->m_blocks[inBlockIdx];
        UINT32 lowBits = block >> lowBitsShift;
        while (inBlockIdx > 0)
        {
            pResult->m_blocks[outBlockIdx] = highBits | lowBits;
            highBits = block << shiftBits;

            --inBlockIdx;
            --outBlockIdx;

            block = pResult->m_blocks[inBlockIdx];
            lowBits = block >> lowBitsShift;
        }

        // output the final blocks
        pResult->m_blocks[outBlockIdx] = highBits | lowBits;
        pResult->m_blocks[outBlockIdx - 1] = block << shiftBits;

        // zero the remaining low blocks
        for (UINT32 i = 0; i < shiftBlocks; ++i)
        {
            pResult->m_blocks[i] = 0;
        }

        // check if the terminating block has no set bits
        if (pResult->m_blocks[pResult->m_len - 1] == 0)
        {
            --pResult->m_len;
        }
    }
}

void BigNum::Pow10(int exp, BigNum& result)
{
    BigNum temp1;
    BigNum temp2;

    BigNum* pCurrentTemp = &temp1;
    BigNum* pNextTemp = &temp2;

    UINT32 smallExp = exp & 0x7;
    pCurrentTemp->SetUInt32(m_power10UInt32Table[smallExp]);

    exp >>= 3;
    UINT32 idx = 0;

    while (exp != 0)
    {
        // if the current bit is set, multiply it with the corresponding power of 10
        if (exp & 1)
        {
            // multiply into the next temporary
            Multiply(*pCurrentTemp, m_power10BigNumTable[idx], *pNextTemp);

            // swap to the next temporary
            BigNum* t = pNextTemp;
            pNextTemp = pCurrentTemp;
            pCurrentTemp = t;
        }

        // advance to the next bit
        ++idx;
        exp >>= 1;
    }

    result = *pCurrentTemp;
}

void BigNum::PrepareHeuristicDivide(BigNum* pDividend, BigNum* pDivisor)
{
    UINT32 hiBlock = pDivisor->m_blocks[pDivisor->m_len - 1];
    if (hiBlock < 8 || hiBlock > 429496729)
    {
        // Inspired by http://www.ryanjuckett.com/programming/printing-floating-point-numbers/
        // Perform a bit shift on all values to get the highest block of the divisor into
        // the range [8,429496729]. We are more likely to make accurate quotient estimations
        // in heuristicDivide() with higher divisor values so
        // we shift the divisor to place the highest bit at index 27 of the highest block.
        // This is safe because (2^28 - 1) = 268435455 which is less than 429496729. This means
        // that all values with a highest bit at index 27 are within range.
        UINT32 hiBlockLog2 = LogBase2(hiBlock);
        UINT32 shift = (59 - hiBlockLog2) % 32;

        BigNum::ShiftLeft(pDivisor, shift);
        BigNum::ShiftLeft(pDividend, shift);
    }
}

UINT32 BigNum::HeuristicDivide(BigNum* pDividend, const BigNum& divisor)
{
    UINT8 len = divisor.m_len;
    if (pDividend->m_len < len)
    {
        return 0;
    }

    const UINT32* pFinalDivisorBlock = divisor.m_blocks + len - 1;
    UINT32* pFinalDividendBlock = pDividend->m_blocks + len - 1;

    // This is an estimated quotient. Its error should be less than 2.
    // Reference inequality:
    // a/b - floor(floor(a)/(floor(b) + 1)) < 2
    UINT32 quotient = *pFinalDividendBlock / (*pFinalDivisorBlock + 1);

    if (quotient != 0)
    {
        // Now we use our estimated quotient to update each block of dividend.
        // dividend = dividend - divisor * quotient
        const UINT32 *pDivisorCurrent = divisor.m_blocks;
        UINT32 *pDividendCurrent = pDividend->m_blocks;

        UINT64 borrow = 0;
        UINT64 carry = 0;
        do
        {
            UINT64 product = (UINT64)*pDivisorCurrent * (UINT64)quotient + carry;
            carry = product >> 32;

            UINT64 difference = (UINT64)*pDividendCurrent - (product & 0xFFFFFFFF) - borrow;
            borrow = (difference >> 32) & 1;

            *pDividendCurrent = difference & 0xFFFFFFFF;

            ++pDivisorCurrent;
            ++pDividendCurrent;
        } while (pDivisorCurrent <= pFinalDivisorBlock);

        // Remove all leading zero blocks from dividend
        while (len > 0 && pDividend->m_blocks[len - 1] == 0)
        {
            --len;
        }

        pDividend->m_len = len;
    }

    // If the dividend is still larger than the divisor, we overshot our estimate quotient. To correct,
    // we increment the quotient and subtract one more divisor from the dividend (Because we guaranteed the error range).
    if (BigNum::Compare(*pDividend, divisor) >= 0)
    {
        ++quotient;

        // dividend = dividend - divisor
        const UINT32 *pDivisorCur = divisor.m_blocks;
        UINT32 *pDividendCur = pDividend->m_blocks;

        UINT64 borrow = 0;
        do
        {
            UINT64 difference = (UINT64)*pDividendCur - (UINT64)*pDivisorCur - borrow;
            borrow = (difference >> 32) & 1;

            *pDividendCur = difference & 0xFFFFFFFF;

            ++pDivisorCur;
            ++pDividendCur;
        } while (pDivisorCur <= pFinalDivisorBlock);

        // Remove all leading zero blocks from dividend
        while (len > 0 && pDividend->m_blocks[len - 1] == 0)
        {
            --len;
        }

        pDividend->m_len = len;
    }

    return quotient;
}

void BigNum::Multiply(UINT32 value)
{
    Multiply(*this, value, *this);
}

void BigNum::Multiply(const BigNum& value)
{
    BigNum temp;
    BigNum::Multiply(*this, value, temp);

    memcpy(m_blocks, temp.m_blocks, ((UINT32)temp.m_len) * sizeof(UINT32));
    m_len = temp.m_len;
}

void BigNum::Multiply(const BigNum& lhs, UINT32 value, BigNum& result)
{
    if (lhs.m_len == 0)
    {
        return;
    }

    if (lhs.m_len < BIGSIZE)
    {
        // Set the highest + 1 bit to zero so that
        // we can check if there's a carry later.
        result.m_blocks[lhs.m_len] = 0;
    }

    const UINT32* pCurrent = lhs.m_blocks;
    const UINT32* pEnd = pCurrent + lhs.m_len;
    UINT32* pResultCurrent = result.m_blocks;

    UINT64 carry = 0;
    while (pCurrent != pEnd)
    {
        UINT64 product = (UINT64)(*pCurrent) * (UINT64)value + carry;
        carry = product >> 32;
        *pResultCurrent = (UINT32)(product & 0xFFFFFFFF);

        ++pResultCurrent;
        ++pCurrent;
    }

    if (lhs.m_len < BIGSIZE && result.m_blocks[lhs.m_len] != 0)
    {
        result.m_len = lhs.m_len + 1;
    }
    else
    {
        result.m_len = lhs.m_len;
    }
}

void BigNum::Multiply(const BigNum& lhs, const BigNum& rhs, BigNum& result)
{
    const BigNum* pLarge = NULL;
    const BigNum* pSmall = NULL;
    if (lhs.m_len < rhs.m_len)
    {
        pSmall = &lhs;
        pLarge = &rhs;
    }
    else
    {
        pSmall = &rhs;
        pLarge = &lhs;
    }

    UINT8 maxResultLength = pSmall->m_len + pLarge->m_len;

    // Zero out result internal blocks.
    memset(result.m_blocks, 0, sizeof(UINT32) * BIGSIZE);

    const UINT32* pLargeBegin = pLarge->m_blocks;
    const UINT32* pLargeEnd = pLarge->m_blocks + pLarge->m_len;

    UINT32* pResultStart = result.m_blocks;
    const UINT32* pSmallCurrent = pSmall->m_blocks;
    const UINT32* pSmallEnd = pSmallCurrent + pSmall->m_len;

    while (pSmallCurrent != pSmallEnd)
    {
        // Multiply each block of large BigNum.
        if (*pSmallCurrent != 0)
        {
            const UINT32* pLargeCurrent = pLargeBegin;
            UINT32* pResultCurrent = pResultStart;
            UINT64 carry = 0;

            do
            {
                UINT64 product = (UINT64)(*pResultCurrent) + (UINT64)(*pSmallCurrent) * (UINT64)(*pLargeCurrent) + carry;
                carry = product >> 32;
                *pResultCurrent = (UINT32)(product & 0xFFFFFFFF);

                ++pResultCurrent;
                ++pLargeCurrent;
            } while (pLargeCurrent != pLargeEnd);

            *pResultCurrent = (UINT32)(carry & 0xFFFFFFFF);
        }

        ++pSmallCurrent;
        ++pResultStart;
    }

    if (maxResultLength > 0 && result.m_blocks[maxResultLength - 1] == 0)
    {
        result.m_len = maxResultLength - 1;
    }
    else
    {
        result.m_len = maxResultLength;
    }
}

bool BigNum::IsZero()
{
    if (m_len == 0)
    {
        return true;
    }

    for (UINT8 i = 0; i < m_len; ++i)
    {
        if (m_blocks[i] != 0)
        {
            return false;
        }
    }

    return true;
}

void BigNum::SetUInt32(UINT32 value)
{
    m_len = 1;
    m_blocks[0] = value;
}

void BigNum::SetUInt64(UINT64 value)
{
    m_len = 0;
    m_blocks[0] = (UINT32)(value & 0xFFFFFFFF);
    m_len++;

    UINT32 highBits = (UINT32)(value >> 32);
    if (highBits != 0)
    {
        m_blocks[1] = highBits;
        m_len++;
    }
}

void BigNum::ExtendBlock(UINT32 newBlock)
{
    m_blocks[m_len] = newBlock;
    ++m_len;
}

UINT32 BigNum::LogBase2(UINT32 val)
{
    static const UINT8 logTable[256] =
    {
        0, 0, 1, 1, 2, 2, 2, 2, 3, 3, 3, 3, 3, 3, 3, 3,
        4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5, 5,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6, 6,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7,
        7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7, 7
    };

    UINT32 temp = val >> 24;
    if (temp != 0)
    {
        return 24 + logTable[temp];
    }

    temp = val >> 16;
    if (temp != 0)
    {
        return 16 + logTable[temp];
    }

    temp = val >> 8;
    if (temp != 0)
    {
        return 8 + logTable[temp];
    }

    return logTable[val];
}

UINT32 BigNum::LogBase2(UINT64 val)
{
    UINT64 temp = val >> 32;
    if (temp != 0)
    {
        return 32 + LogBase2((UINT32)temp);
    }

    return LogBase2((UINT32)val);
}
