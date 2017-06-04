#ifndef _ITERATOR_RANGE_H_
#define _ITERATOR_RANGE_H_

#include <iterator>

template<typename Iterator>
class iterator_range
{
public:
    //
    // Types.
    //

    typedef typename std::iterator_traits<Iterator>::iterator_category
        iterator_category;
    typedef typename std::iterator_traits<Iterator>::value_type
        value_type;
    typedef typename std::iterator_traits<Iterator>::difference_type
        difference_type;
    typedef typename std::iterator_traits<Iterator>::reference
        reference;
    typedef typename std::iterator_traits<Iterator>::pointer
        pointer;

    //
    // Constructors.
    //

    iterator_range() = default;

    iterator_range(Iterator begin, Iterator end)
        : m_begin(begin)
        , m_end(end)
    {}

    //
    // Iterator access.
    //

    Iterator begin() const
    {
        return m_begin;
    }

    Iterator end() const
    {
        return m_end;
    }

private:
    Iterator m_begin;
    Iterator m_end;
};

// Deducing constructor wrappers.
template<typename Iterator>
inline iterator_range<Iterator>
make_iterator_range(Iterator begin, Iterator end)
{
    return iterator_range<Iterator>(begin, end);
}

#endif // _ITERATOR_RANGE_H_
