using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitConversationIdResolver"/>
    /// </summary>
    public class GivenSubmitConversationIdResolverFacts
    {
        [Fact]
        public void ResolveSubmitConversationId_IfNotEmpty()
        {
            // Arrange
            var sut = SubmitConversationIdResolver.Default;
            const string expectedId = "submit id";
            var message = new SubmitMessage {Collaboration = {ConversationId = expectedId}};

            // Act
            string actualId = sut.Resolve(message);

            // Assert
            Assert.Equal(expectedId, actualId);
        }
    }
}
