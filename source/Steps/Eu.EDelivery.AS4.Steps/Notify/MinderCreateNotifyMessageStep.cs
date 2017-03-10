using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
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

namespace Eu.EDelivery.AS4.Steps.Notify
{
    public abstract class MinderCreateNotifyMessageStep : IStep
    {
        // TODO: this step should be replaced by a Transformer
       
        private readonly ILogger _logger;

        protected abstract string MinderUriPrefix { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConformanceTestCreateNotifyMessageStep"/> class
        /// </summary>
        protected MinderCreateNotifyMessageStep()
        {
            this._logger = LogManager.GetCurrentClassLogger();
        }

        /// <summary>
        /// Start create notify message step
        /// </summary>
        /// <param name="internalMessage"></param>        
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<StepResult> ExecuteAsync(InternalMessage internalMessage, CancellationToken cancellationToken)
        {
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

            var notifyEnvelope = await CreateMinderNotifyMessageEnvelope(userMessage, signalMessage);

            internalMessage.NotifyMessage = notifyEnvelope;

            return await StepResult.SuccessAsync(internalMessage);
        }

        private async Task<NotifyMessageEnvelope> CreateMinderNotifyMessageEnvelope(UserMessage userMessage, SignalMessage signalMessage)
        {
            if (userMessage == null && signalMessage != null)
            {
                userMessage = await RetrieveRelatedUserMessage(signalMessage);
            }

            if (userMessage == null)
            {
                this._logger.Warn("The related usermessage for the received signalmessage could not be found");
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
                    using (var stream = new MemoryStream(ent.MessageBody))
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
//            userMessage.Sender = new Party($"{MinderUriPrefix}/sut", userMessage.Receiver.PartyIds.FirstOrDefault());

            userMessage.Receiver.PartyIds.First().Id = "minder";
            userMessage.Receiver.Role = $"{MinderUriPrefix}/testdriver";
        }

    }
}
