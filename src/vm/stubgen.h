// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: StubGen.h
// 

//


#ifndef __STUBGEN_H__
#define __STUBGEN_H__

#include "stublink.h"

struct LocalDesc
{
    const static size_t MAX_LOCALDESC_ELEMENTS = 8;
    
    BYTE    ElementType[MAX_LOCALDESC_ELEMENTS];
    size_t  cbType;
    TypeHandle InternalToken;  // only valid with ELEMENT_TYPE_INTERNAL

    // used only for E_T_FNPTR and E_T_ARRAY
    PCCOR_SIGNATURE pSig;
    union
    {
        Module*         pSigModule;
        size_t          cbArrayBoundsInfo;
        BOOL            bIsCopyConstructed; // used for E_T_PTR
    };

    LocalDesc()
    {
    }

    inline LocalDesc(CorElementType elemType)
    {
        ElementType[0]     = static_cast<BYTE>(elemType);
        cbType             = 1;
        bIsCopyConstructed = FALSE;
    }

    inline LocalDesc(TypeHandle thType)
    {
        ElementType[0]     = ELEMENT_TYPE_INTERNAL;
        cbType             = 1;
        InternalToken      = thType;
        bIsCopyConstructed = FALSE;
    }

    inline LocalDesc(MethodTable *pMT)
    {
        WRAPPER_NO_CONTRACT;
        ElementType[0]     = ELEMENT_TYPE_INTERNAL;
        cbType             = 1;
        InternalToken      = TypeHandle(pMT);
        bIsCopyConstructed = FALSE;
    }

    void MakeByRef()
    {
        ChangeType(ELEMENT_TYPE_BYREF);
    }

    void MakePinned()
    {
        ChangeType(ELEMENT_TYPE_PINNED);
    }

    // makes the LocalDesc semantically equivalent to ET_TYPE_CMOD_REQD<IsCopyConstructed>/ET_TYPE_CMOD_REQD<NeedsCopyConstructorModifier>
    void MakeCopyConstructedPointer()
    {
        ChangeType(ELEMENT_TYPE_PTR);
        bIsCopyConstructed = TRUE;
    }

    void ChangeType(CorElementType elemType)
    {
        LIMITED_METHOD_CONTRACT;
        PREFIX_ASSUME((MAX_LOCALDESC_ELEMENTS-1) >= cbType);
        
        for (size_t i = cbType; i >= 1; i--)
        {
            ElementType[i]  = ElementType[i-1];
        }
        
        ElementType[0]  = static_cast<BYTE>(elemType);
        cbType          += 1;
    }

    bool IsValueClass()
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_ANY;
            PRECONDITION(cbType == 1);    // this only works on 1-element types for now
        }
        CONTRACTL_END;
        
        if (ElementType[0] == ELEMENT_TYPE_VALUETYPE)
        {
            return true;
        }
        else if ((ElementType[0] == ELEMENT_TYPE_INTERNAL) &&
                    (InternalToken.IsNativeValueType() ||
                     InternalToken.GetMethodTable()->IsValueType()))
        {
            return true;
        }

        return false;
    }
};

class StubSigBuilder
{
public:
    StubSigBuilder();

    DWORD   Append(LocalDesc* pLoc);

protected:
    CQuickBytes     m_qbSigBuffer;
    DWORD           m_nItems;
    BYTE*           m_pbSigCursor;
    size_t          m_cbSig;

    enum Constants { INITIAL_BUFFER_SIZE  = 256 };

    void EnsureEnoughQuickBytes(size_t cbToAppend);
};

//---------------------------------------------------------------------------------------
// 
class LocalSigBuilder : protected StubSigBuilder
{
public:
    DWORD NewLocal(LocalDesc * pLoc)
    {
        CONTRACTL
        {
            THROWS;
            GC_TRIGGERS;
            MODE_ANY;
            PRECONDITION(CheckPointer(pLoc));
        }
        CONTRACTL_END;
        
        return Append(pLoc);
    }

    DWORD GetSigSize();
    DWORD GetSig(BYTE * pbSig, DWORD cbBuffer);

};  // class LocalSigBuilder

//---------------------------------------------------------------------------------------
// 
class FunctionSigBuilder : protected StubSigBuilder
{
public:
    FunctionSigBuilder();

    DWORD NewArg(LocalDesc * pArg)
    {
        WRAPPER_NO_CONTRACT;
        
        return Append(pArg);
    }

    DWORD GetNumArgs()
    {
        LIMITED_METHOD_CONTRACT;
        return m_nItems;
    }

    void SetCallingConv(CorCallingConvention callingConv)
    {
        LIMITED_METHOD_CONTRACT;
        m_callingConv = callingConv;
    }

    CorCallingConvention GetCallingConv()
    {
        LIMITED_METHOD_CONTRACT;
        return m_callingConv;
    }

    void SetSig(PCCOR_SIGNATURE pSig, DWORD cSig);

    DWORD GetSigSize();
    DWORD GetSig(BYTE * pbSig, DWORD cbBuffer);

    void SetReturnType(LocalDesc* pLoc);

    CorElementType GetReturnElementType()
    {
        LIMITED_METHOD_CONTRACT;

        CONSISTENCY_CHECK(m_qbReturnSig.Size() > 0);
        return *(CorElementType *)m_qbReturnSig.Ptr();
    }

    PCCOR_SIGNATURE GetReturnSig()
    {
        LIMITED_METHOD_CONTRACT;

        CONSISTENCY_CHECK(m_qbReturnSig.Size() > 0);
        return (PCCOR_SIGNATURE)m_qbReturnSig.Ptr();
    }

protected:
    CorCallingConvention m_callingConv;
    CQuickBytes          m_qbReturnSig;
};  // class FunctionSigBuilder

#ifdef _DEBUG
// exercise the resize code
#define TOKEN_LOOKUP_MAP_SIZE  (8*sizeof(void*))
#else // _DEBUG
#define TOKEN_LOOKUP_MAP_SIZE  (64*sizeof(void*))
#endif // _DEBUG

//---------------------------------------------------------------------------------------
// 
class TokenLookupMap
{
public:    
    TokenLookupMap()
    {
        STANDARD_VM_CONTRACT;
        
        m_qbEntries.AllocThrows(TOKEN_LOOKUP_MAP_SIZE);
        m_nextAvailableRid = 0;
    }

    // copy ctor
    TokenLookupMap(TokenLookupMap* pSrc)
    {
        STANDARD_VM_CONTRACT;

        m_nextAvailableRid = pSrc->m_nextAvailableRid;
        size_t size = pSrc->m_qbEntries.Size();
        m_qbEntries.AllocThrows(size);
        memcpy(m_qbEntries.Ptr(), pSrc->m_qbEntries.Ptr(), size);
    }
    
    TypeHandle LookupTypeDef(mdToken token)
    {
        WRAPPER_NO_CONTRACT;
        return LookupTokenWorker<mdtTypeDef, MethodTable*>(token);
    }
    MethodDesc* LookupMethodDef(mdToken token)
    {
        WRAPPER_NO_CONTRACT;
        return LookupTokenWorker<mdtMethodDef, MethodDesc*>(token);
    }
    FieldDesc* LookupFieldDef(mdToken token)
    {
        WRAPPER_NO_CONTRACT;
        return LookupTokenWorker<mdtFieldDef, FieldDesc*>(token);
    }

    mdToken GetToken(TypeHandle pMT)
    {
        WRAPPER_NO_CONTRACT;
        return GetTokenWorker<mdtTypeDef, TypeHandle>(pMT);
    }
    mdToken GetToken(MethodDesc* pMD)
    {
        WRAPPER_NO_CONTRACT;
        return GetTokenWorker<mdtMethodDef, MethodDesc*>(pMD);
    }
    mdToken GetToken(FieldDesc* pFieldDesc)
    {
        WRAPPER_NO_CONTRACT;
        return GetTokenWorker<mdtFieldDef, FieldDesc*>(pFieldDesc);
    }

protected:
    template<mdToken TokenType, typename HandleType>
    HandleType LookupTokenWorker(mdToken token)
    {
        CONTRACTL
        {
            THROWS;
            MODE_ANY;
            GC_NOTRIGGER;
            PRECONDITION(RidFromToken(token)-1 < m_nextAvailableRid);
            PRECONDITION(RidFromToken(token) != 0);
            PRECONDITION(TypeFromToken(token) == TokenType);
        }
        CONTRACTL_END;
        
        return ((HandleType*)m_qbEntries.Ptr())[RidFromToken(token)-1];
    }

    template<mdToken TokenType, typename HandleType>
    mdToken GetTokenWorker(HandleType handle)
    {
        CONTRACTL
        {
            THROWS;
            MODE_ANY;
            GC_NOTRIGGER;
            PRECONDITION(handle != NULL);
        }
        CONTRACTL_END;
        
        if (m_qbEntries.Size() <= (sizeof(handle) * m_nextAvailableRid))
        {
            m_qbEntries.ReSizeThrows(2 * m_qbEntries.Size());
        }

        mdToken token = TokenFromRid(m_nextAvailableRid++, TokenType)+1;
        
        ((HandleType*)m_qbEntries.Ptr())[RidFromToken(token)-1] = handle;

        return token;
    }
    
    unsigned int                                    m_nextAvailableRid;
    CQuickBytesSpecifySize<TOKEN_LOOKUP_MAP_SIZE>   m_qbEntries;
};

struct ILStubEHClause
{
    enum Kind { kNone, kTypedCatch, kFinally };

    DWORD kind;
    DWORD dwTryBeginOffset;
    DWORD cbTryLength;
    DWORD dwHandlerBeginOffset;
    DWORD cbHandlerLength;
    DWORD dwTypeToken;
};


class ILCodeLabel;
class ILCodeStream;
//---------------------------------------------------------------------------------------
// 
class ILStubLinker
{
    friend class ILCodeLabel;
    friend class ILCodeStream;
    
public:

    ILStubLinker(Module* pModule, const Signature &signature, SigTypeContext *pTypeContext, MethodDesc *pMD,
                 BOOL fTargetHasThis, BOOL fStubHasThis, BOOL fIsNDirectStub = FALSE);
    ~ILStubLinker();
    
    void GenerateCode(BYTE* pbBuffer, size_t cbBufferSize);
    void ClearCode();

protected:

    void DeleteCodeLabels();
    void DeleteCodeStreams();

    struct ILInstruction
    {
        UINT16      uInstruction;
        INT16       iStackDelta;
        UINT_PTR    uArg;
    };

    static void PatchInstructionArgument(ILCodeLabel* pLabel, UINT_PTR uNewArg
        DEBUG_ARG(UINT16 uExpectedInstruction));

#ifdef _DEBUG
    bool IsInCodeStreamList(ILCodeStream* pcs);
#endif // _DEBUG

public:

    void    SetHasThis (bool fHasThis);
    bool    HasThis () { LIMITED_METHOD_CONTRACT; return m_fHasThis; }

    DWORD GetLocalSigSize();
    DWORD GetLocalSig(BYTE * pbLocalSig, DWORD cbBuffer);

    DWORD GetStubTargetMethodSigSize();
    DWORD GetStubTargetMethodSig(BYTE * pbLocalSig, DWORD cbBuffer);

    void SetStubTargetMethodSig(PCCOR_SIGNATURE pSig, DWORD cSig);

    void GetStubTargetReturnType(LocalDesc * pLoc);
    void GetStubTargetReturnType(LocalDesc * pLoc, Module * pModule);

    void GetStubArgType(LocalDesc * pLoc);
    void GetStubArgType(LocalDesc * pLoc, Module * pModule);
    void GetStubReturnType(LocalDesc * pLoc);
    void GetStubReturnType(LocalDesc * pLoc, Module * pModule);
    CorCallingConvention GetStubTargetCallingConv();

    
    CorElementType GetStubTargetReturnElementType() { WRAPPER_NO_CONTRACT; return m_nativeFnSigBuilder.GetReturnElementType(); }

    static void GetManagedTypeHelper(LocalDesc* pLoc, Module* pModule, PCCOR_SIGNATURE pSig, SigTypeContext *pTypeContext, MethodDesc *pMD);

    BOOL StubHasVoidReturnType();

    Stub *Link(LoaderHeap *pHeap, UINT *pcbSize /* = NULL*/, BOOL fMC);

    size_t  Link(UINT* puMaxStack);


    TokenLookupMap* GetTokenLookupMap() { LIMITED_METHOD_CONTRACT; return &m_tokenMap; }

    enum CodeStreamType
    {
        kSetup,
        kMarshal,
        kDispatch,
        kReturnUnmarshal,
        kUnmarshal,
        kExceptionCleanup,
        kCleanup,
        kExceptionHandler,
    };
    
    ILCodeStream* NewCodeStream(CodeStreamType codeStreamType);

    MethodDesc *GetTargetMD() { LIMITED_METHOD_CONTRACT; return m_pMD; }
    Signature GetStubSignature() { LIMITED_METHOD_CONTRACT; return m_stubSig; }

    void ClearCodeStreams();

    void LogILStub(DWORD dwJitFlags, SString *pDumpILStubCode = NULL);
protected:
    void LogILStubWorker(ILInstruction* pInstrBuffer, UINT numInstr, size_t* pcbCode, INT* piCurStack, SString *pDumpILStubCode = NULL);
    void LogILInstruction(size_t curOffset, bool isLabeled, INT iCurStack, ILInstruction* pInstruction, SString *pDumpILStubCode = NULL);
    
private:
    ILCodeStream*       m_pCodeStreamList;
    
    TokenLookupMap      m_tokenMap;
    LocalSigBuilder     m_localSigBuilder;
    FunctionSigBuilder  m_nativeFnSigBuilder;
    BYTE                m_rgbBuffer[sizeof(COR_ILMETHOD_DECODER)];

    Signature       m_stubSig;      // managed sig of stub
    SigTypeContext* m_pTypeContext; // type context for m_stubSig

    SigPointer      m_managedSigPtr;
    void*           m_pCode;
    Module*         m_pStubSigModule;
    ILCodeLabel*    m_pLabelList;

    bool    FirstPassLink(ILInstruction* pInstrBuffer, UINT numInstr, size_t* pcbCode, INT* piCurStack, UINT* puMaxStack);
    void    SecondPassLink(ILInstruction* pInstrBuffer, UINT numInstr, size_t* pCurCodeOffset);

    BYTE*   GenerateCodeWorker(BYTE* pbBuffer, ILInstruction* pInstrBuffer, UINT numInstr, size_t* pcbCode);

    static ILCodeStream* FindLastCodeStream(ILCodeStream* pList);

protected:
    //
    // the public entrypoints for these methods are in ILCodeStream
    //
    ILCodeLabel* NewCodeLabel();
    int GetToken(MethodDesc* pMD);
    int GetToken(MethodTable* pMT);
    int GetToken(TypeHandle th);
    int GetToken(FieldDesc* pFD);
    DWORD NewLocal(CorElementType typ = ELEMENT_TYPE_I);
    DWORD NewLocal(LocalDesc loc);

    DWORD SetStubTargetArgType(CorElementType typ, bool fConsumeStubArg = true);
    DWORD SetStubTargetArgType(LocalDesc* pLoc = NULL, bool fConsumeStubArg = true);       // passing pLoc = NULL means "use stub arg type"
    void SetStubTargetReturnType(CorElementType typ);
    void SetStubTargetReturnType(LocalDesc* pLoc);
    void SetStubTargetCallingConv(CorCallingConvention uNativeCallingConv);

    void TransformArgForJIT(LocalDesc *pLoc);

    Module * GetStubSigModule();
    SigTypeContext *GetStubSigTypeContext();

    BOOL    m_StubHasVoidReturnType;
    INT     m_iTargetStackDelta;
    DWORD   m_cbCurrentCompressedSigLen;
    DWORD   m_nLocals;

    bool    m_fHasThis;

    // We need this MethodDesc so we can reconstruct the generics
    // SigTypeContext info, if needed.
    MethodDesc * m_pMD;
};  // class ILStubLinker


//---------------------------------------------------------------------------------------
// 
class ILCodeLabel
{
    friend class ILStubLinker;
    friend class ILCodeStream;
    
public:
    ILCodeLabel();
    ~ILCodeLabel();

    size_t GetCodeOffset();

private:
    void SetCodeOffset(size_t codeOffset);

    ILCodeLabel*  m_pNext;
    ILStubLinker* m_pOwningStubLinker;
    ILCodeStream* m_pCodeStreamOfLabel;         // this is the ILCodeStream that the index is relative to
    size_t        m_codeOffset;                 // this is the absolute resolved IL offset after linking
    UINT          m_idxLabeledInstruction;      // this is the index within the instruction buffer of the owning ILCodeStream
};

class ILCodeStream
{
    friend class ILStubLinker;

public:
    enum ILInstrEnum
    {
#define OPDEF(name,string,pop,push,oprType,opcType,l,s1,s2,ctrl) \
        name,
    
#include "opcode.def"
#undef OPDEF
    };

private:
    static ILInstrEnum LowerOpcode(ILInstrEnum instr, ILStubLinker::ILInstruction* pInstr);

#ifdef _DEBUG
    static bool IsSupportedInstruction(ILInstrEnum instr);
#endif // _DEBUG

    static bool IsBranchInstruction(ILInstrEnum instr)
    {
        LIMITED_METHOD_CONTRACT;
        return ((instr >= CEE_BR) && (instr <= CEE_BLT_UN)) || (instr == CEE_LEAVE);
    }


public:
    void EmitADD        ();
    void EmitADD_OVF    ();
    void EmitAND        ();
    void EmitARGLIST    ();
    void EmitBEQ        (ILCodeLabel* pCodeLabel);
    void EmitBGE        (ILCodeLabel* pCodeLabel);
    void EmitBGE_UN(ILCodeLabel* pCodeLabel);
    void EmitBGT        (ILCodeLabel* pCodeLabel);
    void EmitBLE        (ILCodeLabel* pCodeLabel);
    void EmitBLE_UN     (ILCodeLabel* pCodeLabel);
    void EmitBLT        (ILCodeLabel* pCodeLabel);
    void EmitBR         (ILCodeLabel* pCodeLabel);
    void EmitBREAK      ();
    void EmitBRFALSE    (ILCodeLabel* pCodeLabel);
    void EmitBRTRUE     (ILCodeLabel* pCodeLabel);
    void EmitCALL       (int token, int numInArgs, int numRetArgs);
    void EmitCALLI      (int token, int numInArgs, int numRetArgs);
    void EmitCEQ        ();
    void EmitCGT        ();
    void EmitCGT_UN     ();
    void EmitCLT        ();
    void EmitCLT_UN     ();
    void EmitCONV_I     ();
    void EmitCONV_I1    ();
    void EmitCONV_I2    ();
    void EmitCONV_I4    ();
    void EmitCONV_I8    ();
    void EmitCONV_U     ();
    void EmitCONV_U1    ();
    void EmitCONV_U2    ();
    void EmitCONV_U4    ();
    void EmitCONV_U8    ();
    void EmitCONV_R4    ();
    void EmitCONV_R8    ();
    void EmitCONV_OVF_I4();
    void EmitCONV_T     (CorElementType type);
    void EmitCPBLK      ();
    void EmitCPOBJ      (int token); 
    void EmitDUP        ();
    void EmitENDFINALLY ();
    void EmitINITBLK    ();
    void EmitINITOBJ    (int token);
    void EmitJMP        (int token);
    void EmitLDARG      (unsigned uArgIdx);
    void EmitLDARGA     (unsigned uArgIdx);
    void EmitLDC        (DWORD_PTR uConst);
    void EmitLDC_R4     (UINT32 uConst);
    void EmitLDC_R8     (UINT64 uConst);
    void EmitLDELEM_REF ();
    void EmitLDFLD      (int token);
    void EmitLDFLDA     (int token);
    void EmitLDFTN      (int token);
    void EmitLDIND_I    ();
    void EmitLDIND_I1   ();
    void EmitLDIND_I2   ();
    void EmitLDIND_I4   ();
    void EmitLDIND_I8   ();
    void EmitLDIND_R4   ();
    void EmitLDIND_R8   ();
    void EmitLDIND_REF  ();
    void EmitLDIND_T    (LocalDesc* pType);
    void EmitLDIND_U1   ();
    void EmitLDIND_U2   ();
    void EmitLDIND_U4   ();
    void EmitLDLEN      ();
    void EmitLDLOC      (DWORD dwLocalNum);
    void EmitLDLOCA     (DWORD dwLocalNum);
    void EmitLDNULL     ();
    void EmitLDOBJ      (int token);
    void EmitLDSFLD     (int token);
    void EmitLDSFLDA    (int token);
    void EmitLDTOKEN    (int token);
    void EmitLEAVE      (ILCodeLabel* pCodeLabel);
    void EmitLOCALLOC   ();
    void EmitMUL        ();
    void EmitMUL_OVF    ();
    void EmitNEWOBJ     (int token, int numInArgs);
    void EmitNOP        (LPCSTR pszNopComment);
    void EmitPOP        ();
    void EmitRET        ();
    void EmitSHR_UN     ();
    void EmitSTARG      (unsigned uArgIdx);
    void EmitSTELEM_REF ();
    void EmitSTIND_I    ();
    void EmitSTIND_I1   ();
    void EmitSTIND_I2   ();
    void EmitSTIND_I4   ();
    void EmitSTIND_I8   ();
    void EmitSTIND_R4   ();
    void EmitSTIND_R8   ();
    void EmitSTIND_REF  ();
    void EmitSTIND_T    (LocalDesc* pType);
    void EmitSTFLD      (int token);
    void EmitSTLOC      (DWORD dwLocalNum);
    void EmitSTOBJ      (int token);
    void EmitSTSFLD     (int token);
    void EmitSUB        ();
    void EmitTHROW      ();

    // Overloads to simplify common usage patterns
    void EmitNEWOBJ     (BinderMethodID id, int numInArgs);
    void EmitCALL       (BinderMethodID id, int numInArgs, int numRetArgs);

    void EmitLabel(ILCodeLabel* pLabel);
    void EmitLoadThis ();
    void EmitLoadNullPtr();
    void EmitArgIteratorCreateAndLoad();

    ILCodeLabel* NewCodeLabel();

    void ClearCode();

    //
    // these functions just forward to the owning ILStubLinker
    //

    int GetToken(MethodDesc* pMD);
    int GetToken(MethodTable* pMT);
    int GetToken(TypeHandle th);
    int GetToken(FieldDesc* pFD);

    DWORD NewLocal(CorElementType typ = ELEMENT_TYPE_I);
    DWORD NewLocal(LocalDesc loc);
    DWORD SetStubTargetArgType(CorElementType typ, bool fConsumeStubArg = true);
    DWORD SetStubTargetArgType(LocalDesc* pLoc = NULL, bool fConsumeStubArg = true);       // passing pLoc = NULL means "use stub arg type"
    void SetStubTargetReturnType(CorElementType typ);
    void SetStubTargetReturnType(LocalDesc* pLoc);


    //
    // ctors/dtor
    //

    ILCodeStream(ILStubLinker* pOwner, ILStubLinker::CodeStreamType codeStreamType) : 
        m_pNextStream(NULL),
        m_pOwner(pOwner),
        m_pqbILInstructions(NULL),
        m_uCurInstrIdx(0),
        m_codeStreamType(codeStreamType)        
    {
    }

    ~ILCodeStream()
    {
        CONTRACTL
        {
            MODE_ANY;
            NOTHROW;
            GC_TRIGGERS;
        }
        CONTRACTL_END;
        
        if (NULL != m_pqbILInstructions)
        {
            delete m_pqbILInstructions;
            m_pqbILInstructions = NULL;
        }
    }

    ILStubLinker::CodeStreamType GetStreamType() { return m_codeStreamType; }
    
    LPCSTR GetStreamDescription(ILStubLinker::CodeStreamType streamType);
    
protected:

    void Emit(ILInstrEnum instr, INT16 iStackDelta, UINT_PTR uArg);

    enum Constants 
    { 
        INITIAL_NUM_IL_INSTRUCTIONS = 64,
        INITIAL_IL_INSTRUCTION_BUFFER_SIZE = INITIAL_NUM_IL_INSTRUCTIONS * sizeof(ILStubLinker::ILInstruction),
    };

    typedef CQuickBytesSpecifySize<INITIAL_IL_INSTRUCTION_BUFFER_SIZE> ILCodeStreamBuffer;

    ILCodeStream*       m_pNextStream;
    ILStubLinker*       m_pOwner;
    ILCodeStreamBuffer* m_pqbILInstructions;
    UINT                m_uCurInstrIdx;
    ILStubLinker::CodeStreamType      m_codeStreamType;       // Type of the ILCodeStream

#ifndef _WIN64
    const static UINT32 SPECIAL_VALUE_NAN_64_ON_32 = 0xFFFFFFFF;
#endif // _WIN64
};

#define TOKEN_ILSTUB_TARGET_SIG (TokenFromRid(0xFFFFFF, mdtSignature))

#endif  // __STUBGEN_H__
