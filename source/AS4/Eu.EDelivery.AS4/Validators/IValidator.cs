namespace Eu.EDelivery.AS4.Validators
{
    /// <summary>
    /// Validate interface to describe the required
    /// items to define a valid Model
    /// </summary>
    /// <typeparam name="T">
    /// </typeparam>
    public interface IValidator<in T> where T : class
    {
        /// <summary>
        /// Validate the given <paramref name="model"/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        bool Validate(T model);
    }
}
