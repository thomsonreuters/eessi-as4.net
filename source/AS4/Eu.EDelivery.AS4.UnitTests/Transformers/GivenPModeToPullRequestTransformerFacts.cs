using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Transformers;
using Eu.EDelivery.AS4.UnitTests.Model.Internal;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Moq;
using Xunit;
using Xunit.Extensions;

namespace Eu.EDelivery.AS4.UnitTests.Transformers
{
    /// <summary>
    /// Testing <see cref="PModeToPullRequestTransformer" />
    /// </summary>
    public class GivenPModeToPullRequestTransformerFacts
    {
        /// <summary>
        /// Gets the received message source.
        /// </summary>
        /// <value>
        /// The received message source.
        /// </value>
        public static IEnumerable<object[]> ReceivedMessageSource
        {
            get
            {
                yield return new object[] { new ReceivedMessage(requestStream: null) };

                SendingProcessingMode invalidSendingPMode = new ValidSendingPModeFactory().Create("my id");
                invalidSendingPMode.MepBinding = MessageExchangePatternBinding.Pull;
                invalidSendingPMode.PushConfiguration = new PushConfiguration();
                invalidSendingPMode.PullConfiguration = null;

                yield return new object[] { new ReceivedMessage(AS4XmlSerializer.ToStream(invalidSendingPMode)) };
                yield return new object[] { new SaboteurReceivedMessage() };
            }
        }

        [Theory]
        [MemberData(nameof(ReceivedMessageSource))]
        public async Task FailsWithNoPullConfigurationSection(ReceivedMessage receivedMessage)
        {
            // Arrange
            var transformer = new PModeToPullRequestTransformer();

            // Act
            InternalMessage message = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            Assert.NotNull(message.Exception);
            Assert.NotEmpty(message.Exception.Message);
        }

        [Fact]
        public async Task SucceedsWithAValidPullConfiguration()
        {
            // Arrange
            const string expectedMpc = "expected-mpc";
            SendingProcessingMode expectedSendingPMode = CreateAnonymousSendingPModeWith(expectedMpc);
            var receivedMessage = new ReceivedMessage(AS4XmlSerializer.ToStream(expectedSendingPMode));

            var transformer = new PModeToPullRequestTransformer();

            // Act
            InternalMessage message = await transformer.TransformAsync(receivedMessage, CancellationToken.None);

            // Assert
            var actualSignalMessage = message.AS4Message.PrimarySignalMessage as PullRequest;
            Assert.Equal(expectedMpc, actualSignalMessage?.Mpc);
            Assert.Equal(expectedSendingPMode.Id, message.AS4Message.SendingPMode.Id);
        }

        private static SendingProcessingMode CreateAnonymousSendingPModeWith(string expectedMpc)
        {
            SendingProcessingMode expectedSendingPMode = new ValidSendingPModeFactory().Create("expected-id");
            expectedSendingPMode.PullConfiguration.Mpc = expectedMpc;

            return expectedSendingPMode;
        }
    }
}