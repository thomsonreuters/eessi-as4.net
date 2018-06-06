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
        public string FriendlyName { get; }
        /// <summary>
        /// Gets the data type.
        /// </summary>
        /// <value>
        /// The type.
        /// </value>
        public string Type { get; }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        /// <value>
        /// The default value.
        /// </value>
        public object DefaultValue { get; }

        /// <summary>
        /// Gets a value indicating whether this setting is required.
        /// </summary>
        /// <value>
        ///   <c>true</c> if required; otherwise, <c>false</c>.
        /// </value>
        public bool Required { get; }

        public string[] Attributes { get; }

        /// <summary>
        /// Gets the regex which is used to limit/validate a value.
        /// </summary>
        /// <value>
        /// The regex.
        /// </value>
        public string Regex { get; }

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
        /// Initializes a new instance of the <see cref="InfoAttribute" /> class.
        /// </summary>
        /// <param name="friendlyName">Name of the friendly.</param>
        /// <param name="regex">The regex.</param>
        /// <param name="type">The type.</param>
        /// <param name="defaultValue">The default value.</param>
        /// <param name="required">if set to <c>true</c> then the property is required.</param>
        public InfoAttribute(string friendlyName, string regex = "", string type = "", object defaultValue = null, bool required = false, string[] attributes = null)
        {
            FriendlyName = friendlyName;
            Regex = regex;
            Type = type;
            DefaultValue = defaultValue;
            Required = required;
            Attributes = attributes;
        }
    }
}