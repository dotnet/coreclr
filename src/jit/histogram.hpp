// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#ifndef _HISTOGRAM_HPP_
#define _HISTOGRAM_HPP_

class histo
{
public:
                    histo(IAllocator* alloc, unsigned* sizeTab, unsigned sizeCnt = 0);
                   ~histo();

    void            histoClr();
    void            histoDsp(FILE* fout);
    void            histoRec(unsigned __int64 siz, unsigned cnt);
    void            histoRec(unsigned siz, unsigned cnt);

private:

    void            histoEnsureAllocated();

    IAllocator*     histoAlloc;
    unsigned        histoSizCnt;
    unsigned*       histoSizTab;
    unsigned*       histoCounts;
};

#endif
