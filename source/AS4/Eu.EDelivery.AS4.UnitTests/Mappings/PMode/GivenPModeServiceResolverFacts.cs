using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.Core;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;

namespace Eu.EDelivery.AS4.UnitTests.Mappings.PMode
{
    /// <summary>
    /// Testing <see cref="PModeServiceResolver" />
    /// </summary>
    public class GivenPModeServiceResolverFacts
    {
        public class GivenValidArguments : GivenPModeServiceResolverFacts
        {
            [Fact]
            public void ThenResolverGetsDefaultService()
            {
                // Arrange
                var pmode = new SendingProcessingMode();
                var resolver = new PModeServiceResolver();

                // Act
                Service service = resolver.Resolve(pmode);

                // Assert
                Assert.True(string.IsNullOrWhiteSpace(service.Value));                
            }

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

            private static SendingProcessingMode CreateDefaultSendingPMode()
            {
                return new SendingProcessingMode
                {
                    MessagePackaging =
                    {
                        CollaborationInfo =
                            new AS4.Model.PMode.CollaborationInfo {Service = new Service {Value = "name", Type = "type"}}
                    }
                };
            }
        }
    }
}