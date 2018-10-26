using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Strategies.Sender;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Sender
{
    /// <summary>
    /// Testing <see cref="DeliverSenderProvider"/>
    /// </summary>
    public class GivenDeliverSenderProviderFacts
    {
        public static IEnumerable<object[]> DeliverSenders
        {
            get
            {
                yield return new object[] { "FILE", typeof(FileSender) };
                yield return new object[] { "HTTP", typeof(HttpSender) };
            }
        }

        [Theory]
        [MemberData(nameof(DeliverSenders))]
        public void DeliverSenderProviderGetsSender(
            string expectedKey,
            Type expectedSenderType)
        {
            // Arrange
            var provider = DeliverSenderProvider.Instance;

            // Act
            IDeliverSender actualSender = provider.GetDeliverSender(expectedKey);

            // Assert
            Assert.IsType<ReliableSender>(actualSender);
            Assert.IsType(expectedSenderType, ((ReliableSender)actualSender).InnerDeliverSender);
        }

        [Fact]
        public void FailsToGetSender_IfSenderIsNotRegistered()
        {
            // Arrange
            var sut = DeliverSenderProvider.Instance;

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => sut.GetDeliverSender("not exsising key"));
        }
    }
}
