#include "pch.h"
#include "Component.Contracts.KeyValuePairTesting.h"

namespace winrt::Component::Contracts::implementation
{
    Windows::Foundation::Collections::IKeyValuePair<int32_t, int32_t> KeyValuePairTesting::MakeSimplePair(int32_t key, int32_t value)
    {
        throw hresult_not_implemented();
    }

    Windows::Foundation::Collections::IKeyValuePair<hstring, hstring> KeyValuePairTesting::MakeMarshaledPair(hstring const& key, hstring const& value)
    {
        throw hresult_not_implemented();
    }

    Windows::Foundation::Collections::IKeyValuePair<int32_t, Windows::Foundation::Collections::IIterable<int32_t>> KeyValuePairTesting::MakeProjectedPair(int32_t key, array_view<int32_t const> values)
    {
        throw hresult_not_implemented();
    }
}
