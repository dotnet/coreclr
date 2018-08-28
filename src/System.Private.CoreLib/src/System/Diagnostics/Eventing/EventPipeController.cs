// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if FEATURE_PERFTRACING
using Internal.IO;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics.Tracing
{
    /// <summary>
    /// Simple out-of-process listener for controlling EventPipe.
    /// The following environment variables are used to configure EventPipe:
    ///  - COMPlus_EnableEventPipe=4 : Enables this controller and creates a thread to listen for enable/disable events.
    ///  - COMPlus_EventPipeConfig : Provides the configuration in xperf string form for which providers/keywords/levels to be enabled.
    ///                              If not specified, the default configuration is used.
    ///  - COMPlus_EventPipeOutputFile : The full path to the netperf file to be written.
    /// Once the configuration is set and this controller is enabled, tracing is enabled by creating a marker file that this controller listens for.
    /// Tracing is disabled by deleting the marker file.  The marker file is the target trace file path with ".ctl" appended to it.  For example,
    /// if the trace file is /path/to/trace.netperf then the marker file is /path/to/trace.netperf.ctl.
    /// This listener does not poll very often, and thus takes time to enable and disable tracing.  This is by design to ensure that the listener does
    /// not starve other threads on the system.
    /// NOTE: If COMPlus_EnableEventPipe != 4 then this listener is not created and does not add any overhead to the process.
    /// </summary>
    internal sealed class EventPipeController
    {
        private const string MarkerFileExtension = ".ctl";
        private const int PollingIntervalMilliseconds = 10000; // 10 seconds
        private readonly char[] ProviderConfigDelimiter = new char[] { ',' };
        private readonly char[] ConfigComponentDelimiter = new char[] { ':' };

        private readonly EventPipeProviderConfiguration[] DefaultProviderConfiguration = new EventPipeProviderConfiguration[]
        {
            new EventPipeProviderConfiguration("Microsoft-Windows-DotNETRuntime", 0x4c14fccbd, 5),
            new EventPipeProviderConfiguration("Microsoft-Windows-DotNETRuntimePrivate", 0x4002000b, 5),
            new EventPipeProviderConfiguration("Microsoft-DotNETCore-SampleProfiler", 0x0, 5)
        };

        private static EventPipeController s_controllerInstance;

        private string m_traceFilePath;
        private string m_markerFilePath;
        private bool m_markerFileExists;

        internal static void Initialize()
        {
            // Don't allow failures to propagate upstream.
            // Instead, ensure program correctness without tracing.
            try
            {
                if (s_controllerInstance == null)
                {
                    string strEnabledValue = CompatibilitySwitch.GetValueInternal("EnableEventPipe");
                    if (strEnabledValue != null)
                    {
                        int enabledValue = Convert.ToInt32(strEnabledValue);
                        if (enabledValue == 4)
                        {
                            s_controllerInstance = new EventPipeController();
                        }
                    }
                }
            }
            catch { }
        }

        private EventPipeController()
        {
            // Determine the marker file path.
            string traceFilePath = CompatibilitySwitch.GetValueInternal("EventPipeOutputFile");
            if (!string.IsNullOrEmpty(traceFilePath))
            {
                m_traceFilePath = traceFilePath;
                m_markerFilePath = traceFilePath + MarkerFileExtension;
            }
            else
            {
                // If the output file path is not specified then use
                // path/to/current/working/directory/Process-<pid>.netperf.ctl
                m_traceFilePath = EventPipeInternal.GetDefaultTraceFileName();
                m_markerFilePath = m_traceFilePath + MarkerFileExtension;
            }

            m_markerFileExists = false;

            // Start a new thread to listen for tracing commands.
            Task.Factory.StartNew(WaitForTracingCommand, TaskCreationOptions.LongRunning);
        }

        private void WaitForTracingCommand()
        {
            while (true)
            {
                // Make sure that any transient errors don't cause the listener thread to exit.
                try
                {
                    // Check for existence of the file.
                    // If the existence of the file has changed since the last time we checked
                    // this means that we need to act on that change.
                    bool fileExists = File.Exists(m_markerFilePath);
                    if (m_markerFileExists != fileExists)
                    {
                        // Save the result.
                        m_markerFileExists = fileExists;

                        // Take the appropriate action.
                        if (fileExists)
                        {
                            // Enable tracing.
                            EventPipe.Enable(GetConfiguration());
                        }
                        else
                        {
                            // Disable tracing.
                            EventPipe.Disable();
                        }
                    }

                    // Wait for the polling interval.
                    Thread.Sleep(PollingIntervalMilliseconds);
                }
                catch { }
            }
        }

        private EventPipeConfiguration GetConfiguration()
        {
            // Get the circular buffer size.
            string strCircularMB = CompatibilitySwitch.GetValueInternal("EventPipeCircularMB");
            uint circularMB = Convert.ToUInt32(strCircularMB);

            // Create a new configuration object.
            EventPipeConfiguration config = new EventPipeConfiguration(m_traceFilePath, circularMB);

            // Get the configuration.
            string strConfig = CompatibilitySwitch.GetValueInternal("EventPipeConfig");
            if (!string.IsNullOrEmpty(strConfig))
            {
                // String must be of the form "providerName:keywords:level,providerName:keywords:level..."
                string[] providers = strConfig.Split(ProviderConfigDelimiter);
                foreach (string provider in providers)
                {
                    string[] components = provider.Split(ConfigComponentDelimiter);
                    if (components.Length == 3)
                    {
                        string providerName = components[0];
                        ulong keywords = Convert.ToUInt64(components[1], 16);
                        uint level = Convert.ToUInt16(components[2]);
                        config.EnableProvider(providerName, keywords, level);
                    }
                }
            }
            else
            {
                // Specify the default configuration.
                foreach (EventPipeProviderConfiguration providerConfig in DefaultProviderConfiguration)
                {
                    config.EnableProvider(providerConfig);
                }
            }

            return config;
        }
    }
}

#endif // FEATURE_PERFTRACING
