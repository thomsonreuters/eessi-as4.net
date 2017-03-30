using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Steps.Send;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="ExceptionTransformer"/>
    /// </summary>
    public class GivenExceptionTransformerFacts
    {
        private readonly Mock<ISerializerProvider> _mockekdProvider;

        public GivenExceptionTransformerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            this._mockekdProvider = new Mock<ISerializerProvider>();
            this._mockekdProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(new SoapEnvelopeSerializer());
        }

        public class GivenValidArguments : GivenExceptionTransformerFacts
        {
            [Fact]
            public async Task ThenTransformSucceedsWithValidInExceptionAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                var receivedMessage = new ReceivedEntityMessage(inException);
                var transformer = new ExceptionTransformer(base._mockekdProvider.Object);
                // Act
                InternalMessage internalMessage = await transformer
                    .TransformAsync(receivedMessage, CancellationToken.None);
                // Assert
                Assert.NotNull(internalMessage.AS4Message.PrimarySignalMessage);
                Assert.NotNull(internalMessage.AS4Message.SendingPMode);
                Assert.NotNull(internalMessage.AS4Message.EnvelopeDocument);
            }

            [Fact]
            public async Task ThenTransformSucceedsWithValidInExceptionForErrorPropertiesAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                var receivedMessage = new ReceivedEntityMessage(inException);
                var transformer = new ExceptionTransformer(base._mockekdProvider.Object);
                // Act
                InternalMessage internalMessage = await transformer
                    .TransformAsync(receivedMessage, CancellationToken.None);
                // Assert
                var error = internalMessage.AS4Message.PrimarySignalMessage as Error;
                Assert.NotNull(error);
                Assert.True(error.IsFormedByException);
                Assert.Equal(inException.Exception, error.Exception.Message);
            }

            private InException CreateDefaultInException()
            {
                return new InException
                {
                    EbmsRefToMessageId = "ref-to-message-id",
                    Exception = "Test Exception description",
                    PMode = AS4XmlSerializer.ToString(new ReceivingProcessingMode())
                };
            }
        }

        public class GivenInvalidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenMessageIsNotSupportedWithReceivedMessageAsync()
            {
                // Arrange
                var receivedMessage = new ReceivedMessage(Stream.Null, String.Empty);
                var transformer = new ExceptionTransformer();
                // Act / Assert
                await Assert.ThrowsAsync<AS4Exception>(
                    () => transformer.TransformAsync(receivedMessage, CancellationToken.None));
            }

            [Fact]
            public async Task ThenMessageIsNotSupportedWithReceivedMessageEntityAsync()
            {
                // Arrange
                var messageEntity = new InMessage();
                var receivedMessage = new ReceivedEntityMessage(messageEntity);
                var transformer = new ExceptionTransformer();
                // Act / Assert
                await
                    Assert.ThrowsAsync<AS4Exception>(
                        () => transformer.TransformAsync(receivedMessage, CancellationToken.None));
            }
        }
    }
}