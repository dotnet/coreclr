// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX               Intel hardware intrinsic Code Generator                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/
#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#if FEATURE_HW_INTRINSICS

#include "emit.h"
#include "codegen.h"
#include "sideeffects.h"
#include "lower.h"
#include "gcinfo.h"
#include "gcinfoencoder.h"

struct HWIntrinsicInfo
{
    instruction ins[6]; // flt, dbl, i8, i16, i32, i64
    int         ival;   // expected to be 0-255, -1 indicates no ival
}

static const hwIntrinsicInfoArray[] = {
#define HARDWARE_INTRINSIC(id, name, isa, ins1, ins2, ins3, ins4, ins5, ins6, ival)                                    \
    {ins1, ins2, ins3, ins4, ins5, ins6, ival},
#include "hwintrinsiclistxarch.h"
};

//------------------------------------------------------------------------
// insOfHWIntrinsic: map named intrinsic value to its corresponding instruction
//
// Arguments:
//    intrinsic -- id of the intrinsic function.
//    baseType  -- the base type of the intrinsic (flt, dbl, i8, i16, i32, i64)
//
// Return Value:
//    instruction for the intrinsic
//
static instruction insOfHWIntrinsic(NamedIntrinsic intrinsicID, var_types baseType)
{
    assert(intrinsicID != NI_Illegal);
    assert(intrinsicID > NI_HW_INTRINSIC_START && intrinsicID < NI_HW_INTRINSIC_END);

    int index = -1;

    switch (baseType)
    {
        case TYP_BYTE:
        case TYP_UBYTE:
            index = 2;
            break;

        case TYP_SHORT:
        case TYP_USHORT:
            index = 3;
            break;

        case TYP_INT:
        case TYP_UINT:
            index = 4;
            break;

        case TYP_LONG:
        case TYP_ULONG:
            index = 5;
            break;

        case TYP_FLOAT:
            index = 0;
            break;

        case TYP_DOUBLE:
            index = 1;
            break;

        default:
            unreached();
            break;
    }

    instruction ins = hwIntrinsicInfoArray[intrinsicID - NI_HW_INTRINSIC_START - 1].ins[index];
    assert(ins != INS_invalid);
    return ins;
}

//------------------------------------------------------------------------
// ivalOfHWIntrinsic: map named intrinsic value to its corresponding ival
//
// Arguments:
//    intrinsic -- id of the intrinsic function.
//
// Return Value:
//    ival for the intrinsic
//
static int ivalOfHWIntrinsic(NamedIntrinsic intrinsicID)
{
    assert(intrinsicID != NI_Illegal);
    assert(intrinsicID > NI_HW_INTRINSIC_START && intrinsicID < NI_HW_INTRINSIC_END);

    int ival = hwIntrinsicInfoArray[intrinsicID - NI_HW_INTRINSIC_START - 1].ival;
    assert((int8_t)ival == ival);
    return ival;
}

void CodeGen::genHWIntrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    InstructionSet isa         = compiler->isaOfHWIntrinsic(intrinsicID);

    switch (isa)
    {
        case InstructionSet_SSE:
            genSSEIntrinsic(node);
            break;
        case InstructionSet_SSE2:
            genSSE2Intrinsic(node);
            break;
        case InstructionSet_SSE3:
            genSSE3Intrinsic(node);
            break;
        case InstructionSet_SSSE3:
            genSSSE3Intrinsic(node);
            break;
        case InstructionSet_SSE41:
            genSSE41Intrinsic(node);
            break;
        case InstructionSet_SSE42:
            genSSE42Intrinsic(node);
            break;
        case InstructionSet_AVX:
            genAVXIntrinsic(node);
            break;
        case InstructionSet_AVX2:
            genAVX2Intrinsic(node);
            break;
        case InstructionSet_AES:
            genAESIntrinsic(node);
            break;
        case InstructionSet_BMI1:
            genBMI1Intrinsic(node);
            break;
        case InstructionSet_BMI2:
            genBMI2Intrinsic(node);
            break;
        case InstructionSet_FMA:
            genFMAIntrinsic(node);
            break;
        case InstructionSet_LZCNT:
            genLZCNTIntrinsic(node);
            break;
        case InstructionSet_PCLMULQDQ:
            genPCLMULQDQIntrinsic(node);
            break;
        case InstructionSet_POPCNT:
            genPOPCNTIntrinsic(node);
            break;
        default:
            unreached();
            break;
    }
}

void CodeGen::genSSEIntrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    GenTree*       op1         = node->gtGetOp1();
    GenTree*       op2         = node->gtGetOp2();
    regNumber      targetReg   = node->gtRegNum;
    var_types      targetType  = node->TypeGet();
    var_types      baseType    = node->gtSIMDBaseType;

    instruction ins  = INS_invalid;
    emitter*    emit = getEmitter();
    genConsumeOperands(node);

    switch (intrinsicID)
    {
        case NI_SSE_Add:
            assert(ivalOfHWIntrinsic(intrinsicID) == -1);
            ins = insOfHWIntrinsic(intrinsicID, baseType);
            emit->emitIns_SIMD_R_R_R(ins, targetReg, op1->gtRegNum, op2->gtRegNum, TYP_SIMD16);
            break;

        default:
            unreached();
            break;
    }

    genProduceReg(node);
}

void CodeGen::genSSE2Intrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    GenTree*       op1         = node->gtGetOp1();
    GenTree*       op2         = node->gtGetOp2();
    regNumber      targetReg   = node->gtRegNum;
    var_types      targetType  = node->TypeGet();
    var_types      baseType    = node->gtSIMDBaseType;

    instruction ins  = INS_invalid;
    emitter*    emit = getEmitter();
    genConsumeOperands(node);

    switch (intrinsicID)
    {
        case NI_SSE2_Add:
            assert(ivalOfHWIntrinsic(intrinsicID) == -1);
            ins = insOfHWIntrinsic(intrinsicID, baseType);
            emit->emitIns_SIMD_R_R_R(ins, targetReg, op1->gtRegNum, op2->gtRegNum, TYP_SIMD16);
            break;

        default:
            unreached();
            break;
    }

    genProduceReg(node);
}

void CodeGen::genSSE3Intrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement SSE3 intrinsic code generation");
}

void CodeGen::genSSSE3Intrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement SSSE3 intrinsic code generation");
}

void CodeGen::genSSE41Intrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement SSE41 intrinsic code generation");
}

void CodeGen::genSSE42Intrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    GenTree*       op1         = node->gtGetOp1();
    GenTree*       op2         = node->gtGetOp2();
    regNumber      targetReg   = node->gtRegNum;
    var_types      targetType  = node->TypeGet();
    var_types      baseType    = node->gtSIMDBaseType;

    instruction ins = INS_invalid;
    genConsumeOperands(node);

    switch (intrinsicID)
    {
        case NI_SSE42_Crc32:
        {
            assert(ivalOfHWIntrinsic(intrinsicID) == -1);
            regNumber op1Reg = op1->gtRegNum;

            if (op1Reg != targetReg)
            {
                inst_RV_RV(INS_mov, targetReg, op1Reg, targetType, emitTypeSize(targetType));
            }

            ins = insOfHWIntrinsic(intrinsicID, baseType);
            inst_RV_RV(ins, targetReg, op2->gtRegNum, baseType, emitTypeSize(baseType));
            break;
        }

        default:
            unreached();
            break;
    }

    genProduceReg(node);
}

void CodeGen::genAVXIntrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    GenTree*       op1         = node->gtGetOp1();
    GenTree*       op2         = node->gtGetOp2();
    regNumber      targetReg   = node->gtRegNum;
    var_types      targetType  = node->TypeGet();
    var_types      baseType    = node->gtSIMDBaseType;

    instruction ins  = INS_invalid;
    emitter*    emit = getEmitter();
    genConsumeOperands(node);

    switch (intrinsicID)
    {
        case NI_AVX_Add:
            assert(ivalOfHWIntrinsic(intrinsicID) == -1);
            ins = insOfHWIntrinsic(intrinsicID, baseType);
            emit->emitIns_R_R_R(ins, emitTypeSize(TYP_SIMD32), targetReg, op1->gtRegNum, op2->gtRegNum);
            break;

        default:
            unreached();
            break;
    }

    genProduceReg(node);
}

void CodeGen::genAVX2Intrinsic(GenTreeHWIntrinsic* node)
{
    NamedIntrinsic intrinsicID = node->gtHWIntrinsicId;
    GenTree*       op1         = node->gtGetOp1();
    GenTree*       op2         = node->gtGetOp2();
    regNumber      targetReg   = node->gtRegNum;
    var_types      targetType  = node->TypeGet();
    var_types      baseType    = node->gtSIMDBaseType;

    instruction ins  = INS_invalid;
    emitter*    emit = getEmitter();

    genConsumeOperands(node);

    switch (intrinsicID)
    {
        case NI_AVX2_Add:
            assert(ivalOfHWIntrinsic(intrinsicID) == -1);
            ins = insOfHWIntrinsic(intrinsicID, baseType);
            emit->emitIns_R_R_R(ins, emitTypeSize(TYP_SIMD32), targetReg, op1->gtRegNum, op2->gtRegNum);
            break;

        default:
            unreached();
            break;
    }

    genProduceReg(node);
}

void CodeGen::genAESIntrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement AES intrinsic code generation");
}

void CodeGen::genBMI1Intrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement BMI1 intrinsic code generation");
}

void CodeGen::genBMI2Intrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement BMI2 intrinsic code generation");
}

void CodeGen::genFMAIntrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement FMA intrinsic code generation");
}

void CodeGen::genLZCNTIntrinsic(GenTreeHWIntrinsic* node)
{
    assert(node->gtHWIntrinsicId == NI_LZCNT_LeadingZeroCount);
    assert(node->gtRegNum != REG_NA);

    var_types targetType = node->TypeGet();
    genConsumeOperands(node);

    inst_RV_RV(INS_lzcnt, node->gtRegNum, node->gtGetOp1()->gtRegNum, targetType, emitTypeSize(targetType));
    genProduceReg(node);
}

void CodeGen::genPCLMULQDQIntrinsic(GenTreeHWIntrinsic* node)
{
    NYI("Implement PCLMULQDQ intrinsic code generation");
}

void CodeGen::genPOPCNTIntrinsic(GenTreeHWIntrinsic* node)
{
    assert(node->gtHWIntrinsicId == NI_POPCNT_PopCount);
    assert(node->gtRegNum != REG_NA);

    var_types targetType = node->TypeGet();
    genConsumeOperands(node);

    inst_RV_RV(INS_popcnt, node->gtRegNum, node->gtGetOp1()->gtRegNum, targetType, emitTypeSize(targetType));
    genProduceReg(node);
}

#endif // FEATURE_HW_INTRINSICS
