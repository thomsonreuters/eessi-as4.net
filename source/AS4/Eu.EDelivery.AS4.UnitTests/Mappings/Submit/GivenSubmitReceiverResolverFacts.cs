using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.Submit;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Xunit;
using CommonParty = Eu.EDelivery.AS4.Model.Common.Party;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Submit
{
    /// <summary>
    /// Tesing <see cref="SubmitReceiverResolver" />
    /// </summary>
    public class GivenSubmitReceiverResolverFacts
    {
        public class GivenValidArguments : GivenSubmitReceiverResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultParty()
            {
                // Arrange
                var submitMessage = new SubmitMessage { PMode = new SendingProcessingMode() };
                var resolver = SubmitReceiverResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                Assert.Equal(Constants.Namespaces.EbmsDefaultTo, party.PartyIds.First().Id);
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
                            MessagePackaging = { PartyInfo = new PartyInfo { ToParty = CreatePopulatedPModeParty() } }
                        }
                };
                var resolver = SubmitReceiverResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                var toParty = submitMessage.PMode.MessagePackaging.PartyInfo.ToParty;
                Assert.Equal(toParty.Role, party.Role);
                Assert.Equal(toParty.PrimaryPartyId, party.PartyIds.First().Id);
            }

            [Fact]
            public void ThenResolverGetsPartyFromSubmitMessage()
            {
                // Arrange
                var submitMessage = new SubmitMessage
                {
                    PartyInfo = { ToParty = CreatePopulatedCommonParty() },
                    PMode = new SendingProcessingMode { AllowOverride = true }
                };
                var resolver = SubmitReceiverResolver.Default;

                // Act
                CoreParty party = resolver.Resolve(submitMessage);

                // Assert
                var toParty = submitMessage.PartyInfo.ToParty;
                Assert.Equal(toParty.Role, party.Role);
                Assert.Equal(toParty.PartyIds.First().Id, party.PartyIds.First().Id);
                Assert.Equal(toParty.PartyIds.First().Type, party.PartyIds.First().Type);
            }
        }


        [Fact]
        public void ThenResolverFailsWhenOverrideIsNotAllowed()
        {
            // Arrange
            var submitMessage = new SubmitMessage { PartyInfo = { ToParty = CreatePopulatedCommonParty() } };

            var pmode = new SendingProcessingMode
            {
                AllowOverride = false,
                MessagePackaging = { PartyInfo = new PartyInfo { ToParty = CreatePopulatedPModeParty() } }
            };
            submitMessage.PMode = pmode;
            var resolver = SubmitReceiverResolver.Default;

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => resolver.Resolve(submitMessage));
        }


        protected CommonParty CreatePopulatedCommonParty()
        {
            return new CommonParty { Role = "submit-role", PartyIds = new[] { new AS4.Model.Common.PartyId("submit-id", "submit-type") } };
        }

        protected PModeParty CreatePopulatedPModeParty()
        {
            return new PModeParty("pmode-role", "pmode-id");
        }
    }
}