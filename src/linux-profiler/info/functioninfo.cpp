#include <array>

#include "profiler.h"
#include "functioninfo.h"

static void AppendTypeArgName(
    ULONG argIndex,
    BOOL methodFormalArg,
    FunctionInfo::String &className)
{
    char argStart = methodFormalArg ? 'M' : 'T';
    if (argIndex <= 6)
    {
        // The first 7 parameters are printed as M, N, O, P, Q, R, S
        // or as T, U, V, W, X, Y, Z.
        className.append(1, argStart + argIndex);
    }
    else
    {
        // Everything after that as M7, M8, ... or T7, T8, ...
        std::array<WCHAR, 4> argName;
        _snwprintf(
            argName.data(), argName.size(), W("%c%u"), argStart, argIndex);
        className.append(argName.data());
    }
}

HRESULT FunctionInfo::GetClassName(
    const Profiler &profiler,
    IMetaDataImport *pMDImport,
    mdTypeDef classToken,
    String &className) noexcept
{
    _ASSERTE(pMDImport != nullptr);
    _ASSERTE(TypeFromToken(classToken) == mdtTypeDef);

    HRESULT hr;

    try
    {
        if (classToken == mdTypeDefNil)
        {
            className.clear();
            return S_OK;
        }

        ULONG classNameSize;
        DWORD dwTypeDefFlags;
        std::vector<WCHAR> classNameBuffer;
        hr = pMDImport->GetTypeDefProps(
            /* [in]  type token  */ classToken,
            /* [out] name buffer */ nullptr,
            /* [in]  buffer size */ 0,
            /* [out] name length */ &classNameSize,
            /* [out] type flags  */ &dwTypeDefFlags,
            /* [out] base type   */ nullptr
        );
        if (SUCCEEDED(hr))
        {
            classNameBuffer.resize(classNameSize);
            // classNameBuffer.data() can be used safety now.
            hr = pMDImport->GetTypeDefProps(
                /* [in]  type token  */ classToken,
                /* [out] name buffer */ classNameBuffer.data(),
                /* [in]  buffer size */ classNameSize,
                /* [out] name length */ &classNameSize,
                /* [out] type flags  */ nullptr,
                /* [out] base type   */ nullptr
            );
        }
        if (FAILED(hr))
        {
            throw HresultException(
                "FunctionInfo::GetClassName(): GetTypeDefProps()", hr
            );
        }

        if (IsTdNested(dwTypeDefFlags))
        {
            mdTypeDef enclosingClass;
            hr = pMDImport->GetNestedClassProps(classToken, &enclosingClass);
            if (FAILED(hr))
            {
                throw HresultException(
                    "FunctionInfo::GetClassName(): GetNestedClassProps()", hr
                );
            }
            hr = this->GetClassName(
                profiler, pMDImport, enclosingClass, className);
            if (FAILED(hr))
            {
                return hr;
            }
            className.append(1, '.').append(classNameBuffer.data());
        }
        else
        {
            className.assign(classNameBuffer.data());
        }

        String::size_type pos = className.find_last_of('`');
        if (pos != String::npos)
        {
            ULONG genericArgCount = PAL_wcstoul(
                className.data() + pos + 1, nullptr, 10);
            className.erase(pos);

            if (genericArgCount > 0)
            {
                className.append(1, '<');
                for (ULONG i = 0; i < genericArgCount; i++)
                {
                    if (i != 0)
                    {
                        className.append(1, ',');
                    }
                    AppendTypeArgName(i, false, className);
                }
                className.append(1, '>');
            }
        }
    }
    catch (...)
    {
        className = W("?");
        hr = profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT FunctionInfo::InitializeCodeInfo(const Profiler &profiler) noexcept
{
    HRESULT hr;

    try
    {
        const ProfilerInfo &info = profiler.GetProfilerInfo();

        if (info.version() >= 2)
        {
            ULONG32 size;

            hr = info.v2()->GetCodeInfo2(this->id, 0, &size, nullptr);
            if (SUCCEEDED(hr) && size > 0)
            {
                this->codeInfo.resize(size); // data() can be used safety now.
                hr = info.v2()->GetCodeInfo2(
                    this->id, size, &size, this->codeInfo.data());
            }

            if (FAILED(hr))
            {
                throw HresultException(
                    "FunctionInfo::InitializeCodeInfo(): GetCodeInfo2()",
                    hr
                );
            }

            if (!this->codeInfo.empty())
            {
                this->firstCodeInfo = this->codeInfo.front();
            }
        }
        else
        {
            LPCBYTE start;
            ULONG   size;

            hr = info.v1()->GetCodeInfo(this->id, &start, &size);
            if (FAILED(hr))
            {
                throw HresultException(
                    "FunctionInfo::InitializeCodeInfo(): "
                    "GetCodeInfo()", hr
                );
            }

            this->firstCodeInfo = {reinterpret_cast<UINT_PTR>(start), size};
            this->codeInfo.assign(1, this->firstCodeInfo);
        }
    }
    catch (...)
    {
        this->codeInfo.clear();
        this->codeInfo.shrink_to_fit();
        this->firstCodeInfo = {};
        hr = profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT FunctionInfo::InitializeILToNativeMapping(
    const Profiler &profiler) noexcept
{
    HRESULT hr;

    try
    {
        const ProfilerInfo &info = profiler.GetProfilerInfo();

        ULONG32 size;

        hr = info.v1()->GetILToNativeMapping(this->id, 0, &size, nullptr);
        if (SUCCEEDED(hr) && size > 0)
        {
            this->ILToNativeMapping.resize(size); // data() can be used safety now.
            hr = info.v1()->GetILToNativeMapping(
                this->id, size, &size, this->ILToNativeMapping.data());
        }
        if (FAILED(hr))
        {
            throw HresultException(
                "FunctionInfo::InitializeILToNativeMapping(): "
                "GetILToNativeMapping()", hr
            );
        }
    }
    catch (...)
    {
        this->ILToNativeMapping.clear();
        this->ILToNativeMapping.shrink_to_fit();
        hr = profiler.HandleException(std::current_exception());
    }

    return hr;
}

HRESULT FunctionInfo::InitializeNameAndSignature(
    const Profiler &profiler) noexcept
{
    HRESULT hr;

    try
    {
        //
        // Get Common Info.
        //

        const ProfilerInfo &info = profiler.GetProfilerInfo();

        IUnknown *pUnknown = nullptr;
        mdToken funcToken = mdTypeDefNil;

        hr = info.v1()->GetTokenAndMetaDataFromFunction(
            this->id, IID_IMetaDataImport, &pUnknown, &funcToken);
        if (FAILED(hr))
        {
            throw HresultException(
                "FunctionInfo::InitializeNameAndSignature(): "
                "GetTokenAndMetaDataFromFunction()", hr
            );
        }
        IMetaDataImport *pMDImport = dynamic_cast<IMetaDataImport*>(pUnknown);

        _ASSERTE(TypeFromToken(funcToken) == mdtMethodDef);

        ULONG funcNameSize;
        mdTypeDef classToken = mdTypeDefNil;
        DWORD funcAttr = 0;
        PCCOR_SIGNATURE sigBlob = NULL;

        hr = pMDImport->GetMethodProps(
            /* [in]  method token   */ funcToken,
            /* [out] class token    */ &classToken,
            /* [out] name buffer    */ nullptr,
            /* [in]  buffer size    */ 0,
            /* [out] name length    */ &funcNameSize,
            /* [out] method flags   */ &funcAttr,
            /* [out] signature blob */ &sigBlob,
            /* [out] size of blob   */ nullptr,
            /* [out] RVA pointer    */ nullptr,
            /* [out] impl. flags    */ nullptr
        );
        if (FAILED(hr))
        {
            throw HresultException(
                "FunctionInfo::InitializeNameAndSignature(): "
                "GetMethodProps()", hr
            );
        }

        //
        // Get Function Name.
        //

        try
        {
            std::vector<WCHAR> funcNameBuffer(funcNameSize);
            // funcNameBuffer.data() can be used safety now.
            hr = pMDImport->GetMethodProps(
                /* [in]  method token   */ funcToken,
                /* [out] class token    */ nullptr,
                /* [out] name buffer    */ funcNameBuffer.data(),
                /* [in]  buffer size    */ funcNameSize,
                /* [out] name length    */ nullptr,
                /* [out] method flags   */ nullptr,
                /* [out] signature blob */ nullptr,
                /* [out] size of blob   */ nullptr,
                /* [out] RVA pointer    */ nullptr,
                /* [out] impl. flags    */ nullptr
            );
            if (FAILED(hr))
            {
                throw HresultException(
                    "FunctionInfo::InitializeNameAndSignature(): "
                    "GetMethodProps()", hr
                );
            }
            this->name.assign(funcNameBuffer.data());
        }
        catch (...)
        {
            this->name = W("?");
            hr = profiler.HandleException(std::current_exception());
        }

        //
        // Get Class Name.
        //

        this->GetClassName(profiler, pMDImport, classToken, this->className);

        //
        // Get Signature.
        //

        // TODO: ...
    }
    catch (...)
    {
        hr = profiler.HandleException(std::current_exception());
    }

    return hr;
}
