// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                             emitX86.cpp                                   XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#if defined(_TARGET_XARCH_)

/*****************************************************************************/
/*****************************************************************************/

#include "instr.h"
#include "emit.h"
#include "codegen.h"

bool IsSSE2Instruction(instruction ins)
{
    return (ins >= INS_FIRST_SSE2_INSTRUCTION) && (ins <= INS_LAST_SSE2_INSTRUCTION);
}

bool IsSSE4Instruction(instruction ins)
{
    return (ins >= INS_FIRST_SSE4_INSTRUCTION) && (ins <= INS_LAST_SSE4_INSTRUCTION);
}

bool IsSSEOrAVXInstruction(instruction ins)
{
    return (ins >= INS_FIRST_SSE2_INSTRUCTION) && (ins <= INS_LAST_AVX_INSTRUCTION);
}

bool IsAVXOnlyInstruction(instruction ins)
{
    return (ins >= INS_FIRST_AVX_INSTRUCTION) && (ins <= INS_LAST_AVX_INSTRUCTION);
}

bool emitter::IsAVXInstruction(instruction ins)
{
    return UseVEXEncoding() && IsSSEOrAVXInstruction(ins);
}

// Returns true if the AVX instruction is a binary operator that requires 3 operands.
// When we emit an instruction with only two operands, we will duplicate the destination
// as a source.
// TODO-XArch-Cleanup: This is a temporary solution for now. Eventually this needs to
// be formalized by adding an additional field to instruction table to
// to indicate whether a 3-operand instruction.
bool emitter::IsDstDstSrcAVXInstruction(instruction ins)
{
    switch (ins)
    {
        case INS_addpd:
        case INS_addps:
        case INS_addsd:
        case INS_addss:
        case INS_addsubpd:
        case INS_addsubps:
        case INS_andnpd:
        case INS_andnps:
        case INS_andpd:
        case INS_andps:
        case INS_blendpd:
        case INS_blendps:
        case INS_cmppd:
        case INS_cmpps:
        case INS_cmpsd:
        case INS_cmpss:
        case INS_cvtsi2sd:
        case INS_cvtsi2ss:
        case INS_cvtsd2ss:
        case INS_cvtss2sd:
        case INS_divpd:
        case INS_divps:
        case INS_divsd:
        case INS_divss:
        case INS_dppd:
        case INS_dpps:
        case INS_haddpd:
        case INS_haddps:
        case INS_hsubpd:
        case INS_hsubps:
        case INS_insertps:
        case INS_maxpd:
        case INS_maxps:
        case INS_maxsd:
        case INS_maxss:
        case INS_minpd:
        case INS_minps:
        case INS_minsd:
        case INS_minss:
        case INS_movhlps:
        case INS_movlhps:
        case INS_mpsadbw:
        case INS_mulpd:
        case INS_mulps:
        case INS_mulsd:
        case INS_mulss:
        case INS_orpd:
        case INS_orps:
        case INS_packssdw:
        case INS_packsswb:
        case INS_packusdw:
        case INS_packuswb:
        case INS_paddb:
        case INS_paddd:
        case INS_paddq:
        case INS_paddsb:
        case INS_paddsw:
        case INS_paddusb:
        case INS_paddusw:
        case INS_paddw:
        case INS_palignr:
        case INS_pand:
        case INS_pandn:
        case INS_pavgb:
        case INS_pavgw:
        case INS_pblendw:
        case INS_pcmpeqb:
        case INS_pcmpeqd:
        case INS_pcmpeqq:
        case INS_pcmpeqw:
        case INS_pcmpgtb:
        case INS_pcmpgtd:
        case INS_pcmpgtq:
        case INS_pcmpgtw:
        case INS_phaddd:
        case INS_phaddsw:
        case INS_phaddw:
        case INS_phsubd:
        case INS_phsubsw:
        case INS_phsubw:
        case INS_pinsrb:
        case INS_pinsrw:
        case INS_pinsrd:
        case INS_pinsrq:
        case INS_pmaddubsw:
        case INS_pmaddwd:
        case INS_pmaxsb:
        case INS_pmaxsd:
        case INS_pmaxsw:
        case INS_pmaxub:
        case INS_pmaxud:
        case INS_pmaxuw:
        case INS_pminsb:
        case INS_pminsd:
        case INS_pminsw:
        case INS_pminub:
        case INS_pminud:
        case INS_pminuw:
        case INS_pmuldq:
        case INS_pmulhrsw:
        case INS_pmulhuw:
        case INS_pmulhw:
        case INS_pmulld:
        case INS_pmullw:
        case INS_pmuludq:
        case INS_por:
        case INS_psadbw:
        case INS_pshufb:
        case INS_psignb:
        case INS_psignd:
        case INS_psignw:
        case INS_psubb:
        case INS_psubd:
        case INS_psubq:
        case INS_psubsb:
        case INS_psubsw:
        case INS_psubusb:
        case INS_psubusw:
        case INS_psubw:
        case INS_pslld:
        case INS_psllq:
        case INS_psllw:
        case INS_psrld:
        case INS_psrlq:
        case INS_psrlw:
        case INS_psrad:
        case INS_psraw:
        case INS_punpckhbw:
        case INS_punpckhdq:
        case INS_punpckhqdq:
        case INS_punpckhwd:
        case INS_punpcklbw:
        case INS_punpckldq:
        case INS_punpcklqdq:
        case INS_punpcklwd:
        case INS_pxor:
        case INS_shufpd:
        case INS_shufps:
        case INS_subpd:
        case INS_subps:
        case INS_subsd:
        case INS_subss:
        case INS_unpckhps:
        case INS_unpcklps:
        case INS_unpckhpd:
        case INS_unpcklpd:
        case INS_vinsertf128:
        case INS_vinserti128:
        case INS_vmaskmovps:
        case INS_vmaskmovpd:
        case INS_vperm2i128:
        case INS_vperm2f128:
        case INS_vpermilpsvar:
        case INS_vpermilpdvar:
        case INS_vpsrlvd:
        case INS_vpsrlvq:
        case INS_vpsravd:
        case INS_vpsllvd:
        case INS_vpsllvq:
        case INS_xorpd:
        case INS_xorps:
            return IsAVXInstruction(ins);
        default:
            return false;
    }
}

// Returns true if the AVX instruction requires 3 operands that duplicate the source
// register in the vvvv field.
// TODO-XArch-Cleanup: This is a temporary solution for now. Eventually this needs to
// be formalized by adding an additional field to instruction table to
// to indicate whether a 3-operand instruction.
bool emitter::IsDstSrcSrcAVXInstruction(instruction ins)
{
    switch (ins)
    {
        case INS_movhpd:
        case INS_movhps:
        case INS_movlpd:
        case INS_movlps:
        case INS_movsdsse2:
        case INS_movss:
        case INS_rcpss:
        case INS_roundsd:
        case INS_roundss:
        case INS_rsqrtss:
        case INS_sqrtsd:
        case INS_sqrtss:
            return IsAVXInstruction(ins);
        default:
            return false;
    }
}

// -------------------------------------------------------------------
// Is4ByteSSE4Instruction: Returns true if the SSE4 instruction
// is a 4-byte opcode.
//
// Arguments:
//    ins  -  instruction
//
// Note that this should be true for any of the instructions in instrsXArch.h
// that use the SSE38 or SSE3A macro.
bool emitter::Is4ByteSSE4Instruction(instruction ins)
{
    return UseSSE4() && IsSSE4Instruction(ins) && EncodedBySSE38orSSE3A(ins);
}

// ------------------------------------------------------------------------------
// Is4ByteSSE4OrAVXInstruction: Returns true if the SSE4 or AVX instruction is a 4-byte opcode.
//
// Arguments:
//    ins  -  instructions
//
// Note that this should be true for any of the instructions in instrsXArch.h
// that use the SSE38 or SSE3A macro.
bool emitter::Is4ByteSSE4OrAVXInstruction(instruction ins)
{
    return ((UseVEXEncoding() && (IsSSE4Instruction(ins) || IsAVXOnlyInstruction(ins))) ||
            (UseSSE4() && IsSSE4Instruction(ins))) &&
           EncodedBySSE38orSSE3A(ins);
}

// Returns true if this instruction requires a VEX prefix
// All AVX instructions require a VEX prefix
bool emitter::TakesVexPrefix(instruction ins)
{
    // special case vzeroupper as it requires 2-byte VEX prefix
    // special case the fencing, movnti and the prefetch instructions as they never take a VEX prefix
    switch (ins)
    {
        case INS_lfence:
        case INS_mfence:
        case INS_movnti:
        case INS_prefetchnta:
        case INS_prefetcht0:
        case INS_prefetcht1:
        case INS_prefetcht2:
        case INS_sfence:
        case INS_vzeroupper:
            return false;
        default:
            break;
    }

    return IsAVXInstruction(ins);
}

// Add base VEX prefix without setting W, R, X, or B bits
// L bit will be set based on emitter attr.
//
// 3-byte VEX prefix = C4 <R,X,B,m-mmmm> <W,vvvv,L,pp>
//  - R, X, B, W - bits to express corresponding REX prefixes
//  - m-mmmmm (5-bit)
//    0-00001 - implied leading 0F opcode byte
//    0-00010 - implied leading 0F 38 opcode bytes
//    0-00011 - implied leading 0F 3A opcode bytes
//    Rest    - reserved for future use and usage of them will uresult in Undefined instruction exception
//
// - vvvv (4-bits) - register specifier in 1's complement form; must be 1111 if unused
// - L - scalar or AVX-128 bit operations (L=0),  256-bit operations (L=1)
// - pp (2-bits) - opcode extension providing equivalent functionality of a SIMD size prefix
//                 these prefixes are treated mandatory when used with escape opcode 0Fh for
//                 some SIMD instructions
//   00  - None   (0F    - packed float)
//   01  - 66     (66 0F - packed double)
//   10  - F3     (F3 0F - scalar float
//   11  - F2     (F2 0F - scalar double)
//
// TODO-AMD64-CQ: for simplicity of implementation this routine always adds 3-byte VEX
// prefix. Based on 'attr' param we could add 2-byte VEX prefix in case of scalar
// and AVX-128 bit operations.
#define DEFAULT_3BYTE_VEX_PREFIX 0xC4E07800000000ULL
#define DEFAULT_3BYTE_VEX_PREFIX_MASK 0xFFFFFF00000000ULL
#define LBIT_IN_3BYTE_VEX_PREFIX 0x00000400000000ULL
emitter::code_t emitter::AddVexPrefix(instruction ins, code_t code, emitAttr attr)
{
    // Only AVX instructions require VEX prefix
    assert(IsAVXInstruction(ins));

    // Shouldn't have already added Vex prefix
    assert(!hasVexPrefix(code));

    // Set L bit to 1 in case of instructions that operate on 256-bits.
    assert((code & DEFAULT_3BYTE_VEX_PREFIX_MASK) == 0);
    code |= DEFAULT_3BYTE_VEX_PREFIX;
    if (attr == EA_32BYTE)
    {
        code |= LBIT_IN_3BYTE_VEX_PREFIX;
    }

    return code;
}

// Returns true if this instruction, for the given EA_SIZE(attr), will require a REX.W prefix
bool TakesRexWPrefix(instruction ins, emitAttr attr)
{
    // Because the current implementation of AVX does not have a way to distinguish between the register
    // size specification (128 vs. 256 bits) and the operand size specification (32 vs. 64 bits), where both are
    // required, the instruction must be created with the register size attribute (EA_16BYTE or EA_32BYTE),
    // and here we must special case these by the opcode.
    switch (ins)
    {
        case INS_vpermq:
        case INS_vpsrlvq:
        case INS_vpsllvq:
        case INS_pinsrq:
        case INS_pextrq:
            return true;
        default:
            break;
    }

#ifdef _TARGET_AMD64_
    // movsx should always sign extend out to 8 bytes just because we don't track
    // whether the dest should be 4 bytes or 8 bytes (attr indicates the size
    // of the source, not the dest).
    // A 4-byte movzx is equivalent to an 8 byte movzx, so it is not special
    // cased here.
    //
    // Rex_jmp = jmp with rex prefix always requires rex.w prefix.
    if (ins == INS_movsx || ins == INS_rex_jmp)
    {
        return true;
    }

    if (EA_SIZE(attr) != EA_8BYTE)
    {
        return false;
    }

    if (IsSSEOrAVXInstruction(ins))
    {
        switch (ins)
        {
            case INS_cvttsd2si:
            case INS_cvttss2si:
            case INS_cvtsd2si:
            case INS_cvtss2si:
            case INS_cvtsi2sd:
            case INS_cvtsi2ss:
            case INS_mov_xmm2i:
            case INS_mov_i2xmm:
            case INS_movnti:
                return true;
            default:
                return false;
        }
    }

    // TODO-XArch-Cleanup: Better way to not emit REX.W when we don't need it, than just testing all these
    // opcodes...
    // These are all the instructions that default to 8-byte operand without the REX.W bit
    // With 1 special case: movzx because the 4 byte version still zeros-out the hi 4 bytes
    // so we never need it
    if ((ins != INS_push) && (ins != INS_pop) && (ins != INS_movq) && (ins != INS_movzx) && (ins != INS_push_hide) &&
        (ins != INS_pop_hide) && (ins != INS_ret) && (ins != INS_call) && !((ins >= INS_i_jmp) && (ins <= INS_l_jg)))
    {
        return true;
    }
    else
    {
        return false;
    }
#else  //!_TARGET_AMD64 = _TARGET_X86_
    return false;
#endif //!_TARGET_AMD64_
}

// Returns true if using this register will require a REX.* prefix.
// Since XMM registers overlap with YMM registers, this routine
// can also be used to know whether a YMM register if the
// instruction in question is AVX.
bool IsExtendedReg(regNumber reg)
{
#ifdef _TARGET_AMD64_
    return ((reg >= REG_R8) && (reg <= REG_R15)) || ((reg >= REG_XMM8) && (reg <= REG_XMM15));
#else
    // X86 JIT operates in 32-bit mode and hence extended reg are not available.
    return false;
#endif
}

// Returns true if using this register, for the given EA_SIZE(attr), will require a REX.* prefix
bool IsExtendedReg(regNumber reg, emitAttr attr)
{
#ifdef _TARGET_AMD64_
    // Not a register, so doesn't need a prefix
    if (reg > REG_XMM15)
    {
        return false;
    }

    // Opcode field only has 3 bits for the register, these high registers
    // need a 4th bit, that comes from the REX prefix (eiter REX.X, REX.R, or REX.B)
    if (IsExtendedReg(reg))
    {
        return true;
    }

    if (EA_SIZE(attr) != EA_1BYTE)
    {
        return false;
    }

    // There are 12 one byte registers addressible 'below' r8b:
    //     al, cl, dl, bl, ah, ch, dh, bh, spl, bpl, sil, dil.
    // The first 4 are always addressible, the last 8 are divided into 2 sets:
    //     ah,  ch,  dh,  bh
    //          -- or --
    //     spl, bpl, sil, dil
    // Both sets are encoded exactly the same, the difference is the presence
    // of a REX prefix, even a REX prefix with no other bits set (0x40).
    // So in order to get to the second set we need a REX prefix (but no bits).
    //
    // TODO-AMD64-CQ: if we ever want to start using the first set, we'll need a different way of
    // encoding/tracking/encoding registers.
    return (reg >= REG_RSP);
#else
    // X86 JIT operates in 32-bit mode and hence extended reg are not available.
    return false;
#endif
}

// Since XMM registers overlap with YMM registers, this routine
// can also used to know whether a YMM register in case of AVX instructions.
bool IsXMMReg(regNumber reg)
{
#ifdef _TARGET_AMD64_
    return (reg >= REG_XMM0) && (reg <= REG_XMM15);
#else  // !_TARGET_AMD64_
    return (reg >= REG_XMM0) && (reg <= REG_XMM7);
#endif // !_TARGET_AMD64_
}

// Returns bits to be encoded in instruction for the given register.
unsigned RegEncoding(regNumber reg)
{
    static_assert((REG_XMM0 & 0x7) == 0, "bad XMMBASE");
    return (unsigned)(reg & 0x7);
}

// Utility routines that abstract the logic of adding REX.W, REX.R, REX.X, REX.B and REX prefixes
// SSE2: separate 1-byte prefix gets added before opcode.
// AVX:  specific bits within VEX prefix need to be set in bit-inverted form.
emitter::code_t emitter::AddRexWPrefix(instruction ins, code_t code)
{
    if (UseVEXEncoding() && IsAVXInstruction(ins))
    {
        // W-bit is available only in 3-byte VEX prefix that starts with byte C4.
        if (TakesVexPrefix(ins))
        {
            assert(hasVexPrefix(code));

            // W-bit is the only bit that is added in non bit-inverted form.
            return emitter::code_t(code | 0x00008000000000ULL);
        }
    }
#ifdef _TARGET_AMD64_
    return emitter::code_t(code | 0x4800000000ULL);
#else
    assert(!"UNREACHED");
    return code;
#endif
}

#ifdef _TARGET_AMD64_

emitter::code_t emitter::AddRexRPrefix(instruction ins, code_t code)
{
    if (UseVEXEncoding() && IsAVXInstruction(ins))
    {
        // Right now support 3-byte VEX prefix
        if (TakesVexPrefix(ins))
        {
            assert(hasVexPrefix(code));

            // R-bit is added in bit-inverted form.
            return code & 0xFF7FFFFFFFFFFFULL;
        }
    }

    return code | 0x4400000000ULL;
}

emitter::code_t emitter::AddRexXPrefix(instruction ins, code_t code)
{
    if (UseVEXEncoding() && IsAVXInstruction(ins))
    {
        // Right now support 3-byte VEX prefix
        if (TakesVexPrefix(ins))
        {
            assert(hasVexPrefix(code));

            // X-bit is added in bit-inverted form.
            return code & 0xFFBFFFFFFFFFFFULL;
        }
    }

    return code | 0x4200000000ULL;
}

emitter::code_t emitter::AddRexBPrefix(instruction ins, code_t code)
{
    if (UseVEXEncoding() && IsAVXInstruction(ins))
    {
        // Right now support 3-byte VEX prefix
        if (TakesVexPrefix(ins))
        {
            assert(hasVexPrefix(code));

            // B-bit is added in bit-inverted form.
            return code & 0xFFDFFFFFFFFFFFULL;
        }
    }

    return code | 0x4100000000ULL;
}

// Adds REX prefix (0x40) without W, R, X or B bits set
emitter::code_t emitter::AddRexPrefix(instruction ins, code_t code)
{
    assert(!UseVEXEncoding() || !IsAVXInstruction(ins));
    return code | 0x4000000000ULL;
}

#endif //_TARGET_AMD64_

bool isPrefix(BYTE b)
{
    assert(b != 0);    // Caller should check this
    assert(b != 0x67); // We don't use the address size prefix
    assert(b != 0x65); // The GS segment override prefix is emitted separately
    assert(b != 0x64); // The FS segment override prefix is emitted separately
    assert(b != 0xF0); // The lock prefix is emitted separately
    assert(b != 0x2E); // We don't use the CS segment override prefix
    assert(b != 0x3E); // Or the DS segment override prefix
    assert(b != 0x26); // Or the ES segment override prefix
    assert(b != 0x36); // Or the SS segment override prefix

    // That just leaves the size prefixes used in SSE opcodes:
    //      Scalar Double  Scalar Single  Packed Double
    return ((b == 0xF2) || (b == 0xF3) || (b == 0x66));
}

// Outputs VEX prefix (in case of AVX instructions) and REX.R/X/W/B otherwise.
unsigned emitter::emitOutputRexOrVexPrefixIfNeeded(instruction ins, BYTE* dst, code_t& code)
{
    if (hasVexPrefix(code))
    {
        // Only AVX instructions should have a VEX prefix
        assert(UseVEXEncoding() && IsAVXInstruction(ins));
        code_t vexPrefix = (code >> 32) & 0x00FFFFFF;
        code &= 0x00000000FFFFFFFFLL;

        WORD leadingBytes = 0;
        BYTE check        = (code >> 24) & 0xFF;
        if (check != 0)
        {
            // 3-byte opcode: with the bytes ordered as 0x2211RM33 or
            // 4-byte opcode: with the bytes ordered as 0x22114433
            // check for a prefix in the 11 position
            BYTE sizePrefix = (code >> 16) & 0xFF;
            if (sizePrefix != 0 && isPrefix(sizePrefix))
            {
                // 'pp' bits in byte2 of VEX prefix allows us to encode SIMD size prefixes as two bits
                //
                //   00  - None   (0F    - packed float)
                //   01  - 66     (66 0F - packed double)
                //   10  - F3     (F3 0F - scalar float
                //   11  - F2     (F2 0F - scalar double)
                switch (sizePrefix)
                {
                    case 0x66:
                        vexPrefix |= 0x01;
                        break;
                    case 0xF3:
                        vexPrefix |= 0x02;
                        break;
                    case 0xF2:
                        vexPrefix |= 0x03;
                        break;
                    default:
                        assert(!"unrecognized SIMD size prefix");
                        unreached();
                }

                // Now the byte in the 22 position must be an escape byte 0F
                leadingBytes = check;
                assert(leadingBytes == 0x0F);

                // Get rid of both sizePrefix and escape byte
                code &= 0x0000FFFFLL;

                // Check the byte in the 33 position to see if it is 3A or 38.
                // In such a case escape bytes must be 0x0F3A or 0x0F38
                check = code & 0xFF;
                if (check == 0x3A || check == 0x38)
                {
                    leadingBytes = (leadingBytes << 8) | check;
                    code &= 0x0000FF00LL;
                }
            }
        }
        else
        {
            // 2-byte opcode with the bytes ordered as 0x0011RM22
            // the byte in position 11 must be an escape byte.
            leadingBytes = (code >> 16) & 0xFF;
            assert(leadingBytes == 0x0F || leadingBytes == 0x00);
            code &= 0xFFFF;
        }

        // If there is an escape byte it must be 0x0F or 0x0F3A or 0x0F38
        // m-mmmmm bits in byte 1 of VEX prefix allows us to encode these
        // implied leading bytes
        switch (leadingBytes)
        {
            case 0x00:
                // there is no leading byte
                break;
            case 0x0F:
                vexPrefix |= 0x0100;
                break;
            case 0x0F38:
                vexPrefix |= 0x0200;
                break;
            case 0x0F3A:
                vexPrefix |= 0x0300;
                break;
            default:
                assert(!"encountered unknown leading bytes");
                unreached();
        }

        // At this point
        //     VEX.2211RM33 got transformed as VEX.0000RM33
        //     VEX.0011RM22 got transformed as VEX.0000RM22
        //
        // Now output VEX prefix leaving the 4-byte opcode
        emitOutputByte(dst, ((vexPrefix >> 16) & 0xFF));
        emitOutputByte(dst + 1, ((vexPrefix >> 8) & 0xFF));
        emitOutputByte(dst + 2, vexPrefix & 0xFF);
        return 3;
    }

#ifdef _TARGET_AMD64_
    if (code > 0x00FFFFFFFFLL)
    {
        BYTE prefix = (code >> 32) & 0xFF;
        noway_assert(prefix >= 0x40 && prefix <= 0x4F);
        code &= 0x00000000FFFFFFFFLL;

        // TODO-AMD64-Cleanup: when we remove the prefixes (just the SSE opcodes right now)
        // we can remove this code as well

        // The REX prefix is required to come after all other prefixes.
        // Some of our 'opcodes' actually include some prefixes, if that
        // is the case, shift them over and place the REX prefix after
        // the other prefixes, and emit any prefix that got moved out.
        BYTE check = (code >> 24) & 0xFF;
        if (check == 0)
        {
            // 3-byte opcode: with the bytes ordered as 0x00113322
            // check for a prefix in the 11 position
            check = (code >> 16) & 0xFF;
            if (check != 0 && isPrefix(check))
            {
                // Swap the rex prefix and whatever this prefix is
                code = (((DWORD)prefix << 16) | (code & 0x0000FFFFLL));
                // and then emit the other prefix
                return emitOutputByte(dst, check);
            }
        }
        else
        {
            // 4-byte opcode with the bytes ordered as 0x22114433
            // first check for a prefix in the 11 position
            BYTE check2 = (code >> 16) & 0xFF;
            if (isPrefix(check2))
            {
                assert(!isPrefix(check)); // We currently don't use this, so it is untested
                if (isPrefix(check))
                {
                    // 3 prefixes were rex = rr, check = c1, check2 = c2 encoded as 0xrrc1c2XXXX
                    // Change to c2rrc1XXXX, and emit check2 now
                    code = (((code_t)prefix << 24) | ((code_t)check << 16) | (code & 0x0000FFFFLL));
                }
                else
                {
                    // 2 prefixes were rex = rr, check2 = c2 encoded as 0xrrXXc2XXXX, (check is part of the opcode)
                    // Change to c2XXrrXXXX, and emit check2 now
                    code = (((code_t)check << 24) | ((code_t)prefix << 16) | (code & 0x0000FFFFLL));
                }
                return emitOutputByte(dst, check2);
            }
        }

        return emitOutputByte(dst, prefix);
    }
#endif // _TARGET_AMD64_

    return 0;
}

#ifdef _TARGET_AMD64_
/*****************************************************************************
 * Is the last instruction emitted a call instruction?
 */
bool emitter::emitIsLastInsCall()
{
    if ((emitLastIns != nullptr) && (emitLastIns->idIns() == INS_call))
    {
        return true;
    }

    return false;
}

/*****************************************************************************
 * We're about to create an epilog. If the last instruction we output was a 'call',
 * then we need to insert a NOP, to allow for proper exception-handling behavior.
 */
void emitter::emitOutputPreEpilogNOP()
{
    if (emitIsLastInsCall())
    {
        emitIns(INS_nop);
    }
}

#endif //_TARGET_AMD64_

// Size of rex prefix in bytes
unsigned emitter::emitGetRexPrefixSize(instruction ins)
{
    // In case of AVX instructions, REX prefixes are part of VEX prefix.
    // And hence requires no additional byte to encode REX prefixes.
    if (IsAVXInstruction(ins))
    {
        return 0;
    }

    // If not AVX, then we would need 1-byte to encode REX prefix.
    return 1;
}

// Size of vex prefix in bytes
unsigned emitter::emitGetVexPrefixSize(instruction ins, emitAttr attr)
{
    // TODO-XArch-CQ: right now we default to 3-byte VEX prefix. There is a
    // scope for size win by using 2-byte vex prefix for some of the
    // scalar, avx-128 and most common avx-256 instructions.
    if (IsAVXInstruction(ins))
    {
        return 3;
    }

    // If not AVX, then we don't need to encode vex prefix.
    return 0;
}

// VEX prefix encodes some bytes of the opcode and as a result, overall size of the instruction reduces.
// Therefore, to estimate the size adding VEX prefix size and size of instruction opcode bytes will always overstimate.
// Instead this routine will adjust the size of VEX prefix based on the number of bytes of opcode it encodes so that
// instruction size estimate will be accurate.
// Basically this function will decrease the vexPrefixSize,
// so that opcodeSize + vexPrefixAdjustedSize will be the right size.
// rightOpcodeSize + vexPrefixSize
//=(opcodeSize - ExtrabytesSize) + vexPrefixSize
//=opcodeSize + (vexPrefixSize - ExtrabytesSize)
//=opcodeSize + vexPrefixAdjustedSize
unsigned emitter::emitGetVexPrefixAdjustedSize(instruction ins, emitAttr attr, code_t code)
{
    if (IsAVXInstruction(ins))
    {
        unsigned vexPrefixAdjustedSize = emitGetVexPrefixSize(ins, attr);
        // Currently vex prefix size is hard coded as 3 bytes,
        // In future we should support 2 bytes vex prefix.
        assert(vexPrefixAdjustedSize == 3);

        // In this case, opcode will contains escape prefix at least one byte,
        // vexPrefixAdjustedSize should be minus one.
        vexPrefixAdjustedSize -= 1;

        // Get the fourth byte in Opcode.
        // If this byte is non-zero, then we should check whether the opcode contains SIMD prefix or not.
        BYTE check = (code >> 24) & 0xFF;
        if (check != 0)
        {
            // 3-byte opcode: with the bytes ordered as 0x2211RM33 or
            // 4-byte opcode: with the bytes ordered as 0x22114433
            // Simd prefix is at the first byte.
            BYTE sizePrefix = (code >> 16) & 0xFF;
            if (sizePrefix != 0 && isPrefix(sizePrefix))
            {
                vexPrefixAdjustedSize -= 1;
            }

            // If the opcode size is 4 bytes, then the second escape prefix is at fourth byte in opcode.
            // But in this case the opcode has not counted R\M part.
            // opcodeSize + VexPrefixAdjustedSize - ExtraEscapePrefixSize + ModR\MSize
            //=opcodeSize + VexPrefixAdjustedSize -1 + 1
            //=opcodeSize + VexPrefixAdjustedSize
            // So although we may have second byte escape prefix, we won't decrease vexPrefixAjustedSize.
        }

        return vexPrefixAdjustedSize;
    }
    return 0;
}

// Get size of rex or vex prefix emitted in code
unsigned emitter::emitGetPrefixSize(code_t code)
{
    if (hasVexPrefix(code))
    {
        return 3;
    }

    if (hasRexPrefix(code))
    {
        return 1;
    }

    return 0;
}

#ifdef _TARGET_X86_
/*****************************************************************************
 *
 *  Record a non-empty stack
 */

void emitter::emitMarkStackLvl(unsigned stackLevel)
{
    assert(int(stackLevel) >= 0);
    assert(emitCurStackLvl == 0);
    assert(emitCurIG->igStkLvl == 0);
    assert(emitCurIGfreeNext == emitCurIGfreeBase);

    assert(stackLevel && stackLevel % sizeof(int) == 0);

    emitCurStackLvl = emitCurIG->igStkLvl = stackLevel;

    if (emitMaxStackDepth < emitCurStackLvl)
    {
        JITDUMP("Upping emitMaxStackDepth from %d to %d\n", emitMaxStackDepth, emitCurStackLvl);
        emitMaxStackDepth = emitCurStackLvl;
    }
}
#endif

/*****************************************************************************
 *
 *  Get hold of the address mode displacement value for an indirect call.
 */

inline ssize_t emitter::emitGetInsCIdisp(instrDesc* id)
{
    if (id->idIsLargeCall())
    {
        return ((instrDescCGCA*)id)->idcDisp;
    }
    else
    {
        assert(!id->idIsLargeDsp());
        assert(!id->idIsLargeCns());

        return id->idAddr()->iiaAddrMode.amDisp;
    }
}

/** ***************************************************************************
 *
 *  The following table is used by the instIsFP()/instUse/DefFlags() helpers.
 */

#define INST_DEF_FL 0x20 // does the instruction set flags?
#define INST_USE_FL 0x40 // does the instruction use flags?

// clang-format off
const BYTE          CodeGenInterface::instInfo[] =
{
    #define INST0(id, nm, fp, um, rf, wf, mr                 ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #define INST1(id, nm, fp, um, rf, wf, mr                 ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #define INST2(id, nm, fp, um, rf, wf, mr, mi             ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm         ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4     ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr ) (INST_USE_FL*rf|INST_DEF_FL*wf|INST_FP*fp),
    #include "instrs.h"
    #undef  INST0
    #undef  INST1
    #undef  INST2
    #undef  INST3
    #undef  INST4
    #undef  INST5
};
// clang-format on

/*****************************************************************************
 *
 *  Initialize the table used by emitInsModeFormat().
 */

// clang-format off
const BYTE          emitter::emitInsModeFmtTab[] =
{
    #define INST0(id, nm, fp, um, rf, wf, mr                ) um,
    #define INST1(id, nm, fp, um, rf, wf, mr                ) um,
    #define INST2(id, nm, fp, um, rf, wf, mr, mi            ) um,
    #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        ) um,
    #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) um,
    #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) um,
    #include "instrs.h"
    #undef  INST0
    #undef  INST1
    #undef  INST2
    #undef  INST3
    #undef  INST4
    #undef  INST5
};
// clang-format on

#ifdef DEBUG
unsigned const emitter::emitInsModeFmtCnt = _countof(emitInsModeFmtTab);
#endif

/*****************************************************************************
 *
 *  Combine the given base format with the update mode of the instuction.
 */

inline emitter::insFormat emitter::emitInsModeFormat(instruction ins, insFormat base)
{
    assert(IF_RRD + IUM_RD == IF_RRD);
    assert(IF_RRD + IUM_WR == IF_RWR);
    assert(IF_RRD + IUM_RW == IF_RRW);

    return (insFormat)(base + emitInsUpdateMode(ins));
}

// This is a helper we need due to Vs Whidbey #254016 in order to distinguish
// if we can not possibly be updating an integer register. This is not the best
// solution, but the other ones (see bug) are going to be much more complicated.
bool emitter::emitInsCanOnlyWriteSSE2OrAVXReg(instrDesc* id)
{
    instruction ins = id->idIns();

    // The following SSE2 instructions write to a general purpose integer register.
    if (!IsSSEOrAVXInstruction(ins) || ins == INS_mov_xmm2i || ins == INS_cvttsd2si || ins == INS_cvttss2si ||
        ins == INS_cvtsd2si || ins == INS_cvtss2si || ins == INS_pmovmskb || ins == INS_pextrw || ins == INS_pextrb ||
        ins == INS_pextrd || ins == INS_pextrq || ins == INS_extractps)
    {
        return false;
    }

    return true;
}

/*****************************************************************************
 *
 *  Returns the base encoding of the given CPU instruction.
 */

inline size_t insCode(instruction ins)
{
    // clang-format off
    const static
    size_t          insCodes[] =
    {
        #define INST0(id, nm, fp, um, rf, wf, mr                ) mr,
        #define INST1(id, nm, fp, um, rf, wf, mr                ) mr,
        #define INST2(id, nm, fp, um, rf, wf, mr, mi            ) mr,
        #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        ) mr,
        #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) mr,
        #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) mr,
        #include "instrs.h"
        #undef  INST0
        #undef  INST1
        #undef  INST2
        #undef  INST3
        #undef  INST4
        #undef  INST5
    };
    // clang-format on

    assert((unsigned)ins < _countof(insCodes));
    assert((insCodes[ins] != BAD_CODE));

    return insCodes[ins];
}

/*****************************************************************************
 *
 *  Returns the "AL/AX/EAX, imm" accumulator encoding of the given instruction.
 */

inline size_t insCodeACC(instruction ins)
{
    // clang-format off
    const static
    size_t          insCodesACC[] =
    {
        #define INST0(id, nm, fp, um, rf, wf, mr                )
        #define INST1(id, nm, fp, um, rf, wf, mr                )
        #define INST2(id, nm, fp, um, rf, wf, mr, mi            )
        #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        )
        #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) a4,
        #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) a4,
        #include "instrs.h"
        #undef  INST0
        #undef  INST1
        #undef  INST2
        #undef  INST3
        #undef  INST4
        #undef  INST5
    };
    // clang-format on

    assert((unsigned)ins < _countof(insCodesACC));
    assert((insCodesACC[ins] != BAD_CODE));

    return insCodesACC[ins];
}

/*****************************************************************************
 *
 *  Returns the "register" encoding of the given CPU instruction.
 */

inline size_t insCodeRR(instruction ins)
{
    // clang-format off
    const static
    size_t          insCodesRR[] =
    {
        #define INST0(id, nm, fp, um, rf, wf, mr                )
        #define INST1(id, nm, fp, um, rf, wf, mr                )
        #define INST2(id, nm, fp, um, rf, wf, mr, mi            )
        #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        )
        #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    )
        #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) rr,
        #include "instrs.h"
        #undef  INST0
        #undef  INST1
        #undef  INST2
        #undef  INST3
        #undef  INST4
        #undef  INST5
    };
    // clang-format on

    assert((unsigned)ins < _countof(insCodesRR));
    assert((insCodesRR[ins] != BAD_CODE));

    return insCodesRR[ins];
}

// clang-format off
const static
size_t          insCodesRM[] =
{
    #define INST0(id, nm, fp, um, rf, wf, mr                )
    #define INST1(id, nm, fp, um, rf, wf, mr                )
    #define INST2(id, nm, fp, um, rf, wf, mr, mi            )
    #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        ) rm,
    #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) rm,
    #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) rm,
    #include "instrs.h"
    #undef  INST0
    #undef  INST1
    #undef  INST2
    #undef  INST3
    #undef  INST4
    #undef  INST5
};
// clang-format on

// Returns true iff the give CPU instruction has an RM encoding.
inline bool hasCodeRM(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesRM));
    return ((insCodesRM[ins] != BAD_CODE));
}

/*****************************************************************************
 *
 *  Returns the "reg, [r/m]" encoding of the given CPU instruction.
 */

inline size_t insCodeRM(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesRM));
    assert((insCodesRM[ins] != BAD_CODE));

    return insCodesRM[ins];
}

// clang-format off
const static
size_t          insCodesMI[] =
{
    #define INST0(id, nm, fp, um, rf, wf, mr                )
    #define INST1(id, nm, fp, um, rf, wf, mr                )
    #define INST2(id, nm, fp, um, rf, wf, mr, mi            ) mi,
    #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        ) mi,
    #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) mi,
    #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) mi,
    #include "instrs.h"
    #undef  INST0
    #undef  INST1
    #undef  INST2
    #undef  INST3
    #undef  INST4
    #undef  INST5
};
// clang-format on

// Returns true iff the give CPU instruction has an MI encoding.
inline bool hasCodeMI(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesMI));
    return ((insCodesMI[ins] != BAD_CODE));
}

/*****************************************************************************
 *
 *  Returns the "[r/m], 32-bit icon" encoding of the given CPU instruction.
 */

inline size_t insCodeMI(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesMI));
    assert((insCodesMI[ins] != BAD_CODE));

    return insCodesMI[ins];
}

// clang-format off
const static
size_t          insCodesMR[] =
{
    #define INST0(id, nm, fp, um, rf, wf, mr                )
    #define INST1(id, nm, fp, um, rf, wf, mr                ) mr,
    #define INST2(id, nm, fp, um, rf, wf, mr, mi            ) mr,
    #define INST3(id, nm, fp, um, rf, wf, mr, mi, rm        ) mr,
    #define INST4(id, nm, fp, um, rf, wf, mr, mi, rm, a4    ) mr,
    #define INST5(id, nm, fp, um, rf, wf, mr, mi, rm, a4, rr) mr,
    #include "instrs.h"
    #undef  INST0
    #undef  INST1
    #undef  INST2
    #undef  INST3
    #undef  INST4
    #undef  INST5
};
// clang-format on

// Returns true iff the give CPU instruction has an MR encoding.
inline bool hasCodeMR(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesMR));
    return ((insCodesMR[ins] != BAD_CODE));
}

/*****************************************************************************
 *
 *  Returns the "[r/m], reg" or "[r/m]" encoding of the given CPU instruction.
 */

inline size_t insCodeMR(instruction ins)
{
    assert((unsigned)ins < _countof(insCodesMR));
    assert((insCodesMR[ins] != BAD_CODE));

    return insCodesMR[ins];
}

// Return true if the instruction uses the SSE38 or SSE3A macro in instrsXArch.h.
bool emitter::EncodedBySSE38orSSE3A(instruction ins)
{
    const size_t SSE38 = 0x0F660038;
    const size_t SSE3A = 0x0F66003A;
    const size_t MASK  = 0xFFFF00FF;

    size_t insCode = 0;

    if (hasCodeRM(ins))
    {
        insCode = insCodeRM(ins);
    }
    else if (hasCodeMI(ins))
    {
        insCode = insCodeMI(ins);
    }
    else if (hasCodeMR(ins))
    {
        insCode = insCodeMR(ins);
    }

    insCode &= MASK;
    return insCode == SSE38 || insCode == SSE3A;
}

/*****************************************************************************
 *
 *  Returns an encoding for the specified register to be used in the bit0-2
 *  part of an opcode.
 */

inline unsigned emitter::insEncodeReg012(instruction ins, regNumber reg, emitAttr size, code_t* code)
{
    assert(reg < REG_STK);

#ifdef _TARGET_AMD64_
    // Either code is not NULL or reg is not an extended reg.
    // If reg is an extended reg, instruction needs to be prefixed with 'REX'
    // which would require code != NULL.
    assert(code != nullptr || !IsExtendedReg(reg));

    if (IsExtendedReg(reg))
    {
        *code = AddRexBPrefix(ins, *code); // REX.B
    }
    else if ((EA_SIZE(size) == EA_1BYTE) && (reg > REG_RBX) && (code != nullptr))
    {
        // We are assuming that we only use/encode SPL, BPL, SIL and DIL
        // not the corresponding AH, CH, DH, or BH
        *code = AddRexPrefix(ins, *code); // REX
    }
#endif // _TARGET_AMD64_

    unsigned regBits = RegEncoding(reg);

    assert(regBits < 8);
    return regBits;
}

/*****************************************************************************
 *
 *  Returns an encoding for the specified register to be used in the bit3-5
 *  part of an opcode.
 */

inline unsigned emitter::insEncodeReg345(instruction ins, regNumber reg, emitAttr size, code_t* code)
{
    assert(reg < REG_STK);

#ifdef _TARGET_AMD64_
    // Either code is not NULL or reg is not an extended reg.
    // If reg is an extended reg, instruction needs to be prefixed with 'REX'
    // which would require code != NULL.
    assert(code != nullptr || !IsExtendedReg(reg));

    if (IsExtendedReg(reg))
    {
        *code = AddRexRPrefix(ins, *code); // REX.R
    }
    else if ((EA_SIZE(size) == EA_1BYTE) && (reg > REG_RBX) && (code != nullptr))
    {
        // We are assuming that we only use/encode SPL, BPL, SIL and DIL
        // not the corresponding AH, CH, DH, or BH
        *code = AddRexPrefix(ins, *code); // REX
    }
#endif // _TARGET_AMD64_

    unsigned regBits = RegEncoding(reg);

    assert(regBits < 8);
    return (regBits << 3);
}

/***********************************************************************************
 *
 *  Returns modified AVX opcode with the specified register encoded in bits 3-6 of
 *  byte 2 of VEX prefix.
 */
inline emitter::code_t emitter::insEncodeReg3456(instruction ins, regNumber reg, emitAttr size, code_t code)
{
    assert(reg < REG_STK);
    assert(IsAVXInstruction(ins));
    assert(hasVexPrefix(code));

    // Get 4-bit register encoding
    // RegEncoding() gives lower 3 bits
    // IsExtendedReg() gives MSB.
    code_t regBits = RegEncoding(reg);
    if (IsExtendedReg(reg))
    {
        regBits |= 0x08;
    }

    // VEX prefix encodes register operand in 1's complement form
    // Shift count = 4-bytes of opcode + 0-2 bits
    assert(regBits <= 0xF);
    regBits <<= 35;
    return code ^ regBits;
}

/*****************************************************************************
 *
 *  Returns an encoding for the specified register to be used in the bit3-5
 *  part of an SIB byte (unshifted).
 *  Used exclusively to generate the REX.X bit and truncate the register.
 */

inline unsigned emitter::insEncodeRegSIB(instruction ins, regNumber reg, code_t* code)
{
    assert(reg < REG_STK);

#ifdef _TARGET_AMD64_
    // Either code is not NULL or reg is not an extended reg.
    // If reg is an extended reg, instruction needs to be prefixed with 'REX'
    // which would require code != NULL.
    assert(code != nullptr || reg < REG_R8 || (reg >= REG_XMM0 && reg < REG_XMM8));

    if (IsExtendedReg(reg))
    {
        *code = AddRexXPrefix(ins, *code); // REX.X
    }
    unsigned regBits = RegEncoding(reg);
#else  // !_TARGET_AMD64_
    unsigned regBits = reg;
#endif // !_TARGET_AMD64_

    assert(regBits < 8);
    return regBits;
}

/*****************************************************************************
 *
 *  Returns the "[r/m]" opcode with the mod/RM field set to register.
 */

inline emitter::code_t emitter::insEncodeMRreg(instruction ins, code_t code)
{
    // If Byte 4 (which is 0xFF00) is 0, that's where the RM encoding goes.
    // Otherwise, it will be placed after the 4 byte encoding.
    if ((code & 0xFF00) == 0)
    {
        assert((code & 0xC000) == 0);
        code |= 0xC000;
    }

    return code;
}

/*****************************************************************************
 *
 *  Returns the given "[r/m]" opcode with the mod/RM field set to register.
 */

inline emitter::code_t emitter::insEncodeRMreg(instruction ins, code_t code)
{
    // If Byte 4 (which is 0xFF00) is 0, that's where the RM encoding goes.
    // Otherwise, it will be placed after the 4 byte encoding.
    if ((code & 0xFF00) == 0)
    {
        assert((code & 0xC000) == 0);
        code |= 0xC000;
    }
    return code;
}

/*****************************************************************************
 *
 *  Returns the "byte ptr [r/m]" opcode with the mod/RM field set to
 *  the given register.
 */

inline emitter::code_t emitter::insEncodeMRreg(instruction ins, regNumber reg, emitAttr size, code_t code)
{
    assert((code & 0xC000) == 0);
    code |= 0xC000;
    unsigned regcode = insEncodeReg012(ins, reg, size, &code) << 8;
    code |= regcode;
    return code;
}

/*****************************************************************************
 *
 *  Returns the "byte ptr [r/m], icon" opcode with the mod/RM field set to
 *  the given register.
 */

inline emitter::code_t emitter::insEncodeMIreg(instruction ins, regNumber reg, emitAttr size, code_t code)
{
    assert((code & 0xC000) == 0);
    code |= 0xC000;
    unsigned regcode = insEncodeReg012(ins, reg, size, &code) << 8;
    code |= regcode;
    return code;
}

/*****************************************************************************
 *
 *  Returns true iff the given instruction does not have a "[r/m], icon" form, but *does* have a
 *  "reg,reg,imm8" form.
 */
inline bool insNeedsRRIb(instruction ins)
{
    // If this list gets longer, use a switch or a table.
    return ins == INS_imul;
}

/*****************************************************************************
 *
 *  Returns the "reg,reg,imm8" opcode with both the reg's set to the
 *  the given register.
 */
inline emitter::code_t emitter::insEncodeRRIb(instruction ins, regNumber reg, emitAttr size)
{
    assert(size == EA_4BYTE); // All we handle for now.
    assert(insNeedsRRIb(ins));
    // If this list gets longer, use a switch, or a table lookup.
    code_t   code    = 0x69c0;
    unsigned regcode = insEncodeReg012(ins, reg, size, &code);
    // We use the same register as source and destination.  (Could have another version that does both regs...)
    code |= regcode;
    code |= (regcode << 3);
    return code;
}

/*****************************************************************************
 *
 *  Returns the "+reg" opcode with the the given register set into the low
 *  nibble of the opcode
 */

inline emitter::code_t emitter::insEncodeOpreg(instruction ins, regNumber reg, emitAttr size)
{
    code_t   code    = insCodeRR(ins);
    unsigned regcode = insEncodeReg012(ins, reg, size, &code);
    code |= regcode;
    return code;
}

/*****************************************************************************
 *
 *  Return the 'SS' field value for the given index scale factor.
 */

inline unsigned emitter::insSSval(unsigned scale)
{
    assert(scale == 1 || scale == 2 || scale == 4 || scale == 8);

    const static BYTE scales[] = {
        0x00, // 1
        0x40, // 2
        0xFF, // 3
        0x80, // 4
        0xFF, // 5
        0xFF, // 6
        0xFF, // 7
        0xC0, // 8
    };

    return scales[scale - 1];
}

const instruction emitJumpKindInstructions[] = {INS_nop,

#define JMP_SMALL(en, rev, ins) INS_##ins,
#include "emitjmps.h"

                                                INS_call};

const emitJumpKind emitReverseJumpKinds[] = {
    EJ_NONE,

#define JMP_SMALL(en, rev, ins) EJ_##rev,
#include "emitjmps.h"
};

/*****************************************************************************
 * Look up the instruction for a jump kind
 */

/*static*/ instruction emitter::emitJumpKindToIns(emitJumpKind jumpKind)
{
    assert((unsigned)jumpKind < ArrLen(emitJumpKindInstructions));
    return emitJumpKindInstructions[jumpKind];
}

/*****************************************************************************
 * Reverse the conditional jump
 */

/* static */ emitJumpKind emitter::emitReverseJumpKind(emitJumpKind jumpKind)
{
    assert(jumpKind < EJ_COUNT);
    return emitReverseJumpKinds[jumpKind];
}

/*****************************************************************************
 * The size for these instructions is less than EA_4BYTE,
 * but the target register need not be byte-addressable
 */

inline bool emitInstHasNoCode(instruction ins)
{
    if (ins == INS_align)
    {
        return true;
    }

    return false;
}

/*****************************************************************************
 * When encoding instructions that operate on byte registers
 * we have to ensure that we use a low register (EAX, EBX, ECX or EDX)
 * otherwise we will incorrectly encode the instruction
 */

bool emitter::emitVerifyEncodable(instruction ins, emitAttr size, regNumber reg1, regNumber reg2 /* = REG_NA */)
{
#if CPU_HAS_BYTE_REGS
    if (size != EA_1BYTE) // Not operating on a byte register is fine
    {
        return true;
    }

    if ((ins != INS_movsx) && // These three instructions support high register
        (ins != INS_movzx)    // encodings for reg1
#ifdef FEATURE_HW_INTRINSICS
        && (ins != INS_crc32)
#endif
            )
    {
        // reg1 must be a byte-able register
        if ((genRegMask(reg1) & RBM_BYTE_REGS) == 0)
        {
            return false;
        }
    }
    // if reg2 is not REG_NA then reg2 must be a byte-able register
    if ((reg2 != REG_NA) && ((genRegMask(reg2) & RBM_BYTE_REGS) == 0))
    {
        return false;
    }
#endif
    // The instruction can be encoded
    return true;
}

/*****************************************************************************
 *
 *  Estimate the size (in bytes of generated code) of the given instruction.
 */

inline UNATIVE_OFFSET emitter::emitInsSize(code_t code)
{
    UNATIVE_OFFSET size = (code & 0xFF000000) ? 4 : (code & 0x00FF0000) ? 3 : 2;
#ifdef _TARGET_AMD64_
    size += emitGetPrefixSize(code);
#endif
    return size;
}

inline UNATIVE_OFFSET emitter::emitInsSizeRM(instruction ins)
{
    return emitInsSize(insCodeRM(ins));
}

inline UNATIVE_OFFSET emitter::emitInsSizeRR(instruction ins, regNumber reg1, regNumber reg2, emitAttr attr)
{
    emitAttr size = EA_SIZE(attr);

    UNATIVE_OFFSET sz;

    // If Byte 4 (which is 0xFF00) is zero, that's where the RM encoding goes.
    // Otherwise, it will be placed after the 4 byte encoding, making the total 5 bytes.
    // This would probably be better expressed as a different format or something?
    code_t code = insCodeRM(ins);

    if ((code & 0xFF00) != 0)
    {
        sz = 5;
    }
    else
    {
        sz = emitInsSize(insEncodeRMreg(ins, code));
    }

    // Most 16-bit operand instructions will need a prefix
    if (size == EA_2BYTE && ins != INS_movsx && ins != INS_movzx)
    {
        sz += 1;
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, size, insCodeRM(ins));

    // REX prefix
    if (!hasRexPrefix(code))
    {
        if ((TakesRexWPrefix(ins, size) && ((ins != INS_xor) || (reg1 != reg2))) || IsExtendedReg(reg1, attr) ||
            IsExtendedReg(reg2, attr))
        {
            sz += emitGetRexPrefixSize(ins);
        }
    }

    return sz;
}

/*****************************************************************************/

inline UNATIVE_OFFSET emitter::emitInsSizeSV(code_t code, int var, int dsp)
{
    UNATIVE_OFFSET size = emitInsSize(code);
    UNATIVE_OFFSET offs;
    bool           offsIsUpperBound = true;
    bool           EBPbased         = true;

    /*  Is this a temporary? */

    if (var < 0)
    {
        /* An address off of ESP takes an extra byte */

        if (!emitHasFramePtr)
        {
            size++;
        }

        // The offset is already assigned. Find the temp.
        TempDsc* tmp = emitComp->tmpFindNum(var, Compiler::TEMP_USAGE_USED);
        if (tmp == nullptr)
        {
            // It might be in the free lists, if we're working on zero initializing the temps.
            tmp = emitComp->tmpFindNum(var, Compiler::TEMP_USAGE_FREE);
        }
        assert(tmp != nullptr);
        offs = tmp->tdTempOffs();

        // We only care about the magnitude of the offset here, to determine instruction size.
        if (emitComp->isFramePointerUsed())
        {
            if ((int)offs < 0)
            {
                offs = -(int)offs;
            }
        }
        else
        {
            // SP-based offsets must already be positive.
            assert((int)offs >= 0);
        }
    }
    else
    {

        /* Get the frame offset of the (non-temp) variable */

        offs = dsp + emitComp->lvaFrameAddress(var, &EBPbased);

        /* An address off of ESP takes an extra byte */

        if (!EBPbased)
        {
            ++size;
        }

        /* Is this a stack parameter reference? */

        if (emitComp->lvaIsParameter(var)
#if !defined(_TARGET_AMD64_) || defined(UNIX_AMD64_ABI)
            && !emitComp->lvaIsRegArgument(var)
#endif // !_TARGET_AMD64_ || UNIX_AMD64_ABI
                )
        {
            /* If no EBP frame, arguments are off of ESP, above temps */

            if (!EBPbased)
            {
                assert((int)offs >= 0);

                offsIsUpperBound = false; // since #temps can increase
                offs += emitMaxTmpSize;
            }
        }
        else
        {
            /* Locals off of EBP are at negative offsets */

            if (EBPbased)
            {
#if defined(_TARGET_AMD64_) && !defined(UNIX_AMD64_ABI)
                // If localloc is not used, then ebp chaining is done and hence
                // offset of locals will be at negative offsets, Otherwise offsets
                // will be positive.  In future, when RBP gets positioned in the
                // middle of the frame so as to optimize instruction encoding size,
                // the below asserts needs to be modified appropriately.
                // However, for Unix platforms, we always do frame pointer chaining,
                // so offsets from the frame pointer will always be negative.
                if (emitComp->compLocallocUsed || emitComp->opts.compDbgEnC)
                {
                    noway_assert((int)offs >= 0);
                }
                else
#endif
                {
                    // Dev10 804810 - failing this assert can lead to bad codegen and runtime crashes
                    CLANG_FORMAT_COMMENT_ANCHOR;

#ifdef UNIX_AMD64_ABI
                    LclVarDsc* varDsc         = emitComp->lvaTable + var;
                    bool       isRegPassedArg = varDsc->lvIsParam && varDsc->lvIsRegArg;
                    // Register passed args could have a stack offset of 0.
                    noway_assert((int)offs < 0 || isRegPassedArg);
#else  // !UNIX_AMD64_ABI
                    noway_assert((int)offs < 0);
#endif // !UNIX_AMD64_ABI
                }

                assert(emitComp->lvaTempsHaveLargerOffsetThanVars());

                // lvaInlinedPInvokeFrameVar and lvaStubArgumentVar are placed below the temps
                if (unsigned(var) == emitComp->lvaInlinedPInvokeFrameVar ||
                    unsigned(var) == emitComp->lvaStubArgumentVar)
                {
                    offs -= emitMaxTmpSize;
                }

                if ((int)offs < 0)
                {
                    // offset is negative
                    return size + ((int(offs) >= SCHAR_MIN) ? sizeof(char) : sizeof(int));
                }
#ifdef _TARGET_AMD64_
                // This case arises for localloc frames
                else
                {
                    return size + ((offs <= SCHAR_MAX) ? sizeof(char) : sizeof(int));
                }
#endif
            }

            if (emitComp->lvaTempsHaveLargerOffsetThanVars() == false)
            {
                offs += emitMaxTmpSize;
            }
        }
    }

    assert((int)offs >= 0);

#if !FEATURE_FIXED_OUT_ARGS

    /* Are we addressing off of ESP? */

    if (!emitHasFramePtr)
    {
        /* Adjust the effective offset if necessary */

        if (emitCntStackDepth)
            offs += emitCurStackLvl;

        // we could (and used to) check for the special case [sp] here but the stack offset
        // estimator was off, and there is very little harm in overestimating for such a
        // rare case.
    }

#endif // !FEATURE_FIXED_OUT_ARGS

//  printf("lcl = %04X, tmp = %04X, stk = %04X, offs = %04X\n",
//         emitLclSize, emitMaxTmpSize, emitCurStackLvl, offs);

#ifdef _TARGET_AMD64_
    bool useSmallEncoding = (SCHAR_MIN <= (int)offs) && ((int)offs <= SCHAR_MAX);
#else
    bool useSmallEncoding = (offs <= size_t(SCHAR_MAX));
#endif

    // If it is ESP based, and the offset is zero, we will not encode the disp part.
    if (!EBPbased && offs == 0)
    {
        return size;
    }
    else
    {
        return size + (useSmallEncoding ? sizeof(char) : sizeof(int));
    }
}

inline UNATIVE_OFFSET emitter::emitInsSizeSV(instrDesc* id, int var, int dsp, int val)
{
    instruction    ins       = id->idIns();
    UNATIVE_OFFSET valSize   = EA_SIZE_IN_BYTES(id->idOpSize());
    UNATIVE_OFFSET prefix    = 0;
    bool           valInByte = ((signed char)val == val) && (ins != INS_mov) && (ins != INS_test);

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(valSize <= sizeof(int) || !id->idIsCnsReloc());
#endif // _TARGET_AMD64_

    if (valSize > sizeof(int))
    {
        valSize = sizeof(int);
    }

    if (id->idIsCnsReloc())
    {
        valInByte = false; // relocs can't be placed in a byte
        assert(valSize == sizeof(int));
    }

    if (valInByte)
    {
        valSize = sizeof(char);
    }

    // 16-bit operand instructions need a prefix.
    // This referes to 66h size prefix override
    if (id->idOpSize() == EA_2BYTE)
    {
        prefix = 1;
    }

    return prefix + valSize + emitInsSizeSV(insCodeMI(ins), var, dsp);
}

/*****************************************************************************/

static bool baseRegisterRequiresSibByte(regNumber base)
{
#ifdef _TARGET_AMD64_
    return base == REG_ESP || base == REG_R12;
#else
    return base == REG_ESP;
#endif
}

static bool baseRegisterRequiresDisplacement(regNumber base)
{
#ifdef _TARGET_AMD64_
    return base == REG_EBP || base == REG_R13;
#else
    return base == REG_EBP;
#endif
}

UNATIVE_OFFSET emitter::emitInsSizeAM(instrDesc* id, code_t code)
{
    emitAttr    attrSize = id->idOpSize();
    instruction ins      = id->idIns();
    /* The displacement field is in an unusual place for calls */
    ssize_t        dsp       = (ins == INS_call) ? emitGetInsCIdisp(id) : emitGetInsAmdAny(id);
    bool           dspInByte = ((signed char)dsp == (ssize_t)dsp);
    bool           dspIsZero = (dsp == 0);
    UNATIVE_OFFSET size;

    // Note that the values in reg and rgx are used in this method to decide
    // how many bytes will be needed by the address [reg+rgx+cns]
    // this includes the prefix bytes when reg or rgx are registers R8-R15
    regNumber reg;
    regNumber rgx;

    // The idAddr field is a union and only some of the instruction formats use the iiaAddrMode variant
    // these are IF_AWR_*, IF_ARD_*, IF_ARW_* and IF_*_ARD
    // ideally these should really be the only idInsFmts that we see here
    //  but we have some outliers to deal with:
    //     emitIns_R_L adds IF_RWR_LABEL and calls emitInsSizeAM
    //     emitInsRMW adds IF_MRW_CNS, IF_MRW_RRD, IF_MRW_SHF, and calls emitInsSizeAM

    switch (id->idInsFmt())
    {
        case IF_RWR_LABEL:
        case IF_MRW_CNS:
        case IF_MRW_RRD:
        case IF_MRW_SHF:
            reg = REG_NA;
            rgx = REG_NA;
            break;

        default:
            reg = id->idAddr()->iiaAddrMode.amBaseReg;
            rgx = id->idAddr()->iiaAddrMode.amIndxReg;
            break;
    }

    if (id->idIsDspReloc())
    {
        dspInByte = false; // relocs can't be placed in a byte
        dspIsZero = false; // relocs won't always be zero
    }

    if (code & 0xFF000000)
    {
        size = 4;
    }
    else if (code & 0x00FF0000)
    {
        // BT supports 16 bit operands and this code doesn't handle the necessary 66 prefix.
        assert(ins != INS_bt);

        assert((attrSize == EA_4BYTE) || (attrSize == EA_PTRSIZE)    // Only for x64
               || (attrSize == EA_16BYTE) || (attrSize == EA_32BYTE) // only for x64
               || (ins == INS_movzx) || (ins == INS_movsx)
               // The prefetch instructions are always 3 bytes and have part of their modr/m byte hardcoded
               || isPrefetch(ins));
        size = 3;
    }
    else
    {
        size = 2;

        // Most 16-bit operands will require a size prefix.
        // This refers to 66h size prefix override.

        if (attrSize == EA_2BYTE)
        {
            size++;
        }
    }

    size += emitGetVexPrefixAdjustedSize(ins, attrSize, code);

    if (hasRexPrefix(code))
    {
        // REX prefix
        size += emitGetRexPrefixSize(ins);
    }
    else if (TakesRexWPrefix(ins, attrSize))
    {
        // REX.W prefix
        size += emitGetRexPrefixSize(ins);
    }
    else if (IsExtendedReg(reg, EA_PTRSIZE) || IsExtendedReg(rgx, EA_PTRSIZE) ||
             ((ins != INS_call) && IsExtendedReg(id->idReg1(), attrSize)))
    {
        // Should have a REX byte
        size += emitGetRexPrefixSize(ins);
    }

    if (rgx == REG_NA)
    {
        /* The address is of the form "[reg+disp]" */

        if (reg == REG_NA)
        {
            /* The address is of the form "[disp]" */

            size += sizeof(INT32);

#ifdef _TARGET_AMD64_
            // If id is not marked for reloc, add 1 additional byte for SIB that follows disp32
            if (!id->idIsDspReloc())
            {
                size++;
            }
#endif
            return size;
        }

        // If this is just "call reg", we're done.
        if (id->idIsCallRegPtr())
        {
            assert(ins == INS_call);
            assert(dsp == 0);
            return size;
        }

        // If the base register is ESP (or R12 on 64-bit systems), a SIB byte must be used.
        if (baseRegisterRequiresSibByte(reg))
        {
            size++;
        }

        // If the base register is EBP (or R13 on 64-bit systems), a displacement is required.
        // Otherwise, the displacement can be elided if it is zero.
        if (dspIsZero && !baseRegisterRequiresDisplacement(reg))
        {
            return size;
        }

        /* Does the offset fit in a byte? */

        if (dspInByte)
        {
            size += sizeof(char);
        }
        else
        {
            size += sizeof(INT32);
        }
    }
    else
    {
        /* An index register is present */

        size++;

        /* Is the index value scaled? */

        if (emitDecodeScale(id->idAddr()->iiaAddrMode.amScale) > 1)
        {
            /* Is there a base register? */

            if (reg != REG_NA)
            {
                /* The address is "[reg + {2/4/8} * rgx + icon]" */

                if (dspIsZero && !baseRegisterRequiresDisplacement(reg))
                {
                    /* The address is "[reg + {2/4/8} * rgx]" */
                }
                else
                {
                    /* The address is "[reg + {2/4/8} * rgx + disp]" */

                    if (dspInByte)
                    {
                        size += sizeof(char);
                    }
                    else
                    {
                        size += sizeof(int);
                    }
                }
            }
            else
            {
                /* The address is "[{2/4/8} * rgx + icon]" */

                size += sizeof(INT32);
            }
        }
        else
        {
            if (dspIsZero && baseRegisterRequiresDisplacement(reg) && !baseRegisterRequiresDisplacement(rgx))
            {
                /* Swap reg and rgx, such that reg is not EBP/R13 */
                regNumber tmp                       = reg;
                id->idAddr()->iiaAddrMode.amBaseReg = reg = rgx;
                id->idAddr()->iiaAddrMode.amIndxReg = rgx = tmp;
            }

            /* The address is "[reg+rgx+dsp]" */

            if (dspIsZero && !baseRegisterRequiresDisplacement(reg))
            {
                /* This is [reg+rgx]" */
            }
            else
            {
                /* This is [reg+rgx+dsp]" */

                if (dspInByte)
                {
                    size += sizeof(char);
                }
                else
                {
                    size += sizeof(int);
                }
            }
        }
    }

    return size;
}

inline UNATIVE_OFFSET emitter::emitInsSizeAM(instrDesc* id, code_t code, int val)
{
    instruction    ins       = id->idIns();
    UNATIVE_OFFSET valSize   = EA_SIZE_IN_BYTES(id->idOpSize());
    bool           valInByte = ((signed char)val == val) && (ins != INS_mov) && (ins != INS_test);

    // We should never generate BT mem,reg because it has poor performance. BT mem,imm might be useful
    // but it requires special handling of the immediate value (it is always encoded in a byte).
    // Let's not complicate things until this is needed.
    assert(ins != INS_bt);

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(valSize <= sizeof(INT32) || !id->idIsCnsReloc());
#endif // _TARGET_AMD64_

    if (valSize > sizeof(INT32))
    {
        valSize = sizeof(INT32);
    }

    if (id->idIsCnsReloc())
    {
        valInByte = false; // relocs can't be placed in a byte
        assert(valSize == sizeof(INT32));
    }

    if (valInByte)
    {
        valSize = sizeof(char);
    }

    return valSize + emitInsSizeAM(id, code);
}

inline UNATIVE_OFFSET emitter::emitInsSizeCV(instrDesc* id, code_t code)
{
    instruction ins = id->idIns();

    // fgMorph changes any statics that won't fit into 32-bit addresses
    // into constants with an indir, rather than GT_CLS_VAR
    // so we should only hit this path for statics that are RIP-relative
    UNATIVE_OFFSET size = sizeof(INT32);

    // Most 16-bit operand instructions will need a prefix.
    // This refers to 66h size prefix override.

    if (id->idOpSize() == EA_2BYTE && ins != INS_movzx && ins != INS_movsx)
    {
        size++;
    }

    return size + emitInsSize(code);
}

inline UNATIVE_OFFSET emitter::emitInsSizeCV(instrDesc* id, code_t code, int val)
{
    instruction    ins       = id->idIns();
    UNATIVE_OFFSET valSize   = EA_SIZE_IN_BYTES(id->idOpSize());
    bool           valInByte = ((signed char)val == val) && (ins != INS_mov) && (ins != INS_test);

#ifndef _TARGET_AMD64_
    // occasionally longs get here on x86
    if (valSize > sizeof(INT32))
        valSize = sizeof(INT32);
#endif // !_TARGET_AMD64_

    if (id->idIsCnsReloc())
    {
        valInByte = false; // relocs can't be placed in a byte
        assert(valSize == sizeof(INT32));
    }

    if (valInByte)
    {
        valSize = sizeof(char);
    }

    return valSize + emitInsSizeCV(id, code);
}

/*****************************************************************************
 *
 *  Allocate instruction descriptors for instructions with address modes.
 */

inline emitter::instrDesc* emitter::emitNewInstrAmd(emitAttr size, ssize_t dsp)
{
    if (dsp < AM_DISP_MIN || dsp > AM_DISP_MAX)
    {
        instrDescAmd* id = emitAllocInstrAmd(size);

        id->idSetIsLargeDsp();
#ifdef DEBUG
        id->idAddr()->iiaAddrMode.amDisp = AM_DISP_BIG_VAL;
#endif
        id->idaAmdVal = dsp;

        return id;
    }
    else
    {
        instrDesc* id = emitAllocInstr(size);

        id->idAddr()->iiaAddrMode.amDisp = dsp;
        assert(id->idAddr()->iiaAddrMode.amDisp == dsp); // make sure the value fit

        return id;
    }
}

/*****************************************************************************
 *
 *  Set the displacement field in an instruction. Only handles instrDescAmd type.
 */

inline void emitter::emitSetAmdDisp(instrDescAmd* id, ssize_t dsp)
{
    if (dsp < AM_DISP_MIN || dsp > AM_DISP_MAX)
    {
        id->idSetIsLargeDsp();
#ifdef DEBUG
        id->idAddr()->iiaAddrMode.amDisp = AM_DISP_BIG_VAL;
#endif
        id->idaAmdVal = dsp;
    }
    else
    {
        id->idSetIsSmallDsp();
        id->idAddr()->iiaAddrMode.amDisp = dsp;
        assert(id->idAddr()->iiaAddrMode.amDisp == dsp); // make sure the value fit
    }
}

/*****************************************************************************
 *
 *  Allocate an instruction descriptor for an instruction that uses both
 *  an address mode displacement and a constant.
 */

emitter::instrDesc* emitter::emitNewInstrAmdCns(emitAttr size, ssize_t dsp, int cns)
{
    if (dsp >= AM_DISP_MIN && dsp <= AM_DISP_MAX)
    {
        if (cns >= ID_MIN_SMALL_CNS && cns <= ID_MAX_SMALL_CNS)
        {
            instrDesc* id = emitAllocInstr(size);

            id->idSmallCns(cns);

            id->idAddr()->iiaAddrMode.amDisp = dsp;
            assert(id->idAddr()->iiaAddrMode.amDisp == dsp); // make sure the value fit

            return id;
        }
        else
        {
            instrDescCns* id = emitAllocInstrCns(size);

            id->idSetIsLargeCns();
            id->idcCnsVal = cns;

            id->idAddr()->iiaAddrMode.amDisp = dsp;
            assert(id->idAddr()->iiaAddrMode.amDisp == dsp); // make sure the value fit

            return id;
        }
    }
    else
    {
        if (cns >= ID_MIN_SMALL_CNS && cns <= ID_MAX_SMALL_CNS)
        {
            instrDescAmd* id = emitAllocInstrAmd(size);

            id->idSetIsLargeDsp();
#ifdef DEBUG
            id->idAddr()->iiaAddrMode.amDisp = AM_DISP_BIG_VAL;
#endif
            id->idaAmdVal = dsp;

            id->idSmallCns(cns);

            return id;
        }
        else
        {
            instrDescCnsAmd* id = emitAllocInstrCnsAmd(size);

            id->idSetIsLargeCns();
            id->idacCnsVal = cns;

            id->idSetIsLargeDsp();
#ifdef DEBUG
            id->idAddr()->iiaAddrMode.amDisp = AM_DISP_BIG_VAL;
#endif
            id->idacAmdVal = dsp;

            return id;
        }
    }
}

/*****************************************************************************
 *
 *  The next instruction will be a loop head entry point
 *  So insert a dummy instruction here to ensure that
 *  the x86 I-cache alignment rule is followed.
 */

void emitter::emitLoopAlign()
{
    /* Insert a pseudo-instruction to ensure that we align
       the next instruction properly */

    instrDesc* id = emitNewInstrSmall(EA_1BYTE);
    id->idIns(INS_align);
    id->idCodeSize(15); // We may need to skip up to 15 bytes of code
    emitCurIGsize += 15;
}

/*****************************************************************************
 *
 *  Add a NOP instruction of the given size.
 */

void emitter::emitIns_Nop(unsigned size)
{
    assert(size <= 15);

    instrDesc* id = emitNewInstr();
    id->idIns(INS_nop);
    id->idInsFmt(IF_NONE);
    id->idCodeSize(size);

    dispIns(id);
    emitCurIGsize += size;
}

/*****************************************************************************
 *
 *  Add an instruction with no operands.
 */
void emitter::emitIns(instruction ins)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id   = emitNewInstr();
    code_t         code = insCodeMR(ins);

#ifdef DEBUG
    {
        // We cannot have #ifdef inside macro expansion.
        bool assertCond =
            (ins == INS_cdq || ins == INS_int3 || ins == INS_lock || ins == INS_leave || ins == INS_movsb ||
             ins == INS_movsd || ins == INS_movsp || ins == INS_nop || ins == INS_r_movsb || ins == INS_r_movsd ||
             ins == INS_r_movsp || ins == INS_r_stosb || ins == INS_r_stosd || ins == INS_r_stosp || ins == INS_ret ||
             ins == INS_sahf || ins == INS_stosb || ins == INS_stosd || ins == INS_stosp
             // These instructions take zero operands
             || ins == INS_vzeroupper || ins == INS_lfence || ins == INS_mfence || ins == INS_sfence);

        assert(assertCond);
    }
#endif // DEBUG

    assert(!hasRexPrefix(code)); // Can't have a REX bit with no operands, right?

    if (code & 0xFF000000)
    {
        sz = 2; // TODO-XArch-Bug?: Shouldn't this be 4? Or maybe we should assert that we don't see this case.
    }
    else if (code & 0x00FF0000)
    {
        sz = 3;
    }
    else if (code & 0x0000FF00)
    {
        sz = 2;
    }
    else
    {
        sz = 1;
    }

    // vzeroupper includes its 2-byte VEX prefix in its MR code.
    assert((ins != INS_vzeroupper) || (sz == 3));

    insFormat fmt = IF_NONE;

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

// Add an instruction with no operands, but whose encoding depends on the size
// (Only CDQ/CQO currently)
void emitter::emitIns(instruction ins, emitAttr attr)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id   = emitNewInstr(attr);
    code_t         code = insCodeMR(ins);
    assert(ins == INS_cdq);
    assert((code & 0xFFFFFF00) == 0);
    sz = 1;

    insFormat fmt = IF_NONE;

    sz += emitGetVexPrefixAdjustedSize(ins, attr, code);
    if (TakesRexWPrefix(ins, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

//------------------------------------------------------------------------
// emitMapFmtForIns: map the instruction format based on the instruction.
// Shift-by-a-constant instructions have a special format.
//
// Arguments:
//    fmt - the instruction format to map
//    ins - the instruction
//
// Returns:
//    The mapped instruction format.
//
emitter::insFormat emitter::emitMapFmtForIns(insFormat fmt, instruction ins)
{
    switch (ins)
    {
        case INS_rol_N:
        case INS_ror_N:
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
        {
            switch (fmt)
            {
                case IF_RRW_CNS:
                    return IF_RRW_SHF;
                case IF_MRW_CNS:
                    return IF_MRW_SHF;
                case IF_SRW_CNS:
                    return IF_SRW_SHF;
                case IF_ARW_CNS:
                    return IF_ARW_SHF;
                default:
                    unreached();
            }
        }

        default:
            return fmt;
    }
}

//------------------------------------------------------------------------
// emitMapFmtAtoM: map the address mode formats ARD, ARW, and AWR to their direct address equivalents.
//
// Arguments:
//    fmt - the instruction format to map
//
// Returns:
//    The mapped instruction format.
//
emitter::insFormat emitter::emitMapFmtAtoM(insFormat fmt)
{
    switch (fmt)
    {
        case IF_ARD:
            return IF_MRD;
        case IF_AWR:
            return IF_MWR;
        case IF_ARW:
            return IF_MRW;

        case IF_RRD_ARD:
            return IF_RRD_MRD;
        case IF_RWR_ARD:
            return IF_RWR_MRD;
        case IF_RWR_ARD_CNS:
            return IF_RWR_MRD_CNS;
        case IF_RRW_ARD:
            return IF_RRW_MRD;
        case IF_RRW_ARD_CNS:
            return IF_RRW_MRD_CNS;
        case IF_RWR_RRD_ARD:
            return IF_RWR_RRD_MRD;
        case IF_RWR_RRD_ARD_CNS:
            return IF_RWR_RRD_MRD_CNS;

        case IF_ARD_RRD:
            return IF_MRD_RRD;
        case IF_AWR_RRD:
            return IF_MWR_RRD;
        case IF_ARW_RRD:
            return IF_MRW_RRD;

        case IF_ARD_CNS:
            return IF_MRD_CNS;
        case IF_AWR_CNS:
            return IF_MWR_CNS;
        case IF_ARW_CNS:
            return IF_MRW_CNS;

        case IF_AWR_RRD_CNS:
            return IF_MWR_RRD_CNS;

        case IF_ARW_SHF:
            return IF_MRW_SHF;

        default:
            unreached();
    }
}

//------------------------------------------------------------------------
// emitHandleMemOp: For a memory operand, fill in the relevant fields of the instrDesc.
//
// Arguments:
//    indir - the memory operand.
//    id - the instrDesc to fill in.
//    fmt - the instruction format to use. This must be one of the ARD, AWR, or ARW formats. If necessary (such as for
//          GT_CLS_VAR_ADDR), this function will map it to the correct format.
//    ins - the instruction we are generating. This might affect the instruction format we choose.
//
// Assumptions:
//    The correctly sized instrDesc must already be created, e.g., via emitNewInstrAmd() or emitNewInstrAmdCns();
//
// Post-conditions:
//    For base address of int constant:
//        -- the caller must have added the int constant base to the instrDesc when creating it via
//           emitNewInstrAmdCns().
//    For simple address modes (base + scale * index + offset):
//        -- the base register, index register, and scale factor are set.
//        -- the caller must have added the addressing mode offset int constant to the instrDesc when creating it via
//           emitNewInstrAmdCns().
//
//    The instruction format is set.
//
//    idSetIsDspReloc() is called if necessary.
//
void emitter::emitHandleMemOp(GenTreeIndir* indir, instrDesc* id, insFormat fmt, instruction ins)
{
    assert(fmt != IF_NONE);

    GenTree* memBase = indir->Base();

    if ((memBase != nullptr) && memBase->isContained() && (memBase->OperGet() == GT_CLS_VAR_ADDR))
    {
        CORINFO_FIELD_HANDLE fldHnd = memBase->gtClsVar.gtClsVarHnd;

        // Static always need relocs
        if (!jitStaticFldIsGlobAddr(fldHnd))
        {
            // Contract:
            // fgMorphField() changes any statics that won't fit into 32-bit addresses into
            // constants with an indir, rather than GT_CLS_VAR, based on reloc type hint given
            // by VM. Hence emitter should always mark GT_CLS_VAR_ADDR as relocatable.
            //
            // Data section constants: these get allocated close to code block of the method and
            // always addressable IP relative.  These too should be marked as relocatable.

            id->idSetIsDspReloc();
        }

        id->idAddr()->iiaFieldHnd = fldHnd;
        id->idInsFmt(emitMapFmtForIns(emitMapFmtAtoM(fmt), ins));
    }
    else if ((memBase != nullptr) && memBase->IsCnsIntOrI() && memBase->isContained())
    {
        // Absolute addresses marked as contained should fit within the base of addr mode.
        assert(memBase->AsIntConCommon()->FitsInAddrBase(emitComp));

        // Either not generating relocatable code, or addr must be an icon handle, or the
        // constant is zero (which we won't generate a relocation for).
        assert(!emitComp->opts.compReloc || memBase->IsIconHandle() || memBase->IsIntegralConst(0));

        if (memBase->AsIntConCommon()->AddrNeedsReloc(emitComp))
        {
            id->idSetIsDspReloc();
        }

        id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
        id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;
        id->idAddr()->iiaAddrMode.amScale   = emitter::OPSZ1; // for completeness

        id->idInsFmt(emitMapFmtForIns(fmt, ins));

        // Absolute address must have already been set in the instrDesc constructor.
        assert(emitGetInsAmdAny(id) == memBase->AsIntConCommon()->IconValue());
    }
    else
    {
        if (memBase != nullptr)
        {
            id->idAddr()->iiaAddrMode.amBaseReg = memBase->gtRegNum;
        }
        else
        {
            id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
        }

        if (indir->HasIndex())
        {
            id->idAddr()->iiaAddrMode.amIndxReg = indir->Index()->gtRegNum;
        }
        else
        {
            id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;
        }
        id->idAddr()->iiaAddrMode.amScale = emitEncodeScale(indir->Scale());

        id->idInsFmt(emitMapFmtForIns(fmt, ins));

        // disp must have already been set in the instrDesc constructor.
        assert(emitGetInsAmdAny(id) == indir->Offset()); // make sure "disp" is stored properly
    }
}

// Takes care of storing all incoming register parameters
// into its corresponding shadow space (defined by the x64 ABI)
void emitter::spillIntArgRegsToShadowSlots()
{
    unsigned       argNum;
    instrDesc*     id;
    UNATIVE_OFFSET sz;

    assert(emitComp->compGeneratingProlog);

    for (argNum = 0; argNum < MAX_REG_ARG; ++argNum)
    {
        regNumber argReg = intArgRegs[argNum];

        // The offsets for the shadow space start at RSP + 8
        // (right before the caller return address)
        int offset = (argNum + 1) * EA_PTRSIZE;

        id = emitNewInstrAmd(EA_PTRSIZE, offset);
        id->idIns(INS_mov);
        id->idInsFmt(IF_AWR_RRD);
        id->idAddr()->iiaAddrMode.amBaseReg = REG_SPBASE;
        id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;
        id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(1);

        // The offset has already been set in the intrDsc ctor,
        // make sure we got it right.
        assert(emitGetInsAmdAny(id) == ssize_t(offset));

        id->idReg1(argReg);
        sz = emitInsSizeAM(id, insCodeMR(INS_mov));
        id->idCodeSize(sz);
        emitCurIGsize += sz;
    }
}

//------------------------------------------------------------------------
// emitInsLoadInd: Emits a "mov reg, [mem]" (or a variant such as "movzx" or "movss")
// instruction for a GT_IND node.
//
// Arguments:
//    ins - the instruction to emit
//    attr - the instruction operand size
//    dstReg - the destination register
//    mem - the GT_IND node
//
void emitter::emitInsLoadInd(instruction ins, emitAttr attr, regNumber dstReg, GenTreeIndir* mem)
{
    assert(mem->OperIs(GT_IND));

    GenTree* addr = mem->Addr();

    if (addr->OperGet() == GT_CLS_VAR_ADDR)
    {
        emitIns_R_C(ins, attr, dstReg, addr->gtClsVar.gtClsVarHnd, 0);
        return;
    }

    if (addr->OperGet() == GT_LCL_VAR_ADDR)
    {
        GenTreeLclVarCommon* varNode = addr->AsLclVarCommon();
        emitIns_R_S(ins, attr, dstReg, varNode->GetLclNum(), 0);
        codeGen->genUpdateLife(varNode);
        return;
    }

    assert(addr->OperIsAddrMode() || (addr->IsCnsIntOrI() && addr->isContained()) || !addr->isContained());
    ssize_t    offset = mem->Offset();
    instrDesc* id     = emitNewInstrAmd(attr, offset);
    id->idIns(ins);
    id->idReg1(dstReg);
    emitHandleMemOp(mem, id, IF_RWR_ARD, ins);
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

//------------------------------------------------------------------------
// emitInsStoreInd: Emits a "mov [mem], reg/imm" (or a variant such as "movss")
// instruction for a GT_STOREIND node.
//
// Arguments:
//    ins - the instruction to emit
//    attr - the instruction operand size
//    mem - the GT_STOREIND node
//
void emitter::emitInsStoreInd(instruction ins, emitAttr attr, GenTreeStoreInd* mem)
{
    assert(mem->OperIs(GT_STOREIND));

    GenTree* addr = mem->Addr();
    GenTree* data = mem->Data();

    if (addr->OperGet() == GT_CLS_VAR_ADDR)
    {
        if (data->isContainedIntOrIImmed())
        {
            emitIns_C_I(ins, attr, addr->gtClsVar.gtClsVarHnd, 0, (int)data->AsIntConCommon()->IconValue());
        }
        else
        {
            assert(!data->isContained());
            emitIns_C_R(ins, attr, addr->gtClsVar.gtClsVarHnd, data->gtRegNum, 0);
        }
        return;
    }

    if (addr->OperGet() == GT_LCL_VAR_ADDR)
    {
        GenTreeLclVarCommon* varNode = addr->AsLclVarCommon();
        if (data->isContainedIntOrIImmed())
        {
            emitIns_S_I(ins, attr, varNode->GetLclNum(), 0, (int)data->AsIntConCommon()->IconValue());
        }
        else
        {
            assert(!data->isContained());
            emitIns_S_R(ins, attr, data->gtRegNum, varNode->GetLclNum(), 0);
        }
        codeGen->genUpdateLife(varNode);
        return;
    }

    ssize_t        offset = mem->Offset();
    UNATIVE_OFFSET sz;
    instrDesc*     id;

    if (data->isContainedIntOrIImmed())
    {
        int icon = (int)data->AsIntConCommon()->IconValue();
        id       = emitNewInstrAmdCns(attr, offset, icon);
        id->idIns(ins);
        emitHandleMemOp(mem, id, IF_AWR_CNS, ins);
        sz = emitInsSizeAM(id, insCodeMI(ins), icon);
        id->idCodeSize(sz);
    }
    else
    {
        assert(!data->isContained());
        id = emitNewInstrAmd(attr, offset);
        id->idIns(ins);
        emitHandleMemOp(mem, id, IF_AWR_RRD, ins);
        id->idReg1(data->gtRegNum);
        sz = emitInsSizeAM(id, insCodeMR(ins));
        id->idCodeSize(sz);
    }

    dispIns(id);
    emitCurIGsize += sz;
}

//------------------------------------------------------------------------
// emitInsStoreLcl: Emits a "mov [mem], reg/imm" (or a variant such as "movss")
// instruction for a GT_STORE_LCL_VAR node.
//
// Arguments:
//    ins - the instruction to emit
//    attr - the instruction operand size
//    varNode - the GT_STORE_LCL_VAR node
//
void emitter::emitInsStoreLcl(instruction ins, emitAttr attr, GenTreeLclVarCommon* varNode)
{
    assert(varNode->OperIs(GT_STORE_LCL_VAR));
    assert(varNode->gtRegNum == REG_NA); // stack store

    GenTree* data = varNode->gtGetOp1();
    codeGen->inst_set_SV_var(varNode);

    if (data->isContainedIntOrIImmed())
    {
        emitIns_S_I(ins, attr, varNode->GetLclNum(), 0, (int)data->AsIntConCommon()->IconValue());
    }
    else
    {
        assert(!data->isContained());
        emitIns_S_R(ins, attr, data->gtRegNum, varNode->GetLclNum(), 0);
    }
    codeGen->genUpdateLife(varNode);
}

//------------------------------------------------------------------------
// emitInsBinary: Emits an instruction for a node which takes two operands
//
// Arguments:
//    ins - the instruction to emit
//    attr - the instruction operand size
//    dst - the destination and first source operand
//    src - the second source operand
//
// Assumptions:
//  i) caller of this routine needs to call genConsumeReg()
// ii) caller of this routine needs to call genProduceReg()
regNumber emitter::emitInsBinary(instruction ins, emitAttr attr, GenTree* dst, GenTree* src)
{
    // We can only have one memory operand and only src can be a constant operand
    // However, the handling for a given operand type (mem, cns, or other) is fairly
    // consistent regardless of whether they are src or dst. As such, we will find
    // the type of each operand and only check them against src/dst where relevant.

    GenTree* memOp   = nullptr;
    GenTree* cnsOp   = nullptr;
    GenTree* otherOp = nullptr;

    if (dst->isContained() || (dst->isLclField() && (dst->gtRegNum == REG_NA)) || dst->isUsedFromSpillTemp())
    {
        // dst can only be a modrm
        assert(dst->isUsedFromMemory() || (dst->gtRegNum == REG_NA) ||
               instrIs3opImul(ins)); // dst on 3opImul isn't really the dst
        assert(!src->isUsedFromMemory());

        memOp = dst;

        if (src->isContained())
        {
            assert(src->IsCnsIntOrI());
            cnsOp = src;
        }
        else
        {
            otherOp = src;
        }
    }
    else if (src->isContained() || src->isUsedFromSpillTemp())
    {
        assert(!dst->isUsedFromMemory());
        otherOp = dst;

        if ((src->IsCnsIntOrI() || src->IsCnsFltOrDbl()) && !src->isUsedFromSpillTemp())
        {
            assert(!src->isUsedFromMemory() || src->IsCnsFltOrDbl());
            cnsOp = src;
        }
        else
        {
            assert(src->isUsedFromMemory());
            memOp = src;
        }
    }

    // At this point, we either have a memory operand or we don't.
    //
    // If we don't then the logic is very simple and  we will either be emitting a
    // `reg, immed` instruction (if src is a cns) or a `reg, reg` instruction otherwise.
    //
    // If we do have a memory operand, the logic is a bit more complicated as we need
    // to do different things depending on the type of memory operand. These types include:
    //  * Spill temp
    //  * Indirect access
    //    * Local variable
    //    * Class variable
    //    * Addressing mode [base + index * scale + offset]
    //  * Local field
    //  * Local variable
    //
    // Most of these types (except Indirect: Class variable and Indirect: Addressing mode)
    // give us a a local variable number and an offset and access memory on the stack
    //
    // Indirect: Class variable is used for access static class variables and gives us a handle
    // to the memory location we read from
    //
    // Indirect: Addressing mode is used for the remaining memory accesses and will give us
    // a base address, an index, a scale, and an offset. These are combined to let us easily
    // access the given memory location.
    //
    // In all of the memory access cases, we determine which form to emit (e.g. `reg, [mem]`
    // or `[mem], reg`) by comparing memOp to src to determine which `emitIns_*` method needs
    // to be called. The exception is for the `[mem], immed` case (for Indirect: Class variable)
    // where only src can be the immediate.

    if (memOp != nullptr)
    {
        TempDsc* tmpDsc = nullptr;
        unsigned varNum = BAD_VAR_NUM;
        unsigned offset = (unsigned)-1;

        if (memOp->isUsedFromSpillTemp())
        {
            assert(memOp->IsRegOptional());

            tmpDsc = codeGen->getSpillTempDsc(memOp);
            varNum = tmpDsc->tdTempNum();
            offset = 0;

            emitComp->tmpRlsTemp(tmpDsc);
        }
        else if (memOp->isIndir())
        {
            GenTreeIndir* memIndir = memOp->AsIndir();
            GenTree*      memBase  = memIndir->gtOp1;

            switch (memBase->OperGet())
            {
                case GT_LCL_VAR_ADDR:
                {
                    varNum = memBase->AsLclVarCommon()->GetLclNum();
                    offset = 0;

                    // Ensure that all the GenTreeIndir values are set to their defaults.
                    assert(!memIndir->HasIndex());
                    assert(memIndir->Scale() == 1);
                    assert(memIndir->Offset() == 0);

                    break;
                }

                case GT_CLS_VAR_ADDR:
                {
                    if (memOp == src)
                    {
                        assert(otherOp == dst);
                        assert(cnsOp == nullptr);

                        if (instrHasImplicitRegPairDest(ins))
                        {
                            // src is a class static variable
                            // dst is implicit - RDX:RAX
                            emitIns_C(ins, attr, memBase->gtClsVar.gtClsVarHnd, 0);
                        }
                        else
                        {
                            // src is a class static variable
                            // dst is a register
                            emitIns_R_C(ins, attr, dst->gtRegNum, memBase->gtClsVar.gtClsVarHnd, 0);
                        }
                    }
                    else
                    {
                        assert(memOp == dst);

                        if (cnsOp != nullptr)
                        {
                            assert(cnsOp == src);
                            assert(otherOp == nullptr);
                            assert(src->IsCnsIntOrI());

                            // src is an contained immediate
                            // dst is a class static variable
                            emitIns_C_I(ins, attr, memBase->gtClsVar.gtClsVarHnd, 0,
                                        (int)src->gtIntConCommon.IconValue());
                        }
                        else
                        {
                            assert(otherOp == src);

                            // src is a register
                            // dst is a class static variable
                            emitIns_C_R(ins, attr, memBase->gtClsVar.gtClsVarHnd, src->gtRegNum, 0);
                        }
                    }

                    return dst->gtRegNum;
                }

                default: // Addressing mode [base + index * scale + offset]
                {
                    instrDesc* id = nullptr;

                    if (cnsOp != nullptr)
                    {
                        assert(memOp == dst);
                        assert(cnsOp == src);
                        assert(otherOp == nullptr);
                        assert(src->IsCnsIntOrI());

                        id = emitNewInstrAmdCns(attr, memIndir->Offset(), (int)src->gtIntConCommon.IconValue());
                    }
                    else
                    {
                        ssize_t offset = memIndir->Offset();
                        id             = emitNewInstrAmd(attr, offset);
                        id->idIns(ins);

                        GenTree* regTree = (memOp == src) ? dst : src;

                        // there must be one non-contained op
                        assert(!regTree->isContained());
                        id->idReg1(regTree->gtRegNum);
                    }
                    assert(id != nullptr);

                    id->idIns(ins); // Set the instruction.

                    // Determine the instruction format
                    insFormat fmt = IF_NONE;

                    if (memOp == src)
                    {
                        assert(cnsOp == nullptr);
                        assert(otherOp == dst);

                        if (instrHasImplicitRegPairDest(ins))
                        {
                            fmt = emitInsModeFormat(ins, IF_ARD);
                        }
                        else
                        {
                            fmt = emitInsModeFormat(ins, IF_RRD_ARD);
                        }
                    }
                    else
                    {
                        assert(memOp == dst);

                        if (cnsOp != nullptr)
                        {
                            assert(cnsOp == src);
                            assert(otherOp == nullptr);
                            assert(src->IsCnsIntOrI());

                            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
                        }
                        else
                        {
                            assert(otherOp == src);
                            fmt = emitInsModeFormat(ins, IF_ARD_RRD);
                        }
                    }
                    assert(fmt != IF_NONE);
                    emitHandleMemOp(memIndir, id, fmt, ins);

                    // Determine the instruction size
                    UNATIVE_OFFSET sz = 0;

                    if (memOp == src)
                    {
                        assert(otherOp == dst);
                        assert(cnsOp == nullptr);

                        if (instrHasImplicitRegPairDest(ins))
                        {
                            sz = emitInsSizeAM(id, insCode(ins));
                        }
                        else
                        {
                            sz = emitInsSizeAM(id, insCodeRM(ins));
                        }
                    }
                    else
                    {
                        assert(memOp == dst);

                        if (cnsOp != nullptr)
                        {
                            assert(memOp == dst);
                            assert(cnsOp == src);
                            assert(otherOp == nullptr);

                            sz = emitInsSizeAM(id, insCodeMI(ins), (int)src->gtIntConCommon.IconValue());
                        }
                        else
                        {
                            assert(otherOp == src);
                            sz = emitInsSizeAM(id, insCodeMR(ins));
                        }
                    }
                    assert(sz != 0);

                    id->idCodeSize(sz);

                    dispIns(id);
                    emitCurIGsize += sz;

                    return (memOp == src) ? dst->gtRegNum : REG_NA;
                }
            }
        }
        else
        {
            switch (memOp->OperGet())
            {
                case GT_LCL_FLD:
                case GT_STORE_LCL_FLD:
                {
                    GenTreeLclFld* lclField = memOp->AsLclFld();
                    varNum                  = lclField->GetLclNum();
                    offset                  = lclField->gtLclFld.gtLclOffs;
                    break;
                }

                case GT_LCL_VAR:
                {
                    assert(memOp->IsRegOptional() || !emitComp->lvaTable[memOp->gtLclVar.gtLclNum].lvIsRegCandidate());
                    varNum = memOp->AsLclVar()->GetLclNum();
                    offset = 0;
                    break;
                }

                default:
                    unreached();
                    break;
            }
        }

        // Ensure we got a good varNum and offset.
        // We also need to check for `tmpDsc != nullptr` since spill temp numbers
        // are negative and start with -1, which also happens to be BAD_VAR_NUM.
        assert((varNum != BAD_VAR_NUM) || (tmpDsc != nullptr));
        assert(offset != (unsigned)-1);

        if (memOp == src)
        {
            assert(otherOp == dst);
            assert(cnsOp == nullptr);

            if (instrHasImplicitRegPairDest(ins))
            {
                // src is a stack based local variable
                // dst is implicit - RDX:RAX
                emitIns_S(ins, attr, varNum, offset);
            }
            else
            {
                // src is a stack based local variable
                // dst is a register
                emitIns_R_S(ins, attr, dst->gtRegNum, varNum, offset);
            }
        }
        else
        {
            assert(memOp == dst);
            assert((dst->gtRegNum == REG_NA) || dst->IsRegOptional());

            if (cnsOp != nullptr)
            {
                assert(cnsOp == src);
                assert(otherOp == nullptr);
                assert(src->IsCnsIntOrI());

                // src is an contained immediate
                // dst is a stack based local variable
                emitIns_S_I(ins, attr, varNum, offset, (int)src->gtIntConCommon.IconValue());
            }
            else
            {
                assert(otherOp == src);
                assert(!src->isContained());

                // src is a register
                // dst is a stack based local variable
                emitIns_S_R(ins, attr, src->gtRegNum, varNum, offset);
            }
        }
    }
    else if (cnsOp != nullptr) // reg, immed
    {
        assert(cnsOp == src);
        assert(otherOp == dst);

        if (src->IsCnsIntOrI())
        {
            assert(!dst->isContained());
            GenTreeIntConCommon* intCns = src->AsIntConCommon();
            emitIns_R_I(ins, attr, dst->gtRegNum, intCns->IconValue());
        }
        else
        {
            assert(src->IsCnsFltOrDbl());
            GenTreeDblCon* dblCns = src->AsDblCon();

            CORINFO_FIELD_HANDLE hnd = emitFltOrDblConst(dblCns->gtDconVal, emitTypeSize(dblCns));
            emitIns_R_C(ins, attr, dst->gtRegNum, hnd, 0);
        }
    }
    else // reg, reg
    {
        assert(otherOp == nullptr);
        assert(!src->isContained() && !dst->isContained());

        if (instrHasImplicitRegPairDest(ins))
        {
            emitIns_R(ins, attr, src->gtRegNum);
        }
        else
        {
            emitIns_R_R(ins, attr, dst->gtRegNum, src->gtRegNum);
        }
    }

    return dst->gtRegNum;
}

//------------------------------------------------------------------------
// emitInsRMW: Emit logic for Read-Modify-Write binary instructions.
//
// Responsible for emitting a single instruction that will perform an operation of the form:
//      *addr = *addr <BinOp> src
// For example:
//      ADD [RAX], RCX
//
// Arguments:
//    ins - instruction to generate
//    attr - emitter attribute for instruction
//    storeInd - indir for RMW addressing mode
//    src - source operand of instruction
//
// Assumptions:
//    Lowering has taken care of recognizing the StoreInd pattern of:
//          StoreInd( AddressTree, BinOp( Ind ( AddressTree ), Operand ) )
//    The address to store is already sitting in a register.
//
// Notes:
//    This is a no-produce operation, meaning that no register output will
//    be produced for future use in the code stream.
//
void emitter::emitInsRMW(instruction ins, emitAttr attr, GenTreeStoreInd* storeInd, GenTree* src)
{
    GenTree* addr = storeInd->Addr();
    addr          = addr->gtSkipReloadOrCopy();
    assert(addr->OperGet() == GT_LCL_VAR || addr->OperGet() == GT_LCL_VAR_ADDR || addr->OperGet() == GT_LEA ||
           addr->OperGet() == GT_CLS_VAR_ADDR || addr->OperGet() == GT_CNS_INT);

    instrDesc*     id = nullptr;
    UNATIVE_OFFSET sz;

    ssize_t offset = 0;
    if (addr->OperGet() != GT_CLS_VAR_ADDR)
    {
        offset = storeInd->Offset();
    }

    if (src->isContainedIntOrIImmed())
    {
        GenTreeIntConCommon* intConst = src->AsIntConCommon();
        id                            = emitNewInstrAmdCns(attr, offset, (int)intConst->IconValue());
        emitHandleMemOp(storeInd, id, IF_ARW_CNS, ins);
        id->idIns(ins);
        sz = emitInsSizeAM(id, insCodeMI(ins), (int)intConst->IconValue());
    }
    else
    {
        assert(!src->isContained()); // there must be one non-contained src

        // ind, reg
        id = emitNewInstrAmd(attr, offset);
        emitHandleMemOp(storeInd, id, IF_ARW_RRD, ins);
        id->idReg1(src->gtRegNum);
        id->idIns(ins);
        sz = emitInsSizeAM(id, insCodeMR(ins));
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

//------------------------------------------------------------------------
// emitInsRMW: Emit logic for Read-Modify-Write unary instructions.
//
// Responsible for emitting a single instruction that will perform an operation of the form:
//      *addr = UnaryOp *addr
// For example:
//      NOT [RAX]
//
// Arguments:
//    ins - instruction to generate
//    attr - emitter attribute for instruction
//    storeInd - indir for RMW addressing mode
//
// Assumptions:
//    Lowering has taken care of recognizing the StoreInd pattern of:
//          StoreInd( AddressTree, UnaryOp( Ind ( AddressTree ) ) )
//    The address to store is already sitting in a register.
//
// Notes:
//    This is a no-produce operation, meaning that no register output will
//    be produced for future use in the code stream.
//
void emitter::emitInsRMW(instruction ins, emitAttr attr, GenTreeStoreInd* storeInd)
{
    GenTree* addr = storeInd->Addr();
    addr          = addr->gtSkipReloadOrCopy();
    assert(addr->OperGet() == GT_LCL_VAR || addr->OperGet() == GT_LCL_VAR_ADDR || addr->OperGet() == GT_CLS_VAR_ADDR ||
           addr->OperGet() == GT_LEA || addr->OperGet() == GT_CNS_INT);

    ssize_t offset = 0;
    if (addr->OperGet() != GT_CLS_VAR_ADDR)
    {
        offset = storeInd->Offset();
    }

    instrDesc* id = emitNewInstrAmd(attr, offset);
    emitHandleMemOp(storeInd, id, IF_ARW, ins);
    id->idIns(ins);
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction referencing a single register.
 */

void emitter::emitIns_R(instruction ins, emitAttr attr, regNumber reg)
{
    emitAttr size = EA_SIZE(attr);

    assert(size <= EA_PTRSIZE);
    noway_assert(emitVerifyEncodable(ins, size, reg));

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrSmall(attr);

    switch (ins)
    {
        case INS_inc:
        case INS_dec:
#ifdef _TARGET_AMD64_

            sz = 2; // x64 has no 1-byte opcode (it is the same encoding as the REX prefix)

#else // !_TARGET_AMD64_

            if (size == EA_1BYTE)
                sz = 2; // Use the long form as the small one has no 'w' bit
            else
                sz = 1; // Use short form

#endif // !_TARGET_AMD64_

            break;

        case INS_pop:
        case INS_pop_hide:
        case INS_push:
        case INS_push_hide:

            /* We don't currently push/pop small values */

            assert(size == EA_PTRSIZE);

            sz = 1;
            break;

        default:

            /* All the sixteen INS_setCCs are contiguous. */

            if (INS_seto <= ins && ins <= INS_setg)
            {
                // Rough check that we used the endpoints for the range check

                assert(INS_seto + 0xF == INS_setg);

                // The caller must specify EA_1BYTE for 'attr'

                assert(attr == EA_1BYTE);

                /* We expect this to always be a 'big' opcode */

                assert(insEncodeMRreg(ins, reg, attr, insCodeMR(ins)) & 0x00FF0000);

                size = attr;

                sz = 3;
                break;
            }
            else
            {
                sz = 2;
                break;
            }
    }
    insFormat fmt = emitInsModeFormat(ins, IF_RRD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(reg);

    // 16-bit operand instructions will need a prefix.
    // This refers to 66h size prefix override.
    if (size == EA_2BYTE)
    {
        sz += 1;
    }

    // Vex bytes
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insEncodeMRreg(ins, reg, attr, insCodeMR(ins)));

    // REX byte
    if (IsExtendedReg(reg, attr) || TakesRexWPrefix(ins, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

/*****************************************************************************
 *
 *  Add an instruction referencing a register and a constant.
 */

void emitter::emitIns_R_I(instruction ins, emitAttr attr, regNumber reg, ssize_t val)
{
    emitAttr size = EA_SIZE(attr);

    // Allow emitting SSE2/AVX SIMD instructions of R_I form that can specify EA_16BYTE or EA_32BYTE
    assert(size <= EA_PTRSIZE || IsSSEOrAVXInstruction(ins));

    noway_assert(emitVerifyEncodable(ins, size, reg));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(size < EA_8BYTE || ins == INS_mov || ((int)val == val && !EA_IS_CNS_RELOC(attr)));
#endif

    UNATIVE_OFFSET sz;
    instrDesc*     id;
    insFormat      fmt       = emitInsModeFormat(ins, IF_RRD_CNS);
    bool           valInByte = ((signed char)val == val) && (ins != INS_mov) && (ins != INS_test);

    // BT reg,imm might be useful but it requires special handling of the immediate value
    // (it is always encoded in a byte). Let's not complicate things until this is needed.
    assert(ins != INS_bt);

    // Figure out the size of the instruction
    switch (ins)
    {
        case INS_mov:
#ifdef _TARGET_AMD64_
            // mov reg, imm64 is equivalent to mov reg, imm32 if the high order bits are all 0
            // and this isn't a reloc constant.
            if (((size > EA_4BYTE) && (0 == (val & 0xFFFFFFFF00000000LL))) && !EA_IS_CNS_RELOC(attr))
            {
                attr = size = EA_4BYTE;
            }

            if (size > EA_4BYTE)
            {
                sz = 9; // Really it is 10, but we'll add one more later
                break;
            }
#endif // _TARGET_AMD64_
            sz = 5;
            break;

        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_RRW_SHF;
            sz  = 3;
            val &= 0x7F;
            valInByte = true; // shift amount always placed in a byte
            break;

        default:

            if (EA_IS_CNS_RELOC(attr))
            {
                valInByte = false; // relocs can't be placed in a byte
            }

            if (valInByte)
            {
                if (IsSSEOrAVXInstruction(ins))
                {
                    sz = 5;
                }
                else if (size == EA_1BYTE && reg == REG_EAX && !instrIs3opImul(ins))
                {
                    sz = 2;
                }
                else
                {
                    sz = 3;
                }
            }
            else
            {
                if (reg == REG_EAX && !instrIs3opImul(ins))
                {
                    sz = 1;
                }
                else
                {
                    sz = 2;
                }

#ifdef _TARGET_AMD64_
                if (size > EA_4BYTE)
                {
                    // We special-case anything that takes a full 8-byte constant.
                    sz += 4;
                }
                else
#endif // _TARGET_AMD64_
                {
                    sz += EA_SIZE_IN_BYTES(attr);
                }
            }
            break;
    }

    // Vex prefix size
    sz += emitGetVexPrefixSize(ins, attr);

    // Do we need a REX prefix for AMD64? We need one if we are using any extended register (REX.R), or if we have a
    // 64-bit sized operand (REX.W). Note that IMUL in our encoding is special, with a "built-in", implicit, target
    // register. So we also need to check if that built-in register is an extended register.
    if (IsExtendedReg(reg, attr) || TakesRexWPrefix(ins, size) || instrIsExtendedReg3opImul(ins))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id = emitNewInstrSC(attr, val);
    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(reg);

    // 16-bit operand instructions will need a prefix
    if (size == EA_2BYTE)
    {
        sz += 1;
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    if (reg == REG_ESP)
    {
        emitAdjustStackDepth(ins, val);
    }
}

/*****************************************************************************
 *
 *  Add an instruction referencing an integer constant.
 */

void emitter::emitIns_I(instruction ins, emitAttr attr, int val)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id;
    bool           valInByte = ((signed char)val == val);

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    if (EA_IS_CNS_RELOC(attr))
    {
        valInByte = false; // relocs can't be placed in a byte
    }

    switch (ins)
    {
        case INS_loop:
        case INS_jge:
            sz = 2;
            break;

        case INS_ret:
            sz = 3;
            break;

        case INS_push_hide:
        case INS_push:
            sz = valInByte ? 2 : 5;
            break;

        default:
            NO_WAY("unexpected instruction");
    }

    id = emitNewInstrSC(attr, val);
    id->idIns(ins);
    id->idInsFmt(IF_CNS);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

/*****************************************************************************
 *
 *  Add a "jump through a table" instruction.
 */

void emitter::emitIns_IJ(emitAttr attr, regNumber reg, unsigned base)
{
    assert(EA_SIZE(attr) == EA_4BYTE);

    UNATIVE_OFFSET    sz  = 3 + 4;
    const instruction ins = INS_i_jmp;

    if (IsExtendedReg(reg, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    instrDesc* id = emitNewInstrAmd(attr, base);

    id->idIns(ins);
    id->idInsFmt(IF_ARD);
    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = reg;
    id->idAddr()->iiaAddrMode.amScale   = emitter::OPSZP;

#ifdef DEBUG
    id->idDebugOnlyInfo()->idMemCookie = base;
#endif

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction with a static data member operand. If 'size' is 0, the
 *  instruction operates on the address of the static member instead of its
 *  value (e.g. "push offset clsvar", rather than "push dword ptr [clsvar]").
 */

void emitter::emitIns_C(instruction ins, emitAttr attr, CORINFO_FIELD_HANDLE fldHnd, int offs)
{
    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    UNATIVE_OFFSET sz;
    instrDesc*     id;

    /* Are we pushing the offset of the class variable? */

    if (EA_IS_OFFSET(attr))
    {
        assert(ins == INS_push);
        sz = 1 + TARGET_POINTER_SIZE;

        id = emitNewInstrDsp(EA_1BYTE, offs);
        id->idIns(ins);
        id->idInsFmt(IF_MRD_OFF);
    }
    else
    {
        insFormat fmt = emitInsModeFormat(ins, IF_MRD);

        id = emitNewInstrDsp(attr, offs);
        id->idIns(ins);
        id->idInsFmt(fmt);
        sz = emitInsSizeCV(id, insCodeMR(ins));
    }

    // Vex prefix size
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMR(ins));

    if (TakesRexWPrefix(ins, attr))
    {
        // REX.W prefix
        sz += emitGetRexPrefixSize(ins);
    }

    id->idAddr()->iiaFieldHnd = fldHnd;

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

/*****************************************************************************
 *
 *  Add an instruction with two register operands.
 */

void emitter::emitIns_R_R(instruction ins, emitAttr attr, regNumber reg1, regNumber reg2)
{
    emitAttr size = EA_SIZE(attr);

    /* We don't want to generate any useless mov instructions! */
    CLANG_FORMAT_COMMENT_ANCHOR;

#ifdef _TARGET_AMD64_
    // Same-reg 4-byte mov can be useful because it performs a
    // zero-extension to 8 bytes.
    assert(ins != INS_mov || reg1 != reg2 || size == EA_4BYTE);
#else
    assert(ins != INS_mov || reg1 != reg2);
#endif // _TARGET_AMD64_

    assert(size <= EA_32BYTE);
    noway_assert(emitVerifyEncodable(ins, size, reg1, reg2));

    UNATIVE_OFFSET sz = emitInsSizeRR(ins, reg1, reg2, attr);

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require one additional byte
        sz += 1;
    }

    /* Special case: "XCHG" uses a different format */
    insFormat fmt = (ins == INS_xchg) ? IF_RRW_RRW : emitInsModeFormat(ins, IF_RRD_RRD);

    instrDesc* id = emitNewInstrSmall(attr);
    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction with two register operands and an integer constant.
 */

void emitter::emitIns_R_R_I(instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, int ival)
{
    // SSE2 version requires 5 bytes and SSE4/AVX version 6 bytes
    UNATIVE_OFFSET sz = 4;
    if (IsSSEOrAVXInstruction(ins))
    {
        // AVX: 3 byte VEX prefix + 1 byte opcode + 1 byte ModR/M + 1 byte immediate
        // SSE4: 4 byte opcode + 1 byte ModR/M + 1 byte immediate
        // SSE2: 3 byte opcode + 1 byte ModR/M + 1 byte immediate
        sz = (UseVEXEncoding() || UseSSE4()) ? 6 : 5;
    }

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    instrDesc* id = emitNewInstrSC(attr, ival);

    // REX prefix
    if (IsExtendedReg(reg1, attr) || IsExtendedReg(reg2, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    if ((ins == INS_pextrq || ins == INS_pinsrq) && !UseVEXEncoding())
    {
        assert(UseSSE4());
        sz += 1;
    }

    id->idIns(ins);
    id->idInsFmt(IF_RRW_RRW_CNS);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_AR(instruction ins, emitAttr attr, regNumber base, int offs)
{
    assert(ins == INS_prefetcht0 || ins == INS_prefetcht1 || ins == INS_prefetcht2 || ins == INS_prefetchnta);

    instrDesc* id = emitNewInstrAmd(attr, offs);

    id->idIns(ins);

    id->idInsFmt(IF_ARD);
    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_A(instruction ins, emitAttr attr, regNumber reg1, GenTreeIndir* indir, insFormat fmt)
{
    ssize_t    offs = indir->Offset();
    instrDesc* id   = emitNewInstrAmd(attr, offs);

    id->idIns(ins);
    id->idReg1(reg1);

    emitHandleMemOp(indir, id, fmt, ins);

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_A_I(instruction ins, emitAttr attr, regNumber reg1, GenTreeIndir* indir, int ival)
{
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), reg1));
    assert(IsSSEOrAVXInstruction(ins));

    ssize_t    offs = indir->Offset();
    instrDesc* id   = emitNewInstrAmdCns(attr, offs, ival);

    id->idIns(ins);
    id->idReg1(reg1);

    emitHandleMemOp(indir, id, IF_RRW_ARD_CNS, ins);

    // Plus one for the 1-byte immediate (ival)
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require two additional bytes
        sz += 2;
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_AR_I(instruction ins, emitAttr attr, regNumber reg1, regNumber base, int offs, int ival)
{
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), reg1));
    assert(IsSSEOrAVXInstruction(ins));

    instrDesc* id = emitNewInstrAmdCns(attr, offs, ival);

    id->idIns(ins);
    id->idReg1(reg1);

    id->idInsFmt(IF_RRW_ARD_CNS);
    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    // Plus one for the 1-byte immediate (ival)
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require two additional bytes
        sz += 2;
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_C_I(
    instruction ins, emitAttr attr, regNumber reg1, CORINFO_FIELD_HANDLE fldHnd, int offs, int ival)
{
    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), reg1));
    assert(IsSSEOrAVXInstruction(ins));

    instrDesc*     id = emitNewInstrCnsDsp(attr, ival, offs);
    UNATIVE_OFFSET sz = emitInsSizeCV(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require two additional bytes
        sz += 2;
    }

    id->idIns(ins);
    id->idInsFmt(IF_RRW_MRD_CNS);
    id->idReg1(reg1);
    id->idAddr()->iiaFieldHnd = fldHnd;

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_S_I(instruction ins, emitAttr attr, regNumber reg1, int varx, int offs, int ival)
{
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), reg1));
    assert(IsSSEOrAVXInstruction(ins));

    instrDesc*     id = emitNewInstrCns(attr, ival);
    UNATIVE_OFFSET sz =
        emitInsSizeSV(insCodeRM(ins), varx, offs) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require two additional bytes
        sz += 2;
    }

    id->idIns(ins);
    id->idInsFmt(IF_RRW_SRD_CNS);
    id->idReg1(reg1);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);

#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_A(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, GenTreeIndir* indir, insFormat fmt)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    ssize_t    offs = indir->Offset();
    instrDesc* id   = emitNewInstrAmd(attr, offs);

    id->idIns(ins);
    id->idReg1(reg1);
    id->idReg2(reg2);

    emitHandleMemOp(indir, id, fmt, ins);

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_AR(instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, regNumber base, int offs)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    instrDesc* id = emitNewInstrAmd(attr, offs);

    id->idIns(ins);
    id->idReg1(reg1);
    id->idReg2(reg2);

    id->idInsFmt(IF_RWR_RRD_ARD);
    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_C(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, CORINFO_FIELD_HANDLE fldHnd, int offs)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    instrDesc*     id = emitNewInstrDsp(attr, offs);
    UNATIVE_OFFSET sz = emitInsSizeCV(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));

    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_MRD);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idAddr()->iiaFieldHnd = fldHnd;

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
*
*  Add an instruction with three register operands.
*/

void emitter::emitIns_R_R_R(instruction ins, emitAttr attr, regNumber targetReg, regNumber reg1, regNumber reg2)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));
    // Currently vex prefix only use three bytes mode.
    // size = vex + opcode + ModR/M = 3 + 1 + 1 = 5
    // TODO-XArch-CQ: We should create function which can calculate all kinds of AVX instructions size in future
    UNATIVE_OFFSET sz = 5;

    instrDesc* id = emitNewInstr(attr);
    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_RRD);
    id->idReg1(targetReg);
    id->idReg2(reg1);
    id->idReg3(reg2);

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_S(instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, int varx, int offs)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    instrDesc*     id = emitNewInstr(attr);
    UNATIVE_OFFSET sz =
        emitInsSizeSV(insCodeRM(ins), varx, offs) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));

    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_SRD);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);

#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_A_I(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, GenTreeIndir* indir, int ival, insFormat fmt)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    ssize_t    offs = indir->Offset();
    instrDesc* id   = emitNewInstrAmdCns(attr, offs, ival);

    id->idIns(ins);
    id->idReg1(reg1);
    id->idReg2(reg2);

    emitHandleMemOp(indir, id, fmt, ins);

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_AR_I(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, regNumber base, int offs, int ival)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    instrDesc* id = emitNewInstrAmdCns(attr, offs, ival);

    id->idIns(ins);
    id->idReg1(reg1);
    id->idReg2(reg2);

    id->idInsFmt(IF_RWR_RRD_ARD_CNS);
    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    // Plus one for the 1-byte immediate (ival)
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_C_I(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, CORINFO_FIELD_HANDLE fldHnd, int offs, int ival)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    instrDesc*     id = emitNewInstrCnsDsp(attr, ival, offs);
    UNATIVE_OFFSET sz = emitInsSizeCV(id, insCodeRM(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_MRD_CNS);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idAddr()->iiaFieldHnd = fldHnd;

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

/**********************************************************************************
* emitIns_R_R_R_I: Add an instruction with three register operands and an immediate.
*
* Arguments:
*    ins       - the instruction to add
*    attr      - the emitter attribute for instruction
*    targetReg - the target (destination) register
*    reg1      - the first source register
*    reg2      - the second source register
*    ival      - the immediate value
*/

void emitter::emitIns_R_R_R_I(
    instruction ins, emitAttr attr, regNumber targetReg, regNumber reg1, regNumber reg2, int ival)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));
    // Currently vex prefix only use three bytes mode.
    // size = vex + opcode + ModR/M + 1-byte-cns = 3 + 1 + 1 + 1 = 6
    // TODO-XArch-CQ: We should create function which can calculate all kinds of AVX instructions size in future
    UNATIVE_OFFSET sz = 6;

    instrDesc* id = emitNewInstrCns(attr, ival);
    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_RRD_CNS);
    id->idReg1(targetReg);
    id->idReg2(reg1);
    id->idReg3(reg2);

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_S_I(
    instruction ins, emitAttr attr, regNumber reg1, regNumber reg2, int varx, int offs, int ival)
{
    assert(IsSSEOrAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins));

    instrDesc*     id = emitNewInstrCns(attr, ival);
    UNATIVE_OFFSET sz =
        emitInsSizeSV(insCodeRM(ins), varx, offs) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins)) + 1;

    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_SRD_CNS);
    id->idReg1(reg1);
    id->idReg2(reg2);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);

#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_R_R_R(
    instruction ins, emitAttr attr, regNumber targetReg, regNumber reg1, regNumber reg2, regNumber reg3)
{
    assert(isAvxBlendv(ins));
    assert(UseVEXEncoding());
    // Currently vex prefix only use three bytes mode.
    // size = vex + opcode + ModR/M + 1-byte-cns(Reg) = 3 + 1 + 1 + 1 = 6
    // TODO-XArch-CQ: We should create function which can calculate all kinds of AVX instructions size in future
    UNATIVE_OFFSET sz = 6;

    // AVX/AVX2 supports 4-reg format for vblendvps/vblendvpd/vpblendvb,
    // which encodes the fourth register into imm8[7:4]
    int ival = (reg3 - XMMBASE) << 4; // convert reg3 to ival

    instrDesc* id = emitNewInstrCns(attr, ival);
    id->idIns(ins);
    id->idInsFmt(IF_RWR_RRD_RRD_RRD);
    id->idReg1(targetReg);
    id->idReg2(reg1);
    id->idReg3(reg2);
    id->idReg4(reg3);

    id->idCodeSize(sz);
    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction with a register + static member operands.
 */
void emitter::emitIns_R_C(instruction ins, emitAttr attr, regNumber reg, CORINFO_FIELD_HANDLE fldHnd, int offs)
{
    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    emitAttr size = EA_SIZE(attr);

    assert(size <= EA_32BYTE);
    noway_assert(emitVerifyEncodable(ins, size, reg));

    UNATIVE_OFFSET sz;
    instrDesc*     id;

    // Are we MOV'ing the offset of the class variable into EAX?
    if (EA_IS_OFFSET(attr))
    {
        id = emitNewInstrDsp(EA_1BYTE, offs);
        id->idIns(ins);
        id->idInsFmt(IF_RWR_MRD_OFF);

        assert(ins == INS_mov && reg == REG_EAX);

        // Special case: "mov eax, [addr]" is smaller
        sz = 1 + TARGET_POINTER_SIZE;
    }
    else
    {
        insFormat fmt = emitInsModeFormat(ins, IF_RRD_MRD);

        id = emitNewInstrDsp(attr, offs);
        id->idIns(ins);
        id->idInsFmt(fmt);

#ifdef _TARGET_X86_
        // Special case: "mov eax, [addr]" is smaller.
        // This case is not enabled for amd64 as it always uses RIP relative addressing
        // and it results in smaller instruction size than encoding 64-bit addr in the
        // instruction.
        if (ins == INS_mov && reg == REG_EAX)
        {
            sz = 1 + TARGET_POINTER_SIZE;
            if (size == EA_2BYTE)
                sz += 1;
        }
        else
#endif //_TARGET_X86_
        {
            sz = emitInsSizeCV(id, insCodeRM(ins));
        }

        // Special case: mov reg, fs:[ddd]
        if (fldHnd == FLD_GLOBAL_FS)
        {
            sz += 1;
        }
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));

    // REX prefix
    if (TakesRexWPrefix(ins, attr) || IsExtendedReg(reg, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idReg1(reg);
    id->idCodeSize(sz);

    id->idAddr()->iiaFieldHnd = fldHnd;

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction with a static member + register operands.
 */

void emitter::emitIns_C_R(instruction ins, emitAttr attr, CORINFO_FIELD_HANDLE fldHnd, regNumber reg, int offs)
{
    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    emitAttr size = EA_SIZE(attr);

#if defined(_TARGET_X86_)
    // For x86 it is valid to storeind a double sized operand in an xmm reg to memory
    assert(size <= EA_8BYTE);
#else
    assert(size <= EA_PTRSIZE);
#endif

    noway_assert(emitVerifyEncodable(ins, size, reg));

    instrDesc* id  = emitNewInstrDsp(attr, offs);
    insFormat  fmt = emitInsModeFormat(ins, IF_MRD_RRD);

    id->idIns(ins);
    id->idInsFmt(fmt);

    UNATIVE_OFFSET sz;

#ifdef _TARGET_X86_
    // Special case: "mov [addr], EAX" is smaller.
    // This case is not enable for amd64 as it always uses RIP relative addressing
    // and it will result in smaller instruction size than encoding 64-bit addr in
    // the instruction.
    if (ins == INS_mov && reg == REG_EAX)
    {
        sz = 1 + TARGET_POINTER_SIZE;
        if (size == EA_2BYTE)
            sz += 1;
    }
    else
#endif //_TARGET_X86_
    {
        sz = emitInsSizeCV(id, insCodeMR(ins));
    }

    // Special case: mov reg, fs:[ddd]
    if (fldHnd == FLD_GLOBAL_FS)
    {
        sz += 1;
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMR(ins));

    // REX prefix
    if (TakesRexWPrefix(ins, attr) || IsExtendedReg(reg, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idReg1(reg);
    id->idCodeSize(sz);

    id->idAddr()->iiaFieldHnd = fldHnd;

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add an instruction with a static member + constant.
 */

void emitter::emitIns_C_I(instruction ins, emitAttr attr, CORINFO_FIELD_HANDLE fldHnd, int offs, int val)
{
    // Static always need relocs
    if (!jitStaticFldIsGlobAddr(fldHnd))
    {
        attr = EA_SET_FLG(attr, EA_DSP_RELOC_FLG);
    }

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_MRW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_MRD_CNS);
            break;
    }

    instrDesc* id = emitNewInstrCnsDsp(attr, val, offs);
    id->idIns(ins);
    id->idInsFmt(fmt);

    code_t         code = insCodeMI(ins);
    UNATIVE_OFFSET sz   = emitInsSizeCV(id, code, val);

    // Vex prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMI(ins));

    // REX prefix, if not already included in "code"
    if (TakesRexWPrefix(ins, attr) && !hasRexPrefix(code))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idAddr()->iiaFieldHnd = fldHnd;
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_J_S(instruction ins, emitAttr attr, BasicBlock* dst, int varx, int offs)
{
    assert(ins == INS_mov);
    assert(dst->bbFlags & BBF_JMP_TARGET);

    instrDescLbl* id = emitNewInstrLbl();

    id->idIns(ins);
    id->idInsFmt(IF_SWR_LABEL);
    id->idAddr()->iiaBBlabel = dst;

    /* The label reference is always long */

    id->idjShort    = 0;
    id->idjKeepLong = 1;

    /* Record the current IG and offset within it */

    id->idjIG   = emitCurIG;
    id->idjOffs = emitCurIGsize;

    /* Append this instruction to this IG's jump list */

    id->idjNext      = emitCurIGjmpList;
    emitCurIGjmpList = id;

    UNATIVE_OFFSET sz = sizeof(INT32) + emitInsSizeSV(insCodeMI(ins), varx, offs);
    id->dstLclVar.initLclVarAddr(varx, offs);
#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif

#if EMITTER_STATS
    emitTotalIGjmps++;
#endif

#ifndef _TARGET_AMD64_
    // Storing the address of a basicBlock will need a reloc
    // as the instruction uses the absolute address,
    // not a relative address.
    //
    // On Amd64, Absolute code addresses should always go through a reloc to
    // to be encoded as RIP rel32 offset.
    if (emitComp->opts.compReloc)
#endif
    {
        id->idSetIsDspReloc();
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Add a label instruction.
 */
void emitter::emitIns_R_L(instruction ins, emitAttr attr, BasicBlock* dst, regNumber reg)
{
    assert(ins == INS_lea);
    assert(dst->bbFlags & BBF_JMP_TARGET);

    instrDescJmp* id = emitNewInstrJmp();

    id->idIns(ins);
    id->idReg1(reg);
    id->idInsFmt(IF_RWR_LABEL);
    id->idOpSize(EA_SIZE(attr)); // emitNewInstrJmp() sets the size (incorrectly) to EA_1BYTE
    id->idAddr()->iiaBBlabel = dst;

    /* The label reference is always long */

    id->idjShort    = 0;
    id->idjKeepLong = 1;

    /* Record the current IG and offset within it */

    id->idjIG   = emitCurIG;
    id->idjOffs = emitCurIGsize;

    /* Append this instruction to this IG's jump list */

    id->idjNext      = emitCurIGjmpList;
    emitCurIGjmpList = id;

#ifdef DEBUG
    // Mark the catch return
    if (emitComp->compCurBB->bbJumpKind == BBJ_EHCATCHRET)
    {
        id->idDebugOnlyInfo()->idCatchRet = true;
    }
#endif // DEBUG

#if EMITTER_STATS
    emitTotalIGjmps++;
#endif

    // Set the relocation flags - these give hint to zap to perform
    // relocation of the specified 32bit address.
    //
    // Note the relocation flags influence the size estimate.
    id->idSetRelocFlags(attr);

    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  The following adds instructions referencing address modes.
 */

void emitter::emitIns_I_AR(instruction ins, emitAttr attr, int val, regNumber reg, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_ARW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
            break;
    }

    /*
    Useful if you want to trap moves with 0 constant
    if (ins == INS_mov && val == 0 && EA_SIZE(attr) >= EA_4BYTE)
    {
        printf("MOV 0\n");
    }
    */

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmdCns(attr, disp, val);
    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = reg;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMI(ins), val);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_I_AI(instruction ins, emitAttr attr, int val, ssize_t disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_ARW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
            break;
    }

    /*
    Useful if you want to trap moves with 0 constant
    if (ins == INS_mov && val == 0 && EA_SIZE(attr) >= EA_4BYTE)
    {
        printf("MOV 0\n");
    }
    */

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmdCns(attr, disp, val);
    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMI(ins), val);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_AR(instruction ins, emitAttr attr, regNumber ireg, regNumber base, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_32BYTE) && (ireg != REG_NA));
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

    if (ins == INS_lea)
    {
        if (ireg == base && disp == 0)
        {
            // Maybe the emitter is not the common place for this optimization, but it's a better choke point
            // for all the emitIns(ins, tree), we would have to be analyzing at each call site
            //
            return;
        }
    }

    UNATIVE_OFFSET sz;
    instrDesc*     id  = emitNewInstrAmd(attr, disp);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_ARD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);

    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeRM(ins));

    if (Is4ByteSSE4Instruction(ins))
    {
        // The 4-Byte SSE4 instructions require two additional bytes
        sz += 2;
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_AI(instruction ins, emitAttr attr, regNumber ireg, ssize_t disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE) && (ireg != REG_NA));
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

    UNATIVE_OFFSET sz;
    instrDesc*     id  = emitNewInstrAmd(attr, disp);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_ARD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_AR_R(instruction ins, emitAttr attr, regNumber ireg, regNumber base, int disp)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmd(attr, disp);
    insFormat      fmt;

    if (ireg == REG_NA)
    {
        fmt = emitInsModeFormat(ins, IF_ARD);
    }
    else
    {
        fmt = emitInsModeFormat(ins, IF_ARD_RRD);

        assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_32BYTE));
        noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

        id->idReg1(ireg);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

void emitter::emitIns_AR_R_I(instruction ins, emitAttr attr, regNumber base, int disp, regNumber ireg, int ival)
{
    assert(ins == INS_vextracti128 || ins == INS_vextractf128);
    assert(base != REG_NA);
    assert(ireg != REG_NA);
    instrDesc* id = emitNewInstrAmdCns(attr, disp, ival);

    id->idIns(ins);
    id->idInsFmt(IF_AWR_RRD_CNS);
    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;
    id->idReg1(ireg);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    // Plus one for the 1-byte immediate (ival)
    UNATIVE_OFFSET sz = emitInsSizeAM(id, insCodeMR(ins)) + emitGetVexPrefixAdjustedSize(ins, attr, insCodeMR(ins)) + 1;
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_AI_R(instruction ins, emitAttr attr, regNumber ireg, ssize_t disp)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmd(attr, disp);
    insFormat      fmt;

    if (ireg == REG_NA)
    {
        fmt = emitInsModeFormat(ins, IF_ARD);
    }
    else
    {
        fmt = emitInsModeFormat(ins, IF_ARD_RRD);

        assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));
        noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

        id->idReg1(ireg);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = REG_NA;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

void emitter::emitIns_I_ARR(instruction ins, emitAttr attr, int val, regNumber reg, regNumber rg2, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_ARW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
            break;
    }

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmdCns(attr, disp, val);
    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = reg;
    id->idAddr()->iiaAddrMode.amIndxReg = rg2;
    id->idAddr()->iiaAddrMode.amScale   = emitter::OPSZ1;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMI(ins), val);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_ARR(instruction ins, emitAttr attr, regNumber ireg, regNumber base, regNumber index, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE) && (ireg != REG_NA));
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

    UNATIVE_OFFSET sz;
    instrDesc*     id  = emitNewInstrAmd(attr, disp);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_ARD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);

    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = index;
    id->idAddr()->iiaAddrMode.amScale   = emitter::OPSZ1;

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_ARR_R(instruction ins, emitAttr attr, regNumber ireg, regNumber reg, regNumber index, int disp)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmd(attr, disp);
    insFormat      fmt;

    if (ireg == REG_NA)
    {
        fmt = emitInsModeFormat(ins, IF_ARD);
    }
    else
    {
        fmt = emitInsModeFormat(ins, IF_ARD_RRD);

        assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));
        noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

        id->idReg1(ireg);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = reg;
    id->idAddr()->iiaAddrMode.amIndxReg = index;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(1);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

void emitter::emitIns_I_ARX(
    instruction ins, emitAttr attr, int val, regNumber reg, regNumber rg2, unsigned mul, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_ARW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
            break;
    }

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmdCns(attr, disp, val);

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = reg;
    id->idAddr()->iiaAddrMode.amIndxReg = rg2;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMI(ins), val);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_ARX(
    instruction ins, emitAttr attr, regNumber ireg, regNumber base, regNumber index, unsigned mul, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE) && (ireg != REG_NA));
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

    UNATIVE_OFFSET sz;
    instrDesc*     id  = emitNewInstrAmd(attr, disp);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_ARD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);

    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = index;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_ARX_R(
    instruction ins, emitAttr attr, regNumber ireg, regNumber base, regNumber index, unsigned mul, int disp)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmd(attr, disp);
    insFormat      fmt;

    if (ireg == REG_NA)
    {
        fmt = emitInsModeFormat(ins, IF_ARD);
    }
    else
    {
        fmt = emitInsModeFormat(ins, IF_ARD_RRD);

        noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));
        assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

        id->idReg1(ireg);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = base;
    id->idAddr()->iiaAddrMode.amIndxReg = index;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

void emitter::emitIns_I_AX(instruction ins, emitAttr attr, int val, regNumber reg, unsigned mul, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_ARW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_ARD_CNS);
            break;
    }

    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmdCns(attr, disp, val);
    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = reg;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMI(ins), val);
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_AX(instruction ins, emitAttr attr, regNumber ireg, regNumber reg, unsigned mul, int disp)
{
    assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE) && (ireg != REG_NA));
    noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));

    UNATIVE_OFFSET sz;
    instrDesc*     id  = emitNewInstrAmd(attr, disp);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_ARD);

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = reg;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeRM(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_AX_R(instruction ins, emitAttr attr, regNumber ireg, regNumber reg, unsigned mul, int disp)
{
    UNATIVE_OFFSET sz;
    instrDesc*     id = emitNewInstrAmd(attr, disp);
    insFormat      fmt;

    if (ireg == REG_NA)
    {
        fmt = emitInsModeFormat(ins, IF_ARD);
    }
    else
    {
        fmt = emitInsModeFormat(ins, IF_ARD_RRD);
        noway_assert(emitVerifyEncodable(ins, EA_SIZE(attr), ireg));
        assert((CodeGen::instIsFP(ins) == false) && (EA_SIZE(attr) <= EA_8BYTE));

        id->idReg1(ireg);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);

    id->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
    id->idAddr()->iiaAddrMode.amIndxReg = reg;
    id->idAddr()->iiaAddrMode.amScale   = emitEncodeScale(mul);

    assert(emitGetInsAmdAny(id) == disp); // make sure "disp" is stored properly

    sz = emitInsSizeAM(id, insCodeMR(ins));
    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

#ifdef FEATURE_HW_INTRINSICS
void emitter::emitIns_SIMD_R_R_A(instruction ins, emitAttr attr, regNumber reg, regNumber reg1, GenTreeIndir* indir)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_A(ins, attr, reg, reg1, indir, IF_RWR_RRD_ARD);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_A(ins, attr, reg, indir, IF_RRW_ARD);
    }
}

void emitter::emitIns_SIMD_R_R_AR(instruction ins, emitAttr attr, regNumber reg, regNumber reg1, regNumber base)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_AR(ins, attr, reg, reg1, base, 0);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_AR(ins, attr, reg, base, 0);
    }
}

void emitter::emitIns_SIMD_R_R_C(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, CORINFO_FIELD_HANDLE fldHnd, int offs)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_C(ins, attr, reg, reg1, fldHnd, offs);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_C(ins, attr, reg, fldHnd, offs);
    }
}

void emitter::emitIns_SIMD_R_R_R(instruction ins, emitAttr attr, regNumber reg, regNumber reg1, regNumber reg2)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_R(ins, attr, reg, reg1, reg2);
    }
    else
    {
        if (reg1 != reg)
        {
            // Ensure we aren't overwriting op2
            assert(reg2 != reg);

            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_R(ins, attr, reg, reg2);
    }
}

static bool isSseShift(instruction ins)
{
    switch (ins)
    {
        case INS_psrldq:
        case INS_pslldq:
        case INS_psrld:
        case INS_psrlw:
        case INS_psrlq:
        case INS_pslld:
        case INS_psllw:
        case INS_psllq:
        case INS_psrad:
        case INS_psraw:
            return true;
        default:
            return false;
    }
}

//------------------------------------------------------------------------
// IsDstSrcImmAvxInstruction: check if instruction has "R(M) R(M) I" format
// for EVEX, VEX and legacy SSE encodings and has no (E)VEX.NDS
//
// Arguments:
//    instruction -- processor instruction to check
//
// Return Value:
//    true if instruction has "R(M) R(M) I" format and has no (E)VEX.NDS
//
static bool IsDstSrcImmAvxInstruction(instruction ins)
{
    switch (ins)
    {
        case INS_extractps:
        case INS_pextrb:
        case INS_pextrw:
        case INS_pextrd:
        case INS_pextrq:
        case INS_pshufd:
        case INS_pshufhw:
        case INS_pshuflw:
            return true;
        default:
            return false;
    }
}

void emitter::emitIns_SIMD_R_R_I(instruction ins, emitAttr attr, regNumber reg, regNumber reg1, int ival)
{
    // TODO-XARCH refactoring emitIns_R_R_I to handle SSE2/AVX2 shift as well as emitIns_R_I
    bool isShift = isSseShift(ins);
    if (IsDstSrcImmAvxInstruction(ins) || (UseVEXEncoding() && !isShift))
    {
        emitIns_R_R_I(ins, attr, reg, reg1, ival);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        // TODO-XARCH-BUG emitOutputRI cannot work with SSE2 shift instruction on imm8 > 127, so we replace it by the
        // semantic alternatives. https://github.com/dotnet/coreclr/issues/16543
        if (isShift && ival > 127)
        {
            ival = 127;
        }
        emitIns_R_I(ins, attr, reg, ival);
    }
}

void emitter::emitIns_SIMD_R_R_R_R(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, regNumber reg2, regNumber reg3)
{
    assert(isAvxBlendv(ins) || isSse41Blendv(ins));
    if (UseVEXEncoding())
    {
        // convert SSE encoding of SSE4.1 instructions to VEX encoding
        switch (ins)
        {
            case INS_blendvps:
                ins = INS_vblendvps;
                break;
            case INS_blendvpd:
                ins = INS_vblendvpd;
                break;
            case INS_pblendvb:
                ins = INS_vpblendvb;
                break;
            default:
                break;
        }
        emitIns_R_R_R_R(ins, attr, reg, reg1, reg2, reg3);
    }
    else
    {
        assert(isSse41Blendv(ins));
        // SSE4.1 blendv* hardcode the mask vector (op3) in XMM0
        if (reg3 != REG_XMM0)
        {
            // Ensure we aren't overwriting op1 or op2
            assert(reg1 != REG_XMM0);
            assert(reg2 != REG_XMM0);

            emitIns_R_R(INS_movaps, attr, REG_XMM0, reg3);
        }
        if (reg1 != reg)
        {
            // Ensure we aren't overwriting op2 or op3
            assert(reg2 != reg);
            assert((reg3 == REG_XMM0) || (reg != REG_XMM0));

            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_R(ins, attr, reg, reg2);
    }
}

void emitter::emitIns_SIMD_R_R_S(instruction ins, emitAttr attr, regNumber reg, regNumber reg1, int varx, int offs)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_S(ins, attr, reg, reg1, varx, offs);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_S(ins, attr, reg, varx, offs);
    }
}

void emitter::emitIns_SIMD_R_R_A_I(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, GenTreeIndir* indir, int ival)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_A_I(ins, attr, reg, reg1, indir, ival, IF_RWR_RRD_ARD_CNS);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_A_I(ins, attr, reg, indir, ival);
    }
}

void emitter::emitIns_SIMD_R_R_AR_I(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, regNumber base, int ival)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_AR_I(ins, attr, reg, reg1, base, 0, ival);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_AR_I(ins, attr, reg, base, 0, ival);
    }
}

void emitter::emitIns_SIMD_R_R_C_I(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, CORINFO_FIELD_HANDLE fldHnd, int offs, int ival)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_C_I(ins, attr, reg, reg1, fldHnd, offs, ival);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_C_I(ins, attr, reg, fldHnd, offs, ival);
    }
}

void emitter::emitIns_SIMD_R_R_R_I(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, regNumber reg2, int ival)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_R_I(ins, attr, reg, reg1, reg2, ival);
    }
    else
    {
        if (reg1 != reg)
        {
            // Ensure we aren't overwriting op2
            assert(reg2 != reg);

            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_R_I(ins, attr, reg, reg2, ival);
    }
}

void emitter::emitIns_SIMD_R_R_S_I(
    instruction ins, emitAttr attr, regNumber reg, regNumber reg1, int varx, int offs, int ival)
{
    if (UseVEXEncoding())
    {
        emitIns_R_R_S_I(ins, attr, reg, reg1, varx, offs, ival);
    }
    else
    {
        if (reg1 != reg)
        {
            emitIns_R_R(INS_movaps, attr, reg, reg1);
        }
        emitIns_R_S_I(ins, attr, reg, varx, offs, ival);
    }
}
#endif // FEATURE_HW_INTRINSICS

/*****************************************************************************
 *
 *  The following add instructions referencing stack-based local variables.
 */

void emitter::emitIns_S(instruction ins, emitAttr attr, int varx, int offs)
{
    instrDesc*     id  = emitNewInstr(attr);
    UNATIVE_OFFSET sz  = emitInsSizeSV(insCodeMR(ins), varx, offs);
    insFormat      fmt = emitInsModeFormat(ins, IF_SRD);

    // 16-bit operand instructions will need a prefix
    if (EA_SIZE(attr) == EA_2BYTE)
    {
        sz += 1;
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMR(ins));

    // 64-bit operand instructions will need a REX.W prefix
    if (TakesRexWPrefix(ins, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);
    id->idCodeSize(sz);

#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif
    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

void emitter::emitIns_S_R(instruction ins, emitAttr attr, regNumber ireg, int varx, int offs)
{
    instrDesc*     id  = emitNewInstr(attr);
    UNATIVE_OFFSET sz  = emitInsSizeSV(insCodeMR(ins), varx, offs);
    insFormat      fmt = emitInsModeFormat(ins, IF_SRD_RRD);

#ifdef _TARGET_X86_
    if (attr == EA_1BYTE)
    {
        assert(isByteReg(ireg));
    }
#endif
    // 16-bit operand instructions will need a prefix
    if (EA_SIZE(attr) == EA_2BYTE)
    {
        sz++;
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMR(ins));

    // 64-bit operand instructions will need a REX.W prefix
    if (TakesRexWPrefix(ins, attr) || IsExtendedReg(ireg, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);
    id->idCodeSize(sz);
#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_R_S(instruction ins, emitAttr attr, regNumber ireg, int varx, int offs)
{
    emitAttr size = EA_SIZE(attr);
    noway_assert(emitVerifyEncodable(ins, size, ireg));

    instrDesc*     id  = emitNewInstr(attr);
    UNATIVE_OFFSET sz  = emitInsSizeSV(insCodeRM(ins), varx, offs);
    insFormat      fmt = emitInsModeFormat(ins, IF_RRD_SRD);

    // Most 16-bit operand instructions need a prefix
    if (size == EA_2BYTE && ins != INS_movsx && ins != INS_movzx)
    {
        sz++;
    }

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeRM(ins));

    // 64-bit operand instructions will need a REX.W prefix
    if (TakesRexWPrefix(ins, attr) || IsExtendedReg(ireg, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idIns(ins);
    id->idInsFmt(fmt);
    id->idReg1(ireg);
    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);
    id->idCodeSize(sz);
#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif
    dispIns(id);
    emitCurIGsize += sz;
}

void emitter::emitIns_S_I(instruction ins, emitAttr attr, int varx, int offs, int val)
{
#ifdef _TARGET_AMD64_
    // mov reg, imm64 is the only opcode which takes a full 8 byte immediate
    // all other opcodes take a sign-extended 4-byte immediate
    noway_assert(EA_SIZE(attr) < EA_8BYTE || !EA_IS_CNS_RELOC(attr));
#endif

    insFormat fmt;

    switch (ins)
    {
        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            assert(val != 1);
            fmt = IF_SRW_SHF;
            val &= 0x7F;
            break;

        default:
            fmt = emitInsModeFormat(ins, IF_SRD_CNS);
            break;
    }

    instrDesc* id = emitNewInstrCns(attr, val);
    id->idIns(ins);
    id->idInsFmt(fmt);
    UNATIVE_OFFSET sz = emitInsSizeSV(id, varx, offs, val);

    // VEX prefix
    sz += emitGetVexPrefixAdjustedSize(ins, attr, insCodeMI(ins));

    // 64-bit operand instructions will need a REX.W prefix
    if (TakesRexWPrefix(ins, attr))
    {
        sz += emitGetRexPrefixSize(ins);
    }

    id->idAddr()->iiaLclVar.initLclVarAddr(varx, offs);
    id->idCodeSize(sz);
#ifdef DEBUG
    id->idDebugOnlyInfo()->idVarRefOffs = emitVarRefOffs;
#endif
    dispIns(id);
    emitCurIGsize += sz;
}

/*****************************************************************************
 *
 *  Record that a jump instruction uses the short encoding
 *
 */
void emitter::emitSetShortJump(instrDescJmp* id)
{
    if (id->idjKeepLong)
    {
        return;
    }

    id->idjShort = true;
}

/*****************************************************************************
 *
 *  Add a jmp instruction.
 */

void emitter::emitIns_J(instruction ins, BasicBlock* dst, int instrCount /* = 0 */)
{
    UNATIVE_OFFSET sz;
    instrDescJmp*  id = emitNewInstrJmp();

    assert(dst->bbFlags & BBF_JMP_TARGET);

    id->idIns(ins);
    id->idInsFmt(IF_LABEL);
    id->idAddr()->iiaBBlabel = dst;

#ifdef DEBUG
    // Mark the finally call
    if (ins == INS_call && emitComp->compCurBB->bbJumpKind == BBJ_CALLFINALLY)
    {
        id->idDebugOnlyInfo()->idFinallyCall = true;
    }
#endif // DEBUG

    /* Assume the jump will be long */

    id->idjShort    = 0;
    id->idjKeepLong = emitComp->fgInDifferentRegions(emitComp->compCurBB, dst);

    /* Record the jump's IG and offset within it */

    id->idjIG   = emitCurIG;
    id->idjOffs = emitCurIGsize;

    /* Append this jump to this IG's jump list */

    id->idjNext      = emitCurIGjmpList;
    emitCurIGjmpList = id;

#if EMITTER_STATS
    emitTotalIGjmps++;
#endif

    /* Figure out the max. size of the jump/call instruction */

    if (ins == INS_call)
    {
        sz = CALL_INST_SIZE;
    }
    else if (ins == INS_push || ins == INS_push_hide)
    {
        // Pushing the address of a basicBlock will need a reloc
        // as the instruction uses the absolute address,
        // not a relative address
        if (emitComp->opts.compReloc)
        {
            id->idSetIsDspReloc();
        }
        sz = PUSH_INST_SIZE;
    }
    else
    {
        insGroup* tgt;

        /* This is a jump - assume the worst */

        sz = (ins == INS_jmp) ? JMP_SIZE_LARGE : JCC_SIZE_LARGE;

        /* Can we guess at the jump distance? */

        tgt = (insGroup*)emitCodeGetCookie(dst);

        if (tgt)
        {
            int            extra;
            UNATIVE_OFFSET srcOffs;
            int            jmpDist;

            assert(JMP_SIZE_SMALL == JCC_SIZE_SMALL);

            /* This is a backward jump - figure out the distance */

            srcOffs = emitCurCodeOffset + emitCurIGsize + JMP_SIZE_SMALL;

            /* Compute the distance estimate */

            jmpDist = srcOffs - tgt->igOffs;
            assert((int)jmpDist > 0);

            /* How much beyond the max. short distance does the jump go? */

            extra = jmpDist + JMP_DIST_SMALL_MAX_NEG;

#if DEBUG_EMIT
            if (id->idDebugOnlyInfo()->idNum == (unsigned)INTERESTING_JUMP_NUM || INTERESTING_JUMP_NUM == 0)
            {
                if (INTERESTING_JUMP_NUM == 0)
                {
                    printf("[0] Jump %u:\n", id->idDebugOnlyInfo()->idNum);
                }
                printf("[0] Jump source is at %08X\n", srcOffs);
                printf("[0] Label block is at %08X\n", tgt->igOffs);
                printf("[0] Jump  distance  - %04X\n", jmpDist);
                if (extra > 0)
                {
                    printf("[0] Distance excess = %d  \n", extra);
                }
            }
#endif

            if (extra <= 0 && !id->idjKeepLong)
            {
                /* Wonderful - this jump surely will be short */

                emitSetShortJump(id);
                sz = JMP_SIZE_SMALL;
            }
        }
#if DEBUG_EMIT
        else
        {
            if (id->idDebugOnlyInfo()->idNum == (unsigned)INTERESTING_JUMP_NUM || INTERESTING_JUMP_NUM == 0)
            {
                if (INTERESTING_JUMP_NUM == 0)
                {
                    printf("[0] Jump %u:\n", id->idDebugOnlyInfo()->idNum);
                }
                printf("[0] Jump source is at %04X/%08X\n", emitCurIGsize,
                       emitCurCodeOffset + emitCurIGsize + JMP_SIZE_SMALL);
                printf("[0] Label block is unknown\n");
            }
        }
#endif
    }

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

    emitAdjustStackDepthPushPop(ins);
}

#if !FEATURE_FIXED_OUT_ARGS

//------------------------------------------------------------------------
// emitAdjustStackDepthPushPop: Adjust the current and maximum stack depth.
//
// Arguments:
//    ins - the instruction. Only INS_push and INS_pop adjust the stack depth.
//
// Notes:
//    1. Alters emitCurStackLvl and possibly emitMaxStackDepth.
//    2. emitCntStackDepth must be set (0 in prolog/epilog, one DWORD elsewhere)
//
void emitter::emitAdjustStackDepthPushPop(instruction ins)
{
    if (ins == INS_push)
    {
        emitCurStackLvl += emitCntStackDepth;

        if (emitMaxStackDepth < emitCurStackLvl)
        {
            JITDUMP("Upping emitMaxStackDepth from %d to %d\n", emitMaxStackDepth, emitCurStackLvl);
            emitMaxStackDepth = emitCurStackLvl;
        }
    }
    else if (ins == INS_pop)
    {
        emitCurStackLvl -= emitCntStackDepth;
        assert((int)emitCurStackLvl >= 0);
    }
}

//------------------------------------------------------------------------
// emitAdjustStackDepth: Adjust the current and maximum stack depth.
//
// Arguments:
//    ins - the instruction. Only INS_add and INS_sub adjust the stack depth.
//          It is assumed that the add/sub is on the stack pointer.
//    val - the number of bytes to add to or subtract from the stack pointer.
//
// Notes:
//    1. Alters emitCurStackLvl and possibly emitMaxStackDepth.
//    2. emitCntStackDepth must be set (0 in prolog/epilog, one DWORD elsewhere)
//
void emitter::emitAdjustStackDepth(instruction ins, ssize_t val)
{
    // If we're in the prolog or epilog, or otherwise not tracking the stack depth, just return.
    if (emitCntStackDepth == 0)
        return;

    if (ins == INS_sub)
    {
        S_UINT32 newStackLvl(emitCurStackLvl);
        newStackLvl += S_UINT32(val);
        noway_assert(!newStackLvl.IsOverflow());

        emitCurStackLvl = newStackLvl.Value();

        if (emitMaxStackDepth < emitCurStackLvl)
        {
            JITDUMP("Upping emitMaxStackDepth from %d to %d\n", emitMaxStackDepth, emitCurStackLvl);
            emitMaxStackDepth = emitCurStackLvl;
        }
    }
    else if (ins == INS_add)
    {
        S_UINT32 newStackLvl = S_UINT32(emitCurStackLvl) - S_UINT32(val);
        noway_assert(!newStackLvl.IsOverflow());

        emitCurStackLvl = newStackLvl.Value();
    }
}

#endif // EMIT_TRACK_STACK_DEPTH

/*****************************************************************************
 *
 *  Add a call instruction (direct or indirect).
 *      argSize<0 means that the caller will pop the arguments
 *
 * The other arguments are interpreted depending on callType as shown:
 * Unless otherwise specified, ireg,xreg,xmul,disp should have default values.
 *
 * EC_FUNC_TOKEN       : addr is the method address
 * EC_FUNC_TOKEN_INDIR : addr is the indirect method address
 * EC_FUNC_ADDR        : addr is the absolute address of the function
 * EC_FUNC_VIRTUAL     : "call [ireg+disp]"
 *
 * If callType is one of these emitCallTypes, addr has to be NULL.
 * EC_INDIR_R          : "call ireg".
 * EC_INDIR_SR         : "call lcl<disp>" (eg. call [ebp-8]).
 * EC_INDIR_C          : "call clsVar<disp>" (eg. call [clsVarAddr])
 * EC_INDIR_ARD        : "call [ireg+xreg*xmul+disp]"
 *
 */

// clang-format off
void emitter::emitIns_Call(EmitCallType          callType,
                           CORINFO_METHOD_HANDLE methHnd,
                           INDEBUG_LDISASM_COMMA(CORINFO_SIG_INFO* sigInfo) // used to report call sites to the EE
                           void*                 addr,
                           ssize_t               argSize,
                           emitAttr              retSize
                           MULTIREG_HAS_SECOND_GC_RET_ONLY_ARG(emitAttr secondRetSize),
                           VARSET_VALARG_TP      ptrVars,
                           regMaskTP             gcrefRegs,
                           regMaskTP             byrefRegs,
                           IL_OFFSETX            ilOffset, // = BAD_IL_OFFSET
                           regNumber             ireg,     // = REG_NA
                           regNumber             xreg,     // = REG_NA
                           unsigned              xmul,     // = 0
                           ssize_t               disp,     // = 0
                           bool                  isJump,   // = false
                           bool                  isNoGC)   // = false
// clang-format on
{
    /* Sanity check the arguments depending on callType */

    assert(callType < EC_COUNT);
    assert((callType != EC_FUNC_TOKEN && callType != EC_FUNC_TOKEN_INDIR && callType != EC_FUNC_ADDR) ||
           (ireg == REG_NA && xreg == REG_NA && xmul == 0 && disp == 0));
    assert(callType != EC_FUNC_VIRTUAL || (ireg < REG_COUNT && xreg == REG_NA && xmul == 0));
    assert(callType < EC_INDIR_R || callType == EC_INDIR_ARD || callType == EC_INDIR_C || addr == nullptr);
    assert(callType != EC_INDIR_R || (ireg < REG_COUNT && xreg == REG_NA && xmul == 0 && disp == 0));
    assert(callType != EC_INDIR_SR ||
           (ireg == REG_NA && xreg == REG_NA && xmul == 0 && disp < (int)emitComp->lvaCount));
    assert(callType != EC_INDIR_C || (ireg == REG_NA && xreg == REG_NA && xmul == 0 && disp != 0));

    // Our stack level should be always greater than the bytes of arguments we push. Just
    // a sanity test.
    assert((unsigned)abs((signed)argSize) <= codeGen->genStackLevel);

#if STACK_PROBES
    if (emitComp->opts.compNeedStackProbes)
    {
        // If we've pushed more than JIT_RESERVED_STACK allows, do an aditional stack probe
        // Else, just make sure the prolog does a probe for us. Invariant we're trying
        // to get is that at any point we go out to unmanaged code, there is at least
        // CORINFO_STACKPROBE_DEPTH bytes of stack available.
        //
        // The reason why we are not doing one probe for the max size at the prolog
        // is that when don't have the max depth precomputed (it can depend on codegen),
        // and we need it at the time we generate locallocs
        //
        // Compiler::lvaAssignFrameOffsets sets up compLclFrameSize, which takes in
        // account everything except for the arguments of a callee.
        //
        //
        //
        if ((TARGET_POINTER_SIZE + // return address for call
             emitComp->genStackLevel +
             // Current stack level. This gets resetted on every
             // localloc and on the prolog (invariant is that
             // genStackLevel is 0 on basic block entry and exit and
             // after any alloca). genStackLevel will include any arguments
             // to the call, so we will insert an aditional probe if
             // we've consumed more than JIT_RESERVED_STACK bytes
             // of stack, which is what the prolog probe covers (in
             // addition to the EE requested size)
             (emitComp->compHndBBtabCount * TARGET_POINTER_SIZE)
             // Hidden slots for calling finallys
             ) >= JIT_RESERVED_STACK)
        {
            // This happens when you have a call with a lot of arguments or a call is done
            // when there's a lot of stuff pushed on the stack (for example a call whos returned
            // value is an argument of another call that has pushed stuff on the stack)
            // This should't be very frequent.
            // For different values of JIT_RESERVED_STACK
            //
            // For mscorlib (109605 calls)
            //
            // 14190 probes in prologs (56760 bytes of code)
            //
            // JIT_RESERVED_STACK = 16 : 5452 extra probes
            // JIT_RESERVED_STACK = 32 : 1084 extra probes
            // JIT_RESERVED_STACK = 64 :    1 extra probes
            // JIT_RESERVED_STACK = 96 :    0 extra probes
            emitComp->genGenerateStackProbe();
        }
        else
        {
            if (emitComp->compGeneratingProlog || emitComp->compGeneratingEpilog)
            {
                if (emitComp->compStackProbePrologDone)
                {
                    // We already generated a probe and this call is not happening
                    // at a depth >= JIT_RESERVED_STACK, so nothing to do here
                }
                else
                {
                    // 3 possible ways to get here:
                    // - We are in an epilog and haven't generated a probe in the prolog.
                    //   This shouldn't happen as we don't generate any calls in epilog.
                    // - We are in the prolog, but doing a call before generating the probe.
                    //   This shouldn't happen at all.
                    // - We are in the prolog, did not generate a probe but now we need
                    //   to generate a probe because we need a call (eg: profiler). We'll
                    //   need a probe.
                    //
                    // In any case, we need a probe

                    // Ignore the profiler callback for now.
                    if (!emitComp->compIsProfilerHookNeeded())
                    {
                        assert(!"We do not expect to get here");
                        emitComp->genGenerateStackProbe();
                    }
                }
            }
            else
            {
                // We will need a probe and will generate it in the prolog
                emitComp->genNeedPrologStackProbe = true;
            }
        }
    }
#endif // STACK_PROBES

    int argCnt;

    UNATIVE_OFFSET sz;
    instrDesc*     id;

    /* This is the saved set of registers after a normal call */
    unsigned savedSet = RBM_CALLEE_SAVED;

    /* some special helper calls have a different saved set registers */

    if (isNoGC)
    {
        // Get the set of registers that this call kills and remove it from the saved set.
        savedSet = RBM_ALLINT & ~emitComp->compNoGCHelperCallKillSet(Compiler::eeGetHelperNum(methHnd));
    }
    else
    {
        assert(!emitNoGChelper(Compiler::eeGetHelperNum(methHnd)));
    }

    /* Trim out any callee-trashed registers from the live set */

    gcrefRegs &= savedSet;
    byrefRegs &= savedSet;

#ifdef DEBUG
    if (EMIT_GC_VERBOSE)
    {
        printf("\t\t\t\t\t\t\tCall: GCvars=%s ", VarSetOps::ToString(emitComp, ptrVars));
        dumpConvertedVarSet(emitComp, ptrVars);
        printf(", gcrefRegs=");
        printRegMaskInt(gcrefRegs);
        emitDispRegSet(gcrefRegs);
        printf(", byrefRegs=");
        printRegMaskInt(byrefRegs);
        emitDispRegSet(byrefRegs);
        printf("\n");
    }
#endif

    assert(argSize % REGSIZE_BYTES == 0);
    argCnt = (int)(argSize / (int)REGSIZE_BYTES); // we need a signed-divide

    /* Managed RetVal: emit sequence point for the call */
    if (emitComp->opts.compDbgInfo && ilOffset != BAD_IL_OFFSET)
    {
        codeGen->genIPmappingAdd(ilOffset, false);
    }

    /*
        We need to allocate the appropriate instruction descriptor based
        on whether this is a direct/indirect call, and whether we need to
        record an updated set of live GC variables.

        The stats for a ton of classes is as follows:

            Direct call w/o  GC vars        220,216
            Indir. call w/o  GC vars        144,781

            Direct call with GC vars          9,440
            Indir. call with GC vars          5,768
     */

    if (callType >= EC_FUNC_VIRTUAL)
    {
        /* Indirect call, virtual calls */

        assert(callType == EC_FUNC_VIRTUAL || callType == EC_INDIR_R || callType == EC_INDIR_SR ||
               callType == EC_INDIR_C || callType == EC_INDIR_ARD);

        id = emitNewInstrCallInd(argCnt, disp, ptrVars, gcrefRegs, byrefRegs,
                                 retSize MULTIREG_HAS_SECOND_GC_RET_ONLY_ARG(secondRetSize));
    }
    else
    {
        // Helper/static/nonvirtual/function calls (direct or through handle),
        // and calls to an absolute addr.

        assert(callType == EC_FUNC_TOKEN || callType == EC_FUNC_TOKEN_INDIR || callType == EC_FUNC_ADDR);

        id = emitNewInstrCallDir(argCnt, ptrVars, gcrefRegs, byrefRegs,
                                 retSize MULTIREG_HAS_SECOND_GC_RET_ONLY_ARG(secondRetSize));
    }

    /* Update the emitter's live GC ref sets */

    VarSetOps::Assign(emitComp, emitThisGCrefVars, ptrVars);
    emitThisGCrefRegs = gcrefRegs;
    emitThisByrefRegs = byrefRegs;

    /* Set the instruction - special case jumping a function */
    instruction ins = INS_call;

    if (isJump)
    {
        assert(callType == EC_FUNC_TOKEN || callType == EC_FUNC_TOKEN_INDIR);
        if (callType == EC_FUNC_TOKEN)
        {
            ins = INS_l_jmp;
        }
        else
        {
            ins = INS_i_jmp;
        }
    }
    id->idIns(ins);

    id->idSetIsNoGC(isNoGC);

    // Record the address: method, indirection, or funcptr
    if (callType >= EC_FUNC_VIRTUAL)
    {
        // This is an indirect call (either a virtual call or func ptr call)

        switch (callType)
        {
            case EC_INDIR_C:
                // Indirect call using an absolute code address.
                // Must be marked as relocatable and is done at the
                // branch target location.
                goto CALL_ADDR_MODE;

            case EC_INDIR_R: // the address is in a register

                id->idSetIsCallRegPtr();

                __fallthrough;

            case EC_INDIR_ARD: // the address is an indirection

                goto CALL_ADDR_MODE;

            case EC_INDIR_SR: // the address is in a lcl var

                id->idInsFmt(IF_SRD);
                // disp is really a lclVarNum
                noway_assert((unsigned)disp == (size_t)disp);
                id->idAddr()->iiaLclVar.initLclVarAddr((unsigned)disp, 0);
                sz = emitInsSizeSV(insCodeMR(INS_call), (unsigned)disp, 0);

                break;

            case EC_FUNC_VIRTUAL:

            CALL_ADDR_MODE:

                // fall-through

                // The function is "ireg" if id->idIsCallRegPtr(),
                // else [ireg+xmul*xreg+disp]

                id->idInsFmt(IF_ARD);

                id->idAddr()->iiaAddrMode.amBaseReg = ireg;
                id->idAddr()->iiaAddrMode.amIndxReg = xreg;
                id->idAddr()->iiaAddrMode.amScale   = xmul ? emitEncodeScale(xmul) : emitter::OPSZ1;

                sz = emitInsSizeAM(id, insCodeMR(INS_call));

                if (ireg == REG_NA && xreg == REG_NA)
                {
                    if (codeGen->genCodeIndirAddrNeedsReloc(disp))
                    {
                        id->idSetIsDspReloc();
                    }
#ifdef _TARGET_AMD64_
                    else
                    {
                        // An absolute indir address that doesn't need reloc should fit within 32-bits
                        // to be encoded as offset relative to zero.  This addr mode requires an extra
                        // SIB byte
                        noway_assert(static_cast<int>(reinterpret_cast<intptr_t>(addr)) == (size_t)addr);
                        sz++;
                    }
#endif //_TARGET_AMD64_
                }

                break;

            default:
                NO_WAY("unexpected instruction");
                break;
        }
    }
    else if (callType == EC_FUNC_TOKEN_INDIR)
    {
        /* "call [method_addr]" */

        assert(addr != nullptr);

        id->idInsFmt(IF_METHPTR);
        id->idAddr()->iiaAddr = (BYTE*)addr;
        sz                    = 6;

        // Since this is an indirect call through a pointer and we don't
        // currently pass in emitAttr into this function, we query codegen
        // whether addr needs a reloc.
        if (codeGen->genCodeIndirAddrNeedsReloc((size_t)addr))
        {
            id->idSetIsDspReloc();
        }
#ifdef _TARGET_AMD64_
        else
        {
            // An absolute indir address that doesn't need reloc should fit within 32-bits
            // to be encoded as offset relative to zero.  This addr mode requires an extra
            // SIB byte
            noway_assert(static_cast<int>(reinterpret_cast<intptr_t>(addr)) == (size_t)addr);
            sz++;
        }
#endif //_TARGET_AMD64_
    }
    else
    {
        /* This is a simple direct call: "call helper/method/addr" */

        assert(callType == EC_FUNC_TOKEN || callType == EC_FUNC_ADDR);

        assert(addr != nullptr);

        id->idInsFmt(IF_METHOD);
        sz = 5;

        id->idAddr()->iiaAddr = (BYTE*)addr;

        if (callType == EC_FUNC_ADDR)
        {
            id->idSetIsCallAddr();
        }

        // Direct call to a method and no addr indirection is needed.
        if (codeGen->genCodeAddrNeedsReloc((size_t)addr))
        {
            id->idSetIsDspReloc();
        }
    }

#ifdef DEBUG
    if (emitComp->verbose && 0)
    {
        if (id->idIsLargeCall())
        {
            if (callType >= EC_FUNC_VIRTUAL)
            {
                printf("[%02u] Rec call GC vars = %s\n", id->idDebugOnlyInfo()->idNum,
                       VarSetOps::ToString(emitComp, ((instrDescCGCA*)id)->idcGCvars));
            }
            else
            {
                printf("[%02u] Rec call GC vars = %s\n", id->idDebugOnlyInfo()->idNum,
                       VarSetOps::ToString(emitComp, ((instrDescCGCA*)id)->idcGCvars));
            }
        }
    }

    id->idDebugOnlyInfo()->idMemCookie = (size_t)methHnd; // method token
    id->idDebugOnlyInfo()->idCallSig   = sigInfo;
#endif // DEBUG

#ifdef LATE_DISASM
    if (addr != nullptr)
    {
        codeGen->getDisAssembler().disSetMethod((size_t)addr, methHnd);
    }
#endif // LATE_DISASM

    id->idCodeSize(sz);

    dispIns(id);
    emitCurIGsize += sz;

#if !FEATURE_FIXED_OUT_ARGS

    /* The call will pop the arguments */

    if (emitCntStackDepth && argSize > 0)
    {
        noway_assert((ssize_t)emitCurStackLvl >= argSize);
        emitCurStackLvl -= (int)argSize;
        assert((int)emitCurStackLvl >= 0);
    }

#endif // !FEATURE_FIXED_OUT_ARGS
}

#ifdef DEBUG
/*****************************************************************************
 *
 *  The following called for each recorded instruction -- use for debugging.
 */
void emitter::emitInsSanityCheck(instrDesc* id)
{
    // make certain you only try to put relocs on things that can have them.
    ID_OPS idOp = (ID_OPS)emitFmtToOps[id->idInsFmt()];
    if ((idOp == ID_OP_SCNS) && id->idIsLargeCns())
    {
        idOp = ID_OP_CNS;
    }

    if (id->idIsDspReloc())
    {
        assert(idOp == ID_OP_NONE || idOp == ID_OP_AMD || idOp == ID_OP_DSP || idOp == ID_OP_DSP_CNS ||
               idOp == ID_OP_AMD_CNS || idOp == ID_OP_SPEC || idOp == ID_OP_CALL || idOp == ID_OP_JMP ||
               idOp == ID_OP_LBL);
    }

    if (id->idIsCnsReloc())
    {
        assert(idOp == ID_OP_CNS || idOp == ID_OP_AMD_CNS || idOp == ID_OP_DSP_CNS || idOp == ID_OP_SPEC ||
               idOp == ID_OP_CALL || idOp == ID_OP_JMP);
    }
}
#endif

/*****************************************************************************
 *
 *  Return the allocated size (in bytes) of the given instruction descriptor.
 */

size_t emitter::emitSizeOfInsDsc(instrDesc* id)
{
    if (emitIsScnsInsDsc(id))
    {
        return SMALL_IDSC_SIZE;
    }

    assert((unsigned)id->idInsFmt() < emitFmtCount);

    ID_OPS idOp = (ID_OPS)emitFmtToOps[id->idInsFmt()];

    // An INS_call instruction may use a "fat" direct/indirect call descriptor
    // except for a local call to a label (i.e. call to a finally)
    // Only ID_OP_CALL and ID_OP_SPEC check for this, so we enforce that the
    //  INS_call instruction always uses one of these idOps

    if (id->idIns() == INS_call)
    {
        assert(idOp == ID_OP_CALL || // is a direct   call
               idOp == ID_OP_SPEC || // is a indirect call
               idOp == ID_OP_JMP);   // is a local call to finally clause
    }

    switch (idOp)
    {
        case ID_OP_NONE:
            break;

        case ID_OP_LBL:
            return sizeof(instrDescLbl);

        case ID_OP_JMP:
            return sizeof(instrDescJmp);

        case ID_OP_CALL:
        case ID_OP_SPEC:
            if (id->idIsLargeCall())
            {
                /* Must be a "fat" indirect call descriptor */
                return sizeof(instrDescCGCA);
            }

            __fallthrough;

        case ID_OP_SCNS:
        case ID_OP_CNS:
        case ID_OP_DSP:
        case ID_OP_DSP_CNS:
        case ID_OP_AMD:
        case ID_OP_AMD_CNS:
            if (id->idIsLargeCns())
            {
                if (id->idIsLargeDsp())
                {
                    return sizeof(instrDescCnsDsp);
                }
                else
                {
                    return sizeof(instrDescCns);
                }
            }
            else
            {
                if (id->idIsLargeDsp())
                {
                    return sizeof(instrDescDsp);
                }
                else
                {
                    return sizeof(instrDesc);
                }
            }

        default:
            NO_WAY("unexpected instruction descriptor format");
            break;
    }

    return sizeof(instrDesc);
}

/*****************************************************************************/
#ifdef DEBUG
/*****************************************************************************
 *
 *  Return a string that represents the given register.
 */

const char* emitter::emitRegName(regNumber reg, emitAttr attr, bool varName)
{
    static char          rb[2][128];
    static unsigned char rbc = 0;

    const char* rn = emitComp->compRegVarName(reg, varName);

#ifdef _TARGET_AMD64_
    char suffix = '\0';

    switch (EA_SIZE(attr))
    {
        case EA_32BYTE:
            return emitYMMregName(reg);

        case EA_16BYTE:
            return emitXMMregName(reg);

        case EA_8BYTE:
            if ((REG_XMM0 <= reg) && (reg <= REG_XMM15))
            {
                return emitXMMregName(reg);
            }
            break;

        case EA_4BYTE:
            if ((REG_XMM0 <= reg) && (reg <= REG_XMM15))
            {
                return emitXMMregName(reg);
            }

            if (reg > REG_R15)
            {
                break;
            }

            if (reg > REG_RDI)
            {
                suffix = 'd';
                goto APPEND_SUFFIX;
            }
            rbc        = (rbc + 1) % 2;
            rb[rbc][0] = 'e';
            rb[rbc][1] = rn[1];
            rb[rbc][2] = rn[2];
            rb[rbc][3] = 0;
            rn         = rb[rbc];
            break;

        case EA_2BYTE:
            if (reg > REG_RDI)
            {
                suffix = 'w';
                goto APPEND_SUFFIX;
            }
            rn++;
            break;

        case EA_1BYTE:
            if (reg > REG_RDI)
            {
                suffix = 'b';
            APPEND_SUFFIX:
                rbc        = (rbc + 1) % 2;
                rb[rbc][0] = rn[0];
                rb[rbc][1] = rn[1];
                if (rn[2])
                {
                    assert(rn[3] == 0);
                    rb[rbc][2] = rn[2];
                    rb[rbc][3] = suffix;
                    rb[rbc][4] = 0;
                }
                else
                {
                    rb[rbc][2] = suffix;
                    rb[rbc][3] = 0;
                }
            }
            else
            {
                rbc        = (rbc + 1) % 2;
                rb[rbc][0] = rn[1];
                if (reg < 4)
                {
                    rb[rbc][1] = 'l';
                    rb[rbc][2] = 0;
                }
                else
                {
                    rb[rbc][1] = rn[2];
                    rb[rbc][2] = 'l';
                    rb[rbc][3] = 0;
                }
            }

            rn = rb[rbc];
            break;

        default:
            break;
    }
#endif // _TARGET_AMD64_

#ifdef _TARGET_X86_
    assert(strlen(rn) >= 3);

    switch (EA_SIZE(attr))
    {
        case EA_32BYTE:
            return emitYMMregName(reg);

        case EA_16BYTE:
            return emitXMMregName(reg);

        case EA_8BYTE:
            if ((REG_XMM0 <= reg) && (reg <= REG_XMM7))
            {
                return emitXMMregName(reg);
            }
            break;

        case EA_4BYTE:
            if ((REG_XMM0 <= reg) && (reg <= REG_XMM7))
            {
                return emitXMMregName(reg);
            }
            break;

        case EA_2BYTE:
            rn++;
            break;

        case EA_1BYTE:
            rbc        = (rbc + 1) % 2;
            rb[rbc][0] = rn[1];
            rb[rbc][1] = 'l';
            strcpy_s(&rb[rbc][2], sizeof(rb[0]) - 2, rn + 3);

            rn = rb[rbc];
            break;

        default:
            break;
    }
#endif // _TARGET_X86_

#if 0
    // The following is useful if you want register names to be tagged with * or ^ representing gcref or byref, respectively,
    // however it's possibly not interesting most of the time.
    if (EA_IS_GCREF(attr) || EA_IS_BYREF(attr))
    {
        if (rn != rb[rbc])
        {
            rbc = (rbc+1)%2;
            strcpy_s(rb[rbc], sizeof(rb[rbc]), rn);
            rn = rb[rbc];
        }

        if (EA_IS_GCREF(attr))
        {
            strcat_s(rb[rbc], sizeof(rb[rbc]), "*");
        }
        else if (EA_IS_BYREF(attr))
        {
            strcat_s(rb[rbc], sizeof(rb[rbc]), "^");
        }
    }
#endif // 0

    return rn;
}

/*****************************************************************************
 *
 *  Return a string that represents the given FP register.
 */

const char* emitter::emitFPregName(unsigned reg, bool varName)
{
    assert(reg < REG_COUNT);

    return emitComp->compFPregVarName((regNumber)(reg), varName);
}

/*****************************************************************************
 *
 *  Return a string that represents the given XMM register.
 */

const char* emitter::emitXMMregName(unsigned reg)
{
    static const char* const regNames[] = {
#define REGDEF(name, rnum, mask, sname) "x" sname,
#include "register.h"
    };

    assert(reg < REG_COUNT);
    assert(reg < _countof(regNames));

    return regNames[reg];
}

/*****************************************************************************
 *
 *  Return a string that represents the given YMM register.
 */

const char* emitter::emitYMMregName(unsigned reg)
{
    static const char* const regNames[] = {
#define REGDEF(name, rnum, mask, sname) "y" sname,
#include "register.h"
    };

    assert(reg < REG_COUNT);
    assert(reg < _countof(regNames));

    return regNames[reg];
}

/*****************************************************************************
 *
 *  Display a static data member reference.
 */

void emitter::emitDispClsVar(CORINFO_FIELD_HANDLE fldHnd, ssize_t offs, bool reloc /* = false */)
{
    int doffs;

    /* Filter out the special case of fs:[offs] */

    // Munge any pointers if we want diff-able disassembly
    if (emitComp->opts.disDiffable)
    {
        ssize_t top12bits = (offs >> 20);
        if ((top12bits != 0) && (top12bits != -1))
        {
            offs = 0xD1FFAB1E;
        }
    }

    if (fldHnd == FLD_GLOBAL_FS)
    {
        printf("FS:[0x%04X]", offs);
        return;
    }

    if (fldHnd == FLD_GLOBAL_DS)
    {
        printf("[0x%04X]", offs);
        return;
    }

    printf("[");

    doffs = Compiler::eeGetJitDataOffs(fldHnd);

    if (reloc)
    {
        printf("reloc ");
    }

    if (doffs >= 0)
    {
        if (doffs & 1)
        {
            printf("@CNS%02u", doffs - 1);
        }
        else
        {
            printf("@RWD%02u", doffs);
        }

        if (offs)
        {
            printf("%+Id", offs);
        }
    }
    else
    {
        printf("classVar[%#x]", emitComp->dspPtr(fldHnd));

        if (offs)
        {
            printf("%+Id", offs);
        }
    }

    printf("]");

    if (emitComp->opts.varNames && offs < 0)
    {
        printf("'%s", emitComp->eeGetFieldName(fldHnd));
        if (offs)
        {
            printf("%+Id", offs);
        }
        printf("'");
    }
}

/*****************************************************************************
 *
 *  Display a stack frame reference.
 */

void emitter::emitDispFrameRef(int varx, int disp, int offs, bool asmfm)
{
    int  addr;
    bool bEBP;

    printf("[");

    if (!asmfm || emitComp->lvaDoneFrameLayout == Compiler::NO_FRAME_LAYOUT)
    {
        if (varx < 0)
        {
            printf("TEMP_%02u", -varx);
        }
        else
        {
            printf("V%02u", +varx);
        }

        if (disp < 0)
        {
            printf("-0x%X", -disp);
        }
        else if (disp > 0)
        {
            printf("+0x%X", +disp);
        }
    }

    if (emitComp->lvaDoneFrameLayout == Compiler::FINAL_FRAME_LAYOUT)
    {
        if (!asmfm)
        {
            printf(" ");
        }

        addr = emitComp->lvaFrameAddress(varx, &bEBP) + disp;

        if (bEBP)
        {
            printf(STR_FPBASE);

            if (addr < 0)
            {
                printf("-%02XH", -addr);
            }
            else if (addr > 0)
            {
                printf("+%02XH", addr);
            }
        }
        else
        {
            /* Adjust the offset by amount currently pushed on the stack */

            printf(STR_SPBASE);

            if (addr < 0)
            {
                printf("-%02XH", -addr);
            }
            else if (addr > 0)
            {
                printf("+%02XH", addr);
            }

#if !FEATURE_FIXED_OUT_ARGS

            if (emitCurStackLvl)
                printf("+%02XH", emitCurStackLvl);

#endif // !FEATURE_FIXED_OUT_ARGS
        }
    }

    printf("]");

    if (varx >= 0 && emitComp->opts.varNames)
    {
        LclVarDsc*  varDsc;
        const char* varName;

        assert((unsigned)varx < emitComp->lvaCount);
        varDsc  = emitComp->lvaTable + varx;
        varName = emitComp->compLocalVarName(varx, offs);

        if (varName)
        {
            printf("'%s", varName);

            if (disp < 0)
            {
                printf("-%d", -disp);
            }
            else if (disp > 0)
            {
                printf("+%d", +disp);
            }

            printf("'");
        }
    }
}

/*****************************************************************************
 *
 *  Display an reloc value
 *  If we are formatting for an assembly listing don't print the hex value
 *  since it will prevent us from doing assembly diffs
 */
void emitter::emitDispReloc(ssize_t value)
{
    if (emitComp->opts.disAsm)
    {
        printf("(reloc)");
    }
    else
    {
        printf("(reloc 0x%Ix)", emitComp->dspPtr(value));
    }
}

/*****************************************************************************
 *
 *  Display an address mode.
 */

void emitter::emitDispAddrMode(instrDesc* id, bool noDetail)
{
    bool    nsep = false;
    ssize_t disp;

    unsigned     jtno = 0;
    dataSection* jdsc = nullptr;

    /* The displacement field is in an unusual place for calls */

    disp = (id->idIns() == INS_call) ? emitGetInsCIdisp(id) : emitGetInsAmdAny(id);

    /* Display a jump table label if this is a switch table jump */

    if (id->idIns() == INS_i_jmp)
    {
        UNATIVE_OFFSET offs = 0;

        /* Find the appropriate entry in the data section list */

        for (jdsc = emitConsDsc.dsdList, jtno = 0; jdsc; jdsc = jdsc->dsNext)
        {
            UNATIVE_OFFSET size = jdsc->dsSize;

            /* Is this a label table? */

            if (size & 1)
            {
                size--;
                jtno++;

                if (offs == id->idDebugOnlyInfo()->idMemCookie)
                {
                    break;
                }
            }

            offs += size;
        }

        /* If we've found a matching entry then is a table jump */

        if (jdsc)
        {
            if (id->idIsDspReloc())
            {
                printf("reloc ");
            }
            printf("J_M%03u_DS%02u", Compiler::s_compMethodsCount, id->idDebugOnlyInfo()->idMemCookie);
        }

        disp -= id->idDebugOnlyInfo()->idMemCookie;
    }

    bool frameRef = false;

    printf("[");

    if (id->idAddr()->iiaAddrMode.amBaseReg != REG_NA)
    {
        printf("%s", emitRegName(id->idAddr()->iiaAddrMode.amBaseReg));
        nsep = true;
        if (id->idAddr()->iiaAddrMode.amBaseReg == REG_ESP)
        {
            frameRef = true;
        }
        else if (emitComp->isFramePointerUsed() && id->idAddr()->iiaAddrMode.amBaseReg == REG_EBP)
        {
            frameRef = true;
        }
    }

    if (id->idAddr()->iiaAddrMode.amIndxReg != REG_NA)
    {
        size_t scale = emitDecodeScale(id->idAddr()->iiaAddrMode.amScale);

        if (nsep)
        {
            printf("+");
        }
        if (scale > 1)
        {
            printf("%u*", scale);
        }
        printf("%s", emitRegName(id->idAddr()->iiaAddrMode.amIndxReg));
        nsep = true;
    }

    if ((id->idIsDspReloc()) && (id->idIns() != INS_i_jmp))
    {
        if (nsep)
        {
            printf("+");
        }
        emitDispReloc(disp);
    }
    else
    {
        // Munge any pointers if we want diff-able disassembly
        if (emitComp->opts.disDiffable)
        {
            ssize_t top12bits = (disp >> 20);
            if ((top12bits != 0) && (top12bits != -1))
            {
                disp = 0xD1FFAB1E;
            }
        }

        if (disp > 0)
        {
            if (nsep)
            {
                printf("+");
            }
            if (frameRef)
            {
                printf("%02XH", disp);
            }
            else if (disp < 1000)
            {
                printf("%d", disp);
            }
            else if (disp <= 0xFFFF)
            {
                printf("%04XH", disp);
            }
            else
            {
                printf("%08XH", disp);
            }
        }
        else if (disp < 0)
        {
            if (frameRef)
            {
                printf("-%02XH", -disp);
            }
            else if (disp > -1000)
            {
                printf("-%d", -disp);
            }
            else if (disp >= -0xFFFF)
            {
                printf("-%04XH", -disp);
            }
            else if ((disp & 0x7F000000) != 0x7F000000)
            {
                printf("%08XH", disp);
            }
            else
            {
                printf("-%08XH", -disp);
            }
        }
        else if (!nsep)
        {
            printf("%04XH", disp);
        }
    }

    printf("]");

    // pretty print string if it looks like one
    if ((id->idGCref() == GCT_GCREF) && (id->idIns() == INS_mov) && (id->idAddr()->iiaAddrMode.amBaseReg == REG_NA))
    {
        const wchar_t* str = emitComp->eeGetCPString(disp);
        if (str != nullptr)
        {
            printf("      '%S'", str);
        }
    }

    if (jdsc && !noDetail)
    {
        unsigned     cnt = (jdsc->dsSize - 1) / TARGET_POINTER_SIZE;
        BasicBlock** bbp = (BasicBlock**)jdsc->dsCont;

#ifdef _TARGET_AMD64_
#define SIZE_LETTER "Q"
#else
#define SIZE_LETTER "D"
#endif
        printf("\n\n    J_M%03u_DS%02u LABEL   " SIZE_LETTER "WORD", Compiler::s_compMethodsCount, jtno);

        /* Display the label table (it's stored as "BasicBlock*" values) */

        do
        {
            insGroup* lab;

            /* Convert the BasicBlock* value to an IG address */

            lab = (insGroup*)emitCodeGetCookie(*bbp++);
            assert(lab);

            printf("\n            D" SIZE_LETTER "      G_M%03u_IG%02u", Compiler::s_compMethodsCount, lab->igNum);
        } while (--cnt);
    }
}

/*****************************************************************************
 *
 *  If the given instruction is a shift, display the 2nd operand.
 */

void emitter::emitDispShift(instruction ins, int cnt)
{
    switch (ins)
    {
        case INS_rcl_1:
        case INS_rcr_1:
        case INS_rol_1:
        case INS_ror_1:
        case INS_shl_1:
        case INS_shr_1:
        case INS_sar_1:
            printf(", 1");
            break;

        case INS_rcl:
        case INS_rcr:
        case INS_rol:
        case INS_ror:
        case INS_shl:
        case INS_shr:
        case INS_sar:
            printf(", cl");
            break;

        case INS_rcl_N:
        case INS_rcr_N:
        case INS_rol_N:
        case INS_ror_N:
        case INS_shl_N:
        case INS_shr_N:
        case INS_sar_N:
            printf(", %d", cnt);
            break;

        default:
            break;
    }
}

/*****************************************************************************
 *
 *  Display (optionally) the bytes for the instruction encoding in hex
 */

void emitter::emitDispInsHex(BYTE* code, size_t sz)
{
    // We do not display the instruction hex if we want diff-able disassembly
    if (!emitComp->opts.disDiffable)
    {
#ifdef _TARGET_AMD64_
        // how many bytes per instruction we format for
        const size_t digits = 10;
#else // _TARGET_X86
        const size_t digits = 6;
#endif
        printf(" ");
        for (unsigned i = 0; i < sz; i++)
        {
            printf("%02X", (*((BYTE*)(code + i))));
        }

        if (sz < digits)
        {
            printf("%.*s", 2 * (digits - sz), "                         ");
        }
    }
}

/*****************************************************************************
 *
 *  Display the given instruction.
 */

void emitter::emitDispIns(
    instrDesc* id, bool isNew, bool doffs, bool asmfm, unsigned offset, BYTE* code, size_t sz, insGroup* ig)
{
    emitAttr    attr;
    const char* sstr;

    instruction ins = id->idIns();

    if (emitComp->verbose)
    {
        unsigned idNum = id->idDebugOnlyInfo()->idNum;
        printf("IN%04x: ", idNum);
    }

#define ID_INFO_DSP_RELOC ((bool)(id->idIsDspReloc()))

    /* Display a constant value if the instruction references one */

    if (!isNew)
    {
        switch (id->idInsFmt())
        {
            int offs;

            case IF_MRD_RRD:
            case IF_MWR_RRD:
            case IF_MRW_RRD:

            case IF_RRD_MRD:
            case IF_RWR_MRD:
            case IF_RRW_MRD:

            case IF_MRD_CNS:
            case IF_MWR_CNS:
            case IF_MRW_CNS:
            case IF_MRW_SHF:

            case IF_MRD:
            case IF_MWR:
            case IF_MRW:

            case IF_MRD_OFF:

                /* Is this actually a reference to a data section? */

                offs = Compiler::eeGetJitDataOffs(id->idAddr()->iiaFieldHnd);

                if (offs >= 0)
                {
                    void* addr;

                    /* Display a data section reference */

                    assert((unsigned)offs < emitConsDsc.dsdOffs);
                    addr = emitConsBlock ? emitConsBlock + offs : nullptr;

#if 0
                // TODO-XArch-Cleanup: Fix or remove this code.
                /* Is the operand an integer or floating-point value? */

                bool isFP = false;

                if  (CodeGen::instIsFP(id->idIns()))
                {
                    switch (id->idIns())
                    {
                    case INS_fild:
                    case INS_fildl:
                        break;

                    default:
                        isFP = true;
                        break;
                    }
                }

                if (offs & 1)
                    printf("@CNS%02u", offs);
                else
                    printf("@RWD%02u", offs);

                printf("      ");

                if  (addr)
                {
                    addr = 0;
                    // TODO-XArch-Bug?:
                    //          This was busted by switching the order
                    //          in which we output the code block vs.
                    //          the data blocks -- when we get here,
                    //          the data block has not been filled in
                    //          yet, so we'll display garbage.

                    if  (isFP)
                    {
                        if  (id->idOpSize() == EA_4BYTE)
                            printf("DF      %f \n", addr ? *(float   *)addr : 0);
                        else
                            printf("DQ      %lf\n", addr ? *(double  *)addr : 0);
                    }
                    else
                    {
                        if  (id->idOpSize() <= EA_4BYTE)
                            printf("DD      %d \n", addr ? *(int     *)addr : 0);
                        else
                            printf("DQ      %D \n", addr ? *(__int64 *)addr : 0);
                    }
                }
#endif
                }
                break;

            default:
                break;
        }
    }

    // printf("[F=%s] "   , emitIfName(id->idInsFmt()));
    // printf("INS#%03u: ", id->idDebugOnlyInfo()->idNum);
    // printf("[S=%02u] " , emitCurStackLvl); if (isNew) printf("[M=%02u] ", emitMaxStackDepth);
    // printf("[S=%02u] " , emitCurStackLvl/sizeof(INT32));
    // printf("[A=%08X] " , emitSimpleStkMask);
    // printf("[A=%08X] " , emitSimpleByrefStkMask);
    // printf("[L=%02u] " , id->idCodeSize());

    if (!emitComp->opts.dspEmit && !isNew && !asmfm)
    {
        doffs = true;
    }

    /* Display the instruction offset */

    emitDispInsOffs(offset, doffs);

    if (code != nullptr)
    {
        /* Display the instruction hex code */

        emitDispInsHex(code, sz);
    }

    /* Display the instruction name */

    sstr = codeGen->genInsName(ins);

    if (IsAVXInstruction(ins))
    {
        printf(" v%-8s", sstr);
    }
    else
    {
        printf(" %-9s", sstr);
    }
#ifndef FEATURE_PAL
    if (strnlen_s(sstr, 10) >= 8)
#else  // FEATURE_PAL
    if (strnlen(sstr, 10) >= 8)
#endif // FEATURE_PAL
    {
        printf(" ");
    }

    /* By now the size better be set to something */

    assert(emitInstCodeSz(id) || emitInstHasNoCode(ins));

    /* Figure out the operand size */

    if (id->idGCref() == GCT_GCREF)
    {
        attr = EA_GCREF;
        sstr = "gword ptr ";
    }
    else if (id->idGCref() == GCT_BYREF)
    {
        attr = EA_BYREF;
        sstr = "bword ptr ";
    }
    else
    {
        attr = id->idOpSize();
        sstr = codeGen->genSizeStr(attr);

        if (ins == INS_lea)
        {
#ifdef _TARGET_AMD64_
            assert((attr == EA_4BYTE) || (attr == EA_8BYTE));
#else
            assert(attr == EA_4BYTE);
#endif
            sstr = "";
        }
    }

    /* Now see what instruction format we've got */

    // First print the implicit register usage
    if (instrHasImplicitRegPairDest(ins))
    {
        printf("%s:%s, ", emitRegName(REG_EDX, id->idOpSize()), emitRegName(REG_EAX, id->idOpSize()));
    }
    else if (instrIs3opImul(ins))
    {
        regNumber tgtReg = inst3opImulReg(ins);
        printf("%s, ", emitRegName(tgtReg, id->idOpSize()));
    }

    switch (id->idInsFmt())
    {
        ssize_t     val;
        ssize_t     offs;
        CnsVal      cnsVal;
        const char* methodName;

        case IF_CNS:
            val = emitGetInsSC(id);
#ifdef _TARGET_AMD64_
            // no 8-byte immediates allowed here!
            assert((val >= 0xFFFFFFFF80000000LL) && (val <= 0x000000007FFFFFFFLL));
#endif
            if (id->idIsCnsReloc())
            {
                emitDispReloc(val);
            }
            else
            {
            PRINT_CONSTANT:
                // Munge any pointers if we want diff-able disassembly
                if (emitComp->opts.disDiffable)
                {
                    ssize_t top12bits = (val >> 20);
                    if ((top12bits != 0) && (top12bits != -1))
                    {
                        val = 0xD1FFAB1E;
                    }
                }
                if ((val > -1000) && (val < 1000))
                {
                    printf("%d", val);
                }
                else if ((val > 0) || ((val & 0x7F000000) != 0x7F000000))
                {
                    printf("0x%IX", val);
                }
                else
                { // (val < 0)
                    printf("-0x%IX", -val);
                }
            }
            break;

        case IF_ARD:
        case IF_AWR:
        case IF_ARW:

            if (ins == INS_call && id->idIsCallRegPtr())
            {
                printf("%s", emitRegName(id->idAddr()->iiaAddrMode.amBaseReg));
                break;
            }

            printf("%s", sstr);
            emitDispAddrMode(id, isNew);
            emitDispShift(ins);

            if (ins == INS_call)
            {
                assert(id->idInsFmt() == IF_ARD);

                /* Ignore indirect calls */

                if (id->idDebugOnlyInfo()->idMemCookie == 0)
                {
                    break;
                }

                assert(id->idDebugOnlyInfo()->idMemCookie);

                /* This is a virtual call */

                methodName = emitComp->eeGetMethodFullName((CORINFO_METHOD_HANDLE)id->idDebugOnlyInfo()->idMemCookie);
                printf("%s", methodName);
            }
            break;

        case IF_RRD_ARD:
        case IF_RWR_ARD:
        case IF_RRW_ARD:
#ifdef _TARGET_AMD64_
            if (ins == INS_movsxd)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_8BYTE), sstr);
            }
            else
#endif
                if (ins == INS_movsx || ins == INS_movzx)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_PTRSIZE), sstr);
            }
            else
            {
                printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            }
            emitDispAddrMode(id);
            break;

        case IF_RRW_ARD_CNS:
        case IF_RWR_ARD_CNS:
        {
            printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            emitDispAddrMode(id);
            emitGetInsAmdCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }

            break;
        }

        case IF_AWR_RRD_CNS:
        {
            assert(ins == INS_vextracti128 || ins == INS_vextractf128);
            // vextracti/f128 extracts 128-bit data, so we fix sstr as "xmm ptr"
            sstr = codeGen->genSizeStr(EA_ATTR(16));
            printf(sstr);
            emitDispAddrMode(id);
            printf(", %s", emitRegName(id->idReg1(), attr));

            emitGetInsAmdCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }

            break;
        }

        case IF_RWR_RRD_ARD:
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            emitDispAddrMode(id);
            break;

        case IF_RWR_RRD_ARD_CNS:
        {
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            emitDispAddrMode(id);
            emitGetInsAmdCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }

            break;
        }

        case IF_ARD_RRD:
        case IF_AWR_RRD:
        case IF_ARW_RRD:

            printf("%s", sstr);
            emitDispAddrMode(id);
            printf(", %s", emitRegName(id->idReg1(), attr));
            break;

        case IF_ARD_CNS:
        case IF_AWR_CNS:
        case IF_ARW_CNS:
        case IF_ARW_SHF:

            printf("%s", sstr);
            emitDispAddrMode(id);
            emitGetInsAmdCns(id, &cnsVal);
            val = cnsVal.cnsVal;
#ifdef _TARGET_AMD64_
            // no 8-byte immediates allowed here!
            assert((val >= 0xFFFFFFFF80000000LL) && (val <= 0x000000007FFFFFFFLL));
#endif
            if (id->idInsFmt() == IF_ARW_SHF)
            {
                emitDispShift(ins, (BYTE)val);
            }
            else
            {
                printf(", ");
                if (cnsVal.cnsReloc)
                {
                    emitDispReloc(val);
                }
                else
                {
                    goto PRINT_CONSTANT;
                }
            }
            break;

        case IF_SRD:
        case IF_SWR:
        case IF_SRW:

            printf("%s", sstr);

#if !FEATURE_FIXED_OUT_ARGS
            if (ins == INS_pop)
                emitCurStackLvl -= sizeof(int);
#endif

            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);

#if !FEATURE_FIXED_OUT_ARGS
            if (ins == INS_pop)
                emitCurStackLvl += sizeof(int);
#endif

            emitDispShift(ins);
            break;

        case IF_SRD_RRD:
        case IF_SWR_RRD:
        case IF_SRW_RRD:

            printf("%s", sstr);

            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);

            printf(", %s", emitRegName(id->idReg1(), attr));
            break;

        case IF_SRD_CNS:
        case IF_SWR_CNS:
        case IF_SRW_CNS:
        case IF_SRW_SHF:

            printf("%s", sstr);

            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);

            emitGetInsCns(id, &cnsVal);
            val = cnsVal.cnsVal;
#ifdef _TARGET_AMD64_
            // no 8-byte immediates allowed here!
            assert((val >= 0xFFFFFFFF80000000LL) && (val <= 0x000000007FFFFFFFLL));
#endif
            if (id->idInsFmt() == IF_SRW_SHF)
            {
                emitDispShift(ins, (BYTE)val);
            }
            else
            {
                printf(", ");
                if (cnsVal.cnsReloc)
                {
                    emitDispReloc(val);
                }
                else
                {
                    goto PRINT_CONSTANT;
                }
            }
            break;

        case IF_RRD_SRD:
        case IF_RWR_SRD:
        case IF_RRW_SRD:
#ifdef _TARGET_AMD64_
            if (ins == INS_movsxd)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_8BYTE), sstr);
            }
            else
#endif
                if (ins == INS_movsx || ins == INS_movzx)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_PTRSIZE), sstr);
            }
            else
            {
                printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            }

            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);

            break;

        case IF_RRW_SRD_CNS:
        case IF_RWR_SRD_CNS:
        {
            printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);
            emitGetInsCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;
        }

        case IF_RWR_RRD_SRD:
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);
            break;

        case IF_RWR_RRD_SRD_CNS:
        {
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            emitDispFrameRef(id->idAddr()->iiaLclVar.lvaVarNum(), id->idAddr()->iiaLclVar.lvaOffset(),
                             id->idDebugOnlyInfo()->idVarRefOffs, asmfm);
            emitGetInsCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;
        }

        case IF_RRD_RRD:
        case IF_RWR_RRD:
        case IF_RRW_RRD:
            if (ins == INS_mov_i2xmm)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_16BYTE), emitRegName(id->idReg2(), attr));
            }
            else if (ins == INS_mov_xmm2i)
            {
                printf("%s, %s", emitRegName(id->idReg2(), attr), emitRegName(id->idReg1(), EA_16BYTE));
            }
            else if (ins == INS_pmovmskb)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_4BYTE), emitRegName(id->idReg2(), attr));
            }
            else if ((ins == INS_cvtsi2ss) || (ins == INS_cvtsi2sd))
            {
                printf(" %s, %s", emitRegName(id->idReg1(), EA_16BYTE), emitRegName(id->idReg2(), attr));
            }
            else if ((ins == INS_cvttsd2si) || (ins == INS_cvtss2si) || (ins == INS_cvtsd2si) || (ins == INS_cvttss2si))
            {
                printf(" %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), EA_16BYTE));
            }
#ifdef _TARGET_AMD64_
            else if (ins == INS_movsxd)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_8BYTE), emitRegName(id->idReg2(), EA_4BYTE));
            }
#endif // _TARGET_AMD64_
            else if (ins == INS_movsx || ins == INS_movzx)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_PTRSIZE), emitRegName(id->idReg2(), attr));
            }
            else if (ins == INS_bt)
            {
                // INS_bt operands are reversed. Display them in the normal order.
                printf("%s, %s", emitRegName(id->idReg2(), attr), emitRegName(id->idReg1(), attr));
            }
#ifdef FEATURE_HW_INTRINSICS
            else if (ins == INS_crc32 && attr != EA_8BYTE)
            {
                printf("%s, %s", emitRegName(id->idReg1(), EA_4BYTE), emitRegName(id->idReg2(), attr));
            }
#endif // FEATURE_HW_INTRINSICS
            else
            {
                printf("%s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr));
            }
            break;

        case IF_RRW_RRW:
            assert(ins == INS_xchg);
            printf("%s,", emitRegName(id->idReg1(), attr));
            printf(" %s", emitRegName(id->idReg2(), attr));
            break;

        case IF_RWR_RRD_RRD:
            assert(IsAVXInstruction(ins));
            assert(IsThreeOperandAVXInstruction(ins));
            printf("%s, ", emitRegName(id->idReg1(), attr));
            printf("%s, ", emitRegName(id->idReg2(), attr));
            printf("%s", emitRegName(id->idReg3(), attr));
            break;
        case IF_RWR_RRD_RRD_CNS:
            assert(IsAVXInstruction(ins));
            assert(IsThreeOperandAVXInstruction(ins));
            printf("%s, ", emitRegName(id->idReg1(), attr));
            printf("%s, ", emitRegName(id->idReg2(), attr));
            printf("%s, ", emitRegName(id->idReg3(), attr));
            val = emitGetInsSC(id);
            goto PRINT_CONSTANT;
            break;
        case IF_RWR_RRD_RRD_RRD:
            assert(IsAVXOnlyInstruction(ins));
            assert(UseVEXEncoding());
            printf("%s, ", emitRegName(id->idReg1(), attr));
            printf("%s, ", emitRegName(id->idReg2(), attr));
            printf("%s, ", emitRegName(id->idReg3(), attr));
            printf("%s", emitRegName(id->idReg4(), attr));
            break;
        case IF_RRW_RRW_CNS:
            printf("%s,", emitRegName(id->idReg1(), attr));
            printf(" %s", emitRegName(id->idReg2(), attr));
            val = emitGetInsSC(id);
#ifdef _TARGET_AMD64_
            // no 8-byte immediates allowed here!
            assert((val >= 0xFFFFFFFF80000000LL) && (val <= 0x000000007FFFFFFFLL));
#endif
            printf(", ");
            if (id->idIsCnsReloc())
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;

        case IF_RRD:
        case IF_RWR:
        case IF_RRW:
            printf("%s", emitRegName(id->idReg1(), attr));
            emitDispShift(ins);
            break;

        case IF_RRW_SHF:
            printf("%s", emitRegName(id->idReg1(), attr));
            emitDispShift(ins, (BYTE)emitGetInsSC(id));
            break;

        case IF_RRD_MRD:
        case IF_RWR_MRD:
        case IF_RRW_MRD:

            if (ins == INS_movsx || ins == INS_movzx)
            {
                attr = EA_PTRSIZE;
            }
#ifdef _TARGET_AMD64_
            else if (ins == INS_movsxd)
            {
                attr = EA_PTRSIZE;
            }
#endif
            printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            break;

        case IF_RRW_MRD_CNS:
        case IF_RWR_MRD_CNS:
        {
            printf("%s, %s", emitRegName(id->idReg1(), attr), sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            emitGetInsDcmCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;
        }

        case IF_MWR_RRD_CNS:
        {
            assert(ins == INS_vextracti128 || ins == INS_vextractf128);
            // vextracti/f128 extracts 128-bit data, so we fix sstr as "xmm ptr"
            sstr = codeGen->genSizeStr(EA_ATTR(16));
            printf(sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            printf(", %s", emitRegName(id->idReg1(), attr));
            emitGetInsDcmCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }

            break;
        }

        case IF_RWR_RRD_MRD:
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            break;

        case IF_RWR_RRD_MRD_CNS:
        {
            printf("%s, %s, %s", emitRegName(id->idReg1(), attr), emitRegName(id->idReg2(), attr), sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            emitGetInsDcmCns(id, &cnsVal);

            val = cnsVal.cnsVal;
            printf(", ");

            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;
        }

        case IF_RWR_MRD_OFF:

            printf("%s, %s", emitRegName(id->idReg1(), attr), "offset");
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            break;

        case IF_MRD_RRD:
        case IF_MWR_RRD:
        case IF_MRW_RRD:

            printf("%s", sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            printf(", %s", emitRegName(id->idReg1(), attr));
            break;

        case IF_MRD_CNS:
        case IF_MWR_CNS:
        case IF_MRW_CNS:
        case IF_MRW_SHF:

            printf("%s", sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            emitGetInsDcmCns(id, &cnsVal);
            val = cnsVal.cnsVal;
#ifdef _TARGET_AMD64_
            // no 8-byte immediates allowed here!
            assert((val >= 0xFFFFFFFF80000000LL) && (val <= 0x000000007FFFFFFFLL));
#endif
            if (cnsVal.cnsReloc)
            {
                emitDispReloc(val);
            }
            else if (id->idInsFmt() == IF_MRW_SHF)
            {
                emitDispShift(ins, (BYTE)val);
            }
            else
            {
                printf(", ");
                goto PRINT_CONSTANT;
            }
            break;

        case IF_MRD:
        case IF_MWR:
        case IF_MRW:

            printf("%s", sstr);
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            emitDispShift(ins);
            break;

        case IF_MRD_OFF:

            printf("offset ");
            offs = emitGetInsDsp(id);
            emitDispClsVar(id->idAddr()->iiaFieldHnd, offs, ID_INFO_DSP_RELOC);
            break;

        case IF_RRD_CNS:
        case IF_RWR_CNS:
        case IF_RRW_CNS:
            printf("%s, ", emitRegName(id->idReg1(), attr));
            val = emitGetInsSC(id);
            if (id->idIsCnsReloc())
            {
                emitDispReloc(val);
            }
            else
            {
                goto PRINT_CONSTANT;
            }
            break;

        case IF_LABEL:
        case IF_RWR_LABEL:
        case IF_SWR_LABEL:

            if (ins == INS_lea)
            {
                printf("%s, ", emitRegName(id->idReg1(), attr));
            }
            else if (ins == INS_mov)
            {
                /* mov   dword ptr [frame.callSiteReturnAddress], label */
                assert(id->idInsFmt() == IF_SWR_LABEL);
                instrDescLbl* idlbl = (instrDescLbl*)id;

                emitDispFrameRef(idlbl->dstLclVar.lvaVarNum(), idlbl->dstLclVar.lvaOffset(), 0, asmfm);

                printf(", ");
            }

            if (((instrDescJmp*)id)->idjShort)
            {
                printf("SHORT ");
            }

            if (id->idIsBound())
            {
                printf("G_M%03u_IG%02u", Compiler::s_compMethodsCount, id->idAddr()->iiaIGlabel->igNum);
            }
            else
            {
                printf("L_M%03u_BB%02u", Compiler::s_compMethodsCount, id->idAddr()->iiaBBlabel->bbNum);
            }
            break;

        case IF_METHOD:
        case IF_METHPTR:
            if (id->idIsCallAddr())
            {
                offs       = (ssize_t)id->idAddr()->iiaAddr;
                methodName = "";
            }
            else
            {
                offs       = 0;
                methodName = emitComp->eeGetMethodFullName((CORINFO_METHOD_HANDLE)id->idDebugOnlyInfo()->idMemCookie);
            }

            if (id->idInsFmt() == IF_METHPTR)
            {
                printf("[");
            }

            if (offs)
            {
                if (id->idIsDspReloc())
                {
                    printf("reloc ");
                }
                printf("%08X", offs);
            }
            else
            {
                printf("%s", methodName);
            }

            if (id->idInsFmt() == IF_METHPTR)
            {
                printf("]");
            }

            break;

        case IF_NONE:
            break;

        default:
            printf("unexpected format %s", emitIfName(id->idInsFmt()));
            assert(!"unexpectedFormat");
            break;
    }

    if (sz != 0 && sz != id->idCodeSize() && (!asmfm || emitComp->verbose))
    {
        // Code size in the instrDesc is different from the actual code size we've been given!
        printf(" (ECS:%d, ACS:%d)", id->idCodeSize(), sz);
    }

    printf("\n");
}

/*****************************************************************************/
#endif

/*****************************************************************************
 *
 *  Output nBytes bytes of NOP instructions
 */

static BYTE* emitOutputNOP(BYTE* dst, size_t nBytes)
{
    assert(nBytes <= 15);

#ifndef _TARGET_AMD64_
    // TODO-X86-CQ: when VIA C3 CPU's are out of circulation, switch to the
    // more efficient real NOP: 0x0F 0x1F +modR/M
    // Also can't use AMD recommended, multiple size prefixes (i.e. 0x66 0x66 0x90 for 3 byte NOP)
    // because debugger and msdis don't like it, so maybe VIA doesn't either
    // So instead just stick to repeating single byte nops

    switch (nBytes)
    {
        case 15:
            *dst++ = 0x90;
            __fallthrough;
        case 14:
            *dst++ = 0x90;
            __fallthrough;
        case 13:
            *dst++ = 0x90;
            __fallthrough;
        case 12:
            *dst++ = 0x90;
            __fallthrough;
        case 11:
            *dst++ = 0x90;
            __fallthrough;
        case 10:
            *dst++ = 0x90;
            __fallthrough;
        case 9:
            *dst++ = 0x90;
            __fallthrough;
        case 8:
            *dst++ = 0x90;
            __fallthrough;
        case 7:
            *dst++ = 0x90;
            __fallthrough;
        case 6:
            *dst++ = 0x90;
            __fallthrough;
        case 5:
            *dst++ = 0x90;
            __fallthrough;
        case 4:
            *dst++ = 0x90;
            __fallthrough;
        case 3:
            *dst++ = 0x90;
            __fallthrough;
        case 2:
            *dst++ = 0x90;
            __fallthrough;
        case 1:
            *dst++ = 0x90;
            break;
        case 0:
            break;
    }
#else  // _TARGET_AMD64_
    switch (nBytes)
    {
        case 2:
            *dst++ = 0x66;
            __fallthrough;
        case 1:
            *dst++ = 0x90;
            break;
        case 0:
            break;
        case 3:
            *dst++ = 0x0F;
            *dst++ = 0x1F;
            *dst++ = 0x00;
            break;
        case 4:
            *dst++ = 0x0F;
            *dst++ = 0x1F;
            *dst++ = 0x40;
            *dst++ = 0x00;
            break;
        case 6:
            *dst++ = 0x66;
            __fallthrough;
        case 5:
            *dst++ = 0x0F;
            *dst++ = 0x1F;
            *dst++ = 0x44;
            *dst++ = 0x00;
            *dst++ = 0x00;
            break;
        case 7:
            *dst++ = 0x0F;
            *dst++ = 0x1F;
            *dst++ = 0x80;
            *dst++ = 0x00;
            *dst++ = 0x00;
            *dst++ = 0x00;
            *dst++ = 0x00;
            break;
        case 15:
            // More than 3 prefixes is slower than just 2 NOPs
            dst = emitOutputNOP(emitOutputNOP(dst, 7), 8);
            break;
        case 14:
            // More than 3 prefixes is slower than just 2 NOPs
            dst = emitOutputNOP(emitOutputNOP(dst, 7), 7);
            break;
        case 13:
            // More than 3 prefixes is slower than just 2 NOPs
            dst = emitOutputNOP(emitOutputNOP(dst, 5), 8);
            break;
        case 12:
            // More than 3 prefixes is slower than just 2 NOPs
            dst = emitOutputNOP(emitOutputNOP(dst, 4), 8);
            break;
        case 11:
            *dst++ = 0x66;
            __fallthrough;
        case 10:
            *dst++ = 0x66;
            __fallthrough;
        case 9:
            *dst++ = 0x66;
            __fallthrough;
        case 8:
            *dst++ = 0x0F;
            *dst++ = 0x1F;
            *dst++ = 0x84;
            *dst++ = 0x00;
            *dst++ = 0x00;
            *dst++ = 0x00;
            *dst++ = 0x00;
            *dst++ = 0x00;
            break;
    }
#endif // _TARGET_AMD64_

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction involving an address mode.
 */

BYTE* emitter::emitOutputAM(BYTE* dst, instrDesc* id, code_t code, CnsVal* addc)
{
    regNumber reg;
    regNumber rgx;
    ssize_t   dsp;
    bool      dspInByte;
    bool      dspIsZero;

    instruction ins  = id->idIns();
    emitAttr    size = id->idOpSize();
    size_t      opsz = EA_SIZE_IN_BYTES(size);

    // Get the base/index registers
    reg = id->idAddr()->iiaAddrMode.amBaseReg;
    rgx = id->idAddr()->iiaAddrMode.amIndxReg;

    // For INS_call the instruction size is actually the return value size
    if (ins == INS_call)
    {
        // Special case: call via a register
        if (id->idIsCallRegPtr())
        {
            code_t opcode = insEncodeMRreg(INS_call, reg, EA_PTRSIZE, insCodeMR(INS_call));

            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, opcode);
            dst += emitOutputWord(dst, opcode);
            goto DONE;
        }

        // The displacement field is in an unusual place for calls
        dsp = emitGetInsCIdisp(id);

#ifdef _TARGET_AMD64_

        // Compute the REX prefix if it exists
        if (IsExtendedReg(reg, EA_PTRSIZE))
        {
            insEncodeReg012(ins, reg, EA_PTRSIZE, &code);
            // TODO-Cleanup: stop casting RegEncoding() back to a regNumber.
            reg = (regNumber)RegEncoding(reg);
        }

        if (IsExtendedReg(rgx, EA_PTRSIZE))
        {
            insEncodeRegSIB(ins, rgx, &code);
            // TODO-Cleanup: stop casting RegEncoding() back to a regNumber.
            rgx = (regNumber)RegEncoding(rgx);
        }

        // And emit the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

#endif // _TARGET_AMD64_

        goto GOT_DSP;
    }

    // Is there a large constant operand?
    if (addc && (size > EA_1BYTE))
    {
        ssize_t cval = addc->cnsVal;

        // Does the constant fit in a byte?
        if ((signed char)cval == cval && addc->cnsReloc == false && ins != INS_mov && ins != INS_test)
        {
            if (id->idInsFmt() != IF_ARW_SHF)
            {
                code |= 2;
            }

            opsz = 1;
        }
    }

    // Emit VEX prefix if required
    // There are some callers who already add VEX prefix and call this routine.
    // Therefore, add VEX prefix is one is not already present.
    code = AddVexPrefixIfNeededAndNotPresent(ins, code, size);

    // For this format, moves do not support a third operand, so we only need to handle the binary ops.
    if (TakesVexPrefix(ins))
    {
        if (IsDstDstSrcAVXInstruction(ins))
        {
            regNumber src1 = id->idReg2();

            if ((id->idInsFmt() != IF_RWR_RRD_ARD) && (id->idInsFmt() != IF_RWR_RRD_ARD_CNS))
            {
                src1 = id->idReg1();
            }

            // encode source operand reg in 'vvvv' bits in 1's complement form
            code = insEncodeReg3456(ins, src1, size, code);
        }
        else if (IsDstSrcSrcAVXInstruction(ins))
        {
            code = insEncodeReg3456(ins, id->idReg2(), size, code);
        }
    }

    // Emit the REX prefix if required
    if (TakesRexWPrefix(ins, size))
    {
        code = AddRexWPrefix(ins, code);
    }

    if (IsExtendedReg(reg, EA_PTRSIZE))
    {
        insEncodeReg012(ins, reg, EA_PTRSIZE, &code);
        // TODO-Cleanup: stop casting RegEncoding() back to a regNumber.
        reg = (regNumber)RegEncoding(reg);
    }

    if (IsExtendedReg(rgx, EA_PTRSIZE))
    {
        insEncodeRegSIB(ins, rgx, &code);
        // TODO-Cleanup: stop casting RegEncoding() back to a regNumber.
        rgx = (regNumber)RegEncoding(rgx);
    }

    // Special case emitting AVX instructions
    if (Is4ByteSSE4OrAVXInstruction(ins))
    {
        unsigned regcode = insEncodeReg345(ins, id->idReg1(), size, &code);
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        if (UseVEXEncoding())
        {
            // Emit last opcode byte
            // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
            assert((code & 0xFF) == 0);
            dst += emitOutputByte(dst, (code >> 8) & 0xFF);
        }
        else
        {
            dst += emitOutputWord(dst, code >> 16);
            dst += emitOutputWord(dst, code & 0xFFFF);
        }

        code = regcode;
    }
    // Is this a 'big' opcode?
    else if (code & 0xFF000000)
    {
        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Output the highest word of the opcode
        // We need to check again as in case of AVX instructions leading opcode bytes are stripped off
        // and encoded as part of VEX prefix.
        if (code & 0xFF000000)
        {
            dst += emitOutputWord(dst, code >> 16);
            code &= 0x0000FFFF;
        }
    }
    else if (code & 0x00FF0000)
    {
        // BT supports 16 bit operands and this code doesn't handle the necessary 66 prefix.
        assert(ins != INS_bt);

        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Output the highest byte of the opcode
        if (code & 0x00FF0000)
        {
            dst += emitOutputByte(dst, code >> 16);
            code &= 0x0000FFFF;
        }

        // Use the large version if this is not a byte. This trick will not
        // work in case of SSE2 and AVX instructions.
        if ((size != EA_1BYTE) && (ins != INS_imul) && !IsSSE2Instruction(ins) && !IsAVXInstruction(ins))
        {
            code++;
        }
    }
    else if (CodeGen::instIsFP(ins))
    {
        assert(size == EA_4BYTE || size == EA_8BYTE);
        if (size == EA_8BYTE)
        {
            code += 4;
        }
    }
    else if (!IsSSE2Instruction(ins) && !IsAVXInstruction(ins))
    {
        /* Is the operand size larger than a byte? */

        switch (size)
        {
            case EA_1BYTE:
                break;

            case EA_2BYTE:

                /* Output a size prefix for a 16-bit operand */

                dst += emitOutputByte(dst, 0x66);

                __fallthrough;

            case EA_4BYTE:
#ifdef _TARGET_AMD64_
            case EA_8BYTE:
#endif

                /* Set the 'w' bit to get the large version */

                code |= 0x1;
                break;

#ifdef _TARGET_X86_
            case EA_8BYTE:

                /* Double operand - set the appropriate bit */

                code |= 0x04;
                break;

#endif // _TARGET_X86_

            default:
                NO_WAY("unexpected size");
                break;
        }
    }

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    // Get the displacement value
    dsp = emitGetInsAmdAny(id);

GOT_DSP:

    dspInByte = ((signed char)dsp == (ssize_t)dsp);
    dspIsZero = (dsp == 0);

    if (id->idIsDspReloc())
    {
        dspInByte = false; // relocs can't be placed in a byte
    }

    // Is there a [scaled] index component?
    if (rgx == REG_NA)
    {
        // The address is of the form "[reg+disp]"
        switch (reg)
        {
            case REG_NA:
                if (id->idIsDspReloc())
                {
                    INT32 addlDelta = 0;

                    // The address is of the form "[disp]"
                    // On x86 - disp is relative to zero
                    // On Amd64 - disp is relative to RIP
                    if (Is4ByteSSE4OrAVXInstruction(ins))
                    {
                        dst += emitOutputByte(dst, code | 0x05);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x0500);
                    }

                    if (addc)
                    {
                        // It is of the form "ins [disp], immed"
                        // For emitting relocation, we also need to take into account of the
                        // additional bytes of code emitted for immed val.

                        ssize_t cval = addc->cnsVal;

#ifdef _TARGET_AMD64_
                        // all these opcodes only take a sign-extended 4-byte immediate
                        noway_assert(opsz < 8 || ((int)cval == cval && !addc->cnsReloc));
#else
                        noway_assert(opsz <= 4);
#endif

                        switch (opsz)
                        {
                            case 0:
                            case 4:
                            case 8:
                                addlDelta = -4;
                                break;
                            case 2:
                                addlDelta = -2;
                                break;
                            case 1:
                                addlDelta = -1;
                                break;

                            default:
                                assert(!"unexpected operand size");
                                unreached();
                        }
                    }

#ifdef _TARGET_AMD64_
                    // We emit zero on Amd64, to avoid the assert in emitOutputLong()
                    dst += emitOutputLong(dst, 0);
#else
                    dst += emitOutputLong(dst, dsp);
#endif
                    emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_DISP32, 0,
                                         addlDelta);
                }
                else
                {
#ifdef _TARGET_X86_
                    if (Is4ByteSSE4OrAVXInstruction(ins))
                    {
                        dst += emitOutputByte(dst, code | 0x05);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x0500);
                    }
#else  //_TARGET_AMD64_
                    // Amd64: addr fits within 32-bits and can be encoded as a displacement relative to zero.
                    // This addr mode should never be used while generating relocatable ngen code nor if
                    // the addr can be encoded as pc-relative address.
                    noway_assert(!emitComp->opts.compReloc);
                    noway_assert(codeGen->genAddrRelocTypeHint((size_t)dsp) != IMAGE_REL_BASED_REL32);
                    noway_assert((int)dsp == dsp);

                    // This requires, specifying a SIB byte after ModRM byte.
                    if (Is4ByteSSE4OrAVXInstruction(ins))
                    {
                        dst += emitOutputByte(dst, code | 0x04);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x0400);
                    }
                    dst += emitOutputByte(dst, 0x25);
#endif //_TARGET_AMD64_
                    dst += emitOutputLong(dst, dsp);
                }
                break;

            case REG_EBP:
                if (Is4ByteSSE4OrAVXInstruction(ins))
                {
                    // Does the offset fit in a byte?
                    if (dspInByte)
                    {
                        dst += emitOutputByte(dst, code | 0x45);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputByte(dst, code | 0x85);
                        dst += emitOutputLong(dst, dsp);

                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
                else
                {
                    // Does the offset fit in a byte?
                    if (dspInByte)
                    {
                        dst += emitOutputWord(dst, code | 0x4500);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x8500);
                        dst += emitOutputLong(dst, dsp);

                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
                break;

            case REG_ESP:
                if (Is4ByteSSE4OrAVXInstruction(ins))
                {
                    // Is the offset 0 or does it at least fit in a byte?
                    if (dspIsZero)
                    {
                        dst += emitOutputByte(dst, code | 0x04);
                        dst += emitOutputByte(dst, 0x24);
                    }
                    else if (dspInByte)
                    {
                        dst += emitOutputByte(dst, code | 0x44);
                        dst += emitOutputByte(dst, 0x24);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputByte(dst, code | 0x84);
                        dst += emitOutputByte(dst, 0x24);
                        dst += emitOutputLong(dst, dsp);
                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
                else
                {
                    // Is the offset 0 or does it at least fit in a byte?
                    if (dspIsZero)
                    {
                        dst += emitOutputWord(dst, code | 0x0400);
                        dst += emitOutputByte(dst, 0x24);
                    }
                    else if (dspInByte)
                    {
                        dst += emitOutputWord(dst, code | 0x4400);
                        dst += emitOutputByte(dst, 0x24);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x8400);
                        dst += emitOutputByte(dst, 0x24);
                        dst += emitOutputLong(dst, dsp);
                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
                break;

            default:
                if (Is4ByteSSE4OrAVXInstruction(ins))
                {
                    // Put the register in the opcode
                    code |= insEncodeReg012(ins, reg, EA_PTRSIZE, nullptr);

                    // Is there a displacement?
                    if (dspIsZero)
                    {
                        // This is simply "[reg]"
                        dst += emitOutputByte(dst, code);
                    }
                    else
                    {
                        // This is [reg + dsp]" -- does the offset fit in a byte?
                        if (dspInByte)
                        {
                            dst += emitOutputByte(dst, code | 0x40);
                            dst += emitOutputByte(dst, dsp);
                        }
                        else
                        {
                            dst += emitOutputByte(dst, code | 0x80);
                            dst += emitOutputLong(dst, dsp);
                            if (id->idIsDspReloc())
                            {
                                emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                            }
                        }
                    }
                }
                else
                {
                    // Put the register in the opcode
                    code |= insEncodeReg012(ins, reg, EA_PTRSIZE, nullptr) << 8;

                    // Is there a displacement?
                    if (dspIsZero)
                    {
                        // This is simply "[reg]"
                        dst += emitOutputWord(dst, code);
                    }
                    else
                    {
                        // This is [reg + dsp]" -- does the offset fit in a byte?
                        if (dspInByte)
                        {
                            dst += emitOutputWord(dst, code | 0x4000);
                            dst += emitOutputByte(dst, dsp);
                        }
                        else
                        {
                            dst += emitOutputWord(dst, code | 0x8000);
                            dst += emitOutputLong(dst, dsp);
                            if (id->idIsDspReloc())
                            {
                                emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                            }
                        }
                    }
                }

                break;
        }
    }
    else
    {
        unsigned regByte;

        // We have a scaled index operand
        unsigned mul = emitDecodeScale(id->idAddr()->iiaAddrMode.amScale);

        // Is the index operand scaled?
        if (mul > 1)
        {
            // Is there a base register?
            if (reg != REG_NA)
            {
                // The address is "[reg + {2/4/8} * rgx + icon]"
                regByte = insEncodeReg012(ins, reg, EA_PTRSIZE, nullptr) |
                          insEncodeReg345(ins, rgx, EA_PTRSIZE, nullptr) | insSSval(mul);

                if (Is4ByteSSE4OrAVXInstruction(ins))
                {
                    // Emit [ebp + {2/4/8} * rgz] as [ebp + {2/4/8} * rgx + 0]
                    if (dspIsZero && reg != REG_EBP)
                    {
                        // The address is "[reg + {2/4/8} * rgx]"
                        dst += emitOutputByte(dst, code | 0x04);
                        dst += emitOutputByte(dst, regByte);
                    }
                    else
                    {
                        // The address is "[reg + {2/4/8} * rgx + disp]"
                        if (dspInByte)
                        {
                            dst += emitOutputByte(dst, code | 0x44);
                            dst += emitOutputByte(dst, regByte);
                            dst += emitOutputByte(dst, dsp);
                        }
                        else
                        {
                            dst += emitOutputByte(dst, code | 0x84);
                            dst += emitOutputByte(dst, regByte);
                            dst += emitOutputLong(dst, dsp);
                            if (id->idIsDspReloc())
                            {
                                emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                            }
                        }
                    }
                }
                else
                {
                    // Emit [ebp + {2/4/8} * rgz] as [ebp + {2/4/8} * rgx + 0]
                    if (dspIsZero && reg != REG_EBP)
                    {
                        // The address is "[reg + {2/4/8} * rgx]"
                        dst += emitOutputWord(dst, code | 0x0400);
                        dst += emitOutputByte(dst, regByte);
                    }
                    else
                    {
                        // The address is "[reg + {2/4/8} * rgx + disp]"
                        if (dspInByte)
                        {
                            dst += emitOutputWord(dst, code | 0x4400);
                            dst += emitOutputByte(dst, regByte);
                            dst += emitOutputByte(dst, dsp);
                        }
                        else
                        {
                            dst += emitOutputWord(dst, code | 0x8400);
                            dst += emitOutputByte(dst, regByte);
                            dst += emitOutputLong(dst, dsp);
                            if (id->idIsDspReloc())
                            {
                                emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                            }
                        }
                    }
                }
            }
            else
            {
                // The address is "[{2/4/8} * rgx + icon]"
                regByte = insEncodeReg012(ins, REG_EBP, EA_PTRSIZE, nullptr) |
                          insEncodeReg345(ins, rgx, EA_PTRSIZE, nullptr) | insSSval(mul);

                if (Is4ByteSSE4OrAVXInstruction(ins))
                {
                    dst += emitOutputByte(dst, code | 0x04);
                }
                else
                {
                    dst += emitOutputWord(dst, code | 0x0400);
                }

                dst += emitOutputByte(dst, regByte);

                // Special case: jump through a jump table
                if (ins == INS_i_jmp)
                {
                    dsp += (size_t)emitConsBlock;
                }

                dst += emitOutputLong(dst, dsp);
                if (id->idIsDspReloc())
                {
                    emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                }
            }
        }
        else
        {
            // The address is "[reg+rgx+dsp]"
            regByte = insEncodeReg012(ins, reg, EA_PTRSIZE, nullptr) | insEncodeReg345(ins, rgx, EA_PTRSIZE, nullptr);

            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                if (dspIsZero && reg != REG_EBP)
                {
                    // This is [reg+rgx]"
                    dst += emitOutputByte(dst, code | 0x04);
                    dst += emitOutputByte(dst, regByte);
                }
                else
                {
                    // This is [reg+rgx+dsp]" -- does the offset fit in a byte?
                    if (dspInByte)
                    {
                        dst += emitOutputByte(dst, code | 0x44);
                        dst += emitOutputByte(dst, regByte);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputByte(dst, code | 0x84);
                        dst += emitOutputByte(dst, regByte);
                        dst += emitOutputLong(dst, dsp);
                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
            }
            else
            {
                if (dspIsZero && reg != REG_EBP)
                {
                    // This is [reg+rgx]"
                    dst += emitOutputWord(dst, code | 0x0400);
                    dst += emitOutputByte(dst, regByte);
                }
                else
                {
                    // This is [reg+rgx+dsp]" -- does the offset fit in a byte?
                    if (dspInByte)
                    {
                        dst += emitOutputWord(dst, code | 0x4400);
                        dst += emitOutputByte(dst, regByte);
                        dst += emitOutputByte(dst, dsp);
                    }
                    else
                    {
                        dst += emitOutputWord(dst, code | 0x8400);
                        dst += emitOutputByte(dst, regByte);
                        dst += emitOutputLong(dst, dsp);
                        if (id->idIsDspReloc())
                        {
                            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)dsp, IMAGE_REL_BASED_HIGHLOW);
                        }
                    }
                }
            }
        }
    }

    // Now generate the constant value, if present
    if (addc)
    {
        ssize_t cval = addc->cnsVal;

#ifdef _TARGET_AMD64_
        // all these opcodes only take a sign-extended 4-byte immediate
        noway_assert(opsz < 8 || ((int)cval == cval && !addc->cnsReloc));
#endif

        switch (opsz)
        {
            case 0:
            case 4:
            case 8:
                dst += emitOutputLong(dst, cval);
                break;
            case 2:
                dst += emitOutputWord(dst, cval);
                break;
            case 1:
                dst += emitOutputByte(dst, cval);
                break;

            default:
                assert(!"unexpected operand size");
        }

        if (addc->cnsReloc)
        {
            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)(size_t)cval, IMAGE_REL_BASED_HIGHLOW);
            assert(opsz == 4);
        }
    }

DONE:

    // Does this instruction operate on a GC ref value?
    if (id->idGCref())
    {
        switch (id->idInsFmt())
        {
            case IF_ARD:
            case IF_AWR:
            case IF_ARW:
                break;

            case IF_RRD_ARD:
                break;

            case IF_RWR_ARD:
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_RRW_ARD:
                // Mark the destination register as holding a GCT_BYREF
                assert(id->idGCref() == GCT_BYREF && (ins == INS_add || ins == INS_sub));
                emitGCregLiveUpd(GCT_BYREF, id->idReg1(), dst);
                break;

            case IF_ARD_RRD:
            case IF_AWR_RRD:
                break;

            case IF_ARD_CNS:
            case IF_AWR_CNS:
                break;

            case IF_ARW_RRD:
            case IF_ARW_CNS:
                assert(id->idGCref() == GCT_BYREF && (ins == INS_add || ins == INS_sub));
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }

        // mul can never produce a GC ref
        assert(!instrIs3opImul(ins));
        assert(ins != INS_mulEAX && ins != INS_imulEAX);
    }
    else
    {
        if (!emitInsCanOnlyWriteSSE2OrAVXReg(id))
        {
            switch (id->idInsFmt())
            {
                case IF_RWR_ARD:
                    emitGCregDeadUpd(id->idReg1(), dst);
                    break;
                default:
                    break;
            }

            if (ins == INS_mulEAX || ins == INS_imulEAX)
            {
                emitGCregDeadUpd(REG_EAX, dst);
                emitGCregDeadUpd(REG_EDX, dst);
            }

            // For the three operand imul instruction the target register
            // is encoded in the opcode

            if (instrIs3opImul(ins))
            {
                regNumber tgtReg = inst3opImulReg(ins);
                emitGCregDeadUpd(tgtReg, dst);
            }
        }
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction involving a stack frame value.
 */

BYTE* emitter::emitOutputSV(BYTE* dst, instrDesc* id, code_t code, CnsVal* addc)
{
    int  adr;
    int  dsp;
    bool EBPbased;
    bool dspInByte;
    bool dspIsZero;

    instruction ins  = id->idIns();
    emitAttr    size = id->idOpSize();
    size_t      opsz = EA_SIZE_IN_BYTES(size);

    assert(ins != INS_imul || id->idReg1() == REG_EAX || size == EA_4BYTE || size == EA_8BYTE);

    // Is there a large constant operand?
    if (addc && (size > EA_1BYTE))
    {
        ssize_t cval = addc->cnsVal;

        // Does the constant fit in a byte?
        if ((signed char)cval == cval && addc->cnsReloc == false && ins != INS_mov && ins != INS_test)
        {
            if (id->idInsFmt() != IF_SRW_SHF)
            {
                code |= 2;
            }

            opsz = 1;
        }
    }

    // Add VEX prefix if required.
    // There are some callers who already add VEX prefix and call this routine.
    // Therefore, add VEX prefix is one is not already present.
    code = AddVexPrefixIfNeededAndNotPresent(ins, code, size);

    // Compute the REX prefix
    if (TakesRexWPrefix(ins, size))
    {
        code = AddRexWPrefix(ins, code);
    }

    // Special case emitting AVX instructions
    if (Is4ByteSSE4OrAVXInstruction(ins))
    {
        unsigned regcode = insEncodeReg345(ins, id->idReg1(), size, &code);
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        if (UseVEXEncoding())
        {
            // Emit last opcode byte
            // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
            assert((code & 0xFF) == 0);
            dst += emitOutputByte(dst, (code >> 8) & 0xFF);
        }
        else
        {
            dst += emitOutputWord(dst, code >> 16);
            dst += emitOutputWord(dst, code & 0xFFFF);
        }

        code = regcode;
    }
    // Is this a 'big' opcode?
    else if (code & 0xFF000000)
    {
        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Output the highest word of the opcode
        // We need to check again because in case of AVX instructions the leading
        // escape byte(s) (e.g. 0x0F) will be encoded as part of VEX prefix.
        if (code & 0xFF000000)
        {
            dst += emitOutputWord(dst, code >> 16);
            code &= 0x0000FFFF;
        }
    }
    else if (code & 0x00FF0000)
    {
        // BT supports 16 bit operands and this code doesn't add the necessary 66 prefix.
        assert(ins != INS_bt);

        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Output the highest byte of the opcode.
        // We need to check again because in case of AVX instructions the leading
        // escape byte(s) (e.g. 0x0F) will be encoded as part of VEX prefix.
        if (code & 0x00FF0000)
        {
            dst += emitOutputByte(dst, code >> 16);
            code &= 0x0000FFFF;
        }

        // Use the large version if this is not a byte
        if ((size != EA_1BYTE) && (ins != INS_imul) && (!insIsCMOV(ins)) && !IsSSE2Instruction(ins) &&
            !IsAVXInstruction(ins))
        {
            code |= 0x1;
        }
    }
    else if (CodeGen::instIsFP(ins))
    {
        assert(size == EA_4BYTE || size == EA_8BYTE);

        if (size == EA_8BYTE)
        {
            code += 4;
        }
    }
    else if (!IsSSE2Instruction(ins) && !IsAVXInstruction(ins))
    {
        // Is the operand size larger than a byte?
        switch (size)
        {
            case EA_1BYTE:
                break;

            case EA_2BYTE:
                // Output a size prefix for a 16-bit operand
                dst += emitOutputByte(dst, 0x66);
                __fallthrough;

            case EA_4BYTE:
#ifdef _TARGET_AMD64_
            case EA_8BYTE:
#endif // _TARGET_AMD64_

                /* Set the 'w' size bit to indicate 32-bit operation
                 * Note that incrementing "code" for INS_call (0xFF) would
                 * overflow, whereas setting the lower bit to 1 just works out
                 */

                code |= 0x01;
                break;

#ifdef _TARGET_X86_
            case EA_8BYTE:

                // Double operand - set the appropriate bit.
                // I don't know what a legitimate reason to end up in this case would be
                // considering that FP is taken care of above...
                // what is an instruction that takes a double which is not covered by the
                // above instIsFP? Of the list in instrsxarch, only INS_fprem
                code |= 0x04;
                NO_WAY("bad 8 byte op");
                break;
#endif // _TARGET_X86_

            default:
                NO_WAY("unexpected size");
                break;
        }
    }

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    // Figure out the variable's frame position
    int varNum = id->idAddr()->iiaLclVar.lvaVarNum();

    adr = emitComp->lvaFrameAddress(varNum, &EBPbased);
    dsp = adr + id->idAddr()->iiaLclVar.lvaOffset();

    dspInByte = ((signed char)dsp == (int)dsp);
    dspIsZero = (dsp == 0);

    // for stack varaibles the dsp should never be a reloc
    assert(id->idIsDspReloc() == 0);

    if (EBPbased)
    {
        // EBP-based variable: does the offset fit in a byte?
        if (Is4ByteSSE4OrAVXInstruction(ins))
        {
            if (dspInByte)
            {
                dst += emitOutputByte(dst, code | 0x45);
                dst += emitOutputByte(dst, dsp);
            }
            else
            {
                dst += emitOutputByte(dst, code | 0x85);
                dst += emitOutputLong(dst, dsp);
            }
        }
        else
        {
            if (dspInByte)
            {
                dst += emitOutputWord(dst, code | 0x4500);
                dst += emitOutputByte(dst, dsp);
            }
            else
            {
                dst += emitOutputWord(dst, code | 0x8500);
                dst += emitOutputLong(dst, dsp);
            }
        }
    }
    else
    {

#if !FEATURE_FIXED_OUT_ARGS
        // Adjust the offset by the amount currently pushed on the CPU stack
        dsp += emitCurStackLvl;
#endif

        dspInByte = ((signed char)dsp == (int)dsp);
        dspIsZero = (dsp == 0);

        // Does the offset fit in a byte?
        if (Is4ByteSSE4OrAVXInstruction(ins))
        {
            if (dspInByte)
            {
                if (dspIsZero)
                {
                    dst += emitOutputByte(dst, code | 0x04);
                    dst += emitOutputByte(dst, 0x24);
                }
                else
                {
                    dst += emitOutputByte(dst, code | 0x44);
                    dst += emitOutputByte(dst, 0x24);
                    dst += emitOutputByte(dst, dsp);
                }
            }
            else
            {
                dst += emitOutputByte(dst, code | 0x84);
                dst += emitOutputByte(dst, 0x24);
                dst += emitOutputLong(dst, dsp);
            }
        }
        else
        {
            if (dspInByte)
            {
                if (dspIsZero)
                {
                    dst += emitOutputWord(dst, code | 0x0400);
                    dst += emitOutputByte(dst, 0x24);
                }
                else
                {
                    dst += emitOutputWord(dst, code | 0x4400);
                    dst += emitOutputByte(dst, 0x24);
                    dst += emitOutputByte(dst, dsp);
                }
            }
            else
            {
                dst += emitOutputWord(dst, code | 0x8400);
                dst += emitOutputByte(dst, 0x24);
                dst += emitOutputLong(dst, dsp);
            }
        }
    }

    // Now generate the constant value, if present
    if (addc)
    {
        ssize_t cval = addc->cnsVal;

#ifdef _TARGET_AMD64_
        // all these opcodes only take a sign-extended 4-byte immediate
        noway_assert(opsz < 8 || ((int)cval == cval && !addc->cnsReloc));
#endif

        switch (opsz)
        {
            case 0:
            case 4:
            case 8:
                dst += emitOutputLong(dst, cval);
                break;
            case 2:
                dst += emitOutputWord(dst, cval);
                break;
            case 1:
                dst += emitOutputByte(dst, cval);
                break;

            default:
                assert(!"unexpected operand size");
        }

        if (addc->cnsReloc)
        {
            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)(size_t)cval, IMAGE_REL_BASED_HIGHLOW);
            assert(opsz == 4);
        }
    }

    // Does this instruction operate on a GC ref value?
    if (id->idGCref())
    {
        // Factor in the sub-variable offset
        adr += AlignDown(id->idAddr()->iiaLclVar.lvaOffset(), TARGET_POINTER_SIZE);

        switch (id->idInsFmt())
        {
            case IF_SRD:
                // Read  stack                    -- no change
                break;

            case IF_SWR: // Stack Write (So we need to update GC live for stack var)
                // Write stack                    -- GC var may be born
                emitGCvarLiveUpd(adr, varNum, id->idGCref(), dst);
                break;

            case IF_SRD_CNS:
                // Read  stack                    -- no change
                break;

            case IF_SWR_CNS:
                // Write stack                    -- no change
                break;

            case IF_SRD_RRD:
            case IF_RRD_SRD:
                // Read  stack   , read  register -- no change
                break;

            case IF_RWR_SRD: // Register Write, Stack Read (So we need to update GC live for register)

                // Read  stack   , write register -- GC reg may be born
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_SWR_RRD: // Stack Write, Register Read (So we need to update GC live for stack var)
                // Read  register, write stack    -- GC var may be born
                emitGCvarLiveUpd(adr, varNum, id->idGCref(), dst);
                break;

            case IF_RRW_SRD: // Register Read/Write, Stack Read (So we need to update GC live for register)

                // reg could have been a GCREF as GCREF + int=BYREF
                //                             or BYREF+/-int=BYREF
                assert(id->idGCref() == GCT_BYREF && (ins == INS_add || ins == INS_sub));
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_SRW_CNS:
            case IF_SRW_RRD:
            // += -= of a byref, no change

            case IF_SRW:
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }
    }
    else
    {
        if (!emitInsCanOnlyWriteSSE2OrAVXReg(id))
        {
            switch (id->idInsFmt())
            {
                case IF_RWR_SRD: // Register Write, Stack Read
                case IF_RRW_SRD: // Register Read/Write, Stack Read
                    emitGCregDeadUpd(id->idReg1(), dst);
                    break;
                default:
                    break;
            }

            if (ins == INS_mulEAX || ins == INS_imulEAX)
            {
                emitGCregDeadUpd(REG_EAX, dst);
                emitGCregDeadUpd(REG_EDX, dst);
            }

            // For the three operand imul instruction the target register
            // is encoded in the opcode

            if (instrIs3opImul(ins))
            {
                regNumber tgtReg = inst3opImulReg(ins);
                emitGCregDeadUpd(tgtReg, dst);
            }
        }
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction with a static data member (class variable).
 */

BYTE* emitter::emitOutputCV(BYTE* dst, instrDesc* id, code_t code, CnsVal* addc)
{
    BYTE*                addr;
    CORINFO_FIELD_HANDLE fldh;
    ssize_t              offs;
    int                  doff;

    emitAttr    size      = id->idOpSize();
    size_t      opsz      = EA_SIZE_IN_BYTES(size);
    instruction ins       = id->idIns();
    bool        isMoffset = false;

    // Get hold of the field handle and offset
    fldh = id->idAddr()->iiaFieldHnd;
    offs = emitGetInsDsp(id);

    // Special case: mov reg, fs:[ddd]
    if (fldh == FLD_GLOBAL_FS)
    {
        dst += emitOutputByte(dst, 0x64);
    }

    // Compute VEX prefix
    // Some of its callers already add VEX prefix and then call this routine.
    // Therefore add VEX prefix is not already present.
    code = AddVexPrefixIfNeededAndNotPresent(ins, code, size);

    // Compute the REX prefix
    if (TakesRexWPrefix(ins, size))
    {
        code = AddRexWPrefix(ins, code);
    }

    // Is there a large constant operand?
    if (addc && (size > EA_1BYTE))
    {
        ssize_t cval = addc->cnsVal;
        // Does the constant fit in a byte?
        if ((signed char)cval == cval && addc->cnsReloc == false && ins != INS_mov && ins != INS_test)
        {
            if (id->idInsFmt() != IF_MRW_SHF)
            {
                code |= 2;
            }

            opsz = 1;
        }
    }
#ifdef _TARGET_X86_
    else
    {
        // Special case: "mov eax, [addr]" and "mov [addr], eax"
        // Amd64: this is one case where addr can be 64-bit in size.  This is
        // currently unused or not enabled on amd64 as it always uses RIP
        // relative addressing which results in smaller instruction size.
        if (ins == INS_mov && id->idReg1() == REG_EAX)
        {
            switch (id->idInsFmt())
            {
                case IF_RWR_MRD:

                    assert(code == (insCodeRM(ins) | (insEncodeReg345(ins, REG_EAX, EA_PTRSIZE, NULL) << 8) | 0x0500));

                    code &= ~((code_t)0xFFFFFFFF);
                    code |= 0xA0;
                    isMoffset = true;
                    break;

                case IF_MWR_RRD:

                    assert(code == (insCodeMR(ins) | (insEncodeReg345(ins, REG_EAX, EA_PTRSIZE, NULL) << 8) | 0x0500));

                    code &= ~((code_t)0xFFFFFFFF);
                    code |= 0xA2;
                    isMoffset = true;
                    break;

                default:
                    break;
            }
        }
    }
#endif //_TARGET_X86_

    // Special case emitting AVX instructions
    if (Is4ByteSSE4OrAVXInstruction(ins))
    {
        unsigned regcode = insEncodeReg345(ins, id->idReg1(), size, &code);
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        if (UseVEXEncoding())
        {
            // Emit last opcode byte
            // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
            assert((code & 0xFF) == 0);
            dst += emitOutputByte(dst, (code >> 8) & 0xFF);
        }
        else
        {
            dst += emitOutputWord(dst, code >> 16);
            dst += emitOutputWord(dst, code & 0xFFFF);
        }

        // Emit Mod,R/M byte
        dst += emitOutputByte(dst, regcode | 0x05);
        code = 0;
    }
    // Is this a 'big' opcode?
    else if (code & 0xFF000000)
    {
        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Output the highest word of the opcode.
        // Check again since AVX instructions encode leading opcode bytes as part of VEX prefix.
        if (code & 0xFF000000)
        {
            dst += emitOutputWord(dst, code >> 16);
        }
        code &= 0x0000FFFF;
    }
    else if (code & 0x00FF0000)
    {
        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        // Check again as VEX prefix would have encoded leading opcode byte
        if (code & 0x00FF0000)
        {
            dst += emitOutputByte(dst, code >> 16);
            code &= 0x0000FFFF;
        }

        if ((ins == INS_movsx || ins == INS_movzx || ins == INS_cmpxchg || ins == INS_xchg || ins == INS_xadd ||
             insIsCMOV(ins)) &&
            size != EA_1BYTE)
        {
            // movsx and movzx are 'big' opcodes but also have the 'w' bit
            code++;
        }
    }
    else if (CodeGen::instIsFP(ins))
    {
        assert(size == EA_4BYTE || size == EA_8BYTE);

        if (size == EA_8BYTE)
        {
            code += 4;
        }
    }
    else
    {
        // Is the operand size larger than a byte?
        switch (size)
        {
            case EA_1BYTE:
                break;

            case EA_2BYTE:
                // Output a size prefix for a 16-bit operand
                dst += emitOutputByte(dst, 0x66);
                __fallthrough;

            case EA_4BYTE:
#ifdef _TARGET_AMD64_
            case EA_8BYTE:
#endif
                // Set the 'w' bit to get the large version
                code |= 0x1;
                break;

#ifdef _TARGET_X86_
            case EA_8BYTE:
                // Double operand - set the appropriate bit
                code |= 0x04;
                break;
#endif // _TARGET_X86_

            default:
                assert(!"unexpected size");
        }
    }

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    if (code)
    {
        if (id->idInsFmt() == IF_MRD_OFF || id->idInsFmt() == IF_RWR_MRD_OFF || isMoffset)
        {
            dst += emitOutputByte(dst, code);
        }
        else
        {
            dst += emitOutputWord(dst, code);
        }
    }

    // Do we have a constant or a static data member?
    doff = Compiler::eeGetJitDataOffs(fldh);
    if (doff >= 0)
    {
        addr = emitConsBlock + doff;

        int byteSize = EA_SIZE_IN_BYTES(size);

        // this instruction has a fixed size (4) src.
        if (ins == INS_cvttss2si || ins == INS_cvtss2sd || ins == INS_vbroadcastss)
        {
            byteSize = 4;
        }
        // This has a fixed size (8) source.
        if (ins == INS_vbroadcastsd)
        {
            byteSize = 8;
        }

        // Check that the offset is properly aligned (i.e. the ddd in [ddd])
        assert((emitChkAlign == false) || (ins == INS_lea) || (((size_t)addr & (byteSize - 1)) == 0));
    }
    else
    {
        // Special case: mov reg, fs:[ddd] or mov reg, [ddd]
        if (jitStaticFldIsGlobAddr(fldh))
        {
            addr = nullptr;
        }
        else
        {
            addr = (BYTE*)emitComp->info.compCompHnd->getFieldAddress(fldh, nullptr);
            if (addr == nullptr)
            {
                NO_WAY("could not obtain address of static field");
            }
        }
    }

    BYTE* target = (addr + offs);

    if (!isMoffset)
    {
        INT32 addlDelta = 0;

        if (addc)
        {
            // It is of the form "ins [disp], immed"
            // For emitting relocation, we also need to take into account of the
            // additional bytes of code emitted for immed val.

            ssize_t cval = addc->cnsVal;

#ifdef _TARGET_AMD64_
            // all these opcodes only take a sign-extended 4-byte immediate
            noway_assert(opsz < 8 || ((int)cval == cval && !addc->cnsReloc));
#else
            noway_assert(opsz <= 4);
#endif

            switch (opsz)
            {
                case 0:
                case 4:
                case 8:
                    addlDelta = -4;
                    break;
                case 2:
                    addlDelta = -2;
                    break;
                case 1:
                    addlDelta = -1;
                    break;

                default:
                    assert(!"unexpected operand size");
                    unreached();
            }
        }

#ifdef _TARGET_AMD64_
        // All static field and data section constant accesses should be marked as relocatable
        noway_assert(id->idIsDspReloc());
        dst += emitOutputLong(dst, 0);
#else  //_TARGET_X86_
        dst += emitOutputLong(dst, (int)target);
#endif //_TARGET_X86_

        if (id->idIsDspReloc())
        {
            emitRecordRelocation((void*)(dst - sizeof(int)), target, IMAGE_REL_BASED_DISP32, 0, addlDelta);
        }
    }
    else
    {
#ifdef _TARGET_AMD64_
        // This code path should never be hit on amd64 since it always uses RIP relative addressing.
        // In future if ever there is a need to enable this special case, also enable the logic
        // that sets isMoffset to true on amd64.
        unreached();
#else //_TARGET_X86_

        dst += emitOutputSizeT(dst, (ssize_t)target);

        if (id->idIsDspReloc())
        {
            emitRecordRelocation((void*)(dst - TARGET_POINTER_SIZE), target, IMAGE_REL_BASED_MOFFSET);
        }

#endif //_TARGET_X86_
    }

    // Now generate the constant value, if present
    if (addc)
    {
        ssize_t cval = addc->cnsVal;

#ifdef _TARGET_AMD64_
        // all these opcodes only take a sign-extended 4-byte immediate
        noway_assert(opsz < 8 || ((int)cval == cval && !addc->cnsReloc));
#endif

        switch (opsz)
        {
            case 0:
            case 4:
            case 8:
                dst += emitOutputLong(dst, cval);
                break;
            case 2:
                dst += emitOutputWord(dst, cval);
                break;
            case 1:
                dst += emitOutputByte(dst, cval);
                break;

            default:
                assert(!"unexpected operand size");
        }
        if (addc->cnsReloc)
        {
            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)(size_t)cval, IMAGE_REL_BASED_HIGHLOW);
            assert(opsz == 4);
        }
    }

    // Does this instruction operate on a GC ref value?
    if (id->idGCref())
    {
        switch (id->idInsFmt())
        {
            case IF_MRD:
            case IF_MRW:
            case IF_MWR:
                break;

            case IF_RRD_MRD:
                break;

            case IF_RWR_MRD:
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_MRD_RRD:
            case IF_MWR_RRD:
            case IF_MRW_RRD:
                break;

            case IF_MRD_CNS:
            case IF_MWR_CNS:
            case IF_MRW_CNS:
                break;

            case IF_RRW_MRD:

                assert(id->idGCref() == GCT_BYREF);
                assert(ins == INS_add || ins == INS_sub);

                // Mark it as holding a GCT_BYREF
                emitGCregLiveUpd(GCT_BYREF, id->idReg1(), dst);
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }
    }
    else
    {
        if (!emitInsCanOnlyWriteSSE2OrAVXReg(id))
        {
            switch (id->idInsFmt())
            {
                case IF_RWR_MRD:
                    emitGCregDeadUpd(id->idReg1(), dst);
                    break;
                default:
                    break;
            }

            if (ins == INS_mulEAX || ins == INS_imulEAX)
            {
                emitGCregDeadUpd(REG_EAX, dst);
                emitGCregDeadUpd(REG_EDX, dst);
            }

            // For the three operand imul instruction the target register
            // is encoded in the opcode

            if (instrIs3opImul(ins))
            {
                regNumber tgtReg = inst3opImulReg(ins);
                emitGCregDeadUpd(tgtReg, dst);
            }
        }
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction with one register operand.
 */

BYTE* emitter::emitOutputR(BYTE* dst, instrDesc* id)
{
    code_t code;

    instruction ins  = id->idIns();
    regNumber   reg  = id->idReg1();
    emitAttr    size = id->idOpSize();

    // We would to update GC info correctly
    assert(!IsSSE2Instruction(ins));
    assert(!IsAVXInstruction(ins));

    // Get the 'base' opcode
    switch (ins)
    {
        case INS_inc:
        case INS_dec:

#ifdef _TARGET_AMD64_
            if (true)
#else
            if (size == EA_1BYTE)
#endif
            {
                assert(INS_inc_l == INS_inc + 1);
                assert(INS_dec_l == INS_dec + 1);

                // Can't use the compact form, use the long form
                ins = (instruction)(ins + 1);
                if (size == EA_2BYTE)
                {
                    // Output a size prefix for a 16-bit operand
                    dst += emitOutputByte(dst, 0x66);
                }

                code = insCodeRR(ins);
                if (size != EA_1BYTE)
                {
                    // Set the 'w' bit to get the large version
                    code |= 0x1;
                }

                if (TakesRexWPrefix(ins, size))
                {
                    code = AddRexWPrefix(ins, code);
                }

                // Register...
                unsigned regcode = insEncodeReg012(ins, reg, size, &code);

                // Output the REX prefix
                dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

                dst += emitOutputWord(dst, code | (regcode << 8));
            }
            else
            {
                if (size == EA_2BYTE)
                {
                    // Output a size prefix for a 16-bit operand
                    dst += emitOutputByte(dst, 0x66);
                }
                dst += emitOutputByte(dst, insCodeRR(ins) | insEncodeReg012(ins, reg, size, nullptr));
            }
            break;

        case INS_pop:
        case INS_pop_hide:
        case INS_push:
        case INS_push_hide:

            assert(size == EA_PTRSIZE);
            code = insEncodeOpreg(ins, reg, size);

            assert(!TakesVexPrefix(ins));
            assert(!TakesRexWPrefix(ins, size));

            // Output the REX prefix
            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

            dst += emitOutputByte(dst, code);
            break;

        case INS_seto:
        case INS_setno:
        case INS_setb:
        case INS_setae:
        case INS_sete:
        case INS_setne:
        case INS_setbe:
        case INS_seta:
        case INS_sets:
        case INS_setns:
        case INS_setpe:
        case INS_setpo:
        case INS_setl:
        case INS_setge:
        case INS_setle:
        case INS_setg:

            assert(id->idGCref() == GCT_NONE);
            assert(size == EA_1BYTE);

            code = insEncodeMRreg(ins, reg, EA_1BYTE, insCodeMR(ins));

            // Output the REX prefix
            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

            // We expect this to always be a 'big' opcode
            assert(code & 0x00FF0000);

            dst += emitOutputByte(dst, code >> 16);
            dst += emitOutputWord(dst, code & 0x0000FFFF);

            break;

        case INS_mulEAX:
        case INS_imulEAX:

            // Kill off any GC refs in EAX or EDX
            emitGCregDeadUpd(REG_EAX, dst);
            emitGCregDeadUpd(REG_EDX, dst);

            __fallthrough;

        default:

            assert(id->idGCref() == GCT_NONE);

            code = insEncodeMRreg(ins, reg, size, insCodeMR(ins));

            if (size != EA_1BYTE)
            {
                // Set the 'w' bit to get the large version
                code |= 0x1;

                if (size == EA_2BYTE)
                {
                    // Output a size prefix for a 16-bit operand
                    dst += emitOutputByte(dst, 0x66);
                }
            }

            code = AddVexPrefixIfNeeded(ins, code, size);

            if (TakesRexWPrefix(ins, size))
            {
                code = AddRexWPrefix(ins, code);
            }

            // Output the REX prefix
            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

            dst += emitOutputWord(dst, code);
            break;
    }

    // Are we writing the register? if so then update the GC information
    switch (id->idInsFmt())
    {
        case IF_RRD:
            break;
        case IF_RWR:
            if (id->idGCref())
            {
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
            }
            else
            {
                emitGCregDeadUpd(id->idReg1(), dst);
            }
            break;
        case IF_RRW:
        {
#ifdef DEBUG
            regMaskTP regMask = genRegMask(reg);
#endif
            if (id->idGCref())
            {
                // The reg must currently be holding either a gcref or a byref
                // and the instruction must be inc or dec
                assert(((emitThisGCrefRegs | emitThisByrefRegs) & regMask) &&
                       (ins == INS_inc || ins == INS_dec || ins == INS_inc_l || ins == INS_dec_l));
                assert(id->idGCref() == GCT_BYREF);
                // Mark it as holding a GCT_BYREF
                emitGCregLiveUpd(GCT_BYREF, id->idReg1(), dst);
            }
            else
            {
                // Can't use RRW to trash a GC ref.  It's OK for unverifiable code
                // to trash Byrefs.
                assert((emitThisGCrefRegs & regMask) == 0);
            }
        }
        break;
        default:
#ifdef DEBUG
            emitDispIns(id, false, false, false);
#endif
            assert(!"unexpected instruction format");
            break;
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction with two register operands.
 */

BYTE* emitter::emitOutputRR(BYTE* dst, instrDesc* id)
{
    code_t code;

    instruction ins  = id->idIns();
    regNumber   reg1 = id->idReg1();
    regNumber   reg2 = id->idReg2();
    emitAttr    size = id->idOpSize();

    // Get the 'base' opcode
    code = insCodeRM(ins);
    code = AddVexPrefixIfNeeded(ins, code, size);
    if (IsSSEOrAVXInstruction(ins))
    {
        code = insEncodeRMreg(ins, code);

        if (TakesRexWPrefix(ins, size))
        {
            code = AddRexWPrefix(ins, code);
        }
    }
    else if ((ins == INS_movsx) || (ins == INS_movzx) || (insIsCMOV(ins)))
    {
        code = insEncodeRMreg(ins, code) | (int)(size == EA_2BYTE);
#ifdef _TARGET_AMD64_

        assert((size < EA_4BYTE) || (insIsCMOV(ins)));
        if ((size == EA_8BYTE) || (ins == INS_movsx))
        {
            code = AddRexWPrefix(ins, code);
        }
    }
    else if (ins == INS_movsxd)
    {
        code = insEncodeRMreg(ins, code);

#endif // _TARGET_AMD64_
    }
#ifdef FEATURE_HW_INTRINSICS
    else if ((ins == INS_crc32) || (ins == INS_lzcnt) || (ins == INS_popcnt))
    {
        code = insEncodeRMreg(ins, code);
        if ((ins == INS_crc32) && (size > EA_1BYTE))
        {
            code |= 0x0100;
        }

        if (size == EA_2BYTE)
        {
            assert(ins == INS_crc32);
            dst += emitOutputByte(dst, 0x66);
        }
        else if (size == EA_8BYTE)
        {
            code = AddRexWPrefix(ins, code);
        }
    }
#endif // FEATURE_HW_INTRINSICS
    else
    {
        code = insEncodeMRreg(ins, insCodeMR(ins));

        if (ins != INS_test)
        {
            code |= 2;
        }

        switch (size)
        {
            case EA_1BYTE:
                noway_assert(RBM_BYTE_REGS & genRegMask(reg1));
                noway_assert(RBM_BYTE_REGS & genRegMask(reg2));
                break;

            case EA_2BYTE:
                // Output a size prefix for a 16-bit operand
                dst += emitOutputByte(dst, 0x66);
                __fallthrough;

            case EA_4BYTE:
                // Set the 'w' bit to get the large version
                code |= 0x1;
                break;

#ifdef _TARGET_AMD64_
            case EA_8BYTE:
                // TODO-AMD64-CQ: Better way to not emit REX.W when we don't need it
                // Don't need to zero out the high bits explicitly
                if ((ins != INS_xor) || (reg1 != reg2))
                {
                    code = AddRexWPrefix(ins, code);
                }

                // Set the 'w' bit to get the large version
                code |= 0x1;
                break;

#endif // _TARGET_AMD64_

            default:
                assert(!"unexpected size");
        }
    }

    unsigned regCode = insEncodeReg345(ins, reg1, size, &code);
    regCode |= insEncodeReg012(ins, reg2, size, &code);

    if (TakesVexPrefix(ins))
    {
        // In case of AVX instructions that take 3 operands, we generally want to encode reg1
        // as first source.  In this case, reg1 is both a source and a destination.
        // The exception is the "merge" 3-operand case, where we have a move instruction, such
        // as movss, and we want to merge the source with itself.
        //
        // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
        // now we use the single source as source1 and source2.
        if (IsDstDstSrcAVXInstruction(ins))
        {
            // encode source/dest operand reg in 'vvvv' bits in 1's complement form
            code = insEncodeReg3456(ins, reg1, size, code);
        }
        else if (IsDstSrcSrcAVXInstruction(ins))
        {
            // encode source operand reg in 'vvvv' bits in 1's complement form
            code = insEncodeReg3456(ins, reg2, size, code);
        }
    }

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    if (code & 0xFF000000)
    {
        // Output the highest word of the opcode
        dst += emitOutputWord(dst, code >> 16);
        code &= 0x0000FFFF;

        if (Is4ByteSSE4Instruction(ins))
        {
            // Output 3rd byte of the opcode
            dst += emitOutputByte(dst, code);
            code &= 0xFF00;
        }
    }
    else if (code & 0x00FF0000)
    {
        dst += emitOutputByte(dst, code >> 16);
        code &= 0x0000FFFF;
    }

    // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
    if ((code & 0xFF00) == 0xC000)
    {
        dst += emitOutputWord(dst, code | (regCode << 8));
    }
    else if ((code & 0xFF) == 0x00)
    {
        // This case happens for SSE4/AVX instructions only
        assert(IsAVXInstruction(ins) || IsSSE4Instruction(ins));

        dst += emitOutputByte(dst, (code >> 8) & 0xFF);
        dst += emitOutputByte(dst, (0xC0 | regCode));
    }
    else
    {
        dst += emitOutputWord(dst, code);
        dst += emitOutputByte(dst, (0xC0 | regCode));
    }

    // Does this instruction operate on a GC ref value?
    if (id->idGCref())
    {
        switch (id->idInsFmt())
        {
            case IF_RRD_RRD:
                break;

            case IF_RWR_RRD:

                if (emitSyncThisObjReg != REG_NA && emitIGisInProlog(emitCurIG) && reg2 == (int)REG_ARG_0)
                {
                    // We're relocating "this" in the prolog
                    assert(emitComp->lvaIsOriginalThisArg(0));
                    assert(emitComp->lvaTable[0].lvRegister);
                    assert(emitComp->lvaTable[0].lvRegNum == reg1);

                    if (emitFullGCinfo)
                    {
                        emitGCregLiveSet(id->idGCref(), genRegMask(reg1), dst, true);
                        break;
                    }
                    else
                    {
                        /* If emitFullGCinfo==false, the we don't use any
                           regPtrDsc's and so explictly note the location
                           of "this" in GCEncode.cpp
                         */
                    }
                }

                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_RRW_RRD:

                switch (id->idIns())
                {
                    /*
                        This must be one of the following cases:

                        xor reg, reg        to assign NULL

                        and r1 , r2         if (ptr1 && ptr2) ...
                        or  r1 , r2         if (ptr1 || ptr2) ...

                        add r1 , r2         to compute a normal byref
                        sub r1 , r2         to compute a strange byref (VC only)

                    */
                    case INS_xor:
                        assert(id->idReg1() == id->idReg2());
                        emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                        break;

                    case INS_or:
                    case INS_and:
                        emitGCregDeadUpd(id->idReg1(), dst);
                        break;

                    case INS_add:
                    case INS_sub:
                        assert(id->idGCref() == GCT_BYREF);

#ifdef DEBUG
                        regMaskTP regMask;
                        regMask = genRegMask(reg1) | genRegMask(reg2);

                        // r1/r2 could have been a GCREF as GCREF + int=BYREF
                        //                            or BYREF+/-int=BYREF
                        assert(((regMask & emitThisGCrefRegs) && (ins == INS_add)) ||
                               ((regMask & emitThisByrefRegs) && (ins == INS_add || ins == INS_sub)));
#endif
                        // Mark r1 as holding a byref
                        emitGCregLiveUpd(GCT_BYREF, id->idReg1(), dst);
                        break;

                    default:
#ifdef DEBUG
                        emitDispIns(id, false, false, false);
#endif
                        assert(!"unexpected GC reg update instruction");
                }

                break;

            case IF_RRW_RRW:
                // This must be "xchg reg1, reg2"
                assert(id->idIns() == INS_xchg);

                // If we got here, the GC-ness of the registers doesn't match, so we have to "swap" them in the GC
                // register pointer mask.

                GCtype gc1, gc2;

                gc1 = emitRegGCtype(reg1);
                gc2 = emitRegGCtype(reg2);

                if (gc1 != gc2)
                {
                    // Kill the GC-info about the GC registers

                    if (needsGC(gc1))
                    {
                        emitGCregDeadUpd(reg1, dst);
                    }

                    if (needsGC(gc2))
                    {
                        emitGCregDeadUpd(reg2, dst);
                    }

                    // Now, swap the info

                    if (needsGC(gc1))
                    {
                        emitGCregLiveUpd(gc1, reg2, dst);
                    }

                    if (needsGC(gc2))
                    {
                        emitGCregLiveUpd(gc2, reg1, dst);
                    }
                }
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }
    }
    else
    {
        if (!emitInsCanOnlyWriteSSE2OrAVXReg(id))
        {
            switch (id->idInsFmt())
            {
                case IF_RRD_CNS:
                    // INS_mulEAX can not be used with any of these formats
                    assert(ins != INS_mulEAX && ins != INS_imulEAX);

                    // For the three operand imul instruction the target
                    // register is encoded in the opcode

                    if (instrIs3opImul(ins))
                    {
                        regNumber tgtReg = inst3opImulReg(ins);
                        emitGCregDeadUpd(tgtReg, dst);
                    }
                    break;

                case IF_RWR_RRD:
                case IF_RRW_RRD:
                    // INS_movxmm2i writes to reg2.
                    if (ins == INS_mov_xmm2i)
                    {
                        emitGCregDeadUpd(id->idReg2(), dst);
                    }
                    else
                    {
                        emitGCregDeadUpd(id->idReg1(), dst);
                    }
                    break;

                default:
                    break;
            }
        }
    }

    return dst;
}

BYTE* emitter::emitOutputRRR(BYTE* dst, instrDesc* id)
{
    code_t code;

    instruction ins = id->idIns();
    assert(IsAVXInstruction(ins));
    assert(IsThreeOperandAVXInstruction(ins) || isAvxBlendv(ins));
    regNumber targetReg = id->idReg1();
    regNumber src1      = id->idReg2();
    regNumber src2      = id->idReg3();
    emitAttr  size      = id->idOpSize();

    code = insCodeRM(ins);
    code = AddVexPrefixIfNeeded(ins, code, size);
    code = insEncodeRMreg(ins, code);

    if (TakesRexWPrefix(ins, size))
    {
        code = AddRexWPrefix(ins, code);
    }

    unsigned regCode = insEncodeReg345(ins, targetReg, size, &code);
    regCode |= insEncodeReg012(ins, src2, size, &code);
    // encode source operand reg in 'vvvv' bits in 1's complement form
    code = insEncodeReg3456(ins, src1, size, code);

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    // Is this a 'big' opcode?
    if (code & 0xFF000000)
    {
        // Output the highest word of the opcode
        dst += emitOutputWord(dst, code >> 16);
        code &= 0x0000FFFF;
    }
    else if (code & 0x00FF0000)
    {
        dst += emitOutputByte(dst, code >> 16);
        code &= 0x0000FFFF;
    }

    // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
    if ((code & 0xFF00) == 0xC000)
    {
        dst += emitOutputWord(dst, code | (regCode << 8));
    }
    else if ((code & 0xFF) == 0x00)
    {
        // This case happens for AVX instructions only
        assert(IsAVXInstruction(ins));

        dst += emitOutputByte(dst, (code >> 8) & 0xFF);
        dst += emitOutputByte(dst, (0xC0 | regCode));
    }
    else
    {
        dst += emitOutputWord(dst, code);
        dst += emitOutputByte(dst, (0xC0 | regCode));
    }

    noway_assert(!id->idGCref());

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction with a register and constant operands.
 */

BYTE* emitter::emitOutputRI(BYTE* dst, instrDesc* id)
{
    code_t      code;
    emitAttr    size      = id->idOpSize();
    instruction ins       = id->idIns();
    regNumber   reg       = id->idReg1();
    ssize_t     val       = emitGetInsSC(id);
    bool        valInByte = ((signed char)val == val) && (ins != INS_mov) && (ins != INS_test);

    // BT reg,imm might be useful but it requires special handling of the immediate value
    // (it is always encoded in a byte). Let's not complicate things until this is needed.
    assert(ins != INS_bt);

    if (id->idIsCnsReloc())
    {
        valInByte = false; // relocs can't be placed in a byte
    }

    noway_assert(emitVerifyEncodable(ins, size, reg));

    if (IsSSEOrAVXInstruction(ins))
    {
        // Handle SSE2 instructions of the form "opcode reg, immed8"

        assert(id->idGCref() == GCT_NONE);
        assert(valInByte);
        // The left and right shifts use the same encoding, and are distinguished by the Reg/Opcode field.
        regNumber regOpcode;
        switch (ins)
        {
            case INS_psrldq:
                regOpcode = (regNumber)3;
                break;
            case INS_pslldq:
                regOpcode = (regNumber)7;
                break;
            case INS_psrld:
            case INS_psrlw:
            case INS_psrlq:
                regOpcode = (regNumber)2;
                break;
            case INS_pslld:
            case INS_psllw:
            case INS_psllq:
                regOpcode = (regNumber)6;
                break;
            case INS_psrad:
            case INS_psraw:
                regOpcode = (regNumber)4;
                break;
            default:
                assert(!"Invalid instruction for SSE2 instruction of the form: opcode reg, immed8");
                regOpcode = REG_NA;
                break;
        }

        // Get the 'base' opcode.
        code = insCodeMI(ins);
        code = AddVexPrefixIfNeeded(ins, code, size);
        code = insEncodeMIreg(ins, reg, size, code);
        assert(code & 0x00FF0000);
        if (TakesVexPrefix(ins))
        {
            // The 'vvvv' bits encode the destination register, which for this case (RI)
            // is the same as the source.
            code = insEncodeReg3456(ins, reg, size, code);
        }

        unsigned regcode = (insEncodeReg345(ins, regOpcode, size, &code) | insEncodeReg012(ins, reg, size, &code)) << 8;

        // Output the REX prefix
        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        if (code & 0xFF000000)
        {
            dst += emitOutputWord(dst, code >> 16);
        }
        else if (code & 0xFF0000)
        {
            dst += emitOutputByte(dst, code >> 16);
        }

        dst += emitOutputWord(dst, code | regcode);

        dst += emitOutputByte(dst, val);

        return dst;
    }

    // The 'mov' opcode is special
    if (ins == INS_mov)
    {
        code = insCodeACC(ins);
        assert(code < 0x100);

        code |= 0x08; // Set the 'w' bit
        unsigned regcode = insEncodeReg012(ins, reg, size, &code);
        code |= regcode;

        // This is INS_mov and will not take VEX prefix
        assert(!TakesVexPrefix(ins));

        if (TakesRexWPrefix(ins, size))
        {
            code = AddRexWPrefix(ins, code);
        }

        dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

        dst += emitOutputByte(dst, code);
        if (size == EA_4BYTE)
        {
            dst += emitOutputLong(dst, val);
        }
#ifdef _TARGET_AMD64_
        else
        {
            assert(size == EA_PTRSIZE);
            dst += emitOutputSizeT(dst, val);
        }
#endif

        if (id->idIsCnsReloc())
        {
            emitRecordRelocation((void*)(dst - (unsigned)EA_SIZE(size)), (void*)(size_t)val, IMAGE_REL_BASED_MOFFSET);
        }

        goto DONE;
    }

    // Decide which encoding is the shortest
    bool useSigned, useACC;

    if (reg == REG_EAX && !instrIs3opImul(ins))
    {
        if (size == EA_1BYTE || (ins == INS_test))
        {
            // For al, ACC encoding is always the smallest
            useSigned = false;
            useACC    = true;
        }
        else
        {
            /* For ax/eax, we avoid ACC encoding for small constants as we
             * can emit the small constant and have it sign-extended.
             * For big constants, the ACC encoding is better as we can use
             * the 1 byte opcode
             */

            if (valInByte)
            {
                // avoid using ACC encoding
                useSigned = true;
                useACC    = false;
            }
            else
            {
                useSigned = false;
                useACC    = true;
            }
        }
    }
    else
    {
        useACC = false;

        if (valInByte)
        {
            useSigned = true;
        }
        else
        {
            useSigned = false;
        }
    }

    // "test" has no 's' bit
    if (ins == INS_test)
    {
        useSigned = false;
    }

    // Get the 'base' opcode
    if (useACC)
    {
        assert(!useSigned);
        code = insCodeACC(ins);
    }
    else
    {
        assert(!useSigned || valInByte);

        // Some instructions (at least 'imul') do not have a
        // r/m, immed form, but do have a dstReg,srcReg,imm8 form.
        if (valInByte && useSigned && insNeedsRRIb(ins))
        {
            code = insEncodeRRIb(ins, reg, size);
        }
        else
        {
            code = insCodeMI(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeMIreg(ins, reg, size, code);
        }
    }

    switch (size)
    {
        case EA_1BYTE:
            break;

        case EA_2BYTE:
            // Output a size prefix for a 16-bit operand
            dst += emitOutputByte(dst, 0x66);
            __fallthrough;

        case EA_4BYTE:
            // Set the 'w' bit to get the large version
            code |= 0x1;
            break;

#ifdef _TARGET_AMD64_
        case EA_8BYTE:
            /* Set the 'w' bit to get the large version */
            /* and the REX.W bit to get the really large version */

            code = AddRexWPrefix(ins, code);
            code |= 0x1;
            break;
#endif

        default:
            assert(!"unexpected size");
    }

    // Output the REX prefix
    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

    // Does the value fit in a sign-extended byte?
    // Important!  Only set the 's' bit when we have a size larger than EA_1BYTE.
    // Note: A sign-extending immediate when (size == EA_1BYTE) is invalid in 64-bit mode.

    if (useSigned && (size > EA_1BYTE))
    {
        // We can just set the 's' bit, and issue an immediate byte

        code |= 0x2; // Set the 's' bit to use a sign-extended immediate byte.
        dst += emitOutputWord(dst, code);
        dst += emitOutputByte(dst, val);
    }
    else
    {
        // Can we use an accumulator (EAX) encoding?
        if (useACC)
        {
            dst += emitOutputByte(dst, code);
        }
        else
        {
            dst += emitOutputWord(dst, code);
        }

        switch (size)
        {
            case EA_1BYTE:
                dst += emitOutputByte(dst, val);
                break;
            case EA_2BYTE:
                dst += emitOutputWord(dst, val);
                break;
            case EA_4BYTE:
                dst += emitOutputLong(dst, val);
                break;
#ifdef _TARGET_AMD64_
            case EA_8BYTE:
                dst += emitOutputLong(dst, val);
                break;
#endif // _TARGET_AMD64_
            default:
                break;
        }

        if (id->idIsCnsReloc())
        {
            emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)(size_t)val, IMAGE_REL_BASED_HIGHLOW);
            assert(size == EA_4BYTE);
        }
    }

DONE:

    // Does this instruction operate on a GC ref value?
    if (id->idGCref())
    {
        switch (id->idInsFmt())
        {
            case IF_RRD_CNS:
                break;

            case IF_RWR_CNS:
                emitGCregLiveUpd(id->idGCref(), id->idReg1(), dst);
                break;

            case IF_RRW_CNS:
                assert(id->idGCref() == GCT_BYREF);

#ifdef DEBUG
                regMaskTP regMask;
                regMask = genRegMask(reg);
                // FIXNOW review the other places and relax the assert there too

                // The reg must currently be holding either a gcref or a byref
                // GCT_GCREF+int = GCT_BYREF, and GCT_BYREF+/-int = GCT_BYREF
                if (emitThisGCrefRegs & regMask)
                {
                    assert(ins == INS_add);
                }
                if (emitThisByrefRegs & regMask)
                {
                    assert(ins == INS_add || ins == INS_sub);
                }
#endif
                // Mark it as holding a GCT_BYREF
                emitGCregLiveUpd(GCT_BYREF, id->idReg1(), dst);
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }

        // mul can never produce a GC ref
        assert(!instrIs3opImul(ins));
        assert(ins != INS_mulEAX && ins != INS_imulEAX);
    }
    else
    {
        switch (id->idInsFmt())
        {
            case IF_RRD_CNS:
                // INS_mulEAX can not be used with any of these formats
                assert(ins != INS_mulEAX && ins != INS_imulEAX);

                // For the three operand imul instruction the target
                // register is encoded in the opcode

                if (instrIs3opImul(ins))
                {
                    regNumber tgtReg = inst3opImulReg(ins);
                    emitGCregDeadUpd(tgtReg, dst);
                }
                break;

            case IF_RRW_CNS:
            case IF_RWR_CNS:
                assert(!instrIs3opImul(ins));

                emitGCregDeadUpd(id->idReg1(), dst);
                break;

            default:
#ifdef DEBUG
                emitDispIns(id, false, false, false);
#endif
                assert(!"unexpected GC ref instruction format");
        }
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output an instruction with a constant operand.
 */

BYTE* emitter::emitOutputIV(BYTE* dst, instrDesc* id)
{
    code_t      code;
    instruction ins       = id->idIns();
    emitAttr    size      = id->idOpSize();
    ssize_t     val       = emitGetInsSC(id);
    bool        valInByte = ((signed char)val == val);

    // We would to update GC info correctly
    assert(!IsSSE2Instruction(ins));
    assert(!IsAVXInstruction(ins));

#ifdef _TARGET_AMD64_
    // all these opcodes take a sign-extended 4-byte immediate, max
    noway_assert(size < EA_8BYTE || ((int)val == val && !id->idIsCnsReloc()));
#endif

    if (id->idIsCnsReloc())
    {
        valInByte = false; // relocs can't be placed in a byte

        // Of these instructions only the push instruction can have reloc
        assert(ins == INS_push || ins == INS_push_hide);
    }

    switch (ins)
    {
        case INS_jge:
            assert((val >= -128) && (val <= 127));
            dst += emitOutputByte(dst, insCode(ins));
            dst += emitOutputByte(dst, val);
            break;

        case INS_loop:
            assert((val >= -128) && (val <= 127));
            dst += emitOutputByte(dst, insCodeMI(ins));
            dst += emitOutputByte(dst, val);
            break;

        case INS_ret:
            assert(val);
            dst += emitOutputByte(dst, insCodeMI(ins));
            dst += emitOutputWord(dst, val);
            break;

        case INS_push_hide:
        case INS_push:
            code = insCodeMI(ins);

            // Does the operand fit in a byte?
            if (valInByte)
            {
                dst += emitOutputByte(dst, code | 2);
                dst += emitOutputByte(dst, val);
            }
            else
            {
                if (TakesRexWPrefix(ins, size))
                {
                    code = AddRexWPrefix(ins, code);
                    dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);
                }

                dst += emitOutputByte(dst, code);
                dst += emitOutputLong(dst, val);
                if (id->idIsCnsReloc())
                {
                    emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)(size_t)val, IMAGE_REL_BASED_HIGHLOW);
                }
            }

            // Did we push a GC ref value?
            if (id->idGCref())
            {
#ifdef DEBUG
                printf("UNDONE: record GCref push [cns]\n");
#endif
            }

            break;

        default:
            assert(!"unexpected instruction");
    }

    return dst;
}

/*****************************************************************************
 *
 *  Output a local jump instruction.
 *  This function also handles non-jumps that have jump-like characteristics, like RIP-relative LEA of a label that
 *  needs to get bound to an actual address and processed by branch shortening.
 */

BYTE* emitter::emitOutputLJ(BYTE* dst, instrDesc* i)
{
    unsigned srcOffs;
    unsigned dstOffs;
    ssize_t  distVal;

    instrDescJmp* id  = (instrDescJmp*)i;
    instruction   ins = id->idIns();
    bool          jmp;
    bool          relAddr = true; // does the instruction use relative-addressing?

    // SSE2 doesnt make any sense here
    assert(!IsSSE2Instruction(ins));
    assert(!IsAVXInstruction(ins));

    size_t ssz;
    size_t lsz;

    switch (ins)
    {
        default:
            ssz = JCC_SIZE_SMALL;
            lsz = JCC_SIZE_LARGE;
            jmp = true;
            break;

        case INS_jmp:
            ssz = JMP_SIZE_SMALL;
            lsz = JMP_SIZE_LARGE;
            jmp = true;
            break;

        case INS_call:
            ssz = lsz = CALL_INST_SIZE;
            jmp       = false;
            break;

        case INS_push_hide:
        case INS_push:
            ssz = lsz = 5;
            jmp       = false;
            relAddr   = false;
            break;

        case INS_mov:
        case INS_lea:
            ssz = lsz = id->idCodeSize();
            jmp       = false;
            relAddr   = false;
            break;
    }

    // Figure out the distance to the target
    srcOffs = emitCurCodeOffs(dst);
    dstOffs = id->idAddr()->iiaIGlabel->igOffs;

    if (relAddr)
    {
        distVal = (ssize_t)(emitOffsetToPtr(dstOffs) - emitOffsetToPtr(srcOffs));
    }
    else
    {
        distVal = (ssize_t)emitOffsetToPtr(dstOffs);
    }

    if (dstOffs <= srcOffs)
    {
        // This is a backward jump - distance is known at this point
        CLANG_FORMAT_COMMENT_ANCHOR;

#if DEBUG_EMIT
        if (id->idDebugOnlyInfo()->idNum == (unsigned)INTERESTING_JUMP_NUM || INTERESTING_JUMP_NUM == 0)
        {
            size_t blkOffs = id->idjIG->igOffs;

            if (INTERESTING_JUMP_NUM == 0)
            {
                printf("[3] Jump %u:\n", id->idDebugOnlyInfo()->idNum);
            }
            printf("[3] Jump  block is at %08X - %02X = %08X\n", blkOffs, emitOffsAdj, blkOffs - emitOffsAdj);
            printf("[3] Jump        is at %08X - %02X = %08X\n", srcOffs, emitOffsAdj, srcOffs - emitOffsAdj);
            printf("[3] Label block is at %08X - %02X = %08X\n", dstOffs, emitOffsAdj, dstOffs - emitOffsAdj);
        }
#endif

        // Can we use a short jump?
        if (jmp && distVal - ssz >= (size_t)JMP_DIST_SMALL_MAX_NEG)
        {
            emitSetShortJump(id);
        }
    }
    else
    {
        // This is a  forward jump - distance will be an upper limit
        emitFwdJumps = true;

        // The target offset will be closer by at least 'emitOffsAdj', but only if this
        // jump doesn't cross the hot-cold boundary.
        if (!emitJumpCrossHotColdBoundary(srcOffs, dstOffs))
        {
            dstOffs -= emitOffsAdj;
            distVal -= emitOffsAdj;
        }

        // Record the location of the jump for later patching
        id->idjOffs = dstOffs;

        // Are we overflowing the id->idjOffs bitfield?
        if (id->idjOffs != dstOffs)
        {
            IMPL_LIMITATION("Method is too large");
        }

#if DEBUG_EMIT
        if (id->idDebugOnlyInfo()->idNum == (unsigned)INTERESTING_JUMP_NUM || INTERESTING_JUMP_NUM == 0)
        {
            size_t blkOffs = id->idjIG->igOffs;

            if (INTERESTING_JUMP_NUM == 0)
            {
                printf("[4] Jump %u:\n", id->idDebugOnlyInfo()->idNum);
            }
            printf("[4] Jump  block is at %08X\n", blkOffs);
            printf("[4] Jump        is at %08X\n", srcOffs);
            printf("[4] Label block is at %08X - %02X = %08X\n", dstOffs + emitOffsAdj, emitOffsAdj, dstOffs);
        }
#endif

        // Can we use a short jump?
        if (jmp && distVal - ssz <= (size_t)JMP_DIST_SMALL_MAX_POS)
        {
            emitSetShortJump(id);
        }
    }

    // Adjust the offset to emit relative to the end of the instruction
    if (relAddr)
    {
        distVal -= id->idjShort ? ssz : lsz;
    }

#ifdef DEBUG
    if (0 && emitComp->verbose)
    {
        size_t sz          = id->idjShort ? ssz : lsz;
        int    distValSize = id->idjShort ? 4 : 8;
        printf("; %s jump [%08X/%03u] from %0*X to %0*X: dist = %08XH\n", (dstOffs <= srcOffs) ? "Fwd" : "Bwd",
               emitComp->dspPtr(id), id->idDebugOnlyInfo()->idNum, distValSize, srcOffs + sz, distValSize, dstOffs,
               distVal);
    }
#endif

    // What size jump should we use?
    if (id->idjShort)
    {
        // Short jump
        assert(!id->idjKeepLong);
        assert(emitJumpCrossHotColdBoundary(srcOffs, dstOffs) == false);

        assert(JMP_SIZE_SMALL == JCC_SIZE_SMALL);
        assert(JMP_SIZE_SMALL == 2);

        assert(jmp);

        if (emitInstCodeSz(id) != JMP_SIZE_SMALL)
        {
            emitOffsAdj += emitInstCodeSz(id) - JMP_SIZE_SMALL;

#ifdef DEBUG
            if (emitComp->verbose)
            {
                printf("; NOTE: size of jump [%08X] mis-predicted\n", emitComp->dspPtr(id));
            }
#endif
        }

        dst += emitOutputByte(dst, insCode(ins));

        // For forward jumps, record the address of the distance value
        id->idjTemp.idjAddr = (distVal > 0) ? dst : nullptr;

        dst += emitOutputByte(dst, distVal);
    }
    else
    {
        code_t code;

        // Long  jump
        if (jmp)
        {
            // clang-format off
            assert(INS_jmp + (INS_l_jmp - INS_jmp) == INS_l_jmp);
            assert(INS_jo  + (INS_l_jmp - INS_jmp) == INS_l_jo);
            assert(INS_jb  + (INS_l_jmp - INS_jmp) == INS_l_jb);
            assert(INS_jae + (INS_l_jmp - INS_jmp) == INS_l_jae);
            assert(INS_je  + (INS_l_jmp - INS_jmp) == INS_l_je);
            assert(INS_jne + (INS_l_jmp - INS_jmp) == INS_l_jne);
            assert(INS_jbe + (INS_l_jmp - INS_jmp) == INS_l_jbe);
            assert(INS_ja  + (INS_l_jmp - INS_jmp) == INS_l_ja);
            assert(INS_js  + (INS_l_jmp - INS_jmp) == INS_l_js);
            assert(INS_jns + (INS_l_jmp - INS_jmp) == INS_l_jns);
            assert(INS_jpe + (INS_l_jmp - INS_jmp) == INS_l_jpe);
            assert(INS_jpo + (INS_l_jmp - INS_jmp) == INS_l_jpo);
            assert(INS_jl  + (INS_l_jmp - INS_jmp) == INS_l_jl);
            assert(INS_jge + (INS_l_jmp - INS_jmp) == INS_l_jge);
            assert(INS_jle + (INS_l_jmp - INS_jmp) == INS_l_jle);
            assert(INS_jg  + (INS_l_jmp - INS_jmp) == INS_l_jg);
            // clang-format on

            code = insCode((instruction)(ins + (INS_l_jmp - INS_jmp)));
        }
        else if (ins == INS_push || ins == INS_push_hide)
        {
            assert(insCodeMI(INS_push) == 0x68);
            code = 0x68;
        }
        else if (ins == INS_mov)
        {
            // Make it look like IF_SWR_CNS so that emitOutputSV emits the r/m32 for us
            insFormat tmpInsFmt   = id->idInsFmt();
            insGroup* tmpIGlabel  = id->idAddr()->iiaIGlabel;
            bool      tmpDspReloc = id->idIsDspReloc();

            id->idInsFmt(IF_SWR_CNS);
            id->idAddr()->iiaLclVar = ((instrDescLbl*)id)->dstLclVar;
            id->idSetIsDspReloc(false);

            dst = emitOutputSV(dst, id, insCodeMI(ins));

            // Restore id fields with original values
            id->idInsFmt(tmpInsFmt);
            id->idAddr()->iiaIGlabel = tmpIGlabel;
            id->idSetIsDspReloc(tmpDspReloc);
            code = 0xCC;
        }
        else if (ins == INS_lea)
        {
            // Make an instrDesc that looks like IF_RWR_ARD so that emitOutputAM emits the r/m32 for us.
            // We basically are doing what emitIns_R_AI does.
            // TODO-XArch-Cleanup: revisit this.
            instrDescAmd  idAmdStackLocal;
            instrDescAmd* idAmd = &idAmdStackLocal;
            *(instrDesc*)idAmd  = *(instrDesc*)id; // copy all the "core" fields
            memset((BYTE*)idAmd + sizeof(instrDesc), 0,
                   sizeof(instrDescAmd) - sizeof(instrDesc)); // zero out the tail that wasn't copied

            idAmd->idInsFmt(IF_RWR_ARD);
            idAmd->idAddr()->iiaAddrMode.amBaseReg = REG_NA;
            idAmd->idAddr()->iiaAddrMode.amIndxReg = REG_NA;
            emitSetAmdDisp(idAmd, distVal); // set the displacement
            idAmd->idSetIsDspReloc(id->idIsDspReloc());
            assert(emitGetInsAmdAny(idAmd) == distVal); // make sure "disp" is stored properly

            UNATIVE_OFFSET sz = emitInsSizeAM(idAmd, insCodeRM(ins));
            idAmd->idCodeSize(sz);

            code = insCodeRM(ins);
            code |= (insEncodeReg345(ins, id->idReg1(), EA_PTRSIZE, &code) << 8);

            dst = emitOutputAM(dst, idAmd, code, nullptr);

            code = 0xCC;

            // For forward jumps, record the address of the distance value
            // Hard-coded 4 here because we already output the displacement, as the last thing.
            id->idjTemp.idjAddr = (dstOffs > srcOffs) ? (dst - 4) : nullptr;

            // We're done
            return dst;
        }
        else
        {
            code = 0xE8;
        }

        if (ins != INS_mov)
        {
            dst += emitOutputByte(dst, code);

            if (code & 0xFF00)
            {
                dst += emitOutputByte(dst, code >> 8);
            }
        }

        // For forward jumps, record the address of the distance value
        id->idjTemp.idjAddr = (dstOffs > srcOffs) ? dst : nullptr;

        dst += emitOutputLong(dst, distVal);

#ifndef _TARGET_AMD64_ // all REL32 on AMD have to go through recordRelocation
        if (emitComp->opts.compReloc)
#endif
        {
            if (!relAddr)
            {
                emitRecordRelocation((void*)(dst - sizeof(INT32)), (void*)distVal, IMAGE_REL_BASED_HIGHLOW);
            }
            else if (emitJumpCrossHotColdBoundary(srcOffs, dstOffs))
            {
                assert(id->idjKeepLong);
                emitRecordRelocation((void*)(dst - sizeof(INT32)), dst + distVal, IMAGE_REL_BASED_REL32);
            }
        }
    }

    // Local calls kill all registers
    if (ins == INS_call && (emitThisGCrefRegs | emitThisByrefRegs))
    {
        emitGCregDeadUpdMask(emitThisGCrefRegs | emitThisByrefRegs, dst);
    }

    return dst;
}

/*****************************************************************************
 *
 *  Append the machine code corresponding to the given instruction descriptor
 *  to the code block at '*dp'; the base of the code block is 'bp', and 'ig'
 *  is the instruction group that contains the instruction. Updates '*dp' to
 *  point past the generated code, and returns the size of the instruction
 *  descriptor in bytes.
 */

#ifdef _PREFAST_
#pragma warning(push)
#pragma warning(disable : 21000) // Suppress PREFast warning about overly large function
#endif
size_t emitter::emitOutputInstr(insGroup* ig, instrDesc* id, BYTE** dp)
{
    assert(emitIssuing);

    BYTE*         dst           = *dp;
    size_t        sz            = sizeof(instrDesc);
    instruction   ins           = id->idIns();
    unsigned char callInstrSize = 0;

#ifdef DEBUG
    bool dspOffs = emitComp->opts.dspGCtbls;
#endif // DEBUG

    emitAttr size = id->idOpSize();

    assert(REG_NA == (int)REG_NA);

    assert(ins != INS_imul || size >= EA_4BYTE);                  // Has no 'w' bit
    assert(instrIs3opImul(id->idIns()) == 0 || size >= EA_4BYTE); // Has no 'w' bit

    VARSET_TP GCvars(VarSetOps::UninitVal());

    // What instruction format have we got?
    switch (id->idInsFmt())
    {
        code_t   code;
        unsigned regcode;
        int      args;
        CnsVal   cnsVal;

        BYTE* addr;
        bool  recCall;

        regMaskTP gcrefRegs;
        regMaskTP byrefRegs;

        /********************************************************************/
        /*                        No operands                               */
        /********************************************************************/
        case IF_NONE:
            // the loop alignment pseudo instruction
            if (ins == INS_align)
            {
                sz  = SMALL_IDSC_SIZE;
                dst = emitOutputNOP(dst, (-(int)(size_t)dst) & 0x0f);
                assert(((size_t)dst & 0x0f) == 0);
                break;
            }

            if (ins == INS_nop)
            {
                dst = emitOutputNOP(dst, id->idCodeSize());
                break;
            }

            // the cdq instruction kills the EDX register implicitly
            if (ins == INS_cdq)
            {
                emitGCregDeadUpd(REG_EDX, dst);
            }

            assert(id->idGCref() == GCT_NONE);

            code = insCodeMR(ins);

#ifdef _TARGET_AMD64_
            // Support only scalar AVX instructions and hence size is hard coded to 4-byte.
            code = AddVexPrefixIfNeeded(ins, code, EA_4BYTE);

            if (ins == INS_cdq && TakesRexWPrefix(ins, id->idOpSize()))
            {
                code = AddRexWPrefix(ins, code);
            }
            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);
#endif
            // Is this a 'big' opcode?
            if (code & 0xFF000000)
            {
                // The high word and then the low word
                dst += emitOutputWord(dst, code >> 16);
                code &= 0x0000FFFF;
                dst += emitOutputWord(dst, code);
            }
            else if (code & 0x00FF0000)
            {
                // The high byte and then the low word
                dst += emitOutputByte(dst, code >> 16);
                code &= 0x0000FFFF;
                dst += emitOutputWord(dst, code);
            }
            else if (code & 0xFF00)
            {
                // The 2 byte opcode
                dst += emitOutputWord(dst, code);
            }
            else
            {
                // The 1 byte opcode
                dst += emitOutputByte(dst, code);
            }

            break;

        /********************************************************************/
        /*                Simple constant, local label, method              */
        /********************************************************************/

        case IF_CNS:
            dst = emitOutputIV(dst, id);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_LABEL:
        case IF_RWR_LABEL:
        case IF_SWR_LABEL:
            assert(id->idGCref() == GCT_NONE);
            assert(id->idIsBound());

            // TODO-XArch-Cleanup: handle IF_RWR_LABEL in emitOutputLJ() or change it to emitOutputAM()?
            dst = emitOutputLJ(dst, id);
            sz  = (id->idInsFmt() == IF_SWR_LABEL ? sizeof(instrDescLbl) : sizeof(instrDescJmp));
            break;

        case IF_METHOD:
        case IF_METHPTR:
            // Assume we'll be recording this call
            recCall = true;

            // Get hold of the argument count and field Handle
            args = emitGetInsCDinfo(id);

            // Is this a "fat" call descriptor?
            if (id->idIsLargeCall())
            {
                instrDescCGCA* idCall = (instrDescCGCA*)id;
                gcrefRegs             = idCall->idcGcrefRegs;
                byrefRegs             = idCall->idcByrefRegs;
                VarSetOps::Assign(emitComp, GCvars, idCall->idcGCvars);
                sz = sizeof(instrDescCGCA);
            }
            else
            {
                assert(!id->idIsLargeDsp());
                assert(!id->idIsLargeCns());

                gcrefRegs = emitDecodeCallGCregs(id);
                byrefRegs = 0;
                VarSetOps::AssignNoCopy(emitComp, GCvars, VarSetOps::MakeEmpty(emitComp));
                sz = sizeof(instrDesc);
            }

            addr = (BYTE*)id->idAddr()->iiaAddr;
            assert(addr != nullptr);

            // Some helpers don't get recorded in GC tables
            if (id->idIsNoGC())
            {
                recCall = false;
            }

            // What kind of a call do we have here?
            if (id->idInsFmt() == IF_METHPTR)
            {
                // This is call indirect via a method pointer

                code = insCodeMR(ins);
                if (ins == INS_i_jmp)
                {
                    code |= 1;
                }

                if (id->idIsDspReloc())
                {
                    dst += emitOutputWord(dst, code | 0x0500);
#ifdef _TARGET_AMD64_
                    dst += emitOutputLong(dst, 0);
#else
                    dst += emitOutputLong(dst, (int)addr);
#endif
                    emitRecordRelocation((void*)(dst - sizeof(int)), addr, IMAGE_REL_BASED_DISP32);
                }
                else
                {
#ifdef _TARGET_X86_
                    dst += emitOutputWord(dst, code | 0x0500);
#else  //_TARGET_AMD64_
                    // Amd64: addr fits within 32-bits and can be encoded as a displacement relative to zero.
                    // This addr mode should never be used while generating relocatable ngen code nor if
                    // the addr can be encoded as pc-relative address.
                    noway_assert(!emitComp->opts.compReloc);
                    noway_assert(codeGen->genAddrRelocTypeHint((size_t)addr) != IMAGE_REL_BASED_REL32);
                    noway_assert(static_cast<int>(reinterpret_cast<intptr_t>(addr)) == (ssize_t)addr);

                    // This requires, specifying a SIB byte after ModRM byte.
                    dst += emitOutputWord(dst, code | 0x0400);
                    dst += emitOutputByte(dst, 0x25);
#endif //_TARGET_AMD64_
                    dst += emitOutputLong(dst, static_cast<int>(reinterpret_cast<intptr_t>(addr)));
                }
                goto DONE_CALL;
            }

            // Else
            // This is call direct where we know the target, thus we can
            // use a direct call; the target to jump to is in iiaAddr.
            assert(id->idInsFmt() == IF_METHOD);

            // Output the call opcode followed by the target distance
            dst += (ins == INS_l_jmp) ? emitOutputByte(dst, insCode(ins)) : emitOutputByte(dst, insCodeMI(ins));

            ssize_t offset;
#ifdef _TARGET_AMD64_
            // All REL32 on Amd64 go through recordRelocation.  Here we will output zero to advance dst.
            offset = 0;
            assert(id->idIsDspReloc());
#else
            // Calculate PC relative displacement.
            // Although you think we should be using sizeof(void*), the x86 and x64 instruction set
            // only allow a 32-bit offset, so we correctly use sizeof(INT32)
            offset = addr - (dst + sizeof(INT32));
#endif

            dst += emitOutputLong(dst, offset);

            if (id->idIsDspReloc())
            {
                emitRecordRelocation((void*)(dst - sizeof(INT32)), addr, IMAGE_REL_BASED_REL32);
            }

        DONE_CALL:

            /* We update the GC info before the call as the variables cannot be
               used by the call. Killing variables before the call helps with
               boundary conditions if the call is CORINFO_HELP_THROW - see bug 50029.
               If we ever track aliased variables (which could be used by the
               call), we would have to keep them alive past the call.
             */
            assert(FitsIn<unsigned char>(dst - *dp));
            callInstrSize = static_cast<unsigned char>(dst - *dp);
            emitUpdateLiveGCvars(GCvars, *dp);

            // If the method returns a GC ref, mark EAX appropriately
            if (id->idGCref() == GCT_GCREF)
            {
                gcrefRegs |= RBM_EAX;
            }
            else if (id->idGCref() == GCT_BYREF)
            {
                byrefRegs |= RBM_EAX;
            }

#ifdef UNIX_AMD64_ABI
            // If is a multi-register return method is called, mark RDX appropriately (for System V AMD64).
            if (id->idIsLargeCall())
            {
                instrDescCGCA* idCall = (instrDescCGCA*)id;
                if (idCall->idSecondGCref() == GCT_GCREF)
                {
                    gcrefRegs |= RBM_RDX;
                }
                else if (idCall->idSecondGCref() == GCT_BYREF)
                {
                    byrefRegs |= RBM_RDX;
                }
            }
#endif // UNIX_AMD64_ABI

            // If the GC register set has changed, report the new set
            if (gcrefRegs != emitThisGCrefRegs)
            {
                emitUpdateLiveGCregs(GCT_GCREF, gcrefRegs, dst);
            }

            if (byrefRegs != emitThisByrefRegs)
            {
                emitUpdateLiveGCregs(GCT_BYREF, byrefRegs, dst);
            }

            if (recCall || args)
            {
                // For callee-pop, all arguments will be popped  after the call.
                // For caller-pop, any GC arguments will go dead after the call.

                assert(callInstrSize != 0);

                if (args >= 0)
                {
                    emitStackPop(dst, /*isCall*/ true, callInstrSize, args);
                }
                else
                {
                    emitStackKillArgs(dst, -args, callInstrSize);
                }
            }

            // Do we need to record a call location for GC purposes?
            if (!emitFullGCinfo && recCall)
            {
                assert(callInstrSize != 0);
                emitRecordGCcall(dst, callInstrSize);
            }

#ifdef DEBUG
            if (ins == INS_call)
            {
                emitRecordCallSite(emitCurCodeOffs(*dp), id->idDebugOnlyInfo()->idCallSig,
                                   (CORINFO_METHOD_HANDLE)id->idDebugOnlyInfo()->idMemCookie);
            }
#endif // DEBUG

            break;

        /********************************************************************/
        /*                      One register operand                        */
        /********************************************************************/

        case IF_RRD:
        case IF_RWR:
        case IF_RRW:
            dst = emitOutputR(dst, id);
            sz  = SMALL_IDSC_SIZE;
            break;

        /********************************************************************/
        /*                 Register and register/constant                   */
        /********************************************************************/

        case IF_RRW_SHF:
            code = insCodeMR(ins);
            // Emit the VEX prefix if it exists
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeMRreg(ins, id->idReg1(), size, code);

            // set the W bit
            if (size != EA_1BYTE)
            {
                code |= 1;
            }

            // Emit the REX prefix if it exists
            if (TakesRexWPrefix(ins, size))
            {
                code = AddRexWPrefix(ins, code);
            }

            // Output a size prefix for a 16-bit operand
            if (size == EA_2BYTE)
            {
                dst += emitOutputByte(dst, 0x66);
            }

            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);
            dst += emitOutputWord(dst, code);
            dst += emitOutputByte(dst, emitGetInsSC(id));
            sz = emitSizeOfInsDsc(id);

            // Update GC info.
            assert(!id->idGCref());
            emitGCregDeadUpd(id->idReg1(), dst);
            break;

        case IF_RRD_RRD:
        case IF_RWR_RRD:
        case IF_RRW_RRD:
        case IF_RRW_RRW:
            dst = emitOutputRR(dst, id);
            sz  = SMALL_IDSC_SIZE;
            break;

        case IF_RRD_CNS:
        case IF_RWR_CNS:
        case IF_RRW_CNS:
            dst = emitOutputRI(dst, id);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_RWR_RRD_RRD:
            dst = emitOutputRRR(dst, id);
            sz  = emitSizeOfInsDsc(id);
            break;
        case IF_RWR_RRD_RRD_CNS:
        case IF_RWR_RRD_RRD_RRD:
            dst = emitOutputRRR(dst, id);
            sz  = emitSizeOfInsDsc(id);
            dst += emitOutputByte(dst, emitGetInsSC(id));
            break;

        case IF_RRW_RRW_CNS:
            assert(id->idGCref() == GCT_NONE);

            // Get the 'base' opcode (it's a big one)
            // Also, determine which operand goes where in the ModRM byte.
            regNumber mReg;
            regNumber rReg;
            // if (ins == INS_shld || ins == INS_shrd || ins == INS_vextractf128 || ins == INS_vinsertf128)
            if (hasCodeMR(ins))
            {
                code = insCodeMR(ins);
                // Emit the VEX prefix if it exists
                code = AddVexPrefixIfNeeded(ins, code, size);
                code = insEncodeMRreg(ins, code);
                mReg = id->idReg1();
                rReg = id->idReg2();
            }
            else
            {
                code = insCodeRM(ins);
                // Emit the VEX prefix if it exists
                code = AddVexPrefixIfNeeded(ins, code, size);
                code = insEncodeRMreg(ins, code);
                mReg = id->idReg2();
                rReg = id->idReg1();
            }
            assert(code & 0x00FF0000);

            if (TakesRexWPrefix(ins, size))
            {
                code = AddRexWPrefix(ins, code);
            }

            if (TakesVexPrefix(ins))
            {
                if (IsDstDstSrcAVXInstruction(ins))
                {
                    // Encode source/dest operand reg in 'vvvv' bits in 1's complement form
                    // This code will have to change when we support 3 operands.
                    // For now, we always overload this source with the destination (always reg1).
                    // (Though we will need to handle the few ops that can have the 'vvvv' bits as destination,
                    // e.g. pslldq, when/if we support those instructions with 2 registers.)
                    // (see x64 manual Table 2-9. Instructions with a VEX.vvvv destination)
                    code = insEncodeReg3456(ins, id->idReg1(), size, code);
                }
                else if (IsDstSrcSrcAVXInstruction(ins))
                {
                    // This is a "merge" move instruction.
                    // Encode source operand reg in 'vvvv' bits in 1's complement form
                    code = insEncodeReg3456(ins, id->idReg2(), size, code);
                }
            }

            regcode = (insEncodeReg345(ins, rReg, size, &code) | insEncodeReg012(ins, mReg, size, &code));

            // Output the REX prefix
            dst += emitOutputRexOrVexPrefixIfNeeded(ins, dst, code);

            if (code & 0xFF000000)
            {
                // Output the highest word of the opcode
                dst += emitOutputWord(dst, code >> 16);
                code &= 0x0000FFFF;

                if (Is4ByteSSE4Instruction(ins))
                {
                    // Output 3rd byte of the opcode
                    dst += emitOutputByte(dst, code);
                    code &= 0xFF00;
                }
            }
            else if (code & 0x00FF0000)
            {
                dst += emitOutputByte(dst, code >> 16);
                code &= 0x0000FFFF;
            }

            // TODO-XArch-CQ: Right now support 4-byte opcode instructions only
            if ((code & 0xFF00) == 0xC000)
            {
                dst += emitOutputWord(dst, code | (regcode << 8));
            }
            else if ((code & 0xFF) == 0x00)
            {
                // This case happens for SSE4/AVX instructions only
                assert(IsAVXInstruction(ins) || IsSSE4Instruction(ins));

                dst += emitOutputByte(dst, (code >> 8) & 0xFF);
                dst += emitOutputByte(dst, (0xC0 | regcode));
            }
            else
            {
                dst += emitOutputWord(dst, code);
                dst += emitOutputByte(dst, (0xC0 | regcode));
            }

            dst += emitOutputByte(dst, emitGetInsSC(id));
            sz = emitSizeOfInsDsc(id);

            // Kill any GC ref in the destination register if necessary.
            if (!emitInsCanOnlyWriteSSE2OrAVXReg(id))
            {
                emitGCregDeadUpd(id->idReg1(), dst);
            }
            break;

        /********************************************************************/
        /*                      Address mode operand                        */
        /********************************************************************/

        case IF_ARD:
        case IF_AWR:
        case IF_ARW:

            dst = emitCodeWithInstructionSize(dst, emitOutputAM(dst, id, insCodeMR(ins)), &callInstrSize);

            switch (ins)
            {
                case INS_call:

                IND_CALL:
                    // Get hold of the argument count and method handle
                    args = emitGetInsCIargs(id);

                    // Is this a "fat" call descriptor?
                    if (id->idIsLargeCall())
                    {
                        instrDescCGCA* idCall = (instrDescCGCA*)id;

                        gcrefRegs = idCall->idcGcrefRegs;
                        byrefRegs = idCall->idcByrefRegs;
                        VarSetOps::Assign(emitComp, GCvars, idCall->idcGCvars);
                        sz = sizeof(instrDescCGCA);
                    }
                    else
                    {
                        assert(!id->idIsLargeDsp());
                        assert(!id->idIsLargeCns());

                        gcrefRegs = emitDecodeCallGCregs(id);
                        byrefRegs = 0;
                        VarSetOps::AssignNoCopy(emitComp, GCvars, VarSetOps::MakeEmpty(emitComp));
                        sz = sizeof(instrDesc);
                    }

                    recCall = true;

                    goto DONE_CALL;

                default:
                    sz = emitSizeOfInsDsc(id);
                    break;
            }
            break;

        case IF_RRW_ARD_CNS:
        case IF_RWR_ARD_CNS:
            emitGetInsAmdCns(id, &cnsVal);
            code = insCodeRM(ins);

            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputAM(dst, id, code, &cnsVal);
            }
            else
            {
                code    = AddVexPrefixIfNeeded(ins, code, size);
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputAM(dst, id, code | regcode, &cnsVal);
            }

            sz = emitSizeOfInsDsc(id);
            break;

        case IF_AWR_RRD_CNS:
            assert(ins == INS_vextracti128 || ins == INS_vextractf128);
            assert(UseVEXEncoding());
            emitGetInsAmdCns(id, &cnsVal);
            code = insCodeMR(ins);
            dst  = emitOutputAM(dst, id, code, &cnsVal);
            sz   = emitSizeOfInsDsc(id);
            break;

        case IF_RRD_ARD:
        case IF_RWR_ARD:
        case IF_RRW_ARD:
        case IF_RWR_RRD_ARD:
            code = insCodeRM(ins);
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputAM(dst, id, code);
            }
            else
            {
                code    = AddVexPrefixIfNeeded(ins, code, size);
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputAM(dst, id, code | regcode);
            }
            sz = emitSizeOfInsDsc(id);
            break;

        case IF_RWR_RRD_ARD_CNS:
        {
            emitGetInsAmdCns(id, &cnsVal);
            code = insCodeRM(ins);
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputAM(dst, id, code, &cnsVal);
            }
            else
            {
                code    = AddVexPrefixIfNeeded(ins, code, size);
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputAM(dst, id, code | regcode, &cnsVal);
            }
            sz = emitSizeOfInsDsc(id);
            break;
        }

        case IF_ARD_RRD:
        case IF_AWR_RRD:
        case IF_ARW_RRD:
            code    = insCodeMR(ins);
            code    = AddVexPrefixIfNeeded(ins, code, size);
            regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
            dst     = emitOutputAM(dst, id, code | regcode);
            sz      = emitSizeOfInsDsc(id);
            break;

        case IF_ARD_CNS:
        case IF_AWR_CNS:
        case IF_ARW_CNS:
            emitGetInsAmdCns(id, &cnsVal);
            dst = emitOutputAM(dst, id, insCodeMI(ins), &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_ARW_SHF:
            emitGetInsAmdCns(id, &cnsVal);
            dst = emitOutputAM(dst, id, insCodeMR(ins), &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        /********************************************************************/
        /*                      Stack-based operand                         */
        /********************************************************************/

        case IF_SRD:
        case IF_SWR:
        case IF_SRW:

            assert(ins != INS_pop_hide);
            if (ins == INS_pop)
            {
                // The offset in "pop [ESP+xxx]" is relative to the new ESP value
                CLANG_FORMAT_COMMENT_ANCHOR;

#if !FEATURE_FIXED_OUT_ARGS
                emitCurStackLvl -= sizeof(int);
#endif
                dst = emitOutputSV(dst, id, insCodeMR(ins));

#if !FEATURE_FIXED_OUT_ARGS
                emitCurStackLvl += sizeof(int);
#endif
                break;
            }

            dst = emitCodeWithInstructionSize(dst, emitOutputSV(dst, id, insCodeMR(ins)), &callInstrSize);

            if (ins == INS_call)
            {
                goto IND_CALL;
            }

            break;

        case IF_SRD_CNS:
        case IF_SWR_CNS:
        case IF_SRW_CNS:
            emitGetInsCns(id, &cnsVal);
            dst = emitOutputSV(dst, id, insCodeMI(ins), &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_SRW_SHF:
            emitGetInsCns(id, &cnsVal);
            dst = emitOutputSV(dst, id, insCodeMR(ins), &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_RRW_SRD_CNS:
        case IF_RWR_SRD_CNS:
            emitGetInsCns(id, &cnsVal);
            code = insCodeRM(ins);

            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputSV(dst, id, code, &cnsVal);
            }
            else
            {
                code = AddVexPrefixIfNeeded(ins, code, size);

                // In case of AVX instructions that take 3 operands, encode reg1 as first source.
                // Note that reg1 is both a source and a destination.
                //
                // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
                // now we use the single source as source1 and source2.
                // For this format, moves do not support a third operand, so we only need to handle the binary ops.
                if (IsDstDstSrcAVXInstruction(ins))
                {
                    // encode source operand reg in 'vvvv' bits in 1's complement form
                    code = insEncodeReg3456(ins, id->idReg1(), size, code);
                }

                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputSV(dst, id, code | regcode, &cnsVal);
            }

            sz = emitSizeOfInsDsc(id);
            break;

        case IF_RRD_SRD:
        case IF_RWR_SRD:
        case IF_RRW_SRD:
            code = insCodeRM(ins);

            // 4-byte AVX instructions are special cased inside emitOutputSV
            // since they do not have space to encode ModRM byte.
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputSV(dst, id, code);
            }
            else
            {
                code = AddVexPrefixIfNeeded(ins, code, size);

                if (IsDstDstSrcAVXInstruction(ins))
                {
                    // encode source operand reg in 'vvvv' bits in 1's complement form
                    code = insEncodeReg3456(ins, id->idReg1(), size, code);
                }

                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputSV(dst, id, code | regcode);
            }
            break;

        case IF_RWR_RRD_SRD:
        {
            // This should only be called on AVX instructions
            assert(IsAVXInstruction(ins));

            code = insCodeRM(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeReg3456(ins, id->idReg2(), size,
                                    code); // encode source operand reg in 'vvvv' bits in 1's complement form

            // 4-byte AVX instructions are special cased inside emitOutputSV
            // since they do not have space to encode ModRM byte.
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputSV(dst, id, code);
            }
            else
            {
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputSV(dst, id, code | regcode);
            }
            break;
        }

        case IF_RWR_RRD_SRD_CNS:
        {
            // This should only be called on AVX instructions
            assert(IsAVXInstruction(ins));
            emitGetInsCns(id, &cnsVal);

            code = insCodeRM(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeReg3456(ins, id->idReg2(), size,
                                    code); // encode source operand reg in 'vvvv' bits in 1's complement form

            // 4-byte AVX instructions are special cased inside emitOutputSV
            // since they do not have space to encode ModRM byte.
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputSV(dst, id, code, &cnsVal);
            }
            else
            {
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputSV(dst, id, code | regcode, &cnsVal);
            }
            break;
        }

        case IF_SRD_RRD:
        case IF_SWR_RRD:
        case IF_SRW_RRD:
            code = insCodeMR(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);

            // In case of AVX instructions that take 3 operands, encode reg1 as first source.
            // Note that reg1 is both a source and a destination.
            //
            // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
            // now we use the single source as source1 and source2.
            // For this format, moves do not support a third operand, so we only need to handle the binary ops.
            if (IsDstDstSrcAVXInstruction(ins))
            {
                // encode source operand reg in 'vvvv' bits in 1's complement form
                code = insEncodeReg3456(ins, id->idReg1(), size, code);
            }

            regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
            dst     = emitOutputSV(dst, id, code | regcode);
            break;

        /********************************************************************/
        /*                    Direct memory address                         */
        /********************************************************************/

        case IF_MRD:
        case IF_MRW:
        case IF_MWR:

            noway_assert(ins != INS_call);
            dst = emitOutputCV(dst, id, insCodeMR(ins) | 0x0500);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_MRD_OFF:
            dst = emitOutputCV(dst, id, insCodeMI(ins));
            break;

        case IF_RRW_MRD_CNS:
        case IF_RWR_MRD_CNS:
            emitGetInsDcmCns(id, &cnsVal);
            code = insCodeRM(ins);

            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputCV(dst, id, code, &cnsVal);
            }
            else
            {
                code = AddVexPrefixIfNeeded(ins, code, size);

                // In case of AVX instructions that take 3 operands, encode reg1 as first source.
                // Note that reg1 is both a source and a destination.
                //
                // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
                // now we use the single source as source1 and source2.
                // For this format, moves do not support a third operand, so we only need to handle the binary ops.
                if (IsDstDstSrcAVXInstruction(ins))
                {
                    // encode source operand reg in 'vvvv' bits in 1's complement form
                    code = insEncodeReg3456(ins, id->idReg1(), size, code);
                }

                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputCV(dst, id, code | regcode | 0x0500, &cnsVal);
            }

            sz = emitSizeOfInsDsc(id);
            break;

        case IF_MWR_RRD_CNS:
            assert(ins == INS_vextracti128 || ins == INS_vextractf128);
            assert(UseVEXEncoding());
            emitGetInsDcmCns(id, &cnsVal);
            code = insCodeMR(ins);
            // only AVX2 vextracti128 and AVX vextractf128 can reach this path,
            // they do not need VEX.vvvv to encode the register operand
            dst = emitOutputCV(dst, id, code, &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_RRD_MRD:
        case IF_RWR_MRD:
        case IF_RRW_MRD:
            code = insCodeRM(ins);
            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputCV(dst, id, code);
            }
            else
            {
                code = AddVexPrefixIfNeeded(ins, code, size);

                if (IsDstDstSrcAVXInstruction(ins))
                {
                    // encode source operand reg in 'vvvv' bits in 1's complement form
                    code = insEncodeReg3456(ins, id->idReg1(), size, code);
                }

                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputCV(dst, id, code | regcode | 0x0500);
            }
            sz = emitSizeOfInsDsc(id);
            break;

        case IF_RWR_RRD_MRD:
        {
            // This should only be called on AVX instructions
            assert(IsAVXInstruction(ins));

            code = insCodeRM(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeReg3456(ins, id->idReg2(), size,
                                    code); // encode source operand reg in 'vvvv' bits in 1's complement form

            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputCV(dst, id, code);
            }
            else
            {
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputCV(dst, id, code | regcode | 0x0500);
            }
            sz = emitSizeOfInsDsc(id);
            break;
        }

        case IF_RWR_RRD_MRD_CNS:
        {
            // This should only be called on AVX instructions
            assert(IsAVXInstruction(ins));
            emitGetInsCns(id, &cnsVal);

            code = insCodeRM(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);
            code = insEncodeReg3456(ins, id->idReg2(), size,
                                    code); // encode source operand reg in 'vvvv' bits in 1's complement form

            // Special case 4-byte AVX instructions
            if (Is4ByteSSE4OrAVXInstruction(ins))
            {
                dst = emitOutputCV(dst, id, code, &cnsVal);
            }
            else
            {
                regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
                dst     = emitOutputCV(dst, id, code | regcode | 0x0500, &cnsVal);
            }
            sz = emitSizeOfInsDsc(id);
            break;
        }

        case IF_RWR_MRD_OFF:
            code = insCode(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);

            // In case of AVX instructions that take 3 operands, encode reg1 as first source.
            // Note that reg1 is both a source and a destination.
            //
            // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
            // now we use the single source as source1 and source2.
            // For this format, moves do not support a third operand, so we only need to handle the binary ops.
            if (IsDstDstSrcAVXInstruction(ins))
            {
                // encode source operand reg in 'vvvv' bits in 1's complement form
                code = insEncodeReg3456(ins, id->idReg1(), size, code);
            }

            regcode = insEncodeReg012(id->idIns(), id->idReg1(), size, &code);
            dst     = emitOutputCV(dst, id, code | 0x30 | regcode);
            sz      = emitSizeOfInsDsc(id);
            break;

        case IF_MRD_RRD:
        case IF_MWR_RRD:
        case IF_MRW_RRD:
            code = insCodeMR(ins);
            code = AddVexPrefixIfNeeded(ins, code, size);

            // In case of AVX instructions that take 3 operands, encode reg1 as first source.
            // Note that reg1 is both a source and a destination.
            //
            // TODO-XArch-CQ: Eventually we need to support 3 operand instruction formats. For
            // now we use the single source as source1 and source2.
            // For this format, moves do not support a third operand, so we only need to handle the binary ops.
            if (IsDstDstSrcAVXInstruction(ins))
            {
                // encode source operand reg in 'vvvv' bits in 1's complement form
                code = insEncodeReg3456(ins, id->idReg1(), size, code);
            }

            regcode = (insEncodeReg345(ins, id->idReg1(), size, &code) << 8);
            dst     = emitOutputCV(dst, id, code | regcode | 0x0500);
            sz      = emitSizeOfInsDsc(id);
            break;

        case IF_MRD_CNS:
        case IF_MWR_CNS:
        case IF_MRW_CNS:
            emitGetInsDcmCns(id, &cnsVal);
            dst = emitOutputCV(dst, id, insCodeMI(ins) | 0x0500, &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        case IF_MRW_SHF:
            emitGetInsDcmCns(id, &cnsVal);
            dst = emitOutputCV(dst, id, insCodeMR(ins) | 0x0500, &cnsVal);
            sz  = emitSizeOfInsDsc(id);
            break;

        /********************************************************************/
        /*                            oops                                  */
        /********************************************************************/

        default:

#ifdef DEBUG
            printf("unexpected format %s\n", emitIfName(id->idInsFmt()));
            assert(!"don't know how to encode this instruction");
#endif
            break;
    }

    // Make sure we set the instruction descriptor size correctly
    assert(sz == emitSizeOfInsDsc(id));

#if !FEATURE_FIXED_OUT_ARGS
    bool updateStackLevel = !emitIGisInProlog(ig) && !emitIGisInEpilog(ig);

#if FEATURE_EH_FUNCLETS
    updateStackLevel = updateStackLevel && !emitIGisInFuncletProlog(ig) && !emitIGisInFuncletEpilog(ig);
#endif // FEATURE_EH_FUNCLETS

    // Make sure we keep the current stack level up to date
    if (updateStackLevel)
    {
        switch (ins)
        {
            case INS_push:
                // Please note: {INS_push_hide,IF_LABEL} is used to push the address of the
                // finally block for calling it locally for an op_leave.
                emitStackPush(dst, id->idGCref());
                break;

            case INS_pop:
                emitStackPop(dst, false, /*callInstrSize*/ 0, 1);
                break;

            case INS_sub:
                // Check for "sub ESP, icon"
                if (ins == INS_sub && id->idInsFmt() == IF_RRW_CNS && id->idReg1() == REG_ESP)
                {
                    assert((size_t)emitGetInsSC(id) < 0x00000000FFFFFFFFLL);
                    emitStackPushN(dst, (unsigned)(emitGetInsSC(id) / TARGET_POINTER_SIZE));
                }
                break;

            case INS_add:
                // Check for "add ESP, icon"
                if (ins == INS_add && id->idInsFmt() == IF_RRW_CNS && id->idReg1() == REG_ESP)
                {
                    assert((size_t)emitGetInsSC(id) < 0x00000000FFFFFFFFLL);
                    emitStackPop(dst, /*isCall*/ false, /*callInstrSize*/ 0,
                                 (unsigned)(emitGetInsSC(id) / TARGET_POINTER_SIZE));
                }
                break;

            default:
                break;
        }
    }

#endif // !FEATURE_FIXED_OUT_ARGS

    assert((int)emitCurStackLvl >= 0);

    // Only epilog "instructions" and some pseudo-instrs
    // are allowed not to generate any code

    assert(*dp != dst || emitInstHasNoCode(ins));

#ifdef DEBUG
    if (emitComp->opts.disAsm || emitComp->opts.dspEmit || emitComp->verbose)
    {
        emitDispIns(id, false, dspOffs, true, emitCurCodeOffs(*dp), *dp, (dst - *dp));
    }

    if (emitComp->compDebugBreak)
    {
        // set JitEmitPrintRefRegs=1 will print out emitThisGCrefRegs and emitThisByrefRegs
        // at the beginning of this method.
        if (JitConfig.JitEmitPrintRefRegs() != 0)
        {
            printf("Before emitOutputInstr for id->idDebugOnlyInfo()->idNum=0x%02x\n", id->idDebugOnlyInfo()->idNum);
            printf("  emitThisGCrefRegs(0x%p)=", emitComp->dspPtr(&emitThisGCrefRegs));
            printRegMaskInt(emitThisGCrefRegs);
            emitDispRegSet(emitThisGCrefRegs);
            printf("\n");
            printf("  emitThisByrefRegs(0x%p)=", emitComp->dspPtr(&emitThisByrefRegs));
            printRegMaskInt(emitThisByrefRegs);
            emitDispRegSet(emitThisByrefRegs);
            printf("\n");
        }

        // For example, set JitBreakEmitOutputInstr=a6 will break when this method is called for
        // emitting instruction a6, (i.e. IN00a6 in jitdump).
        if ((unsigned)JitConfig.JitBreakEmitOutputInstr() == id->idDebugOnlyInfo()->idNum)
        {
            assert(!"JitBreakEmitOutputInstr reached");
        }
    }
#endif

#ifdef TRANSLATE_PDB
    if (*dp != dst)
    {
        // only map instruction groups to instruction groups
        MapCode(id->idDebugOnlyInfo()->idilStart, *dp);
    }
#endif

    *dp = dst;

#ifdef DEBUG
    if (ins == INS_mulEAX || ins == INS_imulEAX)
    {
        // INS_mulEAX has implicit target of Edx:Eax. Make sure
        // that we detected this cleared its GC-status.

        assert(((RBM_EAX | RBM_EDX) & (emitThisGCrefRegs | emitThisByrefRegs)) == 0);
    }

    if (instrIs3opImul(ins))
    {
        // The target of the 3-operand imul is implicitly encoded. Make sure
        // that we detected the implicit register and cleared its GC-status.

        regMaskTP regMask = genRegMask(inst3opImulReg(ins));
        assert((regMask & (emitThisGCrefRegs | emitThisByrefRegs)) == 0);
    }
#endif

    return sz;
}
#ifdef _PREFAST_
#pragma warning(pop)
#endif

/*****************************************************************************/
/*****************************************************************************/

#endif // defined(_TARGET_XARCH_)
