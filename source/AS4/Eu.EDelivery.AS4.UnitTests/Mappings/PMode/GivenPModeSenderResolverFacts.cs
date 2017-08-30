using System.Linq;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

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
                Party party = resolver.Resolve(pmode);

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
                        PartyInfo = new PartyInfo {FromParty = new Party("role", new PartyId("party-id"))}
                    }
                };
                var resolver = PModeSenderResolver.Default;

                // Act
                Party party = resolver.Resolve(pmode);

                // Assert
                Assert.Equal(pmode.MessagePackaging.PartyInfo.FromParty, party);
            }
        }
    }
}