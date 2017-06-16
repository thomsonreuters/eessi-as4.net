using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Builders.Core;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="CreateDefaultAS4MessageStep"/>
    /// </summary>
    public class GivenCreateDefaultAS4MessageStepFacts : PseudoConfig
    {
        [Fact]
        public async Task CreatesExpectedMessageFromPMode()
        {
            // Arrange
            var sut = new CreateDefaultAS4MessageStep(this);
            sut.Configure(properties: null);
            const string attachmentId = "attachment id";

            // Act
            StepResult result = await sut.ExecuteAsync(AttachmentWithoutUserMessage(attachmentId), CancellationToken.None);

            // Assert
            AS4Message as4Message = result.MessagingContext.AS4Message;
            Assert.Equal(1, as4Message.UserMessages.Count);
            Assert.Contains(attachmentId, as4Message.PrimaryUserMessage.PayloadInfo.First().Href);
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public override SendingProcessingMode GetSendingPMode(string id)
        {
            SendingProcessingMode pmode = new ValidSendingPModeFactory().Create();
            pmode.MessagePackaging.MessageProperties = new List<MessageProperty>();

            return pmode;
        }

        private static MessagingContext AttachmentWithoutUserMessage(string atttachmentId)
        {
            AS4Message message = AS4Message.Empty;
            message.AddAttachment(new Attachment(atttachmentId));

            return new MessagingContext(message, MessagingContextMode.Unknown);
        }
    }
}
