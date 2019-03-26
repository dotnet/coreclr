#include "pch.h"
#include "Component.Contracts.UriTesting.h"

namespace winrt::Component::Contracts::implementation
{
    hstring UriTesting::GetFromUri(Windows::Foundation::Uri const& uri)
    {
        return uri.ToString();
    }

    Windows::Foundation::Uri UriTesting::CreateUriFromString(hstring const& uri)
    {
        return {uri};
    }
}
