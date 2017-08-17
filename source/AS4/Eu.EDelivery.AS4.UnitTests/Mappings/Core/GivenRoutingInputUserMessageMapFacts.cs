using System;
using System.Linq;
using Eu.EDelivery.AS4.Singletons;
using Eu.EDelivery.AS4.Xml;
using Xunit;

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

            Assert.Equal(routingInput.PartyInfo.From.PartyId.First().Value, userMessage.Sender.PartyIds.First().Id);
            Assert.Equal(routingInput.PartyInfo.From.PartyId.First().type, userMessage.Sender.PartyIds.First().Type);
            Assert.Equal(routingInput.PartyInfo.From.Role, userMessage.Sender.Role);

            Assert.Equal(routingInput.PartyInfo.To.PartyId.First().Value, userMessage.Receiver.PartyIds.First().Id);
            Assert.Equal(routingInput.PartyInfo.To.PartyId.First().type, userMessage.Receiver.PartyIds.First().Type);
            Assert.Equal(routingInput.PartyInfo.To.Role, userMessage.Receiver.Role);
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
