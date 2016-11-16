using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;

namespace Eu.EDelivery.AS4.UnitTests.Builders.Core
{
    /// <summary>
    /// Internal Message Builder to create an <see cref="InternalMessage"/>
    /// </summary>
    public class InternalMessageBuilder
    {
        private readonly InternalMessage _internalMessage;

        /// <summary>
        /// Create a <see cref="InternalMessage"/> Builder
        /// </summary>
        public InternalMessageBuilder(string messageId = null)
        {
            this._internalMessage = new InternalMessage {AS4Message = new AS4Message()};
            UserMessage userMessage = CreateDefaultUserMessage(messageId);
            this._internalMessage.AS4Message.UserMessages.Add(userMessage);
        }

        /// <summary>
        /// Add a PMode Id to the Builder
        /// </summary>
        /// <param name="pmodeId"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithPModeId(string pmodeId)
        {
            this._internalMessage.AS4Message.UserMessages.First()
                .CollaborationInfo.AgreementReference.PModeId = pmodeId;

            return this;
        }

        /// <summary>
        /// Add a UserMessage to the Builder
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithUserMessage(UserMessage userMessage)
        {
            this._internalMessage.AS4Message.UserMessages = new List<UserMessage> {userMessage};

            return this;
        }

        /// <summary>
        /// Add a SignalMessage to the Builder
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithSignalMessage(SignalMessage signalMessage)
        {
            this._internalMessage.AS4Message.SignalMessages = new List<SignalMessage> {signalMessage};

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
            UserMessage userMessage = this._internalMessage.AS4Message.UserMessages.First();
            userMessage.Sender = fromParty;
            userMessage.Receiver = toParty;

            return this;
        }

        /// <summary>
        /// Add Agreement Reference to the Builder
        /// </summary>
        /// <param name="agreementRef"></param>
        /// <returns></returns>
        public InternalMessageBuilder WithAgreementRef(AgreementReference agreementRef)
        {
            UserMessage userMessage = this._internalMessage.AS4Message.PrimaryUserMessage;
            userMessage.CollaborationInfo.AgreementReference = agreementRef;

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
            UserMessage userMessage = this._internalMessage.AS4Message.UserMessages.First();
            userMessage.CollaborationInfo.Action = action;
            userMessage.CollaborationInfo.Service.Name = service;

            return this;
        }

        /// <summary>
        /// Build to the Builder
        /// </summary>
        /// <returns></returns>
        public InternalMessage Build()
        {
            return this._internalMessage;
        }

        private UserMessage CreateDefaultUserMessage(string messageId = null)
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            var userMessage = new UserMessage
            {
                CollaborationInfo = {AgreementReference = new AgreementReference()}
            };
            if (messageId != null) userMessage.MessageId = messageId;
            return userMessage;
        }
    }
}