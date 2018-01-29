using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Services.DynamicDiscovery
{
    public class GivenLocalDynamicDiscoveryProfileFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task RetrieveSmpResponseFromDatastore()
        {
            // Arrange
            var expected = new SmpResponse
            {
                ToPartyId = Guid.NewGuid().ToString(),
                PartyRole = "role",
                PartyType = "type"
            };

            InsertSmpResponse(expected);

            var sut = new LocalDynamicDiscoveryProfile(GetDataStoreContext);

            // Act
            XmlDocument actualDoc = await sut.RetrieveSmpMetaData(expected.ToPartyId, properties: null);

            // Assert
            var actual = AS4XmlSerializer.FromString<SmpResponse>(actualDoc.OuterXml);
            Assert.Equal(expected.ToPartyId, actual.ToPartyId);
        }

        private void InsertSmpResponse(SmpResponse smpResponse)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                context.SmpResponses.Add(smpResponse);
                context.SaveChanges();
            }
        }

        [Fact]
        public void DecorateMandatoryInfoToSendingPMode()
        {
            // Arrange
            var smpResponse = new SmpResponse
            {
                PartyRole = "role",
                Url = "http://some/url"
            };

            var doc = new XmlDocument();
            doc.LoadXml(AS4XmlSerializer.ToString(smpResponse));

            var pmode = new SendingProcessingMode();
            var sut = new LocalDynamicDiscoveryProfile(GetDataStoreContext);

            // Act
            SendingProcessingMode actual = sut.DecoratePModeWithSmpMetaData(pmode, doc);

            // Assert
            Assert.Equal(smpResponse.Url, actual.PushConfiguration.Protocol.Url);
        }
    }
}
