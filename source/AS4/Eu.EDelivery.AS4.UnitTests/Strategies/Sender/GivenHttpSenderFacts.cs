using System;
using System.Net;
using Eu.EDelivery.AS4.Model.Deliver;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Strategies.Sender;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Eu.EDelivery.AS4.UnitTests.Strategies.Method;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using MessageInfo = Eu.EDelivery.AS4.Model.Common.MessageInfo;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// Testing <see cref="HttpSender"/>
    /// </summary>
    public class GivenHttpSenderFacts
    {
        [Property]
        public Property Deliver_Returns_Expected_According_To_StatusCode(HttpStatusCode st)
        {
           return TestReturnsExpectedWithHttpStatusCode(
               st, sut => sut.SendAsync(CreateAnonymousDeliverEnvelope()).GetAwaiter().GetResult());

        }

        [Property]
        public Property Notify_Returns_Returns_Expected_According_To_StatusCode(HttpStatusCode st)
        {
            return TestReturnsExpectedWithHttpStatusCode(
                st, sut => sut.SendAsync(CreateAnonymousNotifyEnvelope()).GetAwaiter().GetResult());
        }

        private Property TestReturnsExpectedWithHttpStatusCode(HttpStatusCode st, Func<HttpSender, SendResult> act)
        {
            // Arrange
            StubHttpClient client = StubHttpClient.ThatReturns(st);
            var sut = new HttpSender(client);
            sut.Configure(new LocationMethod("ignored location"));

            // Act
            SendResult r = act(sut);

            // Assert
            var code = (int)st;
            bool isFatal = r == SendResult.FatalFail;
            bool isRetryable = r == SendResult.RetryableFail;
            bool isSuccess = r == SendResult.Success;

            Assert.True(client.IsCalled, "Stub HTTP client isn't called");
            return isRetryable
                .Equals(code >= 500 || code == 408)
                .Or(isSuccess.Equals(code >= 200 && code <= 206))
                .Or(isFatal.Equals(code >= 400 && code < 500))
                .Classify(isSuccess, "Success with code: " + code)
                .Classify(isRetryable, "Retryable with code: " + code)
                .Classify(isFatal, "Fatal with code: " + code);
        }

        private static DeliverMessageEnvelope CreateAnonymousDeliverEnvelope()
        {
            return new DeliverMessageEnvelope(new MessageInfo(), new byte[0], "text/plain");
        }

        private static NotifyMessageEnvelope CreateAnonymousNotifyEnvelope()
        {
            return new NotifyMessageEnvelope(new AS4.Model.Notify.MessageInfo(), default(Status), new byte[0], "text/plain", null);
        }
    }
}