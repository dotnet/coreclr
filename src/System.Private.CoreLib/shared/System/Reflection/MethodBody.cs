// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;

namespace System.Reflection
{
    public class MethodBody
    {
        protected MethodBody() { }

        private byte[] _IL = null;
        private ExceptionHandlingClause[] _exceptionHandlingClauses = null;
        private LocalVariableInfo[] _localVariables = null;
#if CORECLR
        internal MethodBase _methodBase;
#endif
        private int _localSignatureMetadataToken = 0;
        private int _maxStackSize = 0;
        private bool _initLocals = false;

        public virtual int LocalSignatureMetadataToken => _localSignatureMetadataToken;
        public virtual IList<LocalVariableInfo> LocalVariables => Array.AsReadOnly(_localVariables);
        public virtual int MaxStackSize => _maxStackSize;
        public virtual bool InitLocals => _initLocals;
        public virtual byte[] GetILAsByteArray() => _IL;
        public virtual IList<ExceptionHandlingClause> ExceptionHandlingClauses => Array.AsReadOnly(_exceptionHandlingClauses);
    }
}
