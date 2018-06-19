using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
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

        [DefaultValue(false)]
        [Description("Indicate if settings in the PMode can be overwritten by settings from the submit message")]
        public bool AllowOverride { get; set; }

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

        [DefaultValue(Constants.Namespaces.XmlEnc11Aes128)]
        [Description("Defines the algorithm that must be used to encrypt the message symmetrically.")]
        public string Algorithm { get; set; }

        [DefaultValue(128)]
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
            get { return _encryptionCertificateInformation; }
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
            TransportAlgorithm = EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf;
            DigestAlgorithm = EncryptionStrategy.XmlEncSHA256Url;
            MgfAlgorithm = null;
        }

        [DefaultValue(EncryptionStrategy.XmlEncRSAOAEPUrlWithMgf)]
        [Description("The asymetrical encryption algorithm that must be used to encrypt the secret encryption key.")]
        public string TransportAlgorithm { get; set; }

        [DefaultValue(EncryptionStrategy.XmlEncSHA256Url)]
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
            get { return _signingCertificateInformation; }
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

        [Description("Define how the Signing Certificate must be referenced in the Message")]
        [DefaultValue(X509ReferenceType.BSTReference)]
        public X509ReferenceType KeyReferenceMethod { get; set; }

        [Description("Defines the algorithm that must be used to sign the message.")]
        [DefaultValue(DefaultAlgorithm)]
        public string Algorithm { get; set; }

        [Description("Define the hash algorithm that must be used when signing the message.")]
        [DefaultValue(DefaultHashFunction)]
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
        public SendReceiptHandling()
        {
            VerifyNRR = true;
        }

        [DefaultValue(true)]
        [Description("Indicates if the MSH needs to verify the Non-Repudiation Information that is included in the receipt")]
        public bool VerifyNRR { get; set; }
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

        [Description("Defines how many retries the AS4.NET MSH must perform if a Receipt or Error-message is not received within " +
                     "the specified time-frame.")]
        [DefaultValue(5)]
        public int RetryCount { get; set; }

        [DefaultValue("00:01:00")]
        [Description("The timeframe in which the MSH waits before re-sending the Message.  If a Receipt or Error message" +
                     "is received before the end of this timeframe, the MSH will not re-send the message.")]
        public string RetryInterval
        {
            get { return _retryInterval.ToString(@"hh\:mm\:ss"); }
            set { TimeSpan.TryParse(value, out _retryInterval); }
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
        public Protocol()
        {
            UseChunking = false;
            UseHttpCompression = false;
        }

        [Description("The address of the endpoint to where the message must be sent.")]
        public string Url { get; set; }

        [DefaultValue(false)]
        [Description("Indicates if chunking is enabled")]
        public bool UseChunking { get; set; }

        [DefaultValue(false)]
        [Description("Indicates if HTTP compression is enabled")]
        public bool UseHttpCompression { get; set; }
    }

    public class TlsConfiguration
    {
        private object _clientCertificateInformation;

        public TlsConfiguration()
        {
            IsEnabled = false;
            TlsVersion = TlsVersion.Tls12;
        }
        
        [Description("Indicates if TLS is enabled")]
        public bool IsEnabled { get; set; }

        [DefaultValue(TlsVersion.Tls12)]
        [Description("TLS version that must be used.")]
        public TlsVersion TlsVersion { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public TlsCertificateChoiceType CertificateType { get; set; }

        [XmlChoiceIdentifier(nameof(CertificateType))]
        [XmlElement("ClientCertificateReference", typeof(ClientCertificateReference))]
        [XmlElement("PrivateKeyCertificate", typeof(PrivateKeyCertificate))]
        [Description("Client certificate reference settings")]
        public object ClientCertificateInformation
        {
            get { return _clientCertificateInformation; }
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
    }

    public enum TlsCertificateChoiceType
    {
        ClientCertificateReference,
        PrivateKeyCertificate
    }

    public class ClientCertificateReference
    {
        [DefaultValue(X509FindType.FindByThumbprint)]
        [Description("X509 find type")]
        public X509FindType ClientCertificateFindType { get; set; }

        [Description("Value to search for")]
        public string ClientCertificateFindValue { get; set; }
    }

    public class SendMessagePackaging : MessagePackaging
    {
        public SendMessagePackaging()
        {
            UseAS4Compression = true;
            IsMultiHop = false;
            IncludePModeId = false;
            Mpc = Constants.Namespaces.EbmsDefaultMpc;
        }

        [DefaultValue(Constants.Namespaces.EbmsDefaultMpc)]
        [Description("Messaging partition channel")]
        public string Mpc { get; set; }

        [DefaultValue(true)]
        [Description("Indicate whether or not the message must be compressed")]
        public bool UseAS4Compression { get; set; }

        [DefaultValue(false)]
        [Description("Indicates if multihop is enabled")]
        public bool IsMultiHop { get; set; }

        [DefaultValue(false)]
        [Description("Indicate whether or not the PModeId must be included in the message meta-data")]
        public bool IncludePModeId { get; set; }
    }

    public enum TlsVersion
    {
        Ssl30,
        Tls10,
        Tls11,
        Tls12
    }
}