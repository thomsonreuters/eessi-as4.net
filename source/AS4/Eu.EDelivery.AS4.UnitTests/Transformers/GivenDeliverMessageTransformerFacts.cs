using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="DeliverMessageTransformer"/>
    /// </summary>
    public class GivenDeliverMessageTransformerFacts
    {
        [Fact]
        public async Task SucceedsToTransform_IfStreamIsSinglePayloadMessage()
        {
            // Arrange
            var sut = new DeliverMessageTransformer();
            ReceivedMessageEntityMessage message = SinglePayloadMessage();

            // Act
            InternalMessage actualMessage = await sut.TransformAsync(message, CancellationToken.None);

            // Assert
            string expectedMessageId = message.MessageEntity.EbmsMessageId;
            string actualMessageId = actualMessage.DeliverMessage.MessageInfo.MessageId;

            Assert.Equal(expectedMessageId, actualMessageId);
        }

        private static ReceivedMessageEntityMessage SinglePayloadMessage()
        {
            const string contentType =
                "multipart/related; boundary=\"=-PHQq1fuE9QxpIWax7CKj5w==\"; type=\"application/soap+xml\"; charset=\"utf-8\"";

            var messageEntity = new InMessage
            {
                ContentType = contentType,
                EbmsMessageId = "fd85bf2e-2366-408b-b187-010ad63d0070@10.124.29.152",
                EbmsMessageType = MessageType.UserMessage
            };

            return new ReceivedMessageEntityMessage(messageEntity)
            {
                ContentType = contentType,
                RequestStream = new MemoryStream(as4_single_payload)
            };
        }
    }
}