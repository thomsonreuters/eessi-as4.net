using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Eu.EDelivery.AS4.Model.Core
{
    [DebuggerDisplay("Href {" + nameof(Href) + "}")]
    public class PartInfo : IEquatable<PartInfo>
    {
        public string Href { get; }

        public IDictionary<string, string> Properties { get; }

        public IEnumerable<Schema> Schemas { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartInfo"/> class.
        /// </summary>
        public PartInfo(string href) : this(href, new Dictionary<string, string>(), new Schema[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartInfo"/> class.
        /// </summary>
        public PartInfo(
            string href, 
            IDictionary<string, string> properties, 
            IEnumerable<Schema> schemas)
        {
            if (href == null)
            {
                throw new ArgumentNullException(nameof(href));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (schemas == null)
            {
                throw new ArgumentNullException(nameof(schemas));
            }

            Href = href.Replace(" ", string.Empty);
            Properties = properties;
            Schemas = schemas;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(PartInfo other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Href, other.Href);
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

            if (obj is PartInfo p)
            {
                return Equals(p);
            }

            return false;
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Href != null ? Href.GetHashCode() : 0;
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.PartInfo" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(PartInfo left, PartInfo right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.PartInfo" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(PartInfo left, PartInfo right)
        {
            return !Equals(left, right);
        }
    }
}