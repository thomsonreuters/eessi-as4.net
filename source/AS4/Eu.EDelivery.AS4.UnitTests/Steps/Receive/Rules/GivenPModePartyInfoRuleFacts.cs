using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps.Receive.Rules;
using Xunit;
using CoreParty = Eu.EDelivery.AS4.Model.Core.Party;
using CorePartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using PModePartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive.Rules
{
    /// <summary>
    /// Testing <see cref="PModePartyInfoRule"/>
    /// </summary>
    public class GivenPModePartyInfoRuleFacts
    {
        public class GivenValidArguments : GivenPModePartyInfoRuleFacts
        {
            [Theory, InlineData("role", "party-id")]
            public void ThenMaxPointsWhenPartyIdAndRoleMatch(string role, string partyId)
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        CreatePModeParty(role, partyId), 
                        CreatePModeParty(role, partyId));

                UserMessage userMessage = 
                    CreateUserMessageWithParties(
                        CreateCoreParty(role, partyId), 
                        CreateCoreParty(role, partyId));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(16, points);
            }

            [Fact]
            public void Then15PointsWhenBothSenderAndReceiverMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        CreatePModeParty("senderrole1", "sender-id"), 
                        CreatePModeParty("receiverole1", "receiver-id"));

                UserMessage userMessage = 
                    CreateUserMessageWithParties(
                        CreateCoreParty(Guid.NewGuid().ToString(), "sender-id"), 
                        CreateCoreParty(Guid.NewGuid().ToString(), "receiver-id"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(15, points);
            }

            [Fact]
            public void Then8PointsWhenOnlyReceiverIdIsSpecifiedAndMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        null, 
                        CreatePModeParty("receiverrole", "receiver"));

                UserMessage userMessage = 
                    CreateUserMessageWithParties(
                        CreateCoreParty("senderrole2", "senderId2"), 
                        CreateCoreParty(Guid.NewGuid().ToString(), "receiver"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(8, points);
            }

            [Fact]
            public void Then7PointsWhenOnlySenderIdIsSpecifiedAndMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        CreatePModeParty("sender-role1", "sender-id1"), 
                        null);

                UserMessage userMessage = 
                    CreateUserMessageWithParties(
                        CreateCoreParty(Guid.NewGuid().ToString(), "sender-id1"), 
                        CreateCoreParty(Guid.NewGuid().ToString(), "receiver-id2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(7, points);
            }

            [Fact]
            public void Then1PointWhenOnlyRoleMatches()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        CreatePModeParty("sender-role", "sender-1"), 
                        CreatePModeParty("receiving-role", "receiver-1"));

                UserMessage userMessage = 
                    CreateUserMessageWithParties(
                        CreateCoreParty("sender-role", "sender-2"), 
                        CreateCoreParty("receiving-role", "receiver-2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(1, points);
            }

            [Fact]
            public void ThenZeroPointsWhenPartyIdAndRoleDontMatch()
            {
                // Arrange
                ReceivingProcessingMode receivingPMode =
                    CreateReceivingPModeWithParties(
                        CreatePModeParty("sender-role-1", "sender-1"), 
                        CreatePModeParty("receiving-role-1", "receiver-1"));


                UserMessage userMessage =
                    CreateUserMessageWithParties(
                        CreateCoreParty("sender-role-2", "sender-2"), 
                        CreateCoreParty("receiving-role-2", "receiver-2"));

                var rule = new PModePartyInfoRule();
                // Act
                int points = rule.DeterminePoints(receivingPMode, userMessage);
                // Assert
                Assert.Equal(0, points);
            }
        }
      
        protected UserMessage CreateUserMessageWithParties(
            CoreParty fromParty,
            CoreParty toParty)
        {
            return new UserMessage(messageId: "message-id")
            {
                Receiver = toParty,
                Sender = fromParty
            };
        }

        protected ReceivingProcessingMode CreateReceivingPModeWithParties(
            PModeParty fromParty,
            PModeParty toParty)
        {
            return new ReceivingProcessingMode
            {
                MessagePackaging =
                {
                    PartyInfo = new PartyInfo
                    {
                        FromParty = fromParty,
                        ToParty = toParty
                    }
                }
            };
        }

        private static CoreParty CreateCoreParty(string role, string partyId)
        {
            return new CoreParty(role, new CorePartyId(partyId));
        }

        private static PModeParty CreatePModeParty(string role, string partyId)
        {
            return new PModeParty
            {
                Role = role,
                PartyIds = { new PModePartyId { Id = partyId } }
            };
        }
    }
}