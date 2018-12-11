using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;
using RetryReliability = Eu.EDelivery.AS4.Entities.RetryReliability;

namespace Eu.EDelivery.AS4.Services
{
    /// <summary>
    /// Service that centralizes the functionality related to the Piggy-Back approach of bundling <see cref="SignalMessage"/>s to <see cref="PullRequest"/>s.
    /// </summary>
    internal class PiggyBackingService
    {
        private readonly DatastoreContext _context;
        private readonly IDatastoreRepository _repository;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PiggyBackingService"/> class.
        /// </summary>
        internal PiggyBackingService(DatastoreContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _context = context;
            _repository = new DatastoreRepository(_context);
        }

        /// <summary>
        /// Selects the available <see cref="SignalMessage"/>s that are ready to be bundled (PiggyBacked) with the given <see cref="PullRequest"/>.
        /// </summary>
        /// <param name="pr">The <see cref="PullRequest"/> for which a selection of <see cref="SignalMessage"/>s are returned.</param>
        /// <param name="sendingPMode">The sending configuration used to select <see cref="SignalMessage"/>s with the same configuration.</param>
        /// <param name="bodyStore">The body store at which the <see cref="SignalMessage"/>s are persisted.</param>
        /// <returns>
        ///     An subsection of the <see cref="SignalMessage"/>s where the referenced send <see cref="UserMessage"/> matches the given <paramref name="pr"/>
        ///     and where the sending configuration given in the <paramref name="sendingPMode"/> matches the stored <see cref="SignalMessage"/> sending configuration.
        /// </returns>
        internal async Task<IEnumerable<SignalMessage>> SelectToBePiggyBackedSignalMessagesAsync(
            PullRequest pr, 
            SendingProcessingMode sendingPMode,
            IAS4MessageBodyStore bodyStore)
        {
            if (pr == null)
            {
                throw new ArgumentNullException(nameof(pr));
            }

            if (sendingPMode == null)
            {
                throw new ArgumentNullException(nameof(sendingPMode));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            string url = sendingPMode.PushConfiguration?.Protocol?.Url;
            if (String.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(@"Url cannot be null or whitespace.", nameof(sendingPMode.PushConfiguration.Protocol.Url));
            }

            bool pullRequestSigned = sendingPMode.Security?.Signing?.IsEnabled == true;
            return await _context.TransactionalAsync(async db =>
            {
                IEnumerable<OutMessage> query =
                    db.NativeCommands
                      .SelectToBePiggyBackedSignalMessages(url, pr.Mpc);

                var toBePiggyBackedSignals = new Collection<MessageUnit>();
                foreach (OutMessage found in query)
                {
                    Stream body = await bodyStore.LoadMessageBodyAsync(found.MessageLocation);
                    AS4Message signal =
                        await SerializerProvider
                              .Default
                              .Get(found.ContentType)
                              .DeserializeAsync(body, found.ContentType);

                    var toBePiggyBacked = signal.SignalMessages.FirstOrDefault(s => s.MessageId == found.EbmsMessageId);
                    if (toBePiggyBacked is Receipt || toBePiggyBacked is Error)
                    {
                        if (!pullRequestSigned && signal.IsSigned)
                        {
                            Logger.Warn(
                                $"Can't PiggyBack {toBePiggyBacked.GetType().Name} {toBePiggyBacked.MessageId} because SignalMessage is signed "
                                + $"while the SendingPMode {sendingPMode.Id} used is not configured for signing");
                        }
                        else
                        {
                            found.Operation = Operation.Sending;
                            toBePiggyBackedSignals.Add(toBePiggyBacked);
                        }
                    }
                    else if (toBePiggyBacked != null)
                    {
                        Logger.Warn(
                            $"Will not select {toBePiggyBacked.GetType().Name} {toBePiggyBacked.MessageId} "
                            + "for PiggyBacking because only Receipts and Errors are allowed SignalMessages to be PiggyBacked with PullRequests");
                    }
                    else
                    {
                        Logger.Warn("Will not select AS4Message for PiggyBacking because it doesn't contains any Message Units");
                    }
                }

                if (toBePiggyBackedSignals.Any())
                {
                    await db.SaveChangesAsync()
                            .ConfigureAwait(false);
                }

                return toBePiggyBackedSignals.Cast<SignalMessage>().AsEnumerable();
            });
        }

        /// <summary>
        /// Resets the PiggyBacked <see cref="SignalMessage"/>s back to its original <see cref="Operation.ToBePiggyBacked"/> state
        /// so it can be picked-up again by the next send-out <see cref="PullRequest"/>.
        /// </summary>
        /// <param name="signals">The <see cref="SignalMessage"/>s that should be resetted for PiggyBacking.</param>
        /// <param name="sendResult">The result of the bundling operation to use when resetting the <see cref="SignalMessage"/>s.</param>
        internal void ResetSignalMessagesToBePiggyBacked(IEnumerable<SignalMessage> signals, SendResult sendResult)
        {
            if (signals == null)
            {
                throw new ArgumentNullException(nameof(signals));
            }

            IEnumerable<string> nonPrSignals =
                signals.Where(s => !(s is PullRequest))
                       .Select(s => s.MessageId);

            SendResult neverFatalResult =
                sendResult == SendResult.FatalFail
                    ? SendResult.RetryableFail
                    : sendResult;

            if (neverFatalResult == SendResult.Success)
            {
                Logger.Debug("PiggyBacked SignalMessage(s) was/were correctly send to the sender MSH");
            }
            else if (neverFatalResult == SendResult.RetryableFail)
            {
                Logger.Debug("Reset PiggyBacked SignalMessage(s) for the next PullRequest because it was not correctly send to the sender MSH");
            }

            if (nonPrSignals.Any())
            {
                var retryService = new MarkForRetryService(_repository);

                IEnumerable<long> ids = 
                    _repository.GetOutMessageData(m => nonPrSignals.Contains(m.EbmsMessageId), m => m.Id);

                if (ids.Any())
                {
                    foreach (long id in ids)
                    {
                        retryService.UpdateAS4MessageForSendResult(id, neverFatalResult);
                    }
                }
                else
                {
                    Logger.Warn(
                        "No stored SignalMessage can be found to reset for PiggyBacking, "
                        + "are you sure that the bundled SignalMessages with PullRequest are stored?");
                }
            }
            else
            {
                Logger.Debug("No SignalMessages bundled with PullRequest to reset for PiggyBacking");
            }
        }

        /// <summary>
        /// Mark the stored <see cref="OutMessage"/> for retry/delayed piggy backing.
        /// </summary>
        /// <param name="inserts"></param>
        /// <param name="reliability"></param>
        internal void InsertRetryForPiggyBackedSignalMessages(IEnumerable<OutMessage> inserts, Model.PMode.RetryReliability reliability)
        {
            if (reliability?.IsEnabled == true)
            {
                foreach (OutMessage m in inserts.Where(i => i.Operation == Operation.ToBePiggyBacked))
                {
                    var r = RetryReliability.CreateForOutMessage(
                        refToOutMessageId: m.Id,
                        maxRetryCount: reliability.RetryCount,
                        retryInterval: reliability.RetryInterval.AsTimeSpan(),
                        type: RetryType.PiggyBack);

                    Logger.Debug(
                        $"Insert RetryReliability for ToBePiggyBacked SignalMessage OutMessage {m.EbmsMessageId} with "
                        + $"{{RetryCount={r.MaxRetryCount}, RetryInterval={r.RetryInterval}}}");

                    _repository.InsertRetryReliability(r);
                }
            }
            else
            {
                Logger.Debug("Will not insert RetryReliability because ReceivingPMode.ReplyHandling.Reliability is not enabeld");
            }
        }
    }
}
