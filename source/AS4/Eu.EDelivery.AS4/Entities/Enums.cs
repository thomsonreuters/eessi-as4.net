using System;

namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The operation field is used to control the interaction between the different asynchronous agents
    /// </summary>
    public enum Operation
    {
        NotApplicable = 0,
        Undetermined,

        ToBeProcessed,
        Processing,

        ToBeSent,
        Sending,
        Sent,
        DeadLettered,

        ToBeForwarded,
        Forwarding,
        Forwarded,

        ToBeNotified,
        Notifying,
        Notified,

        ToBeDelivered,
        Delivering,
        Delivered
    }


    public static class OperationUtils
    {
        public static Operation Parse(string operationString)
        {
            return (Operation)Enum.Parse(typeof(Operation), operationString, true);
        }
    }

    public enum MessageExchangePattern
    {
        Push = 0,
        Pull = 1
    }

    public static class MessageExchangePatternUtils
    {
        public static MessageExchangePattern Parse(string mepString)
        {
            return (MessageExchangePattern)Enum.Parse(typeof(MessageExchangePattern), mepString, true);
        }
    }

    public enum MessageType
    {
        UserMessage,
        Error,
        Receipt
    }

    public static class MessageTypeUtils
    {
        public static MessageType Parse(string messageTypeString)
        {
            return (MessageType)Enum.Parse(typeof(MessageType), messageTypeString, true);
        }
    }

    /// <summary>
    /// The status field is used for monitoring purposes.
    /// It has the following state machine applied for incoming messages
    /// </summary>
    public enum InStatus
    {
        Received = 0,
        Delivered,
        Created,
        Notified,
        Exception
    }

    public static class InStatusUtils
    {
        public static InStatus Parse(string status)
        {
            return (InStatus)Enum.Parse(typeof(InStatus), status, true);
        }
    }

    /// <summary>
    /// The status field is used for monitoring purposes.
    /// It has the following state machine applied for outgoing messages
    /// </summary>
    public enum OutStatus
    {
        NotApplicable = 0,
        Submitted,
        Nack,
        Ack,
        Sent,
        Exception,
        Created,
        Notified
    }

    public static class OutStatusUtils
    {
        public static OutStatus Parse(string status)
        {
            return (OutStatus)Enum.Parse(typeof(OutStatus), status, true);
        }
    }

    public enum Entities
    {
        OutMessage,
        InMessage,
        OutException,
        InException,
        ReceptionAwareness
    }
}