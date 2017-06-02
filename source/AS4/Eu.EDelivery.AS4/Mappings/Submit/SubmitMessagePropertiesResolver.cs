using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Submit;
using CoreMessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;
using SubmitMessageProperty = Eu.EDelivery.AS4.Model.Common.MessageProperty;


namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="Model.Core.MessageProperty"/>
    /// </summary>
    public class SubmitMessagePropertiesResolver : ISubmitResolver<CoreMessageProperty[]>
    {
        /// <summary>
        /// FOR EACH SubmitMessage / MessageProperties and PMode / Message Packaging / MessageProperties4 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public CoreMessageProperty[] Resolve(SubmitMessage message)
        {
            var returnProperties = new List<CoreMessageProperty>();

            RetrieveCoreMessageProperties(message, returnProperties);

            return returnProperties.ToArray();
        }

        private static void RetrieveCoreMessageProperties(SubmitMessage message, List<CoreMessageProperty> returnProperties)
        {
            if (message.MessageProperties != null)
                MoveSubmitPropertiesToCoreProperties(returnProperties, message.MessageProperties);

            if (message.PMode.MessagePackaging.MessageProperties != null)
                returnProperties.AddRange(message.PMode.MessagePackaging.MessageProperties);
        }

        private static void MoveSubmitPropertiesToCoreProperties(
            ICollection<CoreMessageProperty> returnProperties, IEnumerable<SubmitMessageProperty> submitProperties)
        {
            foreach (SubmitMessageProperty current in submitProperties)
                MoveSubmitPropertyToCoreProperty(returnProperties, current);
        }

        private static void MoveSubmitPropertyToCoreProperty(
            ICollection<CoreMessageProperty> returnProperties, SubmitMessageProperty current)
        {
            var messageProperty = new CoreMessageProperty(current.Name, current.Value);

            returnProperties.Add(messageProperty);
        }
    }
}
