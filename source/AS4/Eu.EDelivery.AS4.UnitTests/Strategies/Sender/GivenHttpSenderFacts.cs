using System.Net;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using Xunit;
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// Testing <see cref="HttpSender"/>
    /// </summary>
    public class GivenHttpSenderFacts
    {
        [Fact]
        public async void ThenUploadPayloadSucceeds_IfDeliverMessage()
        {
            // Arrange
            StubHttpClient spyClient = StubHttpClient.ThatReturns(HttpStatusCode.OK);
            var httpSender = new HttpSender(spyClient);
            httpSender.Configure(new LocationMethod("ignored location"));

            // Act
            await httpSender.SendAsync(CreateAnonymousDeliverEnvelope());

            // Assert
            Assert.True(spyClient.IsCalled);
        }

        private static DeliverMessageEnvelope CreateAnonymousDeliverEnvelope()
        {
            return new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "text/plain");
        }

        [Fact]
        public async void ThenUploadPayloadSucceeds_IfNotifyMessage()
        {
            // Arrange
            StubHttpClient spyClient = StubHttpClient.ThatReturns(HttpStatusCode.OK);
            var sut = new HttpSender(spyClient);
            sut.Configure(new LocationMethod("ignored location"));

            // Act
            await sut.SendAsync(CreateAnonymousNotifyEnvelope());

            // Assert
            Assert.True(spyClient.IsCalled);
        }

        private static NotifyMessageEnvelope CreateAnonymousNotifyEnvelope()
        {
            return new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), default(Status), new byte[0], "text/plain", null);
        }
    }
}