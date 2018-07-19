using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Tests the <see cref="NotifyMessageTransformer"/> to notify on signal-messages
    /// </summary>
    public class GivenSignalToNotifyMessageTransformerFacts
    {
        [Fact]
        public async Task FailsToTransform_IfMessageTypeIsNotSupported()
        {
            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => ExerciseTransform(new ReceivedMessage(Stream.Null)));
        }

        [Fact]
        public async Task FailsToTransform_IfMessageDoesntHaveAnyMatchingSignalMessages()
        {
            // Arrange
            ReceivedMessageEntityMessage receival = await CreateInvalidReceivedReceiptMessage();

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => ExerciseTransform(receival));
        }

        [Fact]
        public async Task ThenNotifyMessageHasCorrectStatusCode()
        {
            // Arrange
            ReceivedMessageEntityMessage receivedSignal = await CreateReceivedReceiptMessage();

            // Act
            MessagingContext result = await ExerciseTransform(receivedSignal);

            // Assert
            NotifyMessageEnvelope notifyMessage = result.NotifyMessage;
            Assert.NotNull(notifyMessage);
            Assert.Equal(Status.Delivered, notifyMessage.StatusCode);
        }

        [Fact]
        public async Task ThenSignalMessageIsTransformedToNotifyEnvelopeWithCorrectContents()
        {
            // Arrange
            ReceivedMessageEntityMessage receivedSignal = await CreateReceivedReceiptMessage();

            // Act
            MessagingContext result = await ExerciseTransform(receivedSignal);

            // Assert
            Assert.NotNull(result.NotifyMessage);

            var notifyMessage =
                AS4XmlSerializer.FromString<NotifyMessage>(Encoding.UTF8.GetString(result.NotifyMessage.NotifyMessage));

            Assert.NotNull(notifyMessage);

            // Assert: check if the original Receipt is a part of the NotifyMessage.
            var document = new XmlDocument { PreserveWhitespace = true };
            document.LoadXml(Encoding.UTF8.GetString(((MemoryStream)receivedSignal.UnderlyingStream).ToArray()));

            Assert.Equal(
                Canonicalize(document.SelectSingleNode("//*[local-name()='SignalMessage']")),
                Canonicalize(notifyMessage.StatusInfo.Any.First()));
        }

        [Fact]
        public async Task ThenSignalMessageIsTransformedToNotifyEnvelopeWithCorrectMessageInfo()
        {
            // Arrange
            ReceivedMessageEntityMessage receivedSignal = await CreateReceivedReceiptMessage();
            MessageEntity receivedMessageEntity = receivedSignal.MessageEntity;

            // Act
            MessagingContext result = await ExerciseTransform(receivedSignal);

            // Assert
            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(receivedMessageEntity.EbmsMessageId, result.NotifyMessage.MessageInfo.MessageId);
            Assert.Equal(receivedMessageEntity.EbmsRefToMessageId, result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        private static async Task<MessagingContext> ExerciseTransform(ReceivedMessage receival)
        {
            var sut = new NotifyMessageTransformer();

            return await sut.TransformAsync(receival);
        }

        private static string Canonicalize(XmlNode input)
        {
            Assert.NotNull(input);

            var doc = new XmlDocument();
            doc.LoadXml(input.OuterXml);

            var t = new XmlDsigC14NTransform();
            t.LoadInput(doc);

            var stream = (Stream)t.GetOutput(typeof(Stream));

            return new StreamReader(stream).ReadToEnd();
        }

        private static async Task<ReceivedMessageEntityMessage> CreateInvalidReceivedReceiptMessage()
        {
            var receiptContent = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.receipt));

            ISerializer serializer = SerializerProvider.Default.Get(Constants.ContentTypes.Soap);
            AS4Message receipt = await serializer.DeserializeAsync(receiptContent, Constants.ContentTypes.Soap, CancellationToken.None);

            receiptContent.Position = 0;
            InMessage receiptInMessage = new InMessage("non-existing-id");
            receiptInMessage.EbmsMessageType = MessageType.Receipt;

            var receivedMessage = new ReceivedMessageEntityMessage(receiptInMessage, receiptContent, receipt.ContentType);

            return receivedMessage;
        }

        private static async Task<ReceivedMessageEntityMessage> CreateReceivedReceiptMessage()
        {
            var receiptContent = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.receipt));

            ISerializer serializer = SerializerProvider.Default.Get(Constants.ContentTypes.Soap);
            AS4Message receiptMessage = await serializer.DeserializeAsync(receiptContent, Constants.ContentTypes.Soap, CancellationToken.None);

            receiptContent.Position = 0;
            InMessage receiptInMessage = CreateInMessageFor(receiptMessage);

            var receivedMessage = new ReceivedMessageEntityMessage(receiptInMessage, receiptContent, receiptInMessage.ContentType);

            return receivedMessage;
        }

        private static InMessage CreateInMessageFor(AS4Message receiptMessage)
        {
            var inMessage = new InMessage(receiptMessage.FirstSignalMessage.MessageId)
            {
                ContentType = Constants.ContentTypes.Soap,
                EbmsRefToMessageId = receiptMessage.FirstSignalMessage.RefToMessageId
            };

            inMessage.SetStatus(InStatus.Received);
            inMessage.Operation = Operation.ToBeNotified;
            inMessage.EbmsMessageType = MessageType.Receipt;

            return inMessage;
        }
    }
}