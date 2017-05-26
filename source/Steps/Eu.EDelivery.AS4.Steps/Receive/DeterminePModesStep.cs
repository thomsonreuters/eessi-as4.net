using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
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
        private readonly IPModeRuleVisitor _visitor;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class
        /// </summary>
        public DeterminePModesStep() : this(Config.Instance, new PModeRuleVisitor()) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep" /> class
        /// Create a Determine Receiving PMode Step
        /// with a given Data store
        /// </summary>
        /// <param name="config"> </param>
        /// <param name="visitor"> </param>
        internal DeterminePModesStep(IConfig config, IPModeRuleVisitor visitor)
        {
            _config = config;
            _visitor = visitor;
        }

        /// <summary>
        /// Start determine the Receiving Processing Mode
        /// </summary>
        /// <param name="messagingContext"></param>
        /// <param name="cancellationToken"></param>
        /// <exception cref="AS4Exception">Throws exception when a PMode cannot be retrieved</exception>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(MessagingContext messagingContext, CancellationToken cancellationToken)
        {
            AS4Message as4Message = messagingContext.AS4Message;

            if (as4Message.IsSignalMessage)
            {
                messagingContext.SendingPMode = GetPModeFromDatastore(messagingContext.AS4Message);
            }
            else
            {
                messagingContext.ReceivingPMode = GetPModeFromSettings(messagingContext.AS4Message);
                messagingContext.SendingPMode = GetReferencedSendingPMode(messagingContext);
            }

            return await StepResult.SuccessAsync(messagingContext);
        }

        private static SendPMode GetPModeFromDatastore(AS4Message as4Message)
        {
            SignalMessage primarySignal = as4Message.PrimarySignalMessage;
            using (DatastoreContext context = Registry.Instance.CreateDatastoreContext())
            {
                var repository = new DatastoreRepository(context);
                SendPMode pmode = repository.RetrieveSendingPModeForOutMessage(primarySignal.RefToMessageId);

                if (pmode == null)
                {
                    throw ThrowAS4Exception(
                        $"Unable to retrieve Sending PMode from Datastore for OutMessage with Id: {primarySignal.RefToMessageId}",
                        as4Message.MessageIds);
                }

                Logger.Info($"Get Sending PMode {pmode.Id} from Datastore");
                return pmode;
            }
        }

        private ReceivePMode GetPModeFromSettings(AS4Message as4Message)
        {
            List<PModeParticipant> participants = GetPModeParticipants(as4Message.PrimaryUserMessage);
            LetParticipantsAcceptVisitor(participants);

            PModeParticipant winningParticipant = participants.Where(p => p.Points >= 10).Max();
            PostConditionsWinningParticipant(participants, winningParticipant, as4Message);

            Logger.Info($"Using Receiving PMode {winningParticipant.PMode.Id} with {winningParticipant.Points} Points");
            return winningParticipant.PMode;
        }

        private List<PModeParticipant> GetPModeParticipants(UserMessage primaryUser)
        {
            return _config.GetReceivingPModes().Select(pmode => CreateParticipant(pmode, primaryUser)).ToList();
        }

        private static PModeParticipant CreateParticipant(ReceivePMode pmode, UserMessage primaryUser)
        {
            return new PModeParticipant(pmode, primaryUser);
        }

        private void LetParticipantsAcceptVisitor(List<PModeParticipant> participants)
        {
            foreach (PModeParticipant participant in participants)
            {
                LetParticipantAcceptVisitor(participant);
            }
        }

        private void LetParticipantAcceptVisitor(PModeParticipant participant)
        {
            participant.Accept(_visitor);
            Logger.Debug($"Receiving PMode: {participant.PMode.Id} has {participant.Points} Points");
        }

        private static void PostConditionsWinningParticipant(List<PModeParticipant> participants, PModeParticipant winningParticipant, AS4Message message)
        {
            if (winningParticipant == null)
            {
                throw ThrowAS4Exception(
                    $"No Receiving PMode was found with for UserMessage with Message Id: {message.GetPrimaryMessageId()}", message.MessageIds);
            }

            if (TheresMoreThanOneWinningParticipant(participants, winningParticipant))
            {
                throw ThrowToManyPModeFoundException(message.MessageIds);
            }
        }

        private static bool TheresMoreThanOneWinningParticipant(List<PModeParticipant> participants, PModeParticipant winningParticipant)
        {
            return participants.Count(p => p.Points == winningParticipant.Points) > 1;
        }

        private static AS4Exception ThrowToManyPModeFoundException(string[] messageIds)
        {
            const string description = "More than one matching PMode was found";
            Logger.Error(description);

            return AS4ExceptionBuilder.WithDescription(description).WithMessageIds(messageIds).Build();
        }

        private SendPMode GetReferencedSendingPMode(MessagingContext messagingContext)
        {
            if (string.IsNullOrWhiteSpace(messagingContext.ReceivingPMode.ReceiptHandling.SendingPMode))
            {
                Logger.Warn("No SendingPMode defined in ReceiptHandling of Received PMode.");
                return null;
            }

            string pmodeId = messagingContext.ReceivingPMode.ReceiptHandling.SendingPMode;

            Logger.Info("Receipt Sending PMode Id: " + pmodeId);

            return TryGetSendingPMode(pmodeId, messagingContext.AS4Message.MessageIds);
        }

        private SendPMode TryGetSendingPMode(string pmodeId, string[] messageIds)
        {
            try
            {
                return _config.GetSendingPMode(pmodeId);
            }
            catch (Exception)
            {
                throw ThrowAS4Exception("Receiving PMode references a non-existing Sending PMode", messageIds);
            }
        }

        private static AS4Exception ThrowAS4Exception(string description, string[] messageIds)
        {
            Logger.Error(description);

            return AS4ExceptionBuilder.WithDescription(description).WithErrorCode(ErrorCode.Ebms0001).WithMessageIds(messageIds).Build();
        }
    }
}