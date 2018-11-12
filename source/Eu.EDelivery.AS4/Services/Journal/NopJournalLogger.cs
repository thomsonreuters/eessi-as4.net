using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Null object implementation that ignores the journal log entries.
    /// </summary>
    public class NopJournalLogger : IJournalLogger
    {
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
