using System;
using System.Collections.Generic;
using System.Linq;
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
        private readonly SendingProcessingMode _sendPmode;
        private readonly List<SignalMessage> _signalMessages;
        private readonly List<UserMessage> _userMessages;

        public AS4MessageBuilder() : this(new SendingProcessingMode {MessagePackaging = {IsMultiHop = false}}) {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageBuilder" /> class.
        /// </summary>
        /// <param name="pmode">The pmode.</param>
        public AS4MessageBuilder(SendingProcessingMode pmode)
        {
            _sendPmode = pmode;
            _userMessages = new List<UserMessage>();
            _signalMessages = new List<SignalMessage>();
            _attachments = new List<Attachment>();
        }

        /// <summary>
        /// For the message unit.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4MessageBuilder ForMessageUnit(SignalMessage message, SendingProcessingMode pmode)
        {
            var builder = new AS4MessageBuilder(pmode);
            builder.WithSignalMessage(message);

            return builder;
        }

        /// <summary>
        /// For the message unit.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="pmode">The pmode.</param>
        /// <returns></returns>
        public static AS4MessageBuilder ForMessageUnit(UserMessage message, SendingProcessingMode pmode)
        {
            var builder = new AS4MessageBuilder(pmode);
            builder.WithUserMessage(message);

            return builder;
        }

        /// <summary>
        /// Add a <see cref="Attachment" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="attachment"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithAttachment(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            _attachments.Add(attachment);
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
            {
                throw new ArgumentNullException(nameof(mpc));
            }

            SignalMessage signalMessage = new PullRequest(mpc);
            _signalMessages.Add(signalMessage);
            return this;
        }

        /// <summary>
        /// Add a <see cref="SignalMessage" /> to the <see cref="AS4Message" />
        /// </summary>
        /// <param name="signalMessage"></param>
        /// <returns></returns>
        public AS4MessageBuilder WithSignalMessage(SignalMessage signalMessage)
        {
            if (signalMessage == null)
            {
                throw new ArgumentNullException(nameof(signalMessage));
            }

            _signalMessages.Add(signalMessage);
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
            {
                throw new ArgumentNullException(nameof(userMessage));
            }

            _userMessages.Add(userMessage);
            return this;
        }

        /// <summary>
        /// Build the <see cref="AS4Message" />
        /// </summary>
        /// <returns></returns>
        public AS4Message Build()
        {
            AS4Message message = AS4Message.Create(_sendPmode);

            BuildingUsermessages(message);
            BuildingSignalMessages(message);
            BuildingAttachments(message);

            return message;
        }

        private void BuildingUsermessages(AS4Message message)
        {
            for (int i = 0, l = _userMessages.Count; i < l; i++)
            {
                message.UserMessages.Add(_userMessages[i]);
            }
        }

        private void BuildingSignalMessages(AS4Message message)
        {
            for (int i = 0, l = _signalMessages.Count; i < l; i++)
            {
                message.SignalMessages.Add(_signalMessages[i]);
            }
        }

        private void BuildingAttachments(AS4Message message)
        {
            for (int i = 0, l = _attachments.Count; i < l; i++)
            {
                message.AddAttachment(_attachments[i]);
            }
        }
    }
}