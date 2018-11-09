using System;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / CollaborationInfo / ConversationId 
    /// 2. Default Conversation Id
    /// </summary>
    internal static class SubmitConversationIdResolver
    {
        /// <summary>
        /// Resolve the Conversation Id
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public static string ResolveConverstationId(SubmitMessage submitMessage)
        {
            if (submitMessage == null)
            {
                throw new ArgumentNullException(nameof(submitMessage));
            }

            if (!String.IsNullOrEmpty(submitMessage.Collaboration?.ConversationId))
            {
                return submitMessage.Collaboration.ConversationId;
            }

            return CollaborationInfo.DefaultConversationId;
        }
    }
}
