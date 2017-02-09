using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Deliver Message
    /// </summary>
    public class MinderCreateDeliverMessageStep : IStep
    {
        private const string ConformanceUriPrefix = "http://www.esens.eu/as4/conformancetest";
        private readonly ILogger _logger;
        private IList<MessageProperty> _properties;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderCreateDeliverMessageStep"/>
        /// </summary>
        public MinderCreateDeliverMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Create Deliver Message");

            internalMessage.DeliverMessage = CreateMinderDeliverMessage(internalMessage);
            AdaptCreatedMessage(internalMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private DeliverMessage CreateMinderDeliverMessage(InternalMessage internalMessage)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            this._properties = userMessage.MessageProperties;

            AssignMessageInfo(userMessage);
            AssignPartyProperties(userMessage);
            AssignCollaborationInfoProperties(userMessage);
            AssignParties(userMessage);
            AssignDeliverServiceAction(userMessage);
            
            return Mapper.Map<DeliverMessage>(userMessage);
        }

        private static void AdaptCreatedMessage(InternalMessage internalMessage)
        {
            AssignSendingUrl(internalMessage);
            ResetSecurityHeader(internalMessage);
            RemoveUnneededUserMessage(internalMessage);
        }

        private static void AssignSendingUrl(InternalMessage internalMessage)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            UserMessage userMessage = as4Message.PrimaryUserMessage;
            MessageProperty originalSender =
                userMessage?.MessageProperties.FirstOrDefault(p => p.Name.Equals("originalSender"));

            int corner = originalSender?.Value.Equals("C1") == true ? 4 : 1;
            as4Message.SendingPMode.PushConfiguration.Protocol.Url = $"http://13.81.109.44:15001/corner{corner}";
        }

        private static void ResetSecurityHeader(InternalMessage internalMessage)
        {
            internalMessage.AS4Message.SecurityHeader = new SecurityHeader();
        }

        private static void RemoveUnneededUserMessage(InternalMessage internalMessage)
        {
            AS4Message as4Message = internalMessage.AS4Message;
            ICollection<UserMessage> userMessages = as4Message.UserMessages;

            Func<UserMessage, bool> whereMessageIdIsDifferent = m => !m.MessageId.Equals(as4Message.PrimaryUserMessage.MessageId);
            UserMessage otherUserMessage = userMessages.FirstOrDefault(whereMessageIdIsDifferent);
            if (otherUserMessage != null) userMessages.Remove(otherUserMessage);
        }

        private void AssignMessageInfo(UserMessage userMessage)
        {
            AddMessageProperty("MessageId", userMessage.MessageId);
        }

        private static void AssignParties(UserMessage userMessage)
        {
            userMessage.Sender.Role = $"{ConformanceUriPrefix}/sut";
            userMessage.Sender.PartyIds.First().Id = "as4-net-c3";

            userMessage.Receiver.Role = $"{ConformanceUriPrefix}/testdriver";
            userMessage.Receiver.PartyIds.First().Id = "minder";
        }

        private void AssignCollaborationInfoProperties(UserMessage userMessage)
        {
            CollaborationInfo info = userMessage.CollaborationInfo;
            AddMessageProperty("Service", info.Service.Value);
            AddMessageProperty("Action", info.Action);
            AddMessageProperty("ConversationId", info.ConversationId);
        }

        private static void AssignDeliverServiceAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Deliver";
            userMessage.CollaborationInfo.Service.Value = ConformanceUriPrefix;
            userMessage.CollaborationInfo.ConversationId = "1";
        }

        private void AssignPartyProperties(UserMessage userMessage)
        {
            AddMessageProperty("FromPartyId", userMessage.Sender.PartyIds.First().Id);
            AddMessageProperty("FromPartyRole", userMessage.Sender.Role);

            AddMessageProperty("ToPartyId", userMessage.Receiver.PartyIds.First().Id);
            AddMessageProperty("ToPartyRole", userMessage.Receiver.Role);
        }

        private void AddMessageProperty(string key, string value)
        {
            this._properties.Add(new MessageProperty(key, value));
        }
    }
}