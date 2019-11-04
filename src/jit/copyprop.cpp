// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//
//
//                                    CopyProp
//
// This stage performs value numbering based copy propagation. Since copy propagation
// is about data flow, we cannot find them in assertion prop phase. In assertion prop
// we can identify copies, like so: if (a == b) else, i.e., control flow assertions.
//
// To identify data flow copies, we'll follow a similar approach to SSA renaming.
// We would walk each path in the graph keeping track of every live definition. Thus
// when we see a variable that shares the VN with a live definition, we'd replace this
// variable with the variable in the live definition, if suitable.
//
///////////////////////////////////////////////////////////////////////////////////////

#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#include "copyprop.h"
#include "ssabuilder.h"

CopyPropogation::CopyPropogation(Compiler* compiler)
    : Phase(compiler, "VnCopyPropogation", PHASE_VN_COPY_PROP)
    , m_memAllocator(compiler->getAllocator(CMK_CopyProp))
    , m_currKillSet(VarSetOps::MakeEmpty(compiler))
    , m_liveLclVarDefs(m_memAllocator)
{
}

void CopyPropogation::DoPhase()
{
    optVnCopyProp();
}

/**************************************************************************************
 *
 * Corresponding to the live definition pushes, pop the stack as we finish a sub-paths
 * of the graph originating from the block. Refer SSA renaming for any additional info.
 * "m_liveLclVarDefs" tracks the currently live definitions.
 */
void CopyPropogation::optBlockCopyPropPopStacks(BasicBlock* block)
{
    Statement* lastStmt = block->lastStmt();
    Statement* stmt     = block->lastStmt();
    while (stmt != nullptr)
    {
        GenTree* lastTree = stmt->GetRootNode();
        for (GenTree* tree = lastTree; tree != nullptr; tree = tree->gtPrev)
        {
            if (!tree->IsLocal())
            {
                continue;
            }
            unsigned lclNum = tree->AsLclVarCommon()->GetLclNum();
            if (!comp->lvaInSsa(lclNum))
            {
                continue;
            }
            if ((tree->gtFlags & GTF_VAR_DEF) != 0)
            {
                GenTreePtrStack* stack = nullptr;
                m_liveLclVarDefs.Lookup(lclNum, &stack);
                stack->Pop();
                if (stack->Empty())
                {
                    m_liveLclVarDefs.Remove(lclNum);
                }
            }
        }
        stmt = stmt->GetPrevStmt();
        if (stmt == lastStmt)
        {
            break;
        }
    }
}

#ifdef DEBUG
void CopyPropogation::optDumpCopyPropStack()
{
    JITDUMP("{ ");
    for (LclNumToGenTreePtrStack::KeyIterator iter = m_liveLclVarDefs.Begin(); !iter.Equal(m_liveLclVarDefs.End());
         ++iter)
    {
        GenTree* node = iter.GetValue()->Top();
        JITDUMP("%d-[%06d]:V%02u ", iter.Get(), comp->dspTreeID(node), node->AsLclVarCommon()->GetLclNum());
    }
    JITDUMP("}\n\n");
}
#endif
/*******************************************************************************************************
 *
 * Given the "lclVar" and "copyVar" compute if the copy prop will be beneficial.
 *
 */
// static
int CopyPropogation::optCopyProp_LclVarScore(LclVarDsc* lclVarDsc, LclVarDsc* copyVarDsc, bool preferOp2)
{
    int score = 0;

    if (lclVarDsc->lvVolatileHint)
    {
        score += 4;
    }

    if (copyVarDsc->lvVolatileHint)
    {
        score -= 4;
    }

    if (lclVarDsc->lvDoNotEnregister)
    {
        score += 4;
    }

    if (copyVarDsc->lvDoNotEnregister)
    {
        score -= 4;
    }

#ifdef _TARGET_X86_
    // For doubles we also prefer to change parameters into non-parameter local variables
    if (lclVarDsc->lvType == TYP_DOUBLE)
    {
        if (lclVarDsc->lvIsParam)
        {
            score += 2;
        }

        if (copyVarDsc->lvIsParam)
        {
            score -= 2;
        }
    }
#endif

    // Otherwise we prefer to use the op2LclNum
    return score + ((preferOp2) ? 1 : -1);
}

//------------------------------------------------------------------------------
// optCopyProp : Perform copy propagation on a given tree as we walk the graph and if it is a local
//               variable, then look up all currently live definitions and check if any of those
//               definitions share the same value number. If so, then we can make the replacement.
//
// Arguments:
//    block       -  Block the tree belongs to
//    stmt        -  Statement the tree belongs to
//    tree        -  The tree to perform copy propagation on
//
void CopyPropogation::optCopyProp(BasicBlock* block, Statement* stmt, GenTree* tree)
{
    // TODO-Review: EH successor/predecessor iteration seems broken.
    if ((block->bbCatchTyp == BBCT_FINALLY) || (block->bbCatchTyp == BBCT_FAULT))
    {
        return;
    }

    // If not local nothing to do.
    if (!tree->IsLocal())
    {
        return;
    }
    if ((tree->OperGet() == GT_PHI_ARG) || (tree->OperGet() == GT_LCL_FLD))
    {
        return;
    }

    // Propagate only on uses.
    if ((tree->gtFlags & GTF_VAR_DEF) != 0)
    {
        return;
    }
    unsigned lclNum = tree->AsLclVarCommon()->GetLclNum();

    // Skip non-SSA variables.
    if (!comp->lvaInSsa(lclNum))
    {
        return;
    }

    assert(tree->gtVNPair.GetConservative() != ValueNumStore::NoVN);

    for (LclNumToGenTreePtrStack::KeyIterator iter = m_liveLclVarDefs.Begin(); !iter.Equal(m_liveLclVarDefs.End());
         ++iter)
    {
        unsigned newLclNum = iter.Get();

        GenTree* op = iter.GetValue()->Top();

        // Nothing to do if same.
        if (lclNum == newLclNum)
        {
            continue;
        }

        LclVarDsc* lclVar    = comp->lvaGetDesc(lclNum);
        LclVarDsc* newLclVar = comp->lvaGetDesc(newLclNum);

        // Skip variables with assignments embedded in the statement (i.e., with a comma). Because we
        // are not currently updating their SSA names as live in the copy-prop pass of the stmt.
        if (VarSetOps::IsMember(comp, m_currKillSet, newLclVar->lvVarIndex))
        {
            continue;
        }

        // Do not copy propagate if the old and new lclVar have different 'doNotEnregister' settings.
        // This is primarily to avoid copy propagating to IND(ADDR(LCL_VAR)) where the replacement lclVar
        // is not marked 'lvDoNotEnregister'.
        // However, in addition, it may not be profitable to propagate a 'doNotEnregister' lclVar to an
        // existing use of an enregisterable lclVar.

        if (lclVar->lvDoNotEnregister != newLclVar->lvDoNotEnregister)
        {
            continue;
        }

        if ((op->gtFlags & GTF_VAR_CAST) != 0)
        {
            continue;
        }
        if ((comp->gsShadowVarInfo != nullptr) && newLclVar->lvIsParam &&
            (comp->gsShadowVarInfo[newLclNum].shadowCopy == lclNum))
        {
            continue;
        }
        ValueNum opVN = comp->GetUseAsgDefVNOrTreeVN(op);
        if (opVN == ValueNumStore::NoVN)
        {
            continue;
        }
        if (op->TypeGet() != tree->TypeGet())
        {
            continue;
        }
        if (opVN != tree->gtVNPair.GetConservative())
        {
            continue;
        }
        if (optCopyProp_LclVarScore(lclVar, newLclVar, true) <= 0)
        {
            continue;
        }
        // Check whether the newLclNum is live before being substituted. Otherwise, we could end
        // up in a situation where there must've been a phi node that got pruned because the variable
        // is not live anymore. For example,
        //  if
        //     x0 = 1
        //  else
        //     x1 = 2
        //  print(c) <-- x is not live here. Let's say 'c' shares the value number with "x0."
        //
        // If we simply substituted 'c' with "x0", we would be wrong. Ideally, there would be a phi
        // node x2 = phi(x0, x1) which can then be used to substitute 'c' with. But because of pruning
        // there would be no such phi node. To solve this we'll check if 'x' is live, before replacing
        // 'c' with 'x.'
        if (!newLclVar->lvVerTypeInfo.IsThisPtr())
        {
            if (newLclVar->lvAddrExposed)
            {
                continue;
            }

            // We compute liveness only on tracked variables. So skip untracked locals.
            if (!newLclVar->lvTracked)
            {
                continue;
            }

            // Because of this dependence on live variable analysis, CopyProp phase is immediately
            // after Liveness, SSA and VN.
            if (!VarSetOps::IsMember(comp, comp->compCurLife, newLclVar->lvVarIndex))
            {
                continue;
            }
        }
        unsigned newSsaNum = SsaConfig::RESERVED_SSA_NUM;
        if ((op->gtFlags & GTF_VAR_DEF) != 0)
        {
            newSsaNum = comp->GetSsaNumForLocalVarDef(op);
        }
        else // parameters, this pointer etc.
        {
            newSsaNum = op->AsLclVarCommon()->GetSsaNum();
        }

        if (newSsaNum == SsaConfig::RESERVED_SSA_NUM)
        {
            continue;
        }

#ifdef DEBUG
        if (VERBOSE)
        {
            JITDUMP("VN based copy assertion for ");
            comp->printTreeID(tree);
            printf(" V%02d @%08X by ", lclNum, tree->GetVN(VNK_Conservative));
            comp->printTreeID(op);
            printf(" V%02d @%08X.\n", newLclNum, op->GetVN(VNK_Conservative));
            comp->gtDispTree(tree, nullptr, nullptr, true);
        }
#endif

        tree->AsLclVarCommon()->SetLclNum(newLclNum);
        tree->AsLclVarCommon()->SetSsaNum(newSsaNum);
#ifdef DEBUG
        if (VERBOSE)
        {
            printf("copy propagated to:\n");
            comp->gtDispTree(tree, nullptr, nullptr, true);
        }
#endif
        break;
    }
    return;
}

/**************************************************************************************
 *
 * Helper to check if tree is a local that participates in SSA numbering.
 */
bool CopyPropogation::optIsSsaLocal(GenTree* tree)
{
    return tree->IsLocal() && comp->lvaInSsa(tree->AsLclVarCommon()->GetLclNum());
}

//------------------------------------------------------------------------------
// optBlockCopyProp : Perform copy propagation using currently live definitions on the current block's
//                    variables. Also as new definitions are encountered update the "m_liveLclVarDefs" which
//                    tracks the currently live definitions.
//
// Arguments:
//    block       -  Block the tree belongs to
//
void CopyPropogation::optBlockCopyProp(BasicBlock* block)
{
    JITDUMP("Copy Assertion for " FMT_BB "\n", block->bbNum);
    JITDUMP("  curSsaName stack: ");
    DBEXEC(VERBOSE, optDumpCopyPropStack());

    // We are not generating code so we don't need to deal with liveness change
    TreeLifeUpdater<false> treeLifeUpdater(comp);

    // There are no definitions at the start of the block. So clear it.
    comp->compCurLifeTree = nullptr;
    VarSetOps::Assign(comp, comp->compCurLife, block->bbLiveIn);
    for (Statement* stmt : block->Statements())
    {
        VarSetOps::ClearD(comp, m_currKillSet);

        // Walk the tree to find if any local variable can be replaced with current live definitions.
        for (GenTree* tree = stmt->GetTreeList(); tree != nullptr; tree = tree->gtNext)
        {
            treeLifeUpdater.UpdateLife(tree);

            optCopyProp(block, stmt, tree);

            // TODO-Review: Merge this loop with the following loop to correctly update the
            // live SSA num while also propagating copies.
            //
            // 1. This loop performs copy prop with currently live (on-top-of-stack) SSA num.
            // 2. The subsequent loop maintains a stack for each lclNum with
            //    currently active SSA numbers when definitions are encountered.
            //
            // If there is an embedded definition using a "comma" in a stmt, then the currently
            // live SSA number will get updated only in the next loop (2). However, this new
            // definition is now supposed to be live (on tos). If we did not update the stacks
            // using (2), copy prop (1) will use a SSA num defined outside the stmt ignoring the
            // embedded update. Killing the variable is a simplification to produce 0 ASM diffs
            // for an update release.
            //
            if (optIsSsaLocal(tree) && (tree->gtFlags & GTF_VAR_DEF))
            {
                unsigned lclNum = tree->AsLclVarCommon()->GetLclNum();
                VarSetOps::AddElemD(comp, m_currKillSet, comp->lvaGetDesc(lclNum)->lvVarIndex);
            }
        }

        // This logic must be in sync with SSA renaming process.
        for (GenTree* tree = stmt->GetTreeList(); tree != nullptr; tree = tree->gtNext)
        {
            if (!optIsSsaLocal(tree))
            {
                continue;
            }

            unsigned lclNum = tree->AsLclVarCommon()->GetLclNum();

            // As we encounter a definition add it to the stack as a live definition.
            if ((tree->gtFlags & GTF_VAR_DEF) != 0)
            {
                GenTreePtrStack* stack;
                if (!m_liveLclVarDefs.Lookup(lclNum, &stack))
                {
                    stack = new (m_memAllocator) GenTreePtrStack(m_memAllocator);
                }
                stack->Push(tree);
                m_liveLclVarDefs.Set(lclNum, stack DEBUGARG(JitHashSetKind::Overwrite));
            }
            // If we encounter first use of a param or this pointer add it as a live definition.
            // Since they are always live, do it only once.
            else if ((tree->gtOper == GT_LCL_VAR) && !(tree->gtFlags & GTF_VAR_USEASG) &&
                     (comp->lvaGetDesc(lclNum)->lvIsParam || comp->lvaGetDesc(lclNum)->lvVerTypeInfo.IsThisPtr()))
            {
                GenTreePtrStack* stack;
                if (!m_liveLclVarDefs.Lookup(lclNum, &stack))
                {
                    stack = new (m_memAllocator) GenTreePtrStack(m_memAllocator);
                    stack->Push(tree);
                    m_liveLclVarDefs.Set(lclNum, stack);
                }
            }
        }
    }
}

/**************************************************************************************
 *
 * This stage performs value numbering based copy propagation. Since copy propagation
 * is about data flow, we cannot find them in assertion prop phase. In assertion prop
 * we can identify copies that like so: if (a == b) else, i.e., control flow assertions.
 *
 * To identify data flow copies, we follow a similar approach to SSA renaming. We walk
 * each path in the graph keeping track of every live definition. Thus when we see a
 * variable that shares the VN with a live definition, we'd replace this variable with
 * the variable in the live definition.
 *
 * We do this to be in conventional SSA form. This can very well be changed later.
 *
 * For example, on some path in the graph:
 *    a0 = x0
 *    :            <- other blocks
 *    :
 *    a1 = y0
 *    :
 *    :            <- other blocks
 *    b0 = x0, we cannot substitute x0 with a0, because currently our backend doesn't
 * treat lclNum and ssaNum together as a variable, but just looks at lclNum. If we
 * substituted x0 with a0, then we'd be in general SSA form.
 *
 */
void CopyPropogation::optVnCopyProp()
{
    JITDUMP("*************** In optVnCopyProp()\n");

    if (comp->fgSsaPassesCompleted == 0)
    {
        return;
    }

    // Compute the domTree to use.
    BlkToBlkVectorMap* domTree = new (m_memAllocator) BlkToBlkVectorMap(m_memAllocator);
    domTree->Reallocate(comp->fgBBcount * 3 / 2); // Prime the allocation
    SsaBuilder::ComputeDominators(comp, domTree);

    struct BlockWork
    {
        BasicBlock* m_blk;
        bool        m_processed;

        BlockWork(BasicBlock* blk, bool processed = false) : m_blk(blk), m_processed(processed)
        {
        }
    };
    typedef jitstd::vector<BlockWork> BlockWorkStack;

    VarSetOps::AssignNoCopy(comp, comp->compCurLife, VarSetOps::MakeEmpty(comp));

    BlockWorkStack* worklist = new (m_memAllocator) BlockWorkStack(m_memAllocator);

    worklist->push_back(BlockWork(comp->fgFirstBB));
    while (!worklist->empty())
    {
        BlockWork work = worklist->back();
        worklist->pop_back();

        BasicBlock* block = work.m_blk;
        if (work.m_processed)
        {
            // Pop all the live definitions for this block.
            optBlockCopyPropPopStacks(block);
            continue;
        }

        // Generate copy assertions in this block, and keeping m_liveLclVarDefs variable up to date.
        worklist->push_back(BlockWork(block, true));

        optBlockCopyProp(block);

        // Add dom children to work on.
        BlkVector* domChildren = domTree->LookupPointer(block);
        if (domChildren != nullptr)
        {
            for (BasicBlock* child : *domChildren)
            {
                worklist->push_back(BlockWork(child));
            }
        }
    }

    // Tracked variable count increases after CopyProp, so don't keep a shorter array around.
    // Destroy (release) the varset.
    VarSetOps::AssignNoCopy(comp, comp->compCurLife, VarSetOps::UninitVal());
}
