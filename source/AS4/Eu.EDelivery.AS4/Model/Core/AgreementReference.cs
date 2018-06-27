using System;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class AgreementReference : IEquatable<AgreementReference>
    {
        public string Value { get; }
        public Maybe<string> Type { get; }
        public Maybe<string> PModeId { get; }

        public AgreementReference(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            Type = Maybe<string>.Nothing;
            PModeId = Maybe<string>.Nothing;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="pmodeId"> </param>
        public AgreementReference(string value, string pmodeId)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (pmodeId == null)
            {
                throw new ArgumentNullException(nameof(pmodeId));
            }

            Value = value;
            Type = Maybe<string>.Nothing;
            PModeId = Maybe.Just(pmodeId);
        }

        public AgreementReference(string value, string type, string pModeId)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (pModeId == null)
            {
                throw new ArgumentNullException(nameof(pModeId));
            }

            Value = value;
            Type = Maybe.Just(type);
            PModeId = Maybe.Just(pModeId);
        }

        public AgreementReference(string value, Maybe<string> type, Maybe<string> pmodeId)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (pmodeId == null)
            {
                throw new ArgumentNullException(nameof(pmodeId));
            }

            Value = value;
            Type = type;
            PModeId = pmodeId;
        }

        /// <summary>
        /// Indicates wheter the curent Agreement Ref is empty
        /// </summary>
        /// <returns></returns>
        public bool IsEmpty()
        {
            return
                string.IsNullOrEmpty(Value) &&
                Type == Maybe<string>.Nothing &&
                PModeId == Maybe<string>.Nothing;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(AgreementReference other)
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
                   && Type.Equals(other.Type)
                   && PModeId.Equals(other.PModeId);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current object.
        /// </summary>
        /// <param name="obj">The object to compare with the current object. </param>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false.</returns>
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

            return obj is AgreementReference a && Equals(a);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = Value.GetHashCode();
                hashCode = (hashCode * 397) ^ Type.GetHashCode();
                hashCode = (hashCode * 397) ^ PModeId.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.AgreementReference" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(AgreementReference left, AgreementReference right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.AgreementReference" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(AgreementReference left, AgreementReference right)
        {
            return !Equals(left, right);
        }
    }
}