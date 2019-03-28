#pragma once

#include "Component.Contracts.ArrayTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct ArrayTesting : ArrayTestingT<ArrayTesting>
    {
        ArrayTesting() = default;

        int32_t Sum(array_view<int32_t const> array);
        bool Xor(array_view<bool const> array);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct ArrayTesting : ArrayTestingT<ArrayTesting, implementation::ArrayTesting>
    {
    };
}
