// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                           Lowering for AMD64                              XX
XX                                                                           XX
XX  This encapsulates all the logic for lowering trees for the AMD64         XX
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

#ifdef _TARGET_XARCH_

#include "jit.h"
#include "sideeffects.h"
#include "lower.h"

// xarch supports both ROL and ROR instructions so no lowering is required.
void Lowering::LowerRotate(GenTreePtr tree)
{
}

//------------------------------------------------------------------------
// LowerStoreLoc: Lower a store of a lclVar
//
// Arguments:
//    storeLoc - the local store (GT_STORE_LCL_FLD or GT_STORE_LCL_VAR)
//
// Notes:
//    This involves:
//    - Widening operations of unsigneds.

void Lowering::LowerStoreLoc(GenTreeLclVarCommon* storeLoc)
{
    GenTree* op1 = storeLoc->gtGetOp1();

    // Try to widen the ops if they are going into a local var.
    if ((storeLoc->gtOper == GT_STORE_LCL_VAR) && (storeLoc->gtOp1->gtOper == GT_CNS_INT))
    {
        GenTreeIntCon* con  = storeLoc->gtOp1->AsIntCon();
        ssize_t        ival = con->gtIconVal;

        unsigned   varNum = storeLoc->gtLclNum;
        LclVarDsc* varDsc = comp->lvaTable + varNum;

        if (varDsc->lvIsSIMDType())
        {
            noway_assert(storeLoc->gtType != TYP_STRUCT);
        }
        unsigned size = genTypeSize(storeLoc);
        // If we are storing a constant into a local variable
        // we extend the size of the store here
        if ((size < 4) && !varTypeIsStruct(varDsc))
        {
            if (!varTypeIsUnsigned(varDsc))
            {
                if (genTypeSize(storeLoc) == 1)
                {
                    if ((ival & 0x7f) != ival)
                    {
                        ival = ival | 0xffffff00;
                    }
                }
                else
                {
                    assert(genTypeSize(storeLoc) == 2);
                    if ((ival & 0x7fff) != ival)
                    {
                        ival = ival | 0xffff0000;
                    }
                }
            }

            // A local stack slot is at least 4 bytes in size, regardless of
            // what the local var is typed as, so auto-promote it here
            // unless it is a field of a promoted struct
            // TODO-XArch-CQ: if the field is promoted shouldn't we also be able to do this?
            if (!varDsc->lvIsStructField)
            {
                storeLoc->gtType = TYP_INT;
                con->SetIconValue(ival);
            }
        }
    }
}

//------------------------------------------------------------------------
// LowerBlockStore: Set block store type
//
// Arguments:
//    blkNode       - The block store node of interest
//
// Return Value:
//    None.
//
void Lowering::LowerBlockStore(GenTreeBlk* blkNode)
{
    GenTree*   dstAddr       = blkNode->Addr();
    unsigned   size          = blkNode->gtBlkSize;
    GenTree*   source        = blkNode->Data();
    Compiler*  compiler      = comp;
    GenTreePtr srcAddrOrFill = nullptr;
    bool       isInitBlk     = blkNode->OperIsInitBlkOp();

    if (!isInitBlk)
    {
        // CopyObj or CopyBlk
        if ((blkNode->OperGet() == GT_STORE_OBJ) && ((blkNode->AsObj()->gtGcPtrCount == 0) || blkNode->gtBlkOpGcUnsafe))
        {
            blkNode->SetOper(GT_STORE_BLK);
        }
        if (source->gtOper == GT_IND)
        {
            srcAddrOrFill = blkNode->Data()->gtGetOp1();
        }
    }

    if (isInitBlk)
    {
        GenTree* initVal = source;
        if (initVal->OperIsInitVal())
        {
            initVal = initVal->gtGetOp1();
        }
        srcAddrOrFill = initVal;
        // If we have an InitBlk with constant block size we can optimize several ways:
        // a) If the size is smaller than a small memory page but larger than INITBLK_UNROLL_LIMIT bytes
        //    we use rep stosb since this reduces the register pressure in LSRA and we have
        //    roughly the same performance as calling the helper.
        // b) If the size is <= INITBLK_UNROLL_LIMIT bytes and the fill byte is a constant,
        //    we can speed this up by unrolling the loop using SSE2 stores.  The reason for
        //    this threshold is because our last investigation (Fall 2013), more than 95% of initblks
        //    in our framework assemblies are actually <= INITBLK_UNROLL_LIMIT bytes size, so this is the
        //    preferred code sequence for the vast majority of cases.

        // This threshold will decide from using the helper or let the JIT decide to inline
        // a code sequence of its choice.
        unsigned helperThreshold = max(INITBLK_STOS_LIMIT, INITBLK_UNROLL_LIMIT);

        // TODO-X86-CQ: Investigate whether a helper call would be beneficial on x86
        if (size != 0 && size <= helperThreshold)
        {
            // Always favor unrolling vs rep stos.
            if (size <= INITBLK_UNROLL_LIMIT && initVal->IsCnsIntOrI())
            {
                // The fill value of an initblk is interpreted to hold a
                // value of (unsigned int8) however a constant of any size
                // may practically reside on the evaluation stack. So extract
                // the lower byte out of the initVal constant and replicate
                // it to a larger constant whose size is sufficient to support
                // the largest width store of the desired inline expansion.

                ssize_t fill = initVal->gtIntCon.gtIconVal & 0xFF;
#ifdef _TARGET_AMD64_
                if (size < REGSIZE_BYTES)
                {
                    initVal->gtIntCon.gtIconVal = 0x01010101 * fill;
                }
                else
                {
                    initVal->gtIntCon.gtIconVal = 0x0101010101010101LL * fill;
                    initVal->gtType             = TYP_LONG;
                }
#else  // !_TARGET_AMD64_
                initVal->gtIntCon.gtIconVal = 0x01010101 * fill;
#endif // !_TARGET_AMD64_

                blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindUnroll;
            }
            else
            {
                blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindRepInstr;
            }
        }
        else
        {
#ifdef _TARGET_AMD64_
            blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindHelper;
#else  // !_TARGET_AMD64_
            blkNode->gtBlkOpKind            = GenTreeBlk::BlkOpKindRepInstr;
#endif // !_TARGET_AMD64_
        }
    }
    else if (blkNode->gtOper == GT_STORE_OBJ)
    {
        // CopyObj

        GenTreeObj* cpObjNode = blkNode->AsObj();

        unsigned slots = cpObjNode->gtSlots;

#ifdef DEBUG
        // CpObj must always have at least one GC-Pointer as a member.
        assert(cpObjNode->gtGcPtrCount > 0);

        assert(dstAddr->gtType == TYP_BYREF || dstAddr->gtType == TYP_I_IMPL);

        CORINFO_CLASS_HANDLE clsHnd    = cpObjNode->gtClass;
        size_t               classSize = comp->info.compCompHnd->getClassSize(clsHnd);
        size_t               blkSize   = roundUp(classSize, TARGET_POINTER_SIZE);

        // Currently, the EE always round up a class data structure so
        // we are not handling the case where we have a non multiple of pointer sized
        // struct. This behavior may change in the future so in order to keeps things correct
        // let's assert it just to be safe. Going forward we should simply
        // handle this case.
        assert(classSize == blkSize);
        assert((blkSize / TARGET_POINTER_SIZE) == slots);
        assert(cpObjNode->HasGCPtr());
#endif

        bool IsRepMovsProfitable = false;

        // If the destination is not on the stack, let's find out if we
        // can improve code size by using rep movsq instead of generating
        // sequences of movsq instructions.
        if (!dstAddr->OperIsLocalAddr())
        {
            // Let's inspect the struct/class layout and determine if it's profitable
            // to use rep movsq for copying non-gc memory instead of using single movsq
            // instructions for each memory slot.
            unsigned i      = 0;
            BYTE*    gcPtrs = cpObjNode->gtGcPtrs;

            do
            {
                unsigned nonGCSlots = 0;
                // Measure a contiguous non-gc area inside the struct and note the maximum.
                while (i < slots && gcPtrs[i] == TYPE_GC_NONE)
                {
                    nonGCSlots++;
                    i++;
                }

                while (i < slots && gcPtrs[i] != TYPE_GC_NONE)
                {
                    i++;
                }

                if (nonGCSlots >= CPOBJ_NONGC_SLOTS_LIMIT)
                {
                    IsRepMovsProfitable = true;
                    break;
                }
            } while (i < slots);
        }
        else if (slots >= CPOBJ_NONGC_SLOTS_LIMIT)
        {
            IsRepMovsProfitable = true;
        }

        // There are two cases in which we need to materialize the
        // struct size:
        // a) When the destination is on the stack we don't need to use the
        //    write barrier, we can just simply call rep movsq and get a win in codesize.
        // b) If we determine we have contiguous non-gc regions in the struct where it's profitable
        //    to use rep movsq instead of a sequence of single movsq instructions.  According to the
        //    Intel Manual, the sweet spot for small structs is between 4 to 12 slots of size where
        //    the entire operation takes 20 cycles and encodes in 5 bytes (moving RCX, and calling rep movsq).
        if (IsRepMovsProfitable)
        {
            // We need the size of the contiguous Non-GC-region to be in RCX to call rep movsq.
            blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindRepInstr;
        }
        else
        {
            blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindUnroll;
        }
    }
    else
    {
        assert((blkNode->OperGet() == GT_STORE_BLK) || (blkNode->OperGet() == GT_STORE_DYN_BLK));
        // CopyBlk
        // In case of a CpBlk with a constant size and less than CPBLK_MOVS_LIMIT size
        // we can use rep movs to generate code instead of the helper call.

        // This threshold will decide between using the helper or let the JIT decide to inline
        // a code sequence of its choice.
        unsigned helperThreshold = max(CPBLK_MOVS_LIMIT, CPBLK_UNROLL_LIMIT);

        // TODO-X86-CQ: Investigate whether a helper call would be beneficial on x86
        if ((size != 0) && (size <= helperThreshold))
        {
            // If we have a buffer between XMM_REGSIZE_BYTES and CPBLK_UNROLL_LIMIT bytes, we'll use SSE2.
            // Structs and buffer with sizes <= CPBLK_UNROLL_LIMIT bytes are occurring in more than 95% of
            // our framework assemblies, so this is the main code generation scheme we'll use.
            if (size <= CPBLK_UNROLL_LIMIT)
            {
                blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindUnroll;
            }
            else
            {
                blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindRepInstr;
            }
        }
#ifdef _TARGET_AMD64_
        else
        {
            blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindHelper;
        }
#elif defined(_TARGET_X86_)
        else
        {
            blkNode->gtBlkOpKind = GenTreeBlk::BlkOpKindRepInstr;
        }
#endif // _TARGET_X86_
        assert(blkNode->gtBlkOpKind != GenTreeBlk::BlkOpKindInvalid);
    }
}

#ifdef FEATURE_PUT_STRUCT_ARG_STK
//------------------------------------------------------------------------
// LowerPutArgStk: Lower a GT_PUTARG_STK.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::LowerPutArgStk(GenTreePutArgStk* putArgStk)
{
#ifdef _TARGET_X86_
    if (putArgStk->gtOp1->gtOper == GT_FIELD_LIST)
    {
        putArgStk->gtNumberReferenceSlots = 0;
        putArgStk->gtPutArgStkKind        = GenTreePutArgStk::Kind::Invalid;

        GenTreeFieldList* fieldList = putArgStk->gtOp1->AsFieldList();

        // The code generator will push these fields in reverse order by offset. Reorder the list here s.t. the order
        // of uses is visible to LSRA.
        unsigned          fieldCount = 0;
        GenTreeFieldList* head       = nullptr;
        for (GenTreeFieldList *current = fieldList, *next; current != nullptr; current = next)
        {
            next = current->Rest();

            // First, insert the field node into the sorted list.
            GenTreeFieldList* prev = nullptr;
            for (GenTreeFieldList* cursor = head;; cursor = cursor->Rest())
            {
                // If the offset of the current list node is greater than the offset of the cursor or if we have
                // reached the end of the list, insert the current node before the cursor and terminate.
                if ((cursor == nullptr) || (current->gtFieldOffset > cursor->gtFieldOffset))
                {
                    if (prev == nullptr)
                    {
                        assert(cursor == head);
                        head = current;
                    }
                    else
                    {
                        prev->Rest() = current;
                    }

                    current->Rest() = cursor;
                    break;
                }
            }

            fieldCount++;
        }

        // In theory, the upper bound for the size of a field list is 8: these constructs only appear when passing the
        // collection of lclVars that represent the fields of a promoted struct lclVar, and we do not promote struct
        // lclVars with more than 4 fields. If each of these lclVars is of type long, decomposition will split the
        // corresponding field list nodes in two, giving an upper bound of 8.
        //
        // The reason that this is important is that the algorithm we use above to sort the field list is O(N^2): if
        // the maximum size of a field list grows significantly, we will need to reevaluate it.
        assert(fieldCount <= 8);

        // The sort above may have changed which node is at the head of the list. Update the PUTARG_STK node if
        // necessary.
        if (head != fieldList)
        {
            head->gtFlags |= GTF_FIELD_LIST_HEAD;
            fieldList->gtFlags &= ~GTF_FIELD_LIST_HEAD;

#ifdef DEBUG
            head->gtSeqNum = fieldList->gtSeqNum;
#endif // DEBUG

            head->gtLsraInfo = fieldList->gtLsraInfo;
            head->gtClearReg(comp);

            BlockRange().InsertAfter(fieldList, head);
            BlockRange().Remove(fieldList);

            fieldList        = head;
            putArgStk->gtOp1 = fieldList;
        }

        // Now that the fields have been sorted, the kind of code we will generate.
        bool     allFieldsAreSlots = true;
        unsigned prevOffset        = putArgStk->getArgSize();
        for (GenTreeFieldList* current = fieldList; current != nullptr; current = current->Rest())
        {
            GenTree* const  fieldNode   = current->Current();
            const var_types fieldType   = fieldNode->TypeGet();
            const unsigned  fieldOffset = current->gtFieldOffset;
            assert(fieldType != TYP_LONG);

            // We can treat as a slot any field that is stored at a slot boundary, where the previous
            // field is not in the same slot. (Note that we store the fields in reverse order.)
            const bool fieldIsSlot = ((fieldOffset % 4) == 0) && ((prevOffset - fieldOffset) >= 4);
            if (!fieldIsSlot)
            {
                allFieldsAreSlots = false;
            }

            if (varTypeIsGC(fieldType))
            {
                putArgStk->gtNumberReferenceSlots++;
            }

            prevOffset = fieldOffset;
        }

        // Set the copy kind.
        // TODO-X86-CQ: Even if we are using push, if there are contiguous floating point fields, we should
        // adjust the stack once for those fields. The latter is really best done in code generation, but
        // this tuning should probably be undertaken as a whole.
        // Also, if there are  floating point fields, it may be better to use the "Unroll" mode
        // of copying the struct as a whole, if the fields are not register candidates.
        if (allFieldsAreSlots)
        {
            putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::PushAllSlots;
        }
        else
        {
            putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::Push;
        }
        return;
    }
#endif // _TARGET_X86_

    if (putArgStk->TypeGet() != TYP_STRUCT)
    {
        return;
    }

    GenTreePtr dst     = putArgStk;
    GenTreePtr src     = putArgStk->gtOp1;
    GenTreePtr srcAddr = nullptr;

    // In case of a CpBlk we could use a helper call. In case of putarg_stk we
    // can't do that since the helper call could kill some already set up outgoing args.
    // TODO-Amd64-Unix: converge the code for putarg_stk with cpyblk/cpyobj.
    // The cpyXXXX code is rather complex and this could cause it to be more complex, but
    // it might be the right thing to do.

    // This threshold will decide from using the helper or let the JIT decide to inline
    // a code sequence of its choice.
    ssize_t helperThreshold = max(CPBLK_MOVS_LIMIT, CPBLK_UNROLL_LIMIT);
    ssize_t size            = putArgStk->gtNumSlots * TARGET_POINTER_SIZE;

    // TODO-X86-CQ: The helper call either is not supported on x86 or required more work
    // (I don't know which).

    // If we have a buffer between XMM_REGSIZE_BYTES and CPBLK_UNROLL_LIMIT bytes, we'll use SSE2.
    // Structs and buffer with sizes <= CPBLK_UNROLL_LIMIT bytes are occurring in more than 95% of
    // our framework assemblies, so this is the main code generation scheme we'll use.
    if (size <= CPBLK_UNROLL_LIMIT && putArgStk->gtNumberReferenceSlots == 0)
    {
#ifdef _TARGET_X86_
        if (size < XMM_REGSIZE_BYTES)
        {
            putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::Push;
        }
        else
#endif // _TARGET_X86_
        {
            putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::Unroll;
        }
    }
#ifdef _TARGET_X86_
    else if (putArgStk->gtNumberReferenceSlots != 0)
    {
        // On x86, we must use `push` to store GC references to the stack in order for the emitter to properly update
        // the function's GC info. These `putargstk` nodes will generate a sequence of `push` instructions.
        putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::Push;
    }
#endif // _TARGET_X86_
    else
    {
        putArgStk->gtPutArgStkKind = GenTreePutArgStk::Kind::RepInstr;
    }
}
#endif // FEATURE_PUT_STRUCT_ARG_STK

//------------------------------------------------------------------------
// TreeNodeInfoInitLogicalOp: Set the NodeInfo for GT_AND/GT_OR/GT_XOR,
// as well as GT_ADD/GT_SUB.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::TreeNodeInfoInitLogicalOp(GenTree* tree)
{
    TreeNodeInfo* info = &(tree->gtLsraInfo);
    LinearScan*   l    = m_lsra;

    // We're not marking a constant hanging on the left of the add
    // as containable so we assign it to a register having CQ impact.
    // TODO-XArch-CQ: Detect this case and support both generating a single instruction
    // for GT_ADD(Constant, SomeTree)
    info->srcCount = 2;
    info->dstCount = 1;

    GenTree* op1 = tree->gtGetOp1();
    GenTree* op2 = tree->gtGetOp2();

    // We can directly encode the second operand if it is either a containable constant or a memory-op.
    // In case of memory-op, we can encode it directly provided its type matches with 'tree' type.
    // This is because during codegen, type of 'tree' is used to determine emit Type size. If the types
    // do not match, they get normalized (i.e. sign/zero extended) on load into a register.
    bool       directlyEncodable = false;
    bool       binOpInRMW        = false;
    GenTreePtr operand           = nullptr;

    if (IsContainableImmed(tree, op2))
    {
        directlyEncodable = true;
        operand           = op2;
    }
    else
    {
        binOpInRMW = IsBinOpInRMWStoreInd(tree);
        if (!binOpInRMW)
        {
            if (op2->isMemoryOp() && tree->TypeGet() == op2->TypeGet())
            {
                directlyEncodable = true;
                operand           = op2;
            }
            else if (tree->OperIsCommutative())
            {
                if (IsContainableImmed(tree, op1) ||
                    (op1->isMemoryOp() && tree->TypeGet() == op1->TypeGet() && IsSafeToContainMem(tree, op1)))
                {
                    // If it is safe, we can reverse the order of operands of commutative operations for efficient
                    // codegen
                    directlyEncodable = true;
                    operand           = op1;
                }
            }
        }
    }

    if (directlyEncodable)
    {
        assert(operand != nullptr);
        MakeSrcContained(tree, operand);
    }
    else if (!binOpInRMW)
    {
        // If this binary op neither has contained operands, nor is a
        // Read-Modify-Write (RMW) operation, we can mark its operands
        // as reg optional.
        SetRegOptionalForBinOp(tree);
    }

    // Codegen of this tree node sets ZF and SF flags.
    tree->gtFlags |= GTF_ZSF_SET;
}

//------------------------------------------------------------------------
// TreeNodeInfoInitModDiv: Set the NodeInfo for GT_MOD/GT_DIV/GT_UMOD/GT_UDIV.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::TreeNodeInfoInitModDiv(GenTree* tree)
{
    TreeNodeInfo* info = &(tree->gtLsraInfo);
    LinearScan*   l    = m_lsra;

    GenTree* op1 = tree->gtGetOp1();
    GenTree* op2 = tree->gtGetOp2();

    info->srcCount = 2;
    info->dstCount = 1;

    switch (tree->OperGet())
    {
        case GT_MOD:
        case GT_DIV:
            if (varTypeIsFloating(tree->TypeGet()))
            {
                // No implicit conversions at this stage as the expectation is that
                // everything is made explicit by adding casts.
                assert(op1->TypeGet() == op2->TypeGet());

                if (op2->isMemoryOp() || op2->IsCnsNonZeroFltOrDbl())
                {
                    MakeSrcContained(tree, op2);
                }
                else
                {
                    // If there are no containable operands, we can make an operand reg optional.
                    // SSE2 allows only op2 to be a memory-op.
                    SetRegOptional(op2);
                }

                return;
            }
            break;

        default:
            break;
    }

    // Amd64 Div/Idiv instruction:
    //    Dividend in RAX:RDX  and computes
    //    Quotient in RAX, Remainder in RDX

    if (tree->OperGet() == GT_MOD || tree->OperGet() == GT_UMOD)
    {
        // We are interested in just the remainder.
        // RAX is used as a trashable register during computation of remainder.
        info->setDstCandidates(l, RBM_RDX);
    }
    else
    {
        // We are interested in just the quotient.
        // RDX gets used as trashable register during computation of quotient
        info->setDstCandidates(l, RBM_RAX);
    }

    bool op2CanBeRegOptional = true;
#ifdef _TARGET_X86_
    if (op1->OperGet() == GT_LONG)
    {
        // To avoid reg move would like to have op1's low part in RAX and high part in RDX.
        GenTree* loVal = op1->gtGetOp1();
        GenTree* hiVal = op1->gtGetOp2();

        // Src count is actually 3, so increment.
        assert(op2->IsCnsIntOrI());
        assert(tree->OperGet() == GT_UMOD);
        info->srcCount++;
        op2CanBeRegOptional = false;

        // This situation also requires an internal register.
        info->internalIntCount = 1;
        info->setInternalCandidates(l, l->allRegs(TYP_INT));

        loVal->gtLsraInfo.setSrcCandidates(l, RBM_EAX);
        hiVal->gtLsraInfo.setSrcCandidates(l, RBM_EDX);
    }
    else
#endif
    {
        // If possible would like to have op1 in RAX to avoid a register move
        op1->gtLsraInfo.setSrcCandidates(l, RBM_RAX);
    }

    // divisor can be an r/m, but the memory indirection must be of the same size as the divide
    if (op2->isMemoryOp() && (op2->TypeGet() == tree->TypeGet()))
    {
        MakeSrcContained(tree, op2);
    }
    else if (op2CanBeRegOptional)
    {
        op2->gtLsraInfo.setSrcCandidates(l, l->allRegs(TYP_INT) & ~(RBM_RAX | RBM_RDX));

        // If there are no containable operands, we can make an operand reg optional.
        // Div instruction allows only op2 to be a memory op.
        SetRegOptional(op2);
    }
}

//------------------------------------------------------------------------
// TreeNodeInfoInitIntrinsic: Set the NodeInfo for a GT_INTRINSIC.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::TreeNodeInfoInitIntrinsic(GenTree* tree)
{
    TreeNodeInfo* info = &(tree->gtLsraInfo);
    LinearScan*   l    = m_lsra;

    // Both operand and its result must be of floating point type.
    GenTree* op1 = tree->gtGetOp1();
    assert(varTypeIsFloating(op1));
    assert(op1->TypeGet() == tree->TypeGet());

    info->srcCount = 1;
    info->dstCount = 1;

    switch (tree->gtIntrinsic.gtIntrinsicId)
    {
        case CORINFO_INTRINSIC_Sqrt:
            if (op1->isMemoryOp() || op1->IsCnsNonZeroFltOrDbl())
            {
                MakeSrcContained(tree, op1);
            }
            else
            {
                // Mark the operand as reg optional since codegen can still
                // generate code if op1 is on stack.
                SetRegOptional(op1);
            }
            break;

        case CORINFO_INTRINSIC_Abs:
            // Abs(float x) = x & 0x7fffffff
            // Abs(double x) = x & 0x7ffffff ffffffff

            // In case of Abs we need an internal register to hold mask.

            // TODO-XArch-CQ: avoid using an internal register for the mask.
            // Andps or andpd both will operate on 128-bit operands.
            // The data section constant to hold the mask is a 64-bit size.
            // Therefore, we need both the operand and mask to be in
            // xmm register. When we add support in emitter to emit 128-bit
            // data constants and instructions that operate on 128-bit
            // memory operands we can avoid the need for an internal register.
            if (tree->gtIntrinsic.gtIntrinsicId == CORINFO_INTRINSIC_Abs)
            {
                info->internalFloatCount = 1;
                info->setInternalCandidates(l, l->internalFloatRegCandidates());
            }
            break;

#ifdef _TARGET_X86_
        case CORINFO_INTRINSIC_Cos:
        case CORINFO_INTRINSIC_Sin:
        case CORINFO_INTRINSIC_Round:
            NYI_X86("Math intrinsics Cos, Sin and Round");
            break;
#endif // _TARGET_X86_

        default:
            // Right now only Sqrt/Abs are treated as math intrinsics
            noway_assert(!"Unsupported math intrinsic");
            unreached();
            break;
    }
}

#ifdef FEATURE_SIMD
//------------------------------------------------------------------------
// TreeNodeInfoInitSIMD: Set the NodeInfo for a GT_SIMD tree.
//
// Arguments:
//    tree       - The GT_SIMD node of interest
//
// Return Value:
//    None.

void Lowering::TreeNodeInfoInitSIMD(GenTree* tree)
{
    GenTreeSIMD*  simdTree = tree->AsSIMD();
    TreeNodeInfo* info     = &(tree->gtLsraInfo);
    LinearScan*   lsra     = m_lsra;
    info->dstCount         = 1;
    switch (simdTree->gtSIMDIntrinsicID)
    {
        GenTree* op1;
        GenTree* op2;

        case SIMDIntrinsicInit:
        {
            info->srcCount = 1;
            op1            = tree->gtOp.gtOp1;

            // This sets all fields of a SIMD struct to the given value.
            // Mark op1 as contained if it is either zero or int constant of all 1's,
            // or a float constant with 16 or 32 byte simdType (AVX case)
            //
            // Should never see small int base type vectors except for zero initialization.
            assert(!varTypeIsSmallInt(simdTree->gtSIMDBaseType) || op1->IsIntegralConst(0));

            if (op1->IsFPZero() || op1->IsIntegralConst(0) ||
                (varTypeIsIntegral(simdTree->gtSIMDBaseType) && op1->IsIntegralConst(-1)))
            {
                MakeSrcContained(tree, tree->gtOp.gtOp1);
                info->srcCount = 0;
            }
            else if ((comp->getSIMDInstructionSet() == InstructionSet_AVX) &&
                     ((simdTree->gtSIMDSize == 16) || (simdTree->gtSIMDSize == 32)))
            {
                // Either op1 is a float or dbl constant or an addr
                if (op1->IsCnsFltOrDbl() || op1->OperIsLocalAddr())
                {
                    MakeSrcContained(tree, tree->gtOp.gtOp1);
                    info->srcCount = 0;
                }
            }
        }
        break;

        case SIMDIntrinsicInitN:
        {
            info->srcCount = (short)(simdTree->gtSIMDSize / genTypeSize(simdTree->gtSIMDBaseType));

            // Need an internal register to stitch together all the values into a single vector in a SIMD reg.
            info->internalFloatCount = 1;
            info->setInternalCandidates(lsra, lsra->allSIMDRegs());
        }
        break;

        case SIMDIntrinsicInitArray:
            // We have an array and an index, which may be contained.
            info->srcCount = 2;
            CheckImmedAndMakeContained(tree, tree->gtGetOp2());
            break;

        case SIMDIntrinsicDiv:
            // SSE2 has no instruction support for division on integer vectors
            noway_assert(varTypeIsFloating(simdTree->gtSIMDBaseType));
            info->srcCount = 2;
            break;

        case SIMDIntrinsicAbs:
            // float/double vectors: This gets implemented as bitwise-And operation
            // with a mask and hence should never see  here.
            //
            // Must be a Vector<int> or Vector<short> Vector<sbyte>
            assert(simdTree->gtSIMDBaseType == TYP_INT || simdTree->gtSIMDBaseType == TYP_SHORT ||
                   simdTree->gtSIMDBaseType == TYP_BYTE);
            assert(comp->getSIMDInstructionSet() >= InstructionSet_SSE3_4);
            info->srcCount = 1;
            break;

        case SIMDIntrinsicSqrt:
            // SSE2 has no instruction support for sqrt on integer vectors.
            noway_assert(varTypeIsFloating(simdTree->gtSIMDBaseType));
            info->srcCount = 1;
            break;

        case SIMDIntrinsicAdd:
        case SIMDIntrinsicSub:
        case SIMDIntrinsicMul:
        case SIMDIntrinsicBitwiseAnd:
        case SIMDIntrinsicBitwiseAndNot:
        case SIMDIntrinsicBitwiseOr:
        case SIMDIntrinsicBitwiseXor:
        case SIMDIntrinsicMin:
        case SIMDIntrinsicMax:
            info->srcCount = 2;

            // SSE2 32-bit integer multiplication requires two temp regs
            if (simdTree->gtSIMDIntrinsicID == SIMDIntrinsicMul && simdTree->gtSIMDBaseType == TYP_INT &&
                comp->getSIMDInstructionSet() == InstructionSet_SSE2)
            {
                info->internalFloatCount = 2;
                info->setInternalCandidates(lsra, lsra->allSIMDRegs());
            }
            break;

        case SIMDIntrinsicEqual:
            info->srcCount = 2;
            break;

        // SSE2 doesn't support < and <= directly on int vectors.
        // Instead we need to use > and >= with swapped operands.
        case SIMDIntrinsicLessThan:
        case SIMDIntrinsicLessThanOrEqual:
            info->srcCount = 2;
            noway_assert(!varTypeIsIntegral(simdTree->gtSIMDBaseType));
            break;

        // SIMDIntrinsicEqual is supported only on non-floating point base type vectors.
        // SSE2 cmpps/pd doesn't support > and >=  directly on float/double vectors.
        // Instead we need to use <  and <= with swapped operands.
        case SIMDIntrinsicGreaterThan:
            noway_assert(!varTypeIsFloating(simdTree->gtSIMDBaseType));
            info->srcCount = 2;
            break;

        case SIMDIntrinsicOpEquality:
        case SIMDIntrinsicOpInEquality:
            info->srcCount = 2;

            // On SSE4/AVX, we can generate optimal code for (in)equality
            // against zero using ptest. We can safely do the this optimization
            // for integral vectors but not for floating-point for the reason
            // that we have +0.0 and -0.0 and +0.0 == -0.0
            op2 = tree->gtGetOp2();
            if ((comp->getSIMDInstructionSet() >= InstructionSet_SSE3_4) && op2->IsIntegralConstVector(0))
            {
                MakeSrcContained(tree, op2);
            }
            else
            {

                // Need one SIMD register as scratch.
                // See genSIMDIntrinsicRelOp() for details on code sequence generated and
                // the need for one scratch register.
                //
                // Note these intrinsics produce a BOOL result, hence internal float
                // registers reserved are guaranteed to be different from target
                // integer register without explicitly specifying.
                info->internalFloatCount = 1;
                info->setInternalCandidates(lsra, lsra->allSIMDRegs());
            }
            break;

        case SIMDIntrinsicDotProduct:
            // Float/Double vectors:
            // For SSE, or AVX with 32-byte vectors, we also need an internal register
            // as scratch. Further we need the targetReg and internal reg to be distinct
            // registers. Note that if this is a TYP_SIMD16 or smaller on AVX, then we
            // don't need a tmpReg.
            //
            // 32-byte integer vector on SSE4/AVX:
            // will take advantage of phaddd, which operates only on 128-bit xmm reg.
            // This will need 1 (in case of SSE4) or 2 (in case of AVX) internal
            // registers since targetReg is an int type register.
            //
            // See genSIMDIntrinsicDotProduct() for details on code sequence generated
            // and the need for scratch registers.
            if (varTypeIsFloating(simdTree->gtSIMDBaseType))
            {
                if ((comp->getSIMDInstructionSet() == InstructionSet_SSE2) ||
                    (simdTree->gtOp.gtOp1->TypeGet() == TYP_SIMD32))
                {
                    info->internalFloatCount     = 1;
                    info->isInternalRegDelayFree = true;
                    info->setInternalCandidates(lsra, lsra->allSIMDRegs());
                }
                // else don't need scratch reg(s).
            }
            else
            {
                assert(simdTree->gtSIMDBaseType == TYP_INT && comp->getSIMDInstructionSet() >= InstructionSet_SSE3_4);

                // No need to set isInternalRegDelayFree since targetReg is a
                // an int type reg and guaranteed to be different from xmm/ymm
                // regs.
                info->internalFloatCount = comp->canUseAVX() ? 2 : 1;
                info->setInternalCandidates(lsra, lsra->allSIMDRegs());
            }
            info->srcCount = 2;
            break;

        case SIMDIntrinsicGetItem:
        {
            // This implements get_Item method. The sources are:
            //  - the source SIMD struct
            //  - index (which element to get)
            // The result is baseType of SIMD struct.
            info->srcCount = 2;
            op1            = tree->gtOp.gtOp1;
            op2            = tree->gtOp.gtOp2;

            // If the index is a constant, mark it as contained.
            if (CheckImmedAndMakeContained(tree, op2))
            {
                info->srcCount = 1;
            }

            if (op1->isMemoryOp())
            {
                MakeSrcContained(tree, op1);

                // Although GT_IND of TYP_SIMD12 reserves an internal float
                // register for reading 4 and 8 bytes from memory and
                // assembling them into target XMM reg, it is not required
                // in this case.
                op1->gtLsraInfo.internalIntCount   = 0;
                op1->gtLsraInfo.internalFloatCount = 0;
            }
            else
            {
                // If the index is not a constant, we will use the SIMD temp location to store the vector.
                // Otherwise, if the baseType is floating point, the targetReg will be a xmm reg and we
                // can use that in the process of extracting the element.
                //
                // If the index is a constant and base type is a small int we can use pextrw, but on AVX
                // we will need a temp if are indexing into the upper half of the AVX register.
                // In all other cases with constant index, we need a temp xmm register to extract the
                // element if index is other than zero.

                if (!op2->IsCnsIntOrI())
                {
                    (void)comp->getSIMDInitTempVarNum();
                }
                else if (!varTypeIsFloating(simdTree->gtSIMDBaseType))
                {
                    bool needFloatTemp;
                    if (varTypeIsSmallInt(simdTree->gtSIMDBaseType) &&
                        (comp->getSIMDInstructionSet() == InstructionSet_AVX))
                    {
                        int byteShiftCnt = (int)op2->AsIntCon()->gtIconVal * genTypeSize(simdTree->gtSIMDBaseType);
                        needFloatTemp    = (byteShiftCnt >= 16);
                    }
                    else
                    {
                        needFloatTemp = !op2->IsIntegralConst(0);
                    }

                    if (needFloatTemp)
                    {
                        info->internalFloatCount = 1;
                        info->setInternalCandidates(lsra, lsra->allSIMDRegs());
                    }
                }
            }
        }
        break;

        case SIMDIntrinsicSetX:
        case SIMDIntrinsicSetY:
        case SIMDIntrinsicSetZ:
        case SIMDIntrinsicSetW:
            info->srcCount = 2;

            // We need an internal integer register for SSE2 codegen
            if (comp->getSIMDInstructionSet() == InstructionSet_SSE2)
            {
                info->internalIntCount = 1;
                info->setInternalCandidates(lsra, lsra->allRegs(TYP_INT));
            }

            break;

        case SIMDIntrinsicCast:
        case SIMDIntrinsicConvertToSingle:
        case SIMDIntrinsicConvertToInt32:
            info->srcCount = 1;
            break;

        case SIMDIntrinsicWiden:
        case SIMDIntrinsicWidenHi:
            if (varTypeIsIntegral(simdTree->gtSIMDBaseType))
            {
                // We need an internal register different from targetReg.
                info->isInternalRegDelayFree = true;
                info->internalFloatCount = 1;
                info->setInternalCandidates(lsra, lsra->allSIMDRegs());
            }
            info->srcCount = 1;
            break;

        case SIMDIntrinsicConvertToInt64:
        case SIMDIntrinsicConvertToDouble:
            // For this case, we need a tmpReg different from targetReg.
            info->isInternalRegDelayFree = true;
            info->srcCount = 1;
            info->internalIntCount = 1;
            if (comp->getSIMDInstructionSet() == InstructionSet_AVX)
            {
                info->internalFloatCount = 2;
            }
            else
            {
                info->internalFloatCount = 1;
            }
            info->setInternalCandidates(lsra, lsra->allSIMDRegs() | lsra->allRegs(TYP_INT));
            break;

        case SIMDIntrinsicNarrow:
            info->srcCount = 2;
            if ((comp->getSIMDInstructionSet() == InstructionSet_AVX) && varTypeIsLong(simdTree->gtSIMDBaseType))
            {
                info->internalFloatCount = 2;
            }
            else
            {
                // We need an internal register different from targetReg.
                info->isInternalRegDelayFree = true;
                info->internalFloatCount = 1;
            }
            info->setInternalCandidates(lsra, lsra->allSIMDRegs());
            break;

        case SIMDIntrinsicShuffleSSE2:
            info->srcCount = 2;
            // Second operand is an integer constant and marked as contained.
            op2 = tree->gtOp.gtOp2;
            noway_assert(op2->IsCnsIntOrI());
            MakeSrcContained(tree, op2);
            break;

        case SIMDIntrinsicGetX:
        case SIMDIntrinsicGetY:
        case SIMDIntrinsicGetZ:
        case SIMDIntrinsicGetW:
        case SIMDIntrinsicGetOne:
        case SIMDIntrinsicGetZero:
        case SIMDIntrinsicGetCount:
        case SIMDIntrinsicGetAllOnes:
            assert(!"Get intrinsics should not be seen during Lowering.");
            unreached();

        default:
            noway_assert(!"Unimplemented SIMD node type.");
            unreached();
    }
}
#endif // FEATURE_SIMD

//------------------------------------------------------------------------
// TreeNodeInfoInitCast: Set the NodeInfo for a GT_CAST.
//
// Arguments:
//    tree      - The node of interest
//
// Return Value:
//    None.
//
void Lowering::TreeNodeInfoInitCast(GenTree* tree)
{
    TreeNodeInfo* info = &(tree->gtLsraInfo);

    // TODO-XArch-CQ: Int-To-Int conversions - castOp cannot be a memory op and must have an assigned register.
    //         see CodeGen::genIntToIntCast()

    info->srcCount = 1;
    info->dstCount = 1;

    // Non-overflow casts to/from float/double are done using SSE2 instructions
    // and that allow the source operand to be either a reg or memop. Given the
    // fact that casts from small int to float/double are done as two-level casts,
    // the source operand is always guaranteed to be of size 4 or 8 bytes.
    var_types  castToType = tree->CastToType();
    GenTreePtr castOp     = tree->gtCast.CastOp();
    var_types  castOpType = castOp->TypeGet();
    if (tree->gtFlags & GTF_UNSIGNED)
    {
        castOpType = genUnsignedType(castOpType);
    }

    if (!tree->gtOverflow() && (varTypeIsFloating(castToType) || varTypeIsFloating(castOpType)))
    {
#ifdef DEBUG
        // If converting to float/double, the operand must be 4 or 8 byte in size.
        if (varTypeIsFloating(castToType))
        {
            unsigned opSize = genTypeSize(castOpType);
            assert(opSize == 4 || opSize == 8);
        }
#endif // DEBUG

        // U8 -> R8 conversion requires that the operand be in a register.
        if (castOpType != TYP_ULONG)
        {
            if (castOp->isMemoryOp() || castOp->IsCnsNonZeroFltOrDbl())
            {
                MakeSrcContained(tree, castOp);
            }
            else
            {
                // Mark castOp as reg optional to indicate codegen
                // can still generate code if it is on stack.
                SetRegOptional(castOp);
            }
        }
    }

#if !defined(_TARGET_64BIT_)
    if (varTypeIsLong(castOpType))
    {
        noway_assert(castOp->OperGet() == GT_LONG);
        info->srcCount = 2;
    }
#endif // !defined(_TARGET_64BIT_)

    // some overflow checks need a temp reg:
    //  - GT_CAST from INT64/UINT64 to UINT32
    if (tree->gtOverflow() && (castToType == TYP_UINT))
    {
        if (genTypeSize(castOpType) == 8)
        {
            // Here we don't need internal register to be different from targetReg,
            // rather require it to be different from operand's reg.
            info->internalIntCount = 1;
        }
    }
}

void Lowering::LowerGCWriteBarrier(GenTree* tree)
{
    assert(tree->OperGet() == GT_STOREIND);

    GenTreeStoreInd* dst  = tree->AsStoreInd();
    GenTreePtr       addr = dst->Addr();
    GenTreePtr       src  = dst->Data();

    if (addr->OperGet() == GT_LEA)
    {
        // In the case where we are doing a helper assignment, if the dst
        // is an indir through an lea, we need to actually instantiate the
        // lea in a register
        GenTreeAddrMode* lea = addr->AsAddrMode();

        int leaSrcCount = 0;
        if (lea->HasBase())
        {
            leaSrcCount++;
        }
        if (lea->HasIndex())
        {
            leaSrcCount++;
        }
        lea->gtLsraInfo.srcCount = leaSrcCount;
        lea->gtLsraInfo.dstCount = 1;
    }

    bool useOptimizedWriteBarrierHelper = false; // By default, assume no optimized write barriers.

#if NOGC_WRITE_BARRIERS

#if defined(_TARGET_X86_)

    useOptimizedWriteBarrierHelper = true; // On x86, use the optimized write barriers by default.
#ifdef DEBUG
    GCInfo::WriteBarrierForm wbf = comp->codeGen->gcInfo.gcIsWriteBarrierCandidate(tree, src);
    if (wbf == GCInfo::WBF_NoBarrier_CheckNotHeapInDebug) // This one is always a call to a C++ method.
    {
        useOptimizedWriteBarrierHelper = false;
    }
#endif

    if (useOptimizedWriteBarrierHelper)
    {
        // Special write barrier:
        // op1 (addr) goes into REG_WRITE_BARRIER (rdx) and
        // op2 (src) goes into any int register.
        addr->gtLsraInfo.setSrcCandidates(m_lsra, RBM_WRITE_BARRIER);
        src->gtLsraInfo.setSrcCandidates(m_lsra, RBM_WRITE_BARRIER_SRC);
    }

#else // !defined(_TARGET_X86_)
#error "NOGC_WRITE_BARRIERS is not supported"
#endif // !defined(_TARGET_X86_)

#endif // NOGC_WRITE_BARRIERS

    if (!useOptimizedWriteBarrierHelper)
    {
        // For the standard JIT Helper calls:
        // op1 (addr) goes into REG_ARG_0 and
        // op2 (src) goes into REG_ARG_1
        addr->gtLsraInfo.setSrcCandidates(m_lsra, RBM_ARG_0);
        src->gtLsraInfo.setSrcCandidates(m_lsra, RBM_ARG_1);
    }

    // Both src and dst must reside in a register, which they should since we haven't set
    // either of them as contained.
    assert(addr->gtLsraInfo.dstCount == 1);
    assert(src->gtLsraInfo.dstCount == 1);
}

//-----------------------------------------------------------------------------------------
// Specify register requirements for address expression of an indirection operation.
//
// Arguments:
//    indirTree    -   GT_IND or GT_STOREIND gentree node
//
void Lowering::SetIndirAddrOpCounts(GenTreePtr indirTree)
{
    assert(indirTree->isIndir());
    // If this is the rhs of a block copy (i.e. non-enregisterable struct),
    // it has no register requirements.
    if (indirTree->TypeGet() == TYP_STRUCT)
    {
        return;
    }

    GenTreePtr    addr = indirTree->gtGetOp1();
    TreeNodeInfo* info = &(indirTree->gtLsraInfo);

    GenTreePtr base  = nullptr;
    GenTreePtr index = nullptr;
    unsigned   mul, cns;
    bool       rev;

#ifdef FEATURE_SIMD
    // If indirTree is of TYP_SIMD12, don't mark addr as contained
    // so that it always get computed to a register.  This would
    // mean codegen side logic doesn't need to handle all possible
    // addr expressions that could be contained.
    //
    // TODO-XArch-CQ: handle other addr mode expressions that could be marked
    // as contained.
    if (indirTree->TypeGet() == TYP_SIMD12)
    {
        // Vector3 is read/written as two reads/writes: 8 byte and 4 byte.
        // To assemble the vector properly we would need an additional
        // XMM register.
        info->internalFloatCount = 1;

        // In case of GT_IND we need an internal register different from targetReg and
        // both of the registers are used at the same time.
        if (indirTree->OperGet() == GT_IND)
        {
            info->isInternalRegDelayFree = true;
        }

        info->setInternalCandidates(m_lsra, m_lsra->allSIMDRegs());

        return;
    }
#endif // FEATURE_SIMD

    if ((indirTree->gtFlags & GTF_IND_REQ_ADDR_IN_REG) != 0)
    {
        // The address of an indirection that requires its address in a reg.
        // Skip any further processing that might otherwise make it contained.
    }
    else if ((addr->OperGet() == GT_CLS_VAR_ADDR) || (addr->OperGet() == GT_LCL_VAR_ADDR))
    {
        // These nodes go into an addr mode:
        // - GT_CLS_VAR_ADDR turns into a constant.
        // - GT_LCL_VAR_ADDR is a stack addr mode.

        // make this contained, it turns into a constant that goes into an addr mode
        MakeSrcContained(indirTree, addr);
    }
    else if (addr->IsCnsIntOrI() && addr->AsIntConCommon()->FitsInAddrBase(comp))
    {
        // Amd64:
        // We can mark any pc-relative 32-bit addr as containable, except for a direct VSD call address.
        // (i.e. those VSD calls for which stub addr is known during JIT compilation time).  In this case,
        // VM requires us to pass stub addr in REG_VIRTUAL_STUB_PARAM - see LowerVirtualStubCall().  For
        // that reason we cannot mark such an addr as contained.  Note that this is not an issue for
        // indirect VSD calls since morphArgs() is explicitly materializing hidden param as a non-standard
        // argument.
        //
        // Workaround:
        // Note that LowerVirtualStubCall() sets addr->gtRegNum to REG_VIRTUAL_STUB_PARAM and Lowering::doPhase()
        // sets destination candidates on such nodes and resets addr->gtRegNum to REG_NA before calling
        // TreeNodeInfoInit(). Ideally we should set a flag on addr nodes that shouldn't be marked as contained
        // (in LowerVirtualStubCall()), but we don't have any GTF_* flags left for that purpose.  As a workaround
        // an explicit check is made here.
        //
        // On x86, direct VSD is done via a relative branch, and in fact it MUST be contained.
        MakeSrcContained(indirTree, addr);
    }
    else if ((addr->OperGet() == GT_LEA) && IsSafeToContainMem(indirTree, addr))
    {
        MakeSrcContained(indirTree, addr);
    }
    else if (comp->codeGen->genCreateAddrMode(addr, -1, true, 0, &rev, &base, &index, &mul, &cns, true /*nogen*/) &&
             !AreSourcesPossiblyModifiedLocals(indirTree, base, index))
    {
        // An addressing mode will be constructed that may cause some
        // nodes to not need a register, and cause others' lifetimes to be extended
        // to the GT_IND or even its parent if it's an assignment

        assert(base != addr);
        m_lsra->clearOperandCounts(addr);

        const bool hasBase  = base != nullptr;
        const bool hasIndex = index != nullptr;
        assert(hasBase || hasIndex); // At least one of a base or an index must be present.

        // If the addressing mode has both a base and an index, bump its source count by one. If it only has one or the
        // other, its source count is already correct (due to the source for the address itself).
        if (hasBase && hasIndex)
        {
            info->srcCount++;
        }

        // Traverse the computation below GT_IND to find the operands
        // for the addressing mode, marking the various constants and
        // intermediate results as not consuming/producing.
        // If the traversal were more complex, we might consider using
        // a traversal function, but the addressing mode is only made
        // up of simple arithmetic operators, and the code generator
        // only traverses one leg of each node.

        bool foundBase  = !hasBase;
        bool foundIndex = !hasIndex;
        for (GenTree *child = addr, *nextChild = nullptr; child != nullptr && !child->OperIsLeaf(); child = nextChild)
        {
            nextChild    = nullptr;
            GenTree* op1 = child->gtOp.gtOp1;
            GenTree* op2 = (child->OperIsBinary()) ? child->gtOp.gtOp2 : nullptr;

            if (op1 == base)
            {
                foundBase = true;
            }
            else if (op1 == index)
            {
                foundIndex = true;
            }
            else
            {
                m_lsra->clearOperandCounts(op1);
                if (!op1->OperIsLeaf())
                {
                    nextChild = op1;
                }
            }

            if (op2 != nullptr)
            {
                if (op2 == base)
                {
                    foundBase = true;
                }
                else if (op2 == index)
                {
                    foundIndex = true;
                }
                else
                {
                    m_lsra->clearOperandCounts(op2);
                    if (!op2->OperIsLeaf())
                    {
                        assert(nextChild == nullptr);
                        nextChild = op2;
                    }
                }
            }
        }
        assert(foundBase && foundIndex);
    }
    else if (addr->gtOper == GT_ARR_ELEM)
    {
        // The GT_ARR_ELEM consumes all the indices and produces the offset.
        // The array object lives until the mem access.
        // We also consume the target register to which the address is
        // computed

        info->srcCount++;
        assert(addr->gtLsraInfo.srcCount >= 2);
        addr->gtLsraInfo.srcCount -= 1;
    }
}

void Lowering::TreeNodeInfoInitCmp(GenTreePtr tree)
{
    assert(tree->OperIsCompare());

    TreeNodeInfo* info = &(tree->gtLsraInfo);

    info->srcCount = 2;
    info->dstCount = 1;

#ifdef _TARGET_X86_
    // If the compare is used by a jump, we just need to set the condition codes. If not, then we need
    // to store the result into the low byte of a register, which requires the dst be a byteable register.
    // We always set the dst candidates, though, because if this is compare is consumed by a jump, they
    // won't be used. We might be able to use GTF_RELOP_JMP_USED to determine this case, but it's not clear
    // that flag is maintained until this location (especially for decomposed long compares).
    info->setDstCandidates(m_lsra, RBM_BYTE_REGS);
#endif // _TARGET_X86_

    GenTreePtr op1     = tree->gtOp.gtOp1;
    GenTreePtr op2     = tree->gtOp.gtOp2;
    var_types  op1Type = op1->TypeGet();
    var_types  op2Type = op2->TypeGet();

#if !defined(_TARGET_64BIT_)
    // Long compares will consume GT_LONG nodes, each of which produces two results.
    // Thus for each long operand there will be an additional source.
    // TODO-X86-CQ: Mark hiOp2 and loOp2 as contained if it is a constant or a memory op.
    if (varTypeIsLong(op1Type))
    {
        info->srcCount++;
    }
    if (varTypeIsLong(op2Type))
    {
        info->srcCount++;
    }
#endif // !defined(_TARGET_64BIT_)

    // If either of op1 or op2 is floating point values, then we need to use
    // ucomiss or ucomisd to compare, both of which support the following form:
    //     ucomis[s|d] xmm, xmm/mem
    // That is only the second operand can be a memory op.
    //
    // Second operand is a memory Op:  Note that depending on comparison operator,
    // the operands of ucomis[s|d] need to be reversed.  Therefore, either op1 or
    // op2 can be a memory op depending on the comparison operator.
    if (varTypeIsFloating(op1Type))
    {
        // The type of the operands has to be the same and no implicit conversions at this stage.
        assert(op1Type == op2Type);

        bool reverseOps;
        if ((tree->gtFlags & GTF_RELOP_NAN_UN) != 0)
        {
            // Unordered comparison case
            reverseOps = (tree->gtOper == GT_GT || tree->gtOper == GT_GE);
        }
        else
        {
            reverseOps = (tree->gtOper == GT_LT || tree->gtOper == GT_LE);
        }

        GenTreePtr otherOp;
        if (reverseOps)
        {
            otherOp = op1;
        }
        else
        {
            otherOp = op2;
        }

        assert(otherOp != nullptr);
        if (otherOp->IsCnsNonZeroFltOrDbl())
        {
            MakeSrcContained(tree, otherOp);
        }
        else if (otherOp->isMemoryOp() && ((otherOp == op2) || IsSafeToContainMem(tree, otherOp)))
        {
            MakeSrcContained(tree, otherOp);
        }
        else
        {
            // SSE2 allows only otherOp to be a memory-op. Since otherOp is not
            // contained, we can mark it reg-optional.
            SetRegOptional(otherOp);
        }

        return;
    }

    // TODO-XArch-CQ: factor out cmp optimization in 'genCondSetFlags' to be used here
    // or in other backend.

    bool hasShortCast = false;
    if (CheckImmedAndMakeContained(tree, op2))
    {
        // If the types are the same, or if the constant is of the correct size,
        // we can treat the isMemoryOp as contained.
        bool op1CanBeContained = (genTypeSize(op1Type) == genTypeSize(op2Type));

        // Do we have a short compare against a constant in op2
        //
        if (varTypeIsSmall(op1Type))
        {
            GenTreeIntCon* con  = op2->AsIntCon();
            ssize_t        ival = con->gtIconVal;

            bool isEqualityCompare = (tree->gtOper == GT_EQ || tree->gtOper == GT_NE);
            bool useTest           = isEqualityCompare && (ival == 0);

            if (!useTest)
            {
                ssize_t lo         = 0; // minimum imm value allowed for cmp reg,imm
                ssize_t hi         = 0; // maximum imm value allowed for cmp reg,imm
                bool    isUnsigned = false;

                switch (op1Type)
                {
                    case TYP_BOOL:
                        op1Type = TYP_UBYTE;
                        __fallthrough;
                    case TYP_UBYTE:
                        lo         = 0;
                        hi         = 0x7f;
                        isUnsigned = true;
                        break;
                    case TYP_BYTE:
                        lo = -0x80;
                        hi = 0x7f;
                        break;
                    case TYP_CHAR:
                        lo         = 0;
                        hi         = 0x7fff;
                        isUnsigned = true;
                        break;
                    case TYP_SHORT:
                        lo = -0x8000;
                        hi = 0x7fff;
                        break;
                    default:
                        unreached();
                }

                if ((ival >= lo) && (ival <= hi))
                {
                    // We can perform a small compare with the immediate 'ival'
                    tree->gtFlags |= GTF_RELOP_SMALL;
                    if (isUnsigned && !isEqualityCompare)
                    {
                        tree->gtFlags |= GTF_UNSIGNED;
                    }
                    // We can treat the isMemoryOp as "contained"
                    op1CanBeContained = true;
                }
            }
        }

        if (op1CanBeContained)
        {
            if (op1->isMemoryOp())
            {
                MakeSrcContained(tree, op1);
            }
            else
            {
                bool op1IsMadeContained = false;

                // When op1 is a GT_AND we can often generate a single "test" instruction
                // instead of two instructions (an "and" instruction followed by a "cmp"/"test").
                //
                // This instruction can only be used for equality or inequality comparisons.
                // and we must have a compare against zero.
                //
                // If we have a postive test for a single bit we can reverse the condition and
                // make the compare be against zero.
                //
                // Example:
                //                  GT_EQ                              GT_NE
                //                  /   \                              /   \
                //             GT_AND   GT_CNS (0x100)  ==>>      GT_AND   GT_CNS (0)
                //             /    \                             /    \
                //          andOp1  GT_CNS (0x100)             andOp1  GT_CNS (0x100)
                //
                // We will mark the GT_AND node as contained if the tree is an equality compare with zero.
                // Additionally, when we do this we also allow for a contained memory operand for "andOp1".
                //
                bool isEqualityCompare = (tree->gtOper == GT_EQ || tree->gtOper == GT_NE);

                if (isEqualityCompare && (op1->OperGet() == GT_AND))
                {
                    GenTreePtr andOp2 = op1->gtOp.gtOp2;
                    if (IsContainableImmed(op1, andOp2))
                    {
                        ssize_t andOp2CnsVal = andOp2->AsIntConCommon()->IconValue();
                        ssize_t relOp2CnsVal = op2->AsIntConCommon()->IconValue();

                        if ((relOp2CnsVal == andOp2CnsVal) && isPow2(andOp2CnsVal))
                        {
                            // We have a single bit test, so now we can change the
                            // tree into the alternative form,
                            // so that we can generate a test instruction.

                            // Reverse the equality comparison
                            tree->SetOperRaw((tree->gtOper == GT_EQ) ? GT_NE : GT_EQ);

                            // Change the relOp2CnsVal to zero
                            relOp2CnsVal = 0;
                            op2->AsIntConCommon()->SetIconValue(0);
                        }

                        // Now do we have a equality compare with zero?
                        //
                        if (relOp2CnsVal == 0)
                        {
                            // Note that child nodes must be made contained before parent nodes

                            // Check for a memory operand for op1 with the test instruction
                            //
                            GenTreePtr andOp1 = op1->gtOp.gtOp1;
                            if (andOp1->isMemoryOp())
                            {
                                // If the type of value memoryOp (andOp1) is not the same as the type of constant
                                // (andOp2) check to see whether it is safe to mark AndOp1 as contained.  For e.g. in
                                // the following case it is not safe to mark andOp1 as contained
                                //    AndOp1 = signed byte and andOp2 is an int constant of value 512.
                                //
                                // If it is safe, we update the type and value of andOp2 to match with andOp1.
                                bool containable = (andOp1->TypeGet() == op1->TypeGet());
                                if (!containable)
                                {
                                    ssize_t newIconVal = 0;

                                    switch (andOp1->TypeGet())
                                    {
                                        default:
                                            break;
                                        case TYP_BYTE:
                                            newIconVal  = (signed char)andOp2CnsVal;
                                            containable = FitsIn<signed char>(andOp2CnsVal);
                                            break;
                                        case TYP_BOOL:
                                        case TYP_UBYTE:
                                            newIconVal  = andOp2CnsVal & 0xFF;
                                            containable = true;
                                            break;
                                        case TYP_SHORT:
                                            newIconVal  = (signed short)andOp2CnsVal;
                                            containable = FitsIn<signed short>(andOp2CnsVal);
                                            break;
                                        case TYP_CHAR:
                                            newIconVal  = andOp2CnsVal & 0xFFFF;
                                            containable = true;
                                            break;
                                        case TYP_INT:
                                            newIconVal  = (INT32)andOp2CnsVal;
                                            containable = FitsIn<INT32>(andOp2CnsVal);
                                            break;
                                        case TYP_UINT:
                                            newIconVal  = andOp2CnsVal & 0xFFFFFFFF;
                                            containable = true;
                                            break;

#ifdef _TARGET_64BIT_
                                        case TYP_LONG:
                                            newIconVal  = (INT64)andOp2CnsVal;
                                            containable = true;
                                            break;
                                        case TYP_ULONG:
                                            newIconVal  = (UINT64)andOp2CnsVal;
                                            containable = true;
                                            break;
#endif //_TARGET_64BIT_
                                    }

                                    if (containable)
                                    {
                                        andOp2->gtType = andOp1->TypeGet();
                                        andOp2->AsIntConCommon()->SetIconValue(newIconVal);
                                    }
                                }

                                // Mark the 'andOp1' memory operand as contained
                                // Note that for equality comparisons we don't need
                                // to deal with any signed or unsigned issues.
                                if (containable)
                                {
                                    MakeSrcContained(op1, andOp1);
                                }
                            }
                            // Mark the 'op1' (the GT_AND) operand as contained
                            MakeSrcContained(tree, op1);
                            op1IsMadeContained = true;

                            // During Codegen we will now generate "test andOp1, andOp2CnsVal"
                        }
                    }
                }
                else if (op1->OperGet() == GT_CAST)
                {
                    // If the op1 is a cast operation, and cast type is one byte sized unsigned type,
                    // we can directly use the number in register, instead of doing an extra cast step.
                    var_types  dstType       = op1->CastToType();
                    bool       isUnsignedDst = varTypeIsUnsigned(dstType);
                    emitAttr   castSize      = EA_ATTR(genTypeSize(dstType));
                    GenTreePtr castOp1       = op1->gtOp.gtOp1;
                    genTreeOps castOp1Oper   = castOp1->OperGet();
                    bool       safeOper      = false;

                    // It is not always safe to change the gtType of 'castOp1' to TYP_UBYTE.
                    // For example when 'castOp1Oper' is a GT_RSZ or GT_RSH then we are shifting
                    // bits from the left into the lower bits.  If we change the type to a TYP_UBYTE
                    // we will instead generate a byte sized shift operation:  shr  al, 24
                    // For the following ALU operations is it safe to change the gtType to the
                    // smaller type:
                    //
                    if ((castOp1Oper == GT_CNS_INT) || (castOp1Oper == GT_CALL) || // the return value from a Call
                        (castOp1Oper == GT_LCL_VAR) || castOp1->OperIsLogical() || // GT_AND, GT_OR, GT_XOR
                        castOp1->isMemoryOp())                                     // isIndir() || isLclField();
                    {
                        safeOper = true;
                    }

                    if ((castSize == EA_1BYTE) && isUnsignedDst && // Unsigned cast to TYP_UBYTE
                        safeOper &&                                // Must be a safe operation
                        !op1->gtOverflow())                        // Must not be an overflow checking cast
                    {
                        // Currently all of the Oper accepted as 'safeOper' are
                        // non-overflow checking operations.  If we were to add
                        // an overflow checking operation then this assert needs
                        // to be moved above to guard entry to this block.
                        //
                        assert(!castOp1->gtOverflowEx()); // Must not be an overflow checking operation

                        // TODO-Cleanup: we're within "if (CheckImmedAndMakeContained(tree, op2))", so isn't
                        // the following condition always true?
                        if (op2->isContainedIntOrIImmed())
                        {
                            ssize_t val = (ssize_t)op2->AsIntConCommon()->IconValue();
                            if (val >= 0 && val <= 255)
                            {
                                GenTreePtr removeTreeNode = op1;
                                tree->gtOp.gtOp1          = castOp1;
                                op1                       = castOp1;
                                castOp1->gtType           = TYP_UBYTE;

                                // trim down the value if castOp1 is an int constant since its type changed to UBYTE.
                                if (castOp1Oper == GT_CNS_INT)
                                {
                                    castOp1->gtIntCon.gtIconVal = (UINT8)castOp1->gtIntCon.gtIconVal;
                                }

                                op2->gtType = TYP_UBYTE;
                                tree->gtFlags |= GTF_UNSIGNED;

                                // right now the op1's type is the same as op2's type.
                                // if op1 is MemoryOp, we should make the op1 as contained node.
                                if (castOp1->isMemoryOp())
                                {
                                    MakeSrcContained(tree, op1);
                                    op1IsMadeContained = true;
                                }

                                BlockRange().Remove(removeTreeNode);

                                // We've changed the type on op1 to TYP_UBYTE, but we already processed that node.
                                // We need to go back and mark it byteable.
                                // TODO-Cleanup: it might be better to move this out of the TreeNodeInfoInit pass to
                                // the earlier "lower" pass, in which case the byteable check would just fall out.
                                // But that is quite complex!
                                TreeNodeInfoInitCheckByteable(op1);

#ifdef DEBUG
                                if (comp->verbose)
                                {
                                    printf("TreeNodeInfoInitCmp: Removing a GT_CAST to TYP_UBYTE and changing "
                                           "castOp1->gtType to TYP_UBYTE\n");
                                    comp->gtDispTreeRange(BlockRange(), tree);
                                }
#endif
                            }
                        }
                    }
                }

                // If not made contained, op1 can be marked as reg-optional.
                if (!op1IsMadeContained)
                {
                    SetRegOptional(op1);

                    // If op1 codegen sets ZF and SF flags and ==/!= against
                    // zero, we don't need to generate test instruction,
                    // provided we don't have another GenTree node between op1
                    // and tree that could potentially modify flags.
                    //
                    // TODO-CQ: right now the below peep is inexpensive and
                    // gets the benefit in most of cases because in majority
                    // of cases op1, op2 and tree would be in that order in
                    // execution.  In general we should be able to check that all
                    // the nodes that come after op1 in execution order do not
                    // modify the flags so that it is safe to avoid generating a
                    // test instruction.  Such a check requires that on each
                    // GenTree node we need to set the info whether its codegen
                    // will modify flags.
                    //
                    // TODO-CQ: We can optimize compare against zero in the
                    // following cases by generating the branch as indicated
                    // against each case.
                    //  1) unsigned compare
                    //        < 0  - always FALSE
                    //       <= 0  - ZF=1 and jne
                    //        > 0  - ZF=0 and je
                    //       >= 0  - always TRUE
                    //
                    // 2) signed compare
                    //        < 0  - SF=1 and js
                    //       >= 0  - SF=0 and jns
                    if (isEqualityCompare && op1->gtSetZSFlags() && op2->IsIntegralConst(0) && (op1->gtNext == op2) &&
                        (op2->gtNext == tree))
                    {
                        // Require codegen of op1 to set the flags.
                        assert(!op1->gtSetFlags());
                        op1->gtFlags |= GTF_SET_FLAGS;
                    }
                }
            }
        }
    }
    else if (op1Type == op2Type)
    {
        if (op2->isMemoryOp())
        {
            MakeSrcContained(tree, op2);
        }
        else if (op1->isMemoryOp() && IsSafeToContainMem(tree, op1))
        {
            MakeSrcContained(tree, op1);
        }
        else if (op1->IsCnsIntOrI())
        {
            // TODO-CQ: We should be able to support swapping op1 and op2 to generate cmp reg, imm,
            // but there is currently an assert in CodeGen::genCompareInt().
            // https://github.com/dotnet/coreclr/issues/7270
            SetRegOptional(op2);
        }
        else
        {
            // One of op1 or op2 could be marked as reg optional
            // to indicate that codegen can still generate code
            // if one of them is on stack.
            SetRegOptional(PreferredRegOptionalOperand(tree));
        }

        if (varTypeIsSmall(op1Type) && varTypeIsUnsigned(op1Type))
        {
            // Mark the tree as doing unsigned comparison if
            // both the operands are small and unsigned types.
            // Otherwise we will end up performing a signed comparison
            // of two small unsigned values without zero extending them to
            // TYP_INT size and which is incorrect.
            tree->gtFlags |= GTF_UNSIGNED;
        }
    }
}

/* Lower GT_CAST(srcType, DstType) nodes.
 *
 * Casts from small int type to float/double are transformed as follows:
 * GT_CAST(byte, float/double)     =   GT_CAST(GT_CAST(byte, int32), float/double)
 * GT_CAST(sbyte, float/double)    =   GT_CAST(GT_CAST(sbyte, int32), float/double)
 * GT_CAST(int16, float/double)    =   GT_CAST(GT_CAST(int16, int32), float/double)
 * GT_CAST(uint16, float/double)   =   GT_CAST(GT_CAST(uint16, int32), float/double)
 *
 * SSE2 conversion instructions operate on signed integers. casts from Uint32/Uint64
 * are morphed as follows by front-end and hence should not be seen here.
 * GT_CAST(uint32, float/double)   =   GT_CAST(GT_CAST(uint32, long), float/double)
 * GT_CAST(uint64, float)          =   GT_CAST(GT_CAST(uint64, double), float)
 *
 *
 * Similarly casts from float/double to a smaller int type are transformed as follows:
 * GT_CAST(float/double, byte)     =   GT_CAST(GT_CAST(float/double, int32), byte)
 * GT_CAST(float/double, sbyte)    =   GT_CAST(GT_CAST(float/double, int32), sbyte)
 * GT_CAST(float/double, int16)    =   GT_CAST(GT_CAST(double/double, int32), int16)
 * GT_CAST(float/double, uint16)   =   GT_CAST(GT_CAST(double/double, int32), uint16)
 *
 * SSE2 has instructions to convert a float/double vlaue into a signed 32/64-bit
 * integer.  The above transformations help us to leverage those instructions.
 *
 * Note that for the following conversions we still depend on helper calls and
 * don't expect to see them here.
 *  i) GT_CAST(float/double, uint64)
 * ii) GT_CAST(float/double, int type with overflow detection)
 *
 * TODO-XArch-CQ: (Low-pri): Jit64 generates in-line code of 8 instructions for (i) above.
 * There are hardly any occurrences of this conversion operation in platform
 * assemblies or in CQ perf benchmarks (1 occurrence in mscorlib, microsoft.jscript,
 * 1 occurence in Roslyn and no occurrences in system, system.core, system.numerics
 * system.windows.forms, scimark, fractals, bio mums). If we ever find evidence that
 * doing this optimization is a win, should consider generating in-lined code.
 */
void Lowering::LowerCast(GenTree* tree)
{
    assert(tree->OperGet() == GT_CAST);

    GenTreePtr op1     = tree->gtOp.gtOp1;
    var_types  dstType = tree->CastToType();
    var_types  srcType = op1->TypeGet();
    var_types  tmpType = TYP_UNDEF;

    // force the srcType to unsigned if GT_UNSIGNED flag is set
    if (tree->gtFlags & GTF_UNSIGNED)
    {
        srcType = genUnsignedType(srcType);
    }

    // We should never see the following casts as they are expected to be lowered
    // apropriately or converted into helper calls by front-end.
    //   srcType = float/double                    dstType = * and overflow detecting cast
    //       Reason: must be converted to a helper call
    //   srcType = float/double,                   dstType = ulong
    //       Reason: must be converted to a helper call
    //   srcType = uint                            dstType = float/double
    //       Reason: uint -> float/double = uint -> long -> float/double
    //   srcType = ulong                           dstType = float
    //       Reason: ulong -> float = ulong -> double -> float
    if (varTypeIsFloating(srcType))
    {
        noway_assert(!tree->gtOverflow());
        noway_assert(dstType != TYP_ULONG);
    }
    else if (srcType == TYP_UINT)
    {
        noway_assert(!varTypeIsFloating(dstType));
    }
    else if (srcType == TYP_ULONG)
    {
        noway_assert(dstType != TYP_FLOAT);
    }

    // Case of src is a small type and dst is a floating point type.
    if (varTypeIsSmall(srcType) && varTypeIsFloating(dstType))
    {
        // These conversions can never be overflow detecting ones.
        noway_assert(!tree->gtOverflow());
        tmpType = TYP_INT;
    }
    // case of src is a floating point type and dst is a small type.
    else if (varTypeIsFloating(srcType) && varTypeIsSmall(dstType))
    {
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

//----------------------------------------------------------------------------------------------
// Lowering::IsRMWIndirCandidate:
//    Returns true if the given operand is a candidate indirection for a read-modify-write
//    operator.
//
//  Arguments:
//     operand - The operand to consider.
//     storeInd - The indirect store that roots the possible RMW operator.
//
bool Lowering::IsRMWIndirCandidate(GenTree* operand, GenTree* storeInd)
{
    // If the operand isn't an indirection, it's trivially not a candidate.
    if (operand->OperGet() != GT_IND)
    {
        return false;
    }

    // If the indirection's source address isn't equivalent to the destination address of the storeIndir, then the
    // indirection is not a candidate.
    GenTree* srcAddr = operand->gtGetOp1();
    GenTree* dstAddr = storeInd->gtGetOp1();
    if ((srcAddr->OperGet() != dstAddr->OperGet()) || !IndirsAreEquivalent(operand, storeInd))
    {
        return false;
    }

    // If it is not safe to contain the entire tree rooted at the indirection, then the indirection is not a
    // candidate. Crawl the IR from the node immediately preceding the storeIndir until the last node in the
    // indirection's tree is visited and check the side effects at each point.

    m_scratchSideEffects.Clear();

    assert((operand->gtLIRFlags & LIR::Flags::Mark) == 0);
    operand->gtLIRFlags |= LIR::Flags::Mark;

    unsigned markCount = 1;
    GenTree* node;
    for (node = storeInd->gtPrev; markCount > 0; node = node->gtPrev)
    {
        assert(node != nullptr);

        if ((node->gtLIRFlags & LIR::Flags::Mark) == 0)
        {
            m_scratchSideEffects.AddNode(comp, node);
        }
        else
        {
            node->gtLIRFlags &= ~LIR::Flags::Mark;
            markCount--;

            if (m_scratchSideEffects.InterferesWith(comp, node, false))
            {
                // The indirection's tree contains some node that can't be moved to the storeInder. The indirection is
                // not a candidate. Clear any leftover mark bits and return.
                for (; markCount > 0; node = node->gtPrev)
                {
                    if ((node->gtLIRFlags & LIR::Flags::Mark) != 0)
                    {
                        node->gtLIRFlags &= ~LIR::Flags::Mark;
                        markCount--;
                    }
                }
                return false;
            }

            for (GenTree* nodeOperand : node->Operands())
            {
                assert((nodeOperand->gtLIRFlags & LIR::Flags::Mark) == 0);
                nodeOperand->gtLIRFlags |= LIR::Flags::Mark;
                markCount++;
            }
        }
    }

    // At this point we've verified that the operand is an indirection, its address is equivalent to the storeIndir's
    // destination address, and that it and the transitive closure of its operand can be safely contained by the
    // storeIndir. This indirection is therefore a candidate for an RMW op.
    return true;
}

//----------------------------------------------------------------------------------------------
// Returns true if this tree is bin-op of a GT_STOREIND of the following form
//      storeInd(subTreeA, binOp(gtInd(subTreeA), subtreeB)) or
//      storeInd(subTreeA, binOp(subtreeB, gtInd(subTreeA)) in case of commutative bin-ops
//
// The above form for storeInd represents a read-modify-write memory binary operation.
//
// Parameters
//     tree   -   GentreePtr of binOp
//
// Return Value
//     True if 'tree' is part of a RMW memory operation pattern
//
bool Lowering::IsBinOpInRMWStoreInd(GenTreePtr tree)
{
    // Must be a non floating-point type binary operator since SSE2 doesn't support RMW memory ops
    assert(!varTypeIsFloating(tree));
    assert(GenTree::OperIsBinary(tree->OperGet()));

    // Cheap bail out check before more expensive checks are performed.
    // RMW memory op pattern requires that one of the operands of binOp to be GT_IND.
    if (tree->gtGetOp1()->OperGet() != GT_IND && tree->gtGetOp2()->OperGet() != GT_IND)
    {
        return false;
    }

    LIR::Use use;
    if (!BlockRange().TryGetUse(tree, &use) || use.User()->OperGet() != GT_STOREIND || use.User()->gtGetOp2() != tree)
    {
        return false;
    }

    // Since it is not relatively cheap to recognize RMW memory op pattern, we
    // cache the result in GT_STOREIND node so that while lowering GT_STOREIND
    // we can use the result.
    GenTreePtr indirCandidate = nullptr;
    GenTreePtr indirOpSource  = nullptr;
    return IsRMWMemOpRootedAtStoreInd(use.User(), &indirCandidate, &indirOpSource);
}

//----------------------------------------------------------------------------------------------
// This method recognizes the case where we have a treeNode with the following structure:
//         storeInd(IndirDst, binOp(gtInd(IndirDst), indirOpSource)) OR
//         storeInd(IndirDst, binOp(indirOpSource, gtInd(IndirDst)) in case of commutative operations OR
//         storeInd(IndirDst, unaryOp(gtInd(IndirDst)) in case of unary operations
//
// Terminology:
//         indirDst = memory write of an addr mode  (i.e. storeind destination)
//         indirSrc = value being written to memory (i.e. storeind source which could either be a binary or unary op)
//         indirCandidate = memory read i.e. a gtInd of an addr mode
//         indirOpSource = source operand used in binary/unary op (i.e. source operand of indirSrc node)
//
// In x86/x64 this storeInd pattern can be effectively encoded in a single instruction of the
// following form in case of integer operations:
//         binOp [addressing mode], RegIndirOpSource
//         binOp [addressing mode], immediateVal
// where RegIndirOpSource is the register where indirOpSource was computed.
//
// Right now, we recognize few cases:
//     a) The gtInd child is a lea/lclVar/lclVarAddr/clsVarAddr/constant
//     b) BinOp is either add, sub, xor, or, and, shl, rsh, rsz.
//     c) unaryOp is either not/neg
//
// Implementation Note: The following routines need to be in sync for RMW memory op optimization
// to be correct and functional.
//     IndirsAreEquivalent()
//     NodesAreEquivalentLeaves()
//     Codegen of GT_STOREIND and genCodeForShiftRMW()
//     emitInsRMW()
//
//  TODO-CQ: Enable support for more complex indirections (if needed) or use the value numbering
//  package to perform more complex tree recognition.
//
//  TODO-XArch-CQ: Add support for RMW of lcl fields (e.g. lclfield binop= source)
//
//  Parameters:
//     tree               -  GT_STOREIND node
//     outIndirCandidate  -  out param set to indirCandidate as described above
//     ouutIndirOpSource  -  out param set to indirOpSource as described above
//
//  Return value
//     True if there is a RMW memory operation rooted at a GT_STOREIND tree
//     and out params indirCandidate and indirOpSource are set to non-null values.
//     Otherwise, returns false with indirCandidate and indirOpSource set to null.
//     Also updates flags of GT_STOREIND tree with its RMW status.
//
bool Lowering::IsRMWMemOpRootedAtStoreInd(GenTreePtr tree, GenTreePtr* outIndirCandidate, GenTreePtr* outIndirOpSource)
{
    assert(!varTypeIsFloating(tree));
    assert(outIndirCandidate != nullptr);
    assert(outIndirOpSource != nullptr);

    *outIndirCandidate = nullptr;
    *outIndirOpSource  = nullptr;

    // Early out if storeInd is already known to be a non-RMW memory op
    GenTreeStoreInd* storeInd = tree->AsStoreInd();
    if (storeInd->IsNonRMWMemoryOp())
    {
        return false;
    }

    GenTreePtr indirDst = storeInd->gtGetOp1();
    GenTreePtr indirSrc = storeInd->gtGetOp2();
    genTreeOps oper     = indirSrc->OperGet();

    // Early out if it is already known to be a RMW memory op
    if (storeInd->IsRMWMemoryOp())
    {
        if (GenTree::OperIsBinary(oper))
        {
            if (storeInd->IsRMWDstOp1())
            {
                *outIndirCandidate = indirSrc->gtGetOp1();
                *outIndirOpSource  = indirSrc->gtGetOp2();
            }
            else
            {
                assert(storeInd->IsRMWDstOp2());
                *outIndirCandidate = indirSrc->gtGetOp2();
                *outIndirOpSource  = indirSrc->gtGetOp1();
            }
            assert(IndirsAreEquivalent(*outIndirCandidate, storeInd));
        }
        else
        {
            assert(GenTree::OperIsUnary(oper));
            assert(IndirsAreEquivalent(indirSrc->gtGetOp1(), storeInd));
            *outIndirCandidate = indirSrc->gtGetOp1();
            *outIndirOpSource  = indirSrc->gtGetOp1();
        }

        return true;
    }

    // If reached here means that we do not know RMW status of tree rooted at storeInd
    assert(storeInd->IsRMWStatusUnknown());

    // Early out if indirDst is not one of the supported memory operands.
    if (indirDst->OperGet() != GT_LEA && indirDst->OperGet() != GT_LCL_VAR && indirDst->OperGet() != GT_LCL_VAR_ADDR &&
        indirDst->OperGet() != GT_CLS_VAR_ADDR && indirDst->OperGet() != GT_CNS_INT)
    {
        storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_ADDR);
        return false;
    }

    // We can not use Read-Modify-Write instruction forms with overflow checking instructions
    // because we are not allowed to modify the target until after the overflow check.
    if (indirSrc->gtOverflowEx())
    {
        storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_OPER);
        return false;
    }

    // At this point we can match one of two patterns:
    //
    //     t_ind = indir t_addr_0
    //       ...
    //     t_value = binop t_ind, t_other
    //       ...
    //     storeIndir t_addr_1, t_value
    //
    // or
    //
    //     t_ind = indir t_addr_0
    //       ...
    //     t_value = unop t_ind
    //       ...
    //     storeIndir t_addr_1, t_value
    //
    // In all cases, we will eventually make the binop that produces t_value and the entire dataflow tree rooted at
    // t_ind contained by t_value.

    GenTree*  indirCandidate = nullptr;
    GenTree*  indirOpSource  = nullptr;
    RMWStatus status         = STOREIND_RMW_STATUS_UNKNOWN;
    if (GenTree::OperIsBinary(oper))
    {
        // Return if binary op is not one of the supported operations for RMW of memory.
        if (oper != GT_ADD && oper != GT_SUB && oper != GT_AND && oper != GT_OR && oper != GT_XOR &&
            !GenTree::OperIsShiftOrRotate(oper))
        {
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_OPER);
            return false;
        }

        if (GenTree::OperIsShiftOrRotate(oper) && varTypeIsSmall(storeInd))
        {
            // In ldind, Integer values smaller than 4 bytes, a boolean, or a character converted to 4 bytes
            // by sign or zero-extension as appropriate. If we directly shift the short type data using sar, we
            // will lose the sign or zero-extension bits.
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_TYPE);
            return false;
        }

        // In the common case, the second operand to the binop will be the indir candidate.
        GenTreeOp* binOp = indirSrc->AsOp();
        if (GenTree::OperIsCommutative(oper) && IsRMWIndirCandidate(binOp->gtOp2, storeInd))
        {
            indirCandidate = binOp->gtOp2;
            indirOpSource  = binOp->gtOp1;
            status         = STOREIND_RMW_DST_IS_OP2;
        }
        else if (IsRMWIndirCandidate(binOp->gtOp1, storeInd))
        {
            indirCandidate = binOp->gtOp1;
            indirOpSource  = binOp->gtOp2;
            status         = STOREIND_RMW_DST_IS_OP1;
        }
        else
        {
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_ADDR);
            return false;
        }
    }
    else if (GenTree::OperIsUnary(oper))
    {
        // Nodes other than GT_NOT and GT_NEG are not yet supported.
        if (oper != GT_NOT && oper != GT_NEG)
        {
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_OPER);
            return false;
        }

        if (indirSrc->gtGetOp1()->OperGet() != GT_IND)
        {
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_ADDR);
            return false;
        }

        GenTreeUnOp* unOp = indirSrc->AsUnOp();
        if (IsRMWIndirCandidate(unOp->gtOp1, storeInd))
        {
            // src and dest are the same in case of unary ops
            indirCandidate = unOp->gtOp1;
            indirOpSource  = unOp->gtOp1;
            status         = STOREIND_RMW_DST_IS_OP1;
        }
        else
        {
            storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_ADDR);
            return false;
        }
    }
    else
    {
        storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_OPER);
        return false;
    }

    // By this point we've verified that we have a supported operand with a supported address. Now we need to ensure
    // that we're able to move the destination address for the source indirection forwards.
    if (!IsSafeToContainMem(storeInd, indirDst))
    {
        storeInd->SetRMWStatus(STOREIND_RMW_UNSUPPORTED_ADDR);
        return false;
    }

    assert(indirCandidate != nullptr);
    assert(indirOpSource != nullptr);
    assert(status != STOREIND_RMW_STATUS_UNKNOWN);

    *outIndirCandidate = indirCandidate;
    *outIndirOpSource  = indirOpSource;
    storeInd->SetRMWStatus(status);
    return true;
}

//------------------------------------------------------------------------------
// isRMWRegOper: Can this binary tree node be used in a Read-Modify-Write format
//
// Arguments:
//    tree      - a binary tree node
//
// Return Value:
//    Returns true if we can use the read-modify-write instruction form
//
// Notes:
//    This is used to determine whether to preference the source to the destination register.
//
bool Lowering::isRMWRegOper(GenTreePtr tree)
{
    // TODO-XArch-CQ: Make this more accurate.
    // For now, We assume that most binary operators are of the RMW form.
    assert(tree->OperIsBinary());

    if (tree->OperIsCompare())
    {
        return false;
    }

    switch (tree->OperGet())
    {
        // These Opers either support a three op form (i.e. GT_LEA), or do not read/write their first operand
        case GT_LEA:
        case GT_STOREIND:
        case GT_ARR_INDEX:
        case GT_STORE_BLK:
        case GT_STORE_OBJ:
            return false;

        // x86/x64 does support a three op multiply when op2|op1 is a contained immediate
        case GT_MUL:
            return (!IsContainableImmed(tree, tree->gtOp.gtOp2) && !IsContainableImmed(tree, tree->gtOp.gtOp1));

        default:
            return true;
    }
}

// anything is in range for AMD64
bool Lowering::IsCallTargetInRange(void* addr)
{
    return true;
}

// return true if the immediate can be folded into an instruction, for example small enough and non-relocatable
bool Lowering::IsContainableImmed(GenTree* parentNode, GenTree* childNode)
{
    if (!childNode->IsIntCnsFitsInI32())
    {
        return false;
    }

    // At this point we know that it is an int const fits within 4-bytes and hence can safely cast to IntConCommon.
    // Icons that need relocation should never be marked as contained immed
    if (childNode->AsIntConCommon()->ImmedValNeedsReloc(comp))
    {
        return false;
    }

    return true;
}

//-----------------------------------------------------------------------
// PreferredRegOptionalOperand: returns one of the operands of given
// binary oper that is to be preferred for marking as reg optional.
//
// Since only one of op1 or op2 can be a memory operand on xarch, only
// one of  them have to be marked as reg optional.  Since Lower doesn't
// know apriori which of op1 or op2 is not likely to get a register, it
// has to make a guess. This routine encapsulates heuristics that
// guess whether it is likely to be beneficial to mark op1 or op2 as
// reg optional.
//
//
// Arguments:
//     tree  -  a binary-op tree node that is either commutative
//              or a compare oper.
//
// Returns:
//     Returns op1 or op2 of tree node that is preferred for
//     marking as reg optional.
//
// Note: if the tree oper is neither commutative nor a compare oper
// then only op2 can be reg optional on xarch and hence no need to
// call this routine.
GenTree* Lowering::PreferredRegOptionalOperand(GenTree* tree)
{
    assert(GenTree::OperIsBinary(tree->OperGet()));
    assert(tree->OperIsCommutative() || tree->OperIsCompare());

    GenTree* op1         = tree->gtGetOp1();
    GenTree* op2         = tree->gtGetOp2();
    GenTree* preferredOp = nullptr;

    // This routine uses the following heuristics:
    //
    // a) If both are tracked locals, marking the one with lower weighted
    // ref count as reg-optional would likely be beneficial as it has
    // higher probability of not getting a register.
    //
    // b) op1 = tracked local and op2 = untracked local: LSRA creates two
    // ref positions for op2: a def and use position. op2's def position
    // requires a reg and it is allocated a reg by spilling another
    // interval (if required) and that could be even op1.  For this reason
    // it is beneficial to mark op1 as reg optional.
    //
    // TODO: It is not always mandatory for a def position of an untracked
    // local to be allocated a register if it is on rhs of an assignment
    // and its use position is reg-optional and has not been assigned a
    // register.  Reg optional def positions is currently not yet supported.
    //
    // c) op1 = untracked local and op2 = tracked local: marking op1 as
    // reg optional is beneficial, since its use position is less likely
    // to get a register.
    //
    // d) If both are untracked locals (i.e. treated like tree temps by
    // LSRA): though either of them could be marked as reg optional,
    // marking op1 as reg optional is likely to be beneficial because
    // while allocating op2's def position, there is a possibility of
    // spilling op1's def and in which case op1 is treated as contained
    // memory operand rather than requiring to reload.
    //
    // e) If only one of them is a local var, prefer to mark it as
    // reg-optional.  This is heuristic is based on the results
    // obtained against CQ perf benchmarks.
    //
    // f) If neither of them are local vars (i.e. tree temps), prefer to
    // mark op1 as reg optional for the same reason as mentioned in (d) above.
    if (op1->OperGet() == GT_LCL_VAR && op2->OperGet() == GT_LCL_VAR)
    {
        LclVarDsc* v1 = comp->lvaTable + op1->AsLclVarCommon()->GetLclNum();
        LclVarDsc* v2 = comp->lvaTable + op2->AsLclVarCommon()->GetLclNum();

        if (v1->lvTracked && v2->lvTracked)
        {
            // Both are tracked locals.  The one with lower weight is less likely
            // to get a register and hence beneficial to mark the one with lower
            // weight as reg optional.
            if (v1->lvRefCntWtd < v2->lvRefCntWtd)
            {
                preferredOp = op1;
            }
            else
            {
                preferredOp = op2;
            }
        }
        else if (v2->lvTracked)
        {
            // v1 is an untracked lcl and it is use position is less likely to
            // get a register.
            preferredOp = op1;
        }
        else if (v1->lvTracked)
        {
            // v2 is an untracked lcl and its def position always
            // needs a reg.  Hence it is better to mark v1 as
            // reg optional.
            preferredOp = op1;
        }
        else
        {
            preferredOp = op1;
            ;
        }
    }
    else if (op1->OperGet() == GT_LCL_VAR)
    {
        preferredOp = op1;
    }
    else if (op2->OperGet() == GT_LCL_VAR)
    {
        preferredOp = op2;
    }
    else
    {
        // Neither of the operands is a local, prefer marking
        // operand that is evaluated first as reg optional
        // since its use position is less likely to get a register.
        bool reverseOps = ((tree->gtFlags & GTF_REVERSE_OPS) != 0);
        preferredOp     = reverseOps ? op2 : op1;
    }

    return preferredOp;
}

#endif // _TARGET_XARCH_

#endif // !LEGACY_BACKEND
