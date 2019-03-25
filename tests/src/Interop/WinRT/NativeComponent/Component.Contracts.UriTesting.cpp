#include "pch.h"
#include "Component.Contracts.UriTesting.h"

namespace winrt::Component::Contracts::implementation
{
    hstring UriTesting::GetFromUri(Windows::Foundation::Uri const& uri)
    {
        throw hresult_not_implemented();
    }

    Windows::Foundation::Uri UriTesting::CreateUriFromString(hstring const& uri)
    {
        throw hresult_not_implemented();
    }
}
