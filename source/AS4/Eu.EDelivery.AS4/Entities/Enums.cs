namespace Eu.EDelivery.AS4.Entities
{
    /// <summary>
    /// The operation field is used to control the interaction between the different asynchronous agents
    /// </summary>
    public enum Operation
    {
        NotApplicable,
        Undetermined,

        ToBeProcessed,
        Processing,

        ToBeSent,
        Sending,
        Sent,
        DeadLettered,

        ToBeForwarded,
        Forwarded,

        ToBeNotified,
        Notifying,
        Notified,

        ToBeDelivered,
        Delivering,
        Delivered
    }

    public enum MessageExchangePattern
    {
        Push = 0,
        Pull = 1
    }

    public enum MessageType
    {
        UserMessage,
        Error,
        Receipt
    }

    /// <summary>
    /// The status field is used for monitoring purposes.
    /// It has the following state machine applied for incoming messages
    /// </summary>
    public enum InStatus
    {
        Received,
        Delivered,
        Created,
        Notified,
        Exception
    }

    /// <summary>
    /// The status field is used for monitoring purposes.
    /// It has the following state machine applied for outgoing messages
    /// </summary>
    public enum OutStatus
    {
        NotApplicable,
        Submitted,
        Nack,
        Ack,
        Sent,
        Exception,
        Created,
        Notified
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