using System;
using System.Net;
using System.Threading.Tasks;
using SimpleHttpMock;

namespace Eu.EDelivery.AS4.IntegrationTests.Positive_Receive_Scenarios._8._3._17_Receive_Single_Payload_with_HTTP_Deliver
{
    public class StubHttpDeliverTarget : IDisposable
    {
        private MockedHttpServer _httpServer;

        /// <summary>
        /// Gets the Deliver Message Content
        /// </summary>
        public string DeliverMessageContent { get; private set; }

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
                    Task.WhenAll(task).ContinueWith(t => target.DeliverMessageContent = task.Result).Wait();

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
