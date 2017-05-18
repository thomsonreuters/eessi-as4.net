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
        private static readonly UserMessageFactory Signalton = new UserMessageFactory();

        public static readonly UserMessageFactory Instance = Signalton;

        /// <summary>
        /// Create default <see cref="UserMessage"/>
        /// </summary>
        /// <returns></returns>
        public UserMessage Create(SendingProcessingMode pmode)
        {
            var result = new UserMessage
            {
                Sender = new PModeSenderResolver().Resolve(pmode),
                Receiver = new PModeReceiverResolver().Resolve(pmode),
                CollaborationInfo = ResolveCollaborationInfo(pmode)                
            };

            pmode.MessagePackaging.MessageProperties.ForEach(p => result.MessageProperties.Add(p));
            
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
