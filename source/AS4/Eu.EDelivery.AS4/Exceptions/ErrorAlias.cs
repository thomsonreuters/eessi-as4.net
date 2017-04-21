namespace Eu.EDelivery.AS4.Exceptions
{
    /// <summary>
    /// Short Description for the <see cref="ErrorCode"/>.
    /// </summary>
    public enum ErrorAlias
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
        ExternalPayloadError,
        Other,
        FailedAuthentication,
        FailedDecryption,
        MissingReceipt,
        InvalidReceipt,
        DecompressionFailure
    }
}
