using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using System.Linq;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using NLog;
using System.IO;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Singletons;
using SignalMessage = Eu.EDelivery.AS4.Model.Core.SignalMessage;
using UserMessage = Eu.EDelivery.AS4.Model.Core.UserMessage;

namespace Eu.EDelivery.AS4.Steps.Notify
{
    /// <summary>
    /// Assemble a <see cref="AS4Message"/> as Notify Message
    /// </summary>
    public class MinderCreateNotifyMessageStep : IStep
    {
        private const string ConformanceUriPrefix = "http://www.esens.eu/as4/conformancetest";
        private readonly ILogger _logger;


        /// <summary>
        /// Initializes a new instance of the <see cref="MinderCreateNotifyMessageStep"/> class
        /// </summary>
        public MinderCreateNotifyMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start create notify message step
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
            this._logger.Info("Minder Create Notify Message");

            UserMessage userMessage = internalMessage.AS4Message.PrimaryUserMessage;
            SignalMessage signalMessage = internalMessage.AS4Message.PrimarySignalMessage;

            if (signalMessage != null)
            {
                this._logger.Info($"Minder Create Notify Message as {signalMessage.GetType().Name}");
            }
            else
            {
                this._logger.Warn($"{internalMessage.Prefix} AS4Message does not contain a primary SignalMessage");
            }

            var notifyEnvelope = CreateMinderNotifyMessageEnvelope(userMessage, signalMessage);

            internalMessage.NotifyMessage = notifyEnvelope;

            return StepResult.SuccessAsync(internalMessage);
        }

        private NotifyMessageEnvelope CreateMinderNotifyMessageEnvelope(UserMessage userMessage, SignalMessage signalMessage)
        {
            if (userMessage == null && signalMessage != null)
            {
                userMessage = RetrieveRelatedUserMessage(signalMessage);
            }

            if (userMessage == null)
            {
                throw new InvalidOperationException("No UserMessage found which could be used as a Notify Message.");
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

        private static UserMessage RetrieveRelatedUserMessage(SignalMessage signalMessage)
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
                    using (var stream = new MemoryStream(ent.MessageBody))

                    {
                        stream.Position = 0;
                        var s = Registry.Instance.SerializerProvider.Get(ent.ContentType);
                        var result =
                            s.DeserializeAsync(stream, ent.ContentType, CancellationToken.None).GetAwaiter().GetResult();

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
            AssignServiceAction(userMessage);
            AssignFromPartyRole(userMessage);

            if (signalMessage != null)
            {
                userMessage.MessageProperties.Add(new MessageProperty("RefToMessageId", signalMessage.RefToMessageId));
                userMessage.MessageProperties.Add(new MessageProperty("SignalType", signalMessage.GetType().Name));

                userMessage.RefToMessageId = signalMessage.MessageId;
            }
        }

        private static void AssignServiceAction(UserMessage userMessage)
        {
            userMessage.CollaborationInfo.Action = "Notify";
            userMessage.CollaborationInfo.Service.Value = ConformanceUriPrefix;
            userMessage.CollaborationInfo.ConversationId = "1";
        }

        private static void AssignFromPartyRole(UserMessage userMessage)
        {
            userMessage.Sender.PartyIds.First().Id = "as4-net-c2";
            userMessage.Sender.Role = $"{ConformanceUriPrefix}/sut";
            userMessage.Receiver.PartyIds.First().Id = "minder";
            userMessage.Receiver.Role = $"{ConformanceUriPrefix}/testdriver";
        }

    }
}