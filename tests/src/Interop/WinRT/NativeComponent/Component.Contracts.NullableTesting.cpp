#include "pch.h"
#include "Component.Contracts.NullableTesting.h"

namespace winrt::Component::Contracts::implementation
{
    bool NullableTesting::IsNull(Windows::Foundation::IReference<int32_t> const& value)
    {
        throw hresult_not_implemented();
    }

    int32_t NullableTesting::GetIntValue(Windows::Foundation::IReference<int32_t> const& value)
    {
        throw hresult_not_implemented();
    }
}
