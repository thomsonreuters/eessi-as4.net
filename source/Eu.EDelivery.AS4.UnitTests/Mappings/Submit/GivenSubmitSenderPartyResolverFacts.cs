using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using static Eu.EDelivery.AS4.Mappings.Submit.SubmitSenderResolver;
using CommonParty = Eu.EDelivery.AS4.Model.Common.Party;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    public class GivenSubmitSenderPartyResolverFacts
    {
        public class GivenValidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultParty()
            {
                // Arrange
                var submitMessage = new SubmitMessage {PMode = new SendingProcessingMode()};

                // Act
                CoreParty party = ResolveSender(submitMessage);

                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultFrom, party.PartyIds.First().Id);
                Assert.Equal(Constants.Namespaces.EbmsDefaultRole, party.Role);
            }

            [Fact]
            public void ThenResolverGetsPartyFromPMode()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PMode =
                        new SendingProcessingMode
                        {
                            MessagePackaging = {PartyInfo = new PartyInfo {FromParty = CreatePopulatedPModeParty()}}
                        }
                };

                // Act
                CoreParty party = ResolveSender(submitMessage);

                // Assert
                var fromParty = submitMessage.PMode.MessagePackaging.PartyInfo.FromParty;
                Assert.Equal(fromParty.Role, party.Role);
                Assert.Equal(fromParty.PrimaryPartyId, party.PartyIds.First().Id);
            }

            [Fact]
            public void ThenResolverGetsPartyFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PartyInfo = {FromParty = CreatePopulatedCommonParty()},
                    PMode = new SendingProcessingMode {AllowOverride = true}
                };

                // Act
                CoreParty party = ResolveSender(submitMessage);

                // Assert
                CommonParty fromParty = submitMessage.PartyInfo.FromParty;
                Assert.Equal(fromParty.Role, party.Role);
                Assert.Equal(fromParty.PartyIds.First().Id, party.PartyIds.First().Id);
                Assert.Equal(fromParty.PartyIds.First().Type, party.PartyIds.First().Type.UnsafeGet);
            }
        }

        public class GivenInvalidArguments : GivenSubmitSenderPartyResolverFacts
        {
            [Fact]
            public void ThenResolverFailsWhenOverrideIsNotAllowed()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PartyInfo = {FromParty = CreatePopulatedCommonParty()},
                    PMode =
                        new SendingProcessingMode
                        {
                            AllowOverride = false,
                            MessagePackaging = {PartyInfo = new PartyInfo {FromParty = CreatePopulatedPModeParty()}}
                        }
                };


                // Act / Assert
                Assert.ThrowsAny<Exception>(() => ResolveSender(submitMessage));
            }
        }

        protected CommonParty CreatePopulatedCommonParty()
        {
            return new CommonParty {Role = "submit-role", PartyIds = new[] {new AS4.Model.Common.PartyId("submit-id", "submit-type")}};
        }

        protected PModeParty CreatePopulatedPModeParty()
        {
            return new PModeParty("pmode-role", "pmode-id");
        }
    }
}