using System;
using Eu.EDelivery.AS4.Strategies.Sender;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// Model to encapsulate the result after the <see cref="IAttachmentUploader"/> implementation has done the uploading.
    /// </summary>
    public class UploadResult : IEquatable<UploadResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult" /> class.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <param name="downloadUrl">The download URL.</param>
        /// <param name="status">The status.</param>
        [JsonConstructor]
        private UploadResult(
            string payloadId,
            string downloadUrl,
            SendResult status)
        {
            PayloadId = payloadId;
            DownloadUrl = downloadUrl;
            Status = status;
        }

        /// <summary>
        /// Gets or sets the Payload Id for which the attachment is uploaded.
        /// </summary>
        public string PayloadId { get; }

        /// <summary>
        /// Gets or sets the Download Url from which the attachment content can be downloaded.
        /// </summary>
        public string DownloadUrl { get; }

        /// <summary>
        /// Gets the status indicating whether the <see cref="SendResult"/> is successful or not.
        /// </summary>
        /// <value>The status.</value>
        public SendResult Status { get; }

        /// <summary>
        /// Creates a successful <see cref="UploadResult"/> with the uploaded response information.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <returns></returns>
        public static UploadResult SuccessWithId(string payloadId)
        {
            return new UploadResult(
                payloadId,
                downloadUrl: null,
                status: SendResult.Success);
        }

        /// <summary>
        /// Creates a successful <see cref="UploadResult"/> with the uploaded response information.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <param name="downloadUrl">The download URL.</param>
        /// <returns></returns>
        public static UploadResult SuccessWithIdAndUrl(string payloadId, string downloadUrl)
        {
            return new UploadResult(
                payloadId,
                downloadUrl,
                status: SendResult.Success);
        }

        /// <summary>
        /// Creates a successful <see cref="UploadResult"/> with the uploaded response information.
        /// </summary>
        /// <param name="downloadUrl">The download URL.</param>
        /// <returns></returns>
        public static UploadResult SuccessWithUrl(string downloadUrl)
        {
            return new UploadResult(
                payloadId: null,
                downloadUrl: downloadUrl,
                status: SendResult.Success);
        }

        /// <summary>
        /// Creates a failure <see cref="UploadResult"/> with a flag indicating that the upload operation can be retried.
        /// </summary>
        /// <returns></returns>
        public static UploadResult RetryableFail { get; } =
            new UploadResult(
                payloadId: null,
                downloadUrl: null,
                status: SendResult.RetryableFail);

        /// <summary>
        /// Creates a failure <see cref="UploadResult"/> with a flag indicating that the upload operation cannot be retried.
        /// </summary>
        /// <returns></returns>
        public static UploadResult FatalFail { get; } =
            new UploadResult(
                payloadId: null,
                downloadUrl: null,
                status: SendResult.FatalFail);


        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        public bool Equals(UploadResult other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(PayloadId, other.PayloadId)
                && string.Equals(DownloadUrl, other.DownloadUrl)
                && Status == other.Status;
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

            return obj is UploadResult r && Equals(r);
        }

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current object.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PayloadId != null ? PayloadId.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (DownloadUrl != null ? DownloadUrl.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Status;

                return hashCode;
            }
        }

        /// <summary>
        /// Returns a value that indicates whether the values of two <see cref="T:Eu.EDelivery.AS4.Strategies.Uploader.UploadResult" /> objects are equal.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if the <paramref name="left" /> and <paramref name="right" /> parameters have the same value; otherwise, false.</returns>
        public static bool operator ==(UploadResult left, UploadResult right)
        {
            return Equals(left, right);
        }

        /// <summary>
        /// Returns a value that indicates whether two <see cref="T:Eu.EDelivery.AS4.Strategies.Uploader.UploadResult" /> objects have different values.
        /// </summary>
        /// <param name="left">The first value to compare.</param>
        /// <param name="right">The second value to compare.</param>
        /// <returns>true if <paramref name="left" /> and <paramref name="right" /> are not equal; otherwise, false.</returns>
        public static bool operator !=(UploadResult left, UploadResult right)
        {
            return !Equals(left, right);
        }
    }
}