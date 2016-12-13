// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

// 

//
//
//  Url is an IIdentity representing url internet sites.
//

namespace System.Security.Policy {
    using System.IO;
    using System.Security.Util;
    using UrlIdentityPermission = System.Security.Permissions.UrlIdentityPermission;
    using System.Runtime.Serialization;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    [Serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public sealed class Url : EvidenceBase, IIdentityPermissionFactory
    {
        private URLString m_url;

        internal Url( String name, bool parsed )
        {
            if (name == null)
                throw new ArgumentNullException( nameof(name) );
            Contract.EndContractBlock();

            m_url = new URLString( name, parsed );
        }

        public Url( String name )
        {
            if (name == null)
                throw new ArgumentNullException( nameof(name) );
            Contract.EndContractBlock();

            m_url = new URLString( name );
        }

        private Url(Url url)
        {
            Debug.Assert(url != null);
            m_url = url.m_url;
        }

        public String Value
        {
            get { return m_url.ToString(); }
        }

        internal URLString GetURLString()
        {
            return m_url;
        }

        public IPermission CreateIdentityPermission( Evidence evidence )
        {
            return new UrlIdentityPermission( m_url );
        }

        public override bool Equals(Object o)
        {
            Url other = o as Url;
            if (other == null)
            {
                return false;
            }

            return other.m_url.Equals(m_url);
        }

        public override int GetHashCode()
        {
            return this.m_url.GetHashCode();
        }

        public override EvidenceBase Clone()
        {
            return new Url(this);
        }

        public Object Copy()
        {
            return Clone();
        }

        // INormalizeForIsolatedStorage is not implemented for startup perf
        // equivalent to INormalizeForIsolatedStorage.Normalize()
        internal Object Normalize()
        {
            return m_url.NormalizeUrl();
        }
    }
}
