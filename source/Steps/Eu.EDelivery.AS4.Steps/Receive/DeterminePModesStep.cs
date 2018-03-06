using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Validators;
using FluentValidation.Results;
using NLog;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using SendPMode = Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Step which describes how the PModes (Sending and Receiving) is determined
    /// </summary>
    [Description("Determines the PMode that must be used to process the received AS4 Message")]
    [Info("Determine PMode for received AS4 Message")]
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
            AS4Message as4Message = messagingContext.AS4Message;

            if (as4Message.IsSignalMessage)
            {
                if (as4Message.IsMultiHopMessage == false && messagingContext.SendingPMode != null)
                {
                    return StepResult.Success(messagingContext);
                }

                SendPMode pmode = await DetermineSendingPModeForSignalMessageAsync(as4Message.PrimarySignalMessage);

                if (pmode != null)
                {
                    messagingContext.SendingPMode = pmode;
                    return StepResult.Success(messagingContext);
                }

                if (as4Message.IsMultiHopMessage == false)
                {
                    throw new InvalidOperationException($"Unable to retrieve Sending PMode from Datastore for OutMessage with Id: {as4Message.PrimarySignalMessage.RefToMessageId}");
                }
            }

            return await DetermineReceivingPModeAsync(messagingContext);
        }

        private async Task<SendPMode> DetermineSendingPModeForSignalMessageAsync(SignalMessage signalMessage)
        {
            if (String.IsNullOrWhiteSpace(signalMessage.RefToMessageId))
            {
                return null;
            }

            using (DatastoreContext dbContext = _createContext())
            {
                var repository = new DatastoreRepository(dbContext);

                // We must take into account that it is possible that we have an OutMessage that has
                // been forwarded; in that case, we must not retrieve the sending - pmode since we 
                // will have to forward the signalmessage.
                string pmodeString =
                    repository.GetOutMessageData(
                                  where: m => m.EbmsMessageId == signalMessage.RefToMessageId && m.Intermediary == false,
                                  selection: m => new { m.PMode, m.ModificationTime })
                              .OrderByDescending(m => m.ModificationTime)
                              .FirstOrDefault()?.PMode;

                return await AS4XmlSerializer.FromStringAsync<SendPMode>(pmodeString);
            }
        }

        private async Task<StepResult> DetermineReceivingPModeAsync(MessagingContext messagingContext)
        {
            UserMessage userMessage = RetrieveUserMessage(messagingContext.AS4Message);

            IEnumerable<ReceivePMode> possibilities = GetPModeFromSettings(userMessage);

            if (possibilities.Any() == false)
            {
                string description =
                    $"No Receiving PMode was found for Message with Id: {messagingContext.AS4Message.GetPrimaryMessageId()}";

                Logger.Error(description);

                if (messagingContext.AS4Message.IsUserMessage)
                {
                    return FailedStepResult(description, messagingContext);
                }

                throw new InvalidOperationException(description);
            }

            if (possibilities.Count() > 1)
            {
                const string description = "More than one matching Receiving PMode was found";

                Logger.Error(description);
                Logger.Error("Candidates are: ");
                Logger.Error(String.Join(Environment.NewLine, possibilities.Select(p => p.Id).ToArray()));

                if (messagingContext.AS4Message.IsUserMessage)
                {
                    return FailedStepResult(description, messagingContext);
                }

                throw new InvalidOperationException(description);
            }

            ReceivePMode pmode = possibilities.First();
            Logger.Info($"Use '{pmode.Id}' as Receiving PMode to process message {messagingContext.EbmsMessageId}");

            ValidationResult validationResult = ReceivingProcessingModeValidator.Instance.Validate(pmode);

            if (validationResult.IsValid == false)
            {
                messagingContext.ErrorResult = new ErrorResult("The receiving PMode is not valid.", ErrorAlias.Other);
                throw new InvalidPModeException($"The Receiving PMode {pmode.Id} is not valid.", validationResult);
            }

            messagingContext.ReceivingPMode = pmode;
            messagingContext.SendingPMode = GetReferencedSendingPMode(pmode);

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static UserMessage RetrieveUserMessage(AS4Message as4Message)
        {
            // TODO: is this enough ?
            // should we explictly check for multihop signals ?
            if (as4Message.IsUserMessage)
            {
                return as4Message.PrimaryUserMessage;
            }

            return AS4Mapper.Map<UserMessage>(as4Message.PrimarySignalMessage.MultiHopRouting);
        }

        private static StepResult FailedStepResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorAlias.ProcessingModeMismatch);
            return StepResult.Failed(context);
        }

        private IEnumerable<ReceivePMode> GetPModeFromSettings(UserMessage userMessage)
        {
            List<PModeParticipant> participants = GetPModeParticipants(userMessage);
            participants.ForEach(p => p.Accept(new PModeRuleVisitor()));

            IEnumerable<int> scoresToConsider = participants.Select(p => p.Points).Where(p => p >= 10);

            if (scoresToConsider.Any() == false)
            {
                return new ReceivePMode[] { };
            }

            int maxPoints = scoresToConsider.Max();

            return participants.Where(p => p.Points == maxPoints).Select(p => p.PMode);
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
            Logger.Info($"Referenced Sending PMode Id: {pmodeId}");

            return _config.GetSendingPMode(pmodeId);
        }
    }
}