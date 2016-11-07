using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Utilities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Utilities
{
    /// <summary>
    /// Testing <seealso cref="AS4.Utilities.IdGenerator" />
    /// </summary>
    public class IdGeneratorTest
    {
        [Fact]
        public void ThenGenerateIdGuidAndIpAddressCorrectIdGenerated()
        {
            // Act
            string id = IdGenerator.Generate("{GUID}@{IPADDRESS}");
            // Assert
            string[] splittedId = id.Split('@');
            Assert.Matches(@"\w+-\w+-\w+-\w+-\w+", splittedId[0]);
            Assert.Matches(@"\d+\.\d+\.\d+\.\d+", splittedId[1]);
        }

        [Fact]
        public void ThenGenerateIdMachineNameCorrectIdGenerated()
        {
            // Act
            string id = IdGenerator.Generate("{MACHINENAME}");
            // Assert
            Assert.NotEqual("{MACHINENAME}", id);
            Assert.Matches(@"\w+", id);
        }
    }
}