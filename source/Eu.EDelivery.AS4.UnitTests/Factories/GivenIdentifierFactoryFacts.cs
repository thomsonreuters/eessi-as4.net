using Eu.EDelivery.AS4.Factories;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Factories
{
    /// <summary>
    /// Testing <seealso cref="IdentifierFactory" />
    /// </summary>
    public class GivenIdentifierFactoryFacts
    {
        [Fact]
        public void ThenGenerateIdGuidAndIpAddressCorrectIdGenerated()
        {
            // Act
            string id = IdentifierFactory.Instance.Create("{GUID}@{IPADDRESS}");

            // Assert
            string[] splittedId = id.Split('@');
            Assert.Matches(@"\w+-\w+-\w+-\w+-\w+", splittedId[0]);
            Assert.Matches(@"\d+\.\d+\.\d+\.\d+", splittedId[1]);
        }

        [Fact]
        public void ThenGenerateIdMachineNameCorrectIdGenerated()
        {
            // Act
            string id = IdentifierFactory.Instance.Create("{MACHINENAME}");

            // Assert
            Assert.NotEqual("{MACHINENAME}", id);
            Assert.Matches(@"\w+", id);
        }
    }
}