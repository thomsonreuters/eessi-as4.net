using System;
using System.Collections.Generic;
using Eu.EDelivery.AS4.Model.Common;
using Eu.EDelivery.AS4.Strategies.Retriever;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Strategies.Retriever
{
    /// <summary>
    /// Testing <see cref="PayloadRetrieverProvider"/>
    /// </summary>
    public class GivenPayloadRetrieverProviderFacts
    {
        public static IEnumerable<object[]> PayloadRetrievers
        {
            get
            {
                yield return new object[] { FilePayloadRetriever.Key, typeof(FilePayloadRetriever) };
                yield return new object[] { FtpPayloadRetriever.Key, typeof(FtpPayloadRetriever) };
                yield return new object[] { HttpPayloadRetriever.Key, typeof(HttpPayloadRetriever) };
                yield return new object[] { TempFilePayloadRetriever.Key, typeof(TempFilePayloadRetriever) };
            }
        }


        [Theory]
        [MemberData(nameof(PayloadRetrievers))]
        public void CanGetKnownPayloadRetriever(string key, Type expectedRetriever)
        {
            // Arrange
            var payload = new Payload(location: $"{key}{Guid.NewGuid()}");

            // Act
            var actualRetriever = PayloadRetrieverProvider.Instance.Get(payload);

            // Assert
            Assert.IsType(expectedRetriever, actualRetriever);
        }


        [Fact]
        public void FailsToGetRetriever_IfNoRetrieverIsRegisteredForType()
        {
            // Arrange
            var payload = new Payload(location: "unknownthing");

            // Act / Assert
            Assert.ThrowsAny<Exception>(() => PayloadRetrieverProvider.Instance.Get(payload));
        }
    }
}
