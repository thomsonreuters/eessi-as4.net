namespace AS4.ParserService.Models
{

    /// <summary>
    /// Represents a payload / attachment that must be included in an AS4 Message.
    /// </summary>
    public class PayloadInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadInfo"/> class.
        /// </summary>
        public PayloadInfo()
        {
        }

        public PayloadInfo(string fileName, string contentType, byte[] content)
        {
            PayloadName= fileName;
            ContentType = contentType;
            Content = content;
        }

        /// <summary>
        /// The name of the payload. 
        /// </summary>
        public string PayloadName { get; set; }

        /// <summary>
        /// The content-type of the payload.
        /// </summary>
        public string ContentType { get; set; }

        /// <summary>
        /// The content of the payload.
        /// </summary>
        public byte[] Content { get; set; }
    }
}