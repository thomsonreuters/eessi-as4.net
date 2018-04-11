using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Transformers;
using Xunit;
using static Eu.EDelivery.AS4.UnitTests.Properties.Resources;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing the <see cref="SubmitMessageXmlTransformer" />
    /// </summary>
    public class GivenSubmitMessageXmlTransformerFacts
    {
        [Fact]
        public async Task ThenPModeIsNotPartOfTheSerializationAsync()
        {
            // Arrange
            var submitMessage = new SubmitMessage
            {
                Collaboration = {AgreementRef = {PModeId = "this-pmode-id"}},
                PMode = new SendingProcessingMode {Id = "other-pmode-id"}
            };

            ReceivedMessage receivedmessage = CreateMessageFrom(submitMessage);

                // Act
                MessagingContext messagingContext = await Transform(receivedmessage);

                // Assert
                Assert.Null(messagingContext.SubmitMessage.PMode);

            receivedmessage.UnderlyingStream.Dispose();
        }

        [Fact]
        public async Task ThenTransformSucceedsWithPModeIdAsync()
        {
            // Arrange
            const string expectedPModeId = "01-pmode";
            var submitMessage = new SubmitMessage
            {
                Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = expectedPModeId}}
            };

            ReceivedMessage receivedMessage = CreateMessageFrom(submitMessage);

                // Act
                MessagingContext messagingContext = await Transform(receivedMessage);

                // Assert
                Assert.Equal(expectedPModeId, messagingContext.SubmitMessage.Collaboration.AgreementRef.PModeId);

            receivedMessage.UnderlyingStream.Dispose();
        }

        [Fact]
        public void TransformInvalidXmlInputFailsDeserializing()
        {
            Assert.All(new[]
            {
                "<Invalid-XML>",
                submitmessage_invalid_messageproperties,
                submitmessage_missing_payload_location,
                submitmessage_missing_schema_location,
                submitmessage_missing_payload_property_name,
                submitmessage_missing_collaboration,
                submitmessage_missing_collaboration_agreement,
                submitmessage_missing_collaboration_agreement_pmodeid
            }, x =>
            {
                // Arrange
                var messageStream = new MemoryStream(Encoding.UTF8.GetBytes(x));
                var receivedMessage = new ReceivedMessage(messageStream);

                // Act / Assert
                Assert.ThrowsAny<Exception>(() => Transform(receivedMessage).Result);
            });
        }

        protected async Task<MessagingContext> Transform(ReceivedMessage message)
        {
            return await new SubmitMessageXmlTransformer().TransformAsync(message);
        }

        private static ReceivedMessage CreateMessageFrom(SubmitMessage submitMessage)
        {
            return new ReceivedMessage(WriteSubmitMessageToStream(submitMessage));
        }

        private static MemoryStream WriteSubmitMessageToStream(SubmitMessage submitMessage)
        {
            var memoryStream = new MemoryStream();
            var serializer = new XmlSerializer(typeof(SubmitMessage));

            serializer.Serialize(memoryStream, submitMessage);
            memoryStream.Position = 0;

            return memoryStream;
        }
    }
}