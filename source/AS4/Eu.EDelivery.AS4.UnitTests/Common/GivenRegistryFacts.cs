using System;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Eu.EDelivery.AS4.Strategies.Uploader;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    /// <summary>
    /// Testing <see cref="Registry"/>
    /// </summary>
    public class GivenRegistryFacts
    {
        private readonly IRegistry _registry;

        public GivenRegistryFacts()
        {
            _registry = new Registry();
        }

        /// <summary>
        /// Testing the Registry with valid arguments
        /// </summary>
        public class GivenValidArgumentsRegistry : GivenRegistryFacts
        {
            [Fact]
            public void ThenGetFilePayloadStrategyProvider()
            {
                // Act
                IPayloadRetrieverProvider provider = _registry.PayloadRetrieverProvider;

                // Assert
                IPayloadRetriever fileRetriever = provider.Get(new Payload("file:///"));
                Assert.NotNull(fileRetriever);
            }

            [Fact]
            public void ThenGetWebPayloadStrategyProvider()
            {
                // Act
                IPayloadRetrieverProvider provider = _registry.PayloadRetrieverProvider;

                // Assert
                IPayloadRetriever webRetriever = provider.Get(new Payload("http"));
                Assert.NotNull(webRetriever);
            }

            [Theory]
            [InlineData("PAYLOAD-SERVICE", typeof(PayloadServiceAttachmentUploader))]
            [InlineData("FILE", typeof(FileAttachmentUploader))]
            [InlineData("EMAIL", typeof(EmailAttachmentUploader))]
            public void ReturnKnwonAttachmentUploader(string key, Type expectedType)
            {
                Assert.IsType(expectedType, _registry.AttachmentUploader.Get(key));
            }
        }

        /// <summary>
        /// Testing the Registry with invalid arguments
        /// </summary>
        public class GivenInvalidArgumentsRegistry : GivenRegistryFacts
        {
            [Fact]
            public void ThenProvidersDoesNotHasPayloadStrategy()
            {
                // Act
                IPayloadRetrieverProvider provider = _registry.PayloadRetrieverProvider;

                // Assert
                Assert.Throws<AS4Exception>(() => provider.Get(new Payload("not-supported-location")));
            }
        }
    }
}