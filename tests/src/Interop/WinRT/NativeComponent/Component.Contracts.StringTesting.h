#pragma once

#include "Component.Contracts.StringTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct StringTesting : StringTestingT<StringTesting>
    {
        StringTesting() = default;

        hstring ConcatStrings(hstring const& left, hstring const& right);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct StringTesting : StringTestingT<StringTesting, implementation::StringTesting>
    {
    };
}
