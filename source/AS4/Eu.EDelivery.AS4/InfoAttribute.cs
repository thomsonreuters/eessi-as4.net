using System;

namespace Eu.EDelivery.AS4
{
    /// <summary>
    /// This attribute is used to decorate runtime times.
    /// </summary>
    /// <seealso cref="System.Attribute" />
    public class InfoAttribute : Attribute
    {
        /// <summary>
        /// Gets the name of the friendly as it should appear in the ui.
        /// </summary>
        /// <value>
        /// The name of the friendly.
        /// </value>
        public string FriendlyName { get; private set; }
        /// <summary>
        /// Gets the data type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; private set; }
        /// <summary>
        /// Gets the regex which is used to limit/validate a value.
        /// </summary>
        /// <value>
        /// The regex.
        /// </value>
        public string Regex { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoAttribute"/> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        public InfoAttribute(string friendlyName)
        {
            FriendlyName = friendlyName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoAttribute"/> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="regex">The regex.</param>
        public InfoAttribute(string friendlyName, string regex)
        {
            FriendlyName = friendlyName;
            Regex = regex;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InfoAttribute"/> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="regex">The regex.</param>
        /// <param name="type">The type.</param>
        public InfoAttribute(string friendlyName, string regex, string type)
        {
            FriendlyName = friendlyName;
            Regex = regex;
            Type = type;
        }
    }
}