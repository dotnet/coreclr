#pragma once

#include "Component.Contracts.BindingViewModel.g.h"

namespace winrt::Component::Contracts::implementation
{
    struct BindingViewModel : BindingViewModelT<BindingViewModel>
    {
        BindingViewModel() = delete;

        Windows::UI::Xaml::Interop::INotifyCollectionChanged Collection();
        void AddElement(int32_t i);
        hstring Name();
        void Name(hstring const& value);
        winrt::event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(winrt::event_token const& token) noexcept;

    private:
        winrt::event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_propertyChangedEvent;
    };
}
