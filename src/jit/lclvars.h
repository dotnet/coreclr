// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*****************************************************************************
 *
 *  The following holds the local variable counts and the descriptor table.
 */

// This is the location of a definition.
struct DefLoc
{
    BasicBlock* m_blk;
    GenTreePtr  m_tree;

    DefLoc() : m_blk(nullptr), m_tree(nullptr)
    {
    }
};

// This class encapsulates all info about a local variable that may vary for different SSA names
// in the family.
class LclSsaVarDsc
{
public:
    ValueNumPair m_vnPair;
    DefLoc       m_defLoc;

    LclSsaVarDsc()
    {
    }
};

typedef ExpandArray<LclSsaVarDsc> PerSsaArray;

class LclVarDsc
{
public:
    // The constructor. Most things can just be zero'ed.
    LclVarDsc(Compiler* comp);

    // note this only packs because var_types is a typedef of unsigned char
    var_types lvType : 5; // TYP_INT/LONG/FLOAT/DOUBLE/REF

    unsigned char lvIsParam : 1;           // is this a parameter?
    unsigned char lvIsRegArg : 1;          // is this a register argument?
    unsigned char lvFramePointerBased : 1; // 0 = off of REG_SPBASE (e.g., ESP), 1 = off of REG_FPBASE (e.g., EBP)

    unsigned char lvStructGcCount : 3; // if struct, how many GC pointer (stop counting at 7). The only use of values >1
                                       // is to help determine whether to use block init in the prolog.
    unsigned char lvOnFrame : 1;       // (part of) the variable lives on the frame
    unsigned char lvDependReg : 1;     // did the predictor depend upon this being enregistered
    unsigned char lvRegister : 1;      // assigned to live in a register? For RyuJIT backend, this is only set if the
                                       // variable is in the same register for the entire function.
    unsigned char lvTracked : 1;       // is this a tracked variable?
    bool          lvTrackedNonStruct()
    {
        return lvTracked && lvType != TYP_STRUCT;
    }
    unsigned char lvPinned : 1; // is this a pinned variable?

    unsigned char lvMustInit : 1;    // must be initialized
    unsigned char lvAddrExposed : 1; // The address of this variable is "exposed" -- passed as an argument, stored in a
                                     // global location, etc.
                                     // We cannot reason reliably about the value of the variable.
    unsigned char lvDoNotEnregister : 1; // Do not enregister this variable.
    unsigned char lvFieldAccessed : 1;   // The var is a struct local, and a field of the variable is accessed.  Affects
                                         // struct promotion.

#ifdef DEBUG
    // These further document the reasons for setting "lvDoNotEnregister".  (Note that "lvAddrExposed" is one of the
    // reasons;
    // also, lvType == TYP_STRUCT prevents enregistration.  At least one of the reasons should be true.
    unsigned char lvVMNeedsStackAddr : 1; // The VM may have access to a stack-relative address of the variable, and
                                          // read/write its value.
    unsigned char lvLiveInOutOfHndlr : 1; // The variable was live in or out of an exception handler, and this required
                                          // the variable to be
                                          // in the stack (at least at those boundaries.)
    unsigned char lvLclFieldExpr : 1;     // The variable is not a struct, but was accessed like one (e.g., reading a
                                          // particular byte from an int).
    unsigned char lvLclBlockOpAddr : 1;   // The variable was written to via a block operation that took its address.
    unsigned char lvLiveAcrossUCall : 1;  // The variable is live across an unmanaged call.
#endif
    unsigned char lvIsCSE : 1;       // Indicates if this LclVar is a CSE variable.
    unsigned char lvRefAssign : 1;   // involved in pointer assignment
    unsigned char lvHasLdAddrOp : 1; // has ldloca or ldarga opcode on this local.
    unsigned char lvStackByref : 1;  // This is a compiler temporary of TYP_BYREF that is known to point into our local
                                     // stack frame.

    unsigned char lvHasILStoreOp : 1;         // there is at least one STLOC or STARG on this local
    unsigned char lvHasMultipleILStoreOp : 1; // there is more than one STLOC on this local

    unsigned char lvIsTemp : 1; // Short-lifetime compiler temp (if lvIsParam is false), or implicit byref parameter
                                // (if lvIsParam is true)
#if OPT_BOOL_OPS
    unsigned char lvIsBoolean : 1; // set if variable is boolean
#endif
    unsigned char lvRngOptDone : 1; // considered for range check opt?
    unsigned char lvLoopInc : 1;    // incremented in the loop?
    unsigned char lvLoopAsg : 1;    // reassigned  in the loop (other than a monotonic inc/dec for the index var)?
    unsigned char lvArrIndx : 1;    // used as an array index?
    unsigned char lvArrIndxOff : 1; // used as an array index with an offset?
    unsigned char lvArrIndxDom : 1; // index dominates loop exit
#if ASSERTION_PROP
    unsigned char lvSingleDef : 1;    // variable has a single def
    unsigned char lvDisqualify : 1;   // variable is no longer OK for add copy optimization
    unsigned char lvVolatileHint : 1; // hint for AssertionProp
#endif

    unsigned char lvSpilled : 1; // enregistered variable was spilled
#ifndef _TARGET_64BIT_
    unsigned char lvStructDoubleAlign : 1; // Must we double align this struct?
#endif                                     // !_TARGET_64BIT_
#ifdef _TARGET_64BIT_
    unsigned char lvQuirkToLong : 1; // Quirk to allocate this LclVar as a 64-bit long
#endif
#ifdef DEBUG
    unsigned char lvKeepType : 1;       // Don't change the type of this variable
    unsigned char lvNoLclFldStress : 1; // Can't apply local field stress on this one
#endif
    unsigned char lvIsPtr : 1; // Might this be used in an address computation? (used by buffer overflow security
                               // checks)
    unsigned char lvIsUnsafeBuffer : 1; // Does this contain an unsafe buffer requiring buffer overflow security checks?
    unsigned char lvPromoted : 1; // True when this local is a promoted struct, a normed struct, or a "split" long on a
                                  // 32-bit target.  For implicit byref parameters, this gets hijacked between
                                  // fgRetypeImplicitByRefArgs and fgMarkDemotedImplicitByRefArgs to indicate whether
                                  // references to the arg are being rewritten as references to a promoted shadow local.
    unsigned char lvIsStructField : 1;          // Is this local var a field of a promoted struct local?
    unsigned char lvContainsFloatingFields : 1; // Does this struct contains floating point fields?
    unsigned char lvOverlappingFields : 1;      // True when we have a struct with possibly overlapping fields
    unsigned char lvContainsHoles : 1;          // True when we have a promoted struct that contains holes
    unsigned char lvCustomLayout : 1;           // True when this struct has "CustomLayout"

    unsigned char lvIsMultiRegArg : 1; // true if this is a multireg LclVar struct used in an argument context
    unsigned char lvIsMultiRegRet : 1; // true if this is a multireg LclVar struct assigned from a multireg call

#ifdef FEATURE_HFA
    unsigned char _lvIsHfa : 1;          // Is this a struct variable who's class handle is an HFA type
    unsigned char _lvIsHfaRegArg : 1;    // Is this a HFA argument variable?    // TODO-CLEANUP: Remove this and replace
                                         // with (lvIsRegArg && lvIsHfa())
    unsigned char _lvHfaTypeIsFloat : 1; // Is the HFA type float or double?
#endif                                   // FEATURE_HFA

#ifdef DEBUG
    // TODO-Cleanup: See the note on lvSize() - this flag is only in use by asserts that are checking for struct
    // types, and is needed because of cases where TYP_STRUCT is bashed to an integral type.
    // Consider cleaning this up so this workaround is not required.
    unsigned char lvUnusedStruct : 1; // All references to this promoted struct are through its field locals.
                                      // I.e. there is no longer any reference to the struct directly.
                                      // In this case we can simply remove this struct local.
#endif
#ifndef LEGACY_BACKEND
    unsigned char lvLRACandidate : 1; // Tracked for linear scan register allocation purposes
#endif                                // !LEGACY_BACKEND

#ifdef FEATURE_SIMD
    // Note that both SIMD vector args and locals are marked as lvSIMDType = true, but the
    // type of an arg node is TYP_BYREF and a local node is TYP_SIMD*.
    unsigned char lvSIMDType : 1;            // This is a SIMD struct
    unsigned char lvUsedInSIMDIntrinsic : 1; // This tells lclvar is used for simd intrinsic
    var_types     lvBaseType : 5;            // Note: this only packs because var_types is a typedef of unsigned char
#endif                                       // FEATURE_SIMD
    unsigned char lvRegStruct : 1;           // This is a reg-sized non-field-addressed struct.

    unsigned char lvClassIsExact : 1; // lvClassHandle is the exact type

#ifdef DEBUG
    unsigned char lvClassInfoUpdated : 1; // true if this var has updated class handle or exactness
#endif

    union {
        unsigned lvFieldLclStart; // The index of the local var representing the first field in the promoted struct
                                  // local.  For implicit byref parameters, this gets hijacked between
                                  // fgRetypeImplicitByRefArgs and fgMarkDemotedImplicitByRefArgs to point to the
                                  // struct local created to model the parameter's struct promotion, if any.
        unsigned lvParentLcl; // The index of the local var representing the parent (i.e. the promoted struct local).
                              // Valid on promoted struct local fields.
    };

    unsigned char lvFieldCnt; //  Number of fields in the promoted VarDsc.
    unsigned char lvFldOffset;
    unsigned char lvFldOrdinal;

#if FEATURE_MULTIREG_ARGS
    regNumber lvRegNumForSlot(unsigned slotNum)
    {
        if (slotNum == 0)
        {
            return lvArgReg;
        }
        else if (slotNum == 1)
        {
            return lvOtherArgReg;
        }
        else
        {
            assert(false && "Invalid slotNum!");
        }

        unreached();
    }
#endif // FEATURE_MULTIREG_ARGS

    bool lvIsHfa() const
    {
#ifdef FEATURE_HFA
        return _lvIsHfa;
#else
        return false;
#endif
    }

    void lvSetIsHfa()
    {
#ifdef FEATURE_HFA
        _lvIsHfa = true;
#endif
    }

    bool lvIsHfaRegArg() const
    {
#ifdef FEATURE_HFA
        return _lvIsHfaRegArg;
#else
        return false;
#endif
    }

    void lvSetIsHfaRegArg(bool value = true)
    {
#ifdef FEATURE_HFA
        _lvIsHfaRegArg = value;
#endif
    }

    bool lvHfaTypeIsFloat() const
    {
#ifdef FEATURE_HFA
        return _lvHfaTypeIsFloat;
#else
        return false;
#endif
    }

    void lvSetHfaTypeIsFloat(bool value)
    {
#ifdef FEATURE_HFA
        _lvHfaTypeIsFloat = value;
#endif
    }

    // on Arm64 - Returns 1-4 indicating the number of register slots used by the HFA
    // on Arm32 - Returns the total number of single FP register slots used by the HFA, max is 8
    //
    unsigned lvHfaSlots() const
    {
        assert(lvIsHfa());
        assert(lvType == TYP_STRUCT);
#ifdef _TARGET_ARM_
        return lvExactSize / sizeof(float);
#else  //  _TARGET_ARM64_
        if (lvHfaTypeIsFloat())
        {
            return lvExactSize / sizeof(float);
        }
        else
        {
            return lvExactSize / sizeof(double);
        }
#endif //  _TARGET_ARM64_
    }

    // lvIsMultiRegArgOrRet()
    //     returns true if this is a multireg LclVar struct used in an argument context
    //               or if this is a multireg LclVar struct assigned from a multireg call
    bool lvIsMultiRegArgOrRet()
    {
        return lvIsMultiRegArg || lvIsMultiRegRet;
    }

private:
    regNumberSmall _lvRegNum; // Used to store the register this variable is in (or, the low register of a
                              // register pair). For LEGACY_BACKEND, this is only set if lvRegister is
                              // non-zero. For non-LEGACY_BACKEND, it is set during codegen any time the
                              // variable is enregistered (in non-LEGACY_BACKEND, lvRegister is only set
                              // to non-zero if the variable gets the same register assignment for its entire
                              // lifetime).
#if !defined(_TARGET_64BIT_)
    regNumberSmall _lvOtherReg; // Used for "upper half" of long var.
#endif                          // !defined(_TARGET_64BIT_)

    regNumberSmall _lvArgReg; // The register in which this argument is passed.

#if FEATURE_MULTIREG_ARGS
    regNumberSmall _lvOtherArgReg; // Used for the second part of the struct passed in a register.
                                   // Note this is defined but not used by ARM32
#endif                             // FEATURE_MULTIREG_ARGS

#ifndef LEGACY_BACKEND
    union {
        regNumberSmall _lvArgInitReg;     // the register      into which the argument is moved at entry
        regPairNoSmall _lvArgInitRegPair; // the register pair into which the argument is moved at entry
    };
#endif // !LEGACY_BACKEND

public:
    // The register number is stored in a small format (8 bits), but the getters return and the setters take
    // a full-size (unsigned) format, to localize the casts here.

    /////////////////////

    __declspec(property(get = GetRegNum, put = SetRegNum)) regNumber lvRegNum;

    regNumber GetRegNum() const
    {
        return (regNumber)_lvRegNum;
    }

    void SetRegNum(regNumber reg)
    {
        _lvRegNum = (regNumberSmall)reg;
        assert(_lvRegNum == reg);
    }

/////////////////////

#if defined(_TARGET_64BIT_)
    __declspec(property(get = GetOtherReg, put = SetOtherReg)) regNumber lvOtherReg;

    regNumber GetOtherReg() const
    {
        assert(!"shouldn't get here"); // can't use "unreached();" because it's NORETURN, which causes C4072
                                       // "unreachable code" warnings
        return REG_NA;
    }

    void SetOtherReg(regNumber reg)
    {
        assert(!"shouldn't get here"); // can't use "unreached();" because it's NORETURN, which causes C4072
                                       // "unreachable code" warnings
    }
#else  // !_TARGET_64BIT_
    __declspec(property(get = GetOtherReg, put = SetOtherReg)) regNumber lvOtherReg;

    regNumber GetOtherReg() const
    {
        return (regNumber)_lvOtherReg;
    }

    void SetOtherReg(regNumber reg)
    {
        _lvOtherReg = (regNumberSmall)reg;
        assert(_lvOtherReg == reg);
    }
#endif // !_TARGET_64BIT_

    /////////////////////

    __declspec(property(get = GetArgReg, put = SetArgReg)) regNumber lvArgReg;

    regNumber GetArgReg() const
    {
        return (regNumber)_lvArgReg;
    }

    void SetArgReg(regNumber reg)
    {
        _lvArgReg = (regNumberSmall)reg;
        assert(_lvArgReg == reg);
    }

#if FEATURE_MULTIREG_ARGS
    __declspec(property(get = GetOtherArgReg, put = SetOtherArgReg)) regNumber lvOtherArgReg;

    regNumber GetOtherArgReg() const
    {
        return (regNumber)_lvOtherArgReg;
    }

    void SetOtherArgReg(regNumber reg)
    {
        _lvOtherArgReg = (regNumberSmall)reg;
        assert(_lvOtherArgReg == reg);
    }
#endif // FEATURE_MULTIREG_ARGS

#ifdef FEATURE_SIMD
    // Is this is a SIMD struct?
    bool lvIsSIMDType() const
    {
        return lvSIMDType;
    }

    // Is this is a SIMD struct which is used for SIMD intrinsic?
    bool lvIsUsedInSIMDIntrinsic() const
    {
        return lvUsedInSIMDIntrinsic;
    }
#else
    // If feature_simd not enabled, return false
    bool lvIsSIMDType() const
    {
        return false;
    }
    bool lvIsUsedInSIMDIntrinsic() const
    {
        return false;
    }
#endif

/////////////////////

#ifndef LEGACY_BACKEND
    __declspec(property(get = GetArgInitReg, put = SetArgInitReg)) regNumber lvArgInitReg;

    regNumber GetArgInitReg() const
    {
        return (regNumber)_lvArgInitReg;
    }

    void SetArgInitReg(regNumber reg)
    {
        _lvArgInitReg = (regNumberSmall)reg;
        assert(_lvArgInitReg == reg);
    }

    /////////////////////

    __declspec(property(get = GetArgInitRegPair, put = SetArgInitRegPair)) regPairNo lvArgInitRegPair;

    regPairNo GetArgInitRegPair() const
    {
        regPairNo regPair = (regPairNo)_lvArgInitRegPair;
        assert(regPair >= REG_PAIR_FIRST && regPair <= REG_PAIR_LAST);
        return regPair;
    }

    void SetArgInitRegPair(regPairNo regPair)
    {
        assert(regPair >= REG_PAIR_FIRST && regPair <= REG_PAIR_LAST);
        _lvArgInitRegPair = (regPairNoSmall)regPair;
        assert(_lvArgInitRegPair == regPair);
    }

    /////////////////////

    bool lvIsRegCandidate() const
    {
        return lvLRACandidate != 0;
    }

    bool lvIsInReg() const
    {
        return lvIsRegCandidate() && (lvRegNum != REG_STK);
    }

#else // LEGACY_BACKEND

    bool lvIsRegCandidate() const
    {
        return lvTracked != 0;
    }

    bool lvIsInReg() const
    {
        return lvRegister != 0;
    }

#endif // LEGACY_BACKEND

    regMaskTP lvRegMask() const
    {
        regMaskTP regMask = RBM_NONE;
        if (varTypeIsFloating(TypeGet()))
        {
            if (lvRegNum != REG_STK)
            {
                regMask = genRegMaskFloat(lvRegNum, TypeGet());
            }
        }
        else
        {
            if (lvRegNum != REG_STK)
            {
                regMask = genRegMask(lvRegNum);
            }

            // For longs we may have two regs
            if (isRegPairType(lvType) && lvOtherReg != REG_STK)
            {
                regMask |= genRegMask(lvOtherReg);
            }
        }
        return regMask;
    }

    regMaskSmall lvPrefReg; // set of regs it prefers to live in

    unsigned short lvVarIndex; // variable tracking index
    unsigned short lvRefCnt;   // unweighted (real) reference count.  For implicit by reference
                               // parameters, this gets hijacked from fgMarkImplicitByRefArgs
                               // through fgMarkDemotedImplicitByRefArgs, to provide a static
                               // appearance count (computed during address-exposed analysis)
                               // that fgMakeOutgoingStructArgCopy consults during global morph
                               // to determine if eliding its copy is legal.
    unsigned lvRefCntWtd;      // weighted reference count
    int      lvStkOffs;        // stack offset of home
    unsigned lvExactSize;      // (exact) size of the type in bytes

    // Is this a promoted struct?
    // This method returns true only for structs (including SIMD structs), not for
    // locals that are split on a 32-bit target.
    // It is only necessary to use this:
    //   1) if only structs are wanted, and
    //   2) if Lowering has already been done.
    // Otherwise lvPromoted is valid.
    bool lvPromotedStruct()
    {
#if !defined(_TARGET_64BIT_)
        return (lvPromoted && !varTypeIsLong(lvType));
#else  // defined(_TARGET_64BIT_)
        return lvPromoted;
#endif // defined(_TARGET_64BIT_)
    }

    unsigned lvSize() const // Size needed for storage representation. Only used for structs or TYP_BLK.
    {
        // TODO-Review: Sometimes we get called on ARM with HFA struct variables that have been promoted,
        // where the struct itself is no longer used because all access is via its member fields.
        // When that happens, the struct is marked as unused and its type has been changed to
        // TYP_INT (to keep the GC tracking code from looking at it).
        // See Compiler::raAssignVars() for details. For example:
        //      N002 (  4,  3) [00EA067C] -------------               return    struct $346
        //      N001 (  3,  2) [00EA0628] -------------                  lclVar    struct(U) V03 loc2
        //                                                                        float  V03.f1 (offs=0x00) -> V12 tmp7
        //                                                                        f8 (last use) (last use) $345
        // Here, the "struct(U)" shows that the "V03 loc2" variable is unused. Not shown is that V03
        // is now TYP_INT in the local variable table. It's not really unused, because it's in the tree.

        assert(varTypeIsStruct(lvType) || (lvType == TYP_BLK) || (lvPromoted && lvUnusedStruct));

#if defined(FEATURE_SIMD) && !defined(_TARGET_64BIT_)
        // For 32-bit architectures, we make local variable SIMD12 types 16 bytes instead of just 12. We can't do
        // this for arguments, which must be passed according the defined ABI. We don't want to do this for
        // dependently promoted struct fields, but we don't know that here. See lvaMapSimd12ToSimd16().
        if ((lvType == TYP_SIMD12) && !lvIsParam)
        {
            assert(lvExactSize == 12);
            return 16;
        }
#endif // defined(FEATURE_SIMD) && !defined(_TARGET_64BIT_)

        return (unsigned)(roundUp(lvExactSize, TARGET_POINTER_SIZE));
    }

    unsigned lvSlotNum; // original slot # (if remapped)

    typeInfo lvVerTypeInfo; // type info needed for verification

    CORINFO_CLASS_HANDLE lvClassHnd; // class handle for the local, or null if not known

    CORINFO_FIELD_HANDLE lvFieldHnd; // field handle for promoted struct fields

    BYTE* lvGcLayout; // GC layout info for structs

#if ASSERTION_PROP
    BlockSet   lvRefBlks;          // Set of blocks that contain refs
    GenTreePtr lvDefStmt;          // Pointer to the statement with the single definition
    void       lvaDisqualifyVar(); // Call to disqualify a local variable from use in optAddCopies
#endif
    var_types TypeGet() const
    {
        return (var_types)lvType;
    }
    bool lvStackAligned() const
    {
        assert(lvIsStructField);
        return ((lvFldOffset % sizeof(void*)) == 0);
    }
    bool lvNormalizeOnLoad() const
    {
        return varTypeIsSmall(TypeGet()) &&
               // lvIsStructField is treated the same as the aliased local, see fgDoNormalizeOnStore.
               (lvIsParam || lvAddrExposed || lvIsStructField);
    }

    bool lvNormalizeOnStore()
    {
        return varTypeIsSmall(TypeGet()) &&
               // lvIsStructField is treated the same as the aliased local, see fgDoNormalizeOnStore.
               !(lvIsParam || lvAddrExposed || lvIsStructField);
    }

    void lvaResetSortAgainFlag(Compiler* pComp);
    void decRefCnts(BasicBlock::weight_t weight, Compiler* pComp, bool propagate = true);
    void incRefCnts(BasicBlock::weight_t weight, Compiler* pComp, bool propagate = true);
    void setPrefReg(regNumber regNum, Compiler* pComp);
    void addPrefReg(regMaskTP regMask, Compiler* pComp);
    bool IsFloatRegType() const
    {
        return isFloatRegType(lvType) || lvIsHfaRegArg();
    }
    var_types GetHfaType() const
    {
        return lvIsHfa() ? (lvHfaTypeIsFloat() ? TYP_FLOAT : TYP_DOUBLE) : TYP_UNDEF;
    }
    void SetHfaType(var_types type)
    {
        assert(varTypeIsFloating(type));
        lvSetHfaTypeIsFloat(type == TYP_FLOAT);
    }

#ifndef LEGACY_BACKEND
    var_types lvaArgType();
#endif

    PerSsaArray lvPerSsaData;

#ifdef DEBUG
    // Keep track of the # of SsaNames, for a bounds check.
    unsigned lvNumSsaNames;
#endif

    // Returns the address of the per-Ssa data for the given ssaNum (which is required
    // not to be the SsaConfig::RESERVED_SSA_NUM, which indicates that the variable is
    // not an SSA variable).
    LclSsaVarDsc* GetPerSsaData(unsigned ssaNum)
    {
        assert(ssaNum != SsaConfig::RESERVED_SSA_NUM);
        assert(SsaConfig::RESERVED_SSA_NUM == 0);
        unsigned zeroBased = ssaNum - SsaConfig::UNINIT_SSA_NUM;
        assert(zeroBased < lvNumSsaNames);
        return &lvPerSsaData.GetRef(zeroBased);
    }

#ifdef DEBUG
public:
    void PrintVarReg() const
    {
        if (isRegPairType(TypeGet()))
        {
            printf("%s:%s", getRegName(lvOtherReg), // hi32
                   getRegName(lvRegNum));           // lo32
        }
        else
        {
            printf("%s", getRegName(lvRegNum));
        }
    }
#endif // DEBUG

}; // class LclVarDsc
