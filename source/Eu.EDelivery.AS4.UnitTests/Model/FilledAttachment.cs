using System.IO;
using System.Text;
using Eu.EDelivery.AS4.Model.Core;

namespace Eu.EDelivery.AS4.UnitTests.Model
{
    public class FilledAttachment : Attachment
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FilledAttachment"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public FilledAttachment(string id = "attachment-id") 
            : base(id, new MemoryStream(Encoding.UTF8.GetBytes("content!")), "text/plain") { }
    }
}
