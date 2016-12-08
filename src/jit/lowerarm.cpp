// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                           Lowering for ARM                                XX
XX                                                                           XX
XX  This encapsulates all the logic for lowering trees for the ARM           XX
XX  architecture.  For a more detailed view of what is lowering, please      XX
XX  take a look at Lower.cpp                                                 XX
XX                                                                           XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#ifndef LEGACY_BACKEND // This file is ONLY used for the RyuJIT backend that uses the linear scan register allocator

// The ARM backend is not yet implemented, so the methods here are all NYI.
// TODO-ARM-NYI: Lowering for ARM.
#ifdef _TARGET_ARM_

#include "jit.h"
#include "sideeffects.h"
#include "lower.h"
#include "lsra.h"

//------------------------------------------------------------------------
// LowerCast: Lower GT_CAST(srcType, DstType) nodes.
//
// Arguments:
//    tree - GT_CAST node to be lowered
//
// Return Value:
//    None.
//
// Notes:
//    Casts from small int type to float/double are transformed as follows:
//    GT_CAST(byte, float/double)     =   GT_CAST(GT_CAST(byte, int32), float/double)
//    GT_CAST(sbyte, float/double)    =   GT_CAST(GT_CAST(sbyte, int32), float/double)
//    GT_CAST(int16, float/double)    =   GT_CAST(GT_CAST(int16, int32), float/double)
//    GT_CAST(uint16, float/double)   =   GT_CAST(GT_CAST(uint16, int32), float/double)
//
//    Similarly casts from float/double to a smaller int type are transformed as follows:
//    GT_CAST(float/double, byte)     =   GT_CAST(GT_CAST(float/double, int32), byte)
//    GT_CAST(float/double, sbyte)    =   GT_CAST(GT_CAST(float/double, int32), sbyte)
//    GT_CAST(float/double, int16)    =   GT_CAST(GT_CAST(double/double, int32), int16)
//    GT_CAST(float/double, uint16)   =   GT_CAST(GT_CAST(double/double, int32), uint16)
//
//    Note that for the overflow conversions we still depend on helper calls and
//    don't expect to see them here.
//    i) GT_CAST(float/double, int type with overflow detection)

void Lowering::LowerCast(GenTree* tree)
{
    assert(tree->OperGet() == GT_CAST);

    JITDUMP("LowerCast for: ");
    DISPNODE(tree);
    JITDUMP("\n");

    GenTreePtr op1     = tree->gtOp.gtOp1;
    var_types  dstType = tree->CastToType();
    var_types  srcType = op1->TypeGet();
    var_types  tmpType = TYP_UNDEF;

    // TODO-ARM-Cleanup: Remove following NYI assertions.
    if (varTypeIsFloating(srcType))
    {
        NYI_ARM("Lowering for cast from float"); // Not tested yet.
        noway_assert(!tree->gtOverflow());
    }

    // Case of src is a small type and dst is a floating point type.
    if (varTypeIsSmall(srcType) && varTypeIsFloating(dstType))
    {
        NYI_ARM("Lowering for cast from small type to float"); // Not tested yet.
        // These conversions can never be overflow detecting ones.
        noway_assert(!tree->gtOverflow());
        tmpType = TYP_INT;
    }
    // case of src is a floating point type and dst is a small type.
    else if (varTypeIsFloating(srcType) && varTypeIsSmall(dstType))
    {
        NYI_ARM("Lowering for cast from float to small type"); // Not tested yet.
        tmpType = TYP_INT;
    }

    if (tmpType != TYP_UNDEF)
    {
        GenTreePtr tmp = comp->gtNewCastNode(tmpType, op1, tmpType);
        tmp->gtFlags |= (tree->gtFlags & (GTF_UNSIGNED | GTF_OVERFLOW | GTF_EXCEPT));

        tree->gtFlags &= ~GTF_UNSIGNED;
        tree->gtOp.gtOp1 = tmp;
        BlockRange().InsertAfter(op1, tmp);
    }
}

void Lowering::LowerRotate(GenTreePtr tree)
{
    NYI_ARM("ARM Lowering for ROL and ROR");
}

void Lowering::TreeNodeInfoInit(GenTree* tree)
{
    Compiler* compiler = comp;

    unsigned      kind         = tree->OperKind();
    TreeNodeInfo* info         = &(tree->gtLsraInfo);
    RegisterType  registerType = TypeGet(tree);

    JITDUMP("TreeNodeInfoInit for: ");
    DISPNODE(tree);
    JITDUMP("\n");

    switch (tree->OperGet())
    {
        // Please add new types below "default" case except cases which can be fit into "default" case.

        // TODO-ARM-Cleanup: These cases can be fit into "default" case after
        // other remaining types are implmented.
        case GT_IL_OFFSET:
            info->srcCount = 0;
            info->dstCount = 0;
            break;
        // TODO-ARM-Cleanup: These cases can be fit into "default" case after
        // other remaining types are implmented.
        case GT_CNS_INT:
            info->dstCount = (tree->TypeGet() == TYP_VOID) ? 0 : 1;
            if (kind & (GTK_CONST | GTK_LEAF))
            {
                info->srcCount = 0;
            }
            else if (kind & (GTK_SMPOP))
            {
                if (tree->gtGetOp2() != nullptr)
                {
                    info->srcCount = 2;
                }
                else
                {
                    info->srcCount = 1;
                }
            }
            else
            {
                unreached();
            }
            break;

        // TODO-ARM-Cleanup: Remove this "default" case, and replace above cases with
        // "default" after other operators are implemented.(see comments above)
        default:
            NYI("ARM TreeNodeInfoInit(default)");
            break;

        // Please add new types below
        case GT_RETURN:
            TreeNodeInfoInitReturn(tree);
            break;
    } // end switch (tree->OperGet())

    // We need to be sure that we've set info->srcCount and info->dstCount appropriately
    assert((info->dstCount < 2) || tree->IsMultiRegCall());
}
//------------------------------------------------------------------------
// TreeNodeInfoInitReturn: Set the NodeInfo for a GT_RETURN.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::TreeNodeInfoInitReturn(GenTree* tree)
{
    TreeNodeInfo* info     = &(tree->gtLsraInfo);
    LinearScan*   l        = m_lsra;
    Compiler*     compiler = comp;

    GenTree*  op1           = tree->gtGetOp1();
    regMaskTP useCandidates = RBM_NONE;

    info->srcCount = (tree->TypeGet() == TYP_VOID) ? 0 : 1;
    info->dstCount = 0;

    if (varTypeIsStruct(tree))
    {
        NYI_ARM("ARM TreeNodeInfoInitReturn for struct");
    }
    else
    {
        // Non-struct type return - determine useCandidates
        switch (tree->TypeGet())
        {
            case TYP_VOID:
                useCandidates = RBM_NONE;
                break;
            case TYP_FLOAT:
            case TYP_DOUBLE:
            case TYP_LONG: // We should consider register layout for TYP_LONG
                NYI_ARM("ARM TreeNodeInfoInitReturn for VOID, FLOAT, DOUBLE, LONG");
                break;
            default:
                useCandidates = RBM_INTRET;
                break;
        }
    }

    if (useCandidates != RBM_NONE)
    {
        op1->gtLsraInfo.setSrcCandidates(l, useCandidates);
    }
}

// returns true if the tree can use the read-modify-write memory instruction form
bool Lowering::isRMWRegOper(GenTreePtr tree)
{
    return false;
}

bool Lowering::IsCallTargetInRange(void* addr)
{
    return comp->codeGen->validImmForBL((ssize_t)addr);
}

// return true if the immediate can be folded into an instruction, for example small enough and non-relocatable
bool Lowering::IsContainableImmed(GenTree* parentNode, GenTree* childNode)
{
    if (varTypeIsFloating(parentNode->TypeGet()))
    {
        // TODO-ARM-Cleanup: not tested yet.
        NYI_ARM("ARM IsContainableImmed for floating point type");

        // We can contain a floating point 0.0 constant in a compare instruction
        switch (parentNode->OperGet())
        {
            default:
                return false;

            case GT_EQ:
            case GT_NE:
            case GT_LT:
            case GT_LE:
            case GT_GE:
            case GT_GT:
                if (childNode->IsIntegralConst(0))
                    return true;
                break;
        }
    }
    else
    {
        // Make sure we have an actual immediate
        if (!childNode->IsCnsIntOrI())
            return false;
        if (childNode->IsIconHandle() && comp->opts.compReloc)
            return false;

        ssize_t  immVal = childNode->gtIntCon.gtIconVal;
        emitAttr attr   = emitActualTypeSize(childNode->TypeGet());
        emitAttr size   = EA_SIZE(attr);

        switch (parentNode->OperGet())
        {
            default:
                return false;

            case GT_ADD:
            case GT_SUB:
                if (emitter::emitIns_valid_imm_for_add(immVal, INS_FLAGS_DONT_CARE))
                    return true;
                break;

            case GT_EQ:
            case GT_NE:
            case GT_LT:
            case GT_LE:
            case GT_GE:
            case GT_GT:
            case GT_AND:
            case GT_OR:
            case GT_XOR:
                if (emitter::emitIns_valid_imm_for_alu(immVal))
                    return true;
                break;

            case GT_STORE_LCL_VAR:
                // TODO-ARM-Cleanup: not tested yet
                NYI_ARM("ARM IsContainableImmed for GT_STORE_LCL_VAR");
                if (immVal == 0)
                    return true;
                break;
        }
    }

    return false;
}

#endif // _TARGET_ARM_

#endif // !LEGACY_BACKEND
