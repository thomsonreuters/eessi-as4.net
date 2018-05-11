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
        [Fact]
        public async Task Agent_Returns_500_StatusCode_When_ReceivingPMode_Cannot_Be_Found()
        {
            // Arrange
            const string settingsFileName = "staticreceiveagent_http_settings.xml";
            OverrideTransformerReceivingPModeSetting(settingsFileName, pmodeId: "non-existing-pmode-id");

            Settings receiveSettings = OverrideSettings(settingsFileName);
            string url = receiveSettings.Agents
                .ReceiveAgents.First().Receiver
                .Setting.First(s => s.Key == "Url").Value;

            var msh = AS4Component.Start(Environment.CurrentDirectory);

            // Act
            HttpResponseMessage response = await StubSender.SendAS4Message(url, AS4Message.Empty);

            // Assert
            Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
            
            // TearDown
            msh.Dispose();
        }

        private void OverrideTransformerReceivingPModeSetting(string settingsFileName, string pmodeId)
        {
            string settingsFilePath = Path.Combine(ComponentTestSettingsPath, settingsFileName);
            var settings = AS4XmlSerializer.FromString<Settings>(File.ReadAllText(settingsFilePath));

            settings.Agents.ReceiveAgents.First()
                    .Transformer
                    .Setting.First(s => s.Key == "ReceivingPMode").Value = pmodeId;

            File.WriteAllText(settingsFilePath, AS4XmlSerializer.ToString(settings));
        }
    }
}
