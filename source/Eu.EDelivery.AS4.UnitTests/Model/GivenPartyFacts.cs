using System;
using Eu.EDelivery.AS4.Model.Core;
using FsCheck;
using FsCheck.Xunit;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    /// <summary>
    /// Testing <see cref="Party" />
    /// </summary>
    public class GivenPartyFacts
    {
        [Property]
        public Property EqualsParties(NonEmptyString roleA, NonEmptyString roleB)
        {
            return Prop.ForAll(
                Gen.OneOf(
                       Arb.From<NonEmptyString>().Generator.Select(x => Tuple.Create(x, x)))
                   .Three()
                   .ToArbitrary(),
                xs =>
                {
                    (NonEmptyString idA, NonEmptyString idB) = xs.Item2;
                    (NonEmptyString typeA, NonEmptyString typeB) = xs.Item3;

                    var a = new Party(roleA.Get, new PartyId(idA.Get, typeA.Get));
                    var b = new Party(roleB.Get, new PartyId(idB.Get, typeB.Get));

                    var equalId = idA.Equals(idB);
                    var equalType = typeA.Equals(typeB);

                    return a.Equals(b) == (equalId && equalType);
                });
        }

        public class GivenValidArguments : GivenPartyFacts
        {
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
            public void ThenTwoPartiesArEqualForUnequalRole(string sharedRole, string sharedId)
            {
                // Arrange
                var partyA = new Party(sharedRole, new PartyId(sharedId));
                var partyB = new Party("not-equal", new PartyId(sharedId));

                // Act
                bool isEqual = partyA.Equals(partyB);

                // Assert
                Assert.True(isEqual);
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