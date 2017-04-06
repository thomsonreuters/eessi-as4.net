using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;
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
    [DebuggerDisplay("{" + nameof(Id) + "}")]
    public class SendingProcessingMode : IPMode
    {
        public SendingProcessingMode()
        {
            AllowOverride = false;
            PushConfiguration = new PushConfiguration();
            PullConfiguration = new PullConfiguration();
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
        public SendReliability Reliability { get; set; }
        public SendHandling ReceiptHandling { get; set; }
        public SendHandling ErrorHandling { get; set; }
        public SendHandling ExceptionHandling { get; set; }
        public Security Security { get; set; }
        public SendMessagePackaging MessagePackaging { get; set; }

        [Info("The id of the sending pmode")]
        public string Id { get; set; }
    }

    public class PModeParty
    {
        public List<PartyId> PartyIds { get; set; }
        public string Role { get; set; }
    }

    public class Security
    {
        public Security()
        {
            this.Signing = new Signing();
            this.Encryption = new Encryption();
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
            this.IsEnabled = false;
            this.Algorithm = "http://www.w3.org/2009/xmlenc11#aes128-gcm";
            this.KeyTransport = new KeyEncryption();
        }

        public bool IsEnabled { get; set; }
        public string Algorithm { get; set; }
        public X509FindType PublicKeyFindType { get; set; }
        public string PublicKeyFindValue { get; set; }

        public KeyEncryption KeyTransport { get; set; }

        #region Properties that control serialization

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AlgorithmSpecified => !String.IsNullOrWhiteSpace(Algorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PublicKeyFindTypeSpecified { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool PublicKeyFindValueSpecified => !String.IsNullOrWhiteSpace(PublicKeyFindValue);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool KeyTransportSpecified => KeyTransport != null;

        #endregion
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
        public bool TransportAlgorithmSpecified => !String.IsNullOrWhiteSpace(TransportAlgorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool DigestAlgorithmSpecified => !String.IsNullOrWhiteSpace(DigestAlgorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool MgfAlgorithmSpecified => !String.IsNullOrWhiteSpace(MgfAlgorithm);

        #endregion
    }

    public class Signing
    {
        public Signing()
        {
            this.IsEnabled = false;
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
        public bool PrivateKeyFindValueSpecified => !String.IsNullOrWhiteSpace(PrivateKeyFindValue);

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
        public bool AlgorithmSpecified => !String.IsNullOrWhiteSpace(Algorithm);

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool HashFunctionSpecified => !String.IsNullOrWhiteSpace(HashFunction);

        #endregion
    }

    public class SendHandling
    {
        public SendHandling()
        {
            this.NotifyMessageProducer = false;
            this.NotifyMethod = new Method();
        }

        public bool NotifyMessageProducer { get; set; }
        public Method NotifyMethod { get; set; }
    }

    [Serializable]
    public class SendReliability
    {
        public SendReliability()
        {
            this.ReceptionAwareness = new ReceptionAwareness();
        }

        public ReceptionAwareness ReceptionAwareness { get; set; }
    }

    public class ReceptionAwareness
    {
        private TimeSpan _retryInterval;

        public ReceptionAwareness()
        {
            this.IsEnabled = false;
            this.RetryCount = 5;
            this._retryInterval = TimeSpan.FromMinutes(1);
        }

        public bool IsEnabled { get; set; }
        public int RetryCount { get; set; }

        public string RetryInterval
        {
            get { return this._retryInterval.ToString(@"hh\:mm\:ss"); }
            set { TimeSpan.TryParse(value, out this._retryInterval); }
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
            this.Protocol = new Protocol();
            this.TlsConfiguration = new TlsConfiguration();
        }

        public string SubChannel { get; set; }

        public Protocol Protocol { get; set; }
        public TlsConfiguration TlsConfiguration { get; set; }
    }

    [Serializable]
    public class PushConfiguration : ISendConfiguration
    {
        public PushConfiguration()
        {
            this.Protocol = new Protocol();
            this.TlsConfiguration = new TlsConfiguration();
        }

        public Protocol Protocol { get; set; }
        public TlsConfiguration TlsConfiguration { get; set; }
    }

    public class Protocol
    {
        public Protocol()
        {
            this.UseChunking = false;
            this.UseHttpCompression = false;
        }

        public string Url { get; set; }
        public bool UseChunking { get; set; }
        public bool UseHttpCompression { get; set; }
    }

    public class TlsConfiguration
    {
        public TlsConfiguration()
        {
            this.IsEnabled = false;
            this.TlsVersion = TlsVersion.Tls12;
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
            this.UseAS4Compression = true;
            this.IsMultiHop = false;
            this.IncludePModeId = false;
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