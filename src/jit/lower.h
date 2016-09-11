// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                               Lower                                       XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#ifndef _LOWER_H_
#define _LOWER_H_

#include "compiler.h"
#include "phase.h"
#include "lsra.h"

class Lowering : public Phase
{
public:
    inline Lowering(Compiler* compiler, LinearScanInterface* lsra)
        : Phase(compiler, "Lowering", PHASE_LOWERING), vtableCallTemp(BAD_VAR_NUM)
    {
        m_lsra = (LinearScan*)lsra;
        assert(m_lsra);
    }
    virtual void DoPhase();

    // If requiresOverflowCheck is false, all other values will be unset
    struct CastInfo
    {
        bool requiresOverflowCheck; // Will the cast require an overflow check
        bool unsignedSource;        // Is the source unsigned
        bool unsignedDest;          // is the dest unsigned

        // All other fields are only meaningful if requiresOverflowCheck is set.

        ssize_t typeMin;       // Lowest storable value of the dest type
        ssize_t typeMax;       // Highest storable value of the dest type
        ssize_t typeMask;      // For converting from/to unsigned
        bool    signCheckOnly; // For converting between unsigned/signed int
    };

#ifdef _TARGET_64BIT_
    static void getCastDescription(GenTreePtr treeNode, CastInfo* castInfo);
#endif // _TARGET_64BIT_

private:
#ifdef DEBUG
    static void CheckCallArg(GenTree* arg);
    static void CheckCall(GenTreeCall* call);
    static void CheckNode(GenTree* node);
    static bool CheckBlock(Compiler* compiler, BasicBlock* block);
#endif // DEBUG

    void LowerBlock(BasicBlock* block);
    GenTree* LowerNode(GenTree* node);
    void CheckVSQuirkStackPaddingNeeded(GenTreeCall* call);

    // ------------------------------
    // Call Lowering
    // ------------------------------
    void LowerCall(GenTree* call);
    void LowerJmpMethod(GenTree* jmp);
    void LowerRet(GenTree* ret);
    GenTree* LowerDelegateInvoke(GenTreeCall* call);
    GenTree* LowerIndirectNonvirtCall(GenTreeCall* call);
    GenTree* LowerDirectCall(GenTreeCall* call);
    GenTree* LowerNonvirtPinvokeCall(GenTreeCall* call);
    GenTree* LowerTailCallViaHelper(GenTreeCall* callNode, GenTree* callTarget);
    void LowerFastTailCall(GenTreeCall* callNode);
    void InsertProfTailCallHook(GenTreeCall* callNode, GenTree* insertionPoint);
    GenTree* LowerVirtualVtableCall(GenTreeCall* call);
    GenTree* LowerVirtualStubCall(GenTreeCall* call);
    void LowerArgsForCall(GenTreeCall* call);
    void ReplaceArgWithPutArgOrCopy(GenTreePtr* ppChild, GenTreePtr newNode);
    GenTree* NewPutArg(GenTreeCall* call, GenTreePtr arg, fgArgTabEntryPtr info, var_types type);
    void LowerArg(GenTreeCall* call, GenTreePtr* ppTree);
    void InsertPInvokeCallProlog(GenTreeCall* call);
    void InsertPInvokeCallEpilog(GenTreeCall* call);
    void InsertPInvokeMethodProlog();
    void InsertPInvokeMethodEpilog(BasicBlock* returnBB DEBUGARG(GenTreePtr lastExpr));
    GenTree* SetGCState(int cns);
    GenTree* CreateReturnTrapSeq();
    enum FrameLinkAction
    {
        PushFrame,
        PopFrame
    };
    GenTree* CreateFrameLinkUpdate(FrameLinkAction);
    GenTree* AddrGen(ssize_t addr, regNumber reg = REG_NA);
    GenTree* AddrGen(void* addr, regNumber reg = REG_NA);

    GenTree* Ind(GenTree* tree)
    {
        return comp->gtNewOperNode(GT_IND, TYP_I_IMPL, tree);
    }

    GenTree* PhysReg(regNumber reg, var_types type = TYP_I_IMPL)
    {
        return comp->gtNewPhysRegNode(reg, type);
    }

    GenTree* PhysRegDst(regNumber reg, GenTree* src)
    {
        return comp->gtNewPhysRegNode(reg, src);
    }

    GenTree* ThisReg(GenTreeCall* call)
    {
        return PhysReg(comp->codeGen->genGetThisArgReg(call), TYP_REF);
    }

    GenTree* Offset(GenTree* base, unsigned offset)
    {
        var_types resultType = (base->TypeGet() == TYP_REF) ? TYP_BYREF : base->TypeGet();
        return new (comp, GT_LEA) GenTreeAddrMode(resultType, base, nullptr, 0, offset);
    }

    // returns true if the tree can use the read-modify-write memory instruction form
    bool isRMWRegOper(GenTreePtr tree);

    // return true if this call target is within range of a pc-rel call on the machine
    bool IsCallTargetInRange(void* addr);

    void TreeNodeInfoInit(GenTree* stmt);

#if defined(_TARGET_XARCH_)
    void TreeNodeInfoInitSimple(GenTree* tree);

    //----------------------------------------------------------------------
    // SetRegOptional - sets a bit to indicate to LSRA that register
    // for a given child node is optional for codegen purpose.  If no
    // register is allocated to such a tree node, its parent node treats
    // it as a contained memory operand during codegen.  This routine
    // also updates srcRegOptional count of parent node.
    //
    // Arguments:
    //    parent  -   Parent GenTree node with 'child' as one of its operands.
    //    child    -  Child GenTree node that is to marked as reg optional.
    //
    // Returns
    //    None
    void SetRegOptional(GenTree* parent, GenTree* child)
    {        
        child->gtLsraInfo.regOptional = true;

        // On xarch, at the most one operand can be honored as
        // reg optional even if more than one operand of parent
        // node is marked as reg optional.
        parent->gtLsraInfo.srcMemOpCount = 1;
    }   

    // ------------------------------------------------------------------
    // SetRegOptionalBinOp - Marks all the operands of a bin-op that are
    // permitted to be memory-op as reg optional. Lsra might not allocate
    // a register to RefTypeUse positions of such operands if it is
    // beneficial. In such a case codegen will treat them as memory
    // operands.
    //
    // Arguments:
    //     tree  -  Gentree of a bininary operation.
    //
    // Returns
    //     None.
    //
    void SetRegOptionalForBinOp(GenTree* tree)
    {
        assert(GenTree::OperIsBinary(tree->OperGet()));

        GenTree* op1 = tree->gtGetOp1();
        GenTree* op2 = tree->gtGetOp2();

        if (tree->OperIsCommutative() && tree->TypeGet() == op1->TypeGet())
        {
            SetRegOptional(tree, op1);
        }
        
        if (tree->TypeGet() == op2->TypeGet())
        {
            SetRegOptional(tree, op2);
        }
    }
#endif // defined(_TARGET_XARCH_)

    void TreeNodeInfoInitReturn(GenTree* tree);
    void TreeNodeInfoInitShiftRotate(GenTree* tree);
    void TreeNodeInfoInitCall(GenTreeCall* call);
    void TreeNodeInfoInitStructArg(GenTreePtr structArg);
    void TreeNodeInfoInitBlockStore(GenTreeBlk* blkNode);
    void TreeNodeInfoInitLogicalOp(GenTree* tree);
    void TreeNodeInfoInitModDiv(GenTree* tree);
    void TreeNodeInfoInitIntrinsic(GenTree* tree);
#ifdef FEATURE_SIMD
    void TreeNodeInfoInitSIMD(GenTree* tree);
#endif // FEATURE_SIMD
    void TreeNodeInfoInitCast(GenTree* tree);
#ifdef _TARGET_ARM64_
    void TreeNodeInfoInitPutArgStk(GenTree* argNode, fgArgTabEntryPtr info);
#endif // _TARGET_ARM64_
#ifdef FEATURE_UNIX_AMD64_STRUCT_PASSING
    void TreeNodeInfoInitPutArgStk(GenTree* tree);
#endif // FEATURE_UNIX_AMD64_STRUCT_PASSING
    void TreeNodeInfoInitLclHeap(GenTree* tree);

    void DumpNodeInfoMap();

    // Per tree node member functions
    void LowerStoreInd(GenTree* node);
    GenTree* LowerAdd(GenTree* node);
    void LowerUnsignedDivOrMod(GenTree* node);
    GenTree* LowerSignedDivOrMod(GenTree* node);
    void LowerBlockStore(GenTreeBlk* blkNode);

    GenTree* TryCreateAddrMode(LIR::Use&& use, bool isIndir);
    void AddrModeCleanupHelper(GenTreeAddrMode* addrMode, GenTree* node);

    GenTree* LowerSwitch(GenTree* node);
    void LowerCast(GenTree* node);

#if defined(_TARGET_XARCH_)
    void SetMulOpCounts(GenTreePtr tree);
#endif // defined(_TARGET_XARCH_)

    void LowerCmp(GenTreePtr tree);

#if !CPU_LOAD_STORE_ARCH
    bool IsBinOpInRMWStoreInd(GenTreePtr tree);
    bool IsRMWMemOpRootedAtStoreInd(GenTreePtr storeIndTree, GenTreePtr* indirCandidate, GenTreePtr* indirOpSource);
    bool SetStoreIndOpCountsIfRMWMemOp(GenTreePtr storeInd);
#endif
    void LowerStoreLoc(GenTreeLclVarCommon* tree);
    void SetIndirAddrOpCounts(GenTree* indirTree);
    void LowerGCWriteBarrier(GenTree* tree);
    GenTree* LowerArrElem(GenTree* node);
    void LowerRotate(GenTree* tree);

    // Utility functions
    void MorphBlkIntoHelperCall(GenTreePtr pTree, GenTreePtr treeStmt);

public:
    static bool IndirsAreEquivalent(GenTreePtr pTreeA, GenTreePtr pTreeB);

private:
    static bool NodesAreEquivalentLeaves(GenTreePtr candidate, GenTreePtr storeInd);

    bool AreSourcesPossiblyModified(GenTree* addr, GenTree* base, GenTree* index);

    // return true if 'childNode' is an immediate that can be contained
    //  by the 'parentNode' (i.e. folded into an instruction)
    //  for example small enough and non-relocatable
    bool IsContainableImmed(GenTree* parentNode, GenTree* childNode);

    // Makes 'childNode' contained in the 'parentNode'
    void MakeSrcContained(GenTreePtr parentNode, GenTreePtr childNode);

    // Checks and makes 'childNode' contained in the 'parentNode'
    bool CheckImmedAndMakeContained(GenTree* parentNode, GenTree* childNode);

    // Checks for memory conflicts in the instructions between childNode and parentNode, and returns true if childNode
    // can be contained.
    bool IsSafeToContainMem(GenTree* parentNode, GenTree* childNode);

    inline LIR::Range& BlockRange() const
    {
        return LIR::AsRange(m_block);
    }

    LinearScan* m_lsra;
    unsigned    vtableCallTemp; // local variable we use as a temp for vtable calls
    BasicBlock* m_block;
};

#endif // _LOWER_H_
