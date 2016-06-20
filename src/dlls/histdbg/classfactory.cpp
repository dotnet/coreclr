#include "classfactory.h"
#include "profiler.h"

ClassFactory::ClassFactory()
	: m_referenceCount(1)
{
}

ClassFactory::~ClassFactory()
{
}

HRESULT STDMETHODCALLTYPE ClassFactory::QueryInterface(REFIID riid, void **ppvObject)
{
	if (riid == IID_IUnknown || riid == IID_IClassFactory)
	{
		*ppvObject = this;
		this->AddRef();

		return S_OK;
	}

	*ppvObject = NULL;
	return E_NOINTERFACE;
}

ULONG STDMETHODCALLTYPE ClassFactory::AddRef(void)
{
	return __sync_fetch_and_add(&m_referenceCount, 1) + 1;
}

ULONG STDMETHODCALLTYPE ClassFactory::Release(void)
{
	LONG result = __sync_fetch_and_sub(&m_referenceCount, 1) - 1;
	if (result == 0)
	{
		delete this;
	}

	return result;
}

HRESULT STDMETHODCALLTYPE ClassFactory::CreateInstance(IUnknown *pUnkOuter, REFIID riid, void **ppvObject)
{
	if (riid == IID_ICorProfilerCallback2)
	{
		if (ppvObject != NULL)
			*ppvObject = new Profiler();

		return S_OK;
	}

	return E_NOINTERFACE;
}

HRESULT STDMETHODCALLTYPE ClassFactory::LockServer(BOOL fLock)
{
	return S_OK;
}