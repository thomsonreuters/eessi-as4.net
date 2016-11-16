using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Deliver;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.Utilities;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.Common.MessageProperty;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using Service = Eu.EDelivery.AS4.Model.Common.Service;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Deliver
{
    /// <summary>
    /// Testing <see cref="CreateDeliverMessageStep"/>
    /// </summary>
    public class GivenCreateDeliverMessageStepFacts
    {
        private readonly CreateDeliverMessageStep _step;

        public GivenCreateDeliverMessageStepFacts()
        {
            this._step = new CreateDeliverMessageStep();
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
        }

        public class GivenValidArguments : GivenCreateDeliverMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidUserMessageAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                Assert.NotNull(result.InternalMessage.DeliverMessage);
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
            public async Task ThenExecuteStepSucceedsWithValidAgreementRefAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                Agreement agreement = result.InternalMessage.DeliverMessage.CollaborationInfo.AgreementRef;
                Assert.NotNull(agreement);
                Assert.NotEmpty(agreement.Value);
                Assert.NotNull(agreement.PModeId);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidServiceAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                Service service = result.InternalMessage.DeliverMessage.CollaborationInfo.Service;
                Assert.NotNull(service);
                Assert.NotEmpty(service.Type);
                Assert.NotEmpty(service.Value);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidFromPartyAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                AS4.Model.Common.Party deliverParty = result.InternalMessage.DeliverMessage.PartyInfo.FromParty;
                Assert.NotNull(deliverParty);
                Assert.NotEmpty(deliverParty.Role);
                Assert.NotEmpty(deliverParty.PartyIds);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithvalidToPartyAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                AS4.Model.Common.Party deliverParty = result.InternalMessage.DeliverMessage.PartyInfo.ToParty;
                Assert.NotNull(deliverParty);
                Assert.NotEmpty(deliverParty.Role);
                Assert.NotEmpty(deliverParty.PartyIds);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidMessagePropertiesAsync()
            {
                // Act
                StepResult result = await ExecuteStepWithDefaultInternalMessage();
                // Assert
                MessageProperty[] props = result.InternalMessage.DeliverMessage.MessageProperties;
                Assert.NotNull(props);
                Assert.NotEmpty(props);
            }

            private async Task<StepResult> ExecuteStepWithDefaultInternalMessage()
            {
                // Arrange
                InternalMessage internalMessage = base.CreateDefaultInternalMessage();
                // Act
                return await base._step.ExecuteAsync(internalMessage, CancellationToken.None);
            }
        }

        protected InternalMessage CreateDefaultInternalMessage()
        {
            UserMessage userMessage = CreateUserMessage();
            AS4Message as4Message = new AS4MessageBuilder().WithUserMessage(userMessage).Build();
            return new InternalMessage(as4Message);
        }

        protected UserMessage CreateUserMessage()
        {
            return new UserMessage(messageId: "c2eee028-e27d-4960-98a6-f5b3d9cd152-639668164@CLT-SMOREELS.ad.codit.eu")
            {
                Mpc = "mpc",
                RefToMessageId = "b2eee028-e27d-4960-98a6-f5b3d9cd152-639668164@CLT-SMOREELS.ad.codit.eu",
                CollaborationInfo = CreateCollaborationInfo(),
                Receiver = CreateParty("Receiver", "org:eu:europa:as4:example"),
                Sender = CreateParty("Sender", "org:holodeckb2b:example:company:A"),
                MessageProperties = new List<AS4.Model.Core.MessageProperty> {new AS4.Model.Core.MessageProperty("Name", "Type", "Value")}
            };
        }

        private CollaborationInfo CreateCollaborationInfo()
        {
            return new CollaborationInfo
            {
                Action = "StoreMessage",
                Service = {Name = "Test", Type = "org:holodeckb2b:services"},
                ConversationId = "org:holodeckb2b:test:conversation",
                AgreementReference = new AgreementReference {Value = "http://agreements.holodeckb2b.org/examples/agreement0", PModeId = "Id"}
            };
        }

        private AS4.Model.Core.Party CreateParty(string role, string partyId)
        {
            return new AS4.Model.Core.Party {Role = role, PartyIds = new List<PartyId> {new PartyId(partyId)}};
        }
    }
}