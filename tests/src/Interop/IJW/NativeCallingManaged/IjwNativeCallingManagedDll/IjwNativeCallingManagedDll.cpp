#pragma managed
int ManagedCallee()
{
    return 100;
}

#pragma unmanaged
int NativeFunction()
{
    return ManagedCallee();
}

#pragma managed
public ref class TestClass
{
public:
    int ManagedEntryPoint()
    {
        return NativeFunction();
    }
};
