using System;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS model to identify different <see cref="Party"/> models.
    /// </summary>
    [DebuggerDisplay(nameof(Id))]
    public class PartyId : IEquatable<PartyId>
    {
        /// <summary>
        /// Gets the value to identify <see cref="Party"/> models.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the optional type of the party identifier.
        /// </summary>
        public Maybe<string> Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id">The value of the party identifier</param>
        /// <exception cref="ArgumentException">The <paramref name="id"/> must be a non-empty string.</exception>
        public PartyId(string id)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentException(@"Id cannot be null or empty.", nameof(id));
            }

            Id = id;
            Type = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException">The <paramref name="id"/> must be a non-empty string.</exception>
        internal PartyId(string id, Maybe<string> type)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentException(@"Id cannot be null or empty.", nameof(id));
            }

            Id = id;
            Type = type ?? Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException">The <paramref name="id"/> must be a non-empty string.</exception>
        public PartyId(string id, string type)
        {
            if (String.IsNullOrEmpty(id))
            {
                throw new ArgumentException(@"Id cannot be null or empty.", nameof(id));
            }

            Id = id;
            Type = (!String.IsNullOrEmpty(type)).ThenMaybe(type);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.
        /// </returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(PartyId other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            bool equalId = string.Equals(Id, other.Id, StringComparison.OrdinalIgnoreCase);
            bool equalType =
                Type == Maybe<string>.Nothing && other.Type == Maybe<string>.Nothing
                || Type.SelectMany(t1 => 
                    other.Type.Select(t2 => 
                        t1.Equals(t2, StringComparison.OrdinalIgnoreCase)))
                    .GetOrElse(false);

            return equalId && equalType;
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

            return obj is PartyId other && Equals(other);
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
                int hashId = StringComparer.OrdinalIgnoreCase.GetHashCode(Id);
                int hashType = StringComparer.OrdinalIgnoreCase.GetHashCode(Type);

                return (hashId * 397) ^ hashType;
            }
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            return Id;
        }
    }
}