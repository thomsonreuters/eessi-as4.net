using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.ComponentTests.Common;
using Xunit;

namespace Eu.EDelivery.AS4.ComponentTests.Agents
{
    public class SendAgentFacts : ComponentTestTemplate
    {
        private readonly AS4Component _as4Msh;

        public SendAgentFacts()
        {
            OverrideSettings("sendagent_http_settings.xml");
            _as4Msh = AS4Component.Start(Environment.CurrentDirectory);
        }

        [Fact]
        public void WhenIntermediary_ThenForwardReceivedSignalMessage()
        {
            
        }
    }
}
