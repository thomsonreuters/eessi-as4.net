using System;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Exceptions.Handlers;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Exceptions.Handlers
{
    public class GivenExceptionHandlerProviderFacts
    {
        [Theory]
        [InlineData(AgentType.Submit, typeof(OutboundExceptionHandler))]
        [InlineData(AgentType.Sent, typeof(OutboundExceptionHandler))]
        [InlineData(AgentType.Receive, typeof(InboundExceptionHanlder))]
        [InlineData(AgentType.Deliver, typeof(OutboundExceptionHandler))]
        [InlineData(AgentType.NotifyConsumer, typeof(InboundExceptionHanlder))]
        [InlineData(AgentType.NotifyProducer, typeof(OutboundExceptionHandler))]
        [InlineData(AgentType.PullReceive, typeof(InboundExceptionHanlder))]
        [InlineData(AgentType.Unknown, typeof(EmptyExceptionHandler))]
        public void GetExpectedHanlder(AgentType type, Type expected)
        {
            // Act
            IAgentExceptionHandler actual = ExceptionHandlerRegistry.GetHandler(type);

            // Assert
            Assert.IsType(expected, actual);
        }
    }
}