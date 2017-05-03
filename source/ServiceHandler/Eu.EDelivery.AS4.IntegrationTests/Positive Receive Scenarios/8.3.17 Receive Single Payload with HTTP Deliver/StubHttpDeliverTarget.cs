using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using SimpleHttpMock;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._17_Receive_Single_Payload_with_HTTP_Deliver
{
    public class StubHttpDeliverTarget : IDisposable
    {
        private readonly ManualResetEvent _waitHandle;
        private MockedHttpServer _httpServer;

        /// <summary>
        /// Initializes a new instance of the <see cref="StubHttpDeliverTarget"/> class.
        /// </summary>
        public StubHttpDeliverTarget()
        {
            _waitHandle = new ManualResetEvent(initialState: false);
        }

        /// <summary>
        /// Gets the Deliver Message Content
        /// </summary>
        public string DeliveredMessage { get; private set; }

        /// <summary>
        /// Gets the value to indicate whether the Stub HTTP server is called or not.
        /// </summary>
        public bool IsCalled => _waitHandle.WaitOne(TimeSpan.FromMinutes(2));

        /// <summary>
        /// Create Stub target at a given <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The HTTP Location.</param>
        /// <returns></returns>
        public static StubHttpDeliverTarget AtLocation(string location)
        {
            var builder = new MockedHttpServerBuilder();
            var target = new StubHttpDeliverTarget();

            builder.WhenPost(location).RespondContent(
                httpStatusCode: HttpStatusCode.OK,
                contentFn: request =>
                {
                    Task<string> task = request.Content.ReadAsStringAsync();
                    target.DeliveredMessage = task.Result;

                    target._waitHandle.Set();

                    return null;
                });

            target._httpServer = builder.Build(location);
            return target;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _httpServer.Dispose();
        }
    }
}
