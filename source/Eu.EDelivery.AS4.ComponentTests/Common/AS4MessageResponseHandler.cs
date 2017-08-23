using System.Net;
using System.Threading;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.ComponentTests.Common
{
    internal sealed class AS4MessageResponseHandler
    {
        private readonly AS4Message _responseMessage;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4MessageResponseHandler"/> class.
        /// </summary>
        public AS4MessageResponseHandler(AS4Message responseMessage)
        {
            _responseMessage = responseMessage;
        }

        public void WriteResponse(HttpListenerResponse response)
        {
            response.StatusCode = (int)HttpStatusCode.OK;
            response.ContentType = _responseMessage.ContentType;

            SerializerProvider.Default.Get(_responseMessage.ContentType).Serialize(_responseMessage, response.OutputStream, CancellationToken.None);
        }
    }
}