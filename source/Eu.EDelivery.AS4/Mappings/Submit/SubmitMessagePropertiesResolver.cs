using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="MessageProperty"/>
    /// </summary>
    public class SubmitMessagePropertiesResolver : ISubmitResolver<MessageProperty[]>
    {
        public static readonly SubmitMessagePropertiesResolver Default = new SubmitMessagePropertiesResolver();

        /// <summary>
        /// FOR EACH SubmitMessage / MessageProperties and PMode / Message Packaging / MessageProperties4 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public MessageProperty[] Resolve(SubmitMessage message)
        {
            IEnumerable<MessageProperty> RetrieveCoreMessageProperties()
            {
                if (message.MessageProperties != null)
                {
                    foreach (var p in message.MessageProperties)
                    {
                        yield return new MessageProperty(p.Name, p.Value, p.Type);
                    }
                }

                if (message.PMode.MessagePackaging.MessageProperties != null)
                {
                    foreach (var p in message.PMode.MessagePackaging.MessageProperties)
                    {
                        yield return new MessageProperty(p.Name, p.Value, p.Type);
                    }
                }
            }

            return RetrieveCoreMessageProperties().ToArray();
        }
    }
}
