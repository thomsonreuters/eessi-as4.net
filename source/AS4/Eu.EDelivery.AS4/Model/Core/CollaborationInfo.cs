using System.ComponentModel;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class CollaborationInfo
    {
        [Description("Agreement reference")]
        public AgreementReference AgreementReference { get; set; }

        [Description("Service")]
        public Service Service { get; set; }

        [Description("Action")]
        public string Action { get; set; }

        [Description("Conversation ID")]
        public string ConversationId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a basic <see cref="CollaborationInfo"/> Model
        /// </summary>
        public CollaborationInfo()
        {
            AgreementReference = new AgreementReference();
            Service = new Service();
            Action = Constants.Namespaces.TestAction;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a <see cref="CollaborationInfo"/> Model
        /// with a given <paramref name="conversationId"/> 
        /// and <paramref name="agreementReference"/>
        /// </summary>
        /// <param name="conversationId">
        /// </param>
        /// <param name="agreementReference">
        /// </param>
        public CollaborationInfo(string conversationId, AgreementReference agreementReference)
        {
            this.ConversationId = conversationId;
            this.AgreementReference = agreementReference;
        }
    }
}