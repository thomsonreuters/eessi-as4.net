using System;
using System.Collections.Generic;
using System.Linq;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Mappings.Submit
{
    /// <summary>
    /// Resolve the <see cref="MessageProperty"/>
    /// </summary>
    internal static class SubmitMessagePropertiesResolver
    {
        /// <summary>
        /// FOR EACH SubmitMessage / MessageProperties and PMode / Message Packaging / MessageProperties4 
        /// </summary>
        /// <param name="message"></param>
        /// <returns></returns>
        public static MessageProperty[] Resolve(SubmitMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            if (message.PMode == null)
            {
                throw new ArgumentNullException(nameof(message.PMode));
            }

            IEnumerable<MessageProperty> RetrieveCoreMessageProperties()
            {
                if (message.MessageProperties != null)
                {
                    foreach (Model.Common.MessageProperty p in message.MessageProperties)
                    {
                        yield return new MessageProperty(p?.Name, p?.Value, p?.Type);
                    }
                }

                if (message.PMode.MessagePackaging?.MessageProperties != null)
                {
                    foreach (Model.PMode.MessageProperty p in message.PMode.MessagePackaging.MessageProperties)
                    {
                        yield return new MessageProperty(p?.Name, p?.Value, p?.Type);
                    }
                }
            }

            return RetrieveCoreMessageProperties().ToArray();
        }
    }
}
