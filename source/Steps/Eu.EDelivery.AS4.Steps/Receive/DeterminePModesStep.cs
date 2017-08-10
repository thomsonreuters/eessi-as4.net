using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Validators;
using NLog;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using SendPMode = Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Step which describes how the PModes (Sending and Receiving) is determined
    /// </summary>
    public class DeterminePModesStep : IStep
    {
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

        private readonly IConfig _config;
        private readonly Func<DatastoreContext> _createContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class
        /// </summary>
        public DeterminePModesStep() : this(Config.Instance, Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="createContext">The create context.</param>
        public DeterminePModesStep(IConfig config, Func<DatastoreContext> createContext)
        {
            _config = config;
            _createContext = createContext;
        }

        /// <summary>
        /// Start determine the Receiving Processing Mode
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            if (messagingContext.AS4Message.IsSignalMessage)
            {
                return await DetermineSendingPModeForSignalMessage(messagingContext);
            }

            return await DetermineReceivingPModeForUserMessage(messagingContext);
        }

        private async Task<StepResult> DetermineSendingPModeForSignalMessage(MessagingContext messagingContext)
        {
            SendPMode pmode = GetPModeFromDatastore(messagingContext.AS4Message);
            if (pmode == null)
            {
                string description =
                    $"Unable to retrieve Sending PMode from Datastore for OutMessage with Id: {messagingContext.AS4Message.PrimarySignalMessage.RefToMessageId}";
                return FailedStepResult(description, messagingContext);
            }

            messagingContext.SendingPMode = pmode;
            return await StepResult.SuccessAsync(messagingContext);
        }

        private SendPMode GetPModeFromDatastore(AS4Message as4Message)
        {
            using (DatastoreContext context = _createContext())
            {
                var repository = new DatastoreRepository(context);

                return repository.GetOutMessageData(
                    as4Message.PrimarySignalMessage.RefToMessageId,
                    m => AS4XmlSerializer.FromString<SendPMode>(m.PMode));
            }
        }

        private async Task<StepResult> DetermineReceivingPModeForUserMessage(MessagingContext messagingContext)
        {
            IEnumerable<ReceivePMode> possibilities = GetPModeFromSettings(messagingContext.AS4Message);

            if (possibilities.Any() == false)
            {
                return FailedStepResult(
                    $"No Receiving PMode was found with for UserMessage with Message Id: {messagingContext.AS4Message.GetPrimaryMessageId()}",
                    messagingContext);
            }

            if (possibilities.Count() > 1)
            {
                return FailedStepResult("More than one matching Receiving PMode was found", messagingContext);
            }

            ReceivePMode pmode = possibilities.First();
            Logger.Info($"Use '{pmode.Id}' as Receiving PMode");

            var validationResult = ReceivingProcessingModeValidator.Instance.Validate(pmode);

            if (validationResult.IsValid == false)
            {
                messagingContext.ErrorResult = new ErrorResult("The receiving PMode is not valid.", ErrorAlias.Other);
                throw new InvalidPModeException($"The Receiving PMode {pmode.Id} is not valid.", validationResult);
            }

            messagingContext.ReceivingPMode = pmode;
            messagingContext.SendingPMode = GetReferencedSendingPMode(pmode);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static StepResult FailedStepResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorAlias.ProcessingModeMismatch);
            return StepResult.Failed(context);
        }

        private IEnumerable<ReceivePMode> GetPModeFromSettings(AS4Message as4Message)
        {
            List<PModeParticipant> participants = GetPModeParticipants(as4Message.PrimaryUserMessage);
            participants.ForEach(p => p.Accept(new PModeRuleVisitor()));

            PModeParticipant winner = participants.Where(p => p.Points >= 10).Max();
            return participants.Where(p => p.Points == winner?.Points).Select(p => p.PMode);
        }

        private List<PModeParticipant> GetPModeParticipants(UserMessage primaryUser)
        {
            return _config.GetReceivingPModes().Select(pmode => new PModeParticipant(pmode, primaryUser)).ToList();
        }

        private SendPMode GetReferencedSendingPMode(ReceivePMode receivePMode)
        {
            if (string.IsNullOrWhiteSpace(receivePMode.ReplyHandling.SendingPMode))
            {
                Logger.Warn("No SendingPMode defined in ReplyHandling of Received PMode.");
                return null;
            }

            string pmodeId = receivePMode.ReplyHandling.SendingPMode;
            Logger.Info("Referenced Sending PMode Id: " + pmodeId);

            return _config.GetSendingPMode(pmodeId);
        }
    }
}