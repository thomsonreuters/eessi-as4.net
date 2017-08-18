using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Eu.EDelivery.AS4.Common
{

    public class TraceLogger : ILogger
    {
        private static readonly TraceSwitch EntityFrameworkLogSwitch = new TraceSwitch("EfLoggingSwitch", "Entity Framework SQL logging switch");
        private static readonly TraceSource Tracer = new TraceSource("Eu.EDelivery.AS4.Common.DatastoreContext");
        
        public IDisposable BeginScope<TState>(TState state) => null;

        private static readonly Dictionary<LogLevel, TraceEventType> LogLevelMap = new Dictionary<LogLevel, TraceEventType>()
        {
            { LogLevel.Critical, TraceEventType.Critical },
            { LogLevel.Debug, TraceEventType.Verbose },
            { LogLevel.Error, TraceEventType.Error },
            { LogLevel.Information, TraceEventType.Information },
            { LogLevel.Trace, TraceEventType.Verbose },
            { LogLevel.Warning, TraceEventType.Warning},
        };

        /// <summary>Writes a log entry.</summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (IsEnabled(logLevel))
            {
                try
                {
                    Tracer.TraceEvent(LogLevelMap[logLevel], eventId.Id, $"{DateTime.Now.ToString("o", CultureInfo.CurrentCulture)} {logLevel} {formatter(state, exception)}");
                }
                catch (Exception ex)
                {
                    NLog.LogManager.GetCurrentClassLogger().Error(ex.Message);
                    throw;
                }
            }
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Critical:
                case LogLevel.Error:
                    return EntityFrameworkLogSwitch.TraceError;

                case LogLevel.Warning:
                    return EntityFrameworkLogSwitch.TraceWarning;

                case LogLevel.Information:
                    return EntityFrameworkLogSwitch.TraceInfo;

                case LogLevel.Debug:
                case LogLevel.Trace:
                    return EntityFrameworkLogSwitch.TraceVerbose;

                case LogLevel.None:
                    return false;

                default:
                    return false;
            }
        }
    }

}