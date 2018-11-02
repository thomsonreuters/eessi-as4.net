using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using NLog;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Journal logger implementation that writes journal entries to the datastore.
    /// </summary>
    internal class JournalDatastoreLogger : IJournalLogger
    {
        private readonly Func<DatastoreContext> _createDatastore;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="JournalDatastoreLogger"/> class.
        /// </summary>
        public JournalDatastoreLogger(Func<DatastoreContext> createDatastore)
        {
            if (createDatastore == null)
            {
                throw new ArgumentNullException(nameof(createDatastore));
            }

            _createDatastore = createDatastore;
        }

        /// <summary>
        /// Writes out a given journal log <paramref name="entries"/>.
        /// </summary>
        /// <param name="entries">The entries that must be written.</param>
        public async Task WriteLogEntriesAsync(IEnumerable<JournalLogEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            if (!entries.Any())
            {
                return;
            }

            using (DatastoreContext db = _createDatastore())
            {
                db.Journal.AddRange(
                    entries.Where(e => e != null)
                           .Select(CreateJournalRecord));

                await db.SaveChangesAsync(acceptAllChangesOnSuccess: false);
            }
        }

        private static Entities.Journal CreateJournalRecord(JournalLogEntry entry)
        {
            var entity = new Entities.Journal
            {
                EbmsMessageId = entry.EbmsMessageId,
                RefToEbmsMessageId = entry.RefToMessageId,
                Action = entry.Action,
                Service = entry.Service,
                FromParty = entry.FromParty,
                ToParty = entry.ToParty,
                LogEntry = String.Join(Environment.NewLine, entry.LogEntries),
                LogDate = DateTimeOffset.Now,
                AgentName = entry.AgentName
            };

            if (entry.AgentType.HasValue)
            {
                entity.SetAgentType(entry.AgentType.Value);
            }

            string ebmsMessageId =
                entry.EbmsMessageId == null
                    ? String.Empty
                    : "EbmsMessageId=" + entry.EbmsMessageId;

            string refToMessageId =
                entry.RefToMessageId == null
                    ? String.Empty
                    : "RefToMessageId=" + entry.RefToMessageId;

            Logger.Debug($"Add Journal entry for message {{{String.Join(", ", ebmsMessageId, refToMessageId)}}}");
            return entity;
        }
    }
}
