using System.Security.Cryptography.X509Certificates;
using System.Xml.Serialization;

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
        public MessageExchangePatternBinding MepBinding { get; set; }
        public ReceiveReliability Reliability { get; set; }
        public ReceiveReceiptHandling ReceiptHandling { get; set; }
        public ReceiveErrorHandling ErrorHandling { get; set; }
        public Receivehandling ExceptionHandling { get; set; }

        [XmlElement(ElementName = "Security")] public ReceiveSecurity Security { get; set; }
        public MessagePackaging MessagePackaging { get; set; }
        public Deliver Deliver { get; set; }

        public ReceivingProcessingMode()
        {
            this.Reliability = new ReceiveReliability();
            this.ReceiptHandling = new ReceiveReceiptHandling();
            this.ErrorHandling = new ReceiveErrorHandling();
            this.ExceptionHandling = new Receivehandling();
            this.Security = new ReceiveSecurity();
            this.MessagePackaging = new MessagePackaging();
            this.Deliver = new Deliver();
        }
    }

    public class ReceiveReliability
    {
        public DuplicateElimination DuplicateElimination { get; set; }

        public ReceiveReliability()
        {
            this.DuplicateElimination = new DuplicateElimination();
        }
    }

    public class DuplicateElimination
    {
        public bool IsEnabled { get; set; }

        public DuplicateElimination()
        {
            this.IsEnabled = false;
        }
    }

    public class ReceiveReceiptHandling
    {
        public bool UseNNRFormat { get; set; }
        public ReplyPattern ReplyPattern { get; set; }
        public string CallbackUrl { get; set; }
        public string SendingPMode { get; set; }

        public ReceiveReceiptHandling()
        {
            this.UseNNRFormat = false;
            this.ReplyPattern = ReplyPattern.Response;
        }
    }

    public class Receivehandling
    {
        public bool NotifyMessageConsumer { get; set; }
        public Method NotifyMethod { get; set; }

        public Receivehandling()
        {
            this.NotifyMessageConsumer = false;
            this.NotifyMethod = new Method();
        }
    }

    public class ReceiveErrorHandling
    {
        public bool UseSoapFault { get; set; }
        public ReplyPattern ReplyPattern { get; set; }
        public string CallbackUrl { get; set; }
        public int ResponseHttpCode { get; set; }
        public string SendingPMode { get; set; }

        public ReceiveErrorHandling()
        {
            this.UseSoapFault = false;
        }
    }

    public class ReceiveSecurity
    {
        public SigningVerification SigningVerification { get; set; }
        public Decryption Decryption { get; set; }

        public ReceiveSecurity()
        {
            this.SigningVerification = new SigningVerification();
            this.Decryption = new Decryption();
        }
    }

    public class SigningVerification
    {
        public Limit Signature { get; set; }

        public SigningVerification()
        {
            this.Signature = Limit.Allowed;
        }
    }

    public class Decryption
    {
        public Limit Encryption { get; set; }
        public string PrivateKeyFindValue { get; set; }
        public X509FindType PrivateKeyFindType { get; set; }

        public Decryption()
        {
            this.Encryption = Limit.Allowed;
        }
    }

    public class Deliver
    {
        public bool IsEnabled { get; set; }
        public Method PayloadReferenceMethod { get; set; }
        public Method DeliverMethod { get; set; }

        public Deliver()
        {
            this.IsEnabled = false;
            this.PayloadReferenceMethod = new Method();
            this.DeliverMethod = new Method();
        }
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