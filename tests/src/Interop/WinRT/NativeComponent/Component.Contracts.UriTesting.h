#pragma once

#include "Component.Contracts.UriTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct UriTesting : UriTestingT<UriTesting>
    {
        UriTesting() = default;

        hstring GetFromUri(Windows::Foundation::Uri const& uri);
        Windows::Foundation::Uri CreateUriFromString(hstring const& uri);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct UriTesting : UriTestingT<UriTesting, implementation::UriTesting>
    {
    };
}
