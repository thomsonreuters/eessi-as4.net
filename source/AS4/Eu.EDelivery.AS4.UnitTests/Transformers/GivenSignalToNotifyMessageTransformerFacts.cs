using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    public class GivenSignalToNotifyMessageTransformerFacts
    {
        [Fact]
        public async void ThenSignalMessageIsTransformedToNotifyEnvelopeWithCorrectMessageInfo()
        {
            var receivedSignal = await CreateReceivedReceiptMessage();
            var receivedMessageEntity = receivedSignal.MessageEntity;

            var sut = new SignalMessageToNotifyMessageTransformer();

            var result = await sut.TransformAsync(receivedSignal, CancellationToken.None);

            Assert.NotNull(result.NotifyMessage);
            Assert.Equal(receivedMessageEntity.EbmsMessageId, result.NotifyMessage.MessageInfo.MessageId);
            Assert.Equal(receivedMessageEntity.EbmsRefToMessageId, result.NotifyMessage.MessageInfo.RefToMessageId);
        }

        [Fact]
        public async void ThenSignalMessageIsTransformedToNotifyEnvelopeWithCorrectContents()
        {
            var receivedSignal = await CreateReceivedReceiptMessage();

            var sut = new SignalMessageToNotifyMessageTransformer();

            var result = await sut.TransformAsync(receivedSignal, CancellationToken.None);

            Assert.NotNull(result.NotifyMessage);

            var notifyMessage =
                    AS4XmlSerializer.FromString<NotifyMessage>(Encoding.UTF8.GetString(result.NotifyMessage.NotifyMessage));

            Assert.NotNull(notifyMessage);

            // Assert: check if the original Receipt is a part of the NotifyMessage.
            var document = new XmlDocument { PreserveWhitespace = true };
            document.LoadXml(Encoding.UTF8.GetString(((MemoryStream)receivedSignal.RequestStream).ToArray()));

            Assert.Equal(
                Canonicalize(document.SelectSingleNode("//*[local-name()='SignalMessage']")),
                Canonicalize(notifyMessage.StatusInfo.Any.First()));
        }

        [Fact]
        public async void ThenNotifyMessageHasCorrectStatusCode()
        {
            var receivedSignal = await CreateReceivedReceiptMessage();

            var sut = new SignalMessageToNotifyMessageTransformer();

            var result = await sut.TransformAsync(receivedSignal, CancellationToken.None);

            // Assert
            NotifyMessageEnvelope notifyMessage = result.NotifyMessage;
            Assert.NotNull(notifyMessage);
            Assert.Equal(Status.Delivered, notifyMessage.StatusCode);
        }

        private static async Task<ReceivedMessageEntityMessage> CreateReceivedReceiptMessage()
        {
            var receiptContent = new MemoryStream(Encoding.UTF8.GetBytes(Properties.Resources.receipt));

            var receiptMessage = await SerializerProvider.Default.Get(Constants.ContentTypes.Soap)
                                                         .DeserializeAsync(receiptContent, Constants.ContentTypes.Soap, CancellationToken.None);

            receiptContent.Position = 0;

            var receiptInMessage = new InMessage
            {
                Status = InStatus.Received,
                Operation = Operation.ToBeNotified,
                EbmsMessageType = MessageType.Receipt,
                ContentType = Constants.ContentTypes.Soap,
                EbmsMessageId = receiptMessage.PrimarySignalMessage.MessageId,
                EbmsRefToMessageId = receiptMessage.PrimarySignalMessage.RefToMessageId
            };

            var receivedMessage = new ReceivedMessageEntityMessage(receiptInMessage)
            {
                ContentType = receiptInMessage.ContentType,
                RequestStream = receiptContent
            };

            return receivedMessage;
        }

        /// <summary>
        /// Canonicalize the given Xml element
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        private static string Canonicalize(XmlNode input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(input.OuterXml);

            XmlDsigC14NTransform t = new XmlDsigC14NTransform();
            t.LoadInput(doc);

            var stream = (Stream)t.GetOutput(typeof(Stream));

            return new StreamReader(stream).ReadToEnd();
        }
    }
}
