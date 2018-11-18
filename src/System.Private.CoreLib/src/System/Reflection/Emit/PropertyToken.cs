// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Reflection.Emit
{
    public struct PropertyToken
    {
        public static readonly PropertyToken Empty = new PropertyToken();

        internal PropertyToken(int str)
        {
            Token = str;
        }

        public int Token { get; }

        // Satisfy value class requirements
        public override int GetHashCode() => Token;

        // Satisfy value class requirements
        public override bool Equals(object obj) => obj is PropertyToken pt && Equals(pt);

        public bool Equals(PropertyToken obj) => obj.Token == Token;

        public static bool operator ==(PropertyToken a, PropertyToken b) => a.Equals(b);

        public static bool operator !=(PropertyToken a, PropertyToken b) => !(a == b);
    }
}
