using System.Collections.Generic;

namespace Eu.EDelivery.AS4.Fe.Runtime
{
    /// <summary>
    /// Class to hold runtime type information
    /// </summary>
    public class ItemType
    {
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets or sets the properties.
        /// </summary>
        /// <value>
        /// The properties.
        /// </value>
        public IEnumerable<Property> Properties { get; set; }
        /// <summary>
        /// Gets the name the technical name. This is the name as known in the code.
        /// </summary>
        /// <value>
        /// The name of the technical.
        /// </value>
        public string TechnicalName { get; internal set; }
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; }
    }
}