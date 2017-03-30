using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Common.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="CreateDeliverMessageStep" />
    /// </summary>
    public class GivenCreateDeliverMessageStepFacts
    {
        private readonly CreateDeliverMessageStep _step;

        public GivenCreateDeliverMessageStepFacts()
        {
            _step = new CreateDeliverMessageStep();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenCreateDeliverMessageStepFacts
        {
            private async Task<StepResult> ExecuteStepWithDefaultInternalMessage()
            {
                // Arrange
                InternalMessage internalMessage = CreateDefaultInternalMessage();

                // Act
                return await _step.ExecuteAsync(internalMessage, CancellationToken.None);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidAgreementRefAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                var deliverMessage =
                    AS4XmlSerializer.Deserialize<DeliverMessage>(
                        Encoding.UTF8.GetString(result.InternalMessage.DeliverMessage.DeliverMessage));
                Agreement agreement = deliverMessage.CollaborationInfo.AgreementRef;
                Assert.NotNull(agreement);
                Assert.NotEmpty(agreement.Value);
                Assert.NotNull(agreement.PModeId);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidFromPartyAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                var deliverMessage =
                    AS4XmlSerializer.Deserialize<DeliverMessage>(
                        Encoding.UTF8.GetString(result.InternalMessage.DeliverMessage.DeliverMessage));
                AS4.Model.Common.Party deliverParty = deliverMessage.PartyInfo.FromParty;
                Assert.NotNull(deliverParty);
                Assert.NotEmpty(deliverParty.Role);
                Assert.NotEmpty(deliverParty.PartyIds);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidMessageInfoAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                MessageInfo messageInfo = result.InternalMessage.DeliverMessage.MessageInfo;
                Assert.NotEmpty(messageInfo.MessageId);
                Assert.NotEmpty(messageInfo.Mpc);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidMessagePropertiesAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                var deliverMessage =
                    AS4XmlSerializer.Deserialize<DeliverMessage>(
                        Encoding.UTF8.GetString(result.InternalMessage.DeliverMessage.DeliverMessage));
                AS4.Model.Common.MessageProperty[] props = deliverMessage.MessageProperties;
                Assert.NotNull(props);
                Assert.NotEmpty(props);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidServiceAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                var deliverMessage =
                    AS4XmlSerializer.Deserialize<DeliverMessage>(
                        Encoding.UTF8.GetString(result.InternalMessage.DeliverMessage.DeliverMessage));
                Service service = deliverMessage.CollaborationInfo.Service;
                Assert.NotNull(service);
                Assert.NotEmpty(service.Type);
                Assert.NotEmpty(service.Value);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithvalidToPartyAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                var deliverMessage =
                    AS4XmlSerializer.Deserialize<DeliverMessage>(
                        Encoding.UTF8.GetString(result.InternalMessage.DeliverMessage.DeliverMessage));

                AS4.Model.Common.Party deliverParty = deliverMessage.PartyInfo.ToParty;
                Assert.NotNull(deliverParty);
                Assert.NotEmpty(deliverParty.Role);
                Assert.NotEmpty(deliverParty.PartyIds);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidUserMessageAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();

                // Assert
                Assert.NotNull(result.InternalMessage.DeliverMessage);
            }
        }

        private static InternalMessage CreateDefaultInternalMessage()
        {
            UserMessage userMessage = CreateUserMessage();
            AS4Message as4Message = new AS4MessageBuilder().WithUserMessage(userMessage).Build();

            return new InternalMessage(as4Message);
        }

        private static UserMessage CreateUserMessage()
        {
            return new UserMessage("c2eee028-e27d-4960-98a6-f5b3d9cd152-639668164@CLT-SMOREELS.ad.codit.eu")
            {
                Mpc = "mpc",
                RefToMessageId = "b2eee028-e27d-4960-98a6-f5b3d9cd152-639668164@CLT-SMOREELS.ad.codit.eu",
                CollaborationInfo = CreateCollaborationInfo(),
                Receiver = CreateParty("Receiver", "org:eu:europa:as4:example"),
                Sender = CreateParty("Sender", "org:holodeckb2b:example:company:A"),
                MessageProperties = CreateMessageProperties()
            };
        }

        private static List<MessageProperty> CreateMessageProperties()
        {
            return new List<MessageProperty> { new MessageProperty("Name", "Type", "Value") };
        }

        private static CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = "StoreMessage",
                Service = {
                             Value = "Test", Type = "org:holodeckb2b:services"
                          },
                ConversationId = "org:holodeckb2b:test:conversation",
                AgreementReference = CreateAgreementReference()
            };
        }

        private static AgreementReference CreateAgreementReference()
        {
            return new AgreementReference
            {
                Value = "http://agreements.holodeckb2b.org/examples/agreement0",
                PModeId = "Id"
            };
        }

        private static Party CreateParty(string role, string partyId)
        {
            var partyIds = new List<PartyId> { new PartyId(partyId) };
            return new Party { Role = role, PartyIds = partyIds };
        }
    }
}