// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: Class that stores local data. This class is used in cooperation
**          with the _LocalDataStoreMgr class.
**
**
=============================================================================*/

namespace System {
    
    using System;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;

    sealed internal class LocalDataStoreElement
    {
        private Object m_value;
        private long m_cookie;  // This is immutable cookie of the slot used to verify that 

        public long Cookie
        {
            get
            {
                return m_cookie;
            }
        }
    }

    // This class will not be marked serializable
    sealed internal class LocalDataStore
    {
        private LocalDataStoreElement[] m_DataTable;
        private LocalDataStoreMgr m_Manager;

        /*=========================================================================
        ** This method does clears the unused slot.
         * Assumes lock on m_Manager is taken
        =========================================================================*/
        internal void FreeData(int slot, long cookie)
        {
            // We try to delay allocate the dataTable (in cases like the manager clearing a
            // just-freed slot in all stores
            if (slot >= m_DataTable.Length)
                return;

            LocalDataStoreElement element = m_DataTable[slot];
            if (element != null && element.Cookie == cookie)
                m_DataTable[slot] = null;
        }
    }
}
