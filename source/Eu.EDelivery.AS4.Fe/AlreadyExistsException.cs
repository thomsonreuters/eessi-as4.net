namespace Eu.EDelivery.AS4.Fe
{
    /// <summary>
    /// Type of business exception thrown to indicate that the requested entity already exists
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.BusinessException" />
    public class AlreadyExistsException : BusinessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AlreadyExistsException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public AlreadyExistsException(string message) : base(message)
        {
        }
    }
}