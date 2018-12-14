using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Eu.EDelivery.AS4.Model.Core
{
    /// <summary>
    /// Element in the <see cref="AS4Message"/> that relates to a <see cref="Attachment"/> instance.
    /// </summary>
    [DebuggerDisplay("Href {" + nameof(Href) + "}")]
    public class PartInfo : IEquatable<PartInfo>
    {
        /// <summary>
        /// The 'Content-Id' of the related <see cref="Attachment"/>, prefixed with &quot;c:d&quot;.
        /// </summary>
        public string Href { get; }

        /// <summary>
        /// Properties of the related <see cref="Attachment"/>.
        /// </summary>
        public IDictionary<string, string> Properties { get; }

        /// <summary>
        /// Gets a value indication whether or not this attachment has a MimeType property configured.
        /// </summary>
        public bool HasMimeType => Properties.ContainsKey("MimeType");

        /// <summary>
        /// Gets or sets the MimeType property of this attachment reference.
        /// </summary>
        public string MimeType
        {
            get => Properties["MimeType"];
            set => Properties["MimeType"] = value;
        }

        /// <summary>
        /// Schemas of the related <see cref="Attachment"/>.
        /// </summary>
        public IEnumerable<Schema> Schemas { get; }

        /// <summary>
        /// Gets or sets the CompressionType property of this attachment reference.
        /// </summary>
        public string CompressionType
        {
            get => Properties["CompressionType"];
            set => Properties["CompressionType"] = value;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartInfo"/> class.
        /// </summary>
        public PartInfo(string href) : this(href, new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase), new Schema[0]) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PartInfo"/> class.
        /// </summary>
        /// <exception cref="ArgumentNullException"></exception>
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

            Href = href.Replace(" ", String.Empty);
            Properties = properties;
            Schemas = schemas;
        }

        /// <summary>
        /// Creates a <see cref="PartInfo"/> element that references the given <paramref name="attachment"/> in the <see cref="UserMessage"/>.
        /// </summary>
        /// <param name="attachment">The payload which the to be created element must reference.</param>
        public static PartInfo CreateFor(Attachment attachment)
        {
            if (attachment == null)
            {
                throw new ArgumentNullException(nameof(attachment));
            }

            return new PartInfo(
                href: "cid:" + attachment.Id,
                properties: new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    ["MimeType"] = attachment.ContentType
                },
                schemas: new Schema[0]);
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

            return obj is PartInfo p && Equals(p);
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