using System;
using System.Collections.Generic;
using System.Configuration;
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
                [AgentType.PushSend] = () => new OutboundExceptionHandler(),
                [AgentType.OutboundProcessing] = () => new OutboundExceptionHandler(),
                [AgentType.PushSend] = () => new OutboundExceptionHandler(),
                [AgentType.Receive] = () => new InboundExceptionHandler(),
                [AgentType.Deliver] = () => new InboundExceptionHandler(),
                [AgentType.Forward] = () => new InboundExceptionHandler(),
                [AgentType.Notify] = () => new NotifyExceptionHandler(),
                [AgentType.PullReceive] = () => new InboundExceptionHandler(),
                [AgentType.PullSend] = () => new PullSendAgentExceptionHandler(),
                [AgentType.ReceptionAwareness] = () => new LogExceptionHandler()
            };

        /// <summary>
        /// Gets the handler.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns></returns>
        public static IAgentExceptionHandler GetHandler(AgentType type)
        {
            if (Handlers.ContainsKey(type) == false)
            {
                throw new ConfigurationErrorsException($"There is no Exception Handler defined for Agents of type '{type}'");
            }

            return new SafeExceptionHandler(Handlers[type]());
        }
    }
}