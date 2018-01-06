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
                    return type.Module;

                throw NotImplemented.ByDesign;
            }
        }

        public virtual bool HasSameMetadataDefinitionAs(MemberInfo other) { throw NotImplemented.ByDesign; }

        public abstract bool IsDefined(Type attributeType, bool inherit);
        public abstract object[] GetCustomAttributes(bool inherit);
        public abstract object[] GetCustomAttributes(Type attributeType, bool inherit);

        public virtual IEnumerable<CustomAttributeData> CustomAttributes => GetCustomAttributesData();
        public virtual IList<CustomAttributeData> GetCustomAttributesData() { throw NotImplemented.ByDesign; }

        public virtual int MetadataToken { get { throw new InvalidOperationException(); } }

        public override bool Equals(object obj) => base.Equals(obj);
        public override int GetHashCode() => base.GetHashCode();

        public static bool operator ==(MemberInfo left, MemberInfo right)
        {
            if (object.ReferenceEquals(left, right))
                return true;

            if ((object)left == null || (object)right == null)
                return false;

            if (left is Type type1 && right is Type type2)
                return type1 == type2;
            else if (left is MethodBase method1 && right is MethodBase method2)
                return method1 == method2;
            else if (left is FieldInfo field1 && right is FieldInfo field2)
                return field1 == field2;
            else if (left is EventInfo event1 && right is EventInfo event2)
                return event1 == event2;
            else if (left is PropertyInfo property1 && right is PropertyInfo property2)
                return property1 == property2;

            return false;
        }

        public static bool operator !=(MemberInfo left, MemberInfo right) => !(left == right);
    }
}
