// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                          GenTree                                          XX
XX                                                                           XX
XX  This is the node in the semantic tree graph. It represents the operation XX
XX  corresponding to the node, and other information during code-gen.        XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************/
#ifndef _GENTREE_H_
#define _GENTREE_H_
/*****************************************************************************/

#include "vartype.h"   // For "var_types"
#include "target.h"    // For "regNumber"
#include "ssaconfig.h" // For "SsaConfig::RESERVED_SSA_NUM"
#include "reglist.h"
#include "valuenumtype.h"
#include "simplerhash.h"
#include "nodeinfo.h"
#include "simd.h"

// Debugging GenTree is much easier if we add a magic virtual function to make the debugger able to figure out what type
// it's got. This is enabled by default in DEBUG. To enable it in RET builds (temporarily!), you need to change the
// build to define DEBUGGABLE_GENTREE=1, as well as pass /OPT:NOICF to the linker (or else all the vtables get merged,
// making the debugging value supplied by them useless). See protojit.nativeproj for a commented example of setting the
// build flags correctly.
#ifndef DEBUGGABLE_GENTREE
#ifdef DEBUG
#define DEBUGGABLE_GENTREE 1
#else // !DEBUG
#define DEBUGGABLE_GENTREE 0
#endif // !DEBUG
#endif // !DEBUGGABLE_GENTREE

// The SpecialCodeKind enum is used to indicate the type of special (unique)
// target block that will be targeted by an instruction.
// These are used by:
//   GenTreeBoundsChk nodes (SCK_RNGCHK_FAIL, SCK_ARG_EXCPN, SCK_ARG_RNG_EXCPN)
//     - these nodes have a field (gtThrowKind) to indicate which kind
//   GenTreeOps nodes, for which codegen will generate the branch
//     - it will use the appropriate kind based on the opcode, though it's not
//       clear why SCK_OVERFLOW == SCK_ARITH_EXCPN
// SCK_PAUSE_EXEC is not currently used.
//
enum SpecialCodeKind
{
    SCK_NONE,
    SCK_RNGCHK_FAIL,                // target when range check fails
    SCK_PAUSE_EXEC,                 // target to stop (e.g. to allow GC)
    SCK_DIV_BY_ZERO,                // target for divide by zero (Not used on X86/X64)
    SCK_ARITH_EXCPN,                // target on arithmetic exception
    SCK_OVERFLOW = SCK_ARITH_EXCPN, // target on overflow
    SCK_ARG_EXCPN,                  // target on ArgumentException (currently used only for SIMD intrinsics)
    SCK_ARG_RNG_EXCPN,              // target on ArgumentOutOfRangeException (currently used only for SIMD intrinsics)
    SCK_COUNT
};

/*****************************************************************************/

DECLARE_TYPED_ENUM(genTreeOps, BYTE)
{
#define GTNODE(en, sn, st, cm, ok) GT_##en,
#include "gtlist.h"

    GT_COUNT,

#ifdef _TARGET_64BIT_
        // GT_CNS_NATIVELONG is the gtOper symbol for GT_CNS_LNG or GT_CNS_INT, depending on the target.
        // For the 64-bit targets we will only use GT_CNS_INT as it used to represent all the possible sizes
        GT_CNS_NATIVELONG = GT_CNS_INT,
#else
        // For the 32-bit targets we use a GT_CNS_LNG to hold a 64-bit integer constant and GT_CNS_INT for all others.
        // In the future when we retarget the JIT for x86 we should consider eliminating GT_CNS_LNG
        GT_CNS_NATIVELONG = GT_CNS_LNG,
#endif
}
END_DECLARE_TYPED_ENUM(genTreeOps, BYTE)

/*****************************************************************************
 *
 *  The following enum defines a set of bit flags that can be used
 *  to classify expression tree nodes. Note that some operators will
 *  have more than one bit set, as follows:
 *
 *          GTK_CONST    implies    GTK_LEAF
 *          GTK_RELOP    implies    GTK_BINOP
 *          GTK_LOGOP    implies    GTK_BINOP
 */

enum genTreeKinds
{
    GTK_SPECIAL = 0x0000, // unclassified operator (special handling reqd)

    GTK_CONST = 0x0001, // constant     operator
    GTK_LEAF  = 0x0002, // leaf         operator
    GTK_UNOP  = 0x0004, // unary        operator
    GTK_BINOP = 0x0008, // binary       operator
    GTK_RELOP = 0x0010, // comparison   operator
    GTK_LOGOP = 0x0020, // logical      operator
    GTK_ASGOP = 0x0040, // assignment   operator

    GTK_KINDMASK = 0x007F, // operator kind mask

    GTK_COMMUTE = 0x0080, // commutative  operator

    GTK_EXOP = 0x0100, // Indicates that an oper for a node type that extends GenTreeOp (or GenTreeUnOp)
                       // by adding non-node fields to unary or binary operator.

    GTK_LOCAL = 0x0200, // is a local access (load, store, phi)

    GTK_NOVALUE = 0x0400, // node does not produce a value
    GTK_NOTLIR  = 0x0800, // node is not allowed in LIR

    GTK_NOCONTAIN = 0x1000, // this node is a value, but may not be contained

    /* Define composite value(s) */

    GTK_SMPOP = (GTK_UNOP | GTK_BINOP | GTK_RELOP | GTK_LOGOP)
};

/*****************************************************************************/

#define SMALL_TREE_NODES 1

/*****************************************************************************/

DECLARE_TYPED_ENUM(gtCallTypes, BYTE)
{
    CT_USER_FUNC,    // User function
        CT_HELPER,   // Jit-helper
        CT_INDIRECT, // Indirect call

        CT_COUNT // fake entry (must be last)
}
END_DECLARE_TYPED_ENUM(gtCallTypes, BYTE)

/*****************************************************************************/

struct BasicBlock;

struct InlineCandidateInfo;

typedef unsigned short AssertionIndex;

static const AssertionIndex NO_ASSERTION_INDEX = 0;

class AssertionInfo
{
    // true if the assertion holds on the bbNext edge instead of the bbJumpDest edge (for GT_JTRUE nodes)
    unsigned short m_isNextEdgeAssertion : 1;
    // 1-based index of the assertion
    unsigned short m_assertionIndex : 15;

    AssertionInfo(bool isNextEdgeAssertion, AssertionIndex assertionIndex)
        : m_isNextEdgeAssertion(isNextEdgeAssertion), m_assertionIndex(assertionIndex)
    {
        assert(m_assertionIndex == assertionIndex);
    }

public:
    AssertionInfo() : AssertionInfo(false, 0)
    {
    }

    AssertionInfo(AssertionIndex assertionIndex) : AssertionInfo(false, assertionIndex)
    {
    }

    static AssertionInfo ForNextEdge(AssertionIndex assertionIndex)
    {
        // Ignore the edge information if there's no assertion
        bool isNextEdge = (assertionIndex != NO_ASSERTION_INDEX);
        return AssertionInfo(isNextEdge, assertionIndex);
    }

    void Clear()
    {
        m_isNextEdgeAssertion = 0;
        m_assertionIndex      = NO_ASSERTION_INDEX;
    }

    bool HasAssertion() const
    {
        return m_assertionIndex != NO_ASSERTION_INDEX;
    }

    AssertionIndex GetAssertionIndex() const
    {
        return m_assertionIndex;
    }

    bool IsNextEdgeAssertion() const
    {
        return m_isNextEdgeAssertion;
    }
};

/*****************************************************************************/

// GT_FIELD nodes will be lowered into more "code-gen-able" representations, like
// GT_IND's of addresses, or GT_LCL_FLD nodes.  We'd like to preserve the more abstract
// information, and will therefore annotate such lowered nodes with FieldSeq's.  A FieldSeq
// represents a (possibly) empty sequence of fields.  The fields are in the order
// in which they are dereferenced.  The first field may be an object field or a struct field;
// all subsequent fields must be struct fields.
struct FieldSeqNode
{
    CORINFO_FIELD_HANDLE m_fieldHnd;
    FieldSeqNode*        m_next;

    FieldSeqNode(CORINFO_FIELD_HANDLE fieldHnd, FieldSeqNode* next) : m_fieldHnd(fieldHnd), m_next(next)
    {
    }

    // returns true when this is the pseudo #FirstElem field sequence
    bool IsFirstElemFieldSeq();

    // returns true when this is the pseudo #ConstantIndex field sequence
    bool IsConstantIndexFieldSeq();

    // returns true when this is the the pseudo #FirstElem field sequence or the pseudo #ConstantIndex field sequence
    bool IsPseudoField();

    // Make sure this provides methods that allow it to be used as a KeyFuncs type in SimplerHash.
    static int GetHashCode(FieldSeqNode fsn)
    {
        return static_cast<int>(reinterpret_cast<intptr_t>(fsn.m_fieldHnd)) ^
               static_cast<int>(reinterpret_cast<intptr_t>(fsn.m_next));
    }

    static bool Equals(FieldSeqNode fsn1, FieldSeqNode fsn2)
    {
        return fsn1.m_fieldHnd == fsn2.m_fieldHnd && fsn1.m_next == fsn2.m_next;
    }
};

// This class canonicalizes field sequences.
class FieldSeqStore
{
    typedef SimplerHashTable<FieldSeqNode, /*KeyFuncs*/ FieldSeqNode, FieldSeqNode*, JitSimplerHashBehavior>
        FieldSeqNodeCanonMap;

    IAllocator*           m_alloc;
    FieldSeqNodeCanonMap* m_canonMap;

    static FieldSeqNode s_notAField; // No value, just exists to provide an address.

    // Dummy variables to provide the addresses for the "pseudo field handle" statics below.
    static int FirstElemPseudoFieldStruct;
    static int ConstantIndexPseudoFieldStruct;

public:
    FieldSeqStore(IAllocator* alloc);

    // Returns the (canonical in the store) singleton field sequence for the given handle.
    FieldSeqNode* CreateSingleton(CORINFO_FIELD_HANDLE fieldHnd);

    // This is a special distinguished FieldSeqNode indicating that a constant does *not*
    // represent a valid field sequence.  This is "infectious", in the sense that appending it
    // (on either side) to any field sequence yields the "NotAField()" sequence.
    static FieldSeqNode* NotAField()
    {
        return &s_notAField;
    }

    // Returns the (canonical in the store) field sequence representing the concatenation of
    // the sequences represented by "a" and "b".  Assumes that "a" and "b" are canonical; that is,
    // they are the results of CreateSingleton, NotAField, or Append calls.  If either of the arguments
    // are the "NotAField" value, so is the result.
    FieldSeqNode* Append(FieldSeqNode* a, FieldSeqNode* b);

    // We have a few "pseudo" field handles:

    // This treats the constant offset of the first element of something as if it were a field.
    // Works for method table offsets of boxed structs, or first elem offset of arrays/strings.
    static CORINFO_FIELD_HANDLE FirstElemPseudoField;

    // If there is a constant index, we make a psuedo field to correspond to the constant added to
    // offset of the indexed field.  This keeps the field sequence structure "normalized", especially in the
    // case where the element type is a struct, so we might add a further struct field offset.
    static CORINFO_FIELD_HANDLE ConstantIndexPseudoField;

    static bool IsPseudoField(CORINFO_FIELD_HANDLE hnd)
    {
        return hnd == FirstElemPseudoField || hnd == ConstantIndexPseudoField;
    }
};

class GenTreeUseEdgeIterator;
class GenTreeOperandIterator;

/*****************************************************************************/

typedef struct GenTree* GenTreePtr;
struct GenTreeArgList;

// Forward declarations of the subtypes
#define GTSTRUCT_0(fn, en) struct GenTree##fn;
#define GTSTRUCT_1(fn, en) struct GenTree##fn;
#define GTSTRUCT_2(fn, en, en2) struct GenTree##fn;
#define GTSTRUCT_3(fn, en, en2, en3) struct GenTree##fn;
#define GTSTRUCT_4(fn, en, en2, en3, en4) struct GenTree##fn;
#define GTSTRUCT_N(fn, ...) struct GenTree##fn;
#include "gtstructs.h"

/*****************************************************************************/

#ifndef _HOST_64BIT_
#include <pshpack4.h>
#endif

struct GenTree
{
// We use GT_STRUCT_0 only for the category of simple ops.
#define GTSTRUCT_0(fn, en)                                                                                             \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        assert(this->OperIsSimple());                                                                                  \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;
#define GTSTRUCT_1(fn, en)                                                                                             \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        assert(this->gtOper == en);                                                                                    \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;
#define GTSTRUCT_2(fn, en, en2)                                                                                        \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        assert(this->gtOper == en || this->gtOper == en2);                                                             \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;
#define GTSTRUCT_3(fn, en, en2, en3)                                                                                   \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        assert(this->gtOper == en || this->gtOper == en2 || this->gtOper == en3);                                      \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;

#define GTSTRUCT_4(fn, en, en2, en3, en4)                                                                              \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        assert(this->gtOper == en || this->gtOper == en2 || this->gtOper == en3 || this->gtOper == en4);               \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;

#ifdef DEBUG
// VC does not optimize out this loop in retail even though the value it computes is unused
// so we need a separate version for non-debug
#define GTSTRUCT_N(fn, ...)                                                                                            \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        genTreeOps validOps[] = {__VA_ARGS__};                                                                         \
        bool       found      = false;                                                                                 \
        for (unsigned i = 0; i < ArrLen(validOps); i++)                                                                \
        {                                                                                                              \
            if (this->gtOper == validOps[i])                                                                           \
            {                                                                                                          \
                found = true;                                                                                          \
                break;                                                                                                 \
            }                                                                                                          \
        }                                                                                                              \
        assert(found);                                                                                                 \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;
#else
#define GTSTRUCT_N(fn, ...)                                                                                            \
    GenTree##fn* As##fn()                                                                                              \
    {                                                                                                                  \
        return reinterpret_cast<GenTree##fn*>(this);                                                                   \
    }                                                                                                                  \
    GenTree##fn& As##fn##Ref()                                                                                         \
    {                                                                                                                  \
        return *As##fn();                                                                                              \
    }                                                                                                                  \
    __declspec(property(get = As##fn##Ref)) GenTree##fn& gt##fn;
#endif

#include "gtstructs.h"

    genTreeOps gtOper; // enum subtype BYTE
    var_types  gtType; // enum subtype BYTE

    genTreeOps OperGet() const
    {
        return gtOper;
    }
    var_types TypeGet() const
    {
        return gtType;
    }

#ifdef DEBUG
    genTreeOps gtOperSave; // Only used to save gtOper when we destroy a node, to aid debugging.
#endif

#if FEATURE_ANYCSE

#define NO_CSE (0)

#define IS_CSE_INDEX(x) (x != 0)
#define IS_CSE_USE(x) (x > 0)
#define IS_CSE_DEF(x) (x < 0)
#define GET_CSE_INDEX(x) ((x > 0) ? x : -x)
#define TO_CSE_DEF(x) (-x)

    signed char gtCSEnum; // 0 or the CSE index (negated if def)
                          // valid only for CSE expressions

#endif // FEATURE_ANYCSE

    unsigned char gtLIRFlags; // Used for nodes that are in LIR. See LIR::Flags in lir.h for the various flags.

#if ASSERTION_PROP
    AssertionInfo gtAssertionInfo; // valid only for non-GT_STMT nodes

    bool GeneratesAssertion() const
    {
        return gtAssertionInfo.HasAssertion();
    }

    void ClearAssertion()
    {
        gtAssertionInfo.Clear();
    }

    AssertionInfo GetAssertionInfo() const
    {
        return gtAssertionInfo;
    }

    void SetAssertionInfo(AssertionInfo info)
    {
        gtAssertionInfo = info;
    }
#endif

#if FEATURE_STACK_FP_X87
    unsigned char gtFPlvl; // x87 stack depth at this node
    void gtCopyFPlvl(GenTree* other)
    {
        gtFPlvl = other->gtFPlvl;
    }
    void gtSetFPlvl(unsigned level)
    {
        noway_assert(FitsIn<unsigned char>(level));
        gtFPlvl = (unsigned char)level;
    }
#else // FEATURE_STACK_FP_X87

    void gtCopyFPlvl(GenTree* other)
    {
    }
    void gtSetFPlvl(unsigned level)
    {
    }

#endif // FEATURE_STACK_FP_X87

    //
    // Cost metrics on the node. Don't allow direct access to the variable for setting.
    //

public:
#ifdef DEBUG
    // You are not allowed to read the cost values before they have been set in gtSetEvalOrder().
    // Keep track of whether the costs have been initialized, and assert if they are read before being initialized.
    // Obviously, this information does need to be initialized when a node is created.
    // This is public so the dumpers can see it.

    bool gtCostsInitialized;
#endif // DEBUG

#define MAX_COST UCHAR_MAX
#define IND_COST_EX 3 // execution cost for an indirection

    __declspec(property(get = GetCostEx)) unsigned char gtCostEx; // estimate of expression execution cost

    __declspec(property(get = GetCostSz)) unsigned char gtCostSz; // estimate of expression code size cost

    unsigned char GetCostEx() const
    {
        assert(gtCostsInitialized);
        return _gtCostEx;
    }
    unsigned char GetCostSz() const
    {
        assert(gtCostsInitialized);
        return _gtCostSz;
    }

    // Set the costs. They are always both set at the same time.
    // Don't use the "put" property: force calling this function, to make it more obvious in the few places
    // that set the values.
    // Note that costs are only set in gtSetEvalOrder() and its callees.
    void SetCosts(unsigned costEx, unsigned costSz)
    {
        assert(costEx != (unsigned)-1); // looks bogus
        assert(costSz != (unsigned)-1); // looks bogus
        INDEBUG(gtCostsInitialized = true;)

        _gtCostEx = (costEx > MAX_COST) ? MAX_COST : (unsigned char)costEx;
        _gtCostSz = (costSz > MAX_COST) ? MAX_COST : (unsigned char)costSz;
    }

    // Opimized copy function, to avoid the SetCosts() function comparisons, and make it more clear that a node copy is
    // happening.
    void CopyCosts(const GenTree* const tree)
    {
        INDEBUG(gtCostsInitialized =
                    tree->gtCostsInitialized;) // If the 'tree' costs aren't initialized, we'll hit an assert below.
        _gtCostEx = tree->gtCostEx;
        _gtCostSz = tree->gtCostSz;
    }

    // Same as CopyCosts, but avoids asserts if the costs we are copying have not been initialized.
    // This is because the importer, for example, clones nodes, before these costs have been initialized.
    // Note that we directly access the 'tree' costs, not going through the accessor functions (either
    // directly or through the properties).
    void CopyRawCosts(const GenTree* const tree)
    {
        INDEBUG(gtCostsInitialized = tree->gtCostsInitialized;)
        _gtCostEx = tree->_gtCostEx;
        _gtCostSz = tree->_gtCostSz;
    }

private:
    unsigned char _gtCostEx; // estimate of expression execution cost
    unsigned char _gtCostSz; // estimate of expression code size cost

    //
    // Register or register pair number of the node.
    //
    CLANG_FORMAT_COMMENT_ANCHOR;

#ifdef DEBUG
public:
    enum genRegTag
    {
        GT_REGTAG_NONE, // Nothing has been assigned to _gtRegNum/_gtRegPair
        GT_REGTAG_REG,  // _gtRegNum  has been assigned
#if CPU_LONG_USES_REGPAIR
        GT_REGTAG_REGPAIR // _gtRegPair has been assigned
#endif
    };
    genRegTag GetRegTag() const
    {
#if CPU_LONG_USES_REGPAIR
        assert(gtRegTag == GT_REGTAG_NONE || gtRegTag == GT_REGTAG_REG || gtRegTag == GT_REGTAG_REGPAIR);
#else
        assert(gtRegTag == GT_REGTAG_NONE || gtRegTag == GT_REGTAG_REG);
#endif
        return gtRegTag;
    }

private:
    genRegTag gtRegTag; // What is in _gtRegNum/_gtRegPair?
#endif                  // DEBUG

private:
    union {
        // NOTE: After LSRA, one of these values may be valid even if GTF_REG_VAL is not set in gtFlags.
        // They store the register assigned to the node. If a register is not assigned, _gtRegNum is set to REG_NA
        // or _gtRegPair is set to REG_PAIR_NONE, depending on the node type.
        regNumberSmall _gtRegNum;  // which register      the value is in
        regPairNoSmall _gtRegPair; // which register pair the value is in
    };

public:
    // The register number is stored in a small format (8 bits), but the getters return and the setters take
    // a full-size (unsigned) format, to localize the casts here.

    __declspec(property(get = GetRegNum, put = SetRegNum)) regNumber gtRegNum;

    bool canBeContained() const;

    // for codegen purposes, is this node a subnode of its parent
    bool isContained() const;

    bool isContainedIndir() const;

    bool isIndirAddrMode();

    bool isIndir() const;

    bool isContainedIntOrIImmed() const
    {
        return isContained() && IsCnsIntOrI() && !isUsedFromSpillTemp();
    }

    bool isContainedFltOrDblImmed() const
    {
        return isContained() && (OperGet() == GT_CNS_DBL);
    }

    bool isLclField() const
    {
        return OperGet() == GT_LCL_FLD || OperGet() == GT_STORE_LCL_FLD;
    }

    bool isUsedFromSpillTemp() const;

    // Indicates whether it is a memory op.
    // Right now it includes Indir and LclField ops.
    bool isMemoryOp() const
    {
        return isIndir() || isLclField();
    }

    bool isUsedFromMemory() const
    {
        return ((isContained() && (isMemoryOp() || (OperGet() == GT_LCL_VAR) || (OperGet() == GT_CNS_DBL))) ||
                isUsedFromSpillTemp());
    }

    bool isLclVarUsedFromMemory() const
    {
        return (OperGet() == GT_LCL_VAR) && (isContained() || isUsedFromSpillTemp());
    }

    bool isLclFldUsedFromMemory() const
    {
        return isLclField() && (isContained() || isUsedFromSpillTemp());
    }

    bool isUsedFromReg() const
    {
        return !isContained() && !isUsedFromSpillTemp();
    }

    regNumber GetRegNum() const
    {
        assert((gtRegTag == GT_REGTAG_REG) || (gtRegTag == GT_REGTAG_NONE)); // TODO-Cleanup: get rid of the NONE case,
                                                                             // and fix everyplace that reads undefined
                                                                             // values
        regNumber reg = (regNumber)_gtRegNum;
        assert((gtRegTag == GT_REGTAG_NONE) || // TODO-Cleanup: get rid of the NONE case, and fix everyplace that reads
                                               // undefined values
               (reg >= REG_FIRST && reg <= REG_COUNT));
        return reg;
    }

    void SetRegNum(regNumber reg)
    {
        assert(reg >= REG_FIRST && reg <= REG_COUNT);
        // Make sure the upper bits of _gtRegPair are clear
        _gtRegPair = (regPairNoSmall)0;
        _gtRegNum  = (regNumberSmall)reg;
        INDEBUG(gtRegTag = GT_REGTAG_REG;)
        assert(_gtRegNum == reg);
    }

#if CPU_LONG_USES_REGPAIR
    __declspec(property(get = GetRegPair, put = SetRegPair)) regPairNo gtRegPair;

    regPairNo GetRegPair() const
    {
        assert((gtRegTag == GT_REGTAG_REGPAIR) || (gtRegTag == GT_REGTAG_NONE)); // TODO-Cleanup: get rid of the NONE
                                                                                 // case, and fix everyplace that reads
                                                                                 // undefined values
        regPairNo regPair = (regPairNo)_gtRegPair;
        assert((gtRegTag == GT_REGTAG_NONE) || // TODO-Cleanup: get rid of the NONE case, and fix everyplace that reads
                                               // undefined values
               (regPair >= REG_PAIR_FIRST && regPair <= REG_PAIR_LAST) ||
               (regPair == REG_PAIR_NONE)); // allow initializing to an undefined value
        return regPair;
    }

    void SetRegPair(regPairNo regPair)
    {
        assert((regPair >= REG_PAIR_FIRST && regPair <= REG_PAIR_LAST) ||
               (regPair == REG_PAIR_NONE)); // allow initializing to an undefined value
        _gtRegPair = (regPairNoSmall)regPair;
        INDEBUG(gtRegTag = GT_REGTAG_REGPAIR;)
        assert(_gtRegPair == regPair);
    }
#endif

    // Copy the _gtRegNum/_gtRegPair/gtRegTag fields
    void CopyReg(GenTreePtr from);

    void gtClearReg(Compiler* compiler);

    bool gtHasReg() const;

    regMaskTP gtGetRegMask() const;

    unsigned gtFlags; // see GTF_xxxx below

#if defined(DEBUG)
    unsigned gtDebugFlags; // see GTF_DEBUG_xxx below
#endif                     // defined(DEBUG)

    ValueNumPair gtVNPair;

    regMaskSmall gtRsvdRegs; // set of fixed trashed  registers

#ifndef LEGACY_BACKEND
    unsigned AvailableTempRegCount(regMaskTP mask = (regMaskTP)-1) const;
    regNumber GetSingleTempReg(regMaskTP mask = (regMaskTP)-1);
    regNumber ExtractTempReg(regMaskTP mask = (regMaskTP)-1);
#endif // !LEGACY_BACKEND

#ifdef LEGACY_BACKEND
    regMaskSmall gtUsedRegs; // set of used (trashed) registers
#endif                       // LEGACY_BACKEND

#ifndef LEGACY_BACKEND
    TreeNodeInfo gtLsraInfo;
#endif // !LEGACY_BACKEND

    void SetVNsFromNode(GenTreePtr tree)
    {
        gtVNPair = tree->gtVNPair;
    }

    ValueNum GetVN(ValueNumKind vnk) const
    {
        if (vnk == VNK_Liberal)
        {
            return gtVNPair.GetLiberal();
        }
        else
        {
            assert(vnk == VNK_Conservative);
            return gtVNPair.GetConservative();
        }
    }
    void SetVN(ValueNumKind vnk, ValueNum vn)
    {
        if (vnk == VNK_Liberal)
        {
            return gtVNPair.SetLiberal(vn);
        }
        else
        {
            assert(vnk == VNK_Conservative);
            return gtVNPair.SetConservative(vn);
        }
    }
    void SetVNs(ValueNumPair vnp)
    {
        gtVNPair = vnp;
    }
    void ClearVN()
    {
        gtVNPair = ValueNumPair(); // Initializes both elements to "NoVN".
    }

// clang-format off

//---------------------------------------------------------------------
//
// GenTree flags stored in gtFlags.
//
//---------------------------------------------------------------------

//---------------------------------------------------------------------
//  The first set of flags can be used with a large set of nodes, and
//  thus they must all have distinct values. That is, one can test any
//  expression node for one of these flags.
//---------------------------------------------------------------------

#define GTF_ASG           0x00000001 // sub-expression contains an assignment
#define GTF_CALL          0x00000002 // sub-expression contains a  func. call
#define GTF_EXCEPT        0x00000004 // sub-expression might throw an exception
#define GTF_GLOB_REF      0x00000008 // sub-expression uses global variable(s)
#define GTF_ORDER_SIDEEFF 0x00000010 // sub-expression has a re-ordering side effect

// If you set these flags, make sure that code:gtExtractSideEffList knows how to find the tree,
// otherwise the C# (run csc /o-) code:
//     var v = side_eff_operation
// with no use of v will drop your tree on the floor.
#define GTF_PERSISTENT_SIDE_EFFECTS (GTF_ASG | GTF_CALL)
#define GTF_SIDE_EFFECT             (GTF_PERSISTENT_SIDE_EFFECTS | GTF_EXCEPT)
#define GTF_GLOB_EFFECT             (GTF_SIDE_EFFECT | GTF_GLOB_REF)
#define GTF_ALL_EFFECT              (GTF_GLOB_EFFECT | GTF_ORDER_SIDEEFF)

// The extra flag GTF_IS_IN_CSE is used to tell the consumer of these flags
// that we are calling in the context of performing a CSE, thus we
// should allow the run-once side effects of running a class constructor.
//
// The only requirement of this flag is that it not overlap any of the
// side-effect flags. The actual bit used is otherwise arbitrary.
#define GTF_IS_IN_CSE GTF_BOOLEAN
#define GTF_PERSISTENT_SIDE_EFFECTS_IN_CSE (GTF_ASG | GTF_CALL | GTF_IS_IN_CSE)

// Can any side-effects be observed externally, say by a caller method?
// For assignments, only assignments to global memory can be observed
// externally, whereas simple assignments to local variables can not.
//
// Be careful when using this inside a "try" protected region as the
// order of assignments to local variables would need to be preserved
// wrt side effects if the variables are alive on entry to the
// "catch/finally" region. In such cases, even assignments to locals
// will have to be restricted.
#define GTF_GLOBALLY_VISIBLE_SIDE_EFFECTS(flags)                                                                       \
    (((flags) & (GTF_CALL | GTF_EXCEPT)) || (((flags) & (GTF_ASG | GTF_GLOB_REF)) == (GTF_ASG | GTF_GLOB_REF)))

#define GTF_REVERSE_OPS 0x00000020 // operand op2 should be evaluated before op1 (normally, op1 is evaluated first and op2 is evaluated second)
#define GTF_REG_VAL     0x00000040 // operand is sitting in a register (or part of a TYP_LONG operand is sitting in a register)
#define GTF_SPILLED     0x00000080 // the value has been spilled

#ifdef LEGACY_BACKEND
#define GTF_SPILLED_OPER 0x00000100 // op1 has been spilled
#define GTF_SPILLED_OP2  0x00000200 // op2 has been spilled
#else  // !LEGACY_BACKEND
#define GTF_NOREG_AT_USE 0x00000100 // tree node is in memory at the point of use
#endif // !LEGACY_BACKEND

#define GTF_ZSF_SET     0x00000400 // the zero(ZF) and sign(SF) flags set to the operand
#define GTF_SET_FLAGS   0x00000800 // Requires that codegen for this node set the flags. Use gtSetFlags() to check this flag.

#define GTF_MAKE_CSE    0x00002000 // Hoisted expression: try hard to make this into CSE (see optPerformHoistExpr)
#define GTF_DONT_CSE    0x00004000 // Don't bother CSE'ing this expr
#define GTF_COLON_COND  0x00008000 // This node is conditionally executed (part of ? :)

#define GTF_NODE_MASK (GTF_COLON_COND)

#define GTF_BOOLEAN     0x00040000 // value is known to be 0/1
#if CPU_HAS_BYTE_REGS && defined(LEGACY_BACKEND)
#define GTF_SMALL_OK    0x00080000 // actual small int sufficient
#endif

#define GTF_UNSIGNED    0x00100000 // With GT_CAST:   the source operand is an unsigned type
                                   // With operators: the specified node is an unsigned operator
#define GTF_LATE_ARG    0x00200000 // The specified node is evaluated to a temp in the arg list, and this temp is added to gtCallLateArgs.
#define GTF_SPILL       0x00400000 // Needs to be spilled here

#define GTF_COMMON_MASK 0x007FFFFF // mask of all the flags above

#define GTF_REUSE_REG_VAL 0x00800000 // This is set by the register allocator on nodes whose value already exists in the
                                     // register assigned to this node, so the code generator does not have to generate
                                     // code to produce the value. It is currently used only on constant nodes.
                                     // It CANNOT be set on var (GT_LCL*) nodes, or on indir (GT_IND or GT_STOREIND) nodes, since
                                     // it is not needed for lclVars and is highly unlikely to be useful for indir nodes.

//---------------------------------------------------------------------
//  The following flags can be used only with a small set of nodes, and
//  thus their values need not be distinct (other than within the set
//  that goes with a particular node/nodes, of course). That is, one can
//  only test for one of these flags if the 'gtOper' value is tested as
//  well to make sure it's the right operator for the particular flag.
//---------------------------------------------------------------------

// NB: GTF_VAR_* and GTF_REG_* share the same namespace of flags, because
// GT_LCL_VAR nodes may be changed to GT_REG_VAR nodes without resetting
// the flags. These are also used by GT_LCL_FLD.
#define GTF_VAR_DEF         0x80000000 // GT_LCL_VAR -- this is a definition
#define GTF_VAR_USEASG      0x40000000 // GT_LCL_VAR -- this is a use/def for a x<op>=y
#define GTF_VAR_USEDEF      0x20000000 // GT_LCL_VAR -- this is a use/def as in x=x+y (only the lhs x is tagged)
#define GTF_VAR_CAST        0x10000000 // GT_LCL_VAR -- has been explictly cast (variable node may not be type of local)
#define GTF_VAR_ITERATOR    0x08000000 // GT_LCL_VAR -- this is a iterator reference in the loop condition
#define GTF_VAR_CLONED      0x01000000 // GT_LCL_VAR -- this node has been cloned or is a clone
                                       // Relevant for inlining optimizations (see fgInlinePrependStatements)

// TODO-Cleanup: Currently, GTF_REG_BIRTH is used only by stackfp
//         We should consider using it more generally for VAR_BIRTH, instead of
//         GTF_VAR_DEF && !GTF_VAR_USEASG
#define GTF_REG_BIRTH       0x04000000 // GT_REG_VAR -- enregistered variable born here
#define GTF_VAR_DEATH       0x02000000 // GT_LCL_VAR, GT_REG_VAR -- variable dies here (last use)

#define GTF_VAR_ARR_INDEX   0x00000020 // The variable is part of (the index portion of) an array index expression.
                                       // Shares a value with GTF_REVERSE_OPS, which is meaningless for local var.

#define GTF_LIVENESS_MASK (GTF_VAR_DEF | GTF_VAR_USEASG | GTF_VAR_USEDEF | GTF_REG_BIRTH | GTF_VAR_DEATH)

#define GTF_CALL_UNMANAGED          0x80000000 // GT_CALL -- direct call to unmanaged code
#define GTF_CALL_INLINE_CANDIDATE   0x40000000 // GT_CALL -- this call has been marked as an inline candidate

#define GTF_CALL_VIRT_KIND_MASK     0x30000000 // GT_CALL -- mask of the below call kinds
#define GTF_CALL_NONVIRT            0x00000000 // GT_CALL -- a non virtual call
#define GTF_CALL_VIRT_STUB          0x10000000 // GT_CALL -- a stub-dispatch virtual call
#define GTF_CALL_VIRT_VTABLE        0x20000000 // GT_CALL -- a  vtable-based virtual call

#define GTF_CALL_NULLCHECK          0x08000000 // GT_CALL -- must check instance pointer for null
#define GTF_CALL_POP_ARGS           0x04000000 // GT_CALL -- caller pop arguments?
#define GTF_CALL_HOISTABLE          0x02000000 // GT_CALL -- call is hoistable
#ifdef LEGACY_BACKEND
#define GTF_CALL_REG_SAVE           0x01000000 // GT_CALL -- This call preserves all integer regs
#endif // LEGACY_BACKEND
                                               // For additional flags for GT_CALL node see GTF_CALL_M_*

#define GTF_NOP_DEATH               0x40000000 // GT_NOP -- operand dies here

#define GTF_FLD_NULLCHECK           0x80000000 // GT_FIELD -- need to nullcheck the "this" pointer
#define GTF_FLD_VOLATILE            0x40000000 // GT_FIELD/GT_CLS_VAR -- same as GTF_IND_VOLATILE
#define GTF_FLD_INITCLASS           0x20000000 // GT_FIELD/GT_CLS_VAR -- field access requires preceding class/static init helper

#define GTF_INX_RNGCHK              0x80000000 // GT_INDEX -- the array reference should be range-checked.
#define GTF_INX_REFARR_LAYOUT       0x20000000 // GT_INDEX -- same as GTF_IND_REFARR_LAYOUT
#define GTF_INX_STRING_LAYOUT       0x40000000 // GT_INDEX -- this uses the special string array layout

#define GTF_IND_NONFAULTING      GTF_SET_FLAGS // GT_IND   -- An indir that cannot fault. GTF_SET_FLAGS is not used on indirs.
#define GTF_IND_VOLATILE            0x40000000 // GT_IND   -- the load or store must use volatile sematics (this is a nop on X86)
#define GTF_IND_REFARR_LAYOUT       0x20000000 // GT_IND   -- the array holds object refs (only affects layout of Arrays)
#define GTF_IND_TGTANYWHERE         0x10000000 // GT_IND   -- the target could be anywhere
#define GTF_IND_TLS_REF             0x08000000 // GT_IND   -- the target is accessed via TLS
#define GTF_IND_ASG_LHS             0x04000000 // GT_IND   -- this GT_IND node is (the effective val) of the LHS of an
                                               //             assignment; don't evaluate it independently.
#define GTF_IND_REQ_ADDR_IN_REG GTF_IND_ASG_LHS // GT_IND  -- requires its addr operand to be evaluated
                                               // into a register. This flag is useful in cases where it
                                               // is required to generate register indirect addressing mode.
                                               // One such case is virtual stub calls on xarch.  This is only
                                               // valid in the backend, where GTF_IND_ASG_LHS is not necessary
                                               // (all such indirections will be lowered to GT_STOREIND).
#define GTF_IND_UNALIGNED           0x02000000 // GT_IND   -- the load or store is unaligned (we assume worst case
                                               //             alignment of 1 byte)
#define GTF_IND_INVARIANT           0x01000000 // GT_IND   -- the target is invariant (a prejit indirection)
#define GTF_IND_ARR_LEN             0x80000000 // GT_IND   -- the indirection represents an array length (of the REF
                                               //             contribution to its argument).
#define GTF_IND_ARR_INDEX           0x00800000 // GT_IND   -- the indirection represents an (SZ) array index

#define GTF_IND_FLAGS \
    (GTF_IND_VOLATILE | GTF_IND_REFARR_LAYOUT | GTF_IND_TGTANYWHERE | GTF_IND_NONFAULTING | GTF_IND_TLS_REF |          \
     GTF_IND_UNALIGNED | GTF_IND_INVARIANT | GTF_IND_ARR_INDEX)

#define GTF_CLS_VAR_VOLATILE        0x40000000 // GT_FIELD/GT_CLS_VAR -- same as GTF_IND_VOLATILE
#define GTF_CLS_VAR_INITCLASS       0x20000000 // GT_FIELD/GT_CLS_VAR -- same as GTF_FLD_INITCLASS
#define GTF_CLS_VAR_ASG_LHS         0x04000000 // GT_CLS_VAR   -- this GT_CLS_VAR node is (the effective val) of the LHS
                                               //                 of an assignment; don't evaluate it independently.

#define GTF_ADDR_ONSTACK            0x80000000 // GT_ADDR    -- this expression is guaranteed to be on the stack

#define GTF_ADDRMODE_NO_CSE         0x80000000 // GT_ADD/GT_MUL/GT_LSH -- Do not CSE this node only, forms complex
                                               //                         addressing mode

#define GTF_MUL_64RSLT              0x40000000 // GT_MUL     -- produce 64-bit result

#ifdef LEGACY_BACKEND
#define GTF_MOD_INT_RESULT          0x80000000 // GT_MOD,    -- the real tree represented by this
                                               // GT_UMOD       node evaluates to an int even though its type is long.
                                               //               The result is placed in the low member of the reg pair.
#endif // LEGACY_BACKEND

#define GTF_RELOP_NAN_UN            0x80000000 // GT_<relop> -- Is branch taken if ops are NaN?
#define GTF_RELOP_JMP_USED          0x40000000 // GT_<relop> -- result of compare used for jump or ?:
#define GTF_RELOP_QMARK             0x20000000 // GT_<relop> -- the node is the condition for ?:
#define GTF_RELOP_ZTT               0x08000000 // GT_<relop> -- Loop test cloned for converting while-loops into do-while
                                               //               with explicit "loop test" in the header block.

#define GTF_QMARK_CAST_INSTOF       0x80000000 // GT_QMARK -- Is this a top (not nested) level qmark created for
                                               //             castclass or instanceof?

#define GTF_BOX_VALUE               0x80000000 // GT_BOX -- "box" is on a value type

#define GTF_ICON_HDL_MASK           0xF0000000 // Bits used by handle types below
#define GTF_ICON_SCOPE_HDL          0x10000000 // GT_CNS_INT -- constant is a scope handle
#define GTF_ICON_CLASS_HDL          0x20000000 // GT_CNS_INT -- constant is a class handle
#define GTF_ICON_METHOD_HDL         0x30000000 // GT_CNS_INT -- constant is a method handle
#define GTF_ICON_FIELD_HDL          0x40000000 // GT_CNS_INT -- constant is a field handle
#define GTF_ICON_STATIC_HDL         0x50000000 // GT_CNS_INT -- constant is a handle to static data
#define GTF_ICON_STR_HDL            0x60000000 // GT_CNS_INT -- constant is a string handle
#define GTF_ICON_PSTR_HDL           0x70000000 // GT_CNS_INT -- constant is a ptr to a string handle
#define GTF_ICON_PTR_HDL            0x80000000 // GT_CNS_INT -- constant is a ldptr handle
#define GTF_ICON_VARG_HDL           0x90000000 // GT_CNS_INT -- constant is a var arg cookie handle
#define GTF_ICON_PINVKI_HDL         0xA0000000 // GT_CNS_INT -- constant is a pinvoke calli handle
#define GTF_ICON_TOKEN_HDL          0xB0000000 // GT_CNS_INT -- constant is a token handle
#define GTF_ICON_TLS_HDL            0xC0000000 // GT_CNS_INT -- constant is a TLS ref with offset
#define GTF_ICON_FTN_ADDR           0xD0000000 // GT_CNS_INT -- constant is a function address
#define GTF_ICON_CIDMID_HDL         0xE0000000 // GT_CNS_INT -- constant is a class or module ID handle
#define GTF_ICON_BBC_PTR            0xF0000000 // GT_CNS_INT -- constant is a basic block count pointer

#define GTF_ICON_FIELD_OFF          0x08000000 // GT_CNS_INT -- constant is a field offset
#define GTF_ICON_SIMD_COUNT         0x04000000 // GT_CNS_INT -- constant is Vector<T>.Count

#define GTF_ICON_INITCLASS          0x02000000 // GT_CNS_INT -- Constant is used to access a static that requires preceding
                                               //               class/static init helper.  In some cases, the constant is
                                               //               the address of the static field itself, and in other cases
                                               //               there's an extra layer of indirection and it is the address
                                               //               of the cell that the runtime will fill in with the address
                                               //               of the static field; in both of those cases, the constant
                                               //               is what gets flagged.

#define GTF_BLK_VOLATILE            GTF_IND_VOLATILE  // GT_ASG, GT_STORE_BLK, GT_STORE_OBJ, GT_STORE_DYNBLK -- is a volatile block operation
#define GTF_BLK_UNALIGNED           GTF_IND_UNALIGNED // GT_ASG, GT_STORE_BLK, GT_STORE_OBJ, GT_STORE_DYNBLK -- is an unaligned block operation

#define GTF_OVERFLOW                0x10000000 // GT_ADD, GT_SUB, GT_MUL, -- Need overflow check. Use gtOverflow(Ex)() to check this flag.
                                               // GT_ASG_ADD, GT_ASG_SUB,
                                               // GT_CAST

#define GTF_ARR_BOUND_INBND         0x80000000 // GT_ARR_BOUNDS_CHECK -- have proved this check is always in-bounds

#define GTF_ARRLEN_ARR_IDX          0x80000000 // GT_ARR_LENGTH -- Length which feeds into an array index expression

#define GTF_FIELD_LIST_HEAD         0x80000000 // GT_FIELD_LIST -- Indicates that this is the first field in a list of
                                               //                  struct fields constituting a single call argument.

#define GTF_SIMD12_OP               0x80000000 // GT_SIMD -- Indicates that the operands need to be handled as SIMD12
                                               //            even if they have been retyped as SIMD16.

#define GTF_STMT_CMPADD             0x80000000 // GT_STMT -- added by compiler
#define GTF_STMT_HAS_CSE            0x40000000 // GT_STMT -- CSE def or use was subsituted

//---------------------------------------------------------------------
//
// GenTree flags stored in gtDebugFlags.
//
//---------------------------------------------------------------------

#if defined(DEBUG)
#define GTF_DEBUG_NONE              0x00000000 // No debug flags.

#define GTF_DEBUG_NODE_MORPHED      0x00000001 // the node has been morphed (in the global morphing phase)
#define GTF_DEBUG_NODE_SMALL        0x00000002
#define GTF_DEBUG_NODE_LARGE        0x00000004
#define GTF_DEBUG_NODE_CG_PRODUCED  0x00000008 // genProduceReg has been called on this node
#define GTF_DEBUG_NODE_CG_CONSUMED  0x00000010 // genConsumeReg has been called on this node

#define GTF_DEBUG_NODE_MASK         0x0000001F // These flags are all node (rather than operation) properties.

#define GTF_DEBUG_VAR_CSE_REF       0x00800000 // GT_LCL_VAR -- This is a CSE LCL_VAR node
#endif // defined(DEBUG)

//---------------------------------------------------------------------
//
// end of GenTree flags definitions
//
//---------------------------------------------------------------------

    // clang-format on

    GenTreePtr gtNext;
    GenTreePtr gtPrev;

#ifdef DEBUG
    unsigned gtTreeID;
    unsigned gtSeqNum; // liveness traversal order within the current statement

    int gtUseNum; // use-ordered traversal within the function
#endif

    static const unsigned short gtOperKindTable[];

    static unsigned OperKind(unsigned gtOper)
    {
        assert(gtOper < GT_COUNT);

        return gtOperKindTable[gtOper];
    }

    unsigned OperKind() const
    {
        assert(gtOper < GT_COUNT);

        return gtOperKindTable[gtOper];
    }

    static bool IsExOp(unsigned opKind)
    {
        return (opKind & GTK_EXOP) != 0;
    }
    // Returns the operKind with the GTK_EX_OP bit removed (the
    // kind of operator, unary or binary, that is extended).
    static unsigned StripExOp(unsigned opKind)
    {
        return opKind & ~GTK_EXOP;
    }

    bool IsValue() const
    {
        if ((OperKind(gtOper) & GTK_NOVALUE) != 0)
        {
            return false;
        }

        if (gtOper == GT_NOP || gtOper == GT_CALL)
        {
            return gtType != TYP_VOID;
        }

        if (gtOper == GT_FIELD_LIST)
        {
            return (gtFlags & GTF_FIELD_LIST_HEAD) != 0;
        }

        return true;
    }

    bool IsLIR() const
    {
        if ((OperKind(gtOper) & GTK_NOTLIR) != 0)
        {
            return false;
        }

        switch (gtOper)
        {
            case GT_NOP:
                // NOPs may only be present in LIR if they do not produce a value.
                return IsNothingNode();

            case GT_ARGPLACE:
            case GT_LIST:
                // ARGPLACE and LIST nodes may not be present in a block's LIR sequence, but they may
                // be present as children of an LIR node.
                return (gtNext == nullptr) && (gtPrev == nullptr);

            case GT_FIELD_LIST:
                // Only the head of the FIELD_LIST is present in the block's LIR sequence.
                return (((gtFlags & GTF_FIELD_LIST_HEAD) != 0) || ((gtNext == nullptr) && (gtPrev == nullptr)));

            case GT_ADDR:
            {
                // ADDR ndoes may only be present in LIR if the location they refer to is not a
                // local, class variable, or IND node.
                GenTree*   location   = const_cast<GenTree*>(this)->gtGetOp1();
                genTreeOps locationOp = location->OperGet();
                return !location->IsLocal() && (locationOp != GT_CLS_VAR) && (locationOp != GT_IND);
            }

            default:
                // All other nodes are assumed to be correct.
                return true;
        }
    }

    bool OperIs(genTreeOps oper)
    {
        return OperGet() == oper;
    }

    template <typename... T>
    bool OperIs(genTreeOps oper, T... rest)
    {
        return OperIs(oper) || OperIs(rest...);
    }

    static bool OperIsConst(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_CONST) != 0;
    }

    bool OperIsConst() const
    {
        return (OperKind(gtOper) & GTK_CONST) != 0;
    }

    static bool OperIsLeaf(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_LEAF) != 0;
    }

    bool OperIsLeaf() const
    {
        return (OperKind(gtOper) & GTK_LEAF) != 0;
    }

    static bool OperIsCompare(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_RELOP) != 0;
    }

    static bool OperIsLocal(genTreeOps gtOper)
    {
        bool result = (OperKind(gtOper) & GTK_LOCAL) != 0;
        assert(result == (gtOper == GT_LCL_VAR || gtOper == GT_PHI_ARG || gtOper == GT_REG_VAR ||
                          gtOper == GT_LCL_FLD || gtOper == GT_STORE_LCL_VAR || gtOper == GT_STORE_LCL_FLD));
        return result;
    }

    static bool OperIsLocalAddr(genTreeOps gtOper)
    {
        return (gtOper == GT_LCL_VAR_ADDR || gtOper == GT_LCL_FLD_ADDR);
    }

    static bool OperIsLocalField(genTreeOps gtOper)
    {
        return (gtOper == GT_LCL_FLD || gtOper == GT_LCL_FLD_ADDR || gtOper == GT_STORE_LCL_FLD);
    }

    inline bool OperIsLocalField() const
    {
        return OperIsLocalField(gtOper);
    }

    static bool OperIsScalarLocal(genTreeOps gtOper)
    {
        return (gtOper == GT_LCL_VAR || gtOper == GT_REG_VAR || gtOper == GT_STORE_LCL_VAR);
    }

    static bool OperIsNonPhiLocal(genTreeOps gtOper)
    {
        return OperIsLocal(gtOper) && (gtOper != GT_PHI_ARG);
    }

    static bool OperIsLocalRead(genTreeOps gtOper)
    {
        return (OperIsLocal(gtOper) && !OperIsLocalStore(gtOper));
    }

    static bool OperIsLocalStore(genTreeOps gtOper)
    {
        return (gtOper == GT_STORE_LCL_VAR || gtOper == GT_STORE_LCL_FLD);
    }

    static bool OperIsAddrMode(genTreeOps gtOper)
    {
        return (gtOper == GT_LEA);
    }

    static bool OperIsInitVal(genTreeOps gtOper)
    {
        return (gtOper == GT_INIT_VAL);
    }

    bool OperIsInitVal() const
    {
        return OperIsInitVal(OperGet());
    }

    bool IsConstInitVal()
    {
        return (gtOper == GT_CNS_INT) || (OperIsInitVal() && (gtGetOp1()->gtOper == GT_CNS_INT));
    }

    bool OperIsBlkOp();
    bool OperIsCopyBlkOp();
    bool OperIsInitBlkOp();
    bool OperIsDynBlkOp();

    static bool OperIsBlk(genTreeOps gtOper)
    {
        return ((gtOper == GT_BLK) || (gtOper == GT_OBJ) || (gtOper == GT_DYN_BLK) || (gtOper == GT_STORE_BLK) ||
                (gtOper == GT_STORE_OBJ) || (gtOper == GT_STORE_DYN_BLK));
    }

    bool OperIsBlk() const
    {
        return OperIsBlk(OperGet());
    }

    static bool OperIsDynBlk(genTreeOps gtOper)
    {
        return ((gtOper == GT_DYN_BLK) || (gtOper == GT_STORE_DYN_BLK));
    }

    bool OperIsDynBlk() const
    {
        return OperIsDynBlk(OperGet());
    }

    static bool OperIsStoreBlk(genTreeOps gtOper)
    {
        return ((gtOper == GT_STORE_BLK) || (gtOper == GT_STORE_OBJ) || (gtOper == GT_STORE_DYN_BLK));
    }

    bool OperIsStoreBlk() const
    {
        return OperIsStoreBlk(OperGet());
    }

    bool OperIsPutArgStk() const
    {
        return gtOper == GT_PUTARG_STK;
    }

    bool OperIsPutArgReg() const
    {
        return gtOper == GT_PUTARG_REG;
    }

    bool OperIsPutArg() const
    {
        return OperIsPutArgStk() || OperIsPutArgReg();
    }

    bool OperIsAddrMode() const
    {
        return OperIsAddrMode(OperGet());
    }

    bool OperIsLocal() const
    {
        return OperIsLocal(OperGet());
    }

    bool OperIsLocalAddr() const
    {
        return OperIsLocalAddr(OperGet());
    }

    bool OperIsScalarLocal() const
    {
        return OperIsScalarLocal(OperGet());
    }

    bool OperIsNonPhiLocal() const
    {
        return OperIsNonPhiLocal(OperGet());
    }

    bool OperIsLocalStore() const
    {
        return OperIsLocalStore(OperGet());
    }

    bool OperIsLocalRead() const
    {
        return OperIsLocalRead(OperGet());
    }

    bool OperIsCompare() const
    {
        return (OperKind(gtOper) & GTK_RELOP) != 0;
    }

    static bool OperIsLogical(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_LOGOP) != 0;
    }

    bool OperIsLogical() const
    {
        return (OperKind(gtOper) & GTK_LOGOP) != 0;
    }

    static bool OperIsShift(genTreeOps gtOper)
    {
        return (gtOper == GT_LSH) || (gtOper == GT_RSH) || (gtOper == GT_RSZ);
    }

    bool OperIsShift() const
    {
        return OperIsShift(OperGet());
    }

    static bool OperIsRotate(genTreeOps gtOper)
    {
        return (gtOper == GT_ROL) || (gtOper == GT_ROR);
    }

    bool OperIsRotate() const
    {
        return OperIsRotate(OperGet());
    }

    static bool OperIsShiftOrRotate(genTreeOps gtOper)
    {
        return OperIsShift(gtOper) || OperIsRotate(gtOper);
    }

    bool OperIsShiftOrRotate() const
    {
        return OperIsShiftOrRotate(OperGet());
    }

    bool OperIsArithmetic() const
    {
        genTreeOps op = OperGet();
        return op == GT_ADD || op == GT_SUB || op == GT_MUL || op == GT_DIV || op == GT_MOD

               || op == GT_UDIV || op == GT_UMOD

               || op == GT_OR || op == GT_XOR || op == GT_AND

               || OperIsShiftOrRotate(op);
    }

#if !defined(LEGACY_BACKEND) && !defined(_TARGET_64BIT_)
    static bool OperIsHigh(genTreeOps gtOper)
    {
        switch (gtOper)
        {
            case GT_ADD_HI:
            case GT_SUB_HI:
            case GT_DIV_HI:
            case GT_MOD_HI:
                return true;
            default:
                return false;
        }
    }

    bool OperIsHigh() const
    {
        return OperIsHigh(OperGet());
    }
#endif // !defined(LEGACY_BACKEND) && !defined(_TARGET_64BIT_)

    static bool OperIsUnary(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_UNOP) != 0;
    }

    bool OperIsUnary() const
    {
        return OperIsUnary(gtOper);
    }

    static bool OperIsBinary(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_BINOP) != 0;
    }

    bool OperIsBinary() const
    {
        return OperIsBinary(gtOper);
    }

    static bool OperIsSimple(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_SMPOP) != 0;
    }

    static bool OperIsSpecial(genTreeOps gtOper)
    {
        return ((OperKind(gtOper) & GTK_KINDMASK) == GTK_SPECIAL);
    }

    bool OperIsSimple() const
    {
        return OperIsSimple(gtOper);
    }

#ifdef FEATURE_SIMD
    bool isCommutativeSIMDIntrinsic();
#else  // !
    bool isCommutativeSIMDIntrinsic()
    {
        return false;
    }
#endif // FEATURE_SIMD

    static bool OperIsCommutative(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_COMMUTE) != 0;
    }

    bool OperIsCommutative()
    {
        return OperIsCommutative(gtOper) || (OperIsSIMD(gtOper) && isCommutativeSIMDIntrinsic());
    }

    static bool OperIsAssignment(genTreeOps gtOper)
    {
        return (OperKind(gtOper) & GTK_ASGOP) != 0;
    }

    bool OperIsAssignment() const
    {
        return OperIsAssignment(gtOper);
    }

    static bool OperIsIndir(genTreeOps gtOper)
    {
        return gtOper == GT_IND || gtOper == GT_STOREIND || gtOper == GT_NULLCHECK || OperIsBlk(gtOper);
    }

    bool OperIsIndir() const
    {
        return OperIsIndir(gtOper);
    }

    static bool OperIsImplicitIndir(genTreeOps gtOper)
    {
        switch (gtOper)
        {
            case GT_LOCKADD:
            case GT_XADD:
            case GT_XCHG:
            case GT_CMPXCHG:
            case GT_BLK:
            case GT_OBJ:
            case GT_DYN_BLK:
            case GT_STORE_BLK:
            case GT_STORE_OBJ:
            case GT_STORE_DYN_BLK:
            case GT_BOX:
            case GT_ARR_INDEX:
            case GT_ARR_ELEM:
            case GT_ARR_OFFSET:
                return true;
            default:
                return false;
        }
    }

    bool OperIsImplicitIndir() const
    {
        return OperIsImplicitIndir(gtOper);
    }

    bool OperIsStore() const
    {
        return OperIsStore(gtOper);
    }

    static bool OperIsStore(genTreeOps gtOper)
    {
        return (gtOper == GT_STOREIND || gtOper == GT_STORE_LCL_VAR || gtOper == GT_STORE_LCL_FLD ||
                gtOper == GT_STORE_BLK || gtOper == GT_STORE_OBJ || gtOper == GT_STORE_DYN_BLK);
    }

    static bool OperIsAtomicOp(genTreeOps gtOper)
    {
        return (gtOper == GT_XADD || gtOper == GT_XCHG || gtOper == GT_LOCKADD || gtOper == GT_CMPXCHG);
    }

    bool OperIsAtomicOp() const
    {
        return OperIsAtomicOp(gtOper);
    }

    // This is basically here for cleaner FEATURE_SIMD #ifdefs.
    static bool OperIsSIMD(genTreeOps gtOper)
    {
#ifdef FEATURE_SIMD
        return gtOper == GT_SIMD;
#else  // !FEATURE_SIMD
        return false;
#endif // !FEATURE_SIMD
    }

    bool OperIsSIMD()
    {
        return OperIsSIMD(gtOper);
    }

    bool OperIsFieldListHead()
    {
        return (gtOper == GT_FIELD_LIST) && ((gtFlags & GTF_FIELD_LIST_HEAD) != 0);
    }

    bool OperIsConditionalJump() const
    {
        return (gtOper == GT_JTRUE) || (gtOper == GT_JCC);
    }

    static bool OperIsBoundsCheck(genTreeOps op)
    {
        if (op == GT_ARR_BOUNDS_CHECK)
        {
            return true;
        }
#ifdef FEATURE_SIMD
        if (op == GT_SIMD_CHK)
        {
            return true;
        }
#endif // FEATURE_SIMD
        return false;
    }

    bool OperIsBoundsCheck() const
    {
        return OperIsBoundsCheck(OperGet());
    }

    // Requires that "op" is an op= operator.  Returns
    // the corresponding "op".
    static genTreeOps OpAsgToOper(genTreeOps op);

#ifdef DEBUG
    bool NullOp1Legal() const
    {
        assert(OperIsSimple(gtOper));
        switch (gtOper)
        {
            case GT_PHI:
            case GT_LEA:
            case GT_RETFILT:
            case GT_NOP:
                return true;
            case GT_RETURN:
                return gtType == TYP_VOID;
            default:
                return false;
        }
    }

    bool NullOp2Legal() const
    {
        assert(OperIsSimple(gtOper) || OperIsBlk(gtOper));
        if (!OperIsBinary(gtOper))
        {
            return true;
        }
        switch (gtOper)
        {
            case GT_LIST:
            case GT_FIELD_LIST:
            case GT_INTRINSIC:
            case GT_LEA:
#ifdef FEATURE_SIMD
            case GT_SIMD:
#endif // !FEATURE_SIMD
                return true;
            default:
                return false;
        }
    }

    static inline bool RequiresNonNullOp2(genTreeOps oper);
    bool IsValidCallArgument();
#endif // DEBUG

    inline bool IsFPZero();
    inline bool IsIntegralConst(ssize_t constVal);
    inline bool IsIntegralConstVector(ssize_t constVal);

    inline bool IsBoxedValue();

    inline bool IsSIMDEqualityOrInequality() const;

    static bool OperIsList(genTreeOps gtOper)
    {
        return gtOper == GT_LIST;
    }

    bool OperIsList() const
    {
        return OperIsList(gtOper);
    }

    static bool OperIsFieldList(genTreeOps gtOper)
    {
        return gtOper == GT_FIELD_LIST;
    }

    bool OperIsFieldList() const
    {
        return OperIsFieldList(gtOper);
    }

    static bool OperIsAnyList(genTreeOps gtOper)
    {
        return OperIsList(gtOper) || OperIsFieldList(gtOper);
    }

    bool OperIsAnyList() const
    {
        return OperIsAnyList(gtOper);
    }

    inline GenTreePtr MoveNext();

    inline GenTreePtr Current();

    inline GenTreePtr* pCurrent();

    inline GenTreePtr gtGetOp1();

    // Directly return op2. Asserts the node is binary. Might return nullptr if the binary node allows
    // a nullptr op2, such as GT_LIST. This is more efficient than gtGetOp2IfPresent() if you know what
    // node type you have.
    inline GenTreePtr gtGetOp2();

    // The returned pointer might be nullptr if the node is not binary, or if non-null op2 is not required.
    inline GenTreePtr gtGetOp2IfPresent();

    // Given a tree node, if this is a child of that node, return the pointer to the child node so that it
    // can be modified; otherwise, return null.
    GenTreePtr* gtGetChildPointer(GenTreePtr parent);

    // Given a tree node, if this node uses that node, return the use as an out parameter and return true.
    // Otherwise, return false.
    bool TryGetUse(GenTree* def, GenTree*** use);

    // Get the parent of this node, and optionally capture the pointer to the child so that it can be modified.
    GenTreePtr gtGetParent(GenTreePtr** parentChildPtrPtr);

    void ReplaceOperand(GenTree** useEdge, GenTree* replacement);

    inline GenTreePtr gtEffectiveVal(bool commaOnly = false);

    // Return the child of this node if it is a GT_RELOAD or GT_COPY; otherwise simply return the node itself
    inline GenTree* gtSkipReloadOrCopy();

    // Returns true if it is a call node returning its value in more than one register
    inline bool IsMultiRegCall() const;

    // Returns true if it is a node returning its value in more than one register
    inline bool IsMultiRegNode() const;

    // Returns true if it is a GT_COPY or GT_RELOAD node
    inline bool IsCopyOrReload() const;

    // Returns true if it is a GT_COPY or GT_RELOAD of a multi-reg call node
    inline bool IsCopyOrReloadOfMultiRegCall() const;

    bool OperMayThrow();

    unsigned GetScaleIndexMul();
    unsigned GetScaleIndexShf();
    unsigned GetScaledIndex();

    // Returns true if "addr" is a GT_ADD node, at least one of whose arguments is an integer
    // (<= 32 bit) constant.  If it returns true, it sets "*offset" to (one of the) constant value(s), and
    // "*addr" to the other argument.
    bool IsAddWithI32Const(GenTreePtr* addr, int* offset);

public:
#if SMALL_TREE_NODES
    static unsigned char s_gtNodeSizes[];
#if NODEBASH_STATS || MEASURE_NODE_SIZE || COUNT_AST_OPERS
    static unsigned char s_gtTrueSizes[];
#endif
#if COUNT_AST_OPERS
    static LONG s_gtNodeCounts[];
#endif
#endif // SMALL_TREE_NODES

    static void InitNodeSize();

    size_t GetNodeSize() const;

    bool IsNodeProperlySized() const;

    void CopyFrom(const GenTree* src, Compiler* comp);

    static genTreeOps ReverseRelop(genTreeOps relop);

    static genTreeOps SwapRelop(genTreeOps relop);

    //---------------------------------------------------------------------

    static bool Compare(GenTreePtr op1, GenTreePtr op2, bool swapOK = false);

//---------------------------------------------------------------------

#if defined(DEBUG)
    static const char* NodeName(genTreeOps op);
#endif

#if defined(DEBUG) || NODEBASH_STATS || MEASURE_NODE_SIZE || COUNT_AST_OPERS
    static const char* OpName(genTreeOps op);
#endif

#if MEASURE_NODE_SIZE && SMALL_TREE_NODES
    static const char* OpStructName(genTreeOps op);
#endif

    //---------------------------------------------------------------------

    bool IsNothingNode() const;
    void gtBashToNOP();

    // Value number update action enumeration
    enum ValueNumberUpdate
    {
        CLEAR_VN,   // Clear value number
        PRESERVE_VN // Preserve value number
    };

    void SetOper(genTreeOps oper, ValueNumberUpdate vnUpdate = CLEAR_VN); // set gtOper
    void SetOperResetFlags(genTreeOps oper);                              // set gtOper and reset flags

    void ChangeOperConst(genTreeOps oper); // ChangeOper(constOper)
    // set gtOper and only keep GTF_COMMON_MASK flags
    void ChangeOper(genTreeOps oper, ValueNumberUpdate vnUpdate = CLEAR_VN);
    void ChangeOperUnchecked(genTreeOps oper);
    void SetOperRaw(genTreeOps oper);

    void ChangeType(var_types newType)
    {
        var_types oldType = gtType;
        gtType            = newType;
        GenTree* node     = this;
        while (node->gtOper == GT_COMMA)
        {
            node = node->gtGetOp2();
            assert(node->gtType == oldType);
            node->gtType = newType;
        }
    }

#if SMALL_TREE_NODES
#if NODEBASH_STATS
    static void RecordOperBashing(genTreeOps operOld, genTreeOps operNew);
    static void ReportOperBashing(FILE* fp);
#else
    static void RecordOperBashing(genTreeOps operOld, genTreeOps operNew)
    { /* do nothing */
    }
    static void ReportOperBashing(FILE* fp)
    { /* do nothing */
    }
#endif
#endif

    bool IsLocal() const
    {
        return OperIsLocal(OperGet());
    }

    // Returns "true" iff 'this' is a GT_LCL_FLD or GT_STORE_LCL_FLD on which the type
    // is not the same size as the type of the GT_LCL_VAR.
    bool IsPartialLclFld(Compiler* comp);

    // Returns "true" iff "this" defines a local variable.  Requires "comp" to be the
    // current compilation.  If returns "true", sets "*pLclVarTree" to the
    // tree for the local that is defined, and, if "pIsEntire" is non-null, sets "*pIsEntire" to
    // true or false, depending on whether the assignment writes to the entirety of the local
    // variable, or just a portion of it.
    bool DefinesLocal(Compiler* comp, GenTreeLclVarCommon** pLclVarTree, bool* pIsEntire = nullptr);

    // Returns true if "this" represents the address of a local, or a field of a local.  If returns true, sets
    // "*pLclVarTree" to the node indicating the local variable.  If the address is that of a field of this node,
    // sets "*pFldSeq" to the field sequence representing that field, else null.
    bool IsLocalAddrExpr(Compiler* comp, GenTreeLclVarCommon** pLclVarTree, FieldSeqNode** pFldSeq);

    // Simpler variant of the above which just returns the local node if this is an expression that
    // yields an address into a local
    GenTreeLclVarCommon* IsLocalAddrExpr();

    // Determine if this is a LclVarCommon node and return some additional info about it in the
    // two out parameters.
    bool IsLocalExpr(Compiler* comp, GenTreeLclVarCommon** pLclVarTree, FieldSeqNode** pFldSeq);

    // Determine whether this is an assignment tree of the form X = X (op) Y,
    // where Y is an arbitrary tree, and X is a lclVar.
    unsigned IsLclVarUpdateTree(GenTree** otherTree, genTreeOps* updateOper);

    // If returns "true", "this" may represent the address of a static or instance field
    // (or a field of such a field, in the case of an object field of type struct).
    // If returns "true", then either "*pObj" is set to the object reference,
    // or "*pStatic" is set to the baseAddr or offset to be added to the "*pFldSeq"
    // Only one of "*pObj" or "*pStatic" will be set, the other one will be null.
    // The boolean return value only indicates that "this" *may* be a field address
    // -- the field sequence must also be checked.
    // If it is a field address, the field sequence will be a sequence of length >= 1,
    // starting with an instance or static field, and optionally continuing with struct fields.
    bool IsFieldAddr(Compiler* comp, GenTreePtr* pObj, GenTreePtr* pStatic, FieldSeqNode** pFldSeq);

    // Requires "this" to be the address of an array (the child of a GT_IND labeled with GTF_IND_ARR_INDEX).
    // Sets "pArr" to the node representing the array (either an array object pointer, or perhaps a byref to the some
    // element).
    // Sets "*pArrayType" to the class handle for the array type.
    // Sets "*inxVN" to the value number inferred for the array index.
    // Sets "*pFldSeq" to the sequence, if any, of struct fields used to index into the array element.
    void ParseArrayAddress(
        Compiler* comp, struct ArrayInfo* arrayInfo, GenTreePtr* pArr, ValueNum* pInxVN, FieldSeqNode** pFldSeq);

    // Helper method for the above.
    void ParseArrayAddressWork(
        Compiler* comp, ssize_t inputMul, GenTreePtr* pArr, ValueNum* pInxVN, ssize_t* pOffset, FieldSeqNode** pFldSeq);

    // Requires "this" to be a GT_IND.  Requires the outermost caller to set "*pFldSeq" to nullptr.
    // Returns true if it is an array index expression, or access to a (sequence of) struct field(s)
    // within a struct array element.  If it returns true, sets *arrayInfo to the array information, and sets *pFldSeq
    // to the sequence of struct field accesses.
    bool ParseArrayElemForm(Compiler* comp, ArrayInfo* arrayInfo, FieldSeqNode** pFldSeq);

    // Requires "this" to be the address of a (possible) array element (or struct field within that).
    // If it is, sets "*arrayInfo" to the array access info, "*pFldSeq" to the sequence of struct fields
    // accessed within the array element, and returns true.  If not, returns "false".
    bool ParseArrayElemAddrForm(Compiler* comp, ArrayInfo* arrayInfo, FieldSeqNode** pFldSeq);

    // Requires "this" to be an int expression.  If it is a sequence of one or more integer constants added together,
    // returns true and sets "*pFldSeq" to the sequence of fields with which those constants are annotated.
    bool ParseOffsetForm(Compiler* comp, FieldSeqNode** pFldSeq);

    // Labels "*this" as an array index expression: label all constants and variables that could contribute, as part of
    // an affine expression, to the value of the of the index.
    void LabelIndex(Compiler* comp, bool isConst = true);

    // Assumes that "this" occurs in a context where it is being dereferenced as the LHS of an assignment-like
    // statement (assignment, initblk, or copyblk).  The "width" should be the number of bytes copied by the
    // operation.  Returns "true" if "this" is an address of (or within)
    // a local variable; sets "*pLclVarTree" to that local variable instance; and, if "pIsEntire" is non-null,
    // sets "*pIsEntire" to true if this assignment writes the full width of the local.
    bool DefinesLocalAddr(Compiler* comp, unsigned width, GenTreeLclVarCommon** pLclVarTree, bool* pIsEntire);

    bool IsRegVar() const
    {
        return OperGet() == GT_REG_VAR ? true : false;
    }
    bool InReg() const
    {
        return (gtFlags & GTF_REG_VAL) ? true : false;
    }
    void SetInReg()
    {
        gtFlags |= GTF_REG_VAL;
    }

    regNumber GetReg() const
    {
        return InReg() ? gtRegNum : REG_NA;
    }
    bool IsRegVarDeath() const
    {
        assert(OperGet() == GT_REG_VAR);
        return (gtFlags & GTF_VAR_DEATH) ? true : false;
    }
    bool IsRegVarBirth() const
    {
        assert(OperGet() == GT_REG_VAR);
        return (gtFlags & GTF_REG_BIRTH) ? true : false;
    }
    bool IsReverseOp() const
    {
        return (gtFlags & GTF_REVERSE_OPS) ? true : false;
    }
    bool IsUnsigned() const
    {
        return ((gtFlags & GTF_UNSIGNED) != 0);
    }

    inline bool IsCnsIntOrI() const;

    inline bool IsIntegralConst() const;

    inline bool IsIntCnsFitsInI32();

    inline bool IsCnsFltOrDbl() const;

    inline bool IsCnsNonZeroFltOrDbl();

    bool IsIconHandle() const
    {
        assert(gtOper == GT_CNS_INT);
        return (gtFlags & GTF_ICON_HDL_MASK) ? true : false;
    }

    bool IsIconHandle(unsigned handleType) const
    {
        assert(gtOper == GT_CNS_INT);
        assert((handleType & GTF_ICON_HDL_MASK) != 0); // check that handleType is one of the valid GTF_ICON_* values
        assert((handleType & ~GTF_ICON_HDL_MASK) == 0);
        return (gtFlags & GTF_ICON_HDL_MASK) == handleType;
    }

    // Return just the part of the flags corresponding to the GTF_ICON_*_HDL flag. For example,
    // GTF_ICON_SCOPE_HDL. The tree node must be a const int, but it might not be a handle, in which
    // case we'll return zero.
    unsigned GetIconHandleFlag() const
    {
        assert(gtOper == GT_CNS_INT);
        return (gtFlags & GTF_ICON_HDL_MASK);
    }

    // Mark this node as no longer being a handle; clear its GTF_ICON_*_HDL bits.
    void ClearIconHandleMask()
    {
        assert(gtOper == GT_CNS_INT);
        gtFlags &= ~GTF_ICON_HDL_MASK;
    }

    // Return true if the two GT_CNS_INT trees have the same handle flag (GTF_ICON_*_HDL).
    static bool SameIconHandleFlag(GenTree* t1, GenTree* t2)
    {
        return t1->GetIconHandleFlag() == t2->GetIconHandleFlag();
    }

    bool IsArgPlaceHolderNode() const
    {
        return OperGet() == GT_ARGPLACE;
    }
    bool IsCall() const
    {
        return OperGet() == GT_CALL;
    }
    bool IsStatement() const
    {
        return OperGet() == GT_STMT;
    }
    inline bool IsHelperCall();

    bool IsVarAddr() const;
    bool gtOverflow() const;
    bool gtOverflowEx() const;
    bool gtSetFlags() const;
    bool gtRequestSetFlags();

    // Returns true if the codegen of this tree node
    // sets ZF and SF flags.
    bool gtSetZSFlags() const
    {
        return (gtFlags & GTF_ZSF_SET) != 0;
    }

#ifdef DEBUG
    bool       gtIsValid64RsltMul();
    static int gtDispFlags(unsigned flags, unsigned debugFlags);
#endif

    // cast operations
    inline var_types  CastFromType();
    inline var_types& CastToType();

    // Returns true if this gentree node is marked by lowering to indicate
    // that codegen can still generate code even if it wasn't allocated a
    // register.
    bool IsRegOptional() const;

    // Returns "true" iff "this" is a phi-related node (i.e. a GT_PHI_ARG, GT_PHI, or a PhiDefn).
    bool IsPhiNode();

    // Returns "true" iff "*this" is an assignment (GT_ASG) tree that defines an SSA name (lcl = phi(...));
    bool IsPhiDefn();

    // Returns "true" iff "*this" is a statement containing an assignment that defines an SSA name (lcl = phi(...));
    bool IsPhiDefnStmt();

    // Can't use an assignment operator, because we need the extra "comp" argument
    // (to provide the allocator necessary for the VarSet assignment).
    // TODO-Cleanup: Not really needed now, w/o liveset on tree nodes
    void CopyTo(class Compiler* comp, const GenTree& gt);

    // Like the above, excepts assumes copying from small node to small node.
    // (Following the code it replaces, it does *not* copy the GenTree fields,
    // which CopyTo does.)
    void CopyToSmall(const GenTree& gt);

    // Because of the fact that we hid the assignment operator of "BitSet" (in DEBUG),
    // we can't synthesize an assignment operator.
    // TODO-Cleanup: Could change this w/o liveset on tree nodes
    // (This is also necessary for the VTable trick.)
    GenTree()
    {
    }

    // Returns the number of children of the current node.
    unsigned NumChildren();

    // Requires "childNum < NumChildren()".  Returns the "n"th child of "this."
    GenTreePtr GetChild(unsigned childNum);

    // Returns an iterator that will produce the use edge to each operand of this node. Differs
    // from the sequence of nodes produced by a loop over `GetChild` in its handling of call, phi,
    // and block op nodes.
    GenTreeUseEdgeIterator UseEdgesBegin();
    GenTreeUseEdgeIterator UseEdgesEnd();

    IteratorPair<GenTreeUseEdgeIterator> UseEdges();

    // Returns an iterator that will produce each operand of this node. Differs from the sequence
    // of nodes produced by a loop over `GetChild` in its handling of call, phi, and block op
    // nodes.
    GenTreeOperandIterator OperandsBegin();
    GenTreeOperandIterator OperandsEnd();

    // Returns a range that will produce the operands of this node in use order.
    IteratorPair<GenTreeOperandIterator> Operands();

    bool Precedes(GenTree* other);

    // The maximum possible # of children of any node.
    static const int MAX_CHILDREN = 6;

    bool IsReuseRegVal() const
    {
        // This can be extended to non-constant nodes, but not to local or indir nodes.
        if (OperIsConst() && ((gtFlags & GTF_REUSE_REG_VAL) != 0))
        {
            return true;
        }
        return false;
    }
    void SetReuseRegVal()
    {
        assert(OperIsConst());
        gtFlags |= GTF_REUSE_REG_VAL;
    }
    void ResetReuseRegVal()
    {
        assert(OperIsConst());
        gtFlags &= ~GTF_REUSE_REG_VAL;
    }

#if MEASURE_NODE_SIZE
    static void DumpNodeSizes(FILE* fp);
#endif

#ifdef DEBUG

private:
    GenTree& operator=(const GenTree& gt)
    {
        assert(!"Don't copy");
        return *this;
    }
#endif // DEBUG

#if DEBUGGABLE_GENTREE
    // In DEBUG builds, add a dummy virtual method, to give the debugger run-time type information.
    virtual void DummyVirt()
    {
    }

    typedef void* VtablePtr;

    VtablePtr GetVtableForOper(genTreeOps oper);
    void SetVtableForOper(genTreeOps oper);

    static VtablePtr s_vtablesForOpers[GT_COUNT];
    static VtablePtr s_vtableForOp;
#endif // DEBUGGABLE_GENTREE

public:
    inline void* operator new(size_t sz, class Compiler*, genTreeOps oper);

    inline GenTree(genTreeOps oper, var_types type DEBUGARG(bool largeNode = false));
};

//------------------------------------------------------------------------
// GenTreeUseEdgeIterator: an iterator that will produce each use edge of a GenTree node in the order in which
//                         they are used.
//
// The use edges of a node may not correspond exactly to the nodes on the other ends of its use edges: in
// particular, GT_LIST nodes are expanded into their component parts. This differs from the behavior of
// GenTree::GetChildPointer(), which does not expand lists.
//
// Operand iteration is common enough in the back end of the compiler that the implementation of this type has
// traded some simplicity for speed:
// - As much work as is reasonable is done in the constructor rather than during operand iteration
// - Node-specific functionality is handled by a small class of "advance" functions called by operator++
//   rather than making operator++ itself handle all nodes
// - Some specialization has been performed for specific node types/shapes (e.g. the advance function for
//   binary nodes is specialized based on whether or not the node has the GTF_REVERSE_OPS flag set)
//
// Valid values of this type may be obtained by calling `GenTree::UseEdgesBegin` and `GenTree::UseEdgesEnd`.
//
class GenTreeUseEdgeIterator final
{
    friend class GenTreeOperandIterator;
    friend GenTreeUseEdgeIterator GenTree::UseEdgesBegin();
    friend GenTreeUseEdgeIterator GenTree::UseEdgesEnd();

    enum
    {
        CALL_INSTANCE     = 0,
        CALL_ARGS         = 1,
        CALL_LATE_ARGS    = 2,
        CALL_CONTROL_EXPR = 3,
        CALL_COOKIE       = 4,
        CALL_ADDRESS      = 5,
        CALL_TERMINAL     = 6,
    };

    typedef void (GenTreeUseEdgeIterator::*AdvanceFn)();

    AdvanceFn m_advance;
    GenTree*  m_node;
    GenTree** m_edge;
    GenTree*  m_argList;
    int       m_state;

    GenTreeUseEdgeIterator(GenTree* node);

    // Advance functions for special nodes
    void AdvanceCmpXchg();
    void AdvanceBoundsChk();
    void AdvanceArrElem();
    void AdvanceArrOffset();
    void AdvanceDynBlk();
    void AdvanceStoreDynBlk();

    template <bool ReverseOperands>
    void           AdvanceBinOp();
    void           SetEntryStateForBinOp();

    // An advance function for list-like nodes (Phi, SIMDIntrinsicInitN, FieldList)
    void AdvanceList();
    void SetEntryStateForList(GenTree* list);

    // The advance function for call nodes
    template <int state>
    void          AdvanceCall();

    void Terminate();

public:
    GenTreeUseEdgeIterator();

    inline GenTree** operator*()
    {
        assert(m_state != -1);
        return m_edge;
    }

    inline GenTree** operator->()
    {
        assert(m_state != -1);
        return m_edge;
    }

    inline bool operator==(const GenTreeUseEdgeIterator& other) const
    {
        if (m_state == -1 || other.m_state == -1)
        {
            return m_state == other.m_state;
        }

        return (m_node == other.m_node) && (m_edge == other.m_edge) && (m_argList == other.m_argList) &&
               (m_state == other.m_state);
    }

    inline bool operator!=(const GenTreeUseEdgeIterator& other) const
    {
        return !(operator==(other));
    }

    GenTreeUseEdgeIterator& operator++();
};

//------------------------------------------------------------------------
// GenTreeOperandIterator: an iterator that will produce each operand of a
//                         GenTree node in the order in which they are
//                         used. This uses `GenTreeUseEdgeIterator` under
//                         the covers and comes with the same caveats
//                         w.r.t. `GetChild`.
//
// Note: valid values of this type may be obtained by calling
// `GenTree::OperandsBegin` and `GenTree::OperandsEnd`.
class GenTreeOperandIterator final
{
    friend GenTreeOperandIterator GenTree::OperandsBegin();
    friend GenTreeOperandIterator GenTree::OperandsEnd();

    GenTreeUseEdgeIterator m_useEdges;

    GenTreeOperandIterator(GenTree* node) : m_useEdges(node)
    {
    }

public:
    GenTreeOperandIterator() : m_useEdges()
    {
    }

    inline GenTree* operator*()
    {
        return *(*m_useEdges);
    }

    inline GenTree* operator->()
    {
        return *(*m_useEdges);
    }

    inline bool operator==(const GenTreeOperandIterator& other) const
    {
        return m_useEdges == other.m_useEdges;
    }

    inline bool operator!=(const GenTreeOperandIterator& other) const
    {
        return !(operator==(other));
    }

    inline GenTreeOperandIterator& operator++()
    {
        ++m_useEdges;
        return *this;
    }
};

/*****************************************************************************/
// In the current design, we never instantiate GenTreeUnOp: it exists only to be
// used as a base class.  For unary operators, we instantiate GenTreeOp, with a NULL second
// argument.  We check that this is true dynamically.  We could tighten this and get static
// checking, but that would entail accessing the first child of a unary operator via something
// like gtUnOp.gtOp1 instead of gtOp.gtOp1.
struct GenTreeUnOp : public GenTree
{
    GenTreePtr gtOp1;

protected:
    GenTreeUnOp(genTreeOps oper, var_types type DEBUGARG(bool largeNode = false))
        : GenTree(oper, type DEBUGARG(largeNode)), gtOp1(nullptr)
    {
    }

    GenTreeUnOp(genTreeOps oper, var_types type, GenTreePtr op1 DEBUGARG(bool largeNode = false))
        : GenTree(oper, type DEBUGARG(largeNode)), gtOp1(op1)
    {
        assert(op1 != nullptr || NullOp1Legal());
        if (op1 != nullptr)
        { // Propagate effects flags from child.
            gtFlags |= op1->gtFlags & GTF_ALL_EFFECT;
        }
    }

#if DEBUGGABLE_GENTREE
    GenTreeUnOp() : GenTree(), gtOp1(nullptr)
    {
    }
#endif
};

struct GenTreeOp : public GenTreeUnOp
{
    GenTreePtr gtOp2;

    GenTreeOp(genTreeOps oper, var_types type, GenTreePtr op1, GenTreePtr op2 DEBUGARG(bool largeNode = false))
        : GenTreeUnOp(oper, type, op1 DEBUGARG(largeNode)), gtOp2(op2)
    {
        // comparisons are always integral types
        assert(!GenTree::OperIsCompare(oper) || varTypeIsIntegral(type));
        // Binary operators, with a few exceptions, require a non-nullptr
        // second argument.
        assert(op2 != nullptr || NullOp2Legal());
        // Unary operators, on the other hand, require a null second argument.
        assert(!OperIsUnary(oper) || op2 == nullptr);
        // Propagate effects flags from child.  (UnOp handled this for first child.)
        if (op2 != nullptr)
        {
            gtFlags |= op2->gtFlags & GTF_ALL_EFFECT;
        }
    }

    // A small set of types are unary operators with optional arguments.  We use
    // this constructor to build those.
    GenTreeOp(genTreeOps oper, var_types type DEBUGARG(bool largeNode = false))
        : GenTreeUnOp(oper, type DEBUGARG(largeNode)), gtOp2(nullptr)
    {
        // Unary operators with optional arguments:
        assert(oper == GT_NOP || oper == GT_RETURN || oper == GT_RETFILT || OperIsBlk(oper));
    }

#if DEBUGGABLE_GENTREE
    GenTreeOp() : GenTreeUnOp(), gtOp2(nullptr)
    {
    }
#endif
};

struct GenTreeVal : public GenTree
{
    size_t gtVal1;

    GenTreeVal(genTreeOps oper, var_types type, ssize_t val) : GenTree(oper, type), gtVal1(val)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeVal() : GenTree()
    {
    }
#endif
};

struct GenTreeIntConCommon : public GenTree
{
    inline INT64 LngValue();
    inline void SetLngValue(INT64 val);
    inline ssize_t IconValue();
    inline void SetIconValue(ssize_t val);

    GenTreeIntConCommon(genTreeOps oper, var_types type DEBUGARG(bool largeNode = false))
        : GenTree(oper, type DEBUGARG(largeNode))
    {
    }

    bool FitsInI32()
    {
        return FitsInI32(IconValue());
    }

    static bool FitsInI32(ssize_t val)
    {
#ifdef _TARGET_64BIT_
        return (int)val == val;
#else
        return true;
#endif
    }

    bool ImmedValNeedsReloc(Compiler* comp);
    bool ImmedValCanBeFolded(Compiler* comp, genTreeOps op);

#ifdef _TARGET_XARCH_
    bool FitsInAddrBase(Compiler* comp);
    bool AddrNeedsReloc(Compiler* comp);
#endif

#if DEBUGGABLE_GENTREE
    GenTreeIntConCommon() : GenTree()
    {
    }
#endif
};

// node representing a read from a physical register
struct GenTreePhysReg : public GenTree
{
    // physregs need a field beyond gtRegNum because
    // gtRegNum indicates the destination (and can be changed)
    // whereas reg indicates the source
    regNumber gtSrcReg;
    GenTreePhysReg(regNumber r, var_types type = TYP_I_IMPL) : GenTree(GT_PHYSREG, type), gtSrcReg(r)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreePhysReg() : GenTree()
    {
    }
#endif
};

#ifndef LEGACY_BACKEND
// gtJumpTable - Switch Jump Table
//
// This node stores a DWORD constant that represents the
// absolute address of a jump table for switches.  The code
// generator uses this table to code the destination for every case
// in an array of addresses which starting position is stored in
// this constant.
struct GenTreeJumpTable : public GenTreeIntConCommon
{
    ssize_t gtJumpTableAddr;

    GenTreeJumpTable(var_types type DEBUGARG(bool largeNode = false))
        : GenTreeIntConCommon(GT_JMPTABLE, type DEBUGARG(largeNode))
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeJumpTable() : GenTreeIntConCommon()
    {
    }
#endif // DEBUG
};
#endif // !LEGACY_BACKEND

/* gtIntCon -- integer constant (GT_CNS_INT) */
struct GenTreeIntCon : public GenTreeIntConCommon
{
    /*
     * This is the GT_CNS_INT struct definition.
     * It's used to hold for both int constants and pointer handle constants.
     * For the 64-bit targets we will only use GT_CNS_INT as it used to represent all the possible sizes
     * For the 32-bit targets we use a GT_CNS_LNG to hold a 64-bit integer constant and GT_CNS_INT for all others.
     * In the future when we retarget the JIT for x86 we should consider eliminating GT_CNS_LNG
     */
    ssize_t gtIconVal; // Must overlap and have the same offset with the gtIconVal field in GenTreeLngCon below.

    /* The InitializeArray intrinsic needs to go back to the newarray statement
       to find the class handle of the array so that we can get its size.  However,
       in ngen mode, the handle in that statement does not correspond to the compile
       time handle (rather it lets you get a handle at run-time).  In that case, we also
       need to store a compile time handle, which goes in this gtCompileTimeHandle field.
    */
    ssize_t gtCompileTimeHandle;

    // TODO-Cleanup: It's not clear what characterizes the cases where the field
    // above is used.  It may be that its uses and those of the "gtFieldSeq" field below
    // are mutually exclusive, and they could be put in a union.  Or else we should separate
    // this type into three subtypes.

    // If this constant represents the offset of one or more fields, "gtFieldSeq" represents that
    // sequence of fields.
    FieldSeqNode* gtFieldSeq;

#if defined(LATE_DISASM)

    /*  If the constant was morphed from some other node,
        these fields enable us to get back to what the node
        originally represented. See use of gtNewIconHandleNode()
     */

    union {
        /* Template struct - The significant field of the other
         * structs should overlap exactly with this struct
         */

        struct
        {
            unsigned gtIconHdl1;
            void*    gtIconHdl2;
        } gtIconHdl;

        /* GT_FIELD, etc */

        struct
        {
            unsigned             gtIconCPX;
            CORINFO_CLASS_HANDLE gtIconCls;
        } gtIconFld;
    };
#endif

    GenTreeIntCon(var_types type, ssize_t value DEBUGARG(bool largeNode = false))
        : GenTreeIntConCommon(GT_CNS_INT, type DEBUGARG(largeNode))
        , gtIconVal(value)
        , gtCompileTimeHandle(0)
        , gtFieldSeq(FieldSeqStore::NotAField())
    {
    }

    GenTreeIntCon(var_types type, ssize_t value, FieldSeqNode* fields DEBUGARG(bool largeNode = false))
        : GenTreeIntConCommon(GT_CNS_INT, type DEBUGARG(largeNode))
        , gtIconVal(value)
        , gtCompileTimeHandle(0)
        , gtFieldSeq(fields)
    {
        assert(fields != nullptr);
    }

    void FixupInitBlkValue(var_types asgType);

#ifdef _TARGET_64BIT_
    void TruncateOrSignExtend32()
    {
        if (gtFlags & GTF_UNSIGNED)
        {
            gtIconVal = UINT32(gtIconVal);
        }
        else
        {
            gtIconVal = INT32(gtIconVal);
        }
    }
#endif // _TARGET_64BIT_

#if DEBUGGABLE_GENTREE
    GenTreeIntCon() : GenTreeIntConCommon()
    {
    }
#endif
};

/* gtLngCon -- long    constant (GT_CNS_LNG) */

struct GenTreeLngCon : public GenTreeIntConCommon
{
    INT64 gtLconVal; // Must overlap and have the same offset with the gtIconVal field in GenTreeIntCon above.
    INT32 LoVal()
    {
        return (INT32)(gtLconVal & 0xffffffff);
    }

    INT32 HiVal()
    {
        return (INT32)(gtLconVal >> 32);
        ;
    }

    GenTreeLngCon(INT64 val) : GenTreeIntConCommon(GT_CNS_NATIVELONG, TYP_LONG)
    {
        SetLngValue(val);
    }
#if DEBUGGABLE_GENTREE
    GenTreeLngCon() : GenTreeIntConCommon()
    {
    }
#endif
};

inline INT64 GenTreeIntConCommon::LngValue()
{
#ifndef _TARGET_64BIT_
    assert(gtOper == GT_CNS_LNG);
    return AsLngCon()->gtLconVal;
#else
    return IconValue();
#endif
}

inline void GenTreeIntConCommon::SetLngValue(INT64 val)
{
#ifndef _TARGET_64BIT_
    assert(gtOper == GT_CNS_LNG);
    AsLngCon()->gtLconVal = val;
#else
    // Compile time asserts that these two fields overlap and have the same offsets:  gtIconVal and gtLconVal
    C_ASSERT(offsetof(GenTreeLngCon, gtLconVal) == offsetof(GenTreeIntCon, gtIconVal));
    C_ASSERT(sizeof(AsLngCon()->gtLconVal) == sizeof(AsIntCon()->gtIconVal));

    SetIconValue(ssize_t(val));
#endif
}

inline ssize_t GenTreeIntConCommon::IconValue()
{
    assert(gtOper == GT_CNS_INT); //  We should never see a GT_CNS_LNG for a 64-bit target!
    return AsIntCon()->gtIconVal;
}

inline void GenTreeIntConCommon::SetIconValue(ssize_t val)
{
    assert(gtOper == GT_CNS_INT); //  We should never see a GT_CNS_LNG for a 64-bit target!
    AsIntCon()->gtIconVal = val;
}

/* gtDblCon -- double  constant (GT_CNS_DBL) */

struct GenTreeDblCon : public GenTree
{
    double gtDconVal;

    bool isBitwiseEqual(GenTreeDblCon* other)
    {
        unsigned __int64 bits      = *(unsigned __int64*)(&gtDconVal);
        unsigned __int64 otherBits = *(unsigned __int64*)(&(other->gtDconVal));
        return (bits == otherBits);
    }

    GenTreeDblCon(double val) : GenTree(GT_CNS_DBL, TYP_DOUBLE), gtDconVal(val)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeDblCon() : GenTree()
    {
    }
#endif
};

/* gtStrCon -- string  constant (GT_CNS_STR) */

struct GenTreeStrCon : public GenTree
{
    unsigned              gtSconCPX;
    CORINFO_MODULE_HANDLE gtScpHnd;

    // Because this node can come from an inlined method we need to
    // have the scope handle, since it will become a helper call.
    GenTreeStrCon(unsigned sconCPX, CORINFO_MODULE_HANDLE mod DEBUGARG(bool largeNode = false))
        : GenTree(GT_CNS_STR, TYP_REF DEBUGARG(largeNode)), gtSconCPX(sconCPX), gtScpHnd(mod)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeStrCon() : GenTree()
    {
    }
#endif
};

// Common supertype of LCL_VAR, LCL_FLD, REG_VAR, PHI_ARG
// This inherits from UnOp because lclvar stores are Unops
struct GenTreeLclVarCommon : public GenTreeUnOp
{
private:
    unsigned _gtLclNum; // The local number. An index into the Compiler::lvaTable array.
    unsigned _gtSsaNum; // The SSA number.

public:
    GenTreeLclVarCommon(genTreeOps oper, var_types type, unsigned lclNum DEBUGARG(bool largeNode = false))
        : GenTreeUnOp(oper, type DEBUGARG(largeNode))
    {
        SetLclNum(lclNum);
    }

    unsigned GetLclNum() const
    {
        return _gtLclNum;
    }
    __declspec(property(get = GetLclNum)) unsigned gtLclNum;

    void SetLclNum(unsigned lclNum)
    {
        _gtLclNum = lclNum;
        _gtSsaNum = SsaConfig::RESERVED_SSA_NUM;
    }

    unsigned GetSsaNum() const
    {
        return _gtSsaNum;
    }
    __declspec(property(get = GetSsaNum)) unsigned gtSsaNum;

    void SetSsaNum(unsigned ssaNum)
    {
        _gtSsaNum = ssaNum;
    }

    bool HasSsaName()
    {
        return (gtSsaNum != SsaConfig::RESERVED_SSA_NUM);
    }

#if DEBUGGABLE_GENTREE
    GenTreeLclVarCommon() : GenTreeUnOp()
    {
    }
#endif
};

// gtLclVar -- load/store/addr of local variable

struct GenTreeLclVar : public GenTreeLclVarCommon
{
    IL_OFFSET gtLclILoffs; // instr offset of ref (only for debug info)

    GenTreeLclVar(var_types type, unsigned lclNum, IL_OFFSET ilOffs DEBUGARG(bool largeNode = false))
        : GenTreeLclVarCommon(GT_LCL_VAR, type, lclNum DEBUGARG(largeNode)), gtLclILoffs(ilOffs)
    {
    }

    GenTreeLclVar(genTreeOps oper, var_types type, unsigned lclNum, IL_OFFSET ilOffs DEBUGARG(bool largeNode = false))
        : GenTreeLclVarCommon(oper, type, lclNum DEBUGARG(largeNode)), gtLclILoffs(ilOffs)
    {
        assert(OperIsLocal(oper) || OperIsLocalAddr(oper));
    }

#if DEBUGGABLE_GENTREE
    GenTreeLclVar() : GenTreeLclVarCommon()
    {
    }
#endif
};

// gtLclFld -- load/store/addr of local variable field

struct GenTreeLclFld : public GenTreeLclVarCommon
{
    unsigned gtLclOffs; // offset into the variable to access

    FieldSeqNode* gtFieldSeq; // This LclFld node represents some sequences of accesses.

    // old/FE style constructor where load/store/addr share same opcode
    GenTreeLclFld(var_types type, unsigned lclNum, unsigned lclOffs)
        : GenTreeLclVarCommon(GT_LCL_FLD, type, lclNum), gtLclOffs(lclOffs), gtFieldSeq(nullptr)
    {
        assert(sizeof(*this) <= s_gtNodeSizes[GT_LCL_FLD]);
    }

    GenTreeLclFld(genTreeOps oper, var_types type, unsigned lclNum, unsigned lclOffs)
        : GenTreeLclVarCommon(oper, type, lclNum), gtLclOffs(lclOffs), gtFieldSeq(nullptr)
    {
        assert(sizeof(*this) <= s_gtNodeSizes[GT_LCL_FLD]);
    }
#if DEBUGGABLE_GENTREE
    GenTreeLclFld() : GenTreeLclVarCommon()
    {
    }
#endif
};

struct GenTreeRegVar : public GenTreeLclVarCommon
{
    // TODO-Cleanup: Note that the base class GenTree already has a gtRegNum field.
    // It's not clear exactly why a GT_REG_VAR has a separate field. When
    // GT_REG_VAR is created, the two are identical. It appears that they may
    // or may not remain so. In particular, there is a comment in stackfp.cpp
    // that states:
    //
    //      There used to be an assertion: assert(src->gtRegNum == src->gtRegVar.gtRegNum, ...)
    //      here, but there's actually no reason to assume that.  AFAICT, for FP vars under stack FP,
    //      src->gtRegVar.gtRegNum is the allocated stack pseudo-register, but src->gtRegNum is the
    //      FP stack position into which that is loaded to represent a particular use of the variable.
    //
    // It might be the case that only for stackfp do they ever differ.
    //
    // The following might be possible: the GT_REG_VAR node has a last use prior to a complex
    // subtree being evaluated. It could then be spilled from the register. Later,
    // it could be unspilled into a different register, which would be recorded at
    // the unspill time in the GenTree::gtRegNum, whereas GenTreeRegVar::gtRegNum
    // is left alone. It's not clear why that is useful.
    //
    // Assuming there is a particular use, like stack fp, that requires it, maybe we
    // can get rid of GT_REG_VAR and just leave it as GT_LCL_VAR, using the base class gtRegNum field.
    // If we need it for stackfp, we could add a GenTreeStackFPRegVar type, which carries both the
    // pieces of information, in a clearer and more specific way (in particular, with
    // a different member name).
    //

private:
    regNumberSmall _gtRegNum;

public:
    GenTreeRegVar(var_types type, unsigned lclNum, regNumber regNum) : GenTreeLclVarCommon(GT_REG_VAR, type, lclNum)
    {
        gtRegNum = regNum;
    }

    // The register number is stored in a small format (8 bits), but the getters return and the setters take
    // a full-size (unsigned) format, to localize the casts here.

    __declspec(property(get = GetRegNum, put = SetRegNum)) regNumber gtRegNum;

    regNumber GetRegNum() const
    {
        return (regNumber)_gtRegNum;
    }

    void SetRegNum(regNumber reg)
    {
        _gtRegNum = (regNumberSmall)reg;
        assert(_gtRegNum == reg);
    }

#if DEBUGGABLE_GENTREE
    GenTreeRegVar() : GenTreeLclVarCommon()
    {
    }
#endif
};

/* gtCast -- conversion to a different type  (GT_CAST) */

struct GenTreeCast : public GenTreeOp
{
    GenTreePtr& CastOp()
    {
        return gtOp1;
    }
    var_types gtCastType;

    GenTreeCast(var_types type, GenTreePtr op, var_types castType DEBUGARG(bool largeNode = false))
        : GenTreeOp(GT_CAST, type, op, nullptr DEBUGARG(largeNode)), gtCastType(castType)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeCast() : GenTreeOp()
    {
    }
#endif
};

// GT_BOX nodes are place markers for boxed values.  The "real" tree
// for most purposes is in gtBoxOp.
struct GenTreeBox : public GenTreeUnOp
{
    // An expanded helper call to implement the "box" if we don't get
    // rid of it any other way.  Must be in same position as op1.

    GenTreePtr& BoxOp()
    {
        return gtOp1;
    }
    // This is the statement that contains the assignment tree when the node is an inlined GT_BOX on a value
    // type
    GenTreePtr gtAsgStmtWhenInlinedBoxValue;

    GenTreeBox(var_types type, GenTreePtr boxOp, GenTreePtr asgStmtWhenInlinedBoxValue)
        : GenTreeUnOp(GT_BOX, type, boxOp), gtAsgStmtWhenInlinedBoxValue(asgStmtWhenInlinedBoxValue)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeBox() : GenTreeUnOp()
    {
    }
#endif
};

/* gtField  -- data member ref  (GT_FIELD) */

struct GenTreeField : public GenTree
{
    GenTreePtr           gtFldObj;
    CORINFO_FIELD_HANDLE gtFldHnd;
    DWORD                gtFldOffset;
    bool                 gtFldMayOverlap;
#ifdef FEATURE_READYTORUN_COMPILER
    CORINFO_CONST_LOOKUP gtFieldLookup;
#endif

    GenTreeField(var_types type) : GenTree(GT_FIELD, type)
    {
        gtFldMayOverlap = false;
    }
#if DEBUGGABLE_GENTREE
    GenTreeField() : GenTree()
    {
    }
#endif
};

// Represents the Argument list of a call node, as a Lisp-style linked list.
// (Originally I had hoped that this could have *only* the m_arg/m_rest fields, but it turns out
// that enough of the GenTree mechanism is used that it makes sense just to make it a subtype.  But
// note that in many ways, this is *not* a "real" node of the tree, but rather a mechanism for
// giving call nodes a flexible number of children.  GenTreeArgListNodes never evaluate to registers,
// for example.)

// Note that while this extends GenTreeOp, it is *not* an EXOP.  We don't add any new fields, and one
// is free to allocate a GenTreeOp of type GT_LIST.  If you use this type, you get the convenient Current/Rest
// method names for the arguments.
struct GenTreeArgList : public GenTreeOp
{
    GenTreePtr& Current()
    {
        return gtOp1;
    }
    GenTreeArgList*& Rest()
    {
        assert(gtOp2 == nullptr || gtOp2->OperIsAnyList());
        return *reinterpret_cast<GenTreeArgList**>(&gtOp2);
    }

#if DEBUGGABLE_GENTREE
    GenTreeArgList() : GenTreeOp()
    {
    }
#endif

    GenTreeArgList(GenTreePtr arg) : GenTreeArgList(arg, nullptr)
    {
    }

    GenTreeArgList(GenTreePtr arg, GenTreeArgList* rest) : GenTreeArgList(GT_LIST, arg, rest)
    {
    }

    GenTreeArgList(genTreeOps oper, GenTreePtr arg, GenTreeArgList* rest) : GenTreeOp(oper, TYP_VOID, arg, rest)
    {
        assert(OperIsAnyList(oper));
        assert((arg != nullptr) && arg->IsValidCallArgument());
        gtFlags |= arg->gtFlags & GTF_ALL_EFFECT;
        if (rest != nullptr)
        {
            gtFlags |= rest->gtFlags & GTF_ALL_EFFECT;
        }
    }
};

// Represents a list of fields constituting a struct, when it is passed as an argument.
// The first field of the struct is marked with the GTF_FIELD_LIST_HEAD flag, and
// in LIR form it is the only member of the list that is threaded into the execution
// order.
// It differs from the GenTreeArgList in a couple of ways:
// - The entire list represents a single argument.
// - It contains additional fields to provide the offset and type of the field.
//
struct GenTreeFieldList : public GenTreeArgList
{
    unsigned  gtFieldOffset;
    var_types gtFieldType;

    bool IsFieldListHead() const
    {
        return (gtFlags & GTF_FIELD_LIST_HEAD) != 0;
    }

#if DEBUGGABLE_GENTREE
    GenTreeFieldList() : GenTreeArgList()
    {
    }
#endif

    GenTreeFieldList*& Rest()
    {
        assert(gtOp2 == nullptr || gtOp2->OperGet() == GT_FIELD_LIST);
        return *reinterpret_cast<GenTreeFieldList**>(&gtOp2);
    }

    GenTreeFieldList(GenTreePtr arg, unsigned fieldOffset, var_types fieldType, GenTreeFieldList* prevList)
        : GenTreeArgList(GT_FIELD_LIST, arg, nullptr)
    {
        // While GT_FIELD_LIST can be in a GT_LIST, GT_FIELD_LISTs cannot be nested or have GT_LISTs.
        assert(!arg->OperIsAnyList());
        gtFieldOffset = fieldOffset;
        gtFieldType   = fieldType;
        if (prevList == nullptr)
        {
            gtFlags |= GTF_FIELD_LIST_HEAD;
        }
        else
        {
            prevList->gtOp2 = this;
        }
    }
};

// There was quite a bit of confusion in the code base about which of gtOp1 and gtOp2 was the
// 'then' and 'else' clause of a colon node.  Adding these accessors, while not enforcing anything,
// at least *allows* the programmer to be obviously correct.
// However, these conventions seem backward.
// TODO-Cleanup: If we could get these accessors used everywhere, then we could switch them.
struct GenTreeColon : public GenTreeOp
{
    GenTreePtr& ThenNode()
    {
        return gtOp2;
    }
    GenTreePtr& ElseNode()
    {
        return gtOp1;
    }

#if DEBUGGABLE_GENTREE
    GenTreeColon() : GenTreeOp()
    {
    }
#endif

    GenTreeColon(var_types typ, GenTreePtr thenNode, GenTreePtr elseNode) : GenTreeOp(GT_COLON, typ, elseNode, thenNode)
    {
    }
};

// gtCall   -- method call      (GT_CALL)
typedef class fgArgInfo* fgArgInfoPtr;
enum class InlineObservation;

// Return type descriptor of a GT_CALL node.
// x64 Unix, Arm64, Arm32 and x86 allow a value to be returned in multiple
// registers. For such calls this struct provides the following info
// on their return type
//    - type of value returned in each return register
//    - ABI return register numbers in which the value is returned
//    - count of return registers in which the value is returned
//
// TODO-ARM: Update this to meet the needs of Arm64 and Arm32
//
// TODO-AllArch: Right now it is used for describing multi-reg returned types.
// Eventually we would want to use it for describing even single-reg
// returned types (e.g. structs returned in single register x64/arm).
// This would allow us not to lie or normalize single struct return
// values in importer/morph.
struct ReturnTypeDesc
{
private:
    var_types m_regType[MAX_RET_REG_COUNT];

#ifdef DEBUG
    bool m_inited;
#endif

public:
    ReturnTypeDesc()
    {
        Reset();
    }

    // Initialize the Return Type Descriptor for a method that returns a struct type
    void InitializeStructReturnType(Compiler* comp, CORINFO_CLASS_HANDLE retClsHnd);

    // Initialize the Return Type Descriptor for a method that returns a TYP_LONG
    // Only needed for X86
    void InitializeLongReturnType(Compiler* comp);

    // Reset type descriptor to defaults
    void Reset()
    {
        for (unsigned i = 0; i < MAX_RET_REG_COUNT; ++i)
        {
            m_regType[i] = TYP_UNKNOWN;
        }
#ifdef DEBUG
        m_inited = false;
#endif
    }

    //--------------------------------------------------------------------------------------------
    // GetReturnRegCount:  Get the count of return registers in which the return value is returned.
    //
    // Arguments:
    //    None
    //
    // Return Value:
    //   Count of return registers.
    //   Returns 0 if the return type is not returned in registers.
    unsigned GetReturnRegCount() const
    {
        assert(m_inited);

        int regCount = 0;
        for (unsigned i = 0; i < MAX_RET_REG_COUNT; ++i)
        {
            if (m_regType[i] == TYP_UNKNOWN)
            {
                break;
            }
            // otherwise
            regCount++;
        }

#ifdef DEBUG
        // Any remaining elements in m_regTypes[] should also be TYP_UNKNOWN
        for (unsigned i = regCount + 1; i < MAX_RET_REG_COUNT; ++i)
        {
            assert(m_regType[i] == TYP_UNKNOWN);
        }
#endif

        return regCount;
    }

    //-----------------------------------------------------------------------
    // IsMultiRegRetType: check whether the type is returned in multiple
    // return registers.
    //
    // Arguments:
    //    None
    //
    // Return Value:
    //    Returns true if the type is returned in multiple return registers.
    //    False otherwise.
    // Note that we only have to examine the first two values to determine this
    //
    bool IsMultiRegRetType() const
    {
        if (MAX_RET_REG_COUNT < 2)
        {
            return false;
        }
        else
        {
            return ((m_regType[0] != TYP_UNKNOWN) && (m_regType[1] != TYP_UNKNOWN));
        }
    }

    //--------------------------------------------------------------------------
    // GetReturnRegType:  Get var_type of the return register specified by index.
    //
    // Arguments:
    //    index - Index of the return register.
    //            First return register will have an index 0 and so on.
    //
    // Return Value:
    //    var_type of the return register specified by its index.
    //    asserts if the index does not have a valid register return type.

    var_types GetReturnRegType(unsigned index)
    {
        var_types result = m_regType[index];
        assert(result != TYP_UNKNOWN);

        return result;
    }

    // Get ith ABI return register
    regNumber GetABIReturnReg(unsigned idx);

    // Get reg mask of ABI return registers
    regMaskTP GetABIReturnRegs();
};

struct GenTreeCall final : public GenTree
{
    GenTreePtr      gtCallObjp;     // The instance argument ('this' pointer)
    GenTreeArgList* gtCallArgs;     // The list of arguments in original evaluation order
    GenTreeArgList* gtCallLateArgs; // On x86:     The register arguments in an optimal order
                                    // On ARM/x64: - also includes any outgoing arg space arguments
                                    //             - that were evaluated into a temp LclVar
    fgArgInfoPtr fgArgInfo;

#if !FEATURE_FIXED_OUT_ARGS
    int     regArgListCount;
    regList regArgList;
#endif

    // TODO-Throughput: Revisit this (this used to be only defined if
    // FEATURE_FIXED_OUT_ARGS was enabled, so this makes GenTreeCall 4 bytes bigger on x86).
    CORINFO_SIG_INFO* callSig; // Used by tail calls and to register callsites with the EE

#ifdef LEGACY_BACKEND
    regMaskTP gtCallRegUsedMask; // mask of registers used to pass parameters
#endif                           // LEGACY_BACKEND

#if FEATURE_MULTIREG_RET
    // State required to support multi-reg returning call nodes.
    // For now it is enabled only for x64 unix.
    //
    // TODO-AllArch: enable for all call nodes to unify single-reg and multi-reg returns.
    ReturnTypeDesc gtReturnTypeDesc;

    // gtRegNum would always be the first return reg.
    // The following array holds the other reg numbers of multi-reg return.
    regNumber gtOtherRegs[MAX_RET_REG_COUNT - 1];

    // GTF_SPILL or GTF_SPILLED flag on a multi-reg call node indicates that one or
    // more of its result regs are in that state.  The spill flag of each of the
    // return register is stored in the below array.
    unsigned gtSpillFlags[MAX_RET_REG_COUNT];
#endif

    //-----------------------------------------------------------------------
    // GetReturnTypeDesc: get the type descriptor of return value of the call
    //
    // Arguments:
    //    None
    //
    // Returns
    //    Type descriptor of the value returned by call
    //
    // Note:
    //    Right now implemented only for x64 unix and yet to be
    //    implemented for other multi-reg target arch (Arm64/Arm32/x86).
    //
    // TODO-AllArch: enable for all call nodes to unify single-reg and multi-reg returns.
    ReturnTypeDesc* GetReturnTypeDesc()
    {
#if FEATURE_MULTIREG_RET
        return &gtReturnTypeDesc;
#else
        return nullptr;
#endif
    }

    //---------------------------------------------------------------------------
    // GetRegNumByIdx: get ith return register allocated to this call node.
    //
    // Arguments:
    //     idx   -   index of the return register
    //
    // Return Value:
    //     Return regNumber of ith return register of call node.
    //     Returns REG_NA if there is no valid return register for the given index.
    //
    regNumber GetRegNumByIdx(unsigned idx) const
    {
        assert(idx < MAX_RET_REG_COUNT);

        if (idx == 0)
        {
            return gtRegNum;
        }

#if FEATURE_MULTIREG_RET
        return gtOtherRegs[idx - 1];
#else
        return REG_NA;
#endif
    }

    //----------------------------------------------------------------------
    // SetRegNumByIdx: set ith return register of this call node
    //
    // Arguments:
    //    reg    -   reg number
    //    idx    -   index of the return register
    //
    // Return Value:
    //    None
    //
    void SetRegNumByIdx(regNumber reg, unsigned idx)
    {
        assert(idx < MAX_RET_REG_COUNT);

        if (idx == 0)
        {
            gtRegNum = reg;
        }
#if FEATURE_MULTIREG_RET
        else
        {
            gtOtherRegs[idx - 1] = reg;
            assert(gtOtherRegs[idx - 1] == reg);
        }
#else
        unreached();
#endif
    }

    //----------------------------------------------------------------------------
    // ClearOtherRegs: clear multi-reg state to indicate no regs are allocated
    //
    // Arguments:
    //    None
    //
    // Return Value:
    //    None
    //
    void ClearOtherRegs()
    {
#if FEATURE_MULTIREG_RET
        for (unsigned i = 0; i < MAX_RET_REG_COUNT - 1; ++i)
        {
            gtOtherRegs[i] = REG_NA;
        }
#endif
    }

    //----------------------------------------------------------------------------
    // CopyOtherRegs: copy multi-reg state from the given call node to this node
    //
    // Arguments:
    //    fromCall  -  GenTreeCall node from which to copy multi-reg state
    //
    // Return Value:
    //    None
    //
    void CopyOtherRegs(GenTreeCall* fromCall)
    {
#if FEATURE_MULTIREG_RET
        for (unsigned i = 0; i < MAX_RET_REG_COUNT - 1; ++i)
        {
            this->gtOtherRegs[i] = fromCall->gtOtherRegs[i];
        }
#endif
    }

    // Get reg mask of all the valid registers of gtOtherRegs array
    regMaskTP GetOtherRegMask() const;

    //----------------------------------------------------------------------
    // GetRegSpillFlagByIdx: get spill flag associated with the return register
    // specified by its index.
    //
    // Arguments:
    //    idx  -  Position or index of the return register
    //
    // Return Value:
    //    Returns GTF_* flags associated with.
    unsigned GetRegSpillFlagByIdx(unsigned idx) const
    {
        assert(idx < MAX_RET_REG_COUNT);

#if FEATURE_MULTIREG_RET
        return gtSpillFlags[idx];
#else
        assert(!"unreached");
        return 0;
#endif
    }

    //----------------------------------------------------------------------
    // SetRegSpillFlagByIdx: set spill flags for the return register
    // specified by its index.
    //
    // Arguments:
    //    flags  -  GTF_* flags
    //    idx    -  Position or index of the return register
    //
    // Return Value:
    //    None
    void SetRegSpillFlagByIdx(unsigned flags, unsigned idx)
    {
        assert(idx < MAX_RET_REG_COUNT);

#if FEATURE_MULTIREG_RET
        gtSpillFlags[idx] = flags;
#else
        unreached();
#endif
    }

    //-------------------------------------------------------------------
    // clearOtherRegFlags: clear GTF_* flags associated with gtOtherRegs
    //
    // Arguments:
    //     None
    //
    // Return Value:
    //     None
    void ClearOtherRegFlags()
    {
#if FEATURE_MULTIREG_RET
        for (unsigned i = 0; i < MAX_RET_REG_COUNT; ++i)
        {
            gtSpillFlags[i] = 0;
        }
#endif
    }

    //-------------------------------------------------------------------------
    // CopyOtherRegFlags: copy GTF_* flags associated with gtOtherRegs from
    // the given call node.
    //
    // Arguments:
    //    fromCall  -  GenTreeCall node from which to copy
    //
    // Return Value:
    //    None
    //
    void CopyOtherRegFlags(GenTreeCall* fromCall)
    {
#if FEATURE_MULTIREG_RET
        for (unsigned i = 0; i < MAX_RET_REG_COUNT; ++i)
        {
            this->gtSpillFlags[i] = fromCall->gtSpillFlags[i];
        }
#endif
    }

// clang-format off

#define GTF_CALL_M_EXPLICIT_TAILCALL     0x00000001 // GT_CALL -- the call is "tail" prefixed and
                                                    // importer has performed tail call checks
#define GTF_CALL_M_TAILCALL              0x00000002 // GT_CALL -- the call is a tailcall
#define GTF_CALL_M_VARARGS               0x00000004 // GT_CALL -- the call uses varargs ABI
#define GTF_CALL_M_RETBUFFARG            0x00000008 // GT_CALL -- first parameter is the return buffer argument
#define GTF_CALL_M_DELEGATE_INV          0x00000010 // GT_CALL -- call to Delegate.Invoke
#define GTF_CALL_M_NOGCCHECK             0x00000020 // GT_CALL -- not a call for computing full interruptability
#define GTF_CALL_M_SPECIAL_INTRINSIC     0x00000040 // GT_CALL -- function that could be optimized as an intrinsic
                                                    // in special cases. Used to optimize fast way out in morphing
#define GTF_CALL_M_UNMGD_THISCALL        0x00000080 // GT_CALL -- "this" pointer (first argument)
                                                    // should be enregistered (only for GTF_CALL_UNMANAGED)
#define GTF_CALL_M_VIRTSTUB_REL_INDIRECT 0x00000080 // the virtstub is indirected through
                                                    // a relative address (only for GTF_CALL_VIRT_STUB)
#define GTF_CALL_M_NONVIRT_SAME_THIS     0x00000080 // GT_CALL -- callee "this" pointer is
                                                    // equal to caller this pointer (only for GTF_CALL_NONVIRT)
#define GTF_CALL_M_FRAME_VAR_DEATH       0x00000100 // GT_CALL -- the compLvFrameListRoot variable dies here (last use)

#ifndef LEGACY_BACKEND
#define GTF_CALL_M_TAILCALL_VIA_HELPER   0x00000200 // GT_CALL -- call is a tail call dispatched via tail call JIT helper.
#endif

#if FEATURE_TAILCALL_OPT
#define GTF_CALL_M_IMPLICIT_TAILCALL     0x00000400 // GT_CALL -- call is an opportunistic
                                                    // tail call and importer has performed tail call checks
#define GTF_CALL_M_TAILCALL_TO_LOOP      0x00000800 // GT_CALL -- call is a fast recursive tail call
                                                    // that can be converted into a loop
#endif

#define GTF_CALL_M_PINVOKE               0x00001000 // GT_CALL -- call is a pinvoke.  This mirrors VM flag CORINFO_FLG_PINVOKE.
                                                    // A call marked as Pinvoke is not necessarily a GT_CALL_UNMANAGED. For e.g.
                                                    // an IL Stub dynamically generated for a PInvoke declaration is flagged as
                                                    // a Pinvoke but not as an unmanaged call. See impCheckForPInvokeCall() to
                                                    // know when these flags are set.

#define GTF_CALL_M_R2R_REL_INDIRECT      0x00002000 // GT_CALL -- ready to run call is indirected through a relative address
#define GTF_CALL_M_DOES_NOT_RETURN       0x00004000 // GT_CALL -- call does not return
#define GTF_CALL_M_SECURE_DELEGATE_INV   0x00008000 // GT_CALL -- call is in secure delegate
#define GTF_CALL_M_FAT_POINTER_CHECK     0x00010000 // GT_CALL -- CoreRT managed calli needs transformation, that checks
                                                    // special bit in calli address. If it is set, then it is necessary
                                                    // to restore real function address and load hidden argument
                                                    // as the first argument for calli. It is CoreRT replacement for instantiating
                                                    // stubs, because executable code cannot be generated at runtime.

    // clang-format on

    bool IsUnmanaged() const
    {
        return (gtFlags & GTF_CALL_UNMANAGED) != 0;
    }
    bool NeedsNullCheck() const
    {
        return (gtFlags & GTF_CALL_NULLCHECK) != 0;
    }
    bool CallerPop() const
    {
        return (gtFlags & GTF_CALL_POP_ARGS) != 0;
    }
    bool IsVirtual() const
    {
        return (gtFlags & GTF_CALL_VIRT_KIND_MASK) != GTF_CALL_NONVIRT;
    }
    bool IsVirtualStub() const
    {
        return (gtFlags & GTF_CALL_VIRT_KIND_MASK) == GTF_CALL_VIRT_STUB;
    }
    bool IsVirtualVtable() const
    {
        return (gtFlags & GTF_CALL_VIRT_KIND_MASK) == GTF_CALL_VIRT_VTABLE;
    }
    bool IsInlineCandidate() const
    {
        return (gtFlags & GTF_CALL_INLINE_CANDIDATE) != 0;
    }

#ifndef LEGACY_BACKEND
    bool HasNonStandardAddedArgs(Compiler* compiler) const;
    int GetNonStandardAddedArgCount(Compiler* compiler) const;
#endif // !LEGACY_BACKEND

    // Returns true if this call uses a retBuf argument and its calling convention
    bool HasRetBufArg() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_RETBUFFARG) != 0;
    }

    //-------------------------------------------------------------------------
    // TreatAsHasRetBufArg:
    //
    // Arguments:
    //     compiler, the compiler instance so that we can call eeGetHelperNum
    //
    // Return Value:
    //     Returns true if we treat the call as if it has a retBuf argument
    //     This method may actually have a retBuf argument
    //     or it could be a JIT helper that we are still transforming during
    //     the importer phase.
    //
    // Notes:
    //     On ARM64 marking the method with the GTF_CALL_M_RETBUFFARG flag
    //     will make HasRetBufArg() return true, but will also force the
    //     use of register x8 to pass the RetBuf argument.
    //
    bool TreatAsHasRetBufArg(Compiler* compiler) const;

    //-----------------------------------------------------------------------------------------
    // HasMultiRegRetVal: whether the call node returns its value in multiple return registers.
    //
    // Arguments:
    //     None
    //
    // Return Value:
    //     True if the call is returning a multi-reg return value. False otherwise.
    //
    // Note:
    //     This is implemented only for x64 Unix and yet to be implemented for
    //     other multi-reg return target arch (arm64/arm32/x86).
    //
    bool HasMultiRegRetVal() const
    {
#if (defined(_TARGET_X86_) || defined(_TARGET_ARM_)) && !defined(LEGACY_BACKEND)
        // LEGACY_BACKEND does not use multi reg returns for calls with long return types
        return varTypeIsLong(gtType);
#elif FEATURE_MULTIREG_RET
        return varTypeIsStruct(gtType) && !HasRetBufArg();
#else
        return false;
#endif
    }

    // Returns true if VM has flagged this method as CORINFO_FLG_PINVOKE.
    bool IsPInvoke() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_PINVOKE) != 0;
    }

    // Note that the distinction of whether tail prefixed or an implicit tail call
    // is maintained on a call node till fgMorphCall() after which it will be
    // either a tail call (i.e. IsTailCall() is true) or a non-tail call.
    bool IsTailPrefixedCall() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_EXPLICIT_TAILCALL) != 0;
    }

    // This method returning "true" implies that tail call flowgraph morhphing has
    // performed final checks and committed to making a tail call.
    bool IsTailCall() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_TAILCALL) != 0;
    }

    // This method returning "true" implies that importer has performed tail call checks
    // and providing a hint that this can be converted to a tail call.
    bool CanTailCall() const
    {
        return IsTailPrefixedCall() || IsImplicitTailCall();
    }

#ifndef LEGACY_BACKEND
    bool IsTailCallViaHelper() const
    {
        return IsTailCall() && (gtCallMoreFlags & GTF_CALL_M_TAILCALL_VIA_HELPER);
    }
#else  // LEGACY_BACKEND
    bool IsTailCallViaHelper() const
    {
        return true;
    }
#endif // LEGACY_BACKEND

#if FEATURE_FASTTAILCALL
    bool IsFastTailCall() const
    {
        return IsTailCall() && !(gtCallMoreFlags & GTF_CALL_M_TAILCALL_VIA_HELPER);
    }
#else  // !FEATURE_FASTTAILCALL
    bool IsFastTailCall() const
    {
        return false;
    }
#endif // !FEATURE_FASTTAILCALL

#if FEATURE_TAILCALL_OPT
    // Returns true if this is marked for opportunistic tail calling.
    // That is, can be tail called though not explicitly prefixed with "tail" prefix.
    bool IsImplicitTailCall() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_IMPLICIT_TAILCALL) != 0;
    }
    bool IsTailCallConvertibleToLoop() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_TAILCALL_TO_LOOP) != 0;
    }
#else  // !FEATURE_TAILCALL_OPT
    bool IsImplicitTailCall() const
    {
        return false;
    }
    bool IsTailCallConvertibleToLoop() const
    {
        return false;
    }
#endif // !FEATURE_TAILCALL_OPT

    bool IsSameThis() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_NONVIRT_SAME_THIS) != 0;
    }
    bool IsDelegateInvoke() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_DELEGATE_INV) != 0;
    }
    bool IsVirtualStubRelativeIndir() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_VIRTSTUB_REL_INDIRECT) != 0;
    }

#ifdef FEATURE_READYTORUN_COMPILER
    bool IsR2RRelativeIndir() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_R2R_REL_INDIRECT) != 0;
    }
    void setEntryPoint(CORINFO_CONST_LOOKUP entryPoint)
    {
        gtEntryPoint = entryPoint;
        if (gtEntryPoint.accessType == IAT_PVALUE)
        {
            gtCallMoreFlags |= GTF_CALL_M_R2R_REL_INDIRECT;
        }
    }
#endif // FEATURE_READYTORUN_COMPILER

    bool IsVarargs() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_VARARGS) != 0;
    }

    bool IsNoReturn() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_DOES_NOT_RETURN) != 0;
    }

    bool IsFatPointerCandidate() const
    {
        return (gtCallMoreFlags & GTF_CALL_M_FAT_POINTER_CHECK) != 0;
    }

    bool IsPure(Compiler* compiler) const;

    void ClearFatPointerCandidate()
    {
        gtCallMoreFlags &= ~GTF_CALL_M_FAT_POINTER_CHECK;
    }

    void SetFatPointerCandidate()
    {
        gtCallMoreFlags |= GTF_CALL_M_FAT_POINTER_CHECK;
    }

    unsigned gtCallMoreFlags; // in addition to gtFlags

    unsigned char gtCallType : 3;   // value from the gtCallTypes enumeration
    unsigned char gtReturnType : 5; // exact return type

    CORINFO_CLASS_HANDLE gtRetClsHnd; // The return type handle of the call if it is a struct; always available

    union {
        // only used for CALLI unmanaged calls (CT_INDIRECT)
        GenTreePtr gtCallCookie;
        // gtInlineCandidateInfo is only used when inlining methods
        InlineCandidateInfo*   gtInlineCandidateInfo;
        void*                  gtStubCallStubAddr;              // GTF_CALL_VIRT_STUB - these are never inlined
        CORINFO_GENERIC_HANDLE compileTimeHelperArgumentHandle; // Used to track type handle argument of dynamic helpers
        void*                  gtDirectCallAddress; // Used to pass direct call address between lower and codegen
    };

    // expression evaluated after args are placed which determines the control target
    GenTree* gtControlExpr;

    union {
        CORINFO_METHOD_HANDLE gtCallMethHnd; // CT_USER_FUNC
        GenTreePtr            gtCallAddr;    // CT_INDIRECT
    };

#ifdef FEATURE_READYTORUN_COMPILER
    // Call target lookup info for method call from a Ready To Run module
    CORINFO_CONST_LOOKUP gtEntryPoint;
#endif

#if defined(DEBUG) || defined(INLINE_DATA)
    // For non-inline candidates, track the first observation
    // that blocks candidacy.
    InlineObservation gtInlineObservation;

    // IL offset of the call wrt its parent method.
    IL_OFFSET gtRawILOffset;
#endif // defined(DEBUG) || defined(INLINE_DATA)

    bool IsHelperCall() const
    {
        return gtCallType == CT_HELPER;
    }

    bool IsHelperCall(CORINFO_METHOD_HANDLE callMethHnd) const
    {
        return IsHelperCall() && (callMethHnd == gtCallMethHnd);
    }

    bool IsHelperCall(Compiler* compiler, unsigned helper) const;

    void ReplaceCallOperand(GenTree** operandUseEdge, GenTree* replacement);

    bool AreArgsComplete() const;

    GenTreeCall(var_types type) : GenTree(GT_CALL, type)
    {
        fgArgInfo = nullptr;
    }
#if DEBUGGABLE_GENTREE
    GenTreeCall() : GenTree()
    {
    }
#endif
};

struct GenTreeCmpXchg : public GenTree
{
    GenTreePtr gtOpLocation;
    GenTreePtr gtOpValue;
    GenTreePtr gtOpComparand;

    GenTreeCmpXchg(var_types type, GenTreePtr loc, GenTreePtr val, GenTreePtr comparand)
        : GenTree(GT_CMPXCHG, type), gtOpLocation(loc), gtOpValue(val), gtOpComparand(comparand)
    {
        // There's no reason to do a compare-exchange on a local location, so we'll assume that all of these
        // have global effects.
        gtFlags |= GTF_GLOB_EFFECT;
    }
#if DEBUGGABLE_GENTREE
    GenTreeCmpXchg() : GenTree()
    {
    }
#endif
};

#if !defined(LEGACY_BACKEND) && defined(_TARGET_ARM_)
struct GenTreeMulLong : public GenTreeOp
{
    regNumber gtOtherReg;

    GenTreeMulLong(var_types type, GenTreePtr op1, GenTreePtr op2) : GenTreeOp(GT_MUL_LONG, type, op1, op2)
    {
    }

#if DEBUGGABLE_GENTREE
    GenTreeMulLong() : GenTreeOp()
    {
    }
#endif
};
#endif

struct GenTreeFptrVal : public GenTree
{
    CORINFO_METHOD_HANDLE gtFptrMethod;

#ifdef FEATURE_READYTORUN_COMPILER
    CORINFO_CONST_LOOKUP gtEntryPoint;
#endif

    GenTreeFptrVal(var_types type, CORINFO_METHOD_HANDLE meth) : GenTree(GT_FTN_ADDR, type), gtFptrMethod(meth)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeFptrVal() : GenTree()
    {
    }
#endif
};

/* gtQmark */
struct GenTreeQmark : public GenTreeOp
{
    // Livesets on entry to then and else subtrees
    VARSET_TP gtThenLiveSet;
    VARSET_TP gtElseLiveSet;

    // The "Compiler*" argument is not a DEBUGARG here because we use it to keep track of the set of
    // (possible) QMark nodes.
    GenTreeQmark(var_types type, GenTreePtr cond, GenTreePtr colonOp, class Compiler* comp);

#if DEBUGGABLE_GENTREE
    GenTreeQmark() : GenTreeOp(GT_QMARK, TYP_INT, nullptr, nullptr)
    {
    }
#endif
};

/* gtIntrinsic   -- intrinsic   (possibly-binary op [NULL op2 is allowed] with an additional field) */

struct GenTreeIntrinsic : public GenTreeOp
{
    CorInfoIntrinsics     gtIntrinsicId;
    CORINFO_METHOD_HANDLE gtMethodHandle; // Method handle of the method which is treated as an intrinsic.

#ifdef FEATURE_READYTORUN_COMPILER
    // Call target lookup info for method call from a Ready To Run module
    CORINFO_CONST_LOOKUP gtEntryPoint;
#endif

    GenTreeIntrinsic(var_types type, GenTreePtr op1, CorInfoIntrinsics intrinsicId, CORINFO_METHOD_HANDLE methodHandle)
        : GenTreeOp(GT_INTRINSIC, type, op1, nullptr), gtIntrinsicId(intrinsicId), gtMethodHandle(methodHandle)
    {
    }

    GenTreeIntrinsic(var_types             type,
                     GenTreePtr            op1,
                     GenTreePtr            op2,
                     CorInfoIntrinsics     intrinsicId,
                     CORINFO_METHOD_HANDLE methodHandle)
        : GenTreeOp(GT_INTRINSIC, type, op1, op2), gtIntrinsicId(intrinsicId), gtMethodHandle(methodHandle)
    {
    }

#if DEBUGGABLE_GENTREE
    GenTreeIntrinsic() : GenTreeOp()
    {
    }
#endif
};

#ifdef FEATURE_SIMD

/* gtSIMD   -- SIMD intrinsic   (possibly-binary op [NULL op2 is allowed] with additional fields) */
struct GenTreeSIMD : public GenTreeOp
{
    SIMDIntrinsicID gtSIMDIntrinsicID; // operation Id
    var_types       gtSIMDBaseType;    // SIMD vector base type
    unsigned        gtSIMDSize;        // SIMD vector size in bytes

    GenTreeSIMD(var_types type, GenTreePtr op1, SIMDIntrinsicID simdIntrinsicID, var_types baseType, unsigned size)
        : GenTreeOp(GT_SIMD, type, op1, nullptr)
        , gtSIMDIntrinsicID(simdIntrinsicID)
        , gtSIMDBaseType(baseType)
        , gtSIMDSize(size)
    {
    }

    GenTreeSIMD(var_types       type,
                GenTreePtr      op1,
                GenTreePtr      op2,
                SIMDIntrinsicID simdIntrinsicID,
                var_types       baseType,
                unsigned        size)
        : GenTreeOp(GT_SIMD, type, op1, op2)
        , gtSIMDIntrinsicID(simdIntrinsicID)
        , gtSIMDBaseType(baseType)
        , gtSIMDSize(size)
    {
    }

#if DEBUGGABLE_GENTREE
    GenTreeSIMD() : GenTreeOp()
    {
    }
#endif
};
#endif // FEATURE_SIMD

/* gtIndex -- array access */

struct GenTreeIndex : public GenTreeOp
{
    GenTreePtr& Arr()
    {
        return gtOp1;
    }
    GenTreePtr& Index()
    {
        return gtOp2;
    }

    unsigned             gtIndElemSize;     // size of elements in the array
    CORINFO_CLASS_HANDLE gtStructElemClass; // If the element type is a struct, this is the struct type.

    GenTreeIndex(var_types type, GenTreePtr arr, GenTreePtr ind, unsigned indElemSize)
        : GenTreeOp(GT_INDEX, type, arr, ind)
        , gtIndElemSize(indElemSize)
        , gtStructElemClass(nullptr) // We always initialize this after construction.
    {
#ifdef DEBUG
        if (JitConfig.JitSkipArrayBoundCheck() == 1)
        {
            // Skip bounds check
        }
        else
#endif
        {
            // Do bounds check
            gtFlags |= GTF_INX_RNGCHK;
        }

        if (type == TYP_REF)
        {
            gtFlags |= GTF_INX_REFARR_LAYOUT;
        }

        gtFlags |= GTF_EXCEPT | GTF_GLOB_REF;
    }
#if DEBUGGABLE_GENTREE
    GenTreeIndex() : GenTreeOp()
    {
    }
#endif
};

/* gtArrLen -- array length (GT_ARR_LENGTH)
   GT_ARR_LENGTH is used for "arr.length" */

struct GenTreeArrLen : public GenTreeUnOp
{
    GenTreePtr& ArrRef()
    {
        return gtOp1;
    } // the array address node
private:
    int gtArrLenOffset; // constant to add to "gtArrRef" to get the address of the array length.

public:
    inline int ArrLenOffset()
    {
        return gtArrLenOffset;
    }

    GenTreeArrLen(var_types type, GenTreePtr arrRef, int lenOffset)
        : GenTreeUnOp(GT_ARR_LENGTH, type, arrRef), gtArrLenOffset(lenOffset)
    {
    }

#if DEBUGGABLE_GENTREE
    GenTreeArrLen() : GenTreeUnOp()
    {
    }
#endif
};

// This takes:
// - a comparison value (generally an array length),
// - an index value, and
// - the label to jump to if the index is out of range.
// - the "kind" of the throw block to branch to on failure
// It generates no result.

struct GenTreeBoundsChk : public GenTree
{
    GenTreePtr gtIndex;  // The index expression.
    GenTreePtr gtArrLen; // An expression for the length of the array being indexed.

    GenTreePtr      gtIndRngFailBB; // Label to jump to for array-index-out-of-range
    SpecialCodeKind gtThrowKind;    // Kind of throw block to branch to on failure

    /* Only out-of-ranges at same stack depth can jump to the same label (finding return address is easier)
       For delayed calling of fgSetRngChkTarget() so that the
       optimizer has a chance of eliminating some of the rng checks */
    unsigned gtStkDepth;

    GenTreeBoundsChk(genTreeOps oper, var_types type, GenTreePtr index, GenTreePtr arrLen, SpecialCodeKind kind)
        : GenTree(oper, type)
        , gtIndex(index)
        , gtArrLen(arrLen)
        , gtIndRngFailBB(nullptr)
        , gtThrowKind(kind)
        , gtStkDepth(0)
    {
        // Effects flags propagate upwards.
        gtFlags |= (arrLen->gtFlags & GTF_ALL_EFFECT);
        gtFlags |= GTF_EXCEPT;
    }
#if DEBUGGABLE_GENTREE
    GenTreeBoundsChk() : GenTree()
    {
    }
#endif

    // If the gtArrLen is really an array length, returns array reference, else "NULL".
    GenTreePtr GetArray()
    {
        if (gtArrLen->OperGet() == GT_ARR_LENGTH)
        {
            return gtArrLen->gtArrLen.ArrRef();
        }
        else
        {
            return nullptr;
        }
    }
};

// gtArrElem -- general array element (GT_ARR_ELEM), for non "SZ_ARRAYS"
//              -- multidimensional arrays, or 1-d arrays with non-zero lower bounds.

struct GenTreeArrElem : public GenTree
{
    GenTreePtr gtArrObj;

#define GT_ARR_MAX_RANK 3
    GenTreePtr    gtArrInds[GT_ARR_MAX_RANK]; // Indices
    unsigned char gtArrRank;                  // Rank of the array

    unsigned char gtArrElemSize; // !!! Caution, this is an "unsigned char", it is used only
                                 // on the optimization path of array intrisics.
                                 // It stores the size of array elements WHEN it can fit
                                 // into an "unsigned char".
                                 // This has caused VSW 571394.
    var_types gtArrElemType;     // The array element type

    // Requires that "inds" is a pointer to an array of "rank" GenTreePtrs for the indices.
    GenTreeArrElem(var_types     type,
                   GenTreePtr    arr,
                   unsigned char rank,
                   unsigned char elemSize,
                   var_types     elemType,
                   GenTreePtr*   inds)
        : GenTree(GT_ARR_ELEM, type), gtArrObj(arr), gtArrRank(rank), gtArrElemSize(elemSize), gtArrElemType(elemType)
    {
        for (unsigned char i = 0; i < rank; i++)
        {
            gtArrInds[i] = inds[i];
        }
        gtFlags |= GTF_EXCEPT;
    }
#if DEBUGGABLE_GENTREE
    GenTreeArrElem() : GenTree()
    {
    }
#endif
};

//--------------------------------------------
//
// GenTreeArrIndex (gtArrIndex): Expression to bounds-check the index for one dimension of a
//    multi-dimensional or non-zero-based array., and compute the effective index
//    (i.e. subtracting the lower bound).
//
// Notes:
//    This node is similar in some ways to GenTreeBoundsChk, which ONLY performs the check.
//    The reason that this node incorporates the check into the effective index computation is
//    to avoid duplicating the codegen, as the effective index is required to compute the
//    offset anyway.
//    TODO-CQ: Enable optimization of the lower bound and length by replacing this:
//                /--*  <arrObj>
//                +--*  <index0>
//             +--* ArrIndex[i, ]
//    with something like:
//                   /--*  <arrObj>
//                /--*  ArrLowerBound[i, ]
//                |  /--*  <arrObj>
//                +--*  ArrLen[i, ]    (either generalize GT_ARR_LENGTH or add a new node)
//                +--*  <index0>
//             +--* ArrIndex[i, ]
//    Which could, for example, be optimized to the following when known to be within bounds:
//                /--*  TempForLowerBoundDim0
//                +--*  <index0>
//             +--* - (GT_SUB)
//
struct GenTreeArrIndex : public GenTreeOp
{
    // The array object - may be any expression producing an Array reference, but is likely to be a lclVar.
    GenTreePtr& ArrObj()
    {
        return gtOp1;
    }
    // The index expression - may be any integral expression.
    GenTreePtr& IndexExpr()
    {
        return gtOp2;
    }
    unsigned char gtCurrDim;     // The current dimension
    unsigned char gtArrRank;     // Rank of the array
    var_types     gtArrElemType; // The array element type

    GenTreeArrIndex(var_types     type,
                    GenTreePtr    arrObj,
                    GenTreePtr    indexExpr,
                    unsigned char currDim,
                    unsigned char arrRank,
                    var_types     elemType)
        : GenTreeOp(GT_ARR_INDEX, type, arrObj, indexExpr)
        , gtCurrDim(currDim)
        , gtArrRank(arrRank)
        , gtArrElemType(elemType)
    {
        gtFlags |= GTF_EXCEPT;
    }
#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    // Used only for GenTree::GetVtableForOper()
    GenTreeArrIndex() : GenTreeOp()
    {
    }
#endif
};

//--------------------------------------------
//
// GenTreeArrOffset (gtArrOffset): Expression to compute the accumulated offset for the address
//    of an element of a multi-dimensional or non-zero-based array.
//
// Notes:
//    The result of this expression is (gtOffset * dimSize) + gtIndex
//    where dimSize is the length/stride/size of the dimension, and is obtained from gtArrObj.
//    This node is generated in conjunction with the GenTreeArrIndex node, which computes the
//    effective index for a single dimension.  The sub-trees can be separately optimized, e.g.
//    within a loop body where the expression for the 0th dimension may be invariant.
//
//    Here is an example of how the tree might look for a two-dimension array reference:
//                /--*  const 0
//                |  /--* <arrObj>
//                |  +--* <index0>
//                +--* ArrIndex[i, ]
//                +--*  <arrObj>
//             /--| arrOffs[i, ]
//             |  +--*  <arrObj>
//             |  +--*  <index1>
//             +--* ArrIndex[*,j]
//             +--*  <arrObj>
//          /--| arrOffs[*,j]
//    TODO-CQ: see comment on GenTreeArrIndex for how its representation may change.  When that
//    is done, we will also want to replace the <arrObj> argument to arrOffs with the
//    ArrLen as for GenTreeArrIndex.
//
struct GenTreeArrOffs : public GenTree
{
    GenTreePtr gtOffset;         // The accumulated offset for lower dimensions - must be TYP_I_IMPL, and
                                 // will either be a CSE temp, the constant 0, or another GenTreeArrOffs node.
    GenTreePtr gtIndex;          // The effective index for the current dimension - must be non-negative
                                 // and can be any expression (though it is likely to be either a GenTreeArrIndex,
                                 // node, a lclVar, or a constant).
    GenTreePtr gtArrObj;         // The array object - may be any expression producing an Array reference,
                                 // but is likely to be a lclVar.
    unsigned char gtCurrDim;     // The current dimension
    unsigned char gtArrRank;     // Rank of the array
    var_types     gtArrElemType; // The array element type

    GenTreeArrOffs(var_types     type,
                   GenTreePtr    offset,
                   GenTreePtr    index,
                   GenTreePtr    arrObj,
                   unsigned char currDim,
                   unsigned char rank,
                   var_types     elemType)
        : GenTree(GT_ARR_OFFSET, type)
        , gtOffset(offset)
        , gtIndex(index)
        , gtArrObj(arrObj)
        , gtCurrDim(currDim)
        , gtArrRank(rank)
        , gtArrElemType(elemType)
    {
        assert(index->gtFlags & GTF_EXCEPT);
        gtFlags |= GTF_EXCEPT;
    }
#if DEBUGGABLE_GENTREE
    GenTreeArrOffs() : GenTree()
    {
    }
#endif
};

/* gtAddrMode -- Target-specific canonicalized addressing expression (GT_LEA) */

struct GenTreeAddrMode : public GenTreeOp
{
    // Address is Base + Index*Scale + Offset.
    // These are the legal patterns:
    //
    //      Base                                // Base != nullptr && Index == nullptr && Scale == 0 && Offset == 0
    //      Base + Index*Scale                  // Base != nullptr && Index != nullptr && Scale != 0 && Offset == 0
    //      Base + Offset                       // Base != nullptr && Index == nullptr && Scale == 0 && Offset != 0
    //      Base + Index*Scale + Offset         // Base != nullptr && Index != nullptr && Scale != 0 && Offset != 0
    //             Index*Scale                  // Base == nullptr && Index != nullptr && Scale >  1 && Offset == 0
    //             Index*Scale + Offset         // Base == nullptr && Index != nullptr && Scale >  1 && Offset != 0
    //                           Offset         // Base == nullptr && Index == nullptr && Scale == 0 && Offset != 0
    //
    // So, for example:
    //      1. Base + Index is legal with Scale==1
    //      2. If Index is null, Scale should be zero (or unintialized / unused)
    //      3. If Scale==1, then we should have "Base" instead of "Index*Scale", and "Base + Offset" instead of
    //         "Index*Scale + Offset".

    // First operand is base address/pointer
    bool HasBase() const
    {
        return gtOp1 != nullptr;
    }
    GenTreePtr& Base()
    {
        return gtOp1;
    }

    // Second operand is scaled index value
    bool HasIndex() const
    {
        return gtOp2 != nullptr;
    }
    GenTreePtr& Index()
    {
        return gtOp2;
    }

    unsigned gtScale;  // The scale factor
    unsigned gtOffset; // The offset to add

    GenTreeAddrMode(var_types type, GenTreePtr base, GenTreePtr index, unsigned scale, unsigned offset)
        : GenTreeOp(GT_LEA, type, base, index)
    {
        assert(base != nullptr || index != nullptr);
        gtScale  = scale;
        gtOffset = offset;
    }
#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    // Used only for GenTree::GetVtableForOper()
    GenTreeAddrMode() : GenTreeOp()
    {
    }
#endif
};

// Indir is just an op, no additional data, but some additional abstractions
struct GenTreeIndir : public GenTreeOp
{
    // The address for the indirection.
    // Since GenTreeDynBlk derives from this, but is an "EXOP" (i.e. it has extra fields),
    // we can't access Op1 and Op2 in the normal manner if we may have a DynBlk.
    GenTreePtr& Addr()
    {
        return gtOp1;
    }

    // these methods provide an interface to the indirection node which
    bool     HasBase();
    bool     HasIndex();
    GenTree* Base();
    GenTree* Index();
    unsigned Scale();
    size_t   Offset();

    GenTreeIndir(genTreeOps oper, var_types type, GenTree* addr, GenTree* data) : GenTreeOp(oper, type, addr, data)
    {
    }

#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    // Used only for GenTree::GetVtableForOper()
    GenTreeIndir() : GenTreeOp()
    {
    }
#endif
};

// gtBlk  -- 'block' (GT_BLK, GT_STORE_BLK).
//
// This is the base type for all of the nodes that represent block or struct
// values.
// Since it can be a store, it includes gtBlkOpKind to specify the type of
// code generation that will be used for the block operation.

struct GenTreeBlk : public GenTreeIndir
{
public:
    // The data to be stored (null for GT_BLK)
    GenTree*& Data()
    {
        return gtOp2;
    }
    void SetData(GenTree* dataNode)
    {
        gtOp2 = dataNode;
    }

    // The size of the buffer to be copied.
    unsigned Size() const
    {
        return gtBlkSize;
    }

    unsigned gtBlkSize;

    // Return true iff the object being copied contains one or more GC pointers.
    bool HasGCPtr();

    // True if this BlkOpNode is a volatile memory operation.
    bool IsVolatile() const
    {
        return (gtFlags & GTF_BLK_VOLATILE) != 0;
    }

    // True if this BlkOpNode is an unaligned memory operation.
    bool IsUnaligned() const
    {
        return (gtFlags & GTF_BLK_UNALIGNED) != 0;
    }

    // Instruction selection: during codegen time, what code sequence we will be using
    // to encode this operation.
    enum
    {
        BlkOpKindInvalid,
        BlkOpKindHelper,
        BlkOpKindRepInstr,
        BlkOpKindUnroll,
    } gtBlkOpKind;

    bool gtBlkOpGcUnsafe;

    GenTreeBlk(genTreeOps oper, var_types type, GenTreePtr addr, unsigned size)
        : GenTreeIndir(oper, type, addr, nullptr)
        , gtBlkSize(size)
        , gtBlkOpKind(BlkOpKindInvalid)
        , gtBlkOpGcUnsafe(false)
    {
        assert(OperIsBlk(oper));
        gtFlags |= (addr->gtFlags & GTF_ALL_EFFECT);
    }

    GenTreeBlk(genTreeOps oper, var_types type, GenTreePtr addr, GenTreePtr data, unsigned size)
        : GenTreeIndir(oper, type, addr, data), gtBlkSize(size), gtBlkOpKind(BlkOpKindInvalid), gtBlkOpGcUnsafe(false)
    {
        assert(OperIsBlk(oper));
        gtFlags |= (addr->gtFlags & GTF_ALL_EFFECT);
        gtFlags |= (data->gtFlags & GTF_ALL_EFFECT);
    }

#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    GenTreeBlk() : GenTreeIndir()
    {
    }
#endif // DEBUGGABLE_GENTREE
};

// gtObj  -- 'object' (GT_OBJ).
//
// This node is used for block values that may have GC pointers.

struct GenTreeObj : public GenTreeBlk
{
    CORINFO_CLASS_HANDLE gtClass; // the class of the object

    // If non-null, this array represents the gc-layout of the class.
    // This may be simply copied when cloning this node, because it is not changed once computed.
    BYTE* gtGcPtrs;

    // If non-zero, this is the number of slots in the class layout that
    // contain gc-pointers.
    __declspec(property(get = GetGcPtrCount)) unsigned gtGcPtrCount;
    unsigned GetGcPtrCount() const
    {
        assert(_gtGcPtrCount != UINT32_MAX);
        return _gtGcPtrCount;
    }
    unsigned _gtGcPtrCount;

    // If non-zero, the number of pointer-sized slots that constitutes the class token.
    unsigned gtSlots;

    bool IsGCInfoInitialized()
    {
        return (_gtGcPtrCount != UINT32_MAX);
    }

    void SetGCInfo(BYTE* gcPtrs, unsigned gcPtrCount, unsigned slots)
    {
        gtGcPtrs      = gcPtrs;
        _gtGcPtrCount = gcPtrCount;
        gtSlots       = slots;
        if (gtGcPtrCount != 0)
        {
            // We assume that we cannot have a struct with GC pointers that is not a multiple
            // of the register size.
            // The EE currently does not allow this, but it could change.
            // Let's assert it just to be safe.
            noway_assert(roundUp(gtBlkSize, REGSIZE_BYTES) == gtBlkSize);
        }
        else
        {
            genTreeOps newOper = GT_BLK;
            if (gtOper == GT_STORE_OBJ)
            {
                newOper = GT_STORE_BLK;
            }
            else
            {
                assert(gtOper == GT_OBJ);
            }
            SetOper(newOper);
        }
    }

    void CopyGCInfo(GenTreeObj* srcObj)
    {
        if (srcObj->IsGCInfoInitialized())
        {
            gtGcPtrs      = srcObj->gtGcPtrs;
            _gtGcPtrCount = srcObj->gtGcPtrCount;
            gtSlots       = srcObj->gtSlots;
        }
    }

    GenTreeObj(var_types type, GenTreePtr addr, CORINFO_CLASS_HANDLE cls, unsigned size)
        : GenTreeBlk(GT_OBJ, type, addr, size), gtClass(cls)
    {
        // By default, an OBJ is assumed to be a global reference.
        gtFlags |= GTF_GLOB_REF;
        noway_assert(cls != NO_CLASS_HANDLE);
        _gtGcPtrCount = UINT32_MAX;
    }

    GenTreeObj(var_types type, GenTreePtr addr, GenTreePtr data, CORINFO_CLASS_HANDLE cls, unsigned size)
        : GenTreeBlk(GT_STORE_OBJ, type, addr, data, size), gtClass(cls)
    {
        // By default, an OBJ is assumed to be a global reference.
        gtFlags |= GTF_GLOB_REF;
        noway_assert(cls != NO_CLASS_HANDLE);
        _gtGcPtrCount = UINT32_MAX;
    }

#if DEBUGGABLE_GENTREE
    GenTreeObj() : GenTreeBlk()
    {
    }
#endif
};

// gtDynBlk  -- 'dynamic block' (GT_DYN_BLK).
//
// This node is used for block values that have a dynamic size.
// Note that such a value can never have GC pointers.

struct GenTreeDynBlk : public GenTreeBlk
{
public:
    GenTreePtr gtDynamicSize;
    bool       gtEvalSizeFirst;

    GenTreeDynBlk(GenTreePtr addr, GenTreePtr dynamicSize)
        : GenTreeBlk(GT_DYN_BLK, TYP_STRUCT, addr, 0), gtDynamicSize(dynamicSize), gtEvalSizeFirst(false)
    {
        // Conservatively the 'addr' could be null or point into the global heap.
        gtFlags |= GTF_EXCEPT | GTF_GLOB_REF;
        gtFlags |= (dynamicSize->gtFlags & GTF_ALL_EFFECT);
    }

#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    GenTreeDynBlk() : GenTreeBlk()
    {
    }
#endif // DEBUGGABLE_GENTREE
};

// Read-modify-write status of a RMW memory op rooted at a storeInd
enum RMWStatus
{
    STOREIND_RMW_STATUS_UNKNOWN, // RMW status of storeInd unknown
                                 // Default status unless modified by IsRMWMemOpRootedAtStoreInd()

    // One of these denote storeind is a RMW memory operation.
    STOREIND_RMW_DST_IS_OP1, // StoreInd is known to be a RMW memory op and dst candidate is op1
    STOREIND_RMW_DST_IS_OP2, // StoreInd is known to be a RMW memory op and dst candidate is op2

    // One of these denote the reason for storeind is marked as non-RMW operation
    STOREIND_RMW_UNSUPPORTED_ADDR, // Addr mode is not yet supported for RMW memory
    STOREIND_RMW_UNSUPPORTED_OPER, // Operation is not supported for RMW memory
    STOREIND_RMW_UNSUPPORTED_TYPE, // Type is not supported for RMW memory
    STOREIND_RMW_INDIR_UNEQUAL     // Indir to read value is not equivalent to indir that writes the value
};

// StoreInd is just a BinOp, with additional RMW status
struct GenTreeStoreInd : public GenTreeIndir
{
#if !CPU_LOAD_STORE_ARCH
    // The below flag is set and used during lowering
    RMWStatus gtRMWStatus;

    bool IsRMWStatusUnknown()
    {
        return gtRMWStatus == STOREIND_RMW_STATUS_UNKNOWN;
    }
    bool IsNonRMWMemoryOp()
    {
        return gtRMWStatus == STOREIND_RMW_UNSUPPORTED_ADDR || gtRMWStatus == STOREIND_RMW_UNSUPPORTED_OPER ||
               gtRMWStatus == STOREIND_RMW_UNSUPPORTED_TYPE || gtRMWStatus == STOREIND_RMW_INDIR_UNEQUAL;
    }
    bool IsRMWMemoryOp()
    {
        return gtRMWStatus == STOREIND_RMW_DST_IS_OP1 || gtRMWStatus == STOREIND_RMW_DST_IS_OP2;
    }
    bool IsRMWDstOp1()
    {
        return gtRMWStatus == STOREIND_RMW_DST_IS_OP1;
    }
    bool IsRMWDstOp2()
    {
        return gtRMWStatus == STOREIND_RMW_DST_IS_OP2;
    }
#endif //! CPU_LOAD_STORE_ARCH

    RMWStatus GetRMWStatus()
    {
#if !CPU_LOAD_STORE_ARCH
        return gtRMWStatus;
#else
        return STOREIND_RMW_STATUS_UNKNOWN;
#endif
    }

    void SetRMWStatusDefault()
    {
#if !CPU_LOAD_STORE_ARCH
        gtRMWStatus = STOREIND_RMW_STATUS_UNKNOWN;
#endif
    }

    void SetRMWStatus(RMWStatus status)
    {
#if !CPU_LOAD_STORE_ARCH
        gtRMWStatus = status;
#endif
    }

    GenTreePtr& Data()
    {
        return gtOp2;
    }

    GenTreeStoreInd(var_types type, GenTree* destPtr, GenTree* data) : GenTreeIndir(GT_STOREIND, type, destPtr, data)
    {
        SetRMWStatusDefault();
    }

#if DEBUGGABLE_GENTREE
protected:
    friend GenTree;
    // Used only for GenTree::GetVtableForOper()
    GenTreeStoreInd() : GenTreeIndir()
    {
        SetRMWStatusDefault();
    }
#endif
};

/* gtRetExp -- Place holder for the return expression from an inline candidate (GT_RET_EXPR) */

struct GenTreeRetExpr : public GenTree
{
    GenTreePtr gtInlineCandidate;

    CORINFO_CLASS_HANDLE gtRetClsHnd;

    GenTreeRetExpr(var_types type) : GenTree(GT_RET_EXPR, type)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeRetExpr() : GenTree()
    {
    }
#endif
};

/* gtStmt   -- 'statement expr' (GT_STMT) */

class InlineContext;

struct GenTreeStmt : public GenTree
{
    GenTreePtr     gtStmtExpr;      // root of the expression tree
    GenTreePtr     gtStmtList;      // first node (for forward walks)
    InlineContext* gtInlineContext; // The inline context for this statement.
    IL_OFFSETX     gtStmtILoffsx;   // instr offset (if available)

#ifdef DEBUG
    IL_OFFSET gtStmtLastILoffs; // instr offset at end of stmt
#endif

    __declspec(property(get = getNextStmt)) GenTreeStmt* gtNextStmt;

    __declspec(property(get = getPrevStmt)) GenTreeStmt* gtPrevStmt;

    GenTreeStmt* getNextStmt()
    {
        if (gtNext == nullptr)
        {
            return nullptr;
        }
        else
        {
            return gtNext->AsStmt();
        }
    }

    GenTreeStmt* getPrevStmt()
    {
        if (gtPrev == nullptr)
        {
            return nullptr;
        }
        else
        {
            return gtPrev->AsStmt();
        }
    }

    GenTreeStmt(GenTreePtr expr, IL_OFFSETX offset)
        : GenTree(GT_STMT, TYP_VOID)
        , gtStmtExpr(expr)
        , gtStmtList(nullptr)
        , gtInlineContext(nullptr)
        , gtStmtILoffsx(offset)
#ifdef DEBUG
        , gtStmtLastILoffs(BAD_IL_OFFSET)
#endif
    {
        // Statements can't have statements as part of their expression tree.
        assert(expr->gtOper != GT_STMT);

        // Set the statement to have the same costs as the top node of the tree.
        // This is used long before costs have been assigned, so we need to copy
        // the raw costs.
        CopyRawCosts(expr);
    }

#if DEBUGGABLE_GENTREE
    GenTreeStmt() : GenTree(GT_STMT, TYP_VOID)
    {
    }
#endif
};

/*  NOTE: Any tree nodes that are larger than 8 bytes (two ints or
    pointers) must be flagged as 'large' in GenTree::InitNodeSize().
 */

/* gtClsVar -- 'static data member' (GT_CLS_VAR) */

struct GenTreeClsVar : public GenTree
{
    CORINFO_FIELD_HANDLE gtClsVarHnd;
    FieldSeqNode*        gtFieldSeq;

    GenTreeClsVar(var_types type, CORINFO_FIELD_HANDLE clsVarHnd, FieldSeqNode* fldSeq)
        : GenTree(GT_CLS_VAR, type), gtClsVarHnd(clsVarHnd), gtFieldSeq(fldSeq)
    {
        gtFlags |= GTF_GLOB_REF;
    }
#if DEBUGGABLE_GENTREE
    GenTreeClsVar() : GenTree()
    {
    }
#endif
};

/* gtArgPlace -- 'register argument placeholder' (GT_ARGPLACE) */

struct GenTreeArgPlace : public GenTree
{
    CORINFO_CLASS_HANDLE gtArgPlaceClsHnd; // Needed when we have a TYP_STRUCT argument

    GenTreeArgPlace(var_types type, CORINFO_CLASS_HANDLE clsHnd) : GenTree(GT_ARGPLACE, type), gtArgPlaceClsHnd(clsHnd)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeArgPlace() : GenTree()
    {
    }
#endif
};

/* gtLabel  -- code label target    (GT_LABEL) */

struct GenTreeLabel : public GenTree
{
    BasicBlock* gtLabBB;

    GenTreeLabel(BasicBlock* bb) : GenTree(GT_LABEL, TYP_VOID), gtLabBB(bb)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeLabel() : GenTree()
    {
    }
#endif
};

/* gtPhiArg -- phi node rhs argument, var = phi(phiarg, phiarg, phiarg...); GT_PHI_ARG */
struct GenTreePhiArg : public GenTreeLclVarCommon
{
    BasicBlock* gtPredBB;

    GenTreePhiArg(var_types type, unsigned lclNum, unsigned snum, BasicBlock* block)
        : GenTreeLclVarCommon(GT_PHI_ARG, type, lclNum), gtPredBB(block)
    {
        SetSsaNum(snum);
    }

#if DEBUGGABLE_GENTREE
    GenTreePhiArg() : GenTreeLclVarCommon()
    {
    }
#endif
};

/* gtPutArgStk -- Argument passed on stack (GT_PUTARG_STK) */

struct GenTreePutArgStk : public GenTreeUnOp
{
    unsigned gtSlotNum; // Slot number of the argument to be passed on stack
#if defined(UNIX_X86_ABI)
    unsigned gtPadAlign; // Number of padding slots for stack alignment
#endif

    // Don't let clang-format mess with the GenTreePutArgStk constructor.
    // clang-format off

    GenTreePutArgStk(genTreeOps   oper,
                     var_types    type,
                     GenTreePtr   op1,
                     unsigned     slotNum
                     PUT_STRUCT_ARG_STK_ONLY_ARG(unsigned numSlots),
                     bool         putInIncomingArgArea = false,
                     GenTreeCall* callNode = nullptr)
        : GenTreeUnOp(oper, type, op1 DEBUGARG(/*largeNode*/ false))
        , gtSlotNum(slotNum)
#if defined(UNIX_X86_ABI)
        , gtPadAlign(0)
#endif
#if FEATURE_FASTTAILCALL
        , gtPutInIncomingArgArea(putInIncomingArgArea)
#endif // FEATURE_FASTTAILCALL
#ifdef FEATURE_PUT_STRUCT_ARG_STK
        , gtPutArgStkKind(Kind::Invalid)
        , gtNumSlots(numSlots)
        , gtNumberReferenceSlots(0)
        , gtGcPtrs(nullptr)
#endif // FEATURE_PUT_STRUCT_ARG_STK
#if defined(DEBUG) || defined(UNIX_X86_ABI)
        , gtCall(callNode)
#endif
    {
    }

// clang-format on

#if FEATURE_FASTTAILCALL

    bool gtPutInIncomingArgArea; // Whether this arg needs to be placed in incoming arg area.
                                 // By default this is false and will be placed in out-going arg area.
                                 // Fast tail calls set this to true.
                                 // In future if we need to add more such bool fields consider bit fields.

    bool putInIncomingArgArea() const
    {
        return gtPutInIncomingArgArea;
    }

#else // !FEATURE_FASTTAILCALL

    bool putInIncomingArgArea() const
    {
        return false;
    }

#endif // !FEATURE_FASTTAILCALL

    unsigned getArgOffset()
    {
        return gtSlotNum * TARGET_POINTER_SIZE;
    }

#if defined(UNIX_X86_ABI)
    unsigned getArgPadding()
    {
        return gtPadAlign;
    }

    void setArgPadding(unsigned padAlign)
    {
        gtPadAlign = padAlign;
    }
#endif

#ifdef FEATURE_PUT_STRUCT_ARG_STK

    unsigned getArgSize()
    {
        return gtNumSlots * TARGET_POINTER_SIZE;
    }

    //------------------------------------------------------------------------
    // setGcPointers: Sets the number of references and the layout of the struct object returned by the VM.
    //
    // Arguments:
    //    numPointers - Number of pointer references.
    //    pointers    - layout of the struct (with pointers marked.)
    //
    // Return Value:
    //    None
    //
    // Notes:
    //    This data is used in the codegen for GT_PUTARG_STK to decide how to copy the struct to the stack by value.
    //    If no pointer references are used, block copying instructions are used.
    //    Otherwise the pointer reference slots are copied atomically in a way that gcinfo is emitted.
    //    Any non pointer references between the pointer reference slots are copied in block fashion.
    //
    void setGcPointers(unsigned numPointers, BYTE* pointers)
    {
        gtNumberReferenceSlots = numPointers;
        gtGcPtrs               = pointers;
    }

    // Instruction selection: during codegen time, what code sequence we will be using
    // to encode this operation.
    // TODO-Throughput: The following information should be obtained from the child
    // block node.

    enum class Kind : __int8{
        Invalid, RepInstr, Unroll, Push, PushAllSlots,
    };

    Kind gtPutArgStkKind;
    bool isPushKind()
    {
        return (gtPutArgStkKind == Kind::Push) || (gtPutArgStkKind == Kind::PushAllSlots);
    }

    unsigned gtNumSlots;             // Number of slots for the argument to be passed on stack
    unsigned gtNumberReferenceSlots; // Number of reference slots.
    BYTE*    gtGcPtrs;               // gcPointers

#endif // FEATURE_PUT_STRUCT_ARG_STK

#if defined(DEBUG) || defined(UNIX_X86_ABI)
    GenTreeCall* gtCall; // the call node to which this argument belongs
#endif

#if DEBUGGABLE_GENTREE
    GenTreePutArgStk() : GenTreeUnOp()
    {
    }
#endif
};

// Represents GT_COPY or GT_RELOAD node
struct GenTreeCopyOrReload : public GenTreeUnOp
{
#if FEATURE_MULTIREG_RET
    // State required to support copy/reload of a multi-reg call node.
    // First register is is always given by gtRegNum.
    //
    regNumber gtOtherRegs[MAX_RET_REG_COUNT - 1];
#endif

    //----------------------------------------------------------
    // ClearOtherRegs: set gtOtherRegs to REG_NA.
    //
    // Arguments:
    //    None
    //
    // Return Value:
    //    None
    //
    void ClearOtherRegs()
    {
#if FEATURE_MULTIREG_RET
        for (unsigned i = 0; i < MAX_RET_REG_COUNT - 1; ++i)
        {
            gtOtherRegs[i] = REG_NA;
        }
#endif
    }

    //-----------------------------------------------------------
    // GetRegNumByIdx: Get regNumber of ith position.
    //
    // Arguments:
    //    idx   -   register position.
    //
    // Return Value:
    //    Returns regNumber assigned to ith position.
    //
    regNumber GetRegNumByIdx(unsigned idx) const
    {
        assert(idx < MAX_RET_REG_COUNT);

        if (idx == 0)
        {
            return gtRegNum;
        }

#if FEATURE_MULTIREG_RET
        return gtOtherRegs[idx - 1];
#else
        return REG_NA;
#endif
    }

    //-----------------------------------------------------------
    // SetRegNumByIdx: Set the regNumber for ith position.
    //
    // Arguments:
    //    reg   -   reg number
    //    idx   -   register position.
    //
    // Return Value:
    //    None.
    //
    void SetRegNumByIdx(regNumber reg, unsigned idx)
    {
        assert(idx < MAX_RET_REG_COUNT);

        if (idx == 0)
        {
            gtRegNum = reg;
        }
#if FEATURE_MULTIREG_RET
        else
        {
            gtOtherRegs[idx - 1] = reg;
            assert(gtOtherRegs[idx - 1] == reg);
        }
#else
        else
        {
            unreached();
        }
#endif
    }

    //----------------------------------------------------------------------------
    // CopyOtherRegs: copy multi-reg state from the given copy/reload node to this
    // node.
    //
    // Arguments:
    //    from  -  GenTree node from which to copy multi-reg state
    //
    // Return Value:
    //    None
    //
    // TODO-ARM: Implement this routine for Arm64 and Arm32
    // TODO-X86: Implement this routine for x86
    void CopyOtherRegs(GenTreeCopyOrReload* from)
    {
        assert(OperGet() == from->OperGet());

#ifdef FEATURE_UNIX_AMD64_STRUCT_PASSING
        for (unsigned i = 0; i < MAX_RET_REG_COUNT - 1; ++i)
        {
            gtOtherRegs[i] = from->gtOtherRegs[i];
        }
#endif
    }

    GenTreeCopyOrReload(genTreeOps oper, var_types type, GenTree* op1) : GenTreeUnOp(oper, type, op1)
    {
        gtRegNum = REG_NA;
        ClearOtherRegs();
    }

#if DEBUGGABLE_GENTREE
    GenTreeCopyOrReload() : GenTreeUnOp()
    {
    }
#endif
};

// Represents GT_ALLOCOBJ node

struct GenTreeAllocObj final : public GenTreeUnOp
{
    unsigned int         gtNewHelper; // Value returned by ICorJitInfo::getNewHelper
    CORINFO_CLASS_HANDLE gtAllocObjClsHnd;

    GenTreeAllocObj(var_types type, unsigned int helper, CORINFO_CLASS_HANDLE clsHnd, GenTreePtr op)
        : GenTreeUnOp(GT_ALLOCOBJ, type, op DEBUGARG(/*largeNode*/ TRUE))
        , // This node in most cases will be changed to a call node
        gtNewHelper(helper)
        , gtAllocObjClsHnd(clsHnd)
    {
    }
#if DEBUGGABLE_GENTREE
    GenTreeAllocObj() : GenTreeUnOp()
    {
    }
#endif
};

// Represents a GT_JCC or GT_SETCC node.

struct GenTreeCC final : public GenTree
{
    genTreeOps gtCondition; // any relop

    GenTreeCC(genTreeOps oper, genTreeOps condition, var_types type = TYP_VOID)
        : GenTree(oper, type DEBUGARG(/*largeNode*/ FALSE)), gtCondition(condition)
    {
        assert(OperIs(GT_JCC, GT_SETCC));
        assert(OperIsCompare(condition));
    }

#if DEBUGGABLE_GENTREE
    GenTreeCC() : GenTree()
    {
    }
#endif // DEBUGGABLE_GENTREE
};

//------------------------------------------------------------------------
// Deferred inline functions of GenTree -- these need the subtypes above to
// be defined already.
//------------------------------------------------------------------------

inline bool GenTree::OperIsBlkOp()
{
    return (((gtOper == GT_ASG) && varTypeIsStruct(gtOp.gtOp1))
#ifndef LEGACY_BACKEND
            || (OperIsBlk() && (AsBlk()->Data() != nullptr))
#endif
                );
}

inline bool GenTree::OperIsDynBlkOp()
{
    if (gtOper == GT_ASG)
    {
        return gtGetOp1()->OperGet() == GT_DYN_BLK;
    }
#ifndef LEGACY_BACKEND
    else if (gtOper == GT_STORE_DYN_BLK)
    {
        return true;
    }
#endif
    return false;
}

inline bool GenTree::OperIsInitBlkOp()
{
    if (!OperIsBlkOp())
    {
        return false;
    }
#ifndef LEGACY_BACKEND
    GenTree* src;
    if (gtOper == GT_ASG)
    {
        src = gtGetOp2();
    }
    else
    {
        src = AsBlk()->Data()->gtSkipReloadOrCopy();
    }
#else  // LEGACY_BACKEND
    GenTree* src = gtGetOp2();
#endif // LEGACY_BACKEND
    return src->OperIsInitVal() || src->OperIsConst();
}

inline bool GenTree::OperIsCopyBlkOp()
{
    return OperIsBlkOp() && !OperIsInitBlkOp();
}

//------------------------------------------------------------------------
// IsFPZero: Checks whether this is a floating point constant with value 0.0
//
// Return Value:
//    Returns true iff the tree is an GT_CNS_DBL, with value of 0.0.

inline bool GenTree::IsFPZero()
{
    if ((gtOper == GT_CNS_DBL) && (gtDblCon.gtDconVal == 0.0))
    {
        return true;
    }
    return false;
}

//------------------------------------------------------------------------
// IsIntegralConst: Checks whether this is a constant node with the given value
//
// Arguments:
//    constVal - the value of interest
//
// Return Value:
//    Returns true iff the tree is an integral constant opcode, with
//    the given value.
//
// Notes:
//    Like gtIconVal, the argument is of ssize_t, so cannot check for
//    long constants in a target-independent way.

inline bool GenTree::IsIntegralConst(ssize_t constVal)

{
    if ((gtOper == GT_CNS_INT) && (gtIntConCommon.IconValue() == constVal))
    {
        return true;
    }

    if ((gtOper == GT_CNS_LNG) && (gtIntConCommon.LngValue() == constVal))
    {
        return true;
    }

    return false;
}

//-------------------------------------------------------------------
// IsIntegralConstVector: returns true if this this is a SIMD vector
// with all its elements equal to an integral constant.
//
// Arguments:
//     constVal  -  const value of vector element
//
// Returns:
//     True if this represents an integral const SIMD vector.
//
inline bool GenTree::IsIntegralConstVector(ssize_t constVal)
{
#ifdef FEATURE_SIMD
    // SIMDIntrinsicInit intrinsic with a const value as initializer
    // represents a const vector.
    if ((gtOper == GT_SIMD) && (gtSIMD.gtSIMDIntrinsicID == SIMDIntrinsicInit) && gtGetOp1()->IsIntegralConst(constVal))
    {
        assert(varTypeIsIntegral(gtSIMD.gtSIMDBaseType));
        assert(gtGetOp2IfPresent() == nullptr);
        return true;
    }
#endif

    return false;
}

inline bool GenTree::IsBoxedValue()
{
    assert(gtOper != GT_BOX || gtBox.BoxOp() != nullptr);
    return (gtOper == GT_BOX) && (gtFlags & GTF_BOX_VALUE);
}

inline bool GenTree::IsSIMDEqualityOrInequality() const
{
#ifdef FEATURE_SIMD
    if (gtOper == GT_SIMD)
    {
        // Has to cast away const-ness since AsSIMD() method is non-const.
        GenTreeSIMD* simdNode = const_cast<GenTree*>(this)->AsSIMD();
        return (simdNode->gtSIMDIntrinsicID == SIMDIntrinsicOpEquality ||
                simdNode->gtSIMDIntrinsicID == SIMDIntrinsicOpInEquality);
    }
#endif

    return false;
}

inline GenTreePtr GenTree::MoveNext()
{
    assert(OperIsAnyList());
    return gtOp.gtOp2;
}

#ifdef DEBUG
//------------------------------------------------------------------------
// IsValidCallArgument: Given an GenTree node that represents an argument
//                      enforce (or don't enforce) the following invariant.
//
// Arguments:
//    instance method for a GenTree node
//
// Return values:
//    true:      the GenTree node is accepted as a valid argument
//    false:     the GenTree node is not accepted as a valid argumeny
//
// Notes:
//    For targets that don't support arguments as a list of fields, we do not support GT_FIELD_LIST.
//
//    Currently for AMD64 UNIX we allow a limited case where a GT_FIELD_LIST is
//    allowed but every element must be a GT_LCL_FLD.
//
//    For the future targets that allow for Multireg args (and this includes the current ARM64 target),
//    or that allow for passing promoted structs, we allow a GT_FIELD_LIST of arbitrary nodes.
//    These would typically start out as GT_LCL_VARs or GT_LCL_FLDS or GT_INDs,
//    but could be changed into constants or GT_COMMA trees by the later
//    optimization phases.

inline bool GenTree::IsValidCallArgument()
{
    if (OperIsList())
    {
        // GT_FIELD_LIST is the only list allowed.
        return false;
    }
    if (OperIsFieldList())
    {
#if defined(LEGACY_BACKEND) || (!FEATURE_MULTIREG_ARGS && !FEATURE_PUT_STRUCT_ARG_STK)
        // Not allowed to have a GT_FIELD_LIST for an argument
        // unless we have a RyuJIT backend and FEATURE_MULTIREG_ARGS or FEATURE_PUT_STRUCT_ARG_STK

        return false;

#else // we have RyuJIT backend and FEATURE_MULTIREG_ARGS or FEATURE_PUT_STRUCT_ARG_STK

#ifdef FEATURE_UNIX_AMD64_STRUCT_PASSING
        // For UNIX ABI we currently only allow a GT_FIELD_LIST of GT_LCL_FLDs nodes
        GenTree* gtListPtr = this;
        while (gtListPtr != nullptr)
        {
            // ToDo: fix UNIX_AMD64 so that we do not generate this kind of a List
            //  Note the list as currently created is malformed, as the last entry is a nullptr
            if (gtListPtr->Current() == nullptr)
            {
                break;
            }

            // Only a list of GT_LCL_FLDs is allowed
            if (gtListPtr->Current()->OperGet() != GT_LCL_FLD)
            {
                return false;
            }
            gtListPtr = gtListPtr->MoveNext();
        }
#endif // FEATURE_UNIX_AMD64_STRUCT_PASSING

        // Note that for non-UNIX ABI the GT_FIELD_LIST may contain any node
        //
        // We allow this GT_FIELD_LIST as an argument
        return true;

#endif // FEATURE_MULTIREG_ARGS
    }
    // We don't have either kind of list, so it satisfies the invariant.
    return true;
}
#endif // DEBUG

inline GenTreePtr GenTree::Current()
{
    assert(OperIsAnyList());
    return gtOp.gtOp1;
}

inline GenTreePtr* GenTree::pCurrent()
{
    assert(OperIsAnyList());
    return &(gtOp.gtOp1);
}

inline GenTreePtr GenTree::gtGetOp1()
{
    return gtOp.gtOp1;
}

#ifdef DEBUG
/* static */
inline bool GenTree::RequiresNonNullOp2(genTreeOps oper)
{
    switch (oper)
    {
        case GT_ADD:
        case GT_SUB:
        case GT_MUL:
        case GT_DIV:
        case GT_MOD:
        case GT_UDIV:
        case GT_UMOD:
        case GT_OR:
        case GT_XOR:
        case GT_AND:
        case GT_LSH:
        case GT_RSH:
        case GT_RSZ:
        case GT_ROL:
        case GT_ROR:
        case GT_INDEX:
        case GT_ASG:
        case GT_ASG_ADD:
        case GT_ASG_SUB:
        case GT_ASG_MUL:
        case GT_ASG_DIV:
        case GT_ASG_MOD:
        case GT_ASG_UDIV:
        case GT_ASG_UMOD:
        case GT_ASG_OR:
        case GT_ASG_XOR:
        case GT_ASG_AND:
        case GT_ASG_LSH:
        case GT_ASG_RSH:
        case GT_ASG_RSZ:
        case GT_EQ:
        case GT_NE:
        case GT_LT:
        case GT_LE:
        case GT_GE:
        case GT_GT:
        case GT_COMMA:
        case GT_QMARK:
        case GT_COLON:
        case GT_MKREFANY:
            return true;
        default:
            return false;
    }
}
#endif // DEBUG

inline GenTreePtr GenTree::gtGetOp2()
{
    assert(OperIsBinary());

    GenTreePtr op2 = gtOp.gtOp2;

    // Only allow null op2 if the node type allows it, e.g. GT_LIST.
    assert((op2 != nullptr) || !RequiresNonNullOp2(gtOper));

    return op2;
}

inline GenTreePtr GenTree::gtGetOp2IfPresent()
{
    /* gtOp.gtOp2 is only valid for GTK_BINOP nodes. */

    GenTreePtr op2 = OperIsBinary() ? gtOp.gtOp2 : nullptr;

    // This documents the genTreeOps for which gtOp.gtOp2 cannot be nullptr.
    // This helps prefix in its analysis of code which calls gtGetOp2()

    assert((op2 != nullptr) || !RequiresNonNullOp2(gtOper));

    return op2;
}

inline GenTreePtr GenTree::gtEffectiveVal(bool commaOnly)
{
    GenTree* effectiveVal = this;
    for (;;)
    {
        if (effectiveVal->gtOper == GT_COMMA)
        {
            effectiveVal = effectiveVal->gtOp.gtOp2;
        }
        else if (!commaOnly && (effectiveVal->gtOper == GT_NOP) && (effectiveVal->gtOp.gtOp1 != nullptr))
        {
            effectiveVal = effectiveVal->gtOp.gtOp1;
        }
        else
        {
            return effectiveVal;
        }
    }
}

inline GenTree* GenTree::gtSkipReloadOrCopy()
{
    // There can be only one reload or copy (we can't have a reload/copy of a reload/copy)
    if (gtOper == GT_RELOAD || gtOper == GT_COPY)
    {
        assert(gtGetOp1()->OperGet() != GT_RELOAD && gtGetOp1()->OperGet() != GT_COPY);
        return gtGetOp1();
    }
    return this;
}

//-----------------------------------------------------------------------------------
// IsMultiRegCall: whether a call node returning its value in more than one register
//
// Arguments:
//     None
//
// Return Value:
//     Returns true if this GenTree is a multi register returning call
inline bool GenTree::IsMultiRegCall() const
{
    if (this->IsCall())
    {
        // We cannot use AsCall() as it is not declared const
        const GenTreeCall* call = reinterpret_cast<const GenTreeCall*>(this);
        return call->HasMultiRegRetVal();
    }

    return false;
}

//-----------------------------------------------------------------------------------
// IsMultiRegNode: whether a node returning its value in more than one register
//
// Arguments:
//     None
//
// Return Value:
//     Returns true if this GenTree is a multi-reg node.
inline bool GenTree::IsMultiRegNode() const
{
    if (IsMultiRegCall())
    {
        return true;
    }

#if !defined(LEGACY_BACKEND) && defined(_TARGET_ARM_)
    if (gtOper == GT_MUL_LONG)
    {
        return true;
    }
#endif

    return false;
}

//-------------------------------------------------------------------------
// IsCopyOrReload: whether this is a GT_COPY or GT_RELOAD node.
//
// Arguments:
//     None
//
// Return Value:
//     Returns true if this GenTree is a copy or reload node.
inline bool GenTree::IsCopyOrReload() const
{
    return (gtOper == GT_COPY || gtOper == GT_RELOAD);
}

//-----------------------------------------------------------------------------------
// IsCopyOrReloadOfMultiRegCall: whether this is a GT_COPY or GT_RELOAD of a multi-reg
// call node.
//
// Arguments:
//     None
//
// Return Value:
//     Returns true if this GenTree is a copy or reload of multi-reg call node.
inline bool GenTree::IsCopyOrReloadOfMultiRegCall() const
{
    if (IsCopyOrReload())
    {
        GenTree* t = const_cast<GenTree*>(this);
        return t->gtGetOp1()->IsMultiRegCall();
    }

    return false;
}

inline bool GenTree::IsCnsIntOrI() const
{
    return (gtOper == GT_CNS_INT);
}

inline bool GenTree::IsIntegralConst() const
{
#ifdef _TARGET_64BIT_
    return IsCnsIntOrI();
#else  // !_TARGET_64BIT_
    return ((gtOper == GT_CNS_INT) || (gtOper == GT_CNS_LNG));
#endif // !_TARGET_64BIT_
}

inline bool GenTree::IsIntCnsFitsInI32()
{
#ifdef _TARGET_64BIT_
    return IsCnsIntOrI() && ((int)gtIntConCommon.IconValue() == gtIntConCommon.IconValue());
#else  // !_TARGET_64BIT_
    return IsCnsIntOrI();
#endif // !_TARGET_64BIT_
}

inline bool GenTree::IsCnsFltOrDbl() const
{
    return OperGet() == GT_CNS_DBL;
}

inline bool GenTree::IsCnsNonZeroFltOrDbl()
{
    if (OperGet() == GT_CNS_DBL)
    {
        double constValue = gtDblCon.gtDconVal;
        return *(__int64*)&constValue != 0;
    }

    return false;
}

inline bool GenTree::IsHelperCall()
{
    return OperGet() == GT_CALL && gtCall.gtCallType == CT_HELPER;
}

inline var_types GenTree::CastFromType()
{
    return this->gtCast.CastOp()->TypeGet();
}
inline var_types& GenTree::CastToType()
{
    return this->gtCast.gtCastType;
}

//-----------------------------------------------------------------------------------
// HasGCPtr: determine whether this block op involves GC pointers
//
// Arguments:
//     None
//
// Return Value:
//    Returns true iff the object being copied contains one or more GC pointers.
//
// Notes:
//    Of the block nodes, only GT_OBJ and ST_STORE_OBJ are allowed to have GC pointers.
//
inline bool GenTreeBlk::HasGCPtr()
{
    if ((gtOper == GT_OBJ) || (gtOper == GT_STORE_OBJ))
    {
        return (AsObj()->gtGcPtrCount != 0);
    }
    return false;
}

inline bool GenTree::isUsedFromSpillTemp() const
{
#if !defined(LEGACY_BACKEND)
    // If spilled and no reg at use, then it is used from the spill temp location rather than being reloaded.
    if (((gtFlags & GTF_SPILLED) != 0) && ((gtFlags & GTF_NOREG_AT_USE) != 0))
    {
        return true;
    }
#endif //! LEGACY_BACKEND

    return false;
}

/*****************************************************************************/

#ifndef _HOST_64BIT_
#include <poppack.h>
#endif

/*****************************************************************************/

#if SMALL_TREE_NODES

// In debug, on some platforms (e.g., when LATE_DISASM is defined), GenTreeIntCon is bigger than GenTreeLclFld.
const size_t TREE_NODE_SZ_SMALL = max(sizeof(GenTreeIntCon), sizeof(GenTreeLclFld));

#endif // SMALL_TREE_NODES

const size_t TREE_NODE_SZ_LARGE = sizeof(GenTreeCall);

/*****************************************************************************
 * Types returned by GenTree::lvaLclVarRefs()
 */

enum varRefKinds
{
    VR_INVARIANT = 0x00, // an invariant value
    VR_NONE      = 0x00,
    VR_IND_REF   = 0x01, // an object reference
    VR_IND_SCL   = 0x02, // a non-object reference
    VR_GLB_VAR   = 0x04, // a global (clsVar)
};
// Add a temp define to avoid merge conflict.
#define VR_IND_PTR VR_IND_REF

/*****************************************************************************/
#endif // !GENTREE_H
/*****************************************************************************/
