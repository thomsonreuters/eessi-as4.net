using System;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// ebMS model which defines the reference of an agreement between to parties.
    /// </summary>
    [DebuggerStepThrough]
    [DebuggerDisplay("Agreement { " + nameof(Value) + " }")]
    public class AgreementReference : IEquatable<AgreementReference>
    {
        /// <summary>
        /// Gets the required value of the agreement reference.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// Gets the optional type of the agreement reference.
        /// </summary>
        public Maybe<string> Type { get; }

        /// <summary>
        /// Gets the optional processing mode identifier of the agreement reference
        /// which defines the processing mode that was used for this <see cref="UserMessage"/>.
        /// </summary>
        public Maybe<string> PModeId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// </summary>
        /// <param name="value">The required value of the agreement reference.</param>
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
        /// <param name="value">The required value of the agreement reference.</param>
        /// <param name="pmodeId">The optional processing mode identifier of the agreement reference.</param>
        public AgreementReference(string value, string pmodeId) : this(value, type: null, pmodeId: pmodeId) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// </summary>
        /// <param name="value">The required value of the agreement reference.</param>
        /// <param name="type">The optional type of the agreement reference.</param>
        /// <param name="pmodeId">The optional processing mode identifier of the agreement reference.</param>
        public AgreementReference(string value, string type, string pmodeId)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            Type = (type != null).ThenMaybe(type);
            PModeId = (pmodeId != null).ThenMaybe(pmodeId);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AgreementReference"/> class. 
        /// </summary>
        /// <param name="value">The required value of the agreement reference.</param>
        /// <param name="type">The optional type of the agreement reference.</param>
        /// <param name="pmodeId">The optional processing mode identifier of the agreement reference.</param>
        internal AgreementReference(string value, Maybe<string> type, Maybe<string> pmodeId)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            Value = value;
            Type = type ?? Maybe<string>.Nothing;
            PModeId = pmodeId ?? Maybe<string>.Nothing;
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

            return String.Equals(Value, other.Value)
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
            if (obj is null)
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

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            string type = Type.Select(x => ", Type = " + x).GetOrElse(String.Empty);
            string pmodeId = PModeId.Select(x => ", PModeId = " + x).GetOrElse(String.Empty);

            return $"Agreement {{ Value = {Value}{type}{pmodeId} }}";
        }
    }
}