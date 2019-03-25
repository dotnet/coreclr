#pragma once

#include "Component.Contracts.BooleanTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct BooleanTesting : BooleanTestingT<BooleanTesting>
    {
        BooleanTesting() = default;

        bool And(bool left, bool right);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct BooleanTesting : BooleanTestingT<BooleanTesting, implementation::BooleanTesting>
    {
    };
}
