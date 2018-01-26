using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;
using static Eu.EDelivery.AS4.ComponentTests.Properties.Resources;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class StaticSubmitAgentFacts : ComponentTestTemplate
    {
        // It would be nice if this could be extracted from the configuration.
        private const string HttpSubmitAgentUrl = "http://localhost:7070/msh/";

        private readonly AS4Component _msh;
        private readonly DatabaseSpy _databaseSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="StaticSubmitAgentFacts"/> class.
        /// </summary>
        public StaticSubmitAgentFacts()
        {
            OverrideSettings("staticsubmitagent_settings.xml");
            _msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_msh.GetConfiguration());
        }

        [Fact]
        public async Task ThenAgentCreatesSubmitMessageFromPayload()
        {
            // Act
            using (HttpResponseMessage response = await StubSender.SendRequest(HttpSubmitAgentUrl, payload, "image/jpg"))
            {
                Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            }

            // Assert
            Assert.NotNull(_databaseSpy.GetOutMessageFor(m => true));
        }

        protected override void Disposing(bool isDisposing)
        {
            _msh.Dispose();
        }
    }
}
