#ifndef _MAPPED_INFO_H_
#define _MAPPED_INFO_H_

#include "baseinfo.h"

template<typename ID>
struct MappedInfo : public BaseInfo
{
    ID id;
};

#endif // _MAPPED_INFO_H_
