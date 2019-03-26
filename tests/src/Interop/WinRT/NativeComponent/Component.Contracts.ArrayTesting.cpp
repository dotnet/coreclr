#include "pch.h"
#include "Component.Contracts.ArrayTesting.h"
#include <numeric>

namespace winrt::Component::Contracts::implementation
{
    int32_t ArrayTesting::Sum(array_view<int32_t const> array)
    {
        return std::accumulate(array.begin(), array.end(), 0);
    }
}
