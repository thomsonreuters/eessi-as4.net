using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing the <see cref="PartyId"/> 
    /// </summary>
    public class GivenPartyIdFacts
    {
        public class GivenValidArguments : GivenPartyIdFacts
        {
            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreEqual(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                PartyId partyIdB = partyIdA;

                // Act
                bool isEqual = partyIdA.Equals(partyIdB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreEqualForIdAndType(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                var partyIdB = new PartyId() { Id = sharedId, Type = sharedType };

                // Act
                bool isEqual = partyIdA.Equals(partyIdB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreEqualForObject(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                var partyIdB = new PartyId() { Id = sharedId, Type = sharedType };

                // Act
                bool isEqual = partyIdA.Equals((object)partyIdB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreNotEqualForId(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                var partyIdB = new PartyId() { Id = "not-equal", Type = sharedType };

                // Act
                bool isEqual = partyIdA.Equals(partyIdB);

                // Assert
                Assert.False(isEqual);
            }

            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreNotEqualForType(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                var partyIdB = new PartyId() { Id = sharedId, Type = "not-equal" };

                // Act
                bool isEqual = partyIdA.Equals(partyIdB);

                // Assert
                Assert.False(isEqual);
            }
        }

        public class GivenInvalidArguments : GivenPartyIdFacts
        {
            [Theory, InlineData("shared-Id", "shared-Type")]
            public void ThenTwoPartyIdsAreNotEqualForNull(string sharedId, string sharedType)
            {
                // Arrange
                var partyIdA = new PartyId() { Id = sharedId, Type = sharedType };
                PartyId partyIdB = null;

                // Act
                bool isEqual = partyIdA.Equals(partyIdB);

                // Assert
                Assert.False(isEqual);
            }
        }
    }
}
