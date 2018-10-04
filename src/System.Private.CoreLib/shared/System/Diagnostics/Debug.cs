// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// Do not remove this, it is needed to retain calls to these conditional methods in release builds
#define DEBUG
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Diagnostics
{
    public abstract class DebugProvider
    {      
//        public DebugProvider() { }
        public abstract bool AutoFlush { get; set; }
        public abstract void Assert(bool condition);
        public abstract void Assert(bool condition, string message);
        public abstract void Assert(bool condition, string message, string detailMessage);
        public abstract void Close();
        public abstract void Fail(string message);
        public abstract void Fail(string message, string detailMessage);
        public abstract void Flush();
        public abstract int IndentLevel { get; set; }
        public abstract int IndentSize { get; set; }
        public abstract void Indent();
        public abstract void Unindent();
        public abstract void Write(object value);
        public abstract void Write(object value, string category);
        public abstract void Write(string message);
        public abstract void Write(string message, string category);
        public abstract void WriteLine(object value);
        public abstract void WriteLine(object value, string category);
        public abstract void WriteLine(string message);
        public abstract void WriteLine(string message, string category);
    }

    /// <summary>
    /// Provides a set of properties and methods for debugging code.
    /// </summary>
    public static partial class Debug
    {
        private static volatile DebugProvider s_provider;

        static Debug()
        {
            RegisterProvider(new DebugInternal());
        }

        internal static void RegisterProvider(DebugProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException(nameof(provider));

            if (s_provider == null)
            {
                // called from Debug static constructor.
                s_provider = provider;
            }

            Interlocked.CompareExchange(ref s_provider, provider, null);
        }

        public static bool AutoFlush { get { return s_provider.AutoFlushFromProvider; } set { s_provider.AutoFlushFromProvider = value; } }
        public static int IndentLevel { get { return s_provider.IndentLevelFromProvider; } set { s_provider.IndentLevelFromProvider = value; } }
        public static int IndentSize { get { return s_provider.IndentSizeFromProvider; } set { s_provider.IndentSizeFromProvider = value; } }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Close() { s_provider.CloseFromProvider(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Flush() { s_provider.FlushFromProvider(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Indent() { s_provider.IndentFromProvider(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Unindent() { s_provider.UnindentFromProvider(); }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Print(string message)
        {
            s_provider.WriteFromProvider(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Print(string format, params object[] args)
        {
            s_provider.WriteFromProvider(string.Format(null, format, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            s_provider.AssertFromProvider(condition);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            s_provider.AssertFromProvider(condition, message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessage)
        {
            s_provider.AssertFromProvider(condition, message, detailMessage);
        }

        internal static void ContractFailure(bool condition, string message, string detailMessage, string failureKindMessage)
        {
            if (!condition)
            {
                string stackTrace;
                try
                {
                    stackTrace = new StackTrace(2, true).ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
                }
                catch
                {
                    stackTrace = "";
                }
                s_provider.WriteLineFromProvider(FormatAssert(stackTrace, message, detailMessage));
                s_ShowDialog(stackTrace, message, detailMessage, SR.GetResourceString(failureKindMessage));
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail(string message)
        {
            s_provider.FailFromProvider(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Fail(string message, string detailMessage)
        {
            s_provider.FailFromProvider(message, detailMessage);
        }

        internal static string FormatAssert(string stackTrace, string message, string detailMessage)
        {
            string newLine = GetIndentString() + Environment.NewLine;
            return SR.DebugAssertBanner + newLine
                   + SR.DebugAssertShortMessage + newLine
                   + message + newLine
                   + SR.DebugAssertLongMessage + newLine
                   + detailMessage + newLine
                   + stackTrace;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Assert(bool condition, string message, string detailMessageFormat, params object[] args)
        {
            s_provider.AssertFromProvider(condition, message, string.Format(detailMessageFormat, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
            s_provider.WriteLineFromProvider(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(string message)
        {
            s_provider.WriteFromProvider(message);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value)
        {
            s_provider.WriteLineFromProvider(value);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(object value, string category)
        {
            s_provider.WriteLineFromProvider(value, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
            s_provider.WriteLineFromProvider(string.Format(null, format, args));
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLine(string message, string category)
        {
            s_provider.WriteLineFromProvider(message, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value)
        {
            s_provider.WriteFromProvider(value);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(string message, string category)
        {
            s_provider.WriteFromProvider(message, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void Write(object value, string category)
        {
            s_provider.WriteFromProvider(value, category);
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message)
        {
            if (condition)
            {
                s_provider.WriteFromProvider(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value)
        {
            if (condition)
            {
                s_provider.WriteFromProvider(value);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, string message, string category)
        {
            if (condition)
            {
                s_provider.WriteFromProvider(message, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteIf(bool condition, object value, string category)
        {
            if (condition)
            {
                s_provider.WriteFromProvider(value, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value)
        {
            if (condition)
            {
                s_provider.WriteLineFromProvider(value);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, object value, string category)
        {
            if (condition)
            {
                s_provider.WriteLineFromProvider(value, category);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message)
        {
            if (condition)
            {
                s_provider.WriteLineFromProvider(message);
            }
        }

        [System.Diagnostics.Conditional("DEBUG")]
        public static void WriteLineIf(bool condition, string message, string category)
        {
            if (condition)
            {
                s_provider.WriteLineFromProvider(message, category);
            }
        }

        private static string s_indentString;

        internal static string GetIndentString()
        {
            int indentCount = s_provider.IndentSizeFromProvider * s_provider.IndentLevelFromProvider;
            if (s_indentString?.Length == indentCount)
            {
                return s_indentString;
            }
            return s_indentString = new string(' ', indentCount);
        }

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

        // internal and not readonly so that the tests can swap this out.
        internal static Action<string, string, string, string> s_ShowDialog = ShowDialog;

        internal static Action<string> s_WriteCore = WriteCore;

        private class DebugInternal : DebugProvider
        {
            private static readonly object s_lock = new object();
            private static bool s_needIndent;

            public override void Assert(bool condition)
            {
                Assert(condition, string.Empty, string.Empty);
            }

            public override void Assert(bool condition, string message)
            {
                Assert(condition, message, string.Empty);
            }

            public override void Assert(bool condition, string message, string detailMessage)
            {
                if (!condition)
                {
                    string stackTrace;
                    try
                    {
                        stackTrace = new StackTrace(0, true).ToString(System.Diagnostics.StackTrace.TraceFormat.Normal);
                    }
                    catch
                    {
                        stackTrace = "";
                    }
                    WriteLine(Debug.FormatAssert(stackTrace, message, detailMessage));
                    Debug.s_ShowDialog(stackTrace, message, detailMessage, "Assertion Failed");
                }
            }

            public override bool AutoFlush { get { return true; } set { } }

            public override void Close() { }

            public override void Fail(string message)
            {
                Assert(false, message, string.Empty);
            }

            public override void Fail(string message, string detailMessage)
            {
                Assert(false, message, detailMessage);
            }

            public override void Flush() { }

            public override void Indent()
            {
                IndentLevel++;
            }

            public override void Unindent()
            {
                IndentLevel--;
            }

            [ThreadStatic]
            private static int s_indentLevel;
            public override int IndentLevel {
                get
                {
                    return s_indentLevel;
                }
                set
                {
                    s_indentLevel = value < 0 ? 0 : value;
                }
            }

            private static int s_indentSize = 4;
            public override int IndentSize {
                get
                {
                    return s_indentSize;
                }
                set
                {
                    s_indentSize = value < 0 ? 0 : value;
                }
            }

            public override void Write(string message)
            {
                lock (s_lock)
                {
                    if (message == null)
                    {
                        Debug.s_WriteCore(string.Empty);
                        return;
                    }
                    if (s_needIndent)
                    {
                        message = Debug.GetIndentString() + message;
                        s_needIndent = false;
                    }
                    Debug.s_WriteCore(message);
                    if (message.EndsWith(Environment.NewLine))
                    {
                        s_needIndent = true;
                    }
                }
            }

            public override void Write(object value)
            {
                Write(value?.ToString());
            }

            public override void Write(string message, string category)
            {
                if (category == null)
                {
                    Write(message);
                }
                else
                {
                    Write(category + ":" + message);
                }
            }

            public override void Write(object value, string category)
            {
                Write(value?.ToString(), category);
            }

            public override void WriteLine(string message)
            {
                Write(message + Environment.NewLine);
            }

            public override void WriteLine(object value)
            {
                WriteLine(value?.ToString());
            }

            public override void WriteLine(object value, string category)
            {
                WriteLine(value?.ToString(), category);
            }

            public override void WriteLine(string message, string category)
            {
                if (category == null)
                {
                    WriteLine(message);
                }
                else
                {
                    WriteLine(category + ":" + message);
                }
            }
        }
    }
}
