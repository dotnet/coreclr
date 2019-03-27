﻿#include "pch.h"
#include "winrt/base.h"
#include "Component.Contracts.KeyValuePairTesting.h"
#include <utility>
#include <vector>

template<typename K, typename V>
struct pair_wrapper : winrt::implements<pair_wrapper<K, V>, winrt::Windows::Foundation::Collections::IKeyValuePair<K, V>>
{
    pair_wrapper(K key, V value)
        :key(key),
        value(value)
    {}

    K Key()
    {
        return key;
    }

    V Value()
    {
        return value;
    }

private:
    K key;
    V value;
};

namespace winrt::Component::Contracts::implementation
{
    Windows::Foundation::Collections::IKeyValuePair<int32_t, int32_t> KeyValuePairTesting::MakeSimplePair(int32_t key, int32_t value)
    {
        return pair_wrapper<int32_t, int32_t>{key, value};
    }

    Windows::Foundation::Collections::IKeyValuePair<hstring, hstring> KeyValuePairTesting::MakeMarshaledPair(hstring const& key, hstring const& value)
    {
        return pair_wrapper<hstring, hstring>{key, value};
    }

    Windows::Foundation::Collections::IKeyValuePair<int32_t, Windows::Foundation::Collections::IIterable<int32_t>> KeyValuePairTesting::MakeProjectedPair(int32_t key, array_view<int32_t const> values)
    {
        return pair_wrapper<int32_t, Windows::Foundation::Collections::IIterable<int32_t>>{key, winrt::single_threaded_vector(std::vector<int32_t>(values.begin(), values.end()))};
    }
}
