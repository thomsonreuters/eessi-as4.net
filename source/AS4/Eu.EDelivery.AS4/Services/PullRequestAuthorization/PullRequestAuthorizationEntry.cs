using System;

namespace Eu.EDelivery.AS4.Services.PullRequestAuthorization
{
    public class PullRequestAuthorizationEntry : IEquatable<PullRequestAuthorizationEntry>
    {
        public string Mpc { get; }
        public string CertificateThumbprint { get; }
        public bool Allowed { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestAuthorizationEntry"/> class.
        /// </summary>
        public PullRequestAuthorizationEntry(string mpc, string certificateThumbprint, bool allowed)
        {
            Mpc = mpc;
            CertificateThumbprint = certificateThumbprint;
            Allowed = allowed;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PullRequestAuthorizationEntry other)
        {
            return StringComparer.InvariantCulture.Equals(Mpc, other.Mpc) &&
                   StringComparer.InvariantCulture.Equals(CertificateThumbprint, other.CertificateThumbprint) &&
                   Allowed == other.Allowed;
        }

        /// <summary>Determines whether the specified object is equal to the current object.</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as PullRequestAuthorizationEntry;

            if (other != null)
            {
                return Equals(other);
            }

            return false;
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return ((Mpc?.GetHashCode() ?? 0) * 397) ^ ((CertificateThumbprint?.GetHashCode() ?? 0) * 13) ^ Allowed.GetHashCode();
        }
    }
}