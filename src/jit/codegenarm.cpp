//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                        ARM Code Generator                                 XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/
#include "jitpch.h"
#ifdef _MSC_VER
#pragma hdrstop
#endif

#ifndef LEGACY_BACKEND // This file is ONLY used for the RyuJIT backend that uses the linear scan register allocator

#ifdef _TARGET_ARM_
#include "codegen.h"
#include "lower.h"
#include "gcinfo.h"
#include "emit.h"

#ifndef JIT32_GCENCODER
#include "gcinfoencoder.h"
#endif

// Get the register assigned to the given node

regNumber CodeGenInterface::genGetAssignedReg(GenTreePtr tree)
{
    return tree->gtRegNum;
}


//------------------------------------------------------------------------
// genSpillVar: Spill a local variable
//
// Arguments:
//    tree      - the lclVar node for the variable being spilled
//
// Return Value:
//    None.
//
// Assumptions:
//    The lclVar must be a register candidate (lvRegCandidate)

void                CodeGen::genSpillVar(GenTreePtr tree)
{
    unsigned varNum = tree->gtLclVarCommon.gtLclNum;
    LclVarDsc * varDsc = &(compiler->lvaTable[varNum]);

    // We don't actually need to spill if it is already living in memory
    bool needsSpill = ((tree->gtFlags & GTF_VAR_DEF) == 0 && varDsc->lvIsInReg());
    if (needsSpill)
    {
        var_types lclTyp = varDsc->TypeGet();
        if (varDsc->lvNormalizeOnStore())
            lclTyp = genActualType(lclTyp);
        emitAttr size = emitTypeSize(lclTyp);

        bool restoreRegVar = false;
        if  (tree->gtOper == GT_REG_VAR)
        {
            tree->SetOper(GT_LCL_VAR);
            restoreRegVar = true;
        }

        // mask off the flag to generate the right spill code, then bring it back
        tree->gtFlags   &= ~GTF_REG_VAL;

        instruction storeIns = ins_Store(tree->TypeGet());

        if (varTypeIsMultiReg(tree))
        {
            assert(varDsc->lvRegNum   == genRegPairLo(tree->gtRegPair));
            assert(varDsc->lvOtherReg == genRegPairHi(tree->gtRegPair));
            regNumber regLo = genRegPairLo(tree->gtRegPair);
            regNumber regHi = genRegPairHi(tree->gtRegPair);
            inst_TT_RV(storeIns, tree, regLo);
            inst_TT_RV(storeIns, tree, regHi, 4);
        }
        else
        {
            assert(varDsc->lvRegNum == tree->gtRegNum);
            inst_TT_RV(storeIns, tree, tree->gtRegNum);
        }
        tree->gtFlags    |= GTF_REG_VAL;

        if (restoreRegVar)
        {
            tree->SetOper(GT_REG_VAR);
        }

        genUpdateRegLife(varDsc, /*isBorn*/ false, /*isDying*/ true DEBUGARG(tree));
        gcInfo.gcMarkRegSetNpt(varDsc->lvRegMask());

        if (VarSetOps::IsMember(compiler, gcInfo.gcTrkStkPtrLcls, varDsc->lvVarIndex))
        {
#ifdef DEBUG
            if (!VarSetOps::IsMember(compiler, gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex))
            {
                JITDUMP("\t\t\t\t\t\t\tVar V%02u becoming live\n", varNum);
            }
            else
            {
                JITDUMP("\t\t\t\t\t\t\tVar V%02u continuing live\n", varNum);
            }
#endif
            VarSetOps::AddElemD(compiler, gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex);
        }

    }

    tree->gtFlags    &= ~GTF_SPILL;
    varDsc->lvRegNum = REG_STK;
    if (varTypeIsMultiReg(tree))
    {
        varDsc->lvOtherReg = REG_STK;
    }
}

// inline
void                CodeGenInterface::genUpdateVarReg(LclVarDsc * varDsc, GenTreePtr tree)
{
    assert(tree->OperIsScalarLocal() || (tree->gtOper == GT_COPY));
    varDsc->lvRegNum = tree->gtRegNum;
}


/*****************************************************************************
 *
 *  Generate code that will set the given register to the integer constant.
 */

void                CodeGen::genSetRegToIcon(regNumber     reg,
                                             ssize_t       val,
                                             var_types     type,
                                             insFlags      flags)
{
    // Reg cannot be a FP reg
    assert(!genIsValidFloatReg(reg));

    // The only TYP_REF constant that can come this path is a managed 'null' since it is not
    // relocatable.  Other ref type constants (e.g. string objects) go through a different
    // code path.
    noway_assert(type != TYP_REF || val == 0);

    instGen_Set_Reg_To_Imm(emitActualTypeSize(type), reg, val);
}


/*****************************************************************************
 *
 *   Generate code to check that the GS cookie wasn't thrashed by a buffer
 *   overrun.  If pushReg is true, preserve all registers around code sequence.
 *   Otherwise, ECX maybe modified.
 */
void                CodeGen::genEmitGSCookieCheck(bool pushReg)
{
    NYI("ARM genEmitGSCookieCheck is not yet implemented for protojit");
}

/*****************************************************************************
 *
 *  Generate code for all the basic blocks in the function.
 */

void                CodeGen::genCodeForBBlist()
{
    unsigned        varNum;
    LclVarDsc   *   varDsc;

    unsigned        savedStkLvl;

#ifdef  DEBUG
    genInterruptibleUsed        = true;
    unsigned        stmtNum     = 0;
    unsigned        totalCostEx = 0;
    unsigned        totalCostSz = 0;

    // You have to be careful if you create basic blocks from now on
    compiler->fgSafeBasicBlockCreation = false;

    // This stress mode is not comptible with fully interruptible GC
    if (genInterruptible && compiler->opts.compStackCheckOnCall)
    {
        compiler->opts.compStackCheckOnCall = false;
    }

    // This stress mode is not comptible with fully interruptible GC
    if (genInterruptible && compiler->opts.compStackCheckOnRet)
    {
        compiler->opts.compStackCheckOnRet = false;
    }
#endif

    // Prepare the blocks for exception handling codegen: mark the blocks that needs labels.
    genPrepForEHCodegen();

    assert(!compiler->fgFirstBBScratch || compiler->fgFirstBB == compiler->fgFirstBBScratch); // compiler->fgFirstBBScratch has to be first.

    /* Initialize the spill tracking logic */

    regSet.rsSpillBeg();

    /* Initialize the line# tracking logic */

#ifdef DEBUGGING_SUPPORT
    if (compiler->opts.compScopeInfo)
    {
        siInit();
    }
#endif

#if 0
    if (compiler->opts.compDbgEnC)
    {
        noway_assert(isFramePointerUsed());
        regSet.rsSetRegsModified(RBM_INT_CALLEE_SAVED & ~RBM_FPBASE);
    }

#if INLINE_NDIRECT
    /* If we have any pinvoke calls, we might potentially trash everything */
    if  (compiler->info.compCallUnmanaged)
    {
        noway_assert(isFramePointerUsed());  // Setup of Pinvoke frame currently requires an EBP style frame
        regSet.rsSetRegsModified(RBM_INT_CALLEE_SAVED & ~RBM_FPBASE);
    }

#endif // INLINE_NDIRECT
#endif

    // The current implementation of switch tables requires the first block to have a label so it
    // can generate offsets to the switch label targets.
    // TODO-ARM-CQ: remove this when switches have been re-implemented to not use this.
    if (compiler->fgHasSwitch)
    {
        compiler->fgFirstBB->bbFlags |= BBF_JMP_TARGET;
    }

    genPendingCallLabel = nullptr;

    /* Initialize the pointer tracking code */

    gcInfo.gcRegPtrSetInit();
    gcInfo.gcVarPtrSetInit();

    /* If any arguments live in registers, mark those regs as such */

    for (varNum = 0, varDsc = compiler->lvaTable;
         varNum < compiler->lvaCount;
         varNum++  , varDsc++)
    {
        /* Is this variable a parameter assigned to a register? */

        if  (!varDsc->lvIsParam || !varDsc->lvRegister)
            continue;

        /* Is the argument live on entry to the method? */

        if  (!VarSetOps::IsMember(compiler, compiler->fgFirstBB->bbLiveIn, varDsc->lvVarIndex))
            continue;

        /* Is this a floating-point argument? */

        if (varDsc->IsFloatRegType())
            continue;

        noway_assert(!varTypeIsFloating(varDsc->TypeGet()));

        /* Mark the register as holding the variable */

        regTracker.rsTrackRegLclVar(varDsc->lvRegNum, varNum);
    }

    unsigned finallyNesting = 0;

    // Make sure a set is allocated for compiler->compCurLife (in the long case), so we can set it to empty without
    // allocation at the start of each basic block.
    VarSetOps::AssignNoCopy(compiler, compiler->compCurLife, VarSetOps::MakeEmpty(compiler));

    /*-------------------------------------------------------------------------
     *
     *  Walk the basic blocks and generate code for each one
     *
     */

    BasicBlock *    block;
    BasicBlock *    lblk;  /* previous block */

    for (lblk =  NULL, block  = compiler->fgFirstBB;
                       block != NULL;
         lblk = block, block  = block->bbNext)
    {
#ifdef DEBUG
        if (compiler->verbose)
        {
            printf("\n=============== Generating ");
            block->dspBlockHeader(compiler, true, true);
            compiler->fgDispBBLiveness(block);
        }
#endif // DEBUG

        /* Figure out which registers hold variables on entry to this block */

        regSet.rsMaskVars       = RBM_NONE;
        gcInfo.gcRegGCrefSetCur = RBM_NONE;
        gcInfo.gcRegByrefSetCur = RBM_NONE;

        compiler->m_pLinearScan->recordVarLocationsAtStartOfBB(block);

        genUpdateLife(block->bbLiveIn);

        // Even if liveness didn't change, we need to update the registers containing GC references.
        // genUpdateLife will update the registers live due to liveness changes. But what about registers that didn't change?
        // We cleared them out above. Maybe we should just not clear them out, but update the ones that change here.
        // That would require handling the changes in recordVarLocationsAtStartOfBB().

        regMaskTP newLiveRegSet = RBM_NONE;
        regMaskTP newRegGCrefSet = RBM_NONE;
        regMaskTP newRegByrefSet = RBM_NONE;
#ifdef DEBUG
        VARSET_TP VARSET_INIT_NOCOPY(removedGCVars, VarSetOps::MakeEmpty(compiler));
        VARSET_TP VARSET_INIT_NOCOPY(addedGCVars, VarSetOps::MakeEmpty(compiler));
#endif
        VARSET_ITER_INIT(compiler, iter, block->bbLiveIn, varIndex);
        while (iter.NextElem(compiler, &varIndex))
        {
            unsigned             varNum  = compiler->lvaTrackedToVarNum[varIndex];
            LclVarDsc*           varDsc  = &(compiler->lvaTable[varNum]);

            if (varDsc->lvIsInReg())
            {
                newLiveRegSet |= varDsc->lvRegMask();
                if (varDsc->lvType == TYP_REF)
                {
                    newRegGCrefSet |= varDsc->lvRegMask();
                }
                else if (varDsc->lvType == TYP_BYREF)
                {
                    newRegByrefSet |= varDsc->lvRegMask();
                }
#ifdef DEBUG
                if (verbose && VarSetOps::IsMember(compiler, gcInfo.gcVarPtrSetCur, varIndex))
                {
                    VarSetOps::AddElemD(compiler, removedGCVars, varIndex);
                }
#endif // DEBUG
                VarSetOps::RemoveElemD(compiler, gcInfo.gcVarPtrSetCur, varIndex);
            }
            else if (compiler->lvaIsGCTracked(varDsc))
            {
#ifdef DEBUG
                if (verbose && !VarSetOps::IsMember(compiler, gcInfo.gcVarPtrSetCur, varIndex))
                {
                    VarSetOps::AddElemD(compiler, addedGCVars, varIndex);
                }
#endif // DEBUG
                VarSetOps::AddElemD(compiler, gcInfo.gcVarPtrSetCur, varIndex);
            }
        }

#ifdef DEBUG
        if (compiler->verbose)
        {
            printf("\t\t\t\t\t\t\tLive regs: ");
            if (regSet.rsMaskVars == newLiveRegSet)
            {
                printf("(unchanged) ");
            }
            else
            {
                printRegMaskInt(regSet.rsMaskVars);
                compiler->getEmitter()->emitDispRegSet(regSet.rsMaskVars);
                printf(" => ");
            }
            printRegMaskInt(newLiveRegSet);
            compiler->getEmitter()->emitDispRegSet(newLiveRegSet);
            printf("\n");
            if (!VarSetOps::IsEmpty(compiler, addedGCVars))
            {
                printf("\t\t\t\t\t\t\tAdded GCVars: ");
                dumpConvertedVarSet(compiler, addedGCVars);
                printf("\n");
            }
            if (!VarSetOps::IsEmpty(compiler, removedGCVars))
            {
                printf("\t\t\t\t\t\t\tRemoved GCVars: ");
                dumpConvertedVarSet(compiler, removedGCVars);
                printf("\n");
            }
        }
#endif // DEBUG

        regSet.rsMaskVars = newLiveRegSet;
        gcInfo.gcMarkRegSetGCref(newRegGCrefSet DEBUG_ARG(true));
        gcInfo.gcMarkRegSetByref(newRegByrefSet DEBUG_ARG(true));

        /* Blocks with handlerGetsXcptnObj()==true use GT_CATCH_ARG to
           represent the exception object (TYP_REF).
           We mark REG_EXCEPTION_OBJECT as holding a GC object on entry
           to the block,  it will be the first thing evaluated
           (thanks to GTF_ORDER_SIDEEFF).
         */

        if (handlerGetsXcptnObj(block->bbCatchTyp))
        {
#if JIT_FEATURE_SSA_SKIP_DEFS
            GenTreePtr firstStmt = block->FirstNonPhiDef();
#else
            GenTreePtr firstStmt = block->bbTreeList;
#endif
            if (firstStmt != NULL)
            {
                GenTreePtr firstTree = firstStmt->gtStmt.gtStmtExpr;
                if (compiler->gtHasCatchArg(firstTree))
                {
                    gcInfo.gcMarkRegSetGCref(RBM_EXCEPTION_OBJECT);
                }
            }
        }

        /* Start a new code output block */

#if FEATURE_EH_FUNCLETS
#if defined(_TARGET_ARM_)
        // If this block is the target of a finally return, we need to add a preceding NOP, in the same EH region,
        // so the unwinder doesn't get confused by our "movw lr, xxx; movt lr, xxx; b Lyyy" calling convention that
        // calls the funclet during non-exceptional control flow.
        if (block->bbFlags & BBF_FINALLY_TARGET)
        {
            assert(block->bbFlags & BBF_JMP_TARGET);

            // Create a label that we'll use for computing the start of an EH region, if this block is
            // at the beginning of such a region. If we used the existing bbEmitCookie as is for
            // determining the EH regions, then this NOP would end up outside of the region, if this
            // block starts an EH region. If we pointed the existing bbEmitCookie here, then the NOP
            // would be executed, which we would prefer not to do.

#ifdef  DEBUG
            if (compiler->verbose)
            {
                printf("\nEmitting finally target NOP predecessor for BB%02u\n", block->bbNum);
            }
#endif

            block->bbUnwindNopEmitCookie = getEmitter()->emitAddLabel(gcInfo.gcVarPtrSetCur,
                                                                      gcInfo.gcRegGCrefSetCur,
                                                                      gcInfo.gcRegByrefSetCur);
            
            instGen(INS_nop);
        }
#endif // defined(_TARGET_ARM_)

        genUpdateCurrentFunclet(block);
#endif // FEATURE_EH_FUNCLETS

#ifdef _TARGET_XARCH_
        if (genAlignLoops && block->bbFlags & BBF_LOOP_HEAD)
        {
            getEmitter()->emitLoopAlign();
        }
#endif

#ifdef  DEBUG
        if  (compiler->opts.dspCode)
            printf("\n      L_M%03u_BB%02u:\n", Compiler::s_compMethodsCount, block->bbNum);
#endif

        block->bbEmitCookie = NULL;

        if  (block->bbFlags & (BBF_JMP_TARGET|BBF_HAS_LABEL))
        {
            /* Mark a label and update the current set of live GC refs */

            block->bbEmitCookie = getEmitter()->emitAddLabel(gcInfo.gcVarPtrSetCur,
                                                             gcInfo.gcRegGCrefSetCur,
                                                             gcInfo.gcRegByrefSetCur,
                                                             FALSE);
        }

        if (block == compiler->fgFirstColdBlock)
        {
#ifdef DEBUG
            if (compiler->verbose)
            {
                printf("\nThis is the start of the cold region of the method\n");
            }
#endif
            // We should never have a block that falls through into the Cold section
            noway_assert(!lblk->bbFallsThrough());

            // We require the block that starts the Cold section to have a label
            noway_assert(block->bbEmitCookie);
            getEmitter()->emitSetFirstColdIGCookie(block->bbEmitCookie);
        }

        /* Both stacks are always empty on entry to a basic block */

        genStackLevel = 0;

#if 0//!FEATURE_FIXED_OUT_ARGS
        /* Check for inserted throw blocks and adjust genStackLevel */

        if  (!isFramePointerUsed() && compiler->fgIsThrowHlpBlk(block))
        {
            noway_assert(block->bbFlags & BBF_JMP_TARGET);

            genStackLevel = compiler->fgThrowHlpBlkStkLevel(block) * sizeof(int);

            if  (genStackLevel)
            {
                NYI("Need emitMarkStackLvl()");
            }
        }
#endif // !FEATURE_FIXED_OUT_ARGS

        savedStkLvl = genStackLevel;

        /* Tell everyone which basic block we're working on */

        compiler->compCurBB = block;

#ifdef DEBUGGING_SUPPORT
        siBeginBlock(block);

        // BBF_INTERNAL blocks don't correspond to any single IL instruction.
        if (compiler->opts.compDbgInfo &&
            (block->bbFlags & BBF_INTERNAL) &&
            !compiler->fgBBisScratch(block))    // If the block is the distinguished first scratch block, then no need to emit a NO_MAPPING entry, immediately after the prolog.
        {
            genIPmappingAdd((IL_OFFSETX) ICorDebugInfo::NO_MAPPING, true);
        }

        bool    firstMapping = true;
#endif // DEBUGGING_SUPPORT

        /*---------------------------------------------------------------------
         *
         *  Generate code for each statement-tree in the block
         *
         */

#if FEATURE_EH_FUNCLETS
        if (block->bbFlags & BBF_FUNCLET_BEG)
        {
            genReserveFuncletProlog(block);
        }
#endif // FEATURE_EH_FUNCLETS

#if JIT_FEATURE_SSA_SKIP_DEFS
        for (GenTreePtr stmt = block->FirstNonPhiDef(); stmt; stmt = stmt->gtNext)
#else
        for (GenTreePtr stmt = block->bbTreeList; stmt; stmt = stmt->gtNext)
#endif
        {
            noway_assert(stmt->gtOper == GT_STMT);

            if (stmt->AsStmt()->gtStmtIsEmbedded())
                continue;

            /* Get hold of the statement tree */
            GenTreePtr  tree = stmt->gtStmt.gtStmtExpr;

#if defined(DEBUGGING_SUPPORT)

            /* Do we have a new IL-offset ? */

            if (stmt->gtStmt.gtStmtILoffsx != BAD_IL_OFFSET)
            {
                /* Create and append a new IP-mapping entry */
                genIPmappingAdd(stmt->gtStmt.gtStmt.gtStmtILoffsx, firstMapping);
                firstMapping = false;
            }

#endif // DEBUGGING_SUPPORT

#ifdef DEBUG
            noway_assert(stmt->gtStmt.gtStmtLastILoffs <= compiler->info.compILCodeSize ||
                         stmt->gtStmt.gtStmtLastILoffs == BAD_IL_OFFSET);

            if (compiler->opts.dspCode && compiler->opts.dspInstrs &&
                stmt->gtStmt.gtStmtLastILoffs != BAD_IL_OFFSET)
            {
                while (genCurDispOffset <= stmt->gtStmt.gtStmtLastILoffs)
                {
                    genCurDispOffset +=
                        dumpSingleInstr(compiler->info.compCode, genCurDispOffset, ">    ");
                }
            }

            stmtNum++;
            if (compiler->verbose)
            {
                printf("\nGenerating BB%02u, stmt %u\t\t", block->bbNum, stmtNum);
                printf("Holding variables: ");
                dspRegMask(regSet.rsMaskVars); printf("\n\n");
                compiler->gtDispTree(compiler->opts.compDbgInfo ? stmt : tree);
                if (compiler->verboseTrees)
                {
                    compiler->gtDispTree(compiler->opts.compDbgInfo ? stmt : tree);
                    printf("\n");
                }
            }
            totalCostEx += (stmt->gtCostEx * block->getBBWeight(compiler));
            totalCostSz +=  stmt->gtCostSz;
#endif // DEBUG

            // Traverse the tree in linear order, generating code for each node in the
            // tree as we encounter it

            compiler->compCurLifeTree = NULL;
            compiler->compCurStmt = stmt;
            for (GenTreePtr treeNode = stmt->gtStmt.gtStmtList;
                 treeNode != NULL;
                 treeNode = treeNode->gtNext)
            {
                genCodeForTreeNode(treeNode);
                if (treeNode->gtHasReg() && treeNode->gtLsraInfo.isLocalDefUse)
                {
                    genConsumeReg(treeNode);
                }
            }

            regSet.rsSpillChk();

#ifdef DEBUG
            /* Make sure we didn't bungle pointer register tracking */

            regMaskTP ptrRegs       = (gcInfo.gcRegGCrefSetCur|gcInfo.gcRegByrefSetCur);
            regMaskTP nonVarPtrRegs = ptrRegs & ~regSet.rsMaskVars;

            // If return is a GC-type, clear it.  Note that if a common
            // epilog is generated (genReturnBB) it has a void return
            // even though we might return a ref.  We can't use the compRetType
            // as the determiner because something we are tracking as a byref
            // might be used as a return value of a int function (which is legal)
            if  (tree->gtOper == GT_RETURN &&
                (varTypeIsGC(compiler->info.compRetType) ||
                    (tree->gtOp.gtOp1 != 0 && varTypeIsGC(tree->gtOp.gtOp1->TypeGet()))))
            {
                nonVarPtrRegs &= ~RBM_INTRET;
            }

            // When profiling, the first statement in a catch block will be the
            // harmless "inc" instruction (does not interfere with the exception
            // object).

            if ((compiler->opts.eeFlags & CORJIT_FLG_BBINSTR) &&
                (stmt == block->bbTreeList) &&
                handlerGetsXcptnObj(block->bbCatchTyp))
            {
                nonVarPtrRegs &= ~RBM_EXCEPTION_OBJECT;
            }

            if  (nonVarPtrRegs)
            {
                printf("Regset after tree=");
                Compiler::printTreeID(tree);
                printf(" BB%02u gcr=", block->bbNum);
                printRegMaskInt(gcInfo.gcRegGCrefSetCur & ~regSet.rsMaskVars);
                compiler->getEmitter()->emitDispRegSet(gcInfo.gcRegGCrefSetCur & ~regSet.rsMaskVars);
                printf(", byr=");
                printRegMaskInt(gcInfo.gcRegByrefSetCur & ~regSet.rsMaskVars);
                compiler->getEmitter()->emitDispRegSet(gcInfo.gcRegByrefSetCur & ~regSet.rsMaskVars);
                printf(", regVars=");
                printRegMaskInt(regSet.rsMaskVars);
                compiler->getEmitter()->emitDispRegSet(regSet.rsMaskVars);
                printf("\n");
            }

            noway_assert(nonVarPtrRegs == 0);

            for (GenTree * node = stmt->gtStmt.gtStmtList; node; node=node->gtNext)
            {
                assert(!(node->gtFlags & GTF_SPILL));
            }

#endif // DEBUG

            noway_assert(stmt->gtOper == GT_STMT);

#ifdef DEBUGGING_SUPPORT
            genEnsureCodeEmitted(stmt->gtStmt.gtStmtILoffsx);
#endif

        } //-------- END-FOR each statement-tree of the current block ---------

#ifdef  DEBUGGING_SUPPORT

        if (compiler->opts.compScopeInfo && (compiler->info.compVarScopesCount > 0))
        {
            siEndBlock(block);

            /* Is this the last block, and are there any open scopes left ? */

            bool isLastBlockProcessed = (block->bbNext == NULL);
            if (block->isBBCallAlwaysPair())
            {
                isLastBlockProcessed = (block->bbNext->bbNext == NULL);
            }

            if (isLastBlockProcessed && siOpenScopeList.scNext)
            {
                /* This assert no longer holds, because we may insert a throw
                   block to demarcate the end of a try or finally region when they
                   are at the end of the method.  It would be nice if we could fix
                   our code so that this throw block will no longer be necessary. */

                //noway_assert(block->bbCodeOffsEnd != compiler->info.compILCodeSize);

                siCloseAllOpenScopes();
            }
        }

#endif // DEBUGGING_SUPPORT

        genStackLevel -= savedStkLvl;

#ifdef DEBUG
        // compCurLife should be equal to the liveOut set, except that we don't keep
        // it up to date for vars that are not register candidates
        // (it would be nice to have a xor set function)

        VARSET_TP VARSET_INIT_NOCOPY(extraLiveVars, VarSetOps::Diff(compiler, block->bbLiveOut, compiler->compCurLife));
        VarSetOps::UnionD(compiler, extraLiveVars, VarSetOps::Diff(compiler, compiler->compCurLife, block->bbLiveOut));
        VARSET_ITER_INIT(compiler, extraLiveVarIter, extraLiveVars, extraLiveVarIndex);
        while (extraLiveVarIter.NextElem(compiler, &extraLiveVarIndex))
        {
            unsigned varNum = compiler->lvaTrackedToVarNum[extraLiveVarIndex];
            LclVarDsc * varDsc = compiler->lvaTable + varNum;
            assert(!varDsc->lvIsRegCandidate());
        }
#endif

        /* Both stacks should always be empty on exit from a basic block */

        noway_assert(genStackLevel == 0);

#ifdef _TARGET_AMD64_
        // On AMD64, we need to generate a NOP after a call that is the last instruction of the block, in several
        // situations, to support proper exception handling semantics. This is mostly to ensure that when the stack
        // walker computes an instruction pointer for a frame, that instruction pointer is in the correct EH region.
        // The document "X64 and ARM ABIs.docx" has more details. The situations:
        // 1. If the call instruction is in a different EH region as the instruction that follows it.
        // 2. If the call immediately precedes an OS epilog. (Note that what the JIT or VM consider an epilog might
        //    be slightly different from what the OS considers an epilog, and it is the OS-reported epilog that matters here.)
        // We handle case #1 here, and case #2 in the emitter.
        if (getEmitter()->emitIsLastInsCall())
        {
            // Ok, the last instruction generated is a call instruction. Do any of the other conditions hold?
            // Note: we may be generating a few too many NOPs for the case of call preceding an epilog. Technically,
            // if the next block is a BBJ_RETURN, an epilog will be generated, but there may be some instructions
            // generated before the OS epilog starts, such as a GS cookie check.
            if ((block->bbNext == nullptr) ||
                !BasicBlock::sameEHRegion(block, block->bbNext))
            {
                // We only need the NOP if we're not going to generate any more code as part of the block end.

                switch (block->bbJumpKind)
                {
                case BBJ_ALWAYS:
                case BBJ_THROW:
                case BBJ_CALLFINALLY:
                case BBJ_EHCATCHRET:
                    // We're going to generate more code below anyway, so no need for the NOP.

                case BBJ_RETURN:
                case BBJ_EHFINALLYRET:
                case BBJ_EHFILTERRET:
                    // These are the "epilog follows" case, handled in the emitter.

                    break;

                case BBJ_NONE:
                    if (block->bbNext == nullptr)
                    {
                        // Call immediately before the end of the code; we should never get here    .
                        instGen(INS_BREAKPOINT); // This should never get executed
                    }
                    else
                    {
                        // We need the NOP
                        instGen(INS_nop);
                    }
                    break;

                case BBJ_COND:
                case BBJ_SWITCH:
                    // These can't have a call as the last instruction!

                default:
                    noway_assert(!"Unexpected bbJumpKind");
                    break;
                }
            }
        }
#endif //_TARGET_AMD64_

        /* Do we need to generate a jump or return? */

        switch (block->bbJumpKind)
        {
        case BBJ_ALWAYS:
            inst_JMP(EJ_jmp, block->bbJumpDest);
            break;

        case BBJ_RETURN:
            genExitCode(block);
            break;

        case BBJ_THROW:
            // If we have a throw at the end of a function or funclet, we need to emit another instruction
            // afterwards to help the OS unwinder determine the correct context during unwind.
            // We insert an unexecuted breakpoint instruction in several situations
            // following a throw instruction:
            // 1. If the throw is the last instruction of the function or funclet. This helps
            //    the OS unwinder determine the correct context during an unwind from the
            //    thrown exception.
            // 2. If this is this is the last block of the hot section.
            // 3. If the subsequent block is a special throw block.
            // 4. On AMD64, if the next block is in a different EH region.
            if ((block->bbNext == NULL)
#if FEATURE_EH_FUNCLETS
                || (block->bbNext->bbFlags & BBF_FUNCLET_BEG)
#endif // FEATURE_EH_FUNCLETS
#ifdef _TARGET_AMD64_
                || !BasicBlock::sameEHRegion(block, block->bbNext)
#endif // _TARGET_AMD64_
                || (!isFramePointerUsed() && compiler->fgIsThrowHlpBlk(block->bbNext))
                || block->bbNext == compiler->fgFirstColdBlock
                )
            {
                instGen(INS_BREAKPOINT); // This should never get executed
            }

            break;

        case BBJ_CALLFINALLY:

            // Now set REG_LR to the address of where the finally funclet should
            // return to directly.

            BasicBlock * bbFinallyRet; bbFinallyRet = NULL;

            // We don't have retless calls, since we use the BBJ_ALWAYS to point at a NOP pad where
            // we would have otherwise created retless calls.
            assert(block->isBBCallAlwaysPair());

            assert(block->bbNext                     != NULL);
            assert(block->bbNext->bbJumpKind         == BBJ_ALWAYS);
            assert(block->bbNext->bbJumpDest         != NULL);
            assert(block->bbNext->bbJumpDest->bbFlags & BBF_FINALLY_TARGET);

            bbFinallyRet = block->bbNext->bbJumpDest;
            bbFinallyRet->bbFlags |= BBF_JMP_TARGET;

#if 0
            // TODO-ARM-CQ:
            // We don't know the address of finally funclet yet.  But adr requires the offset
            // to finally funclet from current IP is within 4095 bytes. So this code is disabled
            // for now.
            getEmitter()->emitIns_J_R (INS_adr,
                                     EA_4BYTE,
                                     bbFinallyRet,
                                     REG_LR);
#else // !0
            // Load the address where the finally funclet should return into LR.
            // The funclet prolog/epilog will do "push {lr}" / "pop {pc}" to do
            // the return.
            getEmitter()->emitIns_R_L (INS_movw,
                                     EA_4BYTE_DSP_RELOC,
                                     bbFinallyRet,
                                     REG_LR);
            getEmitter()->emitIns_R_L (INS_movt,
                                     EA_4BYTE_DSP_RELOC,
                                     bbFinallyRet,
                                     REG_LR);
#endif // !0

            // Jump to the finally BB
            inst_JMP(EJ_jmp, block->bbJumpDest);

            // The BBJ_ALWAYS is used because the BBJ_CALLFINALLY can't point to the
            // jump target using bbJumpDest - that is already used to point
            // to the finally block. So just skip past the BBJ_ALWAYS unless the
            // block is RETLESS.
            if ( !(block->bbFlags & BBF_RETLESS_CALL) )
            {
                assert(block->isBBCallAlwaysPair());

                lblk = block;
                block = block->bbNext;
            }
            break;

#ifdef _TARGET_ARM_

        case BBJ_EHCATCHRET:
            // set r0 to the address the VM should return to after the catch
            getEmitter()->emitIns_R_L (INS_movw,
                                     EA_4BYTE_DSP_RELOC,
                                     block->bbJumpDest,
                                     REG_R0);
            getEmitter()->emitIns_R_L (INS_movt,
                                     EA_4BYTE_DSP_RELOC,
                                     block->bbJumpDest,
                                     REG_R0);

            __fallthrough;

        case BBJ_EHFINALLYRET:
        case BBJ_EHFILTERRET:
            genReserveFuncletEpilog(block);
            break;

#elif defined(_TARGET_AMD64_)

        case BBJ_EHCATCHRET:
            // Set EAX to the address the VM should return to after the catch.
            // Generate a RIP-relative
            //         lea reg, [rip + disp32] ; the RIP is implicit
            // which will be position-indepenent.
            // TODO-ARM-Bug?: For ngen, we need to generate a reloc for the displacement (maybe EA_PTR_DSP_RELOC).
            getEmitter()->emitIns_R_L(INS_lea, EA_PTRSIZE, block->bbJumpDest, REG_INTRET);
            __fallthrough;

        case BBJ_EHFINALLYRET:
        case BBJ_EHFILTERRET:
            genReserveFuncletEpilog(block);
            break;

#endif // _TARGET_AMD64_

        case BBJ_NONE:
        case BBJ_COND:
        case BBJ_SWITCH:
            break;

        default:
            noway_assert(!"Unexpected bbJumpKind");
            break;
        }

#ifdef  DEBUG
        compiler->compCurBB = 0;
#endif

    } //------------------ END-FOR each block of the method -------------------

    /* Nothing is live at this point */
    genUpdateLife(VarSetOps::MakeEmpty(compiler));

    /* Finalize the spill  tracking logic */

    regSet.rsSpillEnd();

    /* Finalize the temp   tracking logic */

    compiler->tmpEnd();

#ifdef  DEBUG
    if (compiler->verbose)
    {
        printf("\n# ");
        printf("totalCostEx = %6d, totalCostSz = %5d ",
               totalCostEx, totalCostSz);
        printf("%s\n", compiler->info.compFullName);
    }
#endif
}

// return the child that has the same reg as the dst (if any)
// other child returned (out param) in 'other'
GenTree *
sameRegAsDst(GenTree *tree, GenTree *&other /*out*/)
{
    if (tree->gtRegNum == REG_NA)
    {
        other = nullptr;
        return NULL;
    }

    GenTreePtr op1 = tree->gtOp.gtOp1;
    GenTreePtr op2 = tree->gtOp.gtOp2;
    if (op1->gtRegNum == tree->gtRegNum)
    {
        other = op2;
        return op1;
    }
    if (op2->gtRegNum == tree->gtRegNum)
    {
        other = op1;
        return op2;
    }
    else
    {
        other = nullptr;
        return NULL;
    }
}

//  move an immediate value into an integer register

void                CodeGen::instGen_Set_Reg_To_Imm(emitAttr    size,
                                                    regNumber   reg,
                                                    ssize_t     imm,
                                                    insFlags    flags)
{
    // reg cannot be a FP register
    assert(!genIsValidFloatReg(reg));

    if (!compiler->opts.compReloc)
    {
        size = EA_SIZE(size);  // Strip any Reloc flags from size if we aren't doing relocs
    }

    if ((imm == 0) && !EA_IS_RELOC(size))
    {
        instGen_Set_Reg_To_Zero(size, reg, flags);
    }
    else
    {
#ifdef _TARGET_AMD64_
        if (AddrShouldUsePCRel(imm))
        {
            getEmitter()->emitIns_R_AI(INS_lea, EA_PTR_DSP_RELOC, reg, imm);
        }
        else
#endif // _TARGET_AMD64_
        {
            if ((imm & 0x0000ffff) == imm)
            {
                getEmitter()->emitIns_R_I(INS_mov, size, reg, imm);
            }
            else if ((flags != INS_FLAGS_SET) && ((imm & 0xffffffff) == imm))
            {
                getEmitter()->emitIns_R_I(INS_movw, size, reg, imm & 0x0000ffff);
                getEmitter()->emitIns_R_I(INS_movt, size, reg, (imm >> 16) & 0x0000ffff);
            }
            else
            {
                NYI_ARM("instGen_Set_Reg_To_Imm flags == INS_FLAGS_SET, imm > 0xffff");
            }
        }
    }
    regTracker.rsTrackRegIntCns(reg, imm);
}

/*****************************************************************************
 *
 * Generate code to set a register 'targetReg' of type 'targetType' to the constant
 * specified by the constant (GT_CNS_INT or GT_CNS_DBL) in 'tree'. This does not call
 * genProduceReg() on the target register.
 */
void                CodeGen::genSetRegToConst(regNumber targetReg, var_types targetType, GenTreePtr tree)
{
    switch (tree->gtOper)
    {
    case GT_CNS_INT:
        {
            // relocatable values tend to come down as a CNS_INT of native int type
            // so the line between these two opcodes is kind of blurry
            GenTreeIntConCommon* con = tree->AsIntConCommon();
            ssize_t cnsVal = con->IconValue();

            bool needReloc = compiler->opts.compReloc && tree->IsIconHandle();
            if (needReloc)
            {
                instGen_Set_Reg_To_Imm(EA_HANDLE_CNS_RELOC, targetReg, cnsVal);
                regTracker.rsTrackRegTrash(targetReg);
            }
            else
            {
                genSetRegToIcon(targetReg, cnsVal, targetType);
            }
        }
        break;

    case GT_CNS_DBL:
        {
            NYI("GT_CNS_DBL");
        }
        break;

    default:
        unreached();
    }
}

// Generate code for ADD, SUB, AND, OR and XOR
void CodeGen::genCodeForBinary(GenTree* treeNode)
{
    const genTreeOps oper = treeNode->OperGet();
    regNumber targetReg  = treeNode->gtRegNum;
    var_types targetType = treeNode->TypeGet();
    emitter *emit = getEmitter();

    assert (oper == GT_ADD  ||
            oper == GT_SUB  ||
            oper == GT_MUL  ||
            oper == GT_AND  ||
            oper == GT_OR   || 
            oper == GT_XOR);
        
    GenTreePtr op1 = treeNode->gtGetOp1();
    GenTreePtr op2 = treeNode->gtGetOp2();
    instruction ins = genGetInsForOper(treeNode->OperGet(), targetType);

    // The arithmetic node must be sitting in a register (since it's not contained)
    noway_assert(targetReg != REG_NA);

    genConsumeOperands(treeNode->AsOp());

    regNumber r = emit->emitInsTernary(ins, emitTypeSize(treeNode), treeNode, op1, op2);
    noway_assert(r == targetReg);

    genProduceReg(treeNode);
}

/*****************************************************************************
 *
 * Generate code for a single node in the tree.
 * Preconditions: All operands have been evaluated
 *
 */
void
CodeGen::genCodeForTreeNode(GenTreePtr treeNode)
{
    regNumber targetReg  = treeNode->gtRegNum;
    var_types targetType = treeNode->TypeGet();
    emitter *emit = getEmitter();

#ifdef  DEBUG
    if (compiler->verbose)
    {
        unsigned seqNum = treeNode->gtSeqNum;   // Useful for setting a conditional break in Visual Studio
        printf("Generating: ");
        compiler->gtDispTree(treeNode, nullptr, nullptr, true);
    }
#endif // DEBUG

    // Is this a node whose value is already in a register?  LSRA denotes this by
    // setting the GTF_REUSE_REG_VAL flag.
    if (treeNode->IsReuseRegVal())
    {
        // For now, this is only used for constant nodes.
        assert((treeNode->OperGet() == GT_CNS_INT) || (treeNode->OperGet() == GT_CNS_DBL));
        JITDUMP("  TreeNode is marked ReuseReg\n");
        return;
    }

    // contained nodes are part of their parents for codegen purposes
    // ex : immediates, most LEAs
    if (treeNode->isContained())
    {
        return;
    }

    switch (treeNode->gtOper)
    {
    case GT_CNS_INT:
    case GT_CNS_DBL:
        genSetRegToConst(targetReg, targetType, treeNode);
        genProduceReg(treeNode);
        break;

    case GT_NEG:
    case GT_NOT:
        {
            NYI("GT_NEG and GT_NOT");
        }
        genProduceReg(treeNode);
        break;

    case GT_OR:
    case GT_XOR:
    case GT_AND:
        assert(varTypeIsIntegralOrI(treeNode));
        __fallthrough;

    case GT_ADD:
    case GT_SUB:
    case GT_MUL:
        genCodeForBinary(treeNode);
        break;
#if 0
        {
            const genTreeOps oper = treeNode->OperGet();
            if ((oper == GT_ADD || oper == GT_SUB) &&
                treeNode->gtOverflow())
            {
                // This is also checked in the importer.
                NYI("Overflow not yet implemented");
            }

            GenTreePtr op1 = treeNode->gtGetOp1();
            GenTreePtr op2 = treeNode->gtGetOp2();
            instruction ins = genGetInsForOper(treeNode->OperGet(), targetType);

            // The arithmetic node must be sitting in a register (since it's not contained)
            noway_assert(targetReg != REG_NA);

            regNumber op1reg = op1->gtRegNum;
            regNumber op2reg = op2->gtRegNum;

            GenTreePtr dst;
            GenTreePtr src;

            genConsumeIfReg(op1);
            genConsumeIfReg(op2);

            // This is the case of reg1 = reg1 op reg2
            // We're ready to emit the instruction without any moves
            if (op1reg == targetReg)
            {
                dst = op1;
                src = op2;
            }
            // We have reg1 = reg2 op reg1
            // In order for this operation to be correct
            // we need that op is a commutative operation so
            // we can convert it into reg1 = reg1 op reg2 and emit
            // the same code as above
            else if (op2reg == targetReg)
            {
                noway_assert(GenTree::OperIsCommutative(treeNode->OperGet()));
                dst = op2;
                src = op1;
            }
            // dest, op1 and op2 registers are different:
            // reg3 = reg1 op reg2
            // We can implement this by issuing a mov:
            // reg3 = reg1
            // reg3 = reg3 op reg2
            else
            {
                inst_RV_RV(ins_Move_Extend(targetType, true), targetReg, op1reg, op1->gtType);
                regTracker.rsTrackRegCopy(targetReg, op1reg);
                gcInfo.gcMarkRegPtrVal(targetReg, targetType);
                dst = treeNode;
                src = op2;
            }

            regNumber r = emit->emitInsBinary(ins, emitTypeSize(treeNode), dst, src);
            noway_assert(r == targetReg);
        }
        genProduceReg(treeNode);
        break;
#endif

    case GT_LSH:
    case GT_RSH:
    case GT_RSZ:
        genCodeForShift(treeNode->gtGetOp1(), treeNode->gtGetOp2(), treeNode);
        // genCodeForShift() calls genProduceReg()
        break;

    case GT_CAST:
        if (varTypeIsFloating(targetType) && varTypeIsFloating(treeNode->gtOp.gtOp1))
        {
            // Casts float/double <--> double/float
            genFloatToFloatCast(treeNode);
        }
        else if (varTypeIsFloating(treeNode->gtOp.gtOp1))
        {
            // Casts float/double --> int32/int64
            genFloatToIntCast(treeNode);
        }
        else if (varTypeIsFloating(targetType))
        {
            // Casts int32/uint32/int64/uint64 --> float/double
            genIntToFloatCast(treeNode);
        }
        else
        {
            // Casts int <--> int
            genIntToIntCast(treeNode);
        }
        // The per-case functions call genProduceReg()
        break;

    case GT_LCL_VAR:
        {
            GenTreeLclVarCommon *lcl = treeNode->AsLclVarCommon();
            // lcl_vars are not defs
            assert((treeNode->gtFlags & GTF_VAR_DEF) == 0);

            bool isRegCandidate = compiler->lvaTable[lcl->gtLclNum].lvIsRegCandidate();

            if (isRegCandidate && !(treeNode->gtFlags & GTF_VAR_DEATH))
            {
                assert((treeNode->InReg()) || (treeNode->gtFlags & GTF_SPILLED));
            }

            // If this is a register candidate that has been spilled, genConsumeReg() will
            // reload it at the point of use.  Otherwise, if it's not in a register, we load it here.

            if (!treeNode->InReg() && !(treeNode->gtFlags & GTF_SPILLED))
            {
                assert(!isRegCandidate);
                emit->emitIns_R_S(ins_Load(treeNode->TypeGet()), emitTypeSize(treeNode), treeNode->gtRegNum, lcl->gtLclNum, 0);
                genProduceReg(treeNode);
            }
        }
        break;

    case GT_LCL_FLD_ADDR:
    case GT_LCL_VAR_ADDR:
        {
            // Address of a local var.  This by itself should never be allocated a register.
            // If it is worth storing the address in a register then it should be cse'ed into
            // a temp and that would be allocated a register.
            noway_assert(targetType == TYP_BYREF);
            noway_assert(!treeNode->InReg());

            inst_RV_TT(INS_lea, targetReg, treeNode, 0, EA_BYREF);
        }
        genProduceReg(treeNode);
        break;

    case GT_LCL_FLD:
        {
            NYI_IF(targetType == TYP_STRUCT, "GT_LCL_FLD: struct load local field not supported");
            NYI_IF(treeNode->gtRegNum == REG_NA, "GT_LCL_FLD: load local field not into a register is not supported");

            emitAttr size = emitTypeSize(targetType);
            unsigned offs = treeNode->gtLclFld.gtLclOffs;
            unsigned varNum = treeNode->gtLclVarCommon.gtLclNum;
            assert(varNum < compiler->lvaCount);

            emit->emitIns_R_S(ins_Move_Extend(targetType, treeNode->InReg()), size, targetReg, varNum, offs);
        }
        genProduceReg(treeNode);
        break;

    case GT_STORE_LCL_FLD:
        {
            NYI_IF(varTypeIsFloating(targetType), "Code generation for FP field assignment");

            noway_assert(targetType != TYP_STRUCT);
            noway_assert(!treeNode->InReg());

            unsigned offs = treeNode->gtLclFld.gtLclOffs;
            unsigned varNum = treeNode->gtLclVarCommon.gtLclNum;
            assert(varNum < compiler->lvaCount);

            GenTreePtr op1 = treeNode->gtOp.gtOp1;
            genConsumeRegs(op1);

            emit->emitIns_R_S(ins_Store(targetType), emitTypeSize(targetType), op1->gtRegNum, varNum, offs);
        }
        break;

    case GT_STORE_LCL_VAR:
        {
            NYI_IF(targetType == TYP_STRUCT, "struct store local not supported");

            GenTreePtr op1 = treeNode->gtOp.gtOp1->gtEffectiveVal();
            genConsumeIfReg(op1);
            if (treeNode->gtRegNum == REG_NA)
            {
                // stack store
                emit->emitInsMov(ins_Store(targetType), emitTypeSize(treeNode), treeNode);
                compiler->lvaTable[treeNode->AsLclVarCommon()->gtLclNum].lvRegNum = REG_STK;
            }
            else if (op1->isContained())
            {
                // Currently, we assume that the contained source of a GT_STORE_LCL_VAR writing to a register
                // must be a constant. However, in the future we might want to support a contained memory op.
                // This is a bit tricky because we have to decide it's contained before register allocation,
                // and this would be a case where, once that's done, we need to mark that node as always
                // requiring a register - which we always assume now anyway, but once we "optimize" that
                // we'll have to take cases like this into account.
                assert((op1->gtRegNum == REG_NA) && op1->OperIsConst());
                genSetRegToConst(treeNode->gtRegNum, targetType, op1);
            }
            else if (op1->gtRegNum != treeNode->gtRegNum)
            {
                assert(op1->gtRegNum != REG_NA);
                emit->emitInsBinary(ins_Move_Extend(targetType, true), emitTypeSize(treeNode), treeNode, op1);
            }
            if (treeNode->gtRegNum != REG_NA)
                genProduceReg(treeNode);
        }
        break;

    case GT_RETFILT:
        // A void GT_RETFILT is the end of a finally. For non-void filter returns we need to load the result in
        // the return register, if it's not already there. The processing is the same as GT_RETURN.
        if (targetType != TYP_VOID)
        {
            // For filters, the IL spec says the result is type int32. Further, the only specified legal values
            // are 0 or 1, with the use of other values "undefined".
            assert(targetType == TYP_INT);
        }

        __fallthrough;

    case GT_RETURN:
        {
            GenTreePtr op1 = treeNode->gtOp.gtOp1;
            if (targetType == TYP_VOID)
            {
                assert(op1 == nullptr);
                break;
            }
            assert(op1 != nullptr);
            op1 = op1->gtEffectiveVal();

            NYI_IF(op1->gtRegNum == REG_NA, "GT_RETURN: return of a value not in register");
            genConsumeReg(op1);

            regNumber retReg = varTypeIsFloating(op1) ? REG_FLOATRET : REG_INTRET;
            if (op1->gtRegNum != retReg)
            {
                inst_RV_RV(ins_Move_Extend(targetType, true), retReg, op1->gtRegNum, targetType);
            }
        }
        break;

    case GT_LEA:
        {
            // if we are here, it is the case where there is an LEA that cannot
            // be folded into a parent instruction
            GenTreeAddrMode *lea = treeNode->AsAddrMode();
            genLeaInstruction(lea);
        }
        // genLeaInstruction calls genProduceReg()
        break;

    case GT_IND:
        genConsumeAddress(treeNode->AsIndir()->Addr());
        emit->emitInsMov(ins_Load(targetType), emitTypeSize(treeNode), treeNode);
        genProduceReg(treeNode);
        break;

    case GT_MOD:
    case GT_UDIV:
    case GT_UMOD:
        // We shouldn't be seeing GT_MOD on float/double args as it should get morphed into a
        // helper call by front-end.  Similarly we shouldn't be seeing GT_UDIV and GT_UMOD
        // on float/double args.
        noway_assert(!varTypeIsFloating(treeNode));
        __fallthrough;

    case GT_DIV:
        noway_assert(!"Codegen for GT_DIV/GT_MOD/GT_UDIV/GT_UMOD");
        break;

    case GT_MATH:
        {
            NYI("GT_MATH");
        }
        genProduceReg(treeNode);
        break;

    case GT_EQ:
    case GT_NE:
    case GT_LT:
    case GT_LE:
    case GT_GE:
    case GT_GT:
        {
            // TODO-ARM-CQ: Check if we can use the currently set flags.
            // TODO-ARM-CQ: Check for the case where we can simply transfer the carry bit to a register
            //         (signed < or >= where targetReg != REG_NA)

            GenTreeOp *tree = treeNode->AsOp();
            GenTreePtr op1 = tree->gtOp1;
            GenTreePtr op2 = tree->gtOp2;

            genConsumeOperands(tree);

            instruction ins = INS_cmp;
            emitAttr cmpAttr;
            if (varTypeIsFloating(op1))
            {
                NYI("Floating point compare");

                bool isUnordered = ((treeNode->gtFlags & GTF_RELOP_NAN_UN) != 0);
                switch (tree->OperGet())
                {
                case GT_EQ:
                    ins = INS_beq;
                case GT_NE:
                    ins =  INS_bne;
                case GT_LT:
                    ins =  isUnordered ? INS_blt : INS_blo;
                case GT_LE:
                    ins =  isUnordered ? INS_ble : INS_bls;
                case GT_GE:
                    ins =  isUnordered ? INS_bpl : INS_bge;
                case GT_GT:
                    ins =  isUnordered ? INS_bhi : INS_bgt;
                default:
                    unreached();
                }
            }
            else
            {
                var_types op1Type = op1->TypeGet();
                var_types op2Type = op2->TypeGet();
                assert(!varTypeIsFloating(op2Type));
                ins = INS_cmp;
                if (op1Type == op2Type)
                {
                    cmpAttr = emitTypeSize(op1Type);
                }
                else
                {
                    var_types cmpType = TYP_INT;
                    bool op1Is64Bit = (varTypeIsLong(op1Type) || op1Type == TYP_REF);
                    bool op2Is64Bit = (varTypeIsLong(op2Type) || op2Type == TYP_REF);
                    NYI_IF(op1Is64Bit || op2Is64Bit, "Long compare");
                    assert(!op1->isContainedMemoryOp() || op1Type == op2Type);
                    assert(!op2->isContainedMemoryOp() || op1Type == op2Type);
                    cmpAttr = emitTypeSize(cmpType);
                }
            }
            emit->emitInsBinary(ins, cmpAttr, op1, op2);

            // Are we evaluating this into a register?
            if (targetReg != REG_NA)
            {
                genSetRegToCond(targetReg, tree);
                genProduceReg(tree);
            }
        }
        break;

    case GT_JTRUE:
        {
            GenTree *cmp = treeNode->gtOp.gtOp1->gtEffectiveVal();
            assert(cmp->OperIsCompare());
            assert(compiler->compCurBB->bbJumpKind == BBJ_COND);

            // Get the "kind" and type of the comparison.  Note that whether it is an unsigned cmp
            // is governed by a flag NOT by the inherent type of the node
            // TODO-ARM-CQ: Check if we can use the currently set flags.

            emitJumpKind jmpKind   = genJumpKindForOper(cmp->gtOper, (cmp->gtFlags & GTF_UNSIGNED) != 0);
            BasicBlock * jmpTarget = compiler->compCurBB->bbJumpDest;

            inst_JMP(jmpKind, jmpTarget);
        }
        break;

    case GT_RETURNTRAP:
        {
            // this is nothing but a conditional call to CORINFO_HELP_STOP_FOR_GC
            // based on the contents of 'data'

            GenTree *data = treeNode->gtOp.gtOp1->gtEffectiveVal();
            genConsumeIfReg(data);
            GenTreeIntCon cns = intForm(TYP_INT, 0);
            emit->emitInsBinary(INS_cmp, emitTypeSize(TYP_INT), data, &cns);

            BasicBlock* skipLabel = genCreateTempLabel();

            inst_JMP(genJumpKindForOper(GT_EQ, true), skipLabel);
            // emit the call to the EE-helper that stops for GC (or other reasons)

            genEmitHelperCall(CORINFO_HELP_STOP_FOR_GC, 0, EA_UNKNOWN);
            genDefineTempLabel(skipLabel);
        }
        break;

    case GT_STOREIND:
        {
            GenTree* data = treeNode->gtOp.gtOp2;
            GenTree* addr = treeNode->gtOp.gtOp1;
            GCInfo::WriteBarrierForm writeBarrierForm = gcInfo.gcIsWriteBarrierCandidate(treeNode, data);
            if (writeBarrierForm != GCInfo::WBF_NoBarrier)
            {
                // data and addr must be in registers.
                // Consume both registers so that any copies of interfering
                // registers are taken care of.
                genConsumeOperands(treeNode->AsOp());
                
                // At this point, we should not have any interference.
                // That is, 'data' must not be in REG_ARG_0,
                //  as that is where 'addr' must go.
                noway_assert(data->gtRegNum != REG_ARG_0);

                // addr goes in REG_ARG_0
                if (addr->gtRegNum != REG_ARG_0)
                {
                    inst_RV_RV(INS_mov, REG_ARG_0, addr->gtRegNum, addr->TypeGet());
                }

                // data goes in REG_ARG_1
                if (data->gtRegNum != REG_ARG_1)
                {
                    inst_RV_RV(INS_mov, REG_ARG_1, data->gtRegNum, data->TypeGet());
                }

                genGCWriteBarrier(treeNode, writeBarrierForm);
            }
            else
            {
                bool reverseOps = ((treeNode->gtFlags & GTF_REVERSE_OPS) != 0);
                bool dataIsUnary = false;
                GenTree* nonRMWsrc = nullptr;
                // We must consume the operands in the proper execution order, 
                // so that liveness is updated appropriately.
                if (!reverseOps)
                {
                    genConsumeAddress(addr);
                }
                if (data->isContained() && !data->OperIsLeaf())
                {
                    dataIsUnary = (GenTree::OperIsUnary(data->OperGet()) != 0);
                    if (!dataIsUnary)
                    {
                        nonRMWsrc = data->gtGetOp1();
                        if (nonRMWsrc->isIndir() && Lowering::IndirsAreEquivalent(nonRMWsrc, treeNode))
                        {
                            nonRMWsrc = data->gtGetOp2();
                        }
                        genConsumeRegs(nonRMWsrc);
                    }
                }
                else
                {
                    genConsumeRegs(data);
                }
                if (reverseOps)
                {
                    genConsumeAddress(addr);
                }
                if (data->isContained() && !data->OperIsLeaf())
                {
                    NYI("RMW?");
                }
                else
                {
                    emit->emitInsMov(ins_Store(targetType), emitTypeSize(treeNode), treeNode);
                }
            }
        }
        break;

    case GT_COPY:
        {
            assert(treeNode->gtOp.gtOp1->IsLocal());
            GenTreeLclVarCommon* lcl = treeNode->gtOp.gtOp1->AsLclVarCommon();
            LclVarDsc* varDsc = &compiler->lvaTable[lcl->gtLclNum];
            inst_RV_RV(ins_Move_Extend(targetType, true), targetReg, genConsumeReg(treeNode->gtOp.gtOp1), targetType, emitTypeSize(targetType));

            // The old location is dying
            genUpdateRegLife(varDsc, /*isBorn*/ false, /*isDying*/ true DEBUGARG(treeNode->gtOp.gtOp1));

            gcInfo.gcMarkRegSetNpt(genRegMask(treeNode->gtOp.gtOp1->gtRegNum));

            genUpdateVarReg(varDsc, treeNode);

            // The new location is going live
            genUpdateRegLife(varDsc, /*isBorn*/ true, /*isDying*/ false DEBUGARG(treeNode));
        }
        genProduceReg(treeNode);
        break;

    case GT_LIST:
    case GT_ARGPLACE:
        // Nothing to do
        break;

    case GT_PUTARG_STK:
        {
            NYI_IF(targetType == TYP_STRUCT, "GT_PUTARG_STK: struct support not implemented");

            // Get argument offset on stack.
            // Here we cross check that argument offset hasn't changed from lowering to codegen since
            // we are storing arg slot number in GT_PUTARG_STK node in lowering phase.
            int argOffset = treeNode->AsPutArgStk()->gtSlotNum * TARGET_POINTER_SIZE;
#ifdef DEBUG
            fgArgTabEntryPtr curArgTabEntry = compiler->gtArgEntryByNode(treeNode->AsPutArgStk()->gtCall, treeNode);
            assert(curArgTabEntry);
            assert(argOffset == (int)curArgTabEntry->slotNum * TARGET_POINTER_SIZE);
#endif

            GenTreePtr data = treeNode->gtOp.gtOp1->gtEffectiveVal();
            if (data->isContained())
            {
                emit->emitIns_S_I(ins_Store(targetType), emitTypeSize(targetType), compiler->lvaOutgoingArgSpaceVar,
                            argOffset,
                            (int) data->AsIntConCommon()->IconValue());
            }
            else
            {
                genConsumeReg(data);
                emit->emitIns_S_R(ins_Store(targetType), emitTypeSize(targetType), data->gtRegNum, compiler->lvaOutgoingArgSpaceVar, argOffset);
            }
        }
        break;

    case GT_PUTARG_REG:
        {
            NYI_IF(targetType == TYP_STRUCT, "GT_PUTARG_REG: struct support not implemented");

            // commas show up here commonly, as part of a nullchk operation
            GenTree *op1 = treeNode->gtOp.gtOp1;
            // If child node is not already in the register we need, move it
            genConsumeReg(op1);
            if (treeNode->gtRegNum != op1->gtRegNum)
            {
                inst_RV_RV(ins_Move_Extend(targetType, true), treeNode->gtRegNum, op1->gtRegNum, targetType);
            }
        }
        genProduceReg(treeNode);
        break;

    case GT_CALL:
        genCallInstruction(treeNode);
        break;

    case GT_LOCKADD:
    case GT_XCHG:
    case GT_XADD:
        genLockedInstructions(treeNode);
        break;

    case GT_MEMORYBARRIER:
        instGen_MemoryBarrier();
        break;

    case GT_CMPXCHG:
        {
            NYI("GT_CMPXCHG");
        }
        genProduceReg(treeNode);
        break;

    case GT_RELOAD:
        // do nothing - reload is just a marker.
        // The parent node will call genConsumeReg on this which will trigger the unspill of this node's child
        // into the register specified in this node.
        break;

    case GT_NOP:
        break;

    case GT_NO_OP:
        NYI("GT_NO_OP");
        break;

    case GT_ARR_BOUNDS_CHECK:
        genRangeCheck(treeNode);
        break;

    case GT_PHYSREG:
        if (treeNode->gtRegNum != treeNode->AsPhysReg()->gtSrcReg)
        {
            inst_RV_RV(INS_mov, treeNode->gtRegNum, treeNode->AsPhysReg()->gtSrcReg, targetType);

            genTransferRegGCState(treeNode->gtRegNum, treeNode->AsPhysReg()->gtSrcReg);
        }
        break;

    case GT_PHYSREGDST:
        break;

    case GT_NULLCHECK:
        {
            assert(!treeNode->gtOp.gtOp1->isContained());
            regNumber reg = genConsumeReg(treeNode->gtOp.gtOp1);
            emit->emitIns_AR_R(INS_cmp, EA_4BYTE, reg, reg, 0);
        }
        break;

    case GT_CATCH_ARG:

        noway_assert(handlerGetsXcptnObj(compiler->compCurBB->bbCatchTyp));

        /* Catch arguments get passed in a register. genCodeForBBlist()
           would have marked it as holding a GC object, but not used. */

        noway_assert(gcInfo.gcRegGCrefSetCur & RBM_EXCEPTION_OBJECT);
        genConsumeReg(treeNode);
        break;

    case GT_PINVOKE_PROLOG:
        noway_assert(((gcInfo.gcRegGCrefSetCur|gcInfo.gcRegByrefSetCur) & ~RBM_ARG_REGS) == 0);

        // the runtime side requires the codegen here to be consistent
        emit->emitDisableRandomNops();
        break;

    case GT_LABEL:
        genPendingCallLabel = genCreateTempLabel();
        treeNode->gtLabel.gtLabBB = genPendingCallLabel;
        emit->emitIns_R_L(INS_lea, EA_PTRSIZE, genPendingCallLabel, treeNode->gtRegNum);
        break;

    default:
        {
#ifdef  DEBUG
            char message[256];
            sprintf(message, "NYI: Unimplemented node type %s\n", GenTree::NodeName(treeNode->OperGet()));
            notYetImplemented(message, __FILE__, __LINE__);
#else
            NYI("unimplemented node");
#endif
        }
        break;
    }
}

// generate code for the locked operations:
// GT_LOCKADD, GT_XCHG, GT_XADD
void
CodeGen::genLockedInstructions(GenTree* treeNode)
{
    NYI("genLockedInstructions");
}


// generate code for BoundsCheck nodes
void
CodeGen::genRangeCheck(GenTreePtr  oper)
{
#ifdef FEATURE_SIMD
    noway_assert(oper->OperGet() == GT_ARR_BOUNDS_CHECK || oper->OperGet() == GT_SIMD_CHK);
#else // !FEATURE_SIMD
    noway_assert(oper->OperGet() == GT_ARR_BOUNDS_CHECK);
#endif // !FEATURE_SIMD

    GenTreeBoundsChk* bndsChk = oper->AsBoundsChk();

    GenTreePtr arrLen = bndsChk->gtArrLen;
    GenTreePtr arrIndex = bndsChk->gtIndex;
    GenTreePtr arrRef = NULL;
    int lenOffset = 0;

    GenTree *src1, *src2;
    emitJumpKind jmpKind;

    genConsumeRegs(arrLen);
    genConsumeRegs(arrIndex);

    if (arrIndex->isContainedIntOrIImmed())
    {
        src1 = arrLen;
        src2 = arrIndex;
        jmpKind = EJ_jbe;
    }
    else
    {
        src1 = arrIndex;
        src2 = arrLen;
        jmpKind = EJ_jae;
    }

    GenTreeIntConCommon* intConst = nullptr;
    if (src2->isContainedIntOrIImmed())
    {
        intConst = src2->AsIntConCommon();
    }

    if (intConst != nullptr)
    {
        getEmitter()->emitIns_R_I(INS_cmp, EA_4BYTE, src1->gtRegNum, intConst->IconValue());
    }
    else
    {
        getEmitter()->emitIns_R_R(INS_cmp, EA_4BYTE, src1->gtRegNum, src2->gtRegNum);
    }

    genJumpToThrowHlpBlk(jmpKind, SCK_RNGCHK_FAIL, bndsChk->gtIndRngFailBB);
}

// make a temporary indir we can feed to pattern matching routines
// in cases where we don't want to instantiate all the indirs that happen
//
GenTreeIndir CodeGen::indirForm(var_types type, GenTree *base)
{
    GenTreeIndir i(GT_IND, type, base, nullptr);
    i.gtRegNum = REG_NA;
    // has to be nonnull (because contained nodes can't be the last in block)
    // but don't want it to be a valid pointer
    i.gtNext = (GenTree *)(-1);
    return i;
}

// make a temporary int we can feed to pattern matching routines
// in cases where we don't want to instantiate
//
GenTreeIntCon CodeGen::intForm(var_types type, ssize_t value)
{
    GenTreeIntCon i(type, value);
    i.gtRegNum = REG_NA;
    // has to be nonnull (because contained nodes can't be the last in block)
    // but don't want it to be a valid pointer
    i.gtNext = (GenTree *)(-1);
    return i;
}


instruction CodeGen::genGetInsForOper(genTreeOps oper, var_types type)
{
    instruction ins;

    if (varTypeIsFloating(type))
        return CodeGen::ins_MathOp(oper, type);

    switch (oper)
    {
    case GT_ADD: ins = INS_add; break;
    case GT_AND: ins = INS_AND; break;
    case GT_MUL: ins = INS_MUL; break;
    case GT_LSH: ins = INS_SHIFT_LEFT_LOGICAL; break;
    case GT_NEG: ins = INS_rsb; break;
    case GT_NOT: ins = INS_NOT; break;
    case GT_OR:  ins = INS_OR;  break;
    case GT_RSH: ins = INS_SHIFT_RIGHT_ARITHM; break;
    case GT_RSZ: ins = INS_SHIFT_RIGHT_LOGICAL; break;
    case GT_SUB: ins = INS_sub; break;
    case GT_XOR: ins = INS_XOR; break;
    default: unreached();
        break;
    }
    return ins;
}

/** Generates the code sequence for a GenTree node that
 * represents a bit shift operation (<<, >>, >>>).
 *
 * Arguments: operand:  the value to be shifted by shiftBy bits.
 *            shiftBy:  the number of bits to shift the operand.
 *            parent:   the actual bitshift node (that specifies the
 *                      type of bitshift to perform.
 *
 * Preconditions:    a) All GenTrees are register allocated.
 *                   b) Either shiftBy is a contained constant or
 *                      it's an expression sitting in RCX.
 *                   c) The actual bit shift node is not stack allocated
 *                      nor contained (not yet supported).
 */
void CodeGen::genCodeForShift(GenTreePtr operand, GenTreePtr shiftBy,
                              GenTreePtr parent)
{
    var_types targetType = parent->TypeGet();
    genTreeOps oper = parent->OperGet();
    instruction ins = genGetInsForOper(oper, targetType);
    emitAttr size = emitTypeSize(parent);

    assert(parent->gtRegNum != REG_NA);
    genConsumeReg(operand);
    
    if (!shiftBy->IsCnsIntOrI())
    {
        genConsumeReg(shiftBy);
        getEmitter()->emitIns_R_R_R(ins, size, parent->gtRegNum, operand->gtRegNum, shiftBy->gtRegNum);
    }
    else
    {
        getEmitter()->emitIns_R_R_I(ins, size, parent->gtRegNum, operand->gtRegNum, shiftBy->gtIntCon.gtIconVal);
    }

    genProduceReg(parent);
}

// TODO-Cleanup: move to CodeGenCommon.cpp
void CodeGen::genUnspillRegIfNeeded(GenTree *tree)
{
    regNumber dstReg = tree->gtRegNum;

    GenTree* unspillTree = tree;
    if (tree->gtOper == GT_RELOAD)
    {
        unspillTree = tree->gtOp.gtOp1;
    }
    if (unspillTree->gtFlags & GTF_SPILLED)
    {
        if (genIsRegCandidateLocal(unspillTree))
        {
            // Reset spilled flag, since we are going to load a local variable from its home location.
            unspillTree->gtFlags &= ~GTF_SPILLED;

            GenTreeLclVarCommon* lcl = unspillTree->AsLclVarCommon();
            LclVarDsc* varDsc = &compiler->lvaTable[lcl->gtLclNum];

            // Load local variable from its home location.
            inst_RV_TT(ins_Load(unspillTree->gtType, compiler->isSIMDTypeLocalAligned(lcl->gtLclNum)), dstReg, unspillTree);

            unspillTree->SetInReg();

            // TODO-Review: We would like to call:
            //      genUpdateRegLife(varDsc, /*isBorn*/ true, /*isDying*/ false DEBUGARG(tree));
            // instead of the following code, but this ends up hitting this assert:
            //      assert((regSet.rsMaskVars & regMask) == 0);
            // due to issues with LSRA resolution moves.
            // So, just force it for now. This probably indicates a condition that creates a GC hole!
            //
            // Extra note: I think we really want to call something like gcInfo.gcUpdateForRegVarMove,
            // because the variable is not really going live or dead, but that method is somewhat poorly
            // factored because it, in turn, updates rsMaskVars which is part of RegSet not GCInfo.
            // This code exists in other CodeGen*.cpp files.

            // Don't update the variable's location if we are just re-spilling it again.

            if ((unspillTree->gtFlags & GTF_SPILL) == 0)
            {
                genUpdateVarReg(varDsc, tree);
#ifdef DEBUG
                if (VarSetOps::IsMember(compiler, gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex))
                {
                    JITDUMP("\t\t\t\t\t\t\tRemoving V%02u from gcVarPtrSetCur\n", lcl->gtLclNum);
                }
#endif // DEBUG
                VarSetOps::RemoveElemD(compiler, gcInfo.gcVarPtrSetCur, varDsc->lvVarIndex);

#ifdef  DEBUG
                if (compiler->verbose)
                {
                    printf("\t\t\t\t\t\t\tV%02u in reg ", lcl->gtLclNum);
                    varDsc->PrintVarReg();
                    printf(" is becoming live  ");
                    compiler->printTreeID(unspillTree);
                    printf("\n");
                }
#endif // DEBUG

                regSet.rsMaskVars |= genGetRegMask(varDsc);
            }
        }
        else
        {
            TempDsc* t = regSet.rsUnspillInPlace(unspillTree);
            getEmitter()->emitIns_R_S(ins_Load(unspillTree->gtType),
                            emitActualTypeSize(unspillTree->gtType),
                            dstReg,
                            t->tdTempNum(),
                            0);
            compiler->tmpRlsTemp(t);

            unspillTree->gtFlags &= ~GTF_SPILLED;
            unspillTree->SetInReg();
        }

        gcInfo.gcMarkRegPtrVal(dstReg, unspillTree->TypeGet());
    }
}

void CodeGen::genRegCopy(GenTree* treeNode)
{
    assert(treeNode->OperGet() == GT_COPY);

    var_types targetType = treeNode->TypeGet();
    regNumber targetReg  = treeNode->gtRegNum;
    assert(targetReg != REG_NA);

    GenTree* op1 = treeNode->gtOp.gtOp1;

    // Check whether this node and the node from which we're copying the value have the same
    // register type.
    // This can happen if (currently iff) we have a SIMD vector type that fits in an integer
    // register, in which case it is passed as an argument, or returned from a call,
    // in an integer register and must be copied if it's in an xmm register.

    if (varTypeIsFloating(treeNode) != varTypeIsFloating(op1))
    {
#if 0
        instruction ins;
        regNumber fpReg;
        regNumber intReg;
        if(varTypeIsFloating(treeNode))
        {
            ins = INS_mov_i2xmm;
            fpReg = targetReg;
            intReg = op1->gtRegNum;
        }
        else
        {
            ins = INS_mov_xmm2i;
            intReg = targetReg;
            fpReg = op1->gtRegNum;
        }
        inst_RV_RV(ins, fpReg, intReg, targetType);
#else
        NYI_ARM("CodeGen - FP/Int RegCopy");
#endif
    }
    else
    {
        inst_RV_RV(ins_Copy(targetType), targetReg, genConsumeReg(op1), targetType);
    }

    if (op1->IsLocal())
    {
        // The lclVar will never be a def.
        // If it is a last use, the lclVar will be killed by genConsumeReg(), as usual, and genProduceReg will
        // appropriately set the gcInfo for the copied value.
        // If not, there are two cases we need to handle:
        // - If this is a TEMPORARY copy (indicated by the GTF_VAR_DEATH flag) the variable
        //   will remain live in its original register.
        //   genProduceReg() will appropriately set the gcInfo for the copied value,
        //   and genConsumeReg will reset it.
        // - Otherwise, we need to update register info for the lclVar.

        GenTreeLclVarCommon* lcl = op1->AsLclVarCommon();
        assert((lcl->gtFlags & GTF_VAR_DEF) == 0);

        if ((lcl->gtFlags & GTF_VAR_DEATH) == 0 && (treeNode->gtFlags & GTF_VAR_DEATH) == 0)
        {
            LclVarDsc* varDsc = &compiler->lvaTable[lcl->gtLclNum];

            // If we didn't just spill it (in genConsumeReg, above), then update the register info
            if (varDsc->lvRegNum != REG_STK)
            {
                // The old location is dying
                genUpdateRegLife(varDsc, /*isBorn*/ false, /*isDying*/ true DEBUGARG(op1));

                gcInfo.gcMarkRegSetNpt(genRegMask(op1->gtRegNum));

                genUpdateVarReg(varDsc, treeNode);

                // The new location is going live
                genUpdateRegLife(varDsc, /*isBorn*/ true, /*isDying*/ false DEBUGARG(treeNode));
            }
        }
    }
    genProduceReg(treeNode);
}

// Do liveness update for a subnode that is being consumed by codegen.
// TODO-Cleanup: move to CodeGenCommon.cpp
regNumber CodeGen::genConsumeReg(GenTree *tree)
{
    if (tree->OperGet() == GT_COPY)
    {
        genRegCopy(tree);
    }
    // Handle the case where we have a lclVar that needs to be copied before use (i.e. because it
    // interferes with one of the other sources (or the target, if it's a "delayed use" register)). 
    // TODO-Cleanup: This is a special copyReg case in LSRA - consider eliminating these and
    // always using GT_COPY to make the lclVar location explicit.
    // Note that we have to do this before calling genUpdateLife because otherwise if we spill it
    // the lvRegNum will be set to REG_STK and we will lose track of what register currently holds
    // the lclVar (normally when a lclVar is spilled it is then used from its former register
    // location, which matches the gtRegNum on the node).
    // (Note that it doesn't matter if we call this before or after genUnspillRegIfNeeded
    // because if it's on the stack it will always get reloaded into tree->gtRegNum).
    if (genIsRegCandidateLocal(tree))
    {
        GenTreeLclVarCommon *lcl = tree->AsLclVarCommon();
        LclVarDsc* varDsc = &compiler->lvaTable[lcl->GetLclNum()];
        if ((varDsc->lvRegNum != REG_STK) && (varDsc->lvRegNum != tree->gtRegNum))
        {
            inst_RV_RV(ins_Copy(tree->TypeGet()), tree->gtRegNum, varDsc->lvRegNum);
        }
    }

    genUnspillRegIfNeeded(tree);

    // genUpdateLife() will also spill local var if marked as GTF_SPILL by calling CodeGen::genSpillVar
    genUpdateLife(tree);
    assert(tree->gtRegNum != REG_NA);

    // there are three cases where consuming a reg means clearing the bit in the live mask
    // 1. it was not produced by a local
    // 2. it was produced by a local that is going dead
    // 3. it was produced by a local that does not live in that reg (like one allocated on the stack)

    if (genIsRegCandidateLocal(tree))
    {
        GenTreeLclVarCommon *lcl = tree->AsLclVarCommon();
        LclVarDsc* varDsc = &compiler->lvaTable[lcl->GetLclNum()];
        assert(varDsc->lvLRACandidate);

        if ((tree->gtFlags & GTF_VAR_DEATH) != 0)
        {
            gcInfo.gcMarkRegSetNpt(genRegMask(varDsc->lvRegNum));
        }
        else if (varDsc->lvRegNum == REG_STK)
        {
            // We have loaded this into a register only temporarily
            gcInfo.gcMarkRegSetNpt(genRegMask(tree->gtRegNum));
        }
    }
    else
    {
        gcInfo.gcMarkRegSetNpt(genRegMask(tree->gtRegNum));
    }

    return tree->gtRegNum;
}

// Do liveness update for an address tree: one of GT_LEA, GT_LCL_VAR, or GT_CNS_INT (for call indirect).
void CodeGen::genConsumeAddress(GenTree* addr)
{
    if (addr->OperGet() == GT_LEA)
    {
        genConsumeAddrMode(addr->AsAddrMode());
    }
    else
    {
        assert(!addr->isContained());
        genConsumeReg(addr);
    }
}

// do liveness update for a subnode that is being consumed by codegen
void CodeGen::genConsumeAddrMode(GenTreeAddrMode *addr)
{
    if (addr->Base())
        genConsumeReg(addr->Base());
    if (addr->Index())
        genConsumeReg(addr->Index());
}

// TODO-Cleanup: move to CodeGenCommon.cpp
void CodeGen::genConsumeRegs(GenTree* tree)
{
    if (tree->OperGet() == GT_LONG)
    {
        NYI_ARM("genConsumeRegs - long");
        return;
    }
    
    if (tree->isContained())
    {
        if (tree->isIndir())
        {
            genConsumeAddress(tree->AsIndir()->Addr());
        }
        else if (tree->OperGet() == GT_AND)
        {
            // This is the special contained GT_AND that we created in Lowering::LowerCmp()
            // Now we need to consume the operands of the GT_AND node.
            genConsumeOperands(tree->AsOp());
        }
        else
        {
            assert(tree->OperIsLeaf());
        }
    }
    else
    {
        genConsumeReg(tree);
    }
}

//------------------------------------------------------------------------
// genConsumeOperands: Do liveness update for the operands of a unary or binary tree
//
// Arguments:
//    tree - the GenTreeOp whose operands will have their liveness updated.
//
// Return Value:
//    None.
//
// Notes:
//    Note that this logic is localized here because we must do the liveness update in
//    the correct execution order.  This is important because we may have two operands
//    that involve the same lclVar, and if one is marked "lastUse" we must handle it
//    after the first.
// TODO-Cleanup: move to CodeGenCommon.cpp

void CodeGen::genConsumeOperands(GenTreeOp* tree)
{
    GenTree* firstOp = tree->gtOp1;
    GenTree* secondOp = tree->gtOp2;
    if ((tree->gtFlags & GTF_REVERSE_OPS) != 0)
    {
        assert(secondOp != nullptr);
        firstOp = secondOp;
        secondOp = tree->gtOp1;
    }
    if (firstOp != nullptr)
    {
        genConsumeRegs(firstOp);
    }
    if (secondOp != nullptr)
    {
        genConsumeRegs(secondOp);
    }
}

// do liveness update for register produced by the current node in codegen
void CodeGen::genProduceReg(GenTree *tree)
{
    if (tree->gtFlags & GTF_SPILL)
    {
        if (genIsRegCandidateLocal(tree))
        {
            // Store local variable to its home location.
            tree->gtFlags &= ~GTF_REG_VAL;
            inst_TT_RV(ins_Store(tree->gtType), tree, tree->gtRegNum);
        }
        else
        {
            tree->SetInReg();
            regSet.rsSpillTree(tree->gtRegNum, tree);
            tree->gtFlags |= GTF_SPILLED;
            tree->gtFlags &= ~GTF_SPILL;
            gcInfo.gcMarkRegSetNpt(genRegMask(tree->gtRegNum));
            return;
        }
    }

    genUpdateLife(tree);

    // If we've produced a register, mark it as a pointer, as needed.
    if (tree->gtHasReg())
    {
        // We only mark the register in the following cases:
        // 1. It is not a register candidate local. In this case, we're producing a
        //    register from a local, but the local is not a register candidate. Thus,
        //    we must be loading it as a temp register, and any "last use" flag on
        //    the register wouldn't be relevant.
        // 2. The register candidate local is going dead. There's no point to mark
        //    the register as live, with a GC pointer, if the variable is dead.
        if (!genIsRegCandidateLocal(tree) ||
            ((tree->gtFlags & GTF_VAR_DEATH) == 0))
        {
            gcInfo.gcMarkRegPtrVal(tree->gtRegNum, tree->TypeGet());
        }
    }
    tree->SetInReg();
}

// transfer gc/byref status of src reg to dst reg
void CodeGen::genTransferRegGCState(regNumber dst, regNumber src)
{
   regMaskTP srcMask = genRegMask(src);
   regMaskTP dstMask = genRegMask(dst);

   if (gcInfo.gcRegGCrefSetCur & srcMask)
   {
       gcInfo.gcMarkRegSetGCref(dstMask);
   }
   else if (gcInfo.gcRegByrefSetCur & srcMask)
   {
       gcInfo.gcMarkRegSetByref(dstMask);
   }
   else
   {
       gcInfo.gcMarkRegSetNpt(dstMask);
   }
}


// generates an ip-relative call or indirect call via reg ('call reg')
//     pass in 'addr' for a relative call or 'base' for a indirect register call
//     methHnd - optional, only used for pretty printing 
//     retSize - emitter type of return for GC purposes, should be EA_BYREF, EA_GCREF, or EA_PTRSIZE(not GC)
// TODO-Cleanup: move to CodeGenCommon.cpp
void CodeGen::genEmitCall(int                   callType,
                          CORINFO_METHOD_HANDLE methHnd,
                          INDEBUG_LDISASM_COMMA(CORINFO_SIG_INFO* sigInfo)
                          void*                 addr,
                          emitAttr              retSize,
                          IL_OFFSETX            ilOffset,
                          regNumber             base,
                          bool                  isJump,
                          bool                  isNoGC)
{
    
    getEmitter()->emitIns_Call(emitter::EmitCallType(callType),
                               methHnd,
                               INDEBUG_LDISASM_COMMA(sigInfo)
                               addr,
                               0,
                               retSize,
                               gcInfo.gcVarPtrSetCur,
                               gcInfo.gcRegGCrefSetCur,
                               gcInfo.gcRegByrefSetCur,
                               ilOffset,
                               base, REG_NA, 0, 0,
                               isJump, 
                               emitter::emitNoGChelper(compiler->eeGetHelperNum(methHnd)));
}

// generates an indirect call via addressing mode (call []) given an indir node
//     methHnd - optional, only used for pretty printing
//     retSize - emitter type of return for GC purposes, should be EA_BYREF, EA_GCREF, or EA_PTRSIZE(not GC)
// TODO-Cleanup: move to CodeGenCommon.cpp
void CodeGen::genEmitCall(int                   callType,
                          CORINFO_METHOD_HANDLE methHnd,
                          INDEBUG_LDISASM_COMMA(CORINFO_SIG_INFO* sigInfo)
                          GenTreeIndir*         indir,
                          emitAttr              retSize,
                          IL_OFFSETX            ilOffset)
{
    genConsumeAddress(indir->Addr());

    getEmitter()->emitIns_Call(emitter::EmitCallType(callType),
                               methHnd,
                               INDEBUG_LDISASM_COMMA(sigInfo)
                               nullptr,
                               0,
                               retSize,
                               gcInfo.gcVarPtrSetCur,
                               gcInfo.gcRegGCrefSetCur,
                               gcInfo.gcRegByrefSetCur,
                               ilOffset, 
                               indir->Base()  ? indir->Base()->gtRegNum : REG_NA,
                               indir->Index() ? indir->Index()->gtRegNum : REG_NA,
                               indir->Scale(),
                               indir->Offset());
}

// Produce code for a GT_CALL node
void CodeGen::genCallInstruction(GenTreePtr node)
{
    GenTreeCall *call = node->AsCall();

    assert(call->gtOper == GT_CALL);

    gtCallTypes callType  = (gtCallTypes)call->gtCallType;

    IL_OFFSETX      ilOffset  = BAD_IL_OFFSET;

    // all virtuals should have been expanded into a control expression
    assert (!call->IsVirtual() || call->gtControlExpr || call->gtCallAddr);

    // Consume all the arg regs
    for (GenTreePtr list = call->gtCallLateArgs; list; list = list->MoveNext())
    {
        assert(list->IsList());

        GenTreePtr argNode = list->Current();

        fgArgTabEntryPtr curArgTabEntry = compiler->gtArgEntryByNode(call, argNode->gtSkipReloadOrCopy());
        assert(curArgTabEntry);
        
        if (curArgTabEntry->regNum == REG_STK)
            continue;

        regNumber argReg = curArgTabEntry->regNum;
        genConsumeReg(argNode);
        if (argNode->gtRegNum != argReg)
        {
            inst_RV_RV(ins_Move_Extend(argNode->TypeGet(), argNode->InReg()), argReg, argNode->gtRegNum);
        }

        // In the case of a varargs call, 
        // the ABI dictates that if we have floating point args,
        // we must pass the enregistered arguments in both the 
        // integer and floating point registers so, let's do that.
        if (call->IsVarargs() && varTypeIsFloating(argNode))
        {
            NYI_ARM("CodeGen - IsVarargs");
        }
    }

    // Insert a null check on "this" pointer if asked.
    if (call->NeedsNullCheck())
    {
        const regNumber regThis = genGetThisArgReg(call);
        // TODO-ARM: Do we need to do anything when we use REG_SCRATCH?
        getEmitter()->emitIns_R_R_I(INS_ldr, EA_4BYTE, REG_SCRATCH, regThis, 0);
        //regTracker.rsTrackRegTrash(REG_SCRATCH);
    }

    // Either gtControlExpr != null or gtCallAddr != null or it is a direct non-virtual call to a user or helper method.
    CORINFO_METHOD_HANDLE methHnd;
    GenTree* target = call->gtControlExpr;
    if (callType == CT_INDIRECT)
    {
        assert(target == nullptr);
        target = call->gtCall.gtCallAddr;
        methHnd = nullptr;
    }
    else
    {
        methHnd = call->gtCallMethHnd;
    }
    
    CORINFO_SIG_INFO* sigInfo = nullptr;
#ifdef DEBUG
    // Pass the call signature information down into the emitter so the emitter can associate
    // native call sites with the signatures they were generated from.
    if (callType != CT_HELPER)
    {
        sigInfo = call->callSig;
    }
#endif // DEBUG

    // If fast tail call, then we are done.  In this case we setup the args (both reg args
    // and stack args in incoming arg area) and call target in rax.  Epilog sequence would
    // generate "br x0".
    if (call->IsFastTailCall())
    {
        NYI_ARM("CodeGen - IsFastTailCall");

        // Don't support fast tail calling JIT helpers
        assert(callType != CT_HELPER);

        // Fast tail calls materialize call target either in gtControlExpr or in gtCallAddr.
        assert(target != nullptr);

        genConsumeReg(target);
#if 0
        if (target->gtRegNum != REG_RAX)
        {
            inst_RV_RV(INS_mov, REG_RAX, target->gtRegNum);
        }
#endif
        return;
    }   

    // For a pinvoke to unmanged code we emit a label to clear 
    // the GC pointer state before the callsite.
    // We can't utilize the typical lazy killing of GC pointers
    // at (or inside) the callsite.
    if (call->IsUnmanaged())
    {
        genDefineTempLabel(genCreateTempLabel());
    }

    // Determine return value size.
    emitAttr retSize = EA_PTRSIZE;
    if (call->gtType == TYP_REF ||
        call->gtType == TYP_ARRAY)
    {
        retSize = EA_GCREF;
    }
    else if (call->gtType == TYP_BYREF)
    {
        retSize = EA_BYREF;
    }

#ifdef DEBUGGING_SUPPORT
    // We need to propagate the IL offset information to the call instruction, so we can emit
    // an IL to native mapping record for the call, to support managed return value debugging.
    // We don't want tail call helper calls that were converted from normal calls to get a record,
    // so we skip this hash table lookup logic in that case.
    if (compiler->opts.compDbgInfo && compiler->genCallSite2ILOffsetMap != nullptr && !call->IsTailCall())
    {
        (void)compiler->genCallSite2ILOffsetMap->Lookup(call, &ilOffset);
    }
#endif // DEBUGGING_SUPPORT
    
    if (target != nullptr)
    {
        // For ARM a call target can not be a contained indirection
        assert(!target->isContainedIndir());
            
        // We have already generated code for gtControlExpr evaluating it into a register.
        // We just need to emit "call reg" in this case.
        //
        assert(genIsValidIntReg(target->gtRegNum));

        genEmitCall(emitter::EC_INDIR_R,
                    methHnd,
                    INDEBUG_LDISASM_COMMA(sigInfo)
                    nullptr, //addr
                    retSize,
                    ilOffset,
                    genConsumeReg(target));
    }
    else
    {
        // Generate a direct call to a non-virtual user defined or helper method
        assert(callType == CT_HELPER || callType == CT_USER_FUNC);
        
        void *addr = nullptr; 
        if (callType == CT_HELPER)
        {            
            // Direct call to a helper method.
            CorInfoHelpFunc helperNum = compiler->eeGetHelperNum(methHnd);
            noway_assert(helperNum != CORINFO_HELP_UNDEF);

            void *pAddr = nullptr;
            addr = compiler->compGetHelperFtn(helperNum, (void **)&pAddr);

            if (addr == nullptr)
            {
                addr = pAddr;
            }
        }
        else
        {
            // Direct call to a non-virtual user function.
            CORINFO_ACCESS_FLAGS  aflags = CORINFO_ACCESS_ANY;
            if (call->IsSameThis())
            {
                aflags = (CORINFO_ACCESS_FLAGS)(aflags | CORINFO_ACCESS_THIS);
            }

            if ((call->NeedsNullCheck()) == 0)
            {
                aflags = (CORINFO_ACCESS_FLAGS)(aflags | CORINFO_ACCESS_NONNULL);
            }

            CORINFO_CONST_LOOKUP addrInfo;
            compiler->info.compCompHnd->getFunctionEntryPoint(methHnd, &addrInfo, aflags);

            addr = addrInfo.addr;
        }
#if 1
        // Use this path if you want to load an absolute call target using 
        //  a sequence of movs followed by an indirect call (blr instruction)

        // Load the call target address in REG_SCRATCH
        // TODO-ARM: Do we need to do anything when we use R10?
        instGen_Set_Reg_To_Imm(EA_4BYTE, REG_SCRATCH, (ssize_t) addr);
        //regTracker.rsTrackRegTrash(REG_SCRATCH);

        // indirect call to constant address in R10
        genEmitCall(emitter::EC_INDIR_R,
                    methHnd, 
                    INDEBUG_LDISASM_COMMA(sigInfo)
                    nullptr, //addr
                    retSize,
                    ilOffset,
                    REG_SCRATCH);
#else
        // Non-virtual direct call to known addresses
        genEmitCall(emitter::EC_FUNC_TOKEN,
                    methHnd, 
                    INDEBUG_LDISASM_COMMA(sigInfo)
                    addr,
                    retSize,
                    ilOffset);
#endif
    }

    // if it was a pinvoke we may have needed to get the address of a label
    if (genPendingCallLabel)
    {
        assert(call->IsUnmanaged());
        genDefineTempLabel(genPendingCallLabel);
        genPendingCallLabel = nullptr;
    }

    // Update GC info:
    // All Callee arg registers are trashed and no longer contain any GC pointers.
    // TODO-ARM-Bug?: As a matter of fact shouldn't we be killing all of callee trashed regs here?
    // For now we will assert that other than arg regs gc ref/byref set doesn't contain any other
    // registers from RBM_CALLEE_TRASH
    assert((gcInfo.gcRegGCrefSetCur & (RBM_CALLEE_TRASH & ~RBM_ARG_REGS)) == 0);
    assert((gcInfo.gcRegByrefSetCur & (RBM_CALLEE_TRASH & ~RBM_ARG_REGS)) == 0);
    gcInfo.gcRegGCrefSetCur &= ~RBM_ARG_REGS;
    gcInfo.gcRegByrefSetCur &= ~RBM_ARG_REGS;

    var_types returnType = call->TypeGet();
    if (returnType != TYP_VOID)
    {
        regNumber returnReg = (varTypeIsFloating(returnType) ? REG_FLOATRET : REG_INTRET);
        if (call->gtRegNum != returnReg)
        {
            inst_RV_RV(ins_Copy(returnType), call->gtRegNum, returnReg, returnType);
        }
        genProduceReg(call);
    }

    // If there is nothing next, that means the result is thrown away, so this value is not live.
    // However, for minopts or debuggable code, we keep it live to support managed return value debugging.
    if ((call->gtNext == nullptr) && !compiler->opts.MinOpts() && !compiler->opts.compDbgCode)
    {
        gcInfo.gcMarkRegSetNpt(RBM_INTRET);
    }
}

// produce code for a GT_LEA subnode
void CodeGen::genLeaInstruction(GenTreeAddrMode *lea)
{
    if (lea->Base() && lea->Index())
    {
        regNumber baseReg = genConsumeReg(lea->Base());
        regNumber indexReg = genConsumeReg(lea->Index());
        getEmitter()->emitIns_R_ARX (INS_lea, EA_BYREF, lea->gtRegNum, baseReg, indexReg, lea->gtScale, lea->gtOffset);
    }
    else if (lea->Base())
    {
        getEmitter()->emitIns_R_AR (INS_lea, EA_BYREF, lea->gtRegNum, genConsumeReg(lea->Base()), lea->gtOffset);
    }

    genProduceReg(lea);
}

// Generate code to materialize a condition into a register
// (the condition codes must already have been appropriately set)

void CodeGen::genSetRegToCond(regNumber dstReg, GenTreePtr tree)
{
    // Get the "jmpKind" using the gtOper kind
    // Note that whether it is an unsigned cmp is governed by the GTF_UNSIGNED flags

    emitJumpKind jmpKind = genJumpKindForOper(tree->gtOper, (tree->gtFlags & GTF_UNSIGNED) != 0);

    inst_SET(jmpKind, dstReg);
}

//------------------------------------------------------------------------
// genIntToIntCast: Generate code for an integer cast
//
// Arguments:
//    treeNode - The GT_CAST node
//
// Return Value:
//    None.
//
// Assumptions:
//    The treeNode must have an assigned register.
//    For a signed convert from byte, the source must be in a byte-addressable register.
//    Neither the source nor target type can be a floating point type.
//
void
CodeGen::genIntToIntCast(GenTreePtr treeNode)
{
    assert(treeNode->OperGet() == GT_CAST);

    GenTreePtr castOp = treeNode->gtCast.CastOp();
    emitter *  emit   = getEmitter();

    var_types dstType = treeNode->CastToType();
    var_types srcType = genActualType(castOp->TypeGet());
    emitAttr  movSize = emitActualTypeSize(dstType);
    bool      movRequired = false;

    bool isUnsignedDst = varTypeIsUnsigned(dstType);
    bool isUnsignedSrc = varTypeIsUnsigned(srcType);

    bool requiresOverflowCheck = false;

    regNumber targetReg = treeNode->gtRegNum;
    regNumber sourceReg = castOp->gtRegNum;

    assert(genIsValidIntReg(targetReg));
    assert(genIsValidIntReg(sourceReg));

    instruction ins = INS_invalid;

    // If necessary, force the srcType to unsigned when the GT_UNSIGNED flag is set.
    if (!isUnsignedSrc && (treeNode->gtFlags & GTF_UNSIGNED) != 0)
    {
        srcType = genUnsignedType(srcType);
        isUnsignedSrc = true;
    }

    if (treeNode->gtOverflow() && (genTypeSize(srcType) >= genTypeSize(dstType) || (srcType == TYP_INT && dstType == TYP_ULONG)))
    {
        requiresOverflowCheck = true;
    }

    genConsumeReg(castOp);

    if (requiresOverflowCheck)
    {
        emitAttr   cmpSize   = EA_ATTR(genTypeSize(srcType));
        ssize_t    typeMin   = 0;
        ssize_t    typeMax   = 0;
        ssize_t    typeMask  = 0;
        bool       signCheckOnly  = false;

        /* Do we need to compare the value, or just check masks */

        switch (dstType)
        {
        case TYP_BYTE:
            typeMask = ssize_t((int)0xFFFFFF80);
            typeMin  = SCHAR_MIN;
            typeMax  = SCHAR_MAX;
            break;

        case TYP_UBYTE:
            typeMask = ssize_t((int)0xFFFFFF00L);
            break;

        case TYP_SHORT:
            typeMask = ssize_t((int)0xFFFF8000);
            typeMin  = SHRT_MIN;
            break;

        case TYP_CHAR:
            typeMask = ssize_t((int)0xFFFF0000L);
            break;

        case TYP_INT:
            if (srcType == TYP_UINT)
            {
                signCheckOnly = true;
            }
            else
            {
                typeMask = 0xFFFFFFFF80000000LL;            
                typeMin  = INT_MIN;
                typeMax  = INT_MAX;
            }
            break;

        case TYP_UINT:
            if (srcType == TYP_INT)
            {
                signCheckOnly = true;
            }
            else
            {
                typeMask = 0x80000000L;
            }
            break;

        case TYP_LONG:
            noway_assert(srcType == TYP_ULONG);
            signCheckOnly = true;
            break;

        case TYP_ULONG:
            noway_assert((srcType == TYP_LONG) ||  (srcType == TYP_INT));
            signCheckOnly = true;
            break;

        default:
            NO_WAY("Unknown type");
            return;
        }

        if (signCheckOnly)
        {
            // We only need to check for a negative value in sourceReg
            emit->emitIns_R_I(INS_cmp, cmpSize, sourceReg, 0);
            genJumpToThrowHlpBlk(EJ_jl, SCK_OVERFLOW);
            if (dstType == TYP_ULONG)
            {
                // cast to TYP_ULONG:
                // We use a mov with size=EA_4BYTE
                // which will zero out the upper bits
                movSize = EA_4BYTE;
                movRequired = true;
            }
        }
        else
        {
            // When we are converting from/to unsigned,
            // we only have to check for any bits set in 'typeMask'
            if (isUnsignedSrc || isUnsignedDst)
            {
                noway_assert(typeMask != 0);
                emit->emitIns_R_I(INS_tst, cmpSize, sourceReg, typeMask);
                genJumpToThrowHlpBlk(EJ_jne, SCK_OVERFLOW);
            }
            else
            {
                // For a narrowing signed cast
                //
                // We must check the value is in a signed range.

                // Compare with the MAX

                noway_assert((typeMin != 0) && (typeMax != 0));

                emit->emitIns_R_I(INS_cmp, cmpSize, sourceReg, typeMax);
                genJumpToThrowHlpBlk(EJ_jg, SCK_OVERFLOW);

                // Compare with the MIN

                emit->emitIns_R_I(INS_cmp, cmpSize, sourceReg, typeMin);
                genJumpToThrowHlpBlk(EJ_jl, SCK_OVERFLOW);
            }
        }
        ins = INS_mov;
    }
    else // Non-overflow checking cast.
    {
        if (genTypeSize(srcType) == genTypeSize(dstType))
        {
            ins = INS_mov;
        }
        else
        {
            var_types extendType;

            if (genTypeSize(srcType) < genTypeSize(dstType))
            {
                extendType = srcType;
                // TODO-ARM: Check correct behaviour here.
#if 0
                if (srcType == TYP_UINT)
                {
                    movSize = EA_4BYTE;  // force a mov EA_4BYTE to zero the upper bits
                    movRequired = true;
                }
#endif
            }
            else // (genTypeSize(srcType) > genTypeSize(dstType))
            {
                extendType = dstType;
                if (varTypeIsShort(dstType))
                {
                    movSize = EA_2BYTE; // a uxth instruction requires EA_2BYTE
                }
                else if (varTypeIsByte(dstType))
                {
                    movSize = EA_1BYTE; // a uxtb instruction requires EA_1BYTE
                }
            }

            ins = ins_Move_Extend(extendType, castOp->InReg());
        }
    }

    if ((ins != INS_mov) || movRequired || (targetReg != sourceReg))
    {            
        emit->emitIns_R_R(ins, movSize, targetReg, sourceReg);
    }

    genProduceReg(treeNode);
}

//------------------------------------------------------------------------
// genFloatToFloatCast: Generate code for a cast between float and double
//
// Arguments:
//    treeNode - The GT_CAST node
//
// Return Value:
//    None.
//
// Assumptions:
//    Cast is a non-overflow conversion.
//    The treeNode must have an assigned register.
//    The cast is between float and double.
//
void
CodeGen::genFloatToFloatCast(GenTreePtr treeNode)
{
    NYI("Cast");
}

//------------------------------------------------------------------------
// genIntToFloatCast: Generate code to cast an int/long to float/double
//
// Arguments:
//    treeNode - The GT_CAST node
//
// Return Value:
//    None.
//
// Assumptions:
//    Cast is a non-overflow conversion.
//    The treeNode must have an assigned register.
//    SrcType= int32/uint32/int64/uint64 and DstType=float/double.
//
void
CodeGen::genIntToFloatCast(GenTreePtr treeNode)
{
    NYI("Cast");
}

//------------------------------------------------------------------------
// genFloatToIntCast: Generate code to cast float/double to int/long
//
// Arguments:
//    treeNode - The GT_CAST node
//
// Return Value:
//    None.
//
// Assumptions:
//    Cast is a non-overflow conversion.
//    The treeNode must have an assigned register.
//    SrcType=float/double and DstType= int32/uint32/int64/uint64
//
void
CodeGen::genFloatToIntCast(GenTreePtr treeNode)
{
    NYI("Cast");
}

/*****************************************************************************
 *
 *  Create and record GC Info for the function.
 */
#ifdef JIT32_GCENCODER
void*
#else
void
#endif
CodeGen::genCreateAndStoreGCInfo(unsigned codeSize, unsigned prologSize, unsigned epilogSize DEBUG_ARG(void* codePtr))
{
#ifdef JIT32_GCENCODER
    return genCreateAndStoreGCInfoJIT32(codeSize, prologSize, epilogSize DEBUG_ARG(codePtr));
#else
    genCreateAndStoreGCInfoX64(codeSize, prologSize DEBUG_ARG(codePtr));
#endif
}

// TODO-ARM-Cleanup: It seems that the ARM JIT (classic and otherwise) uses this method, so it seems to be inappropriately named?

void                CodeGen::genCreateAndStoreGCInfoX64(unsigned codeSize, unsigned prologSize DEBUG_ARG(void* codePtr))
{
    IAllocator* allowZeroAlloc = new (compiler, CMK_GC) AllowZeroAllocator(compiler->getAllocatorGC());
    GcInfoEncoder* gcInfoEncoder = new (compiler, CMK_GC) GcInfoEncoder(compiler->info.compCompHnd, compiler->info.compMethodInfo, allowZeroAlloc);
    assert(gcInfoEncoder != nullptr);

    // Follow the code pattern of the x86 gc info encoder (genCreateAndStoreGCInfoJIT32).
    gcInfo.gcInfoBlockHdrSave(gcInfoEncoder, codeSize, prologSize);

    // First we figure out the encoder ID's for the stack slots and registers.
    gcInfo.gcMakeRegPtrTable(gcInfoEncoder, codeSize, prologSize, GCInfo::MAKE_REG_PTR_MODE_ASSIGN_SLOTS);
    // Now we've requested all the slots we'll need; "finalize" these (make more compact data structures for them).
    gcInfoEncoder->FinalizeSlotIds();
    // Now we can actually use those slot ID's to declare live ranges.
    gcInfo.gcMakeRegPtrTable(gcInfoEncoder, codeSize, prologSize, GCInfo::MAKE_REG_PTR_MODE_DO_WORK);

#if defined(DEBUGGING_SUPPORT)
    if (compiler->opts.compDbgEnC)
    {
        // what we have to preserve is called the "frame header" (see comments in VM\eetwain.cpp)
        // which is:
        //  -return address
        //  -saved off RBP
        //  -saved 'this' pointer and bool for synchronized methods

        // 4 slots for RBP + return address + RSI + RDI
        int preservedAreaSize = 4 * REGSIZE_BYTES;

        if (compiler->info.compFlags & CORINFO_FLG_SYNCH)
        {
            if (!(compiler->info.compFlags & CORINFO_FLG_STATIC))
                preservedAreaSize += REGSIZE_BYTES; 

            preservedAreaSize += 1; // bool for synchronized methods
        }

        // Used to signal both that the method is compiled for EnC, and also the size of the block at the top of the frame
        gcInfoEncoder->SetSizeOfEditAndContinuePreservedArea(preservedAreaSize);
    }  
#endif

    gcInfoEncoder->Build();

    //GC Encoder automatically puts the GC info in the right spot using ICorJitInfo::allocGCInfo(size_t)
    //let's save the values anyway for debugging purposes
    compiler->compInfoBlkAddr = gcInfoEncoder->Emit();
    compiler->compInfoBlkSize = 0; //not exposed by the GCEncoder interface
}

/*****************************************************************************
 *  Emit a call to a helper function.
 */

void        CodeGen::genEmitHelperCall(unsigned    helper,
                                       int         argSize,
                                       emitAttr    retSize
#ifndef LEGACY_BACKEND
                                       ,regNumber   callTargetReg /*= REG_NA */
#endif // !LEGACY_BACKEND
                                       )
{
    void* addr  = nullptr;
    void* pAddr = nullptr;

    emitter::EmitCallType  callType = emitter::EC_FUNC_TOKEN;
    addr = compiler->compGetHelperFtn((CorInfoHelpFunc)helper, &pAddr);
    regNumber callTarget = REG_NA;

    if (addr == nullptr)
    {
        NYI("genEmitHelperCall indirect");
#if 0
        assert(pAddr != nullptr);
        if (genAddrShouldUsePCRel((size_t)pAddr))
        {
            // generate call whose target is specified by PC-relative 32-bit offset.
            callType = emitter::EC_FUNC_TOKEN_INDIR;
            addr = pAddr;
        }
        else
        {
            // If this address cannot be encoded as PC-relative 32-bit offset, load it into REG_HELPER_CALL_TARGET
            // and use register indirect addressing mode to make the call.
            //    mov   reg, addr
            //    call  [reg]
            if (callTargetReg == REG_NA)
            {
                // If a callTargetReg has not been explicitly provided, we will use REG_DEFAULT_HELPER_CALL_TARGET, but
                // this is only a valid assumption if the helper call is known to kill REG_DEFAULT_HELPER_CALL_TARGET.
                callTargetReg = REG_DEFAULT_HELPER_CALL_TARGET;
            }

            regMaskTP callTargetMask = genRegMask(callTargetReg);
            regMaskTP callKillSet = compiler->compHelperCallKillSet((CorInfoHelpFunc)helper);

            // assert that all registers in callTargetMask are in the callKillSet
            noway_assert((callTargetMask & callKillSet) == callTargetMask);

            callTarget = callTargetReg;
            CodeGen::genSetRegToIcon(callTarget, (ssize_t) pAddr, TYP_I_IMPL);
            callType = emitter::EC_INDIR_ARD;
        }
#endif // 0
    }

    if (!validImmForBL((ssize_t) addr))
    {
        // TODO-ARM: Do we need to do anything when we use REG_SCRATCH?
        callTarget = REG_SCRATCH;
        callType = emitter::EC_INDIR_R;
        instGen_Set_Reg_To_Imm(EA_HANDLE_CNS_RELOC, callTarget, (ssize_t)addr);
        regTracker.rsTrackRegTrash(callTarget);
        addr = nullptr;
    }
    
    getEmitter()->emitIns_Call(callType,
                                compiler->eeFindHelper(helper),
                                INDEBUG_LDISASM_COMMA(nullptr)
                                addr,
                                argSize,
                                retSize,
                                gcInfo.gcVarPtrSetCur,
                                gcInfo.gcRegGCrefSetCur,
                                gcInfo.gcRegByrefSetCur,
                                BAD_IL_OFFSET,       /* IL offset */
                                callTarget,          /* ireg */
                                REG_NA, 0, 0,        /* xreg, xmul, disp */
                                false,               /* isJump */
                                emitter::emitNoGChelper(helper));
    
    regMaskTP killMask = compiler->compHelperCallKillSet((CorInfoHelpFunc)helper);
    regTracker.rsTrashRegSet(killMask);
    regTracker.rsTrashRegsForGCInterruptability();
}

/*****************************************************************************/
#ifdef DEBUGGING_SUPPORT
/*****************************************************************************
 *                          genSetScopeInfo
 *
 * Called for every scope info piece to record by the main genSetScopeInfo()
 */

void        CodeGen::genSetScopeInfo  (unsigned             which,
                                       UNATIVE_OFFSET       startOffs,
                                       UNATIVE_OFFSET       length,
                                       unsigned             varNum,
                                       unsigned             LVnum,
                                       bool                 avail,
                                       Compiler::siVarLoc&  varLoc)
{
    /* We need to do some mapping while reporting back these variables */

    unsigned ilVarNum = compiler->compMap2ILvarNum(varNum);
    noway_assert((int)ilVarNum != ICorDebugInfo::UNKNOWN_ILNUM);

    VarName name = nullptr;

#ifdef DEBUG

    for (unsigned scopeNum = 0; scopeNum < compiler->info.compVarScopesCount; scopeNum++)
    {
        if (LVnum == compiler->info.compVarScopes[scopeNum].vsdLVnum)
        {
            name = compiler->info.compVarScopes[scopeNum].vsdName;
        }
    }

    // Hang on to this compiler->info.

    TrnslLocalVarInfo &tlvi = genTrnslLocalVarInfo[which];

    tlvi.tlviVarNum         = ilVarNum;
    tlvi.tlviLVnum          = LVnum;
    tlvi.tlviName           = name;
    tlvi.tlviStartPC        = startOffs;
    tlvi.tlviLength         = length;
    tlvi.tlviAvailable      = avail;
    tlvi.tlviVarLoc         = varLoc;

#endif // DEBUG

    compiler->eeSetLVinfo(which, startOffs, length, ilVarNum, LVnum, name, avail, varLoc);
}
#endif // DEBUGGING_SUPPORT

#endif // _TARGET_AMD64_

#endif // !LEGACY_BACKEND
