using System;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
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
    public class ReceivingProcessingMode : IPMode
    {
        public string Id { get; set; }
        public MessageExchangePattern Mep { get; set; }
        [Info("Message exchange pattern", defaultValue: MessageExchangePatternBinding.Push)]
        public MessageExchangePatternBinding MepBinding { get; set; }
        public ReceiveReliability Reliability { get; set; }
        public ReplyHandlingSetting ReplyHandling { get; set; }
        public ReceiveHandling ExceptionHandling { get; set; }

        [XmlElement(ElementName = "Security")] public ReceiveSecurity Security { get; set; }
        public MessagePackaging MessagePackaging { get; set; }

        public MessageHandling MessageHandling { get; set; }

        public ReceivingProcessingMode()
        {
            Reliability = new ReceiveReliability();
            ReplyHandling = new ReplyHandlingSetting();
            ExceptionHandling = new ReceiveHandling();
            Security = new ReceiveSecurity();
            MessagePackaging = new MessagePackaging();
            MessageHandling = new MessageHandling();
        }
    }

    public class ReceiveReliability
    {
        public DuplicateElimination DuplicateElimination { get; set; }

        public ReceiveReliability()
        {
            DuplicateElimination = new DuplicateElimination();
        }
    }

    public class DuplicateElimination
    {
        [Description("Do not allow duplicate messages")]
        public bool IsEnabled { get; set; }

        public DuplicateElimination()
        {
            IsEnabled = false;
        }
    }

    public class ReplyHandlingSetting
    {
        public ReplyPattern ReplyPattern { get; set; }
        public string SendingPMode { get; set; }
        public ReceiveReceiptHandling ReceiptHandling { get; set; }
        public ReceiveErrorHandling ErrorHandling { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplyHandlingSetting"/> class.
        /// </summary>
        public ReplyHandlingSetting()
        {
            ReplyPattern = ReplyPattern.Response;
            ReceiptHandling = new ReceiveReceiptHandling();
            ErrorHandling = new ReceiveErrorHandling();
        }
    }

    public class ReceiveReceiptHandling
    {
        private bool? _useNNRFormat;

        public bool UseNNRFormat
        {
            get { return _useNNRFormat ?? false; }
            set { _useNNRFormat = value; }
        }

        public ReceiveReceiptHandling()
        {
            UseNNRFormat = false;
        }

        #region Serialization Control properties

        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public bool UseNNRFormatSpecified => _useNNRFormat.HasValue;

        #endregion
    }

    public class ReceiveHandling
    {
        public bool NotifyMessageConsumer { get; set; }
        public Method NotifyMethod { get; set; }

        public ReceiveHandling()
        {
            NotifyMessageConsumer = false;
            NotifyMethod = new Method();
        }
    }

    public class ReceiveErrorHandling
    {
        private bool? _useSoapFault;
        private int? _responseHttpCode;

        public bool UseSoapFault
        {
            get { return _useSoapFault ?? false; }
            set { _useSoapFault = value; }
        }
        public int ResponseHttpCode
        {
            get { return _responseHttpCode ?? 200; }
            set { _responseHttpCode = value; }
        }

        public ReceiveErrorHandling()
        {
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
        public SigningVerification SigningVerification { get; set; }
        public Decryption Decryption { get; set; }

        public ReceiveSecurity()
        {
            SigningVerification = new SigningVerification();
            Decryption = new Decryption();
        }
    }

    public class SigningVerification
    {
        public Limit Signature { get; set; }

        public SigningVerification()
        {
            Signature = Limit.Allowed;
        }
    }

    public class Decryption
    {
        public Limit Encryption { get; set; }
        public X509FindType PrivateKeyFindType { get; set; }
        public string PrivateKeyFindValue { get; set; }

        #region Serialization management

        [XmlIgnore]
        [JsonIgnore]
        public bool PrivateKeyFindTypeSpecified { get; set; }

        [XmlIgnore]
        [JsonIgnore]
        public bool PrivateKeyFindValueSpecified => !String.IsNullOrWhiteSpace(PrivateKeyFindValue);

        #endregion

        public Decryption()
        {
            Encryption = Limit.Allowed;
        }
    }

    public class MessageHandling
    {
        [XmlIgnore]
        [JsonIgnore]
        [ScriptIgnore]
        public MessageHandlingChoiceType MessageHandlingType { get; set; }

        private object _item;

        [XmlChoiceIdentifier(nameof(MessageHandlingType))]
        [XmlElement("Deliver", typeof(Deliver))]
        [XmlElement("Forward", typeof(Forward))]
        public object Item
        {
            get { return _item; }
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
                else
                {
                    MessageHandlingType = MessageHandlingChoiceType.None;
                }
            }
        }

        public Deliver DeliverInformation => Item as Deliver;

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
        public bool IsEnabled { get; set; }
        public Method PayloadReferenceMethod { get; set; }
        public Method DeliverMethod { get; set; }

        public Deliver()
        {
            IsEnabled = false;
            PayloadReferenceMethod = new Method();
            DeliverMethod = new Method();
        }
    }

    public class Forward
    {
        /// <summary>
        /// The Id of the Sending ProcessingMode that must be used to forward the received AS4 message.
        /// </summary>
        public string SendingPMode { get; set; }
    }

    public enum MessageHandlingChoiceType
    {
        None = 0,
        Deliver,
        Forward
    }

    public enum ReplyPattern
    {
        Response = 0,
        Callback
    }

    public enum Limit
    {
        Allowed = 0,
        NotAllowed,
        Required,
        Ignored
    }
}