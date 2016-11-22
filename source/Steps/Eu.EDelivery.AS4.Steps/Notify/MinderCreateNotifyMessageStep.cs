using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Notify Message
    /// </summary>
    public class MinderCreateNotifyMessageStep : IStep
    {
        private IList<MessageProperty> _properties;

        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            AssignNotifyAction(userMessage);
            AssignFromPartyRole(userMessage);

            this._properties = userMessage.MessageProperties;
            AddSignalMessageProperties(signalMessage);

            return StepResult.SuccessAsync(internalMessage);
        }

        private void AssignNotifyAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Notify";
        }

        private void AssignFromPartyRole(UserMessage userMessage)
        {
            userMessage.Sender.Role = "http://www.esens.eu/as4/conformancetest/sut";
        }

        private void AddSignalMessageProperties(SignalMessage signalMessage)
        {
            this._properties.Add(new MessageProperty("RefToMessageId", signalMessage.RefToMessageId));
            this._properties.Add(new MessageProperty("SignalType", signalMessage.GetType().Name));
        }
    }
}