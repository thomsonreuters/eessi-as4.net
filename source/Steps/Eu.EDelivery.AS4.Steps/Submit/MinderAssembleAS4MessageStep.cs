using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Submit
{
    /// <summary>
    /// <see cref="IStep"/> implementation to assemble the <see cref="AS4Message"/>
    /// with the given Message Properties
    /// </summary>
    public class MinderAssembleAS4MessageStep : IStep
    {
        private IList<MessageProperty> _properties;

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            this._properties = userMessage.MessageProperties;

            AssignMessageProperties(userMessage);
            RemoveUnusedAgreementRef(userMessage);
            RemoveAllInfoMessageProperties();

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssignMessageProperties(UserMessage userMessage)
        {
            userMessage.MessageId = GetMessageProperty("MessageId");
            userMessage.CollaborationInfo.ConversationId = GetMessageProperty("ConversationId");
            userMessage.Sender.PartyIds.First().Id = GetMessageProperty("FromPartyId");
            userMessage.Sender.Role = GetMessageProperty("FromPartyRole");
            userMessage.Receiver.PartyIds.First().Id = GetMessageProperty("ToPartyId");
            userMessage.Receiver.Role = GetMessageProperty("ToPartyRole");
            userMessage.CollaborationInfo.Service.Value = GetMessageProperty("Service");
            userMessage.CollaborationInfo.Action = GetMessageProperty("Action");
        }

        private void RemoveUnusedAgreementRef(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.AgreementReference = null;
        }

        private string GetMessageProperty(string propertyName)
        {
            return this._properties.FirstOrDefault(p => p.Name.Equals(propertyName))?.Value;
        }

        private void RemoveAllInfoMessageProperties()
        {
            foreach (MessageProperty property in this._properties)
                if (!property.Name.Equals("originalSender") && !property.Name.Equals("finalRecipient"))
                    this._properties.Remove(property);
        }
    }
}
