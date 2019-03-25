#pragma once

#include "Component.Contracts.NullableTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct NullableTesting : NullableTestingT<NullableTesting>
    {
        NullableTesting() = default;

        bool IsNull(Windows::Foundation::IReference<int32_t> const& value);
        int32_t GetIntValue(Windows::Foundation::IReference<int32_t> const& value);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct NullableTesting : NullableTestingT<NullableTesting, implementation::NullableTesting>
    {
    };
}
