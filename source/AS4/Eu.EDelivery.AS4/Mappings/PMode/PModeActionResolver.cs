using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Mappings.PMode
{
    /// <summary>
    /// Resolve the Action from the <see cref="SendingProcessingMode"/>
    /// </summary>
    public class PModeActionResolver : IPModeResolver<string>
    {
        /// <summary>
        /// Resolve the Action
        /// </summary>
        /// <param name="pmode"></param>
        /// <returns></returns>
        public string Resolve(SendingProcessingMode pmode)
        {
            CollaborationInfo pmodeCollaboration = pmode.MessagePackaging.CollaborationInfo;

            if (!string.IsNullOrEmpty(pmodeCollaboration?.Action))
                return pmodeCollaboration.Action;

            return GetTestAction();
        }

        private string GetTestAction()
        {
            return Constants.Namespaces.TestAction;
        }
    }
}
