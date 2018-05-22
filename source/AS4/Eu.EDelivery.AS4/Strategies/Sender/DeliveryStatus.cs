namespace Eu.EDelivery.AS4.Strategies.Sender
{
    public enum DeliveryStatus
    {
        Success,
        RetryableFail,
        Fail
    }

    public static class DeliveryStatusEx
    {
        public static DeliveryStatus Reduce(DeliveryStatus x, DeliveryStatus y)
        {
            bool bothScuccess =
                x == DeliveryStatus.Success
                && y == DeliveryStatus.Success;

            if (bothScuccess)
            {
                return DeliveryStatus.Success;
            }

            return x == DeliveryStatus.Fail || y == DeliveryStatus.Fail
                ? DeliveryStatus.Fail
                : DeliveryStatus.RetryableFail;
        }
    }
}
