#ifndef LAYOUT_H
#define LAYOUT_H

#include "jit.h"

class ClassLayout
{
    const CORINFO_CLASS_HANDLE m_classHandle;
    const unsigned             m_size;

    unsigned m_isValueClass : 1;
    INDEBUG(unsigned m_gcPtrsInitialized : 1;)
    unsigned m_gcPtrCount : 30;

    union {
        BYTE* m_gcPtrs;
        BYTE  m_gcPtrsArray[sizeof(BYTE*)];
    };

#ifdef _TARGET_AMD64_
    ClassLayout* m_pppQuirkLayout;
#endif

    INDEBUG(const char* m_className;)

public:
    ClassLayout(unsigned size)
        : m_classHandle(NO_CLASS_HANDLE)
        , m_size(size)
        , m_isValueClass(false)
#ifdef DEBUG
        , m_gcPtrsInitialized(true)
#endif
        , m_gcPtrCount(0)
        , m_gcPtrs(nullptr)
#ifdef _TARGET_AMD64_
        , m_pppQuirkLayout(nullptr)
#endif
#ifdef DEBUG
        , m_className("block")
#endif
    {
    }

    ClassLayout(CORINFO_CLASS_HANDLE classHandle, bool isValueClass, unsigned size DEBUGARG(const char* className))
        : m_classHandle(classHandle)
        , m_size(size)
        , m_isValueClass(isValueClass)
#ifdef DEBUG
        , m_gcPtrsInitialized(false)
#endif
        , m_gcPtrCount(0)
        , m_gcPtrs(nullptr)
#ifdef _TARGET_AMD64_
        , m_pppQuirkLayout(nullptr)
#endif
#ifdef DEBUG
        , m_className(className)
#endif
    {
        assert(size != 0);
    }

    CORINFO_CLASS_HANDLE GetClassHandle() const
    {
        return m_classHandle;
    }

    bool IsBlockLayout() const
    {
        return m_classHandle == NO_CLASS_HANDLE;
    }

#ifdef DEBUG
    const char* GetClassName() const
    {
        return m_className;
    }
#endif

    bool IsValueClass() const
    {
        assert(!IsBlockLayout());

        return m_isValueClass;
    }

    unsigned GetSize() const
    {
        return m_size;
    }

    unsigned GetSlotCount() const
    {
        return roundUp(m_size, TARGET_POINTER_SIZE) / TARGET_POINTER_SIZE;
    }

    unsigned GetGCPtrCount() const
    {
        assert(m_gcPtrsInitialized);

        return m_gcPtrCount;
    }

    bool HasGCPtr() const
    {
        assert(m_gcPtrsInitialized);

        return m_gcPtrCount != 0;
    }

private:
    const BYTE* GetGCPtrs() const
    {
        assert(m_gcPtrsInitialized);
        assert(!IsBlockLayout());

        return (GetSlotCount() > sizeof(m_gcPtrsArray)) ? m_gcPtrs : m_gcPtrsArray;
    }

    CorInfoGCType GetGCPtr(unsigned slot) const
    {
        assert(m_gcPtrsInitialized);
        assert(slot < GetSlotCount());

        if (m_gcPtrCount == 0)
        {
            return TYPE_GC_NONE;
        }

        return static_cast<CorInfoGCType>(GetGCPtrs()[slot]);
    }

public:
    bool IsGCPtr(unsigned slot) const
    {
        return GetGCPtr(slot) != TYPE_GC_NONE;
    }

    var_types GetGCPtrType(unsigned slot) const
    {
        switch (GetGCPtr(slot))
        {
            case TYPE_GC_NONE:
                return TYP_I_IMPL;
            case TYPE_GC_REF:
                return TYP_REF;
            case TYPE_GC_BYREF:
                return TYP_BYREF;
            default:
                unreached();
        }
    }

    void InitializeGCPtrs(Compiler* compiler);

#ifdef _TARGET_AMD64_
    ClassLayout* GetPPPQuirkLayout(CompAllocator alloc);
#endif // _TARGET_AMD64_
};

#endif // LAYOUT_H
