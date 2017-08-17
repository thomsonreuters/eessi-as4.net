using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.Extensions.Logging;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Eu.EDelivery.AS4.Common
{

    public class TraceLogger : ILogger
    {
        private readonly string _categoryName;

        private static readonly TraceSwitch EntityFrameworkLogSwitch = new TraceSwitch("EfLoggingSwitch", "Entity Framework SQL logging switch");

        public TraceLogger(string categoryName)
        {
            _categoryName = categoryName;
        }

        public IDisposable BeginScope<TState>(TState state) => null;

        /// <summary>Writes a log entry.</summary>
        /// <param name="logLevel">Entry will be written on this level.</param>
        /// <param name="eventId">Id of the event.</param>
        /// <param name="state">The entry to be written. Can be also an object.</param>
        /// <param name="exception">The exception related to this entry.</param>
        /// <param name="formatter">Function to create a <c>string</c> message of the <paramref name="state" /> and <paramref name="exception" />.</param>
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            Trace.WriteLineIf(EntityFrameworkLogSwitch.TraceVerbose, $"{DateTime.Now.ToString("o", CultureInfo.CurrentCulture)} {logLevel} {eventId.Id} {_categoryName}");
            Trace.WriteLineIf(EntityFrameworkLogSwitch.TraceVerbose, formatter(state, exception));
        }

        /// <summary>
        /// Checks if the given <paramref name="logLevel" /> is enabled.
        /// </summary>
        /// <param name="logLevel">level to be checked.</param>
        /// <returns><c>true</c> if enabled.</returns>
        public bool IsEnabled(LogLevel logLevel)
        {
            return EntityFrameworkLogSwitch.TraceVerbose;
        }
    }

}