using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Services.Journal;
using NLog;

namespace Eu.EDelivery.AS4.Steps
{
    /// <summary>
    /// Result given when a <see cref="IStep" /> is finished executing
    /// </summary>
    public class StepResult
    {
        private readonly ICollection<JournalLogEntry> _journal;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private StepResult(bool succeeded, bool canProceed, MessagingContext context)
        {
            _journal = new Collection<JournalLogEntry>();

            Succeeded = succeeded;
            MessagingContext = context;
            CanProceed = canProceed;
        }

        private StepResult(
            bool succeeded,
            bool canProceed,
            MessagingContext context,
            IEnumerable<JournalLogEntry> journal)
        {
            _journal = journal.ToList();

            Succeeded = succeeded;
            MessagingContext = context;
            CanProceed = canProceed;
        }

        /// <summary>
        /// Gets the included <see cref="MessagingContext"/> send throughout the step execution.
        /// </summary>
        public MessagingContext MessagingContext { get; }
        
        /// <summary>
        /// Gets a value indicating whether the next steps must be executed.
        /// </summary>
        public bool CanProceed { get; }

        /// <summary>
        /// Gets a value indicating whether [was succesful].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [was succesful]; otherwise, <c>false</c>.
        /// </value>
        public bool Succeeded { get; }

        /// <summary>
        /// Gets the complete journal that the message has gone through.
        /// </summary>
        internal IEnumerable<JournalLogEntry> Journal => _journal.AsEnumerable();

        /// <summary>
        /// Replace the entire journal of the message with a new complete list of new entries.
        /// </summary>
        /// <param name="entries">The list containing the complete journal for the message.</param>
        internal StepResult WithJournal(IEnumerable<JournalLogEntry> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            if (entries.Any(e => e is null))
            {
                throw new ArgumentNullException(nameof(entries), @"One or more entries are 'null'");
            }

            return new StepResult(Succeeded, CanProceed, MessagingContext, entries);
        }

        /// <summary>
        /// Promote the <see cref="StepResult"/> with a <see cref="JournalLogEntry"/>.
        /// </summary>
        /// <param name="entry">The entry containing information about the message.</param>
        public StepResult WithJournal(JournalLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }

            Logger.Trace($"Append log to message journal: {String.Join(", ", entry.LogEntries)}");
            return new StepResult(Succeeded, CanProceed, MessagingContext, _journal.Concat(new[] { entry }));
        }

        /// <summary>
        /// Promote the <see cref="StepResult"/> with a <see cref="JournalLogEntry"/>.
        /// </summary>
        /// <param name="entry">The entry containing information about the message.</param>
        public Task<StepResult> WithJournalAsync(JournalLogEntry entry)
        {
            if (entry == null)
            {
                throw new ArgumentNullException(nameof(entry));
            }
            
            return Task.FromResult(WithJournal(entry));
        }

        /// <summary>
        /// Promote the <see cref="StepResult"/> to stop the execution.
        /// </summary>
        /// <returns></returns>
        public StepResult AndStopExecution()
        {
            return new StepResult(Succeeded, canProceed: false, context: MessagingContext);
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Failed(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new StepResult(succeeded: false, canProceed: true, context: context);
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static StepResult Success(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return new StepResult(succeeded: true, canProceed: true, context: context);
        }

        /// <summary>
        /// Return a Successful <see cref="StepResult" />
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> SuccessAsync(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Success(context));
        }

        /// <summary>
        /// Return a Failed <see cref="StepResult" />.
        /// </summary>
        /// <param name="context">The <see cref="MessagingContext"/>.</param>
        /// <returns></returns>
        public static Task<StepResult> FailedAsync(MessagingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            return Task.FromResult(Failed(context));
        }
    }
}