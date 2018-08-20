using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Payload handler interface
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IHandler" />
    public interface IPayloadHandler : IHandler
    {
        /// <summary>
        /// Handles the specified location.
        /// </summary>
        /// <param name="location">The location to send to payload to.</param>
        /// <param name="fileName"></param>
        /// <param name="stream">The stream containing the payload.</param>
        /// <returns>String containing the location of the file.</returns>
        Task<string> Handle(string location, string fileName, Stream stream);
    }
}