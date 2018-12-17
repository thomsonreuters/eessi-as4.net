using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps.Receive.Participant;
using Eu.EDelivery.AS4.Xml;
using NLog;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using SendPMode = Eu.EDelivery.AS4.Model.PMode.SendingProcessingMode;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Receive
{
    /// <summary>
    /// Step which describes how the PModes (Sending and Receiving) is determined
    /// </summary>
    [Info("Determine PMode for received AS4 Message")]
    [Description("Determines the PMode that must be used to process the received AS4 Message")]
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
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (createContext == null)
            {
                throw new ArgumentNullException(nameof(createContext));
            }

            _config = config;
            _createContext = createContext;
        }

        /// <summary>
        /// Start determine the Receiving Processing Mode
        /// </summary>
        /// <param name="messagingContext"></param>
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
                    $"{nameof(DeterminePModesStep)} requires an AS4Message but no AS4Message is present in the MessagingContext");
            }

            AS4Message as4Message = messagingContext.AS4Message;
            bool containsSignalThatNotPullRequest = as4Message.SignalMessages.Any(s => !(s is Model.Core.PullRequest));
            if (containsSignalThatNotPullRequest && (as4Message.IsMultiHopMessage || messagingContext.SendingPMode == null))
            {
                messagingContext.SendingPMode = await DetermineSendingPModeForSignalMessageAsync(as4Message);
            }

            if (messagingContext.ReceivingPMode != null)
            {
                Logger.Debug(
                    $"Will not determine ReceivingPMode: incoming message has already a ReceivingPMode: {messagingContext.ReceivingPMode.Id} configured. "
                    + $"{Environment.NewLine} This happens when the Receive Agent is configured as a \"Static Receive Agent\"");
            }
            else if (as4Message.HasUserMessage || as4Message.SignalMessages.Any(s => s.IsMultihopSignal))
            {
                Logger.Trace("Incoming message hasn't yet a ReceivingPMode, will determine one");
                UserMessage userMessage = GetUserMessageFromFirstMessageUnitOrRoutingInput(messagingContext.AS4Message);
                IEnumerable<ReceivePMode> possibilities = GetMatchingReceivingPModeForUserMessage(userMessage);

                if (possibilities.Any() == false)
                {
                    return NoMatchingPModeFoundFailure(messagingContext);
                }

                if (possibilities.Count() > 1)
                {
                    return TooManyPossibilitiesFailure(messagingContext, possibilities);
                }

                ReceivePMode pmode = possibilities.First();
                Logger.Info($"Found ReceivingPMode \"{pmode.Id}\" to further process the incoming message");
                messagingContext.ReceivingPMode = pmode;
            }

            return StepResult.Success(messagingContext);
        }

        private async Task<SendPMode> DetermineSendingPModeForSignalMessageAsync(AS4Message as4Message)
        {
            var firstNonPrSignalMessage = as4Message.SignalMessages.First(s => !(s is Model.Core.PullRequest));
            async Task<SendPMode> SelectSendingPModeBasedOnSendUserMessage()
            {
                if (String.IsNullOrWhiteSpace(firstNonPrSignalMessage.RefToMessageId))
                {
                    Logger.Warn(
                        $"Cannot determine SendingPMode for received {firstNonPrSignalMessage.GetType().Name} SignalMessage "
                        + "because it doesn't contain a RefToMessageId to link a UserMessage from which the SendingPMode needs to be selected");

                    return null;
                }

                using (DatastoreContext dbContext = _createContext())
                {
                    var repository = new DatastoreRepository(dbContext);

                    // We must take into account that it is possible that we have an OutMessage that has
                    // been forwarded; in that case, we must not retrieve the sending - pmode since we 
                    // will have to forward the signal message.
                    var referenced = repository.GetOutMessageData(
                            where: m => m.EbmsMessageId == firstNonPrSignalMessage.RefToMessageId && m.Intermediary == false,
                            selection: m => new { m.PMode, m.ModificationTime })
                                  .OrderByDescending(m => m.ModificationTime)
                                  .FirstOrDefault();

                    if (referenced == null)
                    {
                        Logger.Warn($"No referenced UserMessage record found for SignalMessage {firstNonPrSignalMessage.MessageId}");
                        return null;
                    }

                    string pmodeString = referenced?.PMode;
                    if (String.IsNullOrWhiteSpace(pmodeString))
                    {
                        Logger.Warn($"No SendingPMode found in stored referenced UserMessage record for SignalMessage {firstNonPrSignalMessage.MessageId}");
                        return null;
                    }

                    return await AS4XmlSerializer.FromStringAsync<SendPMode>(pmodeString);
                }
            }

            SendPMode pmode = await SelectSendingPModeBasedOnSendUserMessage();
            if (pmode != null)
            {
                Logger.Debug($"Determined SendingPMode {pmode.Id} for received SignalMessages");
                return pmode;
            }

            if (as4Message.IsMultiHopMessage == false)
            {
                throw new InvalidOperationException(
                    "Cannot determine SendingPMode for incoming SignalMessage because no "
                    + $"referenced UserMessage {firstNonPrSignalMessage.RefToMessageId} was found on this MSH");
           }

            return null;
        }

        private static UserMessage GetUserMessageFromFirstMessageUnitOrRoutingInput(AS4Message as4Message)
        {
            // TODO: is this enough? should we explicitly check for multi-hop signals ?
            if (as4Message.HasUserMessage)
            {
                Logger.Trace("AS4Message contains UserMessages, so the incoming message itself will be used to match the right ReceivingPMode");
                return as4Message.FirstUserMessage;
            }

            Logger.Debug("AS4Message should be a Multi-Hop SignalMessage, so the embeded Multi-Hop UserMessage will be used to match the right ReceivingPMode");
            Maybe<RoutingInputUserMessage> routedUserMessageM =
                as4Message.SignalMessages.FirstOrDefault(s => s.IsMultihopSignal)?.MultiHopRouting;

            if (routedUserMessageM != null)
            {
                return UserMessageMap.ConvertFromRouting(routedUserMessageM.UnsafeGet);
            }

            throw new InvalidOperationException(
                "Incoming message doesn't have a UserMessage either as message unit or as <RoutedInput/> in a SignalMessage. "
                + "This message can therefore not be used to determine the ReceivingPMode");
        }

        private IEnumerable<ReceivePMode> GetMatchingReceivingPModeForUserMessage(UserMessage userMessage)
        {
            IEnumerable<PModeParticipant> participants = 
                _config.GetReceivingPModes()
                       .Select(pmode => new PModeParticipant(pmode, userMessage))
                       .Select(PModeRuleEngine.ApplyRules);

            IEnumerable<int> scoresToConsider = participants.Select(p => p.Points).Where(p => p >= 10);
            if (scoresToConsider.Any() == false)
            {
                return Enumerable.Empty<ReceivePMode>();
            }

            int maxPoints = scoresToConsider.Max();
            return participants.Where(p => p.Points == maxPoints).Select(p => p.PMode);
        }

        private static StepResult TooManyPossibilitiesFailure(MessagingContext messagingContext, IEnumerable<ReceivePMode> possibilities)
        {
            Logger.Error(
                "Cannot determine ReceivingPMode because more than a single matching PMode was found (greater or equal than 10 points). "
                + $"{Environment.NewLine} Please make the matching information more strict in the message packaging information so that only a single PMode is matched."
                + $"{Environment.NewLine}{String.Join(Environment.NewLine, possibilities.Select(p => $" - {p.Id}"))}");

            return FailedStepResult("Cannot determine ReceivingPMode because more than a single matching PMode was found", messagingContext);
        }

        private static StepResult NoMatchingPModeFoundFailure(MessagingContext messagingContext)
        {
            Logger.Error(
                "Cannot determine ReceivingPMode because no configured PMode matched the message packaging information enough (greater or equal than 10 points). "
                + $"{Environment.NewLine} Please change the message packaging information of your ReceivingPMode(s) to match the message: "
                + $"{Environment.NewLine} - PMode.Id"
                + $"{Environment.NewLine} - PMode.MessagePacakging.PartyInfo.FromParty"
                + $"{Environment.NewLine} - PMode.MessagePacakging.PartyInfo.ToParty"
                + $"{Environment.NewLine} - PMode.MessagePackaging.CollaborationInfo.Service"
                + $"{Environment.NewLine} - PMode.MessagePackaging.CollaborationInfo.Action"
                + $"{Environment.NewLine} See the above trace logging to see for which rules your PMode has accuired points");

            return FailedStepResult(
                "Cannot determine ReceivingPMode because no configured PMode matched the message packaging information", 
                messagingContext);
        }

        private static StepResult FailedStepResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorAlias.ProcessingModeMismatch);
            return StepResult.Failed(context);
        }
    }
}