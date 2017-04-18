using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Receivers;
using Eu.EDelivery.AS4.UnitTests.Http;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Receivers
{
    /// <summary>
    /// Testing <see cref="HttpReceiver" />
    /// </summary>
    public class GivenHttpReceiverFacts
    {
        private static readonly HttpClient HttpClient = new HttpClient();

        [Fact]
        public async Task ReceivesRequest_IfFakeServerResponse()
        {
            // Arrange
            var waitHandle = new ManualResetEvent(initialState: false);
            var sut = new HttpReceiver();

            string sharedUrl = UniqueHost.Create() + "/";
            ConfigureHostUrl(sut, sharedUrl);
            StartReceiving(sut, () => waitHandle.Set());

            // Act
            await HttpClient.PostAsync(sharedUrl, new StringContent(string.Empty));

            // Assert
            sut.StopReceiving();
            Assert.True(waitHandle.WaitOne(timeout: TimeSpan.FromSeconds(5)));
        }

        private static void ConfigureHostUrl(IReceiver receiver, string sharedUrl)
        {
            receiver.Configure(new[] { new Setting("Url", sharedUrl) });
        }

        private static void StartReceiving(IReceiver receiver, Action testAction)
        {
            Task.Run(
                () => receiver.StartReceiving(
                    (message, token) =>
                    {
                        testAction();
                        return Task.FromResult(new InternalMessage());
                    },
                    CancellationToken.None));
        }
    }
}