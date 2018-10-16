using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Entities;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Serialization;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class StaticSubmitAgentFacts : ComponentTestTemplate
    {
        // It would be nice if this could be extracted from the configuration.
        private const string HttpSubmitAgentUrl = "http://localhost:7070/msh/";

        [Fact]
        public async Task ThenAgentCreatesSubmitMessageFromPayload()
        {
            await SendPayloadToStaticSubmit(
                settingsFile: "staticsubmitagent_settings.xml",
                assertion: databaseSpy =>
                {
                    Assert.NotNull(databaseSpy.GetOutMessageFor(m => m.PModeId == "componentsubmittest-pmode"));
                    return Task.CompletedTask;
                });
        }

        [Fact]
        public async Task ThenAgentCreatesMultihopMessageAndDontSetsItToIntermediary()
        {
            await SendPayloadToStaticSubmit(
                settingsFile: "staticsubmitagent_multihop_settings.xml",
                assertion: async databaseSpy =>
                {
                    OutMessage multihopMessage = databaseSpy.GetOutMessageFor(m => m.PModeId == "staticsubmit-multihop-pmode");
                    Assert.False(multihopMessage.Intermediary);

                    Stream messageBody = await Registry.Instance
                        .MessageBodyStore
                        .LoadMessageBodyAsync(multihopMessage.MessageLocation);

                    AS4Message savedMessage = await SerializerProvider.Default
                        .Get(multihopMessage.ContentType)
                        .DeserializeAsync(messageBody, multihopMessage.ContentType);

                    Assert.True(savedMessage.IsMultiHopMessage);
                });
        }

        private async Task SendPayloadToStaticSubmit(string settingsFile, Func<DatabaseSpy, Task> assertion)
        {
            // Arrange
            OverrideSettings(settingsFile);
            var msh = AS4Component.Start(Environment.CurrentDirectory);
            var databaseSpy = new DatabaseSpy(msh.GetConfiguration());

            // Act
            await SubmitAnonymousPayload();

            // Assert
            await assertion(databaseSpy);

            // TearDown
            msh.Dispose();
        }

        private static async Task SubmitAnonymousPayload()
        {
            using (HttpResponseMessage response = await StubSender.SendRequest(HttpSubmitAgentUrl, payload, "image/jpg"))
            {
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }
        }
    }
}
