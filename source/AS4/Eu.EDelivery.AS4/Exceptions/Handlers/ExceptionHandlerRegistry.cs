using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    /// <summary>
    /// Registry to defining <see cref="IAgentExceptionHandler"/> implementations based on a given <see cref="AgentType"/>.
    /// </summary>
    internal static class ExceptionHandlerRegistry
    {
        private static readonly IDictionary<AgentType, Func<IAgentExceptionHandler>> Handlers =
            new Dictionary<AgentType, Func<IAgentExceptionHandler>>
            {
                [AgentType.Submit] = () => new OutboundExceptionHandler(),
                [AgentType.Sent] = () => new OutboundExceptionHandler(),
                [AgentType.Receive] = () => new InboundExceptionHanlder(),
                [AgentType.Deliver] = () => new OutboundExceptionHandler(),
                [AgentType.NotifyConsumer] = () => new InboundExceptionHanlder(),
                [AgentType.NotifyProducer] = () => new OutboundExceptionHandler(),
                [AgentType.PullReceive] = () => new InboundExceptionHanlder(),
                [AgentType.Unknown] = () => new EmptyExceptionHandler()
            };

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IAgentExceptionHandler GetHandler(AgentType type)
        {
            return Handlers[type]();
        }
    }
}