// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Reflection
{
    public class LocalVariableInfo
    {        
        protected LocalVariableInfo() { }
        public virtual Type LocalType { get { Debug.Fail("type must be set!"); return null; } }
        public virtual bool IsPinned => false;
        public virtual int LocalIndex => 0;

        public override string ToString()
        {
            // This is really how the desktop behaves if you don't override, including the NullReference when 
            // it calls ToString() on LocalType's null return.
            string toString = LocalType.ToString() + " (" + LocalIndex + ")";
            return IsPinned ? toString += " (pinned)" : toString;
        }
    }
}

