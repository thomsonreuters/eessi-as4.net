using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Factories;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Steps.Participant;
using Moq;
using Xunit;
using PMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;

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
            IdentifierFactory.Instance.SetContext(StubConfig.Instance);
            _mockedConfig = new Mock<IConfig>();
            _step = new DeterminePModesStep(_mockedConfig.Object, new FakePModeRuleVisotor());
        }

        /// <summary>
        /// Testing the step with valid arguments
        /// </summary>
        public class GivenValidArguments : GivenDeterminePModesStepFacts
        {
            [Theory]
            [InlineData("01-receive")]
            public async Task ThenPModeIsFoundWithIdAsync(string sharedId)
            {
                // Arrange
                var pmode = new PMode {Id = sharedId};
                SetupPModes(pmode, CreateDefaultPMode());

                MessagingContext messagingContext = new InternalMessageBuilder().WithPModeId(sharedId).Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertPMode(pmode, result);
            }

            [Theory]
            [InlineData("From-Id", "To-Id")]
            public async Task ThenPartyInfoMatchesAsync(string fromId, string toId)
            {
                // Arrange
                var fromParty = new Party(fromId, new PartyId(fromId));
                var toParty = new Party(toId, new PartyId(toId));

                ReceivingProcessingMode pmode = CreatePModeWithParties(fromParty, toParty);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                SetupPModes(pmode, new PMode());

                MessagingContext messagingContext = new InternalMessageBuilder().WithPartys(fromParty, toParty).Build();

                // Act               
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertPMode(pmode, result);
            }

            [Theory]
            [InlineData("service", "action")]
            public async Task ThenPartyInfoNotDefindedAsync(string service, string action)
            {
                // Arrange
                PMode pmode = ArrangePModeThenPartyInfoNotDefined(service, action);

                MessagingContext messagingContext =
                    new InternalMessageBuilder().WithUserMessage(new UserMessage("message-id"))
                                                .WithServiceAction(service, action)
                                                .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertPMode(pmode, result);
            }

            private PMode ArrangePModeThenPartyInfoNotDefined(string service, string action)
            {
                PMode pmode = CreatePModeWithActionService(service, action);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                SetupPModes(pmode, new PMode());

                return pmode;
            }

            [Theory]
            [InlineData("From-Id", "To-Id", "01-receive")]
            public async Task ThenPModeIdWinsOverPartyInfoAsync(string fromId, string toId, string sharedId)
            {
                // Arrange 
                var fromParty = new Party(fromId, new PartyId(fromId));
                var toParty = new Party(toId, new PartyId(toId));
                PMode idPMode = ArrangePModeThenPModeWinsOverPartyInfo(sharedId, fromParty, toParty);

                MessagingContext messagingContext =
                    new InternalMessageBuilder().WithPModeId(sharedId).WithPartys(fromParty, toParty).Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertPMode(idPMode, result);
            }

            private PMode ArrangePModeThenPModeWinsOverPartyInfo(string sharedId, Party fromParty, Party toParty)
            {
                PMode partyInfoPMode = CreatePModeWithParties(fromParty, toParty);
                partyInfoPMode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                var idPMode = new PMode {Id = sharedId};
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
                var fromParty = new Party(fromId, new PartyId(fromId));
                var toParty = new Party(toId, new PartyId(toId));

                PMode pmodeParties = CreatePModeWithParties(fromParty, toParty);
                PMode pmodeServiceAction = CreatePModeWithActionService(service, action);
                SetupPModes(pmodeParties, pmodeServiceAction);

                MessagingContext messagingContext =
                    new InternalMessageBuilder().WithPartys(fromParty, toParty)
                                                .WithServiceAction(service, action)
                                                .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

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
                var fromParty = new Party(fromId, new PartyId(fromId));
                var toParty = new Party(toId, new PartyId(toId));

                PMode pmodeServiceAction = CreatePModeWithActionService(service, action);
                PMode pmodeParties = CreatePModeWithParties(fromParty, toParty);

                SetupPModes(pmodeServiceAction, pmodeParties);

                MessagingContext messagingContext =
                    new InternalMessageBuilder().WithServiceAction(service, action)
                                                .WithPartys(fromParty, toParty)
                                                .Build();

                // Act
                StepResult result = await _step.ExecuteAsync(messagingContext, CancellationToken.None);

                // Assert
                AssertPMode(pmodeParties, result);
            }

            private PMode CreatePModeWithParties(Party fromParty, Party toParty)
            {
                PMode pmode = CreateDefaultPMode();
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
                    new InternalMessageBuilder().WithServiceAction(service, action).Build();

                // Act / Assert
                AS4Exception exception =
                    await Assert.ThrowsAsync<AS4Exception>(
                        () => _step.ExecuteAsync(messagingContext, CancellationToken.None));

                Assert.Equal(ErrorCode.Ebms0001, exception.ErrorCode);
            }

            private void ArrangePModeThenServiceAndActionIsNotEnough(string action, string service)
            {
                PMode pmode = CreatePModeWithActionService(service, action);
                pmode.MessagePackaging.CollaborationInfo.AgreementReference.Value = "not-equal";
                DifferntiatePartyInfo(pmode);
                SetupPModes(pmode, new PMode());
            }

            [Theory]
            [InlineData("name", "type")]
            public async Task ThenAgreementRefIsNotEnoughAsync(string name, string type)
            {
                // Arrange
                var agreementRef = new AgreementReference {Value = name, Type = type};
                ArrangePModeThenAgreementRefIsNotEnough(agreementRef);

                MessagingContext messagingContext =
                    new InternalMessageBuilder().WithAgreementRef(agreementRef)
                                                .WithServiceAction("service", "action")
                                                .Build();

                // Act / Assert
                AS4Exception exception =
                    await Assert.ThrowsAsync<AS4Exception>(
                        () => _step.ExecuteAsync(messagingContext, CancellationToken.None));

                Assert.Equal(ErrorCode.Ebms0001, exception.ErrorCode);
            }

            private void ArrangePModeThenAgreementRefIsNotEnough(AgreementReference agreementRef)
            {
                PMode pmode = CreatePModeWithAgreementRef(agreementRef);
                DifferntiatePartyInfo(pmode);
                SetupPModes(pmode, new PMode());
            }
        }

        protected PMode CreateDefaultPMode()
        {
            return new PMode
            {
                MessagePackaging =
                    new MessagePackaging {CollaborationInfo = new CollaborationInfo(), PartyInfo = new PartyInfo()}
            };
        }

        protected void SetupPModes(params PMode[] pmodes)
        {
            _mockedConfig.Setup(c => c.GetReceivingPModes()).Returns(pmodes);
        }

        protected PMode CreatePModeWithAgreementRef(AgreementReference agreementRef)
        {
            PMode pmode = CreateDefaultPMode();
            pmode.MessagePackaging.CollaborationInfo.AgreementReference = agreementRef;

            return pmode;
        }

        protected PMode CreatePModeWithActionService(string service, string action)
        {
            PMode pmode = CreateDefaultPMode();
            pmode.MessagePackaging.CollaborationInfo.Action = action;
            pmode.MessagePackaging.CollaborationInfo.Service.Value = service;

            return pmode;
        }

        protected void AssertPMode(PMode expectedPMode, StepResult result)
        {
            Assert.NotNull(expectedPMode);
            Assert.NotNull(result);
            Assert.Equal(expectedPMode, result.MessagingContext.ReceivingPMode);
        }

        private static void DifferntiatePartyInfo(PMode pmode)
        {
            const string fromId = "from-Id";
            const string toId = "to-Id";

            var fromParty = new Party(fromId, new PartyId(fromId));
            var toParty = new Party(toId, new PartyId(toId));

            pmode.MessagePackaging.PartyInfo = new PartyInfo {FromParty = fromParty, ToParty = toParty};
        }
    }
}