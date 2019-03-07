// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "common.h"
#include "eventpipeprovider.h"
#include "eventpipesessionprovider.h"

#ifdef FEATURE_PERFTRACING

EventPipeSessionProvider::EventPipeSessionProvider(EventPipeSessionProvider &other) :
    EventPipeSessionProvider(
        other.m_pProviderName,
        other.m_keywords,
        other.m_loggingLevel,
        other.m_pFilterData)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;
}

EventPipeSessionProvider::EventPipeSessionProvider(
    LPCWSTR providerName,
    UINT64 keywords,
    EventPipeEventLevel loggingLevel,
    LPCWSTR filterData)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(providerName != NULL);
    }
    CONTRACTL_END;

    if (providerName != NULL)
    {
        size_t bufSize = wcslen(providerName) + 1;
        m_pProviderName = new WCHAR[bufSize];
        wcscpy_s(m_pProviderName, bufSize, providerName);
    }
    else
    {
        m_pProviderName = NULL;
    }

    m_keywords = keywords;
    m_loggingLevel = loggingLevel;

    if (filterData != NULL)
    {
        size_t bufSize = wcslen(filterData) + 1;
        m_pFilterData = new WCHAR[bufSize];
        wcscpy_s(m_pFilterData, bufSize, filterData);
    }
    else
    {
        m_pFilterData = NULL;
    }
}

EventPipeSessionProvider::~EventPipeSessionProvider()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    delete[] m_pProviderName;
    delete[] m_pFilterData;
}

EventPipeSessionProviderList::EventPipeSessionProviderList()
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pProviders = new SList<SListElem<EventPipeSessionProvider *>>();
    m_pCatchAllProvider = NULL;
}

EventPipeSessionProviderList::EventPipeSessionProviderList(
    const EventPipeSessionProviderList &other)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pProviders = new SList<SListElem<EventPipeSessionProvider *>>();
    m_pCatchAllProvider = NULL;

    SListElem<EventPipeSessionProvider *> *pElem = other.m_pProviders->GetHead();
    while (pElem != NULL)
    {
        EventPipeSessionProvider *pProvider = pElem->GetValue();
        m_pProviders->InsertTail(new SListElem<EventPipeSessionProvider*>(
            new EventPipeSessionProvider(*pProvider)));
        pElem = m_pProviders->GetNext(pElem);
    }
}

EventPipeSessionProviderList::EventPipeSessionProviderList(
    EventPipeProviderConfiguration *pConfigs,
    unsigned int numConfigs)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    m_pProviders = new SList<SListElem<EventPipeSessionProvider *>>();
    m_pCatchAllProvider = NULL;

    for (unsigned int i = 0; i < numConfigs; i++)
    {
        EventPipeProviderConfiguration *pConfig = &pConfigs[i];

        // Enable all events if the provider name == '*', all keywords are on and the requested level == verbose.
        if ((wcscmp(W("*"), pConfig->GetProviderName()) == 0) && (pConfig->GetKeywords() == 0xFFFFFFFFFFFFFFFF) && ((EventPipeEventLevel)pConfig->GetLevel() == EventPipeEventLevel::Verbose) && (m_pCatchAllProvider == NULL))
        {
            m_pCatchAllProvider = new EventPipeSessionProvider(NULL, 0xFFFFFFFFFFFFFFFF, EventPipeEventLevel::Verbose, NULL);
        }
        else
        {
            EventPipeSessionProvider *pProvider = new EventPipeSessionProvider(
                pConfig->GetProviderName(),
                pConfig->GetKeywords(),
                (EventPipeEventLevel)pConfig->GetLevel(),
                pConfig->GetFilterData());

            m_pProviders->InsertTail(new SListElem<EventPipeSessionProvider *>(pProvider));
        }
    }
}

EventPipeSessionProviderList::~EventPipeSessionProviderList()
{
    CONTRACTL
    {
        NOTHROW;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    if (m_pProviders != NULL)
    {
        SListElem<EventPipeSessionProvider *> *pElem = m_pProviders->GetHead();
        while (pElem != NULL)
        {
            EventPipeSessionProvider *pProvider = pElem->GetValue();
            delete pProvider;

            SListElem<EventPipeSessionProvider *> *pCurElem = pElem;
            pElem = m_pProviders->GetNext(pElem);
            delete pCurElem;
        }

        delete m_pProviders;
    }

    delete m_pCatchAllProvider;
}

void EventPipeSessionProviderList::AddSessionProvider(EventPipeSessionProvider *pProvider)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
        PRECONDITION(pProvider != NULL);
    }
    CONTRACTL_END;

    if(pProvider != NULL)
    {
        m_pProviders->InsertTail(new SListElem<EventPipeSessionProvider*>(pProvider));
    }
}

EventPipeSessionProvider* EventPipeSessionProviderList::GetSessionProvider(
    EventPipeProvider *pProvider)
{
    CONTRACTL
    {
        THROWS;
        GC_NOTRIGGER;
        MODE_ANY;
    }
    CONTRACTL_END;

    // Exists when tracing was enabled at start-up and all events were requested. This is a diagnostic config.
    if(m_pCatchAllProvider != NULL)
    {
        return m_pCatchAllProvider;
    }

    if(m_pProviders == NULL)
    {
        return NULL;
    }

    SString providerNameStr = pProvider->GetProviderName();
    LPCWSTR providerName = providerNameStr.GetUnicode();

    EventPipeSessionProvider *pSessionProvider = NULL;
    SListElem<EventPipeSessionProvider*> *pElem = m_pProviders->GetHead();
    while(pElem != NULL)
    {
        EventPipeSessionProvider *pCandidate = pElem->GetValue();
        if(wcscmp(providerName, pCandidate->GetProviderName()) == 0)
        {
            pSessionProvider = pCandidate;
            break;
        }
        pElem = m_pProviders->GetNext(pElem);
    }

    return pSessionProvider;
}

bool EventPipeSessionProviderList::IsEmpty() const
{
    LIMITED_METHOD_CONTRACT;

    return (m_pProviders->IsEmpty() && m_pCatchAllProvider == NULL);
}

#endif // FEATURE_PERFTRACING
