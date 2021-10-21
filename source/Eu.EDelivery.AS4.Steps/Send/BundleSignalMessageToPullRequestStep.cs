using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Services;
using log4net;

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

        private static readonly ILog Logger = LogManager.GetLogger( System.Reflection.MethodBase.GetCurrentMethod().DeclaringType );

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
                    $"{nameof(BundleSignalMessageToPullRequestStep)} requires a AS4Message to possible bundle a "
                    + "SignalMessage to the PullRequest but there's not a AS4Message present in the MessagingContext");
            }

            if (messagingContext.SendingPMode == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(BundleSignalMessageToPullRequestStep)} requires a SendingPMode to select the right "
                    + "SignalMessages for piggybacking but there's not a SendingPMode present in the MessagingContext");
            }

            if (!(messagingContext.AS4Message.PrimaryMessageUnit is PullRequest pullRequest))
            {
                throw new InvalidOperationException(
                    $"{nameof(BundleSignalMessageToPullRequestStep)} requires a PullRequest as primary message unit in the "
                    + "AS4Message but there's not a PullRequest present in the MessagingContext");
            }

            using (DatastoreContext db = _createContext())
            {
                var service = new PiggyBackingService(db);
                IEnumerable<SignalMessage> signals = 
                    await service.SelectToBePiggyBackedSignalMessagesAsync(pullRequest, messagingContext.SendingPMode, _bodyStore);

                foreach (SignalMessage signal in signals)
                {
                    Logger.Info(
                        $"PiggyBack the {Config.Encode(signal.GetType().Name)} \"{Config.Encode(signal.MessageId)}\" which reference "
                        + $"UserMessage \"{Config.Encode(signal.RefToMessageId)}\" to the PullRequest");

                    messagingContext.AS4Message.AddMessageUnit(signal);
                }

                return StepResult.Success(messagingContext);
            }
        }
    }
}