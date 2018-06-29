using System;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;
using Moq;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using ReceivePMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    /// <summary>
    /// Testing the <see cref="DeterminePModesStep" />
    /// </summary>
    public class GivenDeterminePModesStepFacts : GivenDatastoreFacts
    {
        private readonly Mock<IConfig> _mockedConfig;
        private readonly DeterminePModesStep _step;

        public GivenDeterminePModesStepFacts()
        {
            _mockedConfig = new Mock<IConfig>();
            _step = new DeterminePModesStep(_mockedConfig.Object, GetDataStoreContext);
        }

        public class GivenValidArguments : GivenDeterminePModesStepFacts
        {
            [Fact]
            public async Task Determine_Both_Sending_And_Receiving_PMode_When_Bundled()
            {
                // Arrange
                var nonMultihopSignal = new Receipt(
                    messageId: $"receipt-{Guid.NewGuid()}", 
                    refToMessageId: $"reftoid-{Guid.NewGuid()}");

                string receivePModeId = $"receive-pmodeid-{Guid.NewGuid()}";
                var userMesssage = new UserMessage(messageId: $"user-{Guid.NewGuid()}");
                userMesssage.CollaborationInfo = 
                    new AS4.Model.Core.CollaborationInfo(
                        new AgreementReference("agreement", receivePModeId));

                string sendPModeId = $"send-pmodeid-{Guid.NewGuid()}";
                var expected = new SendingProcessingMode { Id = sendPModeId };
                InsertOutMessage(nonMultihopSignal.RefToMessageId, expected);

                var msg = AS4Message.Create(userMesssage);
                msg.AddMessageUnit(nonMultihopSignal);

                // Act
                StepResult result = await ExerciseDeterminePModes(
                    msg, 
                    new ReceivePMode
                    {
                        Id = receivePModeId,
                        ReplyHandling = new ReplyHandlingSetting { SendingPMode = "some-other-send-pmodeid" }
                    });

                // Assert
                Assert.Equal(
                    receivePModeId,
                    result.MessagingContext.ReceivingPMode.Id);
                Assert.Equal(
                    sendPModeId,
                    result.MessagingContext.SendingPMode.Id);
            }

            [Fact]
            public async Task Dont_Use_Scoring_System_ReceivingPMode_When_Already_Configure()
            {
                // Arrange
                var expected = new ReceivePMode { Id = "static-receive-configured" };

                // Act
                StepResult result = await _step.ExecuteAsync(
                    new MessagingContext(
                        AS4Message.Empty, 
                        MessagingContextMode.Receive)
                    {
                        ReceivingPMode = expected
                    });

                // Assert
                Assert.Same(
                    expected,
                    result.MessagingContext.ReceivingPMode);
            }

            [Fact]
            public async Task SendingPModeIsFound_IfSignalMessage()
            {
                // Arrange
                string messageId = Guid.NewGuid().ToString();
                var expected = new SendingProcessingMode { Id = Guid.NewGuid().ToString() };
                InsertOutMessage(messageId, expected);

                AS4Message as4Message = AS4Message.Create(new Receipt { RefToMessageId = messageId });

                // Act
                StepResult result = await ExerciseDeterminePModes(as4Message);

                // Assert
                SendingProcessingMode actual = result.MessagingContext.SendingPMode;
                Assert.Equal(expected.Id, actual.Id);
            }

            private void InsertOutMessage(string messageId, SendingProcessingMode pmode)
            {
                var outMessage = new OutMessage(ebmsMessageId: messageId);
                outMessage.SetPModeInformation(pmode);

                GetDataStoreContext.InsertOutMessage(outMessage, withReceptionAwareness: false);
            }

            private async Task<StepResult> ExerciseDeterminePModes(AS4Message message, params ReceivePMode[] pmodes)
            {
                var stubConfig = new Mock<IConfig>();
                stubConfig.Setup(c => c.GetReceivingPModes()).Returns(pmodes);
                var sut = new DeterminePModesStep(stubConfig.Object, GetDataStoreContext);

                return await sut.ExecuteAsync(
                    new MessagingContext(message, MessagingContextMode.Receive));
            }

            [Fact]
            public async Task ThenPModeIsFoundWithId()
            {
                // Arrange
                const string sharedId = "01-receive";
                var pmode = CreateDefaultPMode("01-receive");
                SetupPModes(pmode, CreateDefaultPMode("defaultMode"));

                MessagingContext messagingContext = new MessageContextBuilder().WithPModeId(sharedId).Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(pmode, result);
            }

            [Theory]
            [InlineData("From-Id", "To-Id")]
            public async Task ThenPartyInfoMatchesAsync(string fromId, string toId)
            {
                // Arrange
                var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
                var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };

                ReceivePMode pmode = CreatePModeWithParties(fromParty, toParty);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                SetupPModes(pmode, new ReceivePMode { Id = "other pmode", ReplyHandling = { SendingPMode = "other pmode" }});

                MessagingContext messagingContext = 
                    new MessageContextBuilder()
                        .WithParties(
                            new AS4.Model.Core.Party(fromId, new AS4.Model.Core.PartyId(fromId)), 
                            new AS4.Model.Core.Party(toId, new AS4.Model.Core.PartyId(toId)))
                        .Build();

                // Act               
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(pmode, result);
            }

            [Theory]
            [InlineData("service", "action")]
            public async Task ThenPartyInfoNotDefindedAsync(string service, string action)
            {
                // Arrange
                ReceivePMode pmode = ArrangePModeThenPartyInfoNotDefined(service, action);

                MessagingContext messagingContext =
                    new MessageContextBuilder().WithUserMessage(new UserMessage("message-id"))
                                                .WithServiceAction(service, action)
                                                .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(pmode, result);
            }

            private ReceivePMode ArrangePModeThenPartyInfoNotDefined(string service, string action)
            {
                ReceivePMode pmode = CreatePModeWithActionService(service, action);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                SetupPModes(pmode, new ReceivePMode { Id = "other pmode", ReplyHandling = { SendingPMode = "other pmode" } });

                return pmode;
            }

            [Theory]
            [InlineData("From-Id", "To-Id", "01-receive")]
            public async Task ThenPModeIdWinsOverPartyInfoAsync(string fromId, string toId, string sharedId)
            {
                // Arrange 
                var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
                var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };
                ReceivePMode idPMode = ArrangePModeThenPModeWinsOverPartyInfo(sharedId, fromParty, toParty);

                MessagingContext messagingContext =
                    new MessageContextBuilder()
                        .WithPModeId(sharedId)
                        .WithParties(
                            new AS4.Model.Core.Party(fromId, new AS4.Model.Core.PartyId(fromId)),
                            new AS4.Model.Core.Party(toId, new AS4.Model.Core.PartyId(toId)))
                        .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(idPMode, result);
            }

            private ReceivePMode ArrangePModeThenPModeWinsOverPartyInfo(string sharedId, Party fromParty, Party toParty)
            {
                ReceivePMode partyInfoPMode = CreatePModeWithParties(fromParty, toParty);
                partyInfoPMode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                var idPMode = CreateDefaultPMode(sharedId);

                SetupPModes(partyInfoPMode, idPMode);
                return idPMode;
            }

            [Theory]
            [InlineData("service", "action", "from-Id", "to-Id")]
            public async Task ThenPartyWinsOverServiceActionAsync(
                string service,
                string action,
                string fromId,
                string toId)
            {
                // Arrange
                var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
                var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };

                ReceivePMode pmodeParties = CreatePModeWithParties(fromParty, toParty);
                ReceivePMode pmodeServiceAction = CreatePModeWithActionService(service, action);
                SetupPModes(pmodeParties, pmodeServiceAction);

                MessagingContext messagingContext =
                    new MessageContextBuilder()
                        .WithParties(
                            new AS4.Model.Core.Party(fromId, new AS4.Model.Core.PartyId(fromId)),
                            new AS4.Model.Core.Party(toId, new AS4.Model.Core.PartyId(toId)))
                        .WithServiceAction(service, action)
                        .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(pmodeParties, result);
            }

            [Theory]
            [InlineData("service", "action", "from-Id", "to-Id")]
            public async Task ThenPartiesWinsOverServiceActionAsync(
                string service,
                string action,
                string fromId,
                string toId)
            {
                // Arrange
                var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
                var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };

                ReceivePMode pmodeServiceAction = CreatePModeWithActionService(service, action);
                ReceivePMode pmodeParties = CreatePModeWithParties(fromParty, toParty);

                SetupPModes(pmodeServiceAction, pmodeParties);

                MessagingContext messagingContext =
                    new MessageContextBuilder()
                        .WithServiceAction(service, action)
                        .WithParties(
                            new AS4.Model.Core.Party(fromId, new AS4.Model.Core.PartyId(fromId)),
                            new AS4.Model.Core.Party(toId, new AS4.Model.Core.PartyId(toId)))
                        .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                AssertPMode(pmodeParties, result);
            }

            private ReceivePMode CreatePModeWithParties(Party fromParty, Party toParty)
            {
                ReceivePMode pmode = CreateDefaultPMode("default-PMode");
                pmode.MessagePackaging.PartyInfo.FromParty = fromParty;
                pmode.MessagePackaging.PartyInfo.ToParty = toParty;

                return pmode;
            }
        }

        /// <summary>
        /// Testing the step with invalid arguments
        /// </summary>
        public class GivenInvalidArguments : GivenDeterminePModesStepFacts
        {
            [Theory]
            [InlineData("action", "service")]
            public async Task ThenServiceAndActionIsNotEnoughAsync(string action, string service)
            {
                // Arrange
                ArrangePModeThenServiceAndActionIsNotEnough(action, service);

                MessagingContext messagingContext =
                    new MessageContextBuilder().WithServiceAction(service, action).Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                Assert.False(result.Succeeded);
                ErrorResult errorResult = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0010, errorResult.Code);
            }

            private void ArrangePModeThenServiceAndActionIsNotEnough(string action, string service)
            {
                ReceivePMode pmode = CreatePModeWithActionService(service, action);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                DifferentiatePartyInfo(pmode);
                SetupPModes(pmode, new ReceivePMode() { Id = "other id", ReplyHandling = {SendingPMode = "other pmode"}});
            }

            [Theory]
            [InlineData("name", "type")]
            public async Task ThenAgreementRefIsNotEnoughAsync(string name, string type)
            {
                // Arrange
                var agreementRef = new AS4.Model.PMode.AgreementReference { Value = name, Type = type, PModeId = "pmode-id" };
                ArrangePModeThenAgreementRefIsNotEnough(agreementRef);

                MessagingContext messagingContext =
                    new MessageContextBuilder().WithAgreementRef(new AgreementReference(name, type, "pmode-id"))
                                                .WithServiceAction("service", "action")
                                                .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext);

                // Assert
                Assert.False(result.Succeeded);
                ErrorResult errorResult = result.MessagingContext.ErrorResult;
                Assert.Equal(ErrorCode.Ebms0010, errorResult.Code);
            }

            private void ArrangePModeThenAgreementRefIsNotEnough(AS4.Model.PMode.AgreementReference agreementRef)
            {
                ReceivePMode pmode = CreatePModeWithAgreementRef(agreementRef);
                DifferentiatePartyInfo(pmode);
                SetupPModes(pmode, new ReceivePMode());
            }
        }

        protected ReceivePMode CreateDefaultPMode(string id)
        {
            return new ReceivePMode
            {
                Id = id,
                MessagePackaging = new MessagePackaging
                {
                    CollaborationInfo = new CollaborationInfo(),
                    PartyInfo = new PartyInfo()
                },
                ReplyHandling = { SendingPMode = "response_pmode" }
            };
        }

        protected void SetupPModes(params ReceivePMode[] pmodes)
        {
            _mockedConfig.Setup(c => c.GetReceivingPModes()).Returns(pmodes);
        }

        protected ReceivePMode CreatePModeWithAgreementRef(AS4.Model.PMode.AgreementReference agreementRef)
        {
            ReceivePMode pmode = CreateDefaultPMode("defaultPMode");
            pmode.MessagePackaging.CollaborationInfo.AgreementReference = agreementRef;

            return pmode;
        }

        protected ReceivePMode CreatePModeWithActionService(string service, string action)
        {
            ReceivePMode pmode = CreateDefaultPMode("defaultPMode");
            pmode.MessagePackaging.CollaborationInfo.Action = action;
            pmode.MessagePackaging.CollaborationInfo.Service.Value = service;

            return pmode;
        }

        protected void AssertPMode(ReceivePMode expectedPMode, StepResult result)
        {
            Assert.NotNull(expectedPMode);
            Assert.NotNull(result);
            Assert.Equal(expectedPMode, result.MessagingContext.ReceivingPMode);
        }

        private static void DifferentiatePartyInfo(ReceivePMode pmode)
        {
            const string fromId = "from-Id";
            const string toId = "to-Id";

            var fromParty = new Party { Role = fromId, PartyIds = { new PartyId { Id = fromId } } };
            var toParty = new Party { Role = toId, PartyIds = { new PartyId { Id = toId } } };

            pmode.MessagePackaging.PartyInfo = new PartyInfo { FromParty = fromParty, ToParty = toParty };
        }
    }
}