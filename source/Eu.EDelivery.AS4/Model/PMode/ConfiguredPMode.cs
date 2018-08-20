namespace Eu.EDelivery.AS4.Model.PMode
{
    /// <summary>
    /// Configured PMode (Sending or Receiving)
    /// </summary>
    public class ConfiguredPMode
    {
        public string Filename { get; set; }
        public IPMode PMode { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConfiguredPMode"/> class
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="pmode"></param>
        public ConfiguredPMode(string fullPath, IPMode pmode)
        {
            this.Filename = fullPath;
            this.PMode = pmode;
        }
    }
}
