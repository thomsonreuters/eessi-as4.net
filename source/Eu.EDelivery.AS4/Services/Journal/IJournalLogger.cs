using System.Collections.Generic;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Contract for loggers to write journal entries to external systems.
    /// </summary>
    public interface IJournalLogger
    {
        /// <summary>
        /// Writes out a given journal log <paramref name="entries"/>.
        /// </summary>
        /// <param name="entries">The entry that must be written.</param>
        Task WriteLogEntriesAsync(IEnumerable<JournalLogEntry> entries);
    }
}