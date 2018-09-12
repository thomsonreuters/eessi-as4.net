using System;
using System.Collections.Generic;
using System.Linq;

namespace Eu.EDelivery.AS4.Model.Common
{
    public class Party : IEquatable<Party>, IEquatable<PMode.Party>
    {
        public string Role { get; set; }
        public PartyId[] PartyIds { get; set; }

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
                   && Equals(PartyIds, other.PartyIds);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="pmodeParty">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="pmodeParty" /> parameter; otherwise, false.</returns>
        public bool Equals(PMode.Party pmodeParty)
        {
            IEnumerable<string> submitPartyIds =
                (PartyIds ?? Enumerable.Empty<PartyId>())
                    .Select(p => p?.Id)
                    .OrderBy(id => id);

            IEnumerable<string> pmodePartyIds =
                (pmodeParty?.PartyIds ?? Enumerable.Empty<PMode.PartyId>())
                    .Select(p => p?.Id)
                    .OrderBy(id => id);

            return submitPartyIds.SequenceEqual(pmodePartyIds);
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
            if (ReferenceEquals(null, obj))
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
        /// <returns>
        /// A hash code for the current object.
        /// </returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return ((Role != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(Role) : 0) * 397)
                       ^ (PartyIds?.GetHashCode() ?? 0);
            }
        }
    }
}