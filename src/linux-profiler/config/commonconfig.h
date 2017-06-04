#ifndef _COMMON_CONFIG_H_
#define _COMMON_CONFIG_H_

#include <stdexcept>

//
// Exceptions of type bad_conversion are thrown when value of some type
// can't be converted to another type. Exception should contain description
// of reason why convertion can't be performed.
//
class bad_conversion : public std::runtime_error {
    using std::runtime_error::runtime_error;
};

//
// Exceptions of type config_error are thrown during configuration is updated
// to prevent data inconsistency. It reports about errors in update process.
// Exception should contain description of the problem that causes update
// interruption.
//
// Also this type of exceptions is used in validation process.
//
class config_error : public std::runtime_error {
    using std::runtime_error::runtime_error;
};

//
// Common declaration of convert() template function specialized for various
// types. Function returns its argument converted to Target type that should
// be specified as template parameter.
//
template<typename Target, typename Source>
Target convert(Source);

#endif // _COMMON_CONFIG_H_
