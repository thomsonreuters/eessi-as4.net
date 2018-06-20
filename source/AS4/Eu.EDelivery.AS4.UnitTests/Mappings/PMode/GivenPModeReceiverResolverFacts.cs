using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeReceiverResolver" />
    /// </summary>
    public class GivenPModeReceiverResolverFacts
    {
        public class GivenValidArguments : GivenPModeReceiverResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefautlReceiver()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                var resolver = PModeReceiverResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(pmode);

                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultTo, party.PartyIds.FirstOrDefault()?.Id);
                Assert.Equal(Constants.Namespaces.EbmsDefaultRole, party.Role);
            }

            [Fact]
            public void ThenResolverGetsReceiver()
            {
                // Arrange
                var pmode = new SendingProcessingMode
                {
                    MessagePackaging =
                    {
                        PartyInfo = new PartyInfo {ToParty = new Party("role", "party-id")}
                    }
                };
                var resolver = PModeReceiverResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(pmode);

                // Assert
                Party toParty = pmode.MessagePackaging.PartyInfo.ToParty;
                Assert.Equal(toParty.Role, party.Role);
                Assert.Equal(toParty.PrimaryPartyId, party.PartyIds.First().Id);
            }
        }
    }
}