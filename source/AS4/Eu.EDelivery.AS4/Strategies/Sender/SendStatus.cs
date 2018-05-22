namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public enum SendStatus
    {
        Success,
        RetryableFail,
        Fail
    }

    public static class SendStatusEx
    {
        public static SendStatus Reduce(SendStatus x, SendStatus y)
        {
            bool bothScuccess =
                x == SendStatus.Success
                && y == SendStatus.Success;

            if (bothScuccess)
            {
                return SendStatus.Success;
            }

            return x == SendStatus.Fail || y == SendStatus.Fail
                ? SendStatus.Fail
                : SendStatus.RetryableFail;
        }
    }
}
