#include "pch.h"
#include "Component.Contracts.StringTesting.h"

namespace winrt::Component::Contracts::implementation
{
    hstring StringTesting::ConcatStrings(hstring const& left, hstring const& right)
    {
        throw hresult_not_implemented();
    }
}
