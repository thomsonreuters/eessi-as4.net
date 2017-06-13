using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Transactions;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how a MessageUnit should be selected to be sent via Pulling.
    /// </summary>
    /// <seealso cref="IStep" />
    public class SelectUserMessageToSendStep : IStep
    {
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

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
            OutMessage selection = SelectUserMessageFor(messagingContext);
            AS4Message message = await RetrieveAS4UserMessage(selection, cancellationToken);
            AS4Message as4Message = new AS4MessageBuilder().WithUserMessage(message.PrimaryUserMessage).Build();
            return StepResult.Success(new MessagingContext(as4Message));
        }

        private async Task<AS4Message> RetrieveAS4UserMessage(MessageEntity selection, CancellationToken cancellationToken)
        {
            using (Stream messageStream = await selection.RetrieveMessagesBody(_messageBodyStore))
            {
                ISerializer serializer = Registry.Instance.SerializerProvider.Get(selection.ContentType);
                return await serializer.DeserializeAsync(messageStream, selection.ContentType, cancellationToken);
            }
        }

        private OutMessage SelectUserMessageFor(MessagingContext messagingContext)
        {
            var options = new TransactionOptions {IsolationLevel = IsolationLevel.RepeatableRead};
            using (var scope = new TransactionScope(TransactionScopeOption.Required, options))
            {
                OutMessage outMessage = ConcurrentSelectUserMessage(messagingContext);

                scope.Complete();
                return outMessage;
            }
        }

        private OutMessage ConcurrentSelectUserMessage(MessagingContext messagingContext)
        {
            return _createContext().Using(
                context =>
                {
                    var repository = new DatastoreRepository(context);
                    OutMessage message =
                        repository.GetOutMessageData(
                            m => PullRequestQuery(m, messagingContext.AS4Message.PrimarySignalMessage as PullRequest),
                            m => m);

                    repository.UpdateOutMessage(message.EbmsMessageId, m => m.Operation = Operation.Sending);
                    context.SaveChanges();

                    return message;
                });
        }

        private static bool PullRequestQuery(MessageEntity userMessage, PullRequest pullRequest)
        {
            return userMessage.Mpc == pullRequest.Mpc 
                   && userMessage.Operation == Operation.ToBeSent
                   && userMessage.MEP == MessageExchangePattern.Pull;
        }
    }
}