namespace Eu.EDelivery.AS4.Model.Core
{
    public class CollaborationInfo
    {
        public AgreementReference AgreementReference { get; set; }
        public Service Service { get; set; }

        public string Action { get; set; }
        public string ConversationId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CollaborationInfo"/> class. 
        /// Create a basic <see cref="CollaborationInfo"/> Model
        /// </summary>
        public CollaborationInfo()
        {
            this.AgreementReference = new AgreementReference();
            this.Service = new Service();
            this.Action = "http://docs.oasis-open.org/ebxml-msg/ebMS/v3.0/ns/core/200704/test";
            this.ConversationId = "1";
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