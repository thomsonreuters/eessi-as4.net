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
    /// Testing <see cref="ExceptionTransformer" />
    /// </summary>
    [Obsolete("ExceptionTransformer is obsolete")]
    public class GivenExceptionTransformerFacts
    {
        public GivenExceptionTransformerFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenExceptionTransformerFacts
        {
            [Fact]
            public async Task ThenTransformSucceedsWithValidInExceptionForErrorPropertiesAsync()
            {
                // Arrange
                InException inException = CreateDefaultInException();
                var receivedMessage = new ReceivedEntityMessage(inException);
                var transformer = new ExceptionTransformer(CreateStubSerializerProvider());

                // Act
                InternalMessage internalMessage = 
                    await transformer.TransformAsync(receivedMessage, CancellationToken.None);

                // Assert
                var error = (Error) internalMessage.AS4Message.PrimarySignalMessage;
                Assert.True(error.IsFormedByException);
                Assert.Equal(inException.Exception, error.Exception.Message);
            }

            private static InException CreateDefaultInException()
            {
                return new InException
                {
                    EbmsRefToMessageId = "ref-to-message-id",
                    Exception = "Test Exception description",
                    PMode = Properties.Resources.receivingprocessingmode
                };
            }

            private static ISerializerProvider CreateStubSerializerProvider()
            {
                var stubProvider = new Mock<ISerializerProvider>();

                stubProvider.Setup(p => p.Get(It.IsAny<string>())).Returns(new SoapEnvelopeSerializer());

                return stubProvider.Object;
            }
        }

        public class GivenInvalidArguments : GivenEncryptAS4MessageStepFacts
        {
            [Fact]
            public async Task ThenMessageIsNotSupportedWithReceivedMessageAsync()
            {
                // Arrange
                var receivedMessage = new ReceivedMessage(Stream.Null, string.Empty);
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
                await Assert.ThrowsAsync<AS4Exception>(
                    () => transformer.TransformAsync(receivedMessage, CancellationToken.None));
            }
        }
    }
}