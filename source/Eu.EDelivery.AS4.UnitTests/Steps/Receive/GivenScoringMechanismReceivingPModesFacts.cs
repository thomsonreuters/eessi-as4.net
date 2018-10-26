using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Mappings.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Moq;
using Xunit;
using AgreementReference = Eu.EDelivery.AS4.Model.Core.AgreementReference;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Core.CollaborationInfo;
using Service = Eu.EDelivery.AS4.Model.Core.Service;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;
using PModeCollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using PModeService = Eu.EDelivery.AS4.Model.PMode.Service;
using PModeAgreementRef = Eu.EDelivery.AS4.Model.PMode.AgreementReference;
using PModePartyInfo = Eu.EDelivery.AS4.Model.PMode.PartyInfo;
using PModeParty = Eu.EDelivery.AS4.Model.PMode.Party;
using PModePartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;


namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenScoringMechanismReceivingPModesFacts
    {
        [Fact]
        public async Task Determine_Based_On_PMode_Id_Above_PartyInfo()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new AgreementReference(
                        $"user-agreement-value-{Guid.NewGuid()}", 
                        $"user-agreement-pmodeid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByServiceAction =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByPartyInfo);

            ReceivingProcessingMode pmodeById = 
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByPModeId);

            // Act
            ReceivingProcessingMode actual = 
                await ExerciseScoringSystemAsync(userMessage, pmodeById, pmodeByServiceAction);

            // Assert
            Assert.Equal(pmodeById, actual);
        }

        [Fact]
        public async Task Determine_Based_On_PartyInfo_Above_Unspecified_PartyInfo()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new Party("user-Sender-role", new PartyId($"user-sender-partyid-{Guid.NewGuid()}")),
                new Party("user-Receiver-role", new PartyId($"user-receiver-partyid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByPartyInfo =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByPartyInfo | PModeMatch.ByServiceAction);

            ReceivingProcessingMode pmodeByUnspecifiedPartyInfo =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByUnspecifiedPartyInfo | PModeMatch.ByServiceAction);

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(userMessage, pmodeByUnspecifiedPartyInfo, pmodeByPartyInfo);

            // Assert
            Assert.Equal(pmodeByPartyInfo, actual);
        }

        [Fact]
        public async Task Determine_Based_On_User_PartyInfo_Containing_Any_PMode_PartyIds()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new Party("user-Sender-role", new PartyId($"user-sender-partyid-{Guid.NewGuid()}")),
                new Party("user-Receiver-role", new PartyId($"user-receiver-partyid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByPartyInfo =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByPartyInfo | PModeMatch.ByServiceAction);

            pmodeByPartyInfo.MessagePackaging.PartyInfo.FromParty.PartyIds.Add(new PModePartyId($"another-pmode-partyid-{Guid.NewGuid()}"));
            pmodeByPartyInfo.MessagePackaging.PartyInfo.ToParty.PartyIds.Add(new PModePartyId($"another-pmode-partyid-{Guid.NewGuid()}"));

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(userMessage, pmodeByPartyInfo);

            // Assert
            Assert.Equal(pmodeByPartyInfo, actual);
        }

        [Fact]
        public async Task Determine_Based_On_PMode_PartyInfo_Containing_All_User_PartyIds()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new Party("user-Sender-role", new PartyId($"user-sender-partyid-{Guid.NewGuid()}")),
                new Party("user-Receiver-role", new PartyId($"user-receiver-partyid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByPartyInfo =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByPartyInfo | PModeMatch.ByServiceAction);

            IEnumerable<PartyId> extraSenderPartyIds =
                userMessage.Sender.PartyIds.Concat(new[] { new PartyId($"another-user-partyid-{Guid.NewGuid()}") });
            IEnumerable<PartyId> extraReceiverPartyIds =
                userMessage.Receiver.PartyIds.Concat(new[] { new PartyId($"another-user-partyid-{Guid.NewGuid()}") });

            var extraPartyIdUserMessage = new UserMessage(
                userMessage.MessageId,
                new Party(userMessage.Sender.Role, extraSenderPartyIds),
                new Party(userMessage.Receiver.Role, extraReceiverPartyIds));

            ReceivingProcessingMode pmodeByExtraPartyIdsPartyInfo =
                CreateMatchedReceivingPMode(extraPartyIdUserMessage, PModeMatch.ByPartyInfo | PModeMatch.ByServiceAction);

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(extraPartyIdUserMessage, pmodeByPartyInfo, pmodeByExtraPartyIdsPartyInfo);

            // Assert
            Assert.Equal(pmodeByExtraPartyIdsPartyInfo, actual);
        }

        [Fact]
        public async Task Determine_Based_On_Unspecified_PartyInfo_Above_AgreementRef()
        {
            // Arrange
            var userMesasge = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new AgreementReference(
                        $"user-agreement-value-{Guid.NewGuid()}",
                        $"user-agreement-type-{Guid.NewGuid()}",
                        $"user-agreement-pmodeid-{Guid.NewGuid()}")),
                new Party("user-Sender-role", new PartyId($"user-sender-partyid-{Guid.NewGuid()}")),
                new Party("user-Receiver-role", new PartyId($"user-receiver-partyid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByUnspecifiedPartyInfo =
                CreateMatchedReceivingPMode(userMesasge, PModeMatch.ByUnspecifiedPartyInfo | PModeMatch.ByAgreementRef);

            ReceivingProcessingMode pmodeByAgreementRef =
                CreateMatchedReceivingPMode(userMesasge, PModeMatch.ByAgreementRef);

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(userMesasge, pmodeByUnspecifiedPartyInfo, pmodeByAgreementRef);

            // Assert
            Assert.Equal(pmodeByUnspecifiedPartyInfo, actual);
        }

        [Fact]
        public async Task Determine_Based_On_AgreementRef_Above_Service_Action()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new AgreementReference(
                        $"user-agreement-value-{Guid.NewGuid()}",
                        $"user-agreement-type-{Guid.NewGuid()}",
                        $"user-agreement-pmodeid-{Guid.NewGuid()}")));

            ReceivingProcessingMode pmodeByServiceAction =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByServiceAction | PModeMatch.ByUnspecifiedPartyInfo);

            ReceivingProcessingMode pmodeByAgreementRef =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByAgreementRef | PModeMatch.ByUnspecifiedPartyInfo);

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(userMessage, pmodeByAgreementRef, pmodeByServiceAction);

            // Assert
            Assert.Equal(pmodeByAgreementRef, actual);
        }

        [Fact]
        public async Task Determine_Based_On_Service_Action()
        {
            // Arrange
            var userMessage = new UserMessage(
                $"user-{Guid.NewGuid()}",
                new CollaborationInfo(
                    new Service(
                        $"user-service-value-{Guid.NewGuid()}",
                        $"user-service-type-{Guid.NewGuid()}"),
                    $"user-action-{Guid.NewGuid()}"));

            ReceivingProcessingMode pmodeByUnspecifiedPartyInfo =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByUnspecifiedPartyInfo);

            ReceivingProcessingMode pmodeByServiceAction =
                CreateMatchedReceivingPMode(userMessage, PModeMatch.ByServiceAction | PModeMatch.ByUnspecifiedPartyInfo);

            // Act
            ReceivingProcessingMode actual =
                await ExerciseScoringSystemAsync(userMessage, pmodeByServiceAction, pmodeByUnspecifiedPartyInfo);

            // Assert
            Assert.Equal(pmodeByServiceAction, actual);
        }

        [Flags]
        private enum PModeMatch
        {
            ByServiceAction = 1,
            ByAgreementRef = 2,
            ByUnspecifiedPartyInfo = 4,
            ByPartyInfo = 8,
            ByPModeId = 16
        }

        private static ReceivingProcessingMode CreateMatchedReceivingPMode(UserMessage um, PModeMatch match)
        {
            return new ReceivingProcessingMode
            {
                Id = um.CollaborationInfo
                       .AgreementReference
                       .Where(_ => (match & PModeMatch.ByPModeId) == PModeMatch.ByPModeId)
                       .SelectMany(a => a.PModeId)
                       .GetOrElse($"pmodeid-{Guid.NewGuid()}"),
                MessagePackaging =
                {
                    CollaborationInfo = CreateMatchedCollaborationInfo(um, match),
                    PartyInfo = CreateMatchedPartyInfo(um,match)
                }
            };
        }

        private static PModeCollaborationInfo CreateMatchedCollaborationInfo(UserMessage um, PModeMatch match)
        {
            return new PModeCollaborationInfo
            {
                Action = (match & PModeMatch.ByServiceAction) == PModeMatch.ByServiceAction
                    ? um.CollaborationInfo.Action
                    : $"pmode-action-{Guid.NewGuid()}",
                Service = (match & PModeMatch.ByServiceAction) == PModeMatch.ByServiceAction
                    ? new PModeService
                    {
                        Type = um.CollaborationInfo.Service.Type.GetOrElse(
                            $"pmode-service-type-{Guid.NewGuid()}"),
                        Value = um.CollaborationInfo.Service.Value
                    }
                    : new PModeService
                    {
                        Type = $"pmode-service-type-{Guid.NewGuid()}",
                        Value = $"pmode-service-value-{Guid.NewGuid()}"
                    },
                AgreementReference = (match & PModeMatch.ByAgreementRef) == PModeMatch.ByAgreementRef
                    ? new PModeAgreementRef
                    {
                        Value = um.CollaborationInfo
                                  .AgreementReference
                                  .Select(a => a.Value)
                                  .GetOrElse($"pmode-agreement-value-{Guid.NewGuid()}"),
                        PModeId = um.CollaborationInfo
                                    .AgreementReference
                                    .SelectMany(a => a.PModeId)
                                    .GetOrElse($"pmode-agreement-pmodeid-{Guid.NewGuid()}"),
                        Type = um.CollaborationInfo
                                 .AgreementReference
                                 .SelectMany(a => a.Type)
                                 .GetOrElse($"pmode-agreement-type-{Guid.NewGuid()}")
                    }
                    : new PModeAgreementRef
                    {
                        Value = $"pmode-agreement-value-{Guid.NewGuid()}",
                        Type = $"pmode-agreement-type-{Guid.NewGuid()}",
                        PModeId = $"pmode-agreement-pmodeid-{Guid.NewGuid()}"
                    }
            };
        }

        private static PModePartyInfo CreateMatchedPartyInfo(UserMessage um, PModeMatch match)
        {
            if ((match & PModeMatch.ByPartyInfo) == PModeMatch.ByPartyInfo)
            {
                return new PModePartyInfo
                {
                    FromParty = new PModeParty(um.Sender.Role, um.Sender.PartyIds.Select(id => new PModePartyId(id.Id))),
                    ToParty = new PModeParty(um.Receiver.Role, um.Receiver.PartyIds.Select(id => new PModePartyId(id.Id)))
                };
            }

            if ((match & PModeMatch.ByUnspecifiedPartyInfo) == PModeMatch.ByUnspecifiedPartyInfo)
            {
                return null;
            }

            return new PModePartyInfo
            {
                FromParty = new PModeParty("pmode-Sender-role", $"pmode-frompartyid-{Guid.NewGuid()}"),
                ToParty = new PModeParty("pmode-Receiver-role", $"pmode-topartyid-{Guid.NewGuid()}")
            };
        }

        private static async Task<ReceivingProcessingMode> ExerciseScoringSystemAsync(
            UserMessage um,
            params ReceivingProcessingMode[] availablePModes)
        {
            var stub = new Mock<IConfig>();
            stub.Setup(c => c.GetReceivingPModes())
                 .Returns(availablePModes);

            var sut = new DeterminePModesStep(stub.Object, createContext: () => null);

            var ctx = new MessagingContext(AS4Message.Create(um), MessagingContextMode.Receive);
            StepResult result = await sut.ExecuteAsync(ctx);

            string exception =
                result.MessagingContext?.Exception != null
                    ? $"Exception: {result.MessagingContext.Exception.Message}"
                    : null;

            string error =
                result.MessagingContext?.ErrorResult != null
                    ? $"Error: {result.MessagingContext.ErrorResult.Description}"
                    : null;

            Assert.True(
                result.MessagingContext?.ReceivingPMode != null, 
                $"Step result's ReceivingPMode != null, {String.Join(", ", exception, error)}");

            return result.MessagingContext?.ReceivingPMode;
        }
    }
}