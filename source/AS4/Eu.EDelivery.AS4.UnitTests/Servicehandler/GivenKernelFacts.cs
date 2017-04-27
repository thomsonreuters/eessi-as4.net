using System;
using Eu.EDelivery.AS4.ServiceHandler;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            try
            {
                // Arrange
                var sut = new Kernel(agents: null);

                // Act 
                sut.Dispose();
            }
            catch (Exception exception)
            {
                // Assert
                throw new AssertFailedException("'Kernel has nothing to close test' failed", exception);
            }
        }
    }
}
