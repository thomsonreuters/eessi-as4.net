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
            ReceiptHandling = new SendHandling();
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
        [Description("Message exchange pattern binding")]
        public MessageExchangePatternBinding MepBinding { get; set; }

        [Description("Push configuration")]
        public PushConfiguration PushConfiguration { get; set; }

        [Description("Configuration for dynamic discovery")]
        public DynamicDiscoveryConfiguration DynamicDiscovery { get; set; }

        [Description("Send reliability")]
        public SendReliability Reliability { get; set; }

        [Description("Receipt handling")]
        public SendHandling ReceiptHandling { get; set; }

        [Description("Error handling")]
        public SendHandling ErrorHandling { get; set; }

        [Description("Exception handling")]
        public SendHandling ExceptionHandling { get; set; }

        [Description("Security settings")]
        public Security Security { get; set; }

        [Description("Send message pacjaging")]
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
            Encryption = new Encryption();
        }

        [Description("Signing")]
        public Signing Signing { get; set; }

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
            Algorithm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";
            KeyTransport = new KeyEncryption();
            AlgorithmKeySize = 128;
            CertificateType = PublicKeyCertificateChoiceType.None;
        }

        [Description("Is encryption enabled")]
        public bool IsEnabled { get; set; }

        [DefaultValue("http://www.w3.org/2009/xmlenc11#aes128-gcm")]
        [Description("Encryption algorithm")]
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
        [Description("Transport algorithm")]
        public string TransportAlgorithm { get; set; }

        [DefaultValue(EncryptionStrategy.XmlEncSHA256Url)]
        [Description("Digest algorithm")]
        public string DigestAlgorithm { get; set; }

        [Description("Mgf algorithm")]
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
        }

        [Description("Is signing enabled")]
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

        [Description("Key reference method")]
        public X509ReferenceType KeyReferenceMethod { get; set; }

        [Description("Signing algorithm")]
        [DefaultValue(DefaultAlgorithm)]
        public string Algorithm { get; set; }

        [Description("Hash function to use for the signing")]
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
        }

        [Description("Notify message producer")]
        public bool NotifyMessageProducer { get; set; }

        [Description("Notify method")]
        public Method NotifyMethod { get; set; }
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

        [Description("Retry count")]
        [DefaultValue(5)]
        public int RetryCount { get; set; }

        [DefaultValue("00:01:00")]
        [Description("Interval after which to try again")]
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
        [Description("Service meta locator scheme")]
        public string SmlScheme { get; set; }
        [Description("Service management point domain name")]
        public string SmpServerDomainName { get; set; }
        [Description("Document identifier")]
        public string DocumentIdentifier { get; set; }
        [Description("Document identifer scheme")]
        public string DocumentIdentifierScheme { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicDiscoveryConfiguration"/> class.
        /// </summary>
        public DynamicDiscoveryConfiguration()
        {
            SmlScheme = "iso6523-actorid-upis";
            DocumentIdentifier = "urn:oasis:names:specification:ubl:schema:xsd:Invoice-2::Invoice##urn:www.cenbii.eu:transaction:biitrns010:ver2.0:extended:urn:www.peppol.eu:bis:peppol5a:ver2.0::2.1";
            DocumentIdentifierScheme = "busdox-docid-qns";
            SmpServerDomainName = string.Empty;
        }
    }

    public class Protocol
    {
        public Protocol()
        {
            UseChunking = false;
            UseHttpCompression = false;
        }

        [Description("URL")]
        public string Url { get; set; }

        [Description("Indicates if chunking is enabled")]
        public bool UseChunking { get; set; }

        [Description("Indicates if HTTP compression is enabled")]
        public bool UseHttpCompression { get; set; }
    }

    public class TlsConfiguration
    {
        public TlsConfiguration()
        {
            IsEnabled = false;
            TlsVersion = TlsVersion.Tls12;
        }
        
        [Description("Indicates if TLS is enabled")]
        public bool IsEnabled { get; set; }

        [DefaultValue(TlsVersion.Tls12)]
        [Description("Version for TLS")]
        public TlsVersion TlsVersion { get; set; }

        [Description("Client certificate reference settings")]
        public ClientCertificateReference ClientCertificateReference { get; set; }
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
        [Description("Use AS4 compression")]
        public bool UseAS4Compression { get; set; }

        [DefaultValue(false)]
        [Description("Indicates if multihop is enabled")]
        public bool IsMultiHop { get; set; }

        [DefaultValue(false)]
        [Description("Include PMode")]
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