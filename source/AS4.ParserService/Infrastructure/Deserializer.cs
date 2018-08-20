using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;

namespace AS4.ParserService.Infrastructure
{
    internal class Deserializer
    {
        internal static async Task<SendingProcessingMode> ToSendingPMode(byte[] value)
        {
            using (var stream = new MemoryStream(value))
            {
                return await AS4XmlSerializer.FromStreamAsync<SendingProcessingMode>(stream);
            }
        }

        internal static async Task<ReceivingProcessingMode> ToReceivingPMode(byte[] value)
        {
            using (var stream = new MemoryStream(value))
            {
                return await AS4XmlSerializer.FromStreamAsync<ReceivingProcessingMode>(stream);
            }
        }
    }
}