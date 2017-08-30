using System;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitMpcResolver"/>
    /// </summary>
    public class GivenSubmitMpcResolverFacts
    {
        [Fact]
        public void ResolveSubmitMpc_IfPModeAllowsIt()
        {
            // Arrange
            var sut = SubmitMpcResolver.Default;
            const string expectedMpc = "submit mpc";
            var message = new SubmitMessage
            {
                PMode = new SendingProcessingMode {AllowOverride = true},
                MessageInfo = {Mpc = expectedMpc}
            };

            // Act
            string actualMpc = sut.Resolve(message);

            // Assert
            Assert.Equal(expectedMpc, actualMpc);
        }

        [Fact]
        public void FailsToResolve_IfMessageTriesToOverrideMpc()
        {
            // Arrange
            var sut = SubmitMpcResolver.Default;
            var message = new SubmitMessage
            {
                PMode = new SendingProcessingMode {AllowOverride = false, MessagePackaging = {Mpc = "not empty mpc"}},
                MessageInfo = {Mpc = "also not empty mpc"}
            };

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => sut.Resolve(message));
        }
    }
}
