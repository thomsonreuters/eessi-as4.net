namespace Eu.EDelivery.AS4.PayloadService.Models
{
    /// <summary>
    /// Model to to encapsulate the result of the uploaded payload from the persistence services.
    /// </summary>
    public class UploadResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult"/> class.
        /// </summary>
        /// <param name="payloadId">The payload Id.</param>
        /// <param name="downloadUrl">The download Url.</param>
        public UploadResult(string payloadId, string downloadUrl)
        {
            PayloadId = payloadId;
            DownloadUrl = downloadUrl;
        }

        /// <summary>
        /// Gets the payload id for the uploaded payload.
        /// </summary>
        public string PayloadId { get; }

        /// <summary>
        /// Gets the URL where the payload content can be download.
        /// </summary>
        public string DownloadUrl { get; }
    }
}