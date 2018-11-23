using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Eu.EDelivery.AS4.UnitTests.Common;
using FsCheck.Xunit;
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
            XmlDocument actualDoc = await sut.RetrieveSmpMetaDataAsync(fixture, properties: null);

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
            SendingProcessingMode actual = sut.DecoratePModeWithSmpMetaData(pmode, doc).CompletedSendingPMode;

            // Assert
            Assert.Equal(smpResponse.Url, actual.PushConfiguration.Protocol.Url);
        }

        [Fact]
        public void Decorate_But_Not_Recreate_PushConfiguration()
        {
            // Arrange
            var smpResponse = new SmpConfiguration
            {
                Url = "http://some/url"
            };

            var push = new PushConfiguration
            {
                TlsConfiguration = new TlsConfiguration
                {
                    CertificateType = TlsCertificateChoiceType.PrivateKeyCertificate,
                    ClientCertificateInformation = new ClientCertificateReference()
                }
            };
            var fixture = new SendingProcessingMode { PushConfiguration = push };

            // Act
            SendingProcessingMode result = ExerciseDecorate(fixture, smpResponse);

            // Assert
            Assert.Same(push, result.PushConfiguration);
            Assert.Equal(smpResponse.Url, push.Protocol.Url);
            Assert.Same(
                push.TlsConfiguration.ClientCertificateInformation,
                result.PushConfiguration.TlsConfiguration.ClientCertificateInformation);
        }

        [Fact]
        public void Decorate_Not_Recreate_CollaborationInfo()
        {
            // Arrange
            var smpResponse = new SmpConfiguration();
            var collaboration = new CollaborationInfo
            {
                AgreementReference = new AgreementReference
                {
                    Value = "http://eu.europe.org/agreements"
                }
            };

            var fixture = new SendingProcessingMode
            {
                MessagePackaging = new SendMessagePackaging
                {
                    CollaborationInfo = collaboration
                }
            };

            // Act
            SendingProcessingMode result = ExerciseDecorate(fixture, smpResponse);

            // Assert
            Assert.Same(collaboration, result.MessagePackaging.CollaborationInfo);
            Assert.Equal(
                collaboration.AgreementReference.Value,
                result.MessagePackaging.CollaborationInfo.AgreementReference.Value);
        }

        [Fact]
        public void Dont_Touch_Signing_During_Decoration()
        {
            // Arrange
            var smpResponse = new SmpConfiguration
            {
                EncryptionEnabled = true
            };
            var fixture = new SendingProcessingMode
            {
                Security =
                {
                    Signing = { IsEnabled = true }
                }
            };

            // Act
            SendingProcessingMode result = ExerciseDecorate(fixture, smpResponse);

            // Assert
            Assert.Same(fixture.Security.Signing, result.Security.Signing);
            Assert.True(result.Security.Signing.IsEnabled);
        }

        private SendingProcessingMode ExerciseDecorate(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            var doc = new XmlDocument();
            doc.LoadXml(AS4XmlSerializer.ToString(smpResponse));

            var sut = new LocalDynamicDiscoveryProfile(GetDataStoreContext);
            return sut.DecoratePModeWithSmpMetaData(pmode, doc).CompletedSendingPMode;
        }
    }
}
