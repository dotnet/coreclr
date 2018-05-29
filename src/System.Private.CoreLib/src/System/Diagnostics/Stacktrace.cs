// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Security;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Globalization;
using System.Runtime.Versioning;

namespace System.Diagnostics
{
    // Class which represents a description of a stack trace
    // There is no good reason for the methods of this class to be virtual.  
    public class StackTrace
    {
        private StackFrame[] frames;
        private int m_iNumOfFrames;
        public const int METHODS_TO_SKIP = 0;
        private int m_iMethodsToSkip;

        // Constructs a stack trace from the current location.
        public StackTrace()
        {
            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;
            CaptureStackTrace(METHODS_TO_SKIP, false, null, null);
        }

        // Constructs a stack trace from the current location.
        //
        public StackTrace(bool fNeedFileInfo)
        {
            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;
            CaptureStackTrace(METHODS_TO_SKIP, fNeedFileInfo, null, null);
        }

        // Constructs a stack trace from the current location, in a caller's
        // frame
        //
        public StackTrace(int skipFrames)
        {
            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, false, null, null);
        }

        // Constructs a stack trace from the current location, in a caller's
        // frame
        //
        public StackTrace(int skipFrames, bool fNeedFileInfo)
        {
            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, fNeedFileInfo, null, null);
        }


        // Constructs a stack trace from the current location.
        public StackTrace(Exception e)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;
            CaptureStackTrace(METHODS_TO_SKIP, false, null, e);
        }

        // Constructs a stack trace from the current location.
        //
        public StackTrace(Exception e, bool fNeedFileInfo)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;
            CaptureStackTrace(METHODS_TO_SKIP, fNeedFileInfo, null, e);
        }

        // Constructs a stack trace from the current location, in a caller's
        // frame
        //
        public StackTrace(Exception e, int skipFrames)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, false, null, e);
        }

        // Constructs a stack trace from the current location, in a caller's
        // frame
        //
        public StackTrace(Exception e, int skipFrames, bool fNeedFileInfo)
        {
            if (e == null)
                throw new ArgumentNullException(nameof(e));

            if (skipFrames < 0)
                throw new ArgumentOutOfRangeException(nameof(skipFrames),
                    SR.ArgumentOutOfRange_NeedNonNegNum);

            m_iNumOfFrames = 0;
            m_iMethodsToSkip = 0;

            CaptureStackTrace(skipFrames + METHODS_TO_SKIP, fNeedFileInfo, null, e);
        }


        // Constructs a "fake" stack trace, just containing a single frame.  
        // Does not have the overhead of a full stack trace.
        //
        public StackTrace(StackFrame frame)
        {
            frames = new StackFrame[1];
            frames[0] = frame;
            m_iMethodsToSkip = 0;
            m_iNumOfFrames = 1;
        }


        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GetStackFramesInternal(StackFrameHelper sfh, int iSkip, bool fNeedFileInfo, Exception e);

        internal static int CalculateFramesToSkip(StackFrameHelper StackF, int iNumFrames)
        {
            int iRetVal = 0;
            string PackageName = "System.Diagnostics";

            // Check if this method is part of the System.Diagnostics
            // package. If so, increment counter keeping track of 
            // System.Diagnostics functions
            for (int i = 0; i < iNumFrames; i++)
            {
                MethodBase mb = StackF.GetMethodBase(i);
                if (mb != null)
                {
                    Type t = mb.DeclaringType;
                    if (t == null)
                        break;
                    string ns = t.Namespace;
                    if (ns == null)
                        break;
                    if (string.Compare(ns, PackageName, StringComparison.Ordinal) != 0)
                        break;
                }
                iRetVal++;
            }

            return iRetVal;
        }

        // Retrieves an object with stack trace information encoded.
        // It leaves out the first "iSkip" lines of the stacktrace.
        //
        private void CaptureStackTrace(int iSkip, bool fNeedFileInfo, Thread targetThread, Exception e)
        {
            m_iMethodsToSkip += iSkip;

            StackFrameHelper StackF = new StackFrameHelper(targetThread);
            
            StackF.InitializeSourceInfo(0, fNeedFileInfo, e);

            m_iNumOfFrames = StackF.GetNumberOfFrames();

            if (m_iMethodsToSkip > m_iNumOfFrames)
                m_iMethodsToSkip = m_iNumOfFrames;

            if (m_iNumOfFrames != 0)
            {
                frames = new StackFrame[m_iNumOfFrames];

                for (int i = 0; i < m_iNumOfFrames; i++)
                {
                    bool fDummy1 = true;
                    bool fDummy2 = true;
                    StackFrame sfTemp = new StackFrame(fDummy1, fDummy2);

                    sfTemp.SetMethodBase(StackF.GetMethodBase(i));
                    sfTemp.SetOffset(StackF.GetOffset(i));
                    sfTemp.SetILOffset(StackF.GetILOffset(i));

                    sfTemp.SetIsLastFrameFromForeignExceptionStackTrace(StackF.IsLastFrameFromForeignExceptionStackTrace(i));

                    if (fNeedFileInfo)
                    {
                        sfTemp.SetFileName(StackF.GetFilename(i));
                        sfTemp.SetLineNumber(StackF.GetLineNumber(i));
                        sfTemp.SetColumnNumber(StackF.GetColumnNumber(i));
                    }

                    frames[i] = sfTemp;
                }

                // CalculateFramesToSkip skips all frames in the System.Diagnostics namespace,
                // but this is not desired if building a stack trace from an exception.
                if (e == null)
                    m_iMethodsToSkip += CalculateFramesToSkip(StackF, m_iNumOfFrames);

                m_iNumOfFrames -= m_iMethodsToSkip;
                if (m_iNumOfFrames < 0)
                {
                    m_iNumOfFrames = 0;
                }
            }

            // In case this is the same object being re-used, set frames to null
            else
            {
                frames = null;
            }
        }

        // Property to get the number of frames in the stack trace
        //
        public virtual int FrameCount
        {
            get { return m_iNumOfFrames; }
        }


        // Returns a given stack frame.  Stack frames are numbered starting at
        // zero, which is the last stack frame pushed.
        //
        public virtual StackFrame GetFrame(int index)
        {
            if ((frames != null) && (index < m_iNumOfFrames) && (index >= 0))
                return frames[index + m_iMethodsToSkip];

            return null;
        }

        // Returns an array of all stack frames for this stacktrace.
        // The array is ordered and sized such that GetFrames()[i] == GetFrame(i)
        // The nth element of this array is the same as GetFrame(n). 
        // The length of the array is the same as FrameCount.
        // 
        public virtual StackFrame[] GetFrames()
        {
            if (frames == null || m_iNumOfFrames <= 0)
                return null;

            // We have to return a subset of the array. Unfortunately this
            // means we have to allocate a new array and copy over.
            StackFrame[] array = new StackFrame[m_iNumOfFrames];
            Array.Copy(frames, m_iMethodsToSkip, array, 0, m_iNumOfFrames);
            return array;
        }

        // Builds a readable representation of the stack trace
        //
        public override string ToString()
        {
            // Include a trailing newline for backwards compatibility
            return ToString(TraceFormat.TrailingNewLine);
        }

        // TraceFormat is Used to specify options for how the 
        // string-representation of a StackTrace should be generated.
        internal enum TraceFormat
        {
            Normal,
            TrailingNewLine,        // include a trailing new line character
            NoResourceLookup    // to prevent infinite resource recusion
        }

        // Builds a readable representation of the stack trace, specifying 
        // the format for backwards compatibility.
        internal string ToString(TraceFormat traceFormat)
        {
            bool displayFilenames = true;   // we'll try, but demand may fail
            string word_At = "at";
            string inFileLineNum = "in {0}:line {1}";

            if (traceFormat != TraceFormat.NoResourceLookup)
            {
                word_At = SR.Word_At;
                inFileLineNum = SR.StackTrace_InFileLineNumber;
            }

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
                    if (declaringType != null && declaringType.IsDefined(typeof(CompilerGeneratedAttribute)))
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
                    if (mb is MethodInfo && ((MethodInfo)mb).IsGenericMethod)
                    {
                        Type[] typars = ((MethodInfo)mb).GetGenericArguments();
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
                        sb.Append("+");
                        sb.Append(methodName);
                        sb.Append("()");
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
                IEnumerable<StateMachineAttribute> attributes = candidateMethod.GetCustomAttributes<StateMachineAttribute>();
                if (attributes == null)
                {
                    continue;
                }

                foreach (StateMachineAttribute asma in attributes)
                {
                    if (asma.StateMachineType == declaringType)
                    {
                        method = candidateMethod;
                        declaringType = candidateMethod.DeclaringType;
                        // Mark the iterator as changed; so it gets the + annotation of the original method
                        // async statemachines resolve directly to their builder methods so aren't marked as changed
                        return asma is IteratorStateMachineAttribute;
                    }
                }
            }

            return false;
        }
    }
}
