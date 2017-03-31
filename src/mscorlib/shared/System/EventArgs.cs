// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace System {
    // The base class for all event classes.
    [Serializable]
    public class EventArgs {
        public static readonly EventArgs Empty = new EventArgs();

        public EventArgs() {
        }
    }
}
