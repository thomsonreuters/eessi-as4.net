using System;
using System.Collections.Generic;
using System.Linq;
using static Eu.EDelivery.AS4.Constants.Namespaces;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Party : IEquatable<Party>
    {
        public IEnumerable<PartyId> PartyIds { get; }

        public string Role { get; }

        public static readonly Party DefaultFrom = new Party(EbmsDefaultRole, new PartyId(EbmsDefaultFrom));

        public static readonly Party DefaultTo = new Party(EbmsDefaultRole, new PartyId(EbmsDefaultTo));

        /// <summary>
        /// Initializes a new instance of the <see cref="Party" /> class.
        /// </summary>
        /// <param name="role"></param>
        /// <param name="partyId"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public Party(string role, PartyId partyId) : this(role, new[] { partyId }) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Party" /> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
        public Party(string role, IEnumerable<PartyId> partyIds)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            if (partyIds == null || partyIds.Any(id => id == null))
            {
                throw new ArgumentNullException(nameof(partyIds));
            }

            Role = role;
            PartyIds = partyIds;
        }

        /// <summary>
        /// Gets the primary party identifier of this <see cref="Party" />'s <see cref="PartyId" />.
        /// </summary>
        /// <value>The primary party identifier.</value>
        public string PrimaryPartyId => PartyIds.FirstOrDefault()?.Id;

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Party other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Role, other.Role, StringComparison.OrdinalIgnoreCase)
                   && PartyIds.All(other.PartyIds.Contains);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        /// true if the specified object  is equal to the current object; otherwise, false.
        /// </returns>
        /// <param name="obj">The object to compare with the current object. </param>
        public override bool Equals(object obj)
        {
            if (obj is null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return obj is Party p && Equals(p);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashRole = StringComparer.OrdinalIgnoreCase.GetHashCode(Role);
                return (hashRole * 397) ^ PartyIds.GetHashCode();
            }
        }

        
    }
}