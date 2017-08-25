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
    // There is no good reason for the methods of this class to be virtual.
    public partial class StackFrame
    {
        /// <summary>
        /// Called from the class "StackTrace"
        /// </summary>
        internal StackFrame(bool DummyFlag1, bool DummyFlag2)
        {
            InitMembers();
        }

        /// <summary>
        /// Builds a readable representation of the stack frame
        /// </summary>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder(255);

            if (method != null)
            {
                FormatMethodNameUsingMethodBase(sb);
                FormatOffsetAndFileInfo(sb);
            }
            else
            {
                sb.Append("<null>");
            }
            sb.Append(Environment.NewLine);

            return sb.ToString();
        }


        private void BuildStackFrame(int skipFrames, bool fNeedFileInfo)
        {
            using (StackFrameHelper StackF = new StackFrameHelper(null))
            {
                StackF.InitializeSourceInfo(0, fNeedFileInfo, null);

                int iNumOfFrames = StackF.GetNumberOfFrames();

                skipFrames += StackTrace.CalculateFramesToSkip(StackF, iNumOfFrames);

                if ((iNumOfFrames - skipFrames) > 0)
                {
                    method = StackF.GetMethodBase(skipFrames);
                    offset = StackF.GetOffset(skipFrames);
                    ILOffset = StackF.GetILOffset(skipFrames);
                    if (fNeedFileInfo)
                    {
                        strFileName = StackF.GetFilename(skipFrames);
                        iLineNumber = StackF.GetLineNumber(skipFrames);
                        iColumnNumber = StackF.GetColumnNumber(skipFrames);
                    }
                }
            }
        }
    }
}
