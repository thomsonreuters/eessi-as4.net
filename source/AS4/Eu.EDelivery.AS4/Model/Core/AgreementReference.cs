using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class AgreementReference : IEquatable<AgreementReference>
    {
        public string Value { get; set; }
        public string Type { get; set; }
        public string PModeId { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// Create an empty Agreement Reference
        /// </summary>
        public AgreementReference() {}

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// Create a new Agreement Reference
        /// with a given PMode Id
        /// </summary>
        /// <param name="pmodeId">
        /// </param>
        public AgreementReference(string pmodeId)
        {
            this.PModeId = pmodeId;
        }

        /// <summary>
        /// Indicates whether the current Agreement Ref is equal to another object of the same type.
        /// </summary>
        /// <returns>true if the current Agreement Ref is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An Agreement Ref to compare with this Agreement Ref.</param>
        public bool Equals(AgreementReference other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(this.Value, other.Value, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(this.Type, other.Type, StringComparison.OrdinalIgnoreCase);
        }
    }
}