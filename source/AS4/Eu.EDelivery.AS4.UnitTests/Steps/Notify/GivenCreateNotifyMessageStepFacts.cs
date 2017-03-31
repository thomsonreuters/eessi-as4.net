using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.Notify;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Notify;
using Eu.EDelivery.AS4.UnitTests.Builders.Core;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Notify
{
    /// <summary>
    /// Testing <see cref="CreateNotifyMessageStep" />
    /// </summary>
    public class GivenCreateNotifyMessageStepFacts
    {
        private readonly CreateNotifyMessageStep _step;

        private XmlDocument _envelopeDocument;

        public GivenCreateNotifyMessageStepFacts()
        {
            _step = new CreateNotifyMessageStep();
        }

        public class GivenValidArguments : GivenCreateNotifyMessageStepFacts
        {
            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidReceiptForCopiedReceiptAsync()
            {
                // Arrange
                var receipt = new Receipt("message-id");
                InternalMessage internalMessage = CreateDefaultInternalMessage(receipt);

                // Act
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                NotifyMessageEnvelope notifyMessageEnv = result.InternalMessage.NotifyMessage;
                Assert.NotNull(notifyMessageEnv);

                var notifyMessage =
                    AS4XmlSerializer.FromString<NotifyMessage>(Encoding.UTF8.GetString(notifyMessageEnv.NotifyMessage));
                Assert.NotNull(notifyMessage);

                XmlElement signalMessage = GetSignalMessageFromDocument(internalMessage.AS4Message.EnvelopeDocument);
                Assert.Equal(notifyMessage.StatusInfo.Any, new[] {signalMessage});
            }

            [Fact]
            public async Task ThenExecuteStepSucceedsWithValidReceiptForMessageInfoAsync()
            {
                // Arrange
                var receipt = new Receipt("message-id");
                InternalMessage internalMessage = CreateDefaultInternalMessage(receipt);

                // Act
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                NotifyMessageEnvelope notifyMessage = result.InternalMessage.NotifyMessage;
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
                StepResult result = await _step.ExecuteAsync(internalMessage, CancellationToken.None);

                // Assert
                NotifyMessageEnvelope notifyMessage = result.InternalMessage.NotifyMessage;
                Assert.NotNull(notifyMessage);
                Assert.Equal(Status.Delivered, notifyMessage.StatusCode);
            }
        }

        protected InternalMessage CreateDefaultInternalMessage(Receipt receipt)
        {
            InternalMessage internalMessage = new InternalMessageBuilder().WithSignalMessage(receipt).Build();
            _envelopeDocument = new XmlDocument();
            _envelopeDocument.LoadXml(Properties.Resources.receipt_message);
            internalMessage.AS4Message.EnvelopeDocument = _envelopeDocument;

            return internalMessage;
        }

        protected XmlElement GetSignalMessageFromDocument(XmlDocument document)
        {
            const string xpath = "//*[local-name()='SignalMessage']";
            return (XmlElement)document.SelectSingleNode(xpath);
        }
    }
}