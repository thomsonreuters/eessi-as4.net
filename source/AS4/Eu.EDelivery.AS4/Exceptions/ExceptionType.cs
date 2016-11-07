namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// Exception Type used to determine if an AS4 Message 
    /// has to be resend when an AS4 Exception occur
    /// </summary>
    public enum ExceptionType
    {
        NonApplicable = 0,

        ConnectionFailure,
        ValueNotRecognized,
        FeatureNotSupported,
        ValueInconsistent,
        EmptyMessagePartitionChannel,
        MimeInconsistency,
        InvalidHeader,
        ProcessingModeMismatch,
        ExternalPayloadError
    }
}
