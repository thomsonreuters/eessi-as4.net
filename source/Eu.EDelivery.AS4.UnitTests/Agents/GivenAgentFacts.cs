using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.UnitTests.Model;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Eu.EDelivery.AS4.UnitTests.Transformers;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenAgentFacts
    {
        [Fact]
        public void StopReceiver_IfAgentsStopped()
        {
            // Arrange
            var spyReceiver = Mock.Of<IReceiver>();
            var sut = new Agent(
                new AgentConfig(name: "Agent with Spy Receiver"), 
                spyReceiver, 
                Transformer<StubSubmitTransformer>(), 
                exceptionHandler: null, 
                stepConfiguration: null);

            // Act
            sut.Stop();

            // Assert
            Mock.Get(spyReceiver).Verify(r => r.StopReceiving());
        }

        [Fact]
        public async Task NoStepsAreExecuted_IfNoStepsAreDefined()
        {
            // Arrange
            var spyReceiver = new SpyReceiver();
            var sut = new Agent(
                new AgentConfig("Agent with non-defined Normal Steps"), 
                spyReceiver, 
                Transformer<StubSubmitTransformer>(), 
                exceptionHandler: null, 
                stepConfiguration: new StepConfiguration
                {
                    NormalPipeline = new Step[] {null},
                    ErrorPipeline = null
                });

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Assert.True(spyReceiver.IsCalled);
            Assert.NotNull(spyReceiver.Context.SubmitMessage);
        }

        [Fact]
        public async Task ReceiverGetsExpectedContext_IfHappyPath()
        {
            // Arrange
            var spyReceiver = new SpyReceiver();
            var sut = new Agent(
                new AgentConfig(name: "Agent with Normal Pipeline"), 
                spyReceiver, 
                Transformer<StubSubmitTransformer>(), 
                exceptionHandler: null, 
                stepConfiguration: new StepConfiguration
                {
                    NormalPipeline = AS4MessageSteps(),
                    ErrorPipeline = null
                });

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Assert.True(spyReceiver.IsCalled);
            Assert.Equal(AS4Message.Empty, spyReceiver.Context.AS4Message);
        }

        private static Step[] AS4MessageSteps()
        {
            return new[] {new Step {Type = typeof(StubAS4MessageStep).AssemblyQualifiedName}};
        }

        [Fact]
        public async Task HandlesTransformFailure()
        {
            // Arrange
            var spyHandler = Mock.Of<IAgentExceptionHandler>();
            Agent sut = AgentWithSaboteurTransformer(spyHandler);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Mock.Get(spyHandler)
                .Verify(h => h.HandleTransformationException(It.IsAny<Exception>(), It.IsAny<ReceivedMessage>()), Times.Once);
        }

        private static Agent AgentWithSaboteurTransformer(IAgentExceptionHandler spyHandler)
        {
            return new Agent(
                new AgentConfig(name: "Agent with Saboteur Transformer"), 
                new SpyReceiver(),
                Transformer<DummyTransformer>(),
                spyHandler,
                new StepConfiguration());
        }

        [Fact]
        public async Task HandlesFailureInHappyPath()
        {
            // Arrange
            var spyHandler = Mock.Of<IAgentExceptionHandler>();
            Agent sut = AgentWithHappySaboteurSteps(spyHandler);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Expression<Action<IAgentExceptionHandler>> expression =
                h => h.HandleExecutionException(It.IsAny<Exception>(), It.IsAny<MessagingContext>());

            Mock.Get(spyHandler).Verify(expression, Times.Once);
        }

        private static Agent AgentWithHappySaboteurSteps(IAgentExceptionHandler spyHandler)
        {
            return new Agent(
                new AgentConfig(name: "Agent with Saboteur Steps in Normal Pipeline"), 
                new SpyReceiver(), 
                Transformer<StubSubmitTransformer>(), 
                spyHandler, 
                new StepConfiguration
                {
                    NormalPipeline = Step<SaboteurStep>(),
                    ErrorPipeline = null
                });
        }

        [Fact]
        public async Task HandlesFailureInUnhappyPath()
        {
            // Arrange
            var spyHandler = Mock.Of<IAgentExceptionHandler>();
            Agent sut = AgentWithUnhappySaboteurSteps(spyHandler);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Mock.Get(spyHandler)
                .Verify(h => h.HandleErrorException(It.IsAny<Exception>(), It.IsAny<MessagingContext>()), Times.Once);
        }

        private static Agent AgentWithUnhappySaboteurSteps(IAgentExceptionHandler spyHandler)
        {
            return new Agent(
                new AgentConfig(name: "Agent with Saboteur Steps in Error Pipeline"), 
                new SpyReceiver(), 
                Transformer<StubSubmitTransformer>(), 
                spyHandler, 
                new StepConfiguration
                {
                    NormalPipeline = Step<UnsuccessfulStep>(),
                    ErrorPipeline = Step<SaboteurStep>()
                });
        }

        [Fact]
        public async Task RunsThroughUnhappyPath_IfAnyHappyStepIndicatesUnsuccesful()
        {
            // Arrange
            var spyReceiver = new SpyReceiver();
            Agent sut = AgentWithUnsuccesfulStep(spyReceiver);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Assert.True(spyReceiver.IsCalled);
            Assert.IsType<UnHappyContext>(spyReceiver.Context);
        }

        private static Agent AgentWithUnsuccesfulStep(IReceiver spyReceiver)
        {
            return new Agent(
                new AgentConfig(name: "Agent with Steps that don't succeed succesfully"), 
                spyReceiver, 
                Transformer<StubSubmitTransformer>(), 
                exceptionHandler: Mock.Of<IAgentExceptionHandler>(), 
                stepConfiguration: new StepConfiguration
                {
                    NormalPipeline = Step<UnsuccessfulStep>(),
                    ErrorPipeline = Step<UnhappyStep>()
                });
        }

        private static Transformer Transformer<T>()
        {
            return new Transformer {Type = typeof(T).AssemblyQualifiedName};
        }

        private static Step[] Step<T>()
        {
            return new[] {new Step {Type = typeof(T).AssemblyQualifiedName}};
        }

        public class UnsuccessfulStep : IStep
        {
            /// <summary>
            /// Execute the step for a given <paramref name="messagingContext"/>.
            /// </summary>
            /// <param name="messagingContext">Message used during the step execution.</param>
            /// <returns></returns>
            public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
            {
                var result = new ErrorResult(description: null, alias: default(ErrorAlias));

                return Task.FromResult(StepResult.Failed(new HappyContext {ErrorResult = result}));
            }
        }

        public class UnhappyStep : IStep
        {
            /// <summary>
            /// Execute the step for a given <paramref name="messagingContext"/>.
            /// </summary>
            /// <param name="messagingContext">Message used during the step execution.</param>
            /// <returns></returns>
            public Task<StepResult> ExecuteAsync(MessagingContext messagingContext)
            {
                return Task.FromResult(StepResult.Success(new UnHappyContext()));
            }
        }

        public class HappyContext : EmptyMessagingContext {}

        public class UnHappyContext : EmptyMessagingContext {}
    }
}
