// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading;
using System.Runtime.CompilerServices;
using System.Reflection;

namespace System.Diagnostics
{
    public partial class StackTrace
    {
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void GetStackFramesInternal(StackFrameHelper sfh, int iSkip, bool fNeedFileInfo, Exception e);

        internal static int CalculateFramesToSkip(StackFrameHelper StackF, int iNumFrames)
        {
            int iRetVal = 0;
            const string PackageName = "System.Diagnostics";

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
                    if (!string.Equals(ns, PackageName, StringComparison.Ordinal))
                        break;
                }
                iRetVal++;
            }

            return iRetVal;
        }

        private void InitializeForExceptionFrameIndex(Exception exception, int skipFrames, bool needFileInfo)
        {
            CaptureStackTrace(skipFrames, needFileInfo, exception);
        }

        private void InitializeForThreadFrameIndex(int skipFrames, bool needFileInfo)
        {
            CaptureStackTrace(skipFrames, needFileInfo, null);
        }

        /// <summary>
        /// Retrieves an object with stack trace information encoded.
        /// It leaves out the first "iSkip" lines of the stacktrace.
        /// </summary>
        private void CaptureStackTrace(int iSkip, bool fNeedFileInfo, Exception e)
        {
            m_iMethodsToSkip = iSkip;

            StackFrameHelper StackF = new StackFrameHelper(null);
            
            StackF.InitializeSourceInfo(0, fNeedFileInfo, e);

            m_iNumOfFrames = StackF.GetNumberOfFrames();

            if (m_iMethodsToSkip > m_iNumOfFrames)
                m_iMethodsToSkip = m_iNumOfFrames;

            if (m_iNumOfFrames != 0)
            {
                _stackFrames = new StackFrame[m_iNumOfFrames];

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

                    _stackFrames[i] = sfTemp;
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
        }
    }
}