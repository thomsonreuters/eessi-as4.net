using Eu.EDelivery.AS4.Model.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="Party" />
    /// </summary>
    public class GivenPartyFacts
    {
        public class GivenValidArguments : GivenPartyFacts
        {
            [Theory]
            [InlineData("role")]
            public void ThenPartyIsNotEmptyForPartyId(string role)
            {
                // Arrange
                var partyA = new Party {Role = role};

                // Act
                bool isEmpty = partyA.IsEmpty();

                // Assert
                Assert.False(isEmpty);
            }

            [Theory]
            [InlineData("id")]
            public void ThenPartyIsNotEmptyForRole(string id)
            {
                // Arrange
                var partyA = new Party(new PartyId(id));

                // Act
                bool isEmpty = partyA.IsEmpty();

                // Assert
                Assert.False(isEmpty);
            }

            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreEqual(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                Party partyB = partyA;

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreEqualForObject(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                var partyB = new Party(sharedRole, new PartyId(sharedId));

                // Act
                bool isEqual = partyA.Equals((object)partyB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreEqualForRolAndPartyId(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                var partyB = new Party(sharedRole, new PartyId(sharedId));

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.True(isEqual);
            }

            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreNotEqualForPartyId(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                var partyB = new Party(sharedRole, new PartyId("not-Equal"));

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.False(isEqual);
            }

            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreNotEqualForRole(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                var partyB = new Party("not-equal", new PartyId(sharedId));

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.False(isEqual);
            }
        }

        public class GivenInvalidArguments : GivenPartyFacts
        {
            [Theory]
            [InlineData("shared-role", "shared-id")]
            public void ThenTwoPartiesAreNotEqualForNull(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                Party partyB = null;

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.False(isEqual);
            }
        }
    }
}