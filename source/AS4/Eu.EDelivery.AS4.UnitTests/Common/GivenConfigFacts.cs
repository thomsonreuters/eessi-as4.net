using Eu.EDelivery.AS4.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Common
{
    public class GivenConfigFacts
    {
        [Fact]
        public void DefaultInOutStore()
        {
            // Assert
            Assert.False(Config.Instance.IsInitialized);
            Assert.NotEmpty(Config.Instance.InMessageStoreLocation);
            Assert.NotEmpty(Config.Instance.OutMessageStoreLocation);
        }
    }
}
