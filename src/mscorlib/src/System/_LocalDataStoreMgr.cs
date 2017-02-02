// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*=============================================================================
**
**
**
** Purpose: Class that manages stores of local data. This class is used in 
**          cooperation with the LocalDataStore class.
**
**
=============================================================================*/
namespace System {
    
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Runtime.CompilerServices;
    using System.Diagnostics.Contracts;

    // This class is an encapsulation of a slot so that it is managed in a secure fashion.
    // It is constructed by the LocalDataStoreManager, holds the slot and the manager
    // and cleans up when it is finalized.
    // This class will not be marked serializable
[System.Runtime.InteropServices.ComVisible(true)]
    public sealed class LocalDataStoreSlot
    {
        private LocalDataStoreMgr m_mgr;
        private int m_slot;
        private long m_cookie;

        internal LocalDataStoreSlot() {}

        // Release the slot reserved by this object when this object goes away.
        ~LocalDataStoreSlot()
        {
            LocalDataStoreMgr mgr = m_mgr;
            if (mgr == null)
                return;

            int slot = m_slot;

            // Mark the slot as free.
            m_slot = -1;

            mgr.FreeDataSlot(slot, m_cookie);
        }
    }

    // This class will not be marked serializable
    sealed internal class LocalDataStoreMgr
    {
        private const int InitialSlotTableSize            = 64;
        private const int SlotTableDoubleThreshold        = 512;
        private const int LargeSlotTableSizeIncrease    = 128;

        /*=========================================================================
        ** Free's a previously allocated data slot on ALL the managed data stores.
        =========================================================================*/
        internal void FreeDataSlot(int slot, long cookie)
        {
            bool tookLock = false;
            RuntimeHelpers.PrepareConstrainedRegions();
            try
            {
                Monitor.Enter(this, ref tookLock);
                // Go thru all the managed stores and set the data on the specified slot to 0.
                for (int i = 0; i < m_ManagedLocalDataStores.Count; i++)
                {
                    ((LocalDataStore)m_ManagedLocalDataStores[i]).FreeData(slot, cookie);
                }

                // Mark the slot as being no longer occupied. 
                m_SlotInfoTable[slot] = false;
                if (slot < m_FirstAvailableSlot)
                    m_FirstAvailableSlot = slot;
            }
            finally
            {
                if (tookLock)
                    Monitor.Exit(this);
            }
        }

        private bool[] m_SlotInfoTable = new bool[InitialSlotTableSize];
        private int m_FirstAvailableSlot;
        private List<LocalDataStore> m_ManagedLocalDataStores = new List<LocalDataStore>();
        private Dictionary<String, LocalDataStoreSlot> m_KeyToSlotMap = new Dictionary<String, LocalDataStoreSlot>();
        private long m_CookieGenerator;
    }
}
