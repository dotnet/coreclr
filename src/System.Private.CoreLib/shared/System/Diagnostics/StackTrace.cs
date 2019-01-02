// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;

namespace System.Diagnostics
{
    /// <summary>
    /// Class which represents a description of a stack trace
    /// There is no good reason for the methods of this class to be virtual.
    /// In order to ensure trusted code can trust the data it gets from a
    /// StackTrace, we use an InheritanceDemand to prevent partially-trusted
    /// subclasses.
    /// </summary>
    public partial class StackTrace
    {
        public const int METHODS_TO_SKIP = 0;

        private int m_iNumOfFrames;
        private int m_iMethodsToSkip;
        
        /// <summary>
        /// Stack frames comprising this stack trace.
        /// </summary>
        private StackFrame[] _stackFrames;

        /// <summary>
        /// Constructs a stack trace from the current location.
        /// </summary>
        public StackTrace()
        {
            CaptureStackTrace(METHODS_TO_SKIP, false);
        }

        /// <summary>
        /// Constructs a stack trace from the current location.
        /// </summary>
        public StackTrace(bool fNeedFileInfo)
        {
            CaptureStackTrace(METHODS_TO_SKIP, fNeedFileInfo);
        }

        /// <summary>
        /// Constructs a stack trace from the current location, in a caller's
        /// frame
        /// </summary>
        public StackTrace(int skipFrames)
        {
            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, false);
        }

        /// <summary>
        /// Constructs a stack trace from the current location, in a caller's
        /// frame
        /// </summary>
        public StackTrace(int skipFrames, bool fNeedFileInfo)
        {
            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, fNeedFileInfo);
        }

        /// <summary>
        /// Constructs a stack trace from the current location.
        /// </summary>
        public StackTrace(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            InitializeForExceptionFrameIndex(e, METHODS_TO_SKIP, false);
        }

        /// <summary>
        /// Constructs a stack trace from the current location.
        /// </summary>
        public StackTrace(Exception e, bool fNeedFileInfo)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            InitializeForExceptionFrameIndex(e, METHODS_TO_SKIP, fNeedFileInfo);
        }

        /// <summary>
        /// Constructs a stack trace from the current location, in a caller's
        /// frame
        /// </summary>
        public StackTrace(Exception e, int skipFrames)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            InitializeForExceptionFrameIndex(e, skipFrames + METHODS_TO_SKIP, false);
        }

        /// <summary>
        /// Constructs a stack trace from the current location, in a caller's
        /// frame
        /// </summary>
        public StackTrace(Exception e, int skipFrames, bool fNeedFileInfo)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            InitializeForExceptionFrameIndex(e, skipFrames + METHODS_TO_SKIP, fNeedFileInfo);
        }

        /// <summary>
        /// Constructs a "fake" stack trace, just containing a single frame.
        /// Does not have the overhead of a full stack trace.
        /// </summary>
        public StackTrace(StackFrame frame)
        {
            _stackFrames = new StackFrame[] { frame };
            m_iNumOfFrames = 1;
        }

        /// <summary>
        /// Property to get the number of frames in the stack trace
        /// </summary>
        public virtual int FrameCount
        {
            get { return m_iNumOfFrames; }
        }

        /// <summary>
        /// Returns a given stack frame.  Stack frames are numbered starting at
        /// zero, which is the last stack frame pushed.
        /// </summary>
        public virtual StackFrame GetFrame(int index)
        {
            if (_stackFrames != null && index < m_iNumOfFrames && index >= 0)
                return _stackFrames[index + m_iMethodsToSkip];

            return null;
        }

        /// <summary>
        /// Returns an array of all stack frames for this stacktrace.
        /// The array is ordered and sized such that GetFrames()[i] == GetFrame(i)
        /// The nth element of this array is the same as GetFrame(n).
        /// The length of the array is the same as FrameCount.
        /// </summary>
        public virtual StackFrame[] GetFrames()
        {
            if (_stackFrames == null || m_iNumOfFrames <= 0)
                return null;

            // We have to return a subset of the array. Unfortunately this
            // means we have to allocate a new array and copy over.
            StackFrame[] array = new StackFrame[m_iNumOfFrames];
            Array.Copy(_stackFrames, m_iMethodsToSkip, array, 0, m_iNumOfFrames);
            return array;
        }

        /// <summary>
        /// Builds a readable representation of the stack trace
        /// </summary>
        public override string ToString()
        {
#if CORERT
            // default behavior in RT did not have trailing newline  
            return ToString(TraceFormat.Normal);
#else
            // Include a trailing newline for backwards compatibility
            return ToString(TraceFormat.TrailingNewLine);
#endif
        }

        /// <summary>
        /// TraceFormat is Used to specify options for how the 
        /// string-representation of a StackTrace should be generated.
        /// </summary>
        internal enum TraceFormat
        {
            Normal,
            TrailingNewLine,        // include a trailing new line character
        }

        /// <summary>
        /// Builds a readable representation of the stack trace, specifying 
        /// the format for backwards compatibility.
        /// </summary>
        internal string ToString(TraceFormat traceFormat)
        {
            bool displayFilenames = true;   // we'll try, but demand may fail
            string word_At = SR.Word_At;
            string inFileLineNum = SR.StackTrace_InFileLineNumber;

            bool fFirstFrame = true;
            StringBuilder sb = new StringBuilder(255);
            for (int iFrameIndex = 0; iFrameIndex < m_iNumOfFrames; iFrameIndex++)
            {
                StackFrame sf = GetFrame(iFrameIndex);
                MethodBase mb = sf.GetMethod();
                if (mb != null && (ShowInStackTrace(mb) || 
                                   (iFrameIndex == m_iNumOfFrames - 1))) // Don't filter last frame
                {
                    // We want a newline at the end of every line except for the last
                    if (fFirstFrame)
                        fFirstFrame = false;
                    else
                        sb.Append(Environment.NewLine);

                    sb.AppendFormat(CultureInfo.InvariantCulture, "   {0} ", word_At);

                    bool isAsync = false;
                    Type declaringType = mb.DeclaringType;
                    string methodName = mb.Name;
                    bool methodChanged = false;
                    if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute), inherit: false))
                    {
                        isAsync = typeof(IAsyncStateMachine).IsAssignableFrom(declaringType);
                        if (isAsync || typeof(IEnumerator).IsAssignableFrom(declaringType))
                        {
                            methodChanged = TryResolveStateMachineMethod(ref mb, out declaringType);
                        }
                    }

                    // if there is a type (non global method) print it
                    // ResolveStateMachineMethod may have set declaringType to null
                    if (declaringType != null)
                    {
                        // Append t.FullName, replacing '+' with '.'
                        string fullName = declaringType.FullName;
                        for (int i = 0; i < fullName.Length; i++)
                        {
                            char ch = fullName[i];
                            sb.Append(ch == '+' ? '.' : ch);
                        }
                        sb.Append('.');
                    }
                    sb.Append(mb.Name);

                    // deal with the generic portion of the method
                    if (mb is MethodInfo mi && mi.IsGenericMethod)
                    {
                        Type[] typars = mi.GetGenericArguments();
                        sb.Append('[');
                        int k = 0;
                        bool fFirstTyParam = true;
                        while (k < typars.Length)
                        {
                            if (fFirstTyParam == false)
                                sb.Append(',');
                            else
                                fFirstTyParam = false;

                            sb.Append(typars[k].Name);
                            k++;
                        }
                        sb.Append(']');
                    }

                    ParameterInfo[] pi = null;
                    try
                    {
                        pi = mb.GetParameters();
                    }
                    catch
                    {
                        // The parameter info cannot be loaded, so we don't
                        // append the parameter list.
                    }
                    if (pi != null)
                    {
                        // arguments printing
                        sb.Append('(');
                        bool fFirstParam = true;
                        for (int j = 0; j < pi.Length; j++)
                        {
                            if (fFirstParam == false)
                                sb.Append(", ");
                            else
                                fFirstParam = false;

                            string typeName = "<UnknownType>";
                            if (pi[j].ParameterType != null)
                                typeName = pi[j].ParameterType.Name;
                            sb.Append(typeName);
                            sb.Append(' ');
                            sb.Append(pi[j].Name);
                        }
                        sb.Append(')');
                    }

                    if (methodChanged)
                    {
                        // Append original method name e.g. +MoveNext()
                        sb.Append('+');
                        sb.Append(methodName);
                        sb.Append('(').Append(')');
                    }

                    // source location printing
                    if (displayFilenames && (sf.GetILOffset() != -1))
                    {
                        // If we don't have a PDB or PDB-reading is disabled for the module,
                        // then the file name will be null.
                        string fileName = null;

                        // Getting the filename from a StackFrame is a privileged operation - we won't want
                        // to disclose full path names to arbitrarily untrusted code.  Rather than just omit
                        // this we could probably trim to just the filename so it's still mostly usefull.
                        try
                        {
                            fileName = sf.GetFileName();
                        }
                        catch (SecurityException)
                        {
                            // If the demand for displaying filenames fails, then it won't
                            // succeed later in the loop.  Avoid repeated exceptions by not trying again.
                            displayFilenames = false;
                        }

                        if (fileName != null)
                        {
                            // tack on " in c:\tmp\MyFile.cs:line 5"
                            sb.Append(' ');
                            sb.AppendFormat(CultureInfo.InvariantCulture, inFileLineNum, fileName, sf.GetFileLineNumber());
                        }
                    }

                    if (sf.GetIsLastFrameFromForeignExceptionStackTrace() &&
                        !isAsync) // Skip EDI boundary for async
                    {
                        sb.Append(Environment.NewLine);
                        sb.Append(SR.Exception_EndStackTraceFromPreviousThrow);
                    }
                }
            }

            if (traceFormat == TraceFormat.TrailingNewLine)
                sb.Append(Environment.NewLine);

            return sb.ToString();
        }

        private static bool ShowInStackTrace(MethodBase mb)
        {
            Debug.Assert(mb != null);
            return !(mb.IsDefined(typeof(StackTraceHiddenAttribute)) || (mb.DeclaringType?.IsDefined(typeof(StackTraceHiddenAttribute)) ?? false));
        }

        private static bool TryResolveStateMachineMethod(ref MethodBase method, out Type declaringType)
        {
            Debug.Assert(method != null);
            Debug.Assert(method.DeclaringType != null);

            declaringType = method.DeclaringType;

            Type parentType = declaringType.DeclaringType;
            if (parentType == null)
            {
                return false;
            }

            MethodInfo[] methods = parentType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.DeclaredOnly);
            if (methods == null)
            {
                return false;
            }

            foreach (MethodInfo candidateMethod in methods)
            {
                IEnumerable<StateMachineAttribute> attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>(inherit: false);
                if (attributes == null)
                {
                    continue;
                }

                bool foundAttribute = false, foundIteratorAttribute = false;
                foreach (StateMachineAttribute asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        foundAttribute = true;
                        foundIteratorAttribute |= asma is IteratorStateMachineAttribute || asma is AsyncIteratorStateMachineAttribute;
                    }
                }

                if (foundAttribute)
                {
                    // If this is an iterator (sync or async), mark the iterator as changed, so it gets the + annotation
                    // of the original method. Non-iterator async state machines resolve directly to their builder methods
                    // so aren't marked as changed.
                    method = candidateMethod;
                    declaringType = candidateMethod.DeclaringType;
                    return foundIteratorAttribute;
                }
            }

            return false;
        }
    }
}
