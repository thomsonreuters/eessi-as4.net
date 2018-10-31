using System;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    [DebuggerDisplay("Location {" + nameof(Location) + "}")]
    public class Schema : IEquatable<Schema>
    {
        public string Location { get; }

        public Maybe<string> Version { get; }

        public Maybe<string> Namespace { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema(string location)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            Location = location;
            Version = Maybe<string>.Nothing;
            Namespace = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        public Schema(string location, Maybe<string> version, Maybe<string> @namespace)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            Location = location;
            Version = version;
            Namespace = @namespace;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Schema"/> class.
        /// </summary>
        /// <param name="location"></param>
        /// <param name="version"></param>
        /// <param name="namespace"></param>
        public Schema(string location, string version, string @namespace)
        {
            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (@namespace == null)
            {
                throw new ArgumentNullException(nameof(@namespace));
            }

            Location = location;
            Version = Maybe.Just(version);
            Namespace = Maybe.Just(@namespace);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Schema other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return String.Equals(Location, other.Location)
                   && Version.Equals(other.Version)
                   && Namespace.Equals(other.Namespace);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
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

            if (obj is Schema s)
            {
                return Equals(s);
            }

            return false;
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Location != null ? Location.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (Version != Maybe<string>.Nothing ? Version.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Namespace != Maybe<string>.Nothing ? Namespace.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.Schema" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(Schema left, Schema right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.Schema" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Schema left, Schema right)
        {
            return !Equals(left, right);
        }
    }
}