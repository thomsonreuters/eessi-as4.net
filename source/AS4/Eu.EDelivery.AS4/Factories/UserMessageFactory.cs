using System;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Factories
{
    /// <summary>
    /// Factory to create <see cref="UserMessage"/> Models
    /// </summary>
    public class UserMessageFactory
    {

        public static readonly UserMessageFactory Instance = new UserMessageFactory();

        /// <summary>
        /// Create default <see cref="UserMessage"/>
        /// </summary>
        /// <returns></returns>
        public UserMessage Create(SendingProcessingMode pmode)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            var result = new UserMessage
            {
                Sender = PModeSenderResolver.Default.Resolve(pmode),
                Receiver = PModeReceiverResolver.Default.Resolve(pmode),
                CollaborationInfo = ResolveCollaborationInfo(pmode)
            };

            if (pmode.MessagePackaging?.MessageProperties != null)
            {
                foreach (var messageProperty in pmode.MessagePackaging?.MessageProperties)
                {
                    result.MessageProperties.Add(messageProperty);
                }
            }

            return result;
        }

        private static CollaborationInfo ResolveCollaborationInfo(SendingProcessingMode pmode)
        {
            return new CollaborationInfo()
            {
                Action = new PModeActionResolver().Resolve(pmode),
                AgreementReference = new PModeAgreementRefResolver().Resolve(pmode),
                Service = new PModeServiceResolver().Resolve(pmode)
            };
        }
    }
}
