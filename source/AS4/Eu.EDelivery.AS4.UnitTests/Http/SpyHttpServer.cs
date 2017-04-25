using System;
using System.Net;
using System.Threading;
using SimpleHttpMock;

namespace Eu.EDelivery.AS4.UnitTests.Http
{
    public class SpyHttpServer : IDisposable
    {
        private readonly ManualResetEvent _waitHandle;
        private MockedHttpServer _httpServer;

        /// <summary>
        /// Prevents a default instance of the <see cref="SpyHttpServer"/> class from being created. 
        /// </summary>
        private SpyHttpServer()
        {
            _waitHandle = new ManualResetEvent(initialState: false);
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SpyHttpServer"/> is being called.
        /// </summary>
        public bool IsCalled => _waitHandle.WaitOne(timeout: TimeSpan.FromSeconds(5));

        /// <summary>
        /// Create a <see cref="SpyHttpServer"/> instance that returns a given <paramref name="statusCode"/>.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="statusCode">The HTTP Status Code.</param>
        /// <returns></returns>
        public static SpyHttpServer SpyOn(string url, HttpStatusCode statusCode = HttpStatusCode.OK)
        {
            var builder = new MockedHttpServerBuilder();
            var spyServer = new SpyHttpServer();

            builder.WhenPost(url).RespondContent(
                httpStatusCode: statusCode,
                contentFn: request =>
                {
                    spyServer._waitHandle.Set();
                    return null;
                });

            spyServer._httpServer = builder.Build(url);

            return spyServer;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _waitHandle?.Dispose();
            _httpServer?.Dispose();
        }
    }
}
