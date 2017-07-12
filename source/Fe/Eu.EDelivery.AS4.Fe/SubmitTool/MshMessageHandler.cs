using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// Send messages to an MSH endpoint
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IMessageHandler" />
    public class MshMessageHandler : IMessageHandler
    {
        private static readonly XmlWriterSettings Settings = new XmlWriterSettings
        {
            CloseOutput = false,
            Encoding = new UTF8Encoding(true)
        };

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
            return location.ToLower().StartsWith("http");
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary> 
        /// <param name="message">The message.</param>
        /// <param name="toLocation">To location.</param>
        /// <returns></returns>
        /// <exception cref="BusinessException">
        /// </exception>
        public async Task Handle(SubmitMessage message, string toLocation)
        {
            using (var client = new HttpClient())
            {
                var stringBuilder = new StringBuilder();
                using (var xmlWriter = XmlWriter.Create(stringBuilder, Settings))
                {
                    var serializer = new XmlSerializer(typeof(SubmitMessage));
                    serializer.Serialize(xmlWriter, message);
                    try
                    {
                        var result = await client.PostAsync(toLocation, new StringContent(stringBuilder.ToString(), Encoding.Unicode, "application/soap+xml"));
                        result.EnsureSuccessStatusCode();
                    }
                    catch (HttpRequestException)
                    {
                        throw new BusinessException($"Could not send message to {toLocation}. Please check that the address is correct.");
                    }
                    catch (Exception)
                    {
                        throw new BusinessException($"Unexpected error while trying to send the message to ${toLocation}.");
                    }
                }
            }
        }
    }
}