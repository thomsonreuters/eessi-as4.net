using System.ComponentModel;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.Model.PMode
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
        /// </summary>
        public CollaborationInfo()
        {
            AgreementReference = new AgreementReference();
            Service = new Service();            
        }
    }
}