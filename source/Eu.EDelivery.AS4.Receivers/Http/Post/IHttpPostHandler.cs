using Eu.EDelivery.AS4.Model.Internal;

namespace Eu.EDelivery.AS4.Receivers.Http.Post
{
    internal interface IHttpPostHandler
    {
        /// <summary>
        /// Determines if the resulted context can be handled by this instance.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool CanHandle(MessagingContext context);

        /// <summary>
        /// Handles the resulted context.
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        HttpResult Handle(MessagingContext context);
    }
}