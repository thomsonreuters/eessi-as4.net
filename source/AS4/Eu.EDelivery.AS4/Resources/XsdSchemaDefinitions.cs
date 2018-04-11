using System.IO;
using System.Xml.Schema;

namespace Eu.EDelivery.AS4.Resources
{
    /// <summary>
    /// Data class to collect the information about XSD schema's.
    /// </summary>
    public static class XsdSchemaDefinitions
    {
        /// <summary>
        /// Initializes the <see cref="XsdSchemaDefinitions"/> class.
        /// </summary>
        static XsdSchemaDefinitions()
        {
            SubmitMessage = GetSubmitMessageSchema();
        }

        private static XmlSchema GetSubmitMessageSchema()
        {
            using (var stringReader = new StringReader(Schemas.submitmessage_schema))
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
