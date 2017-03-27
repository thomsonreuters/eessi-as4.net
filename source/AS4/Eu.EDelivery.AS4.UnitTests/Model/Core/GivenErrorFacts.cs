using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.Core
{
    /// <summary>
    /// Testing <see cref="Error"/>
    /// </summary>
    public class GivenErrorFacts
    {
        public class FromPullRequest
        {
            [Fact]
            public void IsTrueWhenSeverityAndShortDescriptionIsExpected()
            {
                // Arrange
                Error error = new PullRequestError();

                // Act
                bool fromPullRequest = error.FromPullRequest;

                // Assert
                Assert.True(fromPullRequest);
            }
        }
    }
}