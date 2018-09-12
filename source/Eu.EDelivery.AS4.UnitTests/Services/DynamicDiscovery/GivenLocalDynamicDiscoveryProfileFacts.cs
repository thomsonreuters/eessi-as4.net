using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.UnitTests.Common;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Services.DynamicDiscovery
{
    public class GivenLocalDynamicDiscoveryProfileFacts : GivenDatastoreFacts
    {
        [Fact]
        public async Task RetrieveSmpResponseFromDatastore()
        {
            // Arrange
            var fixture = new Party("role", new PartyId(Guid.NewGuid().ToString(), "type"));
            var expected = new SmpConfiguration
            {
                PartyRole = fixture.Role,
                ToPartyId = fixture.PrimaryPartyId,
                PartyType = "type"
            };

            InsertSmpResponse(expected);

            var sut = new LocalDynamicDiscoveryProfile(GetDataStoreContext);

            // Act
            XmlDocument actualDoc = await sut.RetrieveSmpMetaData(fixture, properties: null);

            // Assert
            var actual = AS4XmlSerializer.FromString<SmpConfiguration>(actualDoc.OuterXml);
            Assert.Equal(expected.ToPartyId, actual.ToPartyId);
        }

        private void InsertSmpResponse(SmpConfiguration smpConfiguration)
        {
            using (DatastoreContext context = GetDataStoreContext())
            {
                context.SmpConfigurations.Add(smpConfiguration);
                context.SaveChanges();
            }
        }

        [Fact]
        public async Task FailsToRetrieveSmpMetaData_IfPartyIsInvalid()
        {
            // Arrange
            var sut = new LocalDynamicDiscoveryProfile(createDatastore: null);

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.RetrieveSmpMetaData(new Party("role", Enumerable.Empty<PartyId>()), properties: null));
        }

        [Fact]
        public void DecorateMandatoryInfoToSendingPMode()
        {
            // Arrange
            var smpResponse = new SmpConfiguration
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
