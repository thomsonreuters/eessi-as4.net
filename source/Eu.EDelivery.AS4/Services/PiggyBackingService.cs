using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Services
{
    internal class PiggyBackingService
    {
        private readonly DatastoreContext _context;
        private readonly IDatastoreRepository _repository;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="PiggyBackingService"/> class.
        /// </summary>
        public PiggyBackingService(DatastoreContext context)
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
        /// <param name="url">The url at which <see cref="PullRequest"/> are sent.</param>
        /// <param name="bodyStore">The body store at which the <see cref="SignalMessage"/>s are persisted.</param>
        /// <returns></returns>
        public async Task<IEnumerable<SignalMessage>> SelectToBePiggyBackedSignalMessagesAsync(
            PullRequest pr, 
            string url,
            IAS4MessageBodyStore bodyStore)
        {
            if (pr == null)
            {
                throw new ArgumentNullException(nameof(pr));
            }

            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException(@"Url cannot be null or whitespace.", nameof(url));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            return await _context.TransactionalAsync(async db =>
            {
                IEnumerable<OutMessage> query =
                    db.NativeCommands
                      .SelectToBePiggyBackedSignalMessages(url, pr.Mpc);

                var signals = new Collection<SignalMessage>();
                foreach (OutMessage found in query)
                {
                    found.Operation = Operation.Sending;

                    Stream body = await bodyStore.LoadMessageBodyAsync(found.MessageLocation);
                    AS4Message signal =
                        await SerializerProvider
                              .Default
                              .Get(found.ContentType)
                              .DeserializeAsync(body, found.ContentType, CancellationToken.None);

                    if (signal.PrimaryMessageUnit is Receipt r)
                    {
                        Logger.Debug($"Select Receipt {r.MessageId} for PiggyBacking");
                        signals.Add(r);
                    }
                    else if (signal.PrimaryMessageUnit is Error e)
                    {
                        Logger.Debug($"Select Error {e.MessageId} for PiggyBacking");
                        signals.Add(e);
                    }
                    else if (signal.PrimaryMessageUnit != null)
                    {
                        Logger.Warn(
                            $"Will not select {signal.PrimaryMessageUnit.GetType().Name} because only "
                            + "Receipts and Errors are allowed SignalMessages are allowed to be PiggyBacked with PullRequests");
                    }
                    else
                    {
                        Logger.Warn("Will not select AS4Message for PiggyBacking because it doesn't contains any Message Units");
                    }
                }

                if (query.Any())
                {
                    await db.SaveChangesAsync()
                            .ConfigureAwait(false);
                }

                return signals.AsEnumerable();
            });
        }

        /// <summary>
        /// Resets the PiggyBacked <see cref="SignalMessage"/>s back to its original <see cref="Operation.ToBePiggyBacked"/> state
        /// so it can be picked-up again by the next send-out <see cref="PullRequest"/>.
        /// </summary>
        /// <param name="signals">The <see cref="SignalMessage"/>s that should be resetted for PiggyBacking.</param>
        /// <param name="sendResult">The result of the bundling operation to use when resetting the <see cref="SignalMessage"/>s.</param>
        public void ResetSignalMessagesToBePiggyBacked(IEnumerable<SignalMessage> signals, SendResult sendResult)
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
        public void InsertRetryForPiggyBackedSignalMessages(IEnumerable<OutMessage> inserts, Model.PMode.RetryReliability reliability)
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
