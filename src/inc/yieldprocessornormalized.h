// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

#include "contract.h"

const unsigned int MinNsPerNormalizedYield = 37; // measured typically 37-46 on post-Skylake
const unsigned int NsPerOptimalMaxSpinIterationDuration = 272; // approx. 900 cycles, measured 281 on pre-Skylake, 263 on post-Skylake

extern unsigned int g_yieldsPerNormalizedYield;
extern unsigned int g_optimalMaxNormalizedYieldsPerSpinIteration;

void InitializeYieldProcessorNormalizedCrst();
void EnsureYieldProcessorNormalizedInitialized();

class YieldProcessorNormalizationInfo
{
private:
    unsigned int yieldsPerNormalizedYield;
    unsigned int optimalMaxNormalizedYieldsPerSpinIteration;
    unsigned int optimalMaxYieldsPerSpinIteration;

public:
    YieldProcessorNormalizationInfo()
        : yieldsPerNormalizedYield(g_yieldsPerNormalizedYield),
        optimalMaxNormalizedYieldsPerSpinIteration(g_optimalMaxNormalizedYieldsPerSpinIteration),
        optimalMaxYieldsPerSpinIteration(yieldsPerNormalizedYield * optimalMaxNormalizedYieldsPerSpinIteration)
    {
    }

    friend void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &);
    friend void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &, unsigned int);
    friend void YieldProcessorNormalizedForPreSkylakeCount(const YieldProcessorNormalizationInfo &, unsigned int);
    friend void YieldProcessorWithBackOffNormalized(const YieldProcessorNormalizationInfo &, unsigned int);
};

FORCEINLINE void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &normalizationInfo)
{
    LIMITED_METHOD_CONTRACT;

    unsigned int n = normalizationInfo.yieldsPerNormalizedYield;
    _ASSERTE(n != 0);
    do
    {
        YieldProcessor();
    } while (--n != 0);
}

FORCEINLINE void YieldProcessorNormalized()
{
    WRAPPER_NO_CONTRACT;
    YieldProcessorNormalized(YieldProcessorNormalizationInfo());
}

FORCEINLINE void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &normalizationInfo, unsigned int count)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(count != 0);

    if (sizeof(SIZE_T) <= sizeof(unsigned int))
    {
        // On platforms with a small SIZE_T, prevent overflow on the multiply below. normalizationInfo.yieldsPerNormalizedYield
        // is limited to MinNsPerNormalizedYield by InitializeYieldProcessorNormalized().
        const unsigned int MaxCount = (unsigned int)SIZE_T_MAX / MinNsPerNormalizedYield;
        if (count > MaxCount)
        {
            count = MaxCount;
        }
    }

    SIZE_T n = (SIZE_T)count * normalizationInfo.yieldsPerNormalizedYield;
    _ASSERTE(n != 0);
    do
    {
        YieldProcessor();
    } while (--n != 0);
}

FORCEINLINE void YieldProcessorNormalized(unsigned int count)
{
    WRAPPER_NO_CONTRACT;
    YieldProcessorNormalized(YieldProcessorNormalizationInfo(), count);
}

// To be used for spin-wait loops that have not been retuned for recent processors, and where the yield count may be
// unreasonably high
FORCEINLINE void YieldProcessorNormalizedForPreSkylakeCount(
    const YieldProcessorNormalizationInfo &normalizationInfo,
    unsigned int preSkylakeCount)
{
    LIMITED_METHOD_CONTRACT;
    _ASSERTE(preSkylakeCount != 0);

    if (sizeof(SIZE_T) <= sizeof(unsigned int))
    {
        // On platforms with a small SIZE_T, prevent overflow on the multiply below. normalizationInfo.yieldsPerNormalizedYield
        // is limited to MinNsPerNormalizedYield by InitializeYieldProcessorNormalized().
        const unsigned int MaxCount = (unsigned int)SIZE_T_MAX / MinNsPerNormalizedYield;
        if (preSkylakeCount > MaxCount)
        {
            preSkylakeCount = MaxCount;
        }
    }

    const unsigned int PreSkylakeCountToSkylakeCountDivisor = 8;
    SIZE_T n = (SIZE_T)preSkylakeCount * normalizationInfo.yieldsPerNormalizedYield / PreSkylakeCountToSkylakeCountDivisor;
    if (n == 0)
    {
        n = 1;
    }
    do
    {
        YieldProcessor();
    } while (--n != 0);
}

FORCEINLINE void YieldProcessorNormalizedForPreSkylakeCount(unsigned int preSkylakeCount)
{
    WRAPPER_NO_CONTRACT;
    YieldProcessorNormalizedForPreSkylakeCount(YieldProcessorNormalizationInfo(), preSkylakeCount);
}

FORCEINLINE void YieldProcessorWithBackOffNormalized(
    const YieldProcessorNormalizationInfo &normalizationInfo,
    unsigned int spinIteration)
{
    LIMITED_METHOD_CONTRACT;

    // normalizationInfo.optimalMaxNormalizedYieldsPerSpinIteration cannot exceed the value below based on calculations done in
    // InitializeYieldProcessorNormalized()
    const unsigned int MaxOptimalMaxNormalizedYieldsPerSpinIteration =
        NsPerOptimalMaxSpinIterationDuration * 3 / (MinNsPerNormalizedYield * 2) + 1;
    _ASSERTE(normalizationInfo.optimalMaxNormalizedYieldsPerSpinIteration <= MaxOptimalMaxNormalizedYieldsPerSpinIteration);

    // This shift value should be adjusted based on the asserted condition below
    const UINT8 MaxShift = 3;
    static_assert_no_msg(((unsigned int)1 << (MaxShift + 1)) >= MaxOptimalMaxNormalizedYieldsPerSpinIteration);

    unsigned int n;
    if (spinIteration <= MaxShift &&
        ((unsigned int)1 << spinIteration) < normalizationInfo.optimalMaxNormalizedYieldsPerSpinIteration)
    {
        n = ((unsigned int)1 << spinIteration) * normalizationInfo.yieldsPerNormalizedYield;
    }
    else
    {
        n = normalizationInfo.optimalMaxYieldsPerSpinIteration;
    }
    _ASSERTE(n != 0);
    do
    {
        YieldProcessor();
    } while (--n != 0);
}
