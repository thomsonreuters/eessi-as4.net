using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Services.Journal
{
    /// <summary>
    /// Log entry representing a series of actions that has taken place on the message.
    /// </summary>
    public class JournalLogEntry
    {
        private readonly ICollection<string> _logEntries;

        /// <summary>
        /// Gets the ebMS message identifier of the message.
        /// </summary>
        public string EbmsMessageId { get; }

        /// <summary>
        /// Gets the reference to an ebMS message identifier of a message.
        /// </summary>
        public string RefToMessageId { get; }

        /// <summary>
        /// Gets the service value of the message.
        /// </summary>
        public string Service { get; }

        /// <summary>
        /// Gets the action of the message.
        /// </summary>
        public string Action { get; }

        /// <summary>
        /// Gets the receiver party's identifier of the message.
        /// </summary>
        public string ToParty { get; }

        /// <summary>
        /// Gets the sender party's identifier of the message.
        /// </summary>
        public string FromParty { get; }

        /// <summary>
        /// Gets a sequence of log entries about the message.
        /// </summary>
        public IEnumerable<string> LogEntries => _logEntries.AsEnumerable();

        /// <summary>
        /// Gets the name of the agent on which the message was processed.
        /// </summary>
        public string AgentName { get; private set; }

        /// <summary>
        /// Gets the type of the agent on which the message was processed.
        /// </summary>
        public AgentType? AgentType { get; private set; }

        private JournalLogEntry(
            string ebmsMessageId, 
            string refToMessageId, 
            string service, 
            string action, 
            string toParty, 
            string fromParty, 
            IEnumerable<string> logEntries)
        {
            EbmsMessageId = ebmsMessageId;
            RefToMessageId = refToMessageId;
            Service = service;
            Action = action;
            ToParty = toParty;
            FromParty = fromParty;
            _logEntries = logEntries.ToList();
        }

        private JournalLogEntry(
            string ebmsMessageId, 
            string refToMessageId, 
            IEnumerable<string> logEntries)
        {
            EbmsMessageId = ebmsMessageId;
            RefToMessageId = refToMessageId;
            _logEntries = logEntries.ToList();
        }

        /// <summary>
        /// Creates a <see cref="JournalLogEntry"/> based on a given <see cref="AS4Message"/>.
        /// </summary>
        /// <param name="msg">The message to create a log entry from.</param>
        /// <param name="details">The log details collected with the message.</param>
        public static JournalLogEntry CreateFrom(AS4Message msg, string details)
        {
            if (msg == null)
            {
                throw new ArgumentNullException(nameof(msg));
            }

            if (msg.IsEmpty)
            {
                throw new NotSupportedException(
                    $"Can't create {nameof(JournalLogEntry)} from an AS4Message without message units");
            }

            if (msg.PrimaryMessageUnit is UserMessage um)
            {
                return new JournalLogEntry(
                    um.MessageId,
                    um.RefToMessageId,
                    um.CollaborationInfo.Service.Value,
                    um.CollaborationInfo.Action,
                    um.Receiver.PrimaryPartyId,
                    um.Sender.PrimaryPartyId,
                    new [] { details });
            }

            return new JournalLogEntry(
                msg.PrimaryMessageUnit.MessageId,
                msg.PrimaryMessageUnit.RefToMessageId,
                new[] { details });
        }

        /// <summary>
        /// Adds the agent location on which the message was processed.
        /// </summary>
        /// <param name="a">The agent configuration containing information about the agent.</param>
        internal void AddAgentLocation(AgentConfig a)
        {
            if (a == null)
            {
                throw new ArgumentNullException(nameof(a));
            }

            AgentName = a.Name;
            AgentType = a.Type;
        }

        /// <summary>
        /// Adds a series of log entries to this journal log of a message.
        /// </summary>
        /// <param name="entries">The entries to add to this message journal.</param>
        internal void AddLogEntries(IEnumerable<string> entries)
        {
            if (entries == null)
            {
                throw new ArgumentNullException(nameof(entries));
            }

            if (entries.Any(e => e is null))
            {
                throw new ArgumentNullException(nameof(entries), @"One or more entries are 'null'");
            }

            foreach (string entry in entries)
            {
                _logEntries.Add(entry);
            }
        }
    }
}