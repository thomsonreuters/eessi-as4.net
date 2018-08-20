using System.Net;

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

        public static SendResult DetermineSendResultFromHttpResonse(HttpStatusCode statusCode)
        {
            var code = (int) statusCode;
            if (200 <= code && code < 300)
            {
                return SendResult.Success;
            }

            if (500 <= code && code < 600
                || code == 408
                || code == 429)
            {
                return SendResult.RetryableFail;
            }

            return SendResult.FatalFail;
        }
    }
}
