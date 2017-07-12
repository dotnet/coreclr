// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.


using System.Text;
using System;
using System.IO;
using System.Reflection;
using System.Diagnostics.Contracts;

namespace System.Diagnostics
{
    /// <summary>
    /// There is no good reason for the methods of this class to be virtual.
    /// </summary>
    public partial class StackFrame
    {
        private MethodBase method;
        private int offset;
        private int ILOffset;
        private String strFileName;
        private int iLineNumber;
        private int iColumnNumber;

        [System.Runtime.Serialization.OptionalField]
        private bool fIsLastFrameFromForeignExceptionStackTrace;

        internal void InitMembers()
        {
            method = null;
            offset = OFFSET_UNKNOWN;
            ILOffset = OFFSET_UNKNOWN;
            strFileName = null;
            iLineNumber = 0;
            iColumnNumber = 0;
            fIsLastFrameFromForeignExceptionStackTrace = false;
        }

        /// <summary>
        /// Constructs a StackFrame corresponding to the active stack frame.
        /// </summary>
        public StackFrame()
        {
            InitMembers();
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, false);/// <summary>
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, false);/// iSkipFrames=0
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, false);/// </summary>
        }

        /// <summary>
        /// Constructs a StackFrame corresponding to the active stack frame.
        /// </summary>
        public StackFrame(bool fNeedFileInfo)
        {
            InitMembers();
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, fNeedFileInfo);/// <summary>
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, fNeedFileInfo);/// iSkipFrames=0
            BuildStackFrame(0 + StackTrace.METHODS_TO_SKIP, fNeedFileInfo);/// </summary>
        }

        /// <summary>
        /// Constructs a StackFrame corresponding to a calling stack frame.
        /// </summary>
        public StackFrame(int skipFrames)
        {
            InitMembers();
            BuildStackFrame(skipFrames + StackTrace.METHODS_TO_SKIP, false);
        }

        /// <summary>
        /// Constructs a StackFrame corresponding to a calling stack frame.
        /// </summary>
        public StackFrame(int skipFrames, bool fNeedFileInfo)
        {
            InitMembers();
            BuildStackFrame(skipFrames + StackTrace.METHODS_TO_SKIP, fNeedFileInfo);
        }

        /// <summary>
        /// Constructs a "fake" stack frame, just containing the given file
        /// name and line number.  Use when you don't want to use the
        /// debugger's line mapping logic.
        /// </summary>
        public StackFrame(String fileName, int lineNumber)
        {
            InitMembers();
            BuildStackFrame(StackTrace.METHODS_TO_SKIP, false);
            strFileName = fileName;
            iLineNumber = lineNumber;
            iColumnNumber = 0;
        }

        /// <summary>
        /// Constructs a "fake" stack frame, just containing the given file
        /// name, line number and column number.  Use when you don't want to
        /// use the debugger's line mapping logic.
        /// </summary>
        public StackFrame(String fileName, int lineNumber, int colNumber)
        {
            InitMembers();
            BuildStackFrame(StackTrace.METHODS_TO_SKIP, false);
            strFileName = fileName;
            iLineNumber = lineNumber;
            iColumnNumber = colNumber;
        }

        /// <summary>
        /// Constant returned when the native or IL offset is unknown
        /// </summary>
        public const int OFFSET_UNKNOWN = -1;

        internal virtual void SetMethodBase(MethodBase mb)
        {
            method = mb;
        }

        internal virtual void SetOffset(int iOffset)
        {
            offset = iOffset;
        }

        internal virtual void SetILOffset(int iOffset)
        {
            ILOffset = iOffset;
        }

        internal virtual void SetFileName(String strFName)
        {
            strFileName = strFName;
        }

        internal virtual void SetLineNumber(int iLine)
        {
            iLineNumber = iLine;
        }

        internal virtual void SetColumnNumber(int iCol)
        {
            iColumnNumber = iCol;
        }

        internal virtual void SetIsLastFrameFromForeignExceptionStackTrace(bool fIsLastFrame)
        {
            fIsLastFrameFromForeignExceptionStackTrace = fIsLastFrame;
        }

        internal virtual bool GetIsLastFrameFromForeignExceptionStackTrace()
        {
            return fIsLastFrameFromForeignExceptionStackTrace;
        }

        /// <summary>
        /// Returns the method the frame is executing
        /// </summary>
        public virtual MethodBase GetMethod()
        {
            Contract.Ensures(Contract.Result<MethodBase>() != null);

            return method;
        }

        /// <summary>
        /// Returns the offset from the start of the native (jitted) code for the
        /// method being executed
        /// </summary>
        public virtual int GetNativeOffset()
        {
            return offset;
        }


        /// <summary>
        /// Returns the offset from the start of the IL code for the
        /// method being executed.  This offset may be approximate depending
        /// on whether the jitter is generating debuggable code or not.
        /// </summary>
        public virtual int GetILOffset()
        {
            return ILOffset;
        }

        /// <summary>
        /// Returns the file name containing the code being executed.  This
        /// information is normally extracted from the debugging symbols
        /// for the executable.
        /// </summary>
        public virtual String GetFileName()
        {
            return strFileName;
        }

        /// <summary>
        /// Returns the line number in the file containing the code being executed.
        /// This information is normally extracted from the debugging symbols
        /// for the executable.
        /// </summary>
        public virtual int GetFileLineNumber()
        {
            return iLineNumber;
        }

        /// <summary>
        /// Returns the column number in the line containing the code being executed.
        /// This information is normally extracted from the debugging symbols
        /// for the executable.
        /// </summary>
        public virtual int GetFileColumnNumber()
        {
            return iColumnNumber;
        }

        /// <summary>
        /// Format method name assuming the MethodBase information is available.
        /// </summary>
        private void FormatMethodNameUsingMethodBase(StringBuilder sb)
        {
            Debug.Assert(method != null);
            
            sb.Append(method.Name);

            /// <summary>
            /// deal with the generic portion of the method
            /// </summary>
            if (method is MethodInfo && ((MethodInfo)method).IsGenericMethod)
            {
                Type[] typars = ((MethodInfo)method).GetGenericArguments();

                sb.Append('<');
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

                sb.Append('>');
            }
        }

        /// <summary>
        /// Output IL offset and file / line / column information.
        /// </summary>
        private void FormatOffsetAndFileInfo(StringBuilder sb)
        {
            sb.Append(" at offset ");
            if (offset == OFFSET_UNKNOWN)
                sb.Append("<offset unknown>");
            else
                sb.Append(offset);

            sb.Append(" in file:line:column ");

            bool useFileName = (strFileName != null);

            if (!useFileName)
                sb.Append("<filename unknown>");
            else
                sb.Append(strFileName);
            sb.Append(':');
            sb.Append(iLineNumber);
            sb.Append(':');
            sb.Append(iColumnNumber);
        }
    }
}
