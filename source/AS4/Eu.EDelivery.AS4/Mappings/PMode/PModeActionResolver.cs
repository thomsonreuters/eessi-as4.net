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
            if (!string.IsNullOrEmpty(pmode.MessagePackaging.CollaborationInfo?.Action))
                return pmode.MessagePackaging.CollaborationInfo.Action;

            return GetTestAction();
        }

        private string GetTestAction()
        {
            return Constants.Namespaces.TestAction;
        }
    }
}
