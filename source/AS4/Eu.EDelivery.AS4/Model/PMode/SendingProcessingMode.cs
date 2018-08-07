using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Eu.EDelivery.AS4.Serialization;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Model.PMode
{
    /// <summary>
    /// Sending PMode configuration
    /// </summary>
    [XmlType(Namespace = Constants.Namespaces.ProcessingMode)]
    [XmlRoot("PMode", Namespace = Constants.Namespaces.ProcessingMode, IsNullable = false)]
    [DebuggerDisplay("PMode Id = {" + nameof(Id) + "}")]
    public class SendingProcessingMode : IPMode, ICloneable
    {
        private bool? _allowOverride;

        /// <summary>
        /// Initializes a new instance of the <see cref="SendingProcessingMode" /> class.
        /// </summary>
        public SendingProcessingMode()
        {
            MepBinding = MessageExchangePatternBinding.Push;
            AllowOverride = false;
            Reliability = new SendReliability();
            ReceiptHandling = new SendReceiptHandling();
            ErrorHandling = new SendHandling();
            ExceptionHandling = new SendHandling();
            Security = new Security();
            MessagePackaging = new SendMessagePackaging();
        }

        [XmlElement(IsNullable = true)]
        [Description("Id of the PMode")]
        public string Id { get; set; }

        [Info("Allow override", defaultValue: false)]
        [Description("Indicate if settings in the PMode can be overwritten by settings from the submit message")]
        public bool AllowOverride
        {
            get => _allowOverride ?? false;
            set => _allowOverride = value;
        }

        [Info("Message exchange pattern")]
        [Description("Message exchange pattern")]
        public MessageExchangePattern Mep { get; set; }

        [Info("Message exchange pattern binding", defaultValue: MessageExchangePatternBinding.Push)]
        [Description("Indicate if the message will be pushed by the sender to the receiver, or if the message " +
                     "must be sent to the receiver as a response on a PullRequest that has been sent by the receiver.")]
        public MessageExchangePatternBinding MepBinding { get; set; }

        [Description("Push configuration")]
        public PushConfiguration PushConfiguration { get; set; }

        [Description("Configuration for dynamic discovery")]
        public DynamicDiscoveryConfiguration DynamicDiscovery { get; set; }

        [Description("Send reliability")]
        public SendReliability Reliability { get; set; }

        [Description("Receipt handling")]
        public SendReceiptHandling ReceiptHandling { get; set; }

        [Description("Error handling")]
        public SendHandling ErrorHandling { get; set; }

        [Description("Exception handling")]
        public SendHandling ExceptionHandling { get; set; }

        [Description("Security settings")]
        public Security Security { get; set; }

        [Description("Send message packaging")]
        public SendMessagePackaging MessagePackaging { get; set; }

        #region Serialization-control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AllowOverrideSpecified => _allowOverride.HasValue;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PushConfigurationSpecified => PushConfiguration != null;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool DynamicDiscoverySpecified => DynamicDiscovery != null;

        /// <summary>Creates a new object that is a copy of the current instance.</summary>
        /// <returns>A new object that is a copy of this instance.</returns>
        public object Clone()
        {
            // Create an exact copy of this instance by serializing and deserializing it.
            var pmodeString = AS4XmlSerializer.ToString(this);
            return AS4XmlSerializer.FromString<SendingProcessingMode>(pmodeString);
        }

        #endregion
    }

    public class Security
    {
        public Security()
        {
            Signing = new Signing();
            SigningVerification = new SigningVerification();
            Encryption = new Encryption();
        }

        [Description("Signing")]
        public Signing Signing { get; set; }

        [Description("Signing verification")]
        public SigningVerification SigningVerification { get; set; }

        [Description("Encryption")]
        public Encryption Encryption { get; set; }
    }

    public class Encryption
    {
        /// <summary>
        /// An Encryption instance which contains the default settings.
        /// </summary>
        [XmlIgnore]
        public static readonly Encryption Default = new Encryption();

        private object _encryptionCertificateInformation;

        public Encryption()
        {
            IsEnabled = false;
            Algorithm = Constants.Namespaces.XmlEnc11Aes128;
            KeyTransport = new KeyEncryption();
            AlgorithmKeySize = 128;
            CertificateType = PublicKeyCertificateChoiceType.None;
        }

        [Description("Indicate whether or not the message must be encrypted.")]
        public bool IsEnabled { get; set; }

        [Info("Algorithm", defaultValue: Constants.Namespaces.XmlEnc11Aes128)]
        [Description("Defines the algorithm that must be used to encrypt the message symmetrically.")]
        public string Algorithm { get; set; }

        [Info("Algorithm key size", defaultValue: 128)]
        [Description("Algorithm key size")]
        public int AlgorithmKeySize { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        [Description("Public key type")]
        public PublicKeyCertificateChoiceType CertificateType { get; set; }

        [XmlChoiceIdentifier(nameof(CertificateType))]
        [XmlElement("CertificateFindCriteria", typeof(CertificateFindCriteria))]
        [XmlElement("PublicKeyCertificate", typeof(PublicKeyCertificate))]
        [Description("Encryption certificate information")]
        public object EncryptionCertificateInformation
        {
            get => _encryptionCertificateInformation;
            set
            {
                _encryptionCertificateInformation = value;
                if (value is CertificateFindCriteria)
                {
                    CertificateType = PublicKeyCertificateChoiceType.CertificateFindCriteria;
                }
                else if (value is PublicKeyCertificate)
                {
                    CertificateType = PublicKeyCertificateChoiceType.PublicKeyCertificate;
                }
                else
                {
                    CertificateType = PublicKeyCertificateChoiceType.None;
                }
            }
        }

        public KeyEncryption KeyTransport { get; set; }

        #region Properties that control serialization

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AlgorithmSpecified => !string.IsNullOrWhiteSpace(Algorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AlgorithmKeySizeSpecified => AlgorithmKeySize > 0;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool EncryptionCertificateInformationSpecified => EncryptionCertificateInformation != null;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool KeyTransportSpecified => KeyTransport != null;

        #endregion
    }

    [XmlType(IncludeInSchema = false)]
    public enum PublicKeyCertificateChoiceType
    {
        None = 0,
        PublicKeyCertificate,
        CertificateFindCriteria
    }

    [XmlType(IncludeInSchema = false)]
    public enum PrivateKeyCertificateChoiceType
    {
        None = 0,
        PrivateKeyCertificate,
        CertificateFindCriteria
    }

    public class PublicKeyCertificate
    {
        [Description("Certificate to use")]
        public string Certificate { get; set; }
    }

    public class PrivateKeyCertificate
    {
        public string Certificate { get; set; }

        public string Password { get; set; }
    }

    public class KeyEncryption
    {
        /// <summary>
        /// A KeyEncryption instance which contains the default settings.
        /// </summary>
        public static readonly KeyEncryption Default = new KeyEncryption();

        public KeyEncryption()
        {
            TransportAlgorithm = CryptoStrategy.XmlEncRSAOAEPUrlWithMgf;
            DigestAlgorithm = EncryptedXml.XmlEncSHA256Url;
            MgfAlgorithm = null;
        }

        [Info("Transport algorithm", defaultValue: CryptoStrategy.XmlEncRSAOAEPUrlWithMgf)]
        [Description("The asymetrical encryption algorithm that must be used to encrypt the secret encryption key.")]
        public string TransportAlgorithm { get; set; }

        [Info("Digest algorithm", defaultValue: EncryptedXml.XmlEncSHA256Url)]
        [Description("Digest algorithm")]
        public string DigestAlgorithm { get; set; }

        [Description("The Mask Generation Function that must be used.")]
        public string MgfAlgorithm { get; set; }

        #region Properties that control serialization

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool TransportAlgorithmSpecified => !string.IsNullOrWhiteSpace(TransportAlgorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool DigestAlgorithmSpecified => !string.IsNullOrWhiteSpace(DigestAlgorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool MgfAlgorithmSpecified => !string.IsNullOrWhiteSpace(MgfAlgorithm);

        #endregion
    }

    public class Signing
    {
        private const string DefaultAlgorithm = "http://www.w3.org/2001/04/xmldsig-more#rsa-sha256";
        private const string DefaultHashFunction = "http://www.w3.org/2001/04/xmlenc#sha256";

        private object _signingCertificateInformation;

        public Signing()
        {
            IsEnabled = false;
            CertificateType = PrivateKeyCertificateChoiceType.None;
            Algorithm = DefaultAlgorithm;
            HashFunction = DefaultHashFunction;
            KeyReferenceMethod = X509ReferenceType.BSTReference;
        }

        [Description("Indicate whether or not the message must be signed.")]
        public bool IsEnabled { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public PrivateKeyCertificateChoiceType CertificateType { get; set; }

        [XmlChoiceIdentifier(nameof(CertificateType))]
        [XmlElement("CertificateFindCriteria", typeof(CertificateFindCriteria))]
        [XmlElement("PrivateKeyCertificate", typeof(PrivateKeyCertificate))]
        [Description("Signing Certificate")]
        public object SigningCertificateInformation
        {
            get => _signingCertificateInformation;
            set
            {
                _signingCertificateInformation = value;
                if (value is CertificateFindCriteria)
                {
                    CertificateType = PrivateKeyCertificateChoiceType.CertificateFindCriteria;
                }
                else if (value is PrivateKeyCertificate)
                {
                    CertificateType = PrivateKeyCertificateChoiceType.PrivateKeyCertificate;
                }
                else
                {
                    CertificateType = PrivateKeyCertificateChoiceType.None;
                }
            }
        }

        [Info("Key reference method", defaultValue: X509ReferenceType.BSTReference)]
        [Description("Define how the Signing Certificate must be referenced in the Message")]
        public X509ReferenceType KeyReferenceMethod { get; set; }

        [Info("Algorithm", defaultValue: DefaultAlgorithm)]
        [Description("Defines the algorithm that must be used to sign the message.")]
        public string Algorithm { get; set; }

        [Info("Hash function", defaultValue: DefaultHashFunction)]
        [Description("Define the hash algorithm that must be used when signing the message.")]
        public string HashFunction { get; set; }

        #region Properties that control serialization

        public bool SigningCertificateInformationSpecified => SigningCertificateInformation != null;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool KeyReferenceMethodSpecified { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AlgorithmSpecified => !string.IsNullOrWhiteSpace(Algorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool HashFunctionSpecified => !string.IsNullOrWhiteSpace(HashFunction);

        #endregion
    }

    public class SendHandling
    {
        public SendHandling()
        {
            NotifyMessageProducer = false;
            NotifyMethod = new Method();
            Reliability = new RetryReliability();
        }

        [Description("Indicate if the Message Producer must be notified.")]
        public bool NotifyMessageProducer { get; set; }

        [Description("How should the notification messages be sent to the Message Producer.")]
        public Method NotifyMethod { get; set; }

        [Description("Notify reliability")]
        public RetryReliability Reliability { get; set; }
    }

    public class SendReceiptHandling : SendHandling
    {
        private bool? _verifyNRR;

        public SendReceiptHandling()
        {
            VerifyNRR = true;
        }

        [Info("Verify Non-Repudiation Receipts", defaultValue: true)]
        [Description(
            "Indicates if the MSH needs to verify the Non-Repudiation Information that is included in the receipt")]
        public bool VerifyNRR
        {
            get => _verifyNRR ?? false;
            set => _verifyNRR = value;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool VerifyNRRSpecified => _verifyNRR.HasValue;

        #endregion
    }

    [Serializable]
    public class SendReliability
    {
        public SendReliability()
        {
            ReceptionAwareness = new ReceptionAwareness();
        }

        [Description("Reception awareness")]
        public ReceptionAwareness ReceptionAwareness { get; set; }
    }

    public class ReceptionAwareness
    {
        private TimeSpan _retryInterval;

        public ReceptionAwareness()
        {
            IsEnabled = false;
            RetryCount = 5;
            _retryInterval = TimeSpan.FromMinutes(1);
        }

        [Description("Indicates if reception awareness is enabled")]
        public bool IsEnabled { get; set; }

        [Info("Retry count", defaultValue: 5)]
        [Description("Defines how many retries the AS4.NET MSH must perform if a Receipt or Error-message is not received within " +
                     "the specified time-frame.")]
        public int RetryCount { get; set; }

        [Info("Retry interval", defaultValue: "00:01:00")]
        [Description("The timeframe in which the MSH waits before re-sending the Message.  If a Receipt or Error message" +
                     "is received before the end of this timeframe, the MSH will not re-send the message.")]
        public string RetryInterval
        {
            get => _retryInterval.ToString(@"hh\:mm\:ss");
            set => TimeSpan.TryParse(value, out _retryInterval);
        }
    }

    public interface ISendConfiguration
    {
        Protocol Protocol { get; set; }

        TlsConfiguration TlsConfiguration { get; set; }
    }

    [Serializable]
    public class PushConfiguration : ISendConfiguration
    {
        public PushConfiguration()
        {
            Protocol = new Protocol();
            TlsConfiguration = new TlsConfiguration();
        }

        [Description("Protocol settings")]
        public Protocol Protocol { get; set; }

        [Description("TLS configuration")]
        public TlsConfiguration TlsConfiguration { get; set; }
    }

    public class DynamicDiscoveryConfiguration
    {
        [Description("Reference to the Dynamic Discovery Profile implementation")]
        public string SmpProfile { get; set; }

        [Description("Smp Profile custom settings")]
        [XmlArrayItem("Setting")]
        public DynamicDiscoverySetting[] Settings { get; set; }
    }

    public class DynamicDiscoverySetting
    {
        [XmlAttribute(AttributeName = "key")]
        public string Key { get; set; }

        [XmlText]
        public string Value { get; set; }
    }

    public class Protocol
    {
        private bool? _useChunking;
        private bool? _useHttpCompression;

        public Protocol()
        {
            UseChunking = false;
            UseHttpCompression = false;
        }

        [Description("The address of the endpoint to where the message must be sent.")]
        public string Url { get; set; }

        [Info("Use chunking", defaultValue: false)]
        [Description("Indicates if chunking is enabled")]
        public bool UseChunking
        {
            get => _useChunking ?? false;
            set => _useChunking = value;
        }

        [Info("Use HTTP compresstion", defaultValue: false)]
        [Description("Indicates if HTTP compression is enabled")]
        public bool UseHttpCompression
        {
            get => _useHttpCompression ?? false;
            set => _useHttpCompression = value;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseChunkingSpecified => _useChunking.HasValue;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseHttpCompressionSpecified => _useHttpCompression.HasValue;

        #endregion
    }

    public class TlsConfiguration
    {
        private const TlsVersion DefaultTlsVersion = TlsVersion.Tls12;

        private object _clientCertificateInformation;
        private TlsVersion? _tlsVersion;

        public TlsConfiguration()
        {
            IsEnabled = false;
            TlsVersion = DefaultTlsVersion;
        }
        
        [Description("Indicates if TLS is enabled")]
        public bool IsEnabled { get; set; }

        [Info("TLS version", defaultValue: DefaultTlsVersion)]
        [Description("TLS version that must be used.")]
        public TlsVersion TlsVersion
        {
            get => _tlsVersion ?? DefaultTlsVersion;
            set => _tlsVersion = value;
        }

        [XmlIgnore]
        [JsonIgnore]
        public TlsCertificateChoiceType CertificateType { get; set; }

        [XmlChoiceIdentifier(nameof(CertificateType))]
        [XmlElement("ClientCertificateReference", typeof(ClientCertificateReference))]
        [XmlElement("PrivateKeyCertificate", typeof(PrivateKeyCertificate))]
        [Description("Client certificate reference settings")]
        public object ClientCertificateInformation
        {
            get => _clientCertificateInformation;
            set
            {
                _clientCertificateInformation = value;
                if (value is ClientCertificateReference)
                {
                    CertificateType = TlsCertificateChoiceType.ClientCertificateReference;
                }
                else if (value is PrivateKeyCertificate)
                {
                    CertificateType = TlsCertificateChoiceType.PrivateKeyCertificate;
                }
            }
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool TlsVersionSpecified => _tlsVersion.HasValue;

        #endregion
    }

    public enum TlsCertificateChoiceType
    {
        ClientCertificateReference,
        PrivateKeyCertificate
    }

    public class ClientCertificateReference
    {
        [Info("Client certificate find type", defaultValue: X509FindType.FindByThumbprint)]
        [Description("X509 find type")]
        public X509FindType ClientCertificateFindType { get; set; }

        [Description("Value to search for")]
        public string ClientCertificateFindValue { get; set; }
    }

    public class SendMessagePackaging : MessagePackaging
    {
        private bool? _useAS4Compression, 
                      _isMultiHop, 
                      _includePModeId;

        public SendMessagePackaging()
        {
            UseAS4Compression = true;
            IsMultiHop = false;
            IncludePModeId = false;
            Mpc = Constants.Namespaces.EbmsDefaultMpc;
        }

        [Info("Message partition channel", defaultValue: Constants.Namespaces.EbmsDefaultMpc)]
        [Description("Messaging partition channel")]
        public string Mpc { get; set; }

        [Info("Use AS4 compression", defaultValue: true)]
        [Description("Indicate whether or not the message must be compressed")]
        public bool UseAS4Compression
        {
            get => _useAS4Compression ?? false;
            set => _useAS4Compression = value;
        }

        [Info("Is multihop", defaultValue: false)]
        [Description("Indicates if multihop is enabled")]
        public bool IsMultiHop
        {
            get => _isMultiHop ?? false;
            set => _isMultiHop = value;
        }

        [Info("Include Processing Mode Id", defaultValue: false)]
        [Description("Indicate whether or not the Processing Mode Idetifier must be included in the message meta-data")]
        public bool IncludePModeId
        {
            get => _includePModeId ?? false;
            set => _includePModeId = value;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseAS4CompressionSpecified => _useAS4Compression.HasValue;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool IsMultiHopSpecified => _isMultiHop.HasValue;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]

        public bool IncludePModeIdSpecified => _includePModeId.HasValue;

        #endregion
    }

    public enum TlsVersion
    {
        Ssl30,
        Tls10,
        Tls11,
        Tls12
    }
}