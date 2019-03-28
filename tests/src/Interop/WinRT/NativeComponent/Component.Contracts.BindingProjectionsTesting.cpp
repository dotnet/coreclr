#include "pch.h"
#include "Component.Contracts.BindingProjectionsTesting.h"
#include "Component.Contracts.BindingViewModel.h"

namespace winrt::Component::Contracts::implementation
{
    Component::Contracts::BindingViewModel BindingProjectionsTesting::CreateViewModel()
    {
        return make<BindingViewModel>();
    }
}
