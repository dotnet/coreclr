#include <new>

#include <windows.h>

#include "classfactory.h"
#include "profilermanager.h"
#include "profiler.h"

#define  INITGUID
#include "guid.h"
#undef   INITGUID

// This structure is used to declare a global list of coclasses. The class
// factory object is created with a pointer to the correct one of these, so
// that when create instance is called, it can be created.
struct COCLASS_REGISTER
{
    const GUID     *pClsid;         // Class ID of the coclass.
    LPCWSTR        szProgID;        // Prog ID of the class.
    PFN_CREATE_OBJ pfnCreateObject; // Creation function to create instance.
};

// This map contains the list of coclasses which are exported from this module.
const COCLASS_REGISTER g_CoClasses[] =
{
//   pClsid           szProgID       pfnCreateObject
    {&CLSID_PROFILER, W("Profiler"), Profiler::CreateObject},
    {NULL,            NULL,          NULL                  }
};

// An optional entry point into this DLL.
EXTERN_C
BOOL WINAPI DllMain(
    HINSTANCE hinstDLL,    // A handle to the DLL module.
    DWORD     fdwReason,   // The reason code that indicates
                           // why the DLL entry-point function is being called.
    LPVOID    lpvReserved) // The reserved parameter provides additional info
                           // when the DLL is being loaded/unloaded.
{
    switch (fdwReason)
    {
    // The DLL is being loaded into the virtual address space of the current
    // process. The lpReserved parameter indicates whether the DLL is being
    // loaded statically (non-NULL) or dynamically (NULL).
    case DLL_PROCESS_ATTACH:
        // Disables the DLL_THREAD_ATTACH and DLL_THREAD_DETACH
        // notifications for this DLL.
        DisableThreadLibraryCalls(hinstDLL);
        break;

    // The DLL is being unloaded from the virtual address space of the calling
    // process. The lpReserved parameter indicates whether the DLL is being
    // unloaded as a result of a FreeLibrary call (NULL), a failure to load
    // (NULL), or process termination (non-NULL).
    case DLL_PROCESS_DETACH:
        // Notify the Profiler Manager about detach event.
        ProfilerManager::Instance().DllDetachShutdown();
        break;

    default:
        break;
    }

    return TRUE;
}

// Retrieves the class object from a DLL object handler or object application.
STDAPI DllGetClassObject(
    REFCLSID rclsid, // The class to desired.
    REFIID   riid,   // An interface wanted on the class factory.
    LPVOID   *ppv)   // Return the interface pointer here.
{
    HRESULT hr = CLASS_E_CLASSNOTAVAILABLE;
    CClassFactory *pClassFactory;     // To create class factory object.
    const COCLASS_REGISTER *pCoClass; // Loop control.

    // Scan for the right one.
    for (pCoClass = g_CoClasses; pCoClass->pClsid != NULL; pCoClass++)
    {
        if (*pCoClass->pClsid == rclsid)
        {
            // Allocate the new factory object.
            pClassFactory = new (std::nothrow) CClassFactory(
                pCoClass->pfnCreateObject);
            if (!pClassFactory)
                return E_OUTOFMEMORY;

            // Pick the v-table based on the caller's request.
            hr = pClassFactory->QueryInterface(riid, ppv);

            // Always release the local reference, if QI failed it will be
            // the only one and the object gets freed.
            pClassFactory->Release();
            break;
        }
    }

    return hr;
}
