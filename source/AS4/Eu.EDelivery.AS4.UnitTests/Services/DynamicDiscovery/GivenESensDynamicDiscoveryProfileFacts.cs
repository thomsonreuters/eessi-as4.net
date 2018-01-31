using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Services.DynamicDiscovery;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Services.DynamicDiscovery
{
    public class GivenESensDynamicDiscoveryProfileFacts
    {
        [Fact]
        public async Task FailsToRetrieveSmpMetaData_IfPartyIsInvalid()
        {
            // Arrange
            var sut = new ESensDynamicDiscoveryProfile();

            // Act / Assert
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => sut.RetrieveSmpMetaData(new Party(partyId: new PartyId(id: null)), properties: null));
        }

        [Fact]
        public void DecorateSendingPModeWithSMPResponse()
        {
            // Arrange
            var sut = new ESensDynamicDiscoveryProfile();

            var pmode = new SendingProcessingMode();
            var smpMetaData = new XmlDocument();
            smpMetaData.LoadXml(SMPResponse());

            // Act
            SendingProcessingMode actual = sut.DecoratePModeWithSmpMetaData(pmode, smpMetaData);

            // Assert
            Assert.NotEmpty(actual.PushConfiguration.Protocol.Url);
            Assert.NotEmpty(actual.MessagePackaging.PartyInfo.ToParty.PartyIds);

            IEnumerable<string> propNames = actual.MessagePackaging.MessageProperties.Select(p => p.Name);
            Assert.Contains(propNames, p => p == "finalRecipient" || p == "originalSender");
        }

        private static string SMPResponse()
        {
            return @"<?xml version=""1.0"" encoding=""UTF-8"" standalone=""no""?>
                <SignedServiceMetadata 
                    xmlns=""http://docs.oasis-open.org/bdxr/ns/SMP/2016/05"">
                    <ServiceMetadata>
                        <ServiceInformation>
                            <ParticipantIdentifier scheme=""iso6523-actorid-upis"">9915:123456789</ParticipantIdentifier>
                            <DocumentIdentifier scheme=""bdxr-docid-qns"">urn:oasis:names:specification:ubl:schema:xsd:CreditNote-2::CreditNote##urn:www.cenbii.eu:transaction:biitrns014:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1</DocumentIdentifier>
                            <ProcessList>
                                <Process>
                                    <ProcessIdentifier scheme=""cenbii-procid-ubl"">urn:www.cenbii.eu:profile:bii05:ver2.0</ProcessIdentifier>
                                    <ServiceEndpointList>
                                        <Endpoint transportProfile=""bdxr-transport-ebms3-as4-v1p0"">
                                            <EndpointReference>
                                                <Address>https://test.erechnung.gv.at/as4/msh/</Address>
                                            </EndpointReference>
                                            <RequireBusinessLevelSignature>false</RequireBusinessLevelSignature>
                                            <Certificate>MIID7jCCA1egAwIBAgICA+YwDQYJKoZIhvcNAQENBQAwOjELMAkGA1UEBhMCRlIxEzARBgNVBAoMCklIRSBFdXJvcGUxFjAUBgNVBAMMDUlIRSBFdXJvcGUgQ0EwHhcNMTYwNjAxMTQzNTUzWhcNMjYwNjAxMTQzNTUzWjCBgzELMAkGA1UEBhMCUFQxDDAKBgNVBAoMA01vSDENMAsGA1UECwwEU1BNUzENMAsGA1UEKgwESm9hbzEOMAwGA1UEBRMFQ3VuaGExHTAbBgNVBAMMFHFhZXBzb3MubWluLXNhdWRlLnB0MRkwFwYDVQQMDBBTZXJ2aWNlIFByb3ZpZGVyMIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEA1eN4qPSSRZqjVFG9TlcPlxf2WiSimQK9L1nf9Z/s0ezeGQjCukDeDq/Wzqd9fpHhaMMq+XSSOtyEtIr5K/As4kFrViONUUkG12J6UllSWogp0NYFwA4wIqKSFiTnQS5/nRTs05oONCCGILCyJNNeO53JzPlaq3/QbPLssuSAr6XucPE8wBBGM8b/TsB2G/zjG8yuSTgGbhaZekq/Vnf9ftj1fr/vJDDAQgH6Yvzd88Z0DACJPHfW1p4F/OWLI386Bq7g/bo1DUPAyEwlf+CkLgJWRKki3yJlOCIZ9enMA5O7rfeG3rXdgYGmWS7tNEgKXxgC+heiYvi7ZWd7M+/SUwIDAQABo4IBMzCCAS8wPgYDVR0fBDcwNTAzoDGgL4YtaHR0cHM6Ly9nYXplbGxlLmloZS5uZXQvcGtpL2NybC82NDMvY2FjcmwuY3JsMDwGCWCGSAGG+EIBBAQvFi1odHRwczovL2dhemVsbGUuaWhlLm5ldC9wa2kvY3JsLzY0My9jYWNybC5jcmwwPAYJYIZIAYb4QgEDBC8WLWh0dHBzOi8vZ2F6ZWxsZS5paGUubmV0L3BraS9jcmwvNjQzL2NhY3JsLmNybDAfBgNVHSMEGDAWgBTsMw4TyCJeouFrr0N7el3Sd3MdfjAdBgNVHQ4EFgQU1GQ/K1ykIwWFgiONzWJLQzufF/8wDAYDVR0TAQH/BAIwADAOBgNVHQ8BAf8EBAMCBSAwEwYDVR0lBAwwCgYIKwYBBQUHAwEwDQYJKoZIhvcNAQENBQADgYEAZ7t1Qkr9wz3q6+WcF6p/YX7Jr0CzVe7w58FvJFk2AsHeYkSlOyO5hxNpQbs1L1v6JrcqziNFrh2QKGT2v6iPdWtdCT8HBLjmuvVWxxnfzYjdQ0J+kdKMAEV6EtWU78OqL60CCtUZKXE/NKJUq7TTUCFP2fwiARy/t1dTD2NZo8c=
                                            </Certificate>
                                            <ServiceDescription>BRZ Test AP</ServiceDescription>
                                        </Endpoint>
                                    </ServiceEndpointList>
                                </Process>
                            </ProcessList>
                        </ServiceInformation>
                    </ServiceMetadata>
                </SignedServiceMetadata>";
        }
    }
}
