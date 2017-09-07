using System;
using Eu.EDelivery.AS4.Entities;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenInMessageEntityFacts
    {
        [Fact]
        public void InMessageHasDefaultInStatus()
        {
            Assert.Equal(default(InStatus), InStatusUtils.Parse(new InMessage(Guid.NewGuid().ToString()).Status));
        }
    }
}
