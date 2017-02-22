using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Mappings.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Xunit;
using Eu.EDelivery.AS4.Serialization;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="CreateNotifyMessageStep"/>
    /// </summary>
    public class GivenCreateNotifyMessageStepFacts
    {
        private readonly CreateNotifyMessageStep _step;

        protected XmlDocument EnvelopeDocument;

        public GivenCreateNotifyMessageStepFacts()
        {
            MapInitialization.InitializeMapper();

            this._step = new CreateNotifyMessageStep();
        }

        public class GivenValidArguments : GivenCreateNotifyMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidReceiptForMessageInfoAsync()
            {
                // Arrange
                var receipt = new Receipt("message-id");
                InternalMessage internalMessage = CreateDefaultInternalMessage(receipt);
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                var notifyMessage = result.InternalMessage.NotifyMessage;
                Assert.NotNull(notifyMessage);
                Assert.Equal(receipt.MessageId, notifyMessage.MessageInfo.MessageId);
                Assert.Equal(receipt.RefToMessageId, notifyMessage.MessageInfo.RefToMessageId);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidReceiptForStatusInfoAsync()
            {
                // Arrange
                var receipt = new Receipt("message-id");
                InternalMessage internalMessage = CreateDefaultInternalMessage(receipt);
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                var notifyMessage = result.InternalMessage.NotifyMessage;
                Assert.NotNull(notifyMessage);
                Assert.Equal(Status.Delivered, notifyMessage.StatusCode);
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidReceiptForCopiedReceiptAsync()
            {
                // Arrange
                var receipt = new Receipt("message-id");
                InternalMessage internalMessage = CreateDefaultInternalMessage(receipt);
                // Act
                StepResult result = await base._step
                    .ExecuteAsync(internalMessage, CancellationToken.None);
                // Assert
                var notifyMessageEnv = result.InternalMessage.NotifyMessage;
                Assert.NotNull(notifyMessageEnv);

                var notifyMessage = AS4XmlSerializer.Deserialize<NotifyMessage>(System.Text.Encoding.UTF8.GetString(notifyMessageEnv.NotifyMessage));
                Assert.NotNull(notifyMessage);

                XmlElement signalMessage = GetSignalMessageFromDocument(internalMessage.AS4Message.EnvelopeDocument);
                Assert.Equal(notifyMessage.StatusInfo.Any, new[] { signalMessage });
            }
        }

        protected InternalMessage CreateDefaultInternalMessage(Receipt receipt)
        {
            InternalMessage internalMessage = new InternalMessageBuilder()
                .WithSignalMessage(receipt).Build();
            this.EnvelopeDocument = new XmlDocument();
            this.EnvelopeDocument.LoadXml(Properties.Resources.receipt_message);
            internalMessage.AS4Message.EnvelopeDocument = this.EnvelopeDocument;

            return internalMessage;
        }

        protected XmlElement GetSignalMessageFromDocument(XmlDocument document)
        {
            const string xpath = "//*[local-name()='SignalMessage']";
            return (XmlElement)document.SelectSingleNode(xpath);
        }
    }
}