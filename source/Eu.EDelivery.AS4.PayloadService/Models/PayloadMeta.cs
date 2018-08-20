namespace Eu.EDelivery.AS4.PayloadService.Models
{
    /// <summary>
    /// Model to define the Metadata of the saved <see cref="Payload" />.
    /// </summary>
    public class PayloadMeta
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PayloadMeta" /> class.
        /// </summary>
        /// <param name="originalFileName"></param>
        public PayloadMeta(string originalFileName)
        {
            OriginalFileName = originalFileName;
        }

        /// <summary>
        /// Gets the original file name of the uploaded <see cref="Payload" />.
        /// </summary>
        public string OriginalFileName { get; private set; }
    }
}