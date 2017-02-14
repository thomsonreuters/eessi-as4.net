using System;
using Eu.EDelivery.AS4.Exceptions;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// 1. SubmitMessage / CollaborationInfo / Action
    /// 2. PMode / Message Packaging / CollaborationInfo / Action 
    /// 3. Default Test Action Namespace
    /// </summary>
    public class SubmitActionResolver : ISubmitResolver<string>
    {
        private readonly IPModeResolver<string> _pmodeResolver;

        public static readonly SubmitActionResolver Default = new SubmitActionResolver();

        /// <summary>
        /// Initializes a new instance of the <see cref="SubmitActionResolver"/> class
        /// </summary>
        public SubmitActionResolver()
        {
            this._pmodeResolver = new PModeActionResolver();
        }

        /// <summary>
        /// Resolve the Action
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public string Resolve(SubmitMessage submitMessage)
        {
            if (submitMessage.PMode.AllowOverride == false && DoesSubmitMessageTriesToOverridePModeValues(submitMessage))
            {
                throw new AS4Exception($"Submit Message is not allowed by PMode {submitMessage.PMode.Id} to override Action");
            }

            if (!string.IsNullOrEmpty(submitMessage.Collaboration.Action))
            {
                return submitMessage.Collaboration.Action;
            }

            return this._pmodeResolver.Resolve(submitMessage.PMode);
        }

        private static bool DoesSubmitMessageTriesToOverridePModeValues(SubmitMessage submitMessage)
        {

            return !string.IsNullOrEmpty(submitMessage.Collaboration.Action) &&
                   !string.IsNullOrEmpty(submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action) &&
                   !StringComparer.OrdinalIgnoreCase.Equals(submitMessage.Collaboration.Action,
                                                            submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action);
        }
    }
}
