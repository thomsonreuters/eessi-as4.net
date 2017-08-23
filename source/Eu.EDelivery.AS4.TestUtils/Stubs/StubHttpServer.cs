using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Eu.EDelivery.AS4.TestUtils.Stubs
{
    public static class StubHttpServer
    {
        /// <summary>
        /// Starts a Http Server that listens on a predefined url and accepts only one connection.        
        /// </summary>
        /// <remarks>After the request is handled by the <paramref name="responseHandler"/>, the Http Server is shut down.</remarks>
        /// <param name="listenAt">The Url at which the server must listen</param>
        /// <param name="responseHandler">The action that must be performed when a request is received.</param>
        /// <param name="handledSignal">A manual resetevent that is signaled when the request has been handled.</param>
        public static void StartServer(string listenAt, Action<HttpListenerResponse> responseHandler, ManualResetEvent handledSignal)
        {
            HttpListener server = new HttpListener();
            server.Prefixes.Add(listenAt);
            server.Start();

            if (server.IsListening == false)
            {
                throw new InvalidOperationException($"The http server failed to start listening at {listenAt}");
            }

            var request = server.GetContextAsync();

#pragma warning disable 1998
            request.ContinueWith(async t =>
#pragma warning restore 1998
            {
                try
                {
                    responseHandler(t.Result.Response);
                }
                finally
                {
                    t.Result.Response.Close();

                    handledSignal?.Set();

                    await Task.Delay(TimeSpan.FromMilliseconds(50));

                    server.Stop();
                }
            });

        }
    }
}
