#include "pch.h"
#include "Component.Contracts.ExceptionTesting.h"

namespace winrt::Component::Contracts::implementation
{
    void ExceptionTesting::ThrowException(winrt::hresult const& hr)
    {
        winrt::throw_hresult(hr);
    }

    winrt::hresult ExceptionTesting::GetException(int32_t hr)
    {
        return {hr};
    }
}
