namespace AS4.ParserService.Models
{
    /// <summary>
    /// Contains the result of the Encode AS4 operation.
    /// </summary>
    public class EncodeResult
    {
        /// <summary>
        /// The URL to where the AS4 Message must be sent to.
        /// </summary>
        public string SendToUrl { get; set; }

        /// <summary>
        /// The AS4Message that must be sent.
        /// </summary>
        public byte[] AS4Message { get; set; }

        /// <summary>
        /// The Ebms Id that is assigned to the AS4 Message that has been created.
        /// </summary>
        public string EbmsMessageId { get; set; }

        /// <summary>
        /// The ContentType of the AS4Message.
        /// </summary>
        /// <remarks>This value must be used as the value of the Content-Type HTTP header when sending the AS4 Message</remarks>
        public string ContentType { get; set; }
    }
}