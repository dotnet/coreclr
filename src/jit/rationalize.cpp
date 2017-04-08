// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#ifndef LEGACY_BACKEND
// state carried over the tree walk, to be used in making
// a splitting decision.
struct SplitData
{
    GenTree*      root; // root stmt of tree being processed
    BasicBlock*   block;
    Rationalizer* thisPhase;
};

// return op that is the store equivalent of the given load opcode
genTreeOps storeForm(genTreeOps loadForm)
{
    switch (loadForm)
    {
        case GT_LCL_VAR:
            return GT_STORE_LCL_VAR;
        case GT_LCL_FLD:
            return GT_STORE_LCL_FLD;
        case GT_REG_VAR:
            noway_assert(!"reg vars only supported in classic backend\n");
            unreached();
        default:
            noway_assert(!"not a data load opcode\n");
            unreached();
    }
}

// return op that is the addr equivalent of the given load opcode
genTreeOps addrForm(genTreeOps loadForm)
{
    switch (loadForm)
    {
        case GT_LCL_VAR:
            return GT_LCL_VAR_ADDR;
        case GT_LCL_FLD:
            return GT_LCL_FLD_ADDR;
        default:
            noway_assert(!"not a data load opcode\n");
            unreached();
    }
}

// return op that is the load equivalent of the given addr opcode
genTreeOps loadForm(genTreeOps addrForm)
{
    switch (addrForm)
    {
        case GT_LCL_VAR_ADDR:
            return GT_LCL_VAR;
        case GT_LCL_FLD_ADDR:
            return GT_LCL_FLD;
        default:
            noway_assert(!"not a local address opcode\n");
            unreached();
    }
}

// copy the flags determined by mask from src to dst
void copyFlags(GenTree* dst, GenTree* src, unsigned mask)
{
    dst->gtFlags &= ~mask;
    dst->gtFlags |= (src->gtFlags & mask);
}

// Rewrite a SIMD indirection as GT_IND(GT_LEA(obj.op1)), or as a simple
// lclVar if possible.
//
// Arguments:
//    use      - A use reference for a block node
//    keepBlk  - True if this should remain a block node if it is not a lclVar
//
// Return Value:
//    None.
//
// TODO-1stClassStructs: These should be eliminated earlier, once we can handle
// lclVars in all the places that used to have GT_OBJ.
//
void Rationalizer::RewriteSIMDOperand(LIR::Use& use, bool keepBlk)
{
#ifdef FEATURE_SIMD
    // No lowering is needed for non-SIMD nodes, so early out if featureSIMD is not enabled.
    if (!comp->featureSIMD)
    {
        return;
    }

    GenTree* tree = use.Def();
    if (!tree->OperIsIndir())
    {
        return;
    }
    var_types simdType = tree->TypeGet();

    if (!varTypeIsSIMD(simdType))
    {
        return;
    }

    // If we have GT_IND(GT_LCL_VAR_ADDR) and the GT_LCL_VAR_ADDR is TYP_BYREF/TYP_I_IMPL,
    // and the var is a SIMD type, replace the expression by GT_LCL_VAR.
    GenTree* addr = tree->AsIndir()->Addr();
    if (addr->OperIsLocalAddr() && comp->isAddrOfSIMDType(addr))
    {
        BlockRange().Remove(tree);

        addr->SetOper(loadForm(addr->OperGet()));
        addr->gtType = simdType;
        use.ReplaceWith(comp, addr);
    }
    else if ((addr->OperGet() == GT_ADDR) && (addr->gtGetOp1()->OperGet() == GT_SIMD))
    {
        // if we have GT_IND(GT_ADDR(GT_SIMD)), remove the GT_IND(GT_ADDR()), leaving just the GT_SIMD.
        BlockRange().Remove(tree);
        BlockRange().Remove(addr);

        use.ReplaceWith(comp, addr->gtGetOp1());
    }
    else if (!keepBlk)
    {
        tree->SetOper(GT_IND);
        tree->gtType = simdType;
    }
#endif // FEATURE_SIMD
}

// RewriteNodeAsCall : Replace the given tree node by a GT_CALL.
//
// Arguments:
//    ppTree      - A pointer-to-a-pointer for the tree node
//    fgWalkData  - A pointer to tree walk data providing the context
//    callHnd     - The method handle of the call to be generated
//    entryPoint  - The method entrypoint of the call to be generated
//    args        - The argument list of the call to be generated
//
// Return Value:
//    None.
//

void Rationalizer::RewriteNodeAsCall(GenTree**             use,
                                     Compiler::fgWalkData* data,
                                     CORINFO_METHOD_HANDLE callHnd,
#ifdef FEATURE_READYTORUN_COMPILER
                                     CORINFO_CONST_LOOKUP entryPoint,
#endif
                                     GenTreeArgList* args)
{
    GenTreePtr tree          = *use;
    Compiler*  comp          = data->compiler;
    SplitData* tmpState      = (SplitData*)data->pCallbackData;
    GenTreePtr root          = tmpState->root;
    GenTreePtr treeFirstNode = comp->fgGetFirstNode(tree);
    GenTreePtr treeLastNode  = tree;
    GenTreePtr treePrevNode  = treeFirstNode->gtPrev;
    GenTreePtr treeNextNode  = treeLastNode->gtNext;

    // Create the call node
    GenTreeCall* call = comp->gtNewCallNode(CT_USER_FUNC, callHnd, tree->gtType, args);

#if DEBUG
    CORINFO_SIG_INFO sig;
    comp->eeGetMethodSig(callHnd, &sig);
    assert(JITtype2varType(sig.retType) == tree->gtType);
#endif // DEBUG

    call = comp->fgMorphArgs(call);
    // Determine if this call has changed any codegen requirements.
    comp->fgCheckArgCnt();

#ifdef FEATURE_READYTORUN_COMPILER
    call->gtCall.setEntryPoint(entryPoint);
#endif

    // Replace "tree" with "call"
    if (data->parentStack->Height() > 1)
    {
        data->parentStack->Index(1)->ReplaceOperand(use, call);
    }
    else
    {
        // If there's no parent, the tree being replaced is the root of the
        // statement (and no special handling is necessary).
        *use = call;
    }

    // Rebuild the evaluation order.
    comp->gtSetStmtInfo(root);

    // Rebuild the execution order.
    comp->fgSetTreeSeq(call, treePrevNode);

    // Restore linear-order Prev and Next for "call".
    if (treePrevNode)
    {
        treeFirstNode         = comp->fgGetFirstNode(call);
        treeFirstNode->gtPrev = treePrevNode;
        treePrevNode->gtNext  = treeFirstNode;
    }
    else
    {
        // Update the linear oder start of "root" if treeFirstNode
        // appears to have replaced the original first node.
        assert(treeFirstNode == root->gtStmt.gtStmtList);
        root->gtStmt.gtStmtList = comp->fgGetFirstNode(call);
    }

    if (treeNextNode)
    {
        treeLastNode         = call;
        treeLastNode->gtNext = treeNextNode;
        treeNextNode->gtPrev = treeLastNode;
    }

    // Propagate flags of "call" to its parents.
    // 0 is current node, so start at 1
    for (int i = 1; i < data->parentStack->Height(); i++)
    {
        GenTree* node = data->parentStack->Index(i);
        node->gtFlags |= GTF_CALL;
        node->gtFlags |= call->gtFlags & GTF_ALL_EFFECT;
    }

    // Since "tree" is replaced with "call", pop "tree" node (i.e the current node)
    // and replace it with "call" on parent stack.
    assert(data->parentStack->Top() == tree);
    (void)data->parentStack->Pop();
    data->parentStack->Push(call);
}

// RewriteIntrinsicAsUserCall : Rewrite an intrinsic operator as a GT_CALL to the original method.
//
// Arguments:
//    ppTree      - A pointer-to-a-pointer for the intrinsic node
//    fgWalkData  - A pointer to tree walk data providing the context
//
// Return Value:
//    None.
//
// Some intrinsics, such as operation Sqrt, are rewritten back to calls, and some are not.
// The ones that are not being rewritten here must be handled in Codegen.
// Conceptually, the lower is the right place to do the rewrite. Keeping it in rationalization is
// mainly for throughput issue.

void Rationalizer::RewriteIntrinsicAsUserCall(GenTree** use, Compiler::fgWalkData* data)
{
    GenTreeIntrinsic* intrinsic = (*use)->AsIntrinsic();
    Compiler*         comp      = data->compiler;

    GenTreeArgList* args;
    if (intrinsic->gtOp.gtOp2 == nullptr)
    {
        args = comp->gtNewArgList(intrinsic->gtGetOp1());
    }
    else
    {
        args = comp->gtNewArgList(intrinsic->gtGetOp1(), intrinsic->gtGetOp2());
    }

    RewriteNodeAsCall(use, data, intrinsic->gtMethodHandle,
#ifdef FEATURE_READYTORUN_COMPILER
                      intrinsic->gtEntryPoint,
#endif
                      args);
}

// FixupIfSIMDLocal: Fixup the type of a lclVar tree, as needed, if it is a SIMD type vector.
//
// Arguments:
//    comp      - the Compiler object.
//    tree      - the GenTreeLclVarCommon tree to be fixed up.
//
// Return Value:
//    None.
//
// TODO-1stClassStructs: This is now only here to preserve existing behavior. It is actually not
// desirable to change the lclFld nodes back to TYP_SIMD (it will cause them to be loaded
// into a vector register, and then moved to an int register).

void Rationalizer::FixupIfSIMDLocal(GenTreeLclVarCommon* node)
{
#ifdef FEATURE_SIMD
    if (!comp->featureSIMD)
    {
        return;
    }

    LclVarDsc* varDsc = &(comp->lvaTable[node->gtLclNum]);

    // Don't mark byref of SIMD vector as a SIMD type.
    // Note that struct args though marked as lvIsSIMD=true,
    // the tree node representing such an arg should not be
    // marked as a SIMD type, since it is a byref of a SIMD type.
    if (!varTypeIsSIMD(varDsc))
    {
        return;
    }
    switch (node->OperGet())
    {
        default:
            // Nothing to do for most tree nodes.
            break;

        case GT_LCL_FLD:
            // We may see a lclFld used for pointer-sized structs that have been morphed, in which
            // case we can change it to GT_LCL_VAR.
            // However, we may also see a lclFld with FieldSeqStore::NotAField() for structs that can't
            // be analyzed, e.g. those with overlapping fields such as the IL implementation of Vector<T>.
            if ((node->AsLclFld()->gtFieldSeq == FieldSeqStore::NotAField()) && (node->AsLclFld()->gtLclOffs == 0) &&
                (node->gtType == TYP_I_IMPL) && (varDsc->lvExactSize == TARGET_POINTER_SIZE))
            {
                node->SetOper(GT_LCL_VAR);
                node->gtFlags &= ~(GTF_VAR_USEASG);
            }
            else
            {
                // If we access a field of a SIMD lclVar via GT_LCL_FLD, it cannot have been
                // independently promoted.
                assert(comp->lvaGetPromotionType(varDsc) != Compiler::PROMOTION_TYPE_INDEPENDENT);
                return;
            }
            break;
        case GT_STORE_LCL_FLD:
            assert(node->gtType == TYP_I_IMPL);
            node->SetOper(GT_STORE_LCL_VAR);
            node->gtFlags &= ~(GTF_VAR_USEASG);
            break;
    }
    unsigned simdSize = (unsigned int)roundUp(varDsc->lvExactSize, TARGET_POINTER_SIZE);
    node->gtType      = comp->getSIMDTypeForSize(simdSize);
#endif // FEATURE_SIMD
}

#ifdef DEBUG

void Rationalizer::ValidateStatement(GenTree* tree, BasicBlock* block)
{
    assert(tree->gtOper == GT_STMT);
    DBEXEC(TRUE, JitTls::GetCompiler()->fgDebugCheckNodeLinks(block, tree));
}

// sanity checks that apply to all kinds of IR
void Rationalizer::SanityCheck()
{
    // TODO: assert(!IsLIR());
    BasicBlock* block;
    foreach_block(comp, block)
    {
        for (GenTree* statement = block->bbTreeList; statement != nullptr; statement = statement->gtNext)
        {
            ValidateStatement(statement, block);

            for (GenTree* tree = statement->gtStmt.gtStmtList; tree; tree = tree->gtNext)
            {
                // QMARK nodes should have been removed before this phase.
                assert(tree->OperGet() != GT_QMARK);

                if (tree->OperGet() == GT_ASG)
                {
                    if (tree->gtGetOp1()->OperGet() == GT_LCL_VAR)
                    {
                        assert(tree->gtGetOp1()->gtFlags & GTF_VAR_DEF);
                    }
                    else if (tree->gtGetOp2()->OperGet() == GT_LCL_VAR)
                    {
                        assert(!(tree->gtGetOp2()->gtFlags & GTF_VAR_DEF));
                    }
                }
            }
        }
    }
}

void Rationalizer::SanityCheckRational()
{
    // TODO-Cleanup : check that the tree is rational here
    // then do normal checks
    SanityCheck();
}

#endif // DEBUG

static void RewriteAssignmentIntoStoreLclCore(GenTreeOp* assignment,
                                              GenTree*   location,
                                              GenTree*   value,
                                              genTreeOps locationOp)
{
    assert(assignment != nullptr);
    assert(assignment->OperGet() == GT_ASG);
    assert(location != nullptr);
    assert(value != nullptr);

    genTreeOps storeOp = storeForm(locationOp);

#ifdef DEBUG
    JITDUMP("rewriting asg(%s, X) to %s(X)\n", GenTree::NodeName(locationOp), GenTree::NodeName(storeOp));
#endif // DEBUG

    assignment->SetOper(storeOp);
    GenTreeLclVarCommon* store = assignment->AsLclVarCommon();

    GenTreeLclVarCommon* var = location->AsLclVarCommon();
    store->SetLclNum(var->gtLclNum);
    store->SetSsaNum(var->gtSsaNum);

    if (locationOp == GT_LCL_FLD)
    {
        store->gtLclFld.gtLclOffs  = var->gtLclFld.gtLclOffs;
        store->gtLclFld.gtFieldSeq = var->gtLclFld.gtFieldSeq;
    }

    copyFlags(store, var, GTF_LIVENESS_MASK);
    store->gtFlags &= ~GTF_REVERSE_OPS;

    store->gtType = var->TypeGet();
    store->gtOp1  = value;

    DISPNODE(store);
    JITDUMP("\n");
}

void Rationalizer::RewriteAssignmentIntoStoreLcl(GenTreeOp* assignment)
{
    assert(assignment != nullptr);
    assert(assignment->OperGet() == GT_ASG);

    GenTree* location = assignment->gtGetOp1();
    GenTree* value    = assignment->gtGetOp2();

    RewriteAssignmentIntoStoreLclCore(assignment, location, value, location->OperGet());
}

void Rationalizer::RewriteAssignment(LIR::Use& use)
{
    assert(use.IsInitialized());

    GenTreeOp* assignment = use.Def()->AsOp();
    assert(assignment->OperGet() == GT_ASG);

    GenTree* location = assignment->gtGetOp1();
    GenTree* value    = assignment->gtGetOp2();

    genTreeOps locationOp = location->OperGet();

    if (assignment->OperIsBlkOp())
    {
#ifdef FEATURE_SIMD
        if (varTypeIsSIMD(location) && assignment->OperIsInitBlkOp())
        {
            if (location->OperGet() == GT_LCL_VAR)
            {
                var_types simdType = location->TypeGet();
                GenTree*  initVal  = assignment->gtOp.gtOp2;
                var_types baseType = comp->getBaseTypeOfSIMDLocal(location);
                if (baseType != TYP_UNKNOWN)
                {
                    GenTreeSIMD* simdTree = new (comp, GT_SIMD)
                        GenTreeSIMD(simdType, initVal, SIMDIntrinsicInit, baseType, genTypeSize(simdType));
                    assignment->gtOp.gtOp2 = simdTree;
                    value                  = simdTree;
                    initVal->gtNext        = simdTree;
                    simdTree->gtPrev       = initVal;

                    simdTree->gtNext = location;
                    location->gtPrev = simdTree;
                }
            }
        }
#endif // FEATURE_SIMD
        if ((location->TypeGet() == TYP_STRUCT) && !assignment->IsPhiDefn() && !value->IsMultiRegCall())
        {
            if ((location->OperGet() == GT_LCL_VAR))
            {
                // We need to construct a block node for the location.
                // Modify lcl to be the address form.
                location->SetOper(addrForm(locationOp));
                LclVarDsc* varDsc     = &(comp->lvaTable[location->AsLclVarCommon()->gtLclNum]);
                location->gtType      = TYP_BYREF;
                GenTreeBlk*  storeBlk = nullptr;
                unsigned int size     = varDsc->lvExactSize;

                if (varDsc->lvStructGcCount != 0)
                {
                    CORINFO_CLASS_HANDLE structHnd = varDsc->lvVerTypeInfo.GetClassHandle();
                    GenTreeObj*          objNode   = comp->gtNewObjNode(structHnd, location)->AsObj();
                    unsigned int         slots = (unsigned)(roundUp(size, TARGET_POINTER_SIZE) / TARGET_POINTER_SIZE);

                    objNode->SetGCInfo(varDsc->lvGcLayout, varDsc->lvStructGcCount, slots);
                    objNode->ChangeOper(GT_STORE_OBJ);
                    objNode->SetData(value);
                    comp->fgMorphUnsafeBlk(objNode);
                    storeBlk = objNode;
                }
                else
                {
                    storeBlk = new (comp, GT_STORE_BLK) GenTreeBlk(GT_STORE_BLK, TYP_STRUCT, location, value, size);
                }
                storeBlk->gtFlags |= (GTF_REVERSE_OPS | GTF_ASG);
                storeBlk->gtFlags |= ((location->gtFlags | value->gtFlags) & GTF_ALL_EFFECT);

                GenTree* insertionPoint = location->gtNext;
                BlockRange().InsertBefore(insertionPoint, storeBlk);
                use.ReplaceWith(comp, storeBlk);
                BlockRange().Remove(assignment);
                JITDUMP("After transforming local struct assignment into a block op:\n");
                DISPTREERANGE(BlockRange(), use.Def());
                JITDUMP("\n");
                return;
            }
            else
            {
                assert(location->OperIsBlk());
            }
        }
    }

    switch (locationOp)
    {
        case GT_LCL_VAR:
        case GT_LCL_FLD:
        case GT_REG_VAR:
        case GT_PHI_ARG:
            RewriteAssignmentIntoStoreLclCore(assignment, location, value, locationOp);
            BlockRange().Remove(location);
            break;

        case GT_IND:
        {
            GenTreeStoreInd* store =
                new (comp, GT_STOREIND) GenTreeStoreInd(location->TypeGet(), location->gtGetOp1(), value);

            copyFlags(store, assignment, GTF_ALL_EFFECT);
            copyFlags(store, location, GTF_IND_FLAGS);

            if (assignment->IsReverseOp())
            {
                store->gtFlags |= GTF_REVERSE_OPS;
            }

            // TODO: JIT dump

            // Remove the GT_IND node and replace the assignment node with the store
            BlockRange().Remove(location);
            BlockRange().InsertBefore(assignment, store);
            use.ReplaceWith(comp, store);
            BlockRange().Remove(assignment);
        }
        break;

        case GT_CLS_VAR:
        {
            location->SetOper(GT_CLS_VAR_ADDR);
            location->gtType = TYP_BYREF;

            assignment->SetOper(GT_STOREIND);

            // TODO: JIT dump
        }
        break;

        case GT_BLK:
        case GT_OBJ:
        case GT_DYN_BLK:
        {
            assert(varTypeIsStruct(location));
            GenTreeBlk* storeBlk = location->AsBlk();
            genTreeOps  storeOper;
            switch (location->gtOper)
            {
                case GT_BLK:
                    storeOper = GT_STORE_BLK;
                    break;
                case GT_OBJ:
                    storeOper = GT_STORE_OBJ;
                    break;
                case GT_DYN_BLK:
                    storeOper = GT_STORE_DYN_BLK;
                    break;
                default:
                    unreached();
            }
            JITDUMP("Rewriting GT_ASG(%s(X), Y) to %s(X,Y):\n", GenTree::NodeName(location->gtOper),
                    GenTree::NodeName(storeOper));
            storeBlk->SetOperRaw(storeOper);
            storeBlk->gtFlags &= ~GTF_DONT_CSE;
            storeBlk->gtFlags |= (assignment->gtFlags & (GTF_ALL_EFFECT | GTF_REVERSE_OPS | GTF_BLK_VOLATILE |
                                                         GTF_BLK_UNALIGNED | GTF_DONT_CSE));
            storeBlk->gtBlk.Data() = value;

            // Replace the assignment node with the store
            use.ReplaceWith(comp, storeBlk);
            BlockRange().Remove(assignment);
            DISPTREERANGE(BlockRange(), use.Def());
            JITDUMP("\n");
        }
        break;

        default:
            unreached();
            break;
    }
}

void Rationalizer::RewriteAddress(LIR::Use& use)
{
    assert(use.IsInitialized());

    GenTreeUnOp* address = use.Def()->AsUnOp();
    assert(address->OperGet() == GT_ADDR);

    GenTree*   location   = address->gtGetOp1();
    genTreeOps locationOp = location->OperGet();

    if (location->IsLocal())
    {
// We are changing the child from GT_LCL_VAR TO GT_LCL_VAR_ADDR.
// Therefore gtType of the child needs to be changed to a TYP_BYREF
#ifdef DEBUG
        if (locationOp == GT_LCL_VAR)
        {
            JITDUMP("Rewriting GT_ADDR(GT_LCL_VAR) to GT_LCL_VAR_ADDR:\n");
        }
        else
        {
            assert(locationOp == GT_LCL_FLD);
            JITDUMP("Rewriting GT_ADDR(GT_LCL_FLD) to GT_LCL_FLD_ADDR:\n");
        }
#endif // DEBUG

        location->SetOper(addrForm(locationOp));
        location->gtType = TYP_BYREF;
        copyFlags(location, address, GTF_ALL_EFFECT);

        use.ReplaceWith(comp, location);
        BlockRange().Remove(address);
    }
    else if (locationOp == GT_CLS_VAR)
    {
        location->SetOper(GT_CLS_VAR_ADDR);
        location->gtType = TYP_BYREF;
        copyFlags(location, address, GTF_ALL_EFFECT);

        use.ReplaceWith(comp, location);
        BlockRange().Remove(address);

        JITDUMP("Rewriting GT_ADDR(GT_CLS_VAR) to GT_CLS_VAR_ADDR:\n");
    }
    else if (location->OperIsIndir())
    {
        use.ReplaceWith(comp, location->gtGetOp1());
        BlockRange().Remove(location);
        BlockRange().Remove(address);

        JITDUMP("Rewriting GT_ADDR(GT_IND(X)) to X:\n");
    }

    DISPTREERANGE(BlockRange(), use.Def());
    JITDUMP("\n");
}

Compiler::fgWalkResult Rationalizer::RewriteNode(GenTree** useEdge, ArrayStack<GenTree*>& parentStack)
{
    assert(useEdge != nullptr);

    GenTree* node = *useEdge;
    assert(node != nullptr);

#ifdef DEBUG
    const bool isLateArg = (node->gtFlags & GTF_LATE_ARG) != 0;
#endif

    // First, remove any preceeding list nodes, which are not otherwise visited by the tree walk.
    //
    // NOTE: GT_FIELD_LIST head nodes, and GT_LIST nodes used by phi nodes will in fact be visited.
    for (GenTree* prev = node->gtPrev; prev != nullptr && prev->OperIsAnyList() && !(prev->OperIsFieldListHead());
         prev          = node->gtPrev)
    {
        BlockRange().Remove(prev);
    }

    // In addition, remove the current node if it is a GT_LIST node that is not an aggregate.
    if (node->OperIsAnyList())
    {
        GenTreeArgList* list = node->AsArgList();
        if (!list->OperIsFieldListHead())
        {
            BlockRange().Remove(list);
        }
        return Compiler::WALK_CONTINUE;
    }

    LIR::Use use;
    if (parentStack.Height() < 2)
    {
        use = LIR::Use::GetDummyUse(BlockRange(), *useEdge);
    }
    else
    {
        use = LIR::Use(BlockRange(), useEdge, parentStack.Index(1));
    }

    assert(node == use.Def());
    switch (node->OperGet())
    {
        case GT_ASG:
            RewriteAssignment(use);
            break;

        case GT_BOX:
            // GT_BOX at this level just passes through so get rid of it
            use.ReplaceWith(comp, node->gtGetOp1());
            BlockRange().Remove(node);
            break;

        case GT_ADDR:
            RewriteAddress(use);
            break;

        case GT_IND:
            // Clear the `GTF_IND_ASG_LHS` flag, which overlaps with `GTF_IND_REQ_ADDR_IN_REG`.
            node->gtFlags &= ~GTF_IND_ASG_LHS;

            if (varTypeIsSIMD(node))
            {
                RewriteSIMDOperand(use, false);
            }
            else
            {
                // Due to promotion of structs containing fields of type struct with a
                // single scalar type field, we could potentially see IR nodes of the
                // form GT_IND(GT_ADD(lclvarAddr, 0)) where 0 is an offset representing
                // a field-seq. These get folded here.
                //
                // TODO: This code can be removed once JIT implements recursive struct
                // promotion instead of lying about the type of struct field as the type
                // of its single scalar field.
                GenTree* addr = node->AsIndir()->Addr();
                if (addr->OperGet() == GT_ADD && addr->gtGetOp1()->OperGet() == GT_LCL_VAR_ADDR &&
                    addr->gtGetOp2()->IsIntegralConst(0))
                {
                    GenTreeLclVarCommon* lclVarNode = addr->gtGetOp1()->AsLclVarCommon();
                    unsigned             lclNum     = lclVarNode->GetLclNum();
                    LclVarDsc*           varDsc     = comp->lvaTable + lclNum;
                    if (node->TypeGet() == varDsc->TypeGet())
                    {
                        JITDUMP("Rewriting GT_IND(GT_ADD(LCL_VAR_ADDR,0)) to LCL_VAR\n");
                        lclVarNode->SetOper(GT_LCL_VAR);
                        lclVarNode->gtType = node->TypeGet();
                        use.ReplaceWith(comp, lclVarNode);
                        BlockRange().Remove(addr);
                        BlockRange().Remove(addr->gtGetOp2());
                        BlockRange().Remove(node);
                    }
                }
            }
            break;

        case GT_NOP:
            // fgMorph sometimes inserts NOP nodes between defs and uses
            // supposedly 'to prevent constant folding'. In this case, remove the
            // NOP.
            if (node->gtGetOp1() != nullptr)
            {
                use.ReplaceWith(comp, node->gtGetOp1());
                BlockRange().Remove(node);
            }
            break;

        case GT_COMMA:
        {
            GenTree* op1 = node->gtGetOp1();
            if ((op1->gtFlags & GTF_ALL_EFFECT) == 0)
            {
                // The LHS has no side effects. Remove it.
                bool               isClosed    = false;
                unsigned           sideEffects = 0;
                LIR::ReadOnlyRange lhsRange    = BlockRange().GetTreeRange(op1, &isClosed, &sideEffects);

                // None of the transforms performed herein violate tree order, so these
                // should always be true.
                assert(isClosed);
                assert((sideEffects & GTF_ALL_EFFECT) == 0);

                BlockRange().Delete(comp, m_block, std::move(lhsRange));
            }

            GenTree* replacement = node->gtGetOp2();
            if (!use.IsDummyUse())
            {
                use.ReplaceWith(comp, replacement);
            }
            else
            {
                // This is a top-level comma. If the RHS has no side effects we can remove
                // it as well.
                if ((replacement->gtFlags & GTF_ALL_EFFECT) == 0)
                {
                    bool               isClosed    = false;
                    unsigned           sideEffects = 0;
                    LIR::ReadOnlyRange rhsRange    = BlockRange().GetTreeRange(replacement, &isClosed, &sideEffects);

                    // None of the transforms performed herein violate tree order, so these
                    // should always be true.
                    assert(isClosed);
                    assert((sideEffects & GTF_ALL_EFFECT) == 0);

                    BlockRange().Delete(comp, m_block, std::move(rhsRange));
                }
            }

            BlockRange().Remove(node);
        }
        break;

        case GT_ARGPLACE:
            // Remove argplace and list nodes from the execution order.
            //
            // TODO: remove phi args and phi nodes as well?
            BlockRange().Remove(node);
            break;

#if defined(_TARGET_XARCH_) || defined(_TARGET_ARM_)
        case GT_CLS_VAR:
        {
            // Class vars that are the target of an assignment will get rewritten into
            // GT_STOREIND(GT_CLS_VAR_ADDR, val) by RewriteAssignment. This check is
            // not strictly necessary--the GT_IND(GT_CLS_VAR_ADDR) pattern that would
            // otherwise be generated would also be picked up by RewriteAssignment--but
            // skipping the rewrite here saves an allocation and a bit of extra work.
            const bool isLHSOfAssignment = (use.User()->OperGet() == GT_ASG) && (use.User()->gtGetOp1() == node);
            if (!isLHSOfAssignment)
            {
                GenTree* ind = comp->gtNewOperNode(GT_IND, node->TypeGet(), node);

                node->SetOper(GT_CLS_VAR_ADDR);
                node->gtType = TYP_BYREF;

                BlockRange().InsertAfter(node, ind);
                use.ReplaceWith(comp, ind);

                // TODO: JIT dump
            }
        }
        break;
#endif // _TARGET_XARCH_

        case GT_INTRINSIC:
            // Non-target intrinsics should have already been rewritten back into user calls.
            assert(Compiler::IsTargetIntrinsic(node->gtIntrinsic.gtIntrinsicId));
            break;

#ifdef FEATURE_SIMD
        case GT_BLK:
        case GT_OBJ:
        {
            // TODO-1stClassStructs: These should have been transformed to GT_INDs, but in order
            // to preserve existing behavior, we will keep this as a block node if this is the
            // lhs of a block assignment, and either:
            // - It is a "generic" TYP_STRUCT assignment, OR
            // - It is an initblk, OR
            // - Neither the lhs or rhs are known to be of SIMD type.

            GenTree* parent  = use.User();
            bool     keepBlk = false;
            if ((parent->OperGet() == GT_ASG) && (node == parent->gtGetOp1()))
            {
                if ((node->TypeGet() == TYP_STRUCT) || parent->OperIsInitBlkOp())
                {
                    keepBlk = true;
                }
                else if (!comp->isAddrOfSIMDType(node->AsBlk()->Addr()))
                {
                    GenTree* dataSrc = parent->gtGetOp2();
                    if (!dataSrc->IsLocal() && (dataSrc->OperGet() != GT_SIMD))
                    {
                        noway_assert(dataSrc->OperIsIndir());
                        keepBlk = !comp->isAddrOfSIMDType(dataSrc->AsIndir()->Addr());
                    }
                }
            }
            RewriteSIMDOperand(use, keepBlk);
        }
        break;

        case GT_LCL_FLD:
        case GT_STORE_LCL_FLD:
            // TODO-1stClassStructs: Eliminate this.
            FixupIfSIMDLocal(node->AsLclVarCommon());
            break;

        case GT_SIMD:
        {
            noway_assert(comp->featureSIMD);
            GenTreeSIMD* simdNode = node->AsSIMD();
            unsigned     simdSize = simdNode->gtSIMDSize;
            var_types    simdType = comp->getSIMDTypeForSize(simdSize);

            // TODO-1stClassStructs: This should be handled more generally for enregistered or promoted
            // structs that are passed or returned in a different register type than their enregistered
            // type(s).
            if (simdNode->gtType == TYP_I_IMPL && simdNode->gtSIMDSize == TARGET_POINTER_SIZE)
            {
                // This happens when it is consumed by a GT_RET_EXPR.
                // It can only be a Vector2f or Vector2i.
                assert(genTypeSize(simdNode->gtSIMDBaseType) == 4);
                simdNode->gtType = TYP_SIMD8;
            }
            // Certain SIMD trees require rationalizing.
            if (simdNode->gtSIMD.gtSIMDIntrinsicID == SIMDIntrinsicInitArray)
            {
                // Rewrite this as an explicit load.
                JITDUMP("Rewriting GT_SIMD array init as an explicit load:\n");
                unsigned int baseTypeSize = genTypeSize(simdNode->gtSIMDBaseType);
                GenTree*     address = new (comp, GT_LEA) GenTreeAddrMode(TYP_BYREF, simdNode->gtOp1, simdNode->gtOp2,
                                                                      baseTypeSize, offsetof(CORINFO_Array, u1Elems));
                GenTree* ind = comp->gtNewOperNode(GT_IND, simdType, address);

                BlockRange().InsertBefore(simdNode, address, ind);
                use.ReplaceWith(comp, ind);
                BlockRange().Remove(simdNode);

                DISPTREERANGE(BlockRange(), use.Def());
                JITDUMP("\n");
            }
            else
            {
                // This code depends on the fact that NONE of the SIMD intrinsics take vector operands
                // of a different width.  If that assumption changes, we will EITHER have to make these type
                // transformations during importation, and plumb the types all the way through the JIT,
                // OR add a lot of special handling here.
                GenTree* op1 = simdNode->gtGetOp1();
                if (op1 != nullptr && op1->gtType == TYP_STRUCT)
                {
                    op1->gtType = simdType;
                }

                GenTree* op2 = simdNode->gtGetOp2IfPresent();
                if (op2 != nullptr && op2->gtType == TYP_STRUCT)
                {
                    op2->gtType = simdType;
                }
            }
        }
        break;
#endif // FEATURE_SIMD

        default:
            // CMP, TEST, SETCC and JCC nodes should not be present in HIR.
            assert(!node->OperIs(GT_CMP, GT_TEST, GT_SETCC, GT_JCC));
            break;
    }

    // Do some extra processing on top-level nodes to remove unused local reads.
    if (node->OperIsLocalRead())
    {
        if (use.IsDummyUse())
        {
            comp->lvaDecRefCnts(node);
            BlockRange().Remove(node);
        }
        else
        {
            // Local reads are side-effect-free; clear any flags leftover from frontend transformations.
            node->gtFlags &= ~GTF_ALL_EFFECT;
        }
    }

    assert(isLateArg == ((use.Def()->gtFlags & GTF_LATE_ARG) != 0));

    return Compiler::WALK_CONTINUE;
}

void Rationalizer::DoPhase()
{
    DBEXEC(TRUE, SanityCheck());

    comp->compCurBB = nullptr;
    comp->fgOrder   = Compiler::FGOrderLinear;

    BasicBlock* firstBlock = comp->fgFirstBB;

    for (BasicBlock* block = comp->fgFirstBB; block != nullptr; block = block->bbNext)
    {
        comp->compCurBB = block;
        m_block         = block;

        // Establish the first and last nodes for the block. This is necessary in order for the LIR
        // utilities that hang off the BasicBlock type to work correctly.
        GenTreeStmt* firstStatement = block->firstStmt();
        if (firstStatement == nullptr)
        {
            // No statements in this block; skip it.
            block->MakeLIR(nullptr, nullptr);
            continue;
        }

        GenTreeStmt* lastStatement = block->lastStmt();

        // Rewrite intrinsics that are not supported by the target back into user calls.
        // This needs to be done before the transition to LIR because it relies on the use
        // of fgMorphArgs, which is designed to operate on HIR. Once this is done for a
        // particular statement, link that statement's nodes into the current basic block.
        //
        // This walk also clears the GTF_VAR_USEDEF bit on locals, which is not necessary
        // in the backend.
        GenTree* lastNodeInPreviousStatement = nullptr;
        for (GenTreeStmt* statement = firstStatement; statement != nullptr; statement = statement->getNextStmt())
        {
            assert(statement->gtStmtList != nullptr);
            assert(statement->gtStmtList->gtPrev == nullptr);
            assert(statement->gtStmtExpr != nullptr);
            assert(statement->gtStmtExpr->gtNext == nullptr);

            SplitData splitData;
            splitData.root      = statement;
            splitData.block     = block;
            splitData.thisPhase = this;

            comp->fgWalkTreePost(&statement->gtStmtExpr,
                                 [](GenTree** use, Compiler::fgWalkData* walkData) -> Compiler::fgWalkResult {
                                     GenTree* node = *use;
                                     if (node->OperGet() == GT_INTRINSIC &&
                                         Compiler::IsIntrinsicImplementedByUserCall(node->gtIntrinsic.gtIntrinsicId))
                                     {
                                         RewriteIntrinsicAsUserCall(use, walkData);
                                     }
                                     else if (node->OperIsLocal())
                                     {
                                         node->gtFlags &= ~GTF_VAR_USEDEF;
                                     }

                                     return Compiler::WALK_CONTINUE;
                                 },
                                 &splitData, true);

            GenTree* firstNodeInStatement = statement->gtStmtList;
            if (lastNodeInPreviousStatement != nullptr)
            {
                lastNodeInPreviousStatement->gtNext = firstNodeInStatement;
            }

            firstNodeInStatement->gtPrev = lastNodeInPreviousStatement;
            lastNodeInPreviousStatement  = statement->gtStmtExpr;
        }

        block->MakeLIR(firstStatement->gtStmtList, lastStatement->gtStmtExpr);

        // Rewrite HIR nodes into LIR nodes.
        for (GenTreeStmt *statement = firstStatement, *nextStatement; statement != nullptr; statement = nextStatement)
        {
            nextStatement = statement->getNextStmt();

            // If this statement has correct offset information, change it into an IL offset
            // node and insert it into the LIR.
            if (statement->gtStmtILoffsx != BAD_IL_OFFSET)
            {
                assert(!statement->IsPhiDefnStmt());
                statement->SetOper(GT_IL_OFFSET);
                statement->gtNext = nullptr;
                statement->gtPrev = nullptr;

                BlockRange().InsertBefore(statement->gtStmtList, statement);
            }

            m_statement = statement;
            comp->fgWalkTreePost(&statement->gtStmtExpr,
                                 [](GenTree** use, Compiler::fgWalkData* walkData) -> Compiler::fgWalkResult {
                                     return reinterpret_cast<Rationalizer*>(walkData->pCallbackData)
                                         ->RewriteNode(use, *walkData->parentStack);
                                 },
                                 this, true);
        }

        assert(BlockRange().CheckLIR(comp));
    }

    comp->compRationalIRForm = true;
}
#endif // LEGACY_BACKEND
