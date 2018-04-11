using System.IO;
using System.Xml.Schema;

namespace Eu.EDelivery.AS4.Transformers.Resources
{
    /// <summary>
    /// Data class to collect the information about XSD schema's.
    /// </summary>
    public static class Schema
    {
        /// <summary>
        /// Initializes the <see cref="Schema"/> class.
        /// </summary>
        static Schema()
        {
            SubmitMessage = GetSubmitMessageSchema();
        }

        private static XmlSchema GetSubmitMessageSchema()
        {
            using (var stringReader = new StringReader(Properties.Resources.submitmessage_schema))
            {
                return XmlSchema.Read(stringReader, (sender, args) => { });
            }
        }

        /// <summary>
        /// Gets the XSD schema for the <see cref="Model.Submit.SubmitMessage"/> class.
        /// </summary>
        /// <value>The schema.</value>
        public static XmlSchema SubmitMessage { get; }
    }
}
