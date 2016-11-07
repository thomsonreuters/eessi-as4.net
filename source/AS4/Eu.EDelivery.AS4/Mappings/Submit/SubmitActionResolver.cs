using Eu.EDelivery.AS4.Exceptions;
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
        /// <summary>
        /// Resolve the Action
        /// </summary>
        /// <param name="submitMessage"></param>
        /// <returns></returns>
        public string Resolve(SubmitMessage submitMessage)
        {
            if (DoesSubmitMessageTriesToOverridePModeValues(submitMessage))
                throw new AS4Exception($"Submit Message is not allowed by PMode {submitMessage.PMode.Id} to override Action");

            if (!string.IsNullOrEmpty(submitMessage.Collaboration.Action))
                return submitMessage.Collaboration.Action;

            if (!string.IsNullOrEmpty(submitMessage.PMode.MessagePackaging.CollaborationInfo?.Action))
                return submitMessage.PMode.MessagePackaging.CollaborationInfo.Action;

            return GetTestAction();
        }

        private string GetTestAction()
        {
            return Constants.Namespaces.TestAction;
        }

        private bool DoesSubmitMessageTriesToOverridePModeValues(SubmitMessage submitMessage)
        {
            return 
                submitMessage.PMode.AllowOverride == false && 
                !string.IsNullOrEmpty(submitMessage.Collaboration.Action) &&
                !string.IsNullOrEmpty(submitMessage.PMode.MessagePackaging?.CollaborationInfo?.Action);
        }
    }
}
