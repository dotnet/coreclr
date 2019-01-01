// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Reflection
{
    public abstract partial class MemberInfo : ICustomAttributeProvider
    {
        protected MemberInfo() { }

        public abstract MemberTypes MemberType { get; }
        public abstract string Name { get; }
        public abstract Type DeclaringType { get; }
        public abstract Type ReflectedType { get; }

        public virtual Module Module
        {
            get
            {
                // This check is necessary because for some reason, Type adds a new "Module" property that hides the inherited one instead 
                // of overriding.

                if (this is Type type)
                {
                    return type.Module;
                }

                throw NotImplemented.ByDesign;
            }
        }

        public virtual bool HasSameMetadataDefinitionAs(MemberInfo other) { throw NotImplemented.ByDesign; }

        public abstract bool IsDefined(Type attributeType, bool inherit);
        public abstract object[] GetCustomAttributes(bool inherit);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

        public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();
        public virtual IList<CustomAttributeData> GetCustomAttributesData() { throw NotImplemented.ByDesign; }
        public virtual bool IsCollectible => true;
        public virtual int MetadataToken { get { throw new InvalidOperationException(); } }

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(MemberInfo left, MemberInfo right)
        {
            if (ReferenceEquals(left, right))
            {
                return true;
            }

            if (left is null || right is null)
            {
                return false;
            }

            if (left is Type type1)
            {
                // Type has special handling via operator== 
                return (right is Type type2) ? type1 == type2 : false;
            }

            if (left is MethodBase method1)
            {
                // MethodBase has special handling via operator== 
                return (right is MethodBase method2) ? method1 == method2 : false;
            }

            if (left is FieldInfo field1)
            {
                // FieldInfo operator== calls Equals after same reference and null checks above,
                // so just call Equals directly.
                return (right is FieldInfo field2) ? field1.Equals(field2) : false;
            }

            if (left is EventInfo event1)
            {
                // EventInfo operator== calls Equals after same reference and null checks above,
                // so just call Equals directly.
                return (right is EventInfo event2) ? event1.Equals(event2) : false;
            }

            if (left is PropertyInfo property1)
            {
                // PropertyInfo operator== calls Equals after same reference and null checks above,
                // so just call Equals directly.
                return (right is PropertyInfo property2) ? property1.Equals(property2) : false;
            }

            return false;
        }

        public static bool operator !=(MemberInfo left, MemberInfo right) => !(left == right);
    }
}
