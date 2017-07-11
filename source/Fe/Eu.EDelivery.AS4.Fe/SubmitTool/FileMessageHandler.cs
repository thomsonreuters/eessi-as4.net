using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Eu.EDelivery.AS4.Model.Submit;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    /// <summary>
    /// File system message handler
    /// </summary>
    /// <seealso cref="Eu.EDelivery.AS4.Fe.SubmitTool.IMessageHandler" />
    public class FileMessageHandler : IMessageHandler
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
            return !location.ToLower().StartsWith("http");
        }

        /// <summary>
        /// Handles the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="toLocation">To location.</param>
        /// <returns></returns>
        /// <exception cref="BusinessException"></exception>
        public async Task Handle(SubmitMessage message, string toLocation)
        {
            try
            {
                await Task.Run(() =>
                {
                    var serializer = new XmlSerializer(typeof(SubmitMessage));
                    using (var fs = File.Create(Path.Combine(toLocation, $"{message.MessageInfo.MessageId}.xml")))
                    {
                        serializer.Serialize(fs, message);
                    }
                });
            }
            catch (Exception)
            {
                throw new BusinessException($"Could not save file to location {toLocation}. Check that the server has access and that the to location is valid.");
            }
        }
    }
}