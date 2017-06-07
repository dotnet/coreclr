#include "ProfilerCommon.h"

class TestThreadNameChanged
{
    public:

        TestThreadNameChanged();

        HRESULT Verify(IPrfCom * pPrfCom);        

        #pragma region static_wrapper_methods

        static HRESULT ThreadNameChangedWrapper(IPrfCom * pPrfCom, ThreadID managedThreadId, ULONG cchName, WCHAR name[])
        {
            return STATIC_CLASS_CALL(TestThreadNameChanged)->ThreadNameChanged(pPrfCom, managedThreadId, cchName, name);
        }

        #pragma endregion

    private:

        #pragma region callback_handler_prototypes

        HRESULT ThreadNameChanged(IPrfCom * pPrfCom, ThreadID managedThreadId, ULONG cchName, WCHAR name[]);

        #pragma endregion

        ULONG m_ulSuccess;
        ULONG m_ulFailure;
};


/*
 *  Initialize the TestThreadNameChanged class.  
 */
TestThreadNameChanged::TestThreadNameChanged()
{
    // Initialize result counters
    m_ulSuccess   = 0;
    m_ulFailure   = 0;
}


HRESULT TestThreadNameChanged::ThreadNameChanged(IPrfCom * pPrfCom, ThreadID managedThreadId, ULONG cchName, WCHAR name[])
{
    if (cchName == 0x00)
    {
        if (name != NULL)
        {
            m_ulFailure++;
            FAILURE(L"Buffer passed with NULL cchName");
            return E_FAIL;
        }
        DISPLAY(L"ThreadNameChanged with NULL name.");
        m_ulSuccess++;
        return S_OK;
    }

    wstring threadName(name);
    if (threadName.length() != cchName)
    {
        m_ulFailure++;
        FAILURE(L"Reported and actual thread name lengths not equal. " << threadName.length() << " != " << cchName);
    }

    //DISPLAY(L"ThreadNameChanged Callback.  New Name: " << threadName);
	wcout << "ThreadNameChanged Callback.  New Name: " << threadName << endl;

    AppDomainID appDomainID = NULL;
    MUST_PASS(PINFO->GetThreadAppDomain(managedThreadId, &appDomainID));

    switch (PPRFCOM->m_ThreadNameChanged)
    {
        case 1:
            if(threadName != L"MainThread")
            {
                m_ulFailure++;
				wcout << "First names do not match " << threadName << L" != MainThread" << endl;
            }
            break;

        case 2:
            if(threadName != L"SecondThread")
            {
                m_ulFailure++;
				wcout << "Second names do not match " << threadName << L" != SecondThread" << endl;
            }
            break;

        case 3:
            if(threadName != L"ThirdThread")
            {
                m_ulFailure++;
				wcout << "Third names do not match " << threadName << L" != ThirdThread" < endl;
            }
            break;

        default:
            FAILURE(L"Extra ThreadNameChanged callback received.");
            break;
    }
    
    m_ulSuccess++;
    return S_OK;
}


HRESULT TestThreadNameChanged::Verify(IPrfCom * pPrfCom)
{
    DISPLAY(L"ThreadNameChanged Verification...\n");

    if ((m_ulFailure == 0) && (m_ulSuccess == 4))
    {
        DISPLAY(L"Test passed.")
        return S_OK;
    }
    else
    {
        FAILURE(L"Either some checks failed, or no successful checks were completed.\n" <<
                L"\tm_ulFailure = " << m_ulFailure << L" m_ulSuccess = " << m_ulSuccess);
        return E_FAIL;
    }
}


HRESULT ThreadNameChangedVerify(IPrfCom * pPrfCom)
{
    LOCAL_CLASS_POINTER(TestThreadNameChanged);
    MUST_PASS(pTestThreadNameChanged->Verify(pPrfCom))
    FREE_CLASS_POINTER(TestThreadNameChanged);
    return S_OK;
}

void ThreadNameChangedInit(IPrfCom * pPrfCom, PMODULEMETHODTABLE pModuleMethodTable)
{
    DISPLAY(L"Initialize ThreadNameChanged module\n");

    // Create and save an instance of test class
    SET_CLASS_POINTER(new TestThreadNameChanged());

    pModuleMethodTable->FLAGS = COR_PRF_MONITOR_THREADS;

    REGISTER_CALLBACK(VERIFY, ThreadNameChangedVerify);
    REGISTER_CALLBACK(THREADNAMECHANGED, TestThreadNameChanged::ThreadNameChangedWrapper);
    
    return;
}


