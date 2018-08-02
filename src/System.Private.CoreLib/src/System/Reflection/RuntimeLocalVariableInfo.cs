// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace System.Reflection
{
    internal class RuntimeLocalVariableInfo : LocalVariableInfo
    {
        #region Private Data Members
        private RuntimeType _type;
        private int _localIndex;
        private bool _isPinned;
        #endregion

        #region Constructor
        protected RuntimeLocalVariableInfo() { }
        #endregion

        #region Object Overrides
        public override string ToString()
        {
            string toString = LocalType.ToString() + " (" + LocalIndex + ")";

            if (IsPinned)
                toString += " (pinned)";

            return toString;
        }
        #endregion

        #region Public Members
        public virtual Type LocalType { get { Debug.Assert(_type != null, "type must be set!"); return _type; } }
        public virtual int LocalIndex => _localIndex;
        public virtual bool IsPinned => _isPinned;
        #endregion
    }
}

