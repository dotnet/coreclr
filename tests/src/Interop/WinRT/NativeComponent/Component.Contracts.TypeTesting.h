#pragma once

#include "Component.Contracts.TypeTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct TypeTesting : TypeTestingT<TypeTesting>
    {
        TypeTesting() = default;

        hstring GetTypeName(Windows::UI::Xaml::Interop::TypeName const& typeName);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct TypeTesting : TypeTestingT<TypeTesting, implementation::TypeTesting>
    {
    };
}
