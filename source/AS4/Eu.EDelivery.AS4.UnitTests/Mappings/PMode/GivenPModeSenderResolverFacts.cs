using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeSenderResolver" />
    /// </summary>
    public class GivenPModeSenderResolverFacts
    {
        public class GivenValidArguments : GivenPModeSenderResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultSender()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                var resolver = PModeSenderResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(pmode);

                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultFrom, party.PartyIds.FirstOrDefault()?.Id);
                Assert.Equal(Constants.Namespaces.EbmsDefaultRole, party.Role);
            }

            [Fact]
            public void ThenResolverGetsSender()
            {
                // Arrange
                var pmode = new SendingProcessingMode
                {
                    MessagePackaging =
                    {
                        PartyInfo = new PartyInfo {FromParty = new Party("role", "party-id")}
                    }
                };
                var resolver = PModeSenderResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(pmode);

                // Assert
                Party fromParty = pmode.MessagePackaging.PartyInfo.FromParty;
                Assert.Equal(fromParty.Role, party.Role);
                Assert.Equal(fromParty.PrimaryPartyId, party.PartyIds.First().Id);
            }
        }
    }
}