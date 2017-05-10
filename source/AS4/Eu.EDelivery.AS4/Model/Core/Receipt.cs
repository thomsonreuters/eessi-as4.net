using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Receipt : SignalMessage
    {
        /// <summary>
        /// The UserMessage for which this is a receipt.
        /// </summary>
        /// <remarks>This property should only be populated when the NonRepudiationInformation is not filled out.</remarks>
        public UserMessage UserMessage { get; set; }

        /// <summary>
        /// NonRepudiation information of the UserMessage for which this is a receipt.
        /// </summary>
        /// <remarks>This property is only populated when the UserMessage property is not filled out.</remarks>
        public NonRepudiationInformation NonRepudiationInformation { get; set; }

        public Receipt() {}

        public Receipt(string messageId) : base(messageId) {}

        public override string GetActionValue()
        {
            return "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.receipt";
        }
    }

    public class NonRepudiationInformation
    {
        public ICollection<MessagePartNRInformation> MessagePartNRInformation { get; set; }

        public NonRepudiationInformation()
        {
            this.MessagePartNRInformation = new Collection<MessagePartNRInformation>();
        }
    }

    public class MessagePartNRInformation
    {
        public Reference Reference { get; set; }
    }

    public class Reference
    {
        public ICollection<ReferenceTransform> Transforms { get; set; }
        public ReferenceDigestMethod DigestMethod { get; set; }
        public byte[] DigestValue { get; set; }
        public string URI { get; set; }
    }

    public class ReferenceTransforms
    {
        public ReferenceTransform Transform { get; set; }
    }

    public class ReferenceTransform
    {
        public string Algorithm { get; set; }

        public ReferenceTransform(string algorithm)
        {
            this.Algorithm = algorithm;
        }
    }

    public class ReferenceDigestMethod
    {
        public string Algorithm { get; set; }

        public ReferenceDigestMethod() {}

        public ReferenceDigestMethod(string algorithm)
        {
            this.Algorithm = algorithm;
        }
    }
}