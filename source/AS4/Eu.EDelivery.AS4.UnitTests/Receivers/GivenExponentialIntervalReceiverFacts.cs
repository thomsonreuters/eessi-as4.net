using System;
using System.Linq;
using System.Xml;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="ExponentialIntervalReceiver"/>
    /// </summary>
    public class GivenExponentialIntervalReceiverFacts
    {
        private class StubXmlAttribute : XmlAttribute
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
}