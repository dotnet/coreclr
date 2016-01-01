//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information. 
//

/*++



Module Name:

    palinternal.h

Abstract:

    Rotor Platform Adaptation Layer (PAL) header file used by source
    file part of the PAL implementation. This is a wrapper over 
    unix/inc/pal.h. It allows avoiding name collisions when including 
    system header files, and it allows redirecting calls to 'standard' functions
    to their PAL counterpart

Details :

A] Rationale (see B] for the quick recipe)
There are 2 types of namespace collisions that must be handled.

1) standard functions declared in pal.h, which do not need to be 
   implemented in the PAL because the system's implementation is sufficient.

   (examples : memcpy, strlen, fclose)

   The problem with these is that a prototype for them is provided both in 
   pal.h and in a system header (stdio.h, etc). If a PAL file needs to 
   include the files containing both prototypes, the compiler may complain 
   about the multiple declarations.

   To avoid this, the inclusion of pal.h must be wrapped in a 
   #define/#undef pair, which will effectiveily "hide" the pal.h 
   declaration by renaming it to something else. this is done by palinternal.h
   in this way :

   #define some_function DUMMY_some_function
   #include <pal.h>
   #undef some_function

   when a PAL source file includes palinternal.h, it will see a prototype for 
   DUMMY_some_function instead of some_function; so when it includes the 
   system header with the "real" prototype, no collision occurs.

   (note : technically, no functions should ever be treated this way, all 
   system functions should be wrapped according to method 2, so that call 
   logging through ENTRY macros is done for all functions n the PAL. However 
   this reason alone is not currently considered enough to warrant a wrapper)

2) standard functions which must be reimplemented by the PAL, because the 
   system's implementation does not offer suitable functionnality.
   
   (examples : widestring functions, networking)
   
   Here, the problem is more complex. The PAL must provide functions with the 
   same name as system functions. Due to the nature of Unix dynamic linking, 
   if this is done, the PAL's implementation will effectively mask the "real" 
   function, so that all calls are directed to it. This makes it impossible for
   a function to be implemented as calling its counterpart in the system, plus 
   some extra work, because instead of calling the system's implementation, the
   function would only call itself in an infinitely recursing nightmare. Even 
   worse, if by bad luck the system libraries attempt to call the function for 
   which the PAL provides an implementation, it is the PAL's version that will 
   be called.
   It is therefore necessary to give the PAL's implementation of such functions
   a different name. However, PAL consumers (applications built on top of the 
   PAL) must be able to call the function by its 'official' name, not the PAL's 
   internal name. 
   This can be done with some more macro magic, by #defining the official name 
   to the internal name *in pal.h*. :

   #define some_function PAL_some_function

   This way, while PAL consumer code can use the official name, it is the 
   internal name that wil be seen at compile time.
   However, one extra step is needed. While PAL consumers must use the PAL's 
   implementation of these functions, the PAL itself must still have access to
   the "real" functions. This is done by #undefining in palinternal.h the names
   #defined in pal.h :

   #include <pal.h>
   #undef some_function.

   At this point, code in the PAL implementation can access *both* its own 
   implementation of the function (with PAL_some_function) *and* the system's 
   implementation (with some_function)

    [side note : for the Win32 PAL, this can be accomplished without touching 
    pal.h. In Windows, symbols in in dynamic libraries are resolved at 
    compile time. if an application that uses some_function is only linked to 
    pal.dll, some_function will be resolved to the version in that DLL, 
    even if other DLLs in the system provide other implementations. In addition,
    the function in the DLL can actually have a different name (e.g. 
    PAL_some_function), to which the 'official' name is aliased when the DLL 
    is compiled. All this is not possible with Unix dynamic linking, where 
    symbols are resolved at run-time in a first-found-first-used order. A 
    module may end up using the symbols from a module it was never linked with,
    simply because that module was located somewhere in the dependency chain. ]

    It should be mentionned that even if a function name is not documented as 
    being implemented in the system, it can still cause problems if it exists. 
    This is especially a problem for functions in the "reserved" namespace 
    (names starting with an underscore : _exit, etc). (We shouldn't really be 
    implementing functions with such a name, but we don't really have a choice)
    If such a case is detected, it should be wrapped according to method 2

    Note that for all this to work, it is important for the PAL's implementation
    files to #include palinternal.h *before* any system files, and to never 
    include pal.h directly.

B] Procedure for name conflict resolution :

When adding a function to pal.h, which is implemented by the system and 
which does not need a different implementation :

- add a #define function_name DUMMY_function_name to palinternal.h, after all 
  the other DUMMY_ #defines (above the #include <pal.h> line)
- add the function's prototype to pal.h (if that isn't already done)
- add a #undef function_name to palinternal.h near all the other #undefs 
  (after the #include <pal.h> line)
  
When overriding a system function with the PAL's own implementation :

- add a #define function_name PAL_function_name to pal.h, somewhere 
  before the function's prototype, inside a #ifndef _MSCVER/#endif pair 
  (to avoid affecting the Win32 build)
- add a #undef function_name to palinternal.h near all the other #undefs 
  (after the #include <pal.h> line)
- implement the function in the pal, naming it PAL_function_name
- within the PAL, call PAL_function_name() to call the PAL's implementation, 
function_name() to call the system's implementation



--*/

#ifndef _PAL_INTERNAL_H_
#define _PAL_INTERNAL_H_

#define PAL_IMPLEMENTATION

/* Include our configuration information so it's always present when
   compiling PAL implementation files. */
#include "config.h"

#ifdef DEBUG
#define _ENABLE_DEBUG_MESSAGES_ 1
#else
#define _ENABLE_DEBUG_MESSAGES_ 0
#endif

#ifdef PAL_PERF
#include "pal_perf.h"
#endif

/* C runtime functions needed to be renamed to avoid duplicate definition
   of those functions when including standard C header files */
#define div DUMMY_div
#define div_t DUMMY_div_t
#define memcpy DUMMY_memcpy 
#define memcmp DUMMY_memcmp 
#define memset DUMMY_memset 
#define memmove DUMMY_memmove 
#define memchr DUMMY_memchr
#define strlen DUMMY_strlen
#define strnlen DUMMY_strnlen
#define stricmp DUMMY_stricmp 
#define strstr DUMMY_strstr 
#define strcmp DUMMY_strcmp 
#define strcat DUMMY_strcat
#define strncat DUMMY_strncat
#define strcpy DUMMY_strcpy
#define strcspn DUMMY_strcspn
#define strncmp DUMMY_strncmp
#define strncpy DUMMY_strncpy
#define strchr DUMMY_strchr
#define strrchr DUMMY_strrchr 
#define strpbrk DUMMY_strpbrk
#define strtod DUMMY_strtod
#define strspn DUMMY_strspn
#if HAVE__SNPRINTF
#define _snprintf DUMMY__snprintf
#endif /* HAVE__SNPRINTF */
#if HAVE__SNWPRINTF
#define _snwprintf DUMMY__snwprintf
#endif  /* HAVE__SNWPRINTF */
#define tolower DUMMY_tolower
#define toupper DUMMY_toupper
#define islower DUMMY_islower
#define isupper DUMMY_isupper
#define isprint DUMMY_isprint
#define isdigit DUMMY_isdigit
#define srand DUMMY_srand
#define atoi DUMMY_atoi
#define atof DUMMY_atof
#define time DUMMY_time
#define tm PAL_tm
#define size_t DUMMY_size_t
#define time_t PAL_time_t
#define va_list DUMMY_va_list
#define abs DUMMY_abs
#define labs DUMMY_labs
#define llabs DUMMY_llabs

// 7.12.4 Trigonometric Functions
#define acos        DUMMY_acos
#define acosf       DUMMY_acosf

#define asin        DUMMY_asin
#define asinf       DUMMY_asinf

#define atan        DUMMY_atan
#define atanf       DUMMY_atanf

#define atan2       DUMMY_atan2
#define atan2f      DUMMY_atan2f

#define cos         DUMMY_cos
#define cosf        DUMMY_cosf

#define sin         DUMMY_sin
#define sinf        DUMMY_sinf

#define tan         DUMMY_tan
#define tanf        DUMMY_tanf

// 7.12.5 Hyperbolic Functions
#define acosh       DUMMY_acosh
#define acoshf      DUMMY_acoshf

#define asinh       DUMMY_asinh
#define asinhf      DUMMY_asinhf

#define atanh       DUMMY_atanh
#define atanhf      DUMMY_atanhf

#define cosh        DUMMY_cosh
#define coshf       DUMMY_coshf

#define sinh        DUMMY_sinh
#define sinhf       DUMMY_sinhf

#define tanh        DUMMY_tanh
#define tanhf       DUMMY_tanhf

// 7.12.6 Exponential and Logarithmic Functions
#define exp         DUMMY_exp
#define expf        DUMMY_expm1

#define exp2        DUMMY_exp2
#define exp2f       DUMMY_exp2f

#define expm1       DUMMY_expm1
#define expm1f      DUMMY_expm1f

#define frexp       DUMMY_frexp
#define frexpf      DUMMY_frexpf

#define ilogb       DUMMY_ilogb
#define ilogbf      DUMMY_ilogbf

#define ldexp       DUMMY_ldexp
#define ldexpf      DUMMY_ldexpf

#define log         DUMMY_log
#define logf        DUMMY_logf

#define log10       DUMMY_log10
#define log10f      DUMMY_log10f

#define log1p       DUMMY_log1p
#define log1pf      DUMMY_log1pf

#define log2        DUMMY_log2
#define log2f       DUMMY_log2f

#define logb        DUMMY_logb
#define logbf       DUMMY_logbf

#define modf        DUMMY_modf
#define modff       DUMMY_modff

#define scalbn      DUMMY_scalbn
#define scalbnf     DUMMY_scalbnf

#define scalbln     DUMMY_scalbln
#define scalblnf    DUMMY_scalblnf

// 7.12.7 Power and Absolute-Value Functions
#define cbrt        DUMMY_cbrt
#define cbrtf       DUMMY_cbrtf

#define fabs        DUMMY_fabs
#define fabsf       DUMMY_fabsf

#define hypot       DUMMY_hypot
#define hypotf      DUMMY_hypotf

#define pow         DUMMY_pow
#define powf        DUMMY_powf

#define sqrt        DUMMY_sqrt
#define sqrtf       DUMMY_sqrtf

// 7.12.8 Error and Gamma Functions
#define erf         DUMMY_erf
#define erff        DUMMY_erff

#define erfc        DUMMY_erfc
#define erfcf       DUMMY_erfcf

#define lgamma      DUMMY_lgamma
#define lgammaf     DUMMY_lgammaf

#define tgamma      DUMMY_tgamma
#define tgammaf     DUMMY_tgammaf

// 7.12.9 Nearest Integer Functions
#define ceil        DUMMY_ceil
#define ceilf       DUMMY_ceilf

#define floor       DUMMY_floor
#define floorf      DUMMY_floorf

#define nearbyint   DUMMY_nearbyint
#define nearbyintf  DUMMY_nearbyintf

#define rint        DUMMY_rint
#define rintf       DUMMY_rintf

#define lrint       DUMMY_lrint
#define lrintf      DUMMY_lrintf

#define llrint      DUMMY_llrint
#define llrintf     DUMMY_llrintf

#define round       DUMMY_round
#define roundf      DUMMY_roundf

#define lround      DUMMY_lround
#define lroundf     DUMMY_lroundf

#define llround     DUMMY_llround
#define llroundf    DUMMY_llroundf

#define trunc       DUMMY_trunc
#define truncf      DUMMY_truncf
    
// 7.12.10 Remainder Functions
#define fmod        DUMMY_fmod
#define fmodf       DUMMY_fmodf

#define remainder   DUMMY_remainder
#define remainderf  DUMMY_remainderf

#define remquo      DUMMY_remquo
#define remquof     DUMMY_remquof

// 7.12.11 Manipulation Functions
#define copysign    DUMMY_copysign
#define copysignf   DUMMY_copysignf

#define nan         DUMMY_nan
#define nanf        DUMMY_nanf

#define nextafter   DUMMY_nextafter
#define nextafterf  DUMMY_nextafterf

#define nexttoward  DUMMY_nexttoward
#define nexttowardf DUMMY_nexttoward

// 7.12.12 Maximum, Minimum, and Positive Difference Functions
#define fdim        DUMMY_fdim
#define fdimf       DUMMY_fdimf

#define fmax        DUMMY_fmax
#define fmaxf       DUMMY_fmaxf

#define fmin        DUMMY_fmin
#define fminf       DUMMY_fminf

// 7.12.13 Floating Multiply-Add Function
#define fma         DUMMY_fma
#define fmaf        DUMMY_fmaf

/* RAND_MAX needed to be renamed to avoid duplicate definition when including 
   stdlib.h header files. PAL_RAND_MAX should have the same value as RAND_MAX 
   defined in pal.h  */
#define PAL_RAND_MAX 0x7fff

/* The standard headers define isspace and isxdigit as macros and functions,
   To avoid redefinition problems, undefine those macros. */
#ifdef isspace
#undef isspace
#endif
#ifdef isxdigit
#undef isxdigit
#endif
#ifdef isalpha
#undef isalpha
#endif
#ifdef isalnum
#undef isalnum
#endif
#define isspace DUMMY_isspace 
#define isxdigit DUMMY_isxdigit
#define isalpha DUMMY_isalpha
#define isalnum DUMMY_isalnum

#ifdef stdin
#undef stdin
#endif
#ifdef stdout
#undef stdout
#endif
#ifdef stderr
#undef stderr
#endif

#ifdef SCHAR_MIN
#undef SCHAR_MIN
#endif
#ifdef SCHAR_MAX
#undef SCHAR_MAX
#endif
#ifdef SHRT_MIN
#undef SHRT_MIN
#endif
#ifdef SHRT_MAX
#undef SHRT_MAX
#endif
#ifdef UCHAR_MAX
#undef UCHAR_MAX
#endif
#ifdef USHRT_MAX
#undef USHRT_MAX
#endif
#ifdef ULONG_MAX
#undef ULONG_MAX
#endif
#ifdef LONG_MIN
#undef LONG_MIN
#endif
#ifdef LONG_MAX
#undef LONG_MAX
#endif
#ifdef RAND_MAX
#undef RAND_MAX
#endif
#ifdef DBL_MAX
#undef DBL_MAX
#endif
#ifdef FLT_MAX
#undef FLT_MAX
#endif
#ifdef __record_type_class
#undef __record_type_class
#endif
#ifdef __real_type_class
#undef __real_type_class
#endif

// The standard headers define va_start and va_end as macros,
// To avoid redefinition problems, undefine those macros.
#ifdef va_start
#undef va_start
#endif
#ifdef va_end
#undef va_end
#endif
#ifdef va_copy
#undef va_copy
#endif


#ifdef _VAC_
#define wchar_16 wchar_t
#else
#define wchar_t wchar_16
#endif // _VAC_

#define ptrdiff_t PAL_ptrdiff_t
#define intptr_t PAL_intptr_t
#define uintptr_t PAL_uintptr_t
#define timeval PAL_timeval
#define FILE PAL_FILE
#define fpos_t PAL_fpos_t

#include "pal.h"

#include "mbusafecrt.h"

#ifdef _VAC_
#undef CHAR_BIT
#undef va_arg
#endif

#if !defined(_MSC_VER) && defined(FEATURE_PAL) && defined(_WIN64)
#undef _BitScanForward64
#endif 

/* pal.h does "#define alloca _alloca", but we need access to the "real"
   alloca */
#undef alloca

/* Undef all functions and types previously defined so those functions and
   types could be mapped to the C runtime and socket implementation of the 
   native OS */
#undef exit
#undef alloca
#undef atexit
#undef div
#undef div_t
#undef memcpy
#undef memcmp
#undef memset
#undef memmove
#undef memchr
#undef strlen
#undef strnlen
#undef stricmp
#undef strstr
#undef strcmp
#undef strcat
#undef strcspn
#undef strncat
#undef strcpy
#undef strncmp
#undef strncpy
#undef strchr
#undef strrchr
#undef strpbrk
#undef strtoul
#undef strtod
#undef strspn
#undef strtok
#undef strdup
#undef tolower
#undef toupper
#undef islower
#undef isupper
#undef isprint
#undef isdigit
#undef isspace
#undef iswdigit
#undef iswxdigit
#undef iswalpha
#undef iswprint
#undef isxdigit
#undef isalpha
#undef isalnum
#undef atoi
#undef atol
#undef atof
#undef malloc
#undef realloc
#undef free
#undef qsort
#undef bsearch
#undef time
#undef tm
#undef localtime
#undef mktime
#undef FILE
#undef fclose
#undef setbuf
#undef fopen
#undef fread
#undef feof
#undef ferror
#undef ftell
#undef fflush
#undef fwrite
#undef fgets
#undef fgetws
#undef fputc
#undef putchar
#undef fputs
#undef fseek
#undef fgetpos
#undef fsetpos
#undef getcwd
#undef getc
#undef fgetc
#undef ungetc
#undef _flushall
#undef setvbuf
#undef mkstemp
#undef rename
#undef unlink
#undef size_t
#undef time_t
#undef va_list
#undef va_start
#undef va_end
#undef va_copy
#undef stdin
#undef stdout
#undef stderr
#undef abs
#undef labs
#undef llabs
#undef rand
#undef srand
#undef errno
#undef getenv 
#undef wcsspn
#undef open
#undef glob

// 7.12.0 General Macros
#undef INFINITY
#undef NAN

// 7.12.3 Classification Macros
#undef isfinite
#undef isinf
#undef isnan

// 7.12.4 Trigonometric Functions
#undef acos
#undef acosf

#undef asin
#undef asinf

#undef atan
#undef atanf

#undef atan2
#undef atan2f

#undef cos
#undef cosf

#undef sin
#undef sinf

#undef tan
#undef tanf

// 7.12.5 Hyperbolic Functions
#undef acosh
#undef acoshf

#undef asinh
#undef asinhf

#undef atanh
#undef atanhf

#undef cosh
#undef coshf

#undef sinh
#undef sinhf

#undef tanh
#undef tanhf

// 7.12.6 Exponential and Logarithmic Functions
#undef exp
#undef expf

#undef exp2
#undef exp2f

#undef expm1
#undef expm1f

#undef frexp
#undef frexpf

#undef ilogb
#undef ilogbf

#undef ldexp
#undef ldexpf

#undef log
#undef logf

#undef log10
#undef log10f

#undef log1p
#undef log1pf

#undef log2
#undef log2f

#undef logb
#undef logbf

#undef modf
#undef modff

#undef scalbn
#undef scalbnf

#undef scalbln
#undef scalblnf

// 7.12.7 Power and Absolute-Value Functions
#undef cbrt
#undef cbrtf

#undef fabs
#undef fabsf

#undef hypot
#undef hypotf

#undef pow
#undef powf

#undef sqrt
#undef sqrtf

// 7.12.8 Error and Gamma Functions
#undef erf
#undef erff

#undef erfc
#undef erfcf

#undef lgamma
#undef lgammaf

#undef tgamma
#undef tgammaf

// 7.12.9 Nearest Integer Functions
#undef ceil
#undef ceilf

#undef floor
#undef floorf

#undef nearbyint
#undef nearbyintf

#undef rint
#undef rintf

#undef lrint
#undef lrintf

#undef llrint
#undef llrintf

#undef round
#undef roundf

#undef lround
#undef lroundf

#undef llround
#undef llroundf

#undef trunc
#undef truncf
    
// 7.12.10 Remainder Functions
#undef fmod
#undef fmodf

#undef remainder
#undef remainderf

#undef remquo
#undef remquof

// 7.12.11 Manipulation Functions
#undef copysign
#undef copysignf

#undef nan
#undef nanf

#undef nextafter
#undef nextafterf

#undef nexttoward
#undef nexttowardf

// 7.12.12 Maximum, Minimum, and Positive Difference Functions
#undef fdim
#undef fdimf

#undef fmax
#undef fmaxf

#undef fmin
#undef fminf

// 7.12.13 Floating Multiply-Add Function
#undef fma
#undef fmaf

#undef wchar_t
#undef ptrdiff_t
#undef intptr_t
#undef uintptr_t
#undef timeval
#undef fpos_t


#undef printf
#undef fprintf
#undef fwprintf
#undef vfprintf
#undef vfwprintf
#undef vprintf
#undef wprintf
#undef sprintf
#undef swprintf
#undef _snprintf
#if HAVE__SNWPRINTF
#undef _snwprintf
#endif  /* HAVE__SNWPRINTF */
#undef sscanf
#undef wcstod
#undef wcstol
#undef wcstoul
#undef _wcstoui64
#undef wcscat
#undef wcscpy
#undef wcslen
#undef wcsncmp
#undef wcschr
#undef wcsrchr
#undef wsprintf
#undef swscanf
#undef wcspbrk
#undef wcsstr
#undef wcscmp
#undef wcsncat
#undef wcsncpy
#undef wcstok
#undef wcscspn
#undef iswupper
#undef iswspace
#undef towlower
#undef towupper
#undef vsprintf
#undef vswprintf
#undef _vsnprintf
#undef _vsnwprintf
#undef vsnprintf
#undef wvsnprintf

#ifdef _AMD64_ 
#undef _mm_getcsr
#undef _mm_setcsr
#endif // _AMD64_

#undef ctime

#undef SCHAR_MIN
#undef SCHAR_MAX
#undef UCHAR_MAX
#undef SHRT_MIN
#undef SHRT_MAX
#undef USHRT_MAX
#undef LONG_MIN
#undef LONG_MAX
#undef ULONG_MAX
#undef RAND_MAX
#undef DBL_MAX
#undef FLT_MAX
#undef __record_type_class
#undef __real_type_class

#if HAVE_CHAR_BIT
#undef CHAR_BIT
#endif

// We need a sigsetjmp prototype in pal.h for the SEH macros, but we
// can't use the "real" prototype (because we don't want to define sigjmp_buf).
// So we must rename the "real" sigsetjmp to avoid redefinition errors.
#define sigsetjmp REAL_sigsetjmp
#define siglongjmp REAL_siglongjmp
#include <setjmp.h>
#undef sigsetjmp
#undef siglongjmp

#undef _SIZE_T_DEFINED
#undef _WCHAR_T_DEFINED

#define _DONT_USE_CTYPE_INLINE_
#if HAVE_RUNETYPE_H
#include <runetype.h>
#endif
#include <ctype.h>

#define _WITH_GETLINE
#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <sys/types.h>
#include <pwd.h>
#include <unistd.h>
#include <fcntl.h>
#include <glob.h>

#ifdef __APPLE__

#undef GetCurrentThread
#include <CoreServices/CoreServices.h>

#include <malloc/malloc.h>

#endif // __APPLE__

/* we don't really need this header here, but by including it we make sure
   we'll catch any definition conflicts */
#include <sys/socket.h>

#if !HAVE_INFTIM
#define INFTIM  -1
#endif // !HAVE_INFTIM

#if (__GNUC__ >= 4)
#define OffsetOf(TYPE, MEMBER) __builtin_offsetof(TYPE, MEMBER)
#else
#define OffsetOf(s, f) (INT)(SIZE_T)&(((s*)0)->f)
#endif /* __GNUC__ version check*/

#undef assert
#define assert (Use__ASSERTE_instead_of_assert) assert

#define PROCESS_PIPE_NAME_PREFIX ".dotnet-pal-processpipe"

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

typedef enum _TimeConversionConstants
{
    tccSecondsToMillieSeconds       = 1000,         // 10^3
    tccSecondsToMicroSeconds        = 1000000,      // 10^6
    tccSecondsToNanoSeconds         = 1000000000,   // 10^9
    tccMillieSecondsToMicroSeconds  = 1000,         // 10^3
    tccMillieSecondsToNanoSeconds   = 1000000,      // 10^6
    tccMicroSecondsToNanoSeconds    = 1000,         // 10^3
    tccSecondsTo100NanoSeconds      = 10000000,     // 10^7
    tccMicroSecondsTo100NanoSeconds = 10            // 10^1
} TimeConversionConstants;

#ifdef __cplusplus
}

/* This is duplicated in utilcode.h for CLR, with cooler type-traits */
template <typename T>
inline
T* InterlockedExchangePointerT(
    T* volatile *Target,
    T* Value)
{
    return (T*)(InterlockedExchangePointer(
        (PVOID volatile*)Target,
        (PVOID)Value));
}

template <typename T>
inline
T* InterlockedCompareExchangePointerT(
    T* volatile *destination,
    T* exchange,
    T* comparand)
{
    return (T*)(InterlockedCompareExchangePointer(
        (PVOID volatile*)destination,
        (PVOID)exchange,
        (PVOID)comparand));
}

template <typename T>
inline T* InterlockedExchangePointerT(
    T* volatile * target,
    int           value) // When NULL is provided as argument.
{
    //STATIC_ASSERT(value == 0);
    return InterlockedExchangePointerT(target, reinterpret_cast<T*>(value));
}

template <typename T>
inline T* InterlockedCompareExchangePointerT(
    T* volatile * destination,
    int           exchange,  // When NULL is provided as argument.
    T*            comparand)
{
    //STATIC_ASSERT(exchange == 0);
    return InterlockedCompareExchangePointerT(destination, reinterpret_cast<T*>(exchange), comparand);
}

template <typename T>
inline T* InterlockedCompareExchangePointerT(
    T* volatile * destination,
    T*            exchange,
    int           comparand) // When NULL is provided as argument.
{
    //STATIC_ASSERT(comparand == 0);
    return InterlockedCompareExchangePointerT(destination, exchange, reinterpret_cast<T*>(comparand));
}

#undef InterlockedExchangePointer
#define InterlockedExchangePointer InterlockedExchangePointerT
#undef InterlockedCompareExchangePointer
#define InterlockedCompareExchangePointer InterlockedCompareExchangePointerT

#include "volatile.h"

#endif // __cplusplus

#endif /* _PAL_INTERNAL_H_ */
