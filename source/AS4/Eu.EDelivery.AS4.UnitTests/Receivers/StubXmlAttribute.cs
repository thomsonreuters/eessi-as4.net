using System.Xml;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Stub <see cref="XmlAttribute"/> implementation to wrap the key/value pair of the attribute.
    /// </summary>
    public class StubXmlAttribute : XmlAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StubXmlAttribute"/> class.
        /// </summary>
        /// <param name="localName">The local name of the attribute.</param>
        /// <param name="value">The value.</param>
        public StubXmlAttribute(string localName, string value)
            : base(string.Empty, localName, string.Empty, new XmlDocument())
        {
            Value = value;
        }

        /// <summary>
        /// Gets or sets the value of the node.
        /// </summary>
        /// <returns>The value returned depends on the <see cref="P:System.Xml.XmlNode.NodeType" /> of the node. For XmlAttribute nodes, this property is the value of attribute.</returns>
        /// <exception cref="T:System.ArgumentException">The node is read-only and a set operation is called.</exception>
        public override sealed string Value
        {
            get { return base.Value; }
            set { base.Value = value; }
        }
    }
}