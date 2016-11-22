using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Deliver
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Deliver Message
    /// </summary>
    public class MinderCreateDeliverMessageStep : IStep
    {
        private IList<MessageProperty> _properties;

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            AssignDeliverAction(userMessage);

            this._properties = userMessage.MessageProperties;
            AssignMessageInfo(userMessage);
            AssignPartyProperties(userMessage);
            AssignCollaborationInfoProperties(userMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssignMessageInfo(UserMessage userMessage)
        {
            this._properties.Add(new MessageProperty("MessageId", userMessage.MessageId));
        }

        private void AssignCollaborationInfoProperties(UserMessage userMessage)
        {
            this._properties.Add(new MessageProperty("Service", userMessage.CollaborationInfo.Service.Value));
            this._properties.Add(new MessageProperty("Action", userMessage.CollaborationInfo.Action));
            this._properties.Add(new MessageProperty("ConversationId", userMessage.CollaborationInfo.ConversationId));
        }

        private void AssignDeliverAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Deliver";
        }

        private void AssignPartyProperties(UserMessage userMessage)
        {
            this._properties.Add(new MessageProperty("FromPartyId", userMessage.Sender.PartyIds.First().Id));
            this._properties.Add(new MessageProperty("FromPartyRole", userMessage.Sender.Role));
            this._properties.Add(new MessageProperty("ToPartyId", userMessage.Receiver.PartyIds.First().Id));
            this._properties.Add(new MessageProperty("ToPartyRole", userMessage.Sender.Role));
        }
    }
}
