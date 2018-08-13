using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.PMode
{
    public class GivenSendingPModeFacts
    {
        [Fact]
        public async Task DeserializedPModeWithDefaultValuesContainsDefaultValues()
        {
            var sendingPMode = new SendingProcessingMode();

            var serialized = await AS4XmlSerializer.ToStringAsync(sendingPMode);

            var deserialized = await AS4XmlSerializer.FromStringAsync<SendingProcessingMode>(serialized);

            Assert.Equal(sendingPMode.Security.Encryption.Algorithm, deserialized.Security.Encryption.Algorithm);
            Assert.Equal(sendingPMode.Security.Encryption.AlgorithmKeySize, deserialized.Security.Encryption.AlgorithmKeySize);
        }
    }
}
