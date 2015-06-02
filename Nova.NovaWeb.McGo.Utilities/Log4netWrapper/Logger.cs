using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using log4net.Config;
using System.Reflection;
using System.IO;

namespace Nova.NovaWeb.McGo.Utilities.Log4netWrapper
{


        /// <summary>
        /// Provides methods for logging.
        /// </summary>
        public static class Logger
        {
            private static bool isInitialized;

            static Logger()
            {
                isInitialized = false;
            }

            /// <summary>
            /// Initializes the logger.
            /// </summary>
            /// <exception cref="LoggingInitializationException">Thrown if logger is already initialized.</exception>
            public static void Initialize()
            {
                Initialize(null);
            }

            /// <summary>
            /// Initializes the logger to use a specific config file.
            /// </summary>
            /// <param name="configFile">The path of the config file.</param>
            /// <exception cref="LoggingInitializationException">Thrown if logger is already initialized.</exception>
            public static void Initialize(string configFile)
            {
                if (!isInitialized)
                {
                    if (!String.IsNullOrEmpty(configFile))
                        XmlConfigurator.ConfigureAndWatch(new FileInfo(configFile));
                    else
                        XmlConfigurator.Configure();
                    isInitialized = true;
                }
                else
                    throw new LoggingInitializationException("Logging has already been initialized.");
            }

            /// <summary>
            /// Logs an entry to all logs.
            /// </summary>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <exception cref="LoggingInitializationException">Thrown if logger has not been initialized.</exception>
            public static void Log(LoggingLevel loggingLevel, string message)
            {
                Log(loggingLevel, message, null, null);
            }

            /// <summary>
            /// Logs an entry to all logs.
            /// </summary>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <param name="loggingProperties">Any additional properties for the log as defined in the logging configuration.</param>
            /// <exception cref="LoggingInitializationException">Thrown if logger has not been initialized.</exception>
            public static void Log(LoggingLevel loggingLevel, string message, object loggingProperties)
            {
                Log(loggingLevel, message, loggingProperties, null);
            }

            /// <summary>
            /// Logs an entry to all logs.
            /// </summary>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <param name="loggingProperties">Any additional properties for the log as defined in the logging configuration.</param>
            /// <param name="exception">Any exception to be logged.</param>
            public static void Log(LoggingLevel loggingLevel, string message, object loggingProperties, Exception exception)
            {
                foreach (ILog log in LogManager.GetCurrentLoggers())
                    LogBase(log, loggingLevel, message, loggingProperties, exception);
            }

            /// <summary>
            /// Logs an entry to the specified log.
            /// </summary>
            /// <param name="logName">The name of the log.</param>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <exception cref="InvalidLogException">Thrown if <paramref name="logName"/> does not exist.</exception>
            public static void Log(string logName, LoggingLevel loggingLevel, string message)
            {
                Log(logName, loggingLevel, message, null, null);
            }

            /// <summary>
            /// Logs an entry to the specified log.
            /// </summary>
            /// <param name="logName">The name of the log.</param>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <param name="loggingProperties">Any additional properties for the log as defined in the logging configuration.</param>
            /// <exception cref="InvalidLogException">Thrown if <paramref name="logName"/> does not exist.</exception>
            public static void Log(string logName, LoggingLevel loggingLevel, string message, object loggingProperties)
            {
                Log(logName, loggingLevel, message, loggingProperties, null);
            }

            /// <summary>
            /// Logs an entry to the specified log.
            /// </summary>
            /// <param name="logName">The name of the log.</param>
            /// <param name="loggingLevel">The logging level.</param>
            /// <param name="message">The message.</param>
            /// <param name="loggingProperties">Any additional properties for the log as defined in the logging configuration.</param>
            /// <param name="exception">Any exception to be logged.</param>
            /// <exception cref="InvalidLogException">Thrown if <paramref name="logName"/> does not exist.</exception>
            public static void Log(string logName, LoggingLevel loggingLevel, string message, object loggingProperties, Exception exception)
            {
                ILog log = LogManager.GetLogger(logName);
                if (log != null)
                    LogBase(log, loggingLevel, message, loggingProperties, exception);
                else
                    throw new InvalidLogException("The log \"" + logName + "\" does not exist or is invalid.", logName);
            }

            private static void LogBase(ILog log, LoggingLevel loggingLevel, string message, object loggingProperties, Exception exception)
            {
                if (ShouldLog(log, loggingLevel))
                {
                    PushLoggingProperties(loggingProperties);
                    switch (loggingLevel)
                    {
                        case LoggingLevel.Debug: log.Debug(message, exception); break;
                        case LoggingLevel.Info: log.Info(message, exception); break;
                        case LoggingLevel.Warning: log.Warn(message, exception); break;
                        case LoggingLevel.Error: log.Error(message, exception); break;
                        case LoggingLevel.Fatal: log.Fatal(message, exception); break;
                    }
                    PopLoggingProperties(loggingProperties);
                }
            }

            private static void PushLoggingProperties(object loggingProperties)
            {
                if (loggingProperties != null)
                {
                    Type attrType = loggingProperties.GetType();
                    PropertyInfo[] properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    for (int i = 0; i < properties.Length; i++)
                    {
                        object value = properties[i].GetValue(loggingProperties, null);
                        if (value != null)
                            ThreadContext.Stacks[properties[i].Name].Push(value.ToString());
                    }
                }
            }

            private static void PopLoggingProperties(object loggingProperties)
            {
                if (loggingProperties != null)
                {
                    Type attrType = loggingProperties.GetType();
                    PropertyInfo[] properties = attrType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    for (int i = properties.Length - 1; i >= 0; i--)
                    {
                        object value = properties[i].GetValue(loggingProperties, null);
                        if (value != null)
                            ThreadContext.Stacks[properties[i].Name].Pop();
                    }
                }
            }

            private static bool ShouldLog(ILog log, LoggingLevel loggingLevel)
            {
                switch (loggingLevel)
                {
                    case LoggingLevel.Debug: return log.IsDebugEnabled;
                    case LoggingLevel.Info: return log.IsInfoEnabled;
                    case LoggingLevel.Warning: return log.IsWarnEnabled;
                    case LoggingLevel.Error: return log.IsErrorEnabled;
                    case LoggingLevel.Fatal: return log.IsFatalEnabled;
                    default: return false;
                }
            }
        }
    
}
