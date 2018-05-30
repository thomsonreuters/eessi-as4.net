using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class StaticReceiveAgentFacts : ComponentTestTemplate
    {
        private const string StaticReceiveSettings = "staticreceiveagent_http_settings.xml";

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticReceiveAgentFacts"/> class.
        /// </summary>
        public StaticReceiveAgentFacts()
        {
            OverrideTransformerReceivingPModeSetting(
                StaticReceiveSettings, 
                "ComponentTest_ReceiveAgent_Sample1");
        }

        [Fact(Skip="Not yet implemented")]
        public async Task Agent_Returns_BadRequest_When_Receiving_SignalMessage()
        {
            await TestStaticReceive(
                StaticReceiveSettings,
                async url =>
                {
                    // Arrange
                    AS4Message receipt = AS4Message.Create(
                        new Receipt($"ebms-id-receipt-{Guid.NewGuid()}"));

                    // Act
                    HttpResponseMessage response =
                        await StubSender.SendAS4Message(url, receipt);

                    // Assert
                    Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                });
        }

        [Fact]
        public async Task Agent_Returns_500_StatusCode_When_ReceivingPMode_Cannot_Be_Found()
        {
            OverrideTransformerReceivingPModeSetting(
                StaticReceiveSettings, 
                pmodeId: "non-existing-pmode-id");

            await TestStaticReceive(
                StaticReceiveSettings,
                async url =>
                {
                    // Act
                    HttpResponseMessage response =
                        await StubSender.SendAS4Message(url, AS4Message.Empty);

                    // Assert
                    Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
                });

        }

        private async Task TestStaticReceive(string settingsFileName, Func<string, Task> act)
        {
            AS4Component msh = null;
            try
            {
                Settings receiveSettings = OverrideSettings(settingsFileName);
                string url = receiveSettings
                    .Agents
                    .ReceiveAgents.First().Receiver
                    .Setting.First(s => s.Key == "Url").Value;

                msh = AS4Component.Start(Environment.CurrentDirectory);

                await act(url);
            }
            finally
            {
                // TearDown
                msh?.Dispose();
            }
        }

        private void OverrideTransformerReceivingPModeSetting(string settingsFileName, string pmodeId)
        {
            string settingsFilePath = Path.Combine(ComponentTestSettingsPath, settingsFileName);
            var settings = AS4XmlSerializer.FromString<Settings>(File.ReadAllText(settingsFilePath));

            settings.Agents
                    .ReceiveAgents.First()
                    .Transformer
                    .Setting.First(s => s.Key == "ReceivingPMode")
                    .Value = pmodeId;

            File.WriteAllText(settingsFilePath, AS4XmlSerializer.ToString(settings));
        }
    }
}
