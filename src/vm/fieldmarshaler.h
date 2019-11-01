// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
//
// File: FieldMarshaler.h
//


#ifndef __FieldMarshaler_h__
#define __FieldMarshaler_h__

#include "util.hpp"
#include "mlinfo.h"
#include "eeconfig.h"
#include "olevariant.h"

#ifdef FEATURE_PREJIT
#include "compile.h"
#endif // FEATURE_PREJIT

// Forward references
class EEClassLayoutInfo;
class FieldDesc;
class MethodTable;

//=======================================================================
// Magic number for default struct packing size.
//
// Currently we set this to the packing size of the largest supported
// fundamental type and let the field marshaller downsize where needed.
//=======================================================================
#define DEFAULT_PACKING_SIZE 32

//=======================================================================
// This structure contains information about where a field is placed in a structure, as well as it's size and alignment.
// It is used as part of type-loading to determine native layout and (where applicable) managed sequential layout.
//=======================================================================
struct RawFieldPlacementInfo
{
    UINT32 m_offset;
    UINT32 m_size;
    UINT32 m_alignment;
};

enum class ParseNativeTypeFlags : int
{
    None    = 0x00,
    IsAnsi  = 0x01,
    IsWinRT = 0x02,
};

//=======================================================================
// This function returns TRUE if the type passed in is either a value class or a class and if it has layout information 
// and is marshalable. In all other cases it will return FALSE. 
//=======================================================================
BOOL IsStructMarshalable(TypeHandle th);

bool IsFieldBlittable(
    Module* pModule,
    mdFieldDef fd,
    SigPointer fieldSig,
    const SigTypeContext* pTypeContext,
    ParseNativeTypeFlags flags
);

// Describes specific categories of native fields.
enum class NativeFieldCategory : short
{
    // The native representation of the field is a floating point field.
    FLOAT,
    // The field has a nested MethodTable* (i.e. a field of a struct, class, or array)
    NESTED,
    // The native representation of the field can be treated as an integer.
    INTEGER,
    // The field is illegal to marshal.
    ILLEGAL
};

class NativeFieldDescriptor
{
public:
    NativeFieldDescriptor();

    NativeFieldDescriptor(NativeFieldCategory flags, ULONG nativeSize, ULONG alignment);

    NativeFieldDescriptor(PTR_MethodTable pMT, int numElements = 1);

    NativeFieldDescriptor(const NativeFieldDescriptor& other);

    NativeFieldDescriptor& operator=(const NativeFieldDescriptor& other);

    ~NativeFieldDescriptor() = default;

#if defined(FEATURE_PREJIT) && !defined(DACCESS_COMPILE)
    void Save(DataImage * image) const
    {
        STANDARD_VM_CONTRACT;
    }

    void Fixup(DataImage * image)
    {
        STANDARD_VM_CONTRACT;

        image->FixupFieldDescPointer(this, &m_pFD);

        if (m_isNestedType)
        {
            image->FixupMethodTablePointer(this, &m_pNestedType);
        }
    }
#endif // defined(FEATURE_PREJIT) && !defined(DACCESS_COMPILE)

#ifdef _DEBUG
    BOOL IsRestored() const;
#endif

    void Restore();

    NativeFieldCategory GetNativeFieldFlags() const
    {
        return m_flags;
    }

    PTR_MethodTable GetNestedNativeMethodTable() const;

    ULONG GetNumElements() const
    {
        CONTRACTL
        {
            PRECONDITION(m_isNestedType);
        }
        CONTRACTL_END;

        return m_numElements;
    }

    UINT32 NativeSize() const
    {
        if (m_isNestedType)
        {
            MethodTable* pMT = GetNestedNativeMethodTable();
            return pMT->GetNativeSize() * GetNumElements();
        }
        else
        {
            return m_nativeSize;
        }
    }

    UINT32 AlignmentRequirement() const;

    PTR_FieldDesc GetFieldDesc() const;

    void SetFieldDesc(PTR_FieldDesc pFD);

    UINT32 GetExternalOffset() const
    {
        return m_offset;
    }

    void SetExternalOffset(UINT32 offset)
    {
        m_offset = offset;
    }

    BOOL IsUnmarshalable() const
    {
        return m_flags == NativeFieldCategory::ILLEGAL ? TRUE : FALSE;
    }

private:
    RelativeFixupPointer<PTR_FieldDesc> m_pFD;
    union
    {
        struct
        {
            RelativeFixupPointer<PTR_MethodTable> m_pNestedType;
            ULONG m_numElements;
        };
        struct
        {
            UINT32 m_nativeSize;
            UINT32 m_alignmentRequirement;
        };
    };
    UINT32 m_offset;
    NativeFieldCategory m_flags;
    bool m_isNestedType;
};

VOID ParseNativeType(Module* pModule,
    SigPointer                  sig,
    mdFieldDef                  fd,
    ParseNativeTypeFlags        flags,
    NativeFieldDescriptor* pNFD,
    const SigTypeContext* pTypeContext
#ifdef _DEBUG
    ,
    LPCUTF8                     szNamespace,
    LPCUTF8                     szClassName,
    LPCUTF8                     szFieldName
#endif
);

//=======================================================================
// The classloader stores an intermediate representation of the layout
// metadata in an array of these structures. The dual-pass nature
// is a bit extra overhead but building this structure requiring loading
// other classes (for nested structures) and I'd rather keep this
// next to the other places where we load other classes (e.g. the superclass
// and implemented interfaces.)
//
// Each redirected field gets one entry in LayoutRawFieldInfo.
// The array is terminated by one dummy record whose m_MD == mdMemberDefNil.
//=======================================================================
struct LayoutRawFieldInfo
{
    mdFieldDef  m_MD;             // mdMemberDefNil for end of array
    ULONG       m_sequence;       // sequence # from metadata
    RawFieldPlacementInfo m_placement;
    NativeFieldDescriptor m_nfd;
};


class EEClassNativeLayoutInfo
{
private:
    uint8_t m_alignmentRequirement;
#ifdef UNIX_AMD64_ABI
    bool m_passInRegisters;
#endif
#ifdef FEATURE_HFA
    enum class HFAType : uint8_t
    {
        Unset,
        R4,
        R8,
        R16
    };
    HFAType m_hfaType;
#endif
    uint32_t m_size;
    uint32_t m_numFields;

    // An array of NativeFieldDescriptors off the end of this object, used to drive call-time
    // marshaling of NStruct reference parameters. The number of elements
    // equals m_numFields.
    NativeFieldDescriptor m_nativeFieldDescriptors[0];

    static void CollectNativeLayoutFieldMetadataThrowing(MethodTable* pMT, PTR_EEClassNativeLayoutInfo* pNativeLayoutInfoOut);
public:
    static void InitializeNativeLayoutFieldMetadataThrowing(MethodTable* pMT);

    uint32_t GetSize() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_size;
    }

    uint8_t GetLargestAlignmentRequirement() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_alignmentRequirement;
    }

    uint32_t GetNumFields() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_numFields;
    }

    NativeFieldDescriptor * GetNativeFieldDescriptors()
    {
        LIMITED_METHOD_CONTRACT;
        return &m_nativeFieldDescriptors[0];
    }

    NativeFieldDescriptor const* GetNativeFieldDescriptors() const
    {
        LIMITED_METHOD_CONTRACT;
        return &m_nativeFieldDescriptors[0];
    }
    
    CorElementType GetNativeHFATypeRaw() const;

#ifdef FEATURE_HFA
    bool IsNativeHFA() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_hfaType != HFAType::Unset;
    }

    CorElementType GetNativeHFAType() const
    {
        LIMITED_METHOD_CONTRACT;
        switch (m_hfaType)
        {
        case HFAType::R4:
            return ELEMENT_TYPE_R4;
        case HFAType::R8:
            return ELEMENT_TYPE_R8;
        case HFAType::R16:
            return ELEMENT_TYPE_VALUETYPE;
        default:
            return ELEMENT_TYPE_END;
        }
    }

    void SetHFAType(CorElementType hfaType)
    {
        LIMITED_METHOD_CONTRACT;
        // We should call this at most once.
        _ASSERTE(m_hfaType == HFAType::Unset);
        switch (hfaType)
        {
        case ELEMENT_TYPE_R4: m_hfaType = HFAType::R4; break;
        case ELEMENT_TYPE_R8: m_hfaType = HFAType::R8; break;
        case ELEMENT_TYPE_VALUETYPE: m_hfaType = HFAType::R16; break;
        default: _ASSERTE(!"Invalid HFA Type");
        }
    }
#else
    bool IsNativeHFA() const
    {
        return GetNativeHFATypeRaw() != ELEMENT_TYPE_END;
    }
    CorElementType GetNativeHFAType() const
    {
        return GetNativeHFATypeRaw();
    }
#endif

#ifdef UNIX_AMD64_ABI
    bool IsNativeStructPassedInRegisters() const
    {
        LIMITED_METHOD_CONTRACT;
        return m_passInRegisters;
    }
    void SetNativeStructPassedInRegisters()
    {
        LIMITED_METHOD_CONTRACT;
        m_passInRegisters = true;
    }
#else
    bool IsNativeStructPassedInRegisters() const
    {
        return false;
    }
#endif
};

inline CorElementType EEClassNativeLayoutInfo::GetNativeHFATypeRaw() const
{
    uint32_t numReferenceFields = GetNumFields();

    CorElementType hfaType = ELEMENT_TYPE_END;

#ifndef DACCESS_COMPILE
    const NativeFieldDescriptor* pNativeFieldDescriptorsBegin = GetNativeFieldDescriptors();
    const NativeFieldDescriptor* pNativeFieldDescriptorsEnd = pNativeFieldDescriptorsBegin + numReferenceFields;
    for (const NativeFieldDescriptor* pCurrNFD = pNativeFieldDescriptorsBegin; pCurrNFD < pNativeFieldDescriptorsEnd; ++pCurrNFD)
    {
        CorElementType fieldType = ELEMENT_TYPE_END;

        NativeFieldCategory category = pCurrNFD->GetNativeFieldFlags();

        if (category == NativeFieldCategory::FLOAT)
        {
            if (pCurrNFD->NativeSize() == 4)
            {
                fieldType = ELEMENT_TYPE_R4;
            }
            else if (pCurrNFD->NativeSize() == 8)
            {
                fieldType = ELEMENT_TYPE_R8;
            }
            else
            {
                UNREACHABLE_MSG("Invalid NativeFieldCategory.");
                fieldType = ELEMENT_TYPE_END;
            }

            // An HFA can only have aligned float and double fields.
            if (pCurrNFD->GetExternalOffset() % pCurrNFD->AlignmentRequirement() != 0)
            {
                fieldType = ELEMENT_TYPE_END;
            }
        }
        else if (category == NativeFieldCategory::NESTED)
        {
            fieldType = pCurrNFD->GetNestedNativeMethodTable()->GetNativeHFAType();
        }
        else
        {
            return ELEMENT_TYPE_END;
        }

        // Field type should be a valid HFA type.
        if (fieldType == ELEMENT_TYPE_END)
        {
            return ELEMENT_TYPE_END;
        }

        // Initialize with a valid HFA type.
        if (hfaType == ELEMENT_TYPE_END)
        {
            hfaType = fieldType;
        }
        // All field types should be equal.
        else if (fieldType != hfaType)
        {
            return ELEMENT_TYPE_END;
        }
    }

    if (hfaType == ELEMENT_TYPE_END)
        return ELEMENT_TYPE_END;

    int elemSize = 1;
    switch (hfaType)
    {
    case ELEMENT_TYPE_R4: elemSize = sizeof(float); break;
    case ELEMENT_TYPE_R8: elemSize = sizeof(double); break;
#ifdef _TARGET_ARM64_
    case ELEMENT_TYPE_VALUETYPE: elemSize = 16; break;
#endif
    default: _ASSERTE(!"Invalid HFA Type");
    }

    // Note that we check the total size, but do not perform any checks on number of fields:
    // - Type of fields can be HFA valuetype itself
    // - Managed C++ HFA valuetypes have just one <alignment member> of type float to signal that 
    //   the valuetype is HFA and explicitly specified size

    DWORD totalSize = GetSize();

    if (totalSize % elemSize != 0)
        return ELEMENT_TYPE_END;

    // On ARM, HFAs can have a maximum of four fields regardless of whether those are float or double.
    if (totalSize / elemSize > 4)
        return ELEMENT_TYPE_END;

#endif // !DACCESS_COMPILE

    return hfaType;
}

#endif // __FieldMarshaler_h__
