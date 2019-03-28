#include "pch.h"
#include "Component.Contracts.BindingProjectionsTesting.h"
#include "Component.Contracts.BindingViewModel.h"
#include <winrt/Windows.UI.Xaml.Hosting.h>

namespace winrt::Component::Contracts::implementation
{
    Component::Contracts::IBindingViewModel BindingProjectionsTesting::CreateViewModel()
    {
        return make<BindingViewModel>();
    }

    Windows::Foundation::IClosable BindingProjectionsTesting::InitializeXamlFrameworkForCurrentThread()
    {
        return Windows::UI::Xaml::Hosting::WindowsXamlManager::InitializeForCurrentThread();
    }
}
