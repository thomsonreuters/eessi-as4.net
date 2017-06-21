namespace Eu.EDelivery.AS4.Exceptions
{
    public class ErrorResult
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ErrorResult" /> class.
        /// </summary>
        /// <param name="description">The description.</param>
        /// <param name="code">The code.</param>
        /// <param name="alias">The alias.</param>
        public ErrorResult(string description, ErrorCode code, ErrorAlias alias)
        {
            Description = description;
            Code = code;
            Alias = alias;
        }

        public string Description { get; }

        public ErrorCode Code { get; }

        public ErrorAlias Alias { get; }
    }
}
