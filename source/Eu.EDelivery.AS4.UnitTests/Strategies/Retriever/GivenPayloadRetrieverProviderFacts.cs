using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    /// <summary>
    /// Testing <see cref="PayloadRetrieverProvider"/>
    /// </summary>
    public class GivenPayloadRetrieverProviderFacts
    {
        [Fact]
        public void GetsPayloadRetriever_IfPayloadRetrieverIsAccepted()
        {
            // Arrange
            var provider = new PayloadRetrieverProvider();
            var dummyPayload = new Payload();
            IPayloadRetriever expectedRetriever = new Mock<IPayloadRetriever>().Object;

            provider.Accept(payload => payload.Equals(dummyPayload), expectedRetriever);

            // Act
            IPayloadRetriever actualRetriever = provider.Get(dummyPayload);

            // Assert
            Assert.Equal(expectedRetriever, actualRetriever);
        }
    }
}
