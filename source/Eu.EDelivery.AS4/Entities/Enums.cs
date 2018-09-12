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

        ToBeRetried,

        ToBeNotified,
        Notifying,
        Notified,

        ToBeDelivered,
        Delivering,
        Delivered,

        ToBePiggyBacked
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
        Received = 0,
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
        NotApplicable = 0,
        Submitted,
        Nack,
        Ack,
        Sent,
        Exception,
        Created,
        Notified
    }
}