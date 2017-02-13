using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using NLog;

namespace Eu.EDelivery.AS4.Steps.Send
{
    /// <summary>
    /// Minder send <see cref="AS4Message" /> to the corresponding Receiving MSH,
    /// with no exception handling, just catching
    /// </summary>
    [Obsolete("We should not use this step anymore.")]
    public class MinderSendAS4MessageStep : SendAS4MessageStep
    {
        protected override StepResult HandleSendAS4Exception(InternalMessage internalMessage, Exception exception)
        {
            // [CONFORMANCE TESTING] Don't rethrow exception to not have an endless loop of retrying
            // Reason: Minder Interceptor doesn't always return a valid AS4 Message
            // Do some extra logging instead.
            var logger = LogManager.GetCurrentClassLogger();

            logger.Error($"{internalMessage.Prefix} An error occured while sending the Message to a Minder endpoint.");
            logger.Error(exception.Message);
            if (exception.InnerException != null)
            {
                logger.Error(exception.InnerException.Message);
            }

            return StepResult.Failed(CreateFailedSendAS4Exception(internalMessage, exception));
        }
    }
}