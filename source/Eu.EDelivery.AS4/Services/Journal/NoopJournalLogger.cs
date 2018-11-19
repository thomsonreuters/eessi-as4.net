using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Null object implementation that ignores the journal log entries.
    /// </summary>
    public class NoopJournalLogger : IJournalLogger
    {
        /// <summary>
        /// Flyweight instance of this null object implementation.
        /// </summary>
        public static readonly IJournalLogger Instance = new NoopJournalLogger();

        /// <summary>
        /// Prevents default instances of the <see cref="NoopJournalLogger"/> class from being created.
        /// </summary>
        private NoopJournalLogger() { }

        /// <summary>
        /// Writes out a given journal log <paramref name="entries"/>.
        /// </summary>
        /// <param name="entries">The entry that must be written.</param>
        public Task WriteLogEntriesAsync(IEnumerable<JournalLogEntry> entries)
        {
            return Task.CompletedTask;
        }
    }
}
