using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Repositories;
using Eu.EDelivery.AS4.Serialization;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class OutboundProcessingAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="OutboundProcessingAgentFacts"/> class.
        /// </summary>
        public OutboundProcessingAgentFacts()
        {
            OverrideSettings("outboundprocessingagent_settings.xml");
            _msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task ProcessedMessageIsSetToSentWithoutAlteringMultihopInformation()
        {
            // Arrange
            var multihopPMode = new SendingProcessingMode { MessagePackaging = { IsMultiHop = true } };
            var multihopMessage = CreateAS4Message(multihopPMode);

            var datastoreSpy = new DatabaseSpy(_msh.GetConfiguration());
            OutMessage tobeProcessedEntry = CreateToBeProcessedOutMessage(multihopPMode, multihopMessage);

            // Act
            datastoreSpy.InsertOutMessage(tobeProcessedEntry);

            // Assert
            Thread.Sleep(TimeSpan.FromSeconds(4));

            OutMessage processedEntry = datastoreSpy.GetOutMessageFor(
                m => m.EbmsMessageId == multihopMessage.GetPrimaryMessageId());

            Assert.Equal(Operation.ToBeSent, OperationUtils.Parse(processedEntry.Operation));
            Assert.False(processedEntry.Intermediary);

            AS4Message processedMessage = 
                await DeserializeOutMessageBody(Registry.Instance.MessageBodyStore, processedEntry);

            Assert.True(processedMessage.IsMultiHopMessage);
        }

        private static AS4Message CreateAS4Message(SendingProcessingMode pmode)
        {
            return AS4Message.Create(
                pmode: pmode,
                message: new UserMessage("test-" + Guid.NewGuid())
                {
                    Sender = new Party("sender-role", new PartyId("sender-id")),
                    Receiver = new Party("receiver-role", new PartyId("receiver-id"))
                });
        }

        private OutMessage CreateToBeProcessedOutMessage(IPMode pmode, AS4Message msg)
        {
            IAS4MessageBodyStore bodyStore = Registry.Instance.MessageBodyStore;
            string location = _msh.GetConfiguration().OutMessageStoreLocation;

            var tobeProcessedEntry = new OutMessage(msg.GetPrimaryMessageId())
            {
                ContentType = msg.ContentType,
                MessageLocation = bodyStore.SaveAS4Message(location, msg)
            };
            tobeProcessedEntry.SetOperation(Operation.ToBeProcessed);
            tobeProcessedEntry.SetPModeInformation(pmode);

            return tobeProcessedEntry;
        }

        private static async Task<AS4Message> DeserializeOutMessageBody(
            IAS4MessageBodyStore bodyStore, 
            OutMessage processedEntry)
        {
            using (Stream output = await bodyStore.LoadMessageBodyAsync(processedEntry.MessageLocation))
            {
                return await SerializerProvider.Default
                    .Get(processedEntry.ContentType)
                    .DeserializeAsync(output, processedEntry.ContentType, CancellationToken.None);
            }
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}
