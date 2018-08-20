using System;
using System.IO;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Handle payloads by saving them to a file
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IPayloadHandler" />
    public class FilePayloadHandler : IPayloadHandler
    {
        /// <summary>
        /// Determines whether this instance can handle the specified location.
        /// </summary>
        /// <param name="location">The location.</param>
        /// <returns>
        ///   <c>true</c> if this instance can handle the specified location; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="NotImplementedException"></exception>
        public bool CanHandle(string location)
        {
            return !location.ToLower().Contains("http");
        }

        /// <summary>
        /// Handles the specified location.
        /// </summary>
        /// <param name="location">The location to send to payload to.</param>
        /// <param name="fileName"></param>
        /// <param name="stream">The stream containing the payload.</param>
        /// <returns>
        /// String containing the location of the payload.
        /// </returns>
        public async Task<string> Handle(string location, string fileName, Stream stream)
        {
            using (var fileStream = new FileStream(Path.Combine(location, fileName), FileMode.CreateNew))
            {
                await stream.CopyToAsync(fileStream);
                return $"file:///{fileName}";
            }
        }
    }
}