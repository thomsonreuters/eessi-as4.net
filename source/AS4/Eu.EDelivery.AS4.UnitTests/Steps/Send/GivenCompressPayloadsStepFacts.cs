using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Send;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Send
{
    /// <summary>
    /// Testing <seealso cref="CompressAttachmentsStep" />
    /// </summary>
    public class GivenCompressPayloadsStepFacts
    {
        private readonly CompressAttachmentsStep _step;

        public GivenCompressPayloadsStepFacts()
        {
            this._step = new CompressAttachmentsStep();
        }

        protected Attachment CreateAttachment()
        {
            return new Attachment(id: "attachment-id")
            {
                Content = new MemoryStream()
            };
        }

        /// <summary>
        /// Testing if the Transmitter succeeds
        /// </summary>
        public class GivenCompressPayloadsStepSucceeds
            : GivenCompressPayloadsStepFacts
        {
            [Fact]
            public void ThenTransmitMessageSucceeds()
            {
                // Arrange
                AS4Message message = new AS4MessageBuilder()
                    .WithAttachment(base.CreateAttachment())
                    .Build();
                var internalMessage = new InternalMessage(message);
                // Act
                Task<StepResult> result = this._step.ExecuteAsync(
                    internalMessage,
                    CancellationToken.None);
                // Assert
                Assert.NotNull(result);
            }
        }
    }
}