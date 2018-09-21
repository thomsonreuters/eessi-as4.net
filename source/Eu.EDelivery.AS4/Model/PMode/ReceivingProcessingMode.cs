using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Web.Script.Serialization;
using System.Xml.Serialization;
using Newtonsoft.Json;

namespace Eu.EDelivery.AS4.Model.PMode
{
    /// <summary>
    /// Receiving PMode configuration
    /// </summary>
    [XmlType(Namespace = "eu:edelivery:as4:pmode")]
    [XmlRoot("PMode", Namespace = "eu:edelivery:as4:pmode", IsNullable = false)]
    [DebuggerDisplay("PMode Id = {" + nameof(Id) + "}")]
    public class ReceivingProcessingMode : IPMode
    {
        public string Id { get; set; }

        [Description("Receive reliability")]
        public ReceiveReliability Reliability { get; set; }
        [Description("Configure settings for reply handling")]
        public ReplyHandling ReplyHandling { get; set; }
        [Description("Configure settings for exception handling")]
        public ReceiveHandling ExceptionHandling { get; set; }

        [XmlElement(ElementName = "Security")]
        public ReceiveSecurity Security { get; set; }

        [Description("Message packaging")]
        public MessagePackaging MessagePackaging { get; set; }

        [Info("Message handling")]
        public MessageHandling MessageHandling { get; set; }

        public ReceivingProcessingMode()
        {
            Reliability = new ReceiveReliability();
            ReplyHandling = new ReplyHandling();
            ExceptionHandling = new ReceiveHandling();
            Security = new ReceiveSecurity();
            MessagePackaging = new MessagePackaging();
            MessageHandling = new MessageHandling();
        }
    }

    public class ReceiveReliability
    {
        [Description("Duplicate elimination")]
        public DuplicateElimination DuplicateElimination { get; set; }

        public ReceiveReliability()
        {
            DuplicateElimination = new DuplicateElimination();
        }
    }

    public class DuplicateElimination
    {
        [Description("Do not deliver duplicate messages")]
        public bool IsEnabled { get; set; }

        public DuplicateElimination()
        {
            IsEnabled = false;
        }
    }

    public class ReplyHandling
    {
        public const ReplyPattern DefaultReplyPattern = ReplyPattern.Response;

        [Info("Reply pattern", defaultValue: DefaultReplyPattern)]
        [Description("Define whether the response must be sent directly by writing it to the Response stream of the HTTP call, or if it should be sent asynchronously by sending it later.")]
        public ReplyPattern ReplyPattern { get; set; }

        [Description("ID of the (sending) PMode that must be used to send the Receipt or Error message.")]
        public string SendingPMode { get; set; }

        [Description("Receipt handling")]
        public ReceiveReceiptHandling ReceiptHandling { get; set; }

        [Description("Error handling")]
        public ReceiveErrorHandling ErrorHandling { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyHandling"/> class.
        /// </summary>
        public ReplyHandling()
        {
            ReplyPattern = ReplyPattern.Response;
            ReceiptHandling = new ReceiveReceiptHandling();
            ErrorHandling = new ReceiveErrorHandling();
        }
    }

    public class ReceiveReceiptHandling
    {
        private bool? _useNRRFormat;

        /// <summary>
        /// Flag that determines whether of not Non-Repudiation of Receipt must be used.
        /// </summary>
        [Description("Indicate if the Non-Repudiation Information of the received UserMessage must be included in the " +
                     "Receipt that will be sent.")]
        public bool UseNRRFormat
        {
            get => _useNRRFormat ?? false;
            set => _useNRRFormat = value;
        }

        public ReceiveReceiptHandling()
        {
            UseNRRFormat = false;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseNRRFormatSpecified => _useNRRFormat.HasValue;

        #endregion
    }

    public class ReceiveHandling
    {
        [Description("Notify message consumer")]
        public bool NotifyMessageConsumer { get; set; }

        [Description("Method for notification")]
        public Method NotifyMethod { get; set; }

        [Description("Notify exception reliability")]
        public RetryReliability Reliability { get; set; }

        public ReceiveHandling()
        {
            NotifyMessageConsumer = false;
            NotifyMethod = new Method();
            Reliability = new RetryReliability();
        }
    }

    public class ReceiveErrorHandling
    {
        private bool? _useSoapFault;
        private int? _responseHttpCode;

        [Description("Use soap fault")]
        public bool UseSoapFault
        {
            get => _useSoapFault ?? false;
            set => _useSoapFault = value;
        }

        [Info("Response HTTP status code", defaultValue: 200)]
        [Description("HTTP statuscode that must be used when an Error signalmessage is sent.")]
        public int ResponseHttpCode
        {
            get => _responseHttpCode ?? 200;
            set => _responseHttpCode = value;
        }

        #region Serialization Control Properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseSoapFaultSpecified => _useSoapFault.HasValue;

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool ResponseHttpCodeSpecified => _responseHttpCode.HasValue;

        #endregion
    }

    public class ReceiveSecurity
    {
        [Description("Signing verification settings")]
        public SigningVerification SigningVerification { get; set; }

        [Description("Decryption settings")]
        public Decryption Decryption { get; set; }

        public ReceiveSecurity()
        {
            SigningVerification = new SigningVerification();
            Decryption = new Decryption();
        }
    }

    public class SigningVerification
    {
        private bool? _allowUnknownRootCertificate;

        [Description("Define if the received Message must be signed (Required), may not be signed (Not Allowed), can be signed (Allowed) or if the signature must not be verified if present (Ignored).")]
        public Limit Signature { get; set; }

        [Info("Allow unknown root certificates")]
        [Description(
            "When enabled, signature verification will not fail if the certificate chain ends in a root certificate that is not trusted. The default value is false.")]
        public bool AllowUnknownRootCertificate
        {
            get => _allowUnknownRootCertificate ?? false;
            set => _allowUnknownRootCertificate = value;
        }

        public SigningVerification()
        {
            Signature = Limit.Allowed;
            AllowUnknownRootCertificate = false;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool AllowUnknownRootCertificateSpecified => _allowUnknownRootCertificate.HasValue;

        #endregion
    }

    public class Decryption
    {
        private object _decryptCertificateInformation;

        /// <summary>
        /// Initializes a new instance of the <see cref="Decryption"/> class.
        /// </summary>
        public Decryption()
        {
            Encryption = Limit.Ignored;
            CertificateType = PrivateKeyCertificateChoiceType.None;
        }

        [Description("Define if the received Message must be encrypted (Required), may not be encrypted (Not Allowed), can be encrypted (Allowed) or should not be decrypted (Ignored)")]
        public Limit Encryption { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public PrivateKeyCertificateChoiceType CertificateType { get; set; }

        [XmlChoiceIdentifier(nameof(CertificateType))]
        [XmlElement("CertificateFindCriteria", typeof(CertificateFindCriteria))]
        [XmlElement("PrivateKeyCertificate", typeof(PrivateKeyCertificate))]
        [Description("Decryption Certificate")]
        public object DecryptCertificateInformation
        {
            get => _decryptCertificateInformation;
            set
            {
                _decryptCertificateInformation = value;
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

        #region Serialization management

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool DecryptCertificateInformationSpecified => DecryptCertificateInformation != null;

        #endregion

    }

    public class MessageHandling
    {
        [XmlIgnore]
        [ScriptIgnore]
        [Description("Define if the received message must be delivered or forwarded.")]
        public MessageHandlingChoiceType MessageHandlingType { get; set; }

        private object _item;

        [XmlChoiceIdentifier(nameof(MessageHandlingType))]
        [XmlElement("Deliver", typeof(Deliver))]
        [XmlElement("Forward", typeof(Forward))]
        [JsonConverter(typeof(MessageHandlingConverter))]
        public object Item
        {
            get => _item;
            set
            {
                _item = value;
                if (_item is Deliver)
                {
                    MessageHandlingType = MessageHandlingChoiceType.Deliver;
                }
                else if (_item is Forward)
                {
                    MessageHandlingType = MessageHandlingChoiceType.Forward;
                }
            }
        }

        [Description("Settings for message delivery")]
        public Deliver DeliverInformation => Item as Deliver;

        [Description("Settings for message forwarding")]
        public Forward ForwardInformation => Item as Forward;

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageHandling"/> class.
        /// </summary>
        public MessageHandling()
        {
            MessageHandlingType = MessageHandlingChoiceType.Deliver;
            Item = new Deliver();
        }
    }

    public class Deliver
    {
        [Description("Indicate whether or not the Message Payloads must be delivered.")]
        public bool IsEnabled { get; set; }

        [Description("Payload delivery method")]
        public Method PayloadReferenceMethod { get; set; }

        [Description("Deliver method")]
        public Method DeliverMethod { get; set; }

        [Description("Deliver reliability")]
        public RetryReliability Reliability { get; set; }

        public Deliver()
        {
            IsEnabled = false;
            PayloadReferenceMethod = new Method();
            DeliverMethod = new Method();
            Reliability = new RetryReliability();
        }
    }

    public class RetryReliability
    {
        private TimeSpan _retryInterval;

        [Description("Indicate wheter or not the deliver operation should be retried on failure.")]
        public bool IsEnabled { get; set; }

        [Info("Retry count", defaultValue: 4)]
        [JsonConverter(typeof(SafeIntJsonConverter))]
        [Description("Amount of retry cycles the deliver operation should be retried on failure.")]
        public int RetryCount { get; set; }

        [XmlElement("RetryInterval")]
        [Info("Retry interval", defaultValue: "0:00:01:00")]
        [Description("Time interval between each retry cycle the deliver operation should be retried on failure.")]
        public string RetryInterval
        {
            get => _retryInterval.ToString("G");
            set => TimeSpan.TryParse(value, out _retryInterval);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetryReliability"/> class.
        /// </summary>
        public RetryReliability()
        {
            _retryInterval = default(TimeSpan);

            IsEnabled = false;
            RetryCount = 4;
            RetryInterval = TimeSpan.FromMinutes(1).ToString("G");
        }
    }

    public class Forward
    {
        /// <summary>
        /// The Id of the Sending ProcessingMode that must be used to forward the received AS4 message.
        /// </summary>
        [Description("The Id of the Sending ProcessingMode that must be used to forward the received AS4 message.")]
        public string SendingPMode { get; set; }
    }

    public enum MessageHandlingChoiceType
    {
        Deliver = 1,
        Forward
    }

    public enum ReplyPattern
    {
        Response = 0,
        Callback,
        PiggyBack
    }

    public enum Limit
    {
        Allowed = 0,
        NotAllowed,
        Required,
        Ignored
    }
}