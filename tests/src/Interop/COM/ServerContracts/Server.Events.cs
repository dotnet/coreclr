// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#pragma warning disable 618 // Must test deprecated features

namespace Server.Contract.Events
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    using IConnectionPoint = System.Runtime.InteropServices.ComTypes.IConnectionPoint;
    using IConnectionPointContainer = System.Runtime.InteropServices.ComTypes.IConnectionPointContainer;

    [ComVisible(false)]
    public delegate void TestingEvents_OnEventEventHandler(string msg);

    [ComVisible(false)]
    [ComEventInterface(typeof(Contract.TestingEvents), typeof(TestingEvents_EventProvider))]
    public interface TestingEvents_Event
    {
        event TestingEvents_OnEventEventHandler OnEvent;
    }

    public sealed class TestingEvents_EventProvider : TestingEvents_Event, IDisposable
    {
        private readonly WeakReference ConnectionPointContainer;
        private readonly List<TestingEvents_SinkHelper> eventSinkHelpers = new List<TestingEvents_SinkHelper>();

        private IConnectionPoint connectionPoint;
        private bool isDisposed = false;

        public TestingEvents_EventProvider(object container)
        {
            this.ConnectionPointContainer = new WeakReference((IConnectionPointContainer)container, false);
        }

        event TestingEvents_OnEventEventHandler TestingEvents_Event.OnEvent
        {
            add
            {
                lock (this.eventSinkHelpers)
                {
                    if (this.connectionPoint == null)
                    {
                        this.Init();
                    }

                    var sinkHelper = new TestingEvents_SinkHelper();

                    int cookie;
                    this.connectionPoint.Advise(sinkHelper, out cookie);

                    sinkHelper.Cookie = cookie;
                    sinkHelper.OnEventDelegate = value;
                    this.eventSinkHelpers.Add(sinkHelper);
                }
            }
            remove
            {
                lock (this.eventSinkHelpers)
                {
                    TestingEvents_SinkHelper sinkHelper = null;
                    int removeIdx = -1;
                    for (int i = 0; i < this.eventSinkHelpers.Count; ++i)
                    {
                        TestingEvents_SinkHelper sinkHelperMaybe = this.eventSinkHelpers[i];
                        if (sinkHelperMaybe.OnEventDelegate.Equals(value))
                        {
                            removeIdx = i;
                            sinkHelper = sinkHelperMaybe;
                            break;
                        }
                    }

                    if (removeIdx < 0)
                    {
                        return;
                    }

                    this.connectionPoint.Unadvise(sinkHelper.Cookie);
                    this.eventSinkHelpers.RemoveAt(removeIdx);

                    if (this.eventSinkHelpers.Count == 0)
                    {
                        Marshal.ReleaseComObject(this.connectionPoint);
                        this.connectionPoint = null;
                    }
                }
            }
        }

        void IDisposable.Dispose()
        {
            if (this.isDisposed)
            {
                return;
            }

            lock (this.eventSinkHelpers)
            {
                foreach (TestingEvents_SinkHelper sinkHelper in this.eventSinkHelpers)
                {
                    this.connectionPoint.Unadvise(sinkHelper.Cookie);
                }

                this.eventSinkHelpers.Clear();
            }

            Marshal.ReleaseComObject(this.connectionPoint);
            this.connectionPoint = null;

            this.isDisposed = true;
            System.GC.SuppressFinalize(this);
        }

        private void Init()
        {
            var container = (IConnectionPointContainer)this.ConnectionPointContainer.Target;

            Guid iid = typeof(Contract.TestingEvents).GUID;
            IConnectionPoint connectionPoint;
            container.FindConnectionPoint(ref iid, out connectionPoint);

            this.connectionPoint = connectionPoint;
        }
    }

    [ClassInterface(ClassInterfaceType.None)]
    public class TestingEvents_SinkHelper : Contract.TestingEvents
    {
        public int Cookie { get; set; }
        public TestingEvents_OnEventEventHandler OnEventDelegate { get; set; }

        public void OnEvent(string msg)
        {
            if (this.OnEventDelegate != null)
            {
                this.OnEventDelegate(msg);
            }
        }
    }
}

#pragma warning restore 618 // Must test deprecated features
