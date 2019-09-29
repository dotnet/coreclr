// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Runtime.Serialization;
using System.Text;

namespace System
{
    [Serializable]
    [System.Runtime.CompilerServices.TypeForwardedFrom("mscorlib, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089")]
    public partial class Exception : ISerializable
    {
        private protected const string InnerExceptionPrefix = " ---> ";

        public Exception()
        {
            _HResult = HResults.COR_E_EXCEPTION;
        }

        public Exception(string? message)
            : this()
        {
            _message = message;
        }

        // Creates a new Exception.  All derived classes should
        // provide this constructor.
        // Note: the stack trace is not started until the exception
        // is thrown
        //
        public Exception(string? message, Exception? innerException)
            : this()
        {
            _message = message;
            _innerException = innerException;
        }

        protected Exception(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
                throw new ArgumentNullException(nameof(info));

            _message = info.GetString("Message"); // Do not rename (binary serialization)
            _data = (IDictionary?)(info.GetValueNoThrow("Data", typeof(IDictionary))); // Do not rename (binary serialization)
            _innerException = (Exception?)(info.GetValue("InnerException", typeof(Exception))); // Do not rename (binary serialization)
            _helpURL = info.GetString("HelpURL"); // Do not rename (binary serialization)
            _stackTraceString = info.GetString("StackTraceString"); // Do not rename (binary serialization)
            _HResult = info.GetInt32("HResult"); // Do not rename (binary serialization)
            _source = info.GetString("Source"); // Do not rename (binary serialization)

            RestoreRemoteStackTrace(info, context);
        }

        public virtual string Message => _message ?? SR.Format(SR.Exception_WasThrown, GetClassName());

        public virtual IDictionary Data => _data ??= CreateDataContainer();

        private string GetClassName() => GetType().ToString();

        // Retrieves the lowest exception (inner most) for the given Exception.
        // This will traverse exceptions using the innerException property.
        public virtual Exception GetBaseException()
        {
            Exception? inner = InnerException;
            Exception back = this;

            while (inner != null)
            {
                back = inner;
                inner = inner.InnerException;
            }

            return back;
        }

        public Exception? InnerException => _innerException;

        // Sets the help link for this exception.
        // This should be in a URL/URN form, such as:
        // "file:///C:/Applications/Bazzal/help.html#ErrorNum42"
        public virtual string? HelpLink
        {
            get => _helpURL;
            set => _helpURL = value;
        }

        public virtual string? Source
        {
            get => _source ??= CreateSourceName();
            set => _source = value;
        }

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            if (_source == null)
            {
                _source = Source; // Set the Source information correctly before serialization
            }

            info.AddValue("ClassName", GetClassName(), typeof(string)); // Do not rename (binary serialization)
            info.AddValue("Message", _message, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("Data", _data, typeof(IDictionary)); // Do not rename (binary serialization)
            info.AddValue("InnerException", _innerException, typeof(Exception)); // Do not rename (binary serialization)
            info.AddValue("HelpURL", _helpURL, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("StackTraceString", SerializationStackTraceString, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("RemoteStackTraceString", SerializationRemoteStackTraceString, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("RemoteStackIndex", 0, typeof(int)); // Do not rename (binary serialization)
            info.AddValue("ExceptionMethod", null, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("HResult", _HResult); // Do not rename (binary serialization)
            info.AddValue("Source", _source, typeof(string)); // Do not rename (binary serialization)
            info.AddValue("WatsonBuckets", SerializationWatsonBuckets, typeof(byte[])); // Do not rename (binary serialization)
        }

        public override string ToString()
        {
            // Get the lengths of the StackTrace and _innerException first so we know how large to make the ValueStringBuilder
            string? stackTrace = StackTrace;
            string? innerException = null;
            if (_innerException != null)
            {
                innerException = _innerException.ToString();
            }

            int initialLength = 128;
            if (stackTrace != null)
            {
                initialLength += stackTrace.Length;
            }
            if (innerException != null)
            {
                initialLength += innerException.Length;
            }

            ValueStringBuilder sb = new ValueStringBuilder(initialLength);

            sb.Append(GetClassName());

            string? message = Message;
            if (!string.IsNullOrEmpty(message))
            {
                sb.Append(": ");
                sb.Append(message);
            }

            if (innerException != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(InnerExceptionPrefix);
                sb.Append(innerException);
                sb.Append(Environment.NewLine);
                sb.Append("   ");
                sb.Append(SR.Exception_EndOfInnerExceptionStack);
            }

            if (stackTrace != null)
            {
                sb.Append(Environment.NewLine);
                sb.Append(stackTrace);
            }

            return sb.ToString();
        }

        protected event EventHandler<SafeSerializationEventArgs>? SerializeObjectState
        {
            add { throw new PlatformNotSupportedException(SR.PlatformNotSupported_SecureBinarySerialization); }
            remove { throw new PlatformNotSupportedException(SR.PlatformNotSupported_SecureBinarySerialization); }
        }

        public int HResult
        {
            get => _HResult;
            set => _HResult = value;
        }

        // this method is required so Object.GetType is not made virtual by the compiler
        // _Exception.GetType()
        public new Type GetType() => base.GetType();

        partial void RestoreRemoteStackTrace(SerializationInfo info, StreamingContext context);
    }
}
