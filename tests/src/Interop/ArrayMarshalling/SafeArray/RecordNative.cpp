// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include <xplatform.h>
#include <oleauto.h>
#include <algorithm>
#include "platformdefines.h"
#include "Helpers.h"

struct BlittableRecord
{
    int a;
};

class BlittableRecordInfo : public IRecordInfo
{
public:
    HRESULT GetField(PVOID pvData, LPCOLESTR szFieldName, VARIANT* pvarField)
    {
        if (pvData == nullptr || pvarField == nullptr)
        {
            return E_INVALIDARG;
        }

        BlittableRecord* pData = (BlittableRecord*)pvData;

        if (wcscmp(szFieldName, W("a")) == 0)
        {
            VariantClear(pvarField);
            V_VT(pvarField) = VT_I4;
            V_I4(pvarField) = pData->a;
            return S_OK;
        }
        return E_INVALIDARG;
    }

    HRESULT GetFieldNames(ULONG* pcNames, BSTR* rgBstrNames)
    {
        if (pcNames == nullptr)
        {
            return E_INVALIDARG;
        }
        if (rgBstrNames == nullptr)
        {
            *pcNames = 1;
            return S_OK;
        }

        if (*pcNames == 0)
        {
            return S_OK;
        }

        rgBstrNames[0] = TP_SysAllocString(W("a"));
        
        for(size_t i = 1; i < *pcNames; i++)
        {
            rgBstrNames[i] = nullptr;
        }
        
        return S_OK;
    }

    HRESULT GetFieldNoCopy(
        PVOID     pvData,
        LPCOLESTR szFieldName,
        VARIANT   *pvarField,
        PVOID     *ppvDataCArray
    )
    {
        return E_FAIL;
    }

    HRESULT GetGuid(GUID *pguid)
    {
        return E_FAIL;
    }

    HRESULT GetName(BSTR* pbstrName)
    {
        *pbstrName = TP_SysAllocString(W("BlittableRecord"));
        return S_OK;
    }

    HRESULT GetSize(ULONG* pcbSize)
    {
        *pcbSize = sizeof(BlittableRecord);
        return S_OK;
    }

    HRESULT GetTypeInfo(ITypeInfo** ppTypeInfo)
    {
        return TYPE_E_INVALIDSTATE;
    }

    BOOL IsMatchingType(IRecordInfo* pRecordInfo)
    {
        return pRecordInfo == this;
    }

    HRESULT PutField(
        ULONG     wFlags,
        PVOID     pvData,
        LPCOLESTR szFieldName,
        VARIANT   *pvarField
    )
    {
        return E_FAIL;
    }

    HRESULT PutFieldNoCopy(
        ULONG     wFlags,
        PVOID     pvData,
        LPCOLESTR szFieldName,
        VARIANT   *pvarField
    )
    {
        return E_FAIL;
    }

    HRESULT RecordClear(PVOID pvExisting)
    {
        return S_OK;
    }

    HRESULT RecordCopy(PVOID pvExisting, PVOID pvNew)
    {
        ((BlittableRecord*)pvNew)->a = ((BlittableRecord*)pvExisting)->a;
        return S_OK;
    }

    PVOID RecordCreate()
    {
        return CoreClrAlloc(sizeof(BlittableRecord));
    }

    HRESULT RecordCreateCopy(
        PVOID pvSource,
        PVOID *ppvDest
    )
    {
        *ppvDest = RecordCreate();
        return RecordCopy(pvSource, *ppvDest);
    }

    HRESULT RecordDestroy(PVOID pvRecord)
    {
        CoreClrFree(pvRecord);
        return S_OK;
    }

    HRESULT RecordInit(PVOID pvNew)
    {
        ((BlittableRecord*)pvNew)->a = 0;
        return S_OK;
    }

    ULONG AddRef()
    {
        return ++refCount;
    }

    ULONG Release()
    {
        return --refCount;
    }

    HRESULT QueryInterface(const IID& riid, void** ppvObject)
    {
        if (riid == __uuidof(IRecordInfo))
        {
            *ppvObject = static_cast<IRecordInfo*>(this);
        }
        else if (riid == __uuidof(IUnknown))
        {
            *ppvObject = static_cast<IUnknown*>(this);
        }
        else
        {
            *ppvObject = nullptr;
            return E_NOINTERFACE;
        }

        return S_OK;
    }

private:
    ULONG refCount;
} s_BlittableRecordInfo;

extern "C" DLL_EXPORT SAFEARRAY* CreateSafeArrayOfRecords(BlittableRecord records[], int numRecords)
{
    SAFEARRAYBOUND bounds[1] = {
        {numRecords, 0}
    };

    SAFEARRAY* arr = SafeArrayCreateEx(VT_RECORD, 1, bounds, &s_BlittableRecordInfo);

    memcpy(arr->pvData, records, numRecords * sizeof(BlittableRecord));

    return arr;
}
