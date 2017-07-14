using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    public class SendAS4SignalMessageStep : IStep
    {
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep"/> class.
        /// </summary>
        public SendAS4SignalMessageStep() : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep"/> class.
        /// </summary>
        public SendAS4SignalMessageStep(Func<DatastoreContext> createDatastoreContext, IAS4MessageBodyStore messageBodyStore)
        {
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                return await ReturnSameStepResult(messagingContext);
            }

            using (var dataContext = _createDatastoreContext())
            {
                var repository = new DatastoreRepository(dataContext);

                await StoreSignalMessage(messagingContext, repository, _messageBodyStore, cancellationToken);

                await dataContext.SaveChangesAsync(cancellationToken);
            }

            if (IsReplyPatternCallback(messagingContext))
            {
                return await CreateEmptySoapResult(messagingContext);
            }
            else
            {
                return await ReturnSameStepResult(messagingContext);
            }

        }

        private static bool IsReplyPatternCallback(MessagingContext message)
        {
            return message.ReceivingPMode?.ReplyHandling?.ReplyPattern == ReplyPattern.Callback;
        }

        private static async Task StoreSignalMessage(
            MessagingContext messagingContext,
            IDatastoreRepository repository,
            IAS4MessageBodyStore messageBodyStore,
            CancellationToken cancellationToken)
        {
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                await new OutMessageService(repository, messageBodyStore).InsertAS4Message(
                    messagingContext,
                    Operation.NotApplicable, // The service will determine the correct reply-pattern.
                    cancellationToken);

                await context.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        private static async Task<StepResult> CreateEmptySoapResult(MessagingContext messagingContext)
        {
            LogManager.GetCurrentClassLogger()
                      .Info($"{messagingContext.Prefix} Empty SOAP Envelope will be send to requested party");

            AS4Message as4Message = AS4Message.Create(messagingContext.SendingPMode);
            var emptyContext = new MessagingContext(as4Message, MessagingContextMode.Receive)
            {
                ReceivingPMode = messagingContext.ReceivingPMode
            };

            return await StepResult.SuccessAsync(emptyContext);
        }

        private static async Task<StepResult> ReturnSameStepResult(MessagingContext messagingContext)
        {
            return await StepResult.SuccessAsync(messagingContext);
        }
    }
}
