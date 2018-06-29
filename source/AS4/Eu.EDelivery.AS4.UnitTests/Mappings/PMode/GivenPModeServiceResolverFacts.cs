using Eu.EDelivery.AS4.Mappings.PMode;
using Eu.EDelivery.AS4.Model.PMode;
using Xunit;
using Service = Eu.EDelivery.AS4.Model.PMode.Service;

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
                AS4.Model.Core.Service service = PModeServiceResolver.ResolveService(pmode);

                // Assert
                Assert.Equal(AS4.Model.Core.Service.TestService, service);
            }

            [Fact]
            public void ThenResolverGetService()
            {
                // Arrange
                SendingProcessingMode pmode = CreateDefaultSendingPMode();
                var resolver = new PModeServiceResolver();

                // Act
                AS4.Model.Core.Service actual = PModeServiceResolver.ResolveService(pmode);

                // Assert
                var expected = pmode.MessagePackaging.CollaborationInfo.Service;
                Assert.Equal(expected.Value, actual.Value);
                Assert.Equal(Maybe.Just(expected.Type), actual.Type);
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