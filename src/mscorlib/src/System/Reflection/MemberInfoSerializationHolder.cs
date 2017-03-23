// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Serialization;
using System.Globalization;
using System.Diagnostics.Contracts;

namespace System.Reflection
{
    // This class needs to be public both so it can whitelisted for Reflection and so that Reflection.Core can access it.
    [Serializable]
    public class MemberInfoSerializationHolder : ISerializable, IObjectReference
    {
        #region Staitc Public Members
        public static void GetSerializationInfo(SerializationInfo info, FieldInfo f)
        {
            // Compat: Serializing ToString() since the full framework does it but the deserialization logic makes no use of it.
            GetSerializationInfo(info, f.Name, f.ReflectedType, f.ToString(), MemberTypes.Field);
        }

        public static void GetSerializationInfo(SerializationInfo info, EventInfo e)
        {
            GetSerializationInfo(info, e.Name, e.ReflectedType, null, MemberTypes.Event);
        }

        public static void GetSerializationInfo(SerializationInfo info, ConstructorInfo c)
        {
            GetSerializationInfo(info, c.Name, c.ReflectedType, c.ToString(), c.SerializationToString(), MemberTypes.Constructor, genericArguments: null);
        }

        public static void GetSerializationInfo(SerializationInfo info, MethodInfo m)
        {
            Type[] genericArguments = m.IsConstructedGenericMethod ? m.GetGenericArguments() : null;
            GetSerializationInfo(info, m.Name, m.ReflectedType, m.ToString(), m.SerializationToString(), MemberTypes.Method, genericArguments);
        }

        public static void GetSerializationInfo(SerializationInfo info, PropertyInfo p)
        {
            GetSerializationInfo(info, p.Name, p.ReflectedType, p.ToString(), p.SerializationToString(), MemberTypes.Property, genericArguments: null);
        }
        #endregion

        #region Private Static Members
        private static void GetSerializationInfo(SerializationInfo info, string name, Type reflectedClass, string signature, MemberTypes type)
        {
            GetSerializationInfo(info, name, reflectedClass, signature, null, type, null);
        }

        private static void GetSerializationInfo(
            SerializationInfo info,
            string name,
            Type reflectedClass,
            string signature,
            string signature2,
            MemberTypes type,
            Type[] genericArguments)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            Contract.EndContractBlock();

            string assemblyName = reflectedClass.Module.Assembly.FullName;
            string typeName = reflectedClass.FullName;

            info.SetType(typeof(MemberInfoSerializationHolder));
            info.AddValue("Name", name, typeof(string));
            info.AddValue("AssemblyName", assemblyName, typeof(string));
            info.AddValue("ClassName", typeName, typeof(string));
            info.AddValue("Signature", signature, typeof(string));
            info.AddValue("Signature2", signature2, typeof(string));
            info.AddValue("MemberType", (int)type);
            info.AddValue("GenericArguments", genericArguments, typeof(Type[]));
        }
        #endregion

        #region Private Data Members
        private string m_memberName;
        private Type m_reflectedType;
        // m_signature stores the ToString() representation of the member which is sometimes ambiguous.
        // Mulitple overloads of the same methods or properties can identical ToString().
        // m_signature2 stores the SerializationToString() representation which should be unique for each member.
        // It is only written and used by post 4.0 CLR versions.
        private string m_signature;
        private string m_signature2;
        private MemberTypes m_memberType;
        private SerializationInfo m_info;
        #endregion

        #region Constructor
        // Needs to be public so it can be whitelisted in Reflection.
        public MemberInfoSerializationHolder(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));
            Contract.EndContractBlock();

            string assemblyName = info.GetString("AssemblyName");
            string typeName = info.GetString("ClassName");

            if (assemblyName == null || typeName == null)
                throw new SerializationException(SR.Serialization_InsufficientState);

            Assembly assem = Assembly.Load(assemblyName);
            m_reflectedType = assem.GetType(typeName, true, false);
            m_memberName = info.GetString("Name");
            m_signature = info.GetString("Signature");
            // Only v4.0 and later generates and consumes Signature2
            m_signature2 = (string)info.GetValueNoThrow("Signature2", typeof(string));
            m_memberType = (MemberTypes)info.GetInt32("MemberType");
            m_info = info;
        }
        #endregion

        #region ISerializable
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotSupportedException();
        }
        #endregion

        #region IObjectReference
        public virtual object GetRealObject(StreamingContext context)
        {
            if (m_memberName == null || m_reflectedType == null || m_memberType == 0)
                throw new SerializationException(SR.Serialization_InsufficientState);

            BindingFlags bindingFlags =
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Static | BindingFlags.OptionalParamBinding;

            switch (m_memberType)
            {
                #region case MemberTypes.Field:
                case MemberTypes.Field:
                    {
                        FieldInfo[] fields = m_reflectedType.GetMember(m_memberName, MemberTypes.Field, bindingFlags) as FieldInfo[];

                        if (fields.Length == 0)
                            throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));

                        return fields[0];
                    }
                #endregion

                #region case MemberTypes.Event:
                case MemberTypes.Event:
                    {
                        EventInfo[] events = m_reflectedType.GetMember(m_memberName, MemberTypes.Event, bindingFlags) as EventInfo[];

                        if (events.Length == 0)
                            throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));

                        return events[0];
                    }
                #endregion

                #region case MemberTypes.Property:
                case MemberTypes.Property:
                    {
                        PropertyInfo[] properties = m_reflectedType.GetMember(m_memberName, MemberTypes.Property, bindingFlags) as PropertyInfo[];

                        if (properties.Length == 0)
                            throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));

                        if (properties.Length == 1)
                            return properties[0];

                        if (properties.Length > 1)
                        {
                            for (int i = 0; i < properties.Length; i++)
                            {
                                if (m_signature2 != null)
                                {
                                    if (properties[i].SerializationToString().Equals(m_signature2))
                                        return properties[i];
                                }
                                else
                                {
                                    if ((properties[i]).ToString().Equals(m_signature))
                                        return properties[i];
                                }
                            }
                        }

                        throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));
                    }
                #endregion

                #region case MemberTypes.Constructor:
                case MemberTypes.Constructor:
                    {
                        if (m_signature == null)
                            throw new SerializationException(SR.Serialization_NullSignature);

                        ConstructorInfo[] constructors = m_reflectedType.GetMember(m_memberName, MemberTypes.Constructor, bindingFlags) as ConstructorInfo[];

                        if (constructors.Length == 1)
                            return constructors[0];

                        if (constructors.Length > 1)
                        {
                            for (int i = 0; i < constructors.Length; i++)
                            {
                                if (m_signature2 != null)
                                {
                                    if (constructors[i].SerializationToString().Equals(m_signature2))
                                        return constructors[i];
                                }
                                else
                                {
                                    if (constructors[i].ToString().Equals(m_signature))
                                        return constructors[i];
                                }
                            }
                        }

                        throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));
                    }
                #endregion

                #region case MemberTypes.Method:
                case MemberTypes.Method:
                    {
                        MethodInfo methodInfo = null;

                        if (m_signature == null)
                            throw new SerializationException(SR.Serialization_NullSignature);

                        Type[] genericArguments = m_info.GetValueNoThrow("GenericArguments", typeof(Type[])) as Type[];

                        MethodInfo[] methods = m_reflectedType.GetMember(m_memberName, MemberTypes.Method, bindingFlags) as MethodInfo[];

                        if (methods.Length == 1)
                            methodInfo = methods[0];

                        else if (methods.Length > 1)
                        {
                            for (int i = 0; i < methods.Length; i++)
                            {
                                if (m_signature2 != null)
                                {
                                    if (methods[i].SerializationToString().Equals(m_signature2))
                                    {
                                        methodInfo = methods[i];
                                        break;
                                    }
                                }
                                else
                                {
                                    if (methods[i].ToString().Equals(m_signature))
                                    {
                                        methodInfo = methods[i];
                                        break;
                                    }
                                }

                                // Handle generic methods specially since the signature match above probably won't work (the candidate
                                // method info hasn't been instantiated). If our target method is generic as well we can skip this.
                                if (genericArguments != null && methods[i].IsGenericMethod)
                                {
                                    if (methods[i].GetGenericArguments().Length == genericArguments.Length)
                                    {
                                        MethodInfo candidateMethod = methods[i].MakeGenericMethod(genericArguments);

                                        if (m_signature2 != null)
                                        {
                                            if (candidateMethod.SerializationToString().Equals(m_signature2))
                                            {
                                                methodInfo = candidateMethod;
                                                break;
                                            }
                                        }
                                        else
                                        {
                                            if (candidateMethod.ToString().Equals(m_signature))
                                            {
                                                methodInfo = candidateMethod;
                                                break;
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (methodInfo == null)
                            throw new SerializationException(SR.Format(SR.Serialization_UnknownMember, m_memberName));

                        if (!methodInfo.IsGenericMethodDefinition)
                            return methodInfo;

                        if (genericArguments == null)
                            return methodInfo;

                        if (genericArguments[0] == null)
                            return null;

                        return methodInfo.MakeGenericMethod(genericArguments);
                    }
                #endregion

                default:
                    throw new ArgumentException(SR.Serialization_MemberTypeNotRecognized);
            }
        }
        #endregion
    }
}

