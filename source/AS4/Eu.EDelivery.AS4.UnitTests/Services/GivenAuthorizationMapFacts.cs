using Eu.EDelivery.AS4.Services;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Services
{
    public class GivenAuthorizationMapFacts
    {
        [Fact]
        public void PullRequestIsAuthorized_IfMpcMatchesCertificate()
        {
            // Arrange
            var sut = new PullAuthorizationMap();

            // Act
            bool isAuthorized = sut.IsPullRequestAuthorized(request: null, certificate: null);
            
            // Assert
            Assert.True(isAuthorized);
        }
    }
}
