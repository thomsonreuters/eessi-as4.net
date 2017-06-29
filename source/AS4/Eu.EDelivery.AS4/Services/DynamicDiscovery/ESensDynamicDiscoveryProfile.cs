using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    public class ESensDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        /// <summary>
        /// Create the Uri to the SMP Server that must be contacted to GET the SMP Meta-Data
        /// </summary>
        /// <param name="partyId"></param>
        /// <param name="dynamicDiscoveryConfiguration"></param>
        /// <returns></returns>
        public Uri CreateSmpServerUri(string partyId, DynamicDiscoveryConfiguration dynamicDiscoveryConfiguration)
        {
            if (String.IsNullOrWhiteSpace(dynamicDiscoveryConfiguration.SmlScheme))
            {
                throw new ArgumentException("The SmlScheme is not specified");
            }
            if (String.IsNullOrWhiteSpace(dynamicDiscoveryConfiguration.SmpServerDomainName))
            {
                throw new ArgumentException("The SmpServerDomainName is not specified");
            }
            if (String.IsNullOrWhiteSpace(dynamicDiscoveryConfiguration.DocumentIdentifierScheme))
            {
                throw new ArgumentException("The DocumentIdentifierScheme is not specified");
            }
            if (String.IsNullOrWhiteSpace(dynamicDiscoveryConfiguration.DocumentIdentifier))
            {
                throw new ArgumentException("The DocumentIdentifier is not specified");
            }

            string hashedPartyId = CalculateMD5Hash(partyId);

            string trimmedSmlScheme = dynamicDiscoveryConfiguration.SmlScheme.Trim('.');
            string trimmedDomainName = dynamicDiscoveryConfiguration.SmpServerDomainName.Trim('.');
            string trimmedDocIdScheme = dynamicDiscoveryConfiguration.DocumentIdentifierScheme.Trim('.');
            string trimmedDocId = dynamicDiscoveryConfiguration.DocumentIdentifier.Trim('.');

            var host = $"b-{hashedPartyId}.{trimmedSmlScheme}.{trimmedDomainName}";

            var path = $"{trimmedSmlScheme}::{partyId}/services/{trimmedDocIdScheme}::{trimmedDocId}";

            UriBuilder b = new UriBuilder
            {
                Host = host,
                Path = path
            };

            return b.Uri;
        }

        private static string CalculateMD5Hash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                byte[] hash = md5.ComputeHash(inputBytes);

                StringBuilder sb = new StringBuilder();

                foreach (byte t in hash)
                {
                    sb.Append(t.ToString("X2"));
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Complete the <paramref name="pmode"/> with the SMP metadata that is present in the <paramref name="smpMetaData"/> <see cref="XmlDocument"/>
        /// </summary>
        /// <param name="pmode"></param>
        /// <param name="smpMetaData"></param>
        /// <returns></returns>
        public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            CompleteMessageProperties(pmode, smpMetaData);

            CompleteCollaborationInfo(pmode, smpMetaData);

            CompleteSendConfiguration(pmode, smpMetaData);

            return pmode;
        }

        private static void CompleteMessageProperties(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            if (sendingPMode.MessagePackaging.MessageProperties == null)
            {
                sendingPMode.MessagePackaging.MessageProperties = new List<MessageProperty>();
            }

            var finalRecipient = GetFinalRecipient(smpMetaData);

            var existingFinalRecipient =
                sendingPMode.MessagePackaging?.MessageProperties?.FirstOrDefault(
                    p => p.Name.Equals("finalRecipient", StringComparison.OrdinalIgnoreCase));

            if (existingFinalRecipient != null)
            {
                sendingPMode.MessagePackaging.MessageProperties.Remove(existingFinalRecipient);
            }

            sendingPMode.MessagePackaging.MessageProperties.Add(finalRecipient);

            var existingOriginalSender =
                sendingPMode.MessagePackaging?.MessageProperties?.FirstOrDefault(
                    p => p.Name.Equals("originalSender", StringComparison.OrdinalIgnoreCase));

            if (existingOriginalSender == null)
            {
                var originalSender = new MessageProperty("originalSender", "urn:oasis:names:tc:ebcore:partyid-type:unregistered:C1");
                sendingPMode.MessagePackaging.MessageProperties.Add(originalSender);
            }
        }

        private static MessageProperty GetFinalRecipient(XmlDocument smpMetaData)
        {
            var node = smpMetaData.SelectSingleNode("//*[local-name()='ParticipantIdentifier']");

            if (node == null)
            {
                throw new InvalidDataException("No ParticipantIdentifier element found in SMP meta-data");
            }

            var finalRecipient = new MessageProperty("finalRecipient", node.InnerText);

            var schemeAttribute =
                node.Attributes?.OfType<XmlAttribute>().FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase));

            if (schemeAttribute != null)
            {
                finalRecipient.Type = schemeAttribute.Value;
            }

            return finalRecipient;
        }
        
        private static void CompleteCollaborationInfo(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            if (sendingPMode.MessagePackaging.CollaborationInfo == null)
            {
                sendingPMode.MessagePackaging.CollaborationInfo = new CollaborationInfo();
            }

            var processIdentifier =
                smpMetaData.SelectSingleNode("//*[local-name()='ProcessList']/*[local-name()='Process']/*[local-name()='ProcessIdentifier']");

            if (processIdentifier == null)
            {
                throw new ConfigurationErrorsException("Unable to complete CollaborationInfo: ProcessIdentifier element not found in SMP metadata");
            }

            string serviceValue = processIdentifier.InnerText;
            string serviceType = null;

            var schemeAttribute =
                processIdentifier.Attributes?.OfType<XmlAttribute>()
                                 .FirstOrDefault(a => a.Name.Equals("scheme", StringComparison.OrdinalIgnoreCase));

            if (schemeAttribute != null)
            {
                serviceType = schemeAttribute.Value;
            }

            sendingPMode.MessagePackaging.CollaborationInfo.Service = new Service()
            {
                Value = serviceValue,
                Type = serviceType
            };

            var documentIdentifier =
                smpMetaData.SelectSingleNode("//*[local-name()='ServiceInformation']/*[local-name()='DocumentIdentifier']");

            if (documentIdentifier == null)
            {
                throw new ConfigurationErrorsException("Unable to complete CollaborationInfo: DocumentIdentifier element not found in SMP metadata");
            }

            sendingPMode.MessagePackaging.CollaborationInfo.Action = documentIdentifier.InnerText;
        }

        private static void CompleteSendConfiguration(SendingProcessingMode sendingPMode, XmlDocument smpMetaData)
        {
            var endPoint = smpMetaData.SelectSingleNode("//*[local-name()='ServiceEndpointList']/*[local-name()='Endpoint' and @transportProfile='bdxr-transport-ebms3-as4-v1p0']");

            if (endPoint == null)
            {
                throw new InvalidDataException("No ServiceEndpointList/Endpoint element found in SMP meta-data");
            }

            var address = endPoint.SelectSingleNode("*[local-name()='EndpointReference']/*[local-name()='Address']");

            if (address == null)
            {
                throw new InvalidDataException("No ServiceEndpointList/Endpoint/EndpointReference/Address element found in SMP meta-data");
            }

            if (sendingPMode.PushConfiguration == null)
            {
                sendingPMode.PushConfiguration = new PushConfiguration();
            }

            sendingPMode.PushConfiguration.Protocol = new Protocol
            {
                Url = address.InnerText
            };

            var certificateNode = endPoint.SelectSingleNode("*[local-name()='Certificate']");

            if (certificateNode != null)
            {
                sendingPMode.Security.Encryption.PublicKeyInformation = new PublicKeyCertificate
                {
                    Certificate = certificateNode.InnerText
                };
                sendingPMode.Security.Encryption.PublicKeyType = PublicKeyChoiceType.PublicKeyCertificate;

                X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(certificateNode.InnerText), (string)null);

                sendingPMode.MessagePackaging.PartyInfo.ToParty = new Party();
                sendingPMode.MessagePackaging.PartyInfo.ToParty.Role = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/responder";
                sendingPMode.MessagePackaging.PartyInfo.ToParty.PartyIds.Add(new PartyId()
                {
                    Id = cert.GetNameInfo(X509NameType.SimpleName, false),
                    Type = "urn:oasis:names:tc:ebcore:partyid-type:unregistered"
                });                
            }
        }
    }
}