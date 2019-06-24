// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// ZapReadyToRun.h
//

//
// Zapping of ready-to-run specific structures
// 
// ======================================================================================

#ifndef __ZAPREADYTORUN_H__
#define __ZAPREADYTORUN_H__

#include "readytorun.h"

class ZapReadyToRunHeader : public ZapNode
{
    struct Section
    {
        DWORD       type;
        ZapNode *   pSection;
    };

    SArray<Section> m_Sections;

    static int __cdecl SectionCmp(const void* a_, const void* b_)
    {
        return ((Section*)a_)->type - ((Section*)b_)->type;
    }

public:
    ZapReadyToRunHeader(ZapImage * pImage)
    {
    }

    void RegisterSection(DWORD type, ZapNode * pSection)
    {
        Section section;
        section.type = type;
        section.pSection = pSection;
        m_Sections.Append(section);
    }

    virtual DWORD GetSize()
    {
        return sizeof(READYTORUN_HEADER) + sizeof(READYTORUN_SECTION) * m_Sections.GetCount();
    }

    virtual UINT GetAlignment()
    {
        return sizeof(DWORD);
    }

    virtual ZapNodeType GetType()
    {
        return ZapNodeType_NativeHeader;
    }

    virtual void Save(ZapWriter * pZapWriter);

	DWORD EncodeModuleHelper(LPVOID compileContext, CORINFO_MODULE_HANDLE referencedModule);
};

class ZapReadyToRunDependencies : public ZapNode
{
    DWORD m_cDependencies;
    CORCOMPILE_DEPENDENCY* m_pDependencies;

public:
    ZapReadyToRunDependencies(CORCOMPILE_DEPENDENCY* pDependencies, DWORD cDependencies)
        : m_cDependencies(cDependencies), m_pDependencies(pDependencies)
    {
    }

    virtual DWORD GetSize()
    {
        return sizeof(READYTORUN_DEPENDENCY) * m_cDependencies;
    }

    virtual UINT GetAlignment()
    {
        return sizeof(ULARGE_INTEGER);
    }

    virtual ZapNodeType GetType()
    {
        return ZapNodeType_Dependencies;
    }

    virtual void Save(ZapWriter* pZapWriter)
    {
        for (DWORD i = 0; i < m_cDependencies; i++)
        {
            pZapWriter->Write(&m_pDependencies[i].dwAssemblyRef, sizeof(mdAssemblyRef));
            pZapWriter->Write(&m_pDependencies[i].signAssemblyDef.mvid, sizeof(GUID));
            static_assert(sizeof(READYTORUN_DEPENDENCY) == sizeof(mdAssemblyRef) + sizeof(GUID), "");
        }
    }
};
#endif // __ZAPREADYTORUN_H__
