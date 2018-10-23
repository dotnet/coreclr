#include <xplatform.h>

#define NUM_ELEMENTS 10

struct BlittableFixedBuffer
{
    int elements[NUM_ELEMENTS];
};

struct NonBlittableFixedBuffer
{
    BOOL elements[NUM_ELEMENTS];
};

struct ElementAfterNonBlittableFixedBuffer
{
    char fixedString[NUM_ELEMENTS];
    int testValue;
};

extern "C" DLL_EXPORT int STDMETHODCALLTYPE SumBlittableFixedBuffer(BlittableFixedBuffer buffer)
{
    int sum = 0;
    
    for(size_t i = 0; i < NUM_ELEMENTS; i++)
    {
        sum += buffer.elements[i];
    }
    
    return sum;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE AggregateNonBlittableFixedBuffer(NonBlittableFixedBuffer buffer)
{
    BOOL aggregate = TRUE;

    
    for(size_t i = 0; i < NUM_ELEMENTS; i++)
    {
        aggregate = aggregate && buffer.elements[i];
    }
    
    return aggregate;
}

extern "C" DLL_EXPORT BOOL STDMETHODCALLTYPE VerifyElementAfterNonBlittableFixedBuffer(ElementAfterNonBlittableFixedBuffer buffer)
{
    return buffer.testValue == 42;
}
