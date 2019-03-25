#include "pch.h"
#include "Component.Contracts.TypeTesting.h"

namespace winrt::Component::Contracts::implementation
{
    hstring TypeTesting::GetTypeName(Windows::UI::Xaml::Interop::TypeName const& typeName)
    {
        throw hresult_not_implemented();
    }
}
