// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                    Inline functions                                       XX
XX                                                                           XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#ifndef _COMPILER_HPP_
#define _COMPILER_HPP_

#include "emit.h" // for emitter::emitAddLabel

#include "bitvec.h"

#include "compilerbitsettraits.hpp"

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX  Miscellaneous utility functions. Some of these are defined in Utils.cpp  XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************/
/*****************************************************************************/

inline bool getInlinePInvokeEnabled()
{
#ifdef DEBUG
    return JitConfig.JitPInvokeEnabled() && !JitConfig.StressCOMCall();
#else
    return true;
#endif
}

inline bool getInlinePInvokeCheckEnabled()
{
#ifdef DEBUG
    return JitConfig.JitPInvokeCheckEnabled() != 0;
#else
    return false;
#endif
}

// Enforce float narrowing for buggy compilers (notably preWhidbey VC)
inline float forceCastToFloat(double d)
{
    Volatile<float> f = (float)d;
    return f;
}

// Enforce UInt32 narrowing for buggy compilers (notably Whidbey Beta 2 LKG)
inline UINT32 forceCastToUInt32(double d)
{
    Volatile<UINT32> u = (UINT32)d;
    return u;
}

enum RoundLevel
{
    ROUND_NEVER     = 0, // Never round
    ROUND_CMP_CONST = 1, // Round values compared against constants
    ROUND_CMP       = 2, // Round comparands and return values
    ROUND_ALWAYS    = 3, // Round always

    COUNT_ROUND_LEVEL,
    DEFAULT_ROUND_LEVEL = ROUND_NEVER
};

inline RoundLevel getRoundFloatLevel()
{
#ifdef DEBUG
    return (RoundLevel)JitConfig.JitRoundFloat();
#else
    return DEFAULT_ROUND_LEVEL;
#endif
}

/*****************************************************************************/
/*****************************************************************************
 *
 *  Return the lowest bit that is set
 */

template <typename T>
inline T genFindLowestBit(T value)
{
    return (value & (0 - value));
}

/*****************************************************************************/
/*****************************************************************************
 *
 *  Return the highest bit that is set (that is, a mask that includes just the highest bit).
 *  TODO-ARM64-Throughput: we should convert these to use the _BitScanReverse() / _BitScanReverse64()
 *  compiler intrinsics, but our CRT header file intrin.h doesn't define these for ARM64 yet.
 */

inline unsigned int genFindHighestBit(unsigned int mask)
{
    assert(mask != 0);
    unsigned int bit = 1U << ((sizeof(unsigned int) * 8) - 1); // start looking at the top
    while ((bit & mask) == 0)
    {
        bit >>= 1;
    }
    return bit;
}

inline unsigned __int64 genFindHighestBit(unsigned __int64 mask)
{
    assert(mask != 0);
    unsigned __int64 bit = 1ULL << ((sizeof(unsigned __int64) * 8) - 1); // start looking at the top
    while ((bit & mask) == 0)
    {
        bit >>= 1;
    }
    return bit;
}

#if 0
// TODO-ARM64-Cleanup: These should probably be the implementation, when intrin.h is updated for ARM64
inline
unsigned int genFindHighestBit(unsigned int mask)
{
    assert(mask != 0);
    unsigned int index;
    _BitScanReverse(&index, mask);
    return 1L << index;
}

inline
unsigned __int64 genFindHighestBit(unsigned __int64 mask)
{
    assert(mask != 0);
    unsigned int index;
    _BitScanReverse64(&index, mask);
    return 1LL << index;
}
#endif // 0

/*****************************************************************************
 *
 *  Return true if the given 64-bit value has exactly zero or one bits set.
 */

template <typename T>
inline BOOL genMaxOneBit(T value)
{
    return (value & (value - 1)) == 0;
}

/*****************************************************************************
 *
 *  Return true if the given 32-bit value has exactly zero or one bits set.
 */

inline BOOL genMaxOneBit(unsigned value)
{
    return (value & (value - 1)) == 0;
}

/*****************************************************************************
 *
 *  Given a value that has exactly one bit set, return the position of that
 *  bit, in other words return the logarithm in base 2 of the given value.
 */

inline unsigned genLog2(unsigned value)
{
    return BitPosition(value);
}

/*****************************************************************************
 *
 *  Given a value that has exactly one bit set, return the position of that
 *  bit, in other words return the logarithm in base 2 of the given value.
 */

inline unsigned genLog2(unsigned __int64 value)
{
    unsigned lo32 = (unsigned)value;
    unsigned hi32 = (unsigned)(value >> 32);

    if (lo32 != 0)
    {
        assert(hi32 == 0);
        return genLog2(lo32);
    }
    else
    {
        return genLog2(hi32) + 32;
    }
}

/*****************************************************************************
 *
 *  Return the lowest bit that is set in the given register mask.
 */

inline regMaskTP genFindLowestReg(regMaskTP value)
{
    return (regMaskTP)genFindLowestBit(value);
}

/*****************************************************************************
 *
 *  A rather simple routine that counts the number of bits in a given number.
 */

template <typename T>
inline unsigned genCountBits(T bits)
{
    unsigned cnt = 0;

    while (bits)
    {
        cnt++;
        bits -= genFindLowestBit(bits);
    }

    return cnt;
}

/*****************************************************************************
 *
 *  Given 3 masks value, end, start, returns the bits of value between start
 *  and end (exclusive).
 *
 *  value[bitNum(end) - 1, bitNum(start) + 1]
 */

inline unsigned __int64 BitsBetween(unsigned __int64 value, unsigned __int64 end, unsigned __int64 start)
{
    assert(start != 0);
    assert(start < end);
    assert((start & (start - 1)) == 0);
    assert((end & (end - 1)) == 0);

    return value & ~((start - 1) | start) & // Ones to the left of set bit in the start mask.
           (end - 1);                       // Ones to the right of set bit in the end mask.
}

/*****************************************************************************/

inline bool jitIsScaleIndexMul(size_t val)
{
    switch (val)
    {
        case 1:
        case 2:
        case 4:
        case 8:
            return true;

        default:
            return false;
    }
}

// Returns "tree" iff "val" is a valid addressing mode scale shift amount on
// the target architecture.
inline bool jitIsScaleIndexShift(ssize_t val)
{
    // It happens that this is the right test for all our current targets: x86, x64 and ARM.
    // This test would become target-dependent if we added a new target with a different constraint.
    return 0 < val && val < 4;
}

/*****************************************************************************
 * Returns true if value is between [start..end).
 * The comparison is inclusive of start, exclusive of end.
 */

/* static */
inline bool Compiler::jitIsBetween(unsigned value, unsigned start, unsigned end)
{
    return start <= value && value < end;
}

/*****************************************************************************
 * Returns true if value is between [start..end].
 * The comparison is inclusive of both start and end.
 */

/* static */
inline bool Compiler::jitIsBetweenInclusive(unsigned value, unsigned start, unsigned end)
{
    return start <= value && value <= end;
}

/******************************************************************************************
 * Return the EH descriptor for the given region index.
 */
inline EHblkDsc* Compiler::ehGetDsc(unsigned regionIndex)
{
    assert(regionIndex < compHndBBtabCount);
    return &compHndBBtab[regionIndex];
}

/******************************************************************************************
 * Return the EH descriptor index of the enclosing try, for the given region index.
 */
inline unsigned Compiler::ehGetEnclosingTryIndex(unsigned regionIndex)
{
    return ehGetDsc(regionIndex)->ebdEnclosingTryIndex;
}

/******************************************************************************************
 * Return the EH descriptor index of the enclosing handler, for the given region index.
 */
inline unsigned Compiler::ehGetEnclosingHndIndex(unsigned regionIndex)
{
    return ehGetDsc(regionIndex)->ebdEnclosingHndIndex;
}

/******************************************************************************************
 * Return the EH index given a region descriptor.
 */
inline unsigned Compiler::ehGetIndex(EHblkDsc* ehDsc)
{
    assert(compHndBBtab <= ehDsc && ehDsc < compHndBBtab + compHndBBtabCount);
    return (unsigned)(ehDsc - compHndBBtab);
}

/******************************************************************************************
 * Return the EH descriptor for the most nested 'try' region this BasicBlock is a member of
 * (or nullptr if this block is not in a 'try' region).
 */
inline EHblkDsc* Compiler::ehGetBlockTryDsc(BasicBlock* block)
{
    if (!block->hasTryIndex())
    {
        return nullptr;
    }

    return ehGetDsc(block->getTryIndex());
}

/******************************************************************************************
 * Return the EH descriptor for the most nested filter or handler region this BasicBlock is a member of
 * (or nullptr if this block is not in a filter or handler region).
 */
inline EHblkDsc* Compiler::ehGetBlockHndDsc(BasicBlock* block)
{
    if (!block->hasHndIndex())
    {
        return nullptr;
    }

    return ehGetDsc(block->getHndIndex());
}

#if FEATURE_EH_FUNCLETS

/*****************************************************************************
 *  Get the FuncInfoDsc for the funclet we are currently generating code for.
 *  This is only valid during codegen.
 *
 */
inline FuncInfoDsc* Compiler::funCurrentFunc()
{
    return funGetFunc(compCurrFuncIdx);
}

/*****************************************************************************
 *  Change which funclet we are currently generating code for.
 *  This is only valid after funclets are created.
 *
 */
inline void Compiler::funSetCurrentFunc(unsigned funcIdx)
{
    assert(fgFuncletsCreated);
    assert(FitsIn<unsigned short>(funcIdx));
    noway_assert(funcIdx < compFuncInfoCount);
    compCurrFuncIdx = (unsigned short)funcIdx;
}

/*****************************************************************************
 *  Get the FuncInfoDsc for the given funclet.
 *  This is only valid after funclets are created.
 *
 */
inline FuncInfoDsc* Compiler::funGetFunc(unsigned funcIdx)
{
    assert(fgFuncletsCreated);
    assert(funcIdx < compFuncInfoCount);
    return &compFuncInfos[funcIdx];
}

/*****************************************************************************
 *  Get the funcIdx for the EH funclet that begins with block.
 *  This is only valid after funclets are created.
 *  It is only valid for blocks marked with BBF_FUNCLET_BEG because
 *  otherwise we would have to do a more expensive check to determine
 *  if this should return the filter funclet or the filter handler funclet.
 *
 */
inline unsigned Compiler::funGetFuncIdx(BasicBlock* block)
{
    assert(fgFuncletsCreated);
    assert(block->bbFlags & BBF_FUNCLET_BEG);

    EHblkDsc*    eh      = ehGetDsc(block->getHndIndex());
    unsigned int funcIdx = eh->ebdFuncIndex;
    if (eh->ebdHndBeg != block)
    {
        // If this is a filter EH clause, but we want the funclet
        // for the filter (not the filter handler), it is the previous one
        noway_assert(eh->HasFilter());
        noway_assert(eh->ebdFilter == block);
        assert(funGetFunc(funcIdx)->funKind == FUNC_HANDLER);
        assert(funGetFunc(funcIdx)->funEHIndex == funGetFunc(funcIdx - 1)->funEHIndex);
        assert(funGetFunc(funcIdx - 1)->funKind == FUNC_FILTER);
        funcIdx--;
    }

    return funcIdx;
}

#else // !FEATURE_EH_FUNCLETS

/*****************************************************************************
 *  Get the FuncInfoDsc for the funclet we are currently generating code for.
 *  This is only valid during codegen.  For non-funclet platforms, this is
 *  always the root function.
 *
 */
inline FuncInfoDsc* Compiler::funCurrentFunc()
{
    return &compFuncInfoRoot;
}

/*****************************************************************************
 *  Change which funclet we are currently generating code for.
 *  This is only valid after funclets are created.
 *
 */
inline void Compiler::funSetCurrentFunc(unsigned funcIdx)
{
    assert(funcIdx == 0);
}

/*****************************************************************************
 *  Get the FuncInfoDsc for the givven funclet.
 *  This is only valid after funclets are created.
 *
 */
inline FuncInfoDsc* Compiler::funGetFunc(unsigned funcIdx)
{
    assert(funcIdx == 0);
    return &compFuncInfoRoot;
}

/*****************************************************************************
 *  No funclets, so always 0.
 *
 */
inline unsigned Compiler::funGetFuncIdx(BasicBlock* block)
{
    return 0;
}

#endif // !FEATURE_EH_FUNCLETS

//------------------------------------------------------------------------------
// genRegNumFromMask : Maps a single register mask to a register number.
//
// Arguments:
//    mask - the register mask
//
// Return Value:
//    The number of the register contained in the mask.
//
// Assumptions:
//    The mask contains one and only one register.

inline regNumber genRegNumFromMask(regMaskTP mask)
{
    assert(mask != 0); // Must have one bit set, so can't have a mask of zero

    /* Convert the mask to a register number */

    regNumber regNum = (regNumber)genLog2(mask);

    /* Make sure we got it right */

    assert(genRegMask(regNum) == mask);

    return regNum;
}

//------------------------------------------------------------------------------
// genTypeCanRepresentValue: Checks if a value can be represented by a given type.
//
// Arguments:
//    value - the value to check
//    type  - the type
//
// Return Value:
//    True if the value is representable, false otherwise.
//
// Notes:
//    If the type is not integral or ref like (ref/byref/array) then false is
//    always returned.

template <typename TValue>
inline bool genTypeCanRepresentValue(var_types type, TValue value)
{
    switch (type)
    {
        case TYP_UBYTE:
        case TYP_BOOL:
            return FitsIn<UINT8>(value);
        case TYP_BYTE:
            return FitsIn<INT8>(value);
        case TYP_USHORT:
        case TYP_CHAR:
            return FitsIn<UINT16>(value);
        case TYP_SHORT:
            return FitsIn<INT16>(value);
        case TYP_UINT:
            return FitsIn<UINT32>(value);
        case TYP_INT:
            return FitsIn<INT32>(value);
        case TYP_ULONG:
            return FitsIn<UINT64>(value);
        case TYP_LONG:
            return FitsIn<INT64>(value);
        case TYP_REF:
        case TYP_BYREF:
        case TYP_ARRAY:
            return FitsIn<UINT_PTR>(value);
        default:
            return false;
    }
}

/*****************************************************************************
 *
 *  Return the size in bytes of the given type.
 */

extern const BYTE genTypeSizes[TYP_COUNT];

template <class T>
inline unsigned genTypeSize(T type)
{
    assert((unsigned)TypeGet(type) < sizeof(genTypeSizes) / sizeof(genTypeSizes[0]));

    return genTypeSizes[TypeGet(type)];
}

/*****************************************************************************
 *
 *  Return the "stack slot count" of the given type.
 *      returns 1 for 32-bit types and 2 for 64-bit types.
 */

extern const BYTE genTypeStSzs[TYP_COUNT];

inline unsigned genTypeStSz(var_types type)
{
    assert((unsigned)type < sizeof(genTypeStSzs) / sizeof(genTypeStSzs[0]));

    return genTypeStSzs[type];
}

/*****************************************************************************
 *
 *  Return the number of registers required to hold a value of the given type.
 */

/*****************************************************************************
 *
 *  The following function maps a 'precise' type to an actual type as seen
 *  by the VM (for example, 'byte' maps to 'int').
 */

extern const BYTE genActualTypes[TYP_COUNT];

inline var_types genActualType(var_types type)
{
    /* Spot check to make certain the table is in synch with the enum */

    assert(genActualTypes[TYP_DOUBLE] == TYP_DOUBLE);
    assert(genActualTypes[TYP_FNC] == TYP_FNC);
    assert(genActualTypes[TYP_REF] == TYP_REF);

    assert((unsigned)type < sizeof(genActualTypes));
    return (var_types)genActualTypes[type];
}

/*****************************************************************************/

inline var_types genUnsignedType(var_types type)
{
    /* Force signed types into corresponding unsigned type */

    switch (type)
    {
        case TYP_BYTE:
            type = TYP_UBYTE;
            break;
        case TYP_SHORT:
            type = TYP_CHAR;
            break;
        case TYP_INT:
            type = TYP_UINT;
            break;
        case TYP_LONG:
            type = TYP_ULONG;
            break;
        default:
            break;
    }

    return type;
}

/*****************************************************************************/

inline var_types genSignedType(var_types type)
{
    /* Force non-small unsigned type into corresponding signed type */
    /* Note that we leave the small types alone */

    switch (type)
    {
        case TYP_UINT:
            type = TYP_INT;
            break;
        case TYP_ULONG:
            type = TYP_LONG;
            break;
        default:
            break;
    }

    return type;
}

/*****************************************************************************
 *  Can this type be passed as a parameter in a register?
 */

inline bool isRegParamType(var_types type)
{
#if defined(_TARGET_X86_)
    return (type <= TYP_INT || type == TYP_REF || type == TYP_BYREF);
#else  // !_TARGET_X86_
    return true;
#endif // !_TARGET_X86_
}

#if defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_)
/*****************************************************************************/
// Returns true if 'type' is a struct that can be enregistered for call args
//                         or can be returned by value in multiple registers.
//              if 'type' is not a struct the return value will be false.
//
// Arguments:
//    type      - the basic jit var_type for the item being queried
//    typeClass - the handle for the struct when 'type' is TYP_STRUCT
//    typeSize  - Out param (if non-null) is updated with the size of 'type'.
//    forReturn - this is true when we asking about a GT_RETURN context;
//                this is false when we are asking about an argument context
//
inline bool Compiler::VarTypeIsMultiByteAndCanEnreg(var_types            type,
                                                    CORINFO_CLASS_HANDLE typeClass,
                                                    unsigned*            typeSize,
                                                    bool                 forReturn)
{
    bool     result = false;
    unsigned size   = 0;

    if (varTypeIsStruct(type))
    {
        size = info.compCompHnd->getClassSize(typeClass);
        if (forReturn)
        {
            structPassingKind howToReturnStruct;
            type = getReturnTypeForStruct(typeClass, &howToReturnStruct, size);
        }
        else
        {
            structPassingKind howToPassStruct;
            type = getArgTypeForStruct(typeClass, &howToPassStruct, size);
        }
        if (type != TYP_UNKNOWN)
        {
            result = true;
        }
    }
    else
    {
        size = genTypeSize(type);
    }

    if (typeSize != nullptr)
    {
        *typeSize = size;
    }

    return result;
}
#endif //_TARGET_AMD64_ || _TARGET_ARM64_

/*****************************************************************************/

#ifdef DEBUG

inline const char* varTypeGCstring(var_types type)
{
    switch (type)
    {
        case TYP_REF:
            return "gcr";
        case TYP_BYREF:
            return "byr";
        default:
            return "non";
    }
}

#endif

/*****************************************************************************/

const char* varTypeName(var_types);

/*****************************************************************************
 *
 *  Helpers to pull big-endian values out of a byte stream.
 */

inline unsigned genGetU1(const BYTE* addr)
{
    return addr[0];
}

inline signed genGetI1(const BYTE* addr)
{
    return (signed char)addr[0];
}

inline unsigned genGetU2(const BYTE* addr)
{
    return (addr[0] << 8) | addr[1];
}

inline signed genGetI2(const BYTE* addr)
{
    return (signed short)((addr[0] << 8) | addr[1]);
}

inline unsigned genGetU4(const BYTE* addr)
{
    return (addr[0] << 24) | (addr[1] << 16) | (addr[2] << 8) | addr[3];
}

/*****************************************************************************/
//  Helpers to pull little-endian values out of a byte stream.

inline unsigned __int8 getU1LittleEndian(const BYTE* ptr)
{
    return *(UNALIGNED unsigned __int8*)ptr;
}

inline unsigned __int16 getU2LittleEndian(const BYTE* ptr)
{
    return GET_UNALIGNED_VAL16(ptr);
}

inline unsigned __int32 getU4LittleEndian(const BYTE* ptr)
{
    return GET_UNALIGNED_VAL32(ptr);
}

inline signed __int8 getI1LittleEndian(const BYTE* ptr)
{
    return *(UNALIGNED signed __int8*)ptr;
}

inline signed __int16 getI2LittleEndian(const BYTE* ptr)
{
    return GET_UNALIGNED_VAL16(ptr);
}

inline signed __int32 getI4LittleEndian(const BYTE* ptr)
{
    return GET_UNALIGNED_VAL32(ptr);
}

inline signed __int64 getI8LittleEndian(const BYTE* ptr)
{
    return GET_UNALIGNED_VAL64(ptr);
}

inline float getR4LittleEndian(const BYTE* ptr)
{
    __int32 val = getI4LittleEndian(ptr);
    return *(float*)&val;
}

inline double getR8LittleEndian(const BYTE* ptr)
{
    __int64 val = getI8LittleEndian(ptr);
    return *(double*)&val;
}

/*****************************************************************************
 *
 *  Return the normalized index to use in the EXPSET_TP for the CSE with
 *  the given CSE index.
 *  Each GenTree has the following field:
 *    signed char       gtCSEnum;        // 0 or the CSE index (negated if def)
 *  So zero is reserved to mean this node is not a CSE
 *  and postive values indicate CSE uses and negative values indicate CSE defs.
 *  The caller of this method must pass a non-zero postive value.
 *  This precondition is checked by the assert on the first line of this method.
 */

inline unsigned int genCSEnum2bit(unsigned index)
{
    assert((index > 0) && (index <= EXPSET_SZ));

    return (index - 1);
}

#ifdef DEBUG
const char* genES2str(BitVecTraits* traits, EXPSET_TP set);
const char* refCntWtd2str(unsigned refCntWtd);
#endif

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          GenTree                                          XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

void* GenTree::operator new(size_t sz, Compiler* comp, genTreeOps oper)
{
#if SMALL_TREE_NODES
    size_t size = GenTree::s_gtNodeSizes[oper];
#else
    size_t     size  = TREE_NODE_SZ_LARGE;
#endif

#if MEASURE_NODE_SIZE
    genNodeSizeStats.genTreeNodeCnt += 1;
    genNodeSizeStats.genTreeNodeSize += size;
    genNodeSizeStats.genTreeNodeActualSize += sz;

    genNodeSizeStatsPerFunc.genTreeNodeCnt += 1;
    genNodeSizeStatsPerFunc.genTreeNodeSize += size;
    genNodeSizeStatsPerFunc.genTreeNodeActualSize += sz;
#endif // MEASURE_NODE_SIZE

    assert(size >= sz);
    return comp->compGetMem(size, CMK_ASTNode);
}

// GenTree constructor
inline GenTree::GenTree(genTreeOps oper, var_types type DEBUGARG(bool largeNode))
{
    gtOper     = oper;
    gtType     = type;
    gtFlags    = 0;
    gtLIRFlags = 0;
#ifdef DEBUG
    gtDebugFlags = 0;
#endif // DEBUG
#ifdef LEGACY_BACKEND
    gtUsedRegs = 0;
#endif // LEGACY_BACKEND
#if FEATURE_ANYCSE
    gtCSEnum = NO_CSE;
#endif // FEATURE_ANYCSE
#if ASSERTION_PROP
    ClearAssertion();
#endif

#if FEATURE_STACK_FP_X87
    gtFPlvl = 0;
#endif

    gtNext   = nullptr;
    gtPrev   = nullptr;
    gtRegNum = REG_NA;
    INDEBUG(gtRegTag = GT_REGTAG_NONE;)

    INDEBUG(gtCostsInitialized = false;)

#ifdef DEBUG
#if SMALL_TREE_NODES
    size_t size = GenTree::s_gtNodeSizes[oper];
    if (size == TREE_NODE_SZ_SMALL && !largeNode)
    {
        gtDebugFlags |= GTF_DEBUG_NODE_SMALL;
    }
    else if (size == TREE_NODE_SZ_LARGE || largeNode)
    {
        gtDebugFlags |= GTF_DEBUG_NODE_LARGE;
    }
    else
    {
        assert(!"bogus node size");
    }
#endif
#endif

#if COUNT_AST_OPERS
    InterlockedIncrement(&s_gtNodeCounts[oper]);
#endif

#ifdef DEBUG
    gtSeqNum = 0;
    gtTreeID = JitTls::GetCompiler()->compGenTreeID++;
    gtVNPair.SetBoth(ValueNumStore::NoVN);
    gtRegTag   = GT_REGTAG_NONE;
    gtOperSave = GT_NONE;
#endif
}

/*****************************************************************************/

inline GenTreeStmt* Compiler::gtNewStmt(GenTreePtr expr, IL_OFFSETX offset)
{
    /* NOTE - GT_STMT is now a small node in retail */

    GenTreeStmt* stmt = new (this, GT_STMT) GenTreeStmt(expr, offset);

    return stmt;
}

/*****************************************************************************/

inline GenTreePtr Compiler::gtNewOperNode(genTreeOps oper, var_types type, GenTreePtr op1, bool doSimplifications)
{
    assert((GenTree::OperKind(oper) & (GTK_UNOP | GTK_BINOP)) != 0);
    assert((GenTree::OperKind(oper) & GTK_EXOP) ==
           0); // Can't use this to construct any types that extend unary/binary operator.
    assert(op1 != nullptr || oper == GT_PHI || oper == GT_RETFILT || oper == GT_NOP ||
           (oper == GT_RETURN && type == TYP_VOID));

    if (doSimplifications)
    {
        // We do some simplifications here.
        // If this gets to be too many, try a switch...
        // TODO-Cleanup: With the factoring out of array bounds checks, it should not be the
        // case that we need to check for the array index case here, but without this check
        // we get failures (see for example jit\Directed\Languages\Python\test_methods_d.exe)
        if (oper == GT_IND)
        {
            // IND(ADDR(IND(x)) == IND(x)
            if (op1->gtOper == GT_ADDR)
            {
                if (op1->gtOp.gtOp1->gtOper == GT_IND && (op1->gtOp.gtOp1->gtFlags & GTF_IND_ARR_INDEX) == 0)
                {
                    op1 = op1->gtOp.gtOp1->gtOp.gtOp1;
                }
            }
        }
        else if (oper == GT_ADDR)
        {
            // if "x" is not an array index, ADDR(IND(x)) == x
            if (op1->gtOper == GT_IND && (op1->gtFlags & GTF_IND_ARR_INDEX) == 0)
            {
                return op1->gtOp.gtOp1;
            }
        }
    }

    GenTreePtr node = new (this, oper) GenTreeOp(oper, type, op1, nullptr);

    //
    // the GT_ADDR of a Local Variable implies GTF_ADDR_ONSTACK
    //
    if ((oper == GT_ADDR) && (op1->OperGet() == GT_LCL_VAR))
    {
        node->gtFlags |= GTF_ADDR_ONSTACK;
    }

    return node;
}

// Returns an opcode that is of the largest node size in use.
inline genTreeOps LargeOpOpcode()
{
#if SMALL_TREE_NODES
    // Allocate a large node
    assert(GenTree::s_gtNodeSizes[GT_CALL] == TREE_NODE_SZ_LARGE);
#endif
    return GT_CALL;
}

/******************************************************************************
 *
 * Use to create nodes which may later be morphed to another (big) operator
 */

inline GenTreePtr Compiler::gtNewLargeOperNode(genTreeOps oper, var_types type, GenTreePtr op1, GenTreePtr op2)
{
    assert((GenTree::OperKind(oper) & (GTK_UNOP | GTK_BINOP)) != 0);
    assert((GenTree::OperKind(oper) & GTK_EXOP) ==
           0); // Can't use this to construct any types that extend unary/binary operator.
#if SMALL_TREE_NODES
    // Allocate a large node

    assert(GenTree::s_gtNodeSizes[oper] == TREE_NODE_SZ_SMALL);

    GenTreePtr node = new (this, LargeOpOpcode()) GenTreeOp(oper, type, op1, op2 DEBUGARG(/*largeNode*/ true));
#else
    GenTreePtr node  = new (this, oper) GenTreeOp(oper, type, op1, op2);
#endif

    return node;
}

/*****************************************************************************
 *
 *  allocates a integer constant entry that represents a handle (something
 *  that may need to be fixed up).
 */

inline GenTreePtr Compiler::gtNewIconHandleNode(
    size_t value, unsigned flags, FieldSeqNode* fields, unsigned handle1, void* handle2)
{
    GenTreePtr node;
    assert((flags & (GTF_ICON_HDL_MASK | GTF_ICON_FIELD_OFF)) != 0);

    // Interpret "fields == NULL" as "not a field."
    if (fields == nullptr)
    {
        fields = FieldSeqStore::NotAField();
    }

#if defined(LATE_DISASM)
    node = new (this, LargeOpOpcode()) GenTreeIntCon(TYP_I_IMPL, value, fields DEBUGARG(/*largeNode*/ true));

    node->gtIntCon.gtIconHdl.gtIconHdl1 = handle1;
    node->gtIntCon.gtIconHdl.gtIconHdl2 = handle2;
#else
    node             = new (this, GT_CNS_INT) GenTreeIntCon(TYP_I_IMPL, value, fields);
#endif
    node->gtFlags |= flags;
    return node;
}

/*****************************************************************************
 *
 *  It may not be allowed to embed HANDLEs directly into the JITed code (for eg,
 *  as arguments to JIT helpers). Get a corresponding value that can be embedded.
 *  These are versions for each specific type of HANDLE
 */

inline GenTreePtr Compiler::gtNewIconEmbScpHndNode(CORINFO_MODULE_HANDLE scpHnd, unsigned hnd1, void* hnd2)
{
    void *embedScpHnd, *pEmbedScpHnd;

    embedScpHnd = (void*)info.compCompHnd->embedModuleHandle(scpHnd, &pEmbedScpHnd);

    assert((!embedScpHnd) != (!pEmbedScpHnd));

    return gtNewIconEmbHndNode(embedScpHnd, pEmbedScpHnd, GTF_ICON_SCOPE_HDL, hnd1, hnd2, scpHnd);
}

//-----------------------------------------------------------------------------

inline GenTreePtr Compiler::gtNewIconEmbClsHndNode(CORINFO_CLASS_HANDLE clsHnd, unsigned hnd1, void* hnd2)
{
    void *embedClsHnd, *pEmbedClsHnd;

    embedClsHnd = (void*)info.compCompHnd->embedClassHandle(clsHnd, &pEmbedClsHnd);

    assert((!embedClsHnd) != (!pEmbedClsHnd));

    return gtNewIconEmbHndNode(embedClsHnd, pEmbedClsHnd, GTF_ICON_CLASS_HDL, hnd1, hnd2, clsHnd);
}

//-----------------------------------------------------------------------------

inline GenTreePtr Compiler::gtNewIconEmbMethHndNode(CORINFO_METHOD_HANDLE methHnd, unsigned hnd1, void* hnd2)
{
    void *embedMethHnd, *pEmbedMethHnd;

    embedMethHnd = (void*)info.compCompHnd->embedMethodHandle(methHnd, &pEmbedMethHnd);

    assert((!embedMethHnd) != (!pEmbedMethHnd));

    return gtNewIconEmbHndNode(embedMethHnd, pEmbedMethHnd, GTF_ICON_METHOD_HDL, hnd1, hnd2, methHnd);
}

//-----------------------------------------------------------------------------

inline GenTreePtr Compiler::gtNewIconEmbFldHndNode(CORINFO_FIELD_HANDLE fldHnd, unsigned hnd1, void* hnd2)
{
    void *embedFldHnd, *pEmbedFldHnd;

    embedFldHnd = (void*)info.compCompHnd->embedFieldHandle(fldHnd, &pEmbedFldHnd);

    assert((!embedFldHnd) != (!pEmbedFldHnd));

    return gtNewIconEmbHndNode(embedFldHnd, pEmbedFldHnd, GTF_ICON_FIELD_HDL, hnd1, hnd2, fldHnd);
}

/*****************************************************************************/

//------------------------------------------------------------------------------
// gtNewHelperCallNode : Helper to create a call helper node.
//
//
// Arguments:
//    helper    - Call helper
//    type      - Type of the node
//    args      - Call args
//
// Return Value:
//    New CT_HELPER node

inline GenTreeCall* Compiler::gtNewHelperCallNode(unsigned helper, var_types type, GenTreeArgList* args)
{
    unsigned     flags  = s_helperCallProperties.NoThrow((CorInfoHelpFunc)helper) ? 0 : GTF_EXCEPT;
    GenTreeCall* result = gtNewCallNode(CT_HELPER, eeFindHelper(helper), type, args);
    result->gtFlags |= flags;

#if DEBUG
    // Helper calls are never candidates.

    result->gtInlineObservation = InlineObservation::CALLSITE_IS_CALL_TO_HELPER;
#endif

    return result;
}

//------------------------------------------------------------------------
// gtNewAllocObjNode: A little helper to create an object allocation node.
//
// Arguments:
//    helper           - Value returned by ICorJitInfo::getNewHelper
//    clsHnd           - Corresponding class handle
//    type             - Tree return type (e.g. TYP_REF)
//    op1              - Node containing an address of VtablePtr
//
// Return Value:
//    Returns GT_ALLOCOBJ node that will be later morphed into an
//    allocation helper call or local variable allocation on the stack.
inline GenTreePtr Compiler::gtNewAllocObjNode(unsigned int         helper,
                                              CORINFO_CLASS_HANDLE clsHnd,
                                              var_types            type,
                                              GenTreePtr           op1)
{
    GenTreePtr node = new (this, GT_ALLOCOBJ) GenTreeAllocObj(type, helper, clsHnd, op1);
    return node;
}

//------------------------------------------------------------------------
// gtNewRuntimeLookup: Helper to create a runtime lookup node
//
// Arguments:
//    hnd - generic handle being looked up
//    hndTyp - type of the generic handle
//    tree - tree for the lookup
//
// Return Value:
//    New GenTreeRuntimeLookup node.
inline GenTree* Compiler::gtNewRuntimeLookup(CORINFO_GENERIC_HANDLE hnd, CorInfoGenericHandleType hndTyp, GenTree* tree)
{
    assert(tree != nullptr);
    GenTree* node = new (this, GT_RUNTIMELOOKUP) GenTreeRuntimeLookup(hnd, hndTyp, tree);
    return node;
}

/*****************************************************************************/

inline GenTreePtr Compiler::gtNewCodeRef(BasicBlock* block)
{
    GenTreePtr node = new (this, GT_LABEL) GenTreeLabel(block);
    return node;
}

/*****************************************************************************
 *
 *  A little helper to create a data member reference node.
 */

inline GenTreePtr Compiler::gtNewFieldRef(
    var_types typ, CORINFO_FIELD_HANDLE fldHnd, GenTreePtr obj, DWORD offset, bool nullcheck)
{
#if SMALL_TREE_NODES
    /* 'GT_FIELD' nodes may later get transformed into 'GT_IND' */

    assert(GenTree::s_gtNodeSizes[GT_IND] <= GenTree::s_gtNodeSizes[GT_FIELD]);
    GenTreePtr tree = new (this, GT_FIELD) GenTreeField(typ);
#else
    GenTreePtr  tree = new (this, GT_FIELD) GenTreeField(typ);
#endif
    tree->gtField.gtFldObj    = obj;
    tree->gtField.gtFldHnd    = fldHnd;
    tree->gtField.gtFldOffset = offset;

#ifdef FEATURE_READYTORUN_COMPILER
    tree->gtField.gtFieldLookup.addr = nullptr;
#endif

    if (nullcheck)
    {
        tree->gtFlags |= GTF_FLD_NULLCHECK;
    }

    // If "obj" is the address of a local, note that a field of that struct local has been accessed.
    if (obj != nullptr && obj->OperGet() == GT_ADDR && varTypeIsStruct(obj->gtOp.gtOp1) &&
        obj->gtOp.gtOp1->OperGet() == GT_LCL_VAR)
    {
        unsigned lclNum                  = obj->gtOp.gtOp1->gtLclVarCommon.gtLclNum;
        lvaTable[lclNum].lvFieldAccessed = 1;
#if defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_)
        // These structs are passed by reference; we should probably be able to treat these
        // as non-global refs, but downstream logic expects these to be marked this way.
        if (lvaTable[lclNum].lvIsParam)
        {
            tree->gtFlags |= GTF_GLOB_REF;
        }
#endif // defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_)
    }
    else
    {
        tree->gtFlags |= GTF_GLOB_REF;
    }

    return tree;
}

/*****************************************************************************
 *
 *  A little helper to create an array index node.
 */

inline GenTreePtr Compiler::gtNewIndexRef(var_types typ, GenTreePtr arrayOp, GenTreePtr indexOp)
{
    GenTreeIndex* gtIndx = new (this, GT_INDEX) GenTreeIndex(typ, arrayOp, indexOp, genTypeSize(typ));

    return gtIndx;
}

//------------------------------------------------------------------------------
// gtNewArrLen : Helper to create an array length node.
//
//
// Arguments:
//    typ      -  Type of the node
//    arrayOp  -  Array node
//    lenOffset - Offset of the length field
//
// Return Value:
//    New GT_ARR_LENGTH node

inline GenTreeArrLen* Compiler::gtNewArrLen(var_types typ, GenTree* arrayOp, int lenOffset)
{
    GenTreeArrLen* arrLen = new (this, GT_ARR_LENGTH) GenTreeArrLen(typ, arrayOp, lenOffset);
    static_assert_no_msg(GTF_ARRLEN_NONFAULTING == GTF_IND_NONFAULTING);
    arrLen->SetIndirExceptionFlags(this);
    return arrLen;
}

//------------------------------------------------------------------------------
// gtNewIndir : Helper to create an indirection node.
//
// Arguments:
//    typ   -  Type of the node
//    addr  -  Address of the indirection
//
// Return Value:
//    New GT_IND node

inline GenTree* Compiler::gtNewIndir(var_types typ, GenTree* addr)
{
    GenTree* indir = gtNewOperNode(GT_IND, typ, addr);
    indir->SetIndirExceptionFlags(this);
    return indir;
}

/*****************************************************************************
 *
 *  Create (and check for) a "nothing" node, i.e. a node that doesn't produce
 *  any code. We currently use a "nop" node of type void for this purpose.
 */

inline GenTreePtr Compiler::gtNewNothingNode()
{
    return new (this, GT_NOP) GenTreeOp(GT_NOP, TYP_VOID);
}
/*****************************************************************************/

inline bool GenTree::IsNothingNode() const
{
    return (gtOper == GT_NOP && gtType == TYP_VOID);
}

/*****************************************************************************
 *
 *  Change the given node to a NOP - May be later changed to a GT_COMMA
 *
 *****************************************************************************/

inline void GenTree::gtBashToNOP()
{
    ChangeOper(GT_NOP);

    gtType     = TYP_VOID;
    gtOp.gtOp1 = gtOp.gtOp2 = nullptr;

    gtFlags &= ~(GTF_ALL_EFFECT | GTF_REVERSE_OPS);
}

// return new arg placeholder node.  Does not do anything but has a type associated
// with it so we can keep track of register arguments in lists associated w/ call nodes

inline GenTreePtr Compiler::gtNewArgPlaceHolderNode(var_types type, CORINFO_CLASS_HANDLE clsHnd)
{
    GenTreePtr node = new (this, GT_ARGPLACE) GenTreeArgPlace(type, clsHnd);
    return node;
}

/*****************************************************************************/

inline GenTreePtr Compiler::gtUnusedValNode(GenTreePtr expr)
{
    return gtNewOperNode(GT_COMMA, TYP_VOID, expr, gtNewNothingNode());
}

/*****************************************************************************
 *
 * A wrapper for gtSetEvalOrder and gtComputeFPlvls
 * Necessary because the FP levels may need to be re-computed if we reverse
 * operands
 */

inline void Compiler::gtSetStmtInfo(GenTree* stmt)
{
    assert(stmt->gtOper == GT_STMT);
    GenTreePtr expr = stmt->gtStmt.gtStmtExpr;

#if FEATURE_STACK_FP_X87
    /* We will try to compute the FP stack level at each node */
    codeGen->genResetFPstkLevel();

    /* Sometimes we need to redo the FP level computation */
    gtFPstLvlRedo = false;
#endif // FEATURE_STACK_FP_X87

#ifdef DEBUG
    if (verbose && 0)
    {
        gtDispTree(stmt);
    }
#endif

    /* Recursively process the expression */

    gtSetEvalOrder(expr);

    // Set the statement to have the same costs as the top node of the tree.
    stmt->CopyCosts(expr);

#if FEATURE_STACK_FP_X87
    /* Unused float values leave one operand on the stack */
    assert(codeGen->genGetFPstkLevel() == 0 || codeGen->genGetFPstkLevel() == 1);

    /* Do we need to recompute FP stack levels? */

    if (gtFPstLvlRedo)
    {
        codeGen->genResetFPstkLevel();
        gtComputeFPlvls(expr);
        assert(codeGen->genGetFPstkLevel() == 0 || codeGen->genGetFPstkLevel() == 1);
    }
#endif // FEATURE_STACK_FP_X87
}

#if FEATURE_STACK_FP_X87
inline unsigned Compiler::gtSetEvalOrderAndRestoreFPstkLevel(GenTree* tree)
{
    unsigned FPlvlSave     = codeGen->genFPstkLevel;
    unsigned result        = gtSetEvalOrder(tree);
    codeGen->genFPstkLevel = FPlvlSave;

    return result;
}
#else  // !FEATURE_STACK_FP_X87
inline unsigned Compiler::gtSetEvalOrderAndRestoreFPstkLevel(GenTree* tree)
{
    return gtSetEvalOrder(tree);
}
#endif // FEATURE_STACK_FP_X87

/*****************************************************************************/
#if SMALL_TREE_NODES
/*****************************************************************************/

inline void GenTree::SetOper(genTreeOps oper, ValueNumberUpdate vnUpdate)
{
    assert(((gtDebugFlags & GTF_DEBUG_NODE_SMALL) != 0) != ((gtDebugFlags & GTF_DEBUG_NODE_LARGE) != 0));

    /* Make sure the node isn't too small for the new operator */

    assert(GenTree::s_gtNodeSizes[gtOper] == TREE_NODE_SZ_SMALL ||
           GenTree::s_gtNodeSizes[gtOper] == TREE_NODE_SZ_LARGE);

    assert(GenTree::s_gtNodeSizes[oper] == TREE_NODE_SZ_SMALL || GenTree::s_gtNodeSizes[oper] == TREE_NODE_SZ_LARGE);
    assert(GenTree::s_gtNodeSizes[oper] == TREE_NODE_SZ_SMALL || (gtDebugFlags & GTF_DEBUG_NODE_LARGE));

    SetOperRaw(oper);

#ifdef DEBUG
    // Maintain the invariant that unary operators always have NULL gtOp2.
    // If we ever start explicitly allocating GenTreeUnOp nodes, we wouldn't be
    // able to do that (but if we did, we'd have to have a check in gtOp -- perhaps
    // a gtUnOp...)
    if (OperKind(oper) == GTK_UNOP)
    {
        gtOp.gtOp2 = nullptr;
    }
#endif // DEBUG

#if DEBUGGABLE_GENTREE
    // Until we eliminate SetOper/ChangeOper, we also change the vtable of the node, so that
    // it shows up correctly in the debugger.
    SetVtableForOper(oper);
#endif // DEBUGGABLE_GENTREE

    if (oper == GT_CNS_INT)
    {
        gtIntCon.gtFieldSeq = nullptr;
    }

    if (vnUpdate == CLEAR_VN)
    {
        // Clear the ValueNum field as well.
        gtVNPair.SetBoth(ValueNumStore::NoVN);
    }
}

inline GenTreePtr Compiler::gtNewCastNode(var_types typ, GenTreePtr op1, var_types castType)
{
    GenTreePtr res = new (this, GT_CAST) GenTreeCast(typ, op1, castType);
    return res;
}

inline GenTreePtr Compiler::gtNewCastNodeL(var_types typ, GenTreePtr op1, var_types castType)
{
    /* Some casts get transformed into 'GT_CALL' or 'GT_IND' nodes */

    assert(GenTree::s_gtNodeSizes[GT_CALL] >= GenTree::s_gtNodeSizes[GT_CAST]);
    assert(GenTree::s_gtNodeSizes[GT_CALL] >= GenTree::s_gtNodeSizes[GT_IND]);

    /* Make a big node first and then change it to be GT_CAST */

    GenTreePtr res = new (this, LargeOpOpcode()) GenTreeCast(typ, op1, castType DEBUGARG(/*largeNode*/ true));
    return res;
}

/*****************************************************************************/
#else // SMALL_TREE_NODES
/*****************************************************************************/

inline void GenTree::InitNodeSize()
{
}

inline void GenTree::SetOper(genTreeOps oper, ValueNumberUpdate vnUpdate)
{
    SetOperRaw(oper);

    if (vnUpdate == CLEAR_VN)
    {
        // Clear the ValueNum field.
        gtVNPair.SetBoth(ValueNumStore::NoVN);
    }
}

inline void GenTree::ReplaceWith(GenTreePtr src)
{
    RecordOperBashing(OperGet(), src->OperGet()); // nop unless NODEBASH_STATS is enabled
    *this    = *src;
#ifdef DEBUG
    gtSeqNum = 0;
#endif
}

inline GenTreePtr Compiler::gtNewCastNode(var_types typ, GenTreePtr op1, var_types castType)
{
    GenTreePtr tree         = gtNewOperNode(GT_CAST, typ, op1);
    tree->gtCast.gtCastType = castType;
}

inline GenTreePtr Compiler::gtNewCastNodeL(var_types typ, GenTreePtr op1, var_types castType)
{
    return gtNewCastNode(typ, op1, castType);
}

/*****************************************************************************/
#endif // SMALL_TREE_NODES
/*****************************************************************************/

/*****************************************************************************/

inline void GenTree::SetOperRaw(genTreeOps oper)
{
    // Please do not do anything here other than assign to gtOper (debug-only
    // code is OK, but should be kept to a minimum).
    RecordOperBashing(OperGet(), oper); // nop unless NODEBASH_STATS is enabled
    gtOper = oper;
}

inline void GenTree::SetOperResetFlags(genTreeOps oper)
{
    SetOper(oper);
    gtFlags &= GTF_NODE_MASK;
}

inline void GenTree::ChangeOperConst(genTreeOps oper)
{
#ifdef _TARGET_64BIT_
    assert(oper != GT_CNS_LNG); // We should never see a GT_CNS_LNG for a 64-bit target!
#endif
    assert(OperIsConst(oper)); // use ChangeOper() instead
    SetOperResetFlags(oper);
    // Some constant subtypes have additional fields that must be initialized.
    if (oper == GT_CNS_INT)
    {
        gtIntCon.gtFieldSeq = FieldSeqStore::NotAField();
    }
}

inline void GenTree::ChangeOper(genTreeOps oper, ValueNumberUpdate vnUpdate)
{
    assert(!OperIsConst(oper)); // use ChangeOperLeaf() instead

    unsigned mask = GTF_COMMON_MASK;
    if (this->OperIsIndirOrArrLength() && OperIsIndirOrArrLength(oper))
    {
        mask |= GTF_IND_NONFAULTING;
    }
    SetOper(oper, vnUpdate);
    gtFlags &= mask;

    // Do "oper"-specific initializations...
    switch (oper)
    {
        case GT_LCL_FLD:
            gtLclFld.gtLclOffs  = 0;
            gtLclFld.gtFieldSeq = FieldSeqStore::NotAField();
            break;
        default:
            break;
    }
}

inline void GenTree::ChangeOperUnchecked(genTreeOps oper)
{
    unsigned mask = GTF_COMMON_MASK;
    if (this->OperIsIndirOrArrLength() && OperIsIndirOrArrLength(oper))
    {
        mask |= GTF_IND_NONFAULTING;
    }
    SetOperRaw(oper); // Trust the caller and don't use SetOper()
    gtFlags &= mask;
}

/*****************************************************************************
 * Returns true if the node is &var (created by ldarga and ldloca)
 */

inline bool GenTree::IsVarAddr() const
{
    if (gtOper == GT_ADDR)
    {
        if (gtFlags & GTF_ADDR_ONSTACK)
        {
            assert((gtType == TYP_BYREF) || (gtType == TYP_I_IMPL));
            return true;
        }
    }
    return false;
}

/*****************************************************************************
 *
 * Returns true if the node is of the "ovf" variety, for example, add.ovf.i1.
 * + gtOverflow() can only be called for valid operators (that is, we know it is one
 *   of the operators which may have GTF_OVERFLOW set).
 * + gtOverflowEx() is more expensive, and should be called only if gtOper may be
 *   an operator for which GTF_OVERFLOW is invalid.
 */

inline bool GenTree::gtOverflow() const
{
    assert(OperMayOverflow());

    if ((gtFlags & GTF_OVERFLOW) != 0)
    {
        assert(varTypeIsIntegral(TypeGet()));

        return true;
    }
    else
    {
        return false;
    }
}

inline bool GenTree::gtOverflowEx() const
{
    return OperMayOverflow() && gtOverflow();
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          LclVarsInfo                                      XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

inline bool Compiler::lvaHaveManyLocals() const
{
    return (lvaCount >= lclMAX_TRACKED);
}

/*****************************************************************************
 *
 *  Allocate a temporary variable or a set of temp variables.
 */

inline unsigned Compiler::lvaGrabTemp(bool shortLifetime DEBUGARG(const char* reason))
{
    if (compIsForInlining())
    {
        // Grab the temp using Inliner's Compiler instance.
        Compiler* pComp = impInlineInfo->InlinerCompiler; // The Compiler instance for the caller (i.e. the inliner)

        if (pComp->lvaHaveManyLocals())
        {
            // Don't create more LclVar with inlining
            compInlineResult->NoteFatal(InlineObservation::CALLSITE_TOO_MANY_LOCALS);
        }

        unsigned tmpNum = pComp->lvaGrabTemp(shortLifetime DEBUGARG(reason));
        lvaTable        = pComp->lvaTable;
        lvaCount        = pComp->lvaCount;
        lvaTableCnt     = pComp->lvaTableCnt;
        return tmpNum;
    }

    // You cannot allocate more space after frame layout!
    noway_assert(lvaDoneFrameLayout < Compiler::TENTATIVE_FRAME_LAYOUT);

    /* Check if the lvaTable has to be grown */
    if (lvaCount + 1 > lvaTableCnt)
    {
        unsigned newLvaTableCnt = lvaCount + (lvaCount / 2) + 1;

        // Check for overflow
        if (newLvaTableCnt <= lvaCount)
        {
            IMPL_LIMITATION("too many locals");
        }

        // Note: compGetMemArray might throw.
        LclVarDsc* newLvaTable = (LclVarDsc*)compGetMemArray(newLvaTableCnt, sizeof(*lvaTable), CMK_LvaTable);

        memcpy(newLvaTable, lvaTable, lvaCount * sizeof(*lvaTable));
        memset(newLvaTable + lvaCount, 0, (newLvaTableCnt - lvaCount) * sizeof(*lvaTable));

        for (unsigned i = lvaCount; i < newLvaTableCnt; i++)
        {
            new (&newLvaTable[i], jitstd::placement_t()) LclVarDsc(this); // call the constructor.
        }

#if 0
        // TODO-Cleanup: Enable this and test.
#ifdef DEBUG
        // Fill the old table with junks. So to detect the un-intended use.
        memset(lvaTable, fDefaultFill2.val_DontUse_(CLRConfig::INTERNAL_JitDefaultFill, 0xFF), lvaCount * sizeof(*lvaTable));
#endif
#endif

        lvaTableCnt = newLvaTableCnt;
        lvaTable    = newLvaTable;
    }

    lvaTable[lvaCount].lvType    = TYP_UNDEF; // Initialize lvType, lvIsTemp and lvOnFrame
    lvaTable[lvaCount].lvIsTemp  = shortLifetime;
    lvaTable[lvaCount].lvOnFrame = true;

    unsigned tempNum = lvaCount;

    lvaCount++;

#ifdef DEBUG
    if (verbose)
    {
        printf("\nlvaGrabTemp returning %d (", tempNum);
        gtDispLclVar(tempNum, false);
        printf(")%s called for %s.\n", shortLifetime ? "" : " (a long lifetime temp)", reason);
    }
#endif // DEBUG

    return tempNum;
}

inline unsigned Compiler::lvaGrabTemps(unsigned cnt DEBUGARG(const char* reason))
{
    if (compIsForInlining())
    {
        // Grab the temps using Inliner's Compiler instance.
        unsigned tmpNum = impInlineInfo->InlinerCompiler->lvaGrabTemps(cnt DEBUGARG(reason));

        lvaTable    = impInlineInfo->InlinerCompiler->lvaTable;
        lvaCount    = impInlineInfo->InlinerCompiler->lvaCount;
        lvaTableCnt = impInlineInfo->InlinerCompiler->lvaTableCnt;
        return tmpNum;
    }

#ifdef DEBUG
    if (verbose)
    {
        printf("\nlvaGrabTemps(%d) returning %d..%d (long lifetime temps) called for %s", cnt, lvaCount,
               lvaCount + cnt - 1, reason);
    }
#endif

    // You cannot allocate more space after frame layout!
    noway_assert(lvaDoneFrameLayout < Compiler::TENTATIVE_FRAME_LAYOUT);

    /* Check if the lvaTable has to be grown */
    if (lvaCount + cnt > lvaTableCnt)
    {
        unsigned newLvaTableCnt = lvaCount + max(lvaCount / 2 + 1, cnt);

        // Check for overflow
        if (newLvaTableCnt <= lvaCount)
        {
            IMPL_LIMITATION("too many locals");
        }

        // Note: compGetMemArray might throw.
        LclVarDsc* newLvaTable = (LclVarDsc*)compGetMemArray(newLvaTableCnt, sizeof(*lvaTable), CMK_LvaTable);

        memcpy(newLvaTable, lvaTable, lvaCount * sizeof(*lvaTable));
        memset(newLvaTable + lvaCount, 0, (newLvaTableCnt - lvaCount) * sizeof(*lvaTable));
        for (unsigned i = lvaCount; i < newLvaTableCnt; i++)
        {
            new (&newLvaTable[i], jitstd::placement_t()) LclVarDsc(this); // call the constructor.
        }

#if 0
#ifdef DEBUG
        // TODO-Cleanup: Enable this and test.
        // Fill the old table with junks. So to detect the un-intended use.
        memset(lvaTable, fDefaultFill2.val_DontUse_(CLRConfig::INTERNAL_JitDefaultFill, 0xFF), lvaCount * sizeof(*lvaTable));
#endif
#endif

        lvaTableCnt = newLvaTableCnt;
        lvaTable    = newLvaTable;
    }

    unsigned tempNum = lvaCount;

    while (cnt--)
    {
        lvaTable[lvaCount].lvType    = TYP_UNDEF; // Initialize lvType, lvIsTemp and lvOnFrame
        lvaTable[lvaCount].lvIsTemp  = false;
        lvaTable[lvaCount].lvOnFrame = true;
        lvaCount++;
    }

    return tempNum;
}

/*****************************************************************************
 *
 *  Allocate a temporary variable which is implicitly used by code-gen
 *  There will be no explicit references to the temp, and so it needs to
 *  be forced to be kept alive, and not be optimized away.
 */

inline unsigned Compiler::lvaGrabTempWithImplicitUse(bool shortLifetime DEBUGARG(const char* reason))
{
    if (compIsForInlining())
    {
        // Grab the temp using Inliner's Compiler instance.
        unsigned tmpNum = impInlineInfo->InlinerCompiler->lvaGrabTempWithImplicitUse(shortLifetime DEBUGARG(reason));

        lvaTable    = impInlineInfo->InlinerCompiler->lvaTable;
        lvaCount    = impInlineInfo->InlinerCompiler->lvaCount;
        lvaTableCnt = impInlineInfo->InlinerCompiler->lvaTableCnt;
        return tmpNum;
    }

    unsigned lclNum = lvaGrabTemp(shortLifetime DEBUGARG(reason));

    LclVarDsc* varDsc = &lvaTable[lclNum];

    // This will prevent it from being optimized away
    // TODO-CQ: We shouldn't have to go as far as to declare these
    // address-exposed -- DoNotEnregister should suffice?
    lvaSetVarAddrExposed(lclNum);

    // We need lvRefCnt to be non-zero to prevent various asserts from firing.
    varDsc->lvRefCnt    = 1;
    varDsc->lvRefCntWtd = BB_UNITY_WEIGHT;

    return lclNum;
}

/*****************************************************************************
 *
 *  If lvaTrackedFixed is false then set the lvaSortAgain flag
 *   (this allows us to grow the number of tracked variables)
 *   and zero lvRefCntWtd when lvRefCnt is zero
 */

inline void LclVarDsc::lvaResetSortAgainFlag(Compiler* comp)
{
    if (!comp->lvaTrackedFixed)
    {
        /* Flag this change, set lvaSortAgain to true */
        comp->lvaSortAgain = true;
    }
    /* Set weighted ref count to zero if  ref count is zero */
    if (lvRefCnt == 0)
    {
        lvRefCntWtd = 0;
    }
}

/*****************************************************************************
 *
 *  Decrement the ref counts for a local variable
 */

inline void LclVarDsc::decRefCnts(BasicBlock::weight_t weight, Compiler* comp, bool propagate)
{
    /* Decrement lvRefCnt and lvRefCntWtd */
    Compiler::lvaPromotionType promotionType = DUMMY_INIT(Compiler::PROMOTION_TYPE_NONE);
    if (varTypeIsStruct(lvType))
    {
        promotionType = comp->lvaGetPromotionType(this);
    }

    //
    // Decrement counts on the local itself.
    //
    if (lvType != TYP_STRUCT || promotionType != Compiler::PROMOTION_TYPE_INDEPENDENT)
    {
        assert(lvRefCnt); // Can't decrement below zero

        // TODO: Well, the assert above could be bogus.
        // If lvRefCnt has overflowed before, then might drop to 0.
        // Therefore we do need the following check to keep lvRefCnt from underflow:
        if (lvRefCnt > 0)
        {
            //
            // Decrement lvRefCnt
            //
            lvRefCnt--;

            //
            // Decrement lvRefCntWtd
            //
            if (weight != 0)
            {
                if (lvIsTemp && (weight * 2 > weight))
                {
                    weight *= 2;
                }

                if (lvRefCntWtd <= weight)
                { // Can't go below zero
                    lvRefCntWtd = 0;
                }
                else
                {
                    lvRefCntWtd -= weight;
                }
            }
        }
    }

    if (varTypeIsStruct(lvType) && propagate)
    {
        // For promoted struct locals, decrement lvRefCnt on its field locals as well.
        if (promotionType == Compiler::PROMOTION_TYPE_INDEPENDENT ||
            promotionType == Compiler::PROMOTION_TYPE_DEPENDENT)
        {
            for (unsigned i = lvFieldLclStart; i < lvFieldLclStart + lvFieldCnt; ++i)
            {
                comp->lvaTable[i].decRefCnts(comp->lvaMarkRefsWeight, comp, false); // Don't propagate
            }
        }
    }

    if (lvIsStructField && propagate)
    {
        // Depending on the promotion type, decrement the ref count for the parent struct as well.
        promotionType           = comp->lvaGetParentPromotionType(this);
        LclVarDsc* parentvarDsc = &comp->lvaTable[lvParentLcl];
        assert(!parentvarDsc->lvRegStruct);
        if (promotionType == Compiler::PROMOTION_TYPE_DEPENDENT)
        {
            parentvarDsc->decRefCnts(comp->lvaMarkRefsWeight, comp, false); // Don't propagate
        }
    }

    lvaResetSortAgainFlag(comp);

#ifdef DEBUG
    if (comp->verbose)
    {
        unsigned varNum = (unsigned)(this - comp->lvaTable);
        assert(&comp->lvaTable[varNum] == this);
        printf("New refCnts for V%02u: refCnt = %2u, refCntWtd = %s\n", varNum, lvRefCnt, refCntWtd2str(lvRefCntWtd));
    }
#endif
}

/*****************************************************************************
 *
 *  Increment the ref counts for a local variable
 */

inline void LclVarDsc::incRefCnts(BasicBlock::weight_t weight, Compiler* comp, bool propagate)
{
    Compiler::lvaPromotionType promotionType = DUMMY_INIT(Compiler::PROMOTION_TYPE_NONE);
    if (varTypeIsStruct(lvType))
    {
        promotionType = comp->lvaGetPromotionType(this);
    }

    //
    // Increment counts on the local itself.
    //
    if (lvType != TYP_STRUCT || promotionType != Compiler::PROMOTION_TYPE_INDEPENDENT)
    {
        //
        // Increment lvRefCnt
        //
        int newRefCnt = lvRefCnt + 1;
        if (newRefCnt == (unsigned short)newRefCnt) // lvRefCnt is an "unsigned short". Don't overflow it.
        {
            lvRefCnt = (unsigned short)newRefCnt;
        }

        // This fires when an uninitialize value for 'weight' is used (see lvaMarkRefsWeight)
        assert(weight != 0xdddddddd);
        //
        // Increment lvRefCntWtd
        //
        if (weight != 0)
        {
            // We double the weight of internal temps
            //
            if (lvIsTemp && (weight * 2 > weight))
            {
                weight *= 2;
            }

            unsigned newWeight = lvRefCntWtd + weight;
            if (newWeight >= lvRefCntWtd)
            { // lvRefCntWtd is an "unsigned".  Don't overflow it
                lvRefCntWtd = newWeight;
            }
            else
            { // On overflow we assign ULONG_MAX
                lvRefCntWtd = ULONG_MAX;
            }
        }
    }

    if (varTypeIsStruct(lvType) && propagate)
    {
        // For promoted struct locals, increment lvRefCnt on its field locals as well.
        if (promotionType == Compiler::PROMOTION_TYPE_INDEPENDENT ||
            promotionType == Compiler::PROMOTION_TYPE_DEPENDENT)
        {
            for (unsigned i = lvFieldLclStart; i < lvFieldLclStart + lvFieldCnt; ++i)
            {
                comp->lvaTable[i].incRefCnts(comp->lvaMarkRefsWeight, comp, false); // Don't propagate
            }
        }
    }

    if (lvIsStructField && propagate)
    {
        // Depending on the promotion type, increment the ref count for the parent struct as well.
        promotionType           = comp->lvaGetParentPromotionType(this);
        LclVarDsc* parentvarDsc = &comp->lvaTable[lvParentLcl];
        assert(!parentvarDsc->lvRegStruct);
        if (promotionType == Compiler::PROMOTION_TYPE_DEPENDENT)
        {
            parentvarDsc->incRefCnts(comp->lvaMarkRefsWeight, comp, false); // Don't propagate
        }
    }

    lvaResetSortAgainFlag(comp);

#ifdef DEBUG
    if (comp->verbose)
    {
        unsigned varNum = (unsigned)(this - comp->lvaTable);
        assert(&comp->lvaTable[varNum] == this);
        printf("New refCnts for V%02u: refCnt = %2u, refCntWtd = %s\n", varNum, lvRefCnt, refCntWtd2str(lvRefCntWtd));
    }
#endif
}

/*****************************************************************************
 *
 *  Set the lvPrefReg field to reg
 */

inline void LclVarDsc::setPrefReg(regNumber regNum, Compiler* comp)
{
    regMaskTP regMask;
    if (isFloatRegType(TypeGet()))
    {
        // Check for FP struct-promoted field being passed in integer register
        //
        if (!genIsValidFloatReg(regNum))
        {
            return;
        }
        regMask = genRegMaskFloat(regNum, TypeGet());
    }
    else
    {
        regMask = genRegMask(regNum);
    }

#ifdef _TARGET_ARM_
    // Don't set a preferred register for a TYP_STRUCT that takes more than one register slot
    if ((TypeGet() == TYP_STRUCT) && (lvSize() > REGSIZE_BYTES))
        return;
#endif

    /* Only interested if we have a new register bit set */
    if (lvPrefReg & regMask)
    {
        return;
    }

#ifdef DEBUG
    if (comp->verbose)
    {
        if (lvPrefReg)
        {
            printf("Change preferred register for V%02u from ", this - comp->lvaTable);
            dspRegMask(lvPrefReg);
        }
        else
        {
            printf("Set preferred register for V%02u", this - comp->lvaTable);
        }
        printf(" to ");
        dspRegMask(regMask);
        printf("\n");
    }
#endif

    /* Overwrite the lvPrefReg field */

    lvPrefReg = (regMaskSmall)regMask;

#ifdef LEGACY_BACKEND
    // This is specific to the classic register allocator.
    // While walking the trees during reg predict we set the lvPrefReg mask
    // and then re-sort the 'tracked' variable when the lvPrefReg mask changes.
    if (lvTracked)
    {
        /* Flag this change, set lvaSortAgain to true */
        comp->lvaSortAgain = true;
    }
#endif // LEGACY_BACKEND
}

/*****************************************************************************
 *
 *  Add regMask to the lvPrefReg field
 */

inline void LclVarDsc::addPrefReg(regMaskTP regMask, Compiler* comp)
{
    assert(regMask != RBM_NONE);

#ifdef _TARGET_ARM_
    // Don't set a preferred register for a TYP_STRUCT that takes more than one register slot
    if ((lvType == TYP_STRUCT) && (lvSize() > sizeof(void*)))
        return;
#endif

    /* Only interested if we have a new register bit set */
    if (lvPrefReg & regMask)
    {
        return;
    }

#ifdef DEBUG
    if (comp->verbose)
    {
        if (lvPrefReg)
        {
            printf("Additional preferred register for V%02u from ", this - comp->lvaTable);
            dspRegMask(lvPrefReg);
        }
        else
        {
            printf("Set preferred register for V%02u", this - comp->lvaTable);
        }
        printf(" to ");
        dspRegMask(lvPrefReg | regMask);
        printf("\n");
    }
#endif

    /* Update the lvPrefReg field */

    lvPrefReg |= regMask;

#ifdef LEGACY_BACKEND
    // This is specific to the classic register allocator
    // While walking the trees during reg predict we set the lvPrefReg mask
    // and then resort the 'tracked' variable when the lvPrefReg mask changes
    if (lvTracked)
    {
        /* Flag this change, set lvaSortAgain to true */
        comp->lvaSortAgain = true;
    }
#endif // LEGACY_BACKEND
}

/*****************************************************************************
 *
 *  The following returns the mask of all tracked locals
 *  referenced in a statement.
 */

inline VARSET_VALRET_TP Compiler::lvaStmtLclMask(GenTreePtr stmt)
{
    GenTreePtr tree;
    unsigned   varNum;
    LclVarDsc* varDsc;
    VARSET_TP  lclMask(VarSetOps::MakeEmpty(this));

    assert(stmt->gtOper == GT_STMT);
    assert(fgStmtListThreaded);

    for (tree = stmt->gtStmt.gtStmtList; tree; tree = tree->gtNext)
    {
        if (tree->gtOper != GT_LCL_VAR)
        {
            continue;
        }

        varNum = tree->gtLclVarCommon.gtLclNum;
        assert(varNum < lvaCount);
        varDsc = lvaTable + varNum;

        if (!varDsc->lvTracked)
        {
            continue;
        }

        VarSetOps::UnionD(this, lclMask, VarSetOps::MakeSingleton(this, varDsc->lvVarIndex));
    }

    return lclMask;
}

/*****************************************************************************
 * Returns true if the lvType is a TYP_REF or a TYP_BYREF.
 * When the lvType is a TYP_STRUCT it searches the GC layout
 * of the struct and returns true iff it contains a GC ref.
 */

inline bool Compiler::lvaTypeIsGC(unsigned varNum)
{
    if (lvaTable[varNum].TypeGet() == TYP_STRUCT)
    {
        assert(lvaTable[varNum].lvGcLayout != nullptr); // bits are intialized
        return (lvaTable[varNum].lvStructGcCount != 0);
    }
    return (varTypeIsGC(lvaTable[varNum].TypeGet()));
}

/*****************************************************************************
 Is this a synchronized instance method? If so, we will need to report "this"
 in the GC information, so that the EE can release the object lock
 in case of an exception

 We also need to report "this" and keep it alive for all shared generic
 code that gets the actual generic context from the "this" pointer and
 has exception handlers.

 For example, if List<T>::m() is shared between T = object and T = string,
 then inside m() an exception handler "catch E<T>" needs to be able to fetch
 the 'this' pointer to find out what 'T' is in order to tell if we
 should catch the exception or not.
 */

inline bool Compiler::lvaKeepAliveAndReportThis()
{
    if (info.compIsStatic || lvaTable[0].TypeGet() != TYP_REF)
    {
        return false;
    }

    const bool genericsContextIsThis = (info.compMethodInfo->options & CORINFO_GENERICS_CTXT_FROM_THIS) != 0;

#ifdef JIT32_GCENCODER

    if (info.compFlags & CORINFO_FLG_SYNCH)
        return true;

    if (genericsContextIsThis)
    {
        // TODO: Check if any of the exception clauses are
        // typed using a generic type. Else, we do not need to report this.
        if (info.compXcptnsCount > 0)
            return true;

        if (opts.compDbgCode)
            return true;

        if (lvaGenericsContextUseCount > 0)
        {
            JITDUMP("Reporting this as generic context: %u refs\n", lvaGenericsContextUseCount);
            return true;
        }
    }
#else // !JIT32_GCENCODER
    // If the generics context is the this pointer we need to report it if either
    // the VM requires us to keep the generics context alive or it is used in a look-up.
    // We keep it alive in the lookup scenario, even when the VM didn't ask us to,
    // because collectible types need the generics context when gc-ing.
    if (genericsContextIsThis)
    {
        const bool isUsed   = lvaGenericsContextUseCount > 0;
        const bool mustKeep = (info.compMethodInfo->options & CORINFO_GENERICS_CTXT_KEEP_ALIVE) != 0;

        if (isUsed || mustKeep)
        {
            JITDUMP("Reporting this as generic context: %u refs%s\n", lvaGenericsContextUseCount,
                    mustKeep ? ", must keep" : "");

            return true;
        }
    }
#endif

    return false;
}

/*****************************************************************************
  Similar to lvaKeepAliveAndReportThis
 */

inline bool Compiler::lvaReportParamTypeArg()
{
    if (info.compMethodInfo->options & (CORINFO_GENERICS_CTXT_FROM_METHODDESC | CORINFO_GENERICS_CTXT_FROM_METHODTABLE))
    {
        assert(info.compTypeCtxtArg != -1);

        // If the VM requires us to keep the generics context alive and report it (for example, if any catch
        // clause catches a type that uses a generic parameter of this method) this flag will be set.
        if (info.compMethodInfo->options & CORINFO_GENERICS_CTXT_KEEP_ALIVE)
        {
            return true;
        }

        // Otherwise, if an exact type parameter is needed in the body, report the generics context.
        // We do this because collectible types needs the generics context when gc-ing.
        if (lvaGenericsContextUseCount > 0)
        {
            return true;
        }
    }

    // Otherwise, we don't need to report it -- the generics context parameter is unused.
    return false;
}

//*****************************************************************************

inline unsigned Compiler::lvaCachedGenericContextArgOffset()
{
    assert(lvaDoneFrameLayout == FINAL_FRAME_LAYOUT);

    return lvaCachedGenericContextArgOffs;
}

/*****************************************************************************
 *
 *  Return the stack framed offset of the given variable; set *FPbased to
 *  true if the variable is addressed off of FP, false if it's addressed
 *  off of SP. Note that 'varNum' can be a negated spill-temporary var index.
 *
 *  mustBeFPBased - strong about whether the base reg is FP. But it is also
 *  strong about not being FPBased after FINAL_FRAME_LAYOUT. i.e.,
 *  it enforces SP based.
 *
 *  addrModeOffset - is the addressing mode offset, for example: v02 + 0x10
 *  So, V02 itself is at offset sp + 0x10 and then addrModeOffset is what gets
 *  added beyond that.
 */

inline
#ifdef _TARGET_ARM_
    int
    Compiler::lvaFrameAddress(int varNum, bool mustBeFPBased, regNumber* pBaseReg, int addrModeOffset)
#else
    int
    Compiler::lvaFrameAddress(int varNum, bool* pFPbased)
#endif
{
    assert(lvaDoneFrameLayout != NO_FRAME_LAYOUT);

    int       offset;
    bool      FPbased;
    bool      fConservative = false;
    var_types type          = TYP_UNDEF;
    if (varNum >= 0)
    {
        LclVarDsc* varDsc;

        assert((unsigned)varNum < lvaCount);
        varDsc               = lvaTable + varNum;
        type                 = varDsc->TypeGet();
        bool isPrespilledArg = false;
#if defined(_TARGET_ARM_) && defined(PROFILING_SUPPORTED)
        isPrespilledArg = varDsc->lvIsParam && compIsProfilerHookNeeded() &&
                          lvaIsPreSpilled(varNum, codeGen->regSet.rsMaskPreSpillRegs(false));
#endif

        // If we have finished with register allocation, and this isn't a stack-based local,
        // check that this has a valid stack location.
        if (lvaDoneFrameLayout > REGALLOC_FRAME_LAYOUT && !varDsc->lvOnFrame)
        {
#ifdef _TARGET_AMD64_
#ifndef FEATURE_UNIX_AMD64_STRUCT_PASSING
            // On amd64, every param has a stack location, except on Unix-like systems.
            assert(varDsc->lvIsParam);
#endif // FEATURE_UNIX_AMD64_STRUCT_PASSING
#elif !defined(LEGACY_BACKEND)
            // For !LEGACY_BACKEND on other targets, a stack parameter that is enregistered or prespilled
            // for profiling on ARM will have a stack location.
            assert((varDsc->lvIsParam && !varDsc->lvIsRegArg) || isPrespilledArg);
#else  // !(_TARGET_AMD64 || defined(LEGACY_BACKEND))
            // Otherwise, we only have a valid stack location for:
            // A parameter that was passed on the stack, being homed into its register home,
            // or a prespilled argument on arm under profiler.
            assert((varDsc->lvIsParam && !varDsc->lvIsRegArg && varDsc->lvRegister) || isPrespilledArg);
#endif // !(_TARGET_AMD64 || defined(LEGACY_BACKEND))
        }

        FPbased = varDsc->lvFramePointerBased;

#ifdef DEBUG
#if FEATURE_FIXED_OUT_ARGS
        if ((unsigned)varNum == lvaOutgoingArgSpaceVar)
        {
            assert(FPbased == false);
        }
        else
#endif
        {
#if DOUBLE_ALIGN
            assert(FPbased == (isFramePointerUsed() || (genDoubleAlign() && varDsc->lvIsParam && !varDsc->lvIsRegArg)));
#else
#ifdef _TARGET_X86_
            assert(FPbased == isFramePointerUsed());
#endif
#endif
        }
#endif // DEBUG

        offset = varDsc->lvStkOffs;
    }
    else // Its a spill-temp
    {
        FPbased = isFramePointerUsed();
        if (lvaDoneFrameLayout == Compiler::FINAL_FRAME_LAYOUT)
        {
            TempDsc* tmpDsc = tmpFindNum(varNum);
#ifndef LEGACY_BACKEND
            // The temp might be in use, since this might be during code generation.
            if (tmpDsc == nullptr)
            {
                tmpDsc = tmpFindNum(varNum, Compiler::TEMP_USAGE_USED);
            }
#endif // !LEGACY_BACKEND
            assert(tmpDsc != nullptr);
            offset = tmpDsc->tdTempOffs();
            type   = tmpDsc->tdTempType();
        }
        else
        {
            // This value is an estimate until we calculate the
            // offset after the final frame layout
            // ---------------------------------------------------
            //   :                         :
            //   +-------------------------+ base --+
            //   | LR, ++N for ARM         |        |   frameBaseOffset (= N)
            //   +-------------------------+        |
            //   | R11, ++N for ARM        | <---FP |
            //   +-------------------------+      --+
            //   | compCalleeRegsPushed - N|        |   lclFrameOffset
            //   +-------------------------+      --+
            //   | lclVars                 |        |
            //   +-------------------------+        |
            //   | tmp[MAX_SPILL_TEMP]     |        |
            //   | tmp[1]                  |        |
            //   | tmp[0]                  |        |   compLclFrameSize
            //   +-------------------------+        |
            //   | outgoingArgSpaceSize    |        |
            //   +-------------------------+      --+
            //   |                         | <---SP
            //   :                         :
            // ---------------------------------------------------

            type          = compFloatingPointUsed ? TYP_FLOAT : TYP_INT;
            fConservative = true;
            if (!FPbased)
            {
                // Worst case stack based offset.
                CLANG_FORMAT_COMMENT_ANCHOR;
#if FEATURE_FIXED_OUT_ARGS
                int outGoingArgSpaceSize = lvaOutgoingArgSpaceSize;
#else
                int outGoingArgSpaceSize = 0;
#endif
                offset = outGoingArgSpaceSize + max(-varNum * TARGET_POINTER_SIZE, (int)lvaGetMaxSpillTempSize());
            }
            else
            {
                // Worst case FP based offset.
                CLANG_FORMAT_COMMENT_ANCHOR;

#ifdef _TARGET_ARM_
                offset = codeGen->genCallerSPtoInitialSPdelta() - codeGen->genCallerSPtoFPdelta();
#else
                offset                   = -(codeGen->genTotalFrameSize());
#endif
            }
        }
    }

#ifdef _TARGET_ARM_
    if (FPbased)
    {
        if (mustBeFPBased)
        {
            *pBaseReg = REG_FPBASE;
        }
        // Change the FP-based addressing to the SP-based addressing when possible because
        // it generates smaller code on ARM. See frame picture above for the math.
        else
        {
            // If it is the final frame layout phase, we don't have a choice, we should stick
            // to either FP based or SP based that we decided in the earlier phase. Because
            // we have already selected the instruction. Min-opts will have R10 enabled, so just
            // use that.

            int spOffset       = fConservative ? compLclFrameSize : offset + codeGen->genSPtoFPdelta();
            int actualOffset   = (spOffset + addrModeOffset);
            int ldrEncodeLimit = (varTypeIsFloating(type) ? 0x3FC : 0xFFC);
            // Use ldr sp imm encoding.
            if (lvaDoneFrameLayout == FINAL_FRAME_LAYOUT || opts.MinOpts() || (actualOffset <= ldrEncodeLimit))
            {
                offset    = spOffset;
                *pBaseReg = compLocallocUsed ? REG_SAVED_LOCALLOC_SP : REG_SPBASE;
            }
            // Use ldr +/-imm8 encoding.
            else if (offset >= -0x7C && offset <= ldrEncodeLimit)
            {
                *pBaseReg = REG_FPBASE;
            }
            // Use a single movw. prefer locals.
            else if (actualOffset <= 0xFFFC) // Fix 383910 ARM ILGEN
            {
                offset    = spOffset;
                *pBaseReg = compLocallocUsed ? REG_SAVED_LOCALLOC_SP : REG_SPBASE;
            }
            // Use movw, movt.
            else
            {
                *pBaseReg = REG_FPBASE;
            }
        }
    }
    else
    {
        *pBaseReg = REG_SPBASE;
    }
#else
    *pFPbased                            = FPbased;
#endif

    return offset;
}

inline bool Compiler::lvaIsParameter(unsigned varNum)
{
    LclVarDsc* varDsc;

    assert(varNum < lvaCount);
    varDsc = lvaTable + varNum;

    return varDsc->lvIsParam;
}

inline bool Compiler::lvaIsRegArgument(unsigned varNum)
{
    LclVarDsc* varDsc;

    assert(varNum < lvaCount);
    varDsc = lvaTable + varNum;

    return varDsc->lvIsRegArg;
}

inline BOOL Compiler::lvaIsOriginalThisArg(unsigned varNum)
{
    assert(varNum < lvaCount);

    BOOL isOriginalThisArg = (varNum == info.compThisArg) && (info.compIsStatic == false);

#ifdef DEBUG
    if (isOriginalThisArg)
    {
        LclVarDsc* varDsc = lvaTable + varNum;
        // Should never write to or take the address of the original 'this' arg
        CLANG_FORMAT_COMMENT_ANCHOR;

#ifndef JIT32_GCENCODER
        // With the general encoder/decoder, when the original 'this' arg is needed as a generics context param, we
        // copy to a new local, and mark the original as DoNotEnregister, to
        // ensure that it is stack-allocated.  It should not be the case that the original one can be modified -- it
        // should not be written to, or address-exposed.
        assert(!varDsc->lvHasILStoreOp &&
               (!varDsc->lvAddrExposed || ((info.compMethodInfo->options & CORINFO_GENERICS_CTXT_FROM_THIS) != 0)));
#else
        assert(!varDsc->lvHasILStoreOp && !varDsc->lvAddrExposed);
#endif
    }
#endif

    return isOriginalThisArg;
}

inline BOOL Compiler::lvaIsOriginalThisReadOnly()
{
    return lvaArg0Var == info.compThisArg;
}

/*****************************************************************************
 *
 *  The following is used to detect the cases where the same local variable#
 *  is used both as a long/double value and a 32-bit value and/or both as an
 *  integer/address and a float value.
 */

/* static */ inline unsigned Compiler::lvaTypeRefMask(var_types type)
{
    const static BYTE lvaTypeRefMasks[] = {
#define DEF_TP(tn, nm, jitType, verType, sz, sze, asze, st, al, tf, howUsed) howUsed,
#include "typelist.h"
#undef DEF_TP
    };

    assert((unsigned)type < sizeof(lvaTypeRefMasks));
    assert(lvaTypeRefMasks[type] != 0);

    return lvaTypeRefMasks[type];
}

/*****************************************************************************
 *
 *  The following is used to detect the cases where the same local variable#
 *  is used both as a long/double value and a 32-bit value and/or both as an
 *  integer/address and a float value.
 */

inline var_types Compiler::lvaGetActualType(unsigned lclNum)
{
    return genActualType(lvaGetRealType(lclNum));
}

inline var_types Compiler::lvaGetRealType(unsigned lclNum)
{
    return lvaTable[lclNum].TypeGet();
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          Importer                                         XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

inline unsigned Compiler::compMapILargNum(unsigned ILargNum)
{
    assert(ILargNum < info.compILargsCount || tiVerificationNeeded);

    // Note that this works because if compRetBuffArg/compTypeCtxtArg/lvVarargsHandleArg are not present
    // they will be BAD_VAR_NUM (MAX_UINT), which is larger than any variable number.
    if (ILargNum >= info.compRetBuffArg)
    {
        ILargNum++;
        assert(ILargNum < info.compLocalsCount || tiVerificationNeeded); // compLocals count already adjusted.
    }

    if (ILargNum >= (unsigned)info.compTypeCtxtArg)
    {
        ILargNum++;
        assert(ILargNum < info.compLocalsCount || tiVerificationNeeded); // compLocals count already adjusted.
    }

    if (ILargNum >= (unsigned)lvaVarargsHandleArg)
    {
        ILargNum++;
        assert(ILargNum < info.compLocalsCount || tiVerificationNeeded); // compLocals count already adjusted.
    }

    assert(ILargNum < info.compArgsCount || tiVerificationNeeded);
    return (ILargNum);
}

// For ARM varargs, all arguments go in integer registers, so swizzle the type
inline var_types Compiler::mangleVarArgsType(var_types type)
{
#ifdef _TARGET_ARMARCH_
    if (info.compIsVarArgs || opts.compUseSoftFP)
    {
        switch (type)
        {
            case TYP_FLOAT:
                return TYP_INT;
            case TYP_DOUBLE:
                return TYP_LONG;
            default:
                break;
        }
    }
#endif // _TARGET_ARMARCH_
    return type;
}

// For CORECLR there is no vararg on System V systems.
#if FEATURE_VARARG
inline regNumber Compiler::getCallArgIntRegister(regNumber floatReg)
{
#ifdef _TARGET_AMD64_
    switch (floatReg)
    {
        case REG_XMM0:
            return REG_RCX;
        case REG_XMM1:
            return REG_RDX;
        case REG_XMM2:
            return REG_R8;
        case REG_XMM3:
            return REG_R9;
        default:
            unreached();
    }
#else  // !_TARGET_AMD64_
    // How will float args be passed for RyuJIT/x86?
    NYI("getCallArgIntRegister for RyuJIT/x86");
    return REG_NA;
#endif // !_TARGET_AMD64_
}

inline regNumber Compiler::getCallArgFloatRegister(regNumber intReg)
{
#ifdef _TARGET_AMD64_
    switch (intReg)
    {
        case REG_RCX:
            return REG_XMM0;
        case REG_RDX:
            return REG_XMM1;
        case REG_R8:
            return REG_XMM2;
        case REG_R9:
            return REG_XMM3;
        default:
            unreached();
    }
#else  // !_TARGET_AMD64_
    // How will float args be passed for RyuJIT/x86?
    NYI("getCallArgFloatRegister for RyuJIT/x86");
    return REG_NA;
#endif // !_TARGET_AMD64_
}
#endif // FEATURE_VARARG

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                     Register Allocator                                    XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************/

inline bool rpCanAsgOperWithoutReg(GenTreePtr op, bool lclvar)
{
    var_types type;

    switch (op->OperGet())
    {
        case GT_CNS_LNG:
        case GT_CNS_INT:
            return true;
        case GT_LCL_VAR:
            type = genActualType(op->TypeGet());
            if (lclvar && ((type == TYP_INT) || (type == TYP_REF) || (type == TYP_BYREF)))
            {
                return true;
            }
            break;
        default:
            break;
    }

    return false;
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                       FlowGraph                                           XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

inline bool Compiler::compCanEncodePtrArgCntMax()
{
#ifdef JIT32_GCENCODER
    // DDB 204533:
    // The GC encoding for fully interruptible methods does not
    // support more than 1023 pushed arguments, so we have to
    // use a partially interruptible GC info/encoding.
    //
    return (fgPtrArgCntMax < MAX_PTRARG_OFS);
#else // JIT32_GCENCODER
    return true;
#endif
}

/*****************************************************************************
 *
 *  Call the given function pointer for all nodes in the tree. The 'visitor'
 *  fn should return one of the following values:
 *
 *  WALK_ABORT          stop walking and return immediately
 *  WALK_CONTINUE       continue walking
 *  WALK_SKIP_SUBTREES  don't walk any subtrees of the node just visited
 *
 *  computeStack - true if we want to make stack visible to callback function
 */

inline Compiler::fgWalkResult Compiler::fgWalkTreePre(
    GenTreePtr* pTree, fgWalkPreFn* visitor, void* callBackData, bool lclVarsOnly, bool computeStack)

{
    fgWalkData walkData;

    walkData.compiler      = this;
    walkData.wtprVisitorFn = visitor;
    walkData.pCallbackData = callBackData;
    walkData.parent        = nullptr;
    walkData.wtprLclsOnly  = lclVarsOnly;
#ifdef DEBUG
    walkData.printModified = false;
#endif

    fgWalkResult result;
    if (lclVarsOnly && computeStack)
    {
        GenericTreeWalker<true, true, false, true, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else if (lclVarsOnly)
    {
        GenericTreeWalker<false, true, false, true, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else if (computeStack)
    {
        GenericTreeWalker<true, true, false, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else
    {
        GenericTreeWalker<false, true, false, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }

#ifdef DEBUG
    if (verbose && walkData.printModified)
    {
        gtDispTree(*pTree);
    }
#endif

    return result;
}

/*****************************************************************************
 *
 *  Same as above, except the tree walk is performed in a depth-first fashion,
 *  The 'visitor' fn should return one of the following values:
 *
 *  WALK_ABORT          stop walking and return immediately
 *  WALK_CONTINUE       continue walking
 *
 *  computeStack - true if we want to make stack visible to callback function
 */

inline Compiler::fgWalkResult Compiler::fgWalkTreePost(GenTreePtr*   pTree,
                                                       fgWalkPostFn* visitor,
                                                       void*         callBackData,
                                                       bool          computeStack)
{
    fgWalkData walkData;

    walkData.compiler      = this;
    walkData.wtpoVisitorFn = visitor;
    walkData.pCallbackData = callBackData;
    walkData.parent        = nullptr;

    fgWalkResult result;
    if (computeStack)
    {
        GenericTreeWalker<true, false, true, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else
    {
        GenericTreeWalker<false, false, true, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }

    assert(result == WALK_CONTINUE || result == WALK_ABORT);

    return result;
}

/*****************************************************************************
 *
 *  Call the given function pointer for all nodes in the tree. The 'visitor'
 *  fn should return one of the following values:
 *
 *  WALK_ABORT          stop walking and return immediately
 *  WALK_CONTINUE       continue walking
 *  WALK_SKIP_SUBTREES  don't walk any subtrees of the node just visited
 */

inline Compiler::fgWalkResult Compiler::fgWalkTree(GenTreePtr*  pTree,
                                                   fgWalkPreFn* preVisitor,
                                                   fgWalkPreFn* postVisitor,
                                                   void*        callBackData)

{
    fgWalkData walkData;

    walkData.compiler      = this;
    walkData.wtprVisitorFn = preVisitor;
    walkData.wtpoVisitorFn = postVisitor;
    walkData.pCallbackData = callBackData;
    walkData.parent        = nullptr;
    walkData.wtprLclsOnly  = false;
#ifdef DEBUG
    walkData.printModified = false;
#endif

    fgWalkResult result;

    assert(preVisitor || postVisitor);

    if (preVisitor && postVisitor)
    {
        GenericTreeWalker<true, true, true, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else if (preVisitor)
    {
        GenericTreeWalker<true, true, false, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }
    else
    {
        GenericTreeWalker<true, false, true, false, true> walker(&walkData);
        result = walker.WalkTree(pTree, nullptr);
    }

#ifdef DEBUG
    if (verbose && walkData.printModified)
    {
        gtDispTree(*pTree);
    }
#endif

    return result;
}

/*****************************************************************************
 *
 * Has this block been added to throw an inlined exception
 * Returns true if the block was added to throw one of:
 *    range-check exception
 *    argument exception (used by feature SIMD)
 *    argument range-check exception (used by feature SIMD)
 *    divide by zero exception  (Not used on X86/X64)
 *    null reference exception (Not currently used)
 *    overflow exception
 */

inline bool Compiler::fgIsThrowHlpBlk(BasicBlock* block)
{
    if (!fgIsCodeAdded())
    {
        return false;
    }

    if (!(block->bbFlags & BBF_INTERNAL) || block->bbJumpKind != BBJ_THROW)
    {
        return false;
    }

    GenTree* call = block->lastNode();

#ifdef DEBUG
    if (block->IsLIR())
    {
        LIR::Range& blockRange = LIR::AsRange(block);
        for (LIR::Range::ReverseIterator node = blockRange.rbegin(), end = blockRange.rend(); node != end; ++node)
        {
            if (node->OperGet() == GT_CALL)
            {
                assert(*node == call);
                assert(node == blockRange.rbegin());
                break;
            }
        }
    }
#endif

    if (!call || (call->gtOper != GT_CALL))
    {
        return false;
    }

    if (!((call->gtCall.gtCallMethHnd == eeFindHelper(CORINFO_HELP_RNGCHKFAIL)) ||
          (call->gtCall.gtCallMethHnd == eeFindHelper(CORINFO_HELP_THROWDIVZERO)) ||
          (call->gtCall.gtCallMethHnd == eeFindHelper(CORINFO_HELP_THROWNULLREF)) ||
          (call->gtCall.gtCallMethHnd == eeFindHelper(CORINFO_HELP_OVERFLOW))))
    {
        return false;
    }

    // We can get to this point for blocks that we didn't create as throw helper blocks
    // under stress, with crazy flow graph optimizations. So, walk the fgAddCodeList
    // for the final determination.

    for (AddCodeDsc* add = fgAddCodeList; add; add = add->acdNext)
    {
        if (block == add->acdDstBlk)
        {
            return add->acdKind == SCK_RNGCHK_FAIL || add->acdKind == SCK_DIV_BY_ZERO || add->acdKind == SCK_OVERFLOW ||
                   add->acdKind == SCK_ARG_EXCPN || add->acdKind == SCK_ARG_RNG_EXCPN;
        }
    }

    // We couldn't find it in the fgAddCodeList
    return false;
}

/*****************************************************************************
 *
 *  Return the stackLevel of the inserted block that throws exception
 *  (by calling the EE helper).
 */

inline unsigned Compiler::fgThrowHlpBlkStkLevel(BasicBlock* block)
{
    for (AddCodeDsc* add = fgAddCodeList; add; add = add->acdNext)
    {
        if (block == add->acdDstBlk)
        {
            // Compute assert cond separately as assert macro cannot have conditional compilation directives.
            bool cond =
                (add->acdKind == SCK_RNGCHK_FAIL || add->acdKind == SCK_DIV_BY_ZERO || add->acdKind == SCK_OVERFLOW ||
                 add->acdKind == SCK_ARG_EXCPN || add->acdKind == SCK_ARG_RNG_EXCPN);
            assert(cond);

            // TODO: bbTgtStkDepth is DEBUG-only.
            // Should we use it regularly and avoid this search.
            assert(block->bbTgtStkDepth == add->acdStkLvl);
            return add->acdStkLvl;
        }
    }

    noway_assert(!"fgThrowHlpBlkStkLevel should only be called if fgIsThrowHlpBlk() is true, but we can't find the "
                  "block in the fgAddCodeList list");

    /* We couldn't find the basic block: it must not have been a throw helper block */

    return 0;
}

/*
    Small inline function to change a given block to a throw block.

*/
inline void Compiler::fgConvertBBToThrowBB(BasicBlock* block)
{
    // If we're converting a BBJ_CALLFINALLY block to a BBJ_THROW block,
    // then mark the subsequent BBJ_ALWAYS block as unreferenced.
    if (block->isBBCallAlwaysPair())
    {
        BasicBlock* leaveBlk = block->bbNext;
        noway_assert(leaveBlk->bbJumpKind == BBJ_ALWAYS);

        leaveBlk->bbFlags &= ~BBF_DONT_REMOVE;
        leaveBlk->bbRefs  = 0;
        leaveBlk->bbPreds = nullptr;

#if FEATURE_EH_FUNCLETS && defined(_TARGET_ARM_)
        // This function (fgConvertBBToThrowBB) can be called before the predecessor lists are created (e.g., in
        // fgMorph). The fgClearFinallyTargetBit() function to update the BBF_FINALLY_TARGET bit depends on these
        // predecessor lists. If there are no predecessor lists, we immediately clear all BBF_FINALLY_TARGET bits
        // (to allow subsequent dead code elimination to delete such blocks without asserts), and set a flag to
        // recompute them later, before they are required.
        if (fgComputePredsDone)
        {
            fgClearFinallyTargetBit(leaveBlk->bbJumpDest);
        }
        else
        {
            fgClearAllFinallyTargetBits();
            fgNeedToAddFinallyTargetBits = true;
        }
#endif // FEATURE_EH_FUNCLETS && defined(_TARGET_ARM_)
    }

    block->bbJumpKind = BBJ_THROW;
    block->bbSetRunRarely(); // any block with a throw is rare
}

/*****************************************************************************
 *
 *  Return true if we've added any new basic blocks.
 */

inline bool Compiler::fgIsCodeAdded()
{
    return fgAddCodeModf;
}

/*****************************************************************************
  Is the offset too big?
*/
inline bool Compiler::fgIsBigOffset(size_t offset)
{
    return (offset > compMaxUncheckedOffsetForNullObject);
}

/***********************************************************************************
*
*  Returns true if back-end will do other than integer division which currently occurs only
*  if "divisor" is a positive integer constant and a power of 2 other than 1 and INT_MIN
*/

inline bool Compiler::fgIsSignedDivOptimizable(GenTreePtr divisor)
{
    if (!opts.MinOpts() && divisor->IsCnsIntOrI())
    {
        ssize_t ival = divisor->gtIntConCommon.IconValue();

        /* Is the divisor a power of 2 (excluding INT_MIN) ?.
           The intent of the third condition below is to exclude INT_MIN on a 64-bit platform
           and during codegen we need to encode ival-1 within 32 bits.  If ival were INT_MIN
           then ival-1 would cause underflow.

           Note that we could put #ifdef around the third check so that it is applied only on
           64-bit platforms but the below is a more generic way to express it as it is a no-op
           on 32-bit platforms.
         */
        return (ival > 0 && genMaxOneBit(ival) && ((ssize_t)(int)ival == ival));
    }

    return false;
}

/************************************************************************************
*
*  Returns true if back-end will do other than integer division which currently occurs
* if "divisor" is an unsigned integer constant and a power of 2 other than 1 and zero.
*/

inline bool Compiler::fgIsUnsignedDivOptimizable(GenTreePtr divisor)
{
    if (!opts.MinOpts() && divisor->IsCnsIntOrI())
    {
        size_t ival = divisor->gtIntCon.gtIconVal;

        /* Is the divisor a power of 2 ? */
        return ival && genMaxOneBit(ival);
    }

    return false;
}

/*****************************************************************************
*
*  Returns true if back-end will do other than integer division which currently occurs
*  if "divisor" is a positive integer constant and a power of 2 other than zero
*/

inline bool Compiler::fgIsSignedModOptimizable(GenTreePtr divisor)
{
    if (!opts.MinOpts() && divisor->IsCnsIntOrI())
    {
        size_t ival = divisor->gtIntCon.gtIconVal;

        /* Is the divisor a power of 2  ? */
        return ssize_t(ival) > 0 && genMaxOneBit(ival);
    }

    return false;
}

/*****************************************************************************
*
*  Returns true if back-end will do other than integer division which currently occurs
*  if "divisor" is a positive integer constant and a power of 2 other than zero
*/

inline bool Compiler::fgIsUnsignedModOptimizable(GenTreePtr divisor)
{
    if (!opts.MinOpts() && divisor->IsCnsIntOrI())
    {
        size_t ival = divisor->gtIntCon.gtIconVal;

        /* Is the divisor a power of 2  ? */
        return ival != 0 && ival == (unsigned)genFindLowestBit(ival);
    }

    return false;
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          TempsInfo                                        XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************/

/* static */ inline unsigned Compiler::tmpSlot(unsigned size)
{
    noway_assert(size >= sizeof(int));
    noway_assert(size <= TEMP_MAX_SIZE);
    assert((size % sizeof(int)) == 0);

    assert(size < UINT32_MAX);
    return size / sizeof(int) - 1;
}

/*****************************************************************************
 *
 *  Finish allocating temps - should be called each time after a pass is made
 *  over a function body.
 */

inline void Compiler::tmpEnd()
{
#ifdef DEBUG
    if (verbose && (tmpCount > 0))
    {
        printf("%d tmps used\n", tmpCount);
    }
#endif // DEBUG
}

/*****************************************************************************
 *
 *  Shuts down the temp-tracking code. Should be called once per function
 *  compiled.
 */

inline void Compiler::tmpDone()
{
#ifdef DEBUG
    unsigned count;
    TempDsc* temp;

    assert(tmpAllFree());
    for (temp = tmpListBeg(), count = temp ? 1 : 0; temp; temp = tmpListNxt(temp), count += temp ? 1 : 0)
    {
        assert(temp->tdLegalOffset());
    }

    // Make sure that all the temps were released
    assert(count == tmpCount);
    assert(tmpGetCount == 0);
#endif // DEBUG
}

#ifdef DEBUG
inline bool Compiler::shouldUseVerboseTrees()
{
    return (JitConfig.JitDumpVerboseTrees() == 1);
}

inline bool Compiler::shouldUseVerboseSsa()
{
    return (JitConfig.JitDumpVerboseSsa() == 1);
}

//------------------------------------------------------------------------
// shouldDumpASCIITrees: Should we use only ASCII characters for tree dumps?
//
// Notes:
//    This is set to default to 1 in clrConfigValues.h

inline bool Compiler::shouldDumpASCIITrees()
{
    return (JitConfig.JitDumpASCII() == 1);
}

/*****************************************************************************
 *  Should we enable JitStress mode?
 *   0:   No stress
 *   !=2: Vary stress. Performance will be slightly/moderately degraded
 *   2:   Check-all stress. Performance will be REALLY horrible
 */

inline DWORD getJitStressLevel()
{
    return JitConfig.JitStress();
}

/*****************************************************************************
 *  Should we do the strict check for non-virtual call to the virtual method?
 */

inline DWORD StrictCheckForNonVirtualCallToVirtualMethod()
{
    return JitConfig.JitStrictCheckForNonVirtualCallToVirtualMethod() == 1;
}

#endif // DEBUG

/*****************************************************************************/
/* Map a register argument number ("RegArgNum") to a register number ("RegNum").
 * A RegArgNum is in this range:
 *      [0, MAX_REG_ARG)        -- for integer registers
 *      [0, MAX_FLOAT_REG_ARG)  -- for floating point registers
 * Note that RegArgNum's are overlapping for integer and floating-point registers,
 * while RegNum's are not (for ARM anyway, though for x86, it might be different).
 * If we have a fixed return buffer register and are given it's index
 * we return the fixed return buffer register
 */

inline regNumber genMapIntRegArgNumToRegNum(unsigned argNum)
{
    if (hasFixedRetBuffReg() && (argNum == theFixedRetBuffArgNum()))
    {
        return theFixedRetBuffReg();
    }

    assert(argNum < ArrLen(intArgRegs));

    return intArgRegs[argNum];
}

inline regNumber genMapFloatRegArgNumToRegNum(unsigned argNum)
{
#ifndef _TARGET_X86_
    assert(argNum < ArrLen(fltArgRegs));

    return fltArgRegs[argNum];
#else
    assert(!"no x86 float arg regs\n");
    return REG_NA;
#endif
}

__forceinline regNumber genMapRegArgNumToRegNum(unsigned argNum, var_types type)
{
    if (varTypeIsFloating(type))
    {
        return genMapFloatRegArgNumToRegNum(argNum);
    }
    else
    {
        return genMapIntRegArgNumToRegNum(argNum);
    }
}

/*****************************************************************************/
/* Map a register argument number ("RegArgNum") to a register mask of the associated register.
 * Note that for floating-pointer registers, only the low register for a register pair
 * (for a double on ARM) is returned.
 */

inline regMaskTP genMapIntRegArgNumToRegMask(unsigned argNum)
{
    assert(argNum < ArrLen(intArgMasks));

    return intArgMasks[argNum];
}

inline regMaskTP genMapFloatRegArgNumToRegMask(unsigned argNum)
{
#ifndef _TARGET_X86_
    assert(argNum < ArrLen(fltArgMasks));

    return fltArgMasks[argNum];
#else
    assert(!"no x86 float arg regs\n");
    return RBM_NONE;
#endif
}

__forceinline regMaskTP genMapArgNumToRegMask(unsigned argNum, var_types type)
{
    regMaskTP result;
    if (varTypeIsFloating(type))
    {
        result = genMapFloatRegArgNumToRegMask(argNum);
#ifdef _TARGET_ARM_
        if (type == TYP_DOUBLE)
        {
            assert((result & RBM_DBL_REGS) != 0);
            result |= (result << 1);
        }
#endif
    }
    else
    {
        result = genMapIntRegArgNumToRegMask(argNum);
    }
    return result;
}

/*****************************************************************************/
/* Map a register number ("RegNum") to a register argument number ("RegArgNum")
 * If we have a fixed return buffer register we return theFixedRetBuffArgNum
 */

inline unsigned genMapIntRegNumToRegArgNum(regNumber regNum)
{
    assert(genRegMask(regNum) & fullIntArgRegMask());

    switch (regNum)
    {
        case REG_ARG_0:
            return 0;
#if MAX_REG_ARG >= 2
        case REG_ARG_1:
            return 1;
#if MAX_REG_ARG >= 3
        case REG_ARG_2:
            return 2;
#if MAX_REG_ARG >= 4
        case REG_ARG_3:
            return 3;
#if MAX_REG_ARG >= 5
        case REG_ARG_4:
            return 4;
#if MAX_REG_ARG >= 6
        case REG_ARG_5:
            return 5;
#if MAX_REG_ARG >= 7
        case REG_ARG_6:
            return 6;
#if MAX_REG_ARG >= 8
        case REG_ARG_7:
            return 7;
#endif
#endif
#endif
#endif
#endif
#endif
#endif
        default:
            // Check for the Arm64 fixed return buffer argument register
            if (hasFixedRetBuffReg() && (regNum == theFixedRetBuffReg()))
            {
                return theFixedRetBuffArgNum();
            }
            else
            {
                assert(!"invalid register arg register");
                return BAD_VAR_NUM;
            }
    }
}

inline unsigned genMapFloatRegNumToRegArgNum(regNumber regNum)
{
    assert(genRegMask(regNum) & RBM_FLTARG_REGS);

#ifdef _TARGET_ARM_
    return regNum - REG_F0;
#elif defined(_TARGET_ARM64_)
    return regNum - REG_V0;
#elif defined(UNIX_AMD64_ABI)
    return regNum - REG_FLTARG_0;
#else

#if MAX_FLOAT_REG_ARG >= 1
    switch (regNum)
    {
        case REG_FLTARG_0:
            return 0;
#if MAX_REG_ARG >= 2
        case REG_FLTARG_1:
            return 1;
#if MAX_REG_ARG >= 3
        case REG_FLTARG_2:
            return 2;
#if MAX_REG_ARG >= 4
        case REG_FLTARG_3:
            return 3;
#if MAX_REG_ARG >= 5
        case REG_FLTARG_4:
            return 4;
#endif
#endif
#endif
#endif
        default:
            assert(!"invalid register arg register");
            return BAD_VAR_NUM;
    }
#else
    assert(!"flt reg args not allowed");
    return BAD_VAR_NUM;
#endif
#endif // !arm
}

inline unsigned genMapRegNumToRegArgNum(regNumber regNum, var_types type)
{
    if (varTypeIsFloating(type))
    {
        return genMapFloatRegNumToRegArgNum(regNum);
    }
    else
    {
        return genMapIntRegNumToRegArgNum(regNum);
    }
}

/*****************************************************************************/
/* Return a register mask with the first 'numRegs' argument registers set.
 */

inline regMaskTP genIntAllRegArgMask(unsigned numRegs)
{
    assert(numRegs <= MAX_REG_ARG);

    regMaskTP result = RBM_NONE;
    for (unsigned i = 0; i < numRegs; i++)
    {
        result |= intArgMasks[i];
    }
    return result;
}

#if !FEATURE_STACK_FP_X87

inline regMaskTP genFltAllRegArgMask(unsigned numRegs)
{
    assert(numRegs <= MAX_FLOAT_REG_ARG);

    regMaskTP result = RBM_NONE;
    for (unsigned i = 0; i < numRegs; i++)
    {
        result |= fltArgMasks[i];
    }
    return result;
}

#endif // !FEATURE_STACK_FP_X87

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          Liveness                                         XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

/*****************************************************************************
 *
 *  Update the current set of live variables based on the life set recorded
 *  in the given expression tree node.
 */

template <bool ForCodeGen>
inline void Compiler::compUpdateLife(GenTreePtr tree)
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

template <bool ForCodeGen>
inline void Compiler::compUpdateLife(VARSET_VALARG_TP newLife)
{
    if (!VarSetOps::Equal(this, compCurLife, newLife))
    {
        compChangeLife<ForCodeGen>(newLife DEBUGARG(nullptr));
    }
#ifdef DEBUG
    else
    {
        if (verbose)
        {
            printf("Liveness not changing: %s ", VarSetOps::ToString(this, compCurLife));
            dumpConvertedVarSet(this, compCurLife);
            printf("\n");
        }
    }
#endif // DEBUG
}

/*****************************************************************************
 *
 *  We stash cookies in basic blocks for the code emitter; this call retrieves
 *  the cookie associated with the given basic block.
 */

inline void* emitCodeGetCookie(BasicBlock* block)
{
    assert(block);
    return block->bbEmitCookie;
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          Optimizer                                        XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#if LOCAL_ASSERTION_PROP

/*****************************************************************************
 *
 *  The following resets the value assignment table
 *  used only during local assertion prop
 */

inline void Compiler::optAssertionReset(AssertionIndex limit)
{
    PREFAST_ASSUME(optAssertionCount <= optMaxAssertionCount);

    while (optAssertionCount > limit)
    {
        AssertionIndex index        = optAssertionCount;
        AssertionDsc*  curAssertion = optGetAssertion(index);
        optAssertionCount--;
        unsigned lclNum = curAssertion->op1.lcl.lclNum;
        assert(lclNum < lvaTableCnt);
        BitVecOps::RemoveElemD(apTraits, GetAssertionDep(lclNum), index - 1);

        //
        // Find the Copy assertions
        //
        if ((curAssertion->assertionKind == OAK_EQUAL) && (curAssertion->op1.kind == O1K_LCLVAR) &&
            (curAssertion->op2.kind == O2K_LCLVAR_COPY))
        {
            //
            //  op2.lcl.lclNum no longer depends upon this assertion
            //
            lclNum = curAssertion->op2.lcl.lclNum;
            BitVecOps::RemoveElemD(apTraits, GetAssertionDep(lclNum), index - 1);
        }
    }
    while (optAssertionCount < limit)
    {
        AssertionIndex index        = ++optAssertionCount;
        AssertionDsc*  curAssertion = optGetAssertion(index);
        unsigned       lclNum       = curAssertion->op1.lcl.lclNum;
        BitVecOps::AddElemD(apTraits, GetAssertionDep(lclNum), index - 1);

        //
        // Check for Copy assertions
        //
        if ((curAssertion->assertionKind == OAK_EQUAL) && (curAssertion->op1.kind == O1K_LCLVAR) &&
            (curAssertion->op2.kind == O2K_LCLVAR_COPY))
        {
            //
            //  op2.lcl.lclNum now depends upon this assertion
            //
            lclNum = curAssertion->op2.lcl.lclNum;
            BitVecOps::AddElemD(apTraits, GetAssertionDep(lclNum), index - 1);
        }
    }
}

/*****************************************************************************
 *
 *  The following removes the i-th entry in the value assignment table
 *  used only during local assertion prop
 */

inline void Compiler::optAssertionRemove(AssertionIndex index)
{
    assert(index > 0);
    assert(index <= optAssertionCount);
    PREFAST_ASSUME(optAssertionCount <= optMaxAssertionCount);

    AssertionDsc* curAssertion = optGetAssertion(index);

    //  Two cases to consider if (index == optAssertionCount) then the last
    //  entry in the table is to be removed and that happens automatically when
    //  optAssertionCount is decremented and we can just clear the optAssertionDep bits
    //  The other case is when index < optAssertionCount and here we overwrite the
    //  index-th entry in the table with the data found at the end of the table
    //  Since we are reordering the rable the optAssertionDep bits need to be recreated
    //  using optAssertionReset(0) and optAssertionReset(newAssertionCount) will
    //  correctly update the optAssertionDep bits
    //
    if (index == optAssertionCount)
    {
        unsigned lclNum = curAssertion->op1.lcl.lclNum;
        BitVecOps::RemoveElemD(apTraits, GetAssertionDep(lclNum), index - 1);

        //
        // Check for Copy assertions
        //
        if ((curAssertion->assertionKind == OAK_EQUAL) && (curAssertion->op1.kind == O1K_LCLVAR) &&
            (curAssertion->op2.kind == O2K_LCLVAR_COPY))
        {
            //
            //  op2.lcl.lclNum no longer depends upon this assertion
            //
            lclNum = curAssertion->op2.lcl.lclNum;
            BitVecOps::RemoveElemD(apTraits, GetAssertionDep(lclNum), index - 1);
        }

        optAssertionCount--;
    }
    else
    {
        AssertionDsc*  lastAssertion     = optGetAssertion(optAssertionCount);
        AssertionIndex newAssertionCount = optAssertionCount - 1;

        optAssertionReset(0); // This make optAssertionCount equal 0

        memcpy(curAssertion,  // the entry to be removed
               lastAssertion, // last entry in the table
               sizeof(AssertionDsc));

        optAssertionReset(newAssertionCount);
    }
}
#endif // LOCAL_ASSERTION_PROP

inline void Compiler::LoopDsc::AddModifiedField(Compiler* comp, CORINFO_FIELD_HANDLE fldHnd)
{
    if (lpFieldsModified == nullptr)
    {
        lpFieldsModified =
            new (comp->getAllocatorLoopHoist()) Compiler::LoopDsc::FieldHandleSet(comp->getAllocatorLoopHoist());
    }
    lpFieldsModified->Set(fldHnd, true);
}

inline void Compiler::LoopDsc::AddModifiedElemType(Compiler* comp, CORINFO_CLASS_HANDLE structHnd)
{
    if (lpArrayElemTypesModified == nullptr)
    {
        lpArrayElemTypesModified =
            new (comp->getAllocatorLoopHoist()) Compiler::LoopDsc::ClassHandleSet(comp->getAllocatorLoopHoist());
    }
    lpArrayElemTypesModified->Set(structHnd, true);
}

inline void Compiler::LoopDsc::VERIFY_lpIterTree()
{
#ifdef DEBUG
    assert(lpFlags & LPFLG_ITER);

    // iterTree should be "lcl <op>= const"

    assert(lpIterTree);

    assert(lpIterTree->OperIsAssignment());

    if (lpIterTree->OperGet() == GT_ASG)
    {
        GenTreePtr lhs = lpIterTree->gtOp.gtOp1;
        GenTreePtr rhs = lpIterTree->gtOp.gtOp2;
        assert(lhs->OperGet() == GT_LCL_VAR);

        switch (rhs->gtOper)
        {
            case GT_ADD:
            case GT_SUB:
            case GT_MUL:
            case GT_RSH:
            case GT_LSH:
                break;
            default:
                assert(!"Unknown operator for loop increment");
        }
        assert(rhs->gtOp.gtOp1->OperGet() == GT_LCL_VAR);
        assert(rhs->gtOp.gtOp1->AsLclVarCommon()->GetLclNum() == lhs->AsLclVarCommon()->GetLclNum());
        assert(rhs->gtOp.gtOp2->OperGet() == GT_CNS_INT);
    }
    else
    {
        assert(lpIterTree->gtOp.gtOp1->OperGet() == GT_LCL_VAR);
        assert(lpIterTree->gtOp.gtOp2->OperGet() == GT_CNS_INT);
    }
#endif
}

//-----------------------------------------------------------------------------

inline unsigned Compiler::LoopDsc::lpIterVar()
{
    VERIFY_lpIterTree();
    return lpIterTree->gtOp.gtOp1->gtLclVarCommon.gtLclNum;
}

//-----------------------------------------------------------------------------

inline int Compiler::LoopDsc::lpIterConst()
{
    VERIFY_lpIterTree();
    if (lpIterTree->OperGet() == GT_ASG)
    {
        GenTreePtr rhs = lpIterTree->gtOp.gtOp2;
        return (int)rhs->gtOp.gtOp2->gtIntCon.gtIconVal;
    }
    else
    {
        return (int)lpIterTree->gtOp.gtOp2->gtIntCon.gtIconVal;
    }
}

//-----------------------------------------------------------------------------

inline genTreeOps Compiler::LoopDsc::lpIterOper()
{
    VERIFY_lpIterTree();
    if (lpIterTree->OperGet() == GT_ASG)
    {
        GenTreePtr rhs = lpIterTree->gtOp.gtOp2;
        return rhs->OperGet();
    }
    else
    {
        return lpIterTree->OperGet();
    }
}

inline var_types Compiler::LoopDsc::lpIterOperType()
{
    VERIFY_lpIterTree();

    var_types type = lpIterTree->TypeGet();
    assert(genActualType(type) == TYP_INT);

    if ((lpIterTree->gtFlags & GTF_UNSIGNED) && type == TYP_INT)
    {
        type = TYP_UINT;
    }

    return type;
}

inline void Compiler::LoopDsc::VERIFY_lpTestTree()
{
#ifdef DEBUG
    assert(lpFlags & LPFLG_ITER);
    assert(lpTestTree);

    genTreeOps oper = lpTestTree->OperGet();
    assert(GenTree::OperIsCompare(oper));

    GenTreePtr iterator = nullptr;
    GenTreePtr limit    = nullptr;
    if ((lpTestTree->gtOp.gtOp2->gtOper == GT_LCL_VAR) && (lpTestTree->gtOp.gtOp2->gtFlags & GTF_VAR_ITERATOR) != 0)
    {
        iterator = lpTestTree->gtOp.gtOp2;
        limit    = lpTestTree->gtOp.gtOp1;
    }
    else if ((lpTestTree->gtOp.gtOp1->gtOper == GT_LCL_VAR) &&
             (lpTestTree->gtOp.gtOp1->gtFlags & GTF_VAR_ITERATOR) != 0)
    {
        iterator = lpTestTree->gtOp.gtOp1;
        limit    = lpTestTree->gtOp.gtOp2;
    }
    else
    {
        // one of the nodes has to be the iterator
        assert(false);
    }

    if (lpFlags & LPFLG_CONST_LIMIT)
    {
        assert(limit->OperIsConst());
    }
    if (lpFlags & LPFLG_VAR_LIMIT)
    {
        assert(limit->OperGet() == GT_LCL_VAR);
    }
    if (lpFlags & LPFLG_ARRLEN_LIMIT)
    {
        assert(limit->OperGet() == GT_ARR_LENGTH);
    }
#endif
}

//-----------------------------------------------------------------------------

inline bool Compiler::LoopDsc::lpIsReversed()
{
    VERIFY_lpTestTree();
    return ((lpTestTree->gtOp.gtOp2->gtOper == GT_LCL_VAR) &&
            (lpTestTree->gtOp.gtOp2->gtFlags & GTF_VAR_ITERATOR) != 0);
}

//-----------------------------------------------------------------------------

inline genTreeOps Compiler::LoopDsc::lpTestOper()
{
    VERIFY_lpTestTree();
    genTreeOps op = lpTestTree->OperGet();
    return lpIsReversed() ? GenTree::SwapRelop(op) : op;
}

//-----------------------------------------------------------------------------

inline GenTreePtr Compiler::LoopDsc::lpIterator()
{
    VERIFY_lpTestTree();

    return lpIsReversed() ? lpTestTree->gtOp.gtOp2 : lpTestTree->gtOp.gtOp1;
}

//-----------------------------------------------------------------------------

inline GenTreePtr Compiler::LoopDsc::lpLimit()
{
    VERIFY_lpTestTree();

    return lpIsReversed() ? lpTestTree->gtOp.gtOp1 : lpTestTree->gtOp.gtOp2;
}

//-----------------------------------------------------------------------------

inline int Compiler::LoopDsc::lpConstLimit()
{
    VERIFY_lpTestTree();
    assert(lpFlags & LPFLG_CONST_LIMIT);

    GenTreePtr limit = lpLimit();
    assert(limit->OperIsConst());
    return (int)limit->gtIntCon.gtIconVal;
}

//-----------------------------------------------------------------------------

inline unsigned Compiler::LoopDsc::lpVarLimit()
{
    VERIFY_lpTestTree();
    assert(lpFlags & LPFLG_VAR_LIMIT);

    GenTreePtr limit = lpLimit();
    assert(limit->OperGet() == GT_LCL_VAR);
    return limit->gtLclVarCommon.gtLclNum;
}

//-----------------------------------------------------------------------------

inline bool Compiler::LoopDsc::lpArrLenLimit(Compiler* comp, ArrIndex* index)
{
    VERIFY_lpTestTree();
    assert(lpFlags & LPFLG_ARRLEN_LIMIT);

    GenTreePtr limit = lpLimit();
    assert(limit->OperGet() == GT_ARR_LENGTH);

    // Check if we have a.length or a[i][j].length
    if (limit->gtArrLen.ArrRef()->gtOper == GT_LCL_VAR)
    {
        index->arrLcl = limit->gtArrLen.ArrRef()->gtLclVarCommon.gtLclNum;
        index->rank   = 0;
        return true;
    }
    // We have a[i].length, extract a[i] pattern.
    else if (limit->gtArrLen.ArrRef()->gtOper == GT_COMMA)
    {
        return comp->optReconstructArrIndex(limit->gtArrLen.ArrRef(), index, BAD_VAR_NUM);
    }
    return false;
}

/*****************************************************************************
 *  Is "var" assigned in the loop "lnum" ?
 */

inline bool Compiler::optIsVarAssgLoop(unsigned lnum, unsigned var)
{
    assert(lnum < optLoopCount);
    if (var < lclMAX_ALLSET_TRACKED)
    {
        ALLVARSET_TP vs(AllVarSetOps::MakeSingleton(this, var));
        return optIsSetAssgLoop(lnum, vs) != 0;
    }
    else
    {
        return optIsVarAssigned(optLoopTable[lnum].lpHead->bbNext, optLoopTable[lnum].lpBottom, nullptr, var);
    }
}

/*****************************************************************************
 * If the tree is a tracked local variable, return its LclVarDsc ptr.
 */

inline LclVarDsc* Compiler::optIsTrackedLocal(GenTreePtr tree)
{
    LclVarDsc* varDsc;
    unsigned   lclNum;

    if (tree->gtOper != GT_LCL_VAR)
    {
        return nullptr;
    }

    lclNum = tree->gtLclVarCommon.gtLclNum;

    assert(lclNum < lvaCount);
    varDsc = lvaTable + lclNum;

    /* if variable not tracked, return NULL */
    if (!varDsc->lvTracked)
    {
        return nullptr;
    }

    return varDsc;
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                                                                           XX
XX                Optimization activation rules                              XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

// are we compiling for fast code, or are we compiling for blended code and
// inside a loop?
// We return true for BLENDED_CODE if the Block executes more than BB_LOOP_WEIGHT/2
inline bool Compiler::optFastCodeOrBlendedLoop(BasicBlock::weight_t bbWeight)
{
    return (compCodeOpt() == FAST_CODE) ||
           ((compCodeOpt() == BLENDED_CODE) && (bbWeight > (BB_LOOP_WEIGHT / 2 * BB_UNITY_WEIGHT)));
}

// are we running on a Intel Pentium 4?
inline bool Compiler::optPentium4(void)
{
    return (info.genCPU == CPU_X86_PENTIUM_4);
}

// should we use add/sub instead of inc/dec? (faster on P4, but increases size)
inline bool Compiler::optAvoidIncDec(BasicBlock::weight_t bbWeight)
{
    return optPentium4() && optFastCodeOrBlendedLoop(bbWeight);
}

// should we try to replace integer multiplication with lea/add/shift sequences?
inline bool Compiler::optAvoidIntMult(void)
{
    return (compCodeOpt() != SMALL_CODE);
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          EEInterface                                      XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

extern var_types JITtype2varType(CorInfoType type);

#include "ee_il_dll.hpp"

inline CORINFO_METHOD_HANDLE Compiler::eeFindHelper(unsigned helper)
{
    assert(helper < CORINFO_HELP_COUNT);

    /* Helpers are marked by the fact that they are odd numbers
     * force this to be an odd number (will shift it back to extract) */

    return ((CORINFO_METHOD_HANDLE)(size_t)((helper << 2) + 1));
}

inline CorInfoHelpFunc Compiler::eeGetHelperNum(CORINFO_METHOD_HANDLE method)
{
    // Helpers are marked by the fact that they are odd numbers
    if (!(((size_t)method) & 1))
    {
        return (CORINFO_HELP_UNDEF);
    }
    return ((CorInfoHelpFunc)(((size_t)method) >> 2));
}

inline Compiler::fgWalkResult Compiler::CountSharedStaticHelper(GenTreePtr* pTree, fgWalkData* data)
{
    if (Compiler::IsSharedStaticHelper(*pTree))
    {
        int* pCount = (int*)data->pCallbackData;
        (*pCount)++;
    }

    return WALK_CONTINUE;
}

//  TODO-Cleanup: Replace calls to IsSharedStaticHelper with new HelperCallProperties
//

inline bool Compiler::IsSharedStaticHelper(GenTreePtr tree)
{
    if (tree->gtOper != GT_CALL || tree->gtCall.gtCallType != CT_HELPER)
    {
        return false;
    }

    CorInfoHelpFunc helper = eeGetHelperNum(tree->gtCall.gtCallMethHnd);

    bool result1 =
        // More helpers being added to IsSharedStaticHelper (that have similar behaviors but are not true
        // ShareStaticHelperts)
        helper == CORINFO_HELP_STRCNS || helper == CORINFO_HELP_BOX ||

        // helpers being added to IsSharedStaticHelper
        helper == CORINFO_HELP_GETSTATICFIELDADDR_CONTEXT || helper == CORINFO_HELP_GETSTATICFIELDADDR_TLS ||
        helper == CORINFO_HELP_GETGENERICS_GCSTATIC_BASE || helper == CORINFO_HELP_GETGENERICS_NONGCSTATIC_BASE ||
        helper == CORINFO_HELP_GETGENERICS_GCTHREADSTATIC_BASE ||
        helper == CORINFO_HELP_GETGENERICS_NONGCTHREADSTATIC_BASE ||

        helper == CORINFO_HELP_GETSHARED_GCSTATIC_BASE || helper == CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE ||
        helper == CORINFO_HELP_GETSHARED_GCSTATIC_BASE_NOCTOR ||
        helper == CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_NOCTOR ||
        helper == CORINFO_HELP_GETSHARED_GCSTATIC_BASE_DYNAMICCLASS ||
        helper == CORINFO_HELP_GETSHARED_NONGCSTATIC_BASE_DYNAMICCLASS ||
        helper == CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE ||
        helper == CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE ||
        helper == CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_NOCTOR ||
        helper == CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_NOCTOR ||
        helper == CORINFO_HELP_GETSHARED_GCTHREADSTATIC_BASE_DYNAMICCLASS ||
        helper == CORINFO_HELP_GETSHARED_NONGCTHREADSTATIC_BASE_DYNAMICCLASS ||
#ifdef FEATURE_READYTORUN_COMPILER
        helper == CORINFO_HELP_READYTORUN_STATIC_BASE || helper == CORINFO_HELP_READYTORUN_GENERIC_STATIC_BASE ||
#endif
        helper == CORINFO_HELP_CLASSINIT_SHARED_DYNAMICCLASS;
#if 0
    // See above TODO-Cleanup
    bool result2 = s_helperCallProperties.IsPure(helper) && s_helperCallProperties.NonNullReturn(helper);
    assert (result1 == result2);
#endif
    return result1;
}

inline bool Compiler::IsTreeAlwaysHoistable(GenTreePtr tree)
{
    if (IsSharedStaticHelper(tree))
    {
        return (GTF_CALL_HOISTABLE & tree->gtFlags) ? true : false;
    }
    else
    {
        return false;
    }
}

inline bool Compiler::IsGcSafePoint(GenTreePtr tree)
{
    if (tree->IsCall())
    {
        GenTreeCall* call = tree->AsCall();
        if (!call->IsFastTailCall())
        {
            if (call->gtCallType == CT_INDIRECT)
            {
                return true;
            }
            else if (call->gtCallType == CT_USER_FUNC)
            {
                if ((call->gtCallMoreFlags & GTF_CALL_M_NOGCCHECK) == 0)
                {
                    return true;
                }
            }
            // otherwise we have a CT_HELPER
        }
    }

    return false;
}

//
// Note that we want to have two special FIELD_HANDLES that will both
// be considered non-Data Offset handles
//
// The special values that we use are FLD_GLOBAL_DS and FLD_GLOBAL_FS
//

inline bool jitStaticFldIsGlobAddr(CORINFO_FIELD_HANDLE fldHnd)
{
    return (fldHnd == FLD_GLOBAL_DS || fldHnd == FLD_GLOBAL_FS);
}

#if defined(DEBUG) || defined(FEATURE_JIT_METHOD_PERF) || defined(FEATURE_SIMD) || defined(FEATURE_TRACELOGGING)

inline bool Compiler::eeIsNativeMethod(CORINFO_METHOD_HANDLE method)
{
    return ((((size_t)method) & 0x2) == 0x2);
}

inline CORINFO_METHOD_HANDLE Compiler::eeGetMethodHandleForNative(CORINFO_METHOD_HANDLE method)
{
    assert((((size_t)method) & 0x3) == 0x2);
    return (CORINFO_METHOD_HANDLE)(((size_t)method) & ~0x3);
}
#endif

inline CORINFO_METHOD_HANDLE Compiler::eeMarkNativeTarget(CORINFO_METHOD_HANDLE method)
{
    assert((((size_t)method) & 0x3) == 0);
    if (method == nullptr)
    {
        return method;
    }
    else
    {
        return (CORINFO_METHOD_HANDLE)(((size_t)method) | 0x2);
    }
}

/*
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XX                          Compiler                                         XX
XX                      Inline functions                                     XX
XX                                                                           XX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
*/

#ifndef DEBUG
inline bool Compiler::compStressCompile(compStressArea stressArea, unsigned weightPercentage)
{
    return false;
}
#endif

inline ArenaAllocator* Compiler::compGetAllocator()
{
    return compAllocator;
}

/*****************************************************************************
 *
 *  Allocate memory from the no-release allocator. All such memory will be
 *  freed up simulataneously at the end of the procedure
 */

#ifndef DEBUG

inline void* Compiler::compGetMem(size_t sz, CompMemKind cmk)
{
    assert(sz);

#if MEASURE_MEM_ALLOC
    genMemStats.AddAlloc(sz, cmk);
#endif

    return compAllocator->allocateMemory(sz);
}

#endif

// Wrapper for Compiler::compGetMem that can be forward-declared for use in template
// types which Compiler depends on but which need to allocate heap memory.
inline void* compGetMem(Compiler* comp, size_t sz)
{
    return comp->compGetMem(sz);
}

/*****************************************************************************
 *
 * A common memory allocation for arrays of structures involves the
 * multiplication of the number of elements with the size of each element.
 * If this computation overflows, then the memory allocation might succeed,
 * but not allocate sufficient memory for all the elements.  This can cause
 * us to overwrite the allocation, and AV or worse, corrupt memory.
 *
 * This method checks for overflow, and succeeds only when it detects
 * that there's no overflow.  It should be cheap, because when inlined with
 * a constant elemSize, the division should be done in compile time, and so
 * at run time we simply have a check of numElem against some number (this
 * is why we __forceinline).
 */

#define MAX_MEMORY_PER_ALLOCATION (512 * 1024 * 1024)

__forceinline void* Compiler::compGetMemArray(size_t numElem, size_t elemSize, CompMemKind cmk)
{
    if (numElem > (MAX_MEMORY_PER_ALLOCATION / elemSize))
    {
        NOMEM();
    }

    return compGetMem(numElem * elemSize, cmk);
}

__forceinline void* Compiler::compGetMemArrayA(size_t numElem, size_t elemSize, CompMemKind cmk)
{
    if (numElem > (MAX_MEMORY_PER_ALLOCATION / elemSize))
    {
        NOMEM();
    }

    return compGetMemA(numElem * elemSize, cmk);
}

/******************************************************************************
 *
 *  Roundup the allocated size so that if this memory block is aligned,
 *  then the next block allocated too will be aligned.
 *  The JIT will always try to keep all the blocks aligned.
 */

inline void* Compiler::compGetMemA(size_t sz, CompMemKind cmk)
{
    assert(sz);

    size_t allocSz = roundUp(sz, sizeof(size_t));

#if MEASURE_MEM_ALLOC
    genMemStats.AddAlloc(allocSz, cmk);
#endif

    void* ptr = compAllocator->allocateMemory(allocSz);

    // Verify that the current block is aligned. Only then will the next
    // block allocated be on an aligned boundary.
    assert((size_t(ptr) & (sizeof(size_t) - 1)) == 0);

    return ptr;
}

inline void Compiler::compFreeMem(void* ptr)
{
}

#define compFreeMem(ptr) compFreeMem((void*)ptr)

inline bool Compiler::compIsProfilerHookNeeded()
{
#ifdef PROFILING_SUPPORTED
    return compProfilerHookNeeded
           // IL stubs are excluded by VM and we need to do the same even running
           // under a complus env hook to generate profiler hooks
           || (opts.compJitELTHookEnabled && !opts.jitFlags->IsSet(JitFlags::JIT_FLAG_IL_STUB));
#else  // !PROFILING_SUPPORTED
    return false;
#endif // !PROFILING_SUPPORTED
}

/*****************************************************************************
 *
 *  Check for the special case where the object is the constant 0.
 *  As we can't even fold the tree (null+fldOffs), we are left with
 *  op1 and op2 both being a constant. This causes lots of problems.
 *  We simply grab a temp and assign 0 to it and use it in place of the NULL.
 */

inline GenTreePtr Compiler::impCheckForNullPointer(GenTreePtr obj)
{
    /* If it is not a GC type, we will be able to fold it.
       So don't need to do anything */

    if (!varTypeIsGC(obj->TypeGet()))
    {
        return obj;
    }

    if (obj->gtOper == GT_CNS_INT)
    {
        assert(obj->gtType == TYP_REF || obj->gtType == TYP_BYREF);
        assert(obj->gtIntCon.gtIconVal == 0);

        unsigned tmp = lvaGrabTemp(true DEBUGARG("CheckForNullPointer"));

        // We don't need to spill while appending as we are only assigning
        // NULL to a freshly-grabbed temp.

        impAssignTempGen(tmp, obj, (unsigned)CHECK_SPILL_NONE);

        obj = gtNewLclvNode(tmp, obj->gtType);
    }

    return obj;
}

/*****************************************************************************
 *
 *  Check for the special case where the object is the methods original 'this' pointer.
 *  Note that, the original 'this' pointer is always local var 0 for non-static method,
 *  even if we might have created the copy of 'this' pointer in lvaArg0Var.
 */

inline bool Compiler::impIsThis(GenTreePtr obj)
{
    if (compIsForInlining())
    {
        return impInlineInfo->InlinerCompiler->impIsThis(obj);
    }
    else
    {
        return ((obj != nullptr) && (obj->gtOper == GT_LCL_VAR) && lvaIsOriginalThisArg(obj->gtLclVarCommon.gtLclNum));
    }
}

/*****************************************************************************
 *
 *  Check to see if the delegate is created using "LDFTN <TOK>" or not.
 */

inline bool Compiler::impIsLDFTN_TOKEN(const BYTE* delegateCreateStart, const BYTE* newobjCodeAddr)
{
    assert(newobjCodeAddr[0] == CEE_NEWOBJ);
    return (newobjCodeAddr - delegateCreateStart == 6 && // LDFTN <TOK> takes 6 bytes
            delegateCreateStart[0] == CEE_PREFIX1 && delegateCreateStart[1] == (CEE_LDFTN & 0xFF));
}

/*****************************************************************************
 *
 *  Check to see if the delegate is created using "DUP LDVIRTFTN <TOK>" or not.
 */

inline bool Compiler::impIsDUP_LDVIRTFTN_TOKEN(const BYTE* delegateCreateStart, const BYTE* newobjCodeAddr)
{
    assert(newobjCodeAddr[0] == CEE_NEWOBJ);
    return (newobjCodeAddr - delegateCreateStart == 7 && // DUP LDVIRTFTN <TOK> takes 6 bytes
            delegateCreateStart[0] == CEE_DUP && delegateCreateStart[1] == CEE_PREFIX1 &&
            delegateCreateStart[2] == (CEE_LDVIRTFTN & 0xFF));
}
/*****************************************************************************
 *
 * Returns true if the compiler instance is created for import only (verification).
 */

inline bool Compiler::compIsForImportOnly()
{
    return opts.jitFlags->IsSet(JitFlags::JIT_FLAG_IMPORT_ONLY);
}

/*****************************************************************************
 *
 *  Returns true if the compiler instance is created for inlining.
 */

inline bool Compiler::compIsForInlining()
{
    return (impInlineInfo != nullptr);
}

/*****************************************************************************
 *
 *  Check the inline result field in the compiler to see if inlining failed or not.
 */

inline bool Compiler::compDonotInline()
{
    if (compIsForInlining())
    {
        assert(compInlineResult != nullptr);
        return compInlineResult->IsFailure();
    }
    else
    {
        return false;
    }
}

inline bool Compiler::impIsPrimitive(CorInfoType jitType)
{
    return ((CORINFO_TYPE_BOOL <= jitType && jitType <= CORINFO_TYPE_DOUBLE) || jitType == CORINFO_TYPE_PTR);
}

/*****************************************************************************
 *
 *  Get the promotion type of a struct local.
 */

inline Compiler::lvaPromotionType Compiler::lvaGetPromotionType(const LclVarDsc* varDsc)
{
    assert(!varDsc->lvPromoted || varTypeIsPromotable(varDsc) || varDsc->lvUnusedStruct);

    if (!varDsc->lvPromoted)
    {
        // no struct promotion for this LclVar
        return PROMOTION_TYPE_NONE;
    }
    if (varDsc->lvDoNotEnregister)
    {
        // The struct is not enregistered
        return PROMOTION_TYPE_DEPENDENT;
    }
    if (!varDsc->lvIsParam)
    {
        // The struct is a register candidate
        return PROMOTION_TYPE_INDEPENDENT;
    }

    // Has struct promotion for arguments been disabled using COMPlus_JitNoStructPromotion=2
    if (fgNoStructParamPromotion)
    {
        // The struct parameter is not enregistered
        return PROMOTION_TYPE_DEPENDENT;
    }

    // We have a parameter that could be enregistered
    CLANG_FORMAT_COMMENT_ANCHOR;

#if defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_)

    // The struct parameter is a register candidate
    return PROMOTION_TYPE_INDEPENDENT;
#else
    // The struct parameter is not enregistered
    return PROMOTION_TYPE_DEPENDENT;
#endif
}

/*****************************************************************************
 *
 *  Get the promotion type of a struct local.
 */

inline Compiler::lvaPromotionType Compiler::lvaGetPromotionType(unsigned varNum)
{
    assert(varNum < lvaCount);
    return lvaGetPromotionType(&lvaTable[varNum]);
}

/*****************************************************************************
 *
 *  Given a field local, get the promotion type of its parent struct local.
 */

inline Compiler::lvaPromotionType Compiler::lvaGetParentPromotionType(const LclVarDsc* varDsc)
{
    assert(varDsc->lvIsStructField);
    assert(varDsc->lvParentLcl < lvaCount);

    lvaPromotionType promotionType = lvaGetPromotionType(varDsc->lvParentLcl);
    assert(promotionType != PROMOTION_TYPE_NONE);
    return promotionType;
}

/*****************************************************************************
 *
 *  Given a field local, get the promotion type of its parent struct local.
 */

inline Compiler::lvaPromotionType Compiler::lvaGetParentPromotionType(unsigned varNum)
{
    assert(varNum < lvaCount);
    return lvaGetParentPromotionType(&lvaTable[varNum]);
}

/*****************************************************************************
 *
 *  Return true if the local is a field local of a promoted struct of type PROMOTION_TYPE_DEPENDENT.
 *  Return false otherwise.
 */

inline bool Compiler::lvaIsFieldOfDependentlyPromotedStruct(const LclVarDsc* varDsc)
{
    if (!varDsc->lvIsStructField)
    {
        return false;
    }

    lvaPromotionType promotionType = lvaGetParentPromotionType(varDsc);
    if (promotionType == PROMOTION_TYPE_DEPENDENT)
    {
        return true;
    }

    assert(promotionType == PROMOTION_TYPE_INDEPENDENT);
    return false;
}

//------------------------------------------------------------------------
// lvaIsGCTracked: Determine whether this var should be reported
//    as tracked for GC purposes.
//
// Arguments:
//    varDsc - the LclVarDsc for the var in question.
//
// Return Value:
//    Returns true if the variable should be reported as tracked in the GC info.
//
// Notes:
//    This never returns true for struct variables, even if they are tracked.
//    This is because struct variables are never tracked as a whole for GC purposes.
//    It is up to the caller to ensure that the fields of struct variables are
//    correctly tracked.
//    On Amd64, we never GC-track fields of dependently promoted structs, even
//    though they may be tracked for optimization purposes.
//    It seems that on x86 and arm, we simply don't track these
//    fields, though I have not verified that.  I attempted to make these GC-tracked,
//    but there was too much logic that depends on these being untracked, so changing
//    this would require non-trivial effort.

inline bool Compiler::lvaIsGCTracked(const LclVarDsc* varDsc)
{
    if (varDsc->lvTracked && (varDsc->lvType == TYP_REF || varDsc->lvType == TYP_BYREF))
    {
        // Stack parameters are always untracked w.r.t. GC reportings
        const bool isStackParam = varDsc->lvIsParam && !varDsc->lvIsRegArg;
#ifdef _TARGET_AMD64_
        return !isStackParam && !lvaIsFieldOfDependentlyPromotedStruct(varDsc);
#else  // !_TARGET_AMD64_
        return !isStackParam;
#endif // !_TARGET_AMD64_
    }
    else
    {
        return false;
    }
}

inline void Compiler::EndPhase(Phases phase)
{
#if defined(FEATURE_JIT_METHOD_PERF)
    if (pCompJitTimer != nullptr)
    {
        pCompJitTimer->EndPhase(this, phase);
    }
#endif
#if DUMP_FLOWGRAPHS
    fgDumpFlowGraph(phase);
#endif // DUMP_FLOWGRAPHS
    previousCompletedPhase = phase;
#ifdef DEBUG
    if (dumpIR)
    {
        if ((*dumpIRPhase == L'*') || (wcscmp(dumpIRPhase, PhaseShortNames[phase]) == 0))
        {
            printf("\n");
            printf("IR after %s (switch: %ls)\n", PhaseEnums[phase], PhaseShortNames[phase]);
            printf("\n");

            if (dumpIRLinear)
            {
                dFuncIR();
            }
            else if (dumpIRTrees)
            {
                dTrees();
            }

            // If we are just dumping a single method and we have a request to exit
            // after dumping, do so now.

            if (dumpIRExit && ((*dumpIRPhase != L'*') || (phase == PHASE_EMIT_GCEH)))
            {
                exit(0);
            }
        }
    }
#endif
}

/*****************************************************************************/
#if MEASURE_CLRAPI_CALLS

inline void Compiler::CLRApiCallEnter(unsigned apix)
{
    if (pCompJitTimer != nullptr)
    {
        pCompJitTimer->CLRApiCallEnter(apix);
    }
}
inline void Compiler::CLRApiCallLeave(unsigned apix)
{
    if (pCompJitTimer != nullptr)
    {
        pCompJitTimer->CLRApiCallLeave(apix);
    }
}

inline void Compiler::CLR_API_Enter(API_ICorJitInfo_Names ename)
{
    CLRApiCallEnter(ename);
}

inline void Compiler::CLR_API_Leave(API_ICorJitInfo_Names ename)
{
    CLRApiCallLeave(ename);
}

#endif // MEASURE_CLRAPI_CALLS

//------------------------------------------------------------------------------
// fgStructTempNeedsExplicitZeroInit : Check whether temp struct needs
//                                     explicit zero initialization in this basic block.
//
// Arguments:
//    varDsc -           struct local var description
//    block  -           basic block to check
//
// Returns:
//             true if the struct temp needs explicit zero-initialization in this basic block;
//             false otherwise
//
// Notes:
//     Structs with GC pointer fields are fully zero-initialized in the prolog if compInitMem is true.
//     Therefore, we don't need to insert zero-initialization if this block is not in a loop.

bool Compiler::fgStructTempNeedsExplicitZeroInit(LclVarDsc* varDsc, BasicBlock* block)
{
    bool containsGCPtr = (varDsc->lvStructGcCount > 0);
    return (!containsGCPtr || !info.compInitMem || ((block->bbFlags & BBF_BACKWARD_JUMP) != 0));
}

/*****************************************************************************/
bool Compiler::fgExcludeFromSsa(unsigned lclNum)
{
    if (opts.MinOpts())
    {
        return true; // If we're doing MinOpts, no SSA vars.
    }

    LclVarDsc* varDsc = &lvaTable[lclNum];

    if (varDsc->lvAddrExposed)
    {
        return true; // We exclude address-exposed variables.
    }
    if (!varDsc->lvTracked)
    {
        return true; // SSA is only done for tracked variables
    }
    // lvPromoted structs are never tracked...
    assert(!varDsc->lvPromoted);

    if (varDsc->lvOverlappingFields)
    {
        return true; // Don't use SSA on structs that have overlapping fields
    }

    if (varDsc->lvIsStructField && (lvaGetParentPromotionType(lclNum) != PROMOTION_TYPE_INDEPENDENT))
    {
        // SSA must exclude struct fields that are not independent
        // - because we don't model the struct assignment properly when multiple fields can be assigned by one struct
        //   assignment.
        // - SSA doesn't allow a single node to contain multiple SSA definitions.
        // - and PROMOTION_TYPE_DEPENDEDNT fields  are never candidates for a register.
        //
        // Example mscorlib method: CompatibilitySwitches:IsCompatibilitySwitchSet
        //
        return true;
    }
    // otherwise this variable is *not* excluded for SSA
    return false;
}

/*****************************************************************************/
ValueNum Compiler::GetUseAsgDefVNOrTreeVN(GenTreePtr op)
{
    if (op->gtFlags & GTF_VAR_USEASG)
    {
        unsigned lclNum = op->AsLclVarCommon()->GetLclNum();
        unsigned ssaNum = GetSsaNumForLocalVarDef(op);
        return lvaTable[lclNum].GetPerSsaData(ssaNum)->m_vnPair.GetConservative();
    }
    else
    {
        return op->gtVNPair.GetConservative();
    }
}

/*****************************************************************************/
unsigned Compiler::GetSsaNumForLocalVarDef(GenTreePtr lcl)
{
    // Address-taken variables don't have SSA numbers.
    if (fgExcludeFromSsa(lcl->AsLclVarCommon()->gtLclNum))
    {
        return SsaConfig::RESERVED_SSA_NUM;
    }

    if (lcl->gtFlags & GTF_VAR_USEASG)
    {
        // It's an "lcl op= rhs" assignment.  "lcl" is both used and defined here;
        // we've chosen in this case to annotate "lcl" with the SSA number (and VN) of the use,
        // and to store the SSA number of the def in a side table.
        unsigned ssaNum;
        // In case of a remorph (fgMorph) in CSE/AssertionProp after SSA phase, there
        // wouldn't be an entry for the USEASG portion of the indir addr, return
        // reserved.
        if (!GetOpAsgnVarDefSsaNums()->Lookup(lcl, &ssaNum))
        {
            return SsaConfig::RESERVED_SSA_NUM;
        }
        return ssaNum;
    }
    else
    {
        return lcl->AsLclVarCommon()->gtSsaNum;
    }
}

template <typename TVisitor>
void GenTree::VisitOperands(TVisitor visitor)
{
    switch (OperGet())
    {
        // Leaf nodes
        case GT_LCL_VAR:
        case GT_LCL_FLD:
        case GT_LCL_VAR_ADDR:
        case GT_LCL_FLD_ADDR:
        case GT_CATCH_ARG:
        case GT_LABEL:
        case GT_FTN_ADDR:
        case GT_RET_EXPR:
        case GT_CNS_INT:
        case GT_CNS_LNG:
        case GT_CNS_DBL:
        case GT_CNS_STR:
        case GT_MEMORYBARRIER:
        case GT_JMP:
        case GT_JCC:
        case GT_SETCC:
        case GT_NO_OP:
        case GT_START_NONGC:
        case GT_PROF_HOOK:
#if !FEATURE_EH_FUNCLETS
        case GT_END_LFIN:
#endif // !FEATURE_EH_FUNCLETS
        case GT_PHI_ARG:
#ifndef LEGACY_BACKEND
        case GT_JMPTABLE:
#endif // LEGACY_BACKEND
        case GT_REG_VAR:
        case GT_CLS_VAR:
        case GT_CLS_VAR_ADDR:
        case GT_ARGPLACE:
        case GT_PHYSREG:
        case GT_EMITNOP:
        case GT_PINVOKE_PROLOG:
        case GT_PINVOKE_EPILOG:
        case GT_IL_OFFSET:
            return;

        // Unary operators with an optional operand
        case GT_NOP:
        case GT_RETURN:
        case GT_RETFILT:
            if (this->AsUnOp()->gtOp1 == nullptr)
            {
                return;
            }
            __fallthrough;

        // Standard unary operators
        case GT_STORE_LCL_VAR:
        case GT_STORE_LCL_FLD:
        case GT_NOT:
        case GT_NEG:
        case GT_COPY:
        case GT_RELOAD:
        case GT_ARR_LENGTH:
        case GT_CAST:
        case GT_BITCAST:
        case GT_CKFINITE:
        case GT_LCLHEAP:
        case GT_ADDR:
        case GT_IND:
        case GT_OBJ:
        case GT_BLK:
        case GT_BOX:
        case GT_ALLOCOBJ:
        case GT_INIT_VAL:
        case GT_JTRUE:
        case GT_SWITCH:
        case GT_NULLCHECK:
        case GT_PUTARG_REG:
        case GT_PUTARG_STK:
#if defined(_TARGET_ARM_) && !defined(LEGACY_BACKEND)
        case GT_PUTARG_SPLIT:
#endif
        case GT_RETURNTRAP:
            visitor(this->AsUnOp()->gtOp1);
            return;

        // Variadic nodes
        case GT_PHI:
            assert(this->AsUnOp()->gtOp1 != nullptr);
            this->AsUnOp()->gtOp1->VisitListOperands(visitor);
            return;

        case GT_FIELD_LIST:
            VisitListOperands(visitor);
            return;

#ifdef FEATURE_SIMD
        case GT_SIMD:
            if (this->AsSIMD()->gtSIMDIntrinsicID == SIMDIntrinsicInitN)
            {
                assert(this->AsSIMD()->gtOp1 != nullptr);
                this->AsSIMD()->gtOp1->VisitListOperands(visitor);
            }
            else
            {
                VisitBinOpOperands<TVisitor>(visitor);
            }
            return;
#endif // FEATURE_SIMD

        // Special nodes
        case GT_CMPXCHG:
        {
            GenTreeCmpXchg* const cmpXchg = this->AsCmpXchg();
            if (visitor(cmpXchg->gtOpLocation) == VisitResult::Abort)
            {
                return;
            }
            if (visitor(cmpXchg->gtOpValue) == VisitResult::Abort)
            {
                return;
            }
            visitor(cmpXchg->gtOpComparand);
            return;
        }

        case GT_ARR_BOUNDS_CHECK:
#ifdef FEATURE_SIMD
        case GT_SIMD_CHK:
#endif // FEATURE_SIMD
        {
            GenTreeBoundsChk* const boundsChk = this->AsBoundsChk();
            if (visitor(boundsChk->gtIndex) == VisitResult::Abort)
            {
                return;
            }
            visitor(boundsChk->gtArrLen);
            return;
        }

        case GT_FIELD:
            if (this->AsField()->gtFldObj != nullptr)
            {
                visitor(this->AsField()->gtFldObj);
            }
            return;

        case GT_STMT:
            if (this->AsStmt()->gtStmtExpr != nullptr)
            {
                visitor(this->AsStmt()->gtStmtExpr);
            }
            return;

        case GT_ARR_ELEM:
        {
            GenTreeArrElem* const arrElem = this->AsArrElem();
            if (visitor(arrElem->gtArrObj) == VisitResult::Abort)
            {
                return;
            }
            for (unsigned i = 0; i < arrElem->gtArrRank; i++)
            {
                if (visitor(arrElem->gtArrInds[i]) == VisitResult::Abort)
                {
                    return;
                }
            }
            return;
        }

        case GT_ARR_OFFSET:
        {
            GenTreeArrOffs* const arrOffs = this->AsArrOffs();
            if (visitor(arrOffs->gtOffset) == VisitResult::Abort)
            {
                return;
            }
            if (visitor(arrOffs->gtIndex) == VisitResult::Abort)
            {
                return;
            }
            visitor(arrOffs->gtArrObj);
            return;
        }

        case GT_DYN_BLK:
        {
            GenTreeDynBlk* const dynBlock = this->AsDynBlk();
            if (visitor(dynBlock->gtOp1) == VisitResult::Abort)
            {
                return;
            }
            visitor(dynBlock->gtDynamicSize);
            return;
        }

        case GT_STORE_DYN_BLK:
        {
            GenTreeDynBlk* const dynBlock = this->AsDynBlk();
            if (visitor(dynBlock->gtOp1) == VisitResult::Abort)
            {
                return;
            }
            if (visitor(dynBlock->gtOp2) == VisitResult::Abort)
            {
                return;
            }
            visitor(dynBlock->gtDynamicSize);
            return;
        }

        case GT_CALL:
        {
            GenTreeCall* const call = this->AsCall();
            if ((call->gtCallObjp != nullptr) && (visitor(call->gtCallObjp) == VisitResult::Abort))
            {
                return;
            }
            if ((call->gtCallArgs != nullptr) && (call->gtCallArgs->VisitListOperands(visitor) == VisitResult::Abort))
            {
                return;
            }
            if ((call->gtCallLateArgs != nullptr) &&
                (call->gtCallLateArgs->VisitListOperands(visitor)) == VisitResult::Abort)
            {
                return;
            }
            if (call->gtCallType == CT_INDIRECT)
            {
                if ((call->gtCallCookie != nullptr) && (visitor(call->gtCallCookie) == VisitResult::Abort))
                {
                    return;
                }
                if ((call->gtCallAddr != nullptr) && (visitor(call->gtCallAddr) == VisitResult::Abort))
                {
                    return;
                }
            }
            if ((call->gtControlExpr != nullptr))
            {
                visitor(call->gtControlExpr);
            }
            return;
        }

        // Binary nodes
        default:
            assert(this->OperIsBinary());
            VisitBinOpOperands<TVisitor>(visitor);
            return;
    }
}

template <typename TVisitor>
GenTree::VisitResult GenTree::VisitListOperands(TVisitor visitor)
{
    for (GenTreeArgList* node = this->AsArgList(); node != nullptr; node = node->Rest())
    {
        if (visitor(node->gtOp1) == VisitResult::Abort)
        {
            return VisitResult::Abort;
        }
    }

    return VisitResult::Continue;
}

template <typename TVisitor>
void GenTree::VisitBinOpOperands(TVisitor visitor)
{
    assert(this->OperIsBinary());

    GenTreeOp* const op = this->AsOp();

    GenTree* const op1 = op->gtOp1;
    if ((op1 != nullptr) && (visitor(op1) == VisitResult::Abort))
    {
        return;
    }

    GenTree* const op2 = op->gtOp2;
    if (op2 != nullptr)
    {
        visitor(op2);
    }
}

/*****************************************************************************
 *  operator new
 *
 *  Note that compGetMem is an arena allocator that returns memory that is
 *  not zero-initialized and can contain data from a prior allocation lifetime.
 */

inline void* __cdecl operator new(size_t sz, Compiler* context, CompMemKind cmk)
{
    return context->compGetMem(sz, cmk);
}

inline void* __cdecl operator new[](size_t sz, Compiler* context, CompMemKind cmk)
{
    return context->compGetMem(sz, cmk);
}

inline void* __cdecl operator new(size_t sz, void* p, const jitstd::placement_t& /* syntax_difference */)
{
    return p;
}

inline void* __cdecl operator new(size_t sz, IAllocator* alloc)
{
    return alloc->Alloc(sz);
}

inline void* __cdecl operator new[](size_t sz, IAllocator* alloc)
{
    return alloc->Alloc(sz);
}

/*****************************************************************************/

#ifdef DEBUG

inline void printRegMask(regMaskTP mask)
{
    printf(REG_MASK_ALL_FMT, mask);
}

inline char* regMaskToString(regMaskTP mask, Compiler* context)
{
    const size_t cchRegMask = 24;
    char*        regmask    = new (context, CMK_Unknown) char[cchRegMask];

    sprintf_s(regmask, cchRegMask, REG_MASK_ALL_FMT, mask);

    return regmask;
}

inline void printRegMaskInt(regMaskTP mask)
{
    printf(REG_MASK_INT_FMT, (mask & RBM_ALLINT));
}

inline char* regMaskIntToString(regMaskTP mask, Compiler* context)
{
    const size_t cchRegMask = 24;
    char*        regmask    = new (context, CMK_Unknown) char[cchRegMask];

    sprintf_s(regmask, cchRegMask, REG_MASK_INT_FMT, (mask & RBM_ALLINT));

    return regmask;
}

#endif // DEBUG

inline static bool StructHasOverlappingFields(DWORD attribs)
{
    return ((attribs & CORINFO_FLG_OVERLAPPING_FIELDS) != 0);
}

inline static bool StructHasCustomLayout(DWORD attribs)
{
    return ((attribs & CORINFO_FLG_CUSTOMLAYOUT) != 0);
}

/*****************************************************************************
 * This node should not be referenced by anyone now. Set its values to garbage
 * to catch extra references
 */

inline void DEBUG_DESTROY_NODE(GenTreePtr tree)
{
#ifdef DEBUG
    // printf("DEBUG_DESTROY_NODE for [0x%08x]\n", tree);

    // Save gtOper in case we want to find out what this node was
    tree->gtOperSave = tree->gtOper;

    tree->gtType = TYP_UNDEF;
    tree->gtFlags |= 0xFFFFFFFF & ~GTF_NODE_MASK;
    if (tree->OperIsSimple())
    {
        tree->gtOp.gtOp1 = tree->gtOp.gtOp2 = nullptr;
    }
    // Must do this last, because the "gtOp" check above will fail otherwise.
    // Don't call SetOper, because GT_COUNT is not a valid value
    tree->gtOper = GT_COUNT;
#endif
}

/*****************************************************************************/
#endif //_COMPILER_HPP_
/*****************************************************************************/
