using System;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Extensions;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PiggyBackingFacts : ComponentTestTemplate
    {
        private const string SubmitUrl = "http://localhost:7070/msh/";

        private readonly AS4Component _consoleHost;
        private readonly WindowsServiceFixture _windowsService;

        /// <summary>
        /// Initializes a new instance of the <see cref="PiggyBackingFacts"/> class.
        /// </summary>
        public PiggyBackingFacts()
        {
            OverrideSettings("piggyback_console_settings.xml");
            _consoleHost = AS4Component.Start(Environment.CurrentDirectory);

            _windowsService = new WindowsServiceFixture();
        }

        [Fact]
        public async Task PiggyBack_Receipt_On_Next_PullRequest_Result_In_Acked_UserMessage()
        {
            // Arrange
            OverrideServiceSettings("piggyback_service_settings.xml");

            _windowsService.EnsureServiceIsStarted();

            // Act
            await SubmitMessageToSubmitAgent(pullsendagent_piggyback);

            // Assert
            var spy = new DatabaseSpy(_consoleHost.GetConfiguration());
            InMessage receipt = await PollUntilPresent(
                () => spy.GetInMessageFor(
                    m => m.EbmsMessageType == MessageType.Receipt),
                timeout: TimeSpan.FromSeconds(30));

            await PollUntilPresent(
                () => spy.GetOutMessageFor(
                    m => m.EbmsMessageType == MessageType.UserMessage
                         && m.EbmsMessageId == receipt.EbmsRefToMessageId
                         && m.Status.ToEnum(OutStatus.NotApplicable) == OutStatus.Ack),
                timeout: TimeSpan.FromSeconds(10));
        }

        private static async Task SubmitMessageToSubmitAgent(string submitMessage)
        {
            await StubSender.SendRequest(SubmitUrl, Encoding.UTF8.GetBytes(submitMessage), "application/soap+xml");
            
            // Wait a bit so that we're sure that the processing agent has picked up the message.
            await Task.Delay(3000);
        }

        protected override void Disposing(bool isDisposing)
        {
            _consoleHost.Dispose();
            _windowsService.Dispose();
        }
    }
}
