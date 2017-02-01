using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Notify Message
    /// </summary>
    public class MinderCreateNotifyMessageStep : IStep
    {
        private const string ConformanceUriPrefix = "http://www.esens.eu/as4/conformancetest";
        private readonly ILogger _logger;
        private IList<MessageProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderCreateNotifyMessageStep"/> class
        /// </summary>
        public MinderCreateNotifyMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start create notify message step
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Create Notify Message");

            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            if (signalMessage != null)
            {
                this._logger.Info($"Minder Create Notify Message as {signalMessage.GetType().Name}");

                if (userMessage != null)
                    AssignMinderProperties(userMessage, signalMessage);

                AssignSendingUrl(internalMessage);
                //RemoveUnneededUserMessage(internalMessage);
            }
            else
            {
                this._logger.Warn($"{internalMessage.Prefix} AS4Message does not contain a primary SignalMessage");
            }

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssignMinderProperties(UserMessage userMessage, SignalMessage signalMessage)
        {
            AssignServiceAction(userMessage);
            AssignFromPartyRole(userMessage);

            this._properties = userMessage.MessageProperties;

            AddSignalMessageProperties(signalMessage);
            userMessage.RefToMessageId = signalMessage.MessageId;
        }

        private static void AssignSendingUrl(InternalMessage internalMessage)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            IList<MessageProperty> messageProperties = as4Message.PrimaryUserMessage.MessageProperties;
            MessageProperty originalSender = messageProperties.FirstOrDefault(p => p.Name.Equals("originalSender"));

            int corner = originalSender?.Value.Equals("C1") == true ? 1 : 4;
            as4Message.SendingPMode.PushConfiguration.Protocol.Url = $"http://13.81.109.44:15001/corner{corner}";
        }

        private void RemoveUnneededUserMessage(InternalMessage internalMessage)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            ICollection<UserMessage> userMessages = as4Message.UserMessages;

            Func<UserMessage, bool> whereMessageIdIsDifferent = m => !m.MessageId.Equals(as4Message.PrimaryUserMessage.MessageId);
            UserMessage otherMessage = userMessages.FirstOrDefault(whereMessageIdIsDifferent);
            if (otherMessage != null) userMessages.Remove(otherMessage);
        }

        private static void AssignServiceAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Notify";
            userMessage.CollaborationInfo.Service.Value = ConformanceUriPrefix;
            userMessage.CollaborationInfo.ConversationId = "1";
        }

        private static void AssignFromPartyRole(UserMessage userMessage)
        {
            userMessage.Sender.PartyIds.First().Id = "as4-net-c2";
            userMessage.Sender.Role = $"{ConformanceUriPrefix}/sut";
            userMessage.Receiver.PartyIds.First().Id = "minder";
            userMessage.Receiver.Role = $"{ConformanceUriPrefix}/testdriver";
        }

        private void AddSignalMessageProperties(SignalMessage signalMessage)
        {
            this._properties.Add(new MessageProperty("RefToMessageId", signalMessage.RefToMessageId));
            this._properties.Add(new MessageProperty("SignalType", signalMessage.GetType().Name));
        }
    }
}