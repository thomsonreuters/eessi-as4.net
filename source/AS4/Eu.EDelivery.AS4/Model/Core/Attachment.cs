using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Streaming;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Attachment : IEquatable<Attachment>
    {
        /// <summary>
        /// Identifier of the <see cref="Attachment"/> to distinguish between entities.
        /// </summary>
        public string Id { get; }

        // TODO: only the decompression step uses the public set, should be investigated
        public string ContentType { get; set; }

        private Stream _content;

        // TODO: only the decompression step uses the public set, should be investigated
        public Stream Content
        {
            get => _content;
            set => UpdateContent(value);
        }

        private void UpdateContent(Stream value)
        {
            if (ReferenceEquals(_content, value) == false)
            {
                _content?.Dispose();
            }

            _content = value;
            if (_content != null)
            {
                EstimatedContentSize = StreamUtilities.GetStreamSize(_content);
            }
            else
            {
                EstimatedContentSize = -1;
            }
        }

        public long EstimatedContentSize { get; private set; }

        // TODO: only the upload step uses the public set, should be investigated
        public string Location { get; set; }

        public IDictionary<string, string> Properties { get; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="id"></param>
        public Attachment(string id) : this(id, Stream.Null, "application/octet-stream") { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="location"></param>
        /// <param name="contentType"></param>
        public Attachment(
            string id,
            string location,
            string contentType)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            Id = id.Replace(" ", string.Empty);
            Location = location;
            ContentType = contentType;
            EstimatedContentSize = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        public Attachment(Stream content, string contentType) 
            : this(Guid.NewGuid().ToString(), content, contentType) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        public Attachment(
            string id,
            Stream content,
            string contentType)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            Id = id.Replace(" ", string.Empty);
            Content = content;
            ContentType = contentType;
            EstimatedContentSize = StreamUtilities.GetStreamSize(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="location"></param>
        /// <param name="contentType"></param>
        public Attachment(
            string id,
            Stream content,
            string location,
            string contentType)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (location == null)
            {
                throw new ArgumentNullException(nameof(location));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            Id = id.Replace(" ", string.Empty);
            Content = content;
            Location = location;
            ContentType = contentType;
            EstimatedContentSize = StreamUtilities.GetStreamSize(content);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Attachment"/> class.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="content"></param>
        /// <param name="contentType"></param>
        /// <param name="props"></param>
        public Attachment(
            string id,
            Stream content,
            string contentType,
            IDictionary<string, string> props)
        {
            if (id == null)
            {
                throw new ArgumentNullException(nameof(id));
            }

            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (contentType == null)
            {
                throw new ArgumentNullException(nameof(contentType));
            }

            if (props == null)
            {
                throw new ArgumentNullException(nameof(props));
            }

            Id = id.Replace(" ", string.Empty);
            Content = content;
            ContentType = contentType;
            Properties = props;
            EstimatedContentSize = StreamUtilities.GetStreamSize(content);
        }

        /// <summary>
        /// Verifies if this is the Attachment that is referenced by the given <paramref name="partInfo"/>
        /// </summary>
        /// <param name="partInfo"></param>
        /// <returns></returns>
        public bool Matches(PartInfo partInfo)
        {
            return partInfo.Href != null && partInfo.Href.Equals($"cid:{Id}");
        }

        /// <summary>
        /// Verifies if this Attachment is referred by any of the specified <paramref name="partInfos"/>
        /// </summary>
        /// <param name="partInfos"></param>
        /// <returns></returns>
        public bool MatchesAny(IEnumerable<PartInfo> partInfos)
        {
            return partInfos.Any(this.Matches);
        }

        /// <summary>
        /// Verifies if this is the Attachment that is referenced by the given cryptography <paramref name="reference"/>
        /// </summary>
        /// <param name="reference"></param>
        /// <returns></returns>
        public bool Matches(System.Security.Cryptography.Xml.Reference reference)
        {
            return reference.Uri.Equals($"cid:{Id}");
        }

        /// <summary>
        /// Makes sure that the Attachment Content is positioned at the start of the content.
        /// </summary>
        public void ResetContentPosition()
        {
            if (Content != null)
            {
                StreamUtilities.MovePositionToStreamStart(Content);
            }
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(Attachment other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(Id, other.Id);
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

            return obj is Attachment a && Equals(a);
        }

        /// <summary>
        /// Serves as the default hash function. 
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Model.Core.Attachment" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(Attachment left, Attachment right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Model.Core.Attachment" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(Attachment left, Attachment right)
        {
            return !Equals(left, right);
        }
    }
}