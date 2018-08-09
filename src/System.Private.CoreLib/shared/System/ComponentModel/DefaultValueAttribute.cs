// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Globalization;
using System.Reflection;
using System.Threading;

namespace System.ComponentModel
{
    /// <devdoc>
    ///    <para>Specifies the default value for a property.</para>
    /// </devdoc>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1019:DefineAccessorsForAttributeArguments")]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes")]
    [AttributeUsage(AttributeTargets.All)]
    public class DefaultValueAttribute : Attribute
    {
        /// <devdoc>
        ///     This is the default value.
        /// </devdoc>
        private object _value;

        // We cache reflection types for TypeConverter conversion
        static Func<MethodInfo> s_getConverterMethodCached;
        static Func<MethodInfo> s_convertFromInvariantStringMethodCached;

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class, converting the
        ///    specified value to the
        ///    specified type, and using the U.S. English culture as the
        ///    translation
        ///    context.</para>
        /// </devdoc>
        public DefaultValueAttribute(Type type, string value)
        {
            // The try/catch here is because attributes should never throw exceptions.  We would fail to
            // load an otherwise normal class.
            try
            {
                if (TryConvertFromInvariantString(type, value, out object convertedValue))
                {
                    _value = convertedValue;
                }
                else if (type.IsSubclassOf(typeof(Enum)))
                {
                    _value = Enum.Parse(type, value, true);
                }
                else if (type == typeof(TimeSpan))
                {
                    _value = TimeSpan.Parse(value);
                }
                else
                {
                    _value = Convert.ChangeType(value, type, CultureInfo.InvariantCulture);
                }

                return;

                // Try to load and use TypeConverter/TypeDescriptor types
                bool TryConvertFromInvariantString(Type typeToConvert, string stringValue, out object conversionResult)
                {
                    conversionResult = null;

                    // lazy init reflection objects
                    if (s_getConverterMethodCached == null)
                    {
                        Type typeDescriptorObject = Type.GetType("System.ComponentModel.TypeDescriptor, System, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", throwOnError: false);
                        Volatile.Write(ref s_getConverterMethodCached, () => typeDescriptorObject?.GetMethod("GetConverter", new Type[] { typeof(Type) }));
                    }

                    if (s_convertFromInvariantStringMethodCached == null)
                    {
                        Type typeConverterType = Type.GetType("System.ComponentModel.TypeConverter, System, Version=0.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089", throwOnError: false);
                        Volatile.Write(ref s_convertFromInvariantStringMethodCached, () => typeConverterType?.GetMethod("ConvertFromInvariantString", new Type[] { typeof(string) }));
                    }

                    // use local var to avoid double delegate invocation
                    MethodInfo localGetConverterMethod = s_getConverterMethodCached();
                    MethodInfo localConvertFromInvariantStringMethod = s_convertFromInvariantStringMethodCached();

                    // if we didn't found required types on initialization fallback on old code
                    if (localGetConverterMethod == null || localConvertFromInvariantStringMethod == null)
                        return false;

                    // typeConverter cannot be null GetConverter return default converter
                    conversionResult = localConvertFromInvariantStringMethod.Invoke(localGetConverterMethod.Invoke(null, new[] { typeToConvert }), new[] { stringValue });

                    return true;
                }
            }
            catch
            {
            }
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a Unicode
        ///    character.</para>
        /// </devdoc>
        public DefaultValueAttribute(char value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using an 8-bit unsigned
        ///    integer.</para>
        /// </devdoc>
        public DefaultValueAttribute(byte value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a 16-bit signed
        ///    integer.</para>
        /// </devdoc>
        public DefaultValueAttribute(short value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a 32-bit signed
        ///    integer.</para>
        /// </devdoc>
        public DefaultValueAttribute(int value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a 64-bit signed
        ///    integer.</para>
        /// </devdoc>
        public DefaultValueAttribute(long value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a
        ///    single-precision floating point
        ///    number.</para>
        /// </devdoc>
        public DefaultValueAttribute(float value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a
        ///    double-precision floating point
        ///    number.</para>
        /// </devdoc>
        public DefaultValueAttribute(double value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.Boolean'/>
        /// value.</para>
        /// </devdoc>
        public DefaultValueAttribute(bool value)
        {
            _value = value;
        }
        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.String'/>.</para>
        /// </devdoc>
        public DefaultValueAttribute(string value)
        {
            _value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/>
        /// class.</para>
        /// </devdoc>
        public DefaultValueAttribute(object value)
        {
            _value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.SByte'/>
        /// value.</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public DefaultValueAttribute(sbyte value)
        {
            _value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.UInt16'/>
        /// value.</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public DefaultValueAttribute(ushort value)
        {
            _value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.UInt32'/>
        /// value.</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public DefaultValueAttribute(uint value)
        {
            _value = value;
        }

        /// <devdoc>
        /// <para>Initializes a new instance of the <see cref='System.ComponentModel.DefaultValueAttribute'/> class using a <see cref='System.UInt64'/>
        /// value.</para>
        /// </devdoc>
        [CLSCompliant(false)]
        public DefaultValueAttribute(ulong value)
        {
            _value = value;
        }

        /// <devdoc>
        ///    <para>
        ///       Gets the default value of the property this
        ///       attribute is
        ///       bound to.
        ///    </para>
        /// </devdoc>
        public virtual object Value
        {
            get
            {
                return _value;
            }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
            {
                return true;
            }

            DefaultValueAttribute other = obj as DefaultValueAttribute;

            if (other != null)
            {
                if (Value != null)
                {
                    return Value.Equals(other.Value);
                }
                else
                {
                    return (other.Value == null);
                }
            }
            return false;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        protected void SetValue(object value)
        {
            _value = value;
        }
    }
}
