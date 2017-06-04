#include "classfactory.h"

CClassFactory::CClassFactory(PFN_CREATE_OBJ pfnCreateObject)
    : m_cRef(1)
    , m_pfnCreateObject(pfnCreateObject)
{
}

CClassFactory::~CClassFactory()
{
}

HRESULT STDMETHODCALLTYPE CClassFactory::QueryInterface(
    REFIID riid,
    void   **ppvObject)
{
    if (ppvObject == nullptr)
        return E_POINTER;

    // Pick the right v-table based on the IID passed in.
    if (riid == IID_IClassFactory)
    {
        *ppvObject = static_cast<IClassFactory*>(this);
    }
    else if (riid == IID_IUnknown)
    {
        *ppvObject = static_cast<IUnknown*>(this);
    }
    else
    {
        *ppvObject = nullptr;
        return E_NOINTERFACE;
    }

    // If successful, add a reference for out pointer and return.
    this->AddRef();

    return S_OK;
}

ULONG STDMETHODCALLTYPE CClassFactory::AddRef(void)
{
    return InterlockedIncrement(&m_cRef);
}

ULONG STDMETHODCALLTYPE CClassFactory::Release(void)
{
    LONG cRef = InterlockedDecrement(&m_cRef);
    if (cRef == 0)
        delete this;

    return cRef;
}

HRESULT STDMETHODCALLTYPE CClassFactory::CreateInstance(
    IUnknown *pUnkOuter,
    REFIID   riid,
    void     **ppvObject)
{
    // Avoid confusion.
    *ppvObject = NULL;

    // Aggregation is not supported by these objects.
    if (pUnkOuter != NULL)
        return CLASS_E_NOAGGREGATION;

    // Ask the object to create an instance of itself, and check the iid.
    return (*m_pfnCreateObject)(riid, ppvObject);
}

HRESULT STDMETHODCALLTYPE CClassFactory::LockServer(BOOL fLock)
{
    // NOTE: not need to implement.
    return E_FAIL;
}
