include(CheckCSourceCompiles)
include(CheckSymbolExists)

set(CMAKE_REQUIRED_INCLUDES ${UTYPES_H} ${ICU_HOMEBREW_INC_PATH})

CHECK_C_SOURCE_COMPILES("
    #include <unicode/udat.h>
    int main(void) { enum UDateFormatSymbolType e = UDAT_STANDALONE_SHORTER_WEEKDAYS; }
" HAVE_UDAT_STANDALONE_SHORTER_WEEKDAYS)

CHECK_C_SOURCE_COMPILES("
#include <stdatomic.h>
int main(void)
{
    // check for https://bugs.llvm.org/show_bug.cgi?id=37457
    int tmp;
    atomic_store_explicit((_Atomic(int) *)&tmp, 0, memory_order_relaxed);
    return tmp;
}
" HAVE_WORKING_STDATOMIC)

if(NOT CLR_CMAKE_PLATFORM_DARWIN)
    set(CMAKE_REQUIRED_LIBRARIES ${ICUUC} ${ICUI18N})
else()
    set(CMAKE_REQUIRED_LIBRARIES ${ICUCORE})
endif()

check_symbol_exists(
    ucol_setMaxVariable
    "unicode/ucol.h"
    HAVE_SET_MAX_VARIABLE)

unset(CMAKE_REQUIRED_LIBRARIES)
unset(CMAKE_REQUIRED_INCLUDES)

configure_file(
    ${CMAKE_CURRENT_SOURCE_DIR}/config.h.in
    ${CMAKE_CURRENT_BINARY_DIR}/config.h)
