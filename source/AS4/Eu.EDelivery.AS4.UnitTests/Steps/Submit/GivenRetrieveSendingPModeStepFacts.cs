using System;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Model.Submit;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Moq;
using Xunit;
using CollaborationInfo = Eu.EDelivery.AS4.Model.Common.CollaborationInfo;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="RetrieveSendingPModeStep" />
    /// </summary>
    public class GivenRetrieveSendingPModeStepFacts
    {
        private Mock<IConfig> _mockedConfig;
        private readonly string _pmodeId;
        private RetrieveSendingPModeStep _step;

        public GivenRetrieveSendingPModeStepFacts()
        {
            this._step = new RetrieveSendingPModeStep(StubConfig.Instance);
            this._pmodeId = "01-pmode";
            this._mockedConfig = new Mock<IConfig>();
            this._mockedConfig
                .Setup(c => c.GetSendingPMode(It.IsAny<string>()))
                .Returns(GetStubWrongProcessingMode());
        }

        private SendingProcessingMode GetStubWrongProcessingMode()
        {
            Console.WriteLine(Thread.CurrentThread.ManagedThreadId);
            return new SendingProcessingMode
            {
                Id = this._pmodeId
            };
        }

        private SubmitMessage GetStubSubmitMessage()
        {
            return new SubmitMessage
            {
                Collaboration = new CollaborationInfo
                {
                    AgreementRef = new Agreement
                    {
                        PModeId = this._pmodeId
                    }
                }
            };
        }

        /// <summary>
        /// Testing if the Step fails
        /// for the "Execute" Method
        /// </summary>
        public class GivenInalidArgumentsForExecute : GivenRetrieveSendingPModeStepFacts
        {
            [Fact]
            public async Task ThenExecuteMethodRetrievesPModeFailsWithInvalidPModeAsync()
            {
                // Arrange
                var internalMessage = new InternalMessage(base.GetStubSubmitMessage());
                base._step = new RetrieveSendingPModeStep(base._mockedConfig.Object);
                // Act / Assert
                await
                    Assert.ThrowsAsync<AS4Exception>(
                        () => base._step.ExecuteAsync(internalMessage, CancellationToken.None));
            }

            [Fact]
            public async Task ThenExecuteRetrievesPModeSucceedsWithTwoStepsAsync()
            {
                // Arrange
                var internalMessage = new InternalMessage(base.GetStubSubmitMessage());
                base._mockedConfig = new Mock<IConfig>();
                base._mockedConfig
                    .Setup(c => c.GetSendingPMode(It.IsAny<string>()))
                    .Returns(new ValidStubSendingPMode(_pmodeId));
                // Act
                var step1 = new RetrieveSendingPModeStep(base._mockedConfig.Object);
                var step2 = new RetrieveSendingPModeStep(base._mockedConfig.Object);

                StepResult result1 = null, result2 = null;
                await Task.Run(async () => result1 = await step1.ExecuteAsync(internalMessage, CancellationToken.None));
                await Task.Run(async() => result2 = await step2.ExecuteAsync(internalMessage, CancellationToken.None));

                // Assert
                Assert.NotNull(result1);
                Assert.NotNull(result2);
                Assert.Equal(result1.InternalMessage.SendingPModeString, result2.InternalMessage.SendingPModeString);
            }
        }
    }
}