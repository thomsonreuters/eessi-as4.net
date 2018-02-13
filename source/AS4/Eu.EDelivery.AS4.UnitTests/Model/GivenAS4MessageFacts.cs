using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <seealso cref="AS4Message" />
    /// </summary>
    public class GivenAS4MessageFacts
    {
        public GivenAS4MessageFacts()
        {
            IdentifierFactory.Instance.SetContext(StubConfig.Default);
        }

        public class Empty
        {
            [Fact]
            public void EmptyInstance_IsNotTheSameWithDifferentId()
            {
                // Arrange
                AS4Message expected =
                    AS4Message.Create(new FilledUserMessage(), new SendingProcessingMode());

                // Act
                AS4Message actual = AS4Message.Empty;

                // Assert
                Assert.NotEqual(expected, actual);
            }

            [Fact]
            public void EmptyIsntanceReturnsExpected()
            {
                Assert.Equal(AS4Message.Empty, AS4Message.Empty);
            }
        }

        public class AddAttachments
        {
            [Fact]
            public async Task ThenAddAttachmentSucceeds()
            {
                // Arrange
                var submitMessage = new SubmitMessage { Payloads = new[] { new Payload(string.Empty) } };
                AS4Message sut = AS4Message.Empty;

                // Act
                await sut.AddAttachments(submitMessage.Payloads, async payload => await Task.FromResult(Stream.Null));

                // Assert
                Assert.NotNull(sut.Attachments);
                Assert.Equal(Stream.Null, sut.Attachments.First().Content);
            }

            [Fact]
            public async Task ThenNoAttachmentsAreAddedWithZeroPayloads()
            {
                // Arrange
                AS4Message sut = AS4Message.Empty;

                // Act
                await sut.AddAttachments(new Payload[0], async payload => await Task.FromResult(Stream.Null));

                // Assert
                Assert.False(sut.HasAttachments);
            }
        }

        public class IsPulling
        {
            [Fact]
            public void IsTrueWhenSignalMessageIsPullRequest()
            {
                // Arrange
                AS4Message as4Message = AS4Message.Create(new PullRequest(null));

                // Act
                bool isPulling = as4Message.IsPullRequest;

                // Assert
                Assert.True(isPulling);
            }
        }

       
    }
}