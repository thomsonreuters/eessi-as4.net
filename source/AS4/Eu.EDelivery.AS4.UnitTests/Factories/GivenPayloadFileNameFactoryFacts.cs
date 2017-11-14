using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Factories {
    public class GivenPayloadFileNameFactoryFacts
    {
        [Fact]
        public void ThenGenerateFileNameWithMessageIdPattern()
        {
            string messageId = "someMessageId";

            var m = new UserMessage(messageId);

            var payloadFileName = PayloadFileNameFactory.CreateFileName("{messageId}", null, m);

            Assert.Equal(messageId, payloadFileName);
        }

        [Fact]
        public void ThenGenerateFileNameWithAttachmentIdPattern()
        {
            string payloadId = "earth.jpg";

            var attachment = new Attachment(payloadId);

            var payloadFileName = PayloadFileNameFactory.CreateFileName("{ATTACHMENTID}", attachment, null);

            Assert.Equal(payloadId, payloadFileName);
        }

        [Fact]
        public void DefaultPatternIsAttachmentIdPattern()
        {
            string payloadId = "earth.jpg";
            var attachment = new Attachment(payloadId);
            var userMessage = new UserMessage("messageId");

            var payloadFileName = PayloadFileNameFactory.CreateFileName(null, attachment, userMessage);

            Assert.Equal(payloadId, payloadFileName);
        }

        [Fact]
        public void ThenGenerateFileNameWithCombinedPattern()
        {            
            var attachment = new Attachment("earth.jpg");
            var userMessage = new UserMessage("messageId");

            var payloadFileName = PayloadFileNameFactory.CreateFileName("{MessageId}_{AttachmentId}", attachment, userMessage);

            Assert.Equal($"{userMessage.MessageId}_{attachment.Id}", payloadFileName);
        }
    }
}