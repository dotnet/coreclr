// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "jitpch.h"

#ifdef _MSC_VER
#pragma hdrstop
#endif

/*****************************************************************************
 *  Histogram class.
 */

histo::histo(IAllocator* alloc, unsigned * sizeTab, unsigned sizeCnt) :
    histoAlloc(alloc),
    histoCounts(nullptr)
{
    if (sizeCnt == 0)
    {
        do
        {
            sizeCnt++;
        }
        while ((sizeTab[sizeCnt] != 0) && (sizeCnt < 1000));
    }

    histoSizCnt = sizeCnt;
    histoSizTab = sizeTab;
}

histo::~histo()
{
    histoAlloc->Free(histoCounts);
}

// We need to lazy allocate the histogram data so static "histo" variables don't try to call the CLR memory allocator
// in the loader lock, which doesn't work.
void                histo::histoEnsureAllocated()
{
    if (histoCounts == nullptr)
    {
        histoCounts = new (histoAlloc) unsigned[histoSizCnt + 1];
        histoClr();
    }
}

void                histo::histoClr()
{
    histoEnsureAllocated();
    memset(histoCounts, 0, (histoSizCnt + 1) * sizeof(*histoCounts));
}

void                histo::histoDsp(FILE* fout)
{
    histoEnsureAllocated();

    unsigned        i;
    unsigned        c;
    unsigned        t;

    for (i = t = 0; i <= histoSizCnt; i++)
    {
        t += histoCounts[i];
    }

    for (i = c = 0; i <= histoSizCnt; i++)
    {
        if  (i == histoSizCnt)
        {
            if  (!histoCounts[i])
                break;

            fprintf(fout, "      >    %7u", histoSizTab[i-1]);
        }
        else
        {
            if (i == 0)
            {
                fprintf(fout, "     <=    ");
            }
            else
            {
                fprintf(fout, "%7u .. ", histoSizTab[i-1]+1);
            }

            fprintf(fout, "%7u", histoSizTab[i]);
        }

        c += histoCounts[i];

        fprintf(fout, " ===> %7u count (%3u%% of total)\n", histoCounts[i], (int)(100.0 * c / t));
    }
}

void                histo::histoRec(unsigned __int64 siz, unsigned cnt)
{
    assert(FitsIn<unsigned>(siz));
    histoRec((unsigned)siz, cnt);
}

void                histo::histoRec(unsigned siz, unsigned cnt)
{
    histoEnsureAllocated();

    unsigned        i;
    unsigned    *   t;

    for (i = 0, t = histoSizTab;
         i < histoSizCnt;
         i++  , t++)
    {
        if  (*t >= siz)
            break;
    }

    histoCounts[i] += cnt;
}

