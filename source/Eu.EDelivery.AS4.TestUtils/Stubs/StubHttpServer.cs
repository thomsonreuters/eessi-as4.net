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
        /// <remarks>After the request is handled by the <paramref name="responseHandler" />, the Http Server is shut down.</remarks>
        /// <param name="listenAt">The Url at which the server must listen</param>
        /// <param name="responseHandler">The action that must be performed when a request is received.</param>
        /// <param name="onStop">A manual resetevent that is signaled when the request has been handled.</param>
        public static void StartServer(
            string listenAt,
            Action<HttpListenerResponse> responseHandler,
            ManualResetEvent onStop)
        {
            var server = new HttpListener();
            server.Prefixes.Add(listenAt);
            server.Start();

            if (server.IsListening == false)
            {
                throw new InvalidOperationException($"The http server failed to start listening at {listenAt}");
            }

            Task<HttpListenerContext> request = server.GetContextAsync();
            Console.WriteLine($@"Stub HTTP Server: received request at: {listenAt}");

#pragma warning disable 1998
            request.ContinueWith(
                async t =>
#pragma warning restore 1998
                {
                    try
                    {
                        responseHandler(t.Result.Response);
                        Console.WriteLine(
                            $@"Stub HTTP Server: respond to request, StatusCode {t.Result.Response.StatusCode}");
                    }
                    finally
                    {
                        t.Result.Response.Close();

                        onStop?.Set();

                        await Task.Delay(TimeSpan.FromMilliseconds(50));

                        server.Stop();
                    }
                });
        }

        /// <summary>
        /// Starts a Http Server that listens on a predefined url and accepts only one connection.
        /// </summary>
        /// <remarks>After the request is handled by the <paramref name="responseHandler" />, the Http Server is shut down.</remarks>
        /// <param name="listenAt">The Url at which the server must listen</param>
        /// <param name="responseHandler">The action that must be performed when a request is received.</param>
        /// <param name="onStop">A manual resetevent that is signaled when the request has been handled.</param>
        public static void StartServerLifetime(
            string listenAt,
            Func<HttpListenerResponse, ServerLifetime> responseHandler,
            ManualResetEvent onStop)
        {
            var server = new HttpListener();
            server.Prefixes.Add(listenAt);
            server.Start();

            while (server.IsListening)
            {
                HttpListenerContext context = server.GetContext();

                ServerLifetime result = responseHandler(context.Response);
                Console.WriteLine(
                    $@"Stub HTTP Server: respond to request, StatusCode {context.Response.StatusCode}");

                context.Response.Close();

                if (result == ServerLifetime.Stop)
                {
                    onStop?.Set();
                    server.Stop();
                }
            }
        }

        /// <summary>
        /// Starts a Http Server that listens on a predefined url and accepts only one connection.
        /// </summary>
        /// <param name="url">The Url at which the server must listen</param>
        /// <param name="secondAttempt">The second attempt status code.</param>
        /// <param name="onSecondAttempt">A manual resetevent that is signaled when the request has been handled.</param>
        public static void SimulateFailureOnFirstAttempt(
            string url,
            HttpStatusCode secondAttempt,
            ManualResetEvent onSecondAttempt)
        {
            Console.WriteLine(@"Second HTTP Status Code: " + secondAttempt);
            var first = true;
            StartServerLifetime(
                url,
                r =>
                {
                    r.StatusCode = (int)secondAttempt;

                    if (first)
                    {
                        Console.WriteLine(@"First attempt: first=" + first);

                        first = false;
                        r.StatusCode = 500;
                        r.OutputStream.WriteByte(0);
                        r.OutputStream.Dispose();
                        return ServerLifetime.Continue;
                    }

                    Console.WriteLine(@"Second attempt: first=" + first);
                    Console.WriteLine(@"Second attempt: HTTP Status Code=" + r.StatusCode);
                    r.OutputStream.Dispose();
                    return ServerLifetime.Stop;
                },
                onSecondAttempt);
        }
    }
}

public enum ServerLifetime
{
    Continue,
    Stop
}