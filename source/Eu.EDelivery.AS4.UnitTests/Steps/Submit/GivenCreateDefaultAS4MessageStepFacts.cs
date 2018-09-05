using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Submit;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Model.PMode;
using FsCheck;
using FsCheck.Xunit;
using Xunit;
using MessageProperty = Eu.EDelivery.AS4.Model.Core.MessageProperty;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Submit
{
    /// <summary>
    /// Testing <see cref="CreateDefaultAS4MessageStep"/>
    /// </summary>
    public class GivenCreateDefaultAS4MessageStepFacts : PseudoConfig
    {
        [Property]
        public void CreatesExpectedMessageFromPMode(NonEmptyString id)
        {
            // Arrange
            AS4Message message = AS4Message.Empty;
            var attachment = new Attachment(id.Get);
            message.AddAttachment(attachment);

            var sut = new CreateDefaultAS4MessageStep(this);

            // Act
            StepResult result = sut.ExecuteAsync(
                new MessagingContext(message, MessagingContextMode.Unknown)).GetAwaiter().GetResult();

            // Assert
            Assert.Collection(
                result.MessagingContext.AS4Message.UserMessages, 
                m => Assert.Contains(attachment.Id, m.PayloadInfo.First().Href));
        }

        /// <summary>
        /// Retrieve the PMode from the Global Settings
        /// </summary>
        /// <param name="id"></param>
        /// <exception cref="Exception"></exception>
        /// <returns></returns>
        public override SendingProcessingMode GetSendingPMode(string id)
        {
            SendingProcessingMode pmode = ValidSendingPModeFactory.Create();
            pmode.MessagePackaging.MessageProperties = new List<AS4.Model.PMode.MessageProperty>();

            return pmode;
        }
    }
}
