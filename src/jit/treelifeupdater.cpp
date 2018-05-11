#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#include "compiler.h"

#ifndef LEGACY_BACKEND

// Update liveness (always var liveness, i.e., compCurLife, and also, if "ForCodeGen" is true, reg liveness, i.e.,
// regSet.rsMaskVars as well)
// if the given lclVar (or indir(addr(local)))/regVar node is going live (being born) or dying.
template <bool ForCodeGen>
void Compiler::compUpdateLifeVar(GenTree* tree, VARSET_TP* pLastUseVars)
{
    GenTree* indirAddrLocal = fgIsIndirOfAddrOfLocal(tree);
    assert(tree->OperIsNonPhiLocal() || indirAddrLocal != nullptr);

    // Get the local var tree -- if "tree" is "Ldobj(addr(x))", or "ind(addr(x))" this is "x", else it's "tree".
    GenTree* lclVarTree = indirAddrLocal;
    if (lclVarTree == nullptr)
    {
        lclVarTree = tree;
    }
    unsigned int lclNum = lclVarTree->gtLclVarCommon.gtLclNum;
    LclVarDsc*   varDsc = lvaTable + lclNum;

#ifdef DEBUG
#if !defined(_TARGET_AMD64_)
    // There are no addr nodes on ARM and we are experimenting with encountering vars in 'random' order.
    // Struct fields are not traversed in a consistent order, so ignore them when
    // verifying that we see the var nodes in execution order
    if (ForCodeGen)
    {
        if (tree->OperIsIndir())
        {
            assert(indirAddrLocal != NULL);
        }
        else if (tree->gtNext != NULL && tree->gtNext->gtOper == GT_ADDR &&
                 ((tree->gtNext->gtNext == NULL || !tree->gtNext->gtNext->OperIsIndir())))
        {
            assert(tree->IsLocal()); // Can only take the address of a local.
            // The ADDR might occur in a context where the address it contributes is eventually
            // dereferenced, so we can't say that this is not a use or def.
        }
#if 0   
        // TODO-ARM64-Bug?: These asserts don't seem right for ARM64: I don't understand why we have to assert 
        // two consecutive lclvars (in execution order) can only be observed if the first one is a struct field.
        // It seems to me this is code only applicable to the legacy JIT and not RyuJIT (and therefore why it was 
        // ifdef'ed out for AMD64).
        else if (!varDsc->lvIsStructField)
        {
            GenTree* prevTree;
            for (prevTree = tree->gtPrev;
                 prevTree != NULL && prevTree != compCurLifeTree;
                 prevTree = prevTree->gtPrev)
            {
                if ((prevTree->gtOper == GT_LCL_VAR) || (prevTree->gtOper == GT_REG_VAR))
                {
                    LclVarDsc * prevVarDsc = lvaTable + prevTree->gtLclVarCommon.gtLclNum;

                    // These are the only things for which this method MUST be called
                    assert(prevVarDsc->lvIsStructField);
                }
            }
            assert(prevTree == compCurLifeTree);
        }
#endif // 0
    }
#endif // !_TARGET_AMD64_
#endif // DEBUG

    compCurLifeTree = tree;
    VARSET_TP newLife(VarSetOps::MakeCopy(this, compCurLife));

    // By codegen, a struct may not be TYP_STRUCT, so we have to
    // check lvPromoted, for the case where the fields are being
    // tracked.
    if (!varDsc->lvTracked && !varDsc->lvPromoted)
    {
        return;
    }

    bool isBorn = ((tree->gtFlags & GTF_VAR_DEF) != 0 && (tree->gtFlags & GTF_VAR_USEASG) == 0); // if it's "x <op>=
                                                                                                 // ..." then variable
                                                                                                 // "x" must have had a
                                                                                                 // previous, original,
                                                                                                 // site to be born.
    bool isDying = ((tree->gtFlags & GTF_VAR_DEATH) != 0);
#ifndef LEGACY_BACKEND
    bool spill = ((tree->gtFlags & GTF_SPILL) != 0);
#endif // !LEGACY_BACKEND

#ifndef LEGACY_BACKEND
    // For RyuJIT backend, since all tracked vars are register candidates, but not all are in registers at all times,
    // we maintain two separate sets of variables - the total set of variables that are either
    // born or dying here, and the subset of those that are on the stack
    VARSET_TP stackVarDeltaSet(VarSetOps::MakeEmpty(this));
#endif // !LEGACY_BACKEND

    if (isBorn || isDying)
    {
        bool hasDeadTrackedFieldVars = false; // If this is true, then, for a LDOBJ(ADDR(<promoted struct local>)),
        VARSET_TP* deadTrackedFieldVars =
            nullptr; // *deadTrackedFieldVars indicates which tracked field vars are dying.
        VARSET_TP varDeltaSet(VarSetOps::MakeEmpty(this));

        if (varDsc->lvTracked)
        {
            VarSetOps::AddElemD(this, varDeltaSet, varDsc->lvVarIndex);
            if (ForCodeGen)
            {
#ifndef LEGACY_BACKEND
                if (isBorn && varDsc->lvIsRegCandidate() && tree->gtHasReg())
                {
                    codeGen->genUpdateVarReg(varDsc, tree);
                }
#endif // !LEGACY_BACKEND
                if (varDsc->lvIsInReg()
#ifndef LEGACY_BACKEND
                    && tree->gtRegNum != REG_NA
#endif // !LEGACY_BACKEND
                    )
                {
                    codeGen->genUpdateRegLife(varDsc, isBorn, isDying DEBUGARG(tree));
                }
#ifndef LEGACY_BACKEND
                else
                {
                    VarSetOps::AddElemD(this, stackVarDeltaSet, varDsc->lvVarIndex);
                }
#endif // !LEGACY_BACKEND
            }
        }
        else if (varDsc->lvPromoted)
        {
            if (indirAddrLocal != nullptr && isDying)
            {
                assert(!isBorn); // GTF_VAR_DEATH only set for LDOBJ last use.
                hasDeadTrackedFieldVars = GetPromotedStructDeathVars()->Lookup(indirAddrLocal, &deadTrackedFieldVars);
                if (hasDeadTrackedFieldVars)
                {
                    VarSetOps::Assign(this, varDeltaSet, *deadTrackedFieldVars);
                }
            }

            for (unsigned i = varDsc->lvFieldLclStart; i < varDsc->lvFieldLclStart + varDsc->lvFieldCnt; ++i)
            {
                LclVarDsc* fldVarDsc = &(lvaTable[i]);
                noway_assert(fldVarDsc->lvIsStructField);
                if (fldVarDsc->lvTracked)
                {
                    unsigned fldVarIndex = fldVarDsc->lvVarIndex;
                    noway_assert(fldVarIndex < lvaTrackedCount);
                    if (!hasDeadTrackedFieldVars)
                    {
                        VarSetOps::AddElemD(this, varDeltaSet, fldVarIndex);
                        if (ForCodeGen)
                        {
                            // We repeat this call here and below to avoid the VarSetOps::IsMember
                            // test in this, the common case, where we have no deadTrackedFieldVars.
                            if (fldVarDsc->lvIsInReg())
                            {
#ifndef LEGACY_BACKEND
                                if (isBorn)
                                {
                                    codeGen->genUpdateVarReg(fldVarDsc, tree);
                                }
#endif // !LEGACY_BACKEND
                                codeGen->genUpdateRegLife(fldVarDsc, isBorn, isDying DEBUGARG(tree));
                            }
#ifndef LEGACY_BACKEND
                            else
                            {
                                VarSetOps::AddElemD(this, stackVarDeltaSet, fldVarIndex);
                            }
#endif // !LEGACY_BACKEND
                        }
                    }
                    else if (ForCodeGen && VarSetOps::IsMember(this, varDeltaSet, fldVarIndex))
                    {
                        if (lvaTable[i].lvIsInReg())
                        {
#ifndef LEGACY_BACKEND
                            if (isBorn)
                            {
                                codeGen->genUpdateVarReg(fldVarDsc, tree);
                            }
#endif // !LEGACY_BACKEND
                            codeGen->genUpdateRegLife(fldVarDsc, isBorn, isDying DEBUGARG(tree));
                        }
#ifndef LEGACY_BACKEND
                        else
                        {
                            VarSetOps::AddElemD(this, stackVarDeltaSet, fldVarIndex);
                        }
#endif // !LEGACY_BACKEND
                    }
                }
            }
        }

        // First, update the live set
        if (isDying)
        {
            // We'd like to be able to assert the following, however if we are walking
            // through a qmark/colon tree, we may encounter multiple last-use nodes.
            // assert (VarSetOps::IsSubset(compiler, regVarDeltaSet, newLife));
            VarSetOps::DiffD(this, newLife, varDeltaSet);
            if (pLastUseVars != nullptr)
            {
                VarSetOps::Assign(this, *pLastUseVars, varDeltaSet);
            }
        }
        else
        {
            // This shouldn't be in newLife, unless this is debug code, in which
            // case we keep vars live everywhere, OR the variable is address-exposed,
            // OR this block is part of a try block, in which case it may be live at the handler
            // Could add a check that, if it's in newLife, that it's also in
            // fgGetHandlerLiveVars(compCurBB), but seems excessive
            //
            // For a dead store, it can be the case that we set both isBorn and isDying to true.
            // (We don't eliminate dead stores under MinOpts, so we can't assume they're always
            // eliminated.)  If it's both, we handled it above.
            VarSetOps::UnionD(this, newLife, varDeltaSet);
        }
    }

    if (!VarSetOps::Equal(this, compCurLife, newLife))
    {
#ifdef DEBUG
        if (verbose)
        {
            printf("\t\t\t\t\t\t\tLive vars: ");
            dumpConvertedVarSet(this, compCurLife);
            printf(" => ");
            dumpConvertedVarSet(this, newLife);
            printf("\n");
        }
#endif // DEBUG

        VarSetOps::Assign(this, compCurLife, newLife);

        if (ForCodeGen)
        {
#ifndef LEGACY_BACKEND

            // Only add vars to the gcInfo.gcVarPtrSetCur if they are currently on stack, since the
            // gcInfo.gcTrkStkPtrLcls
            // includes all TRACKED vars that EVER live on the stack (i.e. are not always in a register).
            VARSET_TP gcTrkStkDeltaSet(
                VarSetOps::Intersection(this, codeGen->gcInfo.gcTrkStkPtrLcls, stackVarDeltaSet));
            if (!VarSetOps::IsEmpty(this, gcTrkStkDeltaSet))
            {
#ifdef DEBUG
                if (verbose)
                {
                    printf("\t\t\t\t\t\t\tGCvars: ");
                    dumpConvertedVarSet(this, codeGen->gcInfo.gcVarPtrSetCur);
                    printf(" => ");
                }
#endif // DEBUG

                if (isBorn)
                {
                    VarSetOps::UnionD(this, codeGen->gcInfo.gcVarPtrSetCur, gcTrkStkDeltaSet);
                }
                else
                {
                    VarSetOps::DiffD(this, codeGen->gcInfo.gcVarPtrSetCur, gcTrkStkDeltaSet);
                }

#ifdef DEBUG
                if (verbose)
                {
                    dumpConvertedVarSet(this, codeGen->gcInfo.gcVarPtrSetCur);
                    printf("\n");
                }
#endif // DEBUG
            }

#else // LEGACY_BACKEND

#ifdef DEBUG
            if (verbose)
            {
                VARSET_TP gcVarPtrSetNew(VarSetOps::Intersection(this, newLife, codeGen->gcInfo.gcTrkStkPtrLcls));
                if (!VarSetOps::Equal(this, codeGen->gcInfo.gcVarPtrSetCur, gcVarPtrSetNew))
                {
                    printf("\t\t\t\t\t\t\tGCvars: ");
                    dumpConvertedVarSet(this, codeGen->gcInfo.gcVarPtrSetCur);
                    printf(" => ");
                    dumpConvertedVarSet(this, gcVarPtrSetNew);
                    printf("\n");
                }
            }
#endif // DEBUG

            VarSetOps::AssignNoCopy(this, codeGen->gcInfo.gcVarPtrSetCur,
                                    VarSetOps::Intersection(this, newLife, codeGen->gcInfo.gcTrkStkPtrLcls));

#endif // LEGACY_BACKEND

            codeGen->siUpdate();
        }
    }

#ifndef LEGACY_BACKEND
    if (ForCodeGen && spill)
    {
        assert(!varDsc->lvPromoted);
        codeGen->genSpillVar(tree);
        if (VarSetOps::IsMember(this, codeGen->gcInfo.gcTrkStkPtrLcls, varDsc->lvVarIndex))
        {
            if (!VarSetOps::IsMember(this, codeGen->gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex))
            {
                VarSetOps::AddElemD(this, codeGen->gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex);
#ifdef DEBUG
                if (verbose)
                {
                    printf("\t\t\t\t\t\t\tVar V%02u becoming live\n", varDsc - lvaTable);
                }
#endif // DEBUG
            }
        }
    }
#endif // !LEGACY_BACKEND
}

// Need an explicit instantiation.
template void Compiler::compUpdateLifeVar<true>(GenTree* tree, VARSET_TP* pLastUseVars);
template void Compiler::compUpdateLifeVar<false>(GenTree* tree, VARSET_TP* pLastUseVars);

/*****************************************************************************
  *
  *  Update the current set of live variables based on the life set recorded
  *  in the given expression tree node.
  */

template <bool ForCodeGen>
inline void Compiler::compUpdateLife(GenTree* tree)
{
    // TODO-Cleanup: We shouldn't really be calling this more than once
    if (tree == compCurLifeTree)
    {
        return;
    }

    if (!tree->OperIsNonPhiLocal() && fgIsIndirOfAddrOfLocal(tree) == nullptr)
    {
        return;
    }

    compUpdateLifeVar<ForCodeGen>(tree);
}

template void Compiler::compUpdateLife<false>(GenTree* tree);
template void Compiler::compUpdateLife<true>(GenTree* tree);

#endif // !LEGACY_BACKEND
