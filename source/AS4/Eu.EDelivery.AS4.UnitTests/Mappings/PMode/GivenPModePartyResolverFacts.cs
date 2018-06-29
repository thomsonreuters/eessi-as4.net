using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using FsCheck;
using FsCheck.Xunit;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    public class GivenPModePartyResolverFacts
    {
        [Property]
        public Property Then_Sender_Gets_Populated_When_Present(
            NonEmptyString role, 
            NonEmptyString partyId)
        {
            return Prop.ForAll(
                ArbParty(role, partyId),
                p =>
                {
                    var actual = PModePartyResolver.ResolveSender(p);

                    bool isDefault = actual.Equals(CoreParty.DefaultFrom);
                    bool isResolved = actual.Role == role.Get && actual.PrimaryPartyId == partyId.Get;

                    return (isDefault == (p == null)).And(isResolved == (p != null));
                });
        }

        [Property]
        public Property Then_Receiver_Gets_Populated_When_Present(
            NonEmptyString role,
            NonEmptyString partyId)
        {
            return Prop.ForAll(
                ArbParty(role, partyId),
                p =>
                {
                    var actual = PModePartyResolver.ResolveReceiver(p);

                    bool isDefault = actual.Equals(CoreParty.DefaultTo);
                    bool isResolved = actual.Role == role.Get && actual.PrimaryPartyId == partyId.Get;

                    return (isDefault == (p == null)).And(isResolved == (p != null));
                });
        }

        private static Arbitrary<Party> ArbParty(NonEmptyString role, NonEmptyString partyId)
        {
            return Gen.OneOf(
                Gen.Fresh(() => new Party(role.Get, partyId.Get)),
                Gen.Constant<Party>(null))
                      .ToArbitrary();
        }
    }
}