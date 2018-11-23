using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using static Eu.EDelivery.AS4.Constants.Namespaces;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS Party model representation to create a sender and receiver pair in the <see cref="UserMessage"/>.
    /// </summary>
    [DebuggerDisplay("Party Id = {" + nameof(PrimaryPartyId) + "}")]
    public class Party : IEquatable<Party>
    {
        /// <summary>
        /// Gets the role of the agreed party. This is normally set to either 'Sender' or 'Receiver'.
        /// </summary>
        public string Role { get; }

        /// <summary>
        /// Gets the sequence of values to identify the agreed party.
        /// </summary>
        public IEnumerable<PartyId> PartyIds { get; }

        /// <summary>
        /// Gets the default 'From' party with the default role and party identifier for a sending party.
        /// </summary>
        public static readonly Party DefaultFrom = new Party(EbmsDefaultRole, new PartyId(EbmsDefaultFrom));

        /// <summary>
        /// Gets the default 'To' party with the default role and party identifier for a receiving party.
        /// </summary>
        public static readonly Party DefaultTo = new Party(EbmsDefaultRole, new PartyId(EbmsDefaultTo));

        /// <summary>
        /// Initializes a new instance of the <see cref="Party" /> class.
        /// </summary>
        /// <param name="role">The role of the agreed party. This is normally set to either 'Sender' or 'Receiver'.</param>
        /// <param name="partyId">The value to identify this party.</param>
        /// <exception cref="ArgumentException">The <paramref name="role"/> must a non-empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="partyId"/> must be specified.</exception>
        public Party(string role, PartyId partyId) : this(role, new[] { partyId }) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Party" /> class.
        /// </summary>
        /// <param name="role">The role of the agreed party. This is normally set to either 'Sender' or 'Receiver'.</param>
        /// <param name="partyIds">The values to identify this party.</param>
        /// <exception cref="ArgumentException">The <paramref name="partyIds"/> must have at least one element that's not <c>null</c>.</exception>
        /// <exception cref="ArgumentException">The <paramref name="role"/> must a non-empty string.</exception>
        /// <exception cref="ArgumentNullException">The <paramref name="partyIds"/> must be specified.</exception>
        public Party(string role, IEnumerable<PartyId> partyIds)
        {
            if (String.IsNullOrEmpty(role))
            {
                throw new ArgumentException(@"Party role cannot be null or empty.", nameof(role));
            }

            if (partyIds == null)
            {
                throw new ArgumentNullException(nameof(partyIds));
            }

            if (!partyIds.Any())
            {
                throw new ArgumentException(@"Party must have at least one PartyId");
            }

            if (partyIds.Any(id => id == null))
            {
                throw new ArgumentException(@"One more PartyId elements is a 'null' reference", nameof(partyIds));
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
        ///     <c>true</c> if the current object is equal to the <paramref name="other" /> parameter; otherwise, <c>false</c>.
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

            return PartyIds.All(other.PartyIds.Contains);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <returns>
        ///     <c>true</c>if the specified object  is equal to the current object; otherwise, <c>false</c>.
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return $"Party {{Role={Role}, PartyIds=[{String.Join("; ", PartyIds.Select(id => id.ToString()))}]}}";
        }
    }
}