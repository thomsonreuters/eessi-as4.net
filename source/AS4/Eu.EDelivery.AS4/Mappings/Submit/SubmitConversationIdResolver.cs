using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / CollaborationInfo / ConversationId 
    /// 2. Default Conversation Id
    /// </summary>
    public class SubmitConversationIdResolver : ISubmitResolver<string>
    {
        public static readonly SubmitConversationIdResolver Default = new SubmitConversationIdResolver();

        /// <summary>
        /// Resolve the Conversation Id
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public string Resolve(SubmitMessage submitMessage)
        {
            const string defaultConversationId = "1";

            if (!string.IsNullOrEmpty(submitMessage.Collaboration.ConversationId))
            {
                return submitMessage.Collaboration.ConversationId;
            }

            return defaultConversationId;
        }
    }
}
