// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Do not remove this, it is needed to retain calls to these conditional methods in release builds
#define DEBUG

namespace System.Diagnostics
{
    /// <summary>
    /// Provides default implementation for Write and ShowDialog methods in Debug class.
    /// </summary>
    public partial class DebugProvider
    {
        public virtual void Write(string message)
        {
            lock (s_lock)
            {
                if (message == null)
                {
                    WriteCore(string.Empty);
                    return;
                }
                if (_needIndent)
                {
                    message = GetIndentString() + message;
                    _needIndent = false;
                }
                WriteCore(message);
                if (message.EndsWith(Environment.NewLine))
                {
                    _needIndent = true;
                }
            }
        }
        
        public virtual void WriteLine(string message)
        {
            Write(message + Environment.NewLine);
        }

        public virtual void OnIndentLevelChanged(int indentLevel) { }

        public virtual void OnIndentSizeChanged(int indentSize) { }

        private static readonly object s_lock = new object();

        private sealed class DebugAssertException : Exception
        {
            internal DebugAssertException(string stackTrace) :
                base(Environment.NewLine + stackTrace)
            {
            }

            internal DebugAssertException(string message, string stackTrace) :
                base(message + Environment.NewLine + Environment.NewLine + stackTrace)
            {
            }

            internal DebugAssertException(string message, string detailMessage, string stackTrace) :
                base(message + Environment.NewLine + detailMessage + Environment.NewLine + Environment.NewLine + stackTrace)
            {
            }
        }

        private bool _needIndent = true;

        private string _indentString;

        private string GetIndentString()
        {
            int indentCount = Debug.IndentSize * Debug.IndentLevel;
            if (_indentString?.Length == indentCount)
            {
                return _indentString;
            }
            return _indentString = new string(' ', indentCount);
        }

        // internal and not readonly so that the tests can swap this out.
        internal static Action<string> s_WriteCore = null;
    }
}