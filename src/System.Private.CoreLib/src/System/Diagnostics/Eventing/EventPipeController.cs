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
    internal sealed class EventPipeController
    {
        private const string MarkerFileExtension = ".ctl";
        private const int PollingIntervalMilliseconds = 10000; // 10 seconds
        private const uint CircularBufferSizeMB = 1024;
        private static EventPipeController s_controllerInstance;

        private string m_traceFilePath;
        private string m_markerFilePath;
        private bool m_markerFileExists;

        internal static void Initialize()
        {
            System.Diagnostics.Debugger.Launch();
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
                long processID = 0; // TODO: Need to plumb the process ID from native code.
                m_traceFilePath = "Process-" + processID + ".netperf";
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
        }

        private EventPipeConfiguration GetConfiguration()
        {
            // Create a new configuration object.
            EventPipeConfiguration config = new EventPipeConfiguration(m_traceFilePath, CircularBufferSizeMB);

            // Get the configuration.
            string strConfig = CompatibilitySwitch.GetValueInternal("EventPipeConfiguration");
            if (!string.IsNullOrEmpty(strConfig))
            {
                // String must be of the form "providerName:keywords:level,providerName:keywords:level..."
                string[] providers = strConfig.Split(new char[] { ',' });
                foreach (string provider in providers)
                {
                    string[] components = provider.Split(new char[] { ':' });
                    if (components.Length == 3)
                    {
                        string providerName = components[0];
                        ulong keywords = Convert.ToUInt64(components[1]);
                        uint level = Convert.ToUInt16(components[2]);
                        config.EnableProvider(providerName, keywords, level);
                    }
                }
            }
            else
            {
                // Specify the default configuration.
                config.EnableProvider("Microsoft-Windows-DotNETRuntime", 0x4c14fccbd, 5);
                config.EnableProvider("Microsoft-Windows-DotNETRuntimePrivate", 0x4002000b, 5);
                config.EnableProvider("Microsoft-DotNETCore-SampleProfiler", 0x0, 5);
            }

            return config;
        }
    }
}

#endif // FEATURE_PERFTRACING
