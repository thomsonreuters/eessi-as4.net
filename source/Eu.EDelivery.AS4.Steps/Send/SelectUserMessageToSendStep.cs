using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Transactions;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Microsoft.EntityFrameworkCore;
using NLog;
using MessageExchangePattern = Eu.EDelivery.AS4.Entities.MessageExchangePattern;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Describes how a MessageUnit should be selected to be sent via Pulling.
    /// </summary>
    /// <seealso cref="IStep" />
    [Info("Select message to send")]
    [Description(
        "Selects a message that is eligible for sending via pulling. " +
        "This step selects a message that matches the MPC of the received pull-request signalmessage.")]
    public class SelectUserMessageToSendStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _messageBodyStore;

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectUserMessageToSendStep"/> class.
        /// </summary>
        public SelectUserMessageToSendStep()
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectUserMessageToSendStep" /> class.
        /// </summary>
        /// <param name="createContext">The create context.</param>
        /// <param name="messageBodyStore">The message body store.</param>
        public SelectUserMessageToSendStep(
            Func<DatastoreContext> createContext,
            IAS4MessageBodyStore messageBodyStore)
        {
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            if (messageBodyStore == null)
            {
                throw new ArgumentNullException(nameof(messageBodyStore));
            }

            _createContext = createContext;
            _messageBodyStore = messageBodyStore;
        }

        /// <summary>
        /// Execute the step for a given <paramref name="messagingContext" />.
        /// </summary>
        /// <param name="messagingContext">Message used during the step execution.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            var pullRequest = messagingContext?.AS4Message?.FirstSignalMessage as PullRequest;
            if (pullRequest == null)
            {
                throw new InvalidMessageException(
                    "The received message is not a PullRequest message, " +
                    "therefore no UserMessage can be selected to return to the sender");
            }

            (bool hasMatch, OutMessage match) = RetrieveUserMessageForPullRequest(pullRequest);
            if (hasMatch)
            {
                // Retrieve the existing MessageBody and put that stream in the MessagingContext.
                // The HttpReceiver processor will make sure that it gets serialized to the http response stream.
                Stream messageBody = await match.RetrieveMessageBody(_messageBodyStore).ConfigureAwait(false);

                messagingContext.ModifyContext(
                    new ReceivedMessage(messageBody, match.ContentType), 
                    MessagingContextMode.Send);

                messagingContext.SendingPMode = AS4XmlSerializer.FromString<SendingProcessingMode>(match.PMode);

                return StepResult.Success(messagingContext);
            }

            AS4Message pullRequestWarning = AS4Message.Create(new PullRequestError());
            messagingContext.ModifyContext(pullRequestWarning);

            return StepResult.Success(messagingContext).AndStopExecution();
        }

        private (bool, OutMessage) RetrieveUserMessageForPullRequest(PullRequest pullRequest)
        {
            using (DatastoreContext context = _createContext())
            {
                context.Database.BeginTransaction(System.Data.IsolationLevel.RepeatableRead);

                OutMessage message = context.OutMessages
                    .Where(PullRequestQuery(pullRequest))
                    .OrderBy(m => m.InsertionTime).Take(1).FirstOrDefault();

                if (message == null)
                {
                    Logger.Warn($"No UserMessage found for PullRequest.Mpc: {pullRequest.Mpc}");
                    return (false, null);
                }

                message.Operation = Operation.Sent;

                context.SaveChanges();
                context.Database.CommitTransaction();

                Logger.Info($"(PullSend) UserMessage found for PullRequest.Mpc: {pullRequest.Mpc}");
                return (true, message);
            }
        }

        private static Expression<Func<OutMessage, bool>> PullRequestQuery(PullRequest pullRequest)
        {
            Logger.Debug($"Query UserMessages with MPC={pullRequest.Mpc} && Operation=ToBeSent && MEP=Pull");

            return m => m.Mpc == pullRequest.Mpc &&
                        m.Operation == Operation.ToBeSent &&
                        m.MEP == MessageExchangePattern.Pull;
        }
    }
}