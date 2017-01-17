using System.Collections.Generic;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Exceptions;

namespace Eu.EDelivery.AS4.Model.Core
{
    public class Error : SignalMessage
    {
        [XmlIgnore] public AS4Exception Exception { get; set; }
        [XmlIgnore] public bool IsFormedByException => this.Exception != null;
        

        public IList<ErrorDetail> Errors { get; set; }

        /// <summary>
        /// Initializes a new instance of the type <see cref="Error"/> class
        /// </summary>
        public Error() {}

        /// <summary>
        /// Initializes a new instance of the type <see cref="Error"/> class
        /// with a given <paramref name="messageId"/>
        /// </summary>
        /// <param name="messageId"></param>
        public Error(string messageId) : base(messageId) {}

        public override string GetActionValue()
        {
            return "http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/oneWay.error";
        }
    }

    public class ErrorDescription
    {
        public string Language { get; set; }
        public string Value { get; set; }
    }

    public class ErrorDetail
    {
        public string ErrorCode { get; set; }
        public Severity Severity { get; set; }
        public string Origin { get; set; }
        public string Category { get; set; }
        public string RefToMessageInError { get; set; }
        public string ShortDescription { get; set; }
        public ErrorDescription Description { get; set; }
        public string Detail { get; set; }
    }

    public enum Severity
    {
        FAILURE,
        WARNING
    }
}