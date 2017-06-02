using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Transformers;
using Xunit;

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
            InternalMessage internalMessage = await Transform(receivedmessage);

            // Assert
            Assert.Null(internalMessage.SubmitMessage.PMode);

            receivedmessage.RequestStream.Dispose();
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
            InternalMessage internalMessage = await Transform(receivedMessage);

            // Assert
            Assert.Equal(expectedPModeId, internalMessage.SubmitMessage.Collaboration.AgreementRef.PModeId);

            receivedMessage.RequestStream.Dispose();
        }

        [Fact]
        public async Task SubmitMessageWithoutPModeIdIsNotAccepted()
        {
            // Arrange
            var submitMessage = new SubmitMessage
            {
                Collaboration = new CollaborationInfo {AgreementRef = new Agreement {PModeId = string.Empty}}
            };
            ReceivedMessage receivedMessage = CreateMessageFrom(submitMessage);

            // Act / Assert
            await Assert.ThrowsAsync<AS4Exception>(() => Transform(receivedMessage));

            receivedMessage.RequestStream.Dispose();
        }

        [Fact]
        public async Task TransformSubmitMessageFailsDeserializing()
        {
            // Arrange
            var messageStream = new MemoryStream(Encoding.UTF8.GetBytes("<Invalid-XML"));
            var receivedMessage = new ReceivedMessage(messageStream);

            // Act / Assert
            AS4Exception actualException = await Assert.ThrowsAsync<AS4Exception>(() => Transform(receivedMessage));

            Assert.IsType<InvalidOperationException>(actualException.InnerException);
        }

        [Fact]
        public async Task TransformFails_IfInvalidSubmitMessage()
        {
            // Arrange
            var invalidMessage = new SubmitMessage {Collaboration = null};
            ReceivedMessage receivedmessage = CreateMessageFrom(invalidMessage);

            // Act / Assert
            await Assert.ThrowsAnyAsync<Exception>(() => Transform(receivedmessage));
        }
        
        private static async Task<InternalMessage> Transform(ReceivedMessage message)
        {
            return await new SubmitMessageXmlTransformer().TransformAsync(message, CancellationToken.None);
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