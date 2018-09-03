using System;
using System.ComponentModel;
using System.Linq;
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
    [Info("Send AS4 signal message")]
    [Description("Send AS4 signal message back to the original sender")]
    public class SendAS4SignalMessageStep : IStep
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;
        private readonly Func<DatastoreContext> _createDatastoreContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep" /> class.
        /// </summary>
        public SendAS4SignalMessageStep()
            : this(Config.Instance, Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SendAS4SignalMessageStep"/> class.
        /// </summary>
        /// <param name="configuration">The configuration.</param>
        /// <param name="createDatastoreContext">The create datastore context.</param>
        /// <param name="messageBodyStore">The message body store.</param>
        public SendAS4SignalMessageStep(
            IConfig configuration,
            Func<DatastoreContext> createDatastoreContext,
            IAS4MessageBodyStore messageBodyStore)
        {
            if (configuration == null)
            {
                throw new ArgumentNullException(nameof(configuration));
            }

            if (createDatastoreContext == null)
            {
                throw new ArgumentNullException(nameof(createDatastoreContext));
            }

            if (messageBodyStore == null)
            {
                throw new ArgumentNullException(nameof(messageBodyStore));
            }

            _config = configuration;
            _createDatastoreContext = createDatastoreContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Start executing the Receipt Decorator
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                Logger.Debug("No SignalMessage available to send");
                return StepResult.Success(messagingContext);
            }

            await InsertRespondSignalToDatastore(messagingContext);

            ReplyPattern? replyPattern = messagingContext.ReceivingPMode?.ReplyHandling?.ReplyPattern;
            if (replyPattern == ReplyPattern.Callback || messagingContext.Mode == MessagingContextMode.PullReceive)
            {
                return CreateEmptySoapResult(messagingContext);
            }

            return CreateSignalResult(messagingContext);
        }

        private async Task InsertRespondSignalToDatastore(MessagingContext messagingContext)
        {
            using (DatastoreContext dataContext = _createDatastoreContext())
            {
                var service = new OutMessageService(
                    _config,
                    new DatastoreRepository(dataContext),
                    _messageBodyStore);

                service.InsertAS4Message(messagingContext, Operation.NotApplicable);
                await dataContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }

        private static StepResult CreateEmptySoapResult(MessagingContext messagingContext)
        {
            Logger.Info(
                $"{messagingContext.LogTag} Empty Accepted response will be send " + 
                "to requested party since signal will be sent async");

            return StepResult.Success(
                new MessagingContext(
                    AS4Message.Create(messagingContext.SendingPMode),
                    MessagingContextMode.Receive)
                {
                    ReceivingPMode = messagingContext.ReceivingPMode
                });
        }

        private static StepResult CreateSignalResult(MessagingContext context)
        {
            if (Logger.IsInfoEnabled)
            {
                string ConcatErrorDescriptions(Error e)
                {
                    if (e.ErrorLines != null)
                    {
                        return String.Join(", ", e.ErrorLines.Select(er => er.Detail).Choose(x => x));
                    }

                    return String.Empty;
                }

                string errorDescriptions =
                    context.AS4Message.FirstSignalMessage is Error error
                        ? ": " + ConcatErrorDescriptions(error)
                        : String.Empty;

                Logger.Info(
                    $"{context.LogTag} {context.AS4Message.FirstSignalMessage.GetType().Name} " +
                    $"will be written to the response {errorDescriptions}");
            }

            return StepResult.Success(context);
        }
    }
}
