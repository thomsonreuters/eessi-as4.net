using System;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.UnitTests.Extensions;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Model.PMode
{
    public class GivenMessagePackagingFacts
    {
        [Fact]
        public void Then_Parties_Are_Empty_When_No_Defined()
        {
            // Arrange
            var pmode = new PartyInfo {FromParty = CreateEmptyParty(), ToParty = CreateEmptyParty()};

            // Act
            string xml = AS4XmlSerializer.ToString(pmode);

            // Assert
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            Assert.Null(doc.SelectXmlNode("//*[local-name()='FromParty']"));
            Assert.Null(doc.SelectXmlNode("//*[local-name()='ToParty']"));
        }

        private static Party CreateEmptyParty()
        {
            return new Party();
        }

        [Fact]
        public void Then_Parties_Are_Filled_When_Defined()
        {
            // Arrange
            var pmode = new PartyInfo {FromParty = CreateFilledParty(), ToParty = CreateFilledParty()};

            // Act
            string xml = AS4XmlSerializer.ToString(pmode);

            // Assert
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            doc.AssertXmlNodeNotNull("FromParty");
            doc.AssertXmlNodeNotNull("ToParty");
        }

        private static Party CreateFilledParty()
        {
            return new Party(
                role: Guid.NewGuid().ToString(),
                partyId: new PartyId(id: Guid.NewGuid().ToString()));
        }
    }
}
