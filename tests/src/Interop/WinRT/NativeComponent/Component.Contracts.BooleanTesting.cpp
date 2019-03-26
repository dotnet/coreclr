#include "pch.h"
#include "Component.Contracts.BooleanTesting.h"

namespace winrt::Component::Contracts::implementation
{
    bool BooleanTesting::And(bool left, bool right)
    {
        return left && right;
    }
}
