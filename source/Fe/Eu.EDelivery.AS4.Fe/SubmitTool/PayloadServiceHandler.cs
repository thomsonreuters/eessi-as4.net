using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Implementation to upload payloads to a payload service
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IPayloadHandler" />
    public class PayloadServiceHandler : IPayloadHandler
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
            return location.ToLower().StartsWith("http");
        }

        /// <summary>
        /// Handles the specified location.
        /// </summary>
        /// <param name="location">The location to send to payload to.</param>
        /// <param name="fileName"></param>
        /// <param name="stream">The stream containing the payload.</param>
        /// <returns>String containing the download url to be used in the message.</returns>
        public async Task<string> Handle(string location, string fileName, Stream stream)
        {
            // Http upload
            using (var client = new HttpClient())
            {
                var form = new MultipartFormDataContent();
                var content = new StreamContent(stream);
                form.Add(content);
                var response = await client.PostAsync(location, form);
                response.EnsureSuccessStatusCode();
                var result = await response.Content.ReadAsStringAsync();
                return JObject.Parse(result)["downloadUrl"].Value<string>();
            }
        }
    }
}