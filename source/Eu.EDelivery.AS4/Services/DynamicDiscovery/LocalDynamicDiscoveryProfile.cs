using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using NLog;
using CollaborationInfo = Eu.EDelivery.AS4.Model.PMode.CollaborationInfo;
using MessageProperty = Eu.EDelivery.AS4.Model.PMode.MessageProperty;
using Party = Eu.EDelivery.AS4.Model.PMode.Party;
using PartyId = Eu.EDelivery.AS4.Model.PMode.PartyId;
using Service = Eu.EDelivery.AS4.Model.PMode.Service;

namespace Eu.EDelivery.AS4.Services.DynamicDiscovery
{
    /// <summary>
    /// Dynamic Discovery profile that queries the local configuration to look for the right SMP info to complete the
    /// <see cref="SendingProcessingMode" />.
    /// </summary>
    /// <seealso cref="IDynamicDiscoveryProfile" />
    public class LocalDynamicDiscoveryProfile : IDynamicDiscoveryProfile
    {
        private readonly Func<DatastoreContext> _createDatastore;

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDynamicDiscoveryProfile" /> class.
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
        /// Retrieves the SMP meta data <see cref="XmlDocument" /> for a given <paramref name="party" /> using a given
        /// <paramref name="properties" />.
        /// </summary>
        /// <param name="party">The party identifier.</param>
        /// <param name="properties"></param>
        /// <returns></returns>
        public Task<XmlDocument> RetrieveSmpMetaData(Model.Core.Party party, IDictionary<string, string> properties)
        {
            if (party.PrimaryPartyId == null
                || party.PartyIds.FirstOrDefault()?.Type == null
                || party.Role == null)
            {
                throw new InvalidOperationException(
                    "Given invalid 'ToParty', requires 'Role', 'PartyId', and 'PartyType'");
            }

            SmpConfiguration configuration = FindSmpResponseForToParty(party);
            string xml = AS4XmlSerializer.ToString(configuration);

            var document = new XmlDocument();
            document.LoadXml(xml);

            return Task.FromResult(document);
        }

        /// <summary>
        /// Complete the <paramref name="pmode" /> with the SMP metadata that is present in the <paramref name="smpMetaData" />
        /// <see cref="XmlDocument" />
        /// </summary>
        /// <param name="pmode">The <see cref="SendingProcessingMode" /> that must be decorated with the SMP metadata</param>
        /// <param name="smpMetaData">An XmlDocument that contains the SMP MetaData that has been received from an SMP server.</param>
        /// <returns>The completed <see cref="SendingProcessingMode" /></returns>
        public SendingProcessingMode DecoratePModeWithSmpMetaData(SendingProcessingMode pmode, XmlDocument smpMetaData)
        {
            var smpResponse = AS4XmlSerializer.FromString<SmpConfiguration>(smpMetaData.OuterXml);

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.PartyInfo = pmode.MessagePackaging.PartyInfo ?? new PartyInfo();
            pmode.MessagePackaging.PartyInfo.ToParty = new Party
            {
                Role = smpResponse.PartyRole,
                PartyIds = new List<PartyId> { new PartyId { Id = smpResponse.ToPartyId } }
            };

            pmode.MessagePackaging.MessageProperties = pmode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();
            pmode.MessagePackaging.MessageProperties.Add(
                new MessageProperty
                {
                    Name = "finalRecipient",
                    Value = smpResponse.FinalRecipient
                });

            pmode.MessagePackaging.CollaborationInfo = pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Service =
                new Service
                {
                    Type = smpResponse.ServiceType,
                    Value = smpResponse.ServiceValue
                };

            pmode.MessagePackaging.CollaborationInfo.Action = smpResponse.Action;

            pmode.PushConfiguration = new PushConfiguration
            {
                Protocol = new Protocol { Url = smpResponse.Url },
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
                EncryptionCertificateInformation = new PublicKeyCertificate
                {
                    Certificate = TryConvertToBase64String(smpResponse.EncryptPublicKeyCertificate)
                },
                KeyTransport = new KeyEncryption
                {
                    DigestAlgorithm = smpResponse.EncryptKeyDigestAlgorithm,
                    MgfAlgorithm = smpResponse.EncryptKeyMgfAlorithm,
                    TransportAlgorithm = smpResponse.EncryptKeyTransportAlgorithm
                }
            };

            return pmode;
        }

        private SmpConfiguration FindSmpResponseForToParty(Model.Core.Party party)
        {
            using (DatastoreContext context = _createDatastore())
            {
                string primaryPartyType = 
                    party.PartyIds
                         .FirstOrNothing()
                         .SelectMany(x => x.Type)
                         .GetOrElse(() => null);

                SmpConfiguration foundConfiguration = 
                    context.SmpConfigurations
                           .FirstOrDefault(sc =>
                                sc.PartyRole == party.Role
                                && sc.ToPartyId == party.PrimaryPartyId
                                && sc.PartyType
                                == primaryPartyType);

                if (foundConfiguration == null)
                {
                    throw new ConfigurationErrorsException(
                        "No SMP Response found for the given "
                        + $"'Role': {party.Role}, 'PartyId': {party.PrimaryPartyId}, and 'PartyType': {primaryPartyType}");
                }

                return foundConfiguration;
            }
        }

        private static string TryConvertToBase64String(byte[] arr)
        {
            if (arr == null || arr.Any() == false)
            {
                return null;
            }

            try
            {
                return Convert.ToBase64String(arr);
            }
            catch (Exception ex)
            {
                LogManager.GetCurrentClassLogger().Error(ex);
                return null;
            }
        }
    }
}