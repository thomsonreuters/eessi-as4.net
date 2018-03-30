using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Singletons;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    [ExcludeFromCodeCoverage]
    public abstract class MinderNotifyMessageTransformer : ITransformer
    {
        protected abstract string MinderUriPrefix { get; }
        protected Logger Logger => LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Configures the <see cref="ITransformer"/> implementation with specific user-defined properties.
        /// </summary>
        /// <param name="properties">The properties.</param>
        public void Configure(IDictionary<string, string> properties) { }

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="MessagingContext"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <returns></returns>
        public async Task<MessagingContext> TransformAsync(ReceivedMessage message)
        {
            var as4Transformer = new AS4MessageTransformer();
            
            MessagingContext context = await as4Transformer.TransformAsync(message);

            var receivedEntityMessage = message as ReceivedEntityMessage;
            if (receivedEntityMessage == null)
            {
                throw new NotSupportedException($"Minder Notify Transformer only supports transforming instances of type {typeof(ReceivedEntityMessage)}");
            }

            NotifyMessageEnvelope notifyMessage = await CreateNotifyMessageEnvelope(context.AS4Message, receivedEntityMessage.Entity.GetType());
            context.ModifyContext(notifyMessage);

            return context;
        }

        internal async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            UserMessage userMessage = as4Message.PrimaryUserMessage;
            SignalMessage signalMessage = as4Message.PrimarySignalMessage;

            if (signalMessage != null)
            {
                Logger.Info($"Minder Create Notify Message as {signalMessage.GetType().Name}");
            }
            else
            {
                Logger.Warn($"{as4Message.PrimaryUserMessage?.MessageId} AS4Message does not contain a primary SignalMessage");
            }

            return await CreateMinderNotifyMessageEnvelope(userMessage, signalMessage, receivedEntityType).ConfigureAwait(false);
        }

        private async Task<NotifyMessageEnvelope> CreateMinderNotifyMessageEnvelope(
            UserMessage userMessage, SignalMessage signalMessage, Type receivedEntityMessageType)
        {
            if (userMessage == null && signalMessage != null)
            {
                userMessage = await RetrieveRelatedUserMessage(signalMessage);
            }

            if (userMessage == null)
            {
                Logger.Warn("The related usermessage for the received signalmessage could not be found");
                userMessage = new UserMessage();
            }

            AssignMinderProperties(userMessage, signalMessage);

            var notifyMessage = AS4Mapper.Map<NotifyMessage>(signalMessage);

            // The NotifyMessage that Minder expects, is an AS4Message which contains the specific UserMessage.
            var msg = AS4Message.Create(userMessage, new SendingProcessingMode());
            var serializer = Registry.Instance.SerializerProvider.Get(msg.ContentType);

            byte[] content;

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream, CancellationToken.None);
                content = memoryStream.ToArray();
            }

            return new NotifyMessageEnvelope(notifyMessage.MessageInfo, notifyMessage.StatusInfo.Status, content, msg.ContentType, receivedEntityMessageType);
        }

        private static async Task<UserMessage> RetrieveRelatedUserMessage(SignalMessage signalMessage)
        {
            using (var db = Registry.Instance.CreateDatastoreContext())
            {
                UserMessage userMessage = null;

                MessageEntity ent = db.InMessages.FirstOrDefault(
                    m =>
                        m.EbmsMessageId == signalMessage.RefToMessageId &&
                        m.EbmsMessageType == MessageType.UserMessage.ToString());

                if (ent == null)
                {
                    ent = db.OutMessages.FirstOrDefault(
                        m =>
                            m.EbmsMessageId == signalMessage.RefToMessageId &&
                            m.EbmsMessageType == MessageType.UserMessage.ToString());
                }

                if (ent != null)
                {
                    using (var stream = await ent.RetrieveMessageBody(Registry.Instance.MessageBodyStore))
                    {
                        stream.Position = 0;
                        var s = Registry.Instance.SerializerProvider.Get(ent.ContentType);
                        var result =
                            await s.DeserializeAsync(stream, ent.ContentType, CancellationToken.None);

                        if (result != null)
                        {
                            userMessage =
                                result.UserMessages.FirstOrDefault(m => m.MessageId == signalMessage.RefToMessageId);
                        }
                    }
                }

                return userMessage;
            }
        }

        private void AssignMinderProperties(UserMessage userMessage, SignalMessage signalMessage)
        {
            AssignToPartyIdentification(userMessage);
            AssignServiceAction(userMessage);

            if (signalMessage != null)
            {
                userMessage.MessageProperties.Add(new MessageProperty("RefToMessageId", signalMessage.RefToMessageId));
                userMessage.MessageProperties.Add(new MessageProperty("SignalType", signalMessage.GetType().Name));

                userMessage.RefToMessageId = signalMessage.MessageId;
            }
        }

        private void AssignServiceAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Notify";
            userMessage.CollaborationInfo.Service.Value = MinderUriPrefix;
        }

        private void AssignToPartyIdentification(UserMessage userMessage)
        {
            userMessage.Receiver.PartyIds.First().Id = "minder";
            userMessage.Receiver.Role = $"{MinderUriPrefix}/testdriver";
        }
    }
}
