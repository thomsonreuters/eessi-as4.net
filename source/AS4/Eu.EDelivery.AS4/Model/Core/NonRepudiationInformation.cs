using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class NonRepudiationInformation : IEquatable<NonRepudiationInformation>
    {
        // ReSharper disable once InconsistentNaming
        public IEnumerable<Reference> MessagePartNRIReferences { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NonRepudiationInformation"/> class.
        /// </summary>
        public NonRepudiationInformation(IEnumerable<Reference> nriReferences)
        {
            if (nriReferences == null || nriReferences.Any(r => r is null))
            {
                throw new ArgumentNullException(nameof(nriReferences));
            }

            MessagePartNRIReferences = nriReferences;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(NonRepudiationInformation other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return MessagePartNRIReferences.Equals(other.MessagePartNRIReferences);
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

            return obj is NonRepudiationInformation nrr && Equals(nrr);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return MessagePartNRIReferences.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(NonRepudiationInformation left, NonRepudiationInformation right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.NonRepudiationInformation" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(NonRepudiationInformation left, NonRepudiationInformation right)
        {
            return !Equals(left, right);
        }
    }

    public class Reference : IEquatable<Reference>
    {
        public string URI { get; }

        public IEnumerable<ReferenceTransform> Transforms { get; }

        public ReferenceDigestMethod DigestMethod { get; }

        public byte[] DigestValue { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Reference"/> class.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="transforms"></param>
        /// <param name="digestMethod"></param>
        /// <param name="digestValue"></param>
        public Reference(
            string uri,
            IEnumerable<ReferenceTransform> transforms,
            ReferenceDigestMethod digestMethod,
            byte[] digestValue)
        {
            if (uri == null)
            {
                throw new ArgumentNullException(nameof(uri));
            }

            if (transforms == null)
            {
                throw new ArgumentNullException(nameof(transforms));
            }

            if (digestMethod == null)
            {
                throw new ArgumentNullException(nameof(digestMethod));
            }

            if (digestValue == null)
            {
                throw new ArgumentNullException(nameof(digestValue));
            }

            Transforms = transforms;
            DigestMethod = digestMethod;
            DigestValue = digestValue;
            URI = uri;
        }

        /// <summary>
        /// Creates a <see cref="Reference"/> model from a <see cref="System.Security.Cryptography.Xml.Reference"/> element.
        /// </summary>
        /// <param name="refElement"></param>
        /// <returns></returns>
        public static Reference CreateFromReferenceElement(System.Security.Cryptography.Xml.Reference refElement)
        {
            if (refElement == null)
            {
                throw new ArgumentNullException(nameof(refElement));
            }

            IEnumerable<ReferenceTransform> CreateTransformsFromChain(TransformChain chain)
            {
                if (chain != null)
                {
                    foreach (Transform transform in chain)
                    {
                        yield return new ReferenceTransform(transform.Algorithm);
                    }
                }
            }

            return new Reference(
                refElement.Uri,
                CreateTransformsFromChain(refElement.TransformChain),
                new ReferenceDigestMethod(refElement.DigestMethod),
                refElement.DigestValue);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Reference other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(URI, other.URI)
                && Transforms.Equals(other.Transforms)
                && DigestMethod.Equals(other.DigestMethod)
                && DigestValue.Equals(other.DigestValue);
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

            return obj is Reference r && Equals(r);
        }

        /// <summary>Serves as the default hash function. </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = URI.GetHashCode();
                hashCode = (hashCode * 397) ^ Transforms.GetHashCode();
                hashCode = (hashCode * 397) ^ DigestMethod.GetHashCode();
                hashCode = (hashCode * 397) ^ DigestValue.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.Reference" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(Reference left, Reference right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.Reference" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Reference left, Reference right)
        {
            return !Equals(left, right);
        }
    }

    public class ReferenceTransform : IEquatable<ReferenceTransform>
    {
        public string Algorithm { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceTransform"/> class.
        /// </summary>
        /// <param name="algorithm"></param>
        public ReferenceTransform(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            Algorithm = algorithm;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(ReferenceTransform other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Algorithm, other.Algorithm);
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

            return obj is ReferenceTransform t && Equals(t);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Algorithm.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.ReferenceTransform" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(ReferenceTransform left, ReferenceTransform right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.ReferenceTransform" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(ReferenceTransform left, ReferenceTransform right)
        {
            return !Equals(left, right);
        }
    }

    public class ReferenceDigestMethod : IEquatable<ReferenceDigestMethod>
    {
        public string Algorithm { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReferenceDigestMethod"/> class.
        /// </summary>
        /// <param name="algorithm"></param>
        public ReferenceDigestMethod(string algorithm)
        {
            if (algorithm == null)
            {
                throw new ArgumentNullException(nameof(algorithm));
            }

            Algorithm = algorithm;
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(ReferenceDigestMethod other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Algorithm, other.Algorithm);
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

            return obj is ReferenceDigestMethod m && Equals(m);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Algorithm.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.ReferenceDigestMethod" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(ReferenceDigestMethod left, ReferenceDigestMethod right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.ReferenceDigestMethod" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(ReferenceDigestMethod left, ReferenceDigestMethod right)
        {
            return !Equals(left, right);
        }
    }
}