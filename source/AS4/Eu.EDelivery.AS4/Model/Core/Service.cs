using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Service : IEquatable<Service>
    {
        public string Value { get; }

        public Maybe<string> Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="value"></param>
        public Service(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            Type = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Service"/> class.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="type"></param>
        public Service(string value, string type)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            Value = value;
            Type = Maybe.Just(type);
        }

        public static Service TestService = new Service(Constants.Namespaces.TestService);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Service other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Value, other.Value)
                   && string.Equals(Type, other.Type);
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

            return obj is Service s && Equals(s);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (Value.GetHashCode() * 397) ^ Type.GetHashCode();
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.Service" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(Service left, Service right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.Service" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Service left, Service right)
        {
            return !Equals(left, right);
        }
    }
}