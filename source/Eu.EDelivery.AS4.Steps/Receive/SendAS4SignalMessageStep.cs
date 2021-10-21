using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using log4net;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    [Info("Send AS4 signal message")]
    [Description("Send AS4 signal message back to the original sender")]
    public class SendAS4SignalMessageStep : IStep
    {
        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null || messagingContext.AS4Message.IsEmpty)
            {
                Logger.Trace("No SignalMessage available to send");
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
                var repository = new DatastoreRepository(dataContext);
                var outMsgService = new OutMessageService(_config, repository, _messageBodyStore);

                IEnumerable<OutMessage> insertedMessageUnits = 
                    outMsgService.InsertAS4Message(
                        messagingContext.AS4Message, 
                        messagingContext.SendingPMode,
                        messagingContext.ReceivingPMode);

                await dataContext.SaveChangesAsync()
                                 .ConfigureAwait(false);

                ReplyHandling replyHandling = messagingContext.ReceivingPMode?.ReplyHandling;
                ReplyPattern? replyPattern = replyHandling?.ReplyPattern;
                if (replyPattern == ReplyPattern.PiggyBack
                    && replyHandling?.PiggyBackReliability?.IsEnabled == true)
                {
                    var piggyBackService = new PiggyBackingService(dataContext);
                    piggyBackService.InsertRetryForPiggyBackedSignalMessages(
                        insertedMessageUnits,
                        messagingContext.ReceivingPMode?.ReplyHandling?.PiggyBackReliability);

                    await dataContext.SaveChangesAsync()
                                     .ConfigureAwait(false);
                }
            }
        }

        private static StepResult CreateEmptySoapResult(MessagingContext messagingContext)
        {
            Logger.Debug("Empty Accepted response will be send to requested party since signal will be sent async");
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

                MessageUnit primaryMessageUnit = context.AS4Message.PrimaryMessageUnit;
                string errorDescriptions =
                    primaryMessageUnit is Error error
                        ? ": " + ConcatErrorDescriptions(error)
                        : String.Empty;

                Logger.Info(
                    $"({Config.Encode(context.Mode)}) <- response with {Config.Encode(primaryMessageUnit.GetType().Name)} {Config.Encode(primaryMessageUnit.MessageId)} {Config.Encode(errorDescriptions)}");
            }

            return StepResult.Success(context);
        }
    }
}
