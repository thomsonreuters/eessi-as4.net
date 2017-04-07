using System.IO;
using Eu.EDelivery.AS4.PayloadService.Models;
using Xunit;

namespace Eu.EDelivery.AS4.PayloadService.UnitTests.Models
{
    public class GivenPayloadFacts
    {
        [Fact]
        public void ThenPayloadNullObjectIsEqualToSelfCreatedObject()
        {
            Assert.Equal(Payload.Null, new Payload(Stream.Null, new PayloadMeta(string.Empty)));
        }
    }
}
