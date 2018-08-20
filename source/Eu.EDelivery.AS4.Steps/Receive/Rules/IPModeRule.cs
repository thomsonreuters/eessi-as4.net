using Eu.EDelivery.AS4.Model.Core;
using PMode = Eu.EDelivery.AS4.Model.PMode.ReceivingProcessingMode;

namespace Eu.EDelivery.AS4.Steps.Receive.Rules
{
    internal interface IPModeRule
    {
        /// <summary>
        /// Determine the points for the given Receiving PMode and UserMessage
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="userMessage"></param>
        /// <returns></returns>
        int DeterminePoints(PMode pmode, UserMessage userMessage);
    }
}