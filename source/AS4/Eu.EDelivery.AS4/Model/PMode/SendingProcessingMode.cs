using System;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Security.References;
using Eu.EDelivery.AS4.Security.Strategies;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Model.PMode
{
    /// <summary>
    /// Sending PMode configuration
    /// </summary>
    [XmlType(Namespace = "eu:edelivery:as4:pmode")]
    [XmlRoot("PMode", Namespace = "eu:edelivery:as4:pmode", IsNullable = false)]
    [DebuggerDisplay("PMode Id = {" + nameof(Id) + "}")]
    public class SendingProcessingMode : IPMode
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendingProcessingMode" /> class.
        /// </summary>
        public SendingProcessingMode()
        {
            AllowOverride = false;
            Reliability = new SendReliability();
            ReceiptHandling = new SendHandling();
            ErrorHandling = new SendHandling();
            ExceptionHandling = new SendHandling();
            Security = new Security();
            MessagePackaging = new SendMessagePackaging();
        }

        public bool AllowOverride { get; set; }

        public MessageExchangePattern Mep { get; set; }

        public MessageExchangePatternBinding MepBinding { get; set; }

        public PushConfiguration PushConfiguration { get; set; }

        public PullConfiguration PullConfiguration { get; set; }

        public DynamicDiscoveryConfiguration DynamicDiscovery { get; set; }

        public SendReliability Reliability { get; set; }

        public SendHandling ReceiptHandling { get; set; }

        public SendHandling ErrorHandling { get; set; }

        public SendHandling ExceptionHandling { get; set; }

        public Security Security { get; set; }

        public SendMessagePackaging MessagePackaging { get; set; }

        [Info("The id of the sending pmode")]
        public string Id { get; set; }

        #region Serialization-control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PushConfigurationSpecified => PushConfiguration != null;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PullConfigurationSpecified => PullConfiguration != null;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool DynamicDiscoverySpecified => DynamicDiscovery != null;

        #endregion
    }

    public class Security
    {
        public Security()
        {
            Signing = new Signing();
            Encryption = new Encryption();
        }

        public Signing Signing { get; set; }

        public Encryption Encryption { get; set; }
    }

    public class Encryption
    {
        /// <summary>
        /// An Encryption instance which contains the default settings.
        /// </summary>
        public static readonly Encryption Default = new Encryption();

        public Encryption()
        {
            IsEnabled = false;
            Algorithm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";
            KeyTransport = new KeyEncryption();
            AlgorithmKeySize = 128;
            PublicKeyType = PublicKeyChoiceType.FindCriteria;
        }

        public bool IsEnabled { get; set; }

        public string Algorithm { get; set; }

        public int AlgorithmKeySize { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public PublicKeyChoiceType PublicKeyType { get; set; }

        [XmlChoiceIdentifier(nameof(PublicKeyType))]
        [XmlElement("PublicKeyFindCriteria", typeof(PublicKeyFindCriteria))]
        [XmlElement("PublicKeyCertificate", typeof(PublicKeyCertificate))]
        public object PublicKeyInformation { get; set; }

        ////public X509FindType PublicKeyFindType { get; set; }

        ////public string PublicKeyFindValue { get; set; }
        
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

        ////[XmlIgnore]
        ////[JsonIgnore]
        ////[ScriptIgnore]
        ////public bool PublicKeyFindTypeSpecified { get; set; }

        ////[XmlIgnore]
        ////[JsonIgnore]
        ////[ScriptIgnore]
        ////public bool PublicKeyFindValueSpecified => !string.IsNullOrWhiteSpace(PublicKeyFindValue);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool KeyTransportSpecified => KeyTransport != null;

        #endregion
    }

    [XmlType(IncludeInSchema = false)]
    public enum PublicKeyChoiceType
    {
        Certificate,
        FindCriteria
    }

    public class PublicKeyCertificate
    {
        public string Certificate { get; set; }
    }

    public class PublicKeyFindCriteria
    {
        public X509FindType PublicKeyFindType { get; set; }

        public string PublicKeyFindValue { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PublicKeyFindTypeSpecified { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PublicKeyFindValueSpecified => !string.IsNullOrWhiteSpace(PublicKeyFindValue);
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
            DigestAlgorithm = EncryptionStrategy.XmlEncSHA1Url;
            MgfAlgorithm = null;
        }

        public string TransportAlgorithm { get; set; }

        public string DigestAlgorithm { get; set; }

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
        public Signing()
        {
            IsEnabled = false;
        }

        public bool IsEnabled { get; set; }

        public X509ReferenceType KeyReferenceMethod { get; set; }

        public X509FindType PrivateKeyFindType { get; set; }

        public string PrivateKeyFindValue { get; set; }

        public string Algorithm { get; set; }

        public string HashFunction { get; set; }

        #region Properties that control serialization

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PrivateKeyFindValueSpecified => !string.IsNullOrWhiteSpace(PrivateKeyFindValue);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PrivateKeyFindTypeSpecified { get; set; }

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

        public bool NotifyMessageProducer { get; set; }

        public Method NotifyMethod { get; set; }
    }

    [Serializable]
    public class SendReliability
    {
        public SendReliability()
        {
            ReceptionAwareness = new ReceptionAwareness();
        }

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

        public bool IsEnabled { get; set; }

        public int RetryCount { get; set; }

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
    public class PullConfiguration : ISendConfiguration
    {
        public PullConfiguration()
        {
            Protocol = new Protocol();
            TlsConfiguration = new TlsConfiguration();
            Mpc = "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC";
        }

        public string Mpc { get; set; }

        public Protocol Protocol { get; set; }

        public TlsConfiguration TlsConfiguration { get; set; }
    }

    [Serializable]
    public class PushConfiguration : ISendConfiguration
    {
        public PushConfiguration()
        {
            Protocol = new Protocol();
            TlsConfiguration = new TlsConfiguration();
        }

        public Protocol Protocol { get; set; }

        public TlsConfiguration TlsConfiguration { get; set; }
    }

    public class DynamicDiscoveryConfiguration
    {
        public string SmlScheme { get; set; }

        public string SmpServerDomainName { get; set; }

        public string DocumentIdentifier { get; set; }
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

        public string Url { get; set; }

        public bool UseChunking { get; set; }

        public bool UseHttpCompression { get; set; }
    }

    public class TlsConfiguration
    {
        public TlsConfiguration()
        {
            IsEnabled = false;
            TlsVersion = TlsVersion.Tls12;
        }

        public bool IsEnabled { get; set; }

        public TlsVersion TlsVersion { get; set; }

        public ClientCertificateReference ClientCertificateReference { get; set; }
    }

    public class ClientCertificateReference
    {
        public X509FindType ClientCertificateFindType { get; set; }

        public string ClientCertificateFindValue { get; set; }
    }

    public class SendMessagePackaging : MessagePackaging
    {
        public SendMessagePackaging()
        {
            UseAS4Compression = true;
            IsMultiHop = false;
            IncludePModeId = false;
        }

        public string Mpc { get; set; }

        public bool UseAS4Compression { get; set; }

        public bool IsMultiHop { get; set; }

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