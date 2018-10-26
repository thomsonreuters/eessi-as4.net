using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
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
        private Registry Registry { get; } = Registry.Instance;

        [Fact]
        public async Task ExpectedMessageBodyStores()
        {
            // Arrange
            const string acceptedString = "find this string";
            var spyStore = Mock.Of<IAS4MessageBodyStore>();
            Registry.MessageBodyStore.Accept(s => s.Equals(acceptedString), spyStore);

            // Act
            await Registry.MessageBodyStore.LoadMessageBodyAsync(acceptedString);

            // Assert
            Mock.Get(spyStore).Verify(s => s.LoadMessageBodyAsync(It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData("PAYLOAD-SERVICE", typeof(PayloadServiceAttachmentUploader))]
        [InlineData("FILE", typeof(FileAttachmentUploader))]
        [InlineData("EMAIL", typeof(EmailAttachmentUploader))]
        public void ReturnKnwonAttachmentUploader(string key, Type expectedType)
        {
            Assert.IsType(expectedType, AttachmentUploaderProvider.Instance.Get(key));
        }
    
    }
}