// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
** 
** 
**
**
** Purpose: Represents a Field to the ILGenerator Class
**
** 
===========================================================*/

using System;
using System.Reflection;

namespace System.Reflection.Emit
{
    // The FieldToken class is an opaque representation of the Token returned
    // by the Metadata to represent the field.  FieldTokens are generated by 
    // Module.GetFieldToken().  There are no meaningful accessors on this class,
    // but it can be passed to ILGenerator which understands it's internals.
    public struct FieldToken
    {
        public static readonly FieldToken Empty = new FieldToken();

        internal int m_fieldTok;
        internal object m_class;

        // Creates an empty FieldToken.  A publicly visible constructor so that
        // it can be created on the stack.
        //public FieldToken() {
        //    m_fieldTok=0;
        //    m_attributes=0;
        //    m_class=null;
        //}
        // The actual constructor.  Sets the field, attributes and class
        // variables

        internal FieldToken(int field, Type fieldClass)
        {
            m_fieldTok = field;
            m_class = fieldClass;
        }

        public int Token
        {
            get { return m_fieldTok; }
        }


        // Generates the hash code for this field. 
        public override int GetHashCode()
        {
            return (m_fieldTok);
        }

        // Returns true if obj is an instance of FieldToken and is 
        // equal to this instance.
        public override bool Equals(object obj)
        {
            if (obj is FieldToken)
                return Equals((FieldToken)obj);
            else
                return false;
        }

        public bool Equals(FieldToken obj)
        {
            return obj.m_fieldTok == m_fieldTok && obj.m_class == m_class;
        }

        public static bool operator ==(FieldToken a, FieldToken b)
        {
            return a.Equals(b);
        }

        public static bool operator !=(FieldToken a, FieldToken b)
        {
            return !(a == b);
        }
    }
}
