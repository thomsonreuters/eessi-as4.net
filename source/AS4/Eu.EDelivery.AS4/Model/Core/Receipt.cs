using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CryptoReference = System.Security.Cryptography.Xml.Reference;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Receipt : SignalMessage
    {
        /// <summary>
        /// The UserMessage for which this is a receipt.
        /// </summary>
        /// <value>The user message.</value>
        /// <remarks>This property should only be populated when the NonRepudiationInformation is not filled out.</remarks>
        public UserMessage UserMessage { get; set; }

        /// <summary>
        /// NonRepudiation information of the UserMessage for which this is a receipt.
        /// </summary>
        /// <value>The non repudiation information.</value>
        /// <remarks>This property is only populated when the UserMessage property is not filled out.</remarks>
        public NonRepudiationInformation NonRepudiationInformation { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        public Receipt() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="Receipt"/> class.
        /// </summary>
        /// <param name="messageId">The message identifier.</param>
        public Receipt(string messageId) : base(messageId) {}

        /// <summary>
        /// Gets the action value.
        /// </summary>
        /// <returns></returns>
        public override string GetActionValue()
        {
            return "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.receipt";
        }

        /// <summary>
        /// Verifies the Non-Repudiation Information of the <see cref="Receipt"/> against the NRI of the related <see cref="Core.UserMessage"/>.
        /// </summary>
        /// <param name="userMessage">The related <see cref="Core.UserMessage"/>.</param>
        /// <returns></returns>
        public bool VerifyNonRepudiationInfo(AS4Message userMessage)
        {
            IEnumerable<CryptoReference> userReferences = 
                userMessage.SecurityHeader.GetReferences().Cast<CryptoReference>();

            return userReferences.Any()
                   && userReferences.Select(IsNonRepudiationHashEqualToUserReferenceHash).All(r => r);
        }

        private bool IsNonRepudiationHashEqualToUserReferenceHash(CryptoReference r)
        {
            byte[] repudiationHash = GetNonRepudiationHashForUri(r.Uri);
            return repudiationHash != null && r.DigestValue?.SequenceEqual(repudiationHash) == true;
        }

        private byte[] GetNonRepudiationHashForUri(string userMessageReferenceUri)
        {
            return NonRepudiationInformation.MessagePartNRInformation
                .Where(i => i.Reference.URI.Equals(userMessageReferenceUri))
                .Select(i => i.Reference.DigestValue)
                .FirstOrDefault();
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