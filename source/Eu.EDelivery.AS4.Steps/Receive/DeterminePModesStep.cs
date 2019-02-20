using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
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
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using PullRequest = Eu.EDelivery.AS4.Model.Core.PullRequest;

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
        public DeterminePModesStep()
            : this(Config.Instance, Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeterminePModesStep"/> class.
        /// </summary>
        /// <param name="config">The configuration.</param>
        /// <param name="createContext">The create context.</param>
        public DeterminePModesStep(
            IConfig config,
            Func<DatastoreContext> createContext)
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
        public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
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

            (SendPMode sendingPMode, ReceivePMode receivingPMode, ErrorResult error) =
                DeterminePModes(messagingContext.AS4Message, messagingContext.SendingPMode, messagingContext.ReceivingPMode);

            messagingContext.SendingPMode = sendingPMode;
            messagingContext.ReceivingPMode = receivingPMode;
            messagingContext.ErrorResult = error;

            if (sendingPMode != null)
            {
                Logger.Info($"Determine SendingPMode \"{sendingPMode.Id}\"");
            }

            if (receivingPMode != null)
            {
                Logger.Info($"Determine ReceivingPMode \"{receivingPMode.Id}\"");
            }

            return error == null
                ? StepResult.SuccessAsync(messagingContext)
                : StepResult.FailedAsync(messagingContext);
        }

        private (SendPMode sendPMode, ReceivePMode receivePMode, ErrorResult error) DeterminePModes(
            AS4Message message,
            SendPMode currentSendingPMode,
            ReceivePMode currentReceivingPMode)
        {
            SignalMessage firstNonPullRequestSignal =
                message.PrimaryMessageUnit is PullRequest
                    ? message.SignalMessages.Skip(1).FirstOrDefault()
                    : message.FirstSignalMessage;

            SendPMode sendingPMode = currentSendingPMode;
            ReceivePMode receivingPMode = currentReceivingPMode;
            ErrorResult error = null;

            if (firstNonPullRequestSignal != null)
            {
                sendingPMode = currentSendingPMode ?? DetermineSendingPMode(firstNonPullRequestSignal);

                if (sendingPMode == null && firstNonPullRequestSignal.IsMultihopSignal == false)
                {
                    throw new InvalidOperationException($"Unable to process received SignalMessage {firstNonPullRequestSignal.MessageId} because no UserMessage found on this MSH "
                                                        + $"for the received SignalMessage with RefToMessageId {firstNonPullRequestSignal.RefToMessageId}");
                }
            }

            if (currentReceivingPMode == null)
            {
                if (message.FirstUserMessage != null 
                    || ((firstNonPullRequestSignal?.IsMultihopSignal ?? false) && sendingPMode == null))
                {
                    var userMessage = GetUserMessageFromFirstMessageUnitOrRoutingInput(message);
                    var result = DetermineReceivingPMode(userMessage);

                    receivingPMode = result.pmode;
                    error = result.error;
                }
            }

            return (sendingPMode, receivingPMode, error);
        }

        private SendPMode DetermineSendingPMode(SignalMessage signal)
        {
            if (String.IsNullOrWhiteSpace(signal.RefToMessageId))
            {
                Logger.Warn(
                    $"Cannot determine SendingPMode for received {signal.GetType().Name} SignalMessage "
                    + "because it doesn't contain a RefToMessageId to link an UserMessage from which the SendingPMode needs to be selected");

                return null;
            }

            using (DatastoreContext ctx = _createContext())
            {
                // We must take into account that it is possible that we have an OutMessage that has
                // been forwarded; in that case, we must not retrieve the sending - pmode since we 
                // will have to forward the signal message.

                var repository = new DatastoreRepository(ctx);
                return repository.GetOutMessageData(
                    m => m.EbmsMessageType == MessageType.UserMessage
                         && m.EbmsMessageId == signal.RefToMessageId
                         && m.Intermediary == false,
                    m => new { m.PMode, m.ModificationTime })
                          .OrderByDescending(m => m.ModificationTime)
                          .FirstOrNothing()
                          .Where(x => !String.IsNullOrWhiteSpace(x.PMode))
                          .Select(x => AS4XmlSerializer.FromString<SendPMode>(x.PMode))
                          .GetOrElse(() => null);
            }
        }

        private static UserMessage GetUserMessageFromFirstMessageUnitOrRoutingInput(AS4Message as4Message)
        {
            if (as4Message.HasUserMessage)
            {
                Logger.Trace("AS4Message contains UserMessages, so the incoming message itself will be used to match the right ReceivingPMode");
                return as4Message.FirstUserMessage;
            }

            Logger.Debug("AS4Message should be a Multi-Hop SignalMessage, so the embeded Multi-Hop UserMessage will be used to match the right ReceivingPMode");
            Maybe<RoutingInputUserMessage> routedUserMessage =
                as4Message.SignalMessages.FirstOrDefault(s => s.IsMultihopSignal)?.MultiHopRouting;

            if (routedUserMessage != null)
            {
                return UserMessageMap.ConvertFromRouting(routedUserMessage.UnsafeGet);
            }

            throw new InvalidOperationException(
                "Incoming message doesn't have a UserMessage either as message unit or as <RoutedInput/> in a SignalMessage. "
                + "This message can therefore not be used to determine the ReceivingPMode");
        }

        private (ReceivePMode pmode, ErrorResult error) DetermineReceivingPMode(UserMessage user)
        {
            Logger.Trace("Incoming message hasn't yet a ReceivingPMode, will determine one");

            IEnumerable<ReceivePMode> possibilities = GetMatchingReceivingPModeForUserMessage(user);
            if (possibilities.Any() == false)
            {
                return (null, NoMatchingPModeFoundFailure());
            }

            if (possibilities.Count() > 1)
            {
                return (null, TooManyPossibilitiesFailure(possibilities));
            }

            ReceivePMode pmode = possibilities.First();
            return (pmode, null);
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

        private static ErrorResult TooManyPossibilitiesFailure(IEnumerable<ReceivePMode> possibilities)
        {
            Logger.Error(
                "Cannot determine ReceivingPMode because more than a single matching PMode was found (greater or equal than 10 points). "
                + $"{Environment.NewLine} Please make the matching information more strict in the message packaging information so that only a single PMode is matched."
                + $"{Environment.NewLine}{String.Join(Environment.NewLine, possibilities.Select(p => $" - {p.Id}"))}");

            return new ErrorResult(
                "Cannot determine ReceivingPMode because more than a single matching PMode was found",
                ErrorAlias.ProcessingModeMismatch);
        }

        private static ErrorResult NoMatchingPModeFoundFailure()
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

            return new ErrorResult(
                "Cannot determine ReceivingPMode because no configured PMode matched the message packaging information",
                ErrorAlias.ProcessingModeMismatch);
        }
    }
}