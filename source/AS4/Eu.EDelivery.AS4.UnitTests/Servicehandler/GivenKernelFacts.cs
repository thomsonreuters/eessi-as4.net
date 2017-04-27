using Eu.EDelivery.AS4.ServiceHandler;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Servicehandler
{
    /// <summary>
    /// Testing <see cref="Kernel"/>
    /// </summary>
    public class GivenKernelFacts
    {
        [Fact]
        public void KernelHasNothingToClose()
        {
            // Arrange
            var sut = new Kernel(null);

            // Act 
            sut.Dispose();
        }
    }
}
