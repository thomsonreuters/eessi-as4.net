using System;
using Eu.EDelivery.AS4.Strategies.Sender;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Strategies.Uploader
{
    /// <summary>
    /// Model to encapsulate the result after the <see cref="IAttachmentUploader"/> implementation has done the uploading.
    /// </summary>
    public class UploadResult : DeliverResult, IEquatable<UploadResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult" /> class.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <param name="downloadUrl">The download URL.</param>
        /// <param name="status">The status.</param>
        /// <param name="needsAnotherRetry">if set to <c>true</c> [needs another retry].</param>
        [JsonConstructor]
        private UploadResult(
            string payloadId,
            string downloadUrl,
            DeliveryStatus status,
            bool needsAnotherRetry) : base(status, needsAnotherRetry)
        {
            PayloadId = payloadId;
            DownloadUrl = downloadUrl;
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
        /// Creates a successful <see cref="UploadResult"/> with the uploaded response information.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <returns></returns>
        public static UploadResult SuccessWithId(string payloadId)
        {
            return new UploadResult(
                payloadId,
                downloadUrl: null,
                status: DeliveryStatus.Successful,
                needsAnotherRetry: false);
        }

        /// <summary>
        /// Creates a successful <see cref="UploadResult"/> with the uploaded response information.
        /// </summary>
        /// <param name="payloadId">The payload identifier.</param>
        /// <param name="downloadUrl">The download URL.</param>
        /// <returns></returns>
        public static UploadResult Success(string payloadId, string downloadUrl)
        {
            return new UploadResult(
                payloadId,
                downloadUrl,
                status: DeliveryStatus.Successful,
                needsAnotherRetry: false);
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
                status: DeliveryStatus.Successful,
                needsAnotherRetry: false);
        }

        /// <summary>
        /// Creates a failure <see cref="UploadResult"/> with a flag indicating wheter or not the upload operation should be retried.
        /// </summary>
        /// <param name="needsAnotherRetry">if set to <c>true</c> [needs another retry].</param>
        /// <returns></returns>
        public static UploadResult Failure(bool needsAnotherRetry)
        {
            return new UploadResult(
                payloadId: null,
                downloadUrl: null,
                status: DeliveryStatus.Failure,
                needsAnotherRetry: needsAnotherRetry);
        }

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