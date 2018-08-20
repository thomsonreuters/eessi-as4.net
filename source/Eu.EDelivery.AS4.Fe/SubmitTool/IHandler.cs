namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Base Interface to implement submit message creator handlers
    /// </summary>
    public interface IHandler
    {
        /// <summary>
        /// Determines whether this instance can handle the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the specified location; otherwise, <c>false</c>.
        /// </returns>
        bool CanHandle(string location);
    }
}