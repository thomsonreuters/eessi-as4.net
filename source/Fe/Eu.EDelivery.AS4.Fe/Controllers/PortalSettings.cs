using Eu.EDelivery.AS4.Fe.Authentication;
using Eu.EDelivery.AS4.Fe.Monitor;
using Eu.EDelivery.AS4.Fe.Pmodes;
using Eu.EDelivery.AS4.Fe.Settings;
using Eu.EDelivery.AS4.Fe.SubmitTool;

namespace Eu.EDelivery.AS4.Fe.Controllers
{
    /// <summary>
    /// Class containing all the Portal settings
    /// </summary>
    public class PortalSettings
    {
        /// <summary>
        /// Gets or sets the start URL.
        /// </summary>
        /// <value>
        /// The start URL.
        /// </value>
        public string Url { get; set; }
        /// <summary>
        /// Gets or sets the settings.
        /// </summary>
        /// <value>
        /// The settings.
        /// </value>
        public ApplicationSettings Settings { get; set; }
        /// <summary>
        /// Gets or sets the authentication.
        /// </summary>
        /// <value>
        /// The authentication.
        /// </value>
        public AuthenticationConfiguration Authentication { get; set; }
        /// <summary>
        /// Gets or sets the monitor.
        /// </summary>
        /// <value>
        /// The monitor.
        /// </value>
        public MonitorSettings Monitor { get; set; }
        /// <summary>
        /// Gets or sets the pmodes.
        /// </summary>
        /// <value>
        /// The pmodes.
        /// </value>
        public PmodeSettings Pmodes { get; set; }
        /// <summary>
        /// Gets or sets the submit tool.
        /// </summary>
        /// <value>
        /// The submit tool.
        /// </value>
        public SubmitToolOptions SubmitTool { get; set; }
    }
}