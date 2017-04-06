namespace Eu.EDelivery.AS4.PayloadService.Models
{
    public class UploadResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UploadResult"/> class.
        /// </summary>
        public UploadResult(string payloadId)
        {
            PayloadId = payloadId;
        }

        public string PayloadId { get; }
    }
}