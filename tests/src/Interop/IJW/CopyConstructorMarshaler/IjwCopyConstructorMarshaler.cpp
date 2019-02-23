// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma managed
class A
{
    int copyCount;
    int i;

public:
    A()
    :copyCount(0)
    {};

    A(A& other)
    :copyCount(other.copyCount + 1)
    {}

    int GetI()
    {
        return i;
    }

    int GetCopyCount()
    {
        return copyCount;
    }
};

int Managed_GetCopyCount(A a)
{
    return a.GetCopyCount();
}

#pragma unmanaged

int GetCopyCount(A a)
{
    return a.GetCopyCount();
}

int GetCopyCount_ViaManaged(A a)
{
    return Managed_GetCopyCount(a);
}

#pragma managed

public ref class TestClass
{
public:
    int PInvokeNumCopies()
    {
        A a;
        return GetCopyCount(a);
    }

    int ReversePInvokeNumCopies()
    {
        A a;
        return GetCopyCount_ViaManaged(a);
    }
};
