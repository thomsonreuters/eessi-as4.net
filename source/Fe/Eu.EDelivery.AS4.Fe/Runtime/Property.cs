using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Class to hold property information
    /// </summary>
    public class Property
    {
        /// <summary>
        /// Gets or sets the name of the friendly. Which is displayed in the FE.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        public string FriendlyName { get; set; }
        /// <summary>
        /// Gets or sets the name of the technical. The name known in the code.
        /// </summary>
        /// <value>
        /// The name of the technical.
        /// </value>
        public string TechnicalName { get; set; }
        /// <summary>
        /// Gets or sets the data type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; set; }
        /// <summary>
        /// Gets or sets the regex. Which is used to limit/validate string input.
        /// </summary>
        /// <value>
        /// The regex.
        /// </value>
        public string Regex { get; set; }
        /// <summary>
        /// Gets or sets the description which is shown in the ui.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
        /// <summary>
        /// Gets or sets the child properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IEnumerable<Property> Properties { get; set; }
    }
}