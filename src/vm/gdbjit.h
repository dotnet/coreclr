// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//*****************************************************************************
// File: gdbjit.h
// 

//
// Header file for GDB JIT interface implemenation.
//
//*****************************************************************************


#ifndef __GDBJIT_H__
#define __GDBJIT_H__

#include <stdint.h>
#include "method.hpp"
#include "dbginterface.h"
#include "../inc/llvm/ELF.h"
#include "../inc/llvm/Dwarf.h"

#if defined(_TARGET_X86_) || defined(_TARGET_ARM_)
    typedef Elf32_Ehdr  Elf_Ehdr;
    typedef Elf32_Shdr  Elf_Shdr;
    typedef Elf32_Sym   Elf_Sym;
#define ADDRESS_SIZE 4    
#elif defined(_TARGET_AMD64_) || defined(_TARGET_ARM64_)
    typedef Elf64_Ehdr  Elf_Ehdr;
    typedef Elf64_Shdr  Elf_Shdr;
    typedef Elf64_Sym   Elf_Sym;
#define ADDRESS_SIZE 8    
#else
#error "Target is not supported"
#endif


static constexpr const int CorElementTypeToDWEncoding[] = 
{
/* ELEMENT_TYPE_END */          0,
/* ELEMENT_TYPE_VOID */         DW_ATE_address,
/* ELEMENT_TYPE_BOOLEAN */      DW_ATE_boolean,
/* ELEMENT_TYPE_CHAR */         DW_ATE_signed,
/* ELEMENT_TYPE_I1 */           DW_ATE_signed,
/* ELEMENT_TYPE_U1 */           DW_ATE_unsigned,
/* ELEMENT_TYPE_I2 */           DW_ATE_signed,
/* ELEMENT_TYPE_U2 */           DW_ATE_unsigned,
/* ELEMENT_TYPE_I4 */           DW_ATE_signed,
/* ELEMENT_TYPE_U4 */           DW_ATE_unsigned,
/* ELEMENT_TYPE_I8 */           DW_ATE_signed,
/* ELEMENT_TYPE_U8 */           DW_ATE_unsigned,
/* ELEMENT_TYPE_R4 */           DW_ATE_float,
/* ELEMENT_TYPE_R8 */           DW_ATE_float,
/* ELEMENT_TYPE_STRING */       DW_ATE_address,
/* ELEMENT_TYPE_PTR */          DW_ATE_address,
/* ELEMENT_TYPE_BYREF */        DW_ATE_address,
/* ELEMENT_TYPE_VALUETYPE */    DW_ATE_address,
/* ELEMENT_TYPE_CLASS */        DW_ATE_address,
/* ELEMENT_TYPE_VAR */          DW_ATE_address,
/* ELEMENT_TYPE_ARRAY */        DW_ATE_address,
/* ELEMENT_TYPE_GENERICINST */  DW_ATE_address,
/* ELEMENT_TYPE_TYPEDBYREF */   DW_ATE_address,
/* SKIP 17 */                   DW_ATE_address,
/* ELEMENT_TYPE_I */            DW_ATE_signed,
/* ELEMENT_TYPE_U */            DW_ATE_unsigned,
/* SKIP 1a */                   DW_ATE_address,
/* ELEMENT_TYPE_FNPTR */        DW_ATE_address,
/* ELEMENT_TYPE_OBJECT */       DW_ATE_address,
/* ELEMENT_TYPE_SZARRAY */      DW_ATE_address,
/* ELEMENT_TYPE_MVAR */         DW_ATE_address,
/* ELEMENT_TYPE_CMOD_REQD */    DW_ATE_address,
/* ELEMENT_TYPE_CMOD_OPT */     DW_ATE_address,
/* ELEMENT_TYPE_INTERNAL */     DW_ATE_address,
/* ELEMENT_TYPE_MAX */          DW_ATE_address,
};

struct __attribute__((packed)) DwarfCompUnit
{
    uint32_t m_length;
    uint16_t m_version;
    uint32_t m_abbrev_offset;
    uint8_t m_addr_size;
};

struct __attribute__((packed)) DwarfPubHeader
{
    uint32_t m_length;
    uint16_t m_version;
    uint32_t m_debug_info_off;
    uint32_t m_debug_info_len;
};

#define DW_LNS_MAX DW_LNS_set_isa

struct __attribute__((packed)) DwarfLineNumHeader
{
    uint32_t m_length;
    uint16_t m_version;
    uint32_t m_hdr_length;
    uint8_t m_min_instr_len;
    uint8_t m_def_is_stmt;
    int8_t m_line_base;
    uint8_t m_line_range;
    uint8_t m_opcode_base;
    uint8_t m_std_num_arg[DW_LNS_MAX];
};

struct SymbolsInfo
{
    int lineNumber, ilOffset, nativeOffset, fileIndex;
    char fileName[2*MAX_PATH_FNAME];
};

class DwarfDumpable
{
public:
    // writes all string literals this type needs to ptr
    virtual void DumpStrings(char* ptr, int& offset) = 0;

    virtual void DumpDebugInfo(char* ptr, int& offset) = 0;
};

class LocalsInfo 
{
public:
    int size;
    char** localsName;
    ULONG32 countVars;
    ICorDebugInfo::NativeVarInfo *pVars;
};

class TypeMember;

class TypeInfoBase : public DwarfDumpable
{
public:
    TypeInfoBase() 
        : typeHandle(),
          m_type_name(nullptr),
          m_type_name_offset(0),
          m_type_size(0),
          m_type_offset(0)
    {
    }

    virtual ~TypeInfoBase()
    {
        if (m_type_name != nullptr)
        {
            delete[] m_type_name;
        }
    }

    virtual void DumpStrings(char* ptr, int& offset) override;

    TypeHandle typeHandle;
    const char* m_type_name;
    int m_type_name_offset;
    ULONG m_type_size;
    int m_type_offset;
};

class BaseTypeInfo: public TypeInfoBase
{
public:
    BaseTypeInfo(int encoding)
        : TypeInfoBase(),
          m_type_encoding(encoding)
    {
    }

    void DumpDebugInfo(char* ptr, int& offset) override;

    int m_type_encoding;
};

class ClassTypeInfo: public TypeInfoBase
{
public:
    ClassTypeInfo(int num_members);
    ~ClassTypeInfo();

    void DumpStrings(char* ptr, int& offset) override;
    void DumpDebugInfo(char* ptr, int& offset) override;

    int m_num_members;
    TypeMember *members;
};

class TypeMember
{
public:
    TypeMember()
        : m_member_name(nullptr),
          m_member_name_offset(0),
          m_member_offset(0),
          m_member_type(nullptr)
    {
    }

    ~TypeMember()
    {
        if (m_member_name != nullptr)
        {
            delete m_member_name;
        }
    }
    char* m_member_name;
    int m_member_name_offset;
    int m_member_offset;
    TypeInfoBase *m_member_type;
};

struct ArgsDebugInfo
{
    const char* m_arg_name;
    int m_arg_name_offset;
    int m_arg_abbrev;
    int m_il_index;
    int m_native_offset;
    TypeInfoBase *m_arg_type;
};

struct LocalsDebugInfo
{
    char* m_var_name;
    int m_var_name_offset;
    int m_var_abbrev;
    int m_il_index;
    int m_native_offset;
    TypeInfoBase *m_var_type;
};

class NotifyGdb
{
public:
    static void MethodCompiled(MethodDesc* MethodDescPtr);
    static void MethodDropped(MethodDesc* MethodDescPtr);
    template <typename PARENT_TRAITS>
    class DeleteValuesOnDestructSHashTraits : public PARENT_TRAITS
    {
    public:
        static inline void OnDestructPerEntryCleanupAction(typename PARENT_TRAITS::element_t e)
        {
            delete e.Value();
        }
        static const bool s_DestructPerEntryCleanupAction = true;
    };

    typedef MapSHash<MethodTable*, TypeInfoBase*, DeleteValuesOnDestructSHashTraits<MapSHashTraits<MethodTable*,TypeInfoBase*>>> MT_TypeInfoMap;
    typedef MT_TypeInfoMap* PMT_TypeInfoMap;
private:

    struct MemBuf
    {
        NewArrayHolder<char> MemPtr;
        unsigned MemSize;
        MemBuf() : MemPtr(0), MemSize(0)
        {}
    };

    static bool BuildELFHeader(MemBuf& buf);
    static bool BuildSectionNameTable(MemBuf& buf);
    static bool BuildSectionTable(MemBuf& buf);
    static bool BuildSymbolTableSection(MemBuf& buf, PCODE addr, TADDR codeSize);
    static bool BuildStringTableSection(MemBuf& strTab);
    static bool BuildDebugStrings(MemBuf& buf,
                                  PMT_TypeInfoMap pTypeMap,
                                  NewArrayHolder<ArgsDebugInfo>& argsDebug,
                                  unsigned int argsDebugSize,
                                  NewArrayHolder<LocalsDebugInfo>& localsDebug,
                                  unsigned int localsDebugSize);
    static bool BuildDebugAbbrev(MemBuf& buf);
    static bool BuildDebugInfo(MemBuf& buf,
                               PMT_TypeInfoMap pTypeMap,
                               NewArrayHolder<ArgsDebugInfo>& argsDebug,
                               unsigned int argsDebugSize,
                               NewArrayHolder<LocalsDebugInfo>& localsDebug,
                               unsigned int localsDebugSize);
    static bool BuildDebugPub(MemBuf& buf, const char* name, uint32_t size, uint32_t dieOffset);
    static bool BuildLineTable(MemBuf& buf, PCODE startAddr, TADDR codeSize, SymbolsInfo* lines, unsigned nlines);
    static bool BuildFileTable(MemBuf& buf, SymbolsInfo* lines, unsigned nlines);
    static bool BuildLineProg(MemBuf& buf, PCODE startAddr, TADDR codeSize, SymbolsInfo* lines, unsigned nlines);
    static bool FitIntoSpecialOpcode(int8_t line_shift, uint8_t addr_shift);
    static void IssueSetAddress(char*& ptr, PCODE addr);
    static void IssueEndOfSequence(char*& ptr);
    static void IssueSimpleCommand(char*& ptr, uint8_t command);
    static void IssueParamCommand(char*& ptr, uint8_t command, char* param, int param_len);
    static void IssueSpecialCommand(char*& ptr, int8_t line_shift, uint8_t addr_shift);
    static void SplitPathname(const char* path, const char*& pathName, const char*& fileName);
    static bool CollectCalledMethods(CalledMethod* pCM);
    static int Leb128Encode(uint32_t num, char* buf, int size);
    static int Leb128Encode(int32_t num, char* buf, int size);
    static int GetFrameLocation(int nativeOffset, char* varLoc);
    static int GetArgsAndLocalsLen(NewArrayHolder<ArgsDebugInfo>& argsDebug,
                                   unsigned int argsDebugSize,
                                   NewArrayHolder<LocalsDebugInfo>& localsDebug,
                                   unsigned int localsDebugSize);
#ifdef _DEBUG
    static void DumpElf(const char* methodName, const MemBuf& buf);
#endif
};

#endif // #ifndef __GDBJIT_H__
