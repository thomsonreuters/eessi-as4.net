namespace Eu.EDelivery.AS4.Exceptions
{
    public class ErrorResult
    {
        public string Description { get; set; }

        public ErrorCode Code { get; set; }

        public ErrorAlias Alias { get; set; }
    }
}
