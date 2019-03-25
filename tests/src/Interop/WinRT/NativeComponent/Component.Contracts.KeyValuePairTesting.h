#pragma once

#include "Component.Contracts.KeyValuePairTesting.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct KeyValuePairTesting : KeyValuePairTestingT<KeyValuePairTesting>
    {
        KeyValuePairTesting() = default;

        Windows::Foundation::Collections::IKeyValuePair<int32_t, int32_t> MakeSimplePair(int32_t key, int32_t value);
        Windows::Foundation::Collections::IKeyValuePair<hstring, hstring> MakeMarshaledPair(hstring const& key, hstring const& value);
        Windows::Foundation::Collections::IKeyValuePair<int32_t, Windows::Foundation::Collections::IIterable<int32_t>> MakeProjectedPair(int32_t key, array_view<int32_t const> values);
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct KeyValuePairTesting : KeyValuePairTestingT<KeyValuePairTesting, implementation::KeyValuePairTesting>
    {
    };
}
