using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.Steps.Send.Response
{
    /// <summary>
    /// Contract for a chain of handlers that handle the <see cref="AS4Response" />.
    /// </summary>
    public interface IAS4ResponseHandler
    {
        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        Task<StepResult> HandleResponse(IAS4Response response);
    }

    /// <summary>
    /// <see cref="IAS4ResponseHandler"/> implementation to handle the response for a empty body.
    /// </summary>
    public class EmptyBodyResponseHandler : IAS4ResponseHandler
    {
        private readonly IAS4ResponseHandler _nextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmptyBodyResponseHandler"/> class.
        /// </summary>
        /// <param name="nextHandler">The next Handler.</param>
        public EmptyBodyResponseHandler(IAS4ResponseHandler nextHandler)
        {
            _nextHandler = nextHandler;
        }

        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public async Task<StepResult> HandleResponse(IAS4Response response)
        {
            InternalMessage resultedMessage = response.ResultedMessage;

            if (response.StatusCode == HttpStatusCode.Accepted)
            {
                resultedMessage.AS4Message.SignalMessages.Clear();

                return await StepResult.SuccessAsync(resultedMessage);
            }

            return await _nextHandler.HandleResponse(response);
        }
    }

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

    /// <summary>
    /// <see cref="IAS4ResponseHandler" /> implementation that can be used as the 'Tail' of the chain of
    /// <see cref="IAS4ResponseHandler" />.
    /// </summary>
    public class TailResponseHandler : IAS4ResponseHandler
    {
        /// <summary>
        /// Handle the given <paramref name="response" />, but delegate to the next handler if you can't.
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        public Task<StepResult> HandleResponse(IAS4Response response)
        {
            return StepResult.SuccessAsync(response.ResultedMessage);
        }
    }

    /// <summary>
    /// <see cref="IAS4Response" /> HTTP Web Response implementation.
    /// </summary>
    public class AS4Response : IAS4Response
    {
        private readonly HttpWebResponse _httpWebResponse;

        /// <summary>
        /// Initializes a new instance of the <see cref="AS4Response" /> class.
        /// </summary>
        /// <param name="webResponse">The web Response.</param>
        /// <param name="resultedMessage">The resulted Message.</param>
        /// <param name="cancellation">The cancellation.</param>
        public AS4Response(HttpWebResponse webResponse, InternalMessage resultedMessage, CancellationToken cancellation)
        {
            _httpWebResponse = webResponse;

            ResultedMessage = resultedMessage;
            Cancellation = cancellation;
        }

        /// <summary>
        /// Gets the Conten Type of the HTTP response.
        /// </summary>
        public string ContentType => _httpWebResponse?.ContentType;

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        public HttpStatusCode StatusCode => _httpWebResponse?.StatusCode ?? HttpStatusCode.InternalServerError;

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        public InternalMessage ResultedMessage { get; }

        /// <summary>
        /// Gets the cancellation information during the handling of the AS4 response.
        /// </summary>
        public CancellationToken Cancellation { get; }

        /// <summary>
        /// Get the serialized stream of the HTTP response.
        /// </summary>
        /// <returns></returns>
        public Stream GetResponseStream()
        {
            return _httpWebResponse?.GetResponseStream();
        }
    }

    /// <summary>
    /// Contract to define the HTTP/AS4 response being handled.
    /// </summary>
    public interface IAS4Response
    {
        /// <summary>
        /// Gets the Conten Type of the HTTP response.
        /// </summary>
        string ContentType { get; }

        /// <summary>
        /// Gets the HTTP Status Code of the HTTP response.
        /// </summary>
        HttpStatusCode StatusCode { get; }

        /// <summary>
        /// Gets the Message from the AS4 response.
        /// </summary>
        InternalMessage ResultedMessage { get; }

        /// <summary>
        /// Gets the cancellation information during the handling of the AS4 response.
        /// </summary>
        CancellationToken Cancellation { get; }

        /// <summary>
        /// Get the serialized stream of the HTTP response.
        /// </summary>
        /// <returns></returns>
        Stream GetResponseStream();
    }
}