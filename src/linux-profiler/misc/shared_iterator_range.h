#ifndef _SHARED_ITERATOR_RANGE_H_
#define _SHARED_ITERATOR_RANGE_H_

#include <utility>

#include "iterator_range.h"

template<typename Iterator, typename Lock>
class shared_iterator_range : public iterator_range<Iterator>
{
public:
    shared_iterator_range(Iterator begin, Iterator end, Lock &&lock)
        : iterator_range<Iterator>(begin, end)
        , m_lock(std::forward<Lock>(lock))
    {}

private:
    Lock m_lock;
};

// Deducing constructor wrappers.
template<typename Iterator, typename Lock>
inline shared_iterator_range<Iterator, Lock>
make_shared_iterator_range(Iterator begin, Iterator end, Lock &&lock)
{
    return shared_iterator_range<Iterator, Lock>(
        begin, end, std::forward<Lock>(lock));
}

#endif // _SHARED_ITERATOR_RANGE_H_
