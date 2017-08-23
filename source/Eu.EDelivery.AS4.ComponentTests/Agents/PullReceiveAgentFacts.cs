using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Agents;
using Eu.EDelivery.AS4.Common;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Eu.EDelivery.AS4.TestUtils.Stubs;
using Remotion.Linq.Parsing;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class PullReceiveAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;
        private readonly DatabaseSpy _databaseSpy;

        /// <summary>
        /// Initializes a new instance of the <see cref="PullReceiveAgentFacts"/> class.
        /// </summary>
        public PullReceiveAgentFacts()
        {
            OverrideSettings("pullreceiveagent_settings.xml");
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
            string pullSenderUrl = RetrievePullingUrlFromConfig(_as4Msh.GetConfiguration());

            // Wait a little bit to be sure that everything is started and a PullRequest has already been sent.
            await Task.Delay(TimeSpan.FromSeconds(2));

            var waiter = new ManualResetEvent(false);

            StubHttpServer.StartServer(pullSenderUrl, response => { throw new InvalidOperationException(); }, waiter);

            waiter.WaitOne();

            Assert.False(_databaseSpy.GetInExceptions((r) => true).Any(), "No logged InExceptions are expected.");
        }

        private string RetrievePullingUrlFromConfig(IConfig as4Configuration)
        {
            var pullReceiveAgent = as4Configuration.GetAgentsConfiguration().FirstOrDefault(a => a.Type == AgentType.PullReceive);

            if (pullReceiveAgent == null)
            {
                throw new ConfigurationErrorsException("There is no PullReceive Agent configured.");
            }

            string pmodeId = pullReceiveAgent.Settings.Receiver.Setting.First().Key;

            var pmode = _as4Msh.GetConfiguration().GetSendingPMode(pmodeId);

            if (pmode == null)
            {
                throw new ConfigurationErrorsException($"No Sending PMode found with Id {pmodeId}");
            }

            return pmode.PushConfiguration.Protocol.Url;
        }
    }
}
