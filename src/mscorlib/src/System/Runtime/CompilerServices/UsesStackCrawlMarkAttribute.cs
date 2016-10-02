// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Runtime.CompilerServices
{
    // Indicates to the runtime that this method contains a StackCrawlMark
    // local variable. This prevents inlining of both caller and callee.

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method)]
    internal sealed class UsesStackCrawlMarkAttribute : Attribute
    {
        public UsesStackCrawlMarkAttribute() { }
    }
}
