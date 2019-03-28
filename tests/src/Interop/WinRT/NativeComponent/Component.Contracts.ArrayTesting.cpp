#include "pch.h"
#include "Component.Contracts.ArrayTesting.h"
#include <numeric>

namespace winrt::Component::Contracts::implementation
{
    int32_t ArrayTesting::Sum(array_view<int32_t const> array)
    {
        return std::accumulate(array.begin(), array.end(), 0);
    }

    bool ArrayTesting::Xor(array_view<bool const> array)
    {
        return std::accumulate(array.begin(), array.end(), false, [](bool left, bool right) { return left ^ right; });
    }
}
