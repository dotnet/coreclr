// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices {
    // Attribute used to communicate to the VS7 debugger that a class should be treated as if it has global scope.
    
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    public class CompilerGlobalScopeAttribute : Attribute {
        public CompilerGlobalScopeAttribute() { }
    }
}

