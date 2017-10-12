// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma once

extern int g_yieldsPerNormalizedYield;
extern int g_optimalMaxNormalizedYieldsPerSpinIteration;

void InitializeYieldProcessorNormalizedCrst();
void InitializeYieldProcessorNormalized();
void EnsureYieldProcessorNormalizedInitialized();

class YieldProcessorNormalizationInfo
{
private:
    int yieldsPerNormalizedYield;

public:
    YieldProcessorNormalizationInfo() : yieldsPerNormalizedYield(g_yieldsPerNormalizedYield)
    {
    }

    friend void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &);
};

FORCEINLINE void YieldProcessorNormalized(const YieldProcessorNormalizationInfo &normalizationInfo)
{
    LIMITED_METHOD_CONTRACT;

    int n = normalizationInfo.yieldsPerNormalizedYield;
    while (--n >= 0)
    {
        YieldProcessor();
    }
}

class YieldProcessorWithBackOffNormalizationInfo
{
private:
    int yieldsPerNormalizedYield;
    int optimalMaxNormalizedYieldsPerSpinIteration;
    int optimalMaxYieldsPerSpinIteration;

public:
    YieldProcessorWithBackOffNormalizationInfo()
        : yieldsPerNormalizedYield(g_yieldsPerNormalizedYield),
        optimalMaxNormalizedYieldsPerSpinIteration(g_optimalMaxNormalizedYieldsPerSpinIteration),
        optimalMaxYieldsPerSpinIteration(yieldsPerNormalizedYield * optimalMaxNormalizedYieldsPerSpinIteration)
    {
    }

    friend void YieldProcessorWithBackOffNormalized(const YieldProcessorWithBackOffNormalizationInfo &, unsigned int);
};

FORCEINLINE void YieldProcessorWithBackOffNormalized(
    const YieldProcessorWithBackOffNormalizationInfo &normalizationInfo,
    unsigned int spinIteration)
{
    LIMITED_METHOD_CONTRACT;

    int n;
    if (spinIteration <= 30 && (1 << spinIteration) < normalizationInfo.optimalMaxNormalizedYieldsPerSpinIteration)
    {
        n = (1 << spinIteration) * normalizationInfo.yieldsPerNormalizedYield;
    }
    else
    {
        n = normalizationInfo.optimalMaxYieldsPerSpinIteration;
    }
    while (--n >= 0)
    {
        YieldProcessor();
    }
}
