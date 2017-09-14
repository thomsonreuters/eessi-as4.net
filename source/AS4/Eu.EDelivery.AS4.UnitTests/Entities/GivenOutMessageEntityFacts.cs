using System;
using Eu.EDelivery.AS4.Entities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenOutMessageEntityFacts
    {
        [Fact]
        public void OutMessageHasDefaultInStatus()
        {
            Assert.Equal(default(OutStatus), OutStatusUtils.Parse(new OutMessage(Guid.NewGuid().ToString()).Status));
        }
    }
}
