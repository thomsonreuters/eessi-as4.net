using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Common;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Internal Message Builder to create an <see cref="MessagingContext" />
    /// </summary>
    public class InternalMessageBuilder
    {
        private readonly MessagingContext _messagingContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="InternalMessageBuilder"/> class.
        /// </summary>
        /// <param name="messageId">The message Id.</param>
        public InternalMessageBuilder(string messageId = "message-id")
        {
            _messagingContext = new MessagingContext(AS4Message.Empty);
            UserMessage userMessage = CreateDefaultUserMessage(messageId);
            _messagingContext.AS4Message.UserMessages.Add(userMessage);
        }

        /// <summary>
        /// Build to the Builder
        /// </summary>
        /// <returns></returns>
        public MessagingContext Build()
        {
            return _messagingContext;
        }

        /// <summary>
        /// Add Agreement Reference to the Builder
        /// </summary>
        /// <param name="agreementRef"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithAgreementRef(AgreementReference agreementRef)
        {
            UserMessage userMessage = _messagingContext.AS4Message.PrimaryUserMessage;
            userMessage.CollaborationInfo.AgreementReference = agreementRef;

            return this;
        }

        /// <summary>
        /// Add Parties to the Builder
        /// </summary>
        /// <param name="fromParty"></param>
        /// <param name="toParty"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithPartys(Party fromParty, Party toParty)
        {
            UserMessage userMessage = _messagingContext.AS4Message.UserMessages.First();
            userMessage.Sender = fromParty;
            userMessage.Receiver = toParty;

            return this;
        }

        /// <summary>
        /// Add a PMode Id to the Builder
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithPModeId(string pmodeId)
        {
            _messagingContext.AS4Message.UserMessages.First().CollaborationInfo.AgreementReference.PModeId = pmodeId;

            return this;
        }

        /// <summary>
        /// Add Service/Action to the Builder
        /// </summary>
        /// <param name="service"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithServiceAction(string service, string action)
        {
            UserMessage userMessage = _messagingContext.AS4Message.UserMessages.First();
            userMessage.CollaborationInfo.Action = action;
            userMessage.CollaborationInfo.Service.Value = service;

            return this;
        }

        /// <summary>
        /// Add a SignalMessage to the Builder
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithSignalMessage(SignalMessage signalMessage)
        {
            _messagingContext.AS4Message.SignalMessages = new List<SignalMessage> {signalMessage};

            return this;
        }

        /// <summary>
        /// Add a UserMessage to the Builder
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithUserMessage(UserMessage userMessage)
        {
            _messagingContext.AS4Message.UserMessages = new List<UserMessage> {userMessage};

            return this;
        }

        private static UserMessage CreateDefaultUserMessage(string messageId)
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            var userMessage = new UserMessage
            {
                CollaborationInfo = {AgreementReference = new AgreementReference()},
                MessageId = messageId
            };

            return userMessage;
        }
    }
}