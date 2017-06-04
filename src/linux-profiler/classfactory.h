#ifndef _CLASS_FACTORY_H_
#define _CLASS_FACTORY_H_

#include <unknwn.h>

// This typedef is for a function which will create a new instance of an object.
typedef HRESULT (*PFN_CREATE_OBJ)(REFIID riid, void **ppvObject);

// One class factory object satifies all of our clsid's, to reduce overall
// code bloat.
class CClassFactory : public IClassFactory
{
public:
    CClassFactory(PFN_CREATE_OBJ pfnCreateObject);

private:
    CClassFactory(); // Can't use without data.

    virtual ~CClassFactory(); // Factory should be destroyed through public API.

public:
    //
    // IUnknown methods.
    //

    virtual HRESULT STDMETHODCALLTYPE QueryInterface(
        REFIID riid,
        void   **ppvObject);

    virtual ULONG STDMETHODCALLTYPE AddRef();

    virtual ULONG STDMETHODCALLTYPE Release();

    //
    // IClassFactory methods.
    //

    virtual HRESULT STDMETHODCALLTYPE CreateInstance(
        IUnknown *pUnkOuter,
        REFIID   riid,
        void     **ppvObject);

    virtual HRESULT STDMETHODCALLTYPE LockServer(
        BOOL fLock);

private:
    LONG           m_cRef;            // Reference count.
    PFN_CREATE_OBJ m_pfnCreateObject; // Creation function for an instance.
};

#endif // _CLASS_FACTORY_H_
