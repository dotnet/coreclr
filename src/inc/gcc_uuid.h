#ifdef __cplusplus

typedef struct _GUID {          // size is 16
    unsigned int    Data1;
    unsigned short  Data2;
    unsigned short  Data3;
    unsigned char   Data4[8];
} GUID;

#define GUID_DEFINED

typedef GUID IID;

class TypeToIID
{
public:
    // Default version of the GetIID function for types where the type was not bound to IID using the macro below
    template<typename TInterface>
    static IID GetIID();
};


#define BIND_UUID_OF(T) struct T; template<> IID TypeToIID::GetIID<T>() { return IID_##T; }
#define __uuidof(T) TypeToIID::GetIID<T>()
#define DECLSPEC_UUID(x)

#endif

