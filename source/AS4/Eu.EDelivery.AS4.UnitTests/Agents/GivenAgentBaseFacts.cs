using System;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Receivers;
using Eu.EDelivery.AS4.UnitTests.Steps;
using Eu.EDelivery.AS4.UnitTests.Transformers;
using Moq;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Agents
{
    public class GivenAgentBaseFacts
    {
        [Fact]
        public async Task ReceiverGetsExpectedContext_IfHappyPath()
        {
            // Arrange
            var spyReceiver = new SpyReceiver();
            AgentBase sut = CreateHappyAgent(spyReceiver);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Assert.True(spyReceiver.IsCalled);
            spyReceiver.AssertReceiverResult(context => Assert.Equal(AS4Message.Empty, context.AS4Message));
        }

        private static AgentBase CreateHappyAgent(SpyReceiver spyReceiver)
        {
            return new AgentBase(null, spyReceiver, SubmitTransformer(), null, AS4MessageStep());
        }

        private static AS4.Model.Internal.Steps AS4MessageStep()
        {
            return new AS4.Model.Internal.Steps
            {
                Step = new[] {new Step {Type = typeof(StubAS4MessageStep).AssemblyQualifiedName}}
            };
        }

        [Fact]
        public async Task HandlesTransformFailure()
        {
            // Arrange
            var spyHandler = Mock.Of<IAgentExceptionHandler>();
            AgentBase sut = AgentWithSaboteurTransformer(spyHandler);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Mock.Get(spyHandler).Verify(h => h.HandleTransformationException(It.IsAny<Exception>()), Times.Once);
        }

        private static AgentBase AgentWithSaboteurTransformer(IAgentExceptionHandler spyHandler)
        {
            return new AgentBase(null, new SpyReceiver(), SaboteurTransformer(), spyHandler, null);
        }

        private static Transformer SaboteurTransformer()
        {
            return new Transformer {Type = typeof(DummyTransformer).AssemblyQualifiedName};
        }

        [Fact]
        public async Task HandlesFailureInHappyPath()
        {
            // Arrange
            var spyHandler = Mock.Of<IAgentExceptionHandler>();
            AgentBase sut = AgentWithSabtoteurSteps(spyHandler);

            // Act
            await sut.Start(CancellationToken.None);

            // Assert
            Expression<Action<IAgentExceptionHandler>> expression =
                h => h.HandleExecutionException(It.IsAny<Exception>(), It.IsAny<MessagingContext>());

            Mock.Get(spyHandler).Verify(expression, Times.Once);
        }

        private static AgentBase AgentWithSabtoteurSteps(IAgentExceptionHandler spyHandler)
        {
            return new AgentBase(null, new SpyReceiver(), SubmitTransformer(), spyHandler, SaboteurSteps());
        }

        private static Transformer SubmitTransformer()
        {
            return new Transformer {Type = typeof(StubSubmitTransformer).AssemblyQualifiedName};
        }

        private static AS4.Model.Internal.Steps SaboteurSteps()
        {
            return new AS4.Model.Internal.Steps
            {
                Step = new[] {new Step {Type = typeof(SaboteurStep).AssemblyQualifiedName}}
            };
        }
    }
}
