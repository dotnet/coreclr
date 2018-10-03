// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
#if FEATURE_PERFTRACING
using Internal.IO;
using Microsoft.Win32;
using System.IO;
using System.Globalization;
using System.Reflection;
using System.Runtime.Versioning;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics.Tracing
{
    /// <summary>
    /// Simple out-of-process listener for controlling EventPipe.
    /// The following environment variables are used to configure EventPipe:
    ///  - COMPlus_EnableEventPipe=1 : Enable EventPipe immediately for the life of the process.
    ///  - COMPlus_EnableEventPipe=4 : Enables this controller and creates a thread to listen for enable/disable events.
    ///  - COMPlus_EventPipeConfig : Provides the configuration in xperf string form for which providers/keywords/levels to be enabled.
    ///                              If not specified, the default configuration is used.
    ///  - COMPlus_EventPipeOutputFile : The full path to the netperf file to be written.
    ///  - COMPlus_EventPipeCircularMB : The size in megabytes of the circular buffer.
    /// Once the configuration is set and this controller is enabled, tracing is enabled by creating a marker file that this controller listens for.
    /// Tracing is disabled by deleting the marker file.  The marker file is the target trace file path with ".ctl" appended to it.  For example,
    /// if the trace file is /path/to/trace.netperf then the marker file is /path/to/trace.netperf.ctl.
    /// This listener does not poll very often, and thus takes time to enable and disable tracing.  This is by design to ensure that the listener does
    /// not starve other threads on the system.
    /// NOTE: If COMPlus_EnableEventPipe != 4 then this listener is not created and does not add any overhead to the process.
    /// </summary>
    internal sealed class EventPipeController
    {
        // Miscellaneous constants.
        private const string NetPerfFileExtension = ".netperf";
        private const string ConfigFileName = "app.eventpipeconfig";
        private const int EnabledPollingIntervalMilliseconds = 1000; // 1 second
        private const int DisabledPollingIntervalMilliseconds = 5000; // 5 seconds
        private const uint DefaultCircularBufferMB = 1024; // 1 GB
        private static readonly char[] ProviderConfigDelimiter = new char[] { ',' };
        private static readonly char[] ConfigComponentDelimiter = new char[] { ':' };
        private static readonly string[] ConfigFileLineDelimiters = new string[] { "\r\n", "\n" };
        private const char ConfigEntryDelimiter = '=';

        // Config file keys.
        private const string ConfigKey_Providers = "Providers";
        private const string ConfigKey_CircularMB = "CircularMB";
        private const string ConfigKey_OutputPath = "OutputPath";

        // The default set of providers/keywords/levels.  Used if an alternative configuration is not specified.
        private static readonly EventPipeProviderConfiguration[] DefaultProviderConfiguration = new EventPipeProviderConfiguration[]
        {
            new EventPipeProviderConfiguration("Microsoft-Windows-DotNETRuntime", 0x4c14fccbd, 5),
            new EventPipeProviderConfiguration("Microsoft-Windows-DotNETRuntimePrivate", 0x4002000b, 5),
            new EventPipeProviderConfiguration("Microsoft-DotNETCore-SampleProfiler", 0x0, 5)
        };

        // Singleton controller instance.
        private static EventPipeController s_controllerInstance = null;

        // Controller object state.
        private Timer m_timer;
        private string m_configFilePath;
        private DateTime m_configFileUpdateTime;
        private string m_traceFilePath = null;
        private bool m_configFileExists = false;

        internal static void Initialize()
        {
            // Don't allow failures to propagate upstream.  Ensure program correctness without tracing.
            try
            {
                if (s_controllerInstance == null)
                {
                    if (Config_EnableEventPipe > 0)
                    {
                        // Enable tracing immediately.
                        // It will be disabled automatically on shutdown.
                        EventPipe.Enable(BuildConfigFromEnvironment());
                    }
                    else
                    {
                        // Create a new controller to listen for commands.
                        s_controllerInstance = new EventPipeController();
                    }
                }
            }
            catch { }
        }

        private EventPipeController()
        {
            // Set the config file path.
            m_configFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ConfigFileName);

            // Initialize the timer, but don't set it to run.
            // The timer will be set to run each time PollForTracingCommand is called.
            m_timer = new Timer(
                callback: new TimerCallback(PollForTracingCommand),
                state: null,
                dueTime: Timeout.Infinite,
                period: Timeout.Infinite,
                flowExecutionContext: false);

            // Trigger the first poll operation on the start-up path.
            PollForTracingCommand(null);
        }

        private void PollForTracingCommand(object state)
        {
            // Make sure that any transient errors don't cause the listener thread to exit.
            try
            {
                // Check for existence of the config file.
                // If the existence of the file has changed since the last time we checked or the update time has changed
                // this means that we need to act on that change.
                bool fileExists = File.Exists(m_configFilePath);
                if (m_configFileExists != fileExists)
                {
                    // Save the result.
                    m_configFileExists = fileExists;

                    // Take the appropriate action.
                    if (fileExists)
                    {
                        // Enable tracing.
                        EventPipe.Enable(BuildConfigFromFile(m_configFilePath));
                    }
                    else
                    {
                        // Disable tracing.
                        EventPipe.Disable();
                    }
                }

                // Schedule the timer to run again.
                m_timer.Change(fileExists ? EnabledPollingIntervalMilliseconds : DisabledPollingIntervalMilliseconds, Timeout.Infinite);
            }
            catch { }
        }

        private static EventPipeConfiguration BuildConfigFromFile(string configFilePath)
        {
            // Read the config file in once call.
            byte[] configContents = File.ReadAllBytes(configFilePath);

            // Convert the contents to a string.
            string strConfigContents = Encoding.UTF8.GetString(configContents);

            // Read all of the config options.
            string outputPath = null;
            string strProviderConfig = null;
            string strCircularMB = null;

            // Split the configuration entries by line.
            string[] configEntries = strConfigContents.Split(ConfigFileLineDelimiters, StringSplitOptions.RemoveEmptyEntries);
            foreach (string configEntry in configEntries)
            {
                //`Split the key and value by '='.
                string[] entryComponents = configEntry.Split(ConfigEntryDelimiter);
                if(entryComponents.Length == 2)
                {
                    string key = entryComponents[0];
                    if (key.Equals(ConfigKey_Providers))
                    {
                        strProviderConfig = entryComponents[1];
                    }
                    else if (key.Equals(ConfigKey_OutputPath))
                    {
                        outputPath = entryComponents[1];
                    }
                    else if(key.Equals(ConfigKey_CircularMB))
                    {
                        strCircularMB = entryComponents[1];
                    }
                }
            }

            // Ensure that the output path is set.
            if (string.IsNullOrEmpty(outputPath))
            {
                throw new ArgumentNullException(nameof(outputPath));
            }

            // Build the full path to the trace file.
            string traceFileName = Assembly.GetEntryAssembly().GetName().Name + "." + Win32Native.GetCurrentProcessId() + NetPerfFileExtension;
            string outputFile = Path.Combine(outputPath, traceFileName);

            // Get the circular buffer size.
            uint circularMB = DefaultCircularBufferMB;
            if(!string.IsNullOrEmpty(strCircularMB))
            {
                circularMB = Convert.ToUInt32(strCircularMB);
            }

            // Initialize a new configuration object.
            EventPipeConfiguration config = new EventPipeConfiguration(outputFile, circularMB);

            // Set the provider configuration if specified.
            if (!string.IsNullOrEmpty(strProviderConfig))
            {
                SetProviderConfiguration(strProviderConfig, config);
            }
            else
            {
                // If the provider configuration isn't specified, use the default.
                config.EnableProviderRange(DefaultProviderConfiguration);
            }

            return config;
        }

        private static EventPipeConfiguration BuildConfigFromEnvironment()
        {
            // Create a new configuration object.
            EventPipeConfiguration config = new EventPipeConfiguration(
                GetDisambiguatedTraceFilePath(Config_EventPipeOutputFile),
                Config_EventPipeCircularMB);

            // Get the configuration.
            string strConfig = Config_EventPipeConfig;
            if (!string.IsNullOrEmpty(strConfig))
            {
                // If the configuration is specified, parse it and save it to the config object.
                SetProviderConfiguration(strConfig, config);
            }
            else
            {
                // Specify the default configuration.
                config.EnableProviderRange(DefaultProviderConfiguration);
            }

            return config;
        }

        private static void SetProviderConfiguration(string strConfig, EventPipeConfiguration config)
        {
            if (string.IsNullOrEmpty(strConfig))
            {
                throw new ArgumentNullException(nameof(strConfig));
            }

            // Check for the diagnostic configuration '*' which means "enable all events in the entire system."
            if (strConfig.Equals("*"))
            {
                config.EnableAllEvents = true;
            }
            else
            {
                // String must be of the form "providerName:keywords:level,providerName:keywords:level..."
                string[] providers = strConfig.Split(ProviderConfigDelimiter);
                foreach (string provider in providers)
                {
                    string[] components = provider.Split(ConfigComponentDelimiter);
                    if (components.Length == 3)
                    {
                        string providerName = components[0];

                        // We use a try/catch block here because ulong.TryParse won't accept 0x at the beginning
                        // of a hex string.  Thus, we either need to conditionally strip it or handle the exception.
                        // Given that this is not a perf-critical path, catching the exception is the simpler code.
                        ulong keywords = 0;
                        try
                        {
                            keywords = Convert.ToUInt64(components[1], 16);
                        }
                        catch { }

                        uint level;
                        if (!uint.TryParse(components[2], out level))
                        {
                            level = 0;
                        }

                        config.EnableProvider(providerName, keywords, level);
                    }
                }
            }
        }

        /// <summary>
        /// Responsible for disambiguating the trace file path if the specified file already exists.
        /// This can happen if there are multiple applications with tracing enabled concurrently and COMPlus_EventPipeOutputFile
        /// is set to the same value for more than one concurrently running application.
        /// </summary>
        private static string GetDisambiguatedTraceFilePath(string inputPath)
        {
            if (string.IsNullOrEmpty(inputPath))
            {
                throw new ArgumentNullException("inputPath");
            }

            string filePath = inputPath;
            if (File.Exists(filePath))
            {
                string directoryName = Path.GetDirectoryName(filePath);
                string fileWithoutExtension = Path.GetFileName(filePath);
                string extension = Path.GetExtension(filePath);

                string newFileWithExtension = fileWithoutExtension + "." + Win32Native.GetCurrentProcessId() + extension;
                filePath = Path.Combine(directoryName, newFileWithExtension);
            }

            return filePath;
        }

        #region Configuration

        // Cache for COMPlus configuration variables.
        private static int s_Config_EnableEventPipe = -1;
        private static string s_Config_EventPipeConfig = null;
        private static uint s_Config_EventPipeCircularMB = 0;
        private static string s_Config_EventPipeOutputFile = null;

        private static int Config_EnableEventPipe
        {
            get
            {
                if (s_Config_EnableEventPipe == -1)
                {
                    string strEnabledValue = CompatibilitySwitch.GetValueInternal("EnableEventPipe");
                    if ((strEnabledValue == null) || (!int.TryParse(strEnabledValue, out s_Config_EnableEventPipe)))
                    {
                        s_Config_EnableEventPipe = 0;
                    }
                }

                return s_Config_EnableEventPipe;
            }
        }

        private static string Config_EventPipeConfig
        {
            get
            {
                if (s_Config_EventPipeConfig == null)
                {
                    s_Config_EventPipeConfig = CompatibilitySwitch.GetValueInternal("EventPipeConfig");
                }

                return s_Config_EventPipeConfig;
            }
        }

        private static uint Config_EventPipeCircularMB
        {
            get
            {
                if (s_Config_EventPipeCircularMB == 0)
                {
                    string strCircularMB = CompatibilitySwitch.GetValueInternal("EventPipeCircularMB");
                    if ((strCircularMB == null) || (!uint.TryParse(strCircularMB, out s_Config_EventPipeCircularMB)))
                    {
                        s_Config_EventPipeCircularMB = DefaultCircularBufferMB;
                    }
                }

                return s_Config_EventPipeCircularMB;
            }
        }

        private static string Config_EventPipeOutputFile
        {
            get
            {
                if (s_Config_EventPipeOutputFile == null)
                {
                    s_Config_EventPipeOutputFile = CompatibilitySwitch.GetValueInternal("EventPipeOutputFile");
                    if (s_Config_EventPipeOutputFile == null)
                    {
                        s_Config_EventPipeOutputFile = "Process-" + Win32Native.GetCurrentProcessId() + NetPerfFileExtension;
                    }
                }

                return s_Config_EventPipeOutputFile;
            }
        }

        #endregion Configuration
    }
}

#endif // FEATURE_PERFTRACING
