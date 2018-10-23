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

        private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

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
            if (createDatastore == null)
            {
                throw new ArgumentNullException(nameof(createDatastore));
            }

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
            if (party == null)
            {
                throw new ArgumentNullException(nameof(party));
            }

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
                           .FirstOrDefault(
                               sc => sc.PartyRole == party.Role
                                     && sc.ToPartyId == party.PrimaryPartyId
                                     && sc.PartyType == primaryPartyType);

                if (foundConfiguration == null)
                {
                    throw new ConfigurationErrorsException(
                        "No SMP Response found for the given "
                        + $"'Role': {party.Role}, 'PartyId': {party.PrimaryPartyId}, and 'PartyType': {primaryPartyType}");
                }

                return foundConfiguration;
            }
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
            if (pmode == null)
            {
                throw new ArgumentNullException(nameof(pmode));
            }

            if (smpMetaData == null)
            {
                throw new ArgumentNullException(nameof(smpMetaData));
            }

            var smpResponse = AS4XmlSerializer.FromString<SmpConfiguration>(smpMetaData.OuterXml);
            if (smpResponse == null)
            {
                throw new ArgumentNullException(
                    nameof(smpResponse),
                    $@"SMP Response cannot be deserialized correctly to a SmpConfiguration model: {smpMetaData.OuterXml}");
            }

            OverridePushProtocolUrlWithTlsEnabling(pmode, smpResponse);
            OverrideEntireEncryption(pmode, smpResponse);
            OverrideToParty(pmode, smpResponse);
            OverrideCollaborationServiceAction(pmode, smpResponse);
            AddFinalRecipientToMessageProperties(pmode, smpResponse);

            return pmode;
        }

        private static void OverridePushProtocolUrlWithTlsEnabling(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            Logger.Debug($"Decorate SendingPMode {pmode.Id} with SMP from local store");
            Logger.Trace(
                "Override SendingPMode.PushConfiguration with {{"
                + $"Protocol.Url={smpResponse.Url}, "
                + $"TlsConfiguration.IsEnabled={smpResponse.TlsEnabled}}}");

            pmode.PushConfiguration = pmode.PushConfiguration ?? new PushConfiguration();
            pmode.PushConfiguration.Protocol = pmode.PushConfiguration.Protocol ?? new Protocol();
            pmode.PushConfiguration.Protocol.Url = smpResponse.Url;

            pmode.PushConfiguration.TlsConfiguration = pmode.PushConfiguration.TlsConfiguration ?? new TlsConfiguration();
            pmode.PushConfiguration.TlsConfiguration.IsEnabled = smpResponse.TlsEnabled;
        }

        private static void OverrideEntireEncryption(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            Logger.Trace($"Override SendingPMode.Encryption with {{IsEnabled={smpResponse.EncryptionEnabled}}}");
            Logger.Trace(
                "Override SendingPMode.Encryption with {{"
                + $"Algorithm={smpResponse.EncryptAlgorithm}, "
                + $"AlgorithmKeySize={smpResponse.EncryptAlgorithmKeySize}}}");

            Logger.Trace("Override SendingPMode.Encryption with {{CertificateType=PublicKeyCertificate}}");
            Logger.Trace(
                "Override SendingPMode.Encryption.KeyTransport Algorithms with {{"
                + $"Digest={smpResponse.EncryptKeyDigestAlgorithm}, "
                + $"Mgf={smpResponse.EncryptKeyMgfAlorithm}, "
                + $"Transport={smpResponse.EncryptKeyTransportAlgorithm}}}");

            pmode.Security = pmode.Security ?? new Model.PMode.Security();
            pmode.Security.Encryption = pmode.Security.Encryption ?? new Encryption();
            pmode.Security.Encryption.IsEnabled = smpResponse.EncryptionEnabled;
            pmode.Security.Encryption.Algorithm = smpResponse.EncryptAlgorithm;
            pmode.Security.Encryption.AlgorithmKeySize = smpResponse.EncryptAlgorithmKeySize;
            pmode.Security.Encryption.CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;
            pmode.Security.Encryption.EncryptionCertificateInformation = new PublicKeyCertificate
            {
                Certificate = TryConvertToBase64String(smpResponse.EncryptPublicKeyCertificate)
            };
            pmode.Security.Encryption.KeyTransport = pmode.Security.Encryption.KeyTransport ?? new KeyEncryption();
            pmode.Security.Encryption.KeyTransport.DigestAlgorithm = smpResponse.EncryptKeyDigestAlgorithm;
            pmode.Security.Encryption.KeyTransport.MgfAlgorithm = smpResponse.EncryptKeyMgfAlorithm;
            pmode.Security.Encryption.KeyTransport.TransportAlgorithm = smpResponse.EncryptKeyTransportAlgorithm;
        }

        private static void OverrideToParty(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            Logger.Trace(
                "Override SendingPMode.MessagingPackaging.ToParty with {{"
                + $"Role={smpResponse.PartyRole}, "
                + $"PartyId={smpResponse.ToPartyId}}}");

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.PartyInfo = pmode.MessagePackaging.PartyInfo ?? new PartyInfo();
            pmode.MessagePackaging.PartyInfo.ToParty = new Party(smpResponse.PartyRole, new PartyId(smpResponse.ToPartyId));
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
                Logger.Error(ex);
                return null;
            }
        }

        private static void OverrideCollaborationServiceAction(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            Logger.Trace(
                "Override SendingPMode.MessagingPackaing.CollaborationInfo with {{"
                + $"ServiceType={smpResponse.ServiceType}, "
                + $"ServiceValue={smpResponse.ServiceValue}, "
                + $"Action={smpResponse.Action}}}");

            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.CollaborationInfo = pmode.MessagePackaging.CollaborationInfo ?? new CollaborationInfo();
            pmode.MessagePackaging.CollaborationInfo.Action = smpResponse.Action;
            pmode.MessagePackaging.CollaborationInfo.Service = new Service
            {
                Type = smpResponse.ServiceType,
                Value = smpResponse.ServiceValue
            };
        }

        private static void AddFinalRecipientToMessageProperties(SendingProcessingMode pmode, SmpConfiguration smpResponse)
        {
            pmode.MessagePackaging = pmode.MessagePackaging ?? new SendMessagePackaging();
            pmode.MessagePackaging.MessageProperties = pmode.MessagePackaging.MessageProperties ?? new List<MessageProperty>();
            pmode.MessagePackaging.MessageProperties.Add(
                new MessageProperty
                {
                    Name = "finalRecipient",
                    Value = smpResponse.FinalRecipient
                });
        }
    }
}