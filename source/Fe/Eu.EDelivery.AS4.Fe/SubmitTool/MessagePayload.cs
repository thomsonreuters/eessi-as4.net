using System.Collections.Generic;
using HttpMultipartParser;

namespace Eu.EDelivery.AS4.Fe.SubmitTool
{
    public class MessagePayload
    {
        public string SendingPmode { get; set; } = "8.1.1-pmode";
        public int NumberOfSubmitMessages { get; set; } = 10;
        public IList<FilePart> Files { get; set; }
        public string PayloadLocation { get; set; }
        public string To { get; set; }
    }
}