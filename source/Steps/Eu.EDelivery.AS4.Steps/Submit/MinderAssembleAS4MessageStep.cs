using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep"/> implementation to assemble the <see cref="AS4Message"/>
    /// with the given Message Properties
    /// </summary>
    [Obsolete("This Minder specific step should no longer be used.")]
    public class MinderAssembleAS4MessageStep : IStep
    {
        private IList<MessageProperty> _properties;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderAssembleAS4MessageStep"/> class
        /// </summary>
        public MinderAssembleAS4MessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start assembling Minder AS4 message
        /// </summary>
        /// <param name="internalMessage"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Assemble AS4 Message");
            AssembleAS4Message(internalMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssembleAS4Message(InternalMessage internalMessage)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            this._properties = userMessage.MessageProperties;

            AssignMessageProperties(userMessage);
            RemoveUnusedAgreementRef(userMessage);
            RemoveAllInfoMessageProperties(userMessage);

            internalMessage.AS4Message.SecurityHeader = new SecurityHeader();
        }

        private void AssignMessageProperties(UserMessage userMessage)
        {
            AssignMessageInfoProperties(userMessage);
            AssignConversationIdProperty(userMessage);
            AssignSenderProperties(userMessage);
            AssignReceiverProperties(userMessage);
            AssignServiceActionProperties(userMessage);
        }

        private void AssignMessageInfoProperties(UserMessage userMessage)
        {
            userMessage.MessageId = GetMessageProperty("MessageId");
            userMessage.RefToMessageId = GetMessageProperty("RefToMessageId");
            userMessage.Timestamp = DateTimeOffset.UtcNow;
        }

        private void AssignConversationIdProperty(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.ConversationId = GetMessageProperty("ConversationId");
        }

        private void AssignSenderProperties(UserMessage userMessage)
        {
            userMessage.Sender.PartyIds.First().Id = GetMessageProperty("FromPartyId");
            userMessage.Sender.Role = GetMessageProperty("FromPartyRole");
        }

        private void AssignReceiverProperties(UserMessage userMessage)
        {
            userMessage.Receiver.PartyIds.First().Id = GetMessageProperty("ToPartyId");
            userMessage.Receiver.Role = GetMessageProperty("ToPartyRole");
        }

        private void AssignServiceActionProperties(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Service.Value = GetMessageProperty("Service");
            userMessage.CollaborationInfo.Action = GetMessageProperty("Action");
        }

        private static void RemoveUnusedAgreementRef(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.AgreementReference = null;
        }

        private string GetMessageProperty(string propertyName)
        {
            return this._properties.FirstOrDefault(p => p.Name.Equals(propertyName))?.Value;
        }

        private void RemoveAllInfoMessageProperties(UserMessage userMessage)
        {
            userMessage.MessageProperties = this._properties.Where(WherePropertyIsInWhiteList).ToList();
        }

        private static bool WherePropertyIsInWhiteList(MessageProperty property)
        {
            return property.Name.Equals("originalSender") ||
                   property.Name.Equals("finalRecipient") ||
                   property.Name.Equals("trackingIdentifier");
        }
    }
}