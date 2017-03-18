// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Reflection;

namespace System
{
    // This file collects the longer methods of Type to make the main Type class more readable.
    public abstract partial class Type : MemberInfo, IReflect
    {
        public virtual Type[] FindInterfaces(TypeFilter filter, object filterCriteria)
        {
            if (filter == null)
                throw new ArgumentNullException(nameof(filter));

            Type[] c = GetInterfaces();
            int cnt = 0;
            for (int i = 0; i < c.Length; i++)
            {
                if (!filter(c[i], filterCriteria))
                    c[i] = null;
                else
                    cnt++;
            }
            if (cnt == c.Length)
                return c;

            Type[] ret = new Type[cnt];
            cnt = 0;
            for (int i = 0; i < c.Length; i++)
            {
                if (c[i] != null)
                    ret[cnt++] = c[i];
            }
            return ret;
        }

        public virtual MemberInfo[] FindMembers(MemberTypes memberType, BindingFlags bindingAttr, MemberFilter filter, object filterCriteria)
        {
            // Define the work arrays
            MethodInfo[] m = null;
            ConstructorInfo[] c = null;
            FieldInfo[] f = null;
            PropertyInfo[] p = null;
            EventInfo[] e = null;
            Type[] t = null;

            int i = 0;
            int cnt = 0;            // Total Matchs

            // Check the methods
            if ((memberType & MemberTypes.Method) != 0)
            {
                m = GetMethods(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < m.Length; i++)
                        if (!filter(m[i], filterCriteria))
                            m[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += m.Length;
                }
            }

            // Check the constructors
            if ((memberType & MemberTypes.Constructor) != 0)
            {
                c = GetConstructors(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < c.Length; i++)
                        if (!filter(c[i], filterCriteria))
                            c[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += c.Length;
                }
            }

            // Check the fields
            if ((memberType & MemberTypes.Field) != 0)
            {
                f = GetFields(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < f.Length; i++)
                        if (!filter(f[i], filterCriteria))
                            f[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += f.Length;
                }
            }

            // Check the Properties
            if ((memberType & MemberTypes.Property) != 0)
            {
                p = GetProperties(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < p.Length; i++)
                        if (!filter(p[i], filterCriteria))
                            p[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += p.Length;
                }
            }

            // Check the Events
            if ((memberType & MemberTypes.Event) != 0)
            {
                e = GetEvents(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < e.Length; i++)
                        if (!filter(e[i], filterCriteria))
                            e[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += e.Length;
                }
            }

            // Check the Types
            if ((memberType & MemberTypes.NestedType) != 0)
            {
                t = GetNestedTypes(bindingAttr);
                if (filter != null)
                {
                    for (i = 0; i < t.Length; i++)
                        if (!filter(t[i], filterCriteria))
                            t[i] = null;
                        else
                            cnt++;
                }
                else
                {
                    cnt += t.Length;
                }
            }

            // Allocate the Member Info
            MemberInfo[] ret = new MemberInfo[cnt];

            // Copy the Methods
            cnt = 0;
            if (m != null)
            {
                for (i = 0; i < m.Length; i++)
                    if (m[i] != null)
                        ret[cnt++] = m[i];
            }

            // Copy the Constructors
            if (c != null)
            {
                for (i = 0; i < c.Length; i++)
                    if (c[i] != null)
                        ret[cnt++] = c[i];
            }

            // Copy the Fields
            if (f != null)
            {
                for (i = 0; i < f.Length; i++)
                    if (f[i] != null)
                        ret[cnt++] = f[i];
            }

            // Copy the Properties
            if (p != null)
            {
                for (i = 0; i < p.Length; i++)
                    if (p[i] != null)
                        ret[cnt++] = p[i];
            }

            // Copy the Events
            if (e != null)
            {
                for (i = 0; i < e.Length; i++)
                    if (e[i] != null)
                        ret[cnt++] = e[i];
            }

            // Copy the Types
            if (t != null)
            {
                for (i = 0; i < t.Length; i++)
                    if (t[i] != null)
                        ret[cnt++] = t[i];
            }

            return ret;
        }

        public bool IsVisible
        {
            get
            {
#if CORECLR
                RuntimeType rt = this as RuntimeType;
                if (rt != null)
                    return RuntimeTypeHandle.IsVisible(rt);
#endif //CORECLR

                if (IsGenericParameter)
                    return true;

                if (HasElementType)
                    return GetElementType().IsVisible;

                Type type = this;
                while (type.IsNested)
                {
                    if (!type.IsNestedPublic)
                        return false;

                    // this should be null for non-nested types.
                    type = type.DeclaringType;
                }

                // Now "type" should be a top level type
                if (!type.IsPublic)
                    return false;

                if (IsGenericType && !IsGenericTypeDefinition)
                {
                    foreach (Type t in GetGenericArguments())
                    {
                        if (!t.IsVisible)
                            return false;
                    }
                }

                return true;
            }
        }

        public virtual bool ContainsGenericParameters
        {
            get
            {
                if (HasElementType)
                    return GetRootElementType().ContainsGenericParameters;

                if (IsGenericParameter)
                    return true;

                if (!IsGenericType)
                    return false;

                Type[] genericArguments = GetGenericArguments();
                for (int i = 0; i < genericArguments.Length; i++)
                {
                    if (genericArguments[i].ContainsGenericParameters)
                        return true;
                }

                return false;
            }
        }

        internal Type GetRootElementType()
        {
            Type rootElementType = this;

            while (rootElementType.HasElementType)
                rootElementType = rootElementType.GetElementType();

            return rootElementType;
        }

        public virtual bool IsSubclassOf(Type c)
        {
            Type p = this;
            if (p == c)
                return false;
            while (p != null)
            {
                if (p == c)
                    return true;
                p = p.BaseType;
            }
            return false;
        }

        public virtual bool IsAssignableFrom(Type c)
        {
            if (c == null)
                return false;

            if (this == c)
                return true;

            // For backward-compatibility, we need to special case for the types
            // whose UnderlyingSystemType are runtime implemented. 
            Type toType = this.UnderlyingSystemType;
            if (toType.IsRuntimeImplemented())
                return toType.IsAssignableFrom(c);

            // If c is a subclass of this class, then c can be cast to this type.
            if (c.IsSubclassOf(this))
                return true;

            if (this.IsInterface)
            {
                return c.ImplementInterface(this);
            }
            else if (IsGenericParameter)
            {
                Type[] constraints = GetGenericParameterConstraints();
                for (int i = 0; i < constraints.Length; i++)
                    if (!constraints[i].IsAssignableFrom(c))
                        return false;

                return true;
            }

            return false;
        }

        internal bool ImplementInterface(Type ifaceType)
        {
            Type t = this;
            while (t != null)
            {
                Type[] interfaces = t.GetInterfaces();
                if (interfaces != null)
                {
                    for (int i = 0; i < interfaces.Length; i++)
                    {
                        // Interfaces don't derive from other interfaces, they implement them.
                        // So instead of IsSubclassOf, we should use ImplementInterface instead.
                        if (interfaces[i] == ifaceType ||
                            (interfaces[i] != null && interfaces[i].ImplementInterface(ifaceType)))
                            return true;
                    }
                }

                t = t.BaseType;
            }

            return false;
        }
    }
}
