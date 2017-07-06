namespace Eu.EDelivery.AS4.Fe
{
    /// <summary>
    /// Type of business exception thrown to indicate that an entity could not be found
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.BusinessException" />
    public class NotFoundException : BusinessException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotFoundException"/> class.
        /// </summary>
        /// <param name="message">The message that describes the error.</param>
        public NotFoundException(string message) : base(message)
        {
        }
    }
}