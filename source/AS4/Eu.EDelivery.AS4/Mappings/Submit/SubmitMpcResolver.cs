using System;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / MessageInfo / Mpc
    /// 2. PMode / Message Packaging / Mpc
    /// 3. No mpc attribute
    /// </summary>
    public class SubmitMpcResolver : ISubmitResolver<string>
    {
     
        public static readonly SubmitMpcResolver Default = new SubmitMpcResolver();

        /// <summary>
        /// Resolve the Mpc from the <paramref name="submitMessage"/>
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public string Resolve(SubmitMessage submitMessage)
        {
            if (DoesSubmitMessageTriesToOverridePModeMpc(submitMessage))
            {
                throw new InvalidOperationException($"Submit Message is not allowed by PMode {submitMessage.PMode.Id} to override Mpc");
            }

            if (submitMessage.PMode.AllowOverride && submitMessage.MessageInfo.Mpc != null)
            {
                return submitMessage.MessageInfo.Mpc;
            }

            return submitMessage.PMode.MessagePackaging.Mpc;
        }

        private static bool DoesSubmitMessageTriesToOverridePModeMpc(SubmitMessage submitMessage)
        {
            return
                submitMessage.PMode.AllowOverride == false &&
                !string.IsNullOrEmpty(submitMessage.MessageInfo.Mpc);
        }
    }
}
