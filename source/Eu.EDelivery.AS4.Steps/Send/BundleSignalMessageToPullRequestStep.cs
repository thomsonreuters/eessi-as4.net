using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using Eu.EDelivery.AS4.Strategies.Sender;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Adds piggy-backed ebMS signal messages to the <see cref="PullRequest"/> for signal messages that are responses
    /// of ebMS user messages that matches the <see cref="PullRequest.Mpc"/>.
    /// </summary>
    public class BundleSignalMessageToPullRequestStep : IStep
    {
        private readonly Func<DatastoreContext> _createContext;
        private readonly IAS4MessageBodyStore _bodyStore;

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleSignalMessageToPullRequestStep"/> class.
        /// </summary>
        public BundleSignalMessageToPullRequestStep() 
            : this(Registry.Instance.CreateDatastoreContext, Registry.Instance.MessageBodyStore) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="BundleSignalMessageToPullRequestStep"/> class.
        /// </summary>
        /// <param name="createContext">The create datastore context function.</param>
        /// <param name="bodyStore">The store to use for persisting messages.</param>
        public BundleSignalMessageToPullRequestStep(Func<DatastoreContext> createContext, IAS4MessageBodyStore bodyStore)
        {
            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            if (bodyStore == null)
            {
                throw new ArgumentNullException(nameof(bodyStore));
            }

            _createContext = createContext;
            _bodyStore = bodyStore;
        }

        /// <summary>
        /// Execute the step on a given <paramref name="messagingContext"/>.
        /// </summary>
        /// <param name="messagingContext"><see cref="MessagingContext"/> on which the step must be executed.</param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
        {
            if (messagingContext == null)
            {
                throw new ArgumentNullException(nameof(messagingContext));
            }

            if (messagingContext.AS4Message == null)
            {
                throw new InvalidOperationException(
                    $"{typeof(BundleSignalMessageToPullRequestStep)} Requires a AS4Message to possible bundle a "
                    + "SignalMessage to the PullRequest but there's not a AS4Message present in the MessagingContext");
            }

            if (!(messagingContext.AS4Message.PrimaryMessageUnit is PullRequest pullRequest))
            {
                throw new InvalidOperationException(
                    $"{typeof(BundleSignalMessageToPullRequestStep)} Requires a PullRequest as primary message unit in the "
                    + "AS4Message but there's not a PullRequest present in the MessagingContext");
            }

            string url = messagingContext.SendingPMode?.PushConfiguration?.Protocol?.Url;
            bool pullRequestSigned = messagingContext.SendingPMode?.Security?.Signing?.IsEnabled == true;

            using (DatastoreContext db = _createContext())
            {
                var service = new PiggyBackingService(db);
                IEnumerable<AS4Message> signals = 
                    await service.SelectToBePiggyBackedSignalMessagesAsync(pullRequest, url, _bodyStore);

                var resetSignals = new Collection<MessageUnit>();
                foreach (AS4Message signal in signals)
                {
                    var toBePiggyBacked = signal.PrimaryMessageUnit;
                    if (toBePiggyBacked is Receipt || toBePiggyBacked is Error)
                    {
                        if (!pullRequestSigned && signal.IsSigned)
                        {
                            Logger.Warn(
                                $"Can't PiggyBack {toBePiggyBacked.GetType().Name} {toBePiggyBacked.MessageId} because SignalMessage is signed "
                                + $"while the SendingPMode {messagingContext.SendingPMode?.Id} used is not configured for signing");

                            resetSignals.Add(toBePiggyBacked);
                        }
                        else
                        {
                            Logger.Info(
                                $"PiggyBack the {toBePiggyBacked.GetType().Name} which reference "
                                + $"UserMessage \"{toBePiggyBacked.RefToMessageId}\" to the PullRequest");

                            messagingContext.AS4Message.AddMessageUnit(toBePiggyBacked);
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

                if (resetSignals.Any())
                {
                    service.ResetSignalMessagesToBePiggyBacked(
                        resetSignals.Where(s => s is SignalMessage).Cast<SignalMessage>(),
                        SendResult.RetryableFail);

                    await db.SaveChangesAsync().ConfigureAwait(false);
                }

                return StepResult.Success(messagingContext);
            }
        }
    }
}