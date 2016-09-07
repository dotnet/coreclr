#ifndef _BASE_INFO_H_
#define _BASE_INFO_H_

#include <stddef.h>

// As struct we can use this type for overloading.
struct InternalID
{
    size_t id;
};

struct BaseInfo
{
    InternalID internalId;
};

#endif // _BASE_INFO_H_
