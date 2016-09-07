#include <sys/ucontext.h>
#include <libunwind.h>

#include <cor.h>
#include <corhdr.h>
#include <corprof.h>

#include <pal.h>
#include <../pal/src/config.h>
#include <pal/context.h>

#undef unw_getcontext
EXTERN_C void unw_getcontext(unw_context_t*);

UINT_PTR GetCurrentIPFromHandler(
    COR_PRF_CODE_INFO codeInfo, const void *context) noexcept
{
    const native_context_t *ucontext =
        reinterpret_cast<const native_context_t*>(context);

    CONTEXT winContext;
    CONTEXTFromNativeContext(
        ucontext,
        &winContext,
        CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_FLOATING_POINT);
    UINT_PTR pc = CONTEXTGetPC(&winContext);

    // NOTE: we always use only first block of native code.
    UINT_PTR startAddr = codeInfo.startAddress;
    UINT_PTR endAddr   = codeInfo.startAddress + codeInfo.size;

    if (pc >= startAddr && pc < endAddr) {
        // NOTE: we have HIT!
        return pc;
    } else {
        unw_context_t context;
        unw_getcontext(&context);
        unw_cursor_t cursor;
        if (unw_init_local(&cursor, &context) != 0)
        {
            return 0;
        }

        DWORD32 sp = 0;
        DWORD32 prevSP = 0, prevPC = 0;
        unw_word_t val;
        for (int i = 0; i < 100; i++) {
            if (unw_get_reg(&cursor, 13, &val) != 0)
            {
                return 0;
            }
            sp = val;

            if (unw_get_reg(&cursor, 14, &val) == 0)
            {
                pc = val;
                if (pc >= startAddr && pc < endAddr) {
                    // NOTE: we have HIT!
                    return pc;
                }
                if (prevSP == sp && prevPC == pc)
                {
                    break; // NOTE: loop!!!
                }
                prevSP = sp;
                prevPC = pc;
            }
            if (unw_step(&cursor) <= 0)
            {
                break;
            }
        }
        if (unw_get_reg(&cursor, 11, &val) != 0)
        {
            return 0;
        }

        DWORD32 fp = val;
        if (fp < reinterpret_cast<DWORD32>(&cursor)) // Sanity check.
        {
            return 0;
        }
        while (fp > sp && fp < sp + 0x40000)
        {
             pc = reinterpret_cast<DWORD32*>(fp)[1];
             if (pc >= startAddr && pc < endAddr)
             {
                 // NOTE: we have HIT!
                 return pc;
             }
             fp = reinterpret_cast<DWORD32*>(fp)[0];
        }

        return 0;
    }
}
