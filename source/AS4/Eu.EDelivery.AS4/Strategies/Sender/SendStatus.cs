namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public enum SendResult
    {
        Success,
        RetryableFail,
        FatalFail
    }

    public static class SendResultUtils
    {
        public static SendResult Reduce(SendResult x, SendResult y)
        {
            bool bothScuccess =
                x == SendResult.Success
                && y == SendResult.Success;

            if (bothScuccess)
            {
                return SendResult.Success;
            }

            return x == SendResult.FatalFail || y == SendResult.FatalFail
                ? SendResult.FatalFail
                : SendResult.RetryableFail;
        }
    }
}
