#include "pch.h"
#include "Component.Contracts.ArrayTesting.h"

namespace winrt::Component::Contracts::implementation
{
    int32_t ArrayTesting::Sum(array_view<int32_t const> array)
    {
        throw hresult_not_implemented();
    }
}
