using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
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
            AS4Message as4Message = messagingContext?.AS4Message;
            if (as4Message == null)
            {
                throw new InvalidOperationException(
                    $"{nameof(DeterminePModesStep)} requires an AS4Message but no AS4Message is present in the MessagingContext");
            }

            if (as4Message.SignalMessages.Any(s => !(s is Model.Core.PullRequest)))
            {
                if (as4Message.IsMultiHopMessage == false && messagingContext.SendingPMode != null)
                {
                    return StepResult.Success(messagingContext);
                }

                var firstNonPrSignalMessage = as4Message.SignalMessages.First(s => !(s is Model.Core.PullRequest));
                SendPMode pmode = await DetermineSendingPModeForSignalMessageAsync(firstNonPrSignalMessage);
                if (pmode != null)
                {
                    messagingContext.SendingPMode = pmode;
                }
                else if (as4Message.IsMultiHopMessage == false)
                {
                    throw new InvalidOperationException(
                        $"{messagingContext.LogTag} Cannot determine Sending PMode for incoming SignalMessage: " + 
                        $"no referenced OutMessage with Id: {as4Message.FirstSignalMessage.RefToMessageId} " + 
                        "is stored in the Datastore to retrieve the Sending PMode from");
                }
            }

            if (messagingContext.ReceivingPMode != null)
            {
                Logger.Info(
                    $"{messagingContext.LogTag} Will not determine ReceivingPMode: incoming message has already a "
                    + $"ReceivingPMode: {messagingContext.ReceivingPMode.Id} confingured, "
                    + "this happens when the Receive Agent is configured as a \"Static Receive Agent\"");
            }
            else if (as4Message.HasUserMessage
                    || as4Message.SignalMessages.Any(s => s.IsMultihopSignal))
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
                Logger.Info($"Found ReceivingPMode {pmode.Id} to further process the incoming message");
                messagingContext.ReceivingPMode = pmode;
            }

            return StepResult.Success(messagingContext);
        }

        private async Task<SendPMode> DetermineSendingPModeForSignalMessageAsync(MessageUnit signalMessage)
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
                              .FirstOrDefault()
                              ?.PMode;

                return await AS4XmlSerializer.FromStringAsync<SendPMode>(pmodeString);
            }
        }

        private static UserMessage GetUserMessageFromFirstMessageUnitOrRoutingInput(AS4Message as4Message)
        {
            // TODO: is this enough ?
            // should we explictly check for multihop signals ?
            if (as4Message.HasUserMessage)
            {
                Logger.Debug(
                    "Incoming message is a UserMessage, so the incoming message itself will be used to match the right ReceivingPMode");

                return as4Message.FirstUserMessage;
            }

            Logger.Debug(
                "Incoming message is a Multi-Hop SignalMessage, " +
                "so the embeded Multi-Hop UserMessage will be used to match the right Receiving PMode");

            Maybe<RoutingInputUserMessage> routedUserMessageM =
                as4Message.SignalMessages.FirstOrDefault(s => s.IsMultihopSignal)?.MultiHopRouting;

            if (routedUserMessageM != null)
            {
                return AS4Mapper.Map<UserMessage>(routedUserMessageM.UnsafeGet);
            }

            throw new InvalidOperationException(
                $"(Receive)[{as4Message.GetPrimaryMessageId()}] Incoming message doesn't have a UserMessage " + 
                "either as Message Unit or as Routed Input in a SignalMessage. " +
                "This message can therefore not be used to determine the ReceivingPMode");
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
                return new ReceivePMode[] { };
            }

            int maxPoints = scoresToConsider.Max();
            return participants.Where(p => p.Points == maxPoints).Select(p => p.PMode);
        }

        private static StepResult TooManyPossibilitiesFailure(MessagingContext messagingContext, IEnumerable<ReceivePMode> possibilities)
        {
            const string description =
                "Cannot determine ReceivingPMode: more than one matching ReceivingPMode was found. " +
                "Please stricten the matching information in the message packaging information so that only a single PMode is matched";

            Logger.Error(description);
            Logger.Error(
                $"Candidates are:{Environment.NewLine}" +
                String.Join(Environment.NewLine, possibilities.Select(p => p.Id).ToArray()));

            return FailedStepResult(description, messagingContext);
        }

        private static StepResult NoMatchingPModeFoundFailure(MessagingContext messagingContext)
        {
            string description =
                $"{messagingContext.LogTag} Cannot determine ReceivingPMode: " +
                $"no configured ReceivingPMode was found for Message with Id: {messagingContext.AS4Message.GetPrimaryMessageId()}. " +
                @"Please configure a ReceivingPMode at '.\config\receive-pmodes' that matches the message packaging information";

            Logger.Error(description);
            return FailedStepResult(description, messagingContext);
        }

        private static StepResult FailedStepResult(string description, MessagingContext context)
        {
            context.ErrorResult = new ErrorResult(description, ErrorAlias.ProcessingModeMismatch);
            return StepResult.Failed(context);
        }
    }
}