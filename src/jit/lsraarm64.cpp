// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                    Register Requirements for ARM64                        XX
XX                                                                           XX
XX  This encapsulates all the logic for setting register requirements for    XX
XX  the ARM64 architecture.                                                  XX
XX                                                                           XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#ifdef _TARGET_ARM64_

#include "jit.h"
#include "sideeffects.h"
#include "lower.h"

//------------------------------------------------------------------------
// BuildNode: Build the RefPositions for for a node
//
// Arguments:
//    treeNode - the node of interest
//
// Return Value:
//    The number of sources consumed by this node.
//
// Notes:
// Preconditions:
//    LSRA Has been initialized.
//
// Postconditions:
//    RefPositions have been built for all the register defs and uses required
//    for this node.
//
int LinearScan::BuildNode(GenTree* tree)
{
    assert(!tree->isContained());
    Interval* prefSrcInterval = nullptr;
    int       srcCount;
    int       dstCount      = 0;
    regMaskTP dstCandidates = RBM_NONE;
    regMaskTP killMask      = RBM_NONE;
    bool      isLocalDefUse = false;

    // Reset the build-related members of LinearScan.
    clearBuildState();

    RegisterType registerType = TypeGet(tree);

    // Set the default dstCount. This may be modified below.
    if (tree->IsValue())
    {
        dstCount = 1;
        if (tree->IsUnusedValue())
        {
            isLocalDefUse = true;
        }
    }
    else
    {
        dstCount = 0;
    }

    switch (tree->OperGet())
    {
        default:
            srcCount = BuildSimple(tree);
            break;

        case GT_LCL_VAR:
        case GT_LCL_FLD:
        {
            // We handle tracked variables differently from non-tracked ones.  If it is tracked,
            // we will simply add a use of the tracked variable at its parent/consumer.
            // Otherwise, for a use we need to actually add the appropriate references for loading
            // or storing the variable.
            //
            // A tracked variable won't actually get used until the appropriate ancestor tree node
            // is processed, unless this is marked "isLocalDefUse" because it is a stack-based argument
            // to a call or an orphaned dead node.
            //
            LclVarDsc* const varDsc = &compiler->lvaTable[tree->AsLclVarCommon()->gtLclNum];
            if (isCandidateVar(varDsc))
            {
                INDEBUG(dumpNodeInfo(tree, dstCandidates, 0, 1));
                return 0;
            }
            srcCount = 0;
#ifdef FEATURE_SIMD
            // Need an additional register to read upper 4 bytes of Vector3.
            if (tree->TypeGet() == TYP_SIMD12)
            {
                // We need an internal register different from targetReg in which 'tree' produces its result
                // because both targetReg and internal reg will be in use at the same time.
                buildInternalFloatRegisterDefForNode(tree, allSIMDRegs());
                setInternalRegsDelayFree = true;
                buildInternalRegisterUses();
            }
#endif
            BuildDef(tree);
        }
        break;

        case GT_STORE_LCL_FLD:
        case GT_STORE_LCL_VAR:
            srcCount = 1;
            assert(dstCount == 0);
            srcCount = BuildStoreLoc(tree->AsLclVarCommon());
            break;

        case GT_FIELD_LIST:
            // These should always be contained. We don't correctly allocate or
            // generate code for a non-contained GT_FIELD_LIST.
            noway_assert(!"Non-contained GT_FIELD_LIST");
            srcCount = 0;
            break;

        case GT_LIST:
        case GT_ARGPLACE:
        case GT_NO_OP:
        case GT_START_NONGC:
        case GT_PROF_HOOK:
            srcCount = 0;
            assert(dstCount == 0);
            break;

        case GT_CNS_DBL:
        {
            GenTreeDblCon* dblConst   = tree->AsDblCon();
            double         constValue = dblConst->gtDblCon.gtDconVal;

            if (emitter::emitIns_valid_imm_for_fmov(constValue))
            {
                // Directly encode constant to instructions.
            }
            else
            {
                // Reserve int to load constant from memory (IF_LARGELDC)
                buildInternalIntRegisterDefForNode(tree);
                buildInternalRegisterUses();
            }
        }
            __fallthrough;

        case GT_CNS_INT:
        {
            srcCount = 0;
            assert(dstCount == 1);
            RefPosition* def               = BuildDef(tree);
            def->getInterval()->isConstant = true;
        }
        break;

        case GT_BOX:
        case GT_COMMA:
        case GT_QMARK:
        case GT_COLON:
            srcCount = 0;
            assert(dstCount == 0);
            unreached();
            break;

        case GT_RETURN:
            srcCount = BuildReturn(tree);
            break;

        case GT_RETFILT:
            assert(dstCount == 0);
            if (tree->TypeGet() == TYP_VOID)
            {
                srcCount = 0;
            }
            else
            {
                assert(tree->TypeGet() == TYP_INT);
                srcCount = 1;
                BuildUse(tree->gtGetOp1(), RBM_INTRET);
            }
            break;

        case GT_NOP:
            // A GT_NOP is either a passthrough (if it is void, or if it has
            // a child), but must be considered to produce a dummy value if it
            // has a type but no child.
            srcCount = 0;
            if (tree->TypeGet() != TYP_VOID && tree->gtGetOp1() == nullptr)
            {
                assert(dstCount == 1);
                BuildDef(tree);
            }
            else
            {
                assert(dstCount == 0);
            }
            break;

        case GT_JTRUE:
            srcCount = 0;
            assert(dstCount == 0);
            break;

        case GT_JMP:
            srcCount = 0;
            assert(dstCount == 0);
            break;

        case GT_SWITCH:
            // This should never occur since switch nodes must not be visible at this
            // point in the JIT.
            srcCount = 0;
            noway_assert(!"Switch must be lowered at this point");
            break;

        case GT_JMPTABLE:
            srcCount = 0;
            assert(dstCount == 1);
            BuildDef(tree);
            break;

        case GT_SWITCH_TABLE:
            buildInternalIntRegisterDefForNode(tree);
            srcCount = BuildBinaryUses(tree->AsOp());
            assert(dstCount == 0);
            break;

        case GT_ASG:
            noway_assert(!"We should never hit any assignment operator in lowering");
            srcCount = 0;
            break;

        case GT_ADD:
        case GT_SUB:
            if (varTypeIsFloating(tree->TypeGet()))
            {
                // overflow operations aren't supported on float/double types.
                assert(!tree->gtOverflow());

                // No implicit conversions at this stage as the expectation is that
                // everything is made explicit by adding casts.
                assert(tree->gtGetOp1()->TypeGet() == tree->gtGetOp2()->TypeGet());
            }

            __fallthrough;

        case GT_AND:
        case GT_OR:
        case GT_XOR:
        case GT_LSH:
        case GT_RSH:
        case GT_RSZ:
        case GT_ROR:
            srcCount = BuildBinaryUses(tree->AsOp());
            assert(dstCount == 1);
            BuildDef(tree);
            break;

        case GT_RETURNTRAP:
            // this just turns into a compare of its child with an int
            // + a conditional call
            BuildUse(tree->gtGetOp1());
            srcCount = 1;
            assert(dstCount == 0);
            killMask = compiler->compHelperCallKillSet(CORINFO_HELP_STOP_FOR_GC);
            BuildDefsWithKills(tree, 0, RBM_NONE, killMask);
            break;

        case GT_MOD:
        case GT_UMOD:
            NYI_IF(varTypeIsFloating(tree->TypeGet()), "FP Remainder in ARM64");
            assert(!"Shouldn't see an integer typed GT_MOD node in ARM64");
            srcCount = 0;
            break;

        case GT_MUL:
            if (tree->gtOverflow())
            {
                // Need a register different from target reg to check for overflow.
                buildInternalIntRegisterDefForNode(tree);
                setInternalRegsDelayFree = true;
            }
            __fallthrough;

        case GT_DIV:
        case GT_MULHI:
        case GT_UDIV:
        {
            srcCount = BuildBinaryUses(tree->AsOp());
            buildInternalRegisterUses();
            assert(dstCount == 1);
            BuildDef(tree);
        }
        break;

        case GT_INTRINSIC:
        {
            noway_assert((tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Abs) ||
                         (tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Ceiling) ||
                         (tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Floor) ||
                         (tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Round) ||
                         (tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Sqrt));

            // Both operand and its result must be of the same floating point type.
            GenTree* op1 = tree->gtGetOp1();
            assert(varTypeIsFloating(op1));
            assert(op1->TypeGet() == tree->TypeGet());

            BuildUse(op1);
            srcCount = 1;
            assert(dstCount == 1);
            BuildDef(tree);
        }
        break;

#ifdef FEATURE_SIMD
        case GT_SIMD:
            srcCount = BuildSIMD(tree->AsSIMD());
            break;
#endif // FEATURE_SIMD

#ifdef FEATURE_HW_INTRINSICS
        case GT_HWIntrinsic:
            srcCount = BuildHWIntrinsic(tree->AsHWIntrinsic());
            break;
#endif // FEATURE_HW_INTRINSICS

        case GT_CAST:
        {
            // TODO-ARM64-CQ: Int-To-Int conversions - castOp cannot be a memory op and must have an assigned
            //                register.
            //         see CodeGen::genIntToIntCast()

            // Non-overflow casts to/from float/double are done using SSE2 instructions
            // and that allow the source operand to be either a reg or memop. Given the
            // fact that casts from small int to float/double are done as two-level casts,
            // the source operand is always guaranteed to be of size 4 or 8 bytes.
            var_types castToType = tree->CastToType();
            GenTree*  castOp     = tree->gtCast.CastOp();
            var_types castOpType = castOp->TypeGet();
            if (tree->gtFlags & GTF_UNSIGNED)
            {
                castOpType = genUnsignedType(castOpType);
            }

            // Some overflow checks need a temp reg

            Lowering::CastInfo castInfo;
            // Get information about the cast.
            Lowering::getCastDescription(tree, &castInfo);

            if (castInfo.requiresOverflowCheck)
            {
                var_types srcType = castOp->TypeGet();
                emitAttr  cmpSize = EA_ATTR(genTypeSize(srcType));

                // If we cannot store the comparisons in an immediate for either
                // comparing against the max or min value, then we will need to
                // reserve a temporary register.

                bool canStoreMaxValue = emitter::emitIns_valid_imm_for_cmp(castInfo.typeMax, cmpSize);
                bool canStoreMinValue = emitter::emitIns_valid_imm_for_cmp(castInfo.typeMin, cmpSize);

                if (!canStoreMaxValue || !canStoreMinValue)
                {
                    buildInternalIntRegisterDefForNode(tree);
                }
            }
            BuildUse(tree->gtGetOp1());
            srcCount = 1;
            buildInternalRegisterUses();
            assert(dstCount == 1);
            BuildDef(tree);
        }
        break;

        case GT_NEG:
        case GT_NOT:
            BuildUse(tree->gtGetOp1());
            srcCount = 1;
            assert(dstCount == 1);
            BuildDef(tree);
            break;

        case GT_EQ:
        case GT_NE:
        case GT_LT:
        case GT_LE:
        case GT_GE:
        case GT_GT:
        case GT_TEST_EQ:
        case GT_TEST_NE:
        case GT_JCMP:
            srcCount = BuildCmp(tree);
            break;

        case GT_CKFINITE:
            srcCount = 1;
            assert(dstCount == 1);
            buildInternalIntRegisterDefForNode(tree);
            BuildUse(tree->gtGetOp1());
            BuildDef(tree);
            buildInternalRegisterUses();
            break;

        case GT_CMPXCHG:
        {
            GenTreeCmpXchg* cmpXchgNode = tree->AsCmpXchg();
            srcCount                    = cmpXchgNode->gtOpComparand->isContained() ? 2 : 3;
            assert(dstCount == 1);

            buildInternalIntRegisterDefForNode(tree);

            // For ARMv8 exclusives the lifetime of the addr and data must be extended because
            // it may be used used multiple during retries
            RefPosition* locationUse = BuildUse(tree->gtCmpXchg.gtOpLocation);
            setDelayFree(locationUse);
            RefPosition* valueUse = BuildUse(tree->gtCmpXchg.gtOpValue);
            setDelayFree(valueUse);
            if (!cmpXchgNode->gtOpComparand->isContained())
            {
                RefPosition* comparandUse = BuildUse(tree->gtCmpXchg.gtOpComparand);
                setDelayFree(comparandUse);
            }

            // Internals may not collide with target
            setInternalRegsDelayFree = true;
            buildInternalRegisterUses();
            BuildDef(tree);
        }
        break;

        case GT_LOCKADD:
        case GT_XADD:
        case GT_XCHG:
        {
            assert(dstCount == (tree->TypeGet() == TYP_VOID) ? 0 : 1);
            srcCount = tree->gtGetOp2()->isContained() ? 1 : 2;

            // GT_XCHG requires a single internal regiester; the others require two.
            buildInternalIntRegisterDefForNode(tree);
            if (tree->OperGet() != GT_XCHG)
            {
                buildInternalIntRegisterDefForNode(tree);
            }

            // For ARMv8 exclusives the lifetime of the addr and data must be extended because
            // it may be used used multiple during retries
            assert(!tree->gtGetOp1()->isContained());
            RefPosition* op1Use = BuildUse(tree->gtGetOp1());
            RefPosition* op2Use = nullptr;
            if (!tree->gtGetOp2()->isContained())
            {
                op2Use = BuildUse(tree->gtGetOp2());
            }

            // Internals may not collide with target
            if (dstCount == 1)
            {
                setDelayFree(op1Use);
                if (op2Use != nullptr)
                {
                    setDelayFree(op2Use);
                }
                setInternalRegsDelayFree = true;
            }
            buildInternalRegisterUses();
            if (dstCount == 1)
            {
                BuildDef(tree);
            }
        }
        break;

        case GT_PUTARG_STK:
            srcCount = BuildPutArgStk(tree->AsPutArgStk());
            break;

        case GT_PUTARG_REG:
            srcCount = BuildPutArgReg(tree->AsUnOp());
            break;

        case GT_CALL:
            srcCount = BuildCall(tree->AsCall());
            if (tree->AsCall()->HasMultiRegRetVal())
            {
                dstCount = tree->AsCall()->GetReturnTypeDesc()->GetReturnRegCount();
            }
            break;

        case GT_ADDR:
        {
            // For a GT_ADDR, the child node should not be evaluated into a register
            GenTree* child = tree->gtGetOp1();
            assert(!isCandidateLocalRef(child));
            assert(child->isContained());
            assert(dstCount == 1);
            srcCount = 0;
            BuildDef(tree);
        }
        break;

        case GT_BLK:
        case GT_DYN_BLK:
            // These should all be eliminated prior to Lowering.
            assert(!"Non-store block node in Lowering");
            srcCount = 0;
            break;

        case GT_STORE_BLK:
        case GT_STORE_OBJ:
        case GT_STORE_DYN_BLK:
            srcCount = BuildBlockStore(tree->AsBlk());
            break;

        case GT_INIT_VAL:
            // Always a passthrough of its child's value.
            assert(!"INIT_VAL should always be contained");
            srcCount = 0;
            break;

        case GT_LCLHEAP:
        {
            assert(dstCount == 1);

            // Need a variable number of temp regs (see genLclHeap() in codegenamd64.cpp):
            // Here '-' means don't care.
            //
            //  Size?                   Init Memory?    # temp regs
            //   0                          -               0
            //   const and <=6 ptr words    -               0
            //   const and <PageSize        No              0
            //   >6 ptr words               Yes           hasPspSym ? 1 : 0
            //   Non-const                  Yes           hasPspSym ? 1 : 0
            //   Non-const                  No              2
            //
            // PSPSym - If the method has PSPSym increment internalIntCount by 1.
            //
            bool hasPspSym;
#if FEATURE_EH_FUNCLETS
            hasPspSym = (compiler->lvaPSPSym != BAD_VAR_NUM);
#else
            hasPspSym = false;
#endif

            GenTree* size = tree->gtGetOp1();
            if (size->IsCnsIntOrI())
            {
                assert(size->isContained());
                srcCount = 0;

                size_t sizeVal = size->gtIntCon.gtIconVal;

                if (sizeVal != 0)
                {
                    // Compute the amount of memory to properly STACK_ALIGN.
                    // Note: The Gentree node is not updated here as it is cheap to recompute stack aligned size.
                    // This should also help in debugging as we can examine the original size specified with
                    // localloc.
                    sizeVal                          = AlignUp(sizeVal, STACK_ALIGN);
                    size_t cntStackAlignedWidthItems = (sizeVal >> STACK_ALIGN_SHIFT);

                    // For small allocations upto 4 'stp' instructions (i.e. 64 bytes of localloc)
                    //
                    if (cntStackAlignedWidthItems <= 4)
                    {
                        // Need no internal registers
                    }
                    else if (!compiler->info.compInitMem)
                    {
                        // No need to initialize allocated stack space.
                        if (sizeVal < compiler->eeGetPageSize())
                        {
                            // Need no internal registers
                        }
                        else
                        {
                            // We need two registers: regCnt and RegTmp
                            buildInternalIntRegisterDefForNode(tree);
                            buildInternalIntRegisterDefForNode(tree);
                        }
                    }
                    else if (hasPspSym)
                    {
                        // greater than 4 and need to zero initialize allocated stack space.
                        // If the method has PSPSym, we need an internal register to hold regCnt
                        // since targetReg allocated to GT_LCLHEAP node could be the same as one of
                        // the the internal registers.
                        buildInternalIntRegisterDefForNode(tree);
                    }
                }
            }
            else
            {
                srcCount = 1;
                if (!compiler->info.compInitMem)
                {
                    buildInternalIntRegisterDefForNode(tree);
                    buildInternalIntRegisterDefForNode(tree);
                }
                else if (hasPspSym)
                {
                    // If the method has PSPSym, we need an internal register to hold regCnt
                    // since targetReg allocated to GT_LCLHEAP node could be the same as one of
                    // the the internal registers.
                    buildInternalIntRegisterDefForNode(tree);
                }
            }

            // If the method has PSPSym, we need an additional register to relocate it on stack.
            if (hasPspSym)
            {
                // Exclude const size 0
                if (!size->IsCnsIntOrI() || (size->gtIntCon.gtIconVal > 0))
                {
                    buildInternalIntRegisterDefForNode(tree);
                }
            }
            if (!size->isContained())
            {
                BuildUse(size);
            }
            buildInternalRegisterUses();
            BuildDef(tree);
        }
        break;

        case GT_ARR_BOUNDS_CHECK:
#ifdef FEATURE_SIMD
        case GT_SIMD_CHK:
#endif // FEATURE_SIMD
        {
            GenTreeBoundsChk* node = tree->AsBoundsChk();
            // Consumes arrLen & index - has no result
            assert(dstCount == 0);

            GenTree* intCns = nullptr;
            GenTree* other  = nullptr;
            srcCount        = BuildOperandUses(tree->AsBoundsChk()->gtIndex);
            srcCount += BuildOperandUses(tree->AsBoundsChk()->gtArrLen);
        }
        break;

        case GT_ARR_ELEM:
            // These must have been lowered to GT_ARR_INDEX
            noway_assert(!"We should never see a GT_ARR_ELEM in lowering");
            srcCount = 0;
            assert(dstCount == 0);
            break;

        case GT_ARR_INDEX:
        {
            srcCount = 2;
            assert(dstCount == 1);
            buildInternalIntRegisterDefForNode(tree);
            setInternalRegsDelayFree = true;

            // For GT_ARR_INDEX, the lifetime of the arrObj must be extended because it is actually used multiple
            // times while the result is being computed.
            RefPosition* arrObjUse = BuildUse(tree->AsArrIndex()->ArrObj());
            setDelayFree(arrObjUse);
            BuildUse(tree->AsArrIndex()->IndexExpr());
            buildInternalRegisterUses();
            BuildDef(tree);
        }
        break;

        case GT_ARR_OFFSET:
            // This consumes the offset, if any, the arrObj and the effective index,
            // and produces the flattened offset for this dimension.
            srcCount = 2;
            if (!tree->gtArrOffs.gtOffset->isContained())
            {
                BuildUse(tree->AsArrOffs()->gtOffset);
                srcCount++;
            }
            BuildUse(tree->AsArrOffs()->gtIndex);
            BuildUse(tree->AsArrOffs()->gtArrObj);
            assert(dstCount == 1);
            buildInternalIntRegisterDefForNode(tree);
            buildInternalRegisterUses();
            BuildDef(tree);
            break;

        case GT_LEA:
        {
            GenTreeAddrMode* lea = tree->AsAddrMode();

            GenTree* base  = lea->Base();
            GenTree* index = lea->Index();
            int      cns   = lea->Offset();

            // This LEA is instantiating an address, so we set up the srcCount here.
            srcCount = 0;
            if (base != nullptr)
            {
                srcCount++;
                BuildUse(base);
            }
            if (index != nullptr)
            {
                srcCount++;
                BuildUse(index);
            }
            assert(dstCount == 1);

            // On ARM64 we may need a single internal register
            // (when both conditions are true then we still only need a single internal register)
            if ((index != nullptr) && (cns != 0))
            {
                // ARM64 does not support both Index and offset so we need an internal register
                buildInternalIntRegisterDefForNode(tree);
            }
            else if (!emitter::emitIns_valid_imm_for_add(cns, EA_8BYTE))
            {
                // This offset can't be contained in the add instruction, so we need an internal register
                buildInternalIntRegisterDefForNode(tree);
            }
            buildInternalRegisterUses();
            BuildDef(tree);
        }
        break;

        case GT_STOREIND:
        {
            assert(dstCount == 0);

            if (compiler->codeGen->gcInfo.gcIsWriteBarrierAsgNode(tree))
            {
                srcCount = BuildGCWriteBarrier(tree);
                break;
            }

            srcCount = BuildIndir(tree->AsIndir());
            if (!tree->gtGetOp2()->isContained())
            {
                BuildUse(tree->gtGetOp2());
                srcCount++;
            }
        }
        break;

        case GT_NULLCHECK:
            // Unlike ARM, ARM64 implements NULLCHECK as a load to REG_ZR, so no internal register
            // is required, and it is not a localDefUse.
            assert(dstCount == 0);
            assert(!tree->gtGetOp1()->isContained());
            BuildUse(tree->gtGetOp1());
            srcCount = 1;
            break;

        case GT_IND:
            assert(dstCount == 1);
            srcCount = BuildIndir(tree->AsIndir());
            break;

        case GT_CATCH_ARG:
            srcCount = 0;
            assert(dstCount == 1);
            BuildDef(tree, RBM_EXCEPTION_OBJECT);
            break;

        case GT_CLS_VAR:
            srcCount = 0;
            // GT_CLS_VAR, by the time we reach the backend, must always
            // be a pure use.
            // It will produce a result of the type of the
            // node, and use an internal register for the address.

            assert(dstCount == 1);
            assert((tree->gtFlags & (GTF_VAR_DEF | GTF_VAR_USEASG)) == 0);
            buildInternalIntRegisterDefForNode(tree);
            buildInternalRegisterUses();
            BuildDef(tree);
            break;

        case GT_INDEX_ADDR:
            assert(dstCount == 1);
            srcCount = BuildBinaryUses(tree->AsOp());
            buildInternalIntRegisterDefForNode(tree);
            buildInternalRegisterUses();
            BuildDef(tree);
            break;

    } // end switch (tree->OperGet())

    if (tree->IsUnusedValue() && (dstCount != 0))
    {
        isLocalDefUse = true;
    }
    // We need to be sure that we've set srcCount and dstCount appropriately
    assert((dstCount < 2) || tree->IsMultiRegCall());
    assert(isLocalDefUse == (tree->IsValue() && tree->IsUnusedValue()));
    assert(!tree->IsUnusedValue() || (dstCount != 0));
    assert(dstCount == tree->GetRegisterDstCount());
    INDEBUG(dumpNodeInfo(tree, dstCandidates, srcCount, dstCount));
    return srcCount;
}

#ifdef FEATURE_SIMD
//------------------------------------------------------------------------
// BuildSIMD: Set the NodeInfo for a GT_SIMD tree.
//
// Arguments:
//    tree       - The GT_SIMD node of interest
//
// Return Value:
//    The number of sources consumed by this node.
//
int LinearScan::BuildSIMD(GenTreeSIMD* simdTree)
{
    int srcCount = 0;
    // Only SIMDIntrinsicInit can be contained
    if (simdTree->isContained())
    {
        assert(simdTree->gtSIMDIntrinsicID == SIMDIntrinsicInit);
    }
    int dstCount = simdTree->IsValue() ? 1 : 0;
    assert(dstCount == 1);

    bool buildUses = true;

    GenTree* op1 = simdTree->gtGetOp1();
    GenTree* op2 = simdTree->gtGetOp2();

    switch (simdTree->gtSIMDIntrinsicID)
    {
        case SIMDIntrinsicInit:
        case SIMDIntrinsicCast:
        case SIMDIntrinsicSqrt:
        case SIMDIntrinsicAbs:
        case SIMDIntrinsicConvertToSingle:
        case SIMDIntrinsicConvertToInt32:
        case SIMDIntrinsicConvertToDouble:
        case SIMDIntrinsicConvertToInt64:
        case SIMDIntrinsicWidenLo:
        case SIMDIntrinsicWidenHi:
            // No special handling required.
            break;

        case SIMDIntrinsicGetItem:
        {
            op1 = simdTree->gtGetOp1();
            op2 = simdTree->gtGetOp2();

            // We have an object and an index, either of which may be contained.
            bool setOp2DelayFree = false;
            if (!op2->IsCnsIntOrI() && (!op1->isContained() || op1->OperIsLocal()))
            {
                // If the index is not a constant and the object is not contained or is a local
                // we will need a general purpose register to calculate the address
                // internal register must not clobber input index
                // TODO-Cleanup: An internal register will never clobber a source; this code actually
                // ensures that the index (op2) doesn't interfere with the target.
                buildInternalIntRegisterDefForNode(simdTree);
                setOp2DelayFree = true;
            }
            srcCount += BuildOperandUses(op1);
            if (!op2->isContained())
            {
                RefPosition* op2Use = BuildUse(op2);
                if (setOp2DelayFree)
                {
                    setDelayFree(op2Use);
                }
                srcCount++;
            }

            if (!op2->IsCnsIntOrI() && (!op1->isContained()))
            {
                // If vector is not already in memory (contained) and the index is not a constant,
                // we will use the SIMD temp location to store the vector.
                compiler->getSIMDInitTempVarNum();
            }
            buildUses = false;
        }
        break;

        case SIMDIntrinsicAdd:
        case SIMDIntrinsicSub:
        case SIMDIntrinsicMul:
        case SIMDIntrinsicDiv:
        case SIMDIntrinsicBitwiseAnd:
        case SIMDIntrinsicBitwiseAndNot:
        case SIMDIntrinsicBitwiseOr:
        case SIMDIntrinsicBitwiseXor:
        case SIMDIntrinsicMin:
        case SIMDIntrinsicMax:
        case SIMDIntrinsicEqual:
        case SIMDIntrinsicLessThan:
        case SIMDIntrinsicGreaterThan:
        case SIMDIntrinsicLessThanOrEqual:
        case SIMDIntrinsicGreaterThanOrEqual:
            // No special handling required.
            break;

        case SIMDIntrinsicSetX:
        case SIMDIntrinsicSetY:
        case SIMDIntrinsicSetZ:
        case SIMDIntrinsicSetW:
        case SIMDIntrinsicNarrow:
        {
            // Op1 will write to dst before Op2 is free
            BuildUse(op1);
            RefPosition* op2Use = BuildUse(op2);
            setDelayFree(op2Use);
            srcCount  = 2;
            buildUses = false;
            break;
        }

        case SIMDIntrinsicInitN:
        {
            var_types baseType = simdTree->gtSIMDBaseType;
            srcCount           = (short)(simdTree->gtSIMDSize / genTypeSize(baseType));
            if (varTypeIsFloating(simdTree->gtSIMDBaseType))
            {
                // Need an internal register to stitch together all the values into a single vector in a SIMD reg.
                buildInternalFloatRegisterDefForNode(simdTree);
            }

            int initCount = 0;
            for (GenTree* list = op1; list != nullptr; list = list->gtGetOp2())
            {
                assert(list->OperGet() == GT_LIST);
                GenTree* listItem = list->gtGetOp1();
                assert(listItem->TypeGet() == baseType);
                assert(!listItem->isContained());
                BuildUse(listItem);
                initCount++;
            }
            assert(initCount == srcCount);
            buildUses = false;

            break;
        }

        case SIMDIntrinsicInitArray:
            // We have an array and an index, which may be contained.
            break;

        case SIMDIntrinsicOpEquality:
        case SIMDIntrinsicOpInEquality:
            buildInternalFloatRegisterDefForNode(simdTree);
            break;

        case SIMDIntrinsicDotProduct:
            buildInternalFloatRegisterDefForNode(simdTree);
            break;

        case SIMDIntrinsicSelect:
            // TODO-ARM64-CQ Allow lowering to see SIMDIntrinsicSelect so we can generate BSL VC, VA, VB
            // bsl target register must be VC.  Reserve a temp in case we need to shuffle things.
            // This will require a different approach, as GenTreeSIMD has only two operands.
            assert(!"SIMDIntrinsicSelect not yet supported");
            buildInternalFloatRegisterDefForNode(simdTree);
            break;

        case SIMDIntrinsicInitArrayX:
        case SIMDIntrinsicInitFixed:
        case SIMDIntrinsicCopyToArray:
        case SIMDIntrinsicCopyToArrayX:
        case SIMDIntrinsicNone:
        case SIMDIntrinsicGetCount:
        case SIMDIntrinsicGetOne:
        case SIMDIntrinsicGetZero:
        case SIMDIntrinsicGetAllOnes:
        case SIMDIntrinsicGetX:
        case SIMDIntrinsicGetY:
        case SIMDIntrinsicGetZ:
        case SIMDIntrinsicGetW:
        case SIMDIntrinsicInstEquals:
        case SIMDIntrinsicHWAccel:
        case SIMDIntrinsicWiden:
        case SIMDIntrinsicInvalid:
            assert(!"These intrinsics should not be seen during register allocation");
            __fallthrough;

        default:
            noway_assert(!"Unimplemented SIMD node type.");
            unreached();
    }
    if (buildUses)
    {
        assert(!op1->OperIs(GT_LIST));
        assert(srcCount == 0);
        srcCount = BuildOperandUses(op1);
        if ((op2 != nullptr) && !op2->isContained())
        {
            srcCount += BuildOperandUses(op2);
        }
    }
    assert(internalCount <= MaxInternalCount);
    buildInternalRegisterUses();
    if (dstCount == 1)
    {
        BuildDef(simdTree);
    }
    else
    {
        assert(dstCount == 0);
    }
    return srcCount;
}
#endif // FEATURE_SIMD

#ifdef FEATURE_HW_INTRINSICS
#include "hwintrinsic.h"
//------------------------------------------------------------------------
// BuildHWIntrinsic: Set the NodeInfo for a GT_HWIntrinsic tree.
//
// Arguments:
//    tree       - The GT_HWIntrinsic node of interest
//
// Return Value:
//    The number of sources consumed by this node.
//
int LinearScan::BuildHWIntrinsic(GenTreeHWIntrinsic* intrinsicTree)
{
    NamedIntrinsic intrinsicID = intrinsicTree->gtHWIntrinsicId;
    int            numArgs     = Compiler::numArgsOfHWIntrinsic(intrinsicTree);

    GenTree* op1      = intrinsicTree->gtGetOp1();
    GenTree* op2      = intrinsicTree->gtGetOp2();
    GenTree* op3      = nullptr;
    int      srcCount = 0;

    if ((op1 != nullptr) && op1->OperIsList())
    {
        // op2 must be null, and there must be at least two more arguments.
        assert(op2 == nullptr);
        noway_assert(op1->AsArgList()->Rest() != nullptr);
        noway_assert(op1->AsArgList()->Rest()->Rest() != nullptr);
        assert(op1->AsArgList()->Rest()->Rest()->Rest() == nullptr);
        op2 = op1->AsArgList()->Rest()->Current();
        op3 = op1->AsArgList()->Rest()->Rest()->Current();
        op1 = op1->AsArgList()->Current();
    }

    int  dstCount       = intrinsicTree->IsValue() ? 1 : 0;
    bool op2IsDelayFree = false;
    bool op3IsDelayFree = false;

    // Create internal temps, and handle any other special requirements.
    switch (compiler->getHWIntrinsicInfo(intrinsicID).form)
    {
        case HWIntrinsicInfo::Sha1HashOp:
            assert((numArgs == 3) && (op2 != nullptr) && (op3 != nullptr));
            if (!op2->isContained())
            {
                assert(!op3->isContained());
                op2IsDelayFree           = true;
                op3IsDelayFree           = true;
                setInternalRegsDelayFree = true;
            }
            buildInternalFloatRegisterDefForNode(intrinsicTree);
            break;
        case HWIntrinsicInfo::SimdTernaryRMWOp:
            assert((numArgs == 3) && (op2 != nullptr) && (op3 != nullptr));
            if (!op2->isContained())
            {
                assert(!op3->isContained());
                op2IsDelayFree = true;
                op3IsDelayFree = true;
            }
            break;
        case HWIntrinsicInfo::Sha1RotateOp:
            buildInternalFloatRegisterDefForNode(intrinsicTree);
            break;

        case HWIntrinsicInfo::SimdExtractOp:
        case HWIntrinsicInfo::SimdInsertOp:
            if (!op2->isContained())
            {
                // We need a temp to create a switch table
                buildInternalIntRegisterDefForNode(intrinsicTree);
            }
            break;

        default:
            break;
    }

    // Next, build uses
    if (numArgs > 3)
    {
        srcCount = 0;
        assert(!op2IsDelayFree && !op3IsDelayFree);
        assert(op1->OperIs(GT_LIST));
        {
            for (GenTreeArgList* list = op1->AsArgList(); list != nullptr; list = list->Rest())
            {
                srcCount += BuildOperandUses(list->Current());
            }
        }
        assert(srcCount == numArgs);
    }
    else
    {
        if (op1 != nullptr)
        {
            srcCount += BuildOperandUses(op1);
            if (op2 != nullptr)
            {
                srcCount += (op2IsDelayFree) ? BuildDelayFreeUses(op2) : BuildOperandUses(op2);
                if (op3 != nullptr)
                {
                    srcCount += (op3IsDelayFree) ? BuildDelayFreeUses(op3) : BuildOperandUses(op3);
                }
            }
        }
    }
    buildInternalRegisterUses();

    // Now defs
    if (intrinsicTree->IsValue())
    {
        BuildDef(intrinsicTree);
    }

    return srcCount;
}
#endif

#endif // _TARGET_ARM64_
