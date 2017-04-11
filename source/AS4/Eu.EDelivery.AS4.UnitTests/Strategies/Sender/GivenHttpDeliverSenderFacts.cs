using System.Collections.Generic;
using System.Net;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.UnitTests.Http;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    public class GivenHttpDeliverSenderFacts
    {
        private readonly string _sharedUrl = UniqueHost.Create();

        [Fact]
        public void ThenUplaodPayloadSucceeds()
        {
            var httpSender = new HttpDeliverySender();
            httpSender.Configure(new LocationMethod(_sharedUrl));

            using (SpyHttpServer spyServer = SpyHttpServer.CreateWith(_sharedUrl, HttpStatusCode.Accepted))
            {
                httpSender.Send(CreateAnonymousDeliverEnvelope());

                Assert.True(spyServer.IsCalled);
            }
        }

        private static DeliverMessageEnvelope CreateAnonymousDeliverEnvelope()
        {
            return new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "text/plain");
        }
    }
}