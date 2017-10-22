// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

/*============================================================
**
**
**
** Purpose: Debugging Macros for use in the Base Class Libraries
**
**
============================================================*/

namespace System
{
    using System.IO;
    using System.Text;
    using System.Diagnostics;
    using Microsoft.Win32;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security;

    internal enum LogLevel
    {
        Trace = 0,
        Status = 20,
        Warning = 40,
        Error = 50,
        Panic = 100,
    }

    internal struct SwitchStructure
    {
        internal String name;
        internal int value;

        internal SwitchStructure(String n, int v)
        {
            name = n;
            value = v;
        }
    }


    // Only statics, does not need to be marked with the serializable attribute
    internal static class BCLDebug
    {
        internal static volatile bool m_registryChecked = false;
        internal static volatile bool m_loggingNotEnabled = false;
#if _DEBUG
        internal static volatile bool m_domainUnloadAdded;
#endif

        private static readonly SwitchStructure[] switches = {
            new SwitchStructure("NLS",  0x00000001),
            new SwitchStructure("SER",  0x00000002),
            new SwitchStructure("DYNIL",0x00000004),
            new SwitchStructure("REMOTE",0x00000008),
            new SwitchStructure("BINARY",0x00000010),   //Binary Formatter
            new SwitchStructure("SOAP",0x00000020),     // Soap Formatter
            new SwitchStructure("REMOTINGCHANNELS",0x00000040),
            new SwitchStructure("CACHE",0x00000080),
            new SwitchStructure("RESMGRFILEFORMAT", 0x00000100), // .resources files
            new SwitchStructure("PERF", 0x00000200),
            new SwitchStructure("CORRECTNESS", 0x00000400),
            new SwitchStructure("MEMORYFAILPOINT", 0x00000800),
            new SwitchStructure("DATETIME", 0x00001000), // System.DateTime managed tracing
            new SwitchStructure("INTEROP",  0x00002000), // Interop tracing
        };

        private static readonly LogLevel[] levelConversions = {
            LogLevel.Panic,
            LogLevel.Error,
            LogLevel.Error,
            LogLevel.Warning,
            LogLevel.Warning,
            LogLevel.Status,
            LogLevel.Status,
            LogLevel.Trace,
            LogLevel.Trace,
            LogLevel.Trace,
            LogLevel.Trace
        };

        [Conditional("_LOGGING")]
        static public void Log(String message)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
                return;
            if (!m_registryChecked)
            {
                CheckRegistry();
            }
            System.Diagnostics.Log.Trace(message);
            System.Diagnostics.Log.Trace(Environment.NewLine);
        }

        [Conditional("_LOGGING")]
        static public void Log(String switchName, String message)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
                return;
            if (!m_registryChecked)
            {
                CheckRegistry();
            }
            try
            {
                LogSwitch ls;
                ls = LogSwitch.GetSwitch(switchName);
                if (ls != null)
                {
                    System.Diagnostics.Log.Trace(ls, message);
                    System.Diagnostics.Log.Trace(ls, Environment.NewLine);
                }
            }
            catch
            {
                System.Diagnostics.Log.Trace("Exception thrown in logging." + Environment.NewLine);
                System.Diagnostics.Log.Trace("Switch was: " + ((switchName == null) ? "<null>" : switchName) + Environment.NewLine);
                System.Diagnostics.Log.Trace("Message was: " + ((message == null) ? "<null>" : message) + Environment.NewLine);
            }
        }

        //
        // This code gets called during security startup, so we can't go through Marshal to get the values.  This is
        // just a small helper in native code instead of that.
        //
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern static int GetRegistryLoggingValues(out bool loggingEnabled, out bool logToConsole, out int logLevel);

        private static void CheckRegistry()
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
                return;
            if (m_registryChecked)
            {
                return;
            }

            m_registryChecked = true;

            bool loggingEnabled;
            bool logToConsole;
            int logLevel;
            int facilityValue;
            facilityValue = GetRegistryLoggingValues(out loggingEnabled, out logToConsole, out logLevel);

            // Note we can get into some recursive situations where we call
            // ourseves recursively through the .cctor.  That's why we have the 
            // check for levelConversions == null.
            if (!loggingEnabled)
            {
                m_loggingNotEnabled = true;
            }
            if (loggingEnabled && levelConversions != null)
            {
                try
                {
                    //The values returned for the logging levels in the registry don't map nicely onto the
                    //values which we support internally (which are an approximation of the ones that 
                    //the System.Diagnostics namespace uses) so we have a quick map.
                    Debug.Assert(logLevel >= 0 && logLevel <= 10, "logLevel>=0 && logLevel<=10");
                    logLevel = (int)levelConversions[logLevel];

                    if (facilityValue > 0)
                    {
                        for (int i = 0; i < switches.Length; i++)
                        {
                            if ((switches[i].value & facilityValue) != 0)
                            {
                                LogSwitch L = new LogSwitch(switches[i].name, switches[i].name, System.Diagnostics.Log.GlobalSwitch);
                                L.MinimumLevel = (LoggingLevels)logLevel;
                            }
                        }
                        System.Diagnostics.Log.GlobalSwitch.MinimumLevel = (LoggingLevels)logLevel;
                        System.Diagnostics.Log.IsConsoleEnabled = logToConsole;
                    }
                }
                catch
                {
                    //Silently eat any exceptions.
                }
            }
        }

        internal static bool CheckEnabled(String switchName)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
                return false;
            if (!m_registryChecked)
                CheckRegistry();
            LogSwitch logSwitch = LogSwitch.GetSwitch(switchName);
            if (logSwitch == null)
            {
                return false;
            }
            return ((int)logSwitch.MinimumLevel <= (int)LogLevel.Trace);
        }

        private static bool CheckEnabled(String switchName, LogLevel level, out LogSwitch logSwitch)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
            {
                logSwitch = null;
                return false;
            }
            logSwitch = LogSwitch.GetSwitch(switchName);
            if (logSwitch == null)
            {
                return false;
            }
            return ((int)logSwitch.MinimumLevel <= (int)level);
        }

        [Conditional("_LOGGING")]
        public static void Log(String switchName, LogLevel level, params Object[] messages)
        {
            if (AppDomain.CurrentDomain.IsUnloadingForcedFinalize())
                return;
            //Add code to check if logging is enabled in the registry.
            LogSwitch logSwitch;

            if (!m_registryChecked)
            {
                CheckRegistry();
            }

            if (!CheckEnabled(switchName, level, out logSwitch))
            {
                return;
            }

            StringBuilder sb = StringBuilderCache.Acquire();

            for (int i = 0; i < messages.Length; i++)
            {
                String s;
                try
                {
                    if (messages[i] == null)
                    {
                        s = "<null>";
                    }
                    else
                    {
                        s = messages[i].ToString();
                    }
                }
                catch
                {
                    s = "<unable to convert>";
                }
                sb.Append(s);
            }
            System.Diagnostics.Log.LogMessage((LoggingLevels)((int)level), logSwitch, StringBuilderCache.GetStringAndRelease(sb));
        }

        [Conditional("_LOGGING")]
        public static void Trace(String switchName, String format, params Object[] messages)
        {
            if (m_loggingNotEnabled)
            {
                return;
            }

            LogSwitch logSwitch;
            if (!CheckEnabled(switchName, LogLevel.Trace, out logSwitch))
            {
                return;
            }

            StringBuilder sb = StringBuilderCache.Acquire();
            sb.AppendFormat(format, messages);
            sb.Append(Environment.NewLine);

            System.Diagnostics.Log.LogMessage(LoggingLevels.TraceLevel0, logSwitch, StringBuilderCache.GetStringAndRelease(sb));
        }
    }
}

