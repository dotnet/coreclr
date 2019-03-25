#pragma once

#include "Component.Contracts.ArrayTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct ArrayTesting : ArrayTestingT<ArrayTesting>
    {
        ArrayTesting() = delete;

        int32_t Sum(array_view<int32_t const> array);
    };
}
