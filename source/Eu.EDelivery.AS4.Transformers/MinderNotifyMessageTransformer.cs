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
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Singletons;
using NLog;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

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
            var receivedEntityMessage = message as ReceivedEntityMessage;
            if (receivedEntityMessage == null)
            {
                throw new NotSupportedException(
                    $"Minder Notify Transformer only supports transforming instances of type {typeof(ReceivedEntityMessage)}");
            }

            var as4Transformer = new AS4MessageTransformer();
            MessagingContext context = await as4Transformer.TransformAsync(message);

            NotifyMessageEnvelope notifyMessage = await CreateNotifyMessageEnvelope(context.AS4Message, receivedEntityMessage.Entity.GetType());
            context.ModifyContext(notifyMessage);

            return context;
        }

        internal async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message, Type receivedEntityType)
        {
            UserMessage userMessage = as4Message.FirstUserMessage;
            SignalMessage signalMessage = as4Message.FirstSignalMessage;

            if (signalMessage != null)
            {
                Logger.Info($"Minder Create Notify Message as {signalMessage.GetType().Name}");
            }
            else
            {
                Logger.Warn($"{as4Message.FirstUserMessage?.MessageId} AS4Message does not contain a primary SignalMessage");
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

            UserMessage minderUserMessage = CreateUserMessageFromMinderProperties(userMessage, signalMessage);

            var notifyMessage = AS4Mapper.Map<NotifyMessage>(signalMessage);

            // The NotifyMessage that Minder expects, is an AS4Message which contains the specific UserMessage.
            var msg = AS4Message.Create(minderUserMessage, new SendingProcessingMode());
            var serializer = SerializerProvider.Default.Get(msg.ContentType);

            byte[] content;

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream);
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
                        m.EbmsMessageType == MessageType.UserMessage);

                if (ent == null)
                {
                    ent = db.OutMessages.FirstOrDefault(
                        m =>
                            m.EbmsMessageId == signalMessage.RefToMessageId &&
                            m.EbmsMessageType == MessageType.UserMessage);
                }

                if (ent != null)
                {
                    using (var stream = await ent.RetrieveMessageBody(Registry.Instance.MessageBodyStore))
                    {
                        stream.Position = 0;
                        var s = SerializerProvider.Default.Get(ent.ContentType);
                        var result =
                            await s.DeserializeAsync(stream, ent.ContentType);

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

        private UserMessage CreateUserMessageFromMinderProperties(UserMessage userMessage, SignalMessage signalMessage)
        {
            var receiver =
                new Model.Core.Party(
                    role: $"{MinderUriPrefix}/testdriver", 
                    partyId: new Model.Core.PartyId(id: "minder"));

            var collaboration = new CollaborationInfo(
                Maybe<AgreementReference>.Nothing,
                new Service(MinderUriPrefix),
                "Notify",
                CollaborationInfo.DefaultConversationId);

            IEnumerable<MessageProperty> props =
                signalMessage != null
                    ? new[]
                    {
                        new MessageProperty("RefToMessageId", signalMessage.RefToMessageId),
                        new MessageProperty("SignalType", signalMessage.GetType() .Name)
                    }
                    : Enumerable.Empty<MessageProperty>();

            return new UserMessage(
                userMessage.MessageId,
                signalMessage != null ? signalMessage.RefToMessageId : userMessage.RefToMessageId,
                collaboration,
                userMessage.Sender,
                receiver,
                userMessage.PayloadInfo,
                userMessage.MessageProperties.Concat(props));
        }
    }
}
