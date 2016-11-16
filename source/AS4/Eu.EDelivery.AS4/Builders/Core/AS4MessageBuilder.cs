using System;
using System.Collections.Generic;
using System.IO;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Builders.Core
{
    /// <summary>
    /// Builder for AS4Messages
    /// </summary>
    public class AS4MessageBuilder
    {
        private readonly List<Attachment> _attachments;
        private readonly List<SignalMessage> _signalMessages;
        private readonly List<UserMessage> _userMessages;
        private SendingProcessingMode _sendPMode;
        private ReceivingProcessingMode _receivePMode;

        public AS4MessageBuilder()
        {
            this._userMessages = new List<UserMessage>();
            this._signalMessages = new List<SignalMessage>();
            this._attachments = new List<Attachment>();
            this._sendPMode = new SendingProcessingMode();
            this._receivePMode = new ReceivingProcessingMode();
        }

        /// <summary>
        /// Assign a <see cref="SendingProcessingMode" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithSendingPMode(SendingProcessingMode pmode)
        {
            if (pmode == null)
                throw new ArgumentNullException(nameof(pmode));

            this._sendPMode = pmode;
            return this;
        }

        /// <summary>
        /// Assign a <see cref="ReceivingProcessingMode"/> to the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="receivePMode"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithReceivingPMode(ReceivingProcessingMode receivePMode)
        {
            if (receivePMode == null)
                throw new ArgumentNullException(nameof(receivePMode));

            this._receivePMode = receivePMode;
            return this;
        }

        /// <summary>
        /// Add a <see cref="PullRequest" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="mpc">Message Partition Channel</param>
        /// <returns></returns>
        public AS4MessageBuilder WithPullRequest(string mpc)
        {
            if (mpc == null)
                throw new ArgumentNullException(nameof(mpc));

            SignalMessage signalMessage = new PullRequest(mpc);
            this._signalMessages.Add(signalMessage);
            return this;
        }

        /// <summary>
        /// Add a <see cref="UserMessage" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithUserMessage(UserMessage userMessage)
        {
            if (userMessage == null)
                throw new ArgumentNullException(nameof(userMessage));

            this._userMessages.Add(userMessage);
            return this;
        }

        /// <summary>
        /// Add a <see cref="SignalMessage"/> to the <see cref="AS4Message"/>
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithSignalMessage(SignalMessage signalMessage)
        {
            if (signalMessage == null)
                throw new ArgumentNullException(nameof(signalMessage));

            this._signalMessages.Add(signalMessage);
            return this;
        }

        /// <summary>
        /// Add a <see cref="Attachment" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithAttachment(Attachment attachment)
        {
            if (attachment == null)
                throw new ArgumentNullException(nameof(attachment));

            this._attachments.Add(attachment);
            return this;
        }

        /// <summary>
        /// Build the <see cref="AS4Message" />
        /// </summary>
        /// <returns></returns>
        public AS4Message Build()
        {
            var message = new AS4Message();

            BuildingUsermessages(message);
            BuildingSignalMessages(message);
            BuildingAttachments(message);

            message.SendingPMode = this._sendPMode;
            message.ReceivingPMode = this._receivePMode;

            return message;
        }

        private void BuildingUsermessages(AS4Message message)
        {
            for (int i = 0, l = this._userMessages.Count; i < l; i++)
                message.UserMessages.Add(this._userMessages[i]);
        }

        private void BuildingSignalMessages(AS4Message message)
        {
            for (int i = 0, l = this._signalMessages.Count; i < l; i++)
                message.SignalMessages.Add(this._signalMessages[i]);
        }

        private void BuildingAttachments(AS4Message message)
        {
            for (int i = 0, l = this._attachments.Count; i < l; i++)
                message.AddAttachment(this._attachments[i]);
        }

        /// <summary>
        /// Break down the Builder
        /// </summary>
        /// <returns></returns>
        public AS4MessageBuilder BreakDown()
        {
            this._userMessages.Clear();
            this._signalMessages.Clear();
            this._attachments.Clear();
            this._sendPMode = new SendingProcessingMode();
            this._receivePMode = new ReceivingProcessingMode();

            return this;
        }
    }
}