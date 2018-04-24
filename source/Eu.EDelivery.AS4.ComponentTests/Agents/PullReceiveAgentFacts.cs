using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.Model.Internal;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly Settings _pullReceiveSettings;
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullReceiveAgentFacts"/> class.
        /// </summary>
        public PullReceiveAgentFacts()
        {
            _pullReceiveSettings = OverrideSettings("pullreceiveagent_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
            _databaseSpy = new DatabaseSpy(_as4Msh.GetConfiguration());
        }

        protected override void Disposing(bool isDisposing)
        {
            _as4Msh.Dispose();
        }

        [Fact]
        public async Task NoExceptionsAreLoggedWhenPullSenderIsNotAvailable()
        {
            string pullSenderUrl = RetrievePullingUrlFromConfig();

            // Wait a little bit to be sure that everything is started and a PullRequest has already been sent.
            await Task.Delay(TimeSpan.FromSeconds(2));

            var waiter = new ManualResetEvent(false);

            StubHttpServer.StartServer(pullSenderUrl, response => { throw new InvalidOperationException(); }, waiter);

            waiter.WaitOne();

            Assert.False(_databaseSpy.GetInExceptions((r) => true).Any(), "No logged InExceptions are expected.");
        }

        private string RetrievePullingUrlFromConfig()
        {
            var pullReceiveAgent = _pullReceiveSettings.Agents.PullReceiveAgents.FirstOrDefault();

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Receiver.Setting.First().Key;

            var pmode = _as4Msh.GetConfiguration().GetSendingPMode(pmodeId);

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending PMode found with Id {pmodeId}");
            }

            return pmode.PushConfiguration.Protocol.Url;
        }
    }
}
