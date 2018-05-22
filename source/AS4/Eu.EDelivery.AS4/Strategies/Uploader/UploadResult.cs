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
            SendStatus status)
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
        public SendStatus Status { get; }

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
                status: SendStatus.Success);
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
                status: SendStatus.Success);
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
                status: SendStatus.Success);
        }

        /// <summary>
        /// Creates a failure <see cref="UploadResult"/> with a flag indicating that the upload operation can be retried.
        /// </summary>
        /// <returns></returns>
        public static UploadResult RetryableFail { get; } =
            new UploadResult(
                payloadId: null,
                downloadUrl: null,
                status: SendStatus.RetryableFail);

        /// <summary>
        /// Creates a failure <see cref="UploadResult"/> with a flag indicating that the upload operation cannot be retried.
        /// </summary>
        /// <returns></returns>
        public static UploadResult FatalFail { get; } =
            new UploadResult(
                payloadId: null,
                downloadUrl: null,
                status: SendStatus.Fail);

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <returns>true if the current object is equal to the <paramref name="other" /> parameter; otherwise, false.</returns>
        /// <param name="other">An object to compare with this object.</param>
        public bool Equals(UploadResult other)
        {
            return other != null 
                   && PayloadId.Equals(other.PayloadId) 
                   && DownloadUrl.Equals(other.DownloadUrl);
        }

        /// <summary>
        /// To the deliver result.
        /// </summary>
        /// <param name="r">The result during uploading.</param>
        /// <returns></returns>
        public static SendResult ToDeliverResult(UploadResult r)
        {
            return r.Status == SendStatus.Success
                ? SendResult.Success
                : r.Status == SendStatus.RetryableFail
                    ? SendResult.RetryableFail
                    : SendResult.FatalFail;
        }
    }
}