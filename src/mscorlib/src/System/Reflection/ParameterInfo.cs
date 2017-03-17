// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.Serialization;

namespace System.Reflection
{
    public partial class ParameterInfo : ICustomAttributeProvider, IObjectReference
    {
        protected String NameImpl;
        protected Type ClassImpl;
        protected int PositionImpl;
        protected ParameterAttributes AttrsImpl;
        protected Object DefaultValueImpl; // cannot cache this as it may be non agile user defined enum
        protected MemberInfo MemberImpl;

        protected ParameterInfo()
        {
        }

        public virtual Type ParameterType
        {
            get
            {
                return ClassImpl;
            }
        }

        public virtual String Name
        {
            get
            {
                return NameImpl;
            }
        }

        public virtual bool HasDefaultValue { get { throw new NotImplementedException(); } }

        public virtual Object DefaultValue { get { throw new NotImplementedException(); } }
        public virtual Object RawDefaultValue { get { throw new NotImplementedException(); } }

        public virtual int Position { get { return PositionImpl; } }
        public virtual ParameterAttributes Attributes { get { return AttrsImpl; } }

        public virtual MemberInfo Member
        {
            get
            {
                Contract.Ensures(Contract.Result<MemberInfo>() != null);
                return MemberImpl;
            }
        }

        public bool IsIn { get { return ((Attributes & ParameterAttributes.In) != 0); } }
        public bool IsOut { get { return ((Attributes & ParameterAttributes.Out) != 0); } }
        public bool IsLcid { get { return ((Attributes & ParameterAttributes.Lcid) != 0); } }
        public bool IsRetval { get { return ((Attributes & ParameterAttributes.Retval) != 0); } }
        public bool IsOptional { get { return ((Attributes & ParameterAttributes.Optional) != 0); } }

        public virtual int MetadataToken
        {
            get
            {
                // This API was made virtual in V4. Code compiled against V2 might use
                // "call" rather than "callvirt" to call it.
                // This makes sure those code still works.
                RuntimeParameterInfo rtParam = this as RuntimeParameterInfo;
                if (rtParam != null)
                    return rtParam.MetadataToken;

                // return a null token
                return (int)MetadataTokenType.ParamDef;
            }
        }

        public virtual Type[] GetRequiredCustomModifiers()
        {
            return EmptyArray<Type>.Value;
        }

        public virtual Type[] GetOptionalCustomModifiers()
        {
            return EmptyArray<Type>.Value;
        }

        public override String ToString()
        {
            return ParameterType.FormatTypeName() + " " + Name;
        }

        public virtual IEnumerable<CustomAttributeData> CustomAttributes
        {
            get
            {
                return GetCustomAttributesData();
            }
        }

        public virtual Object[] GetCustomAttributes(bool inherit)
        {
            return EmptyArray<Object>.Value;
        }

        public virtual Object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));
            Contract.EndContractBlock();

            return EmptyArray<Object>.Value;
        }

        public virtual bool IsDefined(Type attributeType, bool inherit)
        {
            if (attributeType == null)
                throw new ArgumentNullException(nameof(attributeType));
            Contract.EndContractBlock();

            return false;
        }

        public virtual IList<CustomAttributeData> GetCustomAttributesData()
        {
            throw new NotImplementedException();
        }

        // In V4 RuntimeParameterInfo is introduced.
        // To support deserializing ParameterInfo instances serialized in earlier versions
        // we need to implement IObjectReference.
        public object GetRealObject(StreamingContext context)
        {
            Contract.Ensures(Contract.Result<Object>() != null);

            // Once all the serializable fields have come in we can set up the real
            // instance based on just two of them (MemberImpl and PositionImpl).

            if (MemberImpl == null)
                throw new SerializationException(Environment.GetResourceString(ResId.Serialization_InsufficientState));

            ParameterInfo[] args = null;

            switch (MemberImpl.MemberType)
            {
                case MemberTypes.Constructor:
                case MemberTypes.Method:
                    if (PositionImpl == -1)
                    {
                        if (MemberImpl.MemberType == MemberTypes.Method)
                            return ((MethodInfo)MemberImpl).ReturnParameter;
                        else
                            throw new SerializationException(Environment.GetResourceString(ResId.Serialization_BadParameterInfo));
                    }
                    else
                    {
                        args = ((MethodBase)MemberImpl).GetParametersNoCopy();

                        if (args != null && PositionImpl < args.Length)
                            return args[PositionImpl];
                        else
                            throw new SerializationException(Environment.GetResourceString(ResId.Serialization_BadParameterInfo));
                    }

                case MemberTypes.Property:
                    args = ((RuntimePropertyInfo)MemberImpl).GetIndexParametersNoCopy();

                    if (args != null && PositionImpl > -1 && PositionImpl < args.Length)
                        return args[PositionImpl];
                    else
                        throw new SerializationException(Environment.GetResourceString(ResId.Serialization_BadParameterInfo));

                default:
                    throw new SerializationException(Environment.GetResourceString(ResId.Serialization_NoParameterInfo));
            }
        }
    }
}
