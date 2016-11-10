using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeServiceResolver"/>
    /// </summary>
    public class GivenPModeServiceResolverFacts
    {
        public class GivenValidArguments : GivenPModeServiceResolverFacts
        {
            [Fact]
            public void ThenResolverGetService()
            {
                // Arrange
                SendingProcessingMode pmode = CreateDefaultSendingPMode();
                var resolver = new PModeServiceResolver();
                // Act
                Service service = resolver.Resolve(pmode);
                // Assert
                Assert.Equal(pmode.MessagePackaging.CollaborationInfo.Service, service);
            }

            private SendingProcessingMode CreateDefaultSendingPMode()
            {
                return new SendingProcessingMode
                {
                    MessagePackaging =
                    {
                        CollaborationInfo = new CollaborationInfo {Service = new Service {Name = "name", Type = "type"}}
                    }
                };
            }

            [Fact]
            public void ThenResolverGetsDefaultService()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                var resolver = new PModeServiceResolver();
                // Act
                Service service = resolver.Resolve(pmode);
                // Assert
                Assert.Equal("http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/service", service.Name);
            }
        }
    }
}