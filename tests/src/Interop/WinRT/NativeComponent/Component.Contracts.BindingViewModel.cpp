#include "pch.h"
#include "Component.Contracts.BindingViewModel.h"

namespace winrt::Component::Contracts::implementation
{
    Windows::UI::Xaml::Interop::INotifyCollectionChanged BindingViewModel::Collection()
    {
        throw hresult_not_implemented();
    }

    void BindingViewModel::AddElement(int32_t i)
    {
        throw hresult_not_implemented();
    }

    hstring BindingViewModel::Name()
    {
        throw hresult_not_implemented();
    }

    void BindingViewModel::Name(hstring const& value)
    {
        throw hresult_not_implemented();
    }

    winrt::event_token BindingViewModel::PropertyChanged(Windows::UI::Xaml::Data::PropertyChangedEventHandler const& handler)
    {
        return m_propertyChangedEvent.add(handler);
    }

    void BindingViewModel::PropertyChanged(winrt::event_token const& token) noexcept
    {
        m_propertyChangedEvent.remove(token);
    }
}
