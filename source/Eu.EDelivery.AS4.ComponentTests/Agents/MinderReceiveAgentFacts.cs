using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class MinderReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly string _receiveUrl;
        private readonly AS4Component _as4Msh;

        /// <summary>
        /// Initializes a new instance of the <see cref="MinderReceiveAgentFacts"/> class.
        /// </summary>
        public MinderReceiveAgentFacts()
        {
            Settings settings = OverrideSettings("c3_minderreceiveagent_settings.xml");
            _receiveUrl = settings.Agents.MinderTestAgents.First(a => a.Enabled).Url;

            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public async Task Agent_Correctly_Handles_Message_Which_Bypasses_Interceptor()
        {
            const string contentType =
                "multipart/related; boundary=\"=-GpNI15tekzCG48QM0jucBg==\"; type=\"application/soap+xml\"";

            HttpResponseMessage response = await StubSender.SendRequest(
                _receiveUrl, 
                Properties.Resources.c3_minderreceiveagent_request, 
                contentType);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }
    }
}
