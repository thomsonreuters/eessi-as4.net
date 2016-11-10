using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CommonParty = Eu.EDelivery.AS4.Model.Common.Party;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Testing <see cref="SubmitSenderPartyResolver"/>
    /// </summary>
    public class GivenSubmitSenderPartyResolverFacts
    {
        public class GivenValidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverGetsPartyFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.PartyInfo.FromParty = base.CreatePopulatedCommonParty();
                submitMessage.PMode = new SendingProcessingMode();
                var resolver = new SubmitSenderPartyResolver();
                // Act
                CoreParty party = resolver.Resolve(submitMessage);
                // Assert
                Party fromParty = submitMessage.PartyInfo.FromParty;
                Assert.Equal(fromParty.Role, party.Role);
                Assert.Equal(fromParty.PartyIds.First().Id, party.PartyIds.First().Id);
                Assert.Equal(fromParty.PartyIds.First().Type, party.PartyIds.First().Type);
            }
        }

        protected CommonParty CreatePopulatedCommonParty()
        {
            return new CommonParty
            {
                Role = "submit-role",
                PartyIds = new[] {new PartyId("submit-id", "submit-type")}
            };
        }

        protected CoreParty CreatePopulatedCoreParty()
        {
            return new CoreParty
            {
                Role = "pmode-role",
                PartyIds = new List<AS4.Model.Core.PartyId> {new AS4.Model.Core.PartyId("pmode-id")}
            };
        }
    }
}