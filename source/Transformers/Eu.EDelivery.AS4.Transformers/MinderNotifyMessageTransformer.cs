using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;
using NLog;

namespace Eu.EDelivery.AS4.Transformers
{
    [ExcludeFromCodeCoverage]
    public abstract class MinderNotifyMessageTransformer : ITransformer
    {
        protected abstract string MinderUriPrefix { get; }

        protected Logger Logger = LogManager.GetCurrentClassLogger();

        /// <summary>
        /// Transform a given <see cref="ReceivedMessage"/> to a Canonical <see cref="InternalMessage"/> instance.
        /// </summary>
        /// <param name="message">Given message to transform.</param>
        /// <param name="cancellationToken">Cancellation which stops the transforming.</param>
        /// <returns></returns>
        public async Task<InternalMessage> TransformAsync(ReceivedMessage message, CancellationToken cancellationToken)
        {
            var as4Transformer = new AS4MessageTransformer();
            var internalMessage = await as4Transformer.TransformAsync(message, cancellationToken);

            internalMessage.NotifyMessage = await CreateNotifyMessageEnvelope(internalMessage.AS4Message);

            return internalMessage;
        }

        internal async Task<NotifyMessageEnvelope> CreateNotifyMessageEnvelope(AS4Message as4Message)
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

            var notifyEnvelope = await CreateMinderNotifyMessageEnvelope(userMessage, signalMessage).ConfigureAwait(false);

            return notifyEnvelope;
        }

        private async Task<NotifyMessageEnvelope> CreateMinderNotifyMessageEnvelope(UserMessage userMessage, SignalMessage signalMessage)
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
            var msg = new AS4MessageBuilder().WithUserMessage(userMessage).Build();

            var serializer = Registry.Instance.SerializerProvider.Get(msg.ContentType);

            byte[] content;

            using (var memoryStream = new MemoryStream())
            {
                serializer.Serialize(msg, memoryStream, CancellationToken.None);
                content = memoryStream.ToArray();
            }

            return new NotifyMessageEnvelope(notifyMessage.MessageInfo, notifyMessage.StatusInfo.Status, content, msg.ContentType);
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
                    using (var stream = ent.RetrieveMessageBody(Registry.Instance.MessageBodyRetrieverProvider))
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
