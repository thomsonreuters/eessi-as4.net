using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Serialization;
using AutoMapper;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing the <see cref="AS4MessageTransformer" />
    /// </summary>
    public class GivenAS4MessageTransformerFacts
    {
        public GivenAS4MessageTransformerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        /// <summary>
        /// Testing if the Transformer succeeds
        /// for the "Transform" Method
        /// </summary>
        public class GivenValidReceivedMessageToTransformer : GivenAS4MessageTransformerFacts
        {
            [Fact]
            public async Task ThenTransfromSuceedsWithSoapdAS4StreamAsync()
            {
                // Arrange
                MemoryStream memoryStream = CreateAS4MessageWithAttachment().ToStream();
                var receivedMessage = new ReceivedMessage(memoryStream, Constants.ContentTypes.Mime);

                // Act
                InternalMessage internalMessage = await Transform(receivedMessage);

                // Assert
                Assert.NotNull(internalMessage);
                Assert.NotNull(internalMessage.AS4Message);
            }
            
        }

        /// <summary>
        /// Testing if the Transformer fails
        /// for the "Transform" Method
        /// </summary>
        public class GivenInvalidArgumentsToTransfrormer : GivenAS4MessageTransformerFacts
        {
            [Fact]
            public async Task ThenTransformFailsWithInvalidUserMessageWithSoapAS4StreamAsync()
            {
                // Arrange
                AS4Message as4Message = CreateAS4MessageWithoutAttachments();
                as4Message.UserMessages = new[] { new UserMessage("message-id") };
                MemoryStream memoryStream = as4Message.ToStream();

                var receivedMessage = new ReceivedMessage(memoryStream, Constants.ContentTypes.Soap);

                // Act / Assert
                await Assert.ThrowsAsync<AutoMapperMappingException>(() => Transform(receivedMessage));
            }

            [Fact]
            public async Task ThenTransformFails_IfContentIsNotSupported()
            {
                // Arrange
                var saboteurMessage = new ReceivedMessage(Stream.Null, "not-supported-content-type");

                await VerifyIfTheTranformReturnsErrorMessage(saboteurMessage);
            }

            [Fact]
            public async Task ThenTransformFails_IfRequestStreamIsNull()
            {
                // Arrange
                var saboteurMessage = new ReceivedMessage(requestStream: null);

                await VerifyIfTheTranformReturnsErrorMessage(saboteurMessage);
            }

            private async Task VerifyIfTheTranformReturnsErrorMessage(ReceivedMessage saboteurMessage)
            {
                // Act
                InternalMessage actualMessage = await Transform(saboteurMessage);

                // Assert
                Assert.IsType<Error>(actualMessage.AS4Message.PrimarySignalMessage);
            }
        }

        private static AS4Message CreateAS4MessageWithAttachment()
        {
            AS4Message as4Message = CreateAS4MessageWithoutAttachments();

            as4Message.ContentType = Constants.ContentTypes.Mime;
            as4Message.AddAttachment(CreateAttachment());

            return as4Message;
        }

        private static AS4Message CreateAS4MessageWithoutAttachments()
        {
            var userMessage = new UserMessage("message-id")
            {
                Receiver = new Party("Receiver", new PartyId()),
                Sender = new Party("Sender", new PartyId())
            };

            AS4Message as4Message =
                new AS4MessageBuilder().WithUserMessage(userMessage)
                                       .Build();

            as4Message.ContentType = Constants.ContentTypes.Soap;

            return as4Message;
        }

        private static Attachment CreateAttachment()
        {
            var attachment = new Attachment("attachment-id") { Content = new MemoryStream(), ContentType = "application/xml" };

            var xmlSerializer = new XmlSerializer(typeof(string));
            xmlSerializer.Serialize(attachment.Content, "<?xml version=\"1.0\"?><Root></Root>");
            attachment.Content.Position = 0;

            return attachment;
        }

        protected async Task<InternalMessage> Transform(ReceivedMessage message)
        {
            var transformer = new AS4MessageTransformer(Registry.Instance.SerializerProvider);
            return await transformer.TransformAsync(message, CancellationToken.None);
        }
    }
}