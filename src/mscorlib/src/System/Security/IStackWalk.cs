// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

namespace System.Security
{

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IStackWalk
    {
        [DynamicSecurityMethod]
        void Assert();
        
        [DynamicSecurityMethod]
        void Demand();
        
        [DynamicSecurityMethod]
        void Deny();
        
        [DynamicSecurityMethod]
        void PermitOnly();
    }
}
