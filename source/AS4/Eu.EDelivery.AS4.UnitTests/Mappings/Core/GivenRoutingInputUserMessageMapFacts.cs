using System;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Xml.CollaborationInfo;
using PartyId = Eu.EDelivery.AS4.Xml.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.Core
{
    public class GivenRoutingInputUserMessageMapFacts
    {
        [Fact]
        public void ThenActionIsCorrectlyMapped()
        {
            RoutingInputUserMessage routingInput = CreateDefaultRoutingInputMessage();

            routingInput.CollaborationInfo = new CollaborationInfo()
            {
                Action = "ActionDescription.Response"
            };

            var result = AS4Mapper.Map<AS4.Model.Core.UserMessage>(routingInput);

            Assert.NotNull(result);

            // Verify that the .Response suffix is not present.
            Assert.Equal("ActionDescription", result.CollaborationInfo.Action);
        }

        [Fact]
        public void ThenPartyInfoIsCorrectlyMapped()
        {
            var routingInput = CreateDefaultRoutingInputMessage();

            var userMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(routingInput);

            Assert.NotNull(userMessage);


            From fromParty = routingInput.PartyInfo.From;
            To toParty = routingInput.PartyInfo.To;
            Party sender = userMessage.Sender;
            Party receiver = userMessage.Receiver;

            Assert.Equal(fromParty.PartyId.First().Value, sender.PartyIds.First().Id);
            Assert.True((fromParty.PartyId.First().type == null) == (sender.PartyIds.First().Type == String.Empty));
            Assert.Equal(fromParty.Role, userMessage.Sender.Role);

            Assert.Equal(toParty.PartyId.First().Value, receiver.PartyIds.First().Id);
            Assert.True((toParty.PartyId.First().type == null) == (receiver.PartyIds.First().Type == String.Empty));
            Assert.Equal(toParty.Role, receiver.Role);
        }

        [Fact]
        public void ThenMpcIsCorrectlyMapped()
        {
            var routingInput = CreateDefaultRoutingInputMessage();
            routingInput.mpc = "some-mpc";

            var userMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(routingInput);

            Assert.NotNull(userMessage);
            Assert.Equal(routingInput.mpc, userMessage.Mpc);
        }

        [Fact]
        public void ThenDefaultMpcIsAssignedWhenNoMpcIsPresent()
        {
            var routingInput = CreateDefaultRoutingInputMessage();
            routingInput.mpc = String.Empty;

            var userMessage = AS4Mapper.Map<AS4.Model.Core.UserMessage>(routingInput);

            Assert.NotNull(userMessage);
            Assert.Equal(Constants.Namespaces.EbmsDefaultMpc, userMessage.Mpc);
        }

        private static RoutingInputUserMessage CreateDefaultRoutingInputMessage()
        {
            var routingInput = new RoutingInputUserMessage()
            {
                PartyInfo = new PartyInfo()
                {
                    From = new From() { Role = "Sender", PartyId = new[] { new PartyId() { Value = "partyA" } } },
                    To = new To() { Role = "Receiver", PartyId = new[] { new PartyId() { Value = "partyB" } } },
                }
            };

            return routingInput;
        }
    }
}
