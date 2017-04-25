using System.Net;
using Eu.EDelivery.AS4.Http;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Http;
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
        public void ThenUploadPayloadSucceeds_IfDeliverMessage()
        {
            // Arrange
            StubHttpClient spyClient = StubHttpClient.ThatReturns(HttpStatusCode.OK);
            var httpSender = new HttpSender(spyClient);
            httpSender.Configure(new LocationMethod("ignored location"));

            // Act
            httpSender.Send(CreateAnonymousDeliverEnvelope());

            // Assert
            Assert.True(spyClient.IsCalled);
        }

        private static DeliverMessageEnvelope CreateAnonymousDeliverEnvelope()
        {
            return new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "text/plain");
        }

        [Fact]
        public void ThenUploadPaloadSucceeds_IfNotifyMessage()
        {
            // Arrange
            StubHttpClient spyClient = StubHttpClient.ThatReturns(HttpStatusCode.OK);
            var sut = new HttpSender(spyClient);
            sut.Configure(new LocationMethod("ignored location"));

            // Act
            sut.Send(CreateAnonymousNotifyEnvelope());

            // Assert
            Assert.True(spyClient.IsCalled);
        }

        private static NotifyMessageEnvelope CreateAnonymousNotifyEnvelope()
        {
            return new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), default(Status), new byte[0], "text/plain");
        }
     }
}