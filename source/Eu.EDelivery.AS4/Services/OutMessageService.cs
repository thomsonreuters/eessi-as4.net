using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Entities;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;
using ReceptionAwareness = Eu.EDelivery.AS4.Model.PMode.ReceptionAwareness;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service to expose db operations related to messages that needs to be send out, 
    /// either directly via the Send Agent or via the Outbound Processing Agent.
    /// </summary>
    internal class OutMessageService
    {
        private readonly IDatastoreRepository _repository;
        private readonly IAS4MessageBodyStore _messageBodyStore;
        private readonly IConfig _configuration;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService"/> class. 
        /// </summary>
        /// <param name="repository">The repository used to insert and update <see cref="OutMessage"/>s.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IDatastoreRepository repository, IAS4MessageBodyStore messageBodyStore)
            : this(Config.Instance, repository, messageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="OutMessageService" /> class.
        /// </summary>
        /// <param name="config">The configuration used to retrieve the response <see cref="SendingProcessingMode"/> while inserting messages and the store location for <see cref="OutMessage"/>s.</param>
        /// <param name="repository">The repository used to insert and update <see cref="OutMessage"/>s.</param>
        /// <param name="messageBodyStore">The <see cref="IAS4MessageBodyStore"/> that must be used to persist the AS4 Message Body.</param>
        public OutMessageService(IConfig config, IDatastoreRepository repository, IAS4MessageBodyStore messageBodyStore)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (repository == null)
            {
                throw new ArgumentNullException(nameof(repository));
            }

            if (messageBodyStore == null)
            {
                throw new ArgumentNullException(nameof(messageBodyStore));
            }

            _configuration = config;
            _repository = repository;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Gets the non-intermediary stored <see cref="AS4Message"/>s matching the specified ebMS <paramref name="messageIds"/>.
        /// </summary>
        /// <param name="messageIds">The ebMS message identifiers.</param>
        /// <returns></returns>
        public async Task<IEnumerable<AS4Message>> GetNonIntermediaryAS4UserMessagesForIds(IEnumerable<string> messageIds)
        {
            if (messageIds == null)
            {
                throw new ArgumentNullException(nameof(messageIds));
            }

            if (!messageIds.Any())
            {
                Logger.Debug("Specified ebMS message identifiers is empty");
                return Enumerable.Empty<AS4Message>();
            }

            IEnumerable<OutMessage> messages = 
                _repository.GetOutMessageData(
                               m => messageIds.Contains(m.EbmsMessageId) 
                                    && m.Intermediary == false,
                               m => m)
                           .Where(m => m != null);

            if (!messages.Any())
            {
                return Enumerable.Empty<AS4Message>();
            }

            var foundMessages = new Collection<AS4Message>();

            foreach (OutMessage m in messages)
            {
                Stream body = await _messageBodyStore.LoadMessageBodyAsync(m.MessageLocation);
                AS4Message foundMessage = 
                    await Registry.Instance.SerializerProvider
                                  .Get(m.ContentType)
                                  .DeserializeAsync(body, m.ContentType);

                foundMessages.Add(foundMessage);
            }

            return foundMessages.AsEnumerable();
        }

        /// <summary>
        /// Inserts all the message units of the specified <paramref name="as4Message"/> as <see cref="OutMessage"/> records 
        /// each containing the appropriate Status and Operation.
        /// User messages will be set to <see cref="Operation.ToBeProcessed"/>
        /// Signal messages that must be async returned will be set to <see cref="Operation.ToBeSent"/>.
        /// </summary>
        /// <param name="as4Message">The message for which the containing message units will be inserted.</param>
        /// <param name="sendingPMode">The processing mode that will be stored with each message unit if present.</param>
        /// <param name="receivingPMode">The processing mode that will be used to determine if the signal messages must be async returned and for determining the response pmode if necessary.</param>
        public IEnumerable<OutMessage> InsertAS4Message(
            AS4Message as4Message,
            SendingProcessingMode sendingPMode,
            ReceivingProcessingMode receivingPMode)
        {
            if (as4Message == null)
            {
                throw new ArgumentNullException(nameof(as4Message));
            }

            if (!as4Message.MessageUnits.Any())
            {
                Logger.Debug("Incoming AS4Message hasn't got any message units to insert");
                return Enumerable.Empty<OutMessage>();
            }

            string messageBodyLocation =
                _messageBodyStore.SaveAS4Message(
                    _configuration.OutMessageStoreLocation,
                    as4Message);

            IDictionary<string, MessageExchangePattern> relatedInMessageMeps = 
                GetEbsmsMessageIdsOfRelatedSignals(as4Message);

            var results = new Collection<OutMessage>();
            foreach (MessageUnit messageUnit in as4Message.MessageUnits)
            {
                SendingProcessingMode pmode =
                    SendingOrResponsePMode(messageUnit, sendingPMode, receivingPMode);

                OutMessage outMessage =
                    OutMessageBuilder
                        .ForMessageUnit(messageUnit, as4Message.ContentType, pmode)
                        .Build();

                outMessage.Url = pmode?.PushConfiguration?.Protocol?.Url;
                outMessage.MessageLocation = messageBodyLocation;

                (OutStatus st, Operation op) =
                    DetermineReplyPattern(messageUnit, relatedInMessageMeps, receivingPMode);

                outMessage.SetStatus(st);
                outMessage.Operation = op;

                Logger.Debug($"Insert OutMessage {outMessage.EbmsMessageType} with {{Operation={outMessage.Operation}, Status={outMessage.Status}}}");
                _repository.InsertOutMessage(outMessage);
                results.Add(outMessage);
            }

            return results.AsEnumerable();
        }

        private IDictionary<string, MessageExchangePattern> GetEbsmsMessageIdsOfRelatedSignals(AS4Message as4Message)
        {
            IEnumerable<string> signalMessageIds =
                as4Message.SignalMessages
                          .Select(s => s.RefToMessageId)
                          .Where(id => id != null)
                          .Distinct();

            return _repository
                .GetInMessagesData(signalMessageIds, m => new { m.EbmsMessageId, m.MEP })
                .Distinct()
                .ToDictionary(r => r.EbmsMessageId, r => r.MEP);
        }

        private static (OutStatus, Operation) DetermineReplyPattern(
            MessageUnit mu,
            IDictionary<string, MessageExchangePattern> relatedInMessageMeps,
            ReceivingProcessingMode receivingPMode)
        {
            if (mu is UserMessage)
            {
                return (OutStatus.NotApplicable, Operation.ToBeProcessed);
            }

            string key = mu.RefToMessageId ?? string.Empty;
            if (!relatedInMessageMeps.ContainsKey(key))
            {
                return (OutStatus.Created, Operation.NotApplicable);
            }

            ReplyPattern replyPattern = receivingPMode?.ReplyHandling?.ReplyPattern ?? ReplyHandling.DefaultReplyPattern;

            bool userMessageWasSendViaPull = relatedInMessageMeps[key] == MessageExchangePattern.Pull;
            if (userMessageWasSendViaPull
                && replyPattern == ReplyPattern.Response)
            {
                throw new InvalidOperationException(
                    $"Cannot determine Status and Operation because ReceivingPMode {receivingPMode?.Id} ReplyHandling.ReplyPattern = Response "
                    + "while the UserMessage has been send via pulling. Please change the ReplyPattern to 'CallBack' or 'PiggyBack'");
            }

            bool signalShouldBePiggyBackedToPullRequest = replyPattern == ReplyPattern.PiggyBack;
            if (userMessageWasSendViaPull 
                && signalShouldBePiggyBackedToPullRequest)
            {
                return (OutStatus.Created, Operation.ToBePiggyBacked);
            }

            bool signalShouldBeRespondedAsync = replyPattern == ReplyPattern.Callback;
            if (signalShouldBeRespondedAsync)
            {
                return (OutStatus.Created, Operation.ToBeSent);
            }

            return (OutStatus.Sent, Operation.NotApplicable);
        }

        private SendingProcessingMode SendingOrResponsePMode(
            MessageUnit mu,
            SendingProcessingMode sendPMode,
            ReceivingProcessingMode receivePMode)
        {
            if (sendPMode?.Id != null)
            {
                Logger.Debug($"Use already set SendingPMode {sendPMode.Id} for inserting OutMessage");
                return sendPMode;
            }

            if (mu is SignalMessage
                && receivePMode != null
                && receivePMode.ReplyHandling?.ReplyPattern == ReplyPattern.Callback)
            {
                SendingProcessingMode pmode = _configuration.GetSendingPMode(receivePMode.ReplyHandling?.SendingPMode);
                Logger.Debug($"Use response SendingPMode {pmode.Id} from ReceivingPMode {receivePMode.Id} for inserting OutMessage");

                return pmode;
            }

            Logger.Warn("No SendingPMode was found as either directly set or as response PMode set in the ReceivingPMode");
            return null;
        }

        /// <summary>
        /// Updates a <see cref="AS4Message"/> by marking it as ready for sending.
        /// </summary>
        /// <param name="outMessageId">The Id that uniquely identifies the OutMessage record in the database.</param>
        /// <param name="message">The message to be sent.</param>
        /// <param name="awareness">The reliability reception awareness used during the sending of the message</param>
        public void UpdateAS4MessageToBeSent(
            long outMessageId,
            AS4Message message,
            ReceptionAwareness awareness)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            string messageBodyLocation =
                _repository.GetOutMessageData(outMessageId, m => m.MessageLocation);

            _messageBodyStore.UpdateAS4Message(messageBodyLocation, message);

            _repository.UpdateOutMessage(
                outMessageId,
                m =>
                {
                    m.Operation = Operation.ToBeSent;
                    m.MessageLocation = messageBodyLocation;
                    Logger.Debug($"Update {m.EbmsMessageType} OutMessage {m.EbmsMessageId} with {{Operation=ToBeSent}}");

                    if (awareness?.IsEnabled ?? false)
                    {
                        // When a multihop message is received by an i-MSH, that message must be forwarded to another MSH.
                        // (Send) RetryReliability should not be enabled for this message however; even if this is configured in the SendingPMode.
                        if (message.IsMultiHopMessage)
                        {
                            Logger.Warn(
                                "SendingPMode.Reliability.ReceptionAwareness.IsEnabled = true "
                                + "but the incoming message is a multihop message and must be forwarded");
                        }
                        else
                        {
                            var r = Entities.RetryReliability.CreateForOutMessage(
                                outMessageId,
                                awareness.RetryCount,
                                awareness.RetryInterval.AsTimeSpan(),
                                RetryType.Send);

                            Logger.Debug(
                                $"Insert RetryReliability for OutMessage {m.EbmsMessageId} with "
                                + $"{{RetryCount={awareness.RetryCount}, RetryInterval={awareness.RetryInterval}}}");

                            _repository.InsertRetryReliability(r);
                        }
                    }
                });
        }
    }
}