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
        /// <summary>
        /// Testing if the Transformer succeeds
        /// for the "Execute" Method
        /// </summary>
        public class GivenValidArgumentsToTransform : GivenSubmitMessageXmlTransformerFacts
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
                MessagingContext messagingContext = await Transform(receivedMessage);

                // Assert
                Assert.Equal(expectedPModeId, messagingContext.SubmitMessage.Collaboration.AgreementRef.PModeId);

                receivedMessage.RequestStream.Dispose();
            }
        }

        public class GivenInvalidArgumentsToTransform : GivenSubmitMessageXmlTransformerFacts
        {
            [Fact]
            public async void SubmitMessageWithoutPModeIdIsNotAccepted()
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
            public async void TransformSubmitMessageFailsDeserializing()
            {
                // Arrange
                var messageStream = new MemoryStream(Encoding.UTF8.GetBytes("<Invalid-XML"));
                var receivedMessage = new ReceivedMessage(messageStream);

                // Act / Assert
                AS4Exception actualException = 
                    await Assert.ThrowsAsync<AS4Exception>(() => Transform(receivedMessage));

                Assert.IsType<InvalidOperationException>(actualException.InnerException);
            }
        }

        protected async Task<MessagingContext> Transform(ReceivedMessage message)
        {
            return await new SubmitMessageXmlTransformer().TransformAsync(message, CancellationToken.None);
        }

        protected ReceivedMessage CreateMessageFrom(SubmitMessage submitMessage)
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