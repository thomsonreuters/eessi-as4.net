using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.Model.PMode;
using Heijden.DNS;
using NLog;
using Wiry.Base32;
using Party = Eu.EDelivery.AS4.Model.Core.Party;
using TransportType = Heijden.DNS.TransportType;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// Dynamic Discovery profile to retrieve a compliant eDelivery SMP profile based on the OASIS BDX Service Metadata Publishers (SMP)
    /// to extract information about the unknown receiver MSH. After a successful retrieval, the <see cref="SendingProcessingMode"/> can be extended
    /// with the endpoint address, service value/type, action, receiver party and the public encryption certificate of the receiving MSH.
    /// </summary>
    public class OasisDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private const string SmpHttpRegexPattern = ".*?(http.*[^!])";
        private static readonly Regex SmpHttpRegex = new Regex(SmpHttpRegexPattern, RegexOptions.Compiled);
        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();
        private static readonly HttpClient HttpClient = new HttpClient();
        private static readonly Resolver DnsResolver = new Resolver()
        {
            TransportType = TransportType.Udp,
            Recursion = true,
            Retries = 3
        };

        /// <summary>
        /// Gets the environment of the service provider to include in the DNS NAPTR lookup.
        /// </summary>
        /// ReSharper disable once UnassignedGetOnlyAutoProperty
        internal string ServiceProviderSubDomain { get; }

        /// <summary>
        /// Gets the service provider domain name for the DNS NAPTR lookup.
        /// </summary>
        /// ReSharper disable once UnassignedGetOnlyAutoProperty
        internal string ServiceProviderDomainName { get; }

        /// <summary>
        /// Gets the document identifier to append to the retrieved SMP URL.
        /// </summary>
        /// ReSharper disable once UnassignedGetOnlyAutoProperty
        internal string DocumentIdentifier { get; }

        /// <summary>
        /// Gets the document scheme to append to the retrieved SMP URL.
        /// </summary>
        /// ReSharper disable once UnassignedGetOnlyAutoProperty
        internal string DocumentScheme { get; }

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="party"/> using a given <paramref name="properties"/>.
        /// </summary>
        /// <param name="party">The party identifier to select the right SMP meta-data.</param>
        /// <param name="properties">The information properties specified in the <see cref="SendingProcessingMode"/> for this profile.</param>
        public async Task<XmlDocument> RetrieveSmpMetaDataAsync(Party party, IDictionary<string, string> properties)
        {
            if (party == null)
            {
                throw new ArgumentNullException(nameof(party));
            }

            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            if (!party.PartyIds.Any() || party.PartyIds.All(id => id.Type == Maybe<string>.Nothing))
            {
                throw new ArgumentException(
                    @"ToParty must have at least one PartyId element with a Type to retrieve the SMP meta-data",
                    nameof(party));
            }

            string Base32Encode(byte[] input)
            {
                return Base32Encoding.Standard.GetString(input);
            }

            byte[] SHA256Hash(string input)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    return sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
                }
            }

            string serviceProviderDomainName = properties.ReadMandatoryProperty(nameof(ServiceProviderDomainName));
            string environment = properties.ReadOptionalProperty(nameof(ServiceProviderSubDomain), String.Empty);
            string documentIdentifier = properties.ReadMandatoryProperty(nameof(DocumentIdentifier));
            string documentScheme = properties.ReadMandatoryProperty(nameof(DocumentScheme));

            Model.Core.PartyId participant = party.PartyIds.First(id => id.Type != Maybe<string>.Nothing);
            string participantIdentifier = participant.Id;
            string participantScheme = participant.Type.UnsafeGet;

            string dnsDomainName =
                $"{Base32Encode(SHA256Hash(participantIdentifier)).TrimEnd('=')}"
                + $".{participantScheme}"
                + $"{(String.IsNullOrWhiteSpace(environment) ? String.Empty : "." + environment)}"
                + $".{serviceProviderDomainName}";

            Response dnsResponse = DnsResolver.Query(dnsDomainName, QType.NAPTR, QClass.IN);
            Uri smpUri = SelectSmpUriFromDnsResponse(dnsResponse);

            string smpRestBinding =
                $"{smpUri}{participantScheme}::{participantIdentifier}/services/{documentScheme}::{documentIdentifier}";

            return await GetSmpRestBindingAsync(smpRestBinding);
        }

        private static Uri SelectSmpUriFromDnsResponse(Response dnsResponse)
        {
            RecordNAPTR firstMatchedNaptrRecord =
                dnsResponse.Answers
                           .Select(r => r.RECORD)
                           .Cast<RecordNAPTR>()
                           .FirstOrDefault();

            if (firstMatchedNaptrRecord == null)
            {
                throw new InvalidDataException(
                    "No DNS NAPTR record found to get the SMP REST binding from");
            }

            MatchCollection matches = SmpHttpRegex.Matches(firstMatchedNaptrRecord.REGEXP);
            if (matches.Count == 0)
            {
                throw new InvalidDataException(
                    $"DNS NAPTR record value REGEXP: \"{firstMatchedNaptrRecord.REGEXP}\" doesn't match regular expression: \"{SmpHttpRegexPattern}\"");
            }

            Match firstMatch = matches[0];
            if (firstMatch.Groups.Count < 2)
            {
                throw new InvalidDataException(
                    $"DNS NAPTR record value REGEXP: \"{firstMatchedNaptrRecord.REGEXP}\" doesn't match regular expression: \"{SmpHttpRegexPattern}\"");
            }

            // First group is always the entire matched string like "!^.*$!http://40.115.23.114:38080/".
            // Second group is always only the matched parts like "http://40.115.23.114:38080/"
            string matched = firstMatch.Groups[1].Value;
            return new Uri(matched);
        }

        private static async Task<XmlDocument> GetSmpRestBindingAsync(string binding)
        {
            using (HttpResponseMessage smpResponse = await HttpClient.GetAsync(binding))
            {
                if (!smpResponse.IsSuccessStatusCode)
                {
                    throw new WebException(
                        $"Calling the SMP server at {binding} doesn't result in an successful response");
                }

                Stream xmlStream = await smpResponse.Content.ReadAsStreamAsync();

                var smpMetaData = new XmlDocument();
                smpMetaData.Load(xmlStream);
                return smpMetaData;
            }
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode"/> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode"/></returns>
        public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (smpMetaData == null)
            {
                throw new ArgumentNullException(nameof(smpMetaData));
            }

            var ns = new XmlNamespaceManager(smpMetaData.NameTable);
            ns.AddNamespace("oasis", "http://docs.oasis-open.org/bdxr/ns/SMP/2016/05");

            XmlNode endpointNode = SelectEndpointNode(smpMetaData, ns);
            OverridePushConfigurationProtocolUrl(pmode, endpointNode, ns);
            OverrideMessageProperties(pmode, smpMetaData, ns);
            OverrideCollaborationAction(pmode, smpMetaData, ns);
            OverrideCollaborationService(pmode, smpMetaData, ns);

            XmlNode certificateNode = smpMetaData.SelectSingleNode("//oasis:Certificate", ns);
            if (certificateNode != null)
            {
                string certificateBinaries = certificateNode.InnerText.Replace(" ", "").Replace("\r\n", "");
                OverrideEncryptionCertificate(pmode, certificateBinaries);
                OverrideToParty(pmode, certificateBinaries);
            }
            else
            {
                Logger.Trace("Don't override MessagePackaging.PartyInfo.ToParty because no <Certificate/> element found in SMP meta-data");
                Logger.Trace("Don't override Encryption Certificate because no <Certificate/> element found in SMP meta-data");
            }

            return pmode;
        }

        private static XmlNode SelectEndpointNode(XmlDocument smpMetaData, XmlNamespaceManager ns)
        {
            // TODO: now the first matched tag is selected while we can select more strictly by matching UserMessages with <Process/> elements.
            XmlNode serviceEndpointListNode = smpMetaData.SelectSingleNode("//oasis:ServiceEndpointList", ns);
            if (serviceEndpointListNode == null)
            {
                throw new InvalidDataException("No <ServiceEndpointList/> element found in the SMP meta-data");
            }

            const string supportedTransportProfile = "bdxr-transport-ebms3-as4-v1p0";
            XmlNode endpointNode =
                serviceEndpointListNode.SelectSingleNode($"//oasis:Endpoint[@transportProfile='{supportedTransportProfile}']", ns);

            if (endpointNode == null)
            {
                IEnumerable<string> foundTransportProfiles =
                    serviceEndpointListNode.ChildNodes
                        .Cast<XmlNode>()
                        .Select(n => n?.Attributes?["transportProfile"]?.Value)
                        .Where(p => p != null);

                string foundTransportProfilesFormatted =
                    foundTransportProfiles.Any()
                        ? $"; did found: {String.Join(", ", foundTransportProfiles)} transport profiles"
                        : "; no other transport profiles were found";

                throw new InvalidDataException(
                    "No <Endpoint/> element in an <ServiceEndpointList/> element found in SMP meta-data "
                    + $"where the @transportProfile attribute is {supportedTransportProfile} {foundTransportProfilesFormatted}");
            }

            return endpointNode;
        }

        private static void OverridePushConfigurationProtocolUrl(SendingProcessingMode pmode, XmlNode endpointNode, XmlNamespaceManager ns)
        {
            XmlNode endpointUriNode = endpointNode.SelectSingleNode("//oasis:EndpointURI", ns);
            if (endpointUriNode == null)
            {
                throw new InvalidDataException(
                    "No <EndpointURI/> element in an ServiceEndpointList.Endpoint element found in SMP meta-data");
            }

            string endpointUri = endpointUriNode.InnerText.Replace(" ", "").Replace("\r\n", "");

            Logger.Trace($"Override SendingPMode.PushConfiguration.Protocol.Url with {endpointUri}");
            pmode.PushConfiguration = pmode.PushConfiguration ?? new PushConfiguration();
            pmode.PushConfiguration.Protocol = pmode.PushConfiguration.Protocol ?? new Protocol();
            pmode.PushConfiguration.Protocol.Url = endpointUri;
        }

        private static void OverrideCollaborationAction(SendingProcessingMode pmode, XmlDocument smpMetaData, XmlNamespaceManager ns)
        {
            XmlNode documentIdentifierNode = smpMetaData.SelectSingleNode("//oasis:ServiceInformation/oasis:DocumentIdentifier", ns);
            if (documentIdentifierNode == null)
            {
                throw new InvalidDataException(
                    "No <DocumentIdentifier/> element in an <ServiceInformation/> element found in SMP meta-data");
            }

            Logger.Trace($"Override SendingPMode.MessagePackaging.CollaborationInfo.Action with {documentIdentifierNode.InnerText}");
            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.CollaborationInfo = pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Action = documentIdentifierNode.InnerText;
        }

        private static void OverrideCollaborationService(SendingProcessingMode pmode, XmlDocument smpMetaData, XmlNamespaceManager ns)
        {
            XmlNode processIdentifierNode =
                smpMetaData.SelectSingleNode("//oasis:ProcessList/oasis:Process/oasis:ProcessIdentifier", ns);

            if (processIdentifierNode == null)
            {
                throw new InvalidDataException(
                    "No <ProcessIdentifier/> in an ProcessList.Process element found in SMP meta-data");
            }

            string serviceType =
                processIdentifierNode.Attributes
                    ?.OfType<XmlAttribute>()
                    .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase))
                    ?.Value;

            Logger.Trace(
                "Override SendingPMode.MessagePackaging.CollaborationInfo.Service with "
                + $"{{Value={processIdentifierNode.InnerText}, Type={serviceType}}}");

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.CollaborationInfo = pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Service = new Service { Value = processIdentifierNode.InnerText, Type = serviceType };
        }

        private static void OverrideMessageProperties(SendingProcessingMode pmode,  XmlNode smpMetaData, XmlNamespaceManager ns)
        {
            bool IsFinalRecipient(MessageProperty p)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(p?.Name, "finalRecipient");
            }

            bool IsOriginalSender(MessageProperty p)
            {
                return StringComparer.OrdinalIgnoreCase.Equals(p?.Name, "originalSender");
            }

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.MessageProperties = pmode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();
            pmode.MessagePackaging.MessageProperties.RemoveAll(IsFinalRecipient);
            pmode.MessagePackaging.MessageProperties.Add(CreateFinalRecipient(smpMetaData, ns));
            if (!pmode.MessagePackaging.MessageProperties.Any(IsOriginalSender))
            {
                pmode.MessagePackaging.MessageProperties.Add(CreateOriginalSender());
            }
        }

        private static MessageProperty CreateFinalRecipient(XmlNode smpMetaData, XmlNamespaceManager ns)
        {
            XmlNode participantIdentifierNode = 
                smpMetaData.SelectSingleNode("//oasis:ServiceInformation/oasis:ParticipantIdentifier", ns);

            if (participantIdentifierNode == null)
            {
                throw new InvalidDataException("No ParticipantIdentifier element found in SMP meta-data");
            }

            string participantIdentifier =
                participantIdentifierNode.InnerText.Trim();


            string schemeAttribute =
                participantIdentifierNode.Attributes
                    ?.OfType<XmlAttribute>()
                    .FirstOrDefault(a => StringComparer.OrdinalIgnoreCase.Equals(a.Name, "scheme"))
                    ?.Value;
            
            Logger.Trace($"Add MessageProperty 'finalRecipient' = '{participantIdentifier}' to SendingPMode");
            return new MessageProperty
            {
                Name = "finalRecipient",
                Value = participantIdentifier,
                Type = schemeAttribute
            };
        }

        private static MessageProperty CreateOriginalSender()
        {
            const string defaultUrnTypeValueC1 = "urn:oasis:names:tc:ebcore:partyid-type:unregistered:C1";
            Logger.Trace($"Add MessageProperty 'originalSender'= '{defaultUrnTypeValueC1}' to SendingPMode");

            return new MessageProperty
            {
                Name = "originalSender",
                Value = defaultUrnTypeValueC1
            };
        }

        private static void OverrideEncryptionCertificate(SendingProcessingMode pmode, string certificateBinaries)
        {
            Logger.Trace("Override SendingPMode.Security.Encryption with {Certificate=PublicKeyCertificate}");
            pmode.Security = pmode.Security ?? new Model.PMode.Security();
            pmode.Security.Encryption = pmode.Security.Encryption ?? new Encryption();
            pmode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;
            pmode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate { Certificate = certificateBinaries };
        }

        private static void OverrideToParty(SendingProcessingMode pmode, string certificateBinaries)
        {
            const string defaultResponderRole = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/responder";
            const string defaultUrnTypeValue = "urn:oasis:names:tc:ebcore:partyid-type:unregistered";

            var encryptionCertificate =
                new X509Certificate2(
                    rawData: Convert.FromBase64String(certificateBinaries),
                    password: (string)null);
            string commonName = encryptionCertificate.GetNameInfo(X509NameType.SimpleName, forIssuer: false);

            Logger.Trace(
                "Override SendingPMode.MessagingPackaging.PartyInfo.ToParty with "
                + $"{{Role={defaultResponderRole}, PartyId={commonName}, PartyIdType={defaultUrnTypeValue}}}");

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.PartyInfo = pmode.MessagePackaging.PartyInfo ?? new PartyInfo();
            pmode.MessagePackaging.PartyInfo.ToParty = new Model.PMode.Party
            {
                Role = defaultResponderRole,
                PartyIds = new List<PartyId>
                {
                    new PartyId { Id = commonName, Type = defaultUrnTypeValue }
                }
            };
        }
    }
}
