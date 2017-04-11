using System;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// Model to encapsulate the result after the <see cref="IAttachmentUploader"/> implementation has done the uploading.
    /// </summary>
    public class UploadResult : IEquatable<UploadResult>
    {
        /// <summary>
        /// Gets or sets the Payload Id for which the attachment is uploaded.
        /// </summary>
        public string PayloadId { get; set; }

        /// <summary>
        /// Gets or sets the Download Url from which the attachment content can be downloaded.
        /// </summary>
        public string DownloadUrl { get; set; }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(UploadResult other)
        {
            return 
                PayloadId.Equals(other?.PayloadId) && 
                DownloadUrl.Equals(other?.DownloadUrl);
        }
    }
}