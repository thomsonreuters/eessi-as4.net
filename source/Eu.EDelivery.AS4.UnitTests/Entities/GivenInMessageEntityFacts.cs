using System;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Entities
{
    public class GivenInMessageEntityFacts
    {
        [Fact]
        public void InMessageHasDefaultInStatus()
        {
            Assert.Equal(default(InStatus), new InMessage(Guid.NewGuid().ToString()).Status.ToEnum<InStatus>());
        }
    }
}
