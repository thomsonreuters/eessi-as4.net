using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Model.PMode;
using Eu.EDelivery.AS4.Steps;
using Eu.EDelivery.AS4.Steps.Receive;
using Eu.EDelivery.AS4.UnitTests.Common;
using Eu.EDelivery.AS4.UnitTests.Repositories;

namespace Eu.EDelivery.AS4.UnitTests.Steps.Receive
{
    public class GivenSendSignalMessageStepFacts : GivenDatastoreStepFacts
    {
        private readonly InMemoryMessageBodyStore _messageBodyStore = new InMemoryMessageBodyStore();

        /// <summary>
        /// Initializes a new instance of the <see cref="GivenSendSignalMessageStepFacts"/> class.
        /// </summary>
        public GivenSendSignalMessageStepFacts()
        {
            Step = new SendAS4SignalMessageStep(GetDataStoreContext, _messageBodyStore);
        }

        /// <summary>
        /// Gets a <see cref="IStep" /> implementation to exercise the datastore.
        /// </summary>
        protected override IStep Step { get; }

        protected override void Disposing()
        {
            _messageBodyStore.Dispose();
            base.Disposing();
        }

        [CustomProperty]
        public bool Returns_Empty_Soap_For_Signals_If_ReplyPattern_Is_Callback(SignalMessage signal)
        {
            // Arrange
            var receivePMode =
                new ReceivingProcessingMode {ReplyHandling = {ReplyPattern = ReplyPattern.Callback}};

            MessagingContext context = ContextWithSignal(signal, receivePMode);
            context.SendingPMode = new SendingProcessingMode {Id = "sending-pmode"};

            // Act
            AS4Message result = ExerciseSendSignal(context).GetAwaiter().GetResult();

            // Assert
            return result.IsEmpty;
        }

        [CustomProperty]
        public bool ReturnsSameResultForReceipt_IfReplyPatternIsResponse(SignalMessage signal)
        {
            // Arrange
            var pmode = new ReceivingProcessingMode {ReplyHandling = {ReplyPattern = ReplyPattern.Response}};
            MessagingContext context = ContextWithSignal(signal, pmode);

            // Act
            AS4Message actual = ExerciseSendSignal(context).GetAwaiter().GetResult();

            // Assert
            AS4Message expected = context.AS4Message;
            return expected.Equals(actual);
        }

        private static MessagingContext ContextWithSignal(SignalMessage signal, ReceivingProcessingMode pmode)
        {
            return new MessagingContext(AS4Message.Create(signal), MessagingContextMode.Receive)
            {
                ReceivingPMode = pmode
            };
        }

        private async Task<AS4Message> ExerciseSendSignal(MessagingContext context)
        {
            StepResult result = await Step.ExecuteAsync(context);

            return result.MessagingContext.AS4Message;
        }
    }
}
