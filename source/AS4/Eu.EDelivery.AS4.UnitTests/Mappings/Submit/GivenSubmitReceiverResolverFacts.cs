using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Common;
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
    /// Tesing <see cref="SubmitReceiverResolver"/>
    /// </summary>
    public class GivenSubmitReceiverResolverFacts
    {
        
        public class GivenValidArguments : GivenSubmitReceiverResolverFacts
        {
            [Fact]
            public void ThenResolverGetsPartyFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.PartyInfo.ToParty = base.CreatePopulatedCommonParty();
                submitMessage.PMode = new SendingProcessingMode {AllowOverride = true};
                var resolver = new SubmitReceiverResolver();
                // Act
                CoreParty party = resolver.Resolve(submitMessage);
                // Assert
                CommonParty toParty = submitMessage.PartyInfo.ToParty;
                Assert.Equal(toParty.Role, party.Role);
                Assert.Equal(toParty.PartyIds.First().Id, party.PartyIds.First().Id);
                Assert.Equal(toParty.PartyIds.First().Type, party.PartyIds.First().Type);
            }

            [Fact]
            public void ThenResolverGetsPartyFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                var pmode = new SendingProcessingMode();
                pmode.MessagePackaging.PartyInfo = new AS4.Model.PMode.PartyInfo();
                pmode.MessagePackaging.PartyInfo.ToParty = base.CreatePopulatedCoreParty();
                submitMessage.PMode = pmode;
                var resolver = new SubmitReceiverResolver();
                // Act
                CoreParty party = resolver.Resolve(submitMessage);
                // Assert
                Assert.Equal(submitMessage.PMode.MessagePackaging.PartyInfo.ToParty, party);
            }

            [Fact]
            public void ThenResolverGetsDefaultParty()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.PMode = new SendingProcessingMode();
                var resolver = new SubmitReceiverResolver();
                // Act
                CoreParty party = resolver.Resolve(submitMessage);
                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultTo, party.PartyIds.First().Id);
                Assert.Equal(Constants.Namespaces.EbmsDefaultRole, party.Role);
            }
        }

        public class GivenInvalidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage();
                submitMessage.PartyInfo.ToParty = base.CreatePopulatedCommonParty();

                var pmode = new SendingProcessingMode();
                pmode.AllowOverride = false;
                pmode.MessagePackaging.PartyInfo = new AS4.Model.PMode.PartyInfo();
                pmode.MessagePackaging.PartyInfo.ToParty = base.CreatePopulatedCoreParty();
                submitMessage.PMode = pmode;
                var resolver = new SubmitReceiverResolver();

                // Act / Assert
                Assert.Throws<AS4Exception>(() => resolver.Resolve(submitMessage));
            }
        }

        protected CommonParty CreatePopulatedCommonParty()
        {
            return new CommonParty
            {
                Role = "submit-role",
                PartyIds = new[] { new PartyId("submit-id", "submit-type") }
            };
        }

        protected CoreParty CreatePopulatedCoreParty()
        {
            return new CoreParty
            {
                Role = "pmode-role",
                PartyIds = new List<AS4.Model.Core.PartyId> { new AS4.Model.Core.PartyId("pmode-id") }
            };
        }
    }
}
