#include "pch.h"
#include "Component.Contracts.ExceptionTesting.h"

namespace winrt::Component::Contracts::implementation
{
    void ExceptionTesting::ThrowException(winrt::hresult const& hr)
    {
        throw hresult_not_implemented();
    }

    winrt::hresult ExceptionTesting::GetException(int32_t hr)
    {
        throw hresult_not_implemented();
    }
}
