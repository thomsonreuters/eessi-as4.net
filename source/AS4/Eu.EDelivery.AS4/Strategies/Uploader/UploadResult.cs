using System;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    public class UploadResult : IEquatable<UploadResult>
    {
        public string PayloadId { get; set; }

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