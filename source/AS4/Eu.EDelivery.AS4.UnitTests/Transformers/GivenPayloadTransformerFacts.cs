using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="PayloadTransformer"/>
    /// </summary>
    public class GivenPayloadTransformerFacts
    {
        public GivenPayloadTransformerFacts()
        {
            IdGenerator.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenPayloadTransformerFacts
        {
            [Fact]
            public async Task ThenTransformSucceedsWithValidStreamAsync()
            {
                // Arrange
                var stream = new MemoryStream(Encoding.UTF8.GetBytes("Transform me!"));
                const string contentType = "text/plain";
                var receivedMessage = new ReceivedMessage(stream, contentType);
                // Act
                InternalMessage internalMessage = await new PayloadTransformer()
                    .TransformAsync(receivedMessage, CancellationToken.None);
                // Assert
                Assert.NotNull(internalMessage);
                Attachment firstAttachment = internalMessage.AS4Message.Attachments.First();
                Assert.Equal(contentType, firstAttachment.ContentType);
                Assert.Equal(stream, firstAttachment.Content);
            }
        }
    }
}
