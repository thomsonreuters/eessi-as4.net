using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.ReceptionAwareness
{
    /// <summary>
    /// Describes how the AS4 message has to be behave in a Reception Awareness scenario
    /// </summary>
    public class ReceptionAwarenessUpdateDatastoreStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IAS4MessageBodyStore _inMessageBodyStore;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep"/> class.
        /// </summary>
        public ReceptionAwarenessUpdateDatastoreStep()
            : this(Registry.Instance.MessageBodyStore, Registry.Instance.CreateDatastoreContext) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="ReceptionAwarenessUpdateDatastoreStep" /> class.
        /// </summary>
        /// <param name="inMessageBodyStore">The in message body store.</param>
        /// <param name="createContext">The create context.</param>
        public ReceptionAwarenessUpdateDatastoreStep(
            IAS4MessageBodyStore inMessageBodyStore,
            Func<DatastoreContext> createContext)
        {
            _inMessageBodyStore = inMessageBodyStore;
            _createContext = createContext;
        }

        /// <summary>
        /// Start updating the Data store
        /// </summary>
        /// <param name="messagingContext"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            Entities.ReceptionAwareness receptionAwareness = messagingContext.ReceptionAwareness;

            using (DatastoreContext context = _createContext())
            {
                Logger.Debug("Executing ReceptionAwarenessDataStoreStep");

                var repository = new DatastoreRepository(context);
                var service = new ReceptionAwarenessService(repository);

                context.Attach(receptionAwareness);

                await RunReceptionAwarenessFlow(receptionAwareness, service, cancellationToken);
                await context.SaveChangesAsync(cancellationToken);
            }

            WaitRetryInterval(receptionAwareness);
            return await StepResult.SuccessAsync(messagingContext);
        }

        private async Task RunReceptionAwarenessFlow(
            Entities.ReceptionAwareness receptionAwareness,
            ReceptionAwarenessService service,
            CancellationToken cancellationToken)
        {
            if (service.IsMessageAlreadyAnswered(receptionAwareness))
            {
                service.MarkReferencedMessageAsComplete(receptionAwareness);
            }
            else
            {
                if (service.MessageNeedsToBeResend(receptionAwareness))
                {
                    service.MarkReferencedMessageForResend(receptionAwareness);
                }
                else
                {
                    if (IsMessageUnanswered(receptionAwareness))
                    {
                        Logger.Debug("Message is unanswered.");

                        service.MarkReferencedMessageAsComplete(receptionAwareness);

                        await service.DeadletterOutMessageAsync(
                            messageId: receptionAwareness.InternalMessageId,
                            messageBodyStore: _inMessageBodyStore,
                            cancellationToken: cancellationToken);
                    }
                    else
                    {
                        service.ResetReferencedMessage(receptionAwareness);
                    }
                }
            }
        }

        private static bool IsMessageUnanswered(Entities.ReceptionAwareness receptionAwareness)
        {
            return receptionAwareness.CurrentRetryCount >= receptionAwareness.TotalRetryCount;
        }

        private static void WaitRetryInterval(Entities.ReceptionAwareness receptionAwareness)
        {
            TimeSpan retryInterval = TimeSpan.Parse(receptionAwareness.RetryInterval);
            string messageId = receptionAwareness.InternalMessageId;

            Logger.Info($"[{messageId}] Waiting retry interval...");
            Thread.Sleep(retryInterval);
        }
    }
}