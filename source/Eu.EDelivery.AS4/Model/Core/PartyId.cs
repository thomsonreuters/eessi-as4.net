using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class PartyId : IEquatable<PartyId>
    {
        public string Id { get; }

        public Maybe<string> Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="ArgumentException"></exception>
        public PartyId(string id)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            Id = id;
            Type = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public PartyId(string id, Maybe<string> type)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }


            Id = id;
            Type = type;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartyId" /> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        public PartyId(string id, string type)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }


            Id = id;
            Type = Maybe.Just(type);
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
    }
}