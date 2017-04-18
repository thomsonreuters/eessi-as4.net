using System.Net;
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
            var httpSender = new HttpSender();
            string sharedUrl = UniqueHost.Create();
            httpSender.Configure(new LocationMethod(sharedUrl));

            using (SpyHttpServer spyServer = SpyHttpServer.CreateWith(sharedUrl, HttpStatusCode.Accepted))
            {
                // Act
                httpSender.Send(CreateAnonymousDeliverEnvelope());

                // Assert
                Assert.True(spyServer.IsCalled);
            }
        }

        private static DeliverMessageEnvelope CreateAnonymousDeliverEnvelope()
        {
            return new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "text/plain");
        }

        [Fact]
        public void ThenUploadPaloadSucceeds_IfNotifyMessage()
        {
            // Arrange
            var sut = new HttpSender();
            string sharedUrl = UniqueHost.Create();
            sut.Configure(new LocationMethod(sharedUrl));

            using (SpyHttpServer spyServer = SpyHttpServer.CreateWith(sharedUrl, HttpStatusCode.OK))
            {
                // Act
                sut.Send(CreateAnonymousNotifyEnvelope());

                // Assert
                Assert.True(spyServer.IsCalled);
            }
        }

        private static NotifyMessageEnvelope CreateAnonymousNotifyEnvelope()
        {
            return new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), default(Status), new byte[0], "text/plain");
        }
     }
}