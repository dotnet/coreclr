// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
// Definition of the Unwind API functions.
// Taken from the ABI documentation.
//



#ifndef __PAL_UNWIND_H__
#define __PAL_UNWIND_H__

#if FEATURE_PAL_SXS

#ifdef __cplusplus
extern "C"
{
#endif // __cplusplus

#ifdef _ARM_
    //
    // Exception Handling ABI Level I: Base ABI
    //
    typedef enum
    {
        /* Operation completed successfully */
        _URC_OK                         = 0,
        _URC_FOREIGN_EXCEPTION_CAUGHT   = 1,
        _URC_HANDLER_FOUND              = 6,
        _URC_INSTALL_CONTEXT            = 7,
        _URC_CONTINUE_UNWIND            = 8,
        /* Unspecified failure of some kind */
        _URC_FAILURE                    = 9
    } _Unwind_Reason_Code;

    typedef enum
    {
        _US_VIRTUAL_UNWIND_FRAME   = 0,
        _US_UNWIND_FRAME_STARTING  = 1,
        _US_UNWIND_FRAME_RESUME    = 2
    } _Unwind_State;

    typedef struct _Unwind_Context _Unwind_Context;

    typedef unsigned int _Unwind_EHT_Header;

    struct _Unwind_Control_Block
    {
        char exception_class[8];

        /* Unwinder cache - Private fields for the unwinder's use */
        void (*exception_cleanup)(_Unwind_Reason_Code, _Unwind_Control_Block *);

        /* Propagation barrier cache (valid after phase 1): */
        struct
        {
            /* init reserved1 to 0, then don't touch */
            unsigned int reserved1;
            unsigned int reserved2;
            unsigned int reserved3;
            unsigned int reserved4;
            unsigned int reserved5;
        } unwinder_cache;

        /* Cleanup cache (preserved over cleanup): */
        struct
        {
            unsigned int sp;
            unsigned int bitpattern[5];
        } barrier_cache;

        /* PR cache (for pr's benefit): */
        struct
        {
            unsigned int bitpattern[4];
        } cleanup_cache;

        struct
        {
            /* function start address */
            unsigned int fnstart;
            /* pointer to EHT entry header word */
            _Unwind_EHT_Header *ehtp;
            /* additional data */
            unsigned int additional;
            unsigned int reserved1;
        } pr_cache;

        /* Force alignment of next item to 8-byte boundary */
        long long int :0;
    };

    typedef enum
    {
        /* integer register */
        _UVRSC_CORE  = 0,
        /* vfp */
        _UVRSC_VFP   = 1,
        /* Intel WMMX data register */
        _UVRSC_WMMXD = 3,
        /* Intel WMMX control register */
        _UVRSC_WMMXC = 4
    } _Unwind_VRS_RegClass;

    typedef enum
    {
        _UVRSD_UINT32 = 0,
        _UVRSD_VFPX   = 1,
        _UVRSD_UINT64 = 3,
        _UVRSD_FLOAT  = 4,
        _UVRSD_DOUBLE = 5
    } _Unwind_VRS_DataRepresentation;

    typedef enum
    {
        _UVRSR_OK              = 0,
        _UVRSR_NOT_IMPLEMENTED = 1,
        _UVRSR_FAILED          = 2
    } _Unwind_VRS_Result;

    _Unwind_VRS_Result _Unwind_VRS_Get(_Unwind_Context *context,
                                       _Unwind_VRS_RegClass regclass,
                                       unsigned int regno,
                                       _Unwind_VRS_DataRepresentation representation,
                                       void *valuep);

    _Unwind_VRS_Result _Unwind_VRS_Set(_Unwind_Context *context,
                                       _Unwind_VRS_RegClass regclass,
                                       unsigned int regno,
                                       _Unwind_VRS_DataRepresentation representation,
                                       void *valuep);

    _Unwind_VRS_Result _Unwind_VRS_Pop(_Unwind_Context *context,
                                       _Unwind_VRS_RegClass regclass,
                                       unsigned int discriminator,
                                       _Unwind_VRS_DataRepresentation representation);
    //
    // Exception Handling ABI Level II: C++ ABI
    //
    void *__cxa_begin_catch(_Unwind_Control_Block *ucbp);
    void  __cxa_end_catch(void);

    typedef enum
    {
        ctm_failed                      = 0,
        ctm_succeeded                   = 1,
        ctm_succeeded_with_ptr_to_base  = 2
    } __cxa_type_match_result;


    __cxa_type_match_result __cxa_type_match(_Unwind_Control_Block *ucbp,
                                             void *rttip, // const std::type_info *
                                             bool is_reference_type,
                                             void **matched_object);
#else // ARM
    //
    // Exception Handling ABI Level I: Base ABI
    //

    typedef enum
    {
        _URC_NO_REASON = 0,
        _URC_FOREIGN_EXCEPTION_CAUGHT = 1,
        _URC_FATAL_PHASE2_ERROR = 2,
        _URC_FATAL_PHASE1_ERROR = 3,
        _URC_NORMAL_STOP = 4,
        _URC_END_OF_STACK = 5,
        _URC_HANDLER_FOUND = 6,
        _URC_INSTALL_CONTEXT = 7,
        _URC_CONTINUE_UNWIND = 8,
    } _Unwind_Reason_Code;

    typedef enum
    {
        _UA_SEARCH_PHASE = 1,
        _UA_CLEANUP_PHASE = 2,
        _UA_HANDLER_FRAME = 4,
        _UA_FORCE_UNWIND = 8,
    } _Unwind_Action;
    #define _UA_PHASE_MASK (_UA_SEARCH_PHASE|_UA_CLEANUP_PHASE)

    struct _Unwind_Context;

    void *_Unwind_GetIP(struct _Unwind_Context *context);
    void _Unwind_SetIP(struct _Unwind_Context *context, void *new_value);
    void *_Unwind_GetCFA(struct _Unwind_Context *context);
    void *_Unwind_GetGR(struct _Unwind_Context *context, int index);
    void _Unwind_SetGR(struct _Unwind_Context *context, int index, void *new_value);

    struct _Unwind_Exception;

    typedef void (*_Unwind_Exception_Cleanup_Fn)(
        _Unwind_Reason_Code urc,
        struct _Unwind_Exception *exception_object);

    struct _Unwind_Exception
    {
        ULONG64 exception_class;
        _Unwind_Exception_Cleanup_Fn exception_cleanup;
        UINT_PTR private_1;
        UINT_PTR private_2;
    } __attribute__((aligned));

    void _Unwind_DeleteException(struct _Unwind_Exception *exception_object);

    typedef _Unwind_Reason_Code (*_Unwind_Trace_Fn)(struct _Unwind_Context *context, void *pvParam);
    _Unwind_Reason_Code _Unwind_Backtrace(_Unwind_Trace_Fn pfnTrace, void *pvParam);

    _Unwind_Reason_Code _Unwind_RaiseException(struct _Unwind_Exception *exception_object);
    __attribute__((noreturn)) void _Unwind_Resume(struct _Unwind_Exception *exception_object);

    //
    // Exception Handling ABI Level II: C++ ABI
    //

    void *__cxa_begin_catch(void *exceptionObject);
    void __cxa_end_catch();
#endif

#ifdef __cplusplus
};
#endif // __cplusplus

#endif // FEATURE_PAL_SXS

#endif // __PAL_UNWIND_H__
