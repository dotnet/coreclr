// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//*****************************************************************************
// File: gdbjit.cpp
//

//
// NotifyGdb implementation.
//
//*****************************************************************************

#include "common.h"
#include "formattype.h"
#include "gdbjit.h"
#include "gdbjithelpers.h"

TypeInfoBase* GetTypeInfoFromTypeHandle(TypeHandle typeHandle, NotifyGdb::PMT_TypeInfoMap pTypeMap)
{
    TypeInfoBase *typeInfo = nullptr;

    PTR_MethodTable pMT = typeHandle.GetMethodTable();
    if (pTypeMap->Lookup(pMT, &typeInfo))
    {
        return typeInfo;
    }

    CorElementType corType = typeHandle.GetSignatureCorElementType();
    switch (corType)
    {
        case ELEMENT_TYPE_VOID:
        case ELEMENT_TYPE_BOOLEAN:
        case ELEMENT_TYPE_CHAR:
        case ELEMENT_TYPE_I1:
        case ELEMENT_TYPE_U1:
        case ELEMENT_TYPE_I2:
        case ELEMENT_TYPE_U2:
        case ELEMENT_TYPE_I4:
        case ELEMENT_TYPE_U4:
        case ELEMENT_TYPE_I8:
        case ELEMENT_TYPE_U8:
        case ELEMENT_TYPE_R4:
        case ELEMENT_TYPE_R8:
        case ELEMENT_TYPE_U:
        case ELEMENT_TYPE_I:
            typeInfo = new (nothrow) PrimitiveTypeInfo(CorElementTypeToDWEncoding[corType]);
            if (typeInfo == nullptr)
                return nullptr;

            typeInfo->typeHandle = typeHandle;
            typeInfo->m_type_size = CorTypeInfo::Size(corType);
            break;
        case ELEMENT_TYPE_CLASS:
        {
            ApproxFieldDescIterator fieldDescIterator(pMT, ApproxFieldDescIterator::INSTANCE_FIELDS);
            ULONG cFields = fieldDescIterator.Count();

            typeInfo = new (nothrow) ClassTypeInfo(cFields);

            if (typeInfo == nullptr)
                return nullptr;

            typeInfo->typeHandle = typeHandle;
            typeInfo->m_type_size = typeHandle.AsMethodTable()->GetClass()->GetSize();

            //
            // Now fill in the array
            //
            FieldDesc *pField;
            BOOL fReferenceType = !typeHandle.IsValueType();

            for (ULONG i = 0; i < cFields; i++)
            {
                pField = fieldDescIterator.Next();
                ClassTypeInfo *info = static_cast<ClassTypeInfo*>(typeInfo);

                LPCUTF8 szName = pField->GetName();
                info->members[i].m_member_name = new char[strlen(szName) + 1];
                strcpy(info->members[i].m_member_name, szName);
                info->members[i].m_member_offset = (ULONG)pField->GetOffset() + (fReferenceType ? Object::GetOffsetOfFirstField() : 0);
                info->members[i].m_member_type = GetTypeInfoFromTypeHandle(pField->GetExactFieldType(typeHandle), pTypeMap);
            }
            break;
        }
        case ELEMENT_TYPE_ARRAY:
        case ELEMENT_TYPE_SZARRAY:
            //typeInfo->m_type_name = "array";
            break;
        default:
            break;
            //typeInfo->m_type_name = "unknown";
    }
    // name the type
    SString sName;
    typeInfo->typeHandle.GetName(sName);
    StackScratchBuffer buffer;
    const UTF8 *utf8 = sName.GetUTF8(buffer);
    typeInfo->m_type_name = new char[strlen(utf8) + 1];
    strcpy(typeInfo->m_type_name, utf8);

    pTypeMap->Add(pMT, typeInfo);
    return typeInfo;
}

TypeInfoBase* GetTypeInfoFromSignature(MethodDesc *MethodDescPtr,
                              NotifyGdb::PMT_TypeInfoMap pTypeMap,
                              PCCOR_SIGNATURE typePtr,
                              unsigned typeLen,
                              unsigned ilIndex)
{
    unsigned numArgs;
    PCCOR_SIGNATURE typeEnd = typePtr + typeLen;

     // get the calling convention out
    CorSigUncompressData(typePtr);

    numArgs = CorSigUncompressData(typePtr);
    mdToken tk;
    int i = 0;

    while (typePtr < typeEnd)
    {
        if (i > ilIndex)
            break;

        Module * module = MethodDescPtr->GetMethodTable()->GetModule();

        switch (*typePtr)
        {
            case ELEMENT_TYPE_VOID:
            case ELEMENT_TYPE_BOOLEAN:
            case ELEMENT_TYPE_CHAR:
            case ELEMENT_TYPE_I1:
            case ELEMENT_TYPE_U1:
            case ELEMENT_TYPE_I2:
            case ELEMENT_TYPE_U2:
            case ELEMENT_TYPE_I4:
            case ELEMENT_TYPE_U4:
            case ELEMENT_TYPE_I8:
            case ELEMENT_TYPE_U8:
            case ELEMENT_TYPE_R4:
            case ELEMENT_TYPE_R8:
            case ELEMENT_TYPE_U:
            case ELEMENT_TYPE_I:
                if (i < ilIndex)
                {
                    break;
                }
                {
                    PTR_MethodTable m = MscorlibBinder::GetElementType(static_cast<CorElementType>(*typePtr));
                    if (m != nullptr)
                    {                    
                        return GetTypeInfoFromTypeHandle(TypeHandle(m), pTypeMap);
                    }
                    else
                    {
                        return nullptr;
                    }
                }
                break;
            case ELEMENT_TYPE_CLASS:
            {
                typePtr += CorSigUncompressToken(typePtr+1, &tk);
                if (i < ilIndex)
                {
                    break;
                }
                {
                    TypeHandle typeHandle;

                    if (TypeFromToken(tk) == mdtTypeRef)
                    {
                        typeHandle = module->LookupTypeRef(tk);
                    }
                    else if (TypeFromToken(tk) == mdtTypeDef)
                    {
                        typeHandle = module->LookupTypeDef(tk);
                    }
                    else
                    {
                        printf("TypeFromToken(tk) = 0x%x", TypeFromToken(tk));
                        break;
                    }
                    return GetTypeInfoFromTypeHandle(typeHandle, pTypeMap);
                }
                break;
            }
            case ELEMENT_TYPE_ARRAY:
            case ELEMENT_TYPE_SZARRAY:
                typePtr++;
                break;
            default:
                break;
        }

        i++;
        typePtr++;
    }
    return nullptr;
}

TypeInfoBase* GetArgTypeInfo(MethodDesc* MethodDescPtr,
                    NotifyGdb::PMT_TypeInfoMap pTypeMap,
                    unsigned ilIndex)
{
    DWORD cbSigLen;
    PCCOR_SIGNATURE pComSig;
    MethodDescPtr->GetSig(&pComSig, &cbSigLen);
    return GetTypeInfoFromSignature(MethodDescPtr, pTypeMap, pComSig, cbSigLen, ilIndex);
}

TypeInfoBase* GetLocalTypeInfo(MethodDesc *MethodDescPtr,
                      NotifyGdb::PMT_TypeInfoMap pTypeMap,
                      unsigned ilIndex)
{
    COR_ILMETHOD_DECODER method(MethodDescPtr->GetILHeader());
    if (method.GetLocalVarSigTok())
    {
        DWORD cbSigLen;
        PCCOR_SIGNATURE pComSig;
        CQuickBytes qbMemberSig;
        size_t dwL;

        if (FAILED(MethodDescPtr->GetMDImport()->GetSigFromToken(method.GetLocalVarSigTok(), &cbSigLen, &pComSig)))
        {
            printf("\nInvalid record");
            return nullptr;
        }

        _ASSERTE(*pComSig == IMAGE_CEE_CS_CALLCONV_LOCAL_SIG);
        return GetTypeInfoFromSignature(MethodDescPtr, pTypeMap, pComSig, cbSigLen, ilIndex);
    }
    return nullptr;
}

HRESULT GetArgNameByILIndex(MethodDesc* MethodDescPtr, unsigned index, LPCSTR &paramName)
{
    IMDInternalImport* mdImport = MethodDescPtr->GetMDImport();
    mdParamDef paramToken;
    USHORT seq;
    DWORD attr;
    HRESULT status;

    // Param indexing is 1-based.
    ULONG32 mdIndex = index + 1;

    NewHolder<MetaSig> sig = new MetaSig(MethodDescPtr);
    if (sig->HasThis())
    {
        mdIndex--;
    }
    status = mdImport->FindParamOfMethod(MethodDescPtr->GetMemberDef(), mdIndex, &paramToken);
    if (status == S_OK)
    {
        status = mdImport->GetParamDefProps(paramToken, &seq, &attr, &paramName);
    }
    return status;
}

// Copy-pasted from src/debug/di/module.cpp
HRESULT FindNativeInfoInILVariable(DWORD dwIndex,
                                   SIZE_T ip,
                                   ICorDebugInfo::NativeVarInfo** nativeInfoList,
                                   unsigned int nativeInfoCount,
                                   ICorDebugInfo::NativeVarInfo** ppNativeInfo)
{
    _ASSERTE(ppNativeInfo != NULL);
    *ppNativeInfo = NULL;
    int lastGoodOne = -1;
    for (unsigned int i = 0; i < (unsigned)nativeInfoCount; i++)
    {
        if ((*nativeInfoList)[i].varNumber == dwIndex)
        {
            if ((lastGoodOne == -1) || ((*nativeInfoList)[lastGoodOne].startOffset < (*nativeInfoList)[i].startOffset))
            {
                lastGoodOne = i;
            }

            if (((*nativeInfoList)[i].startOffset <= ip) &&
                ((*nativeInfoList)[i].endOffset > ip))
            {
                *ppNativeInfo = &((*nativeInfoList)[i]);

                return S_OK;
            }
        }
    }

    if ((lastGoodOne > -1) && ((*nativeInfoList)[lastGoodOne].endOffset == ip))
    {
        *ppNativeInfo = &((*nativeInfoList)[lastGoodOne]);
        return S_OK;
    }

    return CORDBG_E_IL_VAR_NOT_AVAILABLE;
}

BYTE* DebugInfoStoreNew(void * pData, size_t cBytes)
{
    return new (nothrow) BYTE[cBytes];
}

/* Get IL to native offsets map */
HRESULT
GetMethodNativeMap(MethodDesc* methodDesc,
                   ULONG32* numMap,
                   DebuggerILToNativeMap** map,
                   ULONG32* pcVars,
                   ICorDebugInfo::NativeVarInfo** ppVars)
{
    // Use the DebugInfoStore to get IL->Native maps.
    // It doesn't matter whether we're jitted, ngenned etc.

    DebugInfoRequest request;
    TADDR nativeCodeStartAddr = PCODEToPINSTR(methodDesc->GetNativeCode());
    request.InitFromStartingAddr(methodDesc, nativeCodeStartAddr);

    // Bounds info.
    ULONG32 countMapCopy;
    NewHolder<ICorDebugInfo::OffsetMapping> mapCopy(NULL);

    BOOL success = DebugInfoManager::GetBoundariesAndVars(request,
                                                          DebugInfoStoreNew,
                                                          NULL, // allocator
                                                          &countMapCopy,
                                                          &mapCopy,
                                                          pcVars,
                                                          ppVars);

    if (!success)
    {
        return E_FAIL;
    }

    // Need to convert map formats.
    *numMap = countMapCopy;

    *map = new (nothrow) DebuggerILToNativeMap[countMapCopy];
    if (!*map)
    {
        return E_OUTOFMEMORY;
    }

    ULONG32 i;
    for (i = 0; i < *numMap; i++)
    {
        (*map)[i].ilOffset = mapCopy[i].ilOffset;
        (*map)[i].nativeStartOffset = mapCopy[i].nativeOffset;
        if (i > 0)
        {
            (*map)[i - 1].nativeEndOffset = (*map)[i].nativeStartOffset;
        }
        (*map)[i].source = mapCopy[i].source;
    }
    if (*numMap >= 1)
    {
        (*map)[i - 1].nativeEndOffset = 0;
    }
    return S_OK;
}

HRESULT GetLocalsDebugInfo(MethodDesc* MethodDescPtr,
                           NotifyGdb::PMT_TypeInfoMap pTypeMap,
                           LocalsInfo& locals,
                           NewArrayHolder<LocalsDebugInfo>& localsDebug,
                           int localsDebugSize,
                           NewArrayHolder<ArgsDebugInfo>& argsDebug,
                           int nArgsCount,
                           int startNativeOffset)
{

    ICorDebugInfo::NativeVarInfo* nativeVar = NULL;
    int thisOffs = 0;
    if (!MethodDescPtr->IsStatic())
    {
        thisOffs = 1;
    }

    for (int i = 0; i < nArgsCount - thisOffs; i++)
    {
        if (FindNativeInfoInILVariable(i + thisOffs, startNativeOffset, &locals.pVars, locals.countVars, &nativeVar) == S_OK)
        {
            argsDebug[i + thisOffs].m_arg_type = GetArgTypeInfo(MethodDescPtr, pTypeMap, i + 1);
            GetArgNameByILIndex(MethodDescPtr, i + thisOffs, argsDebug[i + thisOffs].m_arg_name);
            argsDebug[i + thisOffs].m_il_index = i;
            argsDebug[i + thisOffs].m_native_offset = nativeVar->loc.vlStk.vlsOffset;
        }
    }
    //Add info about 'this' as first argument
    if (thisOffs == 1)
    {
        if (FindNativeInfoInILVariable(0, startNativeOffset, &locals.pVars, locals.countVars, &nativeVar) == S_OK)
        {
            argsDebug[0].m_arg_type = GetTypeInfoFromTypeHandle(TypeHandle(MethodDescPtr->GetMethodTable()), pTypeMap);
            argsDebug[0].m_arg_name = "this";
            argsDebug[0].m_il_index = 0;
            argsDebug[0].m_native_offset = nativeVar->loc.vlStk.vlsOffset;
         }
    }
    for (int i = 0; i < localsDebugSize; i++)
    {
        if (FindNativeInfoInILVariable(
                i + nArgsCount, startNativeOffset, &locals.pVars, locals.countVars, &nativeVar) == S_OK)
        {
            localsDebug[i].m_var_type = GetLocalTypeInfo(MethodDescPtr, pTypeMap, i);
            localsDebug[i].m_var_name = locals.localsName[i]; // FIXME: release memory
            localsDebug[i].m_il_index = i;
            localsDebug[i].m_native_offset = nativeVar->loc.vlStk.vlsOffset;
        }
    }
    return S_OK;
}
/* Get mapping of IL offsets to source line numbers */
HRESULT
GetDebugInfoFromPDB(MethodDesc* MethodDescPtr, SymbolsInfo** symInfo, unsigned int &symInfoLen,  LocalsInfo &locals)
{
    DebuggerILToNativeMap* map = NULL;

    ULONG32 numMap;

    if (!getInfoForMethodDelegate)
        return E_FAIL;
 
    if (GetMethodNativeMap(MethodDescPtr, &numMap, &map, &locals.countVars, &locals.pVars) != S_OK)
        return E_FAIL;

    const Module* mod = MethodDescPtr->GetMethodTable()->GetModule();
    SString modName = mod->GetFile()->GetPath();
    if (modName.IsEmpty())
        return E_FAIL;

    StackScratchBuffer scratch;
    const char* szModName = modName.GetUTF8(scratch);

    NewHolder<MethodDebugInfo> methodDebugInfo = new (nothrow) MethodDebugInfo();
    if (methodDebugInfo == nullptr)
        return E_OUTOFMEMORY;

    methodDebugInfo->points = (SequencePointInfo*) CoTaskMemAlloc(sizeof(SequencePointInfo) * numMap);
    if (methodDebugInfo->points == nullptr)
        return E_OUTOFMEMORY;

    methodDebugInfo->size = numMap;

    if (getInfoForMethodDelegate(szModName, MethodDescPtr->GetMemberDef(), *methodDebugInfo) == FALSE)
        return E_FAIL;

    symInfoLen = methodDebugInfo->size;
    *symInfo = new (nothrow) SymbolsInfo[symInfoLen];
    if (*symInfo == nullptr)
        return E_FAIL;
    locals.size = methodDebugInfo->localsSize;
    locals.localsName = new (nothrow) char *[locals.size];
    if (locals.localsName == nullptr)
        return E_FAIL;

    for (ULONG32 i = 0; i < locals.size; i++)
    {
        size_t sizeRequired = WideCharToMultiByte(CP_UTF8, 0, methodDebugInfo->locals[i], -1, NULL, 0, NULL, NULL);
        locals.localsName[i] = new (nothrow) char[sizeRequired];

        int len = WideCharToMultiByte(
            CP_UTF8, 0, methodDebugInfo->locals[i], -1, locals.localsName[i], sizeRequired, NULL, NULL);
    }

    for (ULONG32 i = 0; i < symInfoLen; i++)
    {
        for (ULONG32 j = 0; j < numMap; j++)
        {
            if (methodDebugInfo->points[i].ilOffset == map[j].ilOffset)
            {
                SymbolsInfo& s = (*symInfo)[i];
                const SequencePointInfo& sp = methodDebugInfo->points[i];

                s.nativeOffset = map[j].nativeStartOffset;
                s.ilOffset = map[j].ilOffset;
                s.fileIndex = 0;
                int len = WideCharToMultiByte(CP_UTF8, 0, sp.fileName, -1, s.fileName, sizeof(s.fileName), NULL, NULL);
                s.fileName[len] = 0;
                s.lineNumber = sp.lineNumber;
            }
        }
    }

    CoTaskMemFree(methodDebugInfo->points);
    return S_OK;
}

// GDB JIT interface
typedef enum
{
  JIT_NOACTION = 0,
  JIT_REGISTER_FN,
  JIT_UNREGISTER_FN
} jit_actions_t;

struct jit_code_entry
{
  struct jit_code_entry *next_entry;
  struct jit_code_entry *prev_entry;
  const char *symfile_addr;
  UINT64 symfile_size;
};

struct jit_descriptor
{
  UINT32 version;
  /* This type should be jit_actions_t, but we use uint32_t
     to be explicit about the bitwidth.  */
  UINT32 action_flag;
  struct jit_code_entry *relevant_entry;
  struct jit_code_entry *first_entry;
};
// GDB puts a breakpoint in this function.
// To prevent from inlining we add noinline attribute and inline assembler statement.
extern "C"
void __attribute__((noinline)) __jit_debug_register_code() { __asm__(""); };

/* Make sure to specify the version statically, because the
   debugger may check the version before we can set it.  */
struct jit_descriptor __jit_debug_descriptor = { 1, 0, 0, 0 };

// END of GDB JIT interface

/* Predefined section names */
const char* SectionNames[] = {
    "", ".text", ".shstrtab", ".debug_str", ".debug_abbrev", ".debug_info",
    ".debug_pubnames", ".debug_pubtypes", ".debug_line", ".symtab", ".strtab", ".thunks", ""
};

const int SectionNamesCount = sizeof(SectionNames) / sizeof(SectionNames[0]);

/* Static data for section headers */
struct SectionHeader {
    uint32_t m_type;
    uint64_t m_flags;
} Sections[] = {
    {SHT_NULL, 0},
    {SHT_PROGBITS, SHF_ALLOC | SHF_EXECINSTR},
    {SHT_STRTAB, 0},
    {SHT_PROGBITS, SHF_MERGE | SHF_STRINGS },
    {SHT_PROGBITS, 0},
    {SHT_PROGBITS, 0},
    {SHT_PROGBITS, 0},
    {SHT_PROGBITS, 0},
    {SHT_PROGBITS, 0},
    {SHT_SYMTAB, 0},
    {SHT_STRTAB, 0},
    {SHT_PROGBITS, SHF_ALLOC | SHF_EXECINSTR}
};

/* Static data for .debug_str section */
const char* DebugStrings[] = {
  "CoreCLR", "" /* module name */, "" /* module path */, "" /* method name */
};

const int DebugStringCount = sizeof(DebugStrings) / sizeof(DebugStrings[0]);

/* Static data for .debug_abbrev */
const unsigned char AbbrevTable[] = {
    1, DW_TAG_compile_unit, DW_CHILDREN_yes,
        DW_AT_producer, DW_FORM_strp, DW_AT_language, DW_FORM_data2, DW_AT_name, DW_FORM_strp,
        DW_AT_stmt_list, DW_FORM_sec_offset, 0, 0,

    2, DW_TAG_base_type, DW_CHILDREN_no,
        DW_AT_name, DW_FORM_strp, DW_AT_encoding, DW_FORM_data1, DW_AT_byte_size, DW_FORM_data1, 0, 0,

    3, DW_TAG_typedef, DW_CHILDREN_no,
        DW_AT_name, DW_FORM_strp, DW_AT_decl_file, DW_FORM_data1, DW_AT_decl_line, DW_FORM_data1,
        DW_AT_type, DW_FORM_ref4, 0, 0,

    4, DW_TAG_subprogram, DW_CHILDREN_yes,
        DW_AT_name, DW_FORM_strp, DW_AT_decl_file, DW_FORM_data1, DW_AT_decl_line, DW_FORM_data1,
        DW_AT_type, DW_FORM_ref4, DW_AT_external, DW_FORM_flag_present,
        DW_AT_low_pc, DW_FORM_addr, DW_AT_high_pc,
#if defined(_TARGET_AMD64_)
        DW_FORM_data8,
#elif defined(_TARGET_ARM_)
        DW_FORM_data4,
#else
#error Unsupported platform!
#endif
        DW_AT_frame_base, DW_FORM_exprloc, 0, 0,

    5, DW_TAG_variable, DW_CHILDREN_no,
        DW_AT_name, DW_FORM_strp, DW_AT_decl_file, DW_FORM_data1, DW_AT_decl_line, DW_FORM_data1, DW_AT_type,
        DW_FORM_ref4, DW_AT_location, DW_FORM_exprloc, 0, 0,

    6, DW_TAG_formal_parameter, DW_CHILDREN_no,
        DW_AT_name, DW_FORM_strp, DW_AT_decl_file, DW_FORM_data1, DW_AT_decl_line, DW_FORM_data1, DW_AT_type,
        DW_FORM_ref4, DW_AT_location, DW_FORM_exprloc, 0, 0,

    7, DW_TAG_class_type, DW_CHILDREN_yes,
        DW_AT_name, DW_FORM_strp, DW_AT_byte_size, DW_FORM_data1, 0, 0,

    8, DW_TAG_member, DW_CHILDREN_no,
        DW_AT_name, DW_FORM_strp, DW_AT_type, DW_FORM_ref4, DW_AT_data_member_location, DW_FORM_data1, 0, 0,

    9, DW_TAG_reference_type, DW_CHILDREN_no,
        DW_AT_type, DW_FORM_ref4, 0, 0,

    0
};

const int AbbrevTableSize = sizeof(AbbrevTable);

/* Static data for .debug_line, including header */
#define DWARF_LINE_BASE (-5)
#define DWARF_LINE_RANGE 14
#define DWARF_OPCODE_BASE 13

DwarfLineNumHeader LineNumHeader = {
    0, 2, 0, 1, 1, DWARF_LINE_BASE, DWARF_LINE_RANGE, DWARF_OPCODE_BASE, {0, 1, 1, 1, 1, 0, 0, 0, 1, 0, 0, 1}
};

/* Static data for .debug_info */
struct __attribute__((packed)) DebugInfoCU
{
    uint8_t m_cu_abbrev;
    uint32_t m_prod_off;
    uint16_t m_lang;
    uint32_t m_cu_name;
    uint32_t m_line_num;
} debugInfoCU = {
    1, 0, DW_LANG_C89, 0, 0
};

struct __attribute__((packed)) DebugInfoSub
{
    uint8_t m_sub_abbrev;
    uint32_t m_sub_name;
    uint8_t m_file, m_line;
    uint32_t m_sub_type;
#if defined(_TARGET_AMD64_)
    uint64_t m_sub_low_pc, m_sub_high_pc;
#elif defined(_TARGET_ARM_)
    uint32_t m_sub_low_pc, m_sub_high_pc;
#else
#error Unsupported platform!
#endif
    uint8_t m_sub_loc[2];
} debugInfoSub = {
    4, 0, 1, 1, 0x1a, 0, 0, {1,
#if defined(_TARGET_AMD64_)
        DW_OP_reg6
#elif defined(_TARGET_ARM_)
        DW_OP_reg11
#else
#error Unsupported platform!
#endif
    },
};



struct __attribute__((packed)) DebugInfoType
{
    uint8_t m_type_abbrev;
    uint32_t m_type_name;
    uint8_t m_encoding;
    uint8_t m_byte_size;
};

struct __attribute__((packed)) DebugInfoVar
{
    uint8_t m_var_abbrev;
    uint32_t m_var_name;
    uint8_t m_var_file, m_var_line;
    uint32_t m_var_type;
};

struct __attribute__((packed)) DebugInfoClassType
{
    uint8_t m_type_abbrev;
    uint32_t m_type_name;
    uint8_t m_byte_size;
};

struct __attribute__((packed)) DebugInfoClassMember
{
    uint8_t m_member_abbrev;
    uint32_t m_member_name;
    uint32_t m_member_type;
    uint8_t m_member_loc;
};

struct __attribute__((packed)) DebugInfoRefType
{
    uint8_t m_type_abbrev;
    uint32_t m_ref_type;
};

void TypeInfoBase::DumpStrings(char* ptr, int& offset)
{
    if (ptr != nullptr)
    {
        strcpy(ptr + offset, m_type_name);
        m_type_name_offset = offset;
    }
    offset += strlen(m_type_name) + 1;
}

void PrimitiveTypeInfo::DumpDebugInfo(char* ptr, int& offset)
{
    if (m_type_offset != 0)
    {
        return;
    }

    if (ptr != nullptr)
    {
        NewHolder<DebugInfoType> bufType = new (nothrow) DebugInfoType;
        if (bufType == nullptr)
            return;

        bufType->m_type_abbrev = 2;
        bufType->m_type_name = m_type_name_offset;
        bufType->m_encoding = m_type_encoding;
        bufType->m_byte_size = m_type_size;

        memcpy(ptr + offset,
               bufType,
               sizeof(DebugInfoType));
        m_type_offset = offset;
    }

    offset += sizeof(DebugInfoType);
}

ClassTypeInfo::ClassTypeInfo(int num_members)
        : TypeInfoBase(),
          m_num_members(num_members),
          members(new TypeMember[num_members])
{
}

ClassTypeInfo::~ClassTypeInfo()
{
    if (members != nullptr && m_num_members > 0)
    {
        delete[] members;
    }
}

void TypeMember::DumpStrings(char* ptr, int& offset)
{
    if (ptr != nullptr)
    {
        strcpy(ptr + offset, m_member_name);
        m_member_name_offset = offset;
    }
    offset += strlen(m_member_name) + 1;
}

void TypeMember::DumpDebugInfo(char* ptr, int& offset)
{
    if (ptr != nullptr)
    {
        NewHolder<DebugInfoClassMember> memberEntry = new (nothrow) DebugInfoClassMember;
        if (memberEntry == nullptr)
            return;

        memberEntry->m_member_abbrev = 8;
        memberEntry->m_member_name = m_member_name_offset;
        memberEntry->m_member_loc = m_member_offset;
        memberEntry->m_member_type = m_member_type->m_type_offset;

        memcpy(ptr + offset, memberEntry, sizeof(DebugInfoClassMember));
    }
    offset += sizeof(DebugInfoClassMember);
}

void FunctionMember::DumpDebugInfo(char* ptr, int& offset)
{
}

void ClassTypeInfo::DumpStrings(char* ptr, int& offset)
{
    TypeInfoBase::DumpStrings(ptr, offset);

    for (int i = 0; i < m_num_members; ++i)
    {
        members[i].DumpStrings(ptr, offset);
    }
}

void ClassTypeInfo::DumpDebugInfo(char* ptr, int& offset)
{
    if (m_type_offset != 0)
    {
        return;
    }
    // make sure that types of all members are dumped
    for (int i = 0; i < m_num_members; ++i)
    {
        if (members[i].m_member_type->m_type_offset == 0)
        {
            members[i].m_member_type->DumpDebugInfo(ptr, offset);
        }
    }

    if (ptr != nullptr)
    {
        NewHolder<DebugInfoClassType> bufType = new (nothrow) DebugInfoClassType;
        if (bufType == nullptr)
            return;

        bufType->m_type_abbrev = 7;
        bufType->m_type_name = m_type_name_offset;
        bufType->m_byte_size = m_type_size;

        memcpy(ptr + offset, bufType, sizeof(DebugInfoClassType));
        m_type_offset = offset;
    }
    offset += sizeof(DebugInfoClassType);

    for (int i = 0; i < m_num_members; ++i)
    {
        members[i].DumpDebugInfo(ptr, offset);
    }

    // members terminator
    if (m_num_members > 0)
    {
        if (ptr != nullptr)
        {
            ptr[offset] = 0;
        }
        offset++;
    }

    if (!typeHandle.IsValueType())
    {
        if (ptr != nullptr)
        {
            NewHolder<DebugInfoRefType> refType = new (nothrow) DebugInfoRefType;
            refType->m_type_abbrev = 9;
            refType->m_ref_type = m_type_offset;

            memcpy(ptr + offset, refType, sizeof(DebugInfoRefType));
            m_type_offset = offset;
        }
        offset += sizeof(DebugInfoRefType);
    }
}

/* static data for symbol strings */
struct Elf_Symbol {
    const char* m_name;
    int m_off;
    TADDR m_value;
    int m_section, m_size;
    Elf_Symbol() : m_name(nullptr), m_off(0), m_value(0), m_section(0), m_size(0) {}
};

int SymbolCount = 0;
NewArrayHolder<Elf_Symbol> SymbolNames;
TADDR MinCallAddr, MaxCallAddr;

/* Create ELF/DWARF debug info for jitted method */
void NotifyGdb::MethodCompiled(MethodDesc* MethodDescPtr)
{
    PCODE pCode = MethodDescPtr->GetNativeCode();
    if (pCode == NULL)
        return;
    unsigned int symInfoLen = 0;
    NewArrayHolder<SymbolsInfo> symInfo = nullptr;
    LocalsInfo locals;



    /* Get method name & size of jitted code */
    LPCUTF8 methodName = MethodDescPtr->GetName();
    EECodeInfo codeInfo(pCode);
    TADDR codeSize = codeInfo.GetCodeManager()->GetFunctionSize(codeInfo.GetGCInfoToken());
    
#ifdef _TARGET_ARM_
    pCode &= ~1; // clear thumb flag for debug info
#endif    

    /* Get module name */
    const Module* mod = MethodDescPtr->GetMethodTable()->GetModule();
    SString modName = mod->GetFile()->GetPath();
    StackScratchBuffer scratch;
    const char* szModName = modName.GetUTF8(scratch);
    const char *szModulePath, *szModuleFile;
    SplitPathname(szModName, szModulePath, szModuleFile);


    int length = MultiByteToWideChar(CP_UTF8, 0, szModuleFile, -1, NULL, 0);
    if (length == 0)
        return;
    NewArrayHolder<WCHAR> wszModuleFile = new (nothrow) WCHAR[length+1];
    length = MultiByteToWideChar(CP_UTF8, 0, szModuleFile, -1, wszModuleFile, length);

    if (length == 0)
        return;

    static NewArrayHolder<WCHAR> wszModuleNames = nullptr;
    DWORD cCharsNeeded = 0;

    // Get names of interesting modules from environment
    if (wszModuleNames == nullptr)
    {
        cCharsNeeded = GetEnvironmentVariableW(W("CORECLR_GDBJIT"), NULL, 0);

        if((cCharsNeeded == 0) || (cCharsNeeded >= MAX_LONGPATH))
            return;
        wszModuleNames = new WCHAR[cCharsNeeded+1];
        cCharsNeeded = GetEnvironmentVariableW(W("CORECLR_GDBJIT"), wszModuleNames, cCharsNeeded);
        if(cCharsNeeded == 0)
            return;
    }
    else
    {
        cCharsNeeded = wcslen(wszModuleNames);
    }

    BOOL isUserDebug = FALSE;

    NewArrayHolder<WCHAR> wszModuleName = new WCHAR[cCharsNeeded+1];
    LPWSTR pComma = wcsstr(wszModuleNames, W(","));
    LPWSTR tmp = wszModuleNames;

    while (pComma != NULL)
    {
        wcsncpy(wszModuleName, tmp, pComma - tmp);
        wszModuleName[pComma - tmp] = W('\0');

        if (wcscmp(wszModuleName, wszModuleFile) == 0)
        {
            isUserDebug = TRUE;
            break;
        }
        tmp = pComma + 1;
        pComma = wcsstr(tmp, W(","));
    }
    if (isUserDebug == FALSE)
    {
        wcsncpy(wszModuleName, tmp, wcslen(tmp));
        wszModuleName[wcslen(tmp)] = W('\0');
        if (wcscmp(wszModuleName, wszModuleFile) == 0)
        {
            isUserDebug = TRUE;
        }
    }

    if (isUserDebug == FALSE)
    {
        return;
    }

    NewHolder<MT_TypeInfoMap> pTypeMap = new MT_TypeInfoMap();

    if (pTypeMap == nullptr)
    {
        return;
    }

    CodeHeader* pCH = ((CodeHeader*)(pCode & ~1)) - 1;
    CalledMethod* pCalledMethods = reinterpret_cast<CalledMethod*>(pCH->GetCalledMethods());
    /* Collect addresses of thunks called by method */
    if (!CollectCalledMethods(pCalledMethods))
    {
        return;
    }
    pCH->SetCalledMethods(NULL);

    /* Get debug info for method from portable PDB */
    HRESULT hr = GetDebugInfoFromPDB(MethodDescPtr, &symInfo, symInfoLen, locals);
    if (FAILED(hr) || symInfoLen == 0)
    {
        return;
    }

    NewArrayHolder<LocalsDebugInfo> localsDebug = new(nothrow) LocalsDebugInfo[locals.size];

    NewHolder<MetaSig> sig = new MetaSig(MethodDescPtr);
    int nArgsCount = sig->NumFixedArgs();
    if (sig->HasThis())
        nArgsCount++;

    GetArgTypeInfo(MethodDescPtr, pTypeMap, 0);

    NewArrayHolder<ArgsDebugInfo> argsDebug = new(nothrow) ArgsDebugInfo[nArgsCount];

    GetLocalsDebugInfo(MethodDescPtr, pTypeMap, locals, localsDebug, locals.size, argsDebug, nArgsCount, symInfo[0].nativeOffset);

    MemBuf elfHeader, sectHeaders, sectStr, sectSymTab, sectStrTab, dbgInfo, dbgAbbrev, dbgPubname, dbgPubType, dbgLine,
        dbgStr, elfFile;

    /* Build .debug_abbrev section */
    if (!BuildDebugAbbrev(dbgAbbrev))
    {
        return;
    }
    debugInfoSub.m_sub_low_pc = pCode;
    debugInfoSub.m_sub_high_pc = codeSize;
    /* Build .debug_line section */
    if (!BuildLineTable(dbgLine, pCode, codeSize, symInfo, symInfoLen))
    {
        return;
    }
    
    DebugStrings[1] = szModuleFile;
    DebugStrings[3] = methodName;
    
    /* Build .debug_str section */
    if (!BuildDebugStrings(dbgStr, pTypeMap, argsDebug, nArgsCount, localsDebug, locals.size))
    {
        return;
    }
    
    /* Build .debug_info section */
    if (!BuildDebugInfo(dbgInfo, pTypeMap, argsDebug, nArgsCount, localsDebug, locals.size))
    {
        return;
    }

    for (int i = 0; i < locals.size; i++)
    {
        delete[] locals.localsName[i];
    }
    /* Build .debug_pubname section */
    if (!BuildDebugPub(dbgPubname, methodName, dbgInfo.MemSize, 0x28))
    {
        return;
    }
    
    /* Build debug_pubtype section */
    if (!BuildDebugPub(dbgPubType, "int", dbgInfo.MemSize, 0x1a))
    {
        return;
    }

    /* Build .strtab section */
     SymbolNames[0].m_name = "";
     SymbolNames[1].m_name = methodName;
     SymbolNames[1].m_value = pCode;
     SymbolNames[1].m_section = 1;
     SymbolNames[1].m_size = codeSize;
        
    if (!BuildStringTableSection(sectStrTab))
    {
        return;
    }
    /* Build .symtab section */
    if (!BuildSymbolTableSection(sectSymTab, pCode, codeSize))
    {
        return;
    }


    /* Build section names section */
    if (!BuildSectionNameTable(sectStr))
    {
        return;
    }

    /* Build section headers table */
    if (!BuildSectionTable(sectHeaders))
    {
        return;
    }

    /* Patch section offsets & sizes */
    long offset = sizeof(Elf_Ehdr);
    Elf_Shdr* pShdr = reinterpret_cast<Elf_Shdr*>(sectHeaders.MemPtr.GetValue());
    ++pShdr; // .text
    pShdr->sh_addr = pCode;
    pShdr->sh_size = codeSize;
    ++pShdr; // .shstrtab
    pShdr->sh_offset = offset;
    pShdr->sh_size = sectStr.MemSize;
    offset += sectStr.MemSize;
    ++pShdr; // .debug_str
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgStr.MemSize;
    offset += dbgStr.MemSize;
    ++pShdr; // .debug_abbrev
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgAbbrev.MemSize;
    offset += dbgAbbrev.MemSize;
    ++pShdr; // .debug_info
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgInfo.MemSize;
    offset += dbgInfo.MemSize;
    ++pShdr; // .debug_pubnames
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgPubname.MemSize;
    offset += dbgPubname.MemSize;
    ++pShdr; // .debug_pubtypes
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgPubType.MemSize;
    offset += dbgPubType.MemSize;
    ++pShdr; // .debug_line
    pShdr->sh_offset = offset;
    pShdr->sh_size = dbgLine.MemSize;
    offset += dbgLine.MemSize;
    ++pShdr; // .symtab
    pShdr->sh_offset = offset;
    pShdr->sh_size = sectSymTab.MemSize;
    pShdr->sh_link = 10;
    offset += sectSymTab.MemSize;
    ++pShdr; // .strtab
    pShdr->sh_offset = offset;
    pShdr->sh_size = sectStrTab.MemSize;
    offset += sectStrTab.MemSize;
    ++pShdr; // .thunks
    pShdr->sh_addr = MinCallAddr;
    pShdr->sh_size = (MaxCallAddr - MinCallAddr) + 8;

    /* Build ELF header */
    if (!BuildELFHeader(elfHeader))
    {
        return;
    }
    Elf_Ehdr* header = reinterpret_cast<Elf_Ehdr*>(elfHeader.MemPtr.GetValue());
#ifdef _TARGET_ARM_
    header->e_flags = EF_ARM_EABI_VER5;
#ifdef ARM_SOFTFP
    header->e_flags |= EF_ARM_SOFT_FLOAT;
#else    
    header->e_flags |= EF_ARM_VFP_FLOAT;
#endif
#endif    
    header->e_shoff = offset;
    header->e_shentsize = sizeof(Elf_Shdr);
    header->e_shnum = SectionNamesCount - 1;
    header->e_shstrndx = 2;

    /* Build ELF image in memory */
    elfFile.MemSize = elfHeader.MemSize + sectStr.MemSize + dbgStr.MemSize + dbgAbbrev.MemSize + dbgInfo.MemSize +
                      dbgPubname.MemSize + dbgPubType.MemSize + dbgLine.MemSize + sectSymTab.MemSize +
                      sectStrTab.MemSize + sectHeaders.MemSize;
    elfFile.MemPtr =  new (nothrow) char[elfFile.MemSize];
    if (elfFile.MemPtr == nullptr)
    {
        return;
    }
    
    /* Copy section data */
    offset = 0;
    memcpy(elfFile.MemPtr, elfHeader.MemPtr, elfHeader.MemSize);
    offset += elfHeader.MemSize;
    memcpy(elfFile.MemPtr + offset, sectStr.MemPtr, sectStr.MemSize);
    offset +=  sectStr.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgStr.MemPtr, dbgStr.MemSize);
    offset +=  dbgStr.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgAbbrev.MemPtr, dbgAbbrev.MemSize);
    offset +=  dbgAbbrev.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgInfo.MemPtr, dbgInfo.MemSize);
    offset +=  dbgInfo.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgPubname.MemPtr, dbgPubname.MemSize);
    offset +=  dbgPubname.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgPubType.MemPtr, dbgPubType.MemSize);
    offset +=  dbgPubType.MemSize;
    memcpy(elfFile.MemPtr + offset, dbgLine.MemPtr, dbgLine.MemSize);
    offset +=  dbgLine.MemSize;
    memcpy(elfFile.MemPtr + offset, sectSymTab.MemPtr, sectSymTab.MemSize);
    offset +=  sectSymTab.MemSize;
    memcpy(elfFile.MemPtr + offset, sectStrTab.MemPtr, sectStrTab.MemSize);
    offset +=  sectStrTab.MemSize;

    memcpy(elfFile.MemPtr + offset, sectHeaders.MemPtr, sectHeaders.MemSize);

    elfFile.MemPtr.SuppressRelease();

#if 1//def GDBJIT_DUMPELF
    DumpElf(methodName, elfFile);
#endif

    /* Create GDB JIT structures */
    NewHolder<jit_code_entry> jit_symbols = new (nothrow) jit_code_entry;
    
    if (jit_symbols == nullptr)
    {
        return;
    }
    
    /* Fill the new entry */
    jit_symbols->next_entry = jit_symbols->prev_entry = 0;
    jit_symbols->symfile_addr = elfFile.MemPtr;
    jit_symbols->symfile_size = elfFile.MemSize;
    
    /* Link into list */
    jit_code_entry *head = __jit_debug_descriptor.first_entry;
    __jit_debug_descriptor.first_entry = jit_symbols;
    if (head != 0)
    {
        jit_symbols->next_entry = head;
        head->prev_entry = jit_symbols;
    }
    
    jit_symbols.SuppressRelease();

    /* Notify the debugger */
    __jit_debug_descriptor.relevant_entry = jit_symbols;
    __jit_debug_descriptor.action_flag = JIT_REGISTER_FN;
    __jit_debug_register_code();
}

void NotifyGdb::MethodDropped(MethodDesc* MethodDescPtr)
{
    PCODE pCode = MethodDescPtr->GetNativeCode();

    if (pCode == NULL)
        return;
    
    /* Find relevant entry */
    for (jit_code_entry* jit_symbols = __jit_debug_descriptor.first_entry; jit_symbols != 0; jit_symbols = jit_symbols->next_entry)
    {
        const char* ptr = jit_symbols->symfile_addr;
        uint64_t size = jit_symbols->symfile_size;
        
        const Elf_Ehdr* pEhdr = reinterpret_cast<const Elf_Ehdr*>(ptr);
        const Elf_Shdr* pShdr = reinterpret_cast<const Elf_Shdr*>(ptr + pEhdr->e_shoff);
        ++pShdr; // bump to .text section
        if (pShdr->sh_addr == pCode)
        {
            /* Notify the debugger */
            __jit_debug_descriptor.relevant_entry = jit_symbols;
            __jit_debug_descriptor.action_flag = JIT_UNREGISTER_FN;
            __jit_debug_register_code();
            
            /* Free memory */
            delete[] ptr;
            
            /* Unlink from list */
            if (jit_symbols->prev_entry == 0)
                __jit_debug_descriptor.first_entry = jit_symbols->next_entry;
            else
                jit_symbols->prev_entry->next_entry = jit_symbols->next_entry;
            delete jit_symbols;
            break;
        }
    }
}

/* Build the DWARF .debug_line section */
bool NotifyGdb::BuildLineTable(MemBuf& buf, PCODE startAddr, TADDR codeSize, SymbolsInfo* lines, unsigned nlines)
{
    MemBuf fileTable, lineProg;
    
    /* Build file table */
    if (!BuildFileTable(fileTable, lines, nlines))
        return false;
    /* Build line info program */ 
    if (!BuildLineProg(lineProg, startAddr, codeSize, lines, nlines))
    {
        return false;
    }
    
    buf.MemSize = sizeof(DwarfLineNumHeader) + 1 + fileTable.MemSize + lineProg.MemSize;
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    
    if (buf.MemPtr == nullptr)
    {
        return false;
    }
    
    /* Fill the line info header */
    DwarfLineNumHeader* header = reinterpret_cast<DwarfLineNumHeader*>(buf.MemPtr.GetValue());
    memcpy(buf.MemPtr, &LineNumHeader, sizeof(DwarfLineNumHeader));
    header->m_length = buf.MemSize - sizeof(uint32_t);
    header->m_hdr_length = sizeof(DwarfLineNumHeader) + 1 + fileTable.MemSize - 2 * sizeof(uint32_t) - sizeof(uint16_t);
    buf.MemPtr[sizeof(DwarfLineNumHeader)] = 0; // this is for missing directory table
    /* copy file table */
    memcpy(buf.MemPtr + sizeof(DwarfLineNumHeader) + 1, fileTable.MemPtr, fileTable.MemSize);
    /* copy line program */
    memcpy(buf.MemPtr + sizeof(DwarfLineNumHeader) + 1 + fileTable.MemSize, lineProg.MemPtr, lineProg.MemSize);

    return true;
}

/* Buid the source files table for DWARF source line info */
bool NotifyGdb::BuildFileTable(MemBuf& buf, SymbolsInfo* lines, unsigned nlines)
{
    NewArrayHolder<const char*> files = nullptr;
    unsigned nfiles = 0;
    
    /* GetValue file names and replace them with indices in file table */
    files = new (nothrow) const char*[nlines];
    if (files == nullptr)
        return false;
    for (unsigned i = 0; i < nlines; ++i)
    {
        const char *filePath, *fileName;
        SplitPathname(lines[i].fileName, filePath, fileName);

        /* if this isn't first then we already added file, so adjust index */
        lines[i].fileIndex = (nfiles) ? (nfiles - 1) : (nfiles);

        bool found = false;
        for (int j = 0; j < nfiles; ++j)
        {
            if (strcmp(fileName, files[j]) == 0)
            {
                found = true;
                break;
            }
        }
        
        /* add new source file */
        if (!found)
        {
            files[nfiles++] = fileName;
        }
    }
    
    /* build file table */
    unsigned totalSize = 0;
    
    for (unsigned i = 0; i < nfiles; ++i)
    {
        totalSize += strlen(files[i]) + 1 + 3;
    }
    totalSize += 1;
    
    buf.MemSize = totalSize;
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    
    if (buf.MemPtr == nullptr)
    {
        return false;
    }
    
    /* copy collected file names */
    char *ptr = buf.MemPtr;
    for (unsigned i = 0; i < nfiles; ++i)
    {
        strcpy(ptr, files[i]);
        ptr += strlen(files[i]) + 1;
        // three LEB128 entries which we don't care
        *ptr++ = 0;
        *ptr++ = 0;
        *ptr++ = 0;
    }
    // final zero byte
    *ptr = 0;

    return true;
}

/* Command to set absolute address */
void NotifyGdb::IssueSetAddress(char*& ptr, PCODE addr)
{
    *ptr++ = 0;
    *ptr++ = ADDRESS_SIZE + 1;
    *ptr++ = DW_LNE_set_address;
    *reinterpret_cast<PCODE*>(ptr) = addr;
    ptr += ADDRESS_SIZE;
}

/* End of line program */
void NotifyGdb::IssueEndOfSequence(char*& ptr)
{
    *ptr++ = 0;
    *ptr++ = 1;
    *ptr++ = DW_LNE_end_sequence;
}

/* Command w/o parameters */
void NotifyGdb::IssueSimpleCommand(char*& ptr, uint8_t command)
{
    *ptr++ = command;
}

/* Command with one LEB128 parameter */
void NotifyGdb::IssueParamCommand(char*& ptr, uint8_t command, char* param, int param_size)
{
    *ptr++ = command;
    while (param_size-- > 0)
    {
        *ptr++ = *param++;
    }
}

/* Special command moves address, line number and issue one row to source line matrix */
void NotifyGdb::IssueSpecialCommand(char*& ptr, int8_t line_shift, uint8_t addr_shift)
{
    *ptr++ = (line_shift - DWARF_LINE_BASE) + addr_shift * DWARF_LINE_RANGE + DWARF_OPCODE_BASE;
}

/* Check to see if given shifts are fit into one byte command */
bool NotifyGdb::FitIntoSpecialOpcode(int8_t line_shift, uint8_t addr_shift)
{
    unsigned opcode = (line_shift - DWARF_LINE_BASE) + addr_shift * DWARF_LINE_RANGE + DWARF_OPCODE_BASE;
    
    return opcode < 255;
}

/* Build program for DWARF source line section */
bool NotifyGdb::BuildLineProg(MemBuf& buf, PCODE startAddr, TADDR codeSize, SymbolsInfo* lines, unsigned nlines)
{
    static char cnv_buf[16];
    
    /* reserve memory assuming worst case: one extended and one special plus advance line command for each line*/
    buf.MemSize = 3 + ADDRESS_SIZE               /* initial set address command */
                + 1                              /* set prolog end command */
                + 6                              /* set file command */
                + nlines * 6                     /* advance line commands */
                + nlines * (4 + ADDRESS_SIZE)    /* 1 extended + 1 special command */
                + 6                              /* advance PC command */
                + 3;                             /* end of sequence command */
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    char* ptr = buf.MemPtr;
  
    if (buf.MemPtr == nullptr)
        return false;
    
    /* set absolute start address */
    IssueSetAddress(ptr, startAddr);
    IssueSimpleCommand(ptr, DW_LNS_set_prologue_end);
    
    int prevLine = 1, prevAddr = 0, prevFile = 0;
    
    for (int i = 0; i < nlines; ++i)
    {
        /* different source file */
        if (lines[i].fileIndex != prevFile)
        {
            int len = Leb128Encode(static_cast<uint32_t>(lines[i].fileIndex+1), cnv_buf, sizeof(cnv_buf));
            IssueParamCommand(ptr, DW_LNS_set_file, cnv_buf, len);
            prevFile = lines[i].fileIndex;
        }
        /* too big line number shift */
        if (lines[i].lineNumber - prevLine > (DWARF_LINE_BASE + DWARF_LINE_RANGE - 1))
        {
            int len = Leb128Encode(static_cast<int32_t>(lines[i].lineNumber - prevLine), cnv_buf, sizeof(cnv_buf));
            IssueParamCommand(ptr, DW_LNS_advance_line, cnv_buf, len);
            prevLine = lines[i].lineNumber;
        }
        /* first try special opcode */
        if (FitIntoSpecialOpcode(lines[i].lineNumber - prevLine, lines[i].nativeOffset - prevAddr))
            IssueSpecialCommand(ptr, lines[i].lineNumber - prevLine, lines[i].nativeOffset - prevAddr);
        else
        {
            IssueSetAddress(ptr, startAddr + lines[i].nativeOffset);
            IssueSpecialCommand(ptr, lines[i].lineNumber - prevLine, 0);
        }
           
        prevLine = lines[i].lineNumber;
        prevAddr = lines[i].nativeOffset;
    }
    
    // Advance PC to the end of function
    if (prevAddr < codeSize) {
        int len = Leb128Encode(static_cast<uint32_t>(codeSize - prevAddr), cnv_buf, sizeof(cnv_buf));
        IssueParamCommand(ptr, DW_LNS_advance_pc, cnv_buf, len);
    }

    IssueEndOfSequence(ptr); 
    
    buf.MemSize = ptr - buf.MemPtr;
    return true;
}

/* Build the DWARF .debug_str section */
bool NotifyGdb::BuildDebugStrings(MemBuf& buf,
                                  PMT_TypeInfoMap pTypeMap,
                                  NewArrayHolder<ArgsDebugInfo>& argsDebug,
                                  unsigned int argsDebugSize,
                                  NewArrayHolder<LocalsDebugInfo>& localsDebug,
                                  unsigned int localsDebugSize)
{
    int totalLength = 0;

    /* calculate total section size */
    for (int i = 0; i < DebugStringCount; ++i)
    {
        totalLength += strlen(DebugStrings[i]) + 1;
    }

    for (int i = 0; i < argsDebugSize; ++i)
    {
        argsDebug[i].m_arg_name_offset = totalLength;
        totalLength += strlen(argsDebug[i].m_arg_name) + 1;
    }

    for (int i = 0; i < localsDebugSize; ++i)
    {

        localsDebug[i].m_var_name_offset = totalLength;
        totalLength += strlen(localsDebug[i].m_var_name) + 1;
    }

    {
        auto iter = pTypeMap->Begin();
        while (iter != pTypeMap->End())
        {
            TypeInfoBase *typeInfo = iter->Value();
            typeInfo->DumpStrings(nullptr, totalLength);
            iter++;
        }
    }

    buf.MemSize = totalLength;
    buf.MemPtr = new (nothrow) char[totalLength];
    
    if (buf.MemPtr == nullptr)
        return false;

    /* copy strings */
    char* bufPtr = buf.MemPtr;
    int offset = 0;
    for (int i = 0; i < DebugStringCount; ++i)
    {
        strcpy(bufPtr + offset, DebugStrings[i]);
        offset += strlen(DebugStrings[i]) + 1;
    }

    for (int i = 0; i < argsDebugSize; ++i)
    {
        strcpy(bufPtr + offset, argsDebug[i].m_arg_name);
        offset += strlen(argsDebug[i].m_arg_name) + 1;
    }

    for (int i = 0; i < localsDebugSize; ++i)
    {
        strcpy(bufPtr + offset, localsDebug[i].m_var_name);
        offset += strlen(localsDebug[i].m_var_name) + 1;
    }

    {
        auto iter = pTypeMap->Begin();
        while (iter != pTypeMap->End())
        {
            TypeInfoBase *typeInfo = iter->Value();
            typeInfo->DumpStrings(bufPtr, offset);
            iter++;
        }
    }

    return true;
}

/* Build the DWARF .debug_abbrev section */
bool NotifyGdb::BuildDebugAbbrev(MemBuf& buf)
{
    buf.MemPtr = new (nothrow) char[AbbrevTableSize];
    buf.MemSize = AbbrevTableSize;

    if (buf.MemPtr == nullptr)
        return false;
    
    memcpy(buf.MemPtr, AbbrevTable, AbbrevTableSize);
    return true;
}

int NotifyGdb::GetArgsAndLocalsLen(NewArrayHolder<ArgsDebugInfo>& argsDebug,
                                   unsigned int argsDebugSize,
                                   NewArrayHolder<LocalsDebugInfo>& localsDebug,
                                   unsigned int localsDebugSize)
{
    int locSize = 0;
    char tmpBuf[16];

    // Format for DWARF location expression: [expression length][operation][offset in SLEB128 encoding]
    for (int i = 0; i < argsDebugSize; i++)
    {
        locSize += 2; // First byte contains expression length, second byte contains operation (DW_OP_fbreg).
        locSize += Leb128Encode(static_cast<int32_t>(argsDebug[i].m_native_offset), tmpBuf, sizeof(tmpBuf));
    }
    for (int i = 0; i < localsDebugSize; i++)
    {
        locSize += 2;  // First byte contains expression length, second byte contains operation (DW_OP_fbreg).
        locSize += Leb128Encode(static_cast<int32_t>(localsDebug[i].m_native_offset), tmpBuf, sizeof(tmpBuf));
    }
    return locSize;
}

int NotifyGdb::GetFrameLocation(int nativeOffset, char* bufVarLoc)
{
    char cnvBuf[16] = {0};
    int len = Leb128Encode(static_cast<int32_t>(nativeOffset), cnvBuf, sizeof(cnvBuf));
    bufVarLoc[0] = len + 1;
    bufVarLoc[1] = DW_OP_fbreg;
    for (int j = 0; j < len; j++)
    {
        bufVarLoc[j + 2] = cnvBuf[j];
    }

    return len + 2;  // We add '2' because first 2 bytes contain length of expression and DW_OP_fbreg operation.
}
/* Build tge DWARF .debug_info section */
bool NotifyGdb::BuildDebugInfo(MemBuf& buf, PMT_TypeInfoMap pTypeMap, NewArrayHolder<ArgsDebugInfo> &argsDebug, unsigned int argsDebugSize,
                               NewArrayHolder<LocalsDebugInfo> &localsDebug, unsigned int localsDebugSize)
{
    int totalTypeSize = 0;
    {
        auto iter = pTypeMap->Begin();
        while (iter != pTypeMap->End())
        {
            TypeInfoBase *typeInfo = iter->Value();
            typeInfo->DumpDebugInfo(nullptr, totalTypeSize);
            iter++;
        }
    }

    int locSize = GetArgsAndLocalsLen(argsDebug, argsDebugSize, localsDebug, localsDebugSize);
    buf.MemSize = sizeof(DwarfCompUnit) + sizeof(DebugInfoCU) + sizeof(DebugInfoSub) +
                  sizeof(DebugInfoVar) * (localsDebugSize + argsDebugSize) + totalTypeSize + locSize + 2;
    buf.MemPtr = new (nothrow) char[buf.MemSize];

    if (buf.MemPtr == nullptr)
        return false;
    int offset = 0;
    /* Compile uint header */
    DwarfCompUnit* cu = reinterpret_cast<DwarfCompUnit*>(buf.MemPtr.GetValue());
    cu->m_length = buf.MemSize - sizeof(uint32_t);
    cu->m_version = 4;
    cu->m_abbrev_offset = 0;
    cu->m_addr_size = ADDRESS_SIZE;
    offset += sizeof(DwarfCompUnit);
    DebugInfoCU* diCU =
       reinterpret_cast<DebugInfoCU*>(buf.MemPtr + offset);
    memcpy(buf.MemPtr + offset, &debugInfoCU, sizeof(DebugInfoCU));
    offset += sizeof(DebugInfoCU);
    diCU->m_prod_off = 0;
    diCU->m_cu_name = strlen(DebugStrings[0]) + 1;
    debugInfoSub.m_sub_type = offset;

    {
        auto iter = pTypeMap->Begin();
        while (iter != pTypeMap->End())
        {
            TypeInfoBase *typeInfo = iter->Value();
            typeInfo->DumpDebugInfo(buf.MemPtr, offset);
            iter++;
        }
    }
    /* copy debug information */
    DebugInfoSub* diSub = reinterpret_cast<DebugInfoSub*>(buf.MemPtr + offset);
    memcpy(buf.MemPtr + offset, &debugInfoSub, sizeof(DebugInfoSub));
    diSub->m_sub_name = strlen(DebugStrings[0]) + 1 + strlen(DebugStrings[1]) + 1 + strlen(DebugStrings[2]) + 1;
    offset += sizeof(DebugInfoSub);
    NewArrayHolder<DebugInfoVar> bufVar = new (nothrow) DebugInfoVar[localsDebugSize + argsDebugSize];
    if (bufVar == nullptr)
        return false;
    char bufVarLoc[16];

    for (int i = 0; i < argsDebugSize; i++)
    {
        bufVar[i].m_var_abbrev = 6;
        bufVar[i].m_var_name = argsDebug[i].m_arg_name_offset;
        bufVar[i].m_var_file = 1;
        bufVar[i].m_var_line = 1;
        bufVar[i].m_var_type = argsDebug[i].m_arg_type->m_type_offset;
        memcpy(buf.MemPtr + offset, &bufVar[i], sizeof(DebugInfoVar));
        offset += sizeof(DebugInfoVar);
        int len = GetFrameLocation(argsDebug[i].m_native_offset, bufVarLoc);
        memcpy(buf.MemPtr + offset, bufVarLoc, len);
        offset += len;
    }

    for (int i = argsDebugSize; i < (localsDebugSize + argsDebugSize); i++)
    {
        bufVar[i].m_var_abbrev = 5;
        bufVar[i].m_var_name = localsDebug[i-argsDebugSize].m_var_name_offset;
        bufVar[i].m_var_file = 1;
        bufVar[i].m_var_line = 1;
        bufVar[i].m_var_type = localsDebug[i-argsDebugSize].m_var_type->m_type_offset;
        memcpy(buf.MemPtr + offset, &bufVar[i], sizeof(DebugInfoVar));
        offset += sizeof(DebugInfoVar);
        int len = GetFrameLocation(localsDebug[i-argsDebugSize].m_native_offset, bufVarLoc);
        memcpy(buf.MemPtr + offset, bufVarLoc, len);
        offset += len;
    }

    memset(buf.MemPtr + offset, 0, buf.MemSize - offset);
    return true;
}

/* Build the DWARF lookup section */
bool NotifyGdb::BuildDebugPub(MemBuf& buf, const char* name, uint32_t size, uint32_t die_offset)
{
    uint32_t length = sizeof(DwarfPubHeader) + sizeof(uint32_t) + strlen(name) + 1 + sizeof(uint32_t);
    
    buf.MemSize = length;
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    
    if (buf.MemPtr == nullptr)
        return false;

    DwarfPubHeader* header = reinterpret_cast<DwarfPubHeader*>(buf.MemPtr.GetValue());
    header->m_length = length - sizeof(uint32_t);
    header->m_version = 2;
    header->m_debug_info_off = 0;
    header->m_debug_info_len = size;
    *reinterpret_cast<uint32_t*>(buf.MemPtr + sizeof(DwarfPubHeader)) = die_offset;
    strcpy(buf.MemPtr + sizeof(DwarfPubHeader) + sizeof(uint32_t), name);
    *reinterpret_cast<uint32_t*>(buf.MemPtr + length - sizeof(uint32_t)) = 0;
    
    return true;
}

/* Store addresses and names of the called methods into symbol table */
bool NotifyGdb::CollectCalledMethods(CalledMethod* pCalledMethods)
{
    int calledCount = 0;
    CalledMethod* pList = pCalledMethods;

    MinCallAddr = (TADDR)~0; MaxCallAddr = (TADDR)0;

    /* count called methods and find min & max addresses */
    while (pList != NULL)
    {
        calledCount++;
        TADDR callAddr = (TADDR)pList->GetCallAddr();
#if defined(_TARGET_ARM_)
        callAddr &= ~1;
#endif
        if(callAddr < MinCallAddr)
            MinCallAddr = callAddr;
        if(callAddr > MaxCallAddr)
            MaxCallAddr = callAddr;
        pList = pList->GetNext();
    }

    SymbolCount = 2 + calledCount;
    SymbolNames = new (nothrow) Elf_Symbol[SymbolCount];

    pList = pCalledMethods;
    for (int i = 2; i < SymbolCount; ++i)
    {
        char buf[256];
        MethodDesc* pMD = pList->GetMethodDesc();
        sprintf(buf, "__thunk_%s", pMD->GetName());
        SymbolNames[i].m_name = new char[strlen(buf) + 1];
        strcpy((char*)SymbolNames[i].m_name, buf);
        TADDR callAddr = (TADDR)pList->GetCallAddr();
        SymbolNames[i].m_value = callAddr - MinCallAddr;
        CalledMethod* ptr = pList;
        pList = pList->GetNext();
        delete ptr;
    }

    return true;
}

/* Build ELF .strtab section */
bool NotifyGdb::BuildStringTableSection(MemBuf& buf)
{
    int len = 0;
    for (int i = 0; i < SymbolCount; ++i)
        len += strlen(SymbolNames[i].m_name) + 1;
    len++; // end table with zero-length string
    
    buf.MemSize = len;
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    if (buf.MemPtr == nullptr)
        return false;
    char* ptr = buf.MemPtr;
    for (int i = 0; i < SymbolCount; ++i)
    {
        SymbolNames[i].m_off = ptr - buf.MemPtr;
        strcpy(ptr, SymbolNames[i].m_name);
        ptr += strlen(SymbolNames[i].m_name) + 1;
    }
    buf.MemPtr[buf.MemSize-1] = 0;
    
    return true;
}

/* Build ELF .symtab section */
bool NotifyGdb::BuildSymbolTableSection(MemBuf& buf, PCODE addr, TADDR codeSize)
{
    buf.MemSize = SymbolCount * sizeof(Elf_Sym);
    buf.MemPtr = new (nothrow) char[buf.MemSize];
    if (buf.MemPtr == nullptr)
        return false;

    Elf_Sym *sym = reinterpret_cast<Elf_Sym*>(buf.MemPtr.GetValue());

    sym[0].st_name = 0;
    sym[0].st_info = 0;
    sym[0].st_other = 0;
    sym[0].st_value = 0;
    sym[0].st_size = 0;
    sym[0].st_shndx = SHN_UNDEF;
    
    sym[1].st_name = SymbolNames[1].m_off;
    sym[1].setBindingAndType(STB_GLOBAL, STT_FUNC);
    sym[1].st_other = 0;
#ifdef _TARGET_ARM_
    sym[1].st_value = 1; // for THUMB code
#else    
    sym[1].st_value = 0;
#endif    
    sym[1].st_shndx = 1; // .text section index
    sym[1].st_size = codeSize;

    for (int i = 2; i < SymbolCount; ++i)
    {
        sym[i].st_name = SymbolNames[i].m_off;
        sym[i].setBindingAndType(STB_GLOBAL, STT_FUNC);
        sym[i].st_other = 0;
        sym[i].st_shndx = 11; // .thunks section index
        sym[i].st_size = 8;
        sym[i].st_value = SymbolNames[i].m_value;
    }
    return true;
}

/* Build ELF string section */
bool NotifyGdb::BuildSectionNameTable(MemBuf& buf)
{
    uint32_t totalLength = 0;
    
    /* calculate total size */
    for (int i = 0; i < SectionNamesCount; ++i)
    {
        totalLength += strlen(SectionNames[i]) + 1;
    }

    buf.MemSize = totalLength;
    buf.MemPtr = new (nothrow) char[totalLength];
    if (buf.MemPtr == nullptr)
        return false;

    /* copy strings */
    char* bufPtr = buf.MemPtr;
    for (int i = 0; i < SectionNamesCount; ++i)
    {
        strcpy(bufPtr, SectionNames[i]);
        bufPtr += strlen(SectionNames[i]) + 1;
    }
    
    return true;
}

/* Build the ELF section headers table */
bool NotifyGdb::BuildSectionTable(MemBuf& buf)
{
    NewArrayHolder<Elf_Shdr> sectionHeaders = new (nothrow) Elf_Shdr[SectionNamesCount - 1];    
    Elf_Shdr* pSh = sectionHeaders;

    if (sectionHeaders == nullptr)
    {
        return false;
    }
    
    /* NULL entry */
    pSh->sh_name = 0;
    pSh->sh_type = SHT_NULL;
    pSh->sh_flags = 0;
    pSh->sh_addr = 0;
    pSh->sh_offset = 0;
    pSh->sh_size = 0;
    pSh->sh_link = SHN_UNDEF;
    pSh->sh_info = 0;
    pSh->sh_addralign = 0;
    pSh->sh_entsize = 0;
    
    ++pSh;
    /* fill section header data */
    uint32_t sectNameOffset = 1;
    for (int i = 1; i < SectionNamesCount - 1; ++i, ++pSh)
    {
        pSh->sh_name = sectNameOffset;
        sectNameOffset += strlen(SectionNames[i]) + 1;
        pSh->sh_type = Sections[i].m_type;
        pSh->sh_flags = Sections[i].m_flags;
        pSh->sh_addr = 0;
        pSh->sh_offset = 0;
        pSh->sh_size = 0;
        pSh->sh_link = SHN_UNDEF;
        pSh->sh_info = 0;
        pSh->sh_addralign = 1;
        if (strcmp(SectionNames[i], ".symtab") == 0)
            pSh->sh_entsize = sizeof(Elf_Sym);
        else
            pSh->sh_entsize = 0;
    }

    sectionHeaders.SuppressRelease();
    buf.MemPtr = reinterpret_cast<char*>(sectionHeaders.GetValue());
    buf.MemSize = sizeof(Elf_Shdr) * (SectionNamesCount - 1);
    return true;
}

/* Build the ELF header */
bool NotifyGdb::BuildELFHeader(MemBuf& buf)
{
    NewHolder<Elf_Ehdr> header = new (nothrow) Elf_Ehdr;
    buf.MemPtr = reinterpret_cast<char*>(header.GetValue());
    buf.MemSize = sizeof(Elf_Ehdr);
    
    if (header == nullptr)
        return false;
    
    header.SuppressRelease();
    return true;
        
}

/* Split full path name into directory & file anmes */
void NotifyGdb::SplitPathname(const char* path, const char*& pathName, const char*& fileName)
{
    char* pSlash = strrchr(path, '/');
    
    if (pSlash != nullptr)
    {
        *pSlash = 0;
        fileName = ++pSlash;
        pathName = path;
    }
    else 
    {
        fileName = path;
        pathName = nullptr;
    }
}

/* LEB128 for 32-bit unsigned integer */
int NotifyGdb::Leb128Encode(uint32_t num, char* buf, int size)
{
    int i = 0;
    
    do
    {
        uint8_t byte = num & 0x7F;
        if (i >= size)
            break;
        num >>= 7;
        if (num != 0)
            byte |= 0x80;
        buf[i++] = byte;
    }
    while (num != 0);
    
    return i;
}

/* LEB128 for 32-bit signed integer */
int NotifyGdb::Leb128Encode(int32_t num, char* buf, int size)
{
    int i = 0;
    bool hasMore = true, isNegative = num < 0;
    
    while (hasMore && i < size)
    {
        uint8_t byte = num & 0x7F;
        num >>= 7;
        
        if ((num == 0 && (byte & 0x40) == 0) || (num  == -1 && (byte & 0x40) == 0x40))
            hasMore = false;
        else
            byte |= 0x80;
        buf[i++] = byte;
    }
    
    return i;
}

#ifdef _DEBUG
void NotifyGdb::DumpElf(const char* methodName, const MemBuf& elfFile)
{
    char dump[1024];
    strcpy(dump, "./");
    strcat(dump, methodName);
    strcat(dump, ".o");
    FILE *f = fopen(dump,  "wb");
    fwrite(elfFile.MemPtr, sizeof(char),elfFile.MemSize, f);
    fclose(f);
}
#endif

/* ELF 32bit header */
Elf32_Ehdr::Elf32_Ehdr()
{
    e_ident[EI_MAG0] = ElfMagic[0];
    e_ident[EI_MAG1] = ElfMagic[1];
    e_ident[EI_MAG2] = ElfMagic[2];
    e_ident[EI_MAG3] = ElfMagic[3];
    e_ident[EI_CLASS] = ELFCLASS32;
    e_ident[EI_DATA] = ELFDATA2LSB;
    e_ident[EI_VERSION] = EV_CURRENT;
    e_ident[EI_OSABI] = ELFOSABI_NONE;
    e_ident[EI_ABIVERSION] = 0;
    for (int i = EI_PAD; i < EI_NIDENT; ++i)
        e_ident[i] = 0;

    e_type = ET_REL;
#if defined(_TARGET_X86_)
    e_machine = EM_386;
#elif defined(_TARGET_ARM_)
    e_machine = EM_ARM;
#endif    
    e_flags = 0;
    e_version = 1;
    e_entry = 0;
    e_phoff = 0;
    e_ehsize = sizeof(Elf32_Ehdr);
    e_phentsize = 0;
    e_phnum = 0;
}

/* ELF 64bit header */
Elf64_Ehdr::Elf64_Ehdr()
{
    e_ident[EI_MAG0] = ElfMagic[0];
    e_ident[EI_MAG1] = ElfMagic[1];
    e_ident[EI_MAG2] = ElfMagic[2];
    e_ident[EI_MAG3] = ElfMagic[3];
    e_ident[EI_CLASS] = ELFCLASS64;
    e_ident[EI_DATA] = ELFDATA2LSB;
    e_ident[EI_VERSION] = EV_CURRENT;
    e_ident[EI_OSABI] = ELFOSABI_NONE;
    e_ident[EI_ABIVERSION] = 0;
    for (int i = EI_PAD; i < EI_NIDENT; ++i)
        e_ident[i] = 0;

    e_type = ET_REL;
#if defined(_TARGET_AMD64_)
    e_machine = EM_X86_64;
#elif defined(_TARGET_ARM64_)
    e_machine = EM_AARCH64;
#endif
    e_flags = 0;
    e_version = 1;
    e_entry = 0;
    e_phoff = 0;
    e_ehsize = sizeof(Elf64_Ehdr);
    e_phentsize = 0;
    e_phnum = 0;
}
