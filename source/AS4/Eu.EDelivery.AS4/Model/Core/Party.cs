using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Party : IEquatable<Party>
    {
        public List<PartyId> PartyIds { get; set; }
        [Description("Role")]
        public string Role { get; set; }

        public Party()
        {
            PartyIds = new List<PartyId>();
        }

        public Party(PartyId partyId) : this()
        {
            if (partyId == null)
            {
                throw new ArgumentNullException(nameof(partyId));
            }

            PartyIds.Add(partyId);
        }

        public Party(string role, PartyId partyId) : this()
        {
            PreConditionsParty(role, partyId);

            Role = role;
            PartyIds.Add(partyId);
        }

        private void PreConditionsParty(string role, PartyId partyId)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }
            if (role.Length == 0)
            {
                throw new ArgumentException(@"Party Role cannot be empty", nameof(role));
            }
            if (partyId == null)
            {
                throw new ArgumentNullException(nameof(partyId));
            }
        }

        public bool IsEmpty()
        {
            return
                string.IsNullOrEmpty(this.Role) && PartyIds.Count == 0;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(Party other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                string.Equals(this.Role, other.Role, StringComparison.OrdinalIgnoreCase) &&
                this.PartyIds.All(other.PartyIds.Contains);
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
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Party)obj);
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
                return ((this.Role != null ? StringComparer.OrdinalIgnoreCase.GetHashCode(this.Role) : 0) * 397)
                       ^ (this.PartyIds?.GetHashCode() ?? 0);
            }
        }
    }
}