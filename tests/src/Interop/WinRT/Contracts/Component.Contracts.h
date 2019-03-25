/* Header file automatically generated from ../Contracts/Component.Contracts.idl */
/*
 * File built with Microsoft(R) MIDLRT Compiler Engine Version 10.00.0223 
 */

#pragma warning( disable: 4049 )  /* more than 64k source lines */

/* verify that the <rpcndr.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCNDR_H_VERSION__
#define __REQUIRED_RPCNDR_H_VERSION__ 500
#endif

/* verify that the <rpcsal.h> version is high enough to compile this file*/
#ifndef __REQUIRED_RPCSAL_H_VERSION__
#define __REQUIRED_RPCSAL_H_VERSION__ 100
#endif

#include <rpc.h>
#include <rpcndr.h>

#ifndef __RPCNDR_H_VERSION__
#error this stub requires an updated version of <rpcndr.h>
#endif /* __RPCNDR_H_VERSION__ */

#ifndef COM_NO_WINDOWS_H
#include <windows.h>
#include <ole2.h>
#endif /*COM_NO_WINDOWS_H*/
#ifndef __Component2EContracts_h__
#define __Component2EContracts_h__
#ifndef __Component2EContracts_p_h__
#define __Component2EContracts_p_h__


#pragma once

// Ensure that the setting of the /ns_prefix command line switch is consistent for all headers.
// If you get an error from the compiler indicating "warning C4005: 'CHECK_NS_PREFIX_STATE': macro redefinition", this
// indicates that you have included two different headers with different settings for the /ns_prefix MIDL command line switch
#if !defined(DISABLE_NS_PREFIX_CHECKS)
#define CHECK_NS_PREFIX_STATE "always"
#endif // !defined(DISABLE_NS_PREFIX_CHECKS)


#pragma push_macro("MIDL_CONST_ID")
#undef MIDL_CONST_ID
#define MIDL_CONST_ID const __declspec(selectany)


// Header files for imported files
#include "winrtbase.h"
#include "C:\Program Files (x86)\Windows Kits\10\References\10.0.17763.0\Windows.Foundation.FoundationContract\3.0.0.0\Windows.Foundation.FoundationContract.h"
#include "C:\Program Files (x86)\Windows Kits\10\References\10.0.17763.0\Windows.Foundation.UniversalApiContract\7.0.0.0\Windows.Foundation.UniversalApiContract.h"
// Importing Collections header
#include <windows.foundation.collections.h>

#if defined(__cplusplus) && !defined(CINTERFACE)
#if defined(__MIDL_USE_C_ENUM)
#define MIDL_ENUM enum
#else
#define MIDL_ENUM enum class
#endif
/* Forward Declarations */
#ifndef ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IBooleanTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIBooleanTesting ABI::Component::Contracts::IBooleanTesting

#endif // ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IStringTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIStringTesting ABI::Component::Contracts::IStringTesting

#endif // ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface INullableTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CINullableTesting ABI::Component::Contracts::INullableTesting

#endif // ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface ITypeTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CITypeTesting ABI::Component::Contracts::ITypeTesting

#endif // ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IExceptionTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIExceptionTesting ABI::Component::Contracts::IExceptionTesting

#endif // ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IKeyValuePairTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting ABI::Component::Contracts::IKeyValuePairTesting

#endif // ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IUriTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIUriTesting ABI::Component::Contracts::IUriTesting

#endif // ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IArrayTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIArrayTesting ABI::Component::Contracts::IArrayTesting

#endif // ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IBindingViewModel;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIBindingViewModel ABI::Component::Contracts::IBindingViewModel

#endif // ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__
namespace ABI {
    namespace Component {
        namespace Contracts {
            interface IBindingProjectionsTesting;
        } /* Component */
    } /* Contracts */} /* ABI */
#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting ABI::Component::Contracts::IBindingProjectionsTesting

#endif // ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__

// Parameterized interface forward declarations (C++)

// Collection interface definitions

#ifndef DEF___FIReference_1_int_USE
#define DEF___FIReference_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation {
template <>
struct __declspec(uuid("548cefbd-bc8a-5fa0-8df2-957440fc8bf4"))
IReference<int> : IReference_impl<int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.IReference`1<Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IReference<int> __FIReference_1_int_t;
#define __FIReference_1_int ABI::Windows::Foundation::__FIReference_1_int_t
/* ABI */ } /* Windows */ } /* Foundation */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIReference_1_int ABI::Windows::Foundation::IReference<int>
//#define __FIReference_1_int_t ABI::Windows::Foundation::IReference<int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIReference_1_int_USE */




#ifndef DEF___FIKeyValuePair_2_int_int_USE
#define DEF___FIKeyValuePair_2_int_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("efbc9aaa-f960-5801-b4d1-2ed931b91ff9"))
IKeyValuePair<int,int> : IKeyValuePair_impl<int,int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IKeyValuePair`2<Int32, Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IKeyValuePair<int,int> __FIKeyValuePair_2_int_int_t;
#define __FIKeyValuePair_2_int_int ABI::Windows::Foundation::Collections::__FIKeyValuePair_2_int_int_t
/* ABI */ } /* Windows */ } /* Foundation */ } /* Collections */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIKeyValuePair_2_int_int ABI::Windows::Foundation::Collections::IKeyValuePair<int,int>
//#define __FIKeyValuePair_2_int_int_t ABI::Windows::Foundation::Collections::IKeyValuePair<int,int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIKeyValuePair_2_int_int_USE */




#ifndef DEF___FIKeyValuePair_2_HSTRING_HSTRING_USE
#define DEF___FIKeyValuePair_2_HSTRING_HSTRING_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("60310303-49c5-52e6-abc6-a9b36eccc716"))
IKeyValuePair<HSTRING,HSTRING> : IKeyValuePair_impl<HSTRING,HSTRING> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IKeyValuePair`2<String, String>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IKeyValuePair<HSTRING,HSTRING> __FIKeyValuePair_2_HSTRING_HSTRING_t;
#define __FIKeyValuePair_2_HSTRING_HSTRING ABI::Windows::Foundation::Collections::__FIKeyValuePair_2_HSTRING_HSTRING_t
/* ABI */ } /* Windows */ } /* Foundation */ } /* Collections */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIKeyValuePair_2_HSTRING_HSTRING ABI::Windows::Foundation::Collections::IKeyValuePair<HSTRING,HSTRING>
//#define __FIKeyValuePair_2_HSTRING_HSTRING_t ABI::Windows::Foundation::Collections::IKeyValuePair<HSTRING,HSTRING>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIKeyValuePair_2_HSTRING_HSTRING_USE */




#ifndef DEF___FIIterator_1_int_USE
#define DEF___FIIterator_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("bfea7f78-50c2-5f1d-a6ea-9e978d2699ff"))
IIterator<int> : IIterator_impl<int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterator`1<Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterator<int> __FIIterator_1_int_t;
#define __FIIterator_1_int ABI::Windows::Foundation::Collections::__FIIterator_1_int_t
/* ABI */ } /* Windows */ } /* Foundation */ } /* Collections */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterator_1_int ABI::Windows::Foundation::Collections::IIterator<int>
//#define __FIIterator_1_int_t ABI::Windows::Foundation::Collections::IIterator<int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterator_1_int_USE */




#ifndef DEF___FIIterable_1_int_USE
#define DEF___FIIterable_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("81a643fb-f51c-5565-83c4-f96425777b66"))
IIterable<int> : IIterable_impl<int> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IIterable`1<Int32>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IIterable<int> __FIIterable_1_int_t;
#define __FIIterable_1_int ABI::Windows::Foundation::Collections::__FIIterable_1_int_t
/* ABI */ } /* Windows */ } /* Foundation */ } /* Collections */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIIterable_1_int ABI::Windows::Foundation::Collections::IIterable<int>
//#define __FIIterable_1_int_t ABI::Windows::Foundation::Collections::IIterable<int>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIIterable_1_int_USE */





#ifndef DEF___FIKeyValuePair_2_int___FIIterable_1_int_USE
#define DEF___FIKeyValuePair_2_int___FIIterable_1_int_USE
#if !defined(RO_NO_TEMPLATE_NAME)
namespace ABI { namespace Windows { namespace Foundation { namespace Collections {
template <>
struct __declspec(uuid("84ecd21a-839b-5521-bb45-34e708426f00"))
IKeyValuePair<int,__FIIterable_1_int*> : IKeyValuePair_impl<int,__FIIterable_1_int*> 
{
    static const wchar_t* z_get_rc_name_impl() 
    {
        return L"Windows.Foundation.Collections.IKeyValuePair`2<Int32, Windows.Foundation.Collections.IIterable`1<Int32>>"; 
    }
};
// Define a typedef for the parameterized interface specialization's mangled name.
// This allows code which uses the mangled name for the parameterized interface to access the
// correct parameterized interface specialization.
typedef IKeyValuePair<int,__FIIterable_1_int*> __FIKeyValuePair_2_int___FIIterable_1_int_t;
#define __FIKeyValuePair_2_int___FIIterable_1_int ABI::Windows::Foundation::Collections::__FIKeyValuePair_2_int___FIIterable_1_int_t
/* ABI */ } /* Windows */ } /* Foundation */ } /* Collections */ }

////  Define an alias for the C version of the interface for compatibility purposes.
//#define __FIKeyValuePair_2_int___FIIterable_1_int ABI::Windows::Foundation::Collections::IKeyValuePair<int,ABI::Windows::Foundation::Collections::IIterable<int>*>
//#define __FIKeyValuePair_2_int___FIIterable_1_int_t ABI::Windows::Foundation::Collections::IKeyValuePair<int,ABI::Windows::Foundation::Collections::IIterable<int>*>
#endif // !defined(RO_NO_TEMPLATE_NAME)
#endif /* DEF___FIKeyValuePair_2_int___FIIterable_1_int_USE */





namespace ABI {
    namespace Component {
        namespace Contracts {
            class BooleanTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IBooleanTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.BooleanTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBooleanTesting[] = L"Component.Contracts.IBooleanTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("971af13a-9793-4af7-b2f2-72d829195592"), version, object, exclusiveto] */
            MIDL_INTERFACE("971af13a-9793-4af7-b2f2-72d829195592")
            IBooleanTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE And(
                    /* [in] */boolean left,
                    /* [in] */boolean right,
                    /* [out, retval] */boolean * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IBooleanTesting=_uuidof(IBooleanTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBooleanTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BooleanTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBooleanTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BooleanTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_BooleanTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BooleanTesting[] = L"Component.Contracts.BooleanTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class StringTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IStringTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.StringTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IStringTesting[] = L"Component.Contracts.IStringTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("c6f1f632-47b6-4a52-86d2-a89807ed2677"), version, object, exclusiveto] */
            MIDL_INTERFACE("c6f1f632-47b6-4a52-86d2-a89807ed2677")
            IStringTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE ConcatStrings(
                    /* [in] */HSTRING left,
                    /* [in] */HSTRING right,
                    /* [out, retval] */HSTRING * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IStringTesting=_uuidof(IStringTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIStringTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.StringTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IStringTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_StringTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_StringTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_StringTesting[] = L"Component.Contracts.StringTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class NullableTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.INullableTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.NullableTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_INullableTesting[] = L"Component.Contracts.INullableTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("939d4ee5-8d41-4c4b-8948-86017ceb9244"), version, object, exclusiveto] */
            MIDL_INTERFACE("939d4ee5-8d41-4c4b-8948-86017ceb9244")
            INullableTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE IsNull(
                    /* [in] */__FIReference_1_int * value,
                    /* [out, retval] */boolean * result
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE GetIntValue(
                    /* [in] */__FIReference_1_int * value,
                    /* [out, retval] */int * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_INullableTesting=_uuidof(INullableTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CINullableTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.NullableTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.INullableTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_NullableTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_NullableTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_NullableTesting[] = L"Component.Contracts.NullableTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class TypeTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.ITypeTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.TypeTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_ITypeTesting[] = L"Component.Contracts.ITypeTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("bb545a14-9ae7-491a-874d-1c03d239fb70"), version, object, exclusiveto] */
            MIDL_INTERFACE("bb545a14-9ae7-491a-874d-1c03d239fb70")
            ITypeTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE GetTypeName(
                    /* [in] */struct ABI::Windows::UI::Xaml::Interop::TypeName typeName,
                    /* [out, retval] */HSTRING * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_ITypeTesting=_uuidof(ITypeTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CITypeTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.TypeTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.ITypeTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_TypeTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_TypeTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_TypeTesting[] = L"Component.Contracts.TypeTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class ExceptionTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IExceptionTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.ExceptionTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IExceptionTesting[] = L"Component.Contracts.IExceptionTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("9162201d-b591-4f30-8f41-f0f79f6ecea3"), version, object, exclusiveto] */
            MIDL_INTERFACE("9162201d-b591-4f30-8f41-f0f79f6ecea3")
            IExceptionTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE ThrowException(
                    /* [in] */struct ABI::Windows::Foundation::HResult hr
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE GetException(
                    /* [in] */int hr,
                    /* [out, retval] */struct ABI::Windows::Foundation::HResult * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IExceptionTesting=_uuidof(IExceptionTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIExceptionTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.ExceptionTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IExceptionTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_ExceptionTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_ExceptionTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_ExceptionTesting[] = L"Component.Contracts.ExceptionTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class KeyValuePairTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IKeyValuePairTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.KeyValuePairTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IKeyValuePairTesting[] = L"Component.Contracts.IKeyValuePairTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("ccd10099-3a45-4382-970d-b76f52780bcd"), version, object, exclusiveto] */
            MIDL_INTERFACE("ccd10099-3a45-4382-970d-b76f52780bcd")
            IKeyValuePairTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE MakeSimplePair(
                    /* [in] */int key,
                    /* [in] */int value,
                    /* [out, retval] */__FIKeyValuePair_2_int_int * * result
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE MakeMarshaledPair(
                    /* [in] */HSTRING key,
                    /* [in] */HSTRING value,
                    /* [out, retval] */__FIKeyValuePair_2_HSTRING_HSTRING * * result
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE MakeProjectedPair(
                    /* [in] */int key,
                    /* [in] */unsigned int valuesLength,
                    /* [size_is(valuesLength), in] */int * values,
                    /* [out, retval] */__FIKeyValuePair_2_int___FIIterable_1_int * * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IKeyValuePairTesting=_uuidof(IKeyValuePairTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIKeyValuePairTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.KeyValuePairTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IKeyValuePairTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_KeyValuePairTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_KeyValuePairTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_KeyValuePairTesting[] = L"Component.Contracts.KeyValuePairTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class UriTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IUriTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.UriTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IUriTesting[] = L"Component.Contracts.IUriTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("e0af24b3-e6c6-4e89-b8d1-a332979ef398"), version, object, exclusiveto] */
            MIDL_INTERFACE("e0af24b3-e6c6-4e89-b8d1-a332979ef398")
            IUriTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE GetFromUri(
                    /* [in] */ABI::Windows::Foundation::IUriRuntimeClass * uri,
                    /* [out, retval] */HSTRING * result
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE CreateUriFromString(
                    /* [in] */HSTRING uri,
                    /* [out, retval] */ABI::Windows::Foundation::IUriRuntimeClass * * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IUriTesting=_uuidof(IUriTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIUriTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.UriTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IUriTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_UriTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_UriTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_UriTesting[] = L"Component.Contracts.UriTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class ArrayTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IArrayTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.ArrayTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IArrayTesting[] = L"Component.Contracts.IArrayTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("821b532d-cc5e-4218-90ab-a8361ac92794"), version, object, exclusiveto] */
            MIDL_INTERFACE("821b532d-cc5e-4218-90ab-a8361ac92794")
            IArrayTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE Sum(
                    /* [in] */unsigned int arrayLength,
                    /* [size_is(arrayLength), in] */int * array,
                    /* [out, retval] */int * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IArrayTesting=_uuidof(IArrayTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIArrayTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.ArrayTesting
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IArrayTesting ** Default Interface **
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_ArrayTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_ArrayTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_ArrayTesting[] = L"Component.Contracts.ArrayTesting";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class BindingViewModel;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IBindingViewModel
 *
 * Interface is a part of the implementation of type Component.Contracts.BindingViewModel
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBindingViewModel[] = L"Component.Contracts.IBindingViewModel";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("4bb923ae-986a-4aad-9bfb-13e0b5ecffa4"), version, object, exclusiveto] */
            MIDL_INTERFACE("4bb923ae-986a-4aad-9bfb-13e0b5ecffa4")
            IBindingViewModel : public IInspectable
            {
            public:
                /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Collection(
                    /* [out, retval] */ABI::Windows::UI::Xaml::Interop::INotifyCollectionChanged * * value
                    ) = 0;
                virtual HRESULT STDMETHODCALLTYPE AddElement(
                    /* [in] */int i
                    ) = 0;
                /* [propget] */virtual HRESULT STDMETHODCALLTYPE get_Name(
                    /* [out, retval] */HSTRING * value
                    ) = 0;
                /* [propput] */virtual HRESULT STDMETHODCALLTYPE put_Name(
                    /* [in] */HSTRING value
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IBindingViewModel=_uuidof(IBindingViewModel);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBindingViewModel;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BindingViewModel
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBindingViewModel ** Default Interface **
 *    Windows.UI.Xaml.Data.INotifyPropertyChanged
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BindingViewModel_DEFINED
#define RUNTIMECLASS_Component_Contracts_BindingViewModel_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BindingViewModel[] = L"Component.Contracts.BindingViewModel";
#endif

namespace ABI {
    namespace Component {
        namespace Contracts {
            class BindingProjectionsTesting;
        } /* Component */
    } /* Contracts */} /* ABI */



/*
 *
 * Interface Component.Contracts.IBindingProjectionsTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.BindingProjectionsTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBindingProjectionsTesting[] = L"Component.Contracts.IBindingProjectionsTesting";
namespace ABI {
    namespace Component {
        namespace Contracts {
            /* [uuid("857e28e1-3e7f-4f6f-8554-efc73feba286"), version, object, exclusiveto] */
            MIDL_INTERFACE("857e28e1-3e7f-4f6f-8554-efc73feba286")
            IBindingProjectionsTesting : public IInspectable
            {
            public:
                virtual HRESULT STDMETHODCALLTYPE CreateViewModel(
                    /* [out, retval] */ABI::Component::Contracts::IBindingViewModel * * result
                    ) = 0;
                
            };

            extern MIDL_CONST_ID IID & IID_IBindingProjectionsTesting=_uuidof(IBindingProjectionsTesting);
            
        } /* Component */
    } /* Contracts */} /* ABI */

EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBindingProjectionsTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BindingProjectionsTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBindingProjectionsTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BindingProjectionsTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_BindingProjectionsTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BindingProjectionsTesting[] = L"Component.Contracts.BindingProjectionsTesting";
#endif


#else // !defined(__cplusplus)
/* Forward Declarations */
#ifndef ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIBooleanTesting __x_ABI_CComponent_CContracts_CIBooleanTesting;

#endif // ____x_ABI_CComponent_CContracts_CIBooleanTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIStringTesting __x_ABI_CComponent_CContracts_CIStringTesting;

#endif // ____x_ABI_CComponent_CContracts_CIStringTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CINullableTesting __x_ABI_CComponent_CContracts_CINullableTesting;

#endif // ____x_ABI_CComponent_CContracts_CINullableTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CITypeTesting __x_ABI_CComponent_CContracts_CITypeTesting;

#endif // ____x_ABI_CComponent_CContracts_CITypeTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIExceptionTesting __x_ABI_CComponent_CContracts_CIExceptionTesting;

#endif // ____x_ABI_CComponent_CContracts_CIExceptionTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIKeyValuePairTesting __x_ABI_CComponent_CContracts_CIKeyValuePairTesting;

#endif // ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIUriTesting __x_ABI_CComponent_CContracts_CIUriTesting;

#endif // ____x_ABI_CComponent_CContracts_CIUriTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIArrayTesting __x_ABI_CComponent_CContracts_CIArrayTesting;

#endif // ____x_ABI_CComponent_CContracts_CIArrayTesting_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIBindingViewModel __x_ABI_CComponent_CContracts_CIBindingViewModel;

#endif // ____x_ABI_CComponent_CContracts_CIBindingViewModel_FWD_DEFINED__

#ifndef ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__
#define ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__
typedef interface __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting;

#endif // ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_FWD_DEFINED__

// Parameterized interface forward declarations (C)

// Collection interface definitions
#if !defined(____FIReference_1_int_INTERFACE_DEFINED__)
#define ____FIReference_1_int_INTERFACE_DEFINED__

typedef interface __FIReference_1_int __FIReference_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIReference_1_int;

typedef struct __FIReference_1_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIReference_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )( __RPC__in __FIReference_1_int * This );
    ULONG ( STDMETHODCALLTYPE *Release )( __RPC__in __FIReference_1_int * This );

    HRESULT ( STDMETHODCALLTYPE *GetIids )( __RPC__in __FIReference_1_int * This, 
                                            /* [out] */ __RPC__out ULONG *iidCount,
                                            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )( __RPC__in __FIReference_1_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )( __RPC__in __FIReference_1_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIReference_1_int * This, /* [retval][out] */ __RPC__out int *value);
    END_INTERFACE
} __FIReference_1_intVtbl;

interface __FIReference_1_int
{
    CONST_VTBL struct __FIReference_1_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIReference_1_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 
#define __FIReference_1_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 
#define __FIReference_1_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 

#define __FIReference_1_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 
#define __FIReference_1_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 
#define __FIReference_1_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 

#define __FIReference_1_int_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIReference_1_int_INTERFACE_DEFINED__


#if !defined(____FIKeyValuePair_2_int_int_INTERFACE_DEFINED__)
#define ____FIKeyValuePair_2_int_int_INTERFACE_DEFINED__

typedef interface __FIKeyValuePair_2_int_int __FIKeyValuePair_2_int_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIKeyValuePair_2_int_int;

typedef struct __FIKeyValuePair_2_int_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIKeyValuePair_2_int_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIKeyValuePair_2_int_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIKeyValuePair_2_int_int * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIKeyValuePair_2_int_int * This,
            /* [out] */ __RPC__out ULONG *iidCount,
            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIKeyValuePair_2_int_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIKeyValuePair_2_int_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Key )(__RPC__in __FIKeyValuePair_2_int_int * This, /* [retval][out] */ __RPC__out int *key);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIKeyValuePair_2_int_int * This, /* [retval][out] */ __RPC__deref_out_opt int *value);
    END_INTERFACE
} __FIKeyValuePair_2_int_intVtbl;

interface __FIKeyValuePair_2_int_int
{
    CONST_VTBL struct __FIKeyValuePair_2_int_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIKeyValuePair_2_int_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIKeyValuePair_2_int_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIKeyValuePair_2_int_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIKeyValuePair_2_int_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIKeyValuePair_2_int_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIKeyValuePair_2_int_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIKeyValuePair_2_int_int_get_Key(This,key)	\
    ( (This)->lpVtbl -> get_Key(This,key) ) 

#define __FIKeyValuePair_2_int_int_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIKeyValuePair_2_int_int_INTERFACE_DEFINED__


#if !defined(____FIKeyValuePair_2_HSTRING_HSTRING_INTERFACE_DEFINED__)
#define ____FIKeyValuePair_2_HSTRING_HSTRING_INTERFACE_DEFINED__

typedef interface __FIKeyValuePair_2_HSTRING_HSTRING __FIKeyValuePair_2_HSTRING_HSTRING;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIKeyValuePair_2_HSTRING_HSTRING;

typedef struct __FIKeyValuePair_2_HSTRING_HSTRINGVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This,
            /* [out] */ __RPC__out ULONG *iidCount,
            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Key )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This, /* [retval][out] */ __RPC__out HSTRING *key);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIKeyValuePair_2_HSTRING_HSTRING * This, /* [retval][out] */ __RPC__deref_out_opt HSTRING *value);
    END_INTERFACE
} __FIKeyValuePair_2_HSTRING_HSTRINGVtbl;

interface __FIKeyValuePair_2_HSTRING_HSTRING
{
    CONST_VTBL struct __FIKeyValuePair_2_HSTRING_HSTRINGVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIKeyValuePair_2_HSTRING_HSTRING_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIKeyValuePair_2_HSTRING_HSTRING_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIKeyValuePair_2_HSTRING_HSTRING_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIKeyValuePair_2_HSTRING_HSTRING_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIKeyValuePair_2_HSTRING_HSTRING_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIKeyValuePair_2_HSTRING_HSTRING_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIKeyValuePair_2_HSTRING_HSTRING_get_Key(This,key)	\
    ( (This)->lpVtbl -> get_Key(This,key) ) 

#define __FIKeyValuePair_2_HSTRING_HSTRING_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIKeyValuePair_2_HSTRING_HSTRING_INTERFACE_DEFINED__


#if !defined(____FIIterator_1_int_INTERFACE_DEFINED__)
#define ____FIIterator_1_int_INTERFACE_DEFINED__

typedef interface __FIIterator_1_int __FIIterator_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterator_1_int;

typedef struct __FIIterator_1_intVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterator_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);
    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterator_1_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterator_1_int * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterator_1_int * This,
        /* [out] */ __RPC__out ULONG *iidCount,
        /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterator_1_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterator_1_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Current )(__RPC__in __FIIterator_1_int * This, /* [retval][out] */ __RPC__out int *current);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_HasCurrent )(__RPC__in __FIIterator_1_int * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *MoveNext )(__RPC__in __FIIterator_1_int * This, /* [retval][out] */ __RPC__out boolean *hasCurrent);
    HRESULT ( STDMETHODCALLTYPE *GetMany )(__RPC__in __FIIterator_1_int * This,
        /* [in] */ unsigned int capacity,
        /* [size_is][length_is][out] */ __RPC__out_ecount_part(capacity, *actual) int *items,
        /* [retval][out] */ __RPC__out unsigned int *actual);

    END_INTERFACE
} __FIIterator_1_intVtbl;

interface __FIIterator_1_int
{
    CONST_VTBL struct __FIIterator_1_intVtbl *lpVtbl;
};



#ifdef COBJMACROS


#define __FIIterator_1_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterator_1_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterator_1_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterator_1_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterator_1_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterator_1_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterator_1_int_get_Current(This,current)	\
    ( (This)->lpVtbl -> get_Current(This,current) ) 

#define __FIIterator_1_int_get_HasCurrent(This,hasCurrent)	\
    ( (This)->lpVtbl -> get_HasCurrent(This,hasCurrent) ) 

#define __FIIterator_1_int_MoveNext(This,hasCurrent)	\
    ( (This)->lpVtbl -> MoveNext(This,hasCurrent) ) 

#define __FIIterator_1_int_GetMany(This,capacity,items,actual)	\
    ( (This)->lpVtbl -> GetMany(This,capacity,items,actual) ) 

#endif /* COBJMACROS */


#endif // ____FIIterator_1_int_INTERFACE_DEFINED__


#if !defined(____FIIterable_1_int_INTERFACE_DEFINED__)
#define ____FIIterable_1_int_INTERFACE_DEFINED__

typedef interface __FIIterable_1_int __FIIterable_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIIterable_1_int;

typedef  struct __FIIterable_1_intVtbl
{
    BEGIN_INTERFACE

    HRESULT ( STDMETHODCALLTYPE *QueryInterface )( 
        __RPC__in __FIIterable_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIIterable_1_int * This);

    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIIterable_1_int * This);

    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIIterable_1_int * This,
                                           /* [out] */ __RPC__out ULONG *iidCount,
                                           /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);

    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIIterable_1_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);

    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIIterable_1_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    HRESULT ( STDMETHODCALLTYPE *First )(__RPC__in __FIIterable_1_int * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterator_1_int **first);

    END_INTERFACE
} __FIIterable_1_intVtbl;

interface __FIIterable_1_int
{
    CONST_VTBL struct __FIIterable_1_intVtbl *lpVtbl;
};

#ifdef COBJMACROS

#define __FIIterable_1_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIIterable_1_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIIterable_1_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIIterable_1_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIIterable_1_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIIterable_1_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIIterable_1_int_First(This,first)	\
    ( (This)->lpVtbl -> First(This,first) ) 

#endif /* COBJMACROS */


#endif // ____FIIterable_1_int_INTERFACE_DEFINED__



#if !defined(____FIKeyValuePair_2_int___FIIterable_1_int_INTERFACE_DEFINED__)
#define ____FIKeyValuePair_2_int___FIIterable_1_int_INTERFACE_DEFINED__

typedef interface __FIKeyValuePair_2_int___FIIterable_1_int __FIKeyValuePair_2_int___FIIterable_1_int;

//  Declare the parameterized interface IID.
EXTERN_C const IID IID___FIKeyValuePair_2_int___FIIterable_1_int;

typedef struct __FIKeyValuePair_2_int___FIIterable_1_intVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This,
        /* [in] */ __RPC__in REFIID riid,
        /* [annotation][iid_is][out] */ 
        _COM_Outptr_  void **ppvObject);

    ULONG ( STDMETHODCALLTYPE *AddRef )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This);
    ULONG ( STDMETHODCALLTYPE *Release )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This);
    HRESULT ( STDMETHODCALLTYPE *GetIids )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This,
            /* [out] */ __RPC__out ULONG *iidCount,
            /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids);
    HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This, /* [out] */ __RPC__deref_out_opt HSTRING *className);
    HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This, /* [out] */ __RPC__out TrustLevel *trustLevel);

    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Key )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This, /* [retval][out] */ __RPC__out int *key);
    /* [propget] */ HRESULT ( STDMETHODCALLTYPE *get_Value )(__RPC__in __FIKeyValuePair_2_int___FIIterable_1_int * This, /* [retval][out] */ __RPC__deref_out_opt __FIIterable_1_int * *value);
    END_INTERFACE
} __FIKeyValuePair_2_int___FIIterable_1_intVtbl;

interface __FIKeyValuePair_2_int___FIIterable_1_int
{
    CONST_VTBL struct __FIKeyValuePair_2_int___FIIterable_1_intVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __FIKeyValuePair_2_int___FIIterable_1_int_QueryInterface(This,riid,ppvObject)	\
    ( (This)->lpVtbl -> QueryInterface(This,riid,ppvObject) ) 

#define __FIKeyValuePair_2_int___FIIterable_1_int_AddRef(This)	\
    ( (This)->lpVtbl -> AddRef(This) ) 

#define __FIKeyValuePair_2_int___FIIterable_1_int_Release(This)	\
    ( (This)->lpVtbl -> Release(This) ) 


#define __FIKeyValuePair_2_int___FIIterable_1_int_GetIids(This,iidCount,iids)	\
    ( (This)->lpVtbl -> GetIids(This,iidCount,iids) ) 

#define __FIKeyValuePair_2_int___FIIterable_1_int_GetRuntimeClassName(This,className)	\
    ( (This)->lpVtbl -> GetRuntimeClassName(This,className) ) 

#define __FIKeyValuePair_2_int___FIIterable_1_int_GetTrustLevel(This,trustLevel)	\
    ( (This)->lpVtbl -> GetTrustLevel(This,trustLevel) ) 


#define __FIKeyValuePair_2_int___FIIterable_1_int_get_Key(This,key)	\
    ( (This)->lpVtbl -> get_Key(This,key) ) 

#define __FIKeyValuePair_2_int___FIIterable_1_int_get_Value(This,value)	\
    ( (This)->lpVtbl -> get_Value(This,value) ) 
#endif /* COBJMACROS */


#endif // ____FIKeyValuePair_2_int___FIIterable_1_int_INTERFACE_DEFINED__





/*
 *
 * Interface Component.Contracts.IBooleanTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.BooleanTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBooleanTesting[] = L"Component.Contracts.IBooleanTesting";
/* [uuid("971af13a-9793-4af7-b2f2-72d829195592"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIBooleanTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBooleanTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *And )(
        __x_ABI_CComponent_CContracts_CIBooleanTesting * This,
        /* [in] */boolean left,
        /* [in] */boolean right,
        /* [out, retval] */boolean * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIBooleanTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIBooleanTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIBooleanTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIBooleanTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIBooleanTesting_And(This,left,right,result) \
    ( (This)->lpVtbl->And(This,left,right,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBooleanTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBooleanTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BooleanTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBooleanTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BooleanTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_BooleanTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BooleanTesting[] = L"Component.Contracts.BooleanTesting";
#endif



/*
 *
 * Interface Component.Contracts.IStringTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.StringTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IStringTesting[] = L"Component.Contracts.IStringTesting";
/* [uuid("c6f1f632-47b6-4a52-86d2-a89807ed2677"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIStringTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIStringTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *ConcatStrings )(
        __x_ABI_CComponent_CContracts_CIStringTesting * This,
        /* [in] */HSTRING left,
        /* [in] */HSTRING right,
        /* [out, retval] */HSTRING * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIStringTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIStringTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIStringTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIStringTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIStringTesting_ConcatStrings(This,left,right,result) \
    ( (This)->lpVtbl->ConcatStrings(This,left,right,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIStringTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIStringTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.StringTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IStringTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_StringTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_StringTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_StringTesting[] = L"Component.Contracts.StringTesting";
#endif



/*
 *
 * Interface Component.Contracts.INullableTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.NullableTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_INullableTesting[] = L"Component.Contracts.INullableTesting";
/* [uuid("939d4ee5-8d41-4c4b-8948-86017ceb9244"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CINullableTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CINullableTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *IsNull )(
        __x_ABI_CComponent_CContracts_CINullableTesting * This,
        /* [in] */__FIReference_1_int * value,
        /* [out, retval] */boolean * result
        );
    HRESULT ( STDMETHODCALLTYPE *GetIntValue )(
        __x_ABI_CComponent_CContracts_CINullableTesting * This,
        /* [in] */__FIReference_1_int * value,
        /* [out, retval] */int * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CINullableTestingVtbl;

interface __x_ABI_CComponent_CContracts_CINullableTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CINullableTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CINullableTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_IsNull(This,value,result) \
    ( (This)->lpVtbl->IsNull(This,value,result) )

#define __x_ABI_CComponent_CContracts_CINullableTesting_GetIntValue(This,value,result) \
    ( (This)->lpVtbl->GetIntValue(This,value,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CINullableTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CINullableTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.NullableTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.INullableTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_NullableTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_NullableTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_NullableTesting[] = L"Component.Contracts.NullableTesting";
#endif



/*
 *
 * Interface Component.Contracts.ITypeTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.TypeTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_ITypeTesting[] = L"Component.Contracts.ITypeTesting";
/* [uuid("bb545a14-9ae7-491a-874d-1c03d239fb70"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CITypeTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CITypeTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *GetTypeName )(
        __x_ABI_CComponent_CContracts_CITypeTesting * This,
        /* [in] */struct __x_ABI_CWindows_CUI_CXaml_CInterop_CTypeName typeName,
        /* [out, retval] */HSTRING * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CITypeTestingVtbl;

interface __x_ABI_CComponent_CContracts_CITypeTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CITypeTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CITypeTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CITypeTesting_GetTypeName(This,typeName,result) \
    ( (This)->lpVtbl->GetTypeName(This,typeName,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CITypeTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CITypeTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.TypeTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.ITypeTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_TypeTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_TypeTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_TypeTesting[] = L"Component.Contracts.TypeTesting";
#endif



/*
 *
 * Interface Component.Contracts.IExceptionTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.ExceptionTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IExceptionTesting[] = L"Component.Contracts.IExceptionTesting";
/* [uuid("9162201d-b591-4f30-8f41-f0f79f6ecea3"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIExceptionTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *ThrowException )(
        __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
        /* [in] */struct __x_ABI_CWindows_CFoundation_CHResult hr
        );
    HRESULT ( STDMETHODCALLTYPE *GetException )(
        __x_ABI_CComponent_CContracts_CIExceptionTesting * This,
        /* [in] */int hr,
        /* [out, retval] */struct __x_ABI_CWindows_CFoundation_CHResult * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIExceptionTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIExceptionTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIExceptionTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIExceptionTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_ThrowException(This,hr) \
    ( (This)->lpVtbl->ThrowException(This,hr) )

#define __x_ABI_CComponent_CContracts_CIExceptionTesting_GetException(This,hr,result) \
    ( (This)->lpVtbl->GetException(This,hr,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIExceptionTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIExceptionTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.ExceptionTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IExceptionTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_ExceptionTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_ExceptionTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_ExceptionTesting[] = L"Component.Contracts.ExceptionTesting";
#endif



/*
 *
 * Interface Component.Contracts.IKeyValuePairTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.KeyValuePairTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IKeyValuePairTesting[] = L"Component.Contracts.IKeyValuePairTesting";
/* [uuid("ccd10099-3a45-4382-970d-b76f52780bcd"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIKeyValuePairTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *MakeSimplePair )(
        __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
        /* [in] */int key,
        /* [in] */int value,
        /* [out, retval] */__FIKeyValuePair_2_int_int * * result
        );
    HRESULT ( STDMETHODCALLTYPE *MakeMarshaledPair )(
        __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
        /* [in] */HSTRING key,
        /* [in] */HSTRING value,
        /* [out, retval] */__FIKeyValuePair_2_HSTRING_HSTRING * * result
        );
    HRESULT ( STDMETHODCALLTYPE *MakeProjectedPair )(
        __x_ABI_CComponent_CContracts_CIKeyValuePairTesting * This,
        /* [in] */int key,
        /* [in] */unsigned int valuesLength,
        /* [size_is(valuesLength), in] */int * values,
        /* [out, retval] */__FIKeyValuePair_2_int___FIIterable_1_int * * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIKeyValuePairTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIKeyValuePairTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIKeyValuePairTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_MakeSimplePair(This,key,value,result) \
    ( (This)->lpVtbl->MakeSimplePair(This,key,value,result) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_MakeMarshaledPair(This,key,value,result) \
    ( (This)->lpVtbl->MakeMarshaledPair(This,key,value,result) )

#define __x_ABI_CComponent_CContracts_CIKeyValuePairTesting_MakeProjectedPair(This,key,valuesLength,values,result) \
    ( (This)->lpVtbl->MakeProjectedPair(This,key,valuesLength,values,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIKeyValuePairTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIKeyValuePairTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.KeyValuePairTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IKeyValuePairTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_KeyValuePairTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_KeyValuePairTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_KeyValuePairTesting[] = L"Component.Contracts.KeyValuePairTesting";
#endif



/*
 *
 * Interface Component.Contracts.IUriTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.UriTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IUriTesting[] = L"Component.Contracts.IUriTesting";
/* [uuid("e0af24b3-e6c6-4e89-b8d1-a332979ef398"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIUriTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIUriTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *GetFromUri )(
        __x_ABI_CComponent_CContracts_CIUriTesting * This,
        /* [in] */__x_ABI_CWindows_CFoundation_CIUriRuntimeClass * uri,
        /* [out, retval] */HSTRING * result
        );
    HRESULT ( STDMETHODCALLTYPE *CreateUriFromString )(
        __x_ABI_CComponent_CContracts_CIUriTesting * This,
        /* [in] */HSTRING uri,
        /* [out, retval] */__x_ABI_CWindows_CFoundation_CIUriRuntimeClass * * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIUriTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIUriTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIUriTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIUriTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_GetFromUri(This,uri,result) \
    ( (This)->lpVtbl->GetFromUri(This,uri,result) )

#define __x_ABI_CComponent_CContracts_CIUriTesting_CreateUriFromString(This,uri,result) \
    ( (This)->lpVtbl->CreateUriFromString(This,uri,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIUriTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIUriTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.UriTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IUriTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_UriTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_UriTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_UriTesting[] = L"Component.Contracts.UriTesting";
#endif



/*
 *
 * Interface Component.Contracts.IArrayTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.ArrayTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IArrayTesting[] = L"Component.Contracts.IArrayTesting";
/* [uuid("821b532d-cc5e-4218-90ab-a8361ac92794"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIArrayTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIArrayTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *Sum )(
        __x_ABI_CComponent_CContracts_CIArrayTesting * This,
        /* [in] */unsigned int arrayLength,
        /* [size_is(arrayLength), in] */int * array,
        /* [out, retval] */int * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIArrayTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIArrayTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIArrayTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIArrayTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIArrayTesting_Sum(This,arrayLength,array,result) \
    ( (This)->lpVtbl->Sum(This,arrayLength,array,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIArrayTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIArrayTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.ArrayTesting
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IArrayTesting ** Default Interface **
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_ArrayTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_ArrayTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_ArrayTesting[] = L"Component.Contracts.ArrayTesting";
#endif



/*
 *
 * Interface Component.Contracts.IBindingViewModel
 *
 * Interface is a part of the implementation of type Component.Contracts.BindingViewModel
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBindingViewModel[] = L"Component.Contracts.IBindingViewModel";
/* [uuid("4bb923ae-986a-4aad-9bfb-13e0b5ecffa4"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIBindingViewModelVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
/* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Collection )(
        __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
        /* [out, retval] */__x_ABI_CWindows_CUI_CXaml_CInterop_CINotifyCollectionChanged * * value
        );
    HRESULT ( STDMETHODCALLTYPE *AddElement )(
        __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
        /* [in] */int i
        );
    /* [propget] */HRESULT ( STDMETHODCALLTYPE *get_Name )(
        __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
        /* [out, retval] */HSTRING * value
        );
    /* [propput] */HRESULT ( STDMETHODCALLTYPE *put_Name )(
        __x_ABI_CComponent_CContracts_CIBindingViewModel * This,
        /* [in] */HSTRING value
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIBindingViewModelVtbl;

interface __x_ABI_CComponent_CContracts_CIBindingViewModel
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIBindingViewModelVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIBindingViewModel_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_get_Collection(This,value) \
    ( (This)->lpVtbl->get_Collection(This,value) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_AddElement(This,i) \
    ( (This)->lpVtbl->AddElement(This,i) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_get_Name(This,value) \
    ( (This)->lpVtbl->get_Name(This,value) )

#define __x_ABI_CComponent_CContracts_CIBindingViewModel_put_Name(This,value) \
    ( (This)->lpVtbl->put_Name(This,value) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBindingViewModel;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBindingViewModel_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BindingViewModel
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBindingViewModel ** Default Interface **
 *    Windows.UI.Xaml.Data.INotifyPropertyChanged
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BindingViewModel_DEFINED
#define RUNTIMECLASS_Component_Contracts_BindingViewModel_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BindingViewModel[] = L"Component.Contracts.BindingViewModel";
#endif



/*
 *
 * Interface Component.Contracts.IBindingProjectionsTesting
 *
 * Interface is a part of the implementation of type Component.Contracts.BindingProjectionsTesting
 *
 *
 */
#if !defined(____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__)
#define ____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__
extern const __declspec(selectany) _Null_terminated_ WCHAR InterfaceName_Component_Contracts_IBindingProjectionsTesting[] = L"Component.Contracts.IBindingProjectionsTesting";
/* [uuid("857e28e1-3e7f-4f6f-8554-efc73feba286"), version, object, exclusiveto] */
typedef struct __x_ABI_CComponent_CContracts_CIBindingProjectionsTestingVtbl
{
    BEGIN_INTERFACE
    HRESULT ( STDMETHODCALLTYPE *QueryInterface)(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This,
    /* [in] */ __RPC__in REFIID riid,
    /* [annotation][iid_is][out] */
    _COM_Outptr_  void **ppvObject
    );

ULONG ( STDMETHODCALLTYPE *AddRef )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This
    );

ULONG ( STDMETHODCALLTYPE *Release )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This
    );

HRESULT ( STDMETHODCALLTYPE *GetIids )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This,
    /* [out] */ __RPC__out ULONG *iidCount,
    /* [size_is][size_is][out] */ __RPC__deref_out_ecount_full_opt(*iidCount) IID **iids
    );

HRESULT ( STDMETHODCALLTYPE *GetRuntimeClassName )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This,
    /* [out] */ __RPC__deref_out_opt HSTRING *className
    );

HRESULT ( STDMETHODCALLTYPE *GetTrustLevel )(
    __RPC__in __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This,
    /* [OUT ] */ __RPC__out TrustLevel *trustLevel
    );
HRESULT ( STDMETHODCALLTYPE *CreateViewModel )(
        __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting * This,
        /* [out, retval] */__x_ABI_CComponent_CContracts_CIBindingViewModel * * result
        );
    END_INTERFACE
    
} __x_ABI_CComponent_CContracts_CIBindingProjectionsTestingVtbl;

interface __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting
{
    CONST_VTBL struct __x_ABI_CComponent_CContracts_CIBindingProjectionsTestingVtbl *lpVtbl;
};

#ifdef COBJMACROS
#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_QueryInterface(This,riid,ppvObject) \
( (This)->lpVtbl->QueryInterface(This,riid,ppvObject) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_AddRef(This) \
        ( (This)->lpVtbl->AddRef(This) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_Release(This) \
        ( (This)->lpVtbl->Release(This) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_GetIids(This,iidCount,iids) \
        ( (This)->lpVtbl->GetIids(This,iidCount,iids) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_GetRuntimeClassName(This,className) \
        ( (This)->lpVtbl->GetRuntimeClassName(This,className) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_GetTrustLevel(This,trustLevel) \
        ( (This)->lpVtbl->GetTrustLevel(This,trustLevel) )

#define __x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_CreateViewModel(This,result) \
    ( (This)->lpVtbl->CreateViewModel(This,result) )


#endif /* COBJMACROS */


EXTERN_C const IID IID___x_ABI_CComponent_CContracts_CIBindingProjectionsTesting;
#endif /* !defined(____x_ABI_CComponent_CContracts_CIBindingProjectionsTesting_INTERFACE_DEFINED__) */


/*
 *
 * Class Component.Contracts.BindingProjectionsTesting
 *
 * RuntimeClass can be activated.
 *
 * Class implements the following interfaces:
 *    Component.Contracts.IBindingProjectionsTesting ** Default Interface **
 *
 * Class Threading Model:  Both Single and Multi Threaded Apartment
 *
 * Class Marshaling Behavior:  Agile - Class is agile
 *
 */

#ifndef RUNTIMECLASS_Component_Contracts_BindingProjectionsTesting_DEFINED
#define RUNTIMECLASS_Component_Contracts_BindingProjectionsTesting_DEFINED
extern const __declspec(selectany) _Null_terminated_ WCHAR RuntimeClass_Component_Contracts_BindingProjectionsTesting[] = L"Component.Contracts.BindingProjectionsTesting";
#endif


#endif // defined(__cplusplus)
#pragma pop_macro("MIDL_CONST_ID")
#endif // __Component2EContracts_p_h__

#endif // __Component2EContracts_h__
