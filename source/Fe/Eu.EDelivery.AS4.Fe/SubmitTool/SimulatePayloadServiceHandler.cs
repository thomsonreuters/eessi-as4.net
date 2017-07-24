using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Handle dummy payloads
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IPayloadHandler" />
    public class SimulatePayloadServiceHandler : IPayloadHandler
    {
        /// <summary>
        /// Determines whether this instance can handle the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the specified location; otherwise, <c>false</c>.
        /// </returns>
        public bool CanHandle(string location)
        {
            return location.ToLower().StartsWith("simulate://");
        }

        /// <summary>
        /// Handles the specified location.
        /// </summary>
        /// <param name="location">The location to send to payload to.</param>
        /// <param name="fileName"></param>
        /// <param name="stream">The stream containing the payload.</param>
        /// <returns>
        /// String containing the location of the file.
        /// </returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public Task<string> Handle(string location, string fileName, Stream stream)
        {
            return Task.FromResult(string.Concat(location, fileName));
        }
    }
}