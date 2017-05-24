using System;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Testing <see cref="AS4.Common.Registry" />
    /// </summary>
    public class GivenRegistryFacts
    {
        private IRegistry Registry { get; } = new Registry();

        [Fact]
        public void ExpectedMessageBodyStores()
        {
            // Arrange
            const string acceptedString = "find this string";
            var spyStore = Mock.Of<IAS4MessageBodyStore>();
            Registry.MessageBodyStore.Accept(s => s.Equals(acceptedString), () => spyStore);

            // Act
            Registry.MessageBodyStore.LoadMessageBody(acceptedString);

            // Assert
            Mock.Get(spyStore).Verify(s => s.LoadMessageBody(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("PAYLOAD-SERVICE", typeof(PayloadServiceAttachmentUploader))]
        [InlineData("FILE", typeof(FileAttachmentUploader))]
        [InlineData("EMAIL", typeof(EmailAttachmentUploader))]
        public void ReturnKnwonAttachmentUploader(string key, Type expectedType)
        {
            Assert.IsType(expectedType, Registry.AttachmentUploader.Get(key));
        }

        [Fact]
        public void ThenGetFilePayloadStrategyProvider()
        {
            // Act
            IPayloadRetrieverProvider provider = Registry.PayloadRetrieverProvider;

            // Assert
            IPayloadRetriever fileRetriever = provider.Get(new Payload("file:///"));
            Assert.NotNull(fileRetriever);
        }

        [Fact]
        public void ThenGetWebPayloadStrategyProvider()
        {
            // Act
            IPayloadRetrieverProvider provider = Registry.PayloadRetrieverProvider;

            // Assert
            IPayloadRetriever webRetriever = provider.Get(new Payload("http"));
            Assert.NotNull(webRetriever);
        }

        [Fact]
        public void ThenProvidersDoesNotHasPayloadStrategy()
        {
            // Act
            IPayloadRetrieverProvider provider = Registry.PayloadRetrieverProvider;

            // Assert
            Assert.Throws<AS4Exception>(() => provider.Get(new Payload("not-supported-location")));
        }
    }
}