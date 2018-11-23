using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Xml;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using NLog;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// Dynamic Discovery profile to retrieve a compliant eDelivery SMP profile based on the OpenPEPPOL BIS/CEN BII Service Metadata Publishers (SMP)
    /// to extract information about the unknown receiver MSH. After a successful retrieval, the <see cref="SendingProcessingMode"/> can be extended
    /// with the endpoint address, service value/type, action, receiver party and the public encryption certificate of the receiving MSH.
    /// </summary>
    public class PeppolDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private const string DocumentIdentifier = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biitrns010:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1";
        private const string DocumentIdentifierScheme = "busdox-docid-qns";

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient = new HttpClient();

        [Info("SML Scheme", defaultValue: "iso6523-actorid-upis")]
        [Description("Used to build the SML Uri")]
        // Property is used to determine the configuration options via reflection
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        private string SmlScheme { get; }

        [Info("SMP Server Domain Name", defaultValue: "isaitb.acc.edelivery.tech.ec.europa.eu")]
        [Description("Domain name that must be used in the Uri")]
        // Property is used to determine the configuration options via reflection
        // ReSharper disable once UnusedMember.Local
        // ReSharper disable once UnassignedGetOnlyAutoProperty
        private string SmpServerDomainName { get; }

        private class ESensConfig
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="ESensConfig"/> class.
            /// </summary>
            /// <param name="smlScheme">The SML scheme.</param>
            /// <param name="smpServerDomainName">Name of the SMP server domain.</param>
            private ESensConfig(string smlScheme, string smpServerDomainName)
            {
                if (smlScheme == null)
                {
                    throw new ArgumentNullException(nameof(smlScheme));
                }

                if (smpServerDomainName == null)
                {
                    throw new ArgumentNullException(nameof(smpServerDomainName));
                }

                SmlScheme = smlScheme;
                SmpServerDomainName = smpServerDomainName;
            }

            public string SmlScheme { get; }

            public string SmpServerDomainName { get; }

            /// <summary>
            /// Creates a <see cref="ESensConfig"/> configuration data object from a set of given <paramref name="properties"/>.
            /// </summary>
            /// <param name="properties">The custom defined properties.</param>
            /// <returns></returns>
            public static ESensConfig From(IDictionary<string, string> properties)
            {
                string TrimDots(string s)
                {
                    return s.Trim('.');
                }

                return new ESensConfig(
                    TrimDots(properties.ReadOptionalProperty("SmlScheme", "iso6523-actorid-upis")),
                    TrimDots(properties.ReadOptionalProperty("SmpServerDomainName", "isaitb.acc.edelivery.tech.ec.europa.eu")));
            }
        }

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier to select the right SMP meta-data.</param>
        /// <param name="properties">The information properties specified in the <see cref="SendingProcessingMode"/> for this profile.</param>
        public async Task<XmlDocument> RetrieveSmpMetaDataAsync(
            Model.Core.Party party,
            IDictionary<string, string> properties)
        {
            if (party == null)
            {
                throw new ArgumentNullException(nameof(party));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (party.PrimaryPartyId == null)
            {
                throw new InvalidOperationException("Given invalid 'ToParty'; requires a 'PartyId'");
            }

            Uri smpUrl = CreateSmpServerUrl(party, ESensConfig.From(properties));

            return await RetrieveSmpMetaData(smpUrl);
        }

        private static Uri CreateSmpServerUrl(Model.Core.Party party, ESensConfig config)
        {
            string hashedPartyId = CalculateMD5Hash(party.PrimaryPartyId);

            string host = $"b-{hashedPartyId}.{config.SmlScheme}.{config.SmpServerDomainName}";
            string path = $"{config.SmlScheme}::{party.PrimaryPartyId}/services/{DocumentIdentifierScheme}::{DocumentIdentifier}";


            var builder = new UriBuilder
            {
                Host = host,
                // DotNetBug: Colons need to be Percentage encoded in final Url for SMP lookup. 
                // Uri/HttpClient.GetAsync components encodes # but not : so we need to do it manually.
                Path = HttpUtility.UrlEncode(path)
            };

            return builder.Uri;
        }

        private static string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);
                byte[] hash = md5.ComputeHash(inputBytes);

                var sb = new StringBuilder();

                foreach (byte t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        private static async Task<XmlDocument> RetrieveSmpMetaData(Uri smpServerUri)
        {
            Logger.Info($"Contacting SMP server at {smpServerUri} to retrieve meta-data");

            HttpResponseMessage response = await HttpClient.GetAsync(smpServerUri);

            if (response.StatusCode != HttpStatusCode.OK)
            {
                throw new HttpListenerException((int)response.StatusCode, "Unexpected result returned from SMP Service");
            }

            if (response.Content.Headers.ContentType.MediaType.IndexOf("xml", StringComparison.OrdinalIgnoreCase) == -1)
            {
                throw new NotSupportedException($"An XML response was expected from the SMP server instead of {response.Content.Headers.ContentType.MediaType}");
            }

            var result = new XmlDocument();
            result.Load(await response.Content.ReadAsStreamAsync());

            return result;
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="smpMetaData"></param>
        /// <returns></returns>
        public DynamicDiscoveryResult DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (smpMetaData == null)
            {
                throw new ArgumentNullException(nameof(smpMetaData));
            }

            XmlNode endpoint = SelectServiceEndpointNode(smpMetaData);
            XmlNode certificateNode = endpoint.SelectSingleNode("*[local-name()='Certificate']");

            Logger.Debug($"Decorate SendingPMode {pmode.Id} with SMP response from ESens SMP Server");

            OverwritePushProtocolUrl(pmode, endpoint);
            DecorateMessageProperties(pmode, smpMetaData);
            OverwriteCollaborationServiceAction(pmode, smpMetaData);

            if (certificateNode != null)
            {
                OverwriteToParty(pmode, certificateNode);
                OverwriteEncryptionCertificate(pmode, certificateNode);
            }
            else
            {
                Logger.Trace("Don't override MessagePackaging.PartyInfo.ToParty because no <Certificate/> element found in SMP response");
                Logger.Trace("Don't override Encryption Certificate because no <Certificate/> element found in SMP response");
            }

            // TODO: should we specify to override the ToParty here also?
            return DynamicDiscoveryResult.Create(pmode);
        }

        private static XmlNode SelectServiceEndpointNode(XmlNode smpMetaData)
        {
            XmlNode serviceEndpointList =
                smpMetaData.SelectSingleNode("//*[local-name()='ServiceEndpointList']");

            if (serviceEndpointList == null)
            {
                throw new InvalidDataException(
                    "No <ServiceEndpointList/> element found in the SMP meta-data");
            }

            const string supportedTransportProfile = "bdxr-transport-ebms3-as4-v1p0";
            XmlNode endPoint =
                smpMetaData.SelectSingleNode(
                    $"//*[local-name()='ServiceEndpointList']/*[local-name()='Endpoint' and @transportProfile='{supportedTransportProfile}']");

            IEnumerable<string> foundTransportProfiles =
                serviceEndpointList
                    .ChildNodes
                    .Cast<XmlNode>()
                    .Select(n => n?.Attributes?["transportProfile"]?.Value)
                    .Where(p => p != null);

            if (endPoint == null)
            {
                string foundTransportProfilesFormatted =
                    foundTransportProfiles.Any()
                        ? $"; did found: {String.Join(", ", foundTransportProfiles)} transport profiles"
                        : "; no other transport profiles were found";

                throw new InvalidDataException(
                    "No <Endpoint/> element in an <ServiceEndpointList/> element found in SMP meta-data "
                    + $"where the @transportProfile attribute is {supportedTransportProfile}"
                    + foundTransportProfilesFormatted);
            }

            return endPoint;
        }

        private static void OverwritePushProtocolUrl(SendingProcessingMode pmode, XmlNode endpoint)
        {
            pmode.PushConfiguration = pmode.PushConfiguration ?? new PushConfiguration();
            pmode.PushConfiguration.Protocol = pmode.PushConfiguration.Protocol ?? new Protocol();
            pmode.PushConfiguration.Protocol.Url = SelectEndpointAddress(endpoint).InnerText;
        }

        private static XmlNode SelectEndpointAddress(XmlNode endpoint)
        {
            XmlNode address = endpoint.SelectSingleNode("*[local-name()='EndpointReference']/*[local-name()='Address']");
            if (address == null)
            {
                throw new InvalidDataException(
                    "No ServiceEndpointList/Endpoint/EndpointReference/Address element found in SMP meta-data");
            }

            Logger.Trace($"Override SendingPMode.PushConfiguration.Protocol with {{Url={address.InnerText}}}");
            return address;
        }

        private static void DecorateMessageProperties(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            bool IsFinalReceipient(MessageProperty p)
            {
                return p?.Name?.Equals("finalRecipient", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            bool IsOriginalSender(MessageProperty p)
            {
                return p?.Name?.Equals("originalSender", StringComparison.OrdinalIgnoreCase) ?? false;
            }

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.MessageProperties = pmode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();
            pmode.MessagePackaging.MessageProperties.RemoveAll(IsFinalReceipient);
            pmode.MessagePackaging.MessageProperties.Add(CreateFinalRecipient(smpMetaData));
            if (!pmode.MessagePackaging.MessageProperties.Any(IsOriginalSender))
            {
                pmode.MessagePackaging.MessageProperties.Add(CreateOriginalSender());
            }
        }

        private static MessageProperty CreateFinalRecipient(XmlNode smpMetaData)
        {
            XmlNode node = smpMetaData.SelectSingleNode("//*[local-name()='ParticipantIdentifier']");
            if (node == null)
            {
                throw new InvalidDataException("No ParticipantIdentifier element found in SMP meta-data");
            }

            string schemeAttribute =
                node.Attributes?
                    .OfType<XmlAttribute>()
                    .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase))
                    ?.Value;

            Logger.Trace("Add MessageProperty 'finalRecipient' to SendingPMode");
            return new MessageProperty
            {
                Name = "finalRecipient",
                Value = node.InnerText,
                Type = schemeAttribute
            };
        }

        private static MessageProperty CreateOriginalSender()
        {
            Logger.Trace("Add MessageProperty 'originalSender' to SendingPMode");
            return new MessageProperty
            {
                Name = "originalSender",
                Value = "urn:oasis:names:tc:ebcore:partyid-type:unregistered:C1"
            };
        }

        private static void OverwriteCollaborationServiceAction(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.CollaborationInfo = pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Action = SelectCollaborationAction(smpMetaData);
            pmode.MessagePackaging.CollaborationInfo.Service = SelectCollaborationService(smpMetaData);
        }

        private static string SelectCollaborationAction(XmlNode smpMetaData)
        {
            XmlNode documentIdentifier =
                smpMetaData.SelectSingleNode(
                    "//*[local-name()='ServiceInformation']/*[local-name()='DocumentIdentifier']");

            if (documentIdentifier == null)
            {
                throw new InvalidDataException(
                    "Unable to complete CollaborationInfo: no ServiceInformation/DocumentIdentifier element not found in SMP metadata");
            }

            Logger.Trace($"Override SendingPMode.MessagingPackaging.CollaborationInfo with {{Action={documentIdentifier.InnerText}}}");
            return documentIdentifier.InnerText;
        }

        private static Service SelectCollaborationService(XmlNode smpMetaData)
        {
            XmlNode processIdentifier =
                smpMetaData.SelectSingleNode(
                    "//*[local-name()='ProcessList']/*[local-name()='Process']/*[local-name()='ProcessIdentifier']");

            if (processIdentifier == null)
            {
                throw new InvalidDataException(
                    "Unable to complete CollaborationInfo: ProcessList/ProcessIdentifier element not found in SMP metadata");
            }

            string serviceValue = processIdentifier.InnerText;
            string serviceType =
                processIdentifier
                    .Attributes
                    ?.OfType<XmlAttribute>()
                    .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase))
                    ?.Value;

            Logger.Trace($"Override SendingPMode.MessagingPackaging.CollaborationInfo with {{ServiceType={serviceType}, ServiceValue={serviceValue}}}");
            return new Service
            {
                Value = serviceValue,
                Type = serviceType
            };
        }

        private static void OverwriteToParty(SendingProcessingMode pmode, XmlNode certificateNode)
        {
            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.PartyInfo = pmode.MessagePackaging.PartyInfo ?? new PartyInfo();

            var cert = new X509Certificate2(
                rawData: Convert.FromBase64String(certificateNode.InnerText),
                password: (string)null);

            const string responderRole = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/responder";
            Logger.Trace($"Override MessagingPackaging.PartyInfo.ToParty with {{Role={responderRole}}}");

            pmode.MessagePackaging.PartyInfo.ToParty = new Party(
                role: responderRole,
                partyId: new PartyId(
                    id: cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false))
                {
                    Type = "urn:oasis:names:tc:ebcore:partyid-type:unregistered"
                });
        }

        private static void OverwriteEncryptionCertificate(SendingProcessingMode pmode, XmlNode certificateNode)
        {
            Logger.Trace("Override SendingPMode.Security.Encryption with {CertificateType=PublicKeyCertificate}");
            pmode.Security = pmode.Security ?? new Model.PMode.Security();
            pmode.Security.Encryption = pmode.Security.Encryption ?? new Encryption();

            pmode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;
            pmode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate
            {
                Certificate = certificateNode.InnerText
            };
        }
    }
}
