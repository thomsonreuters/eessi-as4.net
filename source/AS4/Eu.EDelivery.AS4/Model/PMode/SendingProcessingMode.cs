using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Security.References;

namespace Eu.EDelivery.AS4.Model.PMode
{
    /// <summary>
    /// Sending PMode configuration
    /// </summary>
    [XmlType(Namespace = "eu:edelivery:as4:pmode")]
    [XmlRoot("PMode", Namespace = "eu:edelivery:as4:pmode", IsNullable = false)]
    public class SendingProcessingMode : IPMode
    {
        public string Id { get; set; }
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

        public SendingProcessingMode()
        {
            this.AllowOverride = false;
            this.PushConfiguration = new PushConfiguration();
            this.PullConfiguration = new PullConfiguration();
            this.Reliability = new SendReliability();
            this.ReceiptHandling = new SendHandling();
            this.ErrorHandling = new SendHandling();
            this.ExceptionHandling = new SendHandling();
            this.Security = new Security();
            this.MessagePackaging = new SendMessagePackaging();
        }
    }

    public class PModeParty
    {
        public List<PartyId> PartyIds { get; set; }
        public string Role { get; set; }
    }

    public class Security
    {
        public Signing Signing { get; set; }
        public Encryption Encryption { get; set; }

        public Security()
        {
            this.Signing = new Signing();
            this.Encryption = new Encryption();
        }
    }

    public class Encryption
    {
        public bool IsEnabled { get; set; }
        public string Algorithm { get; set; }
        public X509FindType PublicKeyFindType { get; set; }
        public string PublicKeyFindValue { get; set; }
        public string KeyTransport { get; set; }

        public Encryption()
        {
            this.IsEnabled = false;
        }
    }

    public class Signing
    {
        public bool IsEnabled { get; set; }
        public string PrivateKeyFindValue { get; set; }
        public X509FindType PrivateKeyFindType { get; set; }
        public X509ReferenceType KeyReferenceMethod { get; set; }
        public string Algorithm { get; set; }
        public string HashFunction { get; set; }

        public Signing()
        {
            this.IsEnabled = false;
        }
    }

    public class SendHandling
    {
        public bool NotifyMessageProducer { get; set; }
        public Method NotifyMethod { get; set; }

        public SendHandling()
        {
            this.NotifyMessageProducer = false;
            this.NotifyMethod = new Method();
        }
    }

    [Serializable]
    public class SendReliability
    {
        public ReceptionAwareness ReceptionAwareness { get; set; }

        public SendReliability()
        {
            this.ReceptionAwareness = new ReceptionAwareness();
        }
    }

    public class ReceptionAwareness
    {
        private TimeSpan _retryInterval;

        public bool IsEnabled { get; set; }
        public int RetryCount { get; set; }

        public string RetryInterval
        {
            get { return this._retryInterval.ToString(@"hh\:mm\:ss"); }
            set { TimeSpan.TryParse(value, out this._retryInterval); }
        }

        public ReceptionAwareness()
        {
            this.IsEnabled = false;
            this.RetryCount = 5;
            this._retryInterval = TimeSpan.FromMinutes(1);
        }
    }

    [Serializable]
    public class PullConfiguration
    {
        public string SubChannel { get; set; }
    }

    [Serializable]
    public class PushConfiguration
    {
        public Protocol Protocol { get; set; }
        public TlsConfiguration TlsConfiguration { get; set; }

        public PushConfiguration()
        {
            this.Protocol = new Protocol();
            this.TlsConfiguration = new TlsConfiguration();
        }
    }

    public class Protocol
    {
        public string Url { get; set; }
        public bool UseChunking { get; set; }
        public bool UseHttpCompression { get; set; }

        public Protocol()
        {
            this.UseChunking = false;
            this.UseHttpCompression = false;
        }
    }

    public class TlsConfiguration
    {
        public bool IsEnabled { get; set; }
        public TlsVersion TlsVersion { get; set; }
        public string ClientCertificateReference { get; set; }

        public TlsConfiguration()
        {
            this.IsEnabled = false;
            this.TlsVersion = TlsVersion.Tls12;
        }
    }

    public class SendMessagePackaging : MessagePackaging
    {
        public string Mpc { get; set; }
        public bool UseAS4Compression { get; set; }
        public bool IsMultiHop { get; set; }
        public bool IncludePModeId { get; set; }

        public SendMessagePackaging()
        {
            this.UseAS4Compression = true;
            this.IsMultiHop = false;
            this.IncludePModeId = false;
        }
    }

    public enum TlsVersion
    {
        Ssl30,
        Tls10,
        Tls11,
        Tls12
    }
}