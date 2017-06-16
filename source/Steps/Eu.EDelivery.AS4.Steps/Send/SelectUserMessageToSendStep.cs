using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how a MessageUnit should be selected to be sent via Pulling.
    /// </summary>
    /// <seealso cref="IStep" />
    public class SelectUserMessageToSendStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectUserMessageToSendStep"/> class.
        /// </summary>
        public SelectUserMessageToSendStep()
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectUserMessageToSendStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageBodyStore">The message body store.</param>
        public SelectUserMessageToSendStep(Func<DatastoreContext> createContext, IAS4MessageBodyStore messageBodyStore)
        {
            _createContext = createContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(
            MessagingContext messagingContext,
            CancellationToken cancellationToken)
        {
            (bool hasMatch, OutMessage match) selection = SelectUserMessageFor(messagingContext);

            if (selection.hasMatch)
            {
                Logger.Info($"User Message found for Pull Request: '{messagingContext.AS4Message.GetPrimaryMessageId()}'");
                AS4Message referencedMessage = await RetrieveAS4UserMessage(selection.match, cancellationToken);
                messagingContext.SendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(selection.match.PMode);

                return SuccessStepResult(referencedMessage, messagingContext);
            }

            Logger.Warn($"No User Message found for Pull Request: '{messagingContext.AS4Message.GetPrimaryMessageId()}'");
            AS4Message pullRequestWarning = AS4Message.Create(new PullRequestError());
            return SuccessStepResult(pullRequestWarning, messagingContext).AndStopExecution();
        }

        private async Task<AS4Message> RetrieveAS4UserMessage(MessageEntity selection, CancellationToken cancellationToken)
        {
            // TODO: Attachment Contents are disposed?
            using (Stream messageStream = await selection.RetrieveMessagesBody(_messageBodyStore))
            {
                ISerializer serializer = Registry.Instance.SerializerProvider.Get(selection.ContentType);
                return await serializer.DeserializeAsync(messageStream, selection.ContentType, cancellationToken);
            }
        }

        private (bool, OutMessage) SelectUserMessageFor(MessagingContext messagingContext)
        {
            var options = new TransactionOptions {IsolationLevel = IsolationLevel.RepeatableRead};
            using (var scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                (bool, OutMessage) outMessage = ConcurrentSelectUserMessage(messagingContext);

                scope.Complete();
                return outMessage;
            }
        }

        private (bool, OutMessage) ConcurrentSelectUserMessage(MessagingContext messagingContext)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);
                OutMessage message =
                    repository.GetOutMessageData(
                        m => PullRequestQuery(m, messagingContext.AS4Message.PrimarySignalMessage as PullRequest),
                        m => m);

                if (message == null)
                {
                    return (false, null);
                }

                repository.UpdateOutMessage(message.EbmsMessageId, m => m.Operation = Operation.Sending);
                context.SaveChanges();

                return (true, message);
            }
        }

        private static bool PullRequestQuery(MessageEntity userMessage, PullRequest pullRequest)
        {
            return userMessage.Mpc == pullRequest.Mpc 
                   && userMessage.Operation == Operation.ToBeSent
                   && userMessage.MEP == MessageExchangePattern.Pull;
        }

        private static StepResult SuccessStepResult(AS4Message message, MessagingContext context)
        {
            return StepResult.Success(context.CloneWith(message));
        }
    }
}