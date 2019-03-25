#pragma once

#include "Component.Contracts.ExceptionTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct ExceptionTesting : ExceptionTestingT<ExceptionTesting>
    {
        ExceptionTesting() = default;

        void ThrowException(winrt::hresult const& hr);
        winrt::hresult GetException(int32_t hr);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct ExceptionTesting : ExceptionTestingT<ExceptionTesting, implementation::ExceptionTesting>
    {
    };
}
