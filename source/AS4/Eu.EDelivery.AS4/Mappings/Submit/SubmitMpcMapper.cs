using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / MessageInfo / Mpc
    /// 2. PMode / Message Packaging / Mpc
    /// 3. No mpc attribute
    /// </summary>
    public class SubmitMpcMapper : ISubmitMapper
    {
        /// <summary>
        /// Map to add the Mpc to the <paramref name="userMessage"/>
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <param name="userMessage"></param>
        public void Map(SubmitMessage submitMessage, UserMessage userMessage)
        {
            if (DoesSubmitMessageTriesToOverridePModeMpc(submitMessage))
                throw new AS4Exception($"Submit Message is not allowed by PMode {submitMessage.PMode.Id} to override Mpc");

            if (submitMessage.PMode.AllowOverride && submitMessage.MessageInfo.Mpc != null)
                userMessage.Mpc = submitMessage.MessageInfo.Mpc;

            else userMessage.Mpc = submitMessage.PMode.MessagePackaging.Mpc;
        }

        private bool DoesSubmitMessageTriesToOverridePModeMpc(SubmitMessage submitMessage)
        {
            return 
                submitMessage.PMode.AllowOverride == false && 
                !string.IsNullOrEmpty(submitMessage.MessageInfo.Mpc) &&
                !string.IsNullOrEmpty(submitMessage.PMode.MessagePackaging.Mpc);
        }
    }
}
