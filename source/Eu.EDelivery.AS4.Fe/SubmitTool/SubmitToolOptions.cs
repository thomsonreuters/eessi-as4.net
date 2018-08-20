namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// The config options for the submit tool
    /// </summary>
    public class SubmitToolOptions
    {
        /// <summary>
        /// Gets or sets to HTTP address to send the submitmessage to.
        /// </summary>
        /// <value>
        /// To HTTP address.
        /// </value>
        public string ToHttpAddress { get; set; }
        /// <summary>
        /// Gets or sets the payload HTTP address to send the payload(s) to.
        /// </summary>
        /// <value>
        /// The payload HTTP address.
        /// </value>
        public string PayloadHttpAddress { get; set; }
    }
}