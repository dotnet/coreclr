// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

#pragma once

template<class TInterface>
class CComPtr
{
private:
    TInterface* pointer;
public:
    CComPtr(const CComPtr&) = delete; // Copy constructor
    CComPtr& operator= (const CComPtr&) = delete; // Copy assignment
    CComPtr(CComPtr&&) = delete; // Move constructor
    CComPtr& operator= (CComPtr&&) = delete; // Move assignment

    void* operator new(std::size_t) = delete;
    void* operator new[](std::size_t) = delete;

    void operator delete(void *ptr) = delete;
    void operator delete[](void *ptr) = delete;

    CComPtr()
    {
        this->pointer = nullptr;
    }

    ~CComPtr()
    {
        if (this->pointer)
        {
            this->pointer->Release();
            this->pointer = nullptr;
        }
    }

    operator TInterface*()
    {
        return this->pointer;
    }

    operator TInterface*() const
    {
        return this->pointer;
    }

    TInterface& operator *()
    {
        return *this->pointer;
    }

    TInterface& operator *() const
    {
        return *this->pointer;
    }

    TInterface** operator&()
    {
        return &this->pointer;
    }

    TInterface** operator&() const
    {
        return &this->pointer;
    }

    TInterface* operator->()
    {
        return this->pointer;
    }

    TInterface* operator->() const
    {
        return this->pointer;
    }

    void Release()
    {
        this->~CComPtr();
    }
};