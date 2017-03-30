using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="Attachment"/>
    /// </summary>
    public class GivenAttachmentFacts
    {
        public class GivenValidArguments : GivenAttachmentFacts
        {
            [Fact]
            public void ThenAttachmentHasDefaults()
            {
                // Act
                var attachment = new Attachment(id: "attachment-id");
                
                // Assert
                Assert.NotNull(attachment);
                Assert.NotEmpty(attachment.Id);
                Assert.Equal("application/octet-stream", attachment.ContentType);
            }
        }
    }
}
