using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenOutMessageEntityFacts
    {
        [Fact]
        public void OutMessageHasDefaultInStatus()
        {
            Assert.Equal(default(OutStatus), new OutMessage(Guid.NewGuid().ToString()).Status.ToEnum<OutStatus>());
        }
    }
}
