using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

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
                var resolver = new PModeReceiverResolver();

                // Act
                Party party = resolver.Resolve(pmode);

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
                        PartyInfo = new PartyInfo {ToParty = new Party("role", new PartyId("party-id"))}
                    }
                };
                var resolver = new PModeReceiverResolver();

                // Act
                Party party = resolver.Resolve(pmode);

                // Assert
                Assert.Equal(pmode.MessagePackaging.PartyInfo.ToParty, party);
            }
        }
    }
}