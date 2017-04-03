using System.IO;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to handle the response for a Pull Request.
    /// </summary>
    public class PullRequestResponseHandler : IAS4ResponseHandler
    {
        private readonly ISerializerProvider _serializerProvider;
        private readonly IAS4ResponseHandler _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullRequestResponseHandler"/> class.
        /// </summary>
        /// <param name="serializerProvider">The serializer Provider.</param>
        /// <param name="nextHandler">The next Handler.</param>
        public PullRequestResponseHandler(ISerializerProvider serializerProvider, IAS4ResponseHandler nextHandler)
        {
            _serializerProvider = serializerProvider;
            _nextHandler = nextHandler;
        }

        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<StepResult> HandleResponse(IAS4Response response)
        {
            bool isPulling = response.ResultedMessage.AS4Message.IsPulling;
            AS4Message messageResponse = await DeserializeHttpResponse(response);
            response.ResultedMessage.AS4Message = messageResponse;

            bool isOriginatedFromPullRequest = (messageResponse.PrimarySignalMessage as Error)?.IsWarningForEmptyPullRequest == true;
            bool isRequestBeingSendAPullRequest = isPulling;

            if (isOriginatedFromPullRequest && isRequestBeingSendAPullRequest)
            {
                return StepResult.Success(response.ResultedMessage).AndStopExecution();
            }

            return await _nextHandler.HandleResponse(response);
        }

        private async Task<AS4Message> DeserializeHttpResponse(IAS4Response messageResponse)
        {
            string contentType = messageResponse.ContentType;
            Stream responseStream = messageResponse.GetResponseStream();
            ISerializer serializer = _serializerProvider.Get(contentType);

            AS4Message deserializedResponse = await serializer
                .DeserializeAsync(responseStream, contentType, messageResponse.Cancellation);

            deserializedResponse.SendingPMode = messageResponse.ResultedMessage.AS4Message.SendingPMode;

            return deserializedResponse;
        }
    }
}