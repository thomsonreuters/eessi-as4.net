using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// Dynamic Discovery profile that queries the local configuration to look for the right SMP info to complete the <see cref="SendingProcessingMode"/>.
    /// </summary>
    /// <seealso cref="IDynamicDiscoveryProfile" />
    public class LocalDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private readonly Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDynamicDiscoveryProfile"/> class.
        /// </summary>
        public LocalDynamicDiscoveryProfile() : this(Registry.Instance.CreateDatastoreContext) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDynamicDiscoveryProfile" /> class.
        /// </summary>
        /// <param name="createDatastore">The delegation of the creation of the datastore.</param>
        public LocalDynamicDiscoveryProfile(Func<DatastoreContext> createDatastore)
        {
            _createDatastore = createDatastore;
        }

        /// <summary>
        /// Retrieves the SMP meta data <see cref="XmlDocument"/> for a given <paramref name="partyId"/> using a given <paramref name="config"/>.
        /// </summary>
        /// <param name="partyId">The party identifier.</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Task<XmlDocument> RetrieveSmpMetaData(string partyId, IDictionary<string, string> properties)
        {
            SmpResponse response = FindSmpResponseForToPartyId(partyId);
            string xml = AS4XmlSerializer.ToString(response);

            var document = new XmlDocument();
            document.LoadXml(xml);

            return Task.FromResult(document);
        }

        private SmpResponse FindSmpResponseForToPartyId(string partyId)
        {
            using (DatastoreContext context = _createDatastore())
            {
                SmpResponse foundResponse = context.SmpResponses.FirstOrDefault(r => r.ToPartyId == partyId);

                if (foundResponse == null)
                {
                    throw new ConfigurationErrorsException(
                        "No SMP Response found for the given 'ToPartyId': " + partyId);
                }

                return foundResponse;
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
            var smpResponse = AS4XmlSerializer.FromString<SmpResponse>(smpMetaData.OuterXml);

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.PartyInfo = pmode.MessagePackaging.PartyInfo ?? new PartyInfo();
            pmode.MessagePackaging.PartyInfo.ToParty = new Party(smpResponse.PartyRole, new PartyId(smpResponse.ToPartyId));

            pmode.MessagePackaging.MessageProperties =
                pmode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();
            pmode.MessagePackaging.MessageProperties.Add(new MessageProperty("finalRecipient", smpResponse.FinalRecipient));

            pmode.MessagePackaging.CollaborationInfo =
                pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Service = new Service {Type = smpResponse.ServiceType, Value = smpResponse.ServiceValue};
            pmode.MessagePackaging.CollaborationInfo.Action = smpResponse.Action;

            pmode.PushConfiguration = new PushConfiguration
            {
                Protocol = new Protocol {Url = smpResponse.Url},
                TlsConfiguration = new TlsConfiguration
                {
                    IsEnabled = smpResponse.TlsEnabled
                }
            };

            pmode.Security = pmode.Security ?? new Model.PMode.Security();
            pmode.Security.Encryption = new Encryption
            {
                IsEnabled = smpResponse.EncryptionEnabled,
                Algorithm = smpResponse.EncryptAlgorithm,
                AlgorithmKeySize = smpResponse.EncryptAlgorithmKeySize,
                CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate,
                EncryptionCertificateInformation = new PublicKeyCertificate { Certificate = smpResponse.EncryptPublicKeyCertificate},
                KeyTransport = new KeyEncryption
                {
                    DigestAlgorithm = smpResponse.EncryptKeyDigestAlgorithm,
                    MgfAlgorithm = smpResponse.EncryptKeyMgfAlorithm,
                    TransportAlgorithm = smpResponse.EncryptKeyTransportAlgorithm
                }
            };

            return pmode;
        }
    }
}
