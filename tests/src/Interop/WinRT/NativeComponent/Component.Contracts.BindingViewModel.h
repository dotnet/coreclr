#pragma once

#include "Component.Contracts.BindingViewModel.g.h"
#include <vector>

template<typename T, typename Container = std::vector<T>>
struct ObservableCollection : winrt::implements<ObservableCollection<T, Container>, winrt::Windows::UI::Xaml::Interop::INotifyCollectionChanged>
{
    ObservableCollection() = default;

    winrt::event_token CollectionChanged(winrt::Windows::UI::Xaml::Interop::NotifyCollectionChangedEventHandler const& handler)
    {
        return m_CollectionChangedEvent.add(handler);
    }

    void CollectionChanged(winrt::event_token const& token) noexcept
    {
        m_CollectionChangedEvent.remove(token);
    }

    void push_back(const T& value)
    {
        m_elements.push_back(value);
        winrt::Windows::UI::Xaml::Interop::NotifyCollectionChangedEventArgs args
        {
            winrt::Windows::UI::Xaml::Interop::NotifyCollectionChangedAction::Add,
            std::vector<T>{value},
            nullptr,
            m_elements.size() - 1,
            -1
        };
    }

private:
    Container m_elements;
    winrt::event<winrt::Windows::UI::Xaml::Interop::NotifyCollectionChangedEventHandler> m_CollectionChangedEvent;
};


namespace winrt::Component::Contracts::implementation
{
    struct BindingViewModel : BindingViewModelT<BindingViewModel>
    {
        BindingViewModel() = default;

        Windows::UI::Xaml::Interop::INotifyCollectionChanged Collection();
        void AddElement(int32_t i);
        hstring Name();
        void Name(hstring const& value);
        winrt::event_token PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler);
        void PropertyChanged(winrt::event_token const& token) noexcept;

    private:
        winrt::event<Windows::UI::Xaml::Data::PropertyChangedEventHandler> m_propertyChangedEvent;
        ObservableCollection<int> m_collection;
    };
}

namespace winrt::Component::Contracts::factory_implementation
{
    struct BindingViewModel : BindingViewModelT<BindingViewModel, implementation::BindingViewModel>
    {
    };
}
