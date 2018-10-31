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
            const string expectedMpc = "submit mpc";
            var message = new SubmitMessage
            {
                PMode = new SendingProcessingMode {AllowOverride = true},
                MessageInfo = {Mpc = expectedMpc}
            };

            // Act
            string actualMpc = SubmitMpcResolver.Resolve(message).UnsafeGet;

            // Assert
            Assert.Equal(expectedMpc, actualMpc);
        }

        [Fact]
        public void FailsToResolve_IfMessageTriesToOverrideMpc()
        {
            // Arrange
            var message = new SubmitMessage
            {
                PMode = new SendingProcessingMode {AllowOverride = false, MessagePackaging = {Mpc = "not empty mpc"}},
                MessageInfo = {Mpc = "also not empty mpc"}
            };

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => SubmitMpcResolver.Resolve(message));
        }
    }
}
