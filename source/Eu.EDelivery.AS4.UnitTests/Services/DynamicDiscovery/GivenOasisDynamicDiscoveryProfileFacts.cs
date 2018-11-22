using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Xunit;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using PartyId = Eu.EDelivery.AS4.Model.Core.PartyId;

namespace Eu.EDelivery.AS4.UnitTests.Services.DynamicDiscovery
{
    public class GivenOasisDynamicDiscoveryProfileFacts
    {
        [Fact]
        public async Task Oasis_DynamicDiscovery_ConnectivityTest_Retrieve_SMP_MetaData_From_Properties()
        {
            // Arrange
            var sut = new OasisDynamicDiscoveryProfile();

            // Act
            XmlDocument smpMetaData = await sut.RetrieveSmpMetaDataAsync(
                new Party("Receiver", new PartyId("cefsupport1gw", "connectivity-partid-qns")),
                new Dictionary<string, string>
                {
                    [nameof(sut.ServiceProviderDomainName)] = "acc.edelivery.tech.ec.europa.eu",
                    [nameof(sut.ServiceProviderSubDomain)] = "connectivitytest",
                    [nameof(sut.DocumentIdentifier)] = "doc_id1",
                    [nameof(sut.DocumentScheme)] = "connectivity-docid-qns"
                });

            // Assert
            Assert.NotNull(smpMetaData);
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='EndpointURI']"));
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='ParticipantIdentifier']"));
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='ProcessIdentifier']"));
        }

        [Fact]
        public async Task Oasis_DynamicDiscovery_ConnectivityTest_Retrieve_SMP_Metadata_Via_Fallback()
        {
            // Arrange
            var sut = new OasisDynamicDiscoveryProfile();

            // Act
            XmlDocument smpMetaData = await sut.RetrieveSmpMetaDataAsync(
                new Party("Receiver", new PartyId("cefsupport1gw", "connectivity-partid-qns")),
                new Dictionary<string, string>
                {
                    [nameof(sut.ServiceProviderDomainName)] = "acc.edelivery.tech.ec.europa.eu",
                    [nameof(sut.ServiceProviderSubDomain)] = "connectivitytest",
                });

            // Assert
            Assert.NotNull(smpMetaData);
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='EndpointURI']"));
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='ParticipantIdentifier']"));
            Assert.NotNull(smpMetaData.SelectSingleNode("//*[local-name()='ProcessIdentifier']"));
        }

        [Fact]
        public void Oasis_DynamicDiscovery_SendingPMode_Completion()
        {
            // Arrange
            var sut = new OasisDynamicDiscoveryProfile();
            var fixture = new SendingProcessingMode();

            var smpMetaData = new XmlDocument();
            smpMetaData.LoadXml(OasisConnectivityTestResponse);

            // Act
            SendingProcessingMode result = sut.DecoratePModeWithSmpMetaData(fixture, smpMetaData);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("http://40.115.23.114:8080/domibus/services/msh?domain=dynamic", result.PushConfiguration.Protocol.Url);
            Assert.Equal("urn:www.cenbii.eu:profile:bii04:ver1.0", result.MessagePackaging.CollaborationInfo.Service.Value);
            Assert.Equal("connectivity-procid-qns", result.MessagePackaging.CollaborationInfo.Service.Type);
            Assert.Equal("doc_id1", result.MessagePackaging.CollaborationInfo.Action);
            Assert.Contains(result.MessagePackaging.MessageProperties, p => p.Name == "originalSender");
            Assert.Contains(result.MessagePackaging.MessageProperties, p => p.Name == "finalRecipient" && p.Value == "cefsupport1gw");
            Assert.True(result.Security.Encryption.EncryptionCertificateInformation != null, "no encryption certificate set");
            Assert.True(result.MessagePackaging.PartyInfo.ToParty != null, "no ToParty set");
        }

        private const string OasisConnectivityTestResponse =
            @"<SignedServiceMetadata xmlns=""http://docs.oasis-open.org/bdxr/ns/SMP/2016/05"">
                <ServiceMetadata>
                    <ServiceInformation>
                        <ParticipantIdentifier scheme=""connectivity-partid-qns"">cefsupport1gw</ParticipantIdentifier>
                        <DocumentIdentifier scheme=""connectivity-docid-qns"">doc_id1</DocumentIdentifier>
                        <ProcessList>
                            <Process>
                                <ProcessIdentifier scheme=""connectivity-procid-qns"">urn:www.cenbii.eu:profile:bii04:ver1.0</ProcessIdentifier>
                                <ServiceEndpointList>
                                    <Endpoint transportProfile=""bdxr-transport-ebms3-as4-v1p0"">
                                        <EndpointURI>
                                        http://40.115.23.114:8080/domibus/services/msh?domain=dynamic
                                        </EndpointURI>
                                        <RequireBusinessLevelSignature>false</RequireBusinessLevelSignature>
                                        <ServiceActivationDate>2018-01-01T00:00:00.000+02:00</ServiceActivationDate>
                                        <ServiceExpirationDate>2018-12-31T23:59:59+02:00</ServiceExpirationDate>
                                        <Certificate>
                                        MIIFvjCCA6agAwIBAgICEAswDQYJKoZIhvcNAQELBQAwgbwxCzAJBgNVBAYTAkJF MRAwDgYDVQQIDAdCZWxnaXVtMRowGAYDVQQKDBFDb25uZWN0aXZpdHkgVGVzdDEj MCEGA1UECwwaQ29ubmVjdGluZyBFdXJvcGUgRmFjaWxpdHkxJzAlBgNVBAMMHkNv bm5lY3Rpdml0eSBUZXN0IENvbXBvbmVudCBDQTExMC8GCSqGSIb3DQEJARYiQ0VG LUVERUxJVkVSWS1TVVBQT1JUQGVjLmV1cm9wYS5ldTAeFw0xNzA5MTExMjI1NTha Fw0yNzEyMTgxMjI1NThaMIG+MQswCQYDVQQGEwJCRTEQMA4GA1UECAwHQmVsZ2l1 bTERMA8GA1UEBwwIQnJ1c3NlbHMxGjAYBgNVBAoMEUNvbm5lY3Rpdml0eSBUZXN0 MSMwIQYDVQQLDBpDb25uZWN0aW5nIEV1cm9wZSBGYWNpbGl0eTEWMBQGA1UEAwwN Y2Vmc3VwcG9ydDFndzExMC8GCSqGSIb3DQEJARYiY2VmLWVkZWxpdmVyeS1zdXBw b3J0QGVjLmV1cm9wYS5ldTCCASIwDQYJKoZIhvcNAQEBBQADggEPADCCAQoCggEB AMZpo2/CVmOFYCxb0jjn/KW+6tkpOcSFyP2Bu90b+wXbhdriZHSj4HoIOtiD5dP1 XIjWruWwceWIztpv9q/53eVvxgc+jxdbEUsLuN0xefR/BIVSb4GSG+k7D0jWghPk U25rPovWXc5YibqWrVEEco/bP5ZYSlL3lGCsLUMFQ3pXoj84d7aRPQrPRTMQEZcX ix4uNDDcsbtr+Zu7dV3g2aMEi5YX3tz/6Zoa0ac1B/v6tKzsPDUE5p1DSUzy/oEq HadxUgKRUACZsrnvNjxVQRxqCObN+UEZorxsNt/zV6WXlJ3To+GJd17v5BWW/Vm0 Flhn0h5QZCEzBgWh449PuicCAwEAAaOBxTCBwjAJBgNVHRMEAjAAMBEGCWCGSAGG +EIBAQQEAwIFoDAzBglghkgBhvhCAQ0EJhYkT3BlblNTTCBHZW5lcmF0ZWQgQ2xp ZW50IENlcnRpZmljYXRlMB0GA1UdDgQWBBTjsqsPdyaRUig5uBwDNOtY+MAMkDAf BgNVHSMEGDAWgBS96Nd21/ujY1YLoaLGA7dspLBPLTAOBgNVHQ8BAf8EBAMCBeAw HQYDVR0lBBYwFAYIKwYBBQUHAwIGCCsGAQUFBwMEMA0GCSqGSIb3DQEBCwUAA4IC AQCWvgO0b8sBLNeqcrUjOxKsFJ+n/fE/8mw8zFC498cteJ2yLR0FgnQ0U7kz7g23 JuaAvBgQuS5ONn7Ni0VZNXQ1Ib8pZbZHhFCxDqcG/P+cazmkEe65CcHf5++y3377 Xg+7NqyO2OdTr+QRY9ZlKeQGHBfXngRyRCNcJYT9hHFAzy2XpKDaCe23teUuNBVD tAWv3LvN072TjKM19szJQpm8d1WmBdJP9TtfMeAQmfSkZV3SemNqeIJcr0LFLCSv CvnW8YJEYoCDdhgtFW1fOcnhbZGMTtpCfRPyNgvgeWU3lrheCdcJTYd9T/dhIBgo hT2Cb2lsWWsKB34Y6Xmzmq/Nw/Z1CoFR6D5J9pipIVsMZdb28bs1ZcnrO6dWKiEQ crjXDDxOWUt24vOldZ1kHwRD1AQ0hzEvlxSvWcZ4NvKYhKI6baPZVSdDGTXlfvqI I+71BTAIrsJSysL0w5TVocdCzfCidI3mbBH8JL+ph5PhHcRBa6qizfWCuKeAhco3 qljNSRNXR5ORjQuPBv6GNHwwUilYLG8gl8x+yUpaqWWP+BGwNbDlMUuuizws9mEu 9fFw+vWtJVffRcfSoAAuGJE0oeLU9z9JcIT29OGssaKDWg2FYWlqnU5B0R5WrMAL 0OPl77Gymcpc9Tia5AhZVUpozUymdXjWc+K6A0ynr7Qxug==
                                        </Certificate>
                                        <ServiceDescription>AS4 Connectivity Test</ServiceDescription>
                                        <TechnicalContactUrl>CEF-EDELIVERY-SUPPORT@ec.europa.eu</TechnicalContactUrl>
                                        <TechnicalInformationUrl>http://cef.eu/contact</TechnicalInformationUrl>
                                    </Endpoint>
                                </ServiceEndpointList>
                            </Process>
                        </ProcessList>
                    </ServiceInformation>
                </ServiceMetadata>
            </SignedServiceMetadata>";
    }
}
