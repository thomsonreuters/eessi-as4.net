using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Agents;

namespace Eu.EDelivery.AS4.Exceptions.Handlers
{
    public class ExceptionHandlerProvider
    {
        private readonly IDictionary<AgentType, Func<IAgentExceptionHandler>> _handlers =
            new Dictionary<AgentType, Func<IAgentExceptionHandler>>
            {
                [AgentType.Submit] = () => new OutboundExceptionHandler(),
                [AgentType.Sent] = () => new OutboundExceptionHandler(),
                [AgentType.Receive] = () => new InboundExceptionHanlder(),
                [AgentType.Deliver] = () => new OutboundExceptionHandler(),
                [AgentType.Notify] = () => new OutboundExceptionHandler(),
                [AgentType.PullReceive] = () => new InboundExceptionHanlder(),
                [AgentType.Unknown] = () => new EmptyExceptionHandler()
            };

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public IAgentExceptionHandler GetHandler(AgentType type)
        {
            return _handlers[type]();
        }
    }
}